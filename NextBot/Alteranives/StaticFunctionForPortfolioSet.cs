using Newtonsoft.Json;
using NextBot.Handlers;
using NextBot.Models;
using QuickChart;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace NextBot.Alteranives
{
    public class StaticFunctionForPortfolioSet : StaticFunctions
    {

        public static async Task<Person> SwitchProcessForMainMenuOfPortfolioSet(IChatService chatService, long chatId, string commandText, Person person)
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
                    person.CommandState = 0;
                    person.CommandLevel = 1;
                    await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.SelectOrCreateRKM);
                    break;
                default:
                    await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.PortfolioSetSelectRKM);
                    break;
            }
            return person;
        }

        public async Task<Person> SwitchProcessForReturnOfPortfolioSet(IChatService chatService, long chatId, string commandText, Person person)
        {
            switch (commandText)
            {
                case "بازدهی پرتفوی مرکب تا تاریخ دلخواه📆":
                    person.CommandLevel = 5;
                    await chatService.SendMessage(chatId, message: "محاسبه بازدهی تا تاریخ مورد نظر > تاریخ مورد نظر را انتخاب کنید :", CreateCalendar());
                    break;
                case "بازدهی پرتفوی مرکب تا امروز":
                    await SendReturnOfPortfolioSetToToday(chatService, chatId, person);
                    break;
                case "🔙":
                    person.CommandLevel = 3;
                    await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.PortfolioSetSelectRKM);
                    break;
                default:
                    await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnPortfolioSetTypesRKM);
                    break;
            }
            return person;
        }

        public async Task<Person> SwitchProcessForComparison(IChatService chatService, long chatId, string commandText, Person person)
        {
            switch (commandText)
            {
                case "مقایسه با شاخص تا امروز":
                    await ComparisonPortfolioSetWithIndex(chatService, chatId, person);
                    break;
                case "مقایسه با شاخص تا تاریخ دلخواه📆":
                    person.CommandLevel = 11;
                    await chatService.SendMessage(chatId: chatId, message: "تاریخ مورد نظر خود را انتخاب کنید :", CreateCalendar());
                    break;
                case "مقایسه با صندوق سهامی تا امروز":
                    person = await ComparisonPortfolioSetWithETFs(chatService, chatId, person);
                    break;
                case "مقایسه با صندوق سهامی تا تاریخ دلخواه📆":
                    await chatService.SendMessage(chatId, message: "تاریخ پایان محاسبه بازدهی را انتخاب کنید", rkm: CreateCalendar());
                    person.CommandLevel = 12;
                    break;
                case "مقایسه با پرتفوی مرکب تا امروز":
                    await chatService.SendMessage(chatId: chatId, message: "مقایسه با پرتفوی مرکب مورد نظر تا امروز -> آی دی پرتفوی مرکب مورد نظر برای مقایسه وارد نمایید :");
                    person.CommandLevel = 13;
                    break;
                case "مقایسه با پرتفوی مرکب تا تاریخ دلخواه📆":
                    person.CommandLevel = 14;
                    await chatService.SendMessage(chatId: chatId, message: "مقایسه با پرتفوی مرکب مورد نظر تا تاریخ دلخواه -> آی دی پرتفوی مرکب مورد نظر برای مقایسه وارد نمایید :");
                    break;
                case "🔙":
                    person.CommandLevel = 3;
                    await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.PortfolioSetSelectRKM);
                    break;
                default:
                    await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonSetTypesRKM);
                    break;
            }
            return person;
        }

        public static async Task<Person> SwitchProcessForSelectPortfolioSet(IChatService chatService, long chatId, string commandText, Person person)
        {
            switch (commandText)
            {
                case "بعدی⬇️":
                    person = await ShowPreviousOrNextListInPortfolioSetSelect(chatService, person, null);
                    break;
                case "قبلی⬆️":
                    if (person.ClassicNextSelectState == 21)
                    {
                        person.CommandState = 0;
                        person.CommandLevel = 1;
                        await chatService.SendMessage(chatId: person.ChatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.SelectOrCreateRKM);
                        break;
                    }
                    person.ClassicNextSelectState -= 40;
                    person = await ShowPreviousOrNextListInPortfolioSetSelect(chatService, person, null);
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
            return person;
        }

        public static async Task<Person> ShowSpecificPortfolioSetInClassicNextSelect(IChatService chatService, Person person, String strNumber)
        {
            try
            {
                var num = int.Parse(strNumber.Trim());

                var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/{num}");
                var root = await System.Text.Json.JsonSerializer.DeserializeAsync<PortfolioSet.Rootobject>(await streamTask);

                if (root.IsSuccessful)
                {
                    StringBuilder str = new();
                    str.Append($"id : {root.ResponseObject.Id}\n");
                    str.Append("birthday : " + root.ResponseObject.Birthday + "\n");
                    str.Append("persian birthday : " + root.ResponseObject.BirthdayPersian + "\n");
                    str.Append("Stock and weights : \n");
                    for (int i = 0; i < root.ResponseObject.ClassicNextPortfolioSetElements?.Length; i++)
                    {
                        var item = root.ResponseObject.ClassicNextPortfolioSetElements.ElementAt(i);
                        str.Append($"Element number {item.ElementNumber} :\n");
                        str.Append($"Portfolio id : {item.PortfolioId}\n");
                        str.Append($"Persian birthday : {item.BirthdayPersian}\n\n");
                    }

                    await chatService.SendMessage(person.ChatId, message: str.ToString());
                    Thread.Sleep(500);
                    await chatService.SendMessage(person.ChatId, message: "از بین گزینه های زیر انتخاب کنید :", Markup.PortfolioSetSelectRKM);

                    person.CommandLevel = 3;
                    person.PortfolioIdForClassicNextSelect = num;
                }
                else
                {
                    person.CommandState = 0;
                    person.CommandLevel = 1;
                    await chatService.SendMessage(person.ChatId, message: root.ErrorMessageFa);
                    Thread.Sleep(200);
                    await chatService.SendMessage(person.ChatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.SelectOrCreateRKM);
                }
            }
            catch (Exception)
            {
                person.CommandState = 0;
                person.CommandLevel = 1;
                await chatService.SendMessage(person.ChatId, message: "ورودی نامعتبر !");
                await chatService.SendMessage(person.ChatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.SelectOrCreateRKM);
            }
            return person;
        }

        public async Task<Person> ComparisonTwoPortfolioSetToSpecificDate(IChatService chatService, long chatId, CallbackQuery query, Person person)
        {
            var date = await CheckAndGetDate(chatService, query);
            if (date != null)
            {
                try
                {
                    var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/return/{person.PortfolioIdForClassicNextSelect}/{date}");
                    if (await ShowReturnAndComparisonInPortfolioSet(chatService, person, streamTask, person.PortfolioIdForClassicNextSelect))
                    {
                        var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/return/{person.StartDateWaitingForEndDate}/{date}");
                        await ShowReturnAndComparisonInPortfolioSet(chatService, person, streamTask_, long.Parse(person.StartDateWaitingForEndDate));
                        await CreateChartForComparisonTwoPortfolioSetToSpecificDate(chatService, chatId, person, date);
                    }
                    person.CommandLevel = 9;
                    await chatService.SendMessage(chatId: person.ChatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonSetTypesRKM);

                }
                catch (Exception)
                {
                    await chatService.SendMessage(chatId: chatId, message: "خطایی رخ داده است. از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonSetTypesRKM);
                    person.CommandLevel = 9;
                }
            }
            if (query == null)
            {
                await chatService.SendMessage(chatId: chatId, message: "مقایسه با پرتفوی مرکب مورد نظر تا تاریخ دلخواه -> آی دی پرتفوی مرکب مورد نظر برای مقایسه وارد نمایید :");
                person.CommandLevel = 14;
            }
            return person;
        }

        public static async Task<bool> ShowReturnAndComparisonInPortfolioSet(IChatService chatService, Person person, Task<Stream> streamTask, long portfolioId)
        {
            await chatService.SendMessage(chatId: person.ChatId, message: "تاریخ شروع همان تاریخ ساخت پرتفوی مرکب مورد نظر می باشد ...");
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.RootobjectForCalculateReturnAndComparison>(await streamTask);
            if (root.IsSuccessful)
            {
                await chatService.SendMessage(chatId: person.ChatId, message: $"بازدهی پرتفوی مرکب شماره  {portfolioId} : " + "\n" + Math.Round(Convert.ToDecimal(root.ResponseObject) * 100, 1) + " %");
                Thread.Sleep(500);
                return true;
            }
            else
            {
                await chatService.SendMessage(chatId: person.ChatId, message: root.ErrorMessageFa, Markup.PortfolioSetSelectRKM);
                Thread.Sleep(500);
                return false;
            }

        }


        public static async Task CreateChartForComparisonTwoPortfolioSetToSpecificDate(IChatService chatService, long chatId, Person person, string date)
        {
            var streamTaskChart1 = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/normalPrices/{person.PortfolioIdForClassicNextSelect}/{date}");
            var streamTaskChart2 = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/normalPrices/{person.StartDateWaitingForEndDate}/{date}");
            var configBuilder = await CraeteJsonForGetChartComparisonPortfolioSet(streamTaskChart1, streamTaskChart2, person);
            if (configBuilder != null)
            {
                var qc = new Chart();
                qc.Width = 800;
                qc.Height = 500;
                qc.Config = configBuilder;
                await chatService.SendPhotoAsync(chatId, qc.GetUrl());
            }
        }

        

        public static async Task<Person> ComparisonTwoPortfolioSetToToday(IChatService chatService, long chatId, string commandText, Person person)
        {
            try
            {
                person.StartDateWaitingForEndDate = commandText; // instead of create another property use previous one that in this state is useless.
                var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/return/{person.PortfolioIdForClassicNextSelect}");
                if (await ShowReturnAndComparisonInPortfolioSet(chatService, person, streamTask, person.PortfolioIdForClassicNextSelect))
                {
                    var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/return/{person.StartDateWaitingForEndDate}");
                    await ShowReturnAndComparisonInPortfolioSet(chatService, person, streamTask_, long.Parse(person.StartDateWaitingForEndDate));
                    await CreateChartForComparisonTwoPortfolioSetToToday(chatService, chatId, person);
                }
                person.CommandLevel = 9;
                await chatService.SendMessage(chatId: person.ChatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonSetTypesRKM);
            }
            catch (Exception)
            {
                await chatService.SendMessage(chatId: person.ChatId, message: "خطایی رخ داده است. لطفا مجدد عدد خود را وارد نمایید :");
            }
            return person;
        }

        public static async Task CreateChartForComparisonTwoPortfolioSetToToday(IChatService chatService, long chatId, Person person)
        {
            var streamTaskChart1 = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/normalPrices/{person.PortfolioIdForClassicNextSelect}");
            var streamTaskChart2 = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/normalPrices/{person.StartDateWaitingForEndDate}");
            var configBuilder = await CraeteJsonForGetChartComparisonPortfolioSet(streamTaskChart1, streamTaskChart2, person);
            if (configBuilder != null)
            {
                var qc = new Chart();
                qc.Width = 800;
                qc.Height = 500;
                qc.Config = configBuilder;
                await chatService.SendPhotoAsync(chatId, qc.GetUrl());
            }
        }

        

        public async Task<Person> ComparisonPortfolioSetWithETFsToSpecificDate(IChatService chatService, long chatId, CallbackQuery query, Person person)
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

                    var streamTask0_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/fund/etf/normalPrices/{birthdayPortfolio}/{date}");
                    var etfPrices = await System.Text.Json.JsonSerializer.DeserializeAsync<NormalPrices.RootObjectETF>(await streamTask0_);

                    var streamTask1_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/normalPrices/{person.PortfolioIdForClassicNextSelect}/{date}");
                    var stock = await System.Text.Json.JsonSerializer.DeserializeAsync<NormalPrices.RootObject>(await streamTask1_);
                    await CreateChartForComparisonPortfolioSetWithETFs(chatService, chatId, person, etfPrices, stock);
                }
                else
                {
                    await chatService.SendMessage(chatId: chatId, message: etfs.errorMessageFa);
                }

                Thread.Sleep(1000);
                await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonSetTypesRKM);
                person.CommandLevel = 9;
            }
            if (query == null)
            {
                await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonSetTypesRKM);
                person.CommandLevel = 9;
            }
            return person;
        }

        public static async Task<string> GetBithdayOfPortfolioSet(Person person)
        {
            var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/{person.PortfolioIdForClassicNextSelect}");
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.RootObjectForSpecificPortfolio>(await streamTask);
            var str = root.ResponseObject.BirthdayPersian;
            return str.Replace("/", "");
        }

        public async Task<Person> ComparisonPortfolioSetWithIndexToSpecificDate(IChatService chatService, long chatId, CallbackQuery query, Person person)
        {
            var date = await CheckAndGetDate(chatService, query);
            if (date != null)
            {
                var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/return/{person.PortfolioIdForClassicNextSelect}/{date}");
                if (await ShowReturnAndComparisonInPortfolioSet(chatService, person, streamTask, person.PortfolioIdForClassicNextSelect))
                {
                    var date_ = await GetBithdayOfPortfolioSet(person);
                    var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/stock/return/index/{date_}/{date}");
                    await ShowIndexReturnInClassicNextSelect(chatService, person, streamTask_);
                    await CreateChartForComparisonPortfolioSetWithIndexToSpecificDate(chatService, chatId, person, date, date_);
                }

                Thread.Sleep(500);
                await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonSetTypesRKM);
                person.CommandLevel = 9;
            }
            if (query == null)
            {
                await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonSetTypesRKM);
                person.CommandLevel = 9;
            }
            return person;
        }

        public static async Task CreateChartForComparisonPortfolioSetWithIndexToSpecificDate(IChatService chatService, long chatId, Person person, string date, string date_)
        {
            var streamTaskChart1 = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/normalPrices/{person.PortfolioIdForClassicNextSelect}/{date}");
            var streamTaskChart2 = client.GetStreamAsync($"http://192.168.95.88:30907/api/stock/normalPrices/index/{date_}/{date}");
            var configBuilder = await CraeteJsonForGetChartComparisonPortfolioSetWithIndex(streamTaskChart1, streamTaskChart2, person);
            if (configBuilder != null)
            {
                var qc = new Chart();
                qc.Width = 800;
                qc.Height = 500;
                qc.Config = configBuilder;
                await chatService.SendPhotoAsync(chatId, qc.GetUrl());
            }
        }

        public static async Task<Person> ComparisonPortfolioSetWithETFs(IChatService chatService, long chatId, Person person)
        {
            var streamTask0 = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/return/{person.PortfolioIdForClassicNextSelect}");
            await ShowReturnAndComparisonInPortfolioSet(chatService, person, streamTask0, person.PortfolioIdForClassicNextSelect);

            var birthdayPortfolio = await GetBithdayOfPortfolioSet(person);
            var streamTask1 = client.GetStreamAsync($"http://192.168.95.88:30907/api/fund/etf/returns/{birthdayPortfolio}");
            var etfs = await System.Text.Json.JsonSerializer.DeserializeAsync<Models.ETF.Specific.Rootobject>(await streamTask1);
            if (etfs.responseObject != null)
            {
                StringBuilder str = new();
                for (int i = 0; i < etfs.responseObject.Length; i++)
                {
                    var etf = etfs.responseObject.ElementAt(i);
                    str.Append($"{i + 1}. {etf.fund.symbol} : " + "\n" + Math.Round(Convert.ToDecimal(etf.returnValue) * 100, 1) + " %" + "\n");
                }
                await chatService.SendMessage(chatId: person.ChatId, message: str.ToString());

                var streamTask0_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/fund/etf/normalPrices/{birthdayPortfolio}");
                var etfPrices = await System.Text.Json.JsonSerializer.DeserializeAsync<NormalPrices.RootObjectETF>(await streamTask0_);

                var streamTask1_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/normalPrices/{person.PortfolioIdForClassicNextSelect}");
                var stock = await System.Text.Json.JsonSerializer.DeserializeAsync<NormalPrices.RootObject>(await streamTask1_);
                await CreateChartForComparisonPortfolioSetWithETFs(chatService, chatId, person, etfPrices, stock);
            }
            else
            {
                await chatService.SendMessage(chatId: chatId, message: etfs.errorMessageFa);
            }

            Thread.Sleep(1000);
            await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonSetTypesRKM);
            person.CommandLevel = 9;
            return person;
        }

        public static async Task ComparisonPortfolioSetWithIndex(IChatService chatService, long chatId, Person person)
        {
            var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/return/{person.PortfolioIdForClassicNextSelect}");
            if (await ShowReturnAndComparisonInPortfolioSet(chatService, person, streamTask, person.PortfolioIdForClassicNextSelect))
            {
                var date = await GetBithdayOfPortfolioSet(person);
                var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/stock/return/index/{date}");
                await ShowIndexReturnInClassicNextSelect(chatService, person, streamTask_);
                await CreateChartForComparisonPortfolioSetAndIndex(chatService, chatId, person, date);
            }
            Thread.Sleep(500);
            await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonSetTypesRKM);
        }

        public static async Task CreateChartForComparisonPortfolioSetAndIndex(IChatService chatService, long chatId, Person person, string date)
        {
            var streamTaskChart1 = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/normalPrices/{person.PortfolioIdForClassicNextSelect}");
            var streamTaskChart2 = client.GetStreamAsync($"http://192.168.95.88:30907/api/stock/normalPrices/index/{date}");
            var configBuilder = await CraeteJsonForGetChartComparisonPortfolioSetWithIndex(streamTaskChart1, streamTaskChart2, person);
            if (configBuilder != null)
            {
                var qc = new Chart();
                qc.Width = 800;
                qc.Height = 500;
                qc.Config = configBuilder;
                await chatService.SendPhotoAsync(chatId, qc.GetUrl());
            }
        }

        public static async Task<Person> DeletePortfolioSetOrNot(IChatService chatService, long chatId, string commandText, CallbackQuery query, Person person)
        {
            if (commandText == "خیر")
            {
                await chatService.UpdateMessage(chatId: query.Message.Chat.Id,
                                                        messageId: query.Message.MessageId,
                                                        newText: "پرتفوی مرکب مورد نظر حذف نشد");
                await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.PortfolioSetSelectRKM);
                person.CommandLevel = 3;
            }
            else if (commandText == "بلی")
            {
                await chatService.UpdateMessage(chatId: query.Message.Chat.Id,
                                                        messageId: query.Message.MessageId,
                                                        newText: "پرتفوی مرکب مورد نظر حذف شود");
                person = await ProcessOfDeletingPortfolioSet(chatService, chatId, person);
            }
            return person;
        }

        public static async Task<Person> ProcessOfDeletingPortfolioSet(IChatService chatService, long chatId, Person person)
        {
            var streamTask_ = client.DeleteAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/{person.PortfolioIdForClassicNextSelect}");
            var x = streamTask_.Result.Content.ReadAsStreamAsync();
            var res = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.Delete>(await x);
            if (res.isSuccessful)
            {
                await chatService.SendMessage(chatId: chatId, message: $"پرتفوی مرکب شماره {person.PortfolioIdForClassicNextSelect} حذف شد");
                await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.SelectOrCreateRKM);
                person.CommandState = 0;
                person.CommandLevel = 1;
            }
            else
            {
                await chatService.SendMessage(chatId: chatId, message: res.errorMessageFa);
                await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.PortfolioSetSelectRKM);
                person.CommandLevel = 3;
            }
            return person;
        }

        public static async Task SendReturnOfPortfolioSetToToday(IChatService chatService, long chatId, Person person)
        {
            await chatService.SendMessage(chatId, message: "تاریخ شروع همان تاریخ ساخت پرتفوی مورد نظر می باشد ...");
            var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/return/{person.PortfolioIdForClassicNextSelect}");
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.RootobjectForCalculateReturnAndComparison>(await streamTask);
            if (root.IsSuccessful)
            {
                await chatService.SendMessage(chatId, message: $"بازدهی پرتفوی مرکب شماره  {person.PortfolioIdForClassicNextSelect} : " + "\n" + Math.Round(Convert.ToDecimal(root.ResponseObject) * 100, 1) + " %", Markup.PortfolioSetSelectRKM);
                var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/normalPrices/{person.PortfolioIdForClassicNextSelect}");
                await SendChartOfPortfolioSet(chatService, chatId, person, streamTask_);
            }
            else
                await chatService.SendMessage(chatId, message: root.ErrorMessageFa, Markup.PortfolioSetSelectRKM);

            await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnPortfolioSetTypesRKM);
        }

        public static async Task SendChartOfPortfolioSet(IChatService chatService, long chatId, Person person, Task<Stream> streamTask_)
        {
            var normalPriceRoot = await System.Text.Json.JsonSerializer.DeserializeAsync<NormalPrices.RootObject>(await streamTask_);
            if (normalPriceRoot.isSuccessful)
            {
                var qc = new Chart();
                qc.Width = 800;
                qc.Height = 500;
                var configBuilder = CraeteJsonForGetChartPortfolioSet(normalPriceRoot, person);
                qc.Config = configBuilder;
                await chatService.SendPhotoAsync(chatId, qc.GetUrl());
            }
        }

        public async Task<Person> SendReturnOfPortfolioSetInSpecificDate(IChatService chatService, long chatId, CallbackQuery query, Person person)
        {
            var date = await CheckAndGetDate(chatService, query);
            if (date != null)
            {
                await chatService.SendMessage(chatId, message: "تاریخ شروع همان تاریخ ساخت پرتفوی مرکب مورد نظر می باشد ...");
                var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/return/{person.PortfolioIdForClassicNextSelect}/{date}");
                var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.RootobjectForCalculateReturnAndComparison>(await streamTask);
                if (root.IsSuccessful)
                {
                    await chatService.SendMessage(chatId, message: $"بازدهی پرتفوی مرکب شماره  {person.PortfolioIdForClassicNextSelect} : " + "\n" + Math.Round(Convert.ToDecimal(root.ResponseObject) * 100, 1) + " %", Markup.PortfolioSetSelectRKM);
                    var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/normalPrices/{person.PortfolioIdForClassicNextSelect}/{date}");
                    await SendChartOfPortfolioSet(chatService, chatId, person, streamTask_);
                }
                else
                    await chatService.SendMessage(chatId, message: root.ErrorMessageFa, Markup.PortfolioSetSelectRKM);

                await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnPortfolioSetTypesRKM);
                person.CommandLevel = 7;
            }
            if (query == null)
            {
                await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnPortfolioSetTypesRKM);
                person.CommandLevel = 7;
            }
            return person;
        }

        public static async Task<Person> PostRequestForAddPortfolioToPortfolioSet(IChatService chatService, long chatId, string commandText, Person person)
        {
            try
            {
                var resObj = await PostRequestForGetResponse(commandText, person);
                person = await CheckAddingPortfolioToPortfolioSetIsSuccessful(chatService, chatId, person, resObj);
            }
            catch (Exception)
            {
                await chatService.SendMessage(chatId, message: "ورودی اشتباه ! لطفا اعداد را درست به صورت نمونه وارد نمایید :");
            }

            return person;
        }

        public static async Task<Person> CheckAddingPortfolioToPortfolioSetIsSuccessful(IChatService chatService, long chatId, Person person, PortfolioSet.Rootobject resObj)
        {
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

            return person;
        }

        public static async Task<PortfolioSet.Rootobject> PostRequestForGetResponse(string commandText, Person person)
        {
            var ids = commandText.Split(" ");
            var parameter = new PortfolioSet.AddPortfolioParameter() { PortfolioSetId = person.PortfolioIdForClassicNextSelect, PortfolioIds = Array.ConvertAll(ids, int.Parse) };
            var json = JsonConvert.SerializeObject(parameter);
            var strContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://192.168.95.88:30907/api/classicNext/portfolioSet/add", strContent).Result.Content.ReadAsStringAsync();
            var resObj = JsonConvert.DeserializeObject<PortfolioSet.Rootobject>(response);
            return resObj;
        }

        private static async Task CreateChartForComparisonPortfolioSetWithETFs(IChatService chatService, long chatId, Person person, NormalPrices.RootObjectETF etfPrices, NormalPrices.RootObject stock)
        {
            var rnd = new Random();
            var configbuilder = @"{
                        'type': 'line',
                        'data': {
					        'labels': [";

            var length = (int)Math.Floor((decimal)etfPrices.responseObject.datesPersian.Length / 60) + 1;
            for (int i = 0; i < etfPrices.responseObject.datesPersian.Length; i += length)
            {
                configbuilder += $"'{etfPrices.responseObject.datesPersian.ElementAt(i)}',";
            }
            configbuilder += @"],'datasets':[";

            configbuilder += @"{'label': '";
            configbuilder += $"پرتفوی مرکب شماره {person.PortfolioIdForClassicNextSelect}";
            var rnd1_ = rnd.Next(0, 255);
            var rnd2_ = rnd.Next(0, 255);
            var rnd3_ = rnd.Next(0, 255);
            configbuilder += $"','borderColor': 'rgb({rnd1_}, {rnd2_}, {rnd3_})','backgroundColor': 'rgb({rnd1_}, {rnd2_}, {rnd3_})','fill': false,'data': [";
            for (int j = 0; j < stock.responseObject.normalPrices.Length; j += length)
            {
                configbuilder += $"{stock.responseObject.normalPrices.ElementAt(j)},";
            }
            configbuilder += @"],},";

            for (int i = 0; i < etfPrices.responseObject.fundsNormalPrices.Length; i++)
            {
                if (i < 5 || i > etfPrices.responseObject.fundsNormalPrices.Length - 6)
                {
                    var etf = etfPrices.responseObject.fundsNormalPrices.ElementAt(i);
                    configbuilder += @"{'label': '";
                    configbuilder += $"{etf.fund.symbol}";
                    var rnd1 = rnd.Next(0, 255);
                    var rnd2 = rnd.Next(0, 255);
                    var rnd3 = rnd.Next(0, 255);
                    configbuilder += $"','borderColor': 'rgb({rnd1}, {rnd2}, {rnd3})','backgroundColor': 'rgb({rnd1}, {rnd2}, {rnd3})','fill': false,'data': [";
                    for (int j = 0; j < etf.normalPrices.Length; j += length)
                    {
                        configbuilder += $"{etf.normalPrices.ElementAt(j)},";
                    }
                    configbuilder += @"],},";
                }
            }

            configbuilder += @"],},'options': {
					                'title': {
						                'display': true,
						                'text': 'مقایسه 5 صندوق بالا و 5 صندوق پایین از نظر بازدهی با پرتفوی مرکب'
					                },
					                },}
                                ";
            var qc = new Chart();
            qc.Width = 1400;
            qc.Height = 900;
            qc.Config = configbuilder;
            await chatService.SendPhotoAsync(chatId, qc.GetUrl());
        }

        public static string CraeteJsonForGetChartPortfolioSet(NormalPrices.RootObject root, Person person)
        {
            var configbuilder = @"{
                    'type': 'line',
                    'data': {
					    'labels': [";

            for (int i = 0; i < root.responseObject.datesPersian.Length; i++)
            {
                if (i != root.responseObject.datesPersian.Length - 1)
                    configbuilder += $"'{root.responseObject.datesPersian.ElementAt(i)}',";
                else
                {
                    configbuilder += $"'{root.responseObject.datesPersian.ElementAt(i)}'],'datasets':[";
                    configbuilder += @"{'label': '";
                    configbuilder += $"پرتفوی مرکب شماره {person.PortfolioIdForClassicNextSelect}";
                    configbuilder += @"',
							    'borderColor': 'rgb(255, 99, 132)',
							    'backgroundColor': 'rgb(255, 99, 132)',
							    'fill': false,
							    'data': [";
                }
            }

            for (int i = 0; i < root.responseObject.normalPrices.Length; i++)
            {
                if (i != root.responseObject.normalPrices.Length - 1)
                    configbuilder += $"{root.responseObject.normalPrices.ElementAt(i)},";
                else
                {
                    configbuilder += $"{root.responseObject.normalPrices.ElementAt(i)}]";
                    configbuilder += @"},],},'options': {
					            'title': {
						            'display': true,
						            'text': 'بازدهی پرتفوی مرکب'
					            },
					            },}
                            ";
                }
            }

            return configbuilder;
        }

        public async static Task<string> CraeteJsonForGetChartComparisonPortfolioSetWithIndex(Task<System.IO.Stream> streamTaskChart1, Task<System.IO.Stream> streamTaskChart2, Person person)
        {
            var root1 = await System.Text.Json.JsonSerializer.DeserializeAsync<NormalPrices.RootObject>(await streamTaskChart1);
            var root2 = await System.Text.Json.JsonSerializer.DeserializeAsync<NormalPrices.RootObject>(await streamTaskChart2);
            if (root1.isSuccessful && root2.isSuccessful)
            {
                var configbuilder = @"{
                        'type': 'line',
                        'data': {
					        'labels': [";


                for (int i = 0; i < root1.responseObject.datesPersian.Length; i++)
                {
                    if (i != root1.responseObject.datesPersian.Length - 1)
                        configbuilder += $"'{root1.responseObject.datesPersian.ElementAt(i)}',";
                    else
                    {
                        configbuilder += $"'{root1.responseObject.datesPersian.ElementAt(i)}'],'datasets':[";
                        configbuilder += @"{'label': '";
                        configbuilder += $"پرتفوی مرکب شماره {person.PortfolioIdForClassicNextSelect}";
                        configbuilder += @"',
							        'borderColor': 'rgb(255, 99, 132)',
							        'backgroundColor': 'rgb(255, 99, 132)',
							        'fill': false,
							        'data': [";
                    }
                }

                for (int i = 0; i < root1.responseObject.normalPrices.Length; i++)
                {
                    if (i != root1.responseObject.normalPrices.Length - 1)
                        configbuilder += $"{root1.responseObject.normalPrices.ElementAt(i)},";
                    else
                    {
                        configbuilder += $"{root1.responseObject.normalPrices.ElementAt(i)}]";
                        configbuilder += @"},{'label': '";
                        configbuilder += $"شاخص";
                        configbuilder += @"',
							        'borderColor': 'rgb(54, 162, 235)',
							        'backgroundColor': 'rgb(54, 162, 235)',
							        'fill': false,
							        'data': [";
                    }
                }

                for (int i = 0; i < root2.responseObject.normalPrices.Length; i++)
                {
                    if (i != root2.responseObject.normalPrices.Length - 1)
                        configbuilder += $"'{root2.responseObject.normalPrices.ElementAt(i)}',";
                    else
                    {
                        configbuilder += $"{root2.responseObject.normalPrices.ElementAt(i)}],";
                    }
                }

                configbuilder += @"},],},'options': {
					                'title': {
						                'display': true,
						                'text': 'مقایسه بازدهی پرتفوی مرکب با شاخص کل'
					                },
					                },}
                                ";
                return configbuilder;
            }
            return null;
        }

        public async static Task<string> CraeteJsonForGetChartComparisonPortfolioSet(Task<System.IO.Stream> streamTaskChart1, Task<System.IO.Stream> streamTaskChart2, Person person)
        {
            var root1 = await System.Text.Json.JsonSerializer.DeserializeAsync<NormalPrices.RootObject>(await streamTaskChart1);
            var root2 = await System.Text.Json.JsonSerializer.DeserializeAsync<NormalPrices.RootObject>(await streamTaskChart2);
            if (root1.isSuccessful && root2.isSuccessful)
            {
                var configbuilder = @"{
                        'type': 'line',
                        'data': {
					        'labels': [";
                var greatRoot = root1;
                var lessRoot = root2;
                var greatId = person.PortfolioIdForClassicNextSelect;
                var lessId = person.StartDateWaitingForEndDate;
                if (root1.responseObject.datesPersian.Length < root2.responseObject.datesPersian.Length)
                {
                    greatRoot = root2;
                    lessRoot = root1;
                    lessId = person.PortfolioIdForClassicNextSelect.ToString();
                    greatId = (long)Convert.ToDouble(person.StartDateWaitingForEndDate);
                }

                for (int i = 0; i < greatRoot.responseObject.datesPersian.Length; i++)
                {
                    if (i != greatRoot.responseObject.datesPersian.Length - 1)
                        configbuilder += $"'{greatRoot.responseObject.datesPersian.ElementAt(i)}',";
                    else
                    {
                        configbuilder += $"'{greatRoot.responseObject.datesPersian.ElementAt(i)}'],'datasets':[";
                        configbuilder += @"{'label': '";
                        configbuilder += $"پرتفوی مرکب شماره {greatId}";
                        configbuilder += @"',
							        'borderColor': 'rgb(255, 99, 132)',
							        'backgroundColor': 'rgb(255, 99, 132)',
							        'fill': false,
							        'data': [";
                    }
                }

                for (int i = 0; i < greatRoot.responseObject.normalPrices.Length; i++)
                {
                    if (i != greatRoot.responseObject.normalPrices.Length - 1)
                        configbuilder += $"{greatRoot.responseObject.normalPrices.ElementAt(i)},";
                    else
                    {
                        configbuilder += $"{greatRoot.responseObject.normalPrices.ElementAt(i)}]";
                        configbuilder += @"},{'label': '";
                        configbuilder += $"پرتفوی مرکب شماره {lessId}";
                        configbuilder += @"',
							        'borderColor': 'rgb(54, 162, 235)',
							        'backgroundColor': 'rgb(54, 162, 235)',
							        'fill': false,
							        'data': [";
                    }
                }

                for (int i = 0; i < greatRoot.responseObject.normalPrices.Length - lessRoot.responseObject.normalPrices.Length; i++)
                {
                    configbuilder += @"null,";
                }

                for (int i = 0; i < lessRoot.responseObject.normalPrices.Length; i++)
                {
                    if (i != lessRoot.responseObject.normalPrices.Length - 1)
                        configbuilder += $"'{lessRoot.responseObject.normalPrices.ElementAt(i)}',";
                    else
                    {
                        configbuilder += $"{lessRoot.responseObject.normalPrices.ElementAt(i)}],";
                    }
                }

                configbuilder += @"},],},'options': {
					                'title': {
						                'display': true,
						                'text': 'مقایسه بازدهی دو پرتفوی مرکب'
					                },
					                },}
                                ";
                return configbuilder;
            }
            return null;
        }
    }
}
