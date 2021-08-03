using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NextBot.Commands;
using NextBot.Handlers;
using NextBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Args;

namespace NextBot
{
    public class Bot : IHostedService
    {
        private readonly IChatService _chatService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Bot> _logger;
        private MyDbContext _context;

        public const string UnknownCommandMessage = "متاسفانه چنین دستوری وجود ندارد.";

        public Bot(IChatService chatService, IServiceProvider serviceProvider, ILogger<Bot> logger)
        {
            _chatService = chatService;
            _serviceProvider = serviceProvider;
            _logger = logger; 
            var scope = serviceProvider.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _chatService.ChatMessage += OnChatMessage;
            _chatService.Callback += OnCallback;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _chatService.ChatMessage -= OnChatMessage;
            return Task.CompletedTask;
        }

        private async void OnChatMessage(object? sender, ChatMessageEventArgs chatMessageArgs)
        {
            try
            {
                await ProcessChatMessage(sender, chatMessageArgs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Time}: OnChatMessage - Error {Exception}", DateTime.UtcNow, ex.Message);
            }
        }

        private async void OnCallback(object? sender, CallbackQueryEventArgs callbackEventArgs)
        {
            try
            {
                await ProcessCallback(sender, callbackEventArgs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Time}: OnChatMessage - Error {Exception}", DateTime.UtcNow, ex.Message);
            }
        }

        private async Task ProcessChatMessage(object? sender, ChatMessageEventArgs chatMessageArgs)
        {
            if (sender is IChatService chatService)
            {
                var command = _serviceProvider.GetServices<IBotCommand>().SingleOrDefault(x => $"/{x.Command}".Equals(chatMessageArgs.Command, StringComparison.InvariantCultureIgnoreCase));
                if (command != null)
                {
                    try
                    {
                        _context = await command.Execute(chatService,
                            chatMessageArgs.ChatId,
                            chatMessageArgs.UserId,
                            chatMessageArgs.MessageId,
                            chatMessageArgs.Text,
                            null
                            );

                    }
                    catch (Exception)
                    {
                        await chatService.SendMessage(chatMessageArgs.ChatId, "خطایی رخ داده است. لطفا مجددا تلاش کنید.");
                    }
                }
                else
                {
                    if (chatMessageArgs.Command == "/start")
                    {
                        await chatService.SendMessage(chatMessageArgs.ChatId, "سلام به نکست بات خوش آمدید. منوی اصلی را در پایین صفحه مشاهده می کنید. همچنین برای دریافت آموزش بات روی /help می توانید کلیک کنید. با تشکر ", Markup.MainMenuRKM);
                    }
                    else
                    {
                        _logger.LogTrace("Unknown command was sent");
                        await chatService.SendMessage(chatMessageArgs.ChatId, UnknownCommandMessage, Markup.MainMenuRKM);
                    }
                }
            }
        }

        private async Task ProcessCallback(object? sender, CallbackQueryEventArgs query)
        {
            var person = _context.People.FirstOrDefault(p => p.ChatId == query.CallbackQuery.Message.Chat.Id);

            if (sender is IChatService chatService)
            {
                string commandText = null;
                if (person.CommandState == 1)
                    commandText = "/return";
                else if (person.CommandState == 2)
                    commandText = "/cportfolio";
                else if (person.CommandState == 3)
                    commandText = "/sportfolioset";
                else if (person.CommandState == 4)
                    commandText = "/cportfolioset";
                else if (person.CommandState == 5)
                    commandText = "/sportfolio";

                var command = _serviceProvider.GetServices<IBotCommand>().SingleOrDefault(x => $"/{x.Command}".Equals(commandText, StringComparison.InvariantCultureIgnoreCase));

                if (command != null && !string.IsNullOrEmpty(commandText))
                {
                    try
                    {
                        _context = await command.Execute(chatService,
                            query.CallbackQuery.Message.Chat.Id,
                            query.CallbackQuery.From.Id,
                            query.CallbackQuery.Message.MessageId,
                            query.CallbackQuery.Data?.Replace(commandText, string.Empty).Trim(),
                            query
                            );
                    }
                    catch (Exception)
                    {
                        await chatService.SendMessage(query.CallbackQuery.Message.Chat.Id, "خطایی رخ داده است. لطفا مجددا تلاش کنید.");
                    }
                }
                else
                {
                    _logger.LogCritical("Invalid callback data was provided: {CallbackData}", query);
                }
            }
        }
    }
}
