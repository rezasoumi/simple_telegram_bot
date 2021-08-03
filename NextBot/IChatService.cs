using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace NextBot
{
    public interface IChatService
    {
        event EventHandler<ChatMessageEventArgs> ChatMessage;
        event EventHandler<CallbackQueryEventArgs>? Callback;

        Task<string> BotUserName();
        Task<bool> SendMessage(long chatId, string? message, IReplyMarkup? rkm = null, Dictionary<string, string>? buttons = null);
        Task<bool> UpdateMessage(long chatId, int messageId, string newText, InlineKeyboardMarkup inlineKeyboard = null);
        Task<string> GetChatMemberName(long chatId, int userId);
        Task<bool> AnswerCallbackQueryAsync(string callbackQueryId);
    }
}
