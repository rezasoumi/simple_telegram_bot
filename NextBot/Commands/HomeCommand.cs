using Microsoft.Extensions.DependencyInjection;
using NextBot.Handlers;
using NextBot.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Args;

namespace NextBot.Commands
{
    public class HomeCommand : IBotCommand
    {
        private readonly IServiceProvider _serviceProvider;
        private MyDbContext _context;

        public string Command => "home";

        public string Description => "صفحه اصلی";

        public bool InternalCommand => false;

        public HomeCommand(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var scope = serviceProvider.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        }

        public async Task<MyDbContext> Execute(IChatService chatService, long chatId, long userId, int messageId, string? commandText, CallbackQueryEventArgs? query)
        {
            var person = _context.People.FirstOrDefault(p => p.ChatId == chatId);
            person.CommandState = 0;
            person.CommandLevel = 0;
            await chatService.SendMessage(chatId, message: "از گزینه های موجود یک گزینه را انتخاب کنید :", Markup.MainMenuRKM);
            _context.SaveChanges();
            return _context;
        }
    }
}
