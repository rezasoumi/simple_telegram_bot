using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NextBot.Models
{
    public partial class ClassicNextFormation
    {
        public class RootObject
        {
            [JsonPropertyName("responseObject")]
            public ResponseObject ResponseObject { get; set; }
            [JsonPropertyName("isSuccessful")]
            public bool IsSuccessful { get; set; }
            [JsonPropertyName("errorMessageFa")]
            public string ErrorMessageFa { get; set; }
            [JsonPropertyName("errorMessage")]
            public string ErrorMessage { get; set; }
        }
    }
}
