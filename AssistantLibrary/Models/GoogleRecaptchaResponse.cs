using System;
using Newtonsoft.Json;

namespace AssistantLibrary.Models {

    public sealed class GoogleRecaptchaResponse {
        
        [JsonProperty("success")]
        public bool Result { get; set; }

        [JsonProperty("challenge_ts")]
        public DateTime? VerifiedOn { get; set; }

        [JsonProperty("hostname")]
        public string HostName { get; set; }

        [JsonProperty("error-codes")]
        public string[] Errors { get; set; }
    }
}