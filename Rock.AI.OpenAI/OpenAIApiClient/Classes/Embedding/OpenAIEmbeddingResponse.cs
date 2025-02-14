using System.Collections.Generic;
using Newtonsoft.Json;
using Rock.AI.Classes.Embeddings;

namespace Rock.AI.OpenAI.OpenAIApiClient.Classes.Embedding
{
    internal class OpenAIEmbeddingResponse
    {
        #region Properties

        /// <summary>
        /// The data from the embedding request.
        /// </summary>
        [JsonProperty("data")]
        public List<OpenAIEmbeddingResponseData> Data { get; set; }

        /// <summary>
        /// The usage of tokens in the request.
        /// </summary>
        [JsonProperty( "usage" )]
        public OpenAIEmbeddingTokenResponseUsage Usage { get; set; }

        /// <summary>
        /// Determines if the request was successful.
        /// </summary>
        public bool IsSuccessful { get; set; } = true;

        /// <summary>
        /// Error messages from the request.
        /// </summary>
        public string ErrorMessage { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Convert OpenAIEmbeddingResponse to EmbeddingResponse
        /// </summary>
        /// <returns></returns>
        internal EmbeddingResponse AsEmbeddingResponse()
        {
            var response = new EmbeddingResponse
            {
                IsSuccessful = IsSuccessful,
                ErrorMessage = ErrorMessage 
            };

            if ( IsSuccessful )
            {
                response.Embedding = Data[0].Embedding;
                response.TokensUsed = this.Usage.TotalTokens;
            }

            return response;
        }

        #endregion
    }
}
