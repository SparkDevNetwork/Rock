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
    public class NumberBox : LabeledTextBox
    {
        private RangeValidator rangeValidator;

        /// <summary>
        /// Gets or sets the name of the field (for range validation messages when LabelText is not provided)
        /// </summary>
        /// <value>
        /// The name of the field.
        /// </value>
        public string FieldName
        {
            get { return ViewState["FieldName"] as string ?? LabelText; }
            set { ViewState["FieldName"] = value; }
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
        public override bool IsValid
        {
            get
            {
                return base.IsValid && rangeValidator.IsValid;
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            rangeValidator = new RangeValidator();
            rangeValidator.ID = this.ID + "_RV";
            rangeValidator.ControlToValidate = this.ID;
            rangeValidator.Display = ValidatorDisplay.Dynamic;
            rangeValidator.CssClass = "validation-error help-inline";
            
            rangeValidator.Type = System.Web.UI.WebControls.ValidationDataType.Integer;
            rangeValidator.MinimumValue = int.MinValue.ToString();
            rangeValidator.MaximumValue = int.MaxValue.ToString();

            Controls.Add( rangeValidator );
        }

        /// <summary>
        /// Renders any data validator.
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected override void RenderDataValidator( HtmlTextWriter writer )
        {
            rangeValidator.Type = NumberType;
            rangeValidator.MinimumValue = this.MinimumValue;
            rangeValidator.MaximumValue = this.MaximumValue;
            string dataTypeText = string.Empty;

            int minValue = MinimumValue.AsInteger() ?? int.MinValue;
            int maxValue = MaximumValue.AsInteger() ?? int.MaxValue;

            string rangeMessageFormat = null;

            if ( rangeValidator.Type == ValidationDataType.Integer )
            {
                // if they are in the valid range, but not an integer, they'll see this message
                rangeMessageFormat = "{0} must be an integer";
            }

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

            rangeValidator.ValidationGroup = this.ValidationGroup;
            rangeValidator.RenderControl( writer );
        }
    }
}