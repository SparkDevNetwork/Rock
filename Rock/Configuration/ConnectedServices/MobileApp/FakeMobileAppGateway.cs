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
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Rock.Configuration.ConnectedServices.DataTransferObjects;
using Rock.Configuration.ConnectedServices.MobileApp.DataTransferObjects;

namespace Rock.Configuration.ConnectedServices.MobileApp
{
    /// <summary>
    /// A stand-in for Spark's gateway that answers every call on this server, so the
    /// Connected Services card can be enabled, updated and disabled on a development
    /// machine before the gateway and the church directory exist.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         ARGUS-LIVE: Nothing this class returns has ever come from the real directory.
    ///         Once Spark's gateway answers on the mobile-app service, retest each of these
    ///         against it and then delete this class (see <see cref="IMobileAppGateway"/>):
    ///     </para>
    ///     <list type="bullet">
    ///         <item>The church code. This one is derived from the Rock instance id with a
    ///         made-up alphabet; the real one is minted by Argus with its own alphabet
    ///         (Argus spec, section 2).</item>
    ///         <item>The poster link. This one uses the Debug-only test host the mobile shell
    ///         accepts (<c>argus.spark.test</c>); the real host is Argus's domain.</item>
    ///         <item>Validation. This fake accepts anything Rock's own checks let through,
    ///         including an <c>http</c> API URL. The real directory requires <c>https</c> and
    ///         answers <c>400 validation_failed</c> with the failing fields.</item>
    ///         <item>The response to a config write that follows a gateway enable, and the
    ///         gateway's error bodies, which Rock has never parsed.</item>
    ///         <item>The campus publish. This fake accepts every call and counts the campuses
    ///         itself. The real directory answers <c>404 not_enrolled</c> or
    ///         <c>409 church_inactive</c> for a church it does not list, so retest the campus
    ///         job against a church that was disabled at the gateway but not in Rock.</item>
    ///     </list>
    ///     <para>
    ///         The mobile shell's fake church directory does not know the code this class
    ///         mints, so scanning the fake link in the shell finds no church. Test the shell
    ///         against this Rock with the shell's Debug dev connection seed instead.
    ///     </para>
    /// </remarks>
    internal class FakeMobileAppGateway : IMobileAppGateway
    {
        /// <summary>
        /// The characters the fake church code is built from. No 0, 1, I, L, O or U, so
        /// a code read off a poster is hard to mistype.
        /// </summary>
        private const string CodeAlphabet = "23456789ABCDEFGHJKMNPQRSTVWXYZ";

        /// <summary>
        /// The length of the fake church code, matching the directory's examples.
        /// </summary>
        private const int CodeLength = 8;

        /// <summary>
        /// The Debug-only link host the mobile shell recognizes.
        /// </summary>
        private const string TestLinkHost = "argus.spark.test";

        /// <summary>
        /// The value the church code is derived from, so every call on this Rock answers
        /// with the same code, the way the real directory keeps a church's code forever.
        /// </summary>
        private readonly Guid _instanceId;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeMobileAppGateway"/> class.
        /// </summary>
        /// <param name="instanceId">The Rock instance id the church code is derived from.</param>
        public FakeMobileAppGateway( Guid instanceId )
        {
            _instanceId = instanceId;
        }

        /// <inheritdoc/>
        public bool IsFake => true;

        /// <inheritdoc/>
        public Task<ConfigurationResult<SetEnabledResponse>> SetEnabledAsync( bool enabled, CancellationToken cancellationToken )
        {
            return Task.FromResult( new ConfigurationResult<SetEnabledResponse>
            {
                IsSuccess = true,
                Data = new SetEnabledResponse
                {
                    Enabled = enabled
                }
            } );
        }

        /// <inheritdoc/>
        public Task<ConfigurationResult<MobileAppConfigurationResponse>> SetConfigurationAsync( MobileAppConfigurationRequest request, CancellationToken cancellationToken )
        {
            var churchCode = GetChurchCode();

            return Task.FromResult( new ConfigurationResult<MobileAppConfigurationResponse>
            {
                IsSuccess = true,
                Data = new MobileAppConfigurationResponse
                {
                    ChurchCode = churchCode,
                    Link = $"https://{TestLinkHost}/c/{churchCode}",
                    IsActive = true,
                    Name = request?.Name
                }
            } );
        }

        /// <inheritdoc/>
        public Task<ConfigurationResult<MobileAppCampusesResponse>> SetCampusesAsync( MobileAppCampusesRequest request, CancellationToken cancellationToken )
        {
            var campuses = request.Campuses.ValueKind == JsonValueKind.Array
                ? request.Campuses.EnumerateArray().ToList()
                : new List<JsonElement>();

            return Task.FromResult( new ConfigurationResult<MobileAppCampusesResponse>
            {
                IsSuccess = true,
                Data = new MobileAppCampusesResponse
                {
                    CampusCount = campuses.Count,
                    GeocodedCount = campuses.Count( c => c.TryGetProperty( "lat", out _ ) )
                }
            } );
        }

        /// <summary>
        /// Derives this Rock's fake church code from its instance id.
        /// </summary>
        /// <returns>The church code.</returns>
        internal string GetChurchCode()
        {
            using ( var sha256 = SHA256.Create() )
            {
                var bytes = sha256.ComputeHash( Encoding.UTF8.GetBytes( _instanceId.ToString() ) );

                return new string( bytes
                    .Take( CodeLength )
                    .Select( b => CodeAlphabet[b % CodeAlphabet.Length] )
                    .ToArray() );
            }
        }
    }
}
