using Newtonsoft.Json;
using Rock.AI.Classes.Embeddings;

namespace Rock.AI.OpenAI.OpenAIApiClient.Classes.Embeddings
{
    /// <summary>
    /// The Request object for an embedding.
    /// </summary>
    internal class OpenAIEmbeddingsRequest
    {
        #region Properties

        /// <summary>
        /// The input to embed. This can be a string or an array of strings.
        /// </summary>
        [JsonProperty( "input" )]
        public string Input { get; set; }

        /// <summary>
        /// The model to use for the embedding. See the documentation for your provider for valid values.
        /// </summary>
        [JsonProperty( "model" )]
        public string Model { get; set; }

        /// <summary>
        /// The encoding format to use for the input. See documentation for your provider for valid values.
        /// </summary>
        [JsonProperty( "encoding_format", NullValueHandling = NullValueHandling.Ignore )]
        public string EncodingFormat { get; set; }

        /// <summary>
        /// The number of dimensions in the embedding.
        /// Note: Only work with text-embedding-3 and later models.
        /// </summary>
        [JsonProperty( "dimensions", NullValueHandling = NullValueHandling.Ignore )]
        public int? dimensions { get; set; }

        #endregion

        /// <summary>
        /// Converts the generic embedding request to an OpenAI embedding request.
        /// </summary>
        /// <param name="request"></param>
        public OpenAIEmbeddingsRequest( EmbeddingRequest request )
        {
            this.Input = request.Input;
            this.Model = request.Model;
            this.EncodingFormat = request.EncodingFormat;
            this.dimensions = request.dimensions;
        }
    }
}
