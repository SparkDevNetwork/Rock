//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.DropDownList"/> control with an associated label.
    /// </summary>
    [ToolboxData( "<{0}:LabeledDropDownList runat=server></{0}:LabeledDropDownList>" )]
    public class LabeledDropDownList : DropDownList
    {
        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string LabelText
        {
            get
            {
                string s = ViewState["LabelText"] as string;
                return s == null ? string.Empty : s;
            }
            set
            {
                ViewState["LabelText"] = value;
            }
        }

        /// <summary>
        /// Renders a label and <see cref="T:System.Web.UI.WebControls.DropDownList"/> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that receives the rendered output.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            writer.Write( string.Format( @"<dt><label for=""{0}"">{1}</label></dt><dd>", this.ClientID, LabelText ) );
            base.Render( writer );
            writer.Write( @"</dd>" );
        }

    }
}