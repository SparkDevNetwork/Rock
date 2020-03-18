using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace com.bemaservices.MinistrySafe.MinistrySafeApi
{
    /// <summary>
    /// JSON return structure for the Get Report API Call's Response
    /// </summary>
    internal class GetTrainingResponse
    {
        [JsonProperty( "winner" )]
        public bool IsWinner { get; set; }

        [JsonProperty( "score" )]
        public int? Score { get; set; }

        [JsonProperty( "created_at" )]
        public DateTime CreatedDateTime { get; set; }
        
        [JsonProperty( "complete_date" )]
        public DateTime CompleteDateTime { get; set; }

        [JsonProperty( "survey_name" )]
        public string SurveyName { get; set; }
        
        [JsonProperty( "survey_code" )]
        public string SurveyCode { get; set; }
        
        [JsonProperty( "certificate_url" )]
        public string CertificateUrl { get; set; }
    }
}