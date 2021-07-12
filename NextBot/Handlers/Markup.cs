using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace NextBot.Handlers
{
    public class Markup
    {
        public static readonly ReplyKeyboardMarkup ReplyKeyboardMarkup0 = new(
           new KeyboardButton[][]
           {
                    new KeyboardButton[] { "سهام", "صنعت", "پرتفوی مرکب", "پرتفوی"}
           },
           resizeKeyboard: true
       );

        public static readonly ReplyKeyboardMarkup ReplyKeyboardMarkup1 = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] {""},
            },
            resizeKeyboard: true
        );

        public static readonly ReplyKeyboardMarkup ReplyKeyboardMarkup2 = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "بازگشت" },
                    new KeyboardButton[] { "تشکیل", "انتخاب" },
            },
            resizeKeyboard: true
        );

        public static readonly ReplyKeyboardMarkup ReplyKeyboardMarkup3 = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "بازگشت" },
                    new KeyboardButton[] { "هوشمند", "دستی" },
            },
            resizeKeyboard: true
        );

        public static readonly ReplyKeyboardMarkup ReplyKeyboardMarkup5 = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "ریسک متوسط", "بدون ریسک" },
                    new KeyboardButton[] { "ریسک زیاد", "ریسک خیلی کم" },
                    new KeyboardButton[] { "ریسک خیلی زیاد", "ریسک کم" },
                    new KeyboardButton[] { "بازگشت" },
            },
            resizeKeyboard: true
        );

        public static readonly ReplyKeyboardMarkup ReplyKeyboardMarkup8 = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "بازگشت" },
                    new KeyboardButton[] { "محاسبه بازدهی", "مقایسه" },
            },
            resizeKeyboard: true
        );

        public static readonly ReplyKeyboardMarkup ReplyKeyboardMarkup9 = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "بازگشت" },
                    new KeyboardButton[] { "انتخاب بر اساس وارد کردن آی دی پرتفوی مورد نظر", "انتخاب بر اساس گذر میان پرتفوی ها" },
            },
            resizeKeyboard: true
        );

        public static readonly ReplyKeyboardMarkup ReplyKeyboardMarkup10 = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "بازگشت" },
                    new KeyboardButton[] { "بازدهی پرتفوی تا امروز", "بازدهی پرتفوی تا تاریخ دلخواه" },
            },
            resizeKeyboard: true
        );

        public static readonly ReplyKeyboardMarkup ReplyKeyboardMarkup11 = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "بازگشت" },
                    new KeyboardButton[] { "شاخص", "صندوق سهامی", "پرتفوی" },
            },
            resizeKeyboard: true
        );

        public static readonly ReplyKeyboardMarkup ReplyKeyboardMarkup12 = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "بازگشت" },
                    new KeyboardButton[] { "بازدهی شاخص تا امروز", "بازدهی شاخص تا تاریخ دلخواه" },
            },
            resizeKeyboard: true
        );  

        public static readonly ReplyKeyboardMarkup ReplyKeyboardMarkup13 = new(
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

        public static readonly ReplyKeyboardMarkup ReplyKeyboardMarkup14 = new(
            new KeyboardButton[][]
            {
                    new KeyboardButton[] { "بازگشت" },
                    new KeyboardButton[] { "محاسبه بازدهی" },
            },
            resizeKeyboard: true
        );
    }
}
