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
    /// Renders a term and description
    /// </summary>
    [ToolboxData( "<{0}:TermDescription runat=server></{0}:TermDescription>" )]
    public class TermDescription : Control
    {
        /// <summary>
        /// Gets or sets the term.
        /// </summary>
        /// <value>
        /// The term.
        /// </value>
        public string Term
        {
            get { return ViewState["Term"] as string ?? string.Empty; }
            set { ViewState["Term"] = value; }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description
        {
            get { return ViewState["Description"] as string ?? string.Empty; }
            set { ViewState["Description"] = value; }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                writer.RenderBeginTag( HtmlTextWriterTag.Dt );
                writer.Write( CheckEmpty( Term ) );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Dd );
                writer.Write( CheckEmpty( Description ) );
                writer.RenderEndTag();
            }
        }

        private string CheckEmpty( string value )
        {
            if ( string.IsNullOrEmpty( value ) )
            {
                return "&nbsp;";
            }
            else
            {
                return value;
            }
        }

    }
}