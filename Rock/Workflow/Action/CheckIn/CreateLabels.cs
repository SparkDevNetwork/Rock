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
using System.Data.Entity;
using System.Linq;

using Rock.CheckIn;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Creates Check-in Labels
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Creates Check-in Labels" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Create Labels" )]
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

            if ( checkInState != null )
            {
                var family = checkInState.CheckIn.CurrentFamily;
                if ( family != null )
                {
                    var commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                    var groupMemberService = new GroupMemberService( rockContext );

                    var familyLabelsAdded = new List<Guid>();

                    var people = family.GetPeople( true );
                    foreach ( var person in people )
                    {
                        var personGroupTypes = person.GetGroupTypes( true );
                        var groupTypes = new List<CheckInGroupType>();

                        // Get Primary area group types first
                        personGroupTypes.Where( t => checkInState.ConfiguredGroupTypes.Contains( t.GroupType.Id ) ).ToList().ForEach( t => groupTypes.Add( t ) );

                        // Then get additional areas
                        personGroupTypes.Where( t => !checkInState.ConfiguredGroupTypes.Contains( t.GroupType.Id ) ).ToList().ForEach( t => groupTypes.Add( t ) );

                        var personLabels = GetLabels( person.Person, new List<KioskLabel>() );

                        var personLabelsAdded = new List<Guid>();

                        foreach ( var groupType in groupTypes )
                        {
                            groupType.Labels = new List<CheckInLabel>();

                            var groupTypeLabels = GetLabels( groupType.GroupType, personLabels );

                            var PrinterIPs = new Dictionary<int, string>();

                            foreach ( var group in groupType.GetGroups( true ) )
                            {
                                var groupLabels = GetLabels( group.Group, groupTypeLabels );

                                foreach ( var location in group.GetLocations( true ) )
                                {
                                    var locationLabels = GetLabels( location.Location, groupLabels );

                                    foreach ( var labelCache in locationLabels.OrderBy( l => l.LabelType ).ThenBy( l => l.Order ) )
                                    {
                                        person.SetOptions( labelCache );

                                        if ( labelCache.LabelType == KioskLabelType.Family )
                                        {
                                            if ( familyLabelsAdded.Contains( labelCache.Guid ) ||
                                                personLabelsAdded.Contains( labelCache.Guid ) )
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                familyLabelsAdded.Add( labelCache.Guid );
                                            }
                                        }
                                        else if ( labelCache.LabelType == KioskLabelType.Person )
                                        {
                                            if ( personLabelsAdded.Contains( labelCache.Guid ) )
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                personLabelsAdded.Add( labelCache.Guid );
                                            }
                                        }

                                        var mergeObjects = new Dictionary<string, object>();
                                        foreach ( var keyValue in commonMergeFields )
                                        {
                                            mergeObjects.Add( keyValue.Key, keyValue.Value );
                                        }

                                        mergeObjects.Add( "Location", location );
                                        mergeObjects.Add( "Group", group );
                                        mergeObjects.Add( "Person", person );
                                        mergeObjects.Add( "People", people );
                                        mergeObjects.Add( "GroupType", groupType );

                                        var groupMembers = groupMemberService.Queryable().AsNoTracking()
                                            .Where( m =>
                                                m.PersonId == person.Person.Id &&
                                                m.GroupId == group.Group.Id )
                                            .ToList();
                                        mergeObjects.Add( "GroupMembers", groupMembers );

                                        //string debugInfo = mergeObjects.lavaDebugInfo();
                                        var label = new CheckInLabel( labelCache, mergeObjects, person.Person.Id );
                                        label.FileGuid = labelCache.Guid;
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
                                            var deviceId = location.Location.PrinterDeviceId;
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

                                        groupType.Labels.Add( label );

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

        /// <summary>
        /// Gets the labels for an item (person, grouptype, group, location).
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="existingLabels">The existing labels.</param>
        /// <returns></returns>
        public virtual List<KioskLabel> GetLabels( Attribute.IHasAttributes item, List<KioskLabel> existingLabels )
        {
            var labels = new List<KioskLabel>( existingLabels );

            if ( item.Attributes == null )
            {
                item.LoadAttributes();
            }

            foreach ( var attribute in item.Attributes.OrderBy( a => a.Value.Order ) )
            {
                if ( attribute.Value.FieldType.Class == typeof( Rock.Field.Types.LabelFieldType ).FullName )
                {
                    Guid? binaryFileGuid = item.GetAttributeValue( attribute.Key ).AsGuidOrNull();
                    if ( binaryFileGuid != null )
                    {
                        if ( !labels.Any( l => l.Guid == binaryFileGuid.Value ) )
                        {
                            var labelCache = KioskLabel.Get( binaryFileGuid.Value );
                            labelCache.Order = attribute.Value.Order;
                            if ( labelCache != null && (
                                labelCache.LabelType == KioskLabelType.Family ||
                                labelCache.LabelType == KioskLabelType.Person ||
                                labelCache.LabelType == KioskLabelType.Location ) )
                            {
                                labels.Add( labelCache );
                            }
                        }
                    }
                }
            }

            return labels;
        }

    }
}