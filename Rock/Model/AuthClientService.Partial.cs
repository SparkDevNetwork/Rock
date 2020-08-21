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
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data Access/Service class for <see cref="Rock.Model.AuthClient"/> entities.
    /// </summary>
    public partial class AuthClientService
    {
        /// <summary>
        /// Gets the by client identifier.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        public async Task<AuthClient> GetByClientIdAsync( string clientId )
        {
            return await Queryable().AsNoTracking().FirstOrDefaultAsync( ac => ac.ClientId == clientId );
        }

        /// <summary>
        /// Gets the by client identifier.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        public AuthClient GetByClientId( string clientId )
        {
            return Queryable().AsNoTracking().FirstOrDefault( ac => ac.ClientId == clientId );
        }

        /// <summary>
        /// Gets the by post logout redirect URL.
        /// </summary>
        /// <param name="postLogoutRedirectUrl">The logout redirect URL.</param>
        /// <returns></returns>
        public async Task<AuthClient> GetByPostLogoutRedirectUrlAsync( string postLogoutRedirectUrl )
        {
            return await Queryable().AsNoTracking().FirstOrDefaultAsync( ac =>
                ac.PostLogoutRedirectUri.Equals( postLogoutRedirectUrl, StringComparison.OrdinalIgnoreCase ) );
        }

        /// <summary>
        /// Gets the by identifier and secret.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <returns></returns>
        public async Task<AuthClient> GetByClientIdAndSecretAsync( string clientId, string clientSecret )
        {
            var authClient = await GetByClientIdAsync( clientId );

            if ( authClient == null )
            {
                return null;
            }

            var entityTypeName = EntityTypeCache.Get<Security.Authentication.Database>().Name;
            var databaseAuth = AuthenticationContainer.GetComponent( entityTypeName ) as Security.Authentication.Database;
            var success = databaseAuth.IsBcryptMatch( authClient.ClientSecretHash, clientSecret );

            return success ? authClient : null;
        }
    }
}