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
    public class LabeledDropDownList : RockDropDownList, ILabeledControl, IRequiredControl
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
        /// The validator
        /// </summary>
        protected RequiredFieldValidator requiredValidator;

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
        public virtual bool Required
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
                return helpBlock.Text;
            }
            set
            {
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
        public string Label
        {
            get
            {
                return label.Text;
            }
            set
            {
                label.Text = value;
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
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get
            {
                return requiredValidator.ErrorMessage;
            }
            set
            {
                requiredValidator.ErrorMessage = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabeledDropDownList" /> class.
        /// </summary>
        public LabeledDropDownList()
        {
            label = new Literal();
            helpBlock = new HelpBlock();
            requiredValidator = new RequiredFieldValidator();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            Controls.Add( label );

            requiredValidator.ID = this.ID + "_rfv";
            requiredValidator.ControlToValidate = this.ID;
            requiredValidator.Display = ValidatorDisplay.Dynamic;
            requiredValidator.CssClass = "validation-error help-inline";
            requiredValidator.Enabled = false;
            Controls.Add( requiredValidator );

            Controls.Add( helpBlock );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                bool renderControlGroupDiv = ( !string.IsNullOrWhiteSpace( Label ) || !string.IsNullOrWhiteSpace( Help ) );

                if ( renderControlGroupDiv )
                {
                    writer.AddAttribute( "class", "form-group" +
                        ( IsValid ? "" : " error" ) +
                        ( Required ? " required" : "" ) );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    writer.AddAttribute("for", this.ClientID);
                    writer.RenderBeginTag( HtmlTextWriterTag.Label );
                    label.Visible = this.Visible;
                    label.RenderControl( writer );
                    helpBlock.RenderControl( writer );
                    writer.RenderEndTag();
                }

                writer.AddAttribute("class", "form-control");
                base.RenderControl( writer );

                if ( Required )
                {
                    requiredValidator.Enabled = true;
                    requiredValidator.ValidationGroup = this.ValidationGroup;
                    if ( string.IsNullOrWhiteSpace( requiredValidator.ErrorMessage ) )
                    {
                        requiredValidator.ErrorMessage = Label + " is Required.";
                    }
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

                if ( renderControlGroupDiv )
                {
                    writer.RenderEndTag();
                }
            }
        }

        /// <summary>
        /// Renders any data validator.
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected virtual void RenderDataValidator( HtmlTextWriter writer)
        {
        }

        /// <summary>
        /// Creates a collection to store child controls.
        /// </summary>
        /// <returns>
        /// Always returns an <see cref="T:System.Web.UI.EmptyControlCollection"/>.
        /// </returns>
        protected override ControlCollection CreateControlCollection()
        {
            // By default a DropDownList control does not allow adding of child controls.
            // This method needs to be overridden to allow this
            return new ControlCollection( this );
        }

    }
}