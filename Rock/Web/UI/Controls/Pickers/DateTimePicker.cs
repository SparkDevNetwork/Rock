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

using Rock;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// control to select a date time
    /// </summary>
    [ToolboxData( "<{0}:DateTimePicker runat=server></{0}:DateTimePicker>" )]
    public class DateTimePicker : CompositeControl, IRockControl
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

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimePicker"/> class.
        /// </summary>
        public DateTimePicker()
            : base()
        {
            RockControlHelper.Init( this );
        }

        #endregion

        #region Controls

        private TextBox _date;
        private TextBox _time;
        private CheckBox _cbCurrent;
        private RockTextBox _nbTimeOffset;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the selected date time. 
        /// </summary>
        /// <value>
        /// The selected date time.
        /// </value>
        public DateTime? SelectedDateTime
        {
            get
            {
                EnsureChildControls();
                DateTime? result = _date.Text.AsDateTime();
                if ( result.HasValue )
                {
                    DateTime? timeResult = _time.Text.AsDateTime();
                    if ( timeResult.HasValue )
                    {
                        result = result.Value.Add( timeResult.Value.TimeOfDay );
                    }

                    return result;
                }

                return null;
            }

            set
            {
                EnsureChildControls();
                if ( value.HasValue && value.Value != DateTime.MinValue )
                {
                    _date.Text = value.Value.ToShortDateString();
                    _time.Text = value.Value.ToShortTimeString();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether [selected date time is blank].
        /// </summary>
        /// <value>
        /// <c>true</c> if [selected date time is blank]; otherwise, <c>false</c>.
        /// </value>
        public bool SelectedDateTimeIsBlank
        {
            get
            {
                return ( !SelectedDateTime.HasValue );
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether [display current option].
        /// </summary>
        /// <value>
        /// <c>true</c> if [display current option]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayCurrentOption
        {
            get
            {
                return ViewState["DisplayCurrentOption"] as bool? ?? false;
            }

            set
            {
                ViewState["DisplayCurrentOption"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is current time offset.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is current time offset; otherwise, <c>false</c>.
        /// </value>
        public bool IsCurrentTimeOffset
        {
            get
            {
                if ( DisplayCurrentOption )
                {
                    EnsureChildControls();
                    return _cbCurrent.Checked;
                }

                return false;
            }

            set
            {
                EnsureChildControls();
                _cbCurrent.Checked = value;
            }
        }


        /// <summary>
        /// Gets or sets the current time offset minutes.
        /// </summary>
        /// <value>
        /// The current time offset minutes.
        /// </value>
        public int CurrentTimeOffsetMinutes
        {
            get
            {
                if ( DisplayCurrentOption )
                {
                    EnsureChildControls();
                    return _nbTimeOffset.Text.AsIntegerOrNull() ?? 0;
                }

                return 0;
            }

            set
            {
                EnsureChildControls();
                _nbTimeOffset.Text = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the start view.
        /// </summary>
        /// <value>
        /// The start view.
        /// </value>
        public DatePicker.StartViewOption StartView
        {
            get
            {
                return ViewState["StartView"] as DatePicker.StartViewOption? ?? DatePicker.StartViewOption.month;
            }

            set
            {
                ViewState["StartView"] = value;
            }
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _date = new TextBox();
            _date.ID =  "tbDate";
            _date.AddCssClass( "form-control js-datetime-date" );
            Controls.Add( _date );

            _time = new TextBox();
            _time.ID = "tbTime";
            _time.AddCssClass( "form-control js-datetime-time");
            Controls.Add( _time );

            _cbCurrent = new CheckBox();
            _cbCurrent.ID = "cbCurrent";
            _cbCurrent.AddCssClass( "js-current-datetime-checkbox" );
            _cbCurrent.Text = "Current Time";
            this.Controls.Add( _cbCurrent );

            _nbTimeOffset = new RockTextBox();
            _nbTimeOffset.ID = "nbTimeOffset";
            _nbTimeOffset.Help = "Enter the number of minutes after the current time to use as the date. Use a negative number to specify minutes before.";
            _nbTimeOffset.AddCssClass( "input-width-md js-current-datetime-offset" );
            _nbTimeOffset.Label = "+- Minutes";
            this.Controls.Add( _nbTimeOffset );

            RequiredFieldValidator.ControlToValidate = _date.ID;
        }


        /// <summary>
        /// Registers the javascript.
        /// </summary>
        private void RegisterJavascript()
        {
            // Get current date format and make sure it has double-lower-case month and day designators for the js date picker to use
            var dateFormat = System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern;
            dateFormat = dateFormat.Replace( "M", "m" ).Replace( "m", "mm" ).Replace( "mmmm", "mm" );
            dateFormat = dateFormat.Replace( "d", "dd" ).Replace( "dddd", "dd" );

            var script = string.Format( @"Rock.controls.dateTimePicker.initialize({{ id: '{0}', startView: {1}, format: '{2}' }});",
                this.ClientID, this.StartView.ConvertToInt(), dateFormat );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "datetime_picker-" + this.ClientID, script, true );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            RegisterJavascript();

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
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {

                writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-control-group js-datetime-picker-container" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                if ( IsCurrentTimeOffset )
                {
                    _date.Attributes["disabled"] = "true";
                    _date.AddCssClass( "aspNetDisabled" );
                    _time.Enabled = false;
                    _nbTimeOffset.Enabled = true;
                }
                else
                {
                    _date.Enabled = true;
                    _time.Enabled = true;
                    _nbTimeOffset.Enabled = false;
                }

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "input-group input-width-md" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _date.RenderControl( writer );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "input-group-addon" );
                writer.RenderBeginTag( HtmlTextWriterTag.Span );
                writer.Write( "<i class='fa fa-calendar'></i>" );
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "bootstrap-timepicker input-group input-width-md" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _time.RenderControl( writer );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "input-group-addon" );
                writer.RenderBeginTag( HtmlTextWriterTag.Span );
                writer.Write( "<i class='fa fa-clock-o'></i>" );
                writer.RenderEndTag();
                writer.RenderEndTag();

                if ( DisplayCurrentOption )
                {
                    _cbCurrent.RenderControl( writer );
                    _nbTimeOffset.RenderControl( writer );
                }

                writer.RenderEndTag();   // form-control-group
            }
        }

        #endregion

    }
}
