using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;
using Rock.Web;

namespace RockWeb.Plugins.com_centralaz.Baptism
{

    [DisplayName( "Baptism Add Blackout Date Block" )]
    [Category( "com_centralaz > Baptism" )]
    [Description( "Block for adding blackout dates to baptism schedules" )]
    public partial class BaptismAddBlackoutDate : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        protected List<Schedule> _blackoutDates;
        protected Schedule _blackoutDate;

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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
                if ( PageParameter( "BlackoutId" ).AsIntegerOrNull() == null )
                {
                    dpBlackOutDate.SelectedDate = PageParameter( "SelectedDate" ).AsDateTime();
                    btnDelete.Visible = false;
                    lPanelTitle.Text = "Add Blackout Date";

                }
                else
                {
                    BindValues( PageParameter( "BlackoutId" ).AsInteger() );
                }
            }
        }
        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? blackoutId = PageParameter( pageReference, "BlackoutId" ).AsIntegerOrNull();
            if ( blackoutId != null )
            {
                Schedule blackout = new ScheduleService( new RockContext() ).Get( blackoutId.Value );
                if ( blackout != null )
                {
                    breadCrumbs.Add( new BreadCrumb( "Edit Blackout Date", pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Blackout Date", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_OnClick( object sender, EventArgs e )
        {
            nbNotification.Visible = false;
            //baptisms exist for blackout date
            //blackout date already exists
            GetBlackoutDates();
            if ( _blackoutDates.Any( b => ( b.GetCalenderEvent().DTStart.Date == dpBlackOutDate.SelectedDate.Value.Date ) && ( b.CategoryId == GetCategoryId() ) && ( b.Id != PageParameter( "BlackoutId" ).AsIntegerOrNull() ) ) )
            {
                nbNotification.Text = "Blackout already exists for that date";
                nbNotification.Visible = true;
                return;
            }
            //check that group is valid
            int categoryId = GetCategoryId();
            if ( categoryId == -1 )
            {
                nbNotification.Text = "Error loading campus";
                nbNotification.Visible = true;
                return;
            }
            //save blackout date to db
            RockContext rockContext = new RockContext();
            ScheduleService scheduleService = new ScheduleService( rockContext );
            if ( PageParameter( "BlackoutId" ).AsIntegerOrNull() == null )
            {
                _blackoutDate = new Schedule { Id = 0 };
                _blackoutDate.CategoryId = categoryId;
            }
            else
            {
                _blackoutDate = scheduleService.Get( PageParameter( "BlackoutId" ).AsInteger() );
            }
            iCalendar calendar = new iCalendar();
            DDay.iCal.IDateTime datetime = new iCalDateTime();
            Event theEvent = new Event();
            calendar.Events.Add( theEvent );
            var x1 = theEvent.DTStart;
            datetime.Value = dpBlackOutDate.SelectedDate.Value;
            _blackoutDate.Name = string.Format( "{0} blackout", dpBlackOutDate.SelectedDate.Value.ToShortDateString() );

            calendar.Events[0].DTStart = datetime;
            iCalendarSerializer calSerializer = new iCalendarSerializer( calendar );
            _blackoutDate.iCalendarContent = calSerializer.SerializeToString();
            _blackoutDate.Description = tbDescription.Text;
            if ( _blackoutDate.Id.Equals( 0 ) )
            {
                scheduleService.Add( _blackoutDate );

            }
            rockContext.SaveChanges();
            ReturnToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_OnClick( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            ScheduleService scheduleService = new ScheduleService( rockContext );
            if ( _blackoutDate == null )
            {
                _blackoutDate = scheduleService.Get( PageParameter( "BlackoutId" ).AsInteger() );
            }
            if ( _blackoutDate != null )
            {
                scheduleService.Delete( _blackoutDate );
                rockContext.SaveChanges();
            }
            ReturnToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_OnClick( object sender, EventArgs e )
        {
            ReturnToParentPage();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Grabs all the blackout dates for the campus
        /// </summary>
        protected void GetBlackoutDates()
        {
            int categoryId = GetCategoryId();
            _blackoutDates = new ScheduleService( new RockContext() ).Queryable()
                .Where( s => s.CategoryId == categoryId )
                .ToList();
        }

        /// <summary>
        /// Returns the user to the Campus Schedule page
        /// </summary>
        protected void ReturnToParentPage()
        {
            Dictionary<string, string> dictionaryInfo = new Dictionary<string, string>();
            dictionaryInfo.Add( "GroupId", PageParameter( "GroupId" ) );
            dictionaryInfo.Add( "SelectedDate", dpBlackOutDate.SelectedDate.Value.ToShortDateString() );
            NavigateToParentPage( dictionaryInfo );
        }

        /// <summary>
        /// Binds the values for an existing blackout date to the controls
        /// </summary>
        /// <param name="blackoutId"></param>
        protected void BindValues( int blackoutId )
        {
            Schedule blackoutDate = new ScheduleService( new RockContext() ).Get( blackoutId );
            dpBlackOutDate.SelectedDate = blackoutDate.GetCalenderEvent().DTStart.Date;
            tbDescription.Text = blackoutDate.Description;
            lPanelTitle.Text = "Edit Blackout Date";
        }

        /// <summary>
        /// Grabs the category Id for the campus's blackout date collection
        /// </summary>
        /// <returns></returns>
        protected int GetCategoryId()
        {
            Group group = new GroupService( new RockContext() ).Get( PageParameter( "GroupId" ).AsInteger() );
            group.LoadAttributes();
            Guid categoryguid = group.GetAttributeValue( "BlackoutDates" ).AsGuid();
            CategoryCache category = CategoryCache.Read( categoryguid );
            if ( category == null )
            {
                return -1;
            }
            else
            {
                return category.Id;
            }
        }

        #endregion
    }
}