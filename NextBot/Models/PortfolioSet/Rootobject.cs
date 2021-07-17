using System.Text.Json.Serialization;

namespace NextBot.Models
{
    partial class PortfolioSet
    {
        public class Rootobject
        {
            [JsonPropertyName("responseObject")]
            public Responseobject ResponseObject { get; set; }
            [JsonPropertyName("isSuccessful")]
            public bool IsSuccessful { get; set; }
            [JsonPropertyName("errorMessageFa")]
            public string ErrorMessageFa { get; set; }
            [JsonPropertyName("errorMessage")]
            public string ErrorMessage { get; set; }
        }
    }
}
