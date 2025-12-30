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
using Rock.Net;
using Rock.Net.Geolocation;
using Rock.Web.Cache;

namespace Rock
{
    /// <summary>
    /// Extension methods for <see cref="RockRequestContext"/>.
    /// </summary>
    public static class RockRequestContextExtensions
    {
        /// <summary>
        /// Gets whether the requesting client is forbidden from accessing the requested resource.
        /// </summary>
        /// <param name="context">The <see cref="RockRequestContext"/> that represents this request.</param>
        /// <param name="pageCache">The <see cref="PageCache"/> if this request is associated with a <see cref="Rock.Model.Page"/>.</param>
        /// <returns>Whether the requesting client is forbidden from accessing the requested resource.</returns>
        public static bool IsClientForbidden( this RockRequestContext context, PageCache pageCache = null )
        {
            var countryCode = context?.ClientInformation?.Geolocation?.CountryCode?.ToUpper();
            if ( countryCode.IsNullOrWhiteSpace() )
            {
                // We don't know enough about this request, so we'll let it through.
                return false;
            }

            if ( IpGeoLookup.GloballyRestrictedCountryCodes?.Contains( countryCode ) == true )
            {
                // This country is globally restricted throughout all of Rock.
                return true;
            }

            if ( pageCache?.RestrictedCountryCodes?.Contains( countryCode ) == true )
            {
                // This country is restricted for this page.
                return true;
            }

            return false;
        }
    }
}
