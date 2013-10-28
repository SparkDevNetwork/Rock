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
    /// Control for selecting a number range
    /// </summary>
    [ToolboxData( "<{0}:NumberRangeEditor runat=server></{0}:NumberRangeEditor>" )]
    public class NumberRangeEditor : CompositeControl, IRockControl
    {
        #region IRockControl implementation (Uses non-standard logic for required fields)

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
            set 
            { 
                ViewState["Required"] = value;
                EnsureChildControls();
                _tbLowerValue.Required = value;
                _tbUpperValue.Required = value;
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
                EnsureChildControls();
                return _tbLowerValue.RequiredErrorMessage;
            }

            set
            {
                EnsureChildControls();
                _tbLowerValue.RequiredErrorMessage = value;
                _tbUpperValue.RequiredErrorMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets an optional validation group to use.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                EnsureChildControls();
                return _tbLowerValue.ValidationGroup;
            }

            set
            {
                EnsureChildControls();
                _tbLowerValue.ValidationGroup = value;
                _tbUpperValue.ValidationGroup = value;
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
                return _tbLowerValue.IsValid && _tbUpperValue.IsValid;
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

        #region Controls

        /// <summary>
        /// The lower value edit box
        /// </summary>
        private NumberBox _tbLowerValue;

        /// <summary>
        /// The upper value edit box
        /// </summary>
        private NumberBox _tbUpperValue;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the type of the number.
        /// </summary>
        /// <value>
        /// The type of the number.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The NumberType (Decimal or Integer) for the Controls" )
        ]
        public ValidationDataType NumberType
        {
            get
            {
                EnsureChildControls();
                return _tbLowerValue.NumberType;
            }

            set 
            {
                EnsureChildControls();
                _tbLowerValue.NumberType = value;
                _tbUpperValue.NumberType = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum value that either number can be
        /// </summary>
        /// <value>
        /// The minimum value.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The minimum value that either number can be" )
        ]
        public string MinimumValue
        {
            get {
                EnsureChildControls();
                return _tbLowerValue.MinimumValue; 
            }

            set {
                EnsureChildControls();
                _tbLowerValue.MinimumValue = value;
                _tbUpperValue.MinimumValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum value that either number can be
        /// </summary>
        /// <value>
        /// The maximum value.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The maximum value that either number can be" )
        ]
        public string MaximumValue
        {
            get
            {
                EnsureChildControls();
                return _tbLowerValue.MaximumValue;
            }

            set
            {
                EnsureChildControls();
                _tbLowerValue.MaximumValue = value;
                _tbUpperValue.MaximumValue = value;
            }

        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberRangeEditor"/> class.
        /// </summary>
        public NumberRangeEditor()
            : base()
        {
            HelpBlock = new HelpBlock();
        }

        #endregion

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _tbLowerValue = new NumberBox();
            _tbLowerValue.ID = this.ID + "_lower";
            _tbLowerValue.CssClass = "input-width-md";
            Controls.Add( _tbLowerValue );

            _tbUpperValue = new NumberBox();
            _tbUpperValue.ID = this.ID + "_upper";
            _tbUpperValue.CssClass = "input-width-md";
            Controls.Add( _tbUpperValue );

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
        /// This is where you implment the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl(HtmlTextWriter writer)
        {
            writer.AddAttribute( "class", "form-control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _tbLowerValue.RenderControl( writer );
            writer.Write( "<span class='to'> to </span>" );
            _tbUpperValue.RenderControl( writer );

            writer.RenderEndTag();
        }

        /// <summary>
        /// Gets or sets the lower value.
        /// </summary>
        /// <value>
        /// The lower value.
        /// </value>
        public decimal? LowerValue {
            get
            {
                EnsureChildControls();

                decimal result;
                if (decimal.TryParse(_tbLowerValue.Text, out result))
                {
                    return result;
                }

                return null;
            }

            set
            {
                EnsureChildControls();
                _tbLowerValue.Text = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the upper value.
        /// </summary>
        /// <value>
        /// The upper value.
        /// </value>
        public decimal? UpperValue
        {
            get
            {
                EnsureChildControls();

                decimal result;
                if (decimal.TryParse(_tbUpperValue.Text, out result))
                {
                    return result;
                }

                return null;
            }

            set
            {
                EnsureChildControls();
                _tbUpperValue.Text = value.ToString();
            }
        }
    }
}
