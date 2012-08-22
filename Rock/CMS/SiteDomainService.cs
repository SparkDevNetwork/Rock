//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.CMS
{
	/// <summary>
	/// Site Domain POCO Service class
	/// </summary>
    public partial class SiteDomainService : Service<SiteDomain, SiteDomainDTO>
    {
		/// <summary>
		/// Gets Site Domain by Domain
		/// </summary>
		/// <param name="domain">Domain.</param>
		/// <returns>SiteDomain object.</returns>
	    public SiteDomain GetByDomain( string domain )
        {
            return Repository.FirstOrDefault( t => t.Domain == domain );
        }
		
		/// <summary>
		/// Gets Site Domains by Site Id
		/// </summary>
		/// <param name="siteId">Site Id.</param>
		/// <returns>An enumerable list of SiteDomain objects.</returns>
	    public IEnumerable<SiteDomain> GetBySiteId( int siteId )
        {
            return Repository.Find( t => t.SiteId == siteId );
        }
		
		/// <summary>
		/// Gets Site Domains by Site Id And Domain
		/// </summary>
		/// <param name="siteId">Site Id.</param>
		/// <param name="domain">Domain.</param>
		/// <returns>An enumerable list of SiteDomain objects.</returns>
	    public IEnumerable<SiteDomain> GetBySiteIdAndDomain( int siteId, string domain )
        {
            return Repository.Find( t => t.SiteId == siteId && t.Domain == domain );
        }

        /// <summary>
        /// Gets the site domains that are contained by the specified domain
        /// </summary>
        /// <param name="domain">The full domain.</param>
        /// <returns></returns>
        public SiteDomain GetByDomainContained( string domain )
        {
            return Repository.FirstOrDefault( t => domain.ToLower().Contains( t.Domain.ToLower() ) );
        }

        /// <summary>
        /// Creates a new model
        /// </summary>
        /// <returns></returns>
        public override SiteDomain CreateNew()
        {
            return new SiteDomain();
        }

        /// <summary>
        /// Query DTO objects
        /// </summary>
        /// <returns>A queryable list of related DTO objects.</returns>
        public override IQueryable<SiteDomainDTO> QueryableDTO()
        {
            return this.Queryable().Select( m => new SiteDomainDTO( m ) );
        }
    }
}
