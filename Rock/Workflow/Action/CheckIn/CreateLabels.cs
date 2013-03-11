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
                                groupType.Labels.Add( new CheckInLabel( @"http://www.sparkdevelopmentnetwork.com/public/labels/label1.zpl" ) );
                                groupType.Labels.Add( new CheckInLabel( @"http://www.sparkdevelopmentnetwork.com/public/labels/label1.zpl" ) );
                                groupType.Labels.Add( new CheckInLabel( @"http://www.sparkdevelopmentnetwork.com/public/labels/label1.zpl" ) );

                                var mergeObjects = new Dictionary<string, object>();
                                mergeObjects.Add( "person", person );
                                mergeObjects.Add( "groupType", groupType );

                                var labelCodes = new Dictionary<string, string>();
                                labelCodes.Add( "1", "{{ person.SecurityCode }}".ResolveMergeFields( mergeObjects ) );
                                labelCodes.Add( "2", "{{ person.Person.FirstName }}".ResolveMergeFields( mergeObjects ) );
                                labelCodes.Add( "3", "{{ person.Person.LastName }}".ResolveMergeFields( mergeObjects ) );
                                labelCodes.Add( "4", "{% if person.Person.DaysToBirthday < 180 %}R{% endif %}".ResolveMergeFields( mergeObjects ) );
                                labelCodes.Add( "5", "{% for location in groupType.Locations %}{% for group in location.Groups %}{% for schedule in group.Schedules %}{{ schedule.Schedule.Name }} {% endfor %}{% endfor %}{% endfor %}".ResolveMergeFields( mergeObjects ) );

                                var PrinterIPs = new Dictionary<int, string>();

                                foreach ( var label in groupType.Labels )
                                {
                                    label.PrintFrom = checkInState.Kiosk.Device.PrintFrom;
                                    label.PrintTo = checkInState.Kiosk.Device.PrintToOverride;
                                    label.LabelKey = new Guid( "00000000-0000-0000-0000-000000000003" );

                                    if ( label.PrintTo == PrintTo.Default )
                                    {
                                        label.PrintTo = groupType.GroupType.AttendancePrintTo;
                                    }

                                    if ( label.PrintTo == PrintTo.Kiosk )
                                    {
                                        var device = checkInState.Kiosk.Device;
                                        if ( device != null )
                                        {
                                            label.PrinterDeviceId = device.PrinterDeviceId;
                                        }
                                    }
                                    else if ( label.PrintTo == PrintTo.Location )
                                    {
                                        // Should only be one
                                        var location = groupType.Locations.Where( l => l.Selected ).FirstOrDefault();
                                        if (location != null)
                                        {
                                            var device = location.Location.PrinterDevice;
                                            if ( device != null )
                                            {
                                                label.PrinterDeviceId = device.PrinterDeviceId;
                                            }
                                        }
                                    }

                                    if ( label.PrinterDeviceId.HasValue )
                                    {
                                        if ( PrinterIPs.ContainsKey( label.PrinterDeviceId.Value ) )
                                        {
                                            label.PrinterAddress = PrinterIPs[label.PrinterDeviceId.Value];
                                        }
                                        else
                                        {
                                            var printerDevice = new DeviceService().Get( label.PrinterDeviceId.Value );
                                            if ( printerDevice != null )
                                            {
                                                PrinterIPs.Add( printerDevice.Id, printerDevice.IPAddress );
                                                label.PrinterAddress = printerDevice.IPAddress;
                                            }
                                        }
                                    }

                                    label.MergeFields = labelCodes;
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