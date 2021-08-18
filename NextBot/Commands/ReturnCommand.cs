using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NextBot.Alteranives;
using NextBot.Handlers;
using NextBot.Models;
using NextBot.SmartSearch;
using QuickChart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
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

        public async Task<MyDbContext> Execute(IChatService chatService, long chatId, long userId, int messageId, string? commandText, CallbackQuery? query)
        {
            var person = _context.People.FirstOrDefault(p => p.ChatId == chatId);
            person.CommandState = 1;

            if (person.CommandLevel == 0)
            {
                await chatService.SendMessage(chatId, message: "نام سهم مورد نظر (یا بخشی از آن) را وارد کنید :");
                person.CommandLevel = 1;
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
                var tickerKey = SaveTickerKey(person, commandText, industries);
                if (tickerKey != -1)
                {
                    person.TickerKeyForStock = tickerKey;
                    await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", rkm: Markup.StockReturnRKM);
                    person.CommandLevel = 3;
                }
                else
                {
                    await chatService.SendMessage(chatId, message: "ورودی اشتباه ! لطفا مجدد نام سهام مورد نظر را وارد نمایید :");
                    person.CommandLevel = 1;
                }

            }
            else if (person.CommandLevel == 3)
            {
                switch (commandText)
                {
                    case "محاسبه بازدهی تا تاریخ دلخواه📈":
                        person.CommandLevel = 4;
                        await chatService.SendMessage(chatId, message: "تاریخ شروع محاسبه بازدهی را انتخاب کنید :", CreateCalendar());
                        break;
                    case "محاسبه بازدهی تا امروز📈":
                        await chatService.SendMessage(chatId, message: "تاریخ شروع محاسبه بازدهی را انتخاب کنید :", CreateCalendar());
                        person.CommandLevel = 6;
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
            }
            else if (person.CommandLevel == 4)
            {
                var date = await CheckAndGetDate(chatService, query);
                if (date != null)
                {
                    person.StartDateWaitingForEndDate = date;
                    await chatService.SendMessage(chatId, message: "تاریخ پایان محاسبه بازدهی را انتخاب کنید :", CreateCalendar());
                    person.CommandLevel = 5;
                }
                if (query == null)
                {
                    person.CommandLevel = 3;
                    await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", rkm: Markup.StockReturnRKM);
                }
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
                    
                    var qc = new Chart();
                    qc.Width = 800;
                    qc.Height = 500;
                    string configbuilder = await CraeteJsonForGetChart(strContent);
                    if (configbuilder != null)
                    {
                        qc.Config = configbuilder;
                        await chatService.SendPhotoAsync(chatId, qc.GetUrl());
                    }

                    Thread.Sleep(200);
                    await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", rkm: Markup.StockReturnRKM);
                    person.CommandLevel = 3;
                }
                if (query == null)
                {
                    person.CommandLevel = 3;
                    await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", rkm: Markup.StockReturnRKM);
                }
            }
            else if (person.CommandLevel == 6)
            {
                var date = await CheckAndGetDate(chatService, query);
                if (date != null)
                {
                    var dates = commandText.Split(" ");
                    var parameter = new StockReturn.StockReturnParameterWithEndDate() { BeginDatePersian = int.Parse(date), TickerKeys = new int[] { person.TickerKeyForStock } };
                    var json = JsonConvert.SerializeObject(parameter);
                    var strContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("http://192.168.95.88:30907/api/stock/returns", strContent).Result.Content.ReadAsStringAsync();
                    var resObj = JsonConvert.DeserializeObject<StockReturn.StockReturnRoot>(response);
                    if (resObj.IsSuccessful)
                        await chatService.SendMessage(chatId, message: "بازدهی سهام مورد نظر در این بازه زمانی :" + "\n" + Math.Round(resObj.ResponseObject.First() * 100, 1) + " %");
                    else
                        await chatService.SendMessage(chatId, message: resObj.ErrorMessageFa);

                    var qc = new Chart();
                    qc.Width = 800;
                    qc.Height = 500;
                    string configbuilder = await CraeteJsonForGetChart(strContent);
                    if (configbuilder != null)
                    {
                        qc.Config = configbuilder;
                        await chatService.SendPhotoAsync(chatId, qc.GetUrl());
                    }
                    Thread.Sleep(200);
                    await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", rkm: Markup.StockReturnRKM);
                    person.CommandLevel = 3;
                }
                if (query == null)
                {
                    person.CommandLevel = 3;
                    await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", rkm: Markup.StockReturnRKM);
                }
            }
            else
            {
                person.CommandLevel = 1;
            }
            _context.SaveChanges();
            return _context;
        }

        private static async Task<string> CraeteJsonForGetChart(StringContent strContent)
        {
            var responseForChart = await client.PostAsync("http://192.168.95.88:30907/api/stock/normalPrices", strContent).Result.Content.ReadAsStringAsync(); ;
            var resObjForChart = JsonConvert.DeserializeObject<NormalPrices.RootObjectStock>(responseForChart);
            if (resObjForChart.isSuccessful)
            {
                var configbuilder = @"{
                        'type': 'line',
                        'data': {
					        'labels': [";

                var length = (int)Math.Floor((decimal)resObjForChart.responseObject.datesPersian.Length / 100) + 1;

                for (int i = 0; i < resObjForChart.responseObject.datesPersian.Length; i += length)
                {
                    configbuilder += $"'{resObjForChart.responseObject.datesPersian.ElementAt(i)}',";
                }

                configbuilder += @"],'datasets':[";
                configbuilder += @"{'label': '";
                configbuilder += $"{resObjForChart.responseObject.stocksNormalPrices.First().stock.tickerPooyaFa}";
                configbuilder += @"',
							        'borderColor': 'rgb(255, 99, 132)',
							        'backgroundColor': 'rgb(255, 99, 132)',
							        'fill': false,
							        'data': [";

                var stock = resObjForChart.responseObject.stocksNormalPrices.First();
                for (int i = 0; i < stock.normalPrices.Length; i += length)
                {
                    configbuilder += $"{stock.normalPrices.ElementAt(i)},";
                }
                configbuilder += @"]},],},'options': {
					                'title': {
						                'display': true,
						                'text': 'بازدهی سهام'
					                },
					                },}
                                ";

                return configbuilder;
            }
            else
            {
                return null;
            }
        }
    }
}
