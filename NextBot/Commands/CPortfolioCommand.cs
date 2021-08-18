using Microsoft.Extensions.DependencyInjection;
using NextBot.Alteranives;
using NextBot.Handlers;
using NextBot.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace NextBot.Commands
{
    public class CPortfolioCommand : StaticFunctions, IBotCommand
    {
        private readonly IServiceProvider _serviceProvider;
        private MyDbContext _context;

        public string Command => "cportfolio";

        public string Description => "تشکیل پرتفوی هوشمند";

        public bool InternalCommand => false;

        public CPortfolioCommand(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var scope = serviceProvider.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        }

        public async Task<MyDbContext> Execute(IChatService chatService, long chatId, long userId, int messageId, string? commandText, CallbackQuery? query)
        {
            var person = _context.People.FirstOrDefault(p => p.ChatId == chatId);
            person.CommandState = 2;

            if (person.CommandLevel == 0)
            {
                await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.CreateTypesRKM);
                person.CommandLevel = 1;
            }
            else if (person.CommandLevel == 1)
            {
                switch (commandText)
                {
                    case "ساخت با پارامتر های پیش فرض":
                        person.CreateSmartPortfolioType = 0;
                        break;
                    case "ساخت با ریسک مشخص":
                        person.CreateSmartPortfolioType = 1;
                        break;
                    case "ساخت با ریسک و حداقل وزن مشخص":
                        person.CreateSmartPortfolioType = 2;
                        break;
                    case "ساخت با ریسک و حداقل و حداکثر وزن مشخص":
                        person.CreateSmartPortfolioType = 3;
                        break;
                    case "ساخت با ریسک و حداقل و حداکثر و تاریخ شمسی مشخص":
                        person.CreateSmartPortfolioType = 4;
                        break;
                    case "🔙":
                        person.CommandState = 0;
                        person.CommandLevel = 2;
                        await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.SelectOrCreateRKM);
                        break;
                    default:
                        await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.CreateTypesRKM);
                        break;
                }
                if (person.CommandState != 0)
                {
                    await chatService.SendMessage(chatId, message: "پرتفوی مورد نظر ذخیره شود ؟", GetSaveInlineKeyboard());
                    person.CommandLevel = 2;
                }

            }
            else if (person.CommandLevel == 2)
            {
                if (commandText == "خیر")
                {
                    await chatService.UpdateMessage(chatId: query.Message.Chat.Id,
                                                            messageId: query.Message.MessageId,
                                                            newText: "پرتفوی مورد نظر ذخیره نمی شود");
                    person.Save = false;
                }
                else if (commandText == "بلی")
                {
                    await chatService.UpdateMessage(chatId: query.Message.Chat.Id,
                                                            messageId: query.Message.MessageId,
                                                            newText: "پرتفوی مورد نظر ذخیره می شود");
                    person.Save = true;
                }
                if (person.CreateSmartPortfolioType == 0)
                {
                    SendSmartPortfolioToUser(chatService, person, 0);
                    person.CommandLevel = 1;
                }
                else
                {
                    await chatService.SendMessage(chatId, message: "ریسک مورد نظر خود را انتخاب کنید :", GetRiskInlineKeyboard());
                    person.CommandLevel = 3;
                }
            }
            else if (person.CommandLevel == 3)
            {
                if (commandText == "بدون ریسک")
                {
                    await chatService.UpdateMessage(chatId: query.Message.Chat.Id,
                                                    messageId: query.Message.MessageId,
                                                    newText: "درجه ریسک : بدون ریسک");
                    person.RiskRate = 0;
                }
                else if (commandText == "ریسک خیلی کم")
                {
                    await chatService.UpdateMessage(chatId: query.Message.Chat.Id,
                                                    messageId: query.Message.MessageId,
                                                    newText: "درجه ریسک : ریسک خیلی کم");
                    person.RiskRate = 1;
                }
                else if (commandText == "ریسک کم")
                {
                    await chatService.UpdateMessage(chatId: query.Message.Chat.Id,
                                                    messageId: query.Message.MessageId,
                                                    newText: "درجه ریسک : ریسک کم");
                    person.RiskRate = 2;
                }
                else if (commandText == "ریسک متوسط")
                {
                    await chatService.UpdateMessage(chatId: query.Message.Chat.Id,
                                                    messageId: query.Message.MessageId,
                                                    newText: "درجه ریسک : ریسک متوسط");
                    person.RiskRate = 3;
                }
                else if (commandText == "ریسک زیاد")
                {
                    await chatService.UpdateMessage(chatId: query.Message.Chat.Id,
                                                    messageId: query.Message.MessageId,
                                                    newText: "درجه ریسک : ریسک زیاد");
                    person.RiskRate = 4;
                }
                else if (commandText == "ریسک خیلی زیاد")
                {
                    await chatService.UpdateMessage(chatId: query.Message.Chat.Id,
                                                    messageId: query.Message.MessageId,
                                                    newText: "درجه ریسک : ریسک خیلی زیاد");
                    person.RiskRate = 5;
                }
                if (person.CreateSmartPortfolioType == 1)
                {
                    SendSmartPortfolioToUser(chatService, person, 1);
                    person.CommandLevel = 1;
                }
                else
                {
                    await chatService.SendMessage(chatId, message: "یک عدد به عنوان حداقل وزن سهام ها بین 0.01 و 1 وارد کنید: (عدد را به انگلیسی وارد کنید)");
                    person.CommandLevel = 4;
                }
            }
            else if (person.CommandLevel == 4)
            {
                if (GetMinimumStockWeight(chatService, person, commandText))
                {
                    if (person.CreateSmartPortfolioType == 2)
                    {
                        SendSmartPortfolioToUser(chatService, person, 2);
                        person.CommandLevel = 1;
                    }
                    else
                    {
                        await chatService.SendMessage(chatId, message: "یک عدد به عنوان حداکثر وزن سهام ها بین 0.05 و 1 وارد کنید: (عدد را به انگلیسی وارد کنید)");
                        person.CommandLevel = 5;
                    }
                }
            }
            else if (person.CommandLevel == 5)
            {
                if (GetMaximumStockWeight(chatService, person, commandText))
                {
                    if (person.CreateSmartPortfolioType == 3)
                    {
                        SendSmartPortfolioToUser(chatService, person, 3);
                        person.CommandLevel = 1;
                    }
                    else
                    {
                        await chatService.SendMessage(chatId, message: "تاریخ ساخت خود را که بین 13990101 و تاریخ حال حاضر است انتخاب کنید", CreateCalendar());
                        person.CommandLevel = 6;
                    }
                }
            }
            else if (person.CommandLevel == 6)
            {
                var date = await CheckAndGetDate(chatService, query);
                if (date != null)
                {
                    person.ProductionDate = date;
                    SendSmartPortfolioToUser(chatService, person, 4);
                    person.CommandLevel = 1;
                }
                if (query == null)
                {
                    await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.CreateTypesRKM);
                    person.CommandLevel = 1;
                }
            }
            _context.SaveChanges();
            return _context;
        }

        private bool GetMinimumStockWeight(IChatService chatService, Person person, string text)
        {
            try
            {
                if (double.Parse(text) > 0.01 && double.Parse(text) < 1)
                {
                    person.MinimumStockWeight = double.Parse(text);
                    _context.SaveChanges();
                    return true;
                }
                else
                    chatService.SendMessage(chatId: person.ChatId, message: "لطفا یک عدد در بازه گفته شده وارد نمایید :");
            }
            catch (Exception)
            {
                chatService.SendMessage(chatId: person.ChatId, message: "ورودی نامعتبر! لطفا دوباره تلاش کنید");
            }
            return false;
        }

        private bool GetMaximumStockWeight(IChatService chatService, Person person, string text)
        {
            try
            {
                if (double.Parse(text) > 0.05 && double.Parse(text) < 1)
                {
                    person.MaximumStockWeight = double.Parse(text);
                    _context.SaveChanges();
                    return true;
                }
                else
                    chatService.SendMessage(chatId: person.ChatId, message: "لطفا یک عدد در بازه گفته شده وارد نمایید :");
            }
            catch (Exception)
            {
                chatService.SendMessage(chatId: person.ChatId, message: "ورودی نامعتبر! لطفا دوباره تلاش کنید");
            }
            return false;
        }

    }
}
