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

            /*
                 10/08/2024 - NA

                 An IRockWebBlockType (Obsidian block) should generally never be involved in
                 an IsPostBack, so it should be able to ignore these events and avoid
                 reloading its content.

                 Reason: The Obsidian block content was being reloaded and then discarded.
                         https://app.asana.com/0/1200625776837488/1206779635354257/f

                 06/11/2026 - MSE

                 The above only applies to partial (async) postbacks, where anything this
                 wrapper renders is outside the UpdatePanel being refreshed and is therefore
                 discarded by the PageRequestManager. During a full postback the rendered
                 output is the complete new page, so skipping here caused every Obsidian
                 block on the page to render empty (e.g. the Page Menu disappearing after
                 the WebForms Transaction List block performed its "Move Transactions To
                 Batch" full postback).

                 Reason: Rock menu was removed after moving a transaction to another batch. (Fixes #6871)
            */
            var isInAsyncPostBack = ScriptManager.GetCurrent( Page )?.IsInAsyncPostBack == true;

            if ( Block is IRockWebBlockType webBlock && !isInAsyncPostBack )
            {
                var pageTask = new PageAsyncTask( async () =>
                {
                    using ( var sw = new StringWriter() )
                    {
                        sw.Write( await webBlock.GetControlMarkupAsync() );

                        _cachedRenderContent = sw.ToString();
                    }

                    /*
                        7/23/26 - MSE

                        A block that returned no markup has chosen to render nothing at
                        all, so mark the control as not visible. This lets the
                        RockBlockWrapper suppress the block's Pre-HTML and Post-HTML the
                        same way it does for a WebForms block that sets Visible = false
                        (e.g. the Defined Type Check List block when it is empty and
                        configured with "Hide Block When Empty").

                        Reason: Hide Pre/Post-HTML when an Obsidian block renders no content.
                    */
                    if ( _cachedRenderContent.IsNullOrWhiteSpace() )
                    {
                        Visible = false;
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
