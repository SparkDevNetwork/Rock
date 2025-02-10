using System.Collections.Generic;
using Newtonsoft.Json;
using Rock.AI.Classes.Embeddings;

namespace Rock.AI.OpenAI.OpenAIApiClient.Classes.Embeddings
{
    internal class OpenAIEmbeddingsResponse
    {
        #region Properties

        /// <summary>
        /// The data from the embedding request.
        /// </summary>
        [JsonProperty("data")]
        public List<OpenAIEmbeddingsResponseData> Data { get; set; }

        public OpenAIEmbeddingsTokenResponseUsage Usage { get; set; }

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
        /// Convert OpenAIEmbeddingsResponse to EmbeddingResponse
        /// </summary>
        /// <returns></returns>
        internal EmbeddingResponse AsEmbeddingsResponse()
        {
            var response = new EmbeddingResponse();

            response.IsSuccessful = IsSuccessful;
            response.ErrorMessage = ErrorMessage;

            if ( response.IsSuccessful )
            {
                foreach ( var data in Data )
                {
                    response.Data.Add( new EmbeddingResponseData
                    {
                        Index = data.Index,
                        Embedding = data.Embedding
                    } );
                }
                response.TokensUsed = this.Usage.TotalTokens;
            }

            return response;
        }

        #endregion
    }
}
