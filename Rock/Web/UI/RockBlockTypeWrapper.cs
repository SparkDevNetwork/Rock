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
using System.IO;
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
        /// <summary>
        /// The cached output from RenderControl.
        /// </summary>
        private string _cachedRenderContent;

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

        /// <inheritdoc/>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( Block != null )
            {
                Block.BlockCache = BlockCache;
                Block.PageCache = PageCache;
            }
        }

        /// <inheritdoc/>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( Block is IRockWebBlockType webBlock )
            {
                var pageTask = new PageAsyncTask( async () =>
                {
                    using ( var sw = new StringWriter() )
                    {
                        sw.Write( await webBlock.GetControlMarkupAsync() );

                        _cachedRenderContent = sw.ToString();
                    }
                } );

                Page.RegisterAsyncTask( pageTask );
            }
        
        }

        /// <inheritdoc/>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( _cachedRenderContent != null )
            {
                writer.Write( _cachedRenderContent );
            }
        }

        #endregion
    }
}
