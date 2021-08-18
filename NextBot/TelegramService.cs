using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NextBot.Commands;
using NextBot.Handlers;
using NextBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.IO;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Exceptions;
using NextBot.Alteranives;

namespace NextBot
{
    public class TelegramService : StaticFunctions, IChatService, IDisposable, IUpdateHandler
    {
        private readonly TelegramBotClient _botClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TelegramService> _logger;
        private bool disposedValue;
        public MyDbContext _context;
        public event EventHandler<ChatMessageEventArgs>? ChatMessage;
        //public event EventHandler<CallbackQueryEventArgs>? Callback;
        public event EventHandler<CallbackQuery>? Callback;
        public UpdateType[] AllowedUpdates => null;


        public async Task<string> BotUserName() => $"@{(await _botClient.GetMeAsync()).Username}";

        public TelegramService(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<TelegramService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            var scope = serviceProvider.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<MyDbContext>();

            _botClient = new TelegramBotClient(configuration["Telegram:ApiKey"]);
            RegisterCommands();
            
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            
            _botClient.StartReceiving(this, cancellationToken);
        }

        /// This method registers all the commands with the bot on telegram
        /// so that the user gets some sort of intelisense
        private void RegisterCommands()
        {
            var commands = _serviceProvider
                .GetServices<IBotCommand>()
                .Where(x => !x.InternalCommand)
                .Select(x => new BotCommand
                {
                    Command = x.Command,
                    Description = x.Description
                });
            _botClient.SetMyCommandsAsync(commands).GetAwaiter().GetResult();
        }
        
        public async void OnMessage_(Update update)
        {
            var scope = _serviceProvider.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            _logger.LogTrace("Message received from '{Username}': '{Message}'", update.Message.From.Username ?? update.Message.From.FirstName, update.Message.Text);

            if (update.Message.Text == null /*|| !e.Message.Entities.Any(x => x.Type == MessageEntityType.BotCommand)*/)
            {
                _logger.LogTrace("No command was specified");
                return;
            }
            await CheckRegistering_(update);
        }
        
