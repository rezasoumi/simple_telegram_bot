using Microsoft.Extensions.DependencyInjection;
using NextBot.Alteranives;
using NextBot.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace NextBot.Commands
{
    public class SPortfoli : StaticFunctionForPortfolio, IBotCommand
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

        public async Task<MyDbContext> Execute(IChatService chatService, long chatId, long userId, int messageId, string commandText, CallbackQuery query)
        {
            var person = _context.People.FirstOrDefault(p => p.ChatId == chatId);
            person.CommandState = 5; // sportfolio commandstate is 5

            if (person.CommandLevel == 0)
            {
                person.CommandLevel = 2;
                person.ClassicNextSelectState = 1;
                person = await ShowPreviousOrNextListInClassicNextSelect(chatService, person, null);
            }
            else if (person.CommandLevel == 2)
            {
                person = await SwitchProcessForSelectPortfolio(chatService, chatId, commandText, person);
            }
            else if (person.CommandLevel == 3)
            {
                person = await SwitchProcessForMainMenuOfPortfolio(chatService, chatId, commandText, person);
            }
            else if (person.CommandLevel == 4)
            {
                person = await SwitchProcessForComparison(chatService, chatId, commandText, person);
            }
            else if (person.CommandLevel == 6)
            {
                person = await ComparisonPortfolioWithIndexToSpecificDate(chatService, chatId, query, person);
            }
            else if (person.CommandLevel == 7)
            {
                person.StartDateWaitingForEndDate = commandText; // instead of create another property use previous one that in this state is useless.
                await chatService.SendMessage(chatId: person.ChatId, message: "مقایسه با پرتفوی مورد نظر تا تاریخ مشخص -> تاریخ مورد نظر را انتخاب کنید", CreateCalendar());
                person.CommandLevel = 8;
            }
            else if (person.CommandLevel == 8)
            {
                person = await ComparisonTwoPortfolioToSpecificDate(chatService, chatId, query, person);
            }
            else if (person.CommandLevel == 9)
            {
                person = await SwitchProcessForReturnPortfolio(chatService, chatId, commandText, person);
            }
            else if (person.CommandLevel == 10)
            {
                person = await ReturnPortfolioToSpecificDate(chatService, chatId, query, person);
            }
            else if (person.CommandLevel == 11)
            {
                person = await ComparisonPortfolioWithETFsToSpecificDate(chatService, chatId, query, person);
            }
            else if (person.CommandLevel == 12)
            {
                person = await ComparisonTwoPortfolioToToday(chatService, chatId, commandText, person);
            }
            else if (person.CommandLevel == 14)
            {
                person = await CheckDeletePortfolioOrNot(chatService, chatId, commandText, query, person);
            }
            
            await _context.SaveChangesAsync();
            return _context;
        }
    }
}
