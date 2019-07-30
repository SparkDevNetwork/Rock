using System;

using Rock.Blocks;

namespace Rock.Web.UI
{
    /// <summary>
    /// This is a placeholder wrapper for pre-compiled rock blocks. It allows them to exist
    /// in the UI space so that standard administration tools work on them.
    /// </summary>
    /// <seealso cref="Rock.Web.UI.RockBlock" />
    public class RockBlockTypeWrapper : RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets the block.
        /// </summary>
        /// <value>
        /// The block.
        /// </value>
        public IRockBlockType Block { get; set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( Block != null )
            {
                Block.BlockCache = BlockCache;
                Block.PageCache = PageCache;
            }
        }

        #endregion
    }
}
