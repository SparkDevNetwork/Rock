using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using church.ccv.Hr.Data;
using church.ccv.Hr.Model;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.church_ccv.Hr
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Employee Time Card List" )]
    [Category( "CCV > Time Card" )]
    [Description( "Lists all the time cards for a specific pay period." )]

    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve all timecards, regardless of department." )]
    [LinkedPage( "Detail Page", order: 0 )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Employee Number Attribute", "Select the Person Attribute that is used for the person's employee number.", order: 1 )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Payroll Department Attribute", "Select the Person Attribute that is used for the person's payroll department to be included in the export.", order: 2 )]

    [SystemEmailField( "Approved Email", "The email to send when a time card is approved as part of the bulk-approve. If not specified, an email will not be sent.", false )]
    public partial class TimeCardEmployeeCardList : Rock.Web.UI.RockBlock
    {
        private AttributeCache employeeNumberAttribute = null;
        private AttributeCache payrollDepartmentAttribute = null;

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            BindFilter();

            bool canEdit = IsUserAuthorized( Rock.Security.Authorization.EDIT );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            // TimeCard/Time Card Pay Period is auto created when Employees create time cards
            gList.Actions.ShowAdd = false;
            gList.DataKeyNames = new string[] { "Id" };

            gList.IsDeleteEnabled = true;
            gList.GridRebind += gList_GridRebind;
            gList.Actions.ShowMergePerson = false;
            gList.Actions.ShowMergeTemplate = false;
            gList.Actions.ShowBulkUpdate = false;
            gList.PersonIdField = "PersonAlias.PersonId";

            // disable the normal Export export and add a special CSV Export button instead
            gList.Actions.ShowExcelExport = false;

            var btnExport = new LinkButton();
            btnExport.ID = "btnExport";
            btnExport.CssClass = "btn btn-default btn-sm";
            btnExport.ToolTip = "Export to CSV";
            btnExport.CausesValidation = false;

            btnExport.Text = @"
<i class='fa fa-download' title='Export'></i>
";
            btnExport.Click += btnExport_Click;

            var btnSendToPdf = new LinkButton();
            btnSendToPdf.ID = "btnSendToPdf";
            btnSendToPdf.CssClass = "btn btn-default btn-sm";
            btnSendToPdf.ToolTip = "Export to Pdf";
            btnSendToPdf.CausesValidation = false;

            btnSendToPdf.Text = @"
<i class='fa fa-file-pdf-o ' title='Send To PDF'></i>
";
            btnSendToPdf.Click += btnSendToPdf_Click;

            var btnApprove = new LinkButton();
            btnApprove.ID = "btnApprove";
            btnApprove.CssClass = "btn btn-default btn-sm";
            btnApprove.ToolTip = "Approve";
            btnApprove.CausesValidation = false;

            btnApprove.Text = @"
<i class='fa fa-check' title='Approve'></i>
";
            btnApprove.Click += btnApprove_Click;

            Panel customControls = new Panel();
            customControls.Controls.Add( btnExport );
            customControls.Controls.Add( btnSendToPdf );
            customControls.Controls.Add( btnApprove );

            gList.Actions.AddCustomActionControl( customControls );

            // Register btnExport as a PostBack control so that it triggers a Full Postback instead of a Partial.  This is so we can respond with a CVS File.
            ScriptManager.GetCurrent( this.Page ).RegisterPostBackControl( btnExport );

            // Register btnSendToPDF as a PostBack control so that it triggers a Full Postback instead of a Partial.
            ScriptManager.GetCurrent( this.Page ).RegisterPostBackControl( btnSendToPdf );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            employeeNumberAttribute = AttributeCache.Read( this.GetAttributeValue( "EmployeeNumberAttribute" ).AsGuid() );
            payrollDepartmentAttribute = AttributeCache.Read( this.GetAttributeValue( "PayrollDepartmentAttribute" ).AsGuid() );

            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlTimeCardStatusFilter.BindToEnum<TimeCardStatus>( true );

            cblPayrollWageType.Items.Clear();
            var payrollWageType = DefinedTypeCache.Read( "FCE2B549-7232-4864-A5BE-A30073AD92ED".AsGuid() );
            var selectedWageTypeValues = gfSettings.GetUserPreference( "PayrollWageTypes" ).SplitDelimitedValues().AsGuidList();
            if ( !selectedWageTypeValues.Any() )
            {
                selectedWageTypeValues = "31572E90-522C-4BCB-B858-9C7C2F8DF817,C06B81AD-A812-4204-B172-36056A745147".SplitDelimitedValues().AsGuidList();
            }

            if ( payrollWageType != null )
            {
                foreach ( var item in payrollWageType.DefinedValues )
                {
                    var listItem = new ListItem( item.Value, item.Guid.ToString() );
                    listItem.Selected = selectedWageTypeValues.Contains( item.Guid );
                    cblPayrollWageType.Items.Add( listItem );
                }
            }

            // Set the Active Status
            var itemActiveStatus = ddlTimeCardStatusFilter.Items.FindByValue( gfSettings.GetUserPreference( "TimeCardStatus" ) );
            if ( itemActiveStatus != null )
            {
                itemActiveStatus.Selected = true;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SaveUserPreference( "TimeCardStatus", "Time Card Status", ddlTimeCardStatusFilter.SelectedValue );
            gfSettings.SaveUserPreference( "PayrollWageTypes", "Payroll Wage Types", cblPayrollWageType.SelectedValues.AsDelimited(",") );
            BindGrid();
        }

        /// <summary>
        /// Gfs the settings_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfSettings_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "TimeCardStatus":
                    var timeCardStatus = e.Value.ConvertToEnumOrNull<TimeCardStatus>();
                    e.Value = timeCardStatus.HasValue ? timeCardStatus.Value.ConvertToString( true ) : string.Empty;
                    break;
                case "PayrollWageTypes":
                    var payrollWageTypes = e.Value.SplitDelimitedValues().AsGuidList();

                    e.Value = payrollWageTypes.Select( a => DefinedValueCache.Read( a ) ).Where( a => a != null ).Select( a => a.Value ).ToList().AsDelimited( ",", " and " );
                    break;

            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "TimeCardId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_Delete( object sender, RowEventArgs e )
        {
            var hrContext = new HrContext();
            var timeCardService = new TimeCardService( hrContext );
            var timeCard = timeCardService.Get( e.RowKeyId );
            if ( timeCard != null )
            {
                string errorMessage;
                if ( !timeCardService.CanDelete( timeCard, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                if ( timeCard.HasHoursEntered() )
                {
                    mdGridWarning.Show( "This time card has hours entered.", ModalAlertType.Information );
                    return;
                }

                timeCardService.Delete( timeCard );
                hrContext.SaveChanges();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnExport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void btnExport_Click( object sender, EventArgs e )
        {
            var selectedTimeCardIds = gList.SelectedKeys.Select( a => a.ToString().AsInteger() ).ToList();
            if ( !selectedTimeCardIds.Any() )
            {
                mdGridWarning.Show( "Please select at least one time card to export", ModalAlertType.Warning );
                return;
            }

            var hrContext = new HrContext();
            var timeCardService = new TimeCardService( hrContext );
            var selectedTimeCardsQry = timeCardService.Queryable().Where( a => selectedTimeCardIds.Contains( a.Id ) );
            if ( gList.SortProperty != null )
            {
                selectedTimeCardsQry = selectedTimeCardsQry.Sort( gList.SortProperty );
            }
            else
            {
                selectedTimeCardsQry = selectedTimeCardsQry.OrderBy( a => a.PersonAlias.Person.LastName ).ThenBy( a => a.PersonAlias.Person.FirstName );
            }

            // prevent them from exporting if any of the selected cards are not approved
            TimeCardStatus[] okToExportStatuses = new TimeCardStatus[] { TimeCardStatus.Approved, TimeCardStatus.Exported };
            if ( selectedTimeCardsQry.Any( a => !okToExportStatuses.Contains( a.TimeCardStatus ) ) )
            {
                mdGridWarning.Show( "Time cards must be approved before they are exported.", ModalAlertType.Warning );
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine( "Employee Name,Employee_Number,Department,RegHr,OvtHr,VacHr,Holiday,SickHr" );
            var definedValueService = new DefinedValueService( new Rock.Data.RockContext() );

            foreach ( var timeCard in selectedTimeCardsQry.ToList() )
            {
                string employeeId = string.Empty;
                string departmentId = string.Empty;
                if ( employeeNumberAttribute != null || payrollDepartmentAttribute != null )
                {
                    timeCard.PersonAlias.Person.LoadAttributes( hrContext );
                    if ( employeeNumberAttribute != null )
                    {
                        var employeeIdValue = timeCard.PersonAlias.Person.GetAttributeValue( employeeNumberAttribute.Key );

                        if ( employeeNumberAttribute.FieldType.Field is Rock.Field.Types.DefinedValueFieldType )
                        {
                            var definedValue = definedValueService.Get( employeeIdValue.AsGuid() );
                            employeeId = definedValue != null ? definedValue.Value : null;
                        }
                        else
                        {
                            employeeId = employeeNumberAttribute.FieldType.Field.FormatValue( null, employeeIdValue, employeeNumberAttribute.QualifierValues, false );
                        }
                    }

                    if ( payrollDepartmentAttribute != null )
                    {
                        var departmentIdValue = timeCard.PersonAlias.Person.GetAttributeValue( payrollDepartmentAttribute.Key );
                        if ( payrollDepartmentAttribute.FieldType.Field is Rock.Field.Types.DefinedValueFieldType )
                        {
                            var definedValue = definedValueService.Get( departmentIdValue.AsGuid() );
                            departmentId = definedValue != null ? definedValue.Value : null;
                        }
                        else
                        {
                            departmentId = payrollDepartmentAttribute.FieldType.Field.FormatValue( null, departmentIdValue, payrollDepartmentAttribute.QualifierValues, false );
                        }
                    }
                }

                string formattedLine = string.Format(
                    "\"{0}, {1}\",\"{2}\",\"{3}\",{4:N2},{5:N2},{6:N2},{7:N2},{8:N2}",
                    timeCard.PersonAlias.Person.LastName,
                    timeCard.PersonAlias.Person.FirstName,
                    employeeId,
                    departmentId,
                    timeCard.GetRegularHours().Sum( a => a.Hours ?? 0 ),
                    timeCard.GetOvertimeHours().Sum( a => a.Hours ?? 0 ),
                    timeCard.PaidVacationHours().Sum( a => a.Hours ?? 0 ),
                    timeCard.PaidHolidayHours().Sum( a => a.Hours ?? 0 ),
                    timeCard.PaidSickHours().Sum( a => a.Hours ?? 0 ) );

                sb.AppendLine( formattedLine );

                // update the status and exported date time of the selected timecards
                timeCard.TimeCardStatus = TimeCardStatus.Exported;
                timeCard.ExportedDateTime = RockDateTime.Now;

                var timeCardHistoryService = new TimeCardHistoryService( hrContext );
                var timeCardHistory = new TimeCardHistory();
                timeCardHistory.TimeCardId = timeCard.Id;
                timeCardHistory.TimeCardStatus = timeCard.TimeCardStatus;
                timeCardHistory.StatusPersonAliasId = this.CurrentPersonAliasId;
                timeCardHistory.HistoryDateTime = RockDateTime.Now;

                // NOTE: if status was already Approved, still log it as history
                timeCardHistory.Notes = string.Format( "Exported by {0}", this.CurrentPersonAlias );

                timeCardHistoryService.Add( timeCardHistory );

                hrContext.SaveChanges();
            }

            hrContext.SaveChanges();

            // send the csv export to the browser
            this.Page.EnableViewState = false;
            this.Page.Response.Clear();
            this.Page.Response.ContentType = "text/csv";
            this.Page.Response.AppendHeader( "Content-Disposition", "attachment; filename=" + string.Format( "TimeCardExport_{0}.csv", RockDateTime.Now.ToString( "MMddyyyy_HHmmss" ) ) );

            this.Page.Response.Charset = string.Empty;
            this.Page.Response.Write( sb.ToString() );
            this.Page.Response.Flush();
            this.Page.Response.End();
        }

        /// <summary>
        /// Formats the time card time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public string FormatTimeCardTime( DateTime? dateTime, bool zeroAsDash = false )
        {
            if ( dateTime.HasValue )
            {
                if ( dateTime.Value.TimeOfDay == TimeSpan.Zero && zeroAsDash )
                {
                    return "-";
                }
                else
                {
                    return dateTime.Value.ToString( "hh:mmtt" );
                }
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSendToPdf control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnSendToPdf_Click( object sender, EventArgs e )
        {
            BindGrid();
            var sbSummaryHtml = new StringBuilder();
            sbSummaryHtml.AppendLine( @"<html>
                <head>
                    <link rel='stylesheet' href='https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css' integrity='sha384-BVYiiSIFeK1dGmJRAkycuHAHRg32OmUcww7on3RYdg4Va+PmSTsz/K68vbdEjh4u' crossorigin='anonymous'>
                    <style>
                        .pagebreak { page-break-before: always; }
                    </style>
                </head>
                <body>" );

            var rockContext = new RockContext();
            var noteService = new NoteService( rockContext );

            var entityTypeIdTimeCard = EntityTypeCache.GetId<TimeCard>();

            foreach ( var timeCard in gList.DataSourceAsList.OfType<TimeCard>().ToList() )
            {
                var timeCardNotes = noteService.Queryable().Where( a => a.NoteType.EntityTypeId == ( entityTypeIdTimeCard ?? 0 ) && a.EntityId == timeCard.Id ).OrderBy( a => a.CreatedDateTime ).ToList();
                sbSummaryHtml.AppendFormat( "<h1>{0}{1}</h1>", timeCard.PersonAlias.Person.FullName, timeCardNotes.Any() ? "*" : string.Empty );
                sbSummaryHtml.AppendFormat( "<h2>Pay Period: {0}</h2>", timeCard.TimeCardPayPeriod.ToString() );
                if ( timeCard.ApprovedByPersonAlias != null )
                {
                    sbSummaryHtml.AppendFormat( "<h3>Approved By: {0}</h3>", timeCard.ApprovedByPersonAlias.Person.FullName );
                }

                // per day summary
                var workedRegularHours = timeCard.GetRegularHours();
                var workedOvertimeHours = timeCard.GetOvertimeHours();
                sbSummaryHtml.AppendLine( "<table class='table table-striped'>" );
                sbSummaryHtml.AppendLine( "<tr><th>Date</th><th>Time In</th><th>Lunch Out</th><th>Lunch In</th><th>Time Out</th><th>Hrs Worked</th><th>Overtime Hrs</th><th>Other Hours</th><th>Total Hrs</th><th>Note</th></tr>" );
                foreach ( var timeCardDay in timeCard.TimeCardDays.OrderBy( a => a.StartDateTime ) )
                {
                    sbSummaryHtml.Append( "<tr>" );
                    sbSummaryHtml.AppendFormat( "<td style='white-space:nowrap'>{0}</td>", timeCardDay.StartDateTime.ToString( "ddd MM/dd" ) );

                    // time in
                    sbSummaryHtml.AppendFormat( "<td>{0}</td>", FormatTimeCardTime( timeCardDay.StartDateTime, !timeCardDay.TotalWorkedDuration.HasValue ) );

                    // lunch out
                    sbSummaryHtml.AppendFormat( "<td>{0}</td>", FormatTimeCardTime( timeCardDay.LunchStartDateTime ) );

                    // lunch in
                    sbSummaryHtml.AppendFormat( "<td>{0}</td>", FormatTimeCardTime( timeCardDay.LunchEndDateTime ) );

                    // time out
                    sbSummaryHtml.AppendFormat( "<td>{0}</td>", FormatTimeCardTime( timeCardDay.EndDateTime ) );


                    var regularHoursForDay = workedRegularHours.Where( a => a.TimeCardDay == timeCardDay ).FirstOrDefault();
                    sbSummaryHtml.AppendFormat( "<td>{0}</td>", FormatTimeCardHours( regularHoursForDay != null ? regularHoursForDay.Hours : 0 ) );

                    var workedOvertimeHoursForDay = workedOvertimeHours.Where( a => a.TimeCardDay == timeCardDay ).FirstOrDefault();
                    sbSummaryHtml.AppendFormat( "<td>{0}</td>", FormatTimeCardHours( workedOvertimeHoursForDay != null ? workedOvertimeHoursForDay.Hours : 0 ) );

                    sbSummaryHtml.Append( "<td>" );
                    if ( !string.IsNullOrWhiteSpace( FormatTimeCardHours( timeCardDay.PaidVacationHours ) ) )
                    {
                        sbSummaryHtml.AppendFormat( "{0}(v)", FormatTimeCardHours( timeCardDay.PaidVacationHours ) );
                    }

                    if ( !string.IsNullOrWhiteSpace( FormatTimeCardHours( timeCardDay.TotalHolidayHours ) ) )
                    {
                        sbSummaryHtml.AppendFormat( "{0}(h)", FormatTimeCardHours( timeCardDay.TotalHolidayHours ) );
                    }

                    if ( !string.IsNullOrWhiteSpace( FormatTimeCardHours( timeCardDay.PaidSickHours ) ) )
                    {
                        sbSummaryHtml.AppendFormat( "{0}(s)", FormatTimeCardHours( timeCardDay.PaidSickHours ) );
                    }

                    sbSummaryHtml.Append( "</td>" );

                    decimal totalOtherHours = ( timeCardDay.PaidVacationHours ?? 0 ) + ( timeCardDay.TotalHolidayHours ?? 0 ) + ( timeCardDay.PaidSickHours ?? 0 );
                    sbSummaryHtml.AppendFormat( "<td>{0}</td>", FormatTimeCardHours( ( timeCardDay.TotalWorkedDuration ?? 0 ) + totalOtherHours ) );

                    sbSummaryHtml.AppendFormat( "<td>{0}</td>", timeCardDay.Notes );

                    sbSummaryHtml.Append( "</tr>" );

                    bool isEndOfWeek = timeCardDay.StartDateTime.DayOfWeek == DayOfWeek.Sunday;
                    if ( isEndOfWeek )
                    {
                        sbSummaryHtml.Append( "<tr>" );
                        sbSummaryHtml.AppendFormat( "<td><strong>Subtotal:</strong></td>" );

                        var workedRegularSummaryHours = timeCardDay.TimeCard.GetRegularHours()
                        .Where( a => a.TimeCardDay.StartDateTime.Date <= timeCardDay.StartDateTime.Date
                            && a.TimeCardDay.StartDateTime.Date >= timeCardDay.StartDateTime.Date.AddDays( -6 ) )
                            .Sum( a => a.Hours );
                        var workedOvertimeSummaryHours = timeCardDay.TimeCard.GetOvertimeHours()
                            .Where( a => a.TimeCardDay.StartDateTime.Date <= timeCardDay.StartDateTime.Date
                                && a.TimeCardDay.StartDateTime.Date >= timeCardDay.StartDateTime.Date.AddDays( -6 ) )
                                .Sum( a => a.Hours );
                        var otherSummaryHours = timeCardDay.TimeCard.TimeCardDays
                            .Where( a => a.StartDateTime.Date <= timeCardDay.StartDateTime.Date
                                && a.StartDateTime.Date >= timeCardDay.StartDateTime.Date.AddDays( -6 ) )
                                .Sum( a => ( a.PaidHolidayHours ?? 0 ) + ( a.PaidSickHours ?? 0 ) + ( a.PaidVacationHours ?? 0 ) + ( a.EarnedHolidayHours ?? 0 ) );

                        sbSummaryHtml.AppendFormat( "<td></td><td></td><td></td><td></td>" );
                        sbSummaryHtml.AppendFormat( "<td><strong>{0}</strong></td>", FormatTimeCardHours( workedRegularSummaryHours ) );
                        sbSummaryHtml.AppendFormat( "<td><strong>{0}</strong></td>", FormatTimeCardHours( workedOvertimeSummaryHours ) );
                        sbSummaryHtml.AppendFormat( "<td><strong>{0}</strong></td>", FormatTimeCardHours( otherSummaryHours ) );
                        sbSummaryHtml.AppendFormat( "<td><strong>{0}</strong></td>", FormatTimeCardHours( workedRegularSummaryHours + workedOvertimeSummaryHours + otherSummaryHours ) );
                        sbSummaryHtml.AppendFormat( "<td></td>" );
                        sbSummaryHtml.Append( "</tr>" );
                    }
                }

                sbSummaryHtml.AppendLine( "</table>" );

                // pay period summary
                sbSummaryHtml.AppendLine( "<div class='row'>" );
                sbSummaryHtml.AppendLine( "<div class='col-md-4'>" );
                sbSummaryHtml.AppendLine( "<table class='table'>" );
                sbSummaryHtml.AppendFormat( "<tr><td>{0}</td><td>{1:N2}</td></tr>", "Regular Hours", timeCard.GetRegularHours().Sum( a => a.Hours ?? 0 ) );
                sbSummaryHtml.AppendFormat( "<tr><td>{0}</td><td>{1:N2}</td></tr>", "Overtime Hours", timeCard.GetOvertimeHours().Sum( a => a.Hours ?? 0 ) );
                sbSummaryHtml.AppendFormat( "<tr><td>{0}</td><td>{1:N2}</td></tr>", "Vacation Hours", timeCard.PaidVacationHours().Sum( a => a.Hours ?? 0 ) );
                sbSummaryHtml.AppendFormat( "<tr><td>{0}</td><td>{1:N2}</td></tr>", "Holiday Hours", timeCard.PaidHolidayHours().Sum( a => a.Hours ?? 0 ) );
                sbSummaryHtml.AppendFormat( "<tr><td>{0}</td><td>{1:N2}</td></tr>", "Sick Hours", timeCard.PaidSickHours().Sum( a => a.Hours ?? 0 ) );

                var totalHours = timeCard.GetTotalWorkedHoursPerDay().Sum( a => a.Hours ?? 0 )
                + timeCard.PaidVacationHours().Sum( a => a.Hours ?? 0 )
                + timeCard.PaidHolidayHours().Sum( a => a.Hours ?? 0 )
                + timeCard.PaidSickHours().Sum( a => a.Hours ?? 0 );

                sbSummaryHtml.AppendFormat( "<tr><td>{0}</td><td>{1:N2}</td></tr>", "<strong>All Hours</strong>", totalHours );
                sbSummaryHtml.AppendLine( "</table>" );
                sbSummaryHtml.AppendLine( "</div>" );
                sbSummaryHtml.AppendLine( "</div>" );

                if ( timeCardNotes.Any() )
                {
                    sbSummaryHtml.AppendLine( "<h3>*Notes</h3>" );
                    foreach ( var timeCardNote in timeCardNotes )
                    {
                        sbSummaryHtml.AppendFormat(
@"<div class='row'>
<div class='col-md-12'>
({0} {1} ) - {2}
</div>
</div>
", timeCardNote.CreatedDateTime.ToString(), timeCardNote.ModifiedByPersonAlias, timeCardNote.Text );

                    }
                }

                sbSummaryHtml.AppendLine( "<hr />" );
                sbSummaryHtml.AppendLine( "<div class='pagebreak'></div>" );
            }

            sbSummaryHtml.AppendLine( "</body></html>" );
            var pdfData = TimeCardService.HtmlToPdf( sbSummaryHtml.ToString() );
            int timeCardPayPeriodId = PageParameter( "TimeCardPayPeriodId" ).AsInteger();
            var timeCardPayPeriod = new TimeCardPayPeriodService( new HrContext() ).Get( timeCardPayPeriodId );

            var fileName = string.Format( "TimeCards_PayPeriod_{0}_{1}.pdf", timeCardPayPeriod.StartDate.ToString( "yyyyMMdd" ), timeCardPayPeriod.EndDate.ToString( "yyyyMMdd" ) );

            // send the spreadsheet to the browser
            this.Page.EnableViewState = false;
            this.Page.Response.Clear();
            this.Page.Response.ContentType = "application/pdf";
            this.Page.Response.AppendHeader( "Content-Disposition", "attachment; filename=" + fileName );

            this.Page.Response.Charset = string.Empty;
            this.Page.Response.BinaryWrite( pdfData );
            this.Page.Response.Flush();
            this.Page.Response.End();
        }

        /// <summary>
        /// Formats the time card hours.
        /// </summary>
        /// <param name="hours">The hours.</param>
        /// <returns></returns>
        public string FormatTimeCardHours( decimal? hours )
        {
            if ( hours.HasValue && hours != 0 )
            {
                TimeSpan timeSpan = TimeSpan.FromHours( Convert.ToDouble( hours ) );
                return timeSpan.TotalHours.ToString( "0.##" );
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnApprove control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnApprove_Click( object sender, EventArgs e )
        {
            var selectedTimeCardIds = gList.SelectedKeys.Select( a => a.ToString().AsInteger() ).ToList();
            if ( !selectedTimeCardIds.Any() )
            {
                mdGridWarning.Show( "Please select at least one time card to approve", ModalAlertType.Warning );
                return;
            }

            var hrContext = new HrContext();
            var timeCardService = new TimeCardService( hrContext );
            var selectedTimeCardsQry = timeCardService.Queryable().Where( a => selectedTimeCardIds.Contains( a.Id ) );
            if ( gList.SortProperty != null )
            {
                selectedTimeCardsQry = selectedTimeCardsQry.Sort( gList.SortProperty );
            }
            else
            {
                selectedTimeCardsQry = selectedTimeCardsQry.OrderBy( a => a.PersonAlias.Person.LastName ).ThenBy( a => a.PersonAlias.Person.FirstName );
            }

            // prevent them from approve if any of the selected cards are not 'submitted'
            TimeCardStatus[] okToApproveStatuses = new TimeCardStatus[] { TimeCardStatus.Submitted };
            if ( selectedTimeCardsQry.Any( a => !okToApproveStatuses.Contains( a.TimeCardStatus ) ) )
            {
                mdGridWarning.Show( "Time cards must be submitted before they are approved.", ModalAlertType.Warning );
                return;
            }

            // Send an email (if specified) after timecard is marked approved
            Guid? approvedEmailTemplateGuid = GetAttributeValue( "ApprovedEmail" ).AsGuidOrNull();

            bool someNotApproved = false;

            // NOTE: BindGrid() already limits TimeCards to only ones that the current person can approve, so no need to re-check
            foreach ( var timeCard in selectedTimeCardsQry.ToList() )
            {
                if ( !timeCardService.ApproveTimeCard( timeCard.Id, this.RockPage, approvedEmailTemplateGuid ) )
                {
                    // shouldn't happen, but just in case
                    someNotApproved = true;
                }
            }

            if ( someNotApproved )
            {
                mdGridWarning.Show( "Oops, some time cards were not able to be approved. Try again.", ModalAlertType.Alert );
            }
            else
            {
                nbApproveSuccess.Visible = true;
            }

            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            // first try to use PageParameter
            int? timeCardPayPeriodId = PageParameter( "TimeCardPayPeriodId" ).AsIntegerOrNull();
            TimeCardPayPeriod timeCardPayPeriod = null;

            var hrContext = new HrContext();
            var timeCardService = new TimeCardService( hrContext );
            var timeCardPayPeriodService = new TimeCardPayPeriodService( hrContext );

            if ( !timeCardPayPeriodId.HasValue )
            {
                // if still not set, use current
                timeCardPayPeriod = timeCardPayPeriodService.GetCurrentPayPeriod();
                timeCardPayPeriodId = timeCardPayPeriod != null ? timeCardPayPeriod.Id : (int?)null;
            }

            if ( timeCardPayPeriod == null && timeCardPayPeriodId.HasValue )
            {
                timeCardPayPeriod = timeCardPayPeriodService.Get( timeCardPayPeriodId.Value );
            }

            lblPayPeriod.Text = string.Format( "Pay Period: {0}", timeCardPayPeriod );

            var qry = timeCardService.Queryable( "PersonAlias.Person" ).Where( a => a.TimeCardPayPeriodId == timeCardPayPeriodId );

            if ( !this.IsUserAuthorized( Authorization.APPROVE ) )
            {
                // unless the current user has the Global Approve role, limit cards to approvees
                var staffPersonIds = TimeCardPayPeriodService.GetApproveesForPerson( hrContext, this.CurrentPerson );
                qry = qry.Where( a => staffPersonIds.Contains( a.PersonAlias.PersonId ) );
            }

            var timeCardStatus = gfSettings.GetUserPreference( "TimeCardStatus" ).ConvertToEnumOrNull<TimeCardStatus>();
            if ( timeCardStatus.HasValue )
            {
                qry = qry.Where( a => a.TimeCardStatus == timeCardStatus );
            }

            SortProperty sortProperty = gList.SortProperty;

            if ( sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderBy( a => a.PersonAlias.Person.LastName ).ThenBy( a => a.PersonAlias.Person.FirstName );
            }

            var timeCardList = qry.ToList();

            var selectedWageTypeValues = gfSettings.GetUserPreference( "PayrollWageTypes" ).SplitDelimitedValues().AsGuidList();
            timeCardList.ForEach( a => a.PersonAlias.Person.LoadAttributes( hrContext ) );
            timeCardList = timeCardList.Where( a => selectedWageTypeValues.Contains( a.PersonAlias.Person.GetAttributeValue( "PayrollWageType" ).AsGuid() ) ).ToList();

            gList.DataSource = timeCardList;
            gList.DataBind();

            lTotalRegularHours.Text = FormatTimeCardHours( timeCardList.Sum( a => a.GetRegularHours().Sum( x => x.Hours ) ) );
            lTotalOvertimeHours.Text = FormatTimeCardHours( timeCardList.Sum( a => a.GetOvertimeHours().Sum( x => x.Hours ) ) );
            lTotalVacationHours.Text = FormatTimeCardHours( timeCardList.Sum( a => a.PaidVacationHours().Sum( x => x.Hours ) ) );
            lTotalHolidayHours.Text = FormatTimeCardHours( timeCardList.Sum( a => a.PaidHolidayHours().Sum( x => x.Hours ) ) );
            lTotalSickHours.Text = FormatTimeCardHours( timeCardList.Sum( a => a.PaidSickHours().Sum( x => x.Hours ) ) );
            lTotalAllHours.Text = FormatTimeCardHours( timeCardList.Sum( a => a.GetTotalWorkedHoursPerDay().Sum( x => x.Hours ) )
                + timeCardList.Sum( a => a.PaidVacationHours().Sum( x => x.Hours ) )
                + timeCardList.Sum( a => a.PaidHolidayHours().Sum( x => x.Hours ) )
                + timeCardList.Sum( a => a.PaidSickHours().Sum( x => x.Hours ) ) );
        }

        #endregion

        /// <summary>
        /// Handles the RowDataBound event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gList_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            var repeaterItem = e.Row;
            var timeCard = e.Row.DataItem as TimeCard;
            if ( timeCard == null )
            {
                return;
            }

            string employeeId = string.Empty;
            string departmentId = string.Empty;
            if ( employeeNumberAttribute != null || payrollDepartmentAttribute != null )
            {
                if ( timeCard.PersonAlias.Person.Attributes == null )
                {
                    timeCard.PersonAlias.Person.LoadAttributes();
                }

                if ( employeeNumberAttribute != null )
                {
                    var employeeIdValue = timeCard.PersonAlias.Person.GetAttributeValue( employeeNumberAttribute.Key );
                    employeeId = employeeNumberAttribute.FieldType.Field.FormatValue( null, employeeIdValue, employeeNumberAttribute.QualifierValues, false );
                }

                if ( payrollDepartmentAttribute != null )
                {
                    var departmentIdValue = timeCard.PersonAlias.Person.GetAttributeValue( payrollDepartmentAttribute.Key );
                    departmentId = payrollDepartmentAttribute.FieldType.Field.FormatValue( null, departmentIdValue, payrollDepartmentAttribute.QualifierValues, false );
                }
            }

            Literal lEmployeeNumber = repeaterItem.FindControl( "lEmployeeNumber" ) as Literal;
            lEmployeeNumber.Text = employeeId;
            Literal lDepartment = repeaterItem.FindControl( "lDepartment" ) as Literal;
            lDepartment.Text = departmentId;

            Badge lTimeCardStatus = repeaterItem.FindControl( "lTimeCardStatus" ) as Badge;
            lTimeCardStatus.Text = timeCard.TimeCardStatus.ConvertToString( true );
            switch ( timeCard.TimeCardStatus )
            {
                case TimeCardStatus.Approved:
                    lTimeCardStatus.BadgeType = "Success";
                    break;
                case TimeCardStatus.Submitted:
                    lTimeCardStatus.BadgeType = "Warning";
                    break;
                case TimeCardStatus.Exported:
                    lTimeCardStatus.BadgeType = "Default";
                    break;
                default:
                    lTimeCardStatus.BadgeType = "Info";
                    break;
            }

            Label lRegularHours = repeaterItem.FindControl( "lRegularHours" ) as Label;
            var regularHours = timeCard.GetRegularHours().Sum( a => a.Hours ?? 0 );
            lRegularHours.Text = regularHours.ToString( "0.##" );

            Label lOvertimeHours = repeaterItem.FindControl( "lOvertimeHours" ) as Label;
            var overtimeHours = timeCard.GetOvertimeHours().Sum( a => a.Hours ?? 0 );
            lOvertimeHours.Text = overtimeHours.ToString( "0.##" );
            lOvertimeHours.Visible = lOvertimeHours.Text.AsDecimal() != 0;

            Label lPaidVacationHours = repeaterItem.FindControl( "lPaidVacationHours" ) as Label;
            lPaidVacationHours.Text = timeCard.PaidVacationHours().Sum( a => a.Hours ?? 0 ).ToString( "0.##" );
            lPaidVacationHours.Visible = lPaidVacationHours.Text.AsDecimal() != 0;

            Label lPaidHolidayHours = repeaterItem.FindControl( "lPaidHolidayHours" ) as Label;
            lPaidHolidayHours.Text = timeCard.PaidHolidayHours().Sum( a => a.Hours ?? 0 ).ToString( "0.##" );
            lPaidHolidayHours.Visible = lPaidHolidayHours.Text.AsDecimal() != 0;

            Label lPaidSickHours = repeaterItem.FindControl( "lPaidSickHours" ) as Label;
            lPaidSickHours.Text = timeCard.PaidSickHours().Sum( a => a.Hours ?? 0 ).ToString( "0.##" );
            lPaidSickHours.Visible = lPaidSickHours.Text.AsDecimal() != 0;
        }
    }
}