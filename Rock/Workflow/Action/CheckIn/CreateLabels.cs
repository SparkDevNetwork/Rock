// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Saves the selected check-in data as attendance
    /// </summary>
    [Description( "Saves the selected check-in data as attendance" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Save Attendance" )]
    public class CreateLabels : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( RockContext rockContext, Model.WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );

            var labels = new List<CheckInLabel>();

            if ( checkInState != null )
            {
                var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext );
                var globalMergeValues = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( null );

                foreach ( var family in checkInState.CheckIn.Families.Where( f => f.Selected ) )
                {
                    foreach ( var person in family.People.Where( p => p.Selected ) )
                    {
                        foreach ( var groupType in person.GroupTypes.Where( g => g.Selected ) )
                        {
                            var mergeObjects = new Dictionary<string, object>();
                            foreach ( var keyValue in globalMergeValues )
                            {
                                mergeObjects.Add( keyValue.Key, keyValue.Value );
                            }
                            mergeObjects.Add( "Person", person );
                            mergeObjects.Add( "GroupType", groupType );

                            groupType.Labels = new List<CheckInLabel>();

                            GetGroupTypeLabels( groupType.GroupType, groupType.Labels, mergeObjects );

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
                                    var group = groupType.Groups.Where( g => g.Selected ).FirstOrDefault();
                                    if ( group != null )
                                    {
                                        var location = group.Locations.Where( l => l.Selected ).FirstOrDefault();
                                        if ( location != null )
                                        {
                                            var device = location.Location.PrinterDevice;
                                            if ( device != null )
                                            {
                                                label.PrinterDeviceId = device.PrinterDeviceId;
                                            }
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
                                        var printerDevice = new DeviceService( rockContext ).Get( label.PrinterDeviceId.Value );
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

                return true;

            }

            return false;
        }

        private void GetGroupTypeLabels( GroupTypeCache groupType, List<CheckInLabel> labels, Dictionary<string, object> mergeObjects )
        {
            //groupType.LoadAttributes();
            foreach ( var attribute in groupType.Attributes.OrderBy( a => a.Value.Order ) )
            {
                if ( attribute.Value.FieldType.Guid == SystemGuid.FieldType.BINARY_FILE.AsGuid() &&
                    attribute.Value.QualifierValues.ContainsKey( "binaryFileType" ) &&
                    attribute.Value.QualifierValues["binaryFileType"].Value.Equals( SystemGuid.BinaryFiletype.CHECKIN_LABEL, StringComparison.OrdinalIgnoreCase ) )
                {
                    Guid? binaryFileGuid = groupType.GetAttributeValue( attribute.Key ).AsGuidOrNull();
                    if ( binaryFileGuid != null )
                    {
                        var labelCache = KioskLabel.Read( binaryFileGuid.Value );
                        if ( labelCache != null )
                        {
                            var checkInLabel = new CheckInLabel( labelCache, mergeObjects );
                            checkInLabel.FileGuid = binaryFileGuid.Value;
                            labels.Add( checkInLabel );
                        }
                    }
                }
            }
        }
    }
}