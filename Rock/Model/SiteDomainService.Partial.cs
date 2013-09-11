//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

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
            return Repository.FirstOrDefault( t => t.Domain == domain );
        }
        
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.SiteDomain">SiteDomains</see> the Id of the <see cref="Rock.Model.Site"/> that they reference.
        /// </summary>
        /// <param name="siteId">An <see cref="System.Int32"/> that contains the Id a <see cref="Rock.Model.Site"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.SiteDomain">SiteDomains</see> that reference the provided SiteId.</returns>
        public IEnumerable<SiteDomain> GetBySiteId( int siteId )
        {
            return Repository.Find( t => t.SiteId == siteId );
        }
        
        /// <summary>
        /// Returns an enumerable list of <see cref="Rock.Model.SiteDomain"/> entities by the Id of the <see cref="Rock.Model.Site"/> and domain name.
        /// </summary>
        /// <param name="siteId">An <see cref="System.Int32"/> containing the Id of the <see cref="Rock.Model.Site"/> to search by.</param>
        /// <param name="domain">A <see cref="System.String"/> containing the domain to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.SiteDomain">SiteDomains</see> that match the SiteId and Domain that was provided..</returns>
        public IEnumerable<SiteDomain> GetBySiteIdAndDomain( int siteId, string domain )
        {
            return Repository.Find( t => t.SiteId == siteId && t.Domain == domain );
        }

        /// <summary>
        /// Returns the first matching of <see cref="Rock.Model.SiteDomain">SiteDomains</see> where the domain property contains the provided string.
        /// </summary>
        /// <param name="domain">A <see cref="System.String"/> containing a partial domain name to search by.</param>
        /// <returns>The first matching <see cref="Rock.Model.SiteDomain"/> where the domain partially/fully matches the provided value. </returns>
        public SiteDomain GetByDomainContained( string domain )
        {
            return Repository.FirstOrDefault( t => domain.ToLower().Contains( t.Domain.ToLower() ) );
        }
    }
}
