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
using System.Security.Cryptography;
using System.Text;

using Rock.Attribute;

namespace Rock.Core.Geography
{
    /// <summary>
    /// Produces a deterministic privacy offset for a geographic point so a published location approximates the true one without revealing it.
    /// </summary>
    /// <remarks>
    /// The same entity at the same location always resolves to the same fuzzed point, so repeated lookups cannot be averaged to triangulate the true location, and every consumer that publishes the entity shows the identical approximated point (so two different blocks cannot be compared to triangulate it either). When the entity actually moves, the fuzzed point jumps to a fresh, unpredictable spot, so someone who once knew a real location cannot reuse its offset to de-fuzz a later one. The offset is derived by HMAC-ing the entity's <see cref="Guid"/> together with its quantized location, keyed by a single server-only salt: the salt is never sent to the client, so the offset cannot be reversed from the public inputs and the fuzzed point to recover the true location. The offset reaches at most <see cref="MaxOffsetFractionOfRadius"/> of <see cref="AlgorithmRadiusMeters"/>, so the true location stays inside a circle of that radius drawn around the fuzzed point.
    /// </remarks>
    [RockInternal( "20.0" )]
    public static class LocationObfuscator
    {
        /// <summary>
        /// The system setting that stores the server-only salt keying the offset, so the deterministic per-entity offset cannot be reproduced from the public entity guid.
        /// </summary>
        private const string SaltSettingKey = "core_LocationFuzzSalt";

        /// <summary>
        /// The largest fraction of <see cref="AlgorithmRadiusMeters"/> the fuzzed point may sit from the true location, so the true point falls inside the circle rather than on its edge.
        /// </summary>
        private const double MaxOffsetFractionOfRadius = 0.95;

        /// <summary>
        /// Meters per mile; used to size the algorithm radius in meters.
        /// </summary>
        private const double MetersPerMile = 1609.344;

        /// <summary>
        /// Meters per degree of latitude (constant); used to convert a metric offset into a coordinate shift.
        /// </summary>
        private const double MetersPerDegreeLatitude = 111320;

        /// <summary>
        /// Cells per degree used to quantize the location before it seeds the offset: 1000 gives ~111 m cells (0.001°), on the order of the privacy radius. Coarse enough that a re-save or re-geocode of the same address stays in one cell and keeps the same fuzzed point, fine enough that a real move to a new venue crosses cells and earns a fresh, unpredictable offset.
        /// </summary>
        private const decimal QuantizationCellsPerDegree = 1000m;

        /// <summary>
        /// The radius, in meters, the offset magnitude is scaled to: 500 feet (~152 m). This is a fixed, global value that drives every fuzzed point identically. It is intentionally not configurable and never a parameter of <see cref="GetFuzzedLocation"/>, because scaling the offset per consumer would produce different points for the same entity and reintroduce the triangulation risk this obfuscation exists to prevent. Changing it re-fuzzes every entity uniformly (a global privacy-level decision), which is safe; overriding it for a single caller is not.
        /// </summary>
        private const double AlgorithmRadiusMeters = 500 * MetersPerMile / 5280;

        /// <summary>
        /// The default radius, in meters, for the privacy circle a consumer draws around a fuzzed point, equal to <see cref="AlgorithmRadiusMeters"/> so the drawn circle contains the real point with a small margin.
        /// </summary>
        /// <remarks>
        /// The drawn circle is purely presentational: a consumer MAY draw a larger or smaller circle without ever affecting the fuzzed point (which is fixed), so no drawing choice can enable triangulation. A circle drawn smaller than <see cref="AlgorithmRadiusMeters"/> may no longer contain the real point and can over-imply precision, so shrink it deliberately.
        /// </remarks>
        public const double DefaultCircleRadiusMeters = AlgorithmRadiusMeters;

