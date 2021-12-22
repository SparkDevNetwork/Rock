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
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// SiteDomain data access/service class. 
    /// </summary>
    public partial class SiteDomainService 
    {
        /// <summary>
        /// Returns a  <see cref="Rock.Model.SiteDomain"/> by Domain.
        /// </summary>
        /// <param name="domain">A <see cref="System.String"/> containing a domain/URL to search by.</param>
        /// <returns>A <see cref="Rock.Model.SiteDomain"/> where the Domain matches the provided value. If not results found, will return null.</returns>
        public SiteDomain GetByDomain( string domain )
        {
            return Queryable().FirstOrDefault( t => t.Domain == domain );
        }
        
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.SiteDomain">SiteDomains</see> the Id of the <see cref="Rock.Model.Site"/> that they reference.
        /// </summary>
        /// <param name="siteId">An <see cref="System.Int32"/> that contains the Id a <see cref="Rock.Model.Site"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.SiteDomain">SiteDomains</see> that reference the provided SiteId.</returns>
        public IQueryable<SiteDomain> GetBySiteId( int siteId )
        {
            return Queryable().Where( t => t.SiteId == siteId );
        }
        
        /// <summary>
        /// Returns an enumerable list of <see cref="Rock.Model.SiteDomain"/> entities by the Id of the <see cref="Rock.Model.Site"/> and domain name.
        /// </summary>
        /// <param name="siteId">An <see cref="System.Int32"/> containing the Id of the <see cref="Rock.Model.Site"/> to search by.</param>
        /// <param name="domain">A <see cref="System.String"/> containing the domain to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.SiteDomain">SiteDomains</see> that match the SiteId and Domain that was provided..</returns>
        public IQueryable<SiteDomain> GetBySiteIdAndDomain( int siteId, string domain )
        {
            return Queryable().Where( t => t.SiteId == siteId && t.Domain == domain );
        }

        /// <summary>
        /// Returns the first matching of <see cref="Rock.Model.SiteDomain">SiteDomains</see> where the domain property contains the provided string.
        /// </summary>
        /// <param name="domain">A <see cref="System.String"/> containing a partial domain name to search by.</param>
        /// <returns>The first matching <see cref="Rock.Model.SiteDomain"/> where the domain partially/fully matches the provided value. </returns>
        public SiteDomain GetByDomainContained( string domain )
        {
            return Queryable().FirstOrDefault( t => domain.ToLower().Contains( t.Domain.ToLower() ) );
        }
    }
}
