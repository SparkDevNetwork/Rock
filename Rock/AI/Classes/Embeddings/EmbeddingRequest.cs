
namespace Rock.AI.Classes.Embeddings
{
    /// <summary>
    /// The class for creating a new request for an embedding.
    /// </summary>
    public class EmbeddingRequest
    {
        /// <summary>
        /// The input to embed. This can be a string or an array of strings.
        /// </summary>
        public string Input { get; set; }

        /// <summary>
        /// The model to use for the embedding. See the documentation for your provider for valid values.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// The encoding format to use for the input. See documentation for your provider for valid values.
        /// </summary>
        public string EncodingFormat { get; set; }
    }
}
