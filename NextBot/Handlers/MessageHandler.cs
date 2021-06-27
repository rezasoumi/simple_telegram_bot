using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NextBot.Models;
using NextBot.SmartSearch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace NextBot.Handlers
{
    public class MessageHandler
    {
        private readonly MyDbContext _context;
        private static WebProxy proxy = new WebProxy("https://t.me/proxy?server=194.5.193.32&port=443&secret=ee0dc65c949cdcc6b4a70d61db96e9e8367777772e636c6f7564666c6172652e636f6d");
        public static readonly TelegramBotClient Bot =
            new TelegramBotClient("1634983005:AAHLfFThddFbjkOZAVWK84w1afJePX1Mo8Y", proxy);

        private static readonly HttpClient client = new HttpClient();

        private static readonly ReplyKeyboardMarkup ReplyKeyboardMarkup0 = new ReplyKeyboardMarkup(
           new KeyboardButton[][]
           {
                    new KeyboardButton[] { "سهام", "صنعت", "پرتفوی مرکب", "پرتفوی"}
           },
           resizeKeyboard: true
       );

        private static readonly ReplyKeyboardMarkup ReplyKeyboardMarkup1 = new ReplyKeyboardMarkup(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] {""},
            },
            resizeKeyboard: true
        );
        
        private static readonly ReplyKeyboardMarkup ReplyKeyboardMarkup2 = new ReplyKeyboardMarkup(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "بازگشت" },
                    new KeyboardButton[] { "تشکیل", "انتخاب" },
            },
            resizeKeyboard: true
        );
        
        private static readonly ReplyKeyboardMarkup ReplyKeyboardMarkup3 = new ReplyKeyboardMarkup(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "بازگشت" },
                    new KeyboardButton[] { "هوشمند", "دستی" },
            },
            resizeKeyboard: true
        );
        
        private static readonly ReplyKeyboardMarkup ReplyKeyboardMarkup4 = new ReplyKeyboardMarkup(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "بازگشت" },
                    new KeyboardButton[] { "تنظیمات", "ساخت" },
            },
            resizeKeyboard: true
        );
        
        private static readonly ReplyKeyboardMarkup ReplyKeyboardMarkup5 = new ReplyKeyboardMarkup(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "ریسک متوسط", "بدون ریسک" },
                    new KeyboardButton[] { "ریسک زیاد", "ریسک خیلی کم" },
                    new KeyboardButton[] { "ریسک خیلی زیاد", "ریسک کم" },
                    new KeyboardButton[] { "بازگشت" },
            },
            resizeKeyboard: true
        );

        private static readonly ReplyKeyboardMarkup ReplyKeyboardMarkup6 = new ReplyKeyboardMarkup(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "بازگشت" },
                    new KeyboardButton[] { "تنظیمات بیشتر" , "ساخت" },
            },
            resizeKeyboard: true
        );

        private static readonly ReplyKeyboardMarkup ReplyKeyboardMarkup7 = new ReplyKeyboardMarkup(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "بازگشت", "ساخت" },
            },
            resizeKeyboard: true
        );


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
                        SendMessage(message, "send your stock name:", ReplyKeyboardMarkup1);
                        break;
                    case "صنعت":
                        person.State = 2;
                        SendMessage(message, "send your industry name:", ReplyKeyboardMarkup1);
                        break;
                    case "پرتفوی مرکب":
                        person.State = 3;
                        SendMessage(message, "choose:", ReplyKeyboardMarkup2);
                        break;
                    case "پرتفوی":
                        person.State = 4;
                        SendMessage(message, "choose:", ReplyKeyboardMarkup2);
                        break;
                    default:
                        SendMessage(message, "ar", ReplyKeyboardMarkup1);
                        break;
                }
            }
            else if (person.State == 1)
            {
                var streamTask = client.GetStreamAsync("http://192.168.95.88:30907/api/industry/stocks");
                var industries = await System.Text.Json.JsonSerializer.DeserializeAsync<List<IndustryStocks.Industry>>(await streamTask);

                //StreamReader r = new StreamReader("json.json");
                //string json = r.ReadToEnd();
                //var industries = JsonConvert.DeserializeObject<List<IndustryStocks.Industry>>(json);

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
                SendMessage(message, "enter stock you want ...", new ReplyKeyboardMarkup(buttons, resizeKeyboard: true));
            }
            else if (person.State == 2)
            {
                var streamTask = client.GetStreamAsync("http://192.168.95.88:30907/api/industry/stocks");
                var industries = await System.Text.Json.JsonSerializer.DeserializeAsync<List<IndustryStocks.Industry>>(await streamTask);

                //StreamReader r = new StreamReader("json.json");
                //string json = r.ReadToEnd();
                //var industries = JsonConvert.DeserializeObject<List<IndustryStocks.Industry>>(json);
                
                var industriesName = new List<String>();
                foreach (var industry in industries)
                {
                    industriesName.Add(industry.NameFa);
                }

                var smartDictionary = new SmartDDictionary<string>(m => m, industriesName);

                var buttons = smartDictionary.Search(message.Text, 10).Select(x => new[] { new KeyboardButton(x) }).ToArray();

                person.State = 6;
                SendMessage(message, "enter stock you want ...", new ReplyKeyboardMarkup(buttons, resizeKeyboard: true));
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
                        SendMessage(message, "choose:", ReplyKeyboardMarkup3);
                        break;
                    case "انتخاب":

                        break;
                    case "بازگشت":
                        person.State = 0;
                        SendMessage(message, "choose:", ReplyKeyboardMarkup0);
                        break;
                    default:
                        SendMessage(message, "you say sth wrong try again", ReplyKeyboardMarkup2);
                        break;
                }
            }
            else if (person.State == 5)
            {
                person.State = 0;
                SendMessage(message, "has not implemented yet... choose:", ReplyKeyboardMarkup0);
            }
            else if (person.State == 6)
            {
                person.State = 0;
                SendMessage(message, "has not implemented yet... choose:", ReplyKeyboardMarkup0);
            }
            else if (person.State == 7)
            {
                switch (message.Text)
                {
                    case "هوشمند":
                        person.State = 8;
                        SendMessage(message, "choose:", ReplyKeyboardMarkup4);
                        break;
                    case "دستی":

                        break;
                    case "بازگشت":
                        person.State = 4;
                        SendMessage(message, "choose:", ReplyKeyboardMarkup2);
                        break;
                    default:
                        SendMessage(message, "you say sth wrong try again", ReplyKeyboardMarkup3);
                        break;
                }
            }
            else if (person.State == 8)
            {
                switch (message.Text)
                {
                    case "ساخت":
                        //call api 
                        //StreamReader r = new StreamReader("json1.json");
                        //string json = r.ReadToEnd();
                        //var root = JsonConvert.DeserializeObject<RootObject>(json);

                        var streamTask = client.GetStreamAsync("http://192.168.95.88:30907/api/classicNext/portfolio");
                        await SendClassicNextPortfolioToUser(message, streamTask);
                        break;
                    case "تنظیمات":
                        person.State = 9;
                        SendMessage(message, "choose your risk rate:", ReplyKeyboardMarkup5);
                        break;
                    case "بازگشت":
                        person.State = 7;
                        SendMessage(message, "choose:", ReplyKeyboardMarkup3);
                        break;
                    default:
                        SendMessage(message, "you say sth wrong try again", ReplyKeyboardMarkup4);
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
                        SendMessage(message, "choose:", ReplyKeyboardMarkup4);
                        break;
                    default:
                        valid = false;
                        SendMessage(message, "you say sth wrong try again", ReplyKeyboardMarkup5);
                        break;
                }
                if (valid)
                {
                    person.State = 10;
                    SendMessage(message, "choose:", ReplyKeyboardMarkup6);
                }
            }
            else if (person.State == 10)
            {
                CreateOrEditMoreSetting(person, message, "send a number for minimum stock weight between 0.01 and 1: (send english digit)", 11);
            }
            else if (person.State == 11)
            {
                TakeMaxStockWeight(person, message, false);
            }
            else if (person.State == 12)
            {
                CreateOrEditMoreSetting(person, message, "send a number for maximum stock weight between 0.05 and 1: (send english digit)", 13);
            }
            else if (person.State == 13)
            {
                TakeMaxStockWeight(person, message, true);
            }
            else if (person.State == 14)
            {
                CreateOrEditMoreSetting(person, message, "send your production date between 13990101 to current-date ( format -> 13990721 ) if you send future date :", 15);
            }
            else if (person.State == 15)
            {
                if (message.Text.Length == 8 && int.Parse(message.Text) > 139900101)
                {
                    person.SmartPortfolioSetting.ProductionDate = message.Text;
                    person.State = 16;
                    SendMessage(message, "choose:", ReplyKeyboardMarkup7);
                }
                else
                {
                    SendMessage(message, "please send a valid date.", ReplyKeyboardMarkup1);
                }
            }
            else if (person.State == 16)
            {
                switch (message.Text)
                {
                    case "ساخت":
                        // call api with all manuall settings
                        var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/false/{person.SmartPortfolioSetting.RiskRate}/{person.SmartPortfolioSetting.MinimumStockWeight}/{person.SmartPortfolioSetting.MaximumStockWeight}/{person.SmartPortfolioSetting.ProductionDate}");
                        await SendClassicNextPortfolioToUser(message, streamTask);
                        break;
                    default:
                        SendMessage(message, "you say sth wrong try again", ReplyKeyboardMarkup6);
                        break;
                }
            }
        }

        private static void SetRiskRate(Person person, long rate)
        {
            person.SmartPortfolioSetting.RiskRate = rate;
        }

        private async static void CreateOrEditMoreSetting(Person person, Message message, string moreSettingsMessage, long state)
        {
            switch (message.Text)
            {
                case "ساخت":
                    // call api with more settings
                    System.Threading.Tasks.Task<Stream> streamTask = null;

                    if (state == 10)
                        //streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/false/{person.SmartPortfolioSetting.RiskRate}");
                    if (state == 12)
                        //streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/false/{person.SmartPortfolioSetting.RiskRate}/{person.SmartPortfolioSetting.MinimumStockWeight}");
                    if (state == 14)
                        //streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/false/{person.SmartPortfolioSetting.RiskRate}/{person.SmartPortfolioSetting.MinimumStockWeight}/{person.SmartPortfolioSetting.MaximumStockWeight}");

                    //StreamReader r = new StreamReader("json1.json");
                    //string json = r.ReadToEnd();
                    //var root = JsonConvert.DeserializeObject<RootObject>(json);

                    await SendClassicNextPortfolioToUser(message, streamTask);

                    break;
                case "تنظیمات بیشتر":
                    person.State = state;
                    SendMessage(message, moreSettingsMessage, ReplyKeyboardMarkup1);
                    break;
                case "بازگشت":
                    person.State = 9;
                    SendMessage(message, "choose your risk rate:", ReplyKeyboardMarkup5);
                    break;
                default:
                    SendMessage(message, "you say sth wrong try again", ReplyKeyboardMarkup6);
                    break;
            }
        }

        private static async System.Threading.Tasks.Task SendClassicNextPortfolioToUser(Message message, System.Threading.Tasks.Task<Stream> streamTask)
        {
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<RootObject>(await streamTask);

            if (root.IsSuccessful)
            {
                foreach (var item in root.ResponseObject.StockAndWeights)
                {
                    StringBuilder str = new StringBuilder();
                    str.Append("tickerKey: " + item.Stock.TickerKey.ToString() + "\n");
                    str.Append("isin: " + item.Stock.Isin + "\n");
                    str.Append("tickerPooyaFa: " + item.Stock.TickerPooyaFa + "\n");
                    str.Append("tickerNamePooyaFa: " + item.Stock.TickerNamePooyaFa + "\n");
                    str.Append("exchangeKey: " + item.Stock.ExchangeKey.ToString() + "\n");
                    str.Append("marketType: " + item.Stock.MarketType + "\n");
                    str.Append("weight: " + item.Weight.ToString() + "\n");
                    SendMessageWithoutReply(message, str.ToString(), ReplyKeyboardMarkup1);
                    SendMessageWithoutReply(message, "choose: ", ReplyKeyboardMarkup4);
                }
            }
            else
            {
                SendMessage(message, root.ErrorMessageFa, ReplyKeyboardMarkup4);
            }
        }

        private static void TakeMaxStockWeight(Person person, Message message, bool max)
        {
            try
            {
                bool check;
                if (max)
                    check = double.Parse(message.Text) >= 0.05 && double.Parse(message.Text) <= 1;
                else 
                    check = double.Parse(message.Text) >= 0.01 && double.Parse(message.Text) <= 1;
                if (check)
                {
                    var state = (max) ? 12 : 14;
                    var str = (max) ? "maximum stock weight change successfully." : "minimum stock weight change successfully.";
                    person.State = state;

                    if (max)
                        person.SmartPortfolioSetting.MaximumStockWeight = double.Parse(message.Text);
                    else
                        person.SmartPortfolioSetting.MinimumStockWeight = double.Parse(message.Text);

                    SendMessage(message, str, ReplyKeyboardMarkup1);
                    SendMessage(message, "choose:", ReplyKeyboardMarkup6);
                }
                else
                {
                    SendMessage(message, "please send a valid number.", ReplyKeyboardMarkup1);
                }
            }
            catch (Exception)
            {
                SendMessage(message, "do not send persian number. switch your keyboard to english.", ReplyKeyboardMarkup1);
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
                    ChatId = (long)messageEventArgs.Message.Chat.Id
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
