﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace NextBot
{
    public interface IChatService
    {
        event EventHandler<ChatMessageEventArgs> ChatMessage;
        event EventHandler<CallbackEventArgs>? Callback;

        Task<string> BotUserName();
        Task<bool> SendMessage(long chatId, string? message, IReplyMarkup? rkm = null, Dictionary<string, string>? buttons = null);
        Task<bool> UpdateMessage(long chatId, int messageId, string newText, Dictionary<string, string>? buttons = null);
        Task<string> GetChatMemberName(long chatId, int userId);
    }
}