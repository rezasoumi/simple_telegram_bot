using Microsoft.Extensions.DependencyInjection;
using NextBot.Models;
using NextBot.SmartSearch;
using System;
using System.Collections.Generic;
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
            new TelegramBotClient("1634983005:AAHLfFThddFbjkOZAVWK84w1afJePX1Mo8Y");
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
                        SendMessage(message, "نام سهم مورد نظر را وارد کنید :", Markup.ReplyKeyboardMarkup1);
                        break;
                    case "صنعت":
                        person.State = 2;
                        SendMessage(message, "نام صنعت مورد نظر را وارد کنید :", Markup.ReplyKeyboardMarkup1);
                        break;
                    case "پرتفوی مرکب":
                        person.State = 3;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReplyKeyboardMarkup2);
                        break;
                    case "پرتفوی":
                        person.State = 4;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReplyKeyboardMarkup2);
                        break;
                    default:
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReplyKeyboardMarkup0);
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

                person.State = 6;
                SendMessage(message, "صنعت مورد نظر را از گزینه های موجود انتخاب کنید :", new ReplyKeyboardMarkup(buttons, resizeKeyboard: true));
            }
            else if (person.State == 3)
            {

            }
            else if (person.State == 4)
            {
                switch (message.Text)
                {
                    case "تشکیل":
                        person.State = 7;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReplyKeyboardMarkup3);
                        break;
                    case "انتخاب":
                        person.State = 20;
                        SendMessage(message, "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.ReplyKeyboardMarkup9);
                        break;
                    case "بازگشت":
                        person.State = 0;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReplyKeyboardMarkup0);
                        break;
                    default:
                        SendMessage(message, "ورودی اشتباه ! لطفا دوباره تلاش کنید", Markup.ReplyKeyboardMarkup2);
                        break;
                }
            }
            else if (person.State == 5)
            {
                person.State = 0;
                SendMessage(message, "هنوز پیاده سازی نشده است ... ازگزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReplyKeyboardMarkup0);
            }
            else if (person.State == 6)
            {
                person.State = 0;
                SendMessage(message, "هنوز پیاده سازی نشده است ... ازگزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReplyKeyboardMarkup0);
            }
            else if (person.State == 7)
            {
                switch (message.Text)
                {
                    case "هوشمند":
                        person.State = 8;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReplyKeyboardMarkup4);
                        break;
                    case "دستی":
                        //TODO
                        break;
                    case "بازگشت":
                        person.State = 4;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReplyKeyboardMarkup2);
                        break;
                    default:
                        SendMessage(message, "ورودی اشتباه ! لطفا دوباره تلاش کنید", Markup.ReplyKeyboardMarkup3);
                        break;
                }
            }
            else if (person.State == 8)
            {
                switch (message.Text)
                {
                    case "ساخت":
                        SendMessageWithoutReply(message, "ساخت پرتفوی هوشمند برای تاریخ امروز با پارامتر های پیش فرض ...", Markup.ReplyKeyboardMarkup1);
                        var streamTask = client.GetStreamAsync("http://192.168.95.88:30907/api/classicNext/portfolio/create/smart");
                        await SendClassicNextPortfolioToUser(message, streamTask, false);
                        break;
                    case "ساخت + ذخیره پرتفوی":
                        SendMessageWithoutReply(message, "ساخت پرتفوی هوشمند برای تاریخ امروز با پارامتر های پیش فرض به همراه ذخیره سازی پرتفوی ...", Markup.ReplyKeyboardMarkup1);
                        var streamTask_ = client.GetStreamAsync("http://192.168.95.88:30907/api/classicNext/portfolio/create/smart/true");
                        await SendClassicNextPortfolioToUser(message, streamTask_, false);
                        break;
                    case "تنظیمات":
                        person.State = 9;
                        SendMessage(message, "میزان ریسک خود را از بین گزینه های موجود انتخاب کنید :", Markup.ReplyKeyboardMarkup5);
                        break;
                    case "بازگشت":
                        person.State = 7;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReplyKeyboardMarkup3);
                        break;
                    default:
                        SendMessage(message, "ورودی اشتباه ! لطفا دوباره تلاش کنید", Markup.ReplyKeyboardMarkup4);
                        break;
                }
            }
            else if (person.State == 9)
            {
                var valid = true;
                switch (message.Text)
                {
                    case "بدون ریسک":
                        SetRiskRate(person, 0);
                        break;
                    case "ریسک خیلی کم":
                        SetRiskRate(person, 1);
                        break;
                    case "ریسک کم":
                        SetRiskRate(person, 2);
                        break;
                    case "ریسک متوسط":
                        SetRiskRate(person, 3);
                        break;
                    case "ریسک زیاد":
                        SetRiskRate(person, 4);
                        break;
                    case "ریسک خیلی زیاد":
                        SetRiskRate(person, 5);
                        break;
                    case "بازگشت":
                        valid = false;
                        person.State = 8;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReplyKeyboardMarkup4);
                        break;
                    default:
                        valid = false;
                        SendMessage(message, "ورودی اشتباه ! لطفا دوباره تلاش کنید", Markup.ReplyKeyboardMarkup5);
                        break;
                }
                if (valid)
                {
                    person.State = 10;
                    SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReplyKeyboardMarkup6);
                }
            }
            else if (person.State == 10)
            {
                CreateOrEditMoreSetting(person, message, "یک عدد به عنوان حداقل وزن سهام ها بین 0.01 و 1 وارد کنید: (عدد را یه انگلیسی وارد کنید)", 10);
            }
            else if (person.State == 11)
            {
                TakeMaxStockWeight(person, message, false);
            }
            else if (person.State == 12)
            {
                CreateOrEditMoreSetting(person, message, "یک عدد به عنوان حداکثر وزن سهام ها بین 0.05 و 1 وارد کنید: (عدد را یه انگلیسی وارد کنید)", 12);
            }
            else if (person.State == 13)
            {
                TakeMaxStockWeight(person, message, true);
            }
            else if (person.State == 14)
            {
                CreateOrEditMoreSetting(person, message, "تاریخ ساخت خود را به شمسی که بین 13990101 و تاریخ حال حاضر است وارد کنید: (فرمت مورد نظر برای مثال:13990701)", 14);
            }
            else if (person.State == 15)
            {
                if (message.Text.Length == 8 && int.Parse(message.Text) > 13990101)
                {
                    person.SmartPortfolioSetting.ProductionDate = message.Text;
                    person.State = 16;
                    SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReplyKeyboardMarkup7);
                }
                else
                {
                    SendMessage(message, "لطفا یک تاریخ معتبر وارد کنید.", Markup.ReplyKeyboardMarkup1);
                }
            }
            else if (person.State == 16)
            {
                switch (message.Text)
                {
                    case "ساخت":
                        SendMessageWithoutReply(message, "ساخت پرتفوی هوشمند برای تاریخ امروز با پارامتر های پیش فرض و ریسک و حداقل و حداکثر وزن سهام و تاریخ شمسی مشخص ...", Markup.ReplyKeyboardMarkup1);
                        var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/create/smart/false/{person.SmartPortfolioSetting.RiskRate}/{person.SmartPortfolioSetting.MinimumStockWeight}/{person.SmartPortfolioSetting.MaximumStockWeight}/{person.SmartPortfolioSetting.ProductionDate}");
                        await SendClassicNextPortfolioToUser(message, streamTask, false);
                        break;
                    case "ساخت + ذخیره پرتفوی":
                        SendMessageWithoutReply(message, "ساخت پرتفوی هوشمند برای تاریخ امروز با پارامتر های پیش فرض و ریسک و حداقل و حداکثر وزن سهام و تاریخ شمسی مشخص به همراه ذخیره سازی پرتفوی ...", Markup.ReplyKeyboardMarkup1);
                        var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/create/smart/true/{person.SmartPortfolioSetting.RiskRate}/{person.SmartPortfolioSetting.MinimumStockWeight}/{person.SmartPortfolioSetting.MaximumStockWeight}/{person.SmartPortfolioSetting.ProductionDate}");
                        await SendClassicNextPortfolioToUser(message, streamTask_, false);
                        break;
                    case "تنظیمات":
                        person.State = 9;
                        SendMessage(message, "میزان ریسک خود را از بین گزینه های موجود انتخاب کنید :", Markup.ReplyKeyboardMarkup5);
                        break;
                    case "بازگشت":
                        person.State = 15;
                        SendMessage(message, "تاریخ ساخت خود را به شمسی که بین 13990101 و تاریخ حال حاضر است وارد کنید: (فرمت مورد نظر برای مثال:13990701)", Markup.ReplyKeyboardMarkup1);
                        break;
                    default:
                        SendMessage(message, "ورودی اشتباه ! لطفا دوباره تلاش کنید", Markup.ReplyKeyboardMarkup6);
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
                            SendMessage(message, "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.ReplyKeyboardMarkup9);
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
                        SendMessage(message, "گزینه مورد نظر را انتخاب کنید :", Markup.ReplyKeyboardMarkup11);
                        break;
                    case "محاسبه بازدهی":
                        person.State = 21;
                        SendMessage(message, "نوع بازدهی را از بین دو گزینه زیر انتخاب کنید :", Markup.ReplyKeyboardMarkup10);
                        break;
                    case "بازگشت":
                        person.State = 20;
                        SendMessage(message, "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.ReplyKeyboardMarkup9);
                        break;
                    default:
                        SendMessage(message, "ورودی نامعتبر !", Markup.ReplyKeyboardMarkup8);
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
                        SendMessage(message, "آی دی پرتفوی مورد نظر را به صورت یک عدد انگلیسی وارد کنید :", Markup.ReplyKeyboardMarkup1);
                        break;
                    case "انتخاب بر اساس گذر میان پرتفوی ها":
                        person.State = 17;
                        person.ClassicNextSelectState = 1;
                        ShowPreviousOrNextListInClassicNextSelect(person, message);
                        break;
                    case "بازگشت":
                        person.State = 4;
                        SendMessage(message, "از گزینه های موجود یک گزینه را وارد کنید :", Markup.ReplyKeyboardMarkup2);
                        break;
                    default:
                        break;
                }
            }
            else if (person.State == 21)
            {
                switch (message.Text)
                {
                    case "بازدهی پرتفوی تا تاریخ دلخواه":
                        person.State = 22;
                        SendMessage(message, "تاریخ مورد نظر خود را به انگلیسی همانند فرمت نمونه وارد کنید (نمونه : 13991026)", Markup.ReplyKeyboardMarkup1);
                        break;
                    case "بازدهی پرتفوی تا امروز":
                        var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.PorfolioIdForClassicNextSelect}");
                        await ShowReturnAndComparisonInClassicNextSelect(message, streamTask);
                        break;
                    case "بازگشت":
                        person.State = 18;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReplyKeyboardMarkup8);
                        break;
                    default:
                        break;
                }
            }
            else if (person.State == 22)
            {
                if (message.Text.Length == 8)
                {
                    var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.PorfolioIdForClassicNextSelect}/{message.Text}");
                    await ShowReturnAndComparisonInClassicNextSelect(message, streamTask);
                    person.State = 21;
                }
                else
                    SendMessage(message, "لطفا یک تاریخ معتبر وارد کنید :", Markup.ReplyKeyboardMarkup1);
            }
            else if (person.State == 23)
            {
                switch (message.Text)
                {
                    case "شاخص":
                        person.State = 24;
                        SendMessage(message, "نوع بازدهی را مشخص کنید :", Markup.ReplyKeyboardMarkup12);
                        break;
                    case "صندوق سهامی":
                        break;
                    case "پرتفوی":
                        break;
                    case "بازگشت":
                        person.State = 18;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReplyKeyboardMarkup8);
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
                        SendMessage(message, "تاریخ شروع و پایان محاسبه بازدهی را در یک پیام و با یک فاصله با اعداد انگلیسی مانند نمونه وارد کنید : \n(نمونه : 14000420 13991129)", Markup.ReplyKeyboardMarkup1);
                        break;
                    case "بازدهی شاخص تا امروز":
                        person.State = 26;
                        SendMessage(message, "تاریخ شروع محاسبه بازدهی را مانند نمونه وارد کنید : (نمونه : 13991128)", Markup.ReplyKeyboardMarkup1);
                        break;
                    case "بازگشت":
                        person.State = 23;
                        SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReplyKeyboardMarkup11);
                        break;
                    default:
                        break;
                }
            }
            else if (person.State == 25)
            {
                var dates = message.Text.Split(" ");
                if (dates.Length == 2 && dates[0].Length == 8 && dates[1].Length == 8)
                {
                    var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/stock/return/index/{dates[0]}/{dates[1]}");
                    await ShowReturnAndComparisonInClassicNextSelect(message, streamTask);
                    person.State = 24;
                }
                else
                    SendMessage(message, "ورودی نامعتبر! لطفا مجدد تلاش کنید :", Markup.ReplyKeyboardMarkup1);
            }
            else if (person.State == 26)
            {
                if (message.Text.Length == 8)
                {
                    var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/stock/return/index/{message.Text}");
                    await ShowReturnAndComparisonInClassicNextSelect(message, streamTask);
                    person.State = 24;
                }
                else
                    SendMessage(message, "ورودی نامعتبر! لطفا مجدد تلاش کنید :", Markup.ReplyKeyboardMarkup1);
            }
        }

        private static async System.Threading.Tasks.Task ShowReturnAndComparisonInClassicNextSelect(Message message, System.Threading.Tasks.Task<Stream> streamTask)
        {
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.RootobjectForCalculateReturnAndComparison>(await streamTask);
            
            if (root.IsSuccessful)
                SendMessage(message, $"response : {root.ResponseObject}", Markup.ReplyKeyboardMarkup1);
            else
                SendMessage(message, root.ErrorMessageFa, Markup.ReplyKeyboardMarkup1);
            
            SendMessageWithoutReply(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReplyKeyboardMarkup10);
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

                    SendMessageWithoutReply(message, str.ToString(), Markup.ReplyKeyboardMarkup1);
                    Thread.Sleep(500);
                    SendMessage(message, "از بین گزینه های زیر انتخاب کنید :", Markup.ReplyKeyboardMarkup8);

                    person.State = 18;
                    person.PorfolioIdForClassicNextSelect = num;
                }
                else
                {
                    person.State = 20;
                    SendMessage(message, root.ErrorMessageFa, Markup.ReplyKeyboardMarkup1);
                    SendMessage(message, "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.ReplyKeyboardMarkup9);
                }
            }
            catch (Exception)
            {
                person.State = 20;
                SendMessageWithoutReply(message, "ورودی نامعتبر !", Markup.ReplyKeyboardMarkup1);
                SendMessage(message, "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.ReplyKeyboardMarkup9);
            }
        }

        private static async void ShowPreviousOrNextListInClassicNextSelect(Person person, Message message)
        {
            var streamTask = client.GetStreamAsync("http://192.168.95.88:30907/api/classicNext/portfolio/all");
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.Rootobject>(await streamTask);
            
            var list_0 = new List<string> { "قبلی" };
            for (long i = person.ClassicNextSelectState; i <= Math.Min(19 + person.ClassicNextSelectState, root.ResponseObject.Length); i++)
            {
                list_0.Add($"پرتفوی شماره {i}");
            }

            person.ClassicNextSelectState += 20;
            if (root.ResponseObject.Length - person.ClassicNextSelectState > 0)
            {
                list_0.Add("بعدی");
            }
            var buttons_0 = list_0.Select(x => new[] { new KeyboardButton(x) }).ToArray();
            SendMessage(message, "پرتفوی مورد نظر را انتخاب کنید :", new ReplyKeyboardMarkup(buttons_0, resizeKeyboard: true));
        }

        private static void SetRiskRate(Person person, long rate)
        {
            person.SmartPortfolioSetting.RiskRate = rate;
        }

        private async static void CreateOrEditMoreSetting(Person person, Message message, string moreSettingsMessage, long state)
        {
            System.Threading.Tasks.Task<Stream> streamTask = null;

            switch (message.Text)
            {
                case "ساخت":
                    if (state == 10)
                    {
                        SendMessageWithoutReply(message, "ساخت پرتفوی هوشمند برای تاریخ امروز با پارامتر های پیش فرض و ریسک مشخص ...", Markup.ReplyKeyboardMarkup1);
                        streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/create/smart/false/{person.SmartPortfolioSetting.RiskRate}");
                    }
                    if (state == 12)
                    {
                        SendMessageWithoutReply(message, "ساخت پرتفوی هوشمند برای تاریخ امروز با پارامتر های پیش فرض و ریسک و حداقل وزن سهام مشخص ...", Markup.ReplyKeyboardMarkup1);
                        streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/create/smart/false/{person.SmartPortfolioSetting.RiskRate}/{person.SmartPortfolioSetting.MinimumStockWeight}");
                    }
                    if (state == 14)
                    {
                        SendMessageWithoutReply(message, "ساخت پرتفوی هوشمند برای تاریخ امروز با پارامتر های پیش فرض و ریسک و حداقل و حداکثر وزن سهام مشخص ...", Markup.ReplyKeyboardMarkup1);
                        streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/create/smart/false/{person.SmartPortfolioSetting.RiskRate}/{person.SmartPortfolioSetting.MinimumStockWeight}/{person.SmartPortfolioSetting.MaximumStockWeight}");
                    }
                    await SendClassicNextPortfolioToUser(message, streamTask, true);
                    break;
                case "تنظیمات بیشتر":
                    person.State = (state + 1);
                    SendMessage(message, moreSettingsMessage, Markup.ReplyKeyboardMarkup1);
                    break;
                case "ساخت + ذخیره پرتفوی":
                    if (state == 10)
                    {
                        SendMessageWithoutReply(message, "ساخت پرتفوی هوشمند برای تاریخ امروز با پارامتر های پیش فرض و ریسک مشخص به همراه ذخیره پرتفوی ...", Markup.ReplyKeyboardMarkup1);
                        streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/create/smart/true/{person.SmartPortfolioSetting.RiskRate}");
                    }
                    if (state == 12)
                    {
                        SendMessageWithoutReply(message, "ساخت پرتفوی هوشمند برای تاریخ امروز با پارامتر های پیش فرض و ریسک و حداقل وزن سهام مشخص به همراه ذخیره پرتفوی ...", Markup.ReplyKeyboardMarkup1);
                        streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/create/smart/true/{person.SmartPortfolioSetting.RiskRate}/{person.SmartPortfolioSetting.MinimumStockWeight}");
                    }
                    if (state == 14)
                    {
                        SendMessageWithoutReply(message, "ساخت پرتفوی هوشمند برای تاریخ امروز با پارامتر های پیش فرض و ریسک و حداقل و حداکثر وزن سهام مشخص به همراه ذخیره پرتفوی ...", Markup.ReplyKeyboardMarkup1);
                        streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/create/smart/true/{person.SmartPortfolioSetting.RiskRate}/{person.SmartPortfolioSetting.MinimumStockWeight}/{person.SmartPortfolioSetting.MaximumStockWeight}");
                    }
                    await SendClassicNextPortfolioToUser(message, streamTask, true);
                    break;
                case "بازگشت":
                    person.State = 9;
                    SendMessage(message, "میزان ریسک خود را از بین گزینه های موجود انتخاب کنید :", Markup.ReplyKeyboardMarkup5);
                    break;
                default:
                    SendMessage(message, "ورودی اشتباه ! لطفا دوباره تلاش کنید", Markup.ReplyKeyboardMarkup6);
                    break;
            }
        }

        private static async System.Threading.Tasks.Task SendClassicNextPortfolioToUser(Message message, System.Threading.Tasks.Task<Stream> streamTask, bool moreSettings)
        {
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
                    str.Append("weight: " + Math.Round(item.Weight * 100, 2) + " %\n\n");
                }
                SendMessageWithoutReply(message, str.ToString(), Markup.ReplyKeyboardMarkup1);
                Thread.Sleep(500);
                if (!moreSettings)
                    SendMessageWithoutReply(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReplyKeyboardMarkup4);
                else
                    SendMessageWithoutReply(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReplyKeyboardMarkup6);
            }
            else
            {
                SendMessage(message, root.ErrorMessageFa, Markup.ReplyKeyboardMarkup4);
            }
        }

        private static void TakeMaxStockWeight(Person person, Message message, bool max)
        {
            try
            {
                bool isCorrectRangeOfDigit;
                if (max)
                    isCorrectRangeOfDigit = double.Parse(message.Text) >= 0.05 && double.Parse(message.Text) <= 1;
                else
                    isCorrectRangeOfDigit = double.Parse(message.Text) >= 0.01 && double.Parse(message.Text) <= 1;
                if (isCorrectRangeOfDigit)
                {
                    var state = (max) ? 14 : 12;
                    var str = (max) ? "حداکثر وزن سهام ها با موفقیت ثبت شد." : "حداقل وزن سهام ها با موفقیت ثبت شد.";
                    person.State = state;

                    if (max)
                        person.SmartPortfolioSetting.MaximumStockWeight = double.Parse(message.Text);
                    else
                        person.SmartPortfolioSetting.MinimumStockWeight = double.Parse(message.Text);

                    SendMessage(message, str, Markup.ReplyKeyboardMarkup1);
                    SendMessage(message, "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReplyKeyboardMarkup6);
                }
                else
                {
                    SendMessage(message, "لطفا یک عدد صحیح وارد کنید.", Markup.ReplyKeyboardMarkup1);
                }
            }
            catch (Exception)
            {
                SendMessage(message, "لطفا یک عدد انگلیسی وارد کنید.", Markup.ReplyKeyboardMarkup1);
            }
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

        private static async void SendMessage(Message message, string text, ReplyKeyboardMarkup rkm)
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

        private void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
