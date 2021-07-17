using System;
using System.Text.Json.Serialization;

namespace NextBot.Models
{
    partial class PortfolioSet
    {
        public class Classicnextportfoliosetelement
        {
            [JsonPropertyName("elementNumber")]
            public int ElementNumber { get; set; }
            [JsonPropertyName("portfolioId")]
            public int PortfolioId { get; set; }
            [JsonPropertyName("birthday")]
            public DateTime Birthday { get; set; }
            [JsonPropertyName("birthdayPersian")]
            public string BirthdayPersian { get; set; }
        }
    }
}
