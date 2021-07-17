using System;
using System.Text.Json.Serialization;

namespace NextBot.Models
{
    partial class PortfolioSet
    {
        public class Responseobject
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            [JsonPropertyName("birthday")]
            public DateTime? Birthday { get; set; }
            [JsonPropertyName("birthdayPersian")]
            public string? BirthdayPersian { get; set; }
            [JsonPropertyName("classicNextPortfolioSetElements")]
            public Classicnextportfoliosetelement[]? ClassicNextPortfolioSetElements { get; set; }
        }
    }
}
