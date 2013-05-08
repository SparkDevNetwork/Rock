//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;

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

            this.Attributes.Add( "height", "500px" );

            HtmlGenericControl buttonIcon = new HtmlGenericControl( "i" );
            buttonIcon.Attributes.Add( "class", "icon-lock" );
            this.Controls.Add( buttonIcon );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            string url = this.Page.ResolveUrl( string.Format( "~/Secure/{0}/{1}?t={2}&pb=&sb=Done", EntityTypeId, EntityId, Title ) );
            this.HRef = "javascript: Rock.controls.modal.show($(this), '" + url + "')";

            base.RenderControl( writer );
        }
    }
}