using Rock.Net;
using Rock.Web.Cache;

namespace Rock.Blocks
{
    /// <summary>
    /// Defines the basic elements that are required for all rock blocks to implement.
    /// </summary>
    public interface IRockBlockType
    {
        /// <summary>
        /// Gets the block identifier.
        /// </summary>
        /// <value>
        /// The block identifier.
        /// </value>
        int BlockId { get; }

        /// <summary>
        /// Gets or sets the block cache.
        /// </summary>
        /// <value>
        /// The block cache.
        /// </value>
        BlockCache BlockCache { get; set; }

        /// <summary>
        /// Gets or sets the page cache.
        /// </summary>
        /// <value>
        /// The page cache.
        /// </value>
        PageCache PageCache { get; set; }

        /// <summary>
        /// Gets or sets the request context.
        /// </summary>
        /// <value>
        /// The request context.
        /// </value>
        RockRequestContext RequestContext { get; set; }
    }
}
