using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace NextBot.Handlers
{
    public class Markup
    {
        public static readonly ReplyKeyboardMarkup MainMenuRKM = new(
           new KeyboardButton[][]
           {
                    new KeyboardButton[] { "سهام", "صنعت", "پرتفوی مرکب", "پرتفوی"}
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
                    new KeyboardButton[] { "بازگشت" },
                    new KeyboardButton[] { "تشکیل", "انتخاب" },
            },
            resizeKeyboard: true
        );

        public static readonly ReplyKeyboardMarkup SmartOrHandMadeRKM = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "بازگشت" },
                    new KeyboardButton[] { "هوشمند", "دستی" },
            },
            resizeKeyboard: true
        );

        public static readonly ReplyKeyboardMarkup ReturnOrComparisonRKM = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "بازگشت" },
                    new KeyboardButton[] { "محاسبه بازدهی", "مقایسه" },
            },
            resizeKeyboard: true
        );

        public static readonly ReplyKeyboardMarkup SelectTypesRKM = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "بازگشت" },
                    new KeyboardButton[] { "انتخاب بر اساس وارد کردن آی دی پرتفوی مورد نظر", "انتخاب بر اساس گذر میان پرتفوی ها" },
            },
            resizeKeyboard: true
        );

        public static readonly ReplyKeyboardMarkup ReturnPortfolioTypesRKM = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "بازگشت" },
                    new KeyboardButton[] { "بازدهی پرتفوی تا امروز", "بازدهی پرتفوی تا تاریخ دلخواه" },
            },
            resizeKeyboard: true
        );

        public static readonly ReplyKeyboardMarkup ComparisonTypesRKM = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "بازگشت" },
                    new KeyboardButton[] { "شاخص", "صندوق سهامی", "پرتفوی" },
            },
            resizeKeyboard: true
        );

        public static readonly ReplyKeyboardMarkup ReturnIndexTypesRKM = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "بازگشت" },
                    new KeyboardButton[] { "بازدهی شاخص تا امروز", "بازدهی شاخص تا تاریخ دلخواه" },
            },
            resizeKeyboard: true
        );  

        public static readonly ReplyKeyboardMarkup CreateTypesRKM = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "بازگشت" },
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
                    new KeyboardButton[] { "بازگشت" },
                    new KeyboardButton[] { "محاسبه بازدهی" },
            },
            resizeKeyboard: true
        );
        
        public static readonly ReplyKeyboardMarkup PortfolioSetSelectRKM = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "بازگشت" },
                    new KeyboardButton[] { "مقایسه", "محاسبه بازدهی", "حذف پرتفوی", "افزودن پرتفوی" },
            },
            resizeKeyboard: true
        );
    }
}
