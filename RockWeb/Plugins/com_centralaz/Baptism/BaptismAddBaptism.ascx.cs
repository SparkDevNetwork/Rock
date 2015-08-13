using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

using DDay.iCal;

using com.centralaz.Baptism.Model;
using com.centralaz.Baptism.Data;

using Rock;
using Rock.Web;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Constants;
using Rock.Security;
using Rock.Web.UI;
namespace RockWeb.Plugins.com_centralaz.Baptism
{

    [DisplayName( "Baptism Add Baptism Block" )]
    [Category( "com_centralaz > Baptism" )]
    [Description( "Block for adding a baptism" )]
    [BooleanField( "Limit To Valid Service Times" )]
    public partial class BaptismAddBaptism : Rock.Web.UI.RockBlock
    {
        #region Fields

        List<DateTime> _serviceTimes;
        List<DateTime> _specialEvents;
        List<Schedule> _blackoutDates;
        Baptizee _baptizee = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
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
                if ( PageParameter( "BaptizeeId" ).AsIntegerOrNull() == null )
                {
                    btnDelete.Visible = false;
                    dtpBaptismDate.SelectedDateTime = PageParameter( "SelectedDate" ).AsDateTime();
                    lPanelTitle.Text = "Add Baptism";
                }
                else
                {
                    BindValues( PageParameter( "BaptizeeId" ).AsInteger() );
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

            int? baptizeeId = PageParameter( pageReference, "BaptizeeId" ).AsIntegerOrNull();
            if ( baptizeeId != null )
            {
                Baptizee baptizee = new BaptizeeService( new BaptismContext() ).Get( baptizeeId.Value );
                if ( baptizee != null )
                {
                    breadCrumbs.Add( new BreadCrumb( String.Format( "Edit {0}", baptizee.Person.Person.FullName ), pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Baptism", pageReference ) );
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

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_OnClick( object sender, EventArgs e )
        {
            GetBlackoutDates();
            nbErrorWarning.Visible = false;
            if ( !dtpBaptismDate.SelectedDateTime.HasValue )
            {
                nbErrorWarning.Text = "Please select a date and time";
                nbErrorWarning.Visible = true;
                return;
            }

            if ( ppBaptizee.PersonId == null )
            {
                nbErrorWarning.Text = "Please select a person to be baptized";
                nbErrorWarning.Visible = true;
                return;
            }

            if ( _blackoutDates.Any( b => b.GetCalenderEvent().DTStart.Date == dtpBaptismDate.SelectedDateTime.Value.Date ) )
            {
                nbErrorWarning.Text = "The date you selected is a blackout date";
                nbErrorWarning.Visible = true;
                return;
            }

            if ( GetAttributeValue( "LimitToValidServiceTimes" ).AsBoolean() )
            {
                GetServiceTimes();
                if ( !_serviceTimes.Any( s => ( s.DayOfWeek == dtpBaptismDate.SelectedDateTime.Value.DayOfWeek ) && ( s.TimeOfDay == dtpBaptismDate.SelectedDateTime.Value.TimeOfDay ) ) )
                {
                    if ( !_specialEvents.Any( s => s == dtpBaptismDate.SelectedDateTime.Value ) )
                    {
                        nbErrorWarning.Title = "Please enter a valid service time such as: <br>";
                        nbErrorWarning.Text = BuildInvalidServiceTimeString();
                        nbErrorWarning.Visible = true;
                        return;
                    }
                }
            }

            var changes = new List<string>();
            BaptismContext baptismContext = new BaptismContext();
            BaptizeeService baptizeeService = new BaptizeeService( baptismContext );
            RockContext rockContext = new RockContext();
            PersonService personService = new PersonService( rockContext );
            PersonAliasService personAliasService = new PersonAliasService( rockContext );
            if ( PageParameter( "BaptizeeId" ).AsIntegerOrNull() == null )
            {
                _baptizee = new Baptizee { Id = 0 };
                _baptizee.GroupId = PageParameter( "GroupId" ).AsInteger();
                History.EvaluateChange( changes, "Baptism Date/Time", "", dtpBaptismDate.SelectedDateTime.Value.ToString( "g" ) );
            }
            else
            {
                _baptizee = baptizeeService.Get( PageParameter( "BaptizeeId" ).AsInteger() );
                History.EvaluateChange( changes, "Baptism Date/Time", _baptizee.BaptismDateTime.ToString( "g" ), dtpBaptismDate.SelectedDateTime.Value.ToString( "g" ) );
            }

            _baptizee.BaptismDateTime = (DateTime)dtpBaptismDate.SelectedDateTime;

            int theId = (int)personAliasService.GetPrimaryAliasId( (int)ppBaptizee.PersonId );
            _baptizee.PersonAliasId = theId;

            if ( ppBaptizer1.PersonId != null )
            {

                theId = (int)personAliasService.GetPrimaryAliasId( (int)ppBaptizer1.PersonId );
                History.EvaluateChange( changes, "Baptizer 1", ( _baptizee.Baptizer1 != null ) ? _baptizee.Baptizer1.Person.FullName : "", ppBaptizer1.PersonName );
                _baptizee.Baptizer1AliasId = theId;
            }

            if ( ppBaptizer2.PersonId != null )
            {
                theId = (int)personAliasService.GetPrimaryAliasId( (int)ppBaptizer2.PersonId );
                History.EvaluateChange( changes, "Baptizer 2", ( _baptizee.Baptizer2 != null ) ? _baptizee.Baptizer2.Person.FullName : "", ppBaptizer2.PersonName );
                _baptizee.Baptizer2AliasId = theId;
            }

            if ( ppApprover.PersonId != null )
            {
                theId = (int)personAliasService.GetPrimaryAliasId( (int)ppApprover.PersonId );
                History.EvaluateChange( changes, "Approver", ( _baptizee.Approver != null ) ? _baptizee.Approver.Person.FullName : "", ppApprover.PersonName );
                _baptizee.ApproverAliasId = theId;
            }

            History.EvaluateChange( changes, "Confirmed", _baptizee.IsConfirmed, cbIsConfirmed.Checked );
            _baptizee.IsConfirmed = cbIsConfirmed.Checked;
            if ( _baptizee.Id.Equals( 0 ) )
            {
                baptizeeService.Add( _baptizee );
            }

            baptismContext.SaveChanges();

            // Create the history records
            if ( changes.Any() )
            {
                HistoryService.AddChanges( rockContext, typeof( Person ), com.centralaz.Baptism.SystemGuid.Category.HISTORY_PERSON_BAPTISM_CHANGES.AsGuid(),
                    (int)ppBaptizee.PersonId, changes );
                rockContext.SaveChanges();
            }

            ReturnToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_OnClick( object sender, EventArgs e )
        {
            BaptismContext baptismContext = new BaptismContext();
            BaptizeeService baptizeeService = new BaptizeeService( baptismContext );
            if ( _baptizee == null )
            {
                _baptizee = baptizeeService.Get( PageParameter( "BaptizeeId" ).AsInteger() );
            }

            if ( _baptizee != null )
            {
                baptizeeService.Delete( _baptizee );
                baptismContext.SaveChanges();

                // Create the history records
                var changes = new List<string>();

                History.EvaluateChange( changes, "Baptizer 1", ( _baptizee.Baptizer1 != null ) ? _baptizee.Baptizer1.Person.FullName : "", "" );
                History.EvaluateChange( changes, "Baptizer 2", ( _baptizee.Baptizer2 != null ) ? _baptizee.Baptizer2.Person.FullName : "", "" );
                History.EvaluateChange( changes, "Approver", ( _baptizee.Approver != null ) ? _baptizee.Approver.Person.FullName : "", "" );
                History.EvaluateChange( changes, "Confirmed", _baptizee.IsConfirmed, false );
                History.EvaluateChange( changes, "Baptism Date/Time", _baptizee.BaptismDateTime.ToString( "g" ), "" );

                RockContext rockContext = new RockContext();
                HistoryService.AddChanges( rockContext, typeof( Person ), com.centralaz.Baptism.SystemGuid.Category.HISTORY_PERSON_BAPTISM_CHANGES.AsGuid(),
                        (int)ppBaptizee.PersonId, changes );
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
        /// Grabs the service times for the campus
        /// </summary>
        protected void GetServiceTimes()
        {
            _serviceTimes = new List<DateTime>();
            _specialEvents = new List<DateTime>();
            Group group = new GroupService( new RockContext() ).Get( PageParameter( "GroupId" ).AsInteger() );
            group.LoadAttributes();

            Guid categoryguid = group.GetAttributeValue( "ServiceTimes" ).AsGuid();
            CategoryCache category = CategoryCache.Read( categoryguid );
            List<Schedule> serviceSchedules = new ScheduleService( new RockContext() ).Queryable()
                .Where( s => s.CategoryId == category.Id )
                .ToList();

            //What happens in the case of a special service
            foreach ( Schedule s in serviceSchedules )
            {
                iCalendar calendar = iCalendar.LoadFromStream( new StringReader( s.iCalendarContent ) ).First() as iCalendar;
                if ( calendar.RecurringItems.FirstOrDefault().RecurrenceRules.Count == 0 )
                {
                    var specialEvent = calendar.Events[0].Start;
                    if ( calendar.Events[0].Start != null )
                    {
                        _specialEvents.Add( specialEvent.Value );
                    }
                }
                else
                {
                    DateTime serviceTime = calendar.Events[0].Start.Value;
                    if ( serviceTime != null )
                    {
                        DayOfWeek dayOfWeek = calendar.RecurringItems.FirstOrDefault().RecurrenceRules.FirstOrDefault().ByDay[0].DayOfWeek;
                        double daysToAdd = dayOfWeek - serviceTime.DayOfWeek;
                        serviceTime = serviceTime.AddDays( daysToAdd );
                        _serviceTimes.Add( serviceTime );
                    }
                }
            }
        }

        /// <summary>
        /// Grabs the blackout dates for the campus
        /// </summary>
        protected void GetBlackoutDates()
        {
            Group group = new GroupService( new RockContext() ).Get( PageParameter( "GroupId" ).AsInteger() );
            group.LoadAttributes();
            Guid categoryguid = group.GetAttributeValue( "BlackoutDates" ).AsGuid();
            CategoryCache category = CategoryCache.Read( categoryguid );
            _blackoutDates = new ScheduleService( new RockContext() ).Queryable()
                .Where( s => s.CategoryId == category.Id )
                .ToList();
        }

        /// <summary>
        /// Returns the user to the schedule page
        /// </summary>
        protected void ReturnToParentPage()
        {
            Dictionary<string, string> dictionaryInfo = new Dictionary<string, string>();
            dictionaryInfo.Add( "GroupId", PageParameter( "GroupId" ) );
            dictionaryInfo.Add( "SelectedDate", PageParameter( "SelectedDate" ) );
            NavigateToParentPage( dictionaryInfo );
        }

        /// <summary>
        /// For existing baptisms, binds the values to the controls
        /// </summary>
        /// <param name="baptizeeId"></param>
        protected void BindValues( int baptizeeId )
        {
            Baptizee baptizee = new BaptizeeService( new BaptismContext() ).Get( baptizeeId );
            lPanelTitle.Text = String.Format( "Edit {0}", baptizee.Person.Person.FullName.FormatAsHtmlTitle() );
            dtpBaptismDate.SelectedDateTime = baptizee.BaptismDateTime;
            ppBaptizee.SetValue( baptizee.Person.Person );
            if ( baptizee.Baptizer1 != null )
            {
                ppBaptizer1.SetValue( baptizee.Baptizer1.Person );
            }

            if ( baptizee.Baptizer2 != null )
            {
                ppBaptizer2.SetValue( baptizee.Baptizer2.Person );
            }

            if ( baptizee.Approver != null )
            {
                ppApprover.SetValue( baptizee.Approver.Person );
            }

            cbIsConfirmed.Checked = baptizee.IsConfirmed;
        }

        /// <summary>
        /// If the user entered an invalid service time, and the baptism times are bound to the service times, returns a string with a list of valid service times
        /// </summary>
        /// <returns>returns an unordered list of valid service times</returns>
        protected string BuildInvalidServiceTimeString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append( "<ul>" );
            foreach ( DateTime d in _serviceTimes )
            {

                stringBuilder.AppendLine( String.Format( "<li>{0}</li>", d.ToString( "dddd h:mm tt" ) ) );
            }
            stringBuilder.Append( "</ul>" );
            return stringBuilder.ToString();
        }

        #endregion
    }
}