using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.AI.Classes.Embeddings
{
    /// <summary>
    /// The response from an embedding request.
    /// </summary>
    public class EmbeddingResponse
    {
        /// <summary>
        /// The data from the embedding request.
        /// </summary>
        public List<EmbeddingResponseData> Data { get; set; } = new List<EmbeddingResponseData>();

        /// <summary>
        /// Determines if the request was successful.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Error messages from the request.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// The number of tokens used in the request.
        /// </summary>
        public int TokensUsed { get; set; }
    }
}