        private async Task CheckRegistering_(Update update)
        {
            try
            {
                var channel = new ChatId(-1001391973136);
                var chatMember1 = await _botClient.GetChatMemberAsync(channel, Convert.ToInt32(update.Message.Chat.Id));

                // Creator Or Member Or Left
                if (chatMember1.Status.ToString() != "Left")
                {
                    RegisterWithChatId_(update);
                    await OnMessageAfterRegistering_(update);
                }
                else
                {
                    // Joining to our channel message
                    await _botClient.SendTextMessageAsync(
                            chatId: update.Message.Chat,
                            text: $"سلام {update.Message.Chat.FirstName} عزیز" +
                            $"\nبرای استفاده از ربات ابتدا باید در کانال نکست عضو بشید." +
                            $"\nبعد از عضویت، مجدد پیام ارسال کنید تا ربات برای شما فعال شود."
                        );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        private void RegisterWithChatId_(Update update)
        {
            var person1 = _context.People.Where(p => p.ChatId == update.Message.Chat.Id);

            Person newPerson;
            if (!person1.Any())
            {
                newPerson = new Person()
                {
                    ChatId = update.Message.Chat.Id
                };
                _context.People.Add(newPerson);
                _context.SaveChanges();
            }
        }
        
        private async Task OnMessageAfterRegistering_(Update update)
        {
            _logger.Log(LogLevel.Information, $"{update.Message.Chat.Username} --> {update.Message.Text} at {update.Message.Date}", DateTime.UtcNow);
            try
            {
                using StreamWriter file = new("log.txt", append: true);
                await file.WriteLineAsync($"{update.Message.Chat.Username} --> {update.Message.Text} at {update.Message.Date}");
            }
            catch (Exception)
            {
                _logger.Log(LogLevel.Error, $"log cannot append to log.txt", DateTime.UtcNow);
            }

            if (update.Message.Entities?.Count(x => x.Type == MessageEntityType.BotCommand) > 1)
            {
                _logger.LogTrace("More than one command was specified");
                await _botClient.SendTextMessageAsync(update.Message.Chat.Id, "Please only send one command at a time.");
                return;
            }

            var person = _context.People.FirstOrDefault(p => p.ChatId == update.Message.Chat.Id);

            if (update.Message.Entities?.SingleOrDefault().Type == MessageEntityType.BotCommand)
            {
                var botCommand = update.Message.Entities.Single(x => x.Type == MessageEntityType.BotCommand);
                var command = update.Message.Text.Substring(botCommand.Offset, botCommand.Length);
                command = command.Replace(await BotUserName(), string.Empty);
                person.CommandLevel = 0;
                _context.SaveChanges();
                ChatMessage?.Invoke(this, new ChatMessageEventArgs
                {
                    Text = update.Message.Text.Replace(command, string.Empty).Trim(),
                    UserId = update.Message.From.Id,
                    ChatId = update.Message.Chat.Id,
                    MessageId = update.Message.MessageId,
                    Command = command
                });
            }
            else if (person.CommandState == 0)
            {
                if (person.CommandLevel == 0)
                {
                    switch (update.Message.Text)
                    {
                        case "سهام":
                            person.CommandState = 1;
                            person.CommandLevel = 1;
                            await _botClient.SendTextMessageAsync(person.ChatId, "نام سهم مورد نظر (یا بخشی از آن) را وارد کنید :");
                            break;
                        case "صنعت":
                            await _botClient.SendTextMessageAsync(chatId: person.ChatId, text: "هنوز پیاده سازی نشده است. از گزینه های موجود یک گزینه را انتخاب کنید :", replyMarkup: Markup.MainMenuRKM);
                            break;
                        case "پرتفوی مرکب":
                            person.CommandLevel = 1;
                            await _botClient.SendTextMessageAsync(chatId: person.ChatId, text: "از گزینه های موجود یک گزینه را انتخاب کنید :", replyMarkup: Markup.SelectOrCreateRKM);
                            break;
                        case "پرتفوی":
                            person.CommandLevel = 2;
                            await _botClient.SendTextMessageAsync(chatId: person.ChatId, text: "از گزینه های موجود یک گزینه را انتخاب کنید :", replyMarkup: Markup.SelectOrCreateRKM);
                            break;
                        default:
                            await _botClient.SendTextMessageAsync(chatId: person.ChatId, text: "از گزینه های موجود یک گزینه را انتخاب کنید :", replyMarkup: Markup.MainMenuRKM);
                            break;
                    }
                }
                else if (person.CommandLevel == 1)
                {
                    switch (update.Message.Text)
                    {
                        case "تشکیل💰":
                            ChatMessage?.Invoke(this, new ChatMessageEventArgs
                            {
                                Text = update.Message.Text.Replace("/cportfolioset", string.Empty).Trim(),
                                UserId = update.Message.From.Id,
                                ChatId = update.Message.Chat.Id,
                                MessageId = update.Message.MessageId,
                                Command = "/cportfolioset"
                            });
                            Thread.Sleep(500);
                            await _botClient.SendTextMessageAsync(chatId: person.ChatId, text: "از بین گزینه های زیر انتخاب کنید :", replyMarkup: Markup.SelectOrCreateRKM);
                            break;
                        case "انتخاب🔎":
                            person.CommandState = 3;
                            person.CommandLevel = 2;
                            person.ClassicNextSelectState = 1;
                            person = await ShowPreviousOrNextListInPortfolioSetSelect(null, person, _botClient); 
                            break;
                        case "🔙":
                            person.CommandLevel = 0;
                            await _botClient.SendTextMessageAsync(chatId: person.ChatId, text: "از گزینه های موجود یک گزینه را انتخاب کنید :", replyMarkup: Markup.MainMenuRKM);
                            break;
                        default:
                            await _botClient.SendTextMessageAsync(chatId: person.ChatId, text: "ورودی اشتباه ! لطفا دوباره تلاش کنید", replyMarkup: Markup.SelectOrCreateRKM);
                            break;
                    }
                }
                else if (person.CommandLevel == 2)
                {
                    switch (update.Message.Text)
                    {
                        case "تشکیل💰":
                            person.CommandState = 2;
                            person.CommandLevel = 1;
                            await _botClient.SendTextMessageAsync(chatId: person.ChatId, text: "از گزینه های موجود یک گزینه را انتخاب کنید :", replyMarkup: Markup.CreateTypesRKM);
                            break;
                        case "انتخاب🔎":
                            person.CommandState = 5;
                            person.CommandLevel = 2;
                            person.ClassicNextSelectState = 1;
                            person = await ShowPreviousOrNextListInClassicNextSelect(null, person, _botClient); 
                            break;
                        case "🔙":
                            person.CommandLevel = 0;
                            await _botClient.SendTextMessageAsync(chatId: person.ChatId, text: "از گزینه های موجود یک گزینه را انتخاب کنید :", replyMarkup: Markup.MainMenuRKM);
                            break;
                        default:
                            await _botClient.SendTextMessageAsync(chatId: person.ChatId, text: "ورودی اشتباه ! لطفا دوباره تلاش کنید", replyMarkup: Markup.SelectOrCreateRKM);
                            break;
                    }
                }
            }
            else if (person.CommandState == 1)
            {
                ChatMessage?.Invoke(this, new ChatMessageEventArgs
                {
                    Text = update.Message.Text.Replace("/return", string.Empty).Trim(),
                    UserId = update.Message.From.Id,
                    ChatId = update.Message.Chat.Id,
                    MessageId = update.Message.MessageId,
                    Command = "/return"
                });
            }
            else if (person.CommandState == 2)
            {
                ChatMessage?.Invoke(this, new ChatMessageEventArgs
                {
                    Text = update.Message.Text.Replace("/cportfolio", string.Empty).Trim(),
                    UserId = update.Message.From.Id,
                    ChatId = update.Message.Chat.Id,
                    MessageId = update.Message.MessageId,
                    Command = "/cportfolio"
                });
            }
            else if (person.CommandState == 3)
            {
                ChatMessage?.Invoke(this, new ChatMessageEventArgs
                {
                    Text = update.Message.Text.Replace("/sportfolioset", string.Empty).Trim(),
                    UserId = update.Message.From.Id,
                    ChatId = update.Message.Chat.Id,
                    MessageId = update.Message.MessageId,
                    Command = "/sportfolioset"
                });
            }
            else if (person.CommandState == 4)
            {
                ChatMessage?.Invoke(this, new ChatMessageEventArgs
                {
                    Text = update.Message.Text.Replace("/cportfolioset", string.Empty).Trim(),
                    UserId = update.Message.From.Id,
                    ChatId = update.Message.Chat.Id,
                    MessageId = update.Message.MessageId,
                    Command = "/cportfolioset"
                });
            }
            else if (person.CommandState == 5)
            {
                ChatMessage?.Invoke(this, new ChatMessageEventArgs
                {
                    Text = update.Message.Text.Replace("/sportfolio", string.Empty).Trim(),
                    UserId = update.Message.From.Id,
                    ChatId = update.Message.Chat.Id,
                    MessageId = update.Message.MessageId,
                    Command = "/sportfolio"
                });
            }
            _context.SaveChanges();
        }
        
        private void OnCallbackQuery_(Update update)
        {
            _logger.LogTrace("Callback received from '{Username}': '{Message}'", update.CallbackQuery.From.Username ?? update.CallbackQuery.From.FirstName, update.CallbackQuery.Data);
            
            Callback?.Invoke(this, update.CallbackQuery);
            // This removes the keyboard, but we could also update one here...
            //await client.EditMessageReplyMarkupAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, null);

            //Callback?.Invoke(this, e);
        }
        
        public async Task<bool> UpdateMessage(long chatId, int messageId, string? newText, InlineKeyboardMarkup inlineKeyboard = null)
        {
            try
            {
                newText = EscapeText(newText);

                _logger.LogTrace("Updating message {MessageId}: {NewText}", messageId, newText);

                await _botClient.EditMessageTextAsync(new ChatId(chatId), messageId, newText, parseMode: ParseMode.MarkdownV2, replyMarkup: inlineKeyboard);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating message");
                return false;
            }
        }

        private InlineKeyboardMarkup? GetInlineKeyboard(Dictionary<string, string>? buttons)
        {
            InlineKeyboardMarkup? inlineKeyboard = null;
            if (buttons != null)
            {
                var inlineKeyboardButtons = new List<InlineKeyboardButton>();
                foreach (var button in buttons)
                {
                    inlineKeyboardButtons.Add(InlineKeyboardButton.WithCallbackData(button.Key, button.Value));
                }
                inlineKeyboard = new InlineKeyboardMarkup(inlineKeyboardButtons);
            }
            return inlineKeyboard;
        }

        public async Task<bool> SendMessage(long chatId, string? message, IReplyMarkup? rkm = null, Dictionary<string, string>? buttons = null)
        {
            try
            {
                message = EscapeText(message);

                _logger.LogTrace("Sending message to {ChatId}: {Message}", chatId, message);

                if (rkm == null) 
                    await _botClient.SendTextMessageAsync(new ChatId(chatId), message, parseMode: ParseMode.MarkdownV2, replyMarkup: GetInlineKeyboard(buttons));
                else
                    await _botClient.SendTextMessageAsync(new ChatId(chatId), message, parseMode: ParseMode.MarkdownV2, replyMarkup: rkm);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending message");
                return false;
            }
        }

        public async Task<string> GetChatMemberName(long chatId, int userId)
        {
            var chatMember = await _botClient.GetChatMemberAsync(new ChatId(chatId), userId);
            return chatMember.User.Username ?? chatMember.User.FirstName;
        }

        private string EscapeText(string? source)
        {
            var charactersToEscape = new[] { "_", "*", "[", "]", "(", ")", "~", "`", ">", "#", "+", "-", "=", "|", "{", "}", ".", "!" };
            foreach (var item in charactersToEscape)
            {
                source = source.Replace(item, $@"\{item}");
            }
            return source;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_botClient != null)
                    {
                        //_botClient.OnMessage -= OnMessage;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TelegramService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async Task<bool> AnswerCallbackQueryAsync(string callbackQueryId)
        {
            await _botClient.AnswerCallbackQueryAsync(callbackQueryId);
            return true;
        }

        public async Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ApiRequestException apiRequestException)
            {
                _logger.Log(LogLevel.Error, apiRequestException.Message);
            }
        }

        public async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is Message message)
            {
                OnMessage_(update);
            }
            if (update.CallbackQuery is CallbackQuery query)
            {
                OnCallbackQuery_(update);
            }
        }

        public async Task SendPhotoAsync(long chatId, string url)
        {
            await _botClient.SendPhotoAsync(chatId, url);
        }
    }
}
