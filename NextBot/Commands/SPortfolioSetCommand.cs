using Microsoft.Extensions.DependencyInjection;
using NextBot.Alteranives;
using NextBot.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace NextBot.Commands
{
    public class SPortfolioSetCommand : StaticFunctionForPortfolioSet, IBotCommand
    {
        private readonly IServiceProvider _serviceProvider;
        private MyDbContext _context;

        public string Command => "sportfolioset";

        public string Description => "انتخاب پرتفوی مرکب";

        public bool InternalCommand => false;

        public SPortfolioSetCommand(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var scope = serviceProvider.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        }

        public async Task<MyDbContext> Execute(IChatService chatService, long chatId, long userId, int messageId, string? commandText, CallbackQuery? query)
        {
            var person = _context.People.FirstOrDefault(p => p.ChatId == chatId);
            person.CommandState = 3; // sportfolioset commandstate is 3

            if (person.CommandLevel == 0)
            {
                person.CommandLevel = 2;
                person.ClassicNextSelectState = 1;
                person = await ShowPreviousOrNextListInPortfolioSetSelect(chatService, person, null);
            }
            else if (person.CommandLevel == 2)
            {
                person = await SwitchProcessForSelectPortfolioSet(chatService, chatId, commandText, person);
            }
            else if (person.CommandLevel == 3)
            {
                person = await SwitchProcessForMainMenuOfPortfolioSet(chatService, chatId, commandText, person);
            }
            else if (person.CommandLevel == 4)
            {
                person = await PostRequestForAddPortfolioToPortfolioSet(chatService, chatId, commandText, person);
            }
            else if (person.CommandLevel == 5)
            {
                person = await SendReturnOfPortfolioSetInSpecificDate(chatService, chatId, query, person);
            }
            else if (person.CommandLevel == 7)
            {
                person = await SwitchProcessForReturnOfPortfolioSet(chatService, chatId, commandText, person);
            }
            else if (person.CommandLevel == 8)
            {
                person = await DeletePortfolioSetOrNot(chatService, chatId, commandText, query, person);
            }
            else if (person.CommandLevel == 9)
            {
                person = await SwitchProcessForComparison(chatService, chatId, commandText, person);
            }
            else if (person.CommandLevel == 11)
            {
                person = await ComparisonPortfolioSetWithIndexToSpecificDate(chatService, chatId, query, person);
            }
            else if (person.CommandLevel == 12)
            {
                person = await ComparisonPortfolioSetWithETFsToSpecificDate(chatService, chatId, query, person);
            }
            else if (person.CommandLevel == 13)
            {
                person = await ComparisonTwoPortfolioSetToToday(chatService, chatId, commandText, person);
            }
            else if (person.CommandLevel == 14)
            {
                person.StartDateWaitingForEndDate = commandText; // instead of create another property use previous one that in this state is useless.
                await chatService.SendMessage(chatId: person.ChatId, message: "مقایسه با پرتفوی مرکب مورد نظر تا تاریخ مشخص -> تاریخ مورد نظر را انتخاب کنید", CreateCalendar());
                person.CommandLevel = 15;
            }
            else if (person.CommandLevel == 15)
            {
                person = await ComparisonTwoPortfolioSetToSpecificDate(chatService, chatId, query, person);
            }

            await _context.SaveChangesAsync();
            return _context;
        }
    }
}
