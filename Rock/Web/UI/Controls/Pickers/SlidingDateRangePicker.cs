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
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Utility;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class SlidingDateRangePicker : CompositeControl, IRockControl
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
        /// Gets or sets an optional validation group to use.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get { return ViewState["ValidationGroup"] as string; }
            set { ViewState["ValidationGroup"] = value; }
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

        private DropDownList _ddlLastCurrent;
        private NumberBox _nbNumber;
        private DropDownList _ddlTimeUnitTypeSingular;
        private DropDownList _ddlTimeUnitTypePlural;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlidingDateRangePicker"/> class.
        /// </summary>
        public SlidingDateRangePicker()
            : base()
        {
            HelpBlock = new HelpBlock();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            this.Label = "Date Range";

            _ddlLastCurrent = new DropDownList();
            _ddlLastCurrent.CssClass = "form-control input-width-md";
            _ddlLastCurrent.ID = "ddlLastCurrent_" + this.ID;
            _ddlLastCurrent.SelectedIndexChanged += ddl_SelectedIndexChanged;

            _nbNumber = new NumberBox();
            _nbNumber.CssClass = "form-control input-width-sm js-number";
            _nbNumber.NumberType = ValidationDataType.Integer;
            _nbNumber.ID = "nbNumber_" + this.ID;
            _nbNumber.Text = "1";

            _ddlTimeUnitTypeSingular = new DropDownList();
            _ddlTimeUnitTypeSingular.CssClass = "form-control input-width-md js-time-units-singular";
            _ddlTimeUnitTypeSingular.ID = "ddlTimeUnitTypeSingular_" + this.ID;
            _ddlTimeUnitTypeSingular.SelectedIndexChanged += ddl_SelectedIndexChanged;

            _ddlTimeUnitTypePlural = new DropDownList();
            _ddlTimeUnitTypePlural.CssClass = "form-control input-width-md js-time-units-plural";
            _ddlTimeUnitTypePlural.ID = "ddlTimeUnitTypePlural_" + this.ID;
            _ddlTimeUnitTypePlural.SelectedIndexChanged += ddl_SelectedIndexChanged;

            Controls.Add( _ddlLastCurrent );
            Controls.Add( _nbNumber );
            Controls.Add( _ddlTimeUnitTypeSingular );
            Controls.Add( _ddlTimeUnitTypePlural );

            PopulateDropDowns();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the dropdown list controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddl_SelectedIndexChanged( object sender, EventArgs e )
        {
            EnsureChildControls();

            // "-1" = All, "0" = Last, "1" = Current
            _nbNumber.Style[HtmlTextWriterStyle.Display] = ( _ddlLastCurrent.SelectedValue == "0" ) ? string.Empty : "none";
            _ddlTimeUnitTypeSingular.Style[HtmlTextWriterStyle.Display] = ( _ddlLastCurrent.SelectedValue == "1" ) ? string.Empty : "none";
            _ddlTimeUnitTypePlural.Style[HtmlTextWriterStyle.Display] = ( _ddlLastCurrent.SelectedValue == "0" ) ? string.Empty : "none";

            if ( SelectedDateRangeChanged != null )
            {
                SelectedDateRangeChanged( this, e );
            }
        }

        /// <summary>
        /// Occurs when [selected date range changed].
        /// </summary>
        public event EventHandler SelectedDateRangeChanged;

        /// <summary>
        /// Populates the drop downs.
        /// </summary>
        private void PopulateDropDowns()
        {
            EnsureChildControls();
            _ddlLastCurrent.Items.Clear();
            _ddlLastCurrent.Items.Add( new ListItem( string.Empty, LastCurrentType.All.ConvertToInt().ToString() ) );
            _ddlLastCurrent.Items.Add( new ListItem( LastCurrentType.Last.ConvertToString(), LastCurrentType.Last.ConvertToInt().ToString() ) );
            _ddlLastCurrent.Items.Add( new ListItem( LastCurrentType.Current.ConvertToString(), LastCurrentType.Current.ConvertToInt().ToString() ) );

            _ddlTimeUnitTypeSingular.Items.Clear();
            _ddlTimeUnitTypePlural.Items.Clear();

            foreach ( var item in Enum.GetValues( typeof( TimeUnitType ) ).OfType<TimeUnitType>() )
            {
                _ddlTimeUnitTypeSingular.Items.Add( new ListItem( item.ConvertToString(), item.ConvertToInt().ToString() ) );
                _ddlTimeUnitTypePlural.Items.Add( new ListItem( item.ConvertToString().Pluralize(), item.ConvertToInt().ToString() ) );
            }

            ddl_SelectedIndexChanged( null, null );
        }

        /// <summary>
        /// 
        /// </summary>
        public enum LastCurrentType
        {
            All = -1,
            Last = 0,
            Current = 1
        }

        /// <summary>
        /// 
        /// </summary>
        public enum TimeUnitType
        {
            Hour = 0,
            Day = 1,
            Week = 2,
            Month = 3,
            Year = 4
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
            bool needsAutoPostBack = SelectedDateRangeChanged != null;
            _ddlLastCurrent.AutoPostBack = needsAutoPostBack;
            _ddlTimeUnitTypeSingular.AutoPostBack = needsAutoPostBack;
            _ddlTimeUnitTypePlural.AutoPostBack = needsAutoPostBack;

            writer.AddAttribute( "class", "form-control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _ddlLastCurrent.RenderControl( writer );
            _nbNumber.RenderControl( writer );
            _ddlTimeUnitTypeSingular.RenderControl( writer );
            _ddlTimeUnitTypePlural.RenderControl( writer );

            writer.RenderEndTag();

            RegisterJavaScript();
        }

        /// <summary>
        /// Registers the java script.
        /// </summary>
        protected virtual void RegisterJavaScript()
        {
            string scriptFormat = @"
                $('#{0}').on('change', function(a,b) {{
                    if ($('#{0}').val() == '1') {{
                        // current ...
                        $('#{0}').siblings('.js-number').hide();
                        $('#{0}').siblings('.js-time-units-singular').show();
                        $('#{0}').siblings('.js-time-units-plural').hide();
                    }}
                    else if ($('#{0}').val() == '0') {{
                        // last x ...
                        $('#{0}').siblings('.js-number').show();    
                        $('#{0}').siblings('.js-time-units-singular').hide();
                        $('#{0}').siblings('.js-time-units-plural').show();
                    }}
                    else {{
                        // all    
                        $('#{0}').siblings('.js-number').hide();
                        $('#{0}').siblings('.js-time-units-singular').hide();
                        $('#{0}').siblings('.js-time-units-plural').hide();
                    }}
                }});
";

            ScriptManager.RegisterStartupScript( this, this.GetType(), "sliding-date-range-script", string.Format( scriptFormat, _ddlLastCurrent.ClientID ), true );
        }

        /// <summary>
        /// Gets or sets the select of Last or Current
        /// </summary>
        /// <value>
        /// The last or current.
        /// </value>
        public LastCurrentType LastOrCurrent
        {
            get
            {
                EnsureChildControls();
                return _ddlLastCurrent.SelectedValueAsEnum<LastCurrentType>();
            }

            set
            {
                EnsureChildControls();
                _ddlLastCurrent.SelectedValue = value.ConvertToInt().ToString();
                ddl_SelectedIndexChanged( null, null );
            }
        }

        /// <summary>
        /// Gets or sets the number of time units (x Hours, x Days, etc) depending on TimeUnit selection
        /// </summary>
        /// <value>
        /// The number of time units.
        /// </value>
        public int NumberOfTimeUnits
        {
            get
            {
                EnsureChildControls();
                return _nbNumber.Text.AsIntegerOrNull() ?? 1;
            }

            set
            {
                EnsureChildControls();
                _nbNumber.Text = ( value <= 0 ? 1 : value ).ToString();
            }
        }

        /// <summary>
        /// Gets or sets the time unit (Hour, Day, Year, etc)
        /// </summary>
        /// <value>
        /// The time unit.
        /// </value>
        public TimeUnitType TimeUnit
        {
            get
            {
                EnsureChildControls();
                if ( LastOrCurrent == LastCurrentType.Last )
                {
                    return _ddlTimeUnitTypePlural.SelectedValueAsEnum<TimeUnitType>();
                }
                else
                {
                    return _ddlTimeUnitTypeSingular.SelectedValueAsEnum<TimeUnitType>();
                }
            }

            set
            {
                EnsureChildControls();
                _ddlTimeUnitTypePlural.SelectedValue = value.ConvertToInt().ToString();
                _ddlTimeUnitTypeSingular.SelectedValue = value.ConvertToInt().ToString();
            }
        }

        /// <summary>
        /// Gets or sets the LastOrCurrent, NumberOfTimeUnits, and TimeUnit values by specifying pipe-delimited values
        /// </summary>
        /// <value>
        /// The delimited values.
        /// </value>
        public string DelimitedValues
        {
            get
            {
                return string.Format( "{0}|{1}|{2}", this.LastOrCurrent, this.NumberOfTimeUnits, this.TimeUnit );
            }

            set
            {
                string[] splitValues = ( value ?? string.Empty ).Split( '|' );
                if ( splitValues.Length == 3 )
                {
                    this.LastOrCurrent = splitValues[0].ConvertToEnum<LastCurrentType>();
                    this.NumberOfTimeUnits = splitValues[1].AsIntegerOrNull() ?? 1;
                    this.TimeUnit = splitValues[2].ConvertToEnum<TimeUnitType>();
                }
            }
        }

        /// <summary>
        /// Formats the delimited values.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string FormatDelimitedValues( string value )
        {
            string[] splitValues = ( value ?? string.Empty ).Split( '|' );
            string result = string.Empty;
            if ( splitValues.Length == 3 )
            {
                var lastOrCurrent = splitValues[0].ConvertToEnum<LastCurrentType>();
                var numberOfTimeUnits = splitValues[1].AsIntegerOrNull() ?? 1;
                var timeUnit = splitValues[2].ConvertToEnum<TimeUnitType>();
                if ( lastOrCurrent == LastCurrentType.Current )
                {
                    return string.Format( "{0} {1}", lastOrCurrent.ConvertToString(), timeUnit.ConvertToString() );
                }
                else
                {
                    return string.Format( "{0} {1} {2}", lastOrCurrent.ConvertToString(), numberOfTimeUnits, timeUnit.ConvertToString() );
                }
            }

            return result;
        }

        /// <summary>
        /// Calculates the date range from delimited values.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static DateRange CalculateDateRangeFromDelimitedValues( string value )
        {
            string[] splitValues = ( value ?? "1||4" ).Split( '|' );
            DateRange result = new DateRange();
            if ( splitValues.Length == 3 )
            {
                var lastOrCurrent = splitValues[0].ConvertToEnum<LastCurrentType>();
                var numberOfTimeUnits = splitValues[1].AsIntegerOrNull() ?? 1;
                var timeUnit = splitValues[2].ConvertToEnum<TimeUnitType>();
                DateTime currentDateTime = RockDateTime.Now;
                if ( lastOrCurrent == LastCurrentType.Current )
                {
                    switch ( timeUnit )
                    {
                        case TimeUnitType.Hour:
                            result.Start = new DateTime( currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0 );
                            result.End = result.Start.Value.AddHours( 1 );
                            break;

                        case TimeUnitType.Day:
                            result.Start = currentDateTime.Date;
                            result.End = result.Start.Value.AddDays( 1 );
                            break;

                        case TimeUnitType.Week:

                            // from http://stackoverflow.com/a/38064/1755417
                            int diff = currentDateTime.DayOfWeek - DayOfWeek.Sunday;
                            if ( diff > 0 )
                            {
                                diff += 7;
                            }

                            result.Start = currentDateTime.AddDays( -1 * diff ).Date;
                            result.End = result.Start.Value.AddDays( 7 );
                            break;

                        case TimeUnitType.Month:
                            result.Start = new DateTime( currentDateTime.Year, currentDateTime.Month, 1 );
                            result.End = result.Start.Value.AddMonths( 1 );
                            break;

                        case TimeUnitType.Year:
                            result.Start = new DateTime( currentDateTime.Year, 1, 1 );
                            result.End = new DateTime( currentDateTime.Year + 1, 1, 1 );
                            break;
                    }
                }
                else if ( lastOrCurrent == LastCurrentType.Last )
                {
                    // Last X Days/Hours, 
                    int addCount = numberOfTimeUnits;
                    switch ( timeUnit )
                    {
                        case TimeUnitType.Hour:
                            result.End = new DateTime( currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0 ).AddHours( 1 );
                            result.Start = result.End.Value.AddHours( -addCount );
                            break;

                        case TimeUnitType.Day:
                            result.End = currentDateTime.Date.AddDays( 1 );
                            result.Start = result.End.Value.AddDays( -addCount );
                            break;

                        case TimeUnitType.Week:
                            // from http://stackoverflow.com/a/38064/1755417
                            int diff = currentDateTime.DayOfWeek - DayOfWeek.Sunday;
                            if ( diff < 0 )
                            {
                                diff += 7;
                            }

                            result.End = currentDateTime.AddDays( -1 * diff ).Date.AddDays( 7 );
                            result.Start = result.End.Value.AddDays( -addCount * 7 );
                            break;

                        case TimeUnitType.Month:
                            result.End = new DateTime( currentDateTime.Year, currentDateTime.Month, 1 ).AddMonths( 1 );
                            result.Start = result.End.Value.AddMonths( -addCount );
                            break;

                        case TimeUnitType.Year:
                            result.End = new DateTime( currentDateTime.Year, 1, 1 ).AddYears( 1 );
                            result.Start = result.End.Value.AddYears( -addCount );
                            break;
                    }
                }
            }

            return result;
        }
    }
}