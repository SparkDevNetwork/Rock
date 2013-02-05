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
    /// A <see cref="T:System.Web.UI.WebControls.Literal"/> control with an associated label.
    /// </summary>
    [ToolboxData( "<{0}:LabeledText runat=server></{0}:LabeledText>" )]
    public class LabeledText : CompositeControl, ILabeledControl
    {
        private Label label;
        private Literal literal;

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
                EnsureChildControls();
                return label.Text;
            }
            set
            {
                EnsureChildControls();
                label.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the text CSS class.
        /// </summary>
        /// <value>
        /// The text CSS class.
        /// </value>
        public string TextCssClass
        {
            get
            {
                string s = ViewState["TextCssClass"] as string;
                return s == null ? string.Empty : s;
            }
            set
            {
                ViewState["TextCssClass"] = value;
            }
        }

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
        Description( "The text." )
        ]
        public string Text
        {
            get
            {
                EnsureChildControls();
                return literal.Text;
            }
            set
            {
                EnsureChildControls();
                literal.Text = value;
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            label = new Label();
            literal = new Literal();

            Controls.Add( label );
            Controls.Add( literal );
        }

        /// <summary>
        /// Renders a label and <see cref="T:System.Web.UI.WebControls.TextBox"/> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that receives the rendered output.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            writer.AddAttribute( "class", "control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            label.AddCssClass( "control-label" );
            

            label.RenderControl( writer );

            if ( !string.IsNullOrWhiteSpace( TextCssClass ) )
            {
                writer.AddAttribute( "class", "controls " + TextCssClass );
            }
            else
            {
                writer.AddAttribute( "class", "controls" );
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            literal.RenderControl( writer );

            writer.RenderEndTag();

            writer.RenderEndTag();
        }

    }
}