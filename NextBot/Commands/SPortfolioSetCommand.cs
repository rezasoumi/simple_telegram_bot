using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NextBot.Handlers;
using NextBot.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Args;

namespace NextBot.Commands
{
    public class SPortfolioSetCommand : StaticFunctions, IBotCommand
    {
        private readonly IServiceProvider _serviceProvider;
        private MyDbContext _context;

        public string Command => "sportfolioset";

        public string Description => "انتخاب پرتفوی مرکب";

        public bool InternalCommand => false;

        public SPortfolioSetCommand(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var scope = serviceProvider.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        }

        public async Task<MyDbContext> Execute(IChatService chatService, long chatId, long userId, int messageId, string? commandText, CallbackQueryEventArgs? query)
        {
            var person = _context.People.FirstOrDefault(p => p.ChatId == chatId);
            person.CommandState = 3;
            _context.Entry(person).State = EntityState.Modified;

            if (person.CommandLevel == 0)
            {
                await chatService.SendMessage(chatId, message: "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectTypesRKM);
                person.CommandLevel = 1;
                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 1)
            {
                switch (commandText)
                {
                    case "انتخاب بر اساس وارد کردن آی دی پرتفوی مورد نظر":
                        person.CommandLevel = 6;
                        await chatService.SendMessage(chatId, message: "آی دی پرتفوی مورد نظر را به صورت یک عدد انگلیسی وارد کنید :");
                        break;
                    case "انتخاب بر اساس گذر میان پرتفوی ها":
                        person.CommandLevel = 2;
                        person.ClassicNextSelectState = 1;
                        person = await ShowPreviousOrNextListInPortfolioSetSelect(chatService, person);
                        break;
                    case "🔙":
                        person.CommandState = 0;
                        person.CommandLevel = 0;
                        await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.MainMenuRKM);
                        break;
                    default:
                        await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.SelectTypesRKM);
                        break;
                }
                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 2)
            {
                switch (commandText)
                {
                    case "بعدی⬇️":
                        person = await ShowPreviousOrNextListInPortfolioSetSelect(chatService, person);
                        break;
                    case "قبلی⬆️":
                        if (person.ClassicNextSelectState == 21)
                        {
                            person.CommandLevel = 1;
                            await chatService.SendMessage(chatId, message: "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectTypesRKM);
                            break;
                        }
                        person.ClassicNextSelectState -= 40;
                        person = await ShowPreviousOrNextListInPortfolioSetSelect(chatService, person);
                        break;
                    default:
                        try
                        {
                            var split = commandText.Split(" ");
                            var strNum = split[3];
                            person = await ShowSpecificPortfolioSetInClassicNextSelect(chatService, person, strNum);
                        }
                        catch (Exception e)
                        {
                            await chatService.SendMessage(chatId, message: "خطایی رخ داده است. لطفا مجدد تلاش کنید.");
                        }
                        break;
                }
                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 3)
            {
                switch (commandText)
                {
                    case "افزودن پرتفوی➕":
                        person.CommandLevel = 4;
                        await chatService.SendMessage(chatId, message: "آی دی پرتفوی های مورد نظر را با یک فاصله به صورت انگلیسی وارد نمایید : (نمونه : 13 15 23)");
                        break;
                    case "حذف پرتفوی➖":
                        break;
                    case "محاسبه بازدهی📈":
                        person.CommandLevel = 7;
                        await chatService.SendMessage(chatId, message: "نوع بازدهی را مشخص کنید :", Markup.ReturnPortfolioSetTypesRKM);
                        break;
                    case "مقایسه📊":
                        person.CommandLevel = 9;
                        await chatService.SendMessage(chatId: chatId, message: "گزینه مورد نظر را انتخاب کنید :", Markup.ComparisonSetTypesRKM);
                        break;
                    case "حذف پرتفوی مرکب❌":
                        person.CommandLevel = 8;
                        await chatService.SendMessage(chatId, message: "پرتفوی مورد نظر حذف شود ؟", GetSaveInlineKeyboard());
                        break;
                    case "🔙":
                        person.CommandLevel = 1;
                        await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.SelectTypesRKM);
                        break;
                    default:
                        await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.PortfolioSetSelectRKM);
                        break;
                }
                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 4)
            {
                try
                {
                    var ids = commandText.Split(" ");
                    var parameter = new PortfolioSet.AddPortfolioParameter() { PortfolioSetId = person.PortfolioIdForClassicNextSelect, PortfolioIds = Array.ConvertAll(ids, int.Parse) };
                    var json = JsonConvert.SerializeObject(parameter);
                    var strContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("http://192.168.95.88:30907/api/classicNext/portfolioSet/add", strContent).Result.Content.ReadAsStringAsync();
                    var resObj = JsonConvert.DeserializeObject<PortfolioSet.Rootobject>(response);
                    if (resObj.IsSuccessful)
                    {
                        await chatService.SendMessage(chatId, message: "افزودن پرتفوی با موفقیت انجام شد");
                        person = await ShowSpecificPortfolioSetInClassicNextSelect(chatService, person, person.PortfolioIdForClassicNextSelect.ToString());
                    }
                    else
                    {
                        await chatService.SendMessage(chatId, message: resObj.ErrorMessageFa);
                        await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.PortfolioSetSelectRKM);
                        person.CommandLevel = 3;
                    }
                }
                catch (Exception)
                {
                    await chatService.SendMessage(chatId, message: "ورودی اشتباه ! لطفا اعداد را درست به صورت نمونه وارد نمایید :");
                }
                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 5)
            {
                var date = await CheckAndGetDate(chatService, query);
                if (date != null)
                {
                    await chatService.SendMessage(chatId, message: "تاریخ شروع همان تاریخ ساخت پرتفوی مورد نظر می باشد ...");
                    var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/return/{person.PortfolioIdForClassicNextSelect}/{date}");
                    var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.RootobjectForCalculateReturnAndComparison>(await streamTask);
                    if (root.IsSuccessful)
                        await chatService.SendMessage(chatId, message: $"بازدهی پرتفوی مرکب شماره  {person.PortfolioIdForClassicNextSelect} : " + "\n" + Math.Round(Convert.ToDecimal(root.ResponseObject) * 100, 1) + " %", Markup.PortfolioSetSelectRKM);
                    else
                        await chatService.SendMessage(chatId, message: root.ErrorMessageFa, Markup.PortfolioSetSelectRKM);
                    await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnPortfolioSetTypesRKM);
                    person.CommandLevel = 7;
                }
                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 6)
            {
                person = await ShowSpecificPortfolioSetInClassicNextSelect(chatService, person, commandText);
                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 7)
            {
                switch (commandText)
                {
                    case "بازدهی پرتفوی مرکب تا تاریخ دلخواه📆":
                        person.CommandLevel = 5;
                        await chatService.SendMessage(chatId, message: "محاسبه بازدهی تا تاریخ مورد نظر > تاریخ مورد نظر را انتخاب کنید :", CreateCalendar());
                        break;
                    case "بازدهی پرتفوی مرکب تا امروز":
                        await chatService.SendMessage(chatId, message: "تاریخ شروع همان تاریخ ساخت پرتفوی مورد نظر می باشد ...");
                        var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/return/{person.PortfolioIdForClassicNextSelect}");
                        var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.RootobjectForCalculateReturnAndComparison>(await streamTask);
                        if (root.IsSuccessful)
                            await chatService.SendMessage(chatId, message: $"بازدهی پرتفوی مرکب شماره  {person.PortfolioIdForClassicNextSelect} : " + "\n" + Math.Round(Convert.ToDecimal(root.ResponseObject) * 100, 1) + " %", Markup.PortfolioSetSelectRKM);
                        else
                            await chatService.SendMessage(chatId, message: root.ErrorMessageFa, Markup.PortfolioSetSelectRKM);
                        await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnPortfolioSetTypesRKM);
                        break;
                    case "🔙":
                        person.CommandLevel = 3;
                        await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.PortfolioSetSelectRKM);
                        break;
                    default:
                        await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnPortfolioSetTypesRKM);
                        break;
                }
            }
            else if (person.CommandLevel == 8)
            {
                if (commandText == "خیر")
                {
                    await chatService.UpdateMessage(chatId: query.CallbackQuery.Message.Chat.Id,
                                                            messageId: query.CallbackQuery.Message.MessageId,
                                                            newText: "پرتفوی مرکب مورد نظر حذف نشد");
                    await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.PortfolioSetSelectRKM);
                    person.CommandLevel = 3;
                }
                else if (commandText == "بلی")
                {
                    await chatService.UpdateMessage(chatId: query.CallbackQuery.Message.Chat.Id,
                                                            messageId: query.CallbackQuery.Message.MessageId,
                                                            newText: "پرتفوی مرکب مورد نظر حذف شود");
                    var streamTask_ = client.DeleteAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/{person.PortfolioIdForClassicNextSelect}");
                    var x = streamTask_.Result.Content.ReadAsStreamAsync();
                    var res = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.Delete>(await x);
                    if (res.isSuccessful)
                    {
                        await chatService.SendMessage(chatId: chatId, message: $"پرتفوی مرکب شماره {person.PortfolioIdForClassicNextSelect} حذف شد");
                        await chatService.SendMessage(chatId, message: "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectTypesRKM);
                        person.CommandLevel = 1;
                    }
                    else
                    {
                        await chatService.SendMessage(chatId: chatId, message: res.errorMessageFa);
                        await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.PortfolioSetSelectRKM);
                        person.CommandLevel = 3;
                    }
                }
            }
            else if (person.CommandLevel == 9)
            {
                switch (commandText)
                {
                    case "شاخص":
                        person.CommandLevel = 10;
                        await chatService.SendMessage(chatId: chatId, message: "نوع بازدهی را مشخص کنید :", Markup.ReturnIndexTypesRKM);
                        break;
                    case "صندوق سهامی":
                        await chatService.SendMessage(chatId, message: "تاریخ پایان محاسبه بازدهی را انتخاب کنید", rkm: CreateCalendar());
                        person.CommandLevel = 12;
                        break;
                    case "پرتفوی مرکب":
                        person.CommandLevel = 14;
                        await chatService.SendMessage(chatId: chatId, message: "مقایسه با پرتفوی مورد نظر تا تاریخ مشخص -> آی دی پرتفوی مرکب مورد نظر برای مقایسه وارد نمایید :");
                        break;
                    case "🔙":
                        person.CommandLevel = 3;
                        await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.PortfolioSetSelectRKM);
                        break;
                    default:
                        await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonSetTypesRKM);
                        break;
                }
            }
            else if (person.CommandLevel == 10)
            {
                switch (commandText)
                {
                    case "بازدهی شاخص تا تاریخ دلخواه📆":
                        person.CommandLevel = 11;
                        await chatService.SendMessage(chatId: chatId, message: "تاریخ مورد نظر خود را انتخاب کنید :", CreateCalendar());
                        break;
                    case "بازدهی شاخص تا امروز":
                        var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/return/{person.PortfolioIdForClassicNextSelect}");
                        if (await ShowReturnAndComparisonInPortfolioSet(chatService, person, streamTask, person.PortfolioIdForClassicNextSelect))
                        {
                            var date = await GetBithdayOfPortfolioSet(person);
                            var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/stock/return/index/{date}");
                            await ShowIndextReturnInClassicNextSelect(chatService, person, streamTask_);
                        }
                        Thread.Sleep(500);
                        person.CommandLevel = 10;
                        await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnIndexTypesRKM);
                        break;
                    case "🔙":
                        person.CommandLevel = 9;
                        await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonSetTypesRKM);
                        break;
                    default:
                        await chatService.SendMessage(chatId: chatId, message: "ورودی نامعتبر! از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnIndexTypesRKM);
                        break;
                }
            }
            else if (person.CommandLevel == 11)
            {
                var date = await CheckAndGetDate(chatService, query);
                if (date != null)
                {
                    var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/return/{person.PortfolioIdForClassicNextSelect}/{date}");
                    if (await ShowReturnAndComparisonInPortfolioSet(chatService, person, streamTask, person.PortfolioIdForClassicNextSelect))
                    {
                        var date_ = await GetBithdayOfPortfolioSet(person);
                        var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/stock/return/index/{date_}/{date}");
                        await ShowIndextReturnInClassicNextSelect(chatService, person, streamTask_);
                    }

                    Thread.Sleep(500);
                    await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnIndexTypesRKM);
                    person.CommandLevel = 10;
                }
            }
            else if (person.CommandLevel == 12)
            {
                var date = await CheckAndGetDate(chatService, query);
                if (date != null)
                {
                    var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/return/{person.PortfolioIdForClassicNextSelect}/{date}");
                    await ShowReturnAndComparisonInPortfolioSet(chatService, person, streamTask, person.PortfolioIdForClassicNextSelect);

                    var birthdayPortfolio = await GetBithdayOfPortfolioSet(person);
                    var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/fund/etf/returns/{birthdayPortfolio}/{date}");
                    var etfs = await System.Text.Json.JsonSerializer.DeserializeAsync<Models.ETF.Specific.Rootobject>(await streamTask_);
                    if (etfs.responseObject != null)
                    {
                        StringBuilder str = new();
                        for (int i = 0; i < etfs.responseObject.Length; i++)
                        {
                            var etf = etfs.responseObject.ElementAt(i);
                            str.Append($"{i + 1}. {etf.fund.symbol} : " + "\n" + Math.Round(Convert.ToDecimal(etf.returnValue) * 100, 1) + " %" + "\n");
                        }
                        await chatService.SendMessage(chatId: person.ChatId, message: str.ToString());
                    }
                    else
                    {
                        await chatService.SendMessage(chatId: chatId, message: etfs.errorMessageFa);
                    }

                    Thread.Sleep(1000);
                    await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonSetTypesRKM);
                    person.CommandLevel = 9;
                }
            }
            else if (person.CommandLevel == 13)
            {
            }
            else if (person.CommandLevel == 14)
            {
                person.StartDateWaitingForEndDate = commandText; // instead of create another property use previous one that in this state is useless.
                await chatService.SendMessage(chatId: person.ChatId, message: "مقایسه با پرتفوی مرکب مورد نظر تا تاریخ مشخص -> تاریخ مورد نظر را انتخاب کنید", CreateCalendar());
                person.CommandLevel = 15;
            }
            else if (person.CommandLevel == 15)
            {
                var date = await CheckAndGetDate(chatService, query);
                if (date != null)
                {
                    var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/return/{person.PortfolioIdForClassicNextSelect}/{date}");
                    if (await ShowReturnAndComparisonInPortfolioSet(chatService, person, streamTask, person.PortfolioIdForClassicNextSelect))
                    {
                        var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/return/{person.StartDateWaitingForEndDate}/{date}");
                        await ShowReturnAndComparisonInPortfolioSet(chatService, person, streamTask_, long.Parse(person.StartDateWaitingForEndDate));
                    }
                    person.CommandLevel = 9;
                    await chatService.SendMessage(chatId: person.ChatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonSetTypesRKM);
                }
                _context.Entry(person).State = EntityState.Modified;
            }
            await _context.SaveChangesAsync();
            return _context;
        }
    }
}
