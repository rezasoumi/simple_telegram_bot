﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NextBot.Handlers;
using NextBot.Models;
using NextBot.SmartSearch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace NextBot.Commands
{
    public class SPortfoli : StaticFunctions, IBotCommand
    {
        private readonly IServiceProvider _serviceProvider;
        private MyDbContext _context;

        public string Command => "sportfolio";

        public string Description => "انتخاب پرتفوی هوشمند";

        public bool InternalCommand => false;

        public SPortfoli(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var scope = serviceProvider.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        }

        public async Task<MyDbContext> Execute(IChatService chatService, long chatId, int userId, int messageId, string commandText, CallbackQueryEventArgs query)
        {
            var person = _context.People.FirstOrDefault(p => p.ChatId == chatId);
            person.CommandState = 5;
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
                        person.CommandLevel = 13;
                        await chatService.SendMessage(chatId, message: "آی دی پرتفوی مورد نظر را به صورت یک عدد انگلیسی وارد کنید :");
                        break;
                    case "انتخاب بر اساس گذر میان پرتفوی ها":
                        person.CommandLevel = 2;
                        person.ClassicNextSelectState = 1;
                        person =  await ShowPreviousOrNextListInClassicNextSelect(chatService, person);
                        break;
                    case "بازگشت":
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
                    case "بعدی":
                        person = await ShowPreviousOrNextListInClassicNextSelect(chatService, person);
                        break;
                    case "قبلی":
                        if (person.ClassicNextSelectState == 21)
                        {
                            person.CommandLevel = 1;
                            await chatService.SendMessage(chatId, message: "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectTypesRKM);
                            break;
                        }
                        person.ClassicNextSelectState -= 40;
                        person = await ShowPreviousOrNextListInClassicNextSelect(chatService, person);
                        break;
                    default:
                        var split = commandText.Split(" ");
                        var strNum = split[2];
                        person = await ShowSpecificPortfolioInClassicNextSelect(chatService, person, strNum);
                        break;
                }
                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 3)
            {
                switch (commandText)
                {
                    case "مقایسه":
                        person.CommandLevel = 4;
                        await chatService.SendMessage(chatId: chatId, message: "گزینه مورد نظر را انتخاب کنید :", Markup.ComparisonTypesRKM);
                        break;
                    case "محاسبه بازدهی":
                        person.CommandLevel = 9;
                        await chatService.SendMessage(chatId: chatId, message: "نوع بازدهی را از بین دو گزینه زیر انتخاب کنید :", Markup.ReturnPortfolioTypesRKM);
                        break;
                    case "حذف پرتفوی":
                        person.CommandLevel = 14;
                        await chatService.SendMessage(chatId, message: "پرتفوی مورد نظر حذف شود ؟", GetSaveInlineKeyboard());
                        break;
                    case "بازگشت":
                        person.CommandLevel = 1;
                        await chatService.SendMessage(chatId: chatId, message: "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectTypesRKM);
                        break;
                    default:
                        break;
                }
                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 4)
            {
                switch (commandText)
                {
                    case "شاخص":
                        person.CommandLevel = 5;
                        await chatService.SendMessage(chatId: chatId, message: "نوع بازدهی را مشخص کنید :", Markup.ReturnIndexTypesRKM);
                        break;
                    case "صندوق سهامی":
                        var streamTask = client.GetStreamAsync("http://192.168.95.88:30907/api/fund/etf/all");
                        var etfs = await System.Text.Json.JsonSerializer.DeserializeAsync<Models.ETF.All.Rootobject>(await streamTask);

                        var symbols = new List<String>();
                        foreach (var etf in etfs.responseObject)
                        {
                            symbols.Add(etf.symbol);
                        }

                        var smartDictionary = new SmartDDictionary<string>(m => m, symbols);

                        var buttons = smartDictionary.Search(commandText, 50).Select(x => new[] { new KeyboardButton(x) }).ToArray();

                        await chatService.SendMessage(chatId, message: "صندوق مورد نظر را از گزینه های موجود انتخاب کنید :", rkm: new ReplyKeyboardMarkup(buttons, resizeKeyboard: true));
                        person.CommandLevel = 11;
                        break;
                    case "پرتفوی":
                        person.CommandLevel = 7;
                        await chatService.SendMessage(chatId: chatId, message: "مقایسه با پرتفوی مورد نظر تا تاریخ مشخص -> آی دی پرتفوی مورد نظر برای مقایسه وارد نمایید :");
                        break;
                    case "بازگشت":
                        person.CommandLevel = 3;
                        await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnOrComparisonRKM);
                        break;
                    default:
                        break;
                }
                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 5)
            {
                switch (commandText)
                {
                    case "بازدهی شاخص تا تاریخ دلخواه":
                        person.CommandLevel = 6;
                        await chatService.SendMessage(chatId: chatId, message: "تاریخ مورد نظر خود را انتخاب کنید :", CreateCalendar());
                        break;
                    case "بازدهی شاخص تا امروز":
                        var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.PortfolioIdForClassicNextSelect}");
                        if (await ShowReturnAndComparisonInClassicNextSelect(chatService, person, streamTask, person.PortfolioIdForClassicNextSelect))
                        {
                            var date = await GetBithdayOfPortfolio(person);
                            var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/stock/return/index/{date}");
                            await ShowIndextReturnInClassicNextSelect(chatService, person, streamTask_);
                        }
                        Thread.Sleep(500);
                        await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnIndexTypesRKM);
                        break;
                    case "بازگشت":
                        person.CommandLevel = 4;
                        await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonTypesRKM);
                        break;
                    default:
                        await chatService.SendMessage(chatId: chatId, message: "ورودی نامعتبر! از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnIndexTypesRKM);
                        break;
                }
                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 6)
            {
                var date = await CheckAndGetDate(chatService, query);
                if (date != null)
                {
                    var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.PortfolioIdForClassicNextSelect}/{date}");
                    if (await ShowReturnAndComparisonInClassicNextSelect(chatService, person, streamTask, person.PortfolioIdForClassicNextSelect))
                    {
                        var date_ = await GetBithdayOfPortfolio(person);
                        var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/stock/return/index/{date_}/{date}");
                        await ShowIndextReturnInClassicNextSelect(chatService, person, streamTask_);
                    }
                    Thread.Sleep(500);
                    await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnIndexTypesRKM);
                    person.CommandLevel = 5;
                }
                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 7)
            {
                person.StartDateWaitingForEndDate = commandText; // instead of create another property use previous one that in this state is useless.
                await chatService.SendMessage(chatId: person.ChatId, message: "مقایسه با پرتفوی مورد نظر تا تاریخ مشخص -> تاریخ مورد نظر را انتخاب کنید", CreateCalendar());
                person.CommandLevel = 8;
                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 8)
            {
                var date = await CheckAndGetDate(chatService, query);
                if (date != null)
                {
                    var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.PortfolioIdForClassicNextSelect}/{date}");
                    if (await ShowReturnAndComparisonInClassicNextSelect(chatService, person, streamTask, person.PortfolioIdForClassicNextSelect))
                    {
                        var streamTask_ = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.StartDateWaitingForEndDate}/{date}");
                        await ShowReturnAndComparisonInClassicNextSelect(chatService, person, streamTask_, long.Parse(person.StartDateWaitingForEndDate));
                    }
                    person.CommandLevel = 4;
                    await chatService.SendMessage(chatId: person.ChatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonTypesRKM);
                }
                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 9)
            {
                switch (commandText)
                {
                    case "بازدهی پرتفوی تا تاریخ دلخواه":
                        person.CommandLevel = 10;
                        await chatService.SendMessage(chatId: person.ChatId, message: "تاریخ مورد نظر خود را انتخاب کنید :", CreateCalendar());
                        break;
                    case "بازدهی پرتفوی تا امروز":
                        var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.PortfolioIdForClassicNextSelect}");
                        if (await ShowReturnAndComparisonInClassicNextSelect(chatService, person, streamTask, person.PortfolioIdForClassicNextSelect))
                        {
                            await chatService.SendMessage(chatId: person.ChatId, message: "بازدهی سهم های موجود در پرتفوی :");

                            await ShowReturnOfEveryStockInPortfolio(chatService, person, null);
                        }
                        Thread.Sleep(1000);
                        await chatService.SendMessage(chatId: person.ChatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnPortfolioTypesRKM);
                        break;
                    case "بازگشت":
                        person.CommandLevel = 3;
                        await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnOrComparisonRKM);
                        break;
                    default:
                        await chatService.SendMessage(chatId: person.ChatId, message: "ورودی نامعتبر! از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnPortfolioTypesRKM);
                        break;
                }

                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 10)
            {
                var date = await CheckAndGetDate(chatService, query);
                if (date != null)
                {
                    var streamTask = client.GetStreamAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/return/{person.PortfolioIdForClassicNextSelect}/{date}");
                    if (await ShowReturnAndComparisonInClassicNextSelect(chatService, person, streamTask, person.PortfolioIdForClassicNextSelect))
                    {
                        await chatService.SendMessage(chatId: person.ChatId, message: "بازدهی سهم های موجود در پرتفوی :");
                        await ShowReturnOfEveryStockInPortfolio(chatService, person, date);
                    }
                    Thread.Sleep(1000);
                    await chatService.SendMessage(chatId: person.ChatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnPortfolioTypesRKM);
                    person.CommandLevel = 9;
                }
                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 11)
            {
                var streamTask = client.GetStreamAsync("http://192.168.95.88:30907/api/fund/etf/all");
                var etfs = await System.Text.Json.JsonSerializer.DeserializeAsync<Models.ETF.All.Rootobject>(await streamTask);
                if (SaveTickerKeyForETF(person, commandText, etfs))
                {
                    await chatService.SendMessage(chatId, message: "تاریخ پایان محاسبه بازدهی را انتخاب کنید", rkm: CreateCalendar());
                    person.CommandLevel = 12;
                }
                else
                {
                    person.CommandLevel = 4;
                    await chatService.SendMessage(chatId, message: "ورودی اشتباه ! از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonTypesRKM);

                }
            }
            else if (person.CommandLevel == 12)
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
                        foreach (var etf in etfs.responseObject)
                        {
                            if (etf.fund.tickerKey == person.TickerKeyForStock)
                            {
                                await chatService.SendMessage(chatId: person.ChatId, message: $"بازدهی صندوق  {etf.fund.symbol} : " + "\n" + Math.Round(Convert.ToDecimal(etf.returnValue) * 100, 1) + " %");
                                break;
                            }
                        }
                    }
                    else
                    {
                        await chatService.SendMessage(chatId: chatId, message: etfs.errorMessageFa);
                    }

                    Thread.Sleep(1000);
                    await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ComparisonTypesRKM);
                    person.CommandLevel = 4;
                }  
                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 13)
            {
                person = await ShowSpecificPortfolioInClassicNextSelect(chatService, person, commandText);
                _context.Entry(person).State = EntityState.Modified;
            }
            else if (person.CommandLevel == 14)
            {
                if (commandText == "خیر")
                {
                    await chatService.UpdateMessage(chatId: query.CallbackQuery.Message.Chat.Id,
                                                            messageId: query.CallbackQuery.Message.MessageId,
                                                            newText: "پرتفوی مورد نظر حذف نشد");
                    await chatService.SendMessage(chatId: chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnOrComparisonRKM);
                    person.CommandLevel = 3;
                }
                else if (commandText == "بلی")
                {
                    await chatService.UpdateMessage(chatId: query.CallbackQuery.Message.Chat.Id,
                                                            messageId: query.CallbackQuery.Message.MessageId,
                                                            newText: "پرتفوی مورد نظر حذف شود");
                    var streamTask_ = client.DeleteAsync($"http://192.168.95.88:30907/api/classicNext/portfolio/{person.PortfolioIdForClassicNextSelect}");
                    var x = streamTask_.Result.Content.ReadAsStreamAsync();
                    var res = await System.Text.Json.JsonSerializer.DeserializeAsync<ClassicNextSelect.Delete>(await x);
                    if (res.isSuccessful)
                    {
                        await chatService.SendMessage(chatId: chatId, message: $"پرتفوی شماره {person.PortfolioIdForClassicNextSelect} حذف شد");
                        await chatService.SendMessage(chatId, message: "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectTypesRKM);
                        person.CommandLevel = 1;
                    }
                    else
                    {
                        await chatService.SendMessage(chatId: chatId, message: res.errorMessageFa);
                        await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.ReturnOrComparisonRKM);
                        person.CommandLevel = 3;
                    }
                }
                _context.Entry(person).State = EntityState.Modified;
            }

            _context.SaveChanges();
            return _context;
        }
    }
}
