using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Attribute;

using com.ccvonline.TimeCard.Data;
using com.ccvonline.TimeCard.Model;

namespace RockWeb.Plugins.com_ccvonline.TimeCard
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "Time Card Detail" )]
    [Category( "CCV > Time Card" )]
    [Description( "Displays the details of a time card." )]
    public partial class TimeCardDetail : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
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
                ShowDetail();
            }
        }

        #endregion

        #region repeater helpers

        /// <summary>
        /// Formats the time card time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public string FormatTimeCardTime(DateTime? dateTime)
        {
            if ( dateTime.HasValue )
            {
                if (dateTime.Value.TimeOfDay == TimeSpan.Zero)
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
        /// Formats the time card hours.
        /// </summary>
        /// <param name="hours">The hours.</param>
        /// <returns></returns>
        public string FormatTimeCardHours( object hours )
        {
            if ( hours != null )
            {
                TimeSpan timeSpan = TimeSpan.FromHours( Convert.ToDouble(hours) );
                return timeSpan.TotalHours.ToString("0.00" );
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Formats the regular hours.
        /// </summary>
        /// <param name="timeCardDay">The time card day.</param>
        /// <returns></returns>
        public string FormatRegularHours( TimeCardDay timeCardDay )
        {
            return FormatTimeCardHours( timeCardDay.TimeCard.GetRegularHours().Where( a => a.TimeCardDay == timeCardDay ).FirstOrDefault().Hours );
        }

        /// <summary>
        /// Formats the overtime hours.
        /// </summary>
        /// <param name="timeCardDay">The time card day.</param>
        /// <returns></returns>
        public string FormatOvertimeHours( TimeCardDay timeCardDay )
        {
            return FormatTimeCardHours( timeCardDay.TimeCard.GetOvertimeHours().Where( a => a.TimeCardDay == timeCardDay ).FirstOrDefault().Hours );
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

        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            int timeCardId = PageParameter("TimeCardId").AsInteger();
            hfTimeCardId.Value = timeCardId.ToString();
            
            var dataContext = new TimeCardContext();
            var timeCardDayService = new TimeCardService<TimeCardDay>( dataContext );
            var timeCardService = new TimeCardService<com.ccvonline.TimeCard.Model.TimeCard>(dataContext);
            var timeCard = timeCardService.Get(timeCardId);
            if (timeCard == null)
            {
                return;
            }

            var qry = timeCardDayService.Queryable().Where( a => a.TimeCardId == timeCardId ).OrderBy( a => a.StartDateTime );
            var qryList = qry.ToList();

            // ensure 14 days
            if ( qryList.Count() < 14 )
            {
                var missingDays = new List<TimeCardDay>();
                var startDateTime = timeCard.TimeCardPayPeriod.StartDate;
                while (startDateTime < timeCard.TimeCardPayPeriod.EndDate)
                {
                    if ( !qryList.Any( a => a.StartDateTime.Date == startDateTime.Date ) )
                    {
                        var timeCardDay = new TimeCardDay();
                        timeCardDay.TimeCardId = timeCardId;
                        timeCardDay.StartDateTime = startDateTime;
                        missingDays.Add( timeCardDay );
                    }
                    startDateTime = startDateTime.AddDays( 1 );
                }

                timeCardDayService.AddRange( missingDays );
                dataContext.SaveChanges();
            }

            rptTimeCardDay.DataSource = qry.ToList();
            rptTimeCardDay.DataBind();

        }

        #endregion
    }
}