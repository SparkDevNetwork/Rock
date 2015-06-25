// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

        /// <summary>
        /// Initializes a new instance of the <see cref="DateRangePicker"/> class.
        /// </summary>
        public DateRangePicker()
            : base()
        {
            RequiredFieldValidator = new HiddenFieldValidator();
            HelpBlock = new HelpBlock();
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
            var scriptFormat = @"
$('#{0}').datepicker({{ format: '{2}' }}).on('changeDate', function (ev) {{
        
    if (ev.date.valueOf() > $('#{1}').data('datepicker').dates[0]) {{
        var newDate = new Date(ev.date)
        newDate.setDate(newDate.getDate() + 1);
        $('#{1}').datepicker('update', newDate);

        // disable date selection in the EndDatePicker that are earlier than the startDate
        $('#{1}').datepicker('setStartDate', ev.date);
    }}
    
    if (event && event.type == 'click') {{
        // close the start date picker and set focus to the end date
        $('#{0}').data('datepicker').hide();
        $('#{1}')[0].focus();
    }}
}});

$('#{1}').datepicker({{ format: '{2}' }}).on('changeDate', function (ev) {{
    // close the enddate picker immediately after selecting an end date
    $('#{1}').data('datepicker').hide();
}});

";
            var script = string.Format( scriptFormat, _tbLowerValue.ClientID, _tbUpperValue.ClientID, dateFormat );
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
            _tbLowerValue.ID = this.ID + "_lower";
            _tbLowerValue.CssClass = "input-width-md";
            Controls.Add( _tbLowerValue );

            _tbUpperValue = new DatePicker();
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
                RockControlHelper.RenderControl( this, writer, this.CssClass );
            }
        }

        /// <summary>
        /// This is where you implment the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            RegisterJavaScript();

            writer.AddAttribute( "id", this.ClientID );
            foreach ( var styleKey in this.Style.Keys )
            {
                string styleName = (string)styleKey;
                writer.AddStyleAttribute( styleName, this.Style[styleName] );
            }

            if ( !string.IsNullOrEmpty( this.CssClass ) )
            {
                writer.AddAttribute( "class", this.CssClass );
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", this.InputsClass );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _tbLowerValue.RenderControl( writer );
            writer.Write( "<div class='input-group form-control-static'> to </div>" );
            _tbUpperValue.RenderControl( writer );

            writer.RenderEndTag(); // form-control-group
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
                    return string.Format( "{0},{1}",
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
    }
}
