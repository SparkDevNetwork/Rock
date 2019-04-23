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
using System.Web.UI;

using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// abstract class for controls used to render a Person Profile Badge
    /// </summary>
    public class PersonProfileBadge : Control
    {
        /// <summary>
        /// Gets or sets the name of the badge entity type.
        /// </summary>
        /// <value>
        /// The name of the badge entity type.
        /// </value>
        public PersonBadgeCache PersonBadge { get; set; }

        /// <summary>
        /// Restores view-state information from a previous page request that was saved by the <see cref="M:System.Web.UI.Control.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["PersonBadge"] as string;
            if ( !string.IsNullOrWhiteSpace( json ) )
            {
                PersonBadge = PersonBadgeCache.FromJson( json );
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
            if (PersonBadge != null)
            {
                ViewState["PersonBadge"] = PersonBadge.ToJson();
            }

            return base.SaveViewState();
        }

        /// <summary>
        /// Gets the parent person block.
        /// </summary>
        /// <value>
        /// The parent person block.
        /// </value>
        public PersonBlock ParentPersonBlock
        {
            get
            {
                var parentControl = this.Parent;

                while ( parentControl != null )
                {
                    if ( parentControl is PersonBlock )
                    {
                        return parentControl as PersonBlock;
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
            var badgeComponent = PersonBadge?.BadgeComponent;
            if ( badgeComponent != null )
            {
                var personBlock = ParentPersonBlock;
                if ( personBlock != null )
                {
                    badgeComponent.ParentPersonBlock = personBlock;
                    badgeComponent.Person = personBlock.Person;
                    badgeComponent.Render( PersonBadge, writer );
                }
            }

            const string script = "$('.badge[data-toggle=\"tooltip\"]').tooltip({html: true}); $('.badge[data-toggle=\"popover\"]').popover();";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "badge-popover", script, true );
        }
    }
}