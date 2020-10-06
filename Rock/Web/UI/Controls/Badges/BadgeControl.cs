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
using System.Web;
using System.Web.UI;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// abstract class for controls used to render a Person Profile Badge
    /// </summary>
    public class BadgeControl : Control
    {
        /// <summary>
        /// Gets or sets the badge cache.
        /// </summary>
        public BadgeCache BadgeCache { get; set; }

        /// <summary>
        /// Restores view-state information from a previous page request that was saved by the <see cref="M:System.Web.UI.Control.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var badgeId = ViewState["BadgeId"] as int?;
            if ( badgeId.HasValue )
            {
                BadgeCache = BadgeCache.Get( badgeId.Value );
            }
        }

        /// <summary>
        /// Saves any server control view-state changes that have occurred since the time the page was posted back to the server.
        /// </summary>
        /// <returns>
        /// Returns the server control's current view state. If there is no view state associated with the control, this method returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            if ( BadgeCache != null )
            {
                ViewState["BadgeId"] = BadgeCache.Id.ToString();
            }

            return base.SaveViewState();
        }

        /// <summary>
        /// Gets the parent person block.
        /// </summary>
        /// <value>
        /// The parent person block.
        /// </value>
        public ContextEntityBlock ContextEntityBlock
        {
            get
            {
                var parentControl = this.Parent;

                while ( parentControl != null )
                {
                    if ( parentControl is ContextEntityBlock )
                    {
                        return parentControl as ContextEntityBlock;
                    }

                    parentControl = parentControl.Parent;
                }

                return null;
            }

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object, which writes the content to be rendered on the client.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the server control content.</param>
        protected override void Render( HtmlTextWriter writer )
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var personBadgeCache = new PersonBadgeCache( BadgeCache );
#pragma warning restore CS0618 // Type or member is obsolete
            var badgeComponent = BadgeCache?.BadgeComponent;
            if ( badgeComponent != null )
            {
                var contextEntityBlock = ContextEntityBlock;
                if ( contextEntityBlock != null )
                {
                    if ( BadgeService.DoesBadgeApplyToEntity( BadgeCache, contextEntityBlock.Entity ) )
                    {
                        try
                        {
                            badgeComponent.ParentContextEntityBlock = contextEntityBlock;
                            badgeComponent.Entity = contextEntityBlock.Entity;
                            badgeComponent.Render( BadgeCache, writer );

#pragma warning disable CS0618 // Type or member is obsolete
                            badgeComponent.Render( personBadgeCache, writer );
#pragma warning restore CS0618 // Type or member is obsolete

                            var script = badgeComponent.GetWrappedJavaScript( BadgeCache );

                            if ( !script.IsNullOrWhiteSpace() )
                            {
                                ScriptManager.RegisterStartupScript( this, GetType(), $"badge_{ClientID}", script, true );
                            }
                        }
                        catch ( Exception ex )
                        {
                            var errorMessage = $"An error occurred rendering badge: {BadgeCache?.Name }, badge-id: {BadgeCache?.Id}";
                            ExceptionLogService.LogException( new Exception( errorMessage, ex ) );
                            var badgeNameClass = BadgeCache?.Name.ToLower().RemoveAllNonAlphaNumericCharacters() ?? "error";
                            writer.Write( $"<div class='badge badge-{badgeNameClass} badge-id-{BadgeCache?.Id} badge-error' data-toggle='tooltip' data-original-title='{errorMessage}'>" );
                            writer.Write( $"  <i class='fa fa-exclamation-triangle badge-icon text-warning'></i>" );
                            writer.Write( "</div>" );
                        }
                        finally
                        {
                            const string script = "$('.badge[data-toggle=\"tooltip\"]').tooltip({html: true}); $('.badge[data-toggle=\"popover\"]').popover();";
                            ScriptManager.RegisterStartupScript( this, this.GetType(), "badge-popover", script, true );
                        }
                    }
                }
            }
        }
    }
}