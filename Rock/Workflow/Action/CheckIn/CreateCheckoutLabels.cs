// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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

using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Creates Check-in Labels
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Creates Check-Out Labels" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Create Check-Out Labels" )]
    public class CreateCheckoutLabels : CheckInActionComponent
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

            if ( checkInState != null )
            {
                var family = checkInState.CheckIn.CurrentFamily;
                if ( family != null )
                {
                    var commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                    var attendanceService = new AttendanceService( rockContext );

                    foreach ( var person in family.CheckOutPeople.Where( p => p.Selected ) )
                    {
                        person.Labels = new List<CheckInLabel>();

                        var attendanceRecs = attendanceService
                            .Queryable()
                            .Where( a =>
                                person.AttendanceIds.Contains( a.Id ) &&
                                a.PersonAlias != null &&
                                a.PersonAlias.Person != null &&
                                a.Occurrence != null &&
                                a.Occurrence.Group != null &&
                                a.Occurrence.Location != null )
                            .ToList();

                        foreach ( var attendanceRec in attendanceRecs )
                        {
                            var key = string.Format( "{0}:{1}", attendanceRec.Occurrence.Group.Id, attendanceRec.Occurrence.Location.Id );
                            if ( !person.Labels.Any( l => l.LabelKey.StartsWith( key ) ) )
                            {
                                var groupType = GroupTypeCache.Get( attendanceRec.Occurrence.Group.GroupTypeId );
                                if ( groupType != null )
                                {
                                    var groupLocAttendance = attendanceRecs
                                        .Where( a =>
                                            a.Occurrence.GroupId.HasValue &&
                                            a.Occurrence.GroupId == attendanceRec.Occurrence.Group.Id &&
                                            a.Occurrence.LocationId.HasValue &&
                                            a.Occurrence.LocationId == attendanceRec.Occurrence.Location.Id )
                                        .ToList();

                                    var PrinterIPs = new Dictionary<int, string>();

                                    var groupTypeLabels = GetGroupTypeLabels( groupType );

                                    foreach ( var labelCache in groupTypeLabels.OrderBy( l => l.LabelType ).ThenBy( l => l.Order ) )
                                    {
                                        var mergeObjects = new Dictionary<string, object>();
                                        foreach ( var keyValue in commonMergeFields )
                                        {
                                            mergeObjects.Add( keyValue.Key, keyValue.Value );
                                        }
                                        mergeObjects.Add( "Attendances", groupLocAttendance );
                                        mergeObjects.Add( "Person", attendanceRec.PersonAlias.Person );
                                        mergeObjects.Add( "GroupType", groupType );
                                        mergeObjects.Add( "Group", attendanceRec.Occurrence.Group );
                                        mergeObjects.Add( "Location", attendanceRec.Occurrence.Location );

                                        //string debugInfo = mergeObjects.lavaDebugInfo();
                                        var label = new CheckInLabel( labelCache, mergeObjects, person.Person.Id );
                                        label.LabelKey = string.Format( "{0}:{1}:{2}", attendanceRec.Occurrence.Group.Id, attendanceRec.Occurrence.Location.Id, labelCache.Guid );
                                        label.FileGuid = labelCache.Guid;
                                        label.PrintFrom = checkInState.Kiosk.Device.PrintFrom;

                                        if ( label.PrintTo == PrintTo.Default )
                                        {
                                            label.PrintTo = groupType.AttendancePrintTo;
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
                                            var deviceId = attendanceRec.Occurrence.Location.PrinterDeviceId;
                                            if ( deviceId != null )
                                            {
                                                label.PrinterDeviceId = deviceId;
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

                                        person.Labels.Add( label );
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

        private List<KioskLabel> GetGroupTypeLabels( GroupTypeCache groupType )
        {
            var labels = new List<KioskLabel>();

            //groupType.LoadAttributes();
            foreach ( var attribute in groupType.Attributes.OrderBy( a => a.Value.Order ) )
            {
                if ( attribute.Value.FieldType.Guid == SystemGuid.FieldType.LABEL.AsGuid() )
                {
                    Guid? binaryFileGuid = groupType.GetAttributeValue( attribute.Key ).AsGuidOrNull();
                    if ( binaryFileGuid != null )
                    {
                        var labelCache = KioskLabel.Get( binaryFileGuid.Value );
                        labelCache.Order = attribute.Value.Order;
                        if ( labelCache != null && labelCache.LabelType == KioskLabelType.Checkout )
                        {
                            labels.Add( labelCache );
                        }
                    }
                }
            }

            return labels;
        }
    }
}