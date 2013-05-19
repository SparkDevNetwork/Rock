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
    public class LabeledTextBox : TextBox, ILabeledControl
    {
        /// <summary>
        /// The label
        /// </summary>
        protected Literal label;

        /// <summary>
        /// The help block
        /// </summary>
        protected HelpBlock helpBlock;

        /// <summary>
        /// The required field validator
        /// </summary>
        protected RequiredFieldValidator requiredValidator;

        protected Label prependLabel;
        protected Label appendLabel;

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
                    return (bool)ViewState["Required"];
                else
                    return false;
            }
            set
            {
                ViewState["Required"] = value;
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
                EnsureChildControls();
                return helpBlock.Text;
            }
            set
            {
                EnsureChildControls();
                helpBlock.Text = value;
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
        /// Gets or sets the prepend text.
        /// </summary>
        /// <value>
        /// The prepend text.
        /// </value>
        [
        Bindable( false ),
        Category( "Appearance" ),
        DefaultValue(""),
        Description("Text that appears prepended to the front of the text box.")
        ]
        public string PrependText
        {
            get
            {
                EnsureChildControls();
                return prependLabel.Text;
            } 
            set
            {
                EnsureChildControls();
                prependLabel.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the append text.
        /// </summary>
        /// <value>
        /// The append text.
        /// </value>
        [
        Bindable( false ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "Text that appears appended to the end of the text box." )
        ]
        public string AppendText
        {
            get
            {
                EnsureChildControls();
                return appendLabel.Text;
            }
            set
            {
                EnsureChildControls();
                appendLabel.Text = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || requiredValidator.IsValid;
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            label = new Literal();
            Controls.Add( label );

            requiredValidator = new RequiredFieldValidator();
            requiredValidator.ID = this.ID + "_rfv";
            requiredValidator.ControlToValidate = this.ID;
            requiredValidator.Display = ValidatorDisplay.Dynamic;
            requiredValidator.CssClass = "validation-error help-inline";
            requiredValidator.Enabled = false;
            Controls.Add( requiredValidator );

            helpBlock = new HelpBlock();
            Controls.Add( helpBlock );

            prependLabel = new Label
            {
                ID = this.ID + "_prepend",
                CssClass = "add-on"
            };

            Controls.Add( prependLabel );

            appendLabel = new Label
            {
                ID = this.ID + "_append",
                CssClass = "add-on"
            };
                
            Controls.Add( appendLabel );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            bool renderControlGroupDiv = (!string.IsNullOrEmpty( LabelText ) || !string.IsNullOrWhiteSpace( Help ));
            string wrapperClassName = string.Empty;

            if ( renderControlGroupDiv )
            {
                writer.AddAttribute( "class", "control-group" +
                    ( IsValid ? "" : " error" ) +
                    ( Required ? " required" : "" ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( "class", "control-label" );

                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                label.Visible = this.Visible;
                label.RenderControl( writer );
                helpBlock.RenderControl( writer );
                writer.RenderEndTag();

                wrapperClassName = "controls";
            }
           

            if ( !string.IsNullOrWhiteSpace( PrependText ) )
            {
                wrapperClassName += " input-prepend";
            }

            if ( !string.IsNullOrWhiteSpace( AppendText ) )
            {
                wrapperClassName += " input-append";
            }

            bool renderControlsDiv = !string.IsNullOrWhiteSpace( wrapperClassName );

            if ( renderControlsDiv )
            {
                writer.AddAttribute( "class", wrapperClassName );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
            }

            if ( !string.IsNullOrWhiteSpace( PrependText ) )
            {
                prependLabel.Text = PrependText;
                prependLabel.RenderControl( writer );
            }

            base.RenderControl( writer );

            if ( !string.IsNullOrWhiteSpace( AppendText ) )
            {
                appendLabel.Text = AppendText;
                appendLabel.RenderControl( writer );
            }

            if ( Required )
            {
                requiredValidator.Enabled = true;
                requiredValidator.ErrorMessage = LabelText + " is Required.";
                requiredValidator.RenderControl( writer );
            }

            RenderDataValidator( writer );

            if ( Tip.Trim() != string.Empty )
            {
                writer.AddAttribute( "class", "help-tip" );
                writer.AddAttribute( "href", "#" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.RenderBeginTag( HtmlTextWriterTag.Span );
                writer.Write( Tip.Trim() );
                writer.RenderEndTag();
                writer.RenderEndTag();
            }

            if ( renderControlsDiv )
            {
                writer.RenderEndTag();
            }

            if ( renderControlGroupDiv )
            {
                writer.RenderEndTag();
            }
        }

        /// <summary>
        /// Renders any data validator.
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected virtual void RenderDataValidator( HtmlTextWriter writer )
        {
        }

        /// <summary>
        /// Gets or sets the text content of the <see cref="T:System.Web.UI.WebControls.TextBox" /> control.
        /// </summary>
        /// <returns>The text displayed in the <see cref="T:System.Web.UI.WebControls.TextBox" /> control. The default is an empty string ("").</returns>
        public override string Text
        {
            get
            {
                if ( base.Text == null )
                {
                    return null;
                }
                else
                {
                    return base.Text.Trim();
                }
            }
            set
            {
                base.Text = value;
            }
        }

    }
}