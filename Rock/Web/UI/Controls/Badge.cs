//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Displays a bootstrap badge
    /// </summary>
    [ToolboxData( "<{0}:Badge runat=server></{0}:Badge>" )]
    public class Badge : PlaceHolder
    {
        /// <summary>
        /// Gets or sets the tool tip.
        /// </summary>
        /// <value>
        /// The tool tip.
        /// </value>
        public string ToolTip 
        {
            get { return ViewState["ToolTip"] as string ?? string.Empty; }
            set { ViewState["ToolTip"] = value; }
        }

        /// <summary>
        /// Gets or sets the type of the badge.
        /// </summary>
        /// <value>
        /// The type of the badge.
        /// </value>
        public string BadgeType
        {
            get { return ViewState["BadgeType"] as string ?? string.Empty; }
            set { ViewState["BadgeType"] = value; }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                string css = "badge";
                if ( !string.IsNullOrWhiteSpace(BadgeType))
                {
                    css += " badge-" + BadgeType.ToLower();
                }

                writer.AddAttribute( "class", css );
                if ( !string.IsNullOrWhiteSpace( ToolTip ) )
                {
                    writer.AddAttribute( "title", ToolTip );
                    writer.AddAttribute( "data-toggle", "tooltip" );
                }
                writer.RenderBeginTag( HtmlTextWriterTag.Span );

                base.RenderControl( writer );

                writer.RenderEndTag();
            }
        }
    }

}