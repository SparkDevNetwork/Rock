using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rock.AI.OpenAI.OpenAIApiClient.Classes.Embedding
{
    internal class OpenAIEmbeddingResponseData
    {
        /// <summary>
        /// The index of the embedding in the response.
        /// </summary>
        [JsonProperty( "index" )]
        public int Index { get; set; }

        /// <summary>
        /// The embedding data.
        /// </summary>
        [JsonProperty( "embedding" )]
        public List<double> Embedding { get; set; }
    }
}
