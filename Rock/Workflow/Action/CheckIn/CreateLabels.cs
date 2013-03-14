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
                int labelFileTypeId = new BinaryFileTypeService()
                    .Queryable()
                    .Where( f => f.Guid == new Guid(SystemGuid.BinaryFiletype.CHECKIN_LABEL))
                    .Select( f => f.Id)
                    .FirstOrDefault();

                if (labelFileTypeId != 0)
                {
                    using ( var uow = new Rock.Data.UnitOfWorkScope() )
                    {
                        foreach ( var family in checkInState.CheckIn.Families.Where( f => f.Selected ) )
                        {
                            foreach ( var person in family.People.Where( p => p.Selected ) )
                            {
                                foreach ( var groupType in person.GroupTypes.Where( g => g.Selected ) )
                                {
                                    var mergeObjects = new Dictionary<string, object>();
                                    mergeObjects.Add( "person", person );
                                    mergeObjects.Add( "groupType", groupType );

                                    groupType.Labels = new List<CheckInLabel>();

                                    GetGroupTypeLabels( groupType.GroupType, groupType.Labels, labelFileTypeId, mergeObjects );

                                    var PrinterIPs = new Dictionary<int, string>();

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
                                            if ( location != null )
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

        private void GetGroupTypeLabels( GroupType groupType, List<CheckInLabel> labels, int labelFileTypeId, Dictionary<string, object> mergeObjects )
        {
            //groupType.LoadAttributes();
            foreach ( var attribute in groupType.Attributes )
            {
                if ( attribute.Value.FieldType.Guid == new Guid( SystemGuid.FieldType.BINARY_FILE ) &&
                    attribute.Value.QualifierValues.ContainsKey( "binaryFileType" ) &&
                    attribute.Value.QualifierValues["binaryFileType"].Value == labelFileTypeId.ToString() )
                {
                    string attributeValue = groupType.GetAttributeValue( attribute.Key );
                    if ( attributeValue != null )
                    {
                        int fileId = int.MinValue;
                        if ( int.TryParse( attributeValue, out fileId ) )
                        {
                            var labelCache = KioskCache.GetLabel( fileId );
                            if ( labelCache != null )
                            {
                                var checkInLabel = new CheckInLabel( labelCache, mergeObjects );
                                checkInLabel.FileId = fileId;
                                labels.Add( checkInLabel );
                            }
                        }
                    }
                }
            }
        }

    }


}