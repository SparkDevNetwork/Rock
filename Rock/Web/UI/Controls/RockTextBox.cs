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
    [ToolboxData( "<{0}:RockTextBox runat=server></{0}:RockTextBox>" )]
    public class RockTextBox : TextBox, IRockControl
    {
        #region IRockControl implementation

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
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
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
                return HelpBlock != null ? HelpBlock.Text : string.Empty;
            }
            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
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
            get { return ViewState["Required"] as bool? ?? false; }
            set { ViewState["Required"] = value; }
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
                return RequiredFieldValidator != null ? RequiredFieldValidator.ErrorMessage : string.Empty;
            }
            set
            {
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ErrorMessage = value;
                }
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
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        #region Properties

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
            get { return ViewState["PrependText"] as string ?? string.Empty; }
            set { ViewState["PrependText"] = value; }
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
            get { return ViewState["AppendText"] as string ?? string.Empty; }
            set { ViewState["AppendText"] = value; }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RockTextBox" /> class.
        /// </summary>
        public RockTextBox()
            : base()
        {
            RequiredFieldValidator = new RequiredFieldValidator();
            RequiredFieldValidator.ValidationGroup = this.ValidationGroup;
            HelpBlock = new HelpBlock();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls(this, Controls);
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            // logic to add input groups for preappend and append labels
            bool renderInputGroup = false;

            if ( !string.IsNullOrWhiteSpace( PrependText ) || !string.IsNullOrWhiteSpace( AppendText ) )
            {
                renderInputGroup = true;
            }

            if ( renderInputGroup )
            {
                writer.AddAttribute( "class", "input-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
            }

            if ( !string.IsNullOrWhiteSpace( PrependText ) )
            {
                writer.AddAttribute( "class", "input-group-addon" );
                writer.RenderBeginTag( HtmlTextWriterTag.Span );
                writer.Write( PrependText );
                writer.RenderEndTag();
            }

            ( (WebControl)this ).AddCssClass( "form-control" );
            base.RenderControl( writer );

            if ( !string.IsNullOrWhiteSpace( AppendText ) )
            {
                writer.AddAttribute( "class", "input-group-addon" );
                writer.RenderBeginTag( HtmlTextWriterTag.Span );
                writer.Write( AppendText );
                writer.RenderEndTag();
            }

            if ( renderInputGroup )
            {
                writer.RenderEndTag();  // input-group
            }

            RenderDataValidator( writer );

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