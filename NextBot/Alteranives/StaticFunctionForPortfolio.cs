using Newtonsoft.Json;
using NextBot.Handlers;
using NextBot.Models;
using QuickChart;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace NextBot.Alteranives
{
    public class StaticFunctionForPortfolio : StaticFunctions
    {

        public static async Task<bool> ShowReturnAndComparisonInClassicNextSelect(IChatService chatService, Person person, Task<Stream> streamTask, long portfolioId)
        {
            await chatService.SendMessage(chatId: person.ChatId, message: "تاریخ شروع همان تاریخ ساخت پرتفوی مورد نظر می باشد ...");
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.RootobjectForCalculateReturnAndComparison>(await streamTask);
            if (root.IsSuccessful)
            {
                await chatService.SendMessage(chatId: person.ChatId, message: $"بازدهی پرتفوی شماره  {portfolioId} در مجموع : " + "\n" + Math.Round(Convert.ToDecimal(root.ResponseObject) * 100, 1) + " %");
                Thread.Sleep(500);
                return true;
            }
            else
            {
                await chatService.SendMessage(chatId: person.ChatId, message: root.ErrorMessageFa);
                Thread.Sleep(500);
                return false;
            }
        }

        public static async Task<string> GetBithdayOfPortfolio(Person person)
        {
            var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/{person.PortfolioIdForClassicNextSelect}");
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.RootObjectForSpecificPortfolio>(await streamTask);
            var str = root.ResponseObject.BirthdayPersian;
            return str.Replace("/", "");
        }

        public static async Task<string> ShowReturnOfEveryStockInPortfolio(IChatService chatService, Person person, string text)
        {
            var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/{person.PortfolioIdForClassicNextSelect}");
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.RootObjectForSpecificPortfolio>(await streamTask);
            var tickerKeys = new List<int>();
            var symbols = new List<string>();
            //var weights = new List<float>();
            foreach (var item in root.ResponseObject.StockAndWeights)
            {
                //weights.Add(item.Weight);
                tickerKeys.Add(item.Stock.TickerKey);
                symbols.Add(item.Stock.TickerPooyaFa);
            }
            string json;
            if (person.CommandLevel == 9)
            {
                var parameter = new StockReturn.StockReturnParameter() { BeginDatePersian = int.Parse(await GetBithdayOfPortfolio(person)), TickerKeys = tickerKeys.ToArray() };
                json = JsonConvert.SerializeObject(parameter);
            }
            else
            {
                var parameter = new StockReturn.StockReturnParameterWithEndDate() { BeginDatePersian = int.Parse(await GetBithdayOfPortfolio(person)), TickerKeys = tickerKeys.ToArray(), EndDatePersian = int.Parse(text) };
                json = JsonConvert.SerializeObject(parameter);
            }
            var strContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://192.168.95.88:30907/api/stock/returns", strContent).Result.Content.ReadAsStringAsync();
            var responseForChart = await client.PostAsync("http://192.168.95.88:30907/api/stock/normalPrices", strContent).Result.Content.ReadAsStringAsync();
            var resObj = JsonConvert.DeserializeObject<StockReturn.StockReturnRoot>(response);
            var resObjForChart = JsonConvert.DeserializeObject<NormalPrices.RootObjectStock>(responseForChart);
            StringBuilder str = new();

            for (int i = 0; i < resObj.ResponseObject.Length; i++)
            {
                str.Append($"{symbols.ElementAt(i)} : " + "\n" + Math.Round(resObj.ResponseObject.ElementAt(i) * 100, 1) + " %\n"); //__________________________________________________ $"(%w : {Math.Round(weights.First() * 100, 1)})" +
                //weights.RemoveAt(0);
            }
            var rnd = new Random();

            await chatService.SendMessage(chatId: person.ChatId, message: str.ToString());


            var configbuilder = @"{
                        'type': 'line',
                        'data': {
					        'labels': [";

            var length = (int) Math.Floor((decimal) resObjForChart.responseObject.datesPersian.Length / 50) + 1;
            for (int i = 0; i < resObjForChart.responseObject.datesPersian.Length; i += length)
            {
                configbuilder += $"'{resObjForChart.responseObject.datesPersian.ElementAt(i)}',";
            }
            configbuilder += @"],'datasets':[";

            for (int i = 0; i < resObjForChart.responseObject.stocksNormalPrices.Length; i++)
            {
                var stock = resObjForChart.responseObject.stocksNormalPrices.ElementAt(i);
                configbuilder += @"{'label': '";
                configbuilder += $"{stock.stock.tickerPooyaFa}";
                var rnd1 = rnd.Next(0, 255);
                var rnd2 = rnd.Next(0, 255);
                var rnd3 = rnd.Next(0, 255);
                configbuilder += $"','borderColor': 'rgb({rnd1}, {rnd2}, {rnd3})','backgroundColor': 'rgb({rnd1}, {rnd2}, {rnd3})','fill': false,'data': [";
                for (int j = 0; j < stock.normalPrices.Length; j += length)
                {
                    configbuilder += $"{stock.normalPrices.ElementAt(j)},";
                }
                configbuilder += @"],},";
            }

            configbuilder += @"],},'options': {
					                'title': {
						                'display': true,
						                'text': 'بازدهی سهم های موجود در پرتفوی'
					                },
					                },}
                                ";
            var qc = new Chart();
            qc.Width = 1000;
            qc.Height = 600;
            qc.Config = configbuilder;
            return qc.GetUrl();
        }

        public async static Task<string> CraeteJsonForGetChartComparisonIndex(Task<System.IO.Stream> streamTaskChart1, Task<System.IO.Stream> streamTaskChart2, Person person)
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
                        configbuilder += $"پرتفوی شماره {person.PortfolioIdForClassicNextSelect}";
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
						                'text': 'مقایسه بازدهی پرتفوی با شاخص کل'
					                },
					                },}
                                ";
                return configbuilder;
            }
            return null;
        }

        public async static Task<string> CraeteJsonForGetChartComparisonPortfolio(Task<System.IO.Stream> streamTaskChart1, Task<System.IO.Stream> streamTaskChart2, Person person)
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
                        configbuilder += $"پرتفوی شماره {greatId}";
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
                        configbuilder += $"پرتفوی شماره {lessId}";
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
						                'text': 'مقایسه بازدهی دو پرتفوی'
					                },
					                },}
                                ";
                return configbuilder;
            }
            return null;
        }

        public static string CraeteJsonForGetChart(NormalPrices.RootObject root, Person person)
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
                    configbuilder += $"پرتفوی شماره {person.PortfolioIdForClassicNextSelect}";
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
						            'text': 'بازدهی پرتفوی'
					            },
					            },}
                            ";
                }
            }

            return configbuilder;
        }

        public static async Task<Person> CheckDeletePortfolioOrNot(IChatService chatService, long chatId, string commandText, CallbackQuery query, Person person)
        {
            if (commandText == "خیر")
            {
                await chatService.UpdateMessage(chatId: query.Message.Chat.Id,
                                                        messageId: query.Message.MessageId,
                                                        newText: "پرتفوی مورد نظر حذف نشد");
                await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnOrComparisonRKM);
                person.CommandLevel = 3;
            }
            else if (commandText == "بلی")
            {
                await chatService.UpdateMessage(chatId: query.Message.Chat.Id,
                                                        messageId: query.Message.MessageId,
                                                        newText: "پرتفوی مورد نظر حذف شود");
                person = await DeletePortfolio(chatService, chatId, person);
            }
            return person;
        }

        public static async Task<Person> DeletePortfolio(IChatService chatService, long chatId, Person person)
        {
            var streamTask_ = client.DeleteAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/{person.PortfolioIdForClassicNextSelect}");
            var x = streamTask_.Result.Content.ReadAsStreamAsync();
            var res = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.Delete>(await x);
            if (res.isSuccessful)
            {
                await chatService.SendMessage(chatId: chatId, message: $"پرتفوی شماره {person.PortfolioIdForClassicNextSelect} حذف شد");
                await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.SelectOrCreateRKM);
                person.CommandState = 0;
                person.CommandLevel = 2;
            }
            else
            {
                await chatService.SendMessage(chatId: chatId, message: res.errorMessageFa);
                await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnOrComparisonRKM);
                person.CommandLevel = 3;
            }
            return person;
        }

        public static async Task<Person> ComparisonTwoPortfolioToToday(IChatService chatService, long chatId, string commandText, Person person)
        {
            try
            {
                person.StartDateWaitingForEndDate = commandText; // instead of create another property use previous one that in this state is useless.
                var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.PortfolioIdForClassicNextSelect}");
                if (await ShowReturnAndComparisonInClassicNextSelect(chatService, person, streamTask, person.PortfolioIdForClassicNextSelect))
                {
                    var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.StartDateWaitingForEndDate}/");
                    await ShowReturnAndComparisonInClassicNextSelect(chatService, person, streamTask_, long.Parse(person.StartDateWaitingForEndDate));
                    await CreateChartForComparisonTwoPortfolioToToday(chatService, chatId, person);
                }
                person.CommandLevel = 4;
                await chatService.SendMessage(chatId: person.ChatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonTypesRKM);
            }
            catch (Exception)
            {
                await chatService.SendMessage(chatId: person.ChatId, message: "خطایی رخ داده است. لطفا مجدد عدد خود را وارد نمایید :");
            }
            return person;
        }

        public static async Task CreateChartForComparisonTwoPortfolioToToday(IChatService chatService, long chatId, Person person)
        {
            var streamTaskChart1 = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/normalPrices/{person.PortfolioIdForClassicNextSelect}");
            var streamTaskChart2 = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/normalPrices/{person.StartDateWaitingForEndDate}");
            var configBuilder = await CraeteJsonForGetChartComparisonPortfolio(streamTaskChart1, streamTaskChart2, person);
            if (configBuilder != null)
            {
                var qc = new Chart();
                qc.Width = 800;
                qc.Height = 500;
                qc.Config = configBuilder;
                await chatService.SendPhotoAsync(chatId, qc.GetUrl());
            }
        }

        public async Task<Person> ComparisonPortfolioWithETFsToSpecificDate(IChatService chatService, long chatId, CallbackQuery query, Person person)
        {
            var date = await CheckAndGetDate(chatService, query);
            if (date != null)
            {
                var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.PortfolioIdForClassicNextSelect}/{date}");
                await ShowReturnAndComparisonInClassicNextSelect(chatService, person, streamTask, person.PortfolioIdForClassicNextSelect);

                var birthdayPortfolio = await GetBithdayOfPortfolio(person);
                var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/fund/etf/returns/{birthdayPortfolio}/{date}");
                var etfs = await System.Text.Json.JsonSerializer.DeserializeAsync<Models.ETF.Specific.Rootobject>(await streamTask_);
                if (etfs.responseObject != null)
                {
                    StringBuilder str = new();
                    str.Append("بازدهی صندوق ها :" + "\n");
                    for (int i = 0; i < etfs.responseObject.Length; i++)
                    {
                        var etf = etfs.responseObject.ElementAt(i);
                        str.Append($"{i + 1}. {etf.fund.symbol} : " + "\n" + Math.Round(Convert.ToDecimal(etf.returnValue) * 100, 1) + " %" + "\n");
                    }
                    await chatService.SendMessage(chatId: person.ChatId, message: str.ToString());


                    var streamTask0 = client.GetStreamAsync($"http://192.168.95.88:30907/api/fund/etf/normalPrices/{birthdayPortfolio}/{date}");
                    var etfPrices = await System.Text.Json.JsonSerializer.DeserializeAsync<NormalPrices.RootObjectETF>(await streamTask0);

                    var streamTask1 = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/normalPrices/{person.PortfolioIdForClassicNextSelect}/{date}");
                    var stock = await System.Text.Json.JsonSerializer.DeserializeAsync<NormalPrices.RootObject>(await streamTask1);
                    await CreateChartForComparisonPortfolioWithETFs(chatService, chatId, person, etfPrices, stock);
                }
                else
                {
                    await chatService.SendMessage(chatId: chatId, message: etfs.errorMessageFa);
                }

                Thread.Sleep(1000);
                await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonTypesRKM);
                person.CommandLevel = 4;
            }
            if (query == null)
            {
                await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonTypesRKM);
                person.CommandLevel = 4;
            }
            return person;
        }

        private static async Task CreateChartForComparisonPortfolioWithETFs(IChatService chatService, long chatId, Person person, NormalPrices.RootObjectETF etfPrices, NormalPrices.RootObject stock)
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
            configbuilder += $"پرتفوی شماره {person.PortfolioIdForClassicNextSelect}";
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
						                'text': 'مقایسه 5 صندوق بالا و 5 صندوق پایین از نظر بازدهی با پرتفوی'
					                },
					                },}
                                ";
            var qc = new Chart();
            qc.Width = 1400;
            qc.Height = 900;
            qc.Config = configbuilder;
            await chatService.SendPhotoAsync(chatId, qc.GetUrl());
        }

        public async Task<Person> ReturnPortfolioToSpecificDate(IChatService chatService, long chatId, CallbackQuery query, Person person)
        {
            var date = await CheckAndGetDate(chatService, query);
            if (date != null)
            {
                var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.PortfolioIdForClassicNextSelect}/{date}");
                if (await ShowReturnAndComparisonInClassicNextSelect(chatService, person, streamTask, person.PortfolioIdForClassicNextSelect))
                {
                    await chatService.SendMessage(chatId: person.ChatId, message: "بازدهی سهم های موجود در پرتفوی :");
                    var qc0 = await ShowReturnOfEveryStockInPortfolio(chatService, person, date);
                    await CreateChartOfReturnPortfolioToSpecificDate(chatService, chatId, person, date, qc0);
                }
                Thread.Sleep(1000);
                await chatService.SendMessage(chatId: person.ChatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnPortfolioTypesRKM);
                person.CommandLevel = 9;
            }
            if (query == null)
            {
                await chatService.SendMessage(chatId: person.ChatId, message: "ورودی نامعتبر! از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnPortfolioTypesRKM);
                person.CommandLevel = 9;
            }
            return person;
        }

        public static async Task CreateChartOfReturnPortfolioToSpecificDate(IChatService chatService, long chatId, Person person, string date, string qc0)
        {
            var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/normalPrices/{person.PortfolioIdForClassicNextSelect}/{date}");
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<NormalPrices.RootObject>(await streamTask_);
            if (root.isSuccessful)
            {
                var qc = new Chart();
                qc.Width = 800;
                qc.Height = 500;
                var configBuilder = CraeteJsonForGetChart(root, person);
                qc.Config = configBuilder;
                await chatService.SendPhotoAsync(chatId, qc.GetUrl());
            }
            await chatService.SendPhotoAsync(chatId, qc0);
        }

        public async Task<Person> SwitchProcessForReturnPortfolio(IChatService chatService, long chatId, string commandText, Person person)
        {
            switch (commandText)
            {
                case "بازدهی پرتفوی تا تاریخ دلخواه📆":
                    person.CommandLevel = 10;
                    await chatService.SendMessage(chatId: person.ChatId, message: "تاریخ مورد نظر خود را انتخاب کنید :", CreateCalendar());
                    break;
                case "بازدهی پرتفوی تا امروز":
                    await ReturnPortfolioToToday(chatService, chatId, person);
                    break;
                case "🔙":
                    person.CommandLevel = 3;
                    await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnOrComparisonRKM);
                    break;
                default:
                    await chatService.SendMessage(chatId: person.ChatId, message: "ورودی نامعتبر! از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnPortfolioTypesRKM);
                    break;
            }
            return person;
        }

        public static async Task ReturnPortfolioToToday(IChatService chatService, long chatId, Person person)
        {
            var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.PortfolioIdForClassicNextSelect}");
            if (await ShowReturnAndComparisonInClassicNextSelect(chatService, person, streamTask, person.PortfolioIdForClassicNextSelect))
            {
                await chatService.SendMessage(chatId: person.ChatId, message: "بازدهی سهم های موجود در پرتفوی :");
                var qc0 = await ShowReturnOfEveryStockInPortfolio(chatService, person, null);
                await CreateChartForReturnOfPortfolioToToday(chatService, chatId, person, qc0);
            }
            Thread.Sleep(1000);
            await chatService.SendMessage(chatId: person.ChatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnPortfolioTypesRKM);
        }

        public static async Task CreateChartForReturnOfPortfolioToToday(IChatService chatService, long chatId, Person person, string qc0)
        {
            var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/normalPrices/{person.PortfolioIdForClassicNextSelect}");
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<NormalPrices.RootObject>(await streamTask_);
            if (root.isSuccessful)
            {
                var qc = new Chart();
                qc.Width = 800;
                qc.Height = 500;
                var configBuilder = CraeteJsonForGetChart(root, person);
                qc.Config = configBuilder;
                await chatService.SendPhotoAsync(chatId, qc.GetUrl());
            }
            await chatService.SendPhotoAsync(chatId, qc0);
        }

        public async Task<Person> ComparisonTwoPortfolioToSpecificDate(IChatService chatService, long chatId, CallbackQuery query, Person person)
        {
            var date = await CheckAndGetDate(chatService, query);
            if (date != null)
            {
                try
                {
                    var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.PortfolioIdForClassicNextSelect}/{date}");
                    if (await ShowReturnAndComparisonInClassicNextSelect(chatService, person, streamTask, person.PortfolioIdForClassicNextSelect))
                    {
                        var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.StartDateWaitingForEndDate}/{date}");
                        await ShowReturnAndComparisonInClassicNextSelect(chatService, person, streamTask_, long.Parse(person.StartDateWaitingForEndDate));
                        await CreateChartForComparisonTwoPortfolioToSpecificDate(chatService, chatId, person, date);
                    }
                    person.CommandLevel = 4;
                    await chatService.SendMessage(chatId: person.ChatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonTypesRKM);
                }
                catch (Exception)
                {
                    await chatService.SendMessage(chatId: chatId, message: "خطایی رخ داده است. از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonTypesRKM);
                    person.CommandLevel = 4;
                }
            }
            if (query == null)
            {
                await chatService.SendMessage(chatId: chatId, message: "مقایسه با پرتفوی مورد نظر تا تاریخ دلخواه -> آی دی پرتفوی مورد نظر برای مقایسه وارد نمایید :");
                person.CommandLevel = 7;
            }
            return person;
        }

        public static async Task CreateChartForComparisonTwoPortfolioToSpecificDate(IChatService chatService, long chatId, Person person, string date)
        {
            var streamTaskChart1 = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/normalPrices/{person.PortfolioIdForClassicNextSelect}/{date}");
            var streamTaskChart2 = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/normalPrices/{person.StartDateWaitingForEndDate}/{date}");
            var configBuilder = await CraeteJsonForGetChartComparisonPortfolio(streamTaskChart1, streamTaskChart2, person);
            if (configBuilder != null)
            {
                var qc = new Chart();
                qc.Width = 800;
                qc.Height = 500;
                qc.Config = configBuilder;
                await chatService.SendPhotoAsync(chatId, qc.GetUrl());
            }
        }

        public async Task<Person> ComparisonPortfolioWithIndexToSpecificDate(IChatService chatService, long chatId, CallbackQuery query, Person person)
        {
            var date = await CheckAndGetDate(chatService, query);
            if (date != null)
            {
                var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.PortfolioIdForClassicNextSelect}/{date}");
                if (await ShowReturnAndComparisonInClassicNextSelect(chatService, person, streamTask, person.PortfolioIdForClassicNextSelect))
                {
                    var date_ = await GetBithdayOfPortfolio(person);
                    var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/stock/return/index/{date_}/{date}");
                    await ShowIndexReturnInClassicNextSelect(chatService, person, streamTask_);
                    await CreateChartForComparisonPortfolioWithIndexToSpecificDate(chatService, chatId, person, date, date_);
                }
                Thread.Sleep(500);
                await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonTypesRKM);
                person.CommandLevel = 4;
            }
            if (query == null)
            {
                await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonTypesRKM);
                person.CommandLevel = 4;
            }
            return person;
        }

        public static async Task CreateChartForComparisonPortfolioWithIndexToSpecificDate(IChatService chatService, long chatId, Person person, string date, string date_)
        {
            var streamTaskChart1 = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/normalPrices/{person.PortfolioIdForClassicNextSelect}/{date}");
            var streamTaskChart2 = client.GetStreamAsync($"http://192.168.95.88:30907/api/stock/normalPrices/index/{date_}/{date}");
            var configBuilder = await CraeteJsonForGetChartComparisonIndex(streamTaskChart1, streamTaskChart2, person);
            if (configBuilder != null)
            {
                var qc = new Chart();
                qc.Width = 500;
                qc.Height = 500;
                qc.Config = configBuilder;
                await chatService.SendPhotoAsync(chatId, qc.GetUrl());
            }
        }

        public async Task<Person> SwitchProcessForComparison(IChatService chatService, long chatId, string commandText, Person person)
        {
            switch (commandText)
            {
                case "مقایسه با شاخص تا امروز":
                    await ComparisonPortfolioWithIndexToToday(chatService, chatId, person);
                    break;
                case "مقایسه با شاخص تا تاریخ دلخواه📆":
                    person.CommandLevel = 6;
                    await chatService.SendMessage(chatId: chatId, message: "تاریخ مورد نظر خود را انتخاب کنید :", CreateCalendar());
                    break;
                case "مقایسه با صندوق سهامی تا امروز":
                    person = await  ComparisonPortfolioWithETFsToToday(chatService, chatId, person);
                    break;
                case "مقایسه با صندوق سهامی تا تاریخ دلخواه📆":
                    await chatService.SendMessage(chatId, message: "تاریخ پایان محاسبه بازدهی را انتخاب کنید", rkm: CreateCalendar());
                    person.CommandLevel = 11;
                    break;
                case "مقایسه با پرتفوی تا امروز":
                    await chatService.SendMessage(chatId: chatId, message: "مقایسه با پرتفوی مورد نظر تا امروز -> آی دی پرتفوی مورد نظر برای مقایسه وارد نمایید :");
                    person.CommandLevel = 12;
                    break;
                case "مقایسه با پرتفوی تا تاریخ دلخواه📆":
                    person.CommandLevel = 7;
                    await chatService.SendMessage(chatId: chatId, message: "مقایسه با پرتفوی مورد نظر تا تاریخ دلخواه -> آی دی پرتفوی مورد نظر برای مقایسه وارد نمایید :");
                    break;
                case "🔙":
                    person.CommandLevel = 3;
                    await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnOrComparisonRKM);
                    break;
                default:
                    await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonTypesRKM);
                    break;
            }
            return person;
        }

        public static async Task<Person> ComparisonPortfolioWithETFsToToday(IChatService chatService, long chatId, Person person)
        {
            var streamTask0 = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.PortfolioIdForClassicNextSelect}");
            await ShowReturnAndComparisonInClassicNextSelect(chatService, person, streamTask0, person.PortfolioIdForClassicNextSelect);

            var birthdayPortfolio = await GetBithdayOfPortfolio(person);
            var streamTask1 = client.GetStreamAsync($"http://192.168.95.88:30907/api/fund/etf/returns/{birthdayPortfolio}");
            var etfs = await System.Text.Json.JsonSerializer.DeserializeAsync<Models.ETF.Specific.Rootobject>(await streamTask1);
            if (etfs.responseObject != null)
            {
                StringBuilder str = new();
                str.Append("بازدهی صندوق ها :" + "\n");
                for (int i = 0; i < etfs.responseObject.Length; i++)
                {
                    var etf = etfs.responseObject.ElementAt(i);
                    str.Append($"{i + 1}. {etf.fund.symbol} : " + "\n" + Math.Round(Convert.ToDecimal(etf.returnValue) * 100, 1) + " %" + "\n");
                }
                await chatService.SendMessage(chatId: person.ChatId, message: str.ToString());

                var streamTask0_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/fund/etf/normalPrices/{birthdayPortfolio}");
                var etfPrices = await System.Text.Json.JsonSerializer.DeserializeAsync<NormalPrices.RootObjectETF>(await streamTask0_);

                var streamTask1_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/normalPrices/{person.PortfolioIdForClassicNextSelect}");
                var stock = await System.Text.Json.JsonSerializer.DeserializeAsync<NormalPrices.RootObject>(await streamTask1_);
                await CreateChartForComparisonPortfolioWithETFs(chatService, chatId, person, etfPrices, stock);
            }
            else
            {
                await chatService.SendMessage(chatId: chatId, message: etfs.errorMessageFa);
            }

            Thread.Sleep(1000);
            await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonTypesRKM);
            person.CommandLevel = 4;
            return person;
        }

        public static async Task ComparisonPortfolioWithIndexToToday(IChatService chatService, long chatId, Person person)
        {
            var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.PortfolioIdForClassicNextSelect}");
            if (await ShowReturnAndComparisonInClassicNextSelect(chatService, person, streamTask, person.PortfolioIdForClassicNextSelect))
            {
                var date = await GetBithdayOfPortfolio(person);
                var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/stock/return/index/{date}");
                await ShowIndexReturnInClassicNextSelect(chatService, person, streamTask_);
                await CreateChartForComparisonPortfolioWithIndexToToday(chatService, chatId, person, date);
            }
            Thread.Sleep(500);
            await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonTypesRKM);
        }

        public static async Task CreateChartForComparisonPortfolioWithIndexToToday(IChatService chatService, long chatId, Person person, string date)
        {
            var streamTaskChart1 = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/normalPrices/{person.PortfolioIdForClassicNextSelect}");
            var streamTaskChart2 = client.GetStreamAsync($"http://192.168.95.88:30907/api/stock/normalPrices/index/{date}");
            var configBuilder = await CraeteJsonForGetChartComparisonIndex(streamTaskChart1, streamTaskChart2, person);
            if (configBuilder != null)
            {
                var qc = new Chart();
                qc.Width = 800;
                qc.Height = 500;
                qc.Config = configBuilder;
                await chatService.SendPhotoAsync(chatId, qc.GetUrl());
            }
        }

        public static async Task<Person> SwitchProcessForMainMenuOfPortfolio(IChatService chatService, long chatId, string commandText, Person person)
        {
            switch (commandText)
            {
                case "مقایسه📊":
                    person.CommandLevel = 4;
                    await chatService.SendMessage(chatId: chatId, message: "گزینه مورد نظر را انتخاب کنید :", Markup.ComparisonTypesRKM);
                    break;
                case "محاسبه بازدهی📈":
                    person.CommandLevel = 9;
                    await chatService.SendMessage(chatId: chatId, message: "نوع بازدهی را از بین دو گزینه زیر انتخاب کنید :", Markup.ReturnPortfolioTypesRKM);
                    break;
                case "حذف پرتفوی❌":
                    person.CommandLevel = 14;
                    await chatService.SendMessage(chatId, message: "پرتفوی مورد نظر حذف شود ؟", GetSaveInlineKeyboard());
                    break;
                case "🔙":
                    person.CommandState = 0;
                    person.CommandLevel = 2;
                    await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.SelectOrCreateRKM);
                    break;
                default:
                    await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnOrComparisonRKM);
                    break;
            }
            return person;
        }

        public static async Task<Person> SwitchProcessForSelectPortfolio(IChatService chatService, long chatId, string commandText, Person person)
        {
            switch (commandText)
            {
                case "بعدی⬇️":
                    person = await ShowPreviousOrNextListInClassicNextSelect(chatService, person, null);
                    break;
                case "قبلی⬆️":
                    if (person.ClassicNextSelectState == 21)
                    {
                        person.CommandState = 0;
                        person.CommandLevel = 2;
                        await chatService.SendMessage(chatId: person.ChatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.SelectOrCreateRKM);
                        break;
                    }
                    person.ClassicNextSelectState -= 40;
                    person = await ShowPreviousOrNextListInClassicNextSelect(chatService, person, null);
                    break;
                default:
                    try
                    {
                        var split = commandText.Split(" ");
                        var strNum = split[2];
                        person = await ShowSpecificPortfolioInClassicNextSelect(chatService, person, strNum);
                    }
                    catch (Exception)
                    {
                        await chatService.SendMessage(chatId, message: "خطایی رخ داده است. لطفا مجددا گزینه مورد نظر را انتخاب کنید.");
                    }
                    break;
            }
            return person;
        }

        public static async Task<Person> ShowSpecificPortfolioInClassicNextSelect(IChatService chatService, Person person, String strNumber)
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

                    await chatService.SendMessage(chatId: person.ChatId, message: str.ToString());
                    person.CommandLevel = 3;
                    person.PortfolioIdForClassicNextSelect = num;

                    var symbols = new List<string>();
                    //var weights = new List<float>();
                    foreach (var item in root.ResponseObject.StockAndWeights)
                    {
                        //weights.Add(item.Weight);
                        symbols.Add(item.Stock.TickerPooyaFa);
                    }

                    string configBuilder = null;
                    configBuilder += @"{'type': 'pie','data': { 'datasets': [ { data: [";

                    for (int i = 0; i < root.ResponseObject.StockAndWeights.Length; i++)
                    {
                        configBuilder += Math.Round(root.ResponseObject.StockAndWeights.ElementAt(i).Weight * 100, 1) + ",";
                        if (i == 9) break;
                        //weights.RemoveAt(0);
                    }
                    var rnd = new Random();
                    configBuilder += @"],backgroundColor:[";
                    for (int i = 0; i < symbols.Count; i++)
                    {
                        configBuilder += $"'rgb({rnd.Next(0, 255)}, {rnd.Next(0, 255)}, {rnd.Next(0, 255)})',";
                        if (i == 9)
                            break;
                    }
                    configBuilder += @"],label: 'Dataset 1',},],labels: [";
                    
                    for (int i = 0; i < symbols.Count; i++)
                    {
                        configBuilder += $"'{symbols.ElementAt(i)}',";
                        if (i == 9)
                            break;
                    }
                    configBuilder += @"],},}";

                    var qc = new Chart();
                    qc.Width = 800;
                    qc.Height = 500;
                    qc.Config = configBuilder;
                    await chatService.SendPhotoAsync(person.ChatId, qc.GetUrl());
                    Thread.Sleep(500);
                    await chatService.SendMessage(chatId: person.ChatId, message: "از بین گزینه های زیر انتخاب کنید :", Markup.ReturnOrComparisonRKM);

                }
                else
                {
                    person.CommandState = 0;
                    person.CommandLevel = 2;
                    await chatService.SendMessage(chatId: person.ChatId, message: root.ErrorMessageFa);
                    Thread.Sleep(200);
                    await chatService.SendMessage(chatId: person.ChatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.SelectOrCreateRKM);
                }
            }
            catch (Exception)
            {
                person.CommandState = 0;
                person.CommandLevel = 2;
                await chatService.SendMessage(chatId: person.ChatId, message: "ورودی نامعتبر !");
                await chatService.SendMessage(chatId: person.ChatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.SelectOrCreateRKM);
            }
            return person;
        }
    }
}
