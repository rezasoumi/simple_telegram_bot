using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NextBot.Handlers;
using NextBot.Models;
using NextBot.SmartSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace NextBot.Commands
{
    public class ReturnCommand : IBotCommand
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly MyDbContext _context;
        private static readonly HttpClient client = new();

        public string Command => "return";

        public string Description => "Get Return of stock in any period of time";

        public bool InternalCommand => false;

        public ReturnCommand(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var scope = serviceProvider.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        }

        public async Task Execute(IChatService chatService, long chatId, int userId, int messageId, string? commandText)
        {
            var person = _context.People.FirstOrDefault(p => p.ChatId == chatId);
            person.CommandState = 1;

            if (person.CommandLevel == 0)
            {
                await chatService.SendMessage(chatId, message: "نام سهم مورد نظر را وارد کنید :");
                person.CommandLevel = 1;
                _context.SaveChanges();
            }
            else if (person.CommandLevel == 1)
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

                var buttons = smartDictionary.Search(commandText, 10).Select(x => new[] { new KeyboardButton(x) }).ToArray();

                await chatService.SendMessage(chatId, message: "سهم مورد نظر را از گزینه های موجود انتخاب کنید :", rkm: new ReplyKeyboardMarkup(buttons, resizeKeyboard: true));
                person.CommandLevel = 2;
            }
            else if (person.CommandLevel == 2)
            {
                var streamTask = client.GetStreamAsync("http://192.168.95.88:30907/api/industry/stocks");
                var industries = await System.Text.Json.JsonSerializer.DeserializeAsync<List<IndustryStocks.Industry>>(await streamTask);
                SaveTickerKey(person, commandText, industries);
                await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", rkm: Markup.StockReturnRKM);
                person.CommandLevel = 3;
            }
            else if (person.CommandLevel == 3)
            {
                switch (commandText)
                {
                    case "محاسبه بازدهی":
                        person.CommandLevel = 4;
                        await chatService.SendMessage(chatId, message: "تاریخ شروع و پایان محاسبه بازدهی را به انگلیسی همانند فرمت نمونه وارد کنید(نمونه:14000321 13991026)");
                        break;
                    case "بازگشت":
                        await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", rkm: Markup.MainMenuRKM);
                        person.CommandLevel = 0;
                        person.CommandState = 1000;
                        break;
                    default:
                        await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", rkm: Markup.StockReturnRKM);
                        break;
                }
            }
            else if (person.CommandLevel == 4)
            {
                if (commandText.Length == 17)
                {
                    var dates = commandText.Split(" ");
                    var parameter = new StockReturn.StockReturnParameterWithEndDate() { BeginDatePersian = int.Parse(dates[0]), EndDatePersian = int.Parse(dates[1]), TickerKeys = new int[] { person.TickerKeyForStock } };
                    var json = JsonConvert.SerializeObject(parameter);
                    var strContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("http://192.168.95.88:30907/api/stock/returns", strContent).Result.Content.ReadAsStringAsync();
                    var resObj = JsonConvert.DeserializeObject<StockReturn.StockReturnRoot>(response);
                    if (resObj.IsSuccessful)
                        await chatService.SendMessage(chatId, message: "بازدهی سهام مورد نظر در این بازه زمانی :" + "\n" + Math.Round(resObj.ResponseObject.First() * 100, 1) + " %");
                    else
                        await chatService.SendMessage(chatId, message: resObj.ErrorMessageFa);
                    Thread.Sleep(200);
                    await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", rkm: Markup.StockReturnRKM);
                    person.CommandLevel = 3;
                }
                else
                    await chatService.SendMessage(chatId, message: "ورودی نامعتبر! لطفا مجدد تلاش کنید :");
            }
            else
            {
                await chatService.SendMessage(chatId, "TODO: Create a todo command");
            }
        }
        private static void SaveTickerKey(Person person, string text, List<IndustryStocks.Industry> industries)
        {
            var end = false;
            foreach (var industry in industries)
            {
                foreach (var stock in industry.Stocks)
                {
                    if (stock.Symbol == text)
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
    }
}
