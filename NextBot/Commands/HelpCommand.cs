using NextBot.Models;
using System;
using System.Threading.Tasks;

namespace NextBot.Commands
{
    public class HelpCommand : IBotCommand
    {
        private readonly IServiceProvider _serviceProvider;

        public string Command => "help";

        public string Description => "Get information about the functionality available for this bot";

        public bool InternalCommand => false;

        public HelpCommand(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Execute(IChatService chatService, long chatId, int userId, int messageId, string? commandText, MyDbContext _context)
        {
            await chatService.SendMessage(chatId, "TODO: Create a todo command");
        }
    }
}
