using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Attribute;
using Rock.Constants;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;
using Rock.Security;

namespace com.reallifeministries.Attendance
{
    /// <summary>
    /// Group Attendence Entry
    /// </summary>
    [Category( "Attendance" )]
    [Description( "Group Attendance Entry; Should be placed on a group detail page." )]
    [GroupField( "Group", "Either pick a specific group or choose <none> to have group be determined by the groupId page parameter" )]
    public partial class GroupAttendance : RockBlock
    {
        private RockContext ctx;
        private Group _group = null;
        private DefinedValueCache _inactiveStatus = null;
        private bool _canView = false;
        private bool _takesAttendance = false;
        private string _sortBy = null;
        private bool _sortDescending
        {
            get {
                return Convert.ToBoolean(ViewState["sortDescending"]);
            }
            set
            {
                ViewState["sortDescending"] = value;
            }
            
        }

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            ctx = new RockContext();

            // if this block has a specific GroupId set, use that, otherwise, determine it from the PageParameters
            Guid groupGuid = GetAttributeValue( "Group" ).AsGuid();
            int groupId = 0;

            if (groupGuid == Guid.Empty)
            {
                groupId = PageParameter( "GroupId" ).AsInteger();
            }

            if (!(groupId == 0 && groupGuid == Guid.Empty))
            {
                string key = string.Format( "Group:{0}", groupId );
                _group = RockPage.GetSharedItem( key ) as Group;
                if (_group == null)
                {
                    _group = new GroupService( ctx ).Queryable( "GroupType" )
                        .Where( g => g.Id == groupId || g.Guid == groupGuid )
                        .FirstOrDefault();
                    RockPage.SaveSharedItem( key, _group );
                }

                if (_group != null && _group.IsAuthorized( Authorization.VIEW, CurrentPerson ))
                {
                    _canView = true;
                    if (_group.GroupType.TakesAttendance)
                    {
                        _takesAttendance = true;
                    }
                }
            }

            
        }

        protected void Page_Load( object sender, EventArgs e )
        {
            HandleNotification();

            if (_canView && _takesAttendance && !IsPostBack)
            {
                BindGrid();
                dpAttendedDate.SelectedDateTime = DateTime.Now;
            }
            
        }    

