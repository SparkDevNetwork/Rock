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
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control for selecting a date range
    /// </summary>
    [ToolboxData( "<{0}:DateRangePicker runat=server></{0}:DateRangePicker>" )]
    public class DateRangePicker : CompositeControl, IRockControl
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
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
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
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The warning block." )
        ]
        public string Warning
        {
            get
            {
                return WarningBlock != null ? WarningBlock.Text : string.Empty;
            }

            set
            {
                if ( WarningBlock != null )
                {
                    WarningBlock.Text = value;
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
                EnsureChildControls();
                return CustomValidator.ErrorMessage;
            }

            set
            {
                EnsureChildControls();
                CustomValidator.ErrorMessage = value;
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
                return !Required || CustomValidator.IsValid;
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
        /// Gets or sets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DateRangePicker"/> class.
        /// </summary>
        public DateRangePicker()
            : base()
        {
            CustomValidator = new CustomValidator();
            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

        #region Controls

        /// <summary>
        /// The lower value
        /// </summary>
        private DatePicker _tbLowerValue;

        /// <summary>
        /// The upper value
        /// </summary>
        private DatePicker _tbUpperValue;

        /// <summary>
        /// Gets or sets the custom validator.
        /// </summary>
        /// <value>
        /// The custom validator.
        /// </value>
        public CustomValidator CustomValidator { get; set; }

        /// <summary>
        /// Gets or sets the class that should be applied to the div that wraps the two date pickers
        /// default is "form-control-group"
        /// </summary>
        /// <value>
        /// The inputs class.
        /// </value>
        public string InputsClass
        {
            get
            {
                return ( ViewState["InputsClass"] as string ) ?? "form-control-group";
            }

            set
            {
                ViewState["InputsClass"] = value;
            }
        }

        #endregion

        /// <summary>
        /// Registers the java script.
        /// </summary>
        private void RegisterJavaScript()
        {
            // Get current date format and make sure it has double-lower-case month and day designators for the js date picker to use
            var dateFormat = System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern;
            dateFormat = dateFormat.Replace( "M", "m" ).Replace( "m", "mm" ).Replace( "mmmm", "mm" );
            dateFormat = dateFormat.Replace( "d", "dd" ).Replace( "dddd", "dd" );

            // a little javascript to make the daterange picker behave similar to the bootstrap-datepicker demo site's date range picker
            var script = $@"
$(function() {{
    $('#{this.ClientID}').datepicker({{
        format: '{dateFormat}',
        todayHighlight: true,
        autoclose: true,
        inputs: $('#{this.ClientID} .form-control')
    }});
}});

// if the guest clicks the addon select all the text in the input
$('#{this.ClientID}').find('.input-group-lower .input-group-addon').on('click', function () {{
    $(this).siblings('.form-control').select();
}});

$('#{_tbLowerValue.ClientID}').on('changeDate', function (ev) {{
    // set focus to the end date
    $('#{_tbUpperValue.ClientID}')[0].focus();
}});

// if the guest clicks the addon select all the text in the input
$('#{this.ClientID}').find('.input-group-upper .input-group-addon').on('click', function () {{
    $(this).siblings('.form-control').select();
}});

// if value changes then re-validate the custom validator.
$('#{_tbLowerValue.ClientID},#{_tbUpperValue.ClientID}').on('change', function (ev) {{
    ValidatorValidate({CustomValidator.ClientID});
}});
";

            ScriptManager.RegisterStartupScript( this, this.GetType(), "daterange_picker-" + this.ClientID, script, true );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _tbLowerValue = new DatePicker();
            _tbLowerValue.EnableJavascript = false;
            _tbLowerValue.ID = this.ID + "_lower";
            _tbLowerValue.CssClass = "input-group-lower js-lower";
            Controls.Add( _tbLowerValue );

            _tbUpperValue = new DatePicker();
            _tbUpperValue.EnableJavascript = false;
            _tbUpperValue.ID = this.ID + "_upper";
            _tbUpperValue.CssClass = "input-group-upper js-upper";
            Controls.Add( _tbUpperValue );

            // add custom validator
            CustomValidator.ID = this.ID + "_cfv";
            CustomValidator.ClientValidationFunction = "Rock.controls.dateRangePicker.clientValidate";
            CustomValidator.ErrorMessage = ( this.Label != string.Empty ? this.Label : string.Empty ) + " is required.";
            CustomValidator.CssClass = "validation-error help-inline";
            CustomValidator.Enabled = true;
            CustomValidator.Display = ValidatorDisplay.Dynamic;
            CustomValidator.ValidationGroup = ValidationGroup;
            Controls.Add( CustomValidator );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer, this.CssClass );
            }
        }

        /// <summary>
        /// This is where you implement the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            RegisterJavaScript();

            writer.AddAttribute( "id", this.ClientID );
            writer.AddAttribute( "data-required", this.Required.ToTrueFalse().ToLower() );
            writer.AddAttribute( "data-itemlabel", this.Label );
            foreach ( var styleKey in this.Style.Keys )
            {
                string styleName = ( string ) styleKey;
                writer.AddStyleAttribute( styleName, this.Style[styleName] );
            }

            if ( !string.IsNullOrEmpty( this.CssClass ) )
            {
                writer.AddAttribute( "class", "js-daterangepicker picker-daterange " + this.CssClass );
            }
            else
            {
                writer.AddAttribute( "class", "js-daterangepicker picker-daterange" );
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", this.InputsClass );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _tbLowerValue.RenderControl( writer );
            writer.Write( "<div class='input-group form-control-static'> to </div>" );
            _tbUpperValue.RenderControl( writer );

            writer.RenderEndTag(); // form-control-group

            CustomValidator.RenderControl( writer );

            writer.RenderEndTag(); // id
        }

        /// <summary>
        /// Gets or sets the lower value.
        /// </summary>
        /// <value>
        /// The lower value.
        /// </value>
        public DateTime? LowerValue
        {
            get
            {
                EnsureChildControls();
                return _tbLowerValue.SelectedDate;
            }

            set
            {
                EnsureChildControls();
                _tbLowerValue.SelectedDate = value;
            }
        }

        /// <summary>
        /// Gets or sets the upper value.
        /// </summary>
        /// <value>
        /// The upper value.
        /// </value>
        public DateTime? UpperValue
        {
            get
            {
                EnsureChildControls();
                return _tbUpperValue.SelectedDate;
            }

            set
            {
                EnsureChildControls();
                _tbUpperValue.SelectedDate = value;
            }
        }

        /// <summary>
        /// Gets or sets the date range.
        /// </summary>
        /// <value>
        /// The date range.
        /// </value>
        public DateRange DateRange
        {
            get
            {
                return new DateRange( this.LowerValue, this.UpperValue );
            }

            set
            {
                this.LowerValue = value.Start;
                this.UpperValue = value.End;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [read only].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [read only]; otherwise, <c>false</c>.
        /// </value>
        public bool ReadOnly
        {
            get
            {
                EnsureChildControls();
                return _tbLowerValue.ReadOnly;
            }

            set
            {
                EnsureChildControls();
                _tbLowerValue.ReadOnly = value;
                _tbUpperValue.ReadOnly = value;
            }
        }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                return ViewState["ValidationGroup"] as string;
            }

            set
            {
                ViewState["ValidationGroup"] = value;

                if ( CustomValidator != null )
                {
                    CustomValidator.ValidationGroup = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the lower and upper values by specifying a comma-delimted lower and upper date in ISO 8601 format
        /// </summary>
        /// <value>
        /// The delimited values.
        /// </value>
        public string DelimitedValues
        {
            get
            {
                if ( this.LowerValue == null && this.UpperValue == null )
                {
                    return string.Empty;
                }
                else
                {
                    // serialize the date using ISO 8601 standard
                    return string.Format(
                        "{0},{1}",
                        this.LowerValue.HasValue ? this.LowerValue.Value.ToString( "o" ) : null,
                        this.UpperValue.HasValue ? this.UpperValue.Value.ToString( "o" ) : null );
                }
            }

            set
            {
                if ( value != null )
                {
                    string[] valuePair = value.Split( new char[] { ',' }, StringSplitOptions.None );
                    if ( valuePair.Length == 2 )
                    {
                        this.LowerValue = valuePair[0].AsDateTime();
                        this.UpperValue = valuePair[1].AsDateTime();
                    }
                    else
                    {
                        this.LowerValue = null;
                        this.UpperValue = null;
                    }
                }
                else
                {
                    this.LowerValue = null;
                    this.UpperValue = null;
                }
            }
        }

        /// <summary>
        /// Tries to parse the upper and lower DateTime from the delimited string
        /// </summary>
        /// <param name="delimited">The delimited value</param>
        /// <param name="lower">The lower value</param>
        /// <param name="upper">The upper value</param>
        public static bool TryParse( string delimited, out DateTime lower, out DateTime upper )
        {
            if ( !string.IsNullOrWhiteSpace( delimited ) && delimited.Contains( "," ) )
            {
                var dates = delimited.Split( ',' );

                if ( dates.Length == 2 )
                {
                    var success1 = DateTime.TryParse( dates[0], out lower );
                    var success2 = DateTime.TryParse( dates[1], out upper );
                    return success1 && success2;
                }
            }

            lower = new DateTime();
            upper = new DateTime();
            return false;
        }

        /// <summary>
        /// Formats the delimited values for display purposes
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string FormatDelimitedValues( string value )
        {
            if ( !string.IsNullOrWhiteSpace( value ) && value.Contains( "," ) )
            {
                var dates = value.Split( ',' );
                if ( dates.Length == 2 )
                {
                    return new DateRange( dates[0].AsDateTime(), dates[1].AsDateTime() ).ToString( "d" );
                }
            }

            return null;
        }

        /// <summary>
        /// Calculates the date range from delimited values.
        /// </summary>
        /// <param name="delimitedValues">The delimited values.</param>
        /// <returns></returns>
        public static DateRange CalculateDateRangeFromDelimitedValues( string delimitedValues )
        {
            return DateRange.FromDelimitedValues( delimitedValues );
        }
    }
}