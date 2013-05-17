//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class ScheduleBuilder : CompositeControl, ILabeledControl
    {
        private Label _label;
        private HiddenField _iCalendarContent;
        private LinkButton _btnDialogCancelX;

        private DateTimePicker _dpStartDateTime;
        private NumberBox _tbDurationHours;
        private NumberBox _tbDurationMinutes;

        private RadioButton _radOneTime;
        private RadioButton _radReoccurring;

        private RadioButton _radSpecificDates;
        private RadioButton _radDaily;
        private RadioButton _radWeekly;
        private RadioButton _radMonthly;

        private HiddenField _hfSpecificDateListValues;

        private DatePicker _dpSpecificDate;

        //#TODO

        private LinkButton _btnSaveSchedule;
        private LinkButton _btnCancelSchedule;

        private ScriptManagerProxy _smProxy;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleBuilder"/> class.
        /// </summary>
        public ScheduleBuilder()
        {
            // control
            _label = new Label();

            // modal header
            _btnDialogCancelX = new LinkButton();

            // modal body
            _iCalendarContent = new HiddenField();

            _dpStartDateTime = new DateTimePicker();

            _tbDurationHours = new NumberBox();
            _tbDurationMinutes = new NumberBox();

            _radOneTime = new RadioButton();
            _radReoccurring = new RadioButton();

            _radSpecificDates = new RadioButton();
            _radDaily = new RadioButton();
            _radWeekly = new RadioButton();
            _radMonthly = new RadioButton();

            _hfSpecificDateListValues = new HiddenField();

            _dpSpecificDate = new DatePicker();

            // modal footer
            _btnSaveSchedule = new LinkButton();
            _btnCancelSchedule = new LinkButton();

            _smProxy = new ScriptManagerProxy();
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
        /// Gets or sets the content of the i calendar.
        /// </summary>
        /// <value>
        /// The content of the i calendar.
        /// </value>
        public string iCalendarContent
        {
            get
            {
                EnsureChildControls();
                if ( string.IsNullOrWhiteSpace( _iCalendarContent.Value ) )
                {
                    _iCalendarContent.Value = Rock.Constants.None.IdValue;
                }

                return _iCalendarContent.Value;
            }

            set
            {
                EnsureChildControls();
                _iCalendarContent.Value = value;
            }
        }

        /// <summary>
        /// Occurs when [save schedule].
        /// </summary>
        public event EventHandler SaveSchedule;

        /// <summary>
        /// Occurs when [cancel schedule].
        /// </summary>
        public event EventHandler CancelSchedule;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            RegisterJavaScript();
            var sm = ScriptManager.GetCurrent( this.Page );

            if ( sm != null )
            {
                sm.RegisterAsyncPostBackControl( _btnSaveSchedule );
                sm.RegisterAsyncPostBackControl( _btnDialogCancelX );
            }
        }

        /// <summary>
        /// Registers the java script.
        /// </summary>
        protected virtual void RegisterJavaScript()
        {
            //todo
        }

        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            _btnDialogCancelX.ClientIDMode = ClientIDMode.Static;
            _btnDialogCancelX.CssClass = "close modal-control-close";
            _btnDialogCancelX.ID = "btnDialogCancelX";
            _btnDialogCancelX.Click += btnCancelSchedule_Click;
            _btnDialogCancelX.Text = "&times;";

            _dpStartDateTime.ClientIDMode = ClientIDMode.Static;
            _dpStartDateTime.ID = "dpStartDateTime";
            _dpStartDateTime.LabelText = "Start Date / Time";

            _tbDurationHours.ClientIDMode = ClientIDMode.Static;
            _tbDurationHours.ID = "tbDurationHours";
            _tbDurationHours.CssClass = "input-mini";
            _tbDurationHours.MinimumValue = "0";
            _tbDurationHours.MaximumValue = "24";

            _tbDurationMinutes.ClientIDMode = ClientIDMode.Static;
            _tbDurationMinutes.ID = "tbDurationMinutes";
            _tbDurationMinutes.CssClass = "input-mini";
            _tbDurationMinutes.MinimumValue = "0";
            _tbDurationMinutes.MaximumValue = "59";

            _radOneTime.ClientIDMode = ClientIDMode.Static;
            _radOneTime.ID = "radOneTime";
            _radOneTime.GroupName = "ScheduleTypeGroup";
            _radOneTime.CssClass = "schedule-type";
            _radOneTime.Text = "One Time";
            _radOneTime.Attributes["data-schedule-type"] = "schedule-onetime";
            _radOneTime.Checked = true;

            _radReoccurring.ClientIDMode = ClientIDMode.Static;
            _radReoccurring.ID = "radReoccurring";
            _radReoccurring.GroupName = "ScheduleTypeGroup";
            _radReoccurring.CssClass = "schedule-type";
            _radReoccurring.Text = "Reoccurring";
            _radReoccurring.Attributes["data-schedule-type"] = "schedule-reoccurring";
            _radReoccurring.Checked = false;

            _radSpecificDates.ClientIDMode = ClientIDMode.Static;
            _radSpecificDates.ID = "radSpecificDates";
            _radSpecificDates.GroupName = "reoccurrence-pattern-radio";
            _radSpecificDates.CssClass = "reoccurrence-pattern-radio";
            _radSpecificDates.Text = "Specific Dates";
            _radSpecificDates.Attributes["data-reoccurrence-pattern"] = "reoccurrence-pattern-specific-date";
            _radSpecificDates.Checked = true;

            _radDaily.ClientIDMode = ClientIDMode.Static;
            _radDaily.ID = "radDaily";
            _radDaily.GroupName = "reoccurrence-pattern-radio";
            _radDaily.CssClass = "reoccurrence-pattern-radio";
            _radDaily.Text = "Daily";
            _radDaily.Attributes["data-reoccurrence-pattern"] = "reoccurrence-pattern-daily";
            _radDaily.Checked = false;

            _radWeekly.ClientIDMode = ClientIDMode.Static;
            _radWeekly.ID = "radWeekly";
            _radWeekly.GroupName = "reoccurrence-pattern-radio";
            _radWeekly.CssClass = "reoccurrence-pattern-radio";
            _radWeekly.Text = "Weekly";
            _radWeekly.Attributes["data-reoccurrence-pattern"] = "reoccurrence-pattern-weekly";
            _radWeekly.Checked = false;

            _radMonthly.ClientIDMode = ClientIDMode.Static;
            _radMonthly.ID = "radMonthly";
            _radMonthly.GroupName = "reoccurrence-pattern-radio";
            _radMonthly.CssClass = "reoccurrence-pattern-radio";
            _radMonthly.Text = "Monthly";
            _radMonthly.Attributes["data-reoccurrence-pattern"] = "reoccurrence-pattern-monthly";
            _radMonthly.Checked = false;

            _hfSpecificDateListValues.ClientIDMode = ClientIDMode.Static;
            _hfSpecificDateListValues.ID = "hfSpecificDateListValues";

            _dpSpecificDate.ClientIDMode = ClientIDMode.Static;
            _dpSpecificDate.ID = "dpSpecificDate";

            //#TODO#


            _btnCancelSchedule.ClientIDMode = ClientIDMode.Static;
            _btnCancelSchedule.ID = "btnCancelSchedule";
            _btnCancelSchedule.CssClass = "btn modal-control-close";
            _btnCancelSchedule.Click += btnCancelSchedule_Click;
            _btnCancelSchedule.Text = "Cancel";

            _btnSaveSchedule.ClientIDMode = ClientIDMode.Static;
            _btnSaveSchedule.ID = "btnSaveSchedule";
            _btnSaveSchedule.CssClass = "btn btn-primary modal-control-close";
            _btnSaveSchedule.Click += btnSaveSchedule_Click;
            _btnSaveSchedule.Text = "Save Schedule";

            _smProxy.Scripts.Add( new ScriptReference( "~/Scripts/Rock/Rock.schedulebuilder.js" ) );

            Controls.Add( _btnDialogCancelX );
            Controls.Add( _dpStartDateTime );
            Controls.Add( _tbDurationHours );
            Controls.Add( _tbDurationMinutes );
            Controls.Add( _radOneTime );
            Controls.Add( _radReoccurring );

            Controls.Add( _radSpecificDates );
            Controls.Add( _radDaily );
            Controls.Add( _radWeekly );
            Controls.Add( _radMonthly );

            Controls.Add( _hfSpecificDateListValues );
            Controls.Add( _dpSpecificDate);

            //#TODO#

            Controls.Add( _btnSaveSchedule );
            Controls.Add( _btnCancelSchedule );

            Controls.Add( _smProxy );
        }

        /// <summary>
        /// Handles the Click event of the btnSaveSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveSchedule_Click( object sender, EventArgs e )
        {
            if ( SaveSchedule != null )
            {
                SaveSchedule( sender, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancelSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelSchedule_Click( object sender, EventArgs e )
        {
            if ( CancelSchedule != null )
            {
                CancelSchedule( sender, e );
            }
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            string controlHtmlFragment = @"
    <a href='#myModal' role='button' class='btn btn-small' data-toggle='modal'>
        <i class='icon-calendar'></i>";

            writer.Write( controlHtmlFragment );

            _label.RenderControl( writer );

            controlHtmlFragment = @"
        Schedule
    </a>

    <style>
        .modal-control {
            /* modal is left:50%, but might be getting ignored since most of our modals are in iframe's */
            left: 25%;
            /* override modal's background-color of grey, which always gets overridden in all our other modals by the iframe's body to be white again */
            background-color: white;
            /* override modal's z-index to be bigger than modal backdrop, but less than datepicker, so that datepicker is on top  */
            z-index: 1041;
        }
    </style>

    <div id='myModal' class='modal hide fade modal-control'>
        <div class='modal-header'>";

            writer.Write( controlHtmlFragment );

            _btnDialogCancelX.RenderControl( writer );

            controlHtmlFragment = @"
            <h3>Schedule Builder</h3>
        </div>
        <div class='modal-body'>
            <div id='modal-scroll-container' class='scroll-container scroll-container-picker'>
                <div class='scrollbar'>
                    <div class='track'>
                        <div class='thumb'>
                            <div class='end'></div>
                        </div>
                    </div>
                </div>
                <div class='viewport' style='height: auto; min-height: 400px'>
                    <div class='overview'>

                        <!-- modal body -->
                        <div class='form-horizontal'>";

            // Start DateTime
            writer.Write( controlHtmlFragment );
            _dpStartDateTime.RenderControl( writer );

            // Duration
            writer.AddAttribute( "class", "control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.Write( "<span class='control-label'>Duration</span>" );
            writer.AddAttribute( "class", "controls" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _tbDurationHours.RenderControl( writer );
            writer.Write( " hrs " );
            _tbDurationMinutes.RenderControl( writer );
            writer.Write( " mins " );
            writer.RenderEndTag();
            writer.RenderEndTag();

            // One-time/Recurring Radiobuttons
            writer.AddAttribute( "class", "control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.AddAttribute( "class", "controls" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _radOneTime.RenderControl( writer );
            _radReoccurring.RenderControl( writer );
            writer.RenderEndTag();
            writer.RenderEndTag();

            // Recurrence Panel: Start
            writer.AddAttribute( "style", "display: none;" );
            writer.AddAttribute( "id", "schedule-reoccurrence-panel" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "legend-small" );
            writer.RenderBeginTag( HtmlTextWriterTag.Legend );
            writer.Write( "Reoccurrence" );
            writer.RenderEndTag();

            // OccurrencePattern Radiobuttons
            writer.AddAttribute( "class", "control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.Write( "<span class='control-label'>Occurrence Pattern</span>" );
            writer.AddAttribute( "class", "controls" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _radSpecificDates.RenderControl( writer );
            _radDaily.RenderControl( writer );
            _radWeekly.RenderControl( writer );
            _radMonthly.RenderControl( writer );
            writer.RenderEndTag();
            writer.RenderEndTag();

            // Specific Date Panel
            writer.AddAttribute( "class", "reoccurrence-pattern-type control-group controls" );
            writer.AddAttribute( "id", "reoccurrence-pattern-specific-date" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _hfSpecificDateListValues.RenderControl( writer );
            writer.Write( @"
                <ul id='lstSpecificDates'>
                </ul>
                <a class='btn btn-small' id='add-specific-date'><i class='icon-plus'></i>
                    <span> Add Date</span>
                </a>" );

            writer.AddAttribute( "id", "add-specific-date-group" );
            writer.AddStyleAttribute( "display", "none" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _dpSpecificDate.RenderControl( writer );
            writer.Write( @"
                <a class='btn btn-primary btn-mini' id='add-specific-date-ok'></i>
                    <span>OK</span>
                </a>
                <a class='btn btn-mini' id='add-specific-date-cancel'></i>
                    <span>Cancel</span>
                </a>" );

            writer.RenderEndTag();
            writer.RenderEndTag();

            // daily reoccurence panel
            // #TODO#
            // #TODO#
            // #TODO#

            // Recurrence Panel: End
            writer.RenderEndTag();

            // write out the closing divs that go after the recurrence panel
            writer.Write( @"
                            </div>
                        </div>
                    </div>
                </div>
            </div>
" );

            writer.AddAttribute( "class", "modal-footer" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _btnCancelSchedule.RenderControl( writer );
            _btnSaveSchedule.RenderControl( writer );
            writer.RenderEndTag();

            // write out the closing divs that go after the modal footer
            writer.Write( "</div>" );

            // #TODO# put this in css/less file
            writer.Write( @"
                <style type='text/css'>
                    .legend-small {
                        font-size: 14px;
                        line-height: 26px;
                    }

                    div.controls div {
                        padding-bottom: 5px;
                    }
                </style>" );

            _smProxy.RenderControl( writer );
        }
    }
}