        /// <summary>
        /// Returns the deterministic fuzzed point for an entity: its true coordinates shifted by a per-entity privacy offset.
        /// </summary>
        /// <remarks>
        /// The same entity at the same location always yields the same point (so cross-block lookups cannot be compared to triangulate), while a real move yields a fresh, unpredictable point (so a prior insider cannot de-fuzz the new location). Any entity type may be used. By design this takes no radius parameter: the offset magnitude is the fixed global <see cref="AlgorithmRadiusMeters"/>, and a consumer that wants a different privacy area sizes the circle it draws (see <see cref="DefaultCircleRadiusMeters"/>) rather than the offset.
        /// </remarks>
        /// <param name="entityGuid">The entity identifier that seeds the offset.</param>
        /// <param name="latitude">The entity's true latitude.</param>
        /// <param name="longitude">The entity's true longitude.</param>
        /// <returns>The fuzzed latitude and longitude.</returns>
        public static (double Latitude, double Longitude) GetFuzzedLocation( Guid entityGuid, double latitude, double longitude )
        {
            var saltBytes = GetSalt();

            /*
                08/12/26 - JMH

                The offset is seeded from the entity's guid AND its quantized location, not the guid
                alone, to reconcile two competing goals:

                  1. Reproducible - every consumer must fuzz the same entity at the same place to the
                     exact same point, or two surfaces could be compared to triangulate the real one.
                  2. Unpredictable across a move - if the location changes, the new offset must not be
                     derivable from the old. Seeding on the guid alone fixes one offset vector for the
                     entity's whole life, so anyone who ever learned a real location (e.g. a removed
                     group member) could compute that vector and de-fuzz every later location.

                The location is quantized to a coarse cell (~the privacy radius) before it is hashed.
                Seeding on the raw coordinates would break goal 1: a harmless re-save or re-geocode
                nudges the stored point a few meters, producing a second, independent fuzzed point for
                the same real place - two circles an attacker could intersect to narrow it. Quantizing
                absorbs that jitter (same place -> same cell -> same point), while a real move to a new
                venue crosses cells and earns a fresh vector. The rounding is done in decimal so the cell
                a coordinate falls in is exact and free of binary floating-point artifacts at boundaries.

                Reason: Reproducible per location yet unpredictable across a move, so a prior insider
                cannot de-fuzz a relocated entity.
            */
            var latitudeCell = ( long ) Math.Round( ( decimal ) latitude * QuantizationCellsPerDegree, MidpointRounding.AwayFromZero );
            var longitudeCell = ( long ) Math.Round( ( decimal ) longitude * QuantizationCellsPerDegree, MidpointRounding.AwayFromZero );
            var seedMessage = Encoding.UTF8.GetBytes( FormattableString.Invariant( $"{entityGuid}|{latitudeCell}|{longitudeCell}" ) );

            byte[] seed;
            using ( var hmac = new HMACSHA256( saltBytes ) )
            {
                seed = hmac.ComputeHash( seedMessage );
            }

            var angle = ( BitConverter.ToUInt32( seed, 0 ) / ( double ) uint.MaxValue ) * 2 * Math.PI;
            var distanceSeed = BitConverter.ToUInt32( seed, 4 ) / ( double ) uint.MaxValue;

            // The square root spreads the offset uniformly over the circle's area rather than biasing it
            // toward the center, so the true location is not disproportionately near the fuzzed point (a
            // linear distance would cluster it there and weaken the privacy). The offset reaches at most a
            // fixed fraction of the radius, keeping the true location inside the circle.
            var distanceMeters = Math.Sqrt( distanceSeed ) * MaxOffsetFractionOfRadius * AlgorithmRadiusMeters;

            // A degree of longitude shrinks toward the poles, so scale it by the latitude's cosine.
            var metersPerDegreeLongitude = MetersPerDegreeLatitude * Math.Cos( latitude * Math.PI / 180 );

            var deltaLatitude = ( distanceMeters * Math.Cos( angle ) ) / MetersPerDegreeLatitude;
            var deltaLongitude = metersPerDegreeLongitude > 0
                ? ( distanceMeters * Math.Sin( angle ) ) / metersPerDegreeLongitude
                : 0;

            return ( latitude + deltaLatitude, longitude + deltaLongitude );
        }

        /// <summary>
        /// Gets the server-only salt that keys the offset, generating and persisting one on first use.
        /// </summary>
        /// <remarks>
        /// The salt is read from a cached system setting and never sent to the client, so the deterministic per-entity offset cannot be recomputed from the public entity guid and reversed to recover a true location.
        /// </remarks>
        /// <returns>The salt bytes used as the HMAC key.</returns>
        private static byte[] GetSalt()
        {
            var salt = Rock.Web.SystemSettings.GetValue( SaltSettingKey );
            if ( string.IsNullOrWhiteSpace( salt ) )
            {
                salt = Rock.Security.Encryption.GenerateUniqueToken();
                Rock.Web.SystemSettings.SetValue( SaltSettingKey, salt );
            }

            return Encoding.UTF8.GetBytes( salt );
        }
    }
}
