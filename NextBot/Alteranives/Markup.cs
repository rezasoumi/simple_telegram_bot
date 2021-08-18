using NextBot.Alteranives;
using Telegram.Bot.Types.ReplyMarkups;

namespace NextBot.Handlers
{
    public class Markup : StaticFunctions
    {
        public static readonly ReplyKeyboardMarkup MainMenuRKM = new(
           new KeyboardButton[][]
           {
                    new KeyboardButton[] { "صنعت", "سهام", "پرتفوی مرکب", "پرتفوی"}
           },
           resizeKeyboard: true
       );

        public static readonly ReplyKeyboardMarkup EmptyRKM = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] {""},
            },
            resizeKeyboard: true
        );

        public static readonly ReplyKeyboardMarkup SelectOrCreateRKM = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "🔙" },
                    new KeyboardButton[] { "تشکیل💰", "انتخاب🔎" },
            },
            resizeKeyboard: true
        );

        public static readonly ReplyKeyboardMarkup SmartOrHandMadeRKM = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "🔙" },
                    new KeyboardButton[] { "هوشمند", "دستی" },
            },
            resizeKeyboard: true
        );

        public static readonly ReplyKeyboardMarkup ReturnOrComparisonRKM = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "🔙" },
                    new KeyboardButton[] { "محاسبه بازدهی📈", "مقایسه📊", "حذف پرتفوی❌" },
            },
            resizeKeyboard: true
        );
        /*
        public static readonly ReplyKeyboardMarkup SelectTypesRKM = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "🔙" },
                    new KeyboardButton[] { "انتخاب بر اساس وارد کردن آی دی پرتفوی مورد نظر", "انتخاب بر اساس گذر میان پرتفوی ها" },
            },
            resizeKeyboard: true
        );
        */
        public static readonly ReplyKeyboardMarkup ReturnPortfolioTypesRKM = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "🔙" },
                    new KeyboardButton[] { "بازدهی پرتفوی تا امروز", "بازدهی پرتفوی تا تاریخ دلخواه📆" },
            },
            resizeKeyboard: true
        );
        
        public static readonly ReplyKeyboardMarkup ReturnPortfolioSetTypesRKM = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "🔙" },
                    new KeyboardButton[] { "بازدهی پرتفوی مرکب تا امروز", "بازدهی پرتفوی مرکب تا تاریخ دلخواه📆" },
            },
            resizeKeyboard: true
        );

        public static readonly ReplyKeyboardMarkup ComparisonTypesRKM = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "🔙" },
                    new KeyboardButton[] { "مقایسه با شاخص تا تاریخ دلخواه📆", "مقایسه با شاخص تا امروز" },
                    new KeyboardButton[] { "مقایسه با صندوق سهامی تا تاریخ دلخواه📆", "مقایسه با صندوق سهامی تا امروز" },
                    new KeyboardButton[] { "مقایسه با پرتفوی تا تاریخ دلخواه📆", "مقایسه با پرتفوی تا امروز" },
            },
            resizeKeyboard: true
        );
        
        public static readonly ReplyKeyboardMarkup ComparisonSetTypesRKM = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "🔙" },
                    new KeyboardButton[] { "مقایسه با شاخص تا تاریخ دلخواه📆", "مقایسه با شاخص تا امروز" },
                    new KeyboardButton[] { "مقایسه با صندوق سهامی تا تاریخ دلخواه📆", "مقایسه با صندوق سهامی تا امروز" },
                    new KeyboardButton[] { "مقایسه با پرتفوی مرکب تا تاریخ دلخواه📆", "مقایسه با پرتفوی مرکب تا امروز" },
            },
            resizeKeyboard: true
        );

        public static readonly ReplyKeyboardMarkup ReturnIndexTypesRKM = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "🔙" },
                    new KeyboardButton[] { "بازدهی شاخص تا امروز", "بازدهی شاخص تا تاریخ دلخواه📆" },
            },
            resizeKeyboard: true
        );  

        public static readonly ReplyKeyboardMarkup CreateTypesRKM = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "🔙" },
                    new KeyboardButton[] { "ساخت با پارامتر های پیش فرض" },
                    new KeyboardButton[] { "ساخت با ریسک مشخص" },
                    new KeyboardButton[] { "ساخت با ریسک و حداقل وزن مشخص" },
                    new KeyboardButton[] { "ساخت با ریسک و حداقل و حداکثر وزن مشخص" },
                    new KeyboardButton[] { "ساخت با ریسک و حداقل و حداکثر و تاریخ شمسی مشخص" },
            },
            resizeKeyboard: true
        );

        public static readonly ReplyKeyboardMarkup StockReturnRKM = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "🔙" },
                    new KeyboardButton[] { "محاسبه بازدهی تا امروز📈" },
                    new KeyboardButton[] { "محاسبه بازدهی تا تاریخ دلخواه📈" },
            },
            resizeKeyboard: true
        );
        
        public static readonly ReplyKeyboardMarkup PortfolioSetSelectRKM = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "🔙" },
                    new KeyboardButton[] { "مقایسه📊", "محاسبه بازدهی📈", "حذف پرتفوی➖", "افزودن پرتفوی➕" },
                    new KeyboardButton[] { "حذف پرتفوی مرکب❌" },
            },
            resizeKeyboard: true
        );
    }
}
