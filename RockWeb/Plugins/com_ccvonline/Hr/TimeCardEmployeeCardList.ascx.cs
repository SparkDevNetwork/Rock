using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using com.ccvonline.Hr.Data;
using com.ccvonline.Hr.Model;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_ccvonline.Hr
{
    /// <summary>
    /// Lists all the Referral Agencies.
    /// </summary>
    [DisplayName( "Employee Time Card List" )]
    [Category( "CCV > Time Card" )]
    [Description( "Lists all the time cards for a specific pay period." )]

    [LinkedPage( "Detail Page" )]
    [BooleanField( "Limit To My Staff", "Enable this to only show people that are in the department that you lead.", true )]
    public partial class TimeCardEmployeeCardList : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            bool canEdit = IsUserAuthorized( Rock.Security.Authorization.EDIT );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            // TimeCard/Time Card Pay Period is auto created when Employees create time cards
            gList.Actions.ShowAdd = false;
            gList.DataKeyNames = new string[] { "Id" };

            gList.IsDeleteEnabled = true;
            gList.GridRebind += gList_GridRebind;

            // disable the normal Export export and add a special CSV Export button instead
            gList.Actions.ShowExcelExport = false;

            var btnExport = new LinkButton();
            btnExport.ID = "btnExport";
            btnExport.CssClass = "btn btn-default btn-sm";
            btnExport.ToolTip = "Export to CSV";
            btnExport.CausesValidation = false;
            
            btnExport.Text = @"
<i class='fa fa-file-text-o'></i>
Export
";
            btnExport.Click += btnExport_Click;
            
            // Register btnExport as a PostBack control so that it triggers a Full Postback instead of a Partial.  This is so we can respond with a CVS File.
            ScriptManager.GetCurrent( this.Page ).RegisterPostBackControl( btnExport );
            
            gList.Actions.AddCustomActionControl( btnExport );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            //
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

            StringBuilder sb = new StringBuilder();
            // TODO: What about Worked Holiday Hours??
            sb.AppendLine( "Employee Name,Employee_Number,Department,RegHr,OvtHr,VacHr,Holiday,SickHr" );
            var timeCardService = new TimeCardService( new HrContext() );
            var selectedTimeCardsQry = timeCardService.Queryable().Where( a => selectedTimeCardIds.Contains( a.Id ) );
            if ( gList.SortProperty != null )
            {
                selectedTimeCardsQry = selectedTimeCardsQry.Sort( gList.SortProperty );
            }
            else
            {
                selectedTimeCardsQry = selectedTimeCardsQry.OrderBy( a => a.PersonAlias.Person.LastName ).ThenBy( a => a.PersonAlias.Person.FirstName );
            }

            foreach ( var timeCard in selectedTimeCardsQry.ToList() )
            {
                string employeeId = "TODO";
                string departmentId = "TODO";
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
            }

            // send the csv export to the browser
            this.Page.EnableViewState = false;
            this.Page.Response.Clear();
            this.Page.Response.ContentType = "text/csv";
            this.Page.Response.AppendHeader( "Content-Disposition", "attachment; filename=" + string.Format( "TimeCardExport_{0}.csv", RockDateTime.Now.ToString( "MMddyyyy_HHmmss" ) ) );
            
            this.Page.Response.Charset = "";
            this.Page.Response.Write( sb.ToString() );
            this.Page.Response.Flush();
            this.Page.Response.End();
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

            var limitToMyStaff = this.GetAttributeValue( "LimitToMyStaff" ).AsBooleanOrNull() ?? true;
            if ( limitToMyStaff )
            {
                // TODO use Rock SystemGuids for these after next merge from core
                string GROUPROLE_ORGANIZATION_UNIT_LEADER = "8438D6C5-DB92-4C99-947B-60E9100F223D";
                string GROUPROLE_ORGANIZATION_UNIT_STAFF = "17E516FC-76A4-4BF4-9B6F-0F859B13F563";

                Guid orgUnitGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_ORGANIZATION_UNIT.AsGuid();
                Guid groupLeaderGuid = GROUPROLE_ORGANIZATION_UNIT_LEADER.AsGuid();
                Guid groupStaffGuid = GROUPROLE_ORGANIZATION_UNIT_STAFF.AsGuid();

                // figure out what department the person is a leader in (hopefully at most one department, but we'll deal with multiple just in case)
                var groupMemberService = new GroupMemberService( hrContext );
                var qryPersonDeptLeaderGroup = groupMemberService.Queryable().Where( a => a.PersonId == this.CurrentPersonId ).Where( a => a.Group.GroupType.Guid == orgUnitGroupTypeGuid && a.GroupRole.Guid == groupLeaderGuid ).Select( a => a.Group );

                // get a List vs a Qry since GroupMember and TimeCard use different DbContexts
                var staffPersonIds = groupMemberService.Queryable()
                    .Where( a => qryPersonDeptLeaderGroup.Any( x => x.Id == a.GroupId ) )
                    .Where( a => a.GroupRole.Guid == groupStaffGuid )
                    .Select( a => a.PersonId );

                qry = qry.Where( a => staffPersonIds.Contains( a.PersonAlias.PersonId ) );
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

            gList.DataSource = qry.ToList();
            gList.DataBind();
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

            Label lRegularHours = repeaterItem.FindControl( "lRegularHours" ) as Label;
            var regularHours = timeCard.GetRegularHours().Sum( a => a.Hours ?? 0 );
            lRegularHours.Text = regularHours.ToString( "0.##" );

            Label lOvertimeHours = repeaterItem.FindControl( "lOvertimeHours" ) as Label;
            var overtimeHours = timeCard.GetOvertimeHours().Sum( a => a.Hours ?? 0 );
            lOvertimeHours.Text = overtimeHours.ToString( "0.##" );
            lOvertimeHours.Visible = lOvertimeHours.Text.AsDecimal() != 0;

            Label lWorkedHolidayHours = repeaterItem.FindControl( "lWorkedHolidayHours" ) as Label;
            var workedHolidayHours = timeCard.GetWorkedHolidayHours().Sum( a => a.Hours ?? 0 );
            lWorkedHolidayHours.Text = workedHolidayHours.ToString( "0.##" );
            lWorkedHolidayHours.Visible = lWorkedHolidayHours.Text.AsDecimal() != 0;

            Label lPaidVacationHours = repeaterItem.FindControl( "lPaidVacationHours" ) as Label;
            lPaidVacationHours.Text = timeCard.PaidVacationHours().Sum( a => a.Hours ?? 0 ).ToString( "0.##" );
            lPaidVacationHours.Visible = lPaidVacationHours.Text.AsDecimal() != 0;

            Label lPaidHolidayHours = repeaterItem.FindControl( "lPaidHolidayHours" ) as Label;
            lPaidHolidayHours.Text = timeCard.PaidVacationHours().Sum( a => a.Hours ?? 0 ).ToString( "0.##" );
            lPaidHolidayHours.Visible = lPaidHolidayHours.Text.AsDecimal() != 0;

            Label lPaidSickHours = repeaterItem.FindControl( "lPaidSickHours" ) as Label;
            lPaidSickHours.Text = timeCard.PaidSickHours().Sum( a => a.Hours ?? 0 ).ToString( "0.##" );
            lPaidSickHours.Visible = lPaidSickHours.Text.AsDecimal() != 0;
        }
    }
}