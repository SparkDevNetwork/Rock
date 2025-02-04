using Newtonsoft.Json;

namespace Rock.AI.OpenAI.OpenAIApiClient.Classes.Embeddings
{
    internal class OpenAIEmbeddingsTokenResponseUsage
    {
        /// <summary>
        /// The number of tokens used in the prompt.
        /// </summary>
        [JsonProperty( "prompt_tokens" )]
        public int PromptTokens { get; set; }

        /// <summary>
        /// The total number of tokens used in the request.
        /// </summary>
        [JsonProperty( "total_tokens" )]
        public int TotalTokens { get; set; }
    }
}
