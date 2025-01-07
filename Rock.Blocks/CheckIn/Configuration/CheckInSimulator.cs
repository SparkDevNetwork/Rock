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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.CheckIn.v2;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInSimulator;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.CheckIn.Configuration
{
    /// <summary>
    /// Simulates the check-in process in a UI that can be used to quickly
    /// test different configuration settings.
    /// </summary>

    [DisplayName( "Check-in Simulator" )]
    [Category( "Check-in > Configuration" )]
    [Description( "Simulates the check-in process in a UI that can be used to quickly test different configuration settings." )]
    [IconCssClass( "fa fa-vial" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "23316388-ec1d-495a-8efb-c1b5f6806041" )]
    [Rock.SystemGuid.BlockTypeGuid( "30002636-494b-4fdc-848c-a816f9291764" )]
    public class CheckInSimulator : RockBlockType
    {
        #region Constants

        /// <summary>
        /// The simulator note key prefix used to identify attendance records
        /// that were created by the simulator so they can be deleted.
        /// </summary>
        public const string SimulatorNoteKey = "Simulated Attendance";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var kioskDeviceValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK.AsGuid(), rockContext ).Id;
                var director = new CheckInDirector( rockContext );

                return new CheckInSimulatorOptionsBag
                {
                    Templates = director.GetConfigurationTemplateBags(),
                    Kiosks = DeviceCache.All()
                        .Where( d => d.DeviceTypeValueId == kioskDeviceValueId )
                        .OrderBy( d => d.Name )
                        .Select( d => new ListItemBag
                        {
                            Value = d.Guid.ToString(),
                            Text = d.Name
                        } )
                        .ToList()
                };
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the simulated attendance records for today.
        /// </summary>
        /// <param name="batch">The unique identifier of the values to delete.</param>
        /// <returns>The result of the operation.</returns>
        [BlockAction]
        public BlockActionResult DeleteSimulatedAttendance( Guid? batch )
        {
            var today = RockDateTime.Now.Date;

            using ( var rockContext = new RockContext() )
            {
                var attendanceService = new AttendanceService( rockContext );

                var attendancesQry = attendanceService.Queryable()
                    .Where( a => a.StartDateTime >= today );

                // Either delete a single set of simulated attendance records
                // or delete all of them.
                if ( batch.HasValue )
                {
                    var noteKey = $"{SimulatorNoteKey} {batch}";

                    attendancesQry = attendancesQry.Where( a => a.Note == noteKey );
                }
                else
                {
                    attendancesQry = attendancesQry.Where( a => a.Note.StartsWith( SimulatorNoteKey ) );
                }

                var attendances = attendancesQry.ToList();

                if ( !attendances.Any() )
                {
                    return ActionOk( "No attendance records found." );
                }

                attendanceService.DeleteRange( attendances );

                rockContext.SaveChanges();

                return ActionOk( $"Deleted {attendances.Count} attendance records." );
            }
        }

        #endregion
    }
}
