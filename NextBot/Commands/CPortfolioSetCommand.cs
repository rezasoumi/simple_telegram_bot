using Microsoft.Extensions.DependencyInjection;
using NextBot.Alteranives;
using NextBot.Models;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace NextBot.Commands
{
    public class CPortfolioSetCommand : StaticFunctions, IBotCommand
    {
        private readonly IServiceProvider _serviceProvider;
        private MyDbContext _context;

        public string Command => "cportfolioset";

        public string Description => "تشکیل پرتفوی مرکب";

        public bool InternalCommand => false;

        public CPortfolioSetCommand(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var scope = serviceProvider.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        }

        public async Task<MyDbContext> Execute(IChatService chatService, long chatId, long userId, int messageId, string? commandText, CallbackQuery? callbackQueryEventArgs)
        {
            var person = _context.People.FirstOrDefault(p => p.ChatId == chatId);

            var streamTask = client.GetStreamAsync("http://192.168.95.88:30907/api/classicNext/portfolioSet/create");
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<PortfolioSet.Rootobject>(await streamTask);
            if (root.IsSuccessful)
            {
                StringBuilder str = new();
                str.Append($"Id : {root.ResponseObject.Id}\n");
                str.Append("Persian birthday : " + root.ResponseObject.BirthdayPersian + "\n");
                str.Append("Stock and weights : \n");
                for (int i = 0; i < root.ResponseObject.ClassicNextPortfolioSetElements?.Length; i++)
                {
                    var item = root.ResponseObject.ClassicNextPortfolioSetElements.ElementAt(i);
                    str.Append($"Number {item.ElementNumber}\n");
                    str.Append($"Portfolio Id : {item.PortfolioId}\n");
                    str.Append($"Persian birthday : {item.BirthdayPersian}\n");
                }
                await chatService.SendMessage(chatId, message: str.ToString());
                Thread.Sleep(700);
                await chatService.SendMessage(chatId, "پرتفوی مرکب ساخته شد. در قسمت انتخاب پرتفوی مرکب قابل مشاهده است.");
            }
            else
            {
                await chatService.SendMessage(chatId, message: root.ErrorMessageFa);
                Thread.Sleep(200);
            }
            await _context.SaveChangesAsync();
            return _context;
        }
    }
}
