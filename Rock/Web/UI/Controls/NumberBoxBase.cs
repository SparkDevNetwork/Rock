// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Base class for NumberBox and CurrencyBox controls
    /// </summary>
    /// <seealso cref="Rock.Web.UI.Controls.RockTextBox" />
    public class NumberBoxBase : RockTextBox
    {
        private CustomValidator _customValidator;

        /// <summary>
        /// Gets or sets the name of the field (for range validation messages when Label is not provided)
        /// </summary>
        /// <value>
        /// The name of the field.
        /// </value>
        public string FieldName
        {
            get { return ViewState["FieldName"] as string ?? Label; }
            set { ViewState["FieldName"] = value; }
        }

        /// <summary>
        /// Gets or sets the group of controls for which the <see cref="T:System.Web.UI.WebControls.TextBox" /> control causes validation when it posts back to the server.
        /// </summary>
        /// <returns>The group of controls for which the <see cref="T:System.Web.UI.WebControls.TextBox" /> control causes validation when it posts back to the server. The default value is an empty string ("").</returns>
        public override string ValidationGroup
        {
            get
            {
                return base.ValidationGroup;
            }

            set
            {
                base.ValidationGroup = value;

                EnsureChildControls();

                if ( _customValidator != null )
                {
                    _customValidator.ValidationGroup = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the text content of the <see cref="T:System.Web.UI.WebControls.TextBox" /> control.
        /// </summary>
        public override string Text
        {
            get { return base.Text; }

            set
            {
                if ( NumberType == ValidationDataType.Double || NumberType == ValidationDataType.Currency )
                {
                    // An input type of Number (or currency) will not render the value correctly if it contains a comma 
                    // ( or any other character besides numbers and decimals), so strip those characters out first
                    base.Text = value == null ? string.Empty : System.Text.RegularExpressions.Regex.Replace( value, @"[^-0-9.]", "" );
                }
                else
                {
                    base.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets an integer value for the control.
        /// </summary>
        public int? IntegerValue
        {
            get
            {
                EnsureChildControls();

                return this.Text.ToStringSafe().AsIntegerOrNull();
            }

            set
            {
                EnsureChildControls();

                this.Text = value.ToString();
            }
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
                if ( value != null )
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
                EnsureChildControls();
                return base.IsValid && _customValidator.IsValid;
            }
        }

        /// <summary>
        /// Sets the validation message display mode for the control.
        /// </summary>
        /// <remarks>
        /// The default behavior is:
        /// Client-side validation shows visual feedback only, no error messages.
        /// Server-side validation shows detailed error messages in the validation summary.
        /// </remarks>
        public ValidatorDisplay ValidationDisplay
        {
            get { return ViewState["ValidationDisplay"].ToStringSafe().ConvertToEnum<ValidatorDisplay>( ValidatorDisplay.None ); }
            set { ViewState["ValidationDisplay"] = value; }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            /*
             * The input field is intentionally rendered as type='text' rather type='number'.
             * This avoids the validation issues caused by the different type='number' implementations for various browsers.
             * To set the appropriate onscreen keyboard for browers that support this feature,
             * we specify the inputmode attribute instead.
             */
            string inputmode;
            if ( this.NumberType == ValidationDataType.Integer )
            {
                inputmode = "numeric";
            }
            else if ( this.NumberType == ValidationDataType.Double || this.NumberType == ValidationDataType.Currency )
            {
                inputmode = "decimal";
            }
            else
            {
                inputmode = "text";
            }

            this.Attributes.Add( "inputmode", inputmode );
            this.FormGroupCssClass = String.Join( " ", this.FormGroupCssClass, "js-number-box" );
            base.CreateChildControls();

            _customValidator = new CustomValidator();
            _customValidator.ID = this.ID + nameof( _customValidator );
            _customValidator.ControlToValidate = this.ID;
            _customValidator.Display = this.ValidationDisplay;
            _customValidator.CssClass = "validation-error help-inline js-number-box-validator";
            _customValidator.ClientValidationFunction = "Rock.controls.numberBox.clientValidate";
            _customValidator.ServerValidate += ServerValidation;
            Controls.Add( _customValidator );
        }

        private void ServerValidation( object source, ServerValidateEventArgs args )
        {
            string validationMessage = null;
            var controlName = this.Label ?? "Numeric Value";
            var value = args.Value;

            if ( string.IsNullOrWhiteSpace( value ) )
            {
                // Check if a required value is missing.
                if ( this.Required )
                {
                    validationMessage = $"{controlName} is required.";
                }
            }
            else
            {
                // Validate input is numeric.
                var decimalValue = value.AsDecimalInvariantCultureOrNull();
                if ( decimalValue == null )
                {
                    validationMessage = $"{controlName} must be a valid number.";
                }
                else
                {
                    // Validate number.
                    if ( NumberType == ValidationDataType.Integer )
                    {
                        // Validate integer type.
                        if ( !string.IsNullOrWhiteSpace( value ) )
                        {
                            var intValue = args.Value.AsIntegerOrNull();
                            if ( intValue == null )
                            {
                                validationMessage = $"{controlName} must be an integer value.";
                            }
                        }
                    }
                }

                // Validate range.
                if ( validationMessage == null )
                {
                    var min = MinimumValue.IsNotNullOrWhiteSpace() ? Convert.ToDecimal( MinimumValue ) : int.MinValue;
                    var max = MaximumValue.IsNotNullOrWhiteSpace() ? Convert.ToDecimal( MaximumValue ) : int.MaxValue;

                    if ( decimalValue > max || decimalValue < min )
                    {
                        if ( min == int.MinValue )
                        {
                            validationMessage = string.Format( $"{controlName} must have a value of {max} or less." );
                        }
                        else if ( max == int.MaxValue )
                        {
                            validationMessage = string.Format( $"{controlName} must have a value of {min} or more." );
                        }
                        else
                        {
                            validationMessage = string.Format( $"{controlName} must have a value between {min} and {max}." );
                        }
                    }
                }
            }

            _customValidator.Text = null;
            _customValidator.ErrorMessage = validationMessage;

            args.IsValid = ( validationMessage == null );
        }

        /// <summary>
        /// Renders any data validator.
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected override void RenderDataValidator( HtmlTextWriter writer )
        {
            _customValidator.Attributes.Add( "control", ClientID );
            _customValidator.Attributes.Add( "type", this.NumberType.ToString().ToLower() );

            var minValue = MinimumValue.AsDecimalOrNull();
            if ( minValue != null )
            {
                _customValidator.Attributes.Add( "min", minValue.ToString() );
            }
            var maxValue = MaximumValue.AsDecimalOrNull();
            if ( maxValue != null )
            {
                _customValidator.Attributes.Add( "max", maxValue.ToString() );
            }

            _customValidator.ValidationGroup = this.ValidationGroup;

            _customValidator.RenderControl( writer );
        }
    }
}
