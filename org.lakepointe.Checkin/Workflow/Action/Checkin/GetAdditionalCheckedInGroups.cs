using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;


namespace org.lakepointe.Checkin.Workflow.Action.Checkin
{

    [ActionCategory("LPC Check-In")]
    [Description("For each family member add back groups that they are currently checked in but were added through override or " + 
        "  through another means so that they will be able to check out of them.")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata("ComponentName", "Add Group Exceptions for Checkout")]
    [BooleanField( "Load All", "By default groups are only loaded for the selected person, group type, and location.  Select this option to load groups for all the loaded people and group types." )]
    public class GetAdditionalCheckedInGroups : CheckInActionComponent
    {
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState != null )
            {
                bool loadAll = GetAttributeValue( action, "LoadAll" ).AsBoolean();

                var activeKioskGroupTypes = checkInState.Kiosk.ActiveGroupTypes( checkInState.ConfiguredGroupTypes );

                var activeKioskGroupIds = activeKioskGroupTypes
                    .Select( t => t.KioskGroups.Where( g => g.IsCheckInActive ) )
                    .SelectMany( g => g.Select( g1 => g1.Group.Id ) );


                foreach ( var family in checkInState.CheckIn.GetFamilies(true) )
                {
                    foreach ( var person in family.GetPeople(!loadAll) )
                    {   
                        var yesterday = RockDateTime.Today.AddDays( -1 );
                        var attendanceSvc = new AttendanceService( rockContext );

                        var includedPersonGroupIds = person.GroupTypes
                            .Where( gt => !gt.ExcludedByFilter )
                            .Select( t => t.Groups )
                            .SelectMany( g => g )
                            .Where( g => !g.ExcludedByFilter )
                            .Select( g => g.Group.Id )
                            .ToList();


                        var personCheckins = attendanceSvc
                            .Queryable().AsNoTracking()
                            .Where( a =>
                                a.PersonAlias != null &&
                                a.Occurrence.Group != null &&
                                a.Occurrence.Schedule != null &&
                                a.PersonAlias.PersonId == person.Person.Id &&
                                activeKioskGroupIds.Contains( a.Occurrence.GroupId ?? 0 ) &&
                                !includedPersonGroupIds.Contains( a.Occurrence.Group.Id ) &&
                                a.StartDateTime >= yesterday &&
                                !a.EndDateTime.HasValue &&
                                a.DidAttend.HasValue &&
                                a.DidAttend == true )
                            .Select( a => new
                            {
                                a.Id,
                                a.StartDateTime,
                                a.EndDateTime,
                                PersonId = a.PersonAlias.PersonId,
                                GroupTypeId = a.Occurrence.Group.GroupTypeId,
                                GroupId = a.Occurrence.GroupId,
                                LocationId = a.Occurrence.LocationId,
                                Location = a.Occurrence.Location,
                                ScheduleId = a.Occurrence.ScheduleId,
                                Schedule = a.Occurrence.Schedule,
                                IsAvailable = false
                            } )
                            .ToList();

                        foreach ( var personCheckin in personCheckins )
                        {
                            int? campusId = null;

                            if ( personCheckin.Location != null )
                            {
                                campusId = personCheckin.Location.CampusId;
                            }

                            var locationDateTime = RockDateTime.Now;
                            if ( campusId.HasValue )
                            {
                                locationDateTime = CampusCache.Get( campusId.Value )?.CurrentDateTime ?? DateTime.Now;
                            }

                            var checkOutPerson = family.CheckOutPeople.FirstOrDefault( p => p.Person.Id == person.Person.Id );

                            if ( personCheckin.Schedule.WasScheduleOrCheckInActive( locationDateTime ) )
                            {
                                var personGroupType = person.GroupTypes
                                    .Where( gt => gt.GroupType.Id == personCheckin.GroupTypeId )
                                    .FirstOrDefault();

                                var kioskGroupType = activeKioskGroupTypes
                                    .Where( t => t.GroupType.Id == personCheckin.GroupTypeId )
                                    .FirstOrDefault();


                                if ( personGroupType != null && personGroupType.GroupType.Id > 0 )
                                {
                                    personGroupType.ExcludedByFilter = false;
                                }
                                else
                                {
                                    personGroupType = new CheckInGroupType
                                    {
                                        GroupType = kioskGroupType.GroupType,
                                        ExcludedByFilter = false
                                    };
                                    person.GroupTypes.Add( personGroupType );
                                }

                                var personGroup = personGroupType.Groups
                                    .Where( g => g.Group.Id == personCheckin.GroupId )
                                    .FirstOrDefault();

                                if ( personGroup != null && personGroup.Group.Id > 0 )
                                {
                                    personGroup.ExcludedByFilter = false;
                                }
                                else
                                {
                                    var kioskGroup = kioskGroupType.KioskGroups
                                        .Where( g => g.Group.Id == personCheckin.GroupId )
                                        .FirstOrDefault();
                                    personGroup = new CheckInGroup
                                    {
                                        Group = kioskGroup.Group.Clone( false ),
                                        ExcludedByFilter = false
                                    };
                                    personGroup.Group.CopyAttributesFrom( kioskGroup.Group );
                                    personGroupType.Groups.Add( personGroup );
                                }

                                if ( checkOutPerson == null )
                                {
                                    checkOutPerson = new Rock.CheckIn.CheckOutPerson
                                    {
                                        Person = person.Person
                                    };
                                    family.CheckOutPeople.Add( checkOutPerson );
                                }

                                if ( !checkOutPerson.AttendanceIds.Any( a => a == personCheckin.Id ) )
                                {
                                    checkOutPerson.AttendanceIds.Add( personCheckin.Id );
                                }
                            }
                        }

            
                    }
                }
                return true;
            }
            return false;
        }
    }
}
