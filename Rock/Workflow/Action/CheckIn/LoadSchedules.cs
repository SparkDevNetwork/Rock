//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Loads the schedules available for each group
    /// </summary>
    [Description("Loads the schedules available for each group")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Load Schedules" )]
    [BooleanField( "Load All", "By default schedules are only loaded for the selected person, group type, location, and group.  Select this option to load schedules for all the loaded people, group types, locations, and groups." )]
    public class LoadSchedules : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( Model.WorkflowAction action, Data.IEntity entity, out List<string> errorMessages )
        {
            bool loadAll = false;
            if ( bool.TryParse( GetAttributeValue( "LoadAll" ), out loadAll ) && loadAll )
            {
                loadAll = true;
            }

            var checkInState = GetCheckInState( action, out errorMessages );
             if ( checkInState != null )
             {
                 foreach ( var family in checkInState.CheckIn.Families.Where( f => f.Selected ) )
                 {
                     foreach ( var person in family.People.Where( p => p.Selected ) )
                     {
                         foreach ( var groupType in person.GroupTypes.Where( g => g.Selected || loadAll ) )
                         {
                             var kioskGroupType = checkInState.Kiosk.KioskGroupTypes.Where( g => g.GroupType.Id == groupType.GroupType.Id ).FirstOrDefault();
                             if ( kioskGroupType != null )
                             {
                                 foreach ( var location in groupType.Locations.Where( l => l.Selected || loadAll ) )
                                 {
                                     var kioskLocation = kioskGroupType.KioskLocations.Where( l => l.Location.Id == location.Location.Id ).FirstOrDefault();
                                     if ( kioskLocation != null )
                                     {
                                         foreach ( var group in location.Groups.Where( g => g.Selected || loadAll ) )
                                         {
                                             var kioskGroup = kioskLocation.KioskGroups.Where( g => g.Group.Id == group.Group.Id ).FirstOrDefault();
                                             if ( kioskGroup != null )
                                             {
                                                 foreach ( var kioskSchedule in kioskGroup.KioskSchedules )
                                                 {
                                                     if ( !group.Schedules.Any( s => s.Schedule.Id == kioskSchedule.Schedule.Id ) )
                                                     {
                                                         var checkInSchedule = new CheckInSchedule();
                                                         checkInSchedule.Schedule = kioskSchedule.Schedule.Clone( false );
                                                         checkInSchedule.StartTime = kioskSchedule.StartTime;
                                                         group.Schedules.Add( checkInSchedule );
                                                     }
                                                 }
                                             }
                                         }
                                     }
                                 }
                             }
                         }
                     }
                 }

                 SetCheckInState( action, checkInState );
                 return true;
             }

            return false;
        }
    }
}