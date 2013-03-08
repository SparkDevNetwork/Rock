//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
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
    /// Saves the selected check-in data as attendance
    /// </summary>
    [Description("Saves the selected check-in data as attendance")]
    [Export(typeof(ActionComponent))]
    [ExportMetadata( "ComponentName", "Save Attendance" )]
    public class CreateLabels : CheckInActionComponent
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
            var checkInState = GetCheckInState( action, out errorMessages );

            var labels = new List<CheckInLabel>();
            
            if ( checkInState != null )
            {
                using ( var uow = new Rock.Data.UnitOfWorkScope() )
                {
                    foreach ( var family in checkInState.CheckIn.Families.Where( f => f.Selected ) )
                    {
                        foreach ( var person in family.People.Where( p => p.Selected ) )
                        {
                            foreach ( var groupType in person.GroupTypes.Where( g => g.Selected ) )
                            {
                                
                                groupType.Labels = new List<CheckInLabel>();
                                groupType.Labels.Add(new CheckInLabel( @"\\ccvfs1\30 Day Drop\Jon Edmiston\labeltest1.vpl" ));
                                groupType.Labels.Add(new CheckInLabel( @"\\ccvfs1\30 Day Drop\Jon Edmiston\labeltest2.vpl" ));
                                groupType.Labels.Add(new CheckInLabel( @"\\ccvfs1\30 Day Drop\Jon Edmiston\labeltest3.vpl" ));

                                var mergeObjects = new Dictionary<string, object>();
                                mergeObjects.Add( "person", person );
                                mergeObjects.Add( "groupType", groupType );

                                var labelCodes = new Dictionary<string, string>();
                                labelCodes.Add( "CF0", "{{ person.SecurityCode }}".ResolveMergeFields( mergeObjects ) );
                                labelCodes.Add( "CF1", "{{ person.Person.FirstName }}".ResolveMergeFields( mergeObjects ) );
                                labelCodes.Add( "CF2", "{{ person.Person.LastName }}".ResolveMergeFields( mergeObjects ) );
                                labelCodes.Add( "CF3", "{% for location in groupType.Locations %}{% for group in location.Groups %}{% for schedule in group.Schedules %}{{ schedule.Schedule.Name }} {% endfor %}{% endfor %}{% endfor %}".ResolveMergeFields( mergeObjects ) );

                                foreach ( var label in groupType.Labels )
                                {
                                    label.PrintFrom = checkInState.Kiosk.Device.PrintFrom;
                                    label.PrintTo = checkInState.Kiosk.Device.PrintToOverride;
                                    if ( label.PrintTo == PrintTo.Default )
                                    {
                                        label.PrintTo = groupType.GroupType.AttendancePrintTo;
                                    }

                                    if ( label.PrintTo == PrintTo.Kiosk )
                                    {
                                        label.PrinterDeviceId = checkInState.Kiosk.Device.PrinterDeviceId;
                                    }
                                    label.MergeFields = labelCodes;
                                }

                                // Should only be one
                                foreach ( var location in groupType.Locations.Where( l => l.Selected ) )
                                {
                                    foreach ( var label in groupType.Labels )
                                    {
                                        if ( label.PrintTo == PrintTo.Location )
                                        {
                                            label.PrinterDeviceId = location.Location.PrinterDeviceId;
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