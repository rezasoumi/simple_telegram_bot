using Microsoft.Extensions.DependencyInjection;
using NextBot.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace NextBot.Commands
{
    public class HelpCommand : IBotCommand
    {
        private readonly IServiceProvider _serviceProvider;
        private MyDbContext _context;

        public string Command => "help";

        public string Description => "آموزش بات";

        public bool InternalCommand => false;

        public HelpCommand(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var scope = serviceProvider.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        }

        public async Task<MyDbContext> Execute(IChatService chatService, long chatId, long userId, int messageId, string? commandText, CallbackQuery? query)
        {
            var person = _context.People.FirstOrDefault(p => p.ChatId == chatId);
            await chatService.SendMessage(chatId, message: "معرفی صفحه اصلی نکست بات:" + "\n\n" + 
                "پرتفوی:" + "\n" +
                "انتخاب یا تشکیل پرتفوی هوشمند از طریق این گزینه صورت می گیرد. در قسمت تشکیل امکان ساخت پرتفوی هوشمند وجود دارد. در قسمت انتخاب می توان پرتفوی مورد نظر را انتخاب و روی آن تحلیل هایی انجام داد. " + "\n\n" + 
                "پرتفوی مرکب:" + "\n" + "در این قسمت با انتخاب تشکیل، یک پرتفوی مرکب ساخته می شود و در بخش انتخاب نیز می توان یک پرتفوی مرکب انتخاب کرد تحلیل مورد نظر را روی آن پرتفوی مرکب انجام داد." + "\n\n" + 
                "سهام:" + "\n" + "بازدهی سهام مورد نظر در هر بازه زمانی مورد نظر از طریق این بخش صورت می گیرد. فقط کافی است نماد سهم مورد نظر را وارد کنید و آن را انتخاب کنید و تاریخ های مورد نظر را از تقویم انتخاب کنید." + "\n\n" + 
                "صنعت:" + "\n" + "این قسمت هنوز پیاده سازی نشده است." + "\n\n" + 
                "همچنین از command های بات نیز می توان استفاده کرد و هر جای برنامه می توان از این دستورات که با / شروع می شود استفاده کرد و به بخش مورد نظر رفت.");
            return _context;
        }
    }
}
