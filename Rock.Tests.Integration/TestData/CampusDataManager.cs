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
using System.Linq;

using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared;

using static Rock.Tests.Integration.TestData.LocationDataManager;

namespace Rock.Tests.Integration.TestData
{
    /// <summary>
    /// Provides actions to manage Campus data.
    /// </summary>
    public class CampusDataManager
    {
        private static Lazy<CampusDataManager> _dataManager = new Lazy<CampusDataManager>();
        public static CampusDataManager Instance => _dataManager.Value;

        /// <summary>
        /// Arguments that can be specified using lookup values.
        /// </summary>
        public class CampusInfo
        {
            public string ForeignKey { get; set; }
            public string CampusTypeIdentifier { get; set; }
            public string CampusStatusIdentifier { get; set; }
            public string CampusName { get; set; }
            public string CampusGuid { get; set; }
            public bool? IsActive { get; set; } = true;
            public string LocationIdentifier { get; set; } = "";
        }

        /// <summary>
        /// Arguments that can be specified using lookup values.
        /// </summary>
        public class AddCampusActionArgs : CampusInfo
        {
            public bool ReplaceIfExists { get; set; }
        }

        /// <summary>
        /// Add a new Campus.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public int AddCampus( RockContext rockContext, AddCampusActionArgs args )
        {
            var campus = new Campus();

            rockContext.WrapTransaction( () =>
            {
                var campusService = new CampusService( rockContext );

                var campusGuid = args.CampusGuid.AsGuidOrNull();
                if ( campusGuid != null )
                {
                    var existingCampus = campusService.Queryable().FirstOrDefault( g => g.Guid == campusGuid );
                    if ( existingCampus != null )
                    {
                        if ( !args.ReplaceIfExists )
                        {
                            return;
                        }
                        DeleteCampus( rockContext, args.CampusGuid );
                        rockContext.SaveChanges();
                    }
                }

                UpdateCampusFromCampusInfo( rockContext, campus, args );

                campusService.Add( campus );

                rockContext.SaveChanges();
            } );

            return campus.Id;
        }

        #region Update Campus

        /// <summary>
        /// Arguments that can be specified using lookup values.
        /// </summary>
        public class UpdateCampusActionArgs : CampusInfo
        {
            public string UpdateTargetIdentifier { get; set; }
        }

        /// <summary>
        /// Update an existing Campus.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public void UpdateCampus( RockContext rockContext, UpdateCampusActionArgs args )
        {
            rockContext.WrapTransaction( () =>
            {
                var campusService = new CampusService( rockContext );
                var campus = campusService.GetByIdentifierOrThrow( args.UpdateTargetIdentifier );

                UpdateCampusFromCampusInfo( rockContext, campus, args );

                rockContext.SaveChanges();
            } );
        }

        /// <summary>
        /// Update an existing Campus.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private void UpdateCampusFromCampusInfo( RockContext rockContext, Campus campus, CampusInfo args )
        {
            // Set Guid.
            if ( args.CampusGuid.IsNotNullOrWhiteSpace() )
            {
                campus.Guid = InputParser.ParseToGuidOrThrow( nameof( CampusInfo.CampusGuid ), args.CampusGuid );
            }

            var definedValueService = new DefinedValueService( rockContext );

            // Set Campus Status.
            if ( args.CampusStatusIdentifier.IsNotNullOrWhiteSpace() )
            {
                var campusStatusGuid = SystemGuid.DefinedType.CAMPUS_STATUS.AsGuid();
                var campusStatusValueQuery = definedValueService.Queryable()
                    .Where( v => v.DefinedType.Guid == campusStatusGuid );

                DefinedValue campusStatusValue;
                if ( args.CampusStatusIdentifier.AsGuid() != Guid.Empty
                     || args.CampusStatusIdentifier.AsIntegerOrNull() != null )
                {
                    campusStatusValue = campusStatusValueQuery.GetByIdentifierOrThrow( args.CampusStatusIdentifier );
                }
                else
                {
                    campusStatusValue = campusStatusValueQuery.GetByName( args.CampusStatusIdentifier, "Value" );
                }

                campus.CampusStatusValueId = campusStatusValue?.Id;
            }

            // Set Campus Type.
            if ( args.CampusTypeIdentifier.IsNotNullOrWhiteSpace() )
            {
                var campusTypeGuid = SystemGuid.DefinedType.CAMPUS_TYPE.AsGuid();
                var campusTypeValueQuery = definedValueService.Queryable()
                    .Where( v => v.DefinedType.Guid == campusTypeGuid );

                DefinedValue campusTypeValue;
                if ( args.CampusTypeIdentifier.AsGuid() != Guid.Empty
                     || args.CampusTypeIdentifier.AsIntegerOrNull() != null )
                {
                    campusTypeValue = campusTypeValueQuery.GetByIdentifierOrThrow( args.CampusTypeIdentifier );
                }
                else
                {
                    campusTypeValue = campusTypeValueQuery.GetByName( args.CampusTypeIdentifier, "Value" );
                }

                campus.CampusTypeValueId = campusTypeValue?.Id;
            }

            // Set Location.
            if ( args.LocationIdentifier.IsNotNullOrWhiteSpace() )
            {
                var locationService = new LocationService( rockContext );
                var location = locationService.GetByIdentifierOrThrow( args.LocationIdentifier );

                campus.LocationId = location?.Id;
            }

            // Set Name.
            if ( args.CampusName.IsNotNullOrWhiteSpace() )
            {
                campus.Name = args.CampusName;
            }

            // Set ForeignKey.
            if ( args.ForeignKey.IsNotNullOrWhiteSpace() )
            {
                campus.ForeignKey = args.ForeignKey;
            }

            // Set IsActive.
            if ( args.IsActive.HasValue )
            {
                campus.IsActive = args.IsActive;
            }

            rockContext.SaveChanges();
        }

