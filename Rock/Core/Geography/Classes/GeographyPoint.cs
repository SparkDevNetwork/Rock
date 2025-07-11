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
using Newtonsoft.Json;

namespace Rock.Core.Geography.Classes
{
    /// <summary>
    /// Represents a geographical point with latitude and longitude coordinates, used in various location-related operations.
    /// </summary>
    public class GeographyPoint
    {
        #region Properties
        /// <summary>
        /// The latitude and longitude coordinates of the geographical point.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// The latitude and longitude coordinates of the geographical point.
        /// </summary>
        public double Longitude { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the GeographyPoint class.
        /// </summary>
        public GeographyPoint()
        {
        }

        /// <summary>
        /// Initializes a new instance of the GeographyPoint class with specified latitude and longitude.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public GeographyPoint( double latitude, double longitude )
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        /// <summary>
        /// Initializes a new instance of the GeographyPoint class with specified latitude and longitude.
        /// </summary>
        /// <param name="input"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public GeographyPoint( string input )
        {
            if ( string.IsNullOrWhiteSpace( input ) )
            {
                throw new ArgumentNullException( nameof( input ), "Input string cannot be null or empty." );
            }

            var parts = input.Split( ',' );
            if ( parts.Length != 2 )
            {
                throw new ArgumentException( "Input must be in the format 'latitude,longitude'.", nameof( input ) );
            }

            if ( !double.TryParse( parts[0].Trim(), out var latitude ) )
            {
                throw new ArgumentException( "Latitude must be a valid number.", nameof( input ) );
            }

            if ( !double.TryParse( parts[1].Trim(), out var longitude ) )
            {
                throw new ArgumentException( "Longitude must be a valid number.", nameof( input ) );
            }

            Latitude = latitude;
            Longitude = longitude;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Creates a GeographyPoint from a DbGeography object.
        /// </summary>
        /// <param name="dbGeography"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static GeographyPoint FromDatabase( DbGeography dbGeography )
        {
            if ( dbGeography == null )
            {
                throw new ArgumentNullException( nameof( dbGeography ) );
            }

            if ( !dbGeography.Latitude.HasValue || !dbGeography.Longitude.HasValue )
            {
                throw new ArgumentException( "The DbGeography must have both Latitude and Longitude values." );
            }

            return new GeographyPoint( dbGeography.Latitude.Value, dbGeography.Longitude.Value );
        }

        #endregion

        #region Overrides, Helpers and Equality

        /// <summary>
        /// Attempts to parse a string in the format "latitude,longitude" into a GeographyPoint.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="point">The resulting GeographyPoint if parsing succeeds.</param>
        /// <returns>True if parsing was successful and coordinates are valid; otherwise, false.</returns>
        public static bool TryParse( string input, out GeographyPoint point )
        {
            point = null;

            if ( string.IsNullOrWhiteSpace( input ) )
                return false;

            var parts = input.Split( ',' );

            if ( parts.Length != 2 )
                return false;

            if ( !double.TryParse( parts[0].Trim(), out double latitude ) )
                return false;

            if ( !double.TryParse( parts[1].Trim(), out double longitude ) )
                return false;

            // Validate bounds
            if ( latitude < -90 || latitude > 90 )
                return false;

            if ( longitude < -180 || longitude > 180 )
                return false;

            point = new GeographyPoint( latitude, longitude );
            return true;
        }

        /// <summary>
        /// Determines whether the latitude and longitude are within valid geographic bounds.
        /// </summary>
        /// <returns>True if the coordinates are valid; otherwise, false.</returns>
        public bool IsValid()
        {
            return Latitude >= -90 && Latitude <= 90 &&
                   Longitude >= -180 && Longitude <= 180;
        }

        /// <summary>
        /// Returns a string representation of the geographical point in the format "latitude,longitude".
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Latitude},{Longitude}";

        /// <summary>
        /// Determines whether the specified LocationPoint is equal to the current LocationPoint.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals( GeographyPoint other )
        {
            if ( other is null )
            {
                return false;
            }
            return Latitude == other.Latitude && Longitude == other.Longitude;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current LocationPoint.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals( object obj ) => Equals( obj as GeographyPoint );

        /// <summary>
        /// Overload the == operator so we can do something like:
        /// var matches = results.Where( r => r.LocationPoint == travelDistance.DestinationPoint ).ToList();
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==( GeographyPoint left, GeographyPoint right )
        {
            if ( ReferenceEquals( left, right ) )
                return true;
            if ( left is null || right is null )
                return false;
            return left.Equals( right );
        }

        /// <summary>
        /// Overload the != operator so we can do something like:
        /// var matches = results.Where( r => r.LocationPoint != travelDistance.DestinationPoint ).ToList();
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=( GeographyPoint left, GeographyPoint right )
        {
            return !( left == right );
        }

        /// <summary>
        /// Returns a hash code for the current LocationPoint.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Latitude.GetHashCode();
                hash = hash * 23 + Longitude.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Converts the GeographyPoint to a DbGeography point.
        /// </summary>
        /// <returns>A DbGeography point representing this location.</returns>
        public DbGeography ToDatabase()
        {
            if ( Latitude < -90 || Latitude > 90 || Longitude < -180 || Longitude > 180 )
            {
                throw new ArgumentOutOfRangeException( "Latitude or Longitude are out of valid range." );
            }

            string wkt = $"POINT({Longitude} {Latitude})";
            return DbGeography.PointFromText( wkt, 4326 );
        }
        #endregion
    }
}
