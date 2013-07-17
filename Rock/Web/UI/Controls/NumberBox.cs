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
    /// A <see cref="T:System.Web.UI.WebControls.TextBox"/> control with numerical validation 
    /// </summary>
    [ToolboxData( "<{0}:NumberBox runat=server></{0}:NumberBox>" )]
    public class NumberBox : TextBox, ILabeledControl, IRequiredControl
    {
        private RequiredFieldValidator requiredValidator;
        private RangeValidator rangeValidator;        
        private Label _label;

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
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        public string LabelText
        {
            get { return _label.Text; }
            set { _label.Text = value; }
        }

        /// <summary>
        /// Gets or sets the name of the field (for validation messages)
        /// </summary>
        /// <value>
        /// The name of the field.
        /// </value>
        public string FieldName
        {
            get { return _label.Text; }
            set { _label.Text = value; }
        }

        /// <summary>
        /// Gets or sets the type of the validation data.
        /// </summary>
        /// <value>
        /// The type of the validation data.
        /// </value>
        public ValidationDataType NumberType
        {
            get 
            {
                string value = ViewState["NumberType"] as string;
                if (value != null)
                {
                    return value.ConvertToEnum<ValidationDataType>();
                }
                else
                {
                    return ValidationDataType.Integer;
                }
            }

            set { ViewState["NumberType"] = value.ConvertToString(); }
        }
        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        /// <value>
        /// The minimum value.
        /// </value>
        public string MinimumValue
        {
            get { return ViewState["MinimumValue"] as string; }
            set { ViewState["MinimumValue"] = value; }
        }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        /// <value>
        /// The maximum value.
        /// </value>
        public string MaximumValue
        {
            get { return ViewState["MaximumValue"] as string; }
            set { ViewState["MaximumValue"] = value; }
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
                return ( !Required || requiredValidator.IsValid ) && rangeValidator.IsValid;
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
        /// Initializes a new instance of the <see cref="NumberBox" /> class.
        /// </summary>
        public NumberBox()
        {
            requiredValidator = new RequiredFieldValidator();
            rangeValidator = new RangeValidator();
            _label = new Label();            
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );

            rangeValidator.ControlToValidate = this.ID;
            rangeValidator.ID = this.ID + "_RV";
            rangeValidator.Display = ValidatorDisplay.Dynamic;
            rangeValidator.CssClass = "validation-error help-inline";
            rangeValidator.ErrorMessage = "Numerical value is required";
            rangeValidator.Type = System.Web.UI.WebControls.ValidationDataType.Integer;
            rangeValidator.MinimumValue = int.MinValue.ToString();
            rangeValidator.MaximumValue = int.MaxValue.ToString();
            rangeValidator.Enabled = false;
            Controls.Add( rangeValidator );

            requiredValidator.ID = this.ID + "_rfv";
            requiredValidator.ControlToValidate = this.ID;
            requiredValidator.Display = ValidatorDisplay.Dynamic;
            requiredValidator.CssClass = "validation-error help-inline";
            requiredValidator.Enabled = false;
            Controls.Add( requiredValidator );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            bool renderLabel = !string.IsNullOrEmpty( LabelText );

            if ( renderLabel )
            {
                writer.AddAttribute( "class", "control-group" +
                    ( IsValid ? "" : " error" ) +
                    ( Required ? " required" : "" ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                _label.AddCssClass( "control-label" );
                _label.RenderControl( writer );

                writer.AddAttribute( "class", "controls" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
            }

            base.RenderControl( writer );

            if ( Required )
            {
                requiredValidator.Enabled = true;
                requiredValidator.ValidationGroup = this.ValidationGroup;
                if ( string.IsNullOrWhiteSpace( requiredValidator.ErrorMessage ) )
                {
                    requiredValidator.ErrorMessage = LabelText + " is Required.";
                }
                requiredValidator.RenderControl( writer );
            }

            rangeValidator.Type = NumberType;

            if ( NumberType == ValidationDataType.Double )
            {
                rangeValidator.MinimumValue = !string.IsNullOrEmpty( MinimumValue )
                    ? MinimumValue : Convert.ToDouble( "9e-300" ).ToString( "F0" );  // allows up to 300 digits
                rangeValidator.MaximumValue = !string.IsNullOrEmpty( MaximumValue )
                    ? MaximumValue : Convert.ToDouble( "9e300" ).ToString( "F0" );   // allows up to 300 digits
            }
            else if (NumberType == ValidationDataType.Integer)
            {
                rangeValidator.MinimumValue = !string.IsNullOrEmpty( MinimumValue )
                    ? MinimumValue : int.MinValue.ToString();
                rangeValidator.MaximumValue = !string.IsNullOrEmpty( MaximumValue )
                    ? MaximumValue : int.MaxValue.ToString();
            }   

            int minValue = MinimumValue.AsInteger() ?? int.MinValue;
            int maxValue = MaximumValue.AsInteger() ?? int.MaxValue;

            string rangeMessageFormat = null;
            if ( minValue > int.MinValue)
            {
                rangeMessageFormat = "{0} must be at least " + MinimumValue;
            }
            
            if ( maxValue < int.MaxValue )
            {
                rangeMessageFormat = "{0} must be at most " + MaximumValue;
            }

            if ( ( minValue > int.MinValue ) && ( maxValue < int.MaxValue ) )
            {
                rangeMessageFormat = string.Format( "{{0}} must be between {0} and {1} ", MinimumValue, MaximumValue );
            }

            if ( !string.IsNullOrWhiteSpace( rangeMessageFormat ) )
            {
                rangeValidator.ErrorMessage = string.Format( rangeMessageFormat, string.IsNullOrWhiteSpace(FieldName) ? "Value" : FieldName );
            }

            rangeValidator.Enabled = true;
            rangeValidator.RenderControl( writer );
            
            if ( renderLabel )
            {
                writer.RenderEndTag();

                writer.RenderEndTag();
            }
        }
    }
}