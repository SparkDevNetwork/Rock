// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Web.UI;
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

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            base.RenderControl( writer );

            if ( Block is IRockWebBlockType webBlock )
            {
                writer.Write( webBlock.GetControlMarkup() );
            }
        }

        #endregion
    }
}
