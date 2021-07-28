using Microsoft.Extensions.DependencyInjection;
using NextBot.Handlers;
using NextBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Args;

namespace NextBot.Commands
{
    public class CPortfolioSet : StaticFunctions, IBotCommand
    {
        private readonly IServiceProvider _serviceProvider;
        private MyDbContext _context;

        public string Command => "cportfolioset";

        public string Description => "تشکیل پرتفوی مرکب";

        public bool InternalCommand => false;

        public CPortfolioSet(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var scope = serviceProvider.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        }

        public async Task<MyDbContext> Execute(IChatService chatService, long chatId, int userId, int messageId, string? commandText, CallbackQueryEventArgs? callbackQueryEventArgs)
        {
            var person = _context.People.FirstOrDefault(p => p.ChatId == chatId);
            //person.CommandState = 4;

            var streamTask = client.GetStreamAsync("http://192.168.95.88:30907/api/classicNext/portfolioSet/create");
            var root = await System.Text.Json.JsonSerializer.DeserializeAsync<PortfolioSet.Rootobject>(await streamTask);
            if (root.IsSuccessful)
            {
                //await chatService.SendMessage(chatId: chatId, "در حال ایجاد پرتفوی مرکب ...");
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
                Thread.Sleep(500);
                //await chatService.SendMessage(chatId, message: "از بین گزینه های زیر انتخاب کنید :", Markup.SelectOrCreateRKM);
            }
            else
            {
                await chatService.SendMessage(chatId, message: root.ErrorMessageFa);
                Thread.Sleep(200);
                // must implemented
                //await chatService.SendMessage(chatId, message: "روش انتخاب را از بین دو گزینه موجود وارد کنید :", Markup.SelectOrCreateRKM);
            }
            _context.SaveChanges();
            return _context;
        }
    }
}