        protected void BindGrid()
        {
            if (_group != null)
            {
                _inactiveStatus = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE );

                Guid[] checkinPurposeTypes = {Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_FILTER.AsGuid(), 
                                              Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid()};

                var weekendServiceGroupIds = (from g in ctx.Groups
                                              where checkinPurposeTypes.Contains( g.GroupType.GroupTypePurposeValue.Guid )
                                              select g.Id).ToList();
                

                var query = (from gm in ctx.GroupMembers
                             where gm.GroupId == _group.Id && gm.Person.RecordStatusValueId != _inactiveStatus.Id
                             orderby gm.Person.LastName, gm.Person.FirstName
                             select new
                             {
                                 PersonId = gm.PersonId,
                                 Person = gm.Person,
                                 Role = gm.GroupRole
                             });

                var wkAttendance = (from a in ctx.Attendances
                                    where query.Select(d => d.PersonId).ToList().Contains(a.PersonAlias.PersonId)
                                    where a.DidAttend == true
                                    where weekendServiceGroupIds.Contains( a.GroupId.Value )
                                    group a by a.PersonAlias.PersonId into attPerson
                                    select new
                                    {
                                        PersonId = attPerson.Key,
                                        LastWeekendAttended = attPerson.Max(a => a.StartDateTime)
                                    }).ToList();

                ;

                var grpAttendance = (from a in ctx.Attendances
                                    where query.Select( d => d.PersonId ).ToList().Contains( a.PersonAlias.PersonId )
                                    where a.GroupId.Value == _group.Id
                                    where a.DidAttend == true
                                    group a by a.PersonAlias.PersonId into attPerson
                                    select new
                                    {
                                        PersonId = attPerson.Key,
                                        LastAttendedGroup = attPerson.Max( a => a.StartDateTime )
                                    }).ToList();

                var members = query.ToList();
               
                var groupMembers = (from d in members
                                    let lastWeekendAttended = (
                                        from wa in wkAttendance 
                                        where d.PersonId == wa.PersonId
                                        select wa.LastWeekendAttended).FirstOrDefault()
                                    let lastGroupAttended = (
                                         from ga in grpAttendance
                                         where d.PersonId == ga.PersonId
                                         select ga.LastAttendedGroup).FirstOrDefault()
                                    select new
                                    {
                                        Person = d.Person,
                                        Role = d.Role,
                                        lastAttendedService = lastWeekendAttended,
                                        lastAttendedGroup = lastGroupAttended
                                    });

                var results = groupMembers;
                switch (_sortBy)
                {
                    case "attendedWeekend":
                        results = groupMembers.OrderBy( a => a.lastAttendedService ).ToList();
                        break;
                    case "attendedGroup":
                       results = groupMembers.OrderBy( a => a.lastAttendedGroup ).ToList();
                       break;
                    default:
                        results = groupMembers.OrderBy( a => a.Person.LastName ).ThenBy( a => a.Person.FirstName ).ToList();
                        break;
                }
                if(_sortDescending)
                {
                    results = results.Reverse();
                }
                rptAttendees.DataSource = results;
                rptAttendees.DataBind();
            }
        }

        public string ElaspedTime(System.DateTime? dt)
        {
            if (dt == null || dt < (new DateTime( 2000 )))
            {
                return "Never";
            }
            else
            {
                return dt.ToElapsedString( false, true );
            }
            
        }
        protected void FlashMessage( String message )
        {
            FlashMessage( message, NotificationBoxType.Info );
        }

        protected void FlashMessage( String message, NotificationBoxType type )
        {
            nbMessage.Visible = true;
            nbMessage.Text = message;
            nbMessage.NotificationBoxType = type;
        }

        protected void HandleNotification()
        {
            nbMessage.Visible = false;
            if (_canView)
            {
                if (!_takesAttendance)
                {
                    FlashMessage("Does not take attendance");                
                }
            }
            else
            {
                FlashMessage( "Your security level does not allow this action" , NotificationBoxType.Warning);
            }
        }
               
        protected void btnRecordAttendance_Click( object sender, EventArgs e )
        {
            
            if (!dpAttendedDate.SelectedDateTimeIsBlank)
            {
                var attendendPeopleIds = new List<int>();
                foreach (RepeaterItem item in rptAttendees.Items)
                {
                    var cb = item.FindControl("didAttend") as CheckBox;
                    if (cb.Checked)
                    {
                        var personId = item.FindControl( "personId" ) as HiddenField;
                        attendendPeopleIds.Add( Int32.Parse(personId.Value) );
                    }
                }

                if (attendendPeopleIds.Count > 0)
                {
                  
                    var attendanceService = new AttendanceService( ctx );
                    var peopleService = new PersonService( ctx );
                    var people = peopleService.GetByIds( attendendPeopleIds );
                    var attendances = new List<Rock.Model.Attendance>();

                    foreach (Person person in people) {
                        var attendance = new Rock.Model.Attendance();
                        attendance.PersonAlias = person.PrimaryAlias;
                        attendance.Group = _group;
                        attendance.DidAttend = true;
                        // ADD GROUP LOCATION ?
                        
                        attendance.StartDateTime = (DateTime)dpAttendedDate.SelectedDateTime;
                        if (attendance.IsValid)
                        {
                            attendanceService.Add( attendance );
                            attendances.Add( attendance );
                        }
                    }

                    ctx.SaveChanges();
                    
                    FlashMessage( string.Format(
                        "Attendance Recorded for {1} people on {0}", 
                        dpAttendedDate.SelectedDateTime.Value.ToShortTimeString(),
                        attendendPeopleIds.Count
                    ), NotificationBoxType.Success);

                    resetCheckBoxes();
                    BindGrid();
                }
                else
                {
                    FlashMessage( "Please select at least one Attendee", NotificationBoxType.Danger );
                }
            }
            else
            {
                FlashMessage( "Attended Date is required" , NotificationBoxType.Danger );
            }
        }
        protected void SortGrid(Object sender, CommandEventArgs e)
        {
            _sortBy = e.CommandName.ToString();
    
            _sortDescending = !_sortDescending;
           
            BindGrid();
        }
        protected void resetCheckBoxes()
        {
            cbCheckall.Checked = false;
            foreach (RepeaterItem i in rptAttendees.Items)
            {
                var cb = i.FindControl("didAttend") as CheckBox;
                cb.Checked = false;
            }
        }
    }
}