using System;
using System.Text.Json.Serialization;

namespace NextBot.Models
{
    public partial class ClassicNextSelect
    {
        public class Responseobject
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            [JsonPropertyName("birthday")]
            public DateTime Birthday { get; set; }
            [JsonPropertyName("birthdayPersian")]
            public string BirthdayPersian { get; set; }
            [JsonPropertyName("stockAndWeights")]
            public ClassicNextFormation.Stockandweight[] StockAndWeights { get; set; }
        }
    }
}
