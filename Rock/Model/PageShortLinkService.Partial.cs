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
using System.Data.Entity;
using System.Linq;

using Rock;

namespace Rock.Model
{
    /// <summary>
    /// PageShortLink data access/service class. 
    /// </summary>
    public partial class PageShortLinkService
    {
        /// <summary>
        /// Gets a url by token. If more than one url exists for token (multiple sites)
        /// get the one that matches the included site id.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <returns></returns>
        public PageShortLink GetByToken( string token, int siteId )
        {
            var items = this.Queryable().Where( s => s.Token == token ).ToList();
            if ( items.Any() )
            {
                return items.Where( s => s.SiteId == siteId ).FirstOrDefault() ?? items.First();
            }

            return null;
        }

        /// <summary>
        /// Gets the unique token.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public string GetUniqueToken( int siteId, int length )
        {
            string token = PageShortLink.GetRandomToken( length );

            while ( this.Queryable().AsNoTracking()
                .Any( t => 
                    t.SiteId == siteId &&
                    t.Token == token ) )
            {
                token = PageShortLink.GetRandomToken( length );
            }

            return token;
        }

        /// <summary>
        /// Verifies if the token is unique
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public bool VerifyUniqueToken( int siteId, int id, string token )
        {
            return !this.Queryable().AsNoTracking()
                .Any( t =>
                    t.SiteId == siteId &&
                    t.Token == token &&
                    t.Id != id );
        }

    }
}