        #endregion

        public bool DeleteCampus( RockContext rockContext, string CampusIdentifier )
        {
            rockContext.WrapTransaction( () =>
            {
                var CampusService = new CampusService( rockContext );
                var Campus = CampusService.Get( CampusIdentifier );

                CampusService.Delete( Campus );

                rockContext.SaveChanges();
            } );

            return true;
        }

        #region Test Data

        private const string MainCampusLocationGuid = "20985A47-580C-40A0-A92D-A33725B83E9F";
        private const string SouthCampusGuid = "25B59587-3369-42E2-877D-BD475AC26DD8";
        private const string SouthCampusLocationGuid = "58A78530-8BE2-42C9-B6B2-A7D8BFDA1369";
        private const string NorthCampusGuid = "2C396ECC-4A2F-42AC-84A4-AB85A2496535";
        private const string NorthCampusLocationGuid = "44D20B6E-DFBD-49AA-B8D5-40EFE4338B28";
        private const string OnlineCampusGuid = "F05C5B97-F870-45A3-8A2E-D1B4F564417A";

        public void AddCampusTestDataSet()
        {
            var rockContext = new RockContext();

            // If the online campus exists, then assume we have already initialized the data.
            if ( new CampusService( rockContext ).Get( OnlineCampusGuid.AsGuid() ) != null )
            {
                return;
            }

            var locationManager = LocationDataManager.Instance;
            var campusManager = CampusDataManager.Instance;

            // Set Main Campus Location.
            // 3120 W Cholla St, Phoenix, AZ 85029-4113
            // ~300m from Ted Decker's House
            var argsLocationMain = new AddLocationActionArgs
            {
                LocationGuid = MainCampusLocationGuid,
                LocationName = "Main Campus",
                LocationTypeIdentifier = "Campus",
                GeoLocationWellKnownText = "POINT(-112.1260113729907 33.590845266972586)"
            };
            locationManager.AddLocation( rockContext, argsLocationMain );
            rockContext.SaveChanges();

            var updateCampusArgs = new UpdateCampusActionArgs
            {
                UpdateTargetIdentifier = TestGuids.Crm.CampusMain,
                LocationIdentifier = MainCampusLocationGuid
            };
            campusManager.UpdateCampus( rockContext, updateCampusArgs );
            rockContext.SaveChanges();

            // Add North Campus.
            // 20012 N 35th Ave, Glendale, AZ 85308
            // ~8500m north of Main.
            var argsLocationNorth = new AddLocationActionArgs
            {
                LocationGuid = NorthCampusLocationGuid,
                LocationName = "North Campus",
                LocationTypeIdentifier = "Campus",
                GeoLocationWellKnownText = "POINT(-112.13571728517672 33.66701655198145)"
            };
            locationManager.AddLocation( rockContext, argsLocationNorth );
            rockContext.SaveChanges();

            var argsCampusNorth = new AddCampusActionArgs
            {
                CampusGuid = NorthCampusGuid,
                CampusName = "North Campus",
                CampusStatusIdentifier = "Open",
                CampusTypeIdentifier = "Physical",
                LocationIdentifier = NorthCampusLocationGuid
            };
            campusManager.AddCampus( rockContext, argsCampusNorth );
            rockContext.SaveChanges();

            // Add South Campus.
            // 8441 S 35th Ave, Laveen Village, AZ 85339, United States
            // ~24000m south of Main.
            var argsLocationSouth = new AddLocationActionArgs
            {
                LocationGuid = SouthCampusLocationGuid,
                LocationName = "South Campus",
                LocationTypeIdentifier = "Campus",
                GeoLocationWellKnownText = "POINT(-112.13287113216572 33.37018150974973)"
            };
            locationManager.AddLocation( rockContext, argsLocationSouth );
            rockContext.SaveChanges();

            var argsCampusSouth = new AddCampusActionArgs
            {
                CampusGuid = SouthCampusGuid,
                CampusName = "South Campus",
                CampusStatusIdentifier = "Open",
                CampusTypeIdentifier = "Physical",
                LocationIdentifier = SouthCampusLocationGuid
            };
            campusManager.AddCampus( rockContext, argsCampusSouth );
            rockContext.SaveChanges();

            // Add Online Campus.
            // (Virtual)
            var argsCampusOnline = new AddCampusActionArgs
            {
                CampusGuid = OnlineCampusGuid,
                CampusName = "Online Campus",
                CampusStatusIdentifier = "Open",
                CampusTypeIdentifier = "Online"
            };
            campusManager.AddCampus( rockContext, argsCampusOnline );
            rockContext.SaveChanges();
        }

        #endregion
    }
}
