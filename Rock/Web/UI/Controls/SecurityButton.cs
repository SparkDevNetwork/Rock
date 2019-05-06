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
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A Button for setting security for a particular secured item
    /// </summary>
    [ToolboxData( "<{0}:SecurityButton runat=server></{0}:SecurityButton>" )]
    public class SecurityButton : HtmlAnchor
    {
        /// <summary>
        /// Gets or sets the entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        [
        Bindable( true ),
        Description( "The type of entity to secure." )
        ]
        public int EntityTypeId
        {
            get { return ViewState["EntityTypeId"] as int? ?? 0; }
            set { ViewState["EntityTypeId"] = value; }
        }

        /// <summary>
        /// Gets or sets the entity id.
        /// </summary>
        /// <value>
        /// The entity id.
        /// </value>
        [
        Bindable( true ),
        Description( "The id of the entity to secure." )
        ]
        public int EntityId
        {
            get { return ViewState["EntityId"] as int? ?? 0; }
            set { ViewState["EntityId"] = value; }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            HtmlGenericControl buttonIcon = new HtmlGenericControl( "i" );
            buttonIcon.Attributes.Add( "class", "fa fa-lock" );
            this.Controls.Add( buttonIcon );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            string url = this.Page.ResolveUrl( string.Format( "~/Secure/{0}/{1}?t={2}&pb=&sb=Done", EntityTypeId, EntityId, HttpUtility.JavaScriptStringEncode(Title) ) );
            this.HRef = "javascript: Rock.controls.modal.show($(this), '" + url + "')";

            base.RenderControl( writer );
        }
    }
}