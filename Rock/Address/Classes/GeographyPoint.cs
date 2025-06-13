using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Rock.Address.Classes
{
    /// <summary>
    /// Represents a geographical point with latitude and longitude coordinates, used in various location-related operations.
    /// </summary>
    public class GeographyPoint
    {
        /// <summary>
        /// The latitude and longitude coordinates of the geographical point.
        /// </summary>
        [JsonProperty( "latitude" )]
        public double Latitude { get; set; }

        /// <summary>
        /// The latitude and longitude coordinates of the geographical point.
        /// </summary>
        [JsonProperty( "longitude" )]
        public double Longitude { get; set; }

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
    }
}
