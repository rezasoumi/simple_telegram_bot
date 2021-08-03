using Newtonsoft.Json;
using NextBot.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace NextBot.Handlers
{
    public class StaticFunctions
    {
        public static readonly HttpClient client = new();
        public static Emoji backEmoji = new(0x1F519);
        public static Emoji comparisonEmoji = new(0x1F4CA);
        public static Emoji returnEmoji = new(0x1F4C8);
        public static Emoji moneyBagEmoji = new(0x1F4B0);
        public static Emoji plusSignEmoji = new(0x2795);
        public static Emoji minusSignEmoji = new(0x2796);
        public static Emoji crossMarkEmoji = new(0x274C);
        public static Emoji calendarEmoji = new(0x1F4C6);

        public InlineKeyboardMarkup CreateCalendar(int year = 0, int month = 0)
        {
            var now = PersianDateTime.Now;
            if (year == 0) year = now.Year;
            if (month == 0) month = now.Month;
            var dataIgnore = CreateCallbackData("IGNORE", year, month, 0);

            var row1 = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(PersianDateTime.GetMonthName(month - 2) + " " + year.ToString(), dataIgnore) };

            var row2 = Array.Empty<InlineKeyboardButton>();
            string[] dayOfWeek = new string[] { "ج", "پ", "چ", "س", "د", "ی", "ش" };
            foreach (var item in dayOfWeek)
            {
                row2 = row2.Append(InlineKeyboardButton.WithCallbackData(item, dataIgnore)).ToArray();
            }

            List<string> days;
            if (PersianDateTime.GetDaysInMonth(year, month) == 30)
                days = new List<string> { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30" };
            else
                days = new List<string> { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" };

            var row3 = Array.Empty<InlineKeyboardButton>();
            var row4 = Array.Empty<InlineKeyboardButton>();
            var row5 = Array.Empty<InlineKeyboardButton>();
            var row6 = Array.Empty<InlineKeyboardButton>();
            var row7 = Array.Empty<InlineKeyboardButton>();
            var row8 = Array.Empty<InlineKeyboardButton>();

            var listOne = new List<int> { };
            var listTwo = new List<int> { };
            var listThree = new List<int> { };
            var listFour = new List<int> { };
            var listFive = new List<int> { };
            var listSix = new List<int> { };

            var count = 0;
            foreach (var day in days)
            {
                PersianDateTime persianDate;
                if (month.ToString().Length == 1)
                    persianDate = PersianDateTime.Parse($"{year}/0{month}/{day}");
                else
                    persianDate = PersianDateTime.Parse($"{year}/{month}/{day}");

                if (count == 0) listOne.Add(int.Parse(day));
                if (count == 1) listTwo.Add(int.Parse(day));
                if (count == 2) listThree.Add(int.Parse(day));
                if (count == 3) listFour.Add(int.Parse(day));
                if (count == 4) listFive.Add(int.Parse(day));
                if (count == 5) listSix.Add(int.Parse(day));

                if (persianDate.DayOfWeek == 6)
                    count++;
            }
            row3 = AppendToRow(year, month, dataIgnore, row3, listOne);
            row4 = AppendToRow(year, month, dataIgnore, row4, listTwo);
            row5 = AppendToRow(year, month, dataIgnore, row5, listThree);
            row6 = AppendToRow(year, month, dataIgnore, row6, listFour);
            row7 = AppendToLastRow(year, month, dataIgnore, row7, listFive);
            row8 = AppendToLastRow(year, month, dataIgnore, row8, listSix);

            var row9 = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("<<", CreateCallbackData("PREV-YEAR", year, month, 0)),
                                                    InlineKeyboardButton.WithCallbackData("<", CreateCallbackData("PREV-MONTH", year, month, 0)),
                                                    InlineKeyboardButton.WithCallbackData(" ", dataIgnore),
                                                    InlineKeyboardButton.WithCallbackData(">", CreateCallbackData("NEXT-MONTH", year, month, 0)),
                                                    InlineKeyboardButton.WithCallbackData(">>", CreateCallbackData("NEXT-YEAR", year, month, 0)),
            };

            return new InlineKeyboardMarkup(new InlineKeyboardButton[][] { row1, row2, row3, row4, row5, row6, row7, row8, row9 });
        }

        public InlineKeyboardButton[] AppendToRow(int year, int month, string dataIgnore, InlineKeyboardButton[] row, List<int> list)
        {
            for (int i = (list.Count - 1); i >= 0; i--)
            {
                row = row.Append(InlineKeyboardButton.WithCallbackData(list.ElementAt(i).ToString(), CreateCallbackData("DAY", year, month, list.ElementAt(i)))).ToArray();
                if (i == 0 && list.Count < 7)
                {
                    for (int j = 0; j < 7 - list.Count; j++)
                        row = row.Append(InlineKeyboardButton.WithCallbackData(" ", dataIgnore)).ToArray();
                }
            }
            return row;
        }

        public InlineKeyboardButton[] AppendToLastRow(int year, int month, string dataIgnore, InlineKeyboardButton[] row, List<int> list)
        {
            for (int i = (list.Count - 1); i >= 0; i--)
            {
                if (i == list.Count - 1 && list.Count < 7)
                {
                    for (int j = 0; j < 7 - list.Count; j++)
                        row = row.Append(InlineKeyboardButton.WithCallbackData(" ", dataIgnore)).ToArray();
                }
                row = row.Append(InlineKeyboardButton.WithCallbackData(list.ElementAt(i).ToString(), CreateCallbackData("DAY", year, month, list.ElementAt(i)))).ToArray();
            }
            return row;
        }

        public string CreateCallbackData(string action, int year, int month, int day)
        {
            object[] array = { action, year.ToString(), month.ToString(), day.ToString() };
            return String.Join(";", array);
        }

        public async Task<PersianDateTime> ProcessCalendar(IChatService chatService, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var query = callbackQueryEventArgs.CallbackQuery;
            var data = SeprateCallbackDate(query.Data);
            var action = data[0];
            var year = data[1];
            var month = data[2];
            var day = data[3];

            PersianDateTime curr;
            if (month.Length == 1)
                curr = PersianDateTime.Parse($"{year}/0{month}/01");
            else
                curr = PersianDateTime.Parse($"{year}/{month}/01");

            if (action == "IGNORE")
            {
                await chatService.AnswerCallbackQueryAsync(query.Id);
                return null;
            }
            else if (action == "DAY")
            {
                await chatService.UpdateMessage(chatId: query.Message.Chat.Id,
                                                messageId: query.Message.MessageId,
                                                newText: "تاریخ مورد نظر : " + year + "/" + month + "/" + day);
                if (month.ToString().Length == 1 && day.ToString().Length == 1)
                    return PersianDateTime.Parse($"{year}/0{month}/0{day}");
                else if (month.ToString().Length == 1 && day.ToString().Length != 1)
                    return PersianDateTime.Parse($"{year}/0{month}/{day}");
                else if (month.ToString().Length != 1 && day.ToString().Length == 1)
                    return PersianDateTime.Parse($"{year}/{month}/0{day}");
                else
                    return PersianDateTime.Parse($"{year}/{month}/{day}");
            }
            else if (action == "PREV-MONTH")
            {
                var pre = curr.AddDays(-2);
                await chatService.UpdateMessage(chatId: query.Message.Chat.Id,
                                                messageId: query.Message.MessageId,
                                                newText: query.Message.Text,
                                                inlineKeyboard: CreateCalendar(pre.Year, pre.Month));
                return null;
            }
            else if (action == "NEXT-MONTH")
            {
                var next = curr.AddDays(31);
                await chatService.UpdateMessage(chatId: query.Message.Chat.Id,
                                                messageId: query.Message.MessageId,
                                                newText: query.Message.Text,
                                                inlineKeyboard: CreateCalendar(next.Year, next.Month));
                return null;
            }
            else if (action == "PREV-YEAR")
            {
                var pre = curr.AddYears(-1);
                await chatService.UpdateMessage(chatId: query.Message.Chat.Id,
                                                messageId: query.Message.MessageId,
                                                newText: query.Message.Text,
                                                inlineKeyboard: CreateCalendar(pre.Year, pre.Month));
                return null;
            }
            else if (action == "NEXT-YEAR")
            {
                var next = curr.AddYears(1);
                await chatService.UpdateMessage(chatId: query.Message.Chat.Id,
                                                messageId: query.Message.MessageId,
                                                newText: query.Message.Text,
                                                inlineKeyboard: CreateCalendar(next.Year, next.Month));
                return null;
            }
            else
            {
                await chatService.AnswerCallbackQueryAsync(query.Id);
                return null;
            }
        }

        public static string[] SeprateCallbackDate(string data)
        {
            return data.Split(";");
        }

        public static async Task<Person> ShowPreviousOrNextListInClassicNextSelect(IChatService chatService, Person person)
        {
            var streamTask = client.GetStreamAsync("http://192.168.95.88:30907/api/classicNext/portfolio/all");
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.Rootobject>(await streamTask);
            var list_ = new List<int> { };
            foreach (var item in root.ResponseObject)
            {
                list_.Add(item.Id);
            }
            list_.Reverse();

            var list = new List<string> { "قبلی⬆️" };
            for (long i = person.ClassicNextSelectState; i < Math.Min(20 + person.ClassicNextSelectState, root.ResponseObject.Length); i++)
            {
                list.Add($"پرتفوی شماره {list_.ElementAt(Convert.ToInt32(i))}");
            }

            person.ClassicNextSelectState += 20;
            if (root.ResponseObject.Length - person.ClassicNextSelectState > 0)
            {
                list.Add("بعدی⬇️");
            }
            var buttons = list.Select(x => new[] { new KeyboardButton(x) }).ToArray();
            await chatService.SendMessage(chatId: person.ChatId, message: "پرتفوی مورد نظر را انتخاب کنید :", new ReplyKeyboardMarkup(buttons, resizeKeyboard: true));
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
                    Thread.Sleep(500);
                    await chatService.SendMessage(chatId: person.ChatId, message: "از بین گزینه های زیر انتخاب کنید :", Markup.ReturnOrComparisonRKM);

                    person.CommandLevel = 3;
                    person.PortfolioIdForClassicNextSelect = num;
                }
                else
                {
                    person.CommandLevel = 1;
                    await chatService.SendMessage(chatId: person.ChatId, message: root.ErrorMessageFa);
                    Thread.Sleep(200);
                    await chatService.SendMessage(chatId: person.ChatId, message: "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectTypesRKM);
                }
            }
            catch (Exception)
            {
                person.CommandLevel = 1;
                await chatService.SendMessage(chatId: person.ChatId, message: "ورودی نامعتبر !");
                await chatService.SendMessage(chatId: person.ChatId, message: "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectTypesRKM);
            }
            return person;
        }

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
        
        public static async Task<bool> ShowReturnAndComparisonInPortfolioSet(IChatService chatService, Person person, Task<Stream> streamTask, long portfolioId)
        {
            await chatService.SendMessage(chatId: person.ChatId, message: "تاریخ شروع همان تاریخ ساخت پرتفوی مرکب مورد نظر می باشد ...");
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.RootobjectForCalculateReturnAndComparison>(await streamTask);
            if (root.IsSuccessful)
            {
                await chatService.SendMessage(chatId: person.ChatId, message: $"بازدهی پرتفوی مرکب شماره  {portfolioId} : " + "\n" + Math.Round(Convert.ToDecimal(root.ResponseObject) * 100, 1) + " %", Markup.PortfolioSetSelectRKM);
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

        public static async Task<string> GetBithdayOfPortfolio(Person person)
        {
            var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/{person.PortfolioIdForClassicNextSelect}");
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.RootObjectForSpecificPortfolio>(await streamTask);
            var str = root.ResponseObject.BirthdayPersian;
            return str.Replace("/", "");
        }
        
        public static async Task<string> GetBithdayOfPortfolioSet(Person person)
        {
            var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolioSet/{person.PortfolioIdForClassicNextSelect}");
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.RootObjectForSpecificPortfolio>(await streamTask);
            var str = root.ResponseObject.BirthdayPersian;
            return str.Replace("/", "");
        }

        public static async Task ShowIndextReturnInClassicNextSelect(IChatService chatService, Person person, Task<Stream> streamTask)
        {
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.RootobjectForCalculateReturnAndComparison>(await streamTask);
            if (root.IsSuccessful)
                await chatService.SendMessage(chatId: person.ChatId, message: $"بازدهی شاخص تا همین زمان : " + "\n" + Math.Round(Convert.ToDecimal(root.ResponseObject) * 100, 1) + " %");
            else
                await chatService.SendMessage(chatId: person.ChatId, message: root.ErrorMessageFa);
            Thread.Sleep(300);
        }

        public static async Task ShowReturnOfEveryStockInPortfolio(IChatService chatService, Person person, string text)
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
            var resObj = JsonConvert.DeserializeObject<StockReturn.StockReturnRoot>(response);
            StringBuilder str = new();

            foreach (var item in resObj.ResponseObject)
            {
                str.Append($"{symbols.First()} : " + "\n" + Math.Round(item * 100, 1) + " %\n"); //__________________________________________________ $"(%w : {Math.Round(weights.First() * 100, 1)})" +
                symbols.RemoveAt(0);
                //weights.RemoveAt(0);
            }
            await chatService.SendMessage(chatId: person.ChatId, message: str.ToString());
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
                    person.CommandLevel = 1;
                    await chatService.SendMessage(person.ChatId, message: root.ErrorMessageFa);
                    Thread.Sleep(200);
                    await chatService.SendMessage(person.ChatId, message: "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectTypesRKM);
                }
            }
            catch (Exception)
            {
                person.CommandLevel = 1;
                await chatService.SendMessage(person.ChatId, message: "ورودی نامعتبر !");
                await chatService.SendMessage(person.ChatId, message: "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectTypesRKM);
            }
            return person;
        }

        public static async Task<Person> ShowPreviousOrNextListInPortfolioSetSelect(IChatService chatService, Person person)
        {
            var streamTask = client.GetStreamAsync("http://192.168.95.88:30907/api/classicNext/portfolioSet/all");
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.Rootobject>(await streamTask);
            var list_ = new List<int> { };
            foreach (var item in root.ResponseObject)
            {
                list_.Add(item.Id);
            }
            list_.Reverse();

            var list = new List<string> { "قبلی⬆️" };
            for (long i = person.ClassicNextSelectState; i < Math.Min(20 + person.ClassicNextSelectState, root.ResponseObject.Length); i++)
            {
                list.Add($"پرتفوی مرکب شماره {list_.ElementAt(Convert.ToInt32(i))}");
            }

            person.ClassicNextSelectState += 20;
            if (root.ResponseObject.Length - person.ClassicNextSelectState > 0)
            {
                list.Add("بعدی⬇️");
            }
            var buttons = list.Select(x => new[] { new KeyboardButton(x) }).ToArray();
            await chatService.SendMessage(person.ChatId, message: "پرتفوی مرکب مورد نظر را انتخاب کنید :", new ReplyKeyboardMarkup(buttons, resizeKeyboard: true));
            return person;
        }

        public static InlineKeyboardMarkup GetRiskInlineKeyboard()
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
            return inlineKeyboard;
        }

        public static InlineKeyboardMarkup GetSaveInlineKeyboard()
        {
            InlineKeyboardMarkup inlineKeyboard = new(new InlineKeyboardButton[][]
                                    {
                            new InlineKeyboardButton[]{
                                InlineKeyboardButton.WithCallbackData("بلی", "بلی"),
                                InlineKeyboardButton.WithCallbackData("خیر", "خیر"),
                            }
            });
            return inlineKeyboard;
        }

        public static async void SendSmartPortfolioToUser(IChatService chatService, Person person, int s)
        {
            await chatService.SendMessage(person.ChatId, message: "در حال ساخت پرتفوی مورد نظر ...");
            System.Threading.Tasks.Task<Stream> streamTask = null;
            if (s == 0)
                await (streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/create/smart/{person.Save}"));
            else if (s == 1)
                streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/create/smart/{person.Save}/{person.RiskRate}");
            else if (s == 2)
                streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/create/smart/{person.Save}/{person.RiskRate}/{person.MinimumStockWeight}");
            else if (s == 3)
                streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/create/smart/{person.Save}/{person.RiskRate}/{person.MinimumStockWeight}/{person.MaximumStockWeight}");
            else if (s == 4)
                await (streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/create/smart/{person.Save}/{person.RiskRate}/{person.MinimumStockWeight}/{person.MaximumStockWeight}/{person.ProductionDate}"));

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
                await chatService.SendMessage(person.ChatId, message: str.ToString());
                Thread.Sleep(500);

                await chatService.SendMessage(person.ChatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.CreateTypesRKM);
            }
            else
            {
                await chatService.SendMessage(person.ChatId, message: root.ErrorMessageFa);
                Thread.Sleep(200);
                await chatService.SendMessage(person.ChatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.CreateTypesRKM);
            }
        }

        public static bool SaveTickerKey(Person person, string text, List<IndustryStocks.Industry> industries)
        {
            foreach (var industry in industries)
            {
                foreach (var stock in industry.Stocks)
                {
                    if (stock.Symbol == text)
                    {
                        person.TickerKeyForStock = stock.TickerKey;
                        return true;
                    }
                }

            }
            return false;
        }
        
        public static bool SaveTickerKeyForETF(Person person, string text, Models.ETF.All.Rootobject etfs)
        {
            foreach (var etf in etfs.responseObject)
            {
                if (etf.symbol == text)
                {
                    person.TickerKeyForStock = etf.tickerKey;
                    return true;
                }
            }
            return false;
        }

        public async Task<string> CheckAndGetDate(IChatService chatService, CallbackQueryEventArgs? query)
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
                    return date;
                }
            }
            return null;
        }
    }
}
