using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NextBot.Models;
using NextBot.SmartSearch;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace NextBot.Handlers
{
    public class MessageHandler
    {
        private readonly MyDbContext _context;
        public static readonly TelegramBotClient Bot =
            new("1634983005:AAHLfFThddFbjkOZAVWK84w1afJePX1Mo8Y");
        private static readonly HttpClient client = new();

        public MessageHandler(IServiceProvider services)
        {
            var scope = services.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<MyDbContext>();

            Bot.OnMessage += (sender, messageEventArgs) =>
            {
                Console.WriteLine($"{messageEventArgs.Message.Chat.Username} --> {messageEventArgs.Message.Text}");

                Console.WriteLine(
                    $"{messageEventArgs.Message.From.FirstName} sent message {messageEventArgs.Message.MessageId} " +
                    $"to chat {messageEventArgs.Message.Chat.Id} at {messageEventArgs.Message.Date}. ",
                    $"It is a reply to message {messageEventArgs.Message.ReplyToMessage?.MessageId} ",
                    $"and has {messageEventArgs.Message.Entities?.Length} message entities."
                );

                RegisterWithChatId(sender, messageEventArgs);
                BotOnMessageReceived(sender, messageEventArgs);
            };

            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnInlineQuery += BotOnInlineQueryReceived;
            //Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;
            Bot.StartReceiving();
        }

        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var person = _context.People.FirstOrDefault(p => p.ChatId == messageEventArgs.Message.Chat.Id);
            var message = messageEventArgs.Message;

            if (person.State == 0)
            {
                switch (messageEventArgs.Message.Text)
                {
                    case "سهام":
                        person.State = 1;
                        SendMessage(message, "نام سهم مورد نظر را وارد کنید :", Markup.EmptyRKM);
                        break;
                    case "صنعت":
                        person.State = 2;
                        SendMessage(message, "نام صنعت مورد نظر را وارد کنید :", Markup.EmptyRKM);
                        break;
                    case "پرتفوی مرکب":
                        person.State = 6;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.SelectOrCreateRKM);
                        break;
                    case "پرتفوی":
                        person.State = 4;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.SelectOrCreateRKM);
                        break;
                    default:
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.MainMenuRKM);
                        break;
                }
            }
            else if (person.State == 1)
            {
                var streamTask = client.GetStreamAsync("http://192.168.95.88:30907/api/industry/stocks");
                var industries = await System.Text.Json.JsonSerializer.DeserializeAsync<List<IndustryStocks.Industry>>(await streamTask);

                var symbols = new List<String>();
                foreach (var industry in industries)
                {
                    foreach (var stock in industry.Stocks)
                    {
                        symbols.Add(stock.Symbol);
                    }
                }

                var smartDictionary = new SmartDDictionary<string>(m => m, symbols);

                var buttons = smartDictionary.Search(message.Text, 10).Select(x => new[] { new KeyboardButton(x) }).ToArray();

                person.State = 5;
                SendMessage(message, "سهم مورد نظر را از گزینه های موجود انتخاب کنید :", new ReplyKeyboardMarkup(buttons, resizeKeyboard: true));
            }
            else if (person.State == 2)
            {
                var streamTask = client.GetStreamAsync("http://192.168.95.88:30907/api/industry/stocks");
                var industries = await System.Text.Json.JsonSerializer.DeserializeAsync<List<IndustryStocks.Industry>>(await streamTask);

                var industriesName = new List<String>();
                foreach (var industry in industries)
                {
                    industriesName.Add(industry.NameFa);
                }

                var smartDictionary = new SmartDDictionary<string>(m => m, industriesName);

                var buttons = smartDictionary.Search(message.Text, 10).Select(x => new[] { new KeyboardButton(x) }).ToArray();

                person.State = 30;
                SendMessage(message, "صنعت مورد نظر را از گزینه های موجود انتخاب کنید :", new ReplyKeyboardMarkup(buttons, resizeKeyboard: true));
            }
            else if (person.State == 3)
            {
                SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.SelectOrCreateRKM);
                person.State = 6;
            }
            else if (person.State == 4)
            {
                switch (message.Text)
                {
                    case "تشکیل":
                        person.State = 7;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.SmartOrHandMadeRKM);
                        break;
                    case "انتخاب":
                        person.State = 20;
                        SendMessage(message, "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectTypesRKM);
                        break;
                    case "بازگشت":
                        person.State = 0;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.MainMenuRKM);
                        break;
                    default:
                        SendMessage(message, "ورودی اشتباه ! لطفا دوباره تلاش کنید", Markup.SelectOrCreateRKM);
                        break;
                }
            }
            else if (person.State == 5)
            {
                var streamTask = client.GetStreamAsync("http://192.168.95.88:30907/api/industry/stocks");
                var industries = await System.Text.Json.JsonSerializer.DeserializeAsync<List<IndustryStocks.Industry>>(await streamTask);
                SaveTickerKey(person, message, industries);
                person.State = 26;
                SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.StockReturnRKM);
            }
            else if (person.State == 6)
            {
                switch (message.Text)
                {
                    case "تشکیل":
                        var streamTask = client.GetStreamAsync("http://192.168.95.88:30907/api/classicNext/portfolioSet/create");
                        var root = await System.Text.Json.JsonSerializer.DeserializeAsync<PortfolioSet.Rootobject>(await streamTask);
                        if (root.IsSuccessful)
                        {
                            StringBuilder str = new();
                            str.Append($"Id : {root.ResponseObject.Id}\n");
                            str.Append("Persian birthday : " + root.ResponseObject.BirthdayPersian + "\n");
                            str.Append("Stock and weights : \n");
                            for (int i = 0; i < root.ResponseObject.ClassicNextPortfolioSetElements?.Length; i++)
                            {
                                var item = root.ResponseObject.ClassicNextPortfolioSetElements.ElementAt(i);
                                str.Append($"Number {item.ElementNumber}\n");
                                str.Append($"Portfolio Id : {item.PortfolioId}\n");
                                str.Append($"Persian birthday : {item.BirthdayPersian}\n");
                            }
                            SendMessageWithoutReply(message, str.ToString(), Markup.EmptyRKM);
                            Thread.Sleep(500);
                            SendMessageWithoutReply(message, "از بین گزینه های زیر انتخاب کنید :", Markup.SelectOrCreateRKM);
                        }
                        else
                        {
                            SendMessage(message, root.ErrorMessageFa, Markup.EmptyRKM);
                            Thread.Sleep(200);
                            SendMessage(message, "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectOrCreateRKM);
                        }
                        break;
                    case "انتخاب":
                        person.State = 12;
                        SendMessage(message, "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectTypesRKM);
                        break;
                    case "بازگشت":
                        person.State = 0;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.MainMenuRKM);
                        break;
                    default:
                        SendMessage(message, "ورودی اشتباه ! لطفا دوباره تلاش کنید", Markup.SelectOrCreateRKM);
                        break;
                }
            }
            else if (person.State == 7)
            {
                switch (message.Text)
                {
                    case "هوشمند":
                        person.State = 8;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.CreateTypesRKM);
                        break;
                    case "دستی":
                        //TODO
                        break;
                    case "بازگشت":
                        person.State = 4;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.SelectOrCreateRKM);
                        break;
                    default:
                        SendMessage(message, "ورودی اشتباه ! لطفا دوباره تلاش کنید", Markup.SmartOrHandMadeRKM);
                        break;
                }
            }
            else if (person.State == 8)
            {
                switch (message.Text)
                {
                    case "ساخت با پارامتر های پیش فرض":
                        person.State = 10;
                        person.GetSave = false;
                        SendInlineKeyboardSaveQuestion(message);
                        break;
                    case "ساخت با ریسک مشخص":
                        person.State = 11;
                        person.GetSave = false;
                        person.GetRisk = false;
                        SendInlineKeyboardSaveQuestion(message);
                        break;
                    case "ساخت با ریسک و حداقل وزن مشخص":
                        person.State = 13;
                        person.GetSave = false;
                        person.GetRisk = false;
                        person.GetMinimumStockWeight = false;
                        SendInlineKeyboardSaveQuestion(message);
                        break;
                    case "ساخت با ریسک و حداقل و حداکثر وزن مشخص":
                        person.State = 14;
                        person.GetSave = false;
                        person.GetRisk = false;
                        person.GetMinimumStockWeight = false;
                        person.GetMaximumStockWeight = false;
                        SendInlineKeyboardSaveQuestion(message);
                        break;
                    case "ساخت با ریسک و حداقل و حداکثر و تاریخ شمسی مشخص":
                        person.State = 15;
                        person.GetSave = false;
                        person.GetRisk = false;
                        person.GetMinimumStockWeight = false;
                        person.GetMaximumStockWeight = false;
                        person.GetDate = false;
                        SendInlineKeyboardSaveQuestion(message);
                        break;
                    case "بازگشت":
                        person.State = 7;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.SmartOrHandMadeRKM);
                        break;
                    default:
                        SendMessage(message, "ورودی اشتباه ! لطفا دوباره تلاش کنید", Markup.CreateTypesRKM);
                        break;
                }
            }
            else if (person.State == 9)
            {
                var ids = message.Text.Split(" ");
                var parameter = new PortfolioSet.AddPortfolioParameter() { PortfolioSetId = person.PortfolioIdForClassicNextSelect, PortfolioIds = Array.ConvertAll(ids, int.Parse) };
                var json = JsonConvert.SerializeObject(parameter);
                var strContent = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://192.168.95.88:30907/api/classicNext/portfolioSet/add", strContent).Result.Content.ReadAsStringAsync();
                var resObj = JsonConvert.DeserializeObject<PortfolioSet.Rootobject>(response);
                if (resObj.IsSuccessful)
                {
                    SendMessage(message, "افزودن پرتفوی با موفقیت انجام شد", Markup.EmptyRKM);
                    await ShowSpecificPortfolioSetInClassicNextSelect(person, message, person.PortfolioIdForClassicNextSelect.ToString());
                }
                else
                {
                    SendMessage(message, resObj.ErrorMessageFa, Markup.EmptyRKM);
                    SendMessageWithoutReply(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.PortfolioSetSelectRKM);
                    person.State = 29;
                }
            }
            else if (person.State == 10)
            {
                if (!person.GetSave)
                    SendMessageWithoutReply(message, "شما هنوز سوال < پرتفوی مورد نظر ذخیره شود ؟ > را پاسخ نداده اید", Markup.EmptyRKM);
            }
            else if (person.State == 11)
            {
                if (!person.GetSave || !person.GetRisk)
                    SendMessageWithoutReply(message, "لطفا ریسک خود و امکان ذخیره سازی را ابتدا انتخاب کنید", Markup.EmptyRKM);
            }
            else if (person.State == 12)
            {
                switch (message.Text)
                {
                    case "انتخاب بر اساس وارد کردن آی دی پرتفوی مورد نظر":
                        person.State = 28;
                        SendMessage(message, "آی دی پرتفوی مورد نظر را به صورت یک عدد انگلیسی وارد کنید :", Markup.EmptyRKM);
                        break;
                    case "انتخاب بر اساس گذر میان پرتفوی ها":
                        person.State = 16;
                        person.ClassicNextSelectState = 1;
                        ShowPreviousOrNextListInPortfolioSetSelect(person, message);
                        break;
                    case "بازگشت":
                        person.State = 6;
                        SendMessage(message, "از گزینه های موجود یک گزینه را وارد کنید :", Markup.SelectOrCreateRKM);
                        break;
                    default:
                        SendMessage(message, "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectTypesRKM);
                        break;
                }
            }
            else if (person.State == 13)
            {
                if (!person.GetSave || !person.GetRisk)
                    SendMessageWithoutReply(message, "لطفا ریسک خود و امکان ذخیره سازی را ابتدا انتخاب کنید", Markup.EmptyRKM);
                else if (person.GetSave && person.GetRisk && !person.GetMinimumStockWeight && GetMinimumStockWeight(person, message))
                    SendSmartPortfolioToUser(person, message, 2);
            }
            else if (person.State == 14)
            {
                if (!person.GetSave || !person.GetRisk)
                    SendMessageWithoutReply(message, "لطفا ریسک خود و امکان ذخیره سازی را ابتدا انتخاب کنید", Markup.EmptyRKM);
                else if (person.GetSave && person.GetRisk && !person.GetMinimumStockWeight && GetMinimumStockWeight(person, message))
                {
                    SendMessage(message, "یک عدد به عنوان حداکثر وزن سهام ها بین 0.05 و 1 وارد کنید: (عدد را به انگلیسی وارد کنید)", Markup.EmptyRKM);
                    person.GetMinimumStockWeight = true;
                }
                else if (person.GetSave && person.GetRisk && person.GetMinimumStockWeight && !person.GetMaximumStockWeight && GetMaximumStockWeight(person, message))
                    SendSmartPortfolioToUser(person, message, 3);
            }
            else if (person.State == 15)
            {
                if (!person.GetSave || !person.GetRisk)
                    SendMessageWithoutReply(message, "لطفا ریسک خود و امکان ذخیره سازی را ابتدا انتخاب کنید", Markup.EmptyRKM);
                else if (person.GetSave && person.GetRisk && !person.GetMinimumStockWeight && GetMinimumStockWeight(person, message))
                {
                    SendMessage(message, "یک عدد به عنوان حداکثر وزن سهام ها بین 0.05 و 1 وارد کنید: (عدد را به انگلیسی وارد کنید)", Markup.EmptyRKM);
                    person.GetMinimumStockWeight = true;
                }
                else if (person.GetSave && person.GetRisk && person.GetMinimumStockWeight && !person.GetMaximumStockWeight && GetMaximumStockWeight(person, message))
                {
                    SendMessageWithoutReply(message, "تاریخ ساخت خود را به شمسی که بین 13990101 و تاریخ حال حاضر است وارد کنید: (فرمت مورد نظر برای مثال:13990701)", Markup.EmptyRKM);
                    person.GetMaximumStockWeight = true;
                }
                else if (person.GetSave && person.GetRisk && person.GetMinimumStockWeight && person.GetMaximumStockWeight && !person.GetDate && GetProductionDate(person, message))
                    SendSmartPortfolioToUser(person, message, 4);
            }
            else if (person.State == 16)
            {
                switch (message.Text)
                {
                    case "بعدی":
                        ShowPreviousOrNextListInPortfolioSetSelect(person, message);
                        break;
                    case "قبلی":
                        if (person.ClassicNextSelectState == 21)
                        {
                            person.State = 12;
                            SendMessage(message, "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectTypesRKM);
                            break;
                        }
                        person.ClassicNextSelectState -= 40;
                        ShowPreviousOrNextListInPortfolioSetSelect(person, message);
                        break;
                    default:
                        var split = message.Text.Split(" ");
                        var strNum = split[3];
                        await ShowSpecificPortfolioSetInClassicNextSelect(person, message, strNum);
                        break;
                }
            }
            else if (person.State == 17)
            {
                switch (message.Text)
                {
                    case "بعدی":
                        ShowPreviousOrNextListInClassicNextSelect(person, message);
                        break;
                    case "قبلی":
                        if (person.ClassicNextSelectState == 21)
                        {
                            person.State = 20;
                            SendMessage(message, "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectTypesRKM);
                            break;
                        }
                        person.ClassicNextSelectState -= 40;
                        ShowPreviousOrNextListInClassicNextSelect(person, message);
                        break;
                    default:
                        var split = message.Text.Split(" ");
                        var strNum = split[2];
                        await ShowSpecificPortfolioInClassicNextSelect(person, message, strNum);
                        break;
                }
            }
            else if (person.State == 18)
            {
                switch (message.Text)
                {
                    case "مقایسه":
                        person.State = 23;
                        SendMessage(message, "گزینه مورد نظر را انتخاب کنید :", Markup.ComparisonTypesRKM);
                        break;
                    case "محاسبه بازدهی":
                        person.State = 21;
                        SendMessage(message, "نوع بازدهی را از بین دو گزینه زیر انتخاب کنید :", Markup.ReturnPortfolioTypesRKM);
                        break;
                    case "بازگشت":
                        person.State = 20;
                        SendMessage(message, "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectTypesRKM);
                        break;
                    default:
                        SendMessage(message, "ورودی نامعتبر !", Markup.ReturnOrComparisonRKM);
                        break;
                }
            }
            else if (person.State == 19)
            {
                await ShowSpecificPortfolioInClassicNextSelect(person, message, message.Text);
            }
            else if (person.State == 20)
            {
                switch (message.Text)
                {
                    case "انتخاب بر اساس وارد کردن آی دی پرتفوی مورد نظر":
                        person.State = 19;
                        SendMessage(message, "آی دی پرتفوی مورد نظر را به صورت یک عدد انگلیسی وارد کنید :", Markup.EmptyRKM);
                        break;
                    case "انتخاب بر اساس گذر میان پرتفوی ها":
                        person.State = 17;
                        person.ClassicNextSelectState = 1;
                        ShowPreviousOrNextListInClassicNextSelect(person, message);
                        break;
                    case "بازگشت":
                        person.State = 4;
                        SendMessage(message, "از گزینه های موجود یک گزینه را وارد کنید :", Markup.SelectOrCreateRKM);
                        break;
                    default:
                        SendMessage(message, "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectTypesRKM);
                        break;
                }
            }
            else if (person.State == 21)
            {
                switch (message.Text)
                {
                    case "بازدهی پرتفوی تا تاریخ دلخواه":
                        person.State = 22;
                        SendMessage(message, "تاریخ مورد نظر خود را به انگلیسی همانند فرمت نمونه وارد کنید (نمونه : 13991026)", Markup.EmptyRKM);
                        break;
                    case "بازدهی پرتفوی تا امروز":
                        var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.PortfolioIdForClassicNextSelect}");
                        if(await ShowReturnAndComparisonInClassicNextSelect(person, message, streamTask))
                        {
                            SendMessageWithoutReply(message, "بازدهی سهم های موجود در پرتفوی :", Markup.EmptyRKM);
                            await ShowReturnOfEveryStockInPortfolio(person, message);
                        }

                        Thread.Sleep(1000);
                        SendMessageWithoutReply(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnPortfolioTypesRKM);
                        break;
                    case "بازگشت":
                        person.State = 18;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnOrComparisonRKM);
                        break;
                    default:
                        break;
                }
            }
            else if (person.State == 22)
            {
                if (message.Text.Length == 8)
                {
                    var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.PortfolioIdForClassicNextSelect}/{message.Text}");
                    if(await ShowReturnAndComparisonInClassicNextSelect(person, message, streamTask))
                    {
                        SendMessageWithoutReply(message, "بازدهی سهم های موجود در پرتفوی :", Markup.EmptyRKM);
                        await ShowReturnOfEveryStockInPortfolio(person, message);
                    }
                    Thread.Sleep(1000);
                    SendMessageWithoutReply(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnPortfolioTypesRKM);
                    person.State = 21;
                }
                else
                    SendMessage(message, "لطفا یک تاریخ معتبر وارد کنید :", Markup.EmptyRKM);
            }
            else if (person.State == 23)
            {
                switch (message.Text)
                {
                    case "شاخص":
                        person.State = 24;
                        SendMessage(message, "نوع بازدهی را مشخص کنید :", Markup.ReturnIndexTypesRKM);
                        break;
                    case "صندوق سهامی":
                        break;
                    case "پرتفوی":
                        break;
                    case "بازگشت":
                        person.State = 18;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnOrComparisonRKM);
                        break;
                    default:
                        break;
                }
            }
            else if (person.State == 24)
            {
                switch (message.Text)
                {
                    case "بازدهی شاخص تا تاریخ دلخواه":
                        person.State = 25;
                        SendMessage(message, "تاریخ مورد نظر خود را به انگلیسی همانند فرمت نمونه وارد کنید(نمونه: 13991026)", Markup.EmptyRKM);
                        break;
                    case "بازدهی شاخص تا امروز":
                        var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.PortfolioIdForClassicNextSelect}");
                        if (await ShowReturnAndComparisonInClassicNextSelect(person, message, streamTask))
                        {
                            var date = await GetBithdayOfPortfolio(person);
                            var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/stock/return/index/{date}");
                            await ShowIndextReturnInClassicNextSelect(message, streamTask_);
                        }
                        Thread.Sleep(500);
                        SendMessageWithoutReply(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnIndexTypesRKM);
                        break;
                    case "بازگشت":
                        person.State = 23;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonTypesRKM);
                        break;
                    default:
                        break;
                }
            }
            else if (person.State == 25)
            {
                if (message.Text.Length == 8)
                {
                    var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.PortfolioIdForClassicNextSelect}/{message.Text}");
                    if (await ShowReturnAndComparisonInClassicNextSelect(person, message, streamTask))
                    {
                        var date = await GetBithdayOfPortfolio(person);
                        var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/stock/return/index/{date}/{message.Text}");
                        await ShowIndextReturnInClassicNextSelect(message, streamTask_);
                    }
                    Thread.Sleep(500);
                    SendMessageWithoutReply(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnIndexTypesRKM);
                    person.State = 24;
                }
                else
                    SendMessage(message, "ورودی نامعتبر! لطفا مجدد تلاش کنید :", Markup.EmptyRKM);
            }
            else if (person.State == 26)
            {
                switch (message.Text)
                {
                    case "محاسبه بازدهی":
                        person.State = 27;
                        SendMessage(message, "تاریخ شروع و پایان محاسبه بازدهی را به انگلیسی همانند فرمت نمونه وارد کنید(نمونه:14000321 13991026)", Markup.EmptyRKM);
                        break;
                    case "بازگشت":
                        person.State = 0;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.MainMenuRKM);
                        break;
                    default:
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.StockReturnRKM);
                        break;
                }
            }
            else if (person.State == 27)
            {
                if (message.Text.Length == 17)
                {
                    var dates = message.Text.Split(" ");
                    var parameter = new StockReturn.StockReturnParameterWithEndDate() { BeginDatePersian = int.Parse(dates[0]), EndDatePersian = int.Parse(dates[1]), TickerKeys = new int[] { person.TickerKeyForStock } };
                    var json = JsonConvert.SerializeObject(parameter);
                    var strContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("http://192.168.95.88:30907/api/stock/returns", strContent).Result.Content.ReadAsStringAsync();
                    var resObj = JsonConvert.DeserializeObject<StockReturn.StockReturnRoot>(response);
                    if (resObj.IsSuccessful)
                        SendMessage(message, "بازدهی سهام مورد نظر در این بازه زمانی :" + "\n" + Math.Round(resObj.ResponseObject.First() * 100, 1) + " %", Markup.EmptyRKM);
                    else
                        SendMessage(message, resObj.ErrorMessageFa, Markup.EmptyRKM);
                    Thread.Sleep(200);
                    SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.StockReturnRKM);
                    person.State = 26;
                }
                else
                    SendMessage(message, "ورودی نامعتبر! لطفا مجدد تلاش کنید :", Markup.EmptyRKM);
            }
            else if (person.State == 28)
            {
                await ShowSpecificPortfolioSetInClassicNextSelect(person, message, message.Text);
            }
            else if (person.State == 29)
            {
                switch (message.Text)
                {
                    case "افزودن پرتفوی":
                        person.State = 9;
                        SendMessage(message, "آی دی پرتفوی های مورد نظر را با یک فاصله به صورت انگلیسی وارد نمایید : (نمونه : 13 15 23)", Markup.EmptyRKM);
                        break;
                    case "حذف پرتفوی":
                        break;
                    case "محاسبه بازدهی":
                        break;
                    case "مقایسه":
                        break;
                    case "بازگشت":
                        person.State = 12;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.SelectTypesRKM);
                        break;
                    default:
                        break;
                }
            }
            else if (person.State == 30)
            {
                person.State = 0;
                SendMessage(message, "پیاده سازی نشده است", Markup.MainMenuRKM);
            }
        }

        private static void SaveTickerKey(Person person, Message message, List<IndustryStocks.Industry> industries)
        {
            var end = false;
            foreach (var industry in industries)
            {
                foreach (var stock in industry.Stocks)
                {
                    if (stock.Symbol == message.Text)
                    {
                        person.TickerKeyForStock = stock.TickerKey;
                        end = true;
                        break;
                    }
                    if (end)
                        break;
                }

            }
        }

        private static void SendInlineKeyboardSaveQuestion(Message message)
        {
            InlineKeyboardMarkup inlineKeyboard = new(new InlineKeyboardButton[][]
                                    {
                            new InlineKeyboardButton[]{
                                InlineKeyboardButton.WithCallbackData("بلی", "بلی"),
                                InlineKeyboardButton.WithCallbackData("خیر", "خیر"),
                            }
                                    });
            SendMessage(message, "پرتفوی مورد نظر ذخیره شود ؟", inlineKeyboard);
        }

        private static async System.Threading.Tasks.Task<string> GetBithdayOfPortfolio(Person person)
        {
            var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/{person.PortfolioIdForClassicNextSelect}");
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.RootObjectForSpecificPortfolio>(await streamTask);
            var str = root.ResponseObject.BirthdayPersian;
            return str.Replace("/", "");
        }

        private static bool GetProductionDate(Person person, Message message)
        {
            try
            {
                if (message.Text.Length == 8 && double.Parse(message.Text) > 13990101)
                {
                    person.SmartPortfolioSetting.ProductionDate = message.Text;
                    return true;
                }
                else
                    SendMessage(message, "لطفا یک تاریخ معتبر وارد نمایید :", Markup.EmptyRKM);
            }
            catch (Exception)
            {
                SendMessage(message, "ورودی نامعتبر! لطفا دوباره تلاش کنید", Markup.EmptyRKM);
            }
            return false;
        }

        private static bool GetMinimumStockWeight(Person person, Message message)
        {
            try
            {
                if (double.Parse(message.Text) > 0.01 && double.Parse(message.Text) < 1)
                {
                    person.SmartPortfolioSetting.MinimumStockWeight = double.Parse(message.Text);
                    return true;
                }
                else
                    SendMessage(message, "لطفا یک عدد در بازه گفته شده وارد نمایید :", Markup.EmptyRKM);
            }
            catch (Exception)
            {
                SendMessage(message, "ورودی نامعتبر! لطفا دوباره تلاش کنید", Markup.EmptyRKM);
            }
            return false;
        }
        private static bool GetMaximumStockWeight(Person person, Message message)
        {
            try
            {
                if (double.Parse(message.Text) > 0.05 && double.Parse(message.Text) < 1)
                {
                    person.SmartPortfolioSetting.MaximumStockWeight = double.Parse(message.Text);
                    return true;
                }
                else
                    SendMessage(message, "لطفا یک عدد در بازه گفته شده وارد نمایید :", Markup.EmptyRKM);
            }
            catch (Exception)
            {
                SendMessage(message, "ورودی نامعتبر! لطفا دوباره تلاش کنید", Markup.EmptyRKM);
            }
            return false;
        }

        private static async System.Threading.Tasks.Task ShowReturnOfEveryStockInPortfolio(Person person, Message message)
        {
            var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/{person.PortfolioIdForClassicNextSelect}");
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.RootObjectForSpecificPortfolio>(await streamTask);
            var tickerKeys = new List<int>();
            var symbols = new List<string>();
            foreach (var item in root.ResponseObject.StockAndWeights)
            {
                tickerKeys.Add(item.Stock.TickerKey);
                symbols.Add(item.Stock.TickerPooyaFa);
            }
            string json;
            if (person.State == 21)
            {
                var parameter = new StockReturn.StockReturnParameter() { BeginDatePersian = int.Parse(await GetBithdayOfPortfolio(person)), TickerKeys = tickerKeys.ToArray() };
                json = JsonConvert.SerializeObject(parameter);
            }
            else
            {
                var parameter = new StockReturn.StockReturnParameterWithEndDate() { BeginDatePersian = int.Parse(await GetBithdayOfPortfolio(person)), TickerKeys = tickerKeys.ToArray(), EndDatePersian = int.Parse(message.Text) };
                json = JsonConvert.SerializeObject(parameter);
            }
            var strContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://192.168.95.88:30907/api/stock/returns", strContent).Result.Content.ReadAsStringAsync();
            var resObj = JsonConvert.DeserializeObject<StockReturn.StockReturnRoot>(response);
            foreach (var item in resObj.ResponseObject)
            {
                SendMessageWithoutReply(message, $"بازدهی سهم {symbols.First()} در این بازه زمانی :" + "\n" + Math.Round(item * 100, 1) + " %", Markup.EmptyRKM);
                symbols.RemoveAt(0);
            }
        }

        private static async System.Threading.Tasks.Task<bool> ShowReturnAndComparisonInClassicNextSelect(Person person, Message message, System.Threading.Tasks.Task<Stream> streamTask)
        {
            SendMessageWithoutReply(message, "تاریخ شروع همان تاریخ ساخت پرتفوی مورد نظر می باشد ...", Markup.EmptyRKM);
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.RootobjectForCalculateReturnAndComparison>(await streamTask);
            if (root.IsSuccessful)
            {
                SendMessage(message, $"بازدهی پرتفوی شماره  {person.PortfolioIdForClassicNextSelect} در مجموع : " + "\n" + Math.Round(Convert.ToDecimal(root.ResponseObject) * 100, 1) + " %", Markup.EmptyRKM);
                Thread.Sleep(500);
                return true;
            }
            else
            {
                SendMessage(message, root.ErrorMessageFa, Markup.EmptyRKM);
                Thread.Sleep(500);
                return false;
            }
        }

        private static async System.Threading.Tasks.Task ShowIndextReturnInClassicNextSelect(Message message, System.Threading.Tasks.Task<Stream> streamTask)
        {
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.RootobjectForCalculateReturnAndComparison>(await streamTask);
            if (root.IsSuccessful)
                SendMessage(message, $"بازدهی شاخص تا همین زمان : " + "\n" + Math.Round(Convert.ToDecimal(root.ResponseObject) * 100, 1) + " %", Markup.EmptyRKM);
            else
                SendMessage(message, root.ErrorMessageFa, Markup.EmptyRKM);
            Thread.Sleep(300);
        }

        private static async System.Threading.Tasks.Task ShowSpecificPortfolioInClassicNextSelect(Person person, Message message, String strNumber)
        {
            try
            {
                var num = int.Parse(strNumber.Trim());

                var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/{num}");
                var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.RootObjectForSpecificPortfolio>(await streamTask);
                
                if (root.IsSuccessful)
                {
                    StringBuilder str = new();
                    str.Append($"id : {root.ResponseObject.Id}\n");
                    str.Append("birthday : " + root.ResponseObject.Birthday + "\n");
                    str.Append("persian birthday : " + root.ResponseObject.BirthdayPersian + "\n");

                    SendMessageWithoutReply(message, str.ToString(), Markup.EmptyRKM);
                    Thread.Sleep(500);
                    SendMessageWithoutReply(message, "از بین گزینه های زیر انتخاب کنید :", Markup.ReturnOrComparisonRKM);

                    person.State = 18;
                    person.PortfolioIdForClassicNextSelect = num;
                }
                else
                {
                    person.State = 20;
                    SendMessage(message, root.ErrorMessageFa, Markup.EmptyRKM);
                    Thread.Sleep(200);
                    SendMessage(message, "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectTypesRKM);
                }
            }
            catch (Exception)
            {
                person.State = 20;
                SendMessageWithoutReply(message, "ورودی نامعتبر !", Markup.EmptyRKM);
                SendMessage(message, "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectTypesRKM);
            }
        }
        private static async System.Threading.Tasks.Task ShowSpecificPortfolioSetInClassicNextSelect(Person person, Message message, String strNumber)
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
                        str.Append($"Persian birthday : {item.BirthdayPersian}\n\n" );
                    }
                    SendMessageWithoutReply(message, str.ToString(), Markup.EmptyRKM);
                    Thread.Sleep(500);
                    SendMessageWithoutReply(message, "از بین گزینه های زیر انتخاب کنید :", Markup.PortfolioSetSelectRKM);

                    person.State = 29;
                    person.PortfolioIdForClassicNextSelect = num;
                }
                else
                {
                    person.State = 12;
                    SendMessage(message, root.ErrorMessageFa, Markup.EmptyRKM);
                    Thread.Sleep(200);
                    SendMessage(message, "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectTypesRKM);
                }
            }
            catch (Exception)
            {
                person.State = 12;
                SendMessageWithoutReply(message, "ورودی نامعتبر !", Markup.EmptyRKM);
                SendMessage(message, "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectTypesRKM);
            }
        }

        private static async void ShowPreviousOrNextListInClassicNextSelect(Person person, Message message)
        {
            var streamTask = client.GetStreamAsync("http://192.168.95.88:30907/api/classicNext/portfolio/all");
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.Rootobject>(await streamTask);
            var list_ = new List<int> { };
            foreach (var item in root.ResponseObject)
            {
                list_.Add(item.Id);
            }
            list_.Reverse();

            var list = new List<string> { "قبلی" };
            for (long i = person.ClassicNextSelectState; i < Math.Min(20 + person.ClassicNextSelectState, root.ResponseObject.Length); i++)
            {
                list.Add($"پرتفوی شماره {list_.ElementAt(Convert.ToInt32(i))}");
            }

            person.ClassicNextSelectState += 20;
            if (root.ResponseObject.Length - person.ClassicNextSelectState > 0)
            {
                list.Add("بعدی");
            }
            var buttons = list.Select(x => new[] { new KeyboardButton(x) }).ToArray();
            SendMessage(message, "پرتفوی مورد نظر را انتخاب کنید :", new ReplyKeyboardMarkup(buttons, resizeKeyboard: true));
        }
        
        private static async void ShowPreviousOrNextListInPortfolioSetSelect(Person person, Message message)
        {
            var streamTask = client.GetStreamAsync("http://192.168.95.88:30907/api/classicNext/portfolioSet/all");
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.Rootobject>(await streamTask);
            var list_ = new List<int> { };
            foreach (var item in root.ResponseObject)
            {
                list_.Add(item.Id);
            }
            list_.Reverse();

            var list = new List<string> { "قبلی" };
            for (long i = person.ClassicNextSelectState; i < Math.Min(20 + person.ClassicNextSelectState, root.ResponseObject.Length); i++)
            {
                list.Add($"پرتفوی مرکب شماره {list_.ElementAt(Convert.ToInt32(i))}");
            }

            person.ClassicNextSelectState += 20;
            if (root.ResponseObject.Length - person.ClassicNextSelectState > 0)
            {
                list.Add("بعدی");
            }
            var buttons = list.Select(x => new[] { new KeyboardButton(x) }).ToArray();
            SendMessage(message, "پرتفوی مرکب مورد نظر را انتخاب کنید :", new ReplyKeyboardMarkup(buttons, resizeKeyboard: true));
        }

        private void RegisterWithChatId(object sender, MessageEventArgs messageEventArgs)
        {
            var person1 = _context.People.Where(p => p.ChatId == messageEventArgs.Message.Chat.Id);

            if (!person1.Any())
            {
                _context.People.Add(new Person()
                {
                    State = 0,
                    ChatId = messageEventArgs.Message.Chat.Id
                });
            }

            _context.SaveChanges();
        }

        private static async void SendMessage(Message message, string text, IReplyMarkup rkm)
        {
            await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: text,
                    replyToMessageId: message.MessageId,
                    replyMarkup: rkm
                );
        }

        private static async void SendMessageWithoutReply(Message message, string text, ReplyKeyboardMarkup rkm)
        {
            await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: text,
                    replyMarkup: rkm
                );
        }

        private void BotOnReceiveError(object sender, ReceiveErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var person = _context.People.FirstOrDefault(p => p.ChatId == callbackQueryEventArgs.CallbackQuery.Message.Chat.Id);

            var callbackQuery = callbackQueryEventArgs.CallbackQuery;
            await Bot.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: $"{callbackQueryEventArgs.CallbackQuery.Data} دریافت شد",
                showAlert: false,
                url: null
            );

            if (callbackQuery.Data.Equals("خیر"))
            {
                person.GetSave = true;
                person.SmartPortfolioSetting.Save = false;

                if (person.State == 10)
                    SendSmartPortfolioToUser(person, callbackQueryEventArgs.CallbackQuery.Message, 0);
                if (person.State == 11 || person.State == 13 || person.State == 14 || person.State == 15)
                    GetRiskByInlineKeyboard(callbackQueryEventArgs.CallbackQuery.Message);
            }
            else if (callbackQuery.Data.Equals("بلی"))
            {
                person.GetSave = true;
                person.SmartPortfolioSetting.Save = true;

                if (person.State == 10)
                    SendSmartPortfolioToUser(person, callbackQueryEventArgs.CallbackQuery.Message, 0);
                if (person.State == 11 || person.State == 13 || person.State == 14 || person.State == 15)
                    GetRiskByInlineKeyboard(callbackQueryEventArgs.CallbackQuery.Message);
            }
            else if (callbackQuery.Data.Equals("بدون ریسک"))
            {
                person.SmartPortfolioSetting.RiskRate = 0;
                person.GetRisk = true;

                if (person.State == 11)
                    SendSmartPortfolioToUser(person, callbackQueryEventArgs.CallbackQuery.Message, 1);
                if (person.State == 13 || person.State == 14 || person.State == 15)
                    SendMessageWithoutReply(callbackQueryEventArgs.CallbackQuery.Message, "یک عدد به عنوان حداقل وزن سهام ها بین 0.01 و 1 وارد کنید: (عدد را به انگلیسی وارد کنید)", Markup.EmptyRKM);
            }
            else if (callbackQuery.Data.Equals("ریسک خیلی کم"))
            {
                person.SmartPortfolioSetting.RiskRate = 1;
                person.GetRisk = true;

                if (person.State == 11)
                    SendSmartPortfolioToUser(person, callbackQueryEventArgs.CallbackQuery.Message, 1);
                if (person.State == 13 || person.State == 14 || person.State == 15)
                    SendMessageWithoutReply(callbackQueryEventArgs.CallbackQuery.Message, "یک عدد به عنوان حداقل وزن سهام ها بین 0.01 و 1 وارد کنید: (عدد را به انگلیسی وارد کنید)", Markup.EmptyRKM);
            }
            else if (callbackQuery.Data.Equals("ریسک کم"))
            {
                person.SmartPortfolioSetting.RiskRate = 2;
                person.GetRisk = true;

                if (person.State == 11)
                    SendSmartPortfolioToUser(person, callbackQueryEventArgs.CallbackQuery.Message, 1);
                if (person.State == 13 || person.State == 14 || person.State == 15)
                    SendMessageWithoutReply(callbackQueryEventArgs.CallbackQuery.Message, "یک عدد به عنوان حداقل وزن سهام ها بین 0.01 و 1 وارد کنید: (عدد را به انگلیسی وارد کنید)", Markup.EmptyRKM);
            }
            else if (callbackQuery.Data.Equals("ریسک متوسط"))
            {
                person.SmartPortfolioSetting.RiskRate = 3;
                person.GetRisk = true;

                if (person.State == 11)
                    SendSmartPortfolioToUser(person, callbackQueryEventArgs.CallbackQuery.Message, 1);
                if (person.State == 13 || person.State == 14 || person.State == 15)
                    SendMessageWithoutReply(callbackQueryEventArgs.CallbackQuery.Message, "یک عدد به عنوان حداقل وزن سهام ها بین 0.01 و 1 وارد کنید: (عدد را به انگلیسی وارد کنید)", Markup.EmptyRKM);
            }
            else if (callbackQuery.Data.Equals("ریسک زیاد"))
            {
                person.SmartPortfolioSetting.RiskRate = 4;
                person.GetRisk = true;

                if (person.State == 11)
                    SendSmartPortfolioToUser(person, callbackQueryEventArgs.CallbackQuery.Message, 1);
                if (person.State == 13 || person.State == 14 || person.State == 15)
                    SendMessageWithoutReply(callbackQueryEventArgs.CallbackQuery.Message, "یک عدد به عنوان حداقل وزن سهام ها بین 0.01 و 1 وارد کنید: (عدد را به انگلیسی وارد کنید)", Markup.EmptyRKM);
            }
            else if (callbackQuery.Data.Equals("ریسک خیلی زیاد"))
            {
                person.SmartPortfolioSetting.RiskRate = 5;
                person.GetRisk = true;

                if (person.State == 11)
                    SendSmartPortfolioToUser(person, callbackQueryEventArgs.CallbackQuery.Message, 1);
                if (person.State == 13 || person.State == 14 || person.State == 15)
                    SendMessageWithoutReply(callbackQueryEventArgs.CallbackQuery.Message, "یک عدد به عنوان حداقل وزن سهام ها بین 0.01 و 1 وارد کنید: (عدد را به انگلیسی وارد کنید)", Markup.EmptyRKM);
            }
        }

        private static void GetRiskByInlineKeyboard(Message message)
        {
            InlineKeyboardMarkup inlineKeyboard = new(new InlineKeyboardButton[][]
                {
                    new InlineKeyboardButton[]{
                        InlineKeyboardButton.WithCallbackData("ریسک متوسط", "ریسک متوسط"),
                        InlineKeyboardButton.WithCallbackData("بدون ریسک", "بدون ریسک"),
                    },
                    new InlineKeyboardButton[]{
                        InlineKeyboardButton.WithCallbackData("ریسک زیاد", "ریسک زیاد"),
                        InlineKeyboardButton.WithCallbackData("ریسک خیلی کم", "ریسک خیلی کم"),
                    },
                    new InlineKeyboardButton[]{
                        InlineKeyboardButton.WithCallbackData("ریسک خیلی زیاد","ریسک خیلی زیاد"),
                        InlineKeyboardButton.WithCallbackData("ریسک کم", "ریسک کم"),
                    },
            });
            SendMessage(message, "ریسک مورد نظر خود را انتخاب کنید :", inlineKeyboard);
        }

        private static async void SendSmartPortfolioToUser(Person person, Message message, int s)
        {
            SendMessageWithoutReply(message, "در حال ساخت پرتفوی مورد نظر ...", Markup.EmptyRKM);
            System.Threading.Tasks.Task<Stream> streamTask = null;
            if (s == 0)
                streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/create/smart/{person.SmartPortfolioSetting.Save}");
            else if (s == 1)
                streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/create/smart/{person.SmartPortfolioSetting.Save}/{person.SmartPortfolioSetting.RiskRate}");
            else if (s == 2)
                streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/create/smart/{person.SmartPortfolioSetting.Save}/{person.SmartPortfolioSetting.RiskRate}/{person.SmartPortfolioSetting.MinimumStockWeight}");
            else if (s == 3)
                streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/create/smart/{person.SmartPortfolioSetting.Save}/{person.SmartPortfolioSetting.RiskRate}/{person.SmartPortfolioSetting.MinimumStockWeight}/{person.SmartPortfolioSetting.MaximumStockWeight}");
            else if (s == 4)
                streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/create/smart/{person.SmartPortfolioSetting.Save}/{person.SmartPortfolioSetting.RiskRate}/{person.SmartPortfolioSetting.MinimumStockWeight}/{person.SmartPortfolioSetting.MaximumStockWeight}/{person.SmartPortfolioSetting.ProductionDate}");

            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextFormation.RootObject>(await streamTask);

            if (root.IsSuccessful)
            {
                StringBuilder str = new();
                for (int i = 0; i < root.ResponseObject.StockAndWeights.Length; i++)
                {
                    var item = root.ResponseObject.StockAndWeights.ElementAt(i);
                    str.Append($"number {i + 1} :\n");
                    str.Append("tickerPooyaFa: " + item.Stock.TickerPooyaFa + "\n");
                    str.Append("tickerNamePooyaFa: " + item.Stock.TickerNamePooyaFa + "\n");
                    str.Append("marketType: " + item.Stock.MarketType + "\n");
                    str.Append("weight: " + Math.Round(item.Weight * 100, 1) + " %\n\n");
                }
                Thread.Sleep(500);
                SendMessageWithoutReply(message, str.ToString(), Markup.EmptyRKM);
                Thread.Sleep(500);

                person.State = 8;
                SendMessageWithoutReply(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.CreateTypesRKM);
            }
            else
            {
                person.State = 8;
                SendMessage(message, root.ErrorMessageFa, Markup.EmptyRKM);
                Thread.Sleep(200);
                SendMessageWithoutReply(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.CreateTypesRKM);
            }
        }
    }
}
