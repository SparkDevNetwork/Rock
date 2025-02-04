using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.AI.Classes.Embeddings
{
    /// <summary>
    /// The data from the embedding response.
    /// </summary>
    public class EmbeddingResponseData
    {
        /// <summary>
        /// The index of the embedding in the response.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The embedding data.
        /// </summary>
        public List<double> Embedding { get; set; }
    }
}
