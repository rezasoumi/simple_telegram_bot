using NextBot.Models;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace NextBot.Commands
{
    public interface IBotCommand
    {
        string Command { get; }
        string Description { get; }
        bool InternalCommand { get; }

        Task<MyDbContext> Execute(IChatService chatService, long chatId, long userId, int messageId, string? commandText, CallbackQuery? query);
    }
}
