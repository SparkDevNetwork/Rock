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
    /// A <see cref="T:System.Web.UI.WebControls.TextBox"/> control with an associated label.
    /// </summary>
    [ToolboxData( "<{0}:LabeledTextBox runat=server></{0}:LabeledTextBox>" )]
    public class LabeledTextBox : CompositeControl
    {
        private Label label;
        private TextBox textBox;
        private RequiredFieldValidator validator;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LabeledTextBox"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public bool Required
        {
            get
            {
                if ( ViewState["Required"] != null )
                    return ( bool )ViewState["Required"];
                else
                    return false;
            }
            set
            {
                ViewState["Required"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [display required indicator].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [display required indicator]; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "true" ),
        Description( "Should the required indicator be displayed?" )
        ]
        public bool DisplayRequiredIndicator
        {
            get
            {
                if ( ViewState["DisplayRequiredIndicator"] != null )
                    return ( bool )ViewState["DisplayRequiredIndicator"];
                else
                    return true;
            }
            set
            {
                ViewState["DisplayRequiredIndicator"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the help tip.
        /// </summary>
        /// <value>
        /// The help tip.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help tip." )
        ]
        public string Tip
        {
            get
            {
                string s = ViewState["Tip"] as string;
                return s == null ? string.Empty : s;
            }
            set
            {
                ViewState["Tip"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get
            {
                string s = ViewState["Help"] as string;
                return s == null ? string.Empty : s;
            }
            set
            {
                ViewState["Help"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the text mode for the textbox.
        /// </summary>
        /// <value>
        /// TextBoxMode (single line, multi-line, password)
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "Sets the TextMode for the textbox control." )
        ]
        public TextBoxMode TextBoxTextMode
        {
            get
            {
                EnsureChildControls();
                return textBox.TextMode;
            }
            set
            {
                EnsureChildControls();
                textBox.TextMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of rows for the textbox.
        /// </summary>
        /// <value>
        /// Number of rows for the textbox.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "Sets the number of rows for the textbox control." )
        ]
        public int TextBoxRows
        {
            get
            {
                EnsureChildControls();
                return textBox.Rows;
            }
            set
            {
                EnsureChildControls();
                textBox.Rows = value;
            }
        }

        /// <summary>
        /// Gets or sets the CSS class of the textbox control.
        /// </summary>
        /// <value>
        /// The CSS class(s) for the textbox control.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "Sets the number of rows for the textbox control." )
        ]
        public string TextBoxCssClass
        {
            get
            {
                EnsureChildControls();
                return textBox.CssClass;
            }
            set
            {
                EnsureChildControls();
                textBox.CssClass = value;
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
        /// Gets or sets the textbox text.
        /// </summary>
        /// <value>
        /// The textbox text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the textbox." )
        ]
        public string Text
        {
            get
            {
                EnsureChildControls();
                return textBox.Text;
            }
            set
            {
                EnsureChildControls();
                textBox.Text = value;
            }
        }

        /// <summary>
        /// Gets the text box.
        /// </summary>
        public TextBox TextBox
        {
            get
            {
                EnsureChildControls();
                return textBox;
            }
        }

        /// <summary>
        /// Recreates the child controls in a control derived from <see cref="T:System.Web.UI.WebControls.CompositeControl"/>.
        /// </summary>
        protected override void RecreateChildControls()
        {
            EnsureChildControls();
        }

        /// <summary>
        /// Renders a label and <see cref="T:System.Web.UI.WebControls.TextBox"/> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that receives the rendered output.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            AddAttributesToRender( writer );

            bool isValid = !Required || validator.IsValid;

            writer.AddAttribute( "class", isValid ? "" : "error" );
            writer.RenderBeginTag( HtmlTextWriterTag.Dl );

            writer.RenderBeginTag( HtmlTextWriterTag.Dt );
            if ( Required && DisplayRequiredIndicator )
                writer.AddAttribute( "class", "required" );
            label.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderBeginTag( HtmlTextWriterTag.Dd );
            textBox.RenderControl( writer );

            if ( Required )
            {
                validator.ErrorMessage = LabelText + " is Required.";
                validator.RenderControl( writer );
            }

            if ( Tip.Trim() != string.Empty )
            {
                writer.AddAttribute( "class", "help-tip" );
                writer.AddAttribute( "href", "#" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.Write( "help" );
                writer.RenderBeginTag( HtmlTextWriterTag.Span );
                writer.Write( Tip.Trim() );
                writer.RenderEndTag();
                writer.RenderEndTag();
            }

            if ( Help.Trim() != string.Empty )
            {
                writer.AddAttribute( "class", "help-block" );
                writer.RenderBeginTag( HtmlTextWriterTag.Span );
                writer.Write( Tip.Trim() );
                writer.RenderEndTag();
            }

            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            textBox = new TextBox();
            textBox.ID = "tb";

            label = new Label();
            label.AssociatedControlID = textBox.ID;

            validator = new RequiredFieldValidator();
            validator.ID = "rfv";
            validator.ControlToValidate = textBox.ID;
            validator.Display = ValidatorDisplay.Dynamic;
            validator.CssClass = "validation-error";

            this.Controls.Add( label );
            this.Controls.Add( textBox );
            this.Controls.Add( validator );
        }

    }
}