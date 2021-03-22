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

using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace Rock.Oidc.Client
{
    /// <summary>
    /// Extension Methods for getting claims from a Jwt Security Token.
    /// </summary>
    public static class ClaimExtensionMethods
    {
        /// <summary>
        /// Gets the claim value.
        /// </summary>
        /// <param name="idToken">The identifier token.</param>
        /// <param name="claim">The claim.</param>
        /// <returns></returns>
        public static string GetClaimValue( this JwtSecurityToken idToken, string claim )
        {
            if ( idToken == null )
            {
                return string.Empty;
            }

            if ( claim.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            return idToken.Claims.Where( c => c.Type == claim ).Select( c => c.Value ).FirstOrDefault();
        }
    }
}