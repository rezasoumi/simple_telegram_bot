using Microsoft.EntityFrameworkCore;
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
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace NextBot.Commands
{
    public class ReturnCommand : StaticFunctions, IBotCommand
    {
        private readonly IServiceProvider _serviceProvider;
        private MyDbContext _context;

        public string Command => "return";

        public string Description => "محاسبه بازدهی سهام در بازه زمانی مشخص";

        public bool InternalCommand => false;

        public ReturnCommand(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var scope = serviceProvider.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        }

        public async Task<MyDbContext> Execute(IChatService chatService, long chatId, long userId, int messageId, string? commandText, CallbackQueryEventArgs? query)
        {
            var person = _context.People.FirstOrDefault(p => p.ChatId == chatId);
            person.CommandState = 1;
            _context.Entry(person).State = EntityState.Modified;
            if (person.CommandLevel == 0)
            {
                await chatService.SendMessage(chatId, message: "نام سهم مورد نظر را وارد کنید :");
                person.CommandLevel = 1;
                _context.Entry(person).State = EntityState.Modified;
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
                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 2)
            {
                var streamTask = client.GetStreamAsync("http://192.168.95.88:30907/api/industry/stocks");
                var industries = await System.Text.Json.JsonSerializer.DeserializeAsync<List<IndustryStocks.Industry>>(await streamTask);
                if (SaveTickerKey(person, commandText, industries))
                {
                    await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", rkm: Markup.StockReturnRKM);
                    person.CommandLevel = 3;
                }
                else
                {
                    await chatService.SendMessage(chatId, message: "ورودی اشتباه ! لطفا مجدد نام سهام مورد نظر را وارد نمایید :");
                    person.CommandLevel = 1;
                }

                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 3)
            {
                switch (commandText)
                {
                    case "محاسبه بازدهی📈":
                        person.CommandLevel = 4;
                        await chatService.SendMessage(chatId, message: "تاریخ شروع محاسبه بازدهی را انتخاب کنید :", CreateCalendar());
                        break;
                    case "🔙":
                        await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", rkm: Markup.MainMenuRKM);
                        person.CommandLevel = 0;
                        person.CommandState = 0;
                        break;
                    default:
                        await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", rkm: Markup.StockReturnRKM);
                        break;
                }
                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 4)
            {
                if (query != null)
                {
                    var m = await ProcessCalendar(chatService, query);
                    if (m != null)
                    {
                        string date;
                        if (m.Month.ToString().Length == 1 && m.Day.ToString().Length == 1)
                            date = $"{m.Year}0{m.Month}0{m.Day}";
                        else if (m.Month.ToString().Length == 1 && m.Day.ToString().Length != 1)
                            date = $"{m.Year}0{m.Month}{m.Day}";
                        else if (m.Month.ToString().Length != 1 && m.Day.ToString().Length == 1)
                            date = $"{m.Year}{m.Month}0{m.Day}";
                        else
                            date = $"{m.Year}{m.Month}{m.Day}";

                        person.StartDateWaitingForEndDate = date;
                        await chatService.SendMessage(chatId, message: "تاریخ پایان محاسبه بازدهی را انتخاب کنید :", CreateCalendar());
                        person.CommandLevel = 5;
                    }
                }
                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 5)
            {
                var date = await CheckAndGetDate(chatService, query);
                if (date != null)
                {
                    var dates = commandText.Split(" ");
                    var parameter = new StockReturn.StockReturnParameterWithEndDate() { BeginDatePersian = int.Parse(person.StartDateWaitingForEndDate), EndDatePersian = int.Parse(date), TickerKeys = new int[] { person.TickerKeyForStock } };
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
            }
            else
            {
                await chatService.SendMessage(chatId, "TODO: Create a todo command");
                _context.Entry(person).State = EntityState.Modified;
            }
            _context.SaveChanges();
            return _context;
        }
    }
}
