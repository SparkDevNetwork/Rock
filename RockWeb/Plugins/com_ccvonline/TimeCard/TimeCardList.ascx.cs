using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using com.ccvonline.TimeCard.Data;
using com.ccvonline.TimeCard.Model;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_ccvonline.TimeCard
{
    /// <summary>
    /// Lists all the Referral Agencies.
    /// </summary>
    [DisplayName( "Time Card List" )]
    [Category( "CCV > Time Card" )]
    [Description( "Lists all the time cards for a specific employee." )]

    [LinkedPage( "Detail Page" )]
    [DayOfWeekField( "Payroll Start Day", "The Day of Week that pay periods start on.  The First week of Payroll is determined by the first full week of the year for that day.", false, DayOfWeek.Monday )]
    public partial class TimeCardList : Rock.Web.UI.RockBlock
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

            // TimeCard/Time Card Pay Period is auto created based on current date
            gList.Actions.ShowAdd = false;
            gList.DataKeyNames = new string[] { "Id" };

            gList.IsDeleteEnabled = false;
            gList.Actions.AddClick += gList_AddClick;
            gList.GridRebind += gList_GridRebind;
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
        /// Handles the AddClick event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void gList_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "TimeCardId", 0 );
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
            var dataContext = new TimeCardContext();
            var timeCardService = new TimeCardService( dataContext );
            var timeCard = timeCardService.Get( e.RowKeyId );
            if ( timeCard != null )
            {
                string errorMessage;
                if ( !timeCardService.CanDelete( timeCard, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                timeCardService.Delete( timeCard );
                dataContext.SaveChanges();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the hours HTML.
        /// </summary>
        /// <param name="timeCardDay">The time card day.</param>
        /// <returns></returns>
        public string GetHoursHtml( TimeCardDay timeCardDay )
        {
            var overTimeHours = timeCardDay.TimeCard.GetOvertimeHours().Where( a => a.TimeCardDay.Id == timeCardDay.Id ).FirstOrDefault();
            return "TODO!";
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var dataContext = new TimeCardContext();
            var timeCardService = new TimeCardService( dataContext );
            var timeCardPayPeriodService = new TimeCardPayPeriodService( dataContext );
            DayOfWeek payrollStartDay = this.GetAttributeValue( "PayrollStartDay" ).ConvertToEnum<DayOfWeek>(DayOfWeek.Monday);
            var currentPayPeriod = timeCardPayPeriodService.EnsureCurrentPayPeriod( payrollStartDay );
            SortProperty sortProperty = gList.SortProperty;

            var qry = timeCardService.Queryable().Where( a => a.PersonAliasId == this.CurrentPersonAliasId );

            // ensure that employee has a timecard for the current pay period
            var currentEmployeeTimeCard = qry.Where( a => a.TimeCardPayPeriodId == currentPayPeriod.Id ).FirstOrDefault();
            if ( currentEmployeeTimeCard == null )
            {
                currentEmployeeTimeCard = new com.ccvonline.TimeCard.Model.TimeCard();
                currentEmployeeTimeCard.TimeCardPayPeriodId = currentPayPeriod.Id;
                currentEmployeeTimeCard.TimeCardStatus = TimeCardStatus.InProgress;
                currentEmployeeTimeCard.PersonAliasId = this.CurrentPersonAliasId.Value;
                timeCardService.Add( currentEmployeeTimeCard );
                dataContext.SaveChanges();
            }

            if ( sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderByDescending( a => a.TimeCardPayPeriod.StartDate );
            }

            gList.DataSource = qry.ToList();
            gList.DataBind();
        }

        #endregion
        
        protected void gList_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            var repeaterItem = e.Row;
            var timeCard = e.Row.DataItem as com.ccvonline.TimeCard.Model.TimeCard;
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