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
using System.Data.Entity.Spatial;
using System.Linq;

using Rock.Data;
using Rock.Lava;
using Rock.Model;

namespace Rock.Tests.Integration.TestData
{
    /// <summary>
    /// Provides actions to manage Location data.
    /// </summary>
    public class LocationDataManager
    {
        private static Lazy<LocationDataManager> _dataManager = new Lazy<LocationDataManager>();
        public static LocationDataManager Instance => _dataManager.Value;

        /// <summary>
        /// Arguments that can be specified using lookup values.
        /// </summary>
        public class AddLocationActionArgs : LocationInfo
        {
            public bool ReplaceIfExists { get; set; }
        }

        /// <summary>
        /// Arguments that can be specified using lookup values.
        /// </summary>
        public class LocationInfo
        {
            public string ForeignKey { get; set; }
            public string LocationName { get; set; }
            public string LocationGuid { get; set; }
            public string LocationTypeIdentifier { get; set; }
            public bool? IsActive { get; set; }
            public string GeoLocationWellKnownText { get; set; } = "";
        }

        /// <summary>
        /// Add a new Location.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public int AddLocation( RockContext rockContext, AddLocationActionArgs args )
        {
            var location = new Location();

            rockContext.WrapTransaction( () =>
            {
                var locationService = new LocationService( rockContext );

                var locationGuid = args.LocationGuid.AsGuidOrNull();
                if ( locationGuid != null )
                {
                    var existingLocation = locationService.Queryable().FirstOrDefault( g => g.Guid == locationGuid );
                    if ( existingLocation != null )
                    {
                        if ( !args.ReplaceIfExists )
                        {
                            return;
                        }
                        DeleteLocation( rockContext, args.LocationGuid );
                        rockContext.SaveChanges();
                    }
                }

                UpdateLocationFromLocationInfo( rockContext, location, args );

                locationService.Add( location );

                rockContext.SaveChanges();
            } );

            return location.Id;
        }

        #region Update Location

        /// <summary>
        /// Arguments that can be specified using lookup values.
        /// </summary>
        public class UpdateLocationActionArgs : LocationInfo
        {
            public string UpdateTargetIdentifier { get; set; }
        }

        /// <summary>
        /// Update an existing Location.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public void UpdateLocation( RockContext rockContext, UpdateLocationActionArgs args )
        {
            rockContext.WrapTransaction( () =>
            {
                var locationService = new LocationService( rockContext );
                var location = locationService.GetByIdentifierOrThrow( args.UpdateTargetIdentifier );

                UpdateLocationFromLocationInfo( rockContext, location, args );

                rockContext.SaveChanges();
            } );
        }

        /// <summary>
        /// Update an existing Location.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private void UpdateLocationFromLocationInfo( RockContext rockContext, Location location, LocationInfo args )
        {
            // Set Guid.
            if ( args.LocationGuid.IsNotNullOrWhiteSpace() )
            {
                location.Guid = InputParser.ParseToGuidOrThrow( nameof( LocationInfo.LocationGuid ), args.LocationGuid );
            }

            var definedValueService = new DefinedValueService( rockContext );

            // Get Location Type.
            if ( args.LocationTypeIdentifier.IsNotNullOrWhiteSpace() )
            {
                var locationTypeGuid = SystemGuid.DefinedType.LOCATION_TYPE.AsGuid();
                var locationTypeValueQuery = definedValueService.Queryable()
                    .Where( v => v.DefinedType.Guid == locationTypeGuid );

                DefinedValue locationTypeValue;
                if ( args.LocationTypeIdentifier.AsGuid() != Guid.Empty
                     || args.LocationTypeIdentifier.AsIntegerOrNull() != null )
                {
                    locationTypeValue = locationTypeValueQuery.GetByIdentifierOrThrow( args.LocationTypeIdentifier );
                }
                else
                {
                    locationTypeValue = locationTypeValueQuery.GetByName( args.LocationTypeIdentifier, "Value" );
                }

                location.LocationTypeValueId = locationTypeValue?.Id;
            }

            // Set GeoPoint.
            if ( args.GeoLocationWellKnownText.IsNotNullOrWhiteSpace() )
            {
                DbGeography geoPoint = null;
                if ( !string.IsNullOrWhiteSpace( args.GeoLocationWellKnownText ) )
                {
                    geoPoint = DbGeography.PointFromText( args.GeoLocationWellKnownText, 4326 );
                    location.GeoPoint = geoPoint;
                }
            }

            // Set Name.
            if ( args.LocationName.IsNotNullOrWhiteSpace() )
            {
                location.Name = args.LocationName;
            }

            // Set ForeignKey.
            if ( args.ForeignKey.IsNotNullOrWhiteSpace() )
            {
                location.ForeignKey = args.ForeignKey;
            }

            // Set IsActive.
            if ( args.IsActive.HasValue )
            {
                location.IsActive = args.IsActive.Value;
            }

            rockContext.SaveChanges();
        }

        #endregion

        public bool DeleteLocation( RockContext rockContext, string locationIdentifier )
        {
            rockContext.WrapTransaction( () =>
            {
                var locationService = new LocationService( rockContext );
                var location = locationService.Get( locationIdentifier );

                locationService.Delete( location );

                rockContext.SaveChanges();
            } );

            return true;
        }
    }
}
