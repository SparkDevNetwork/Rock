//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;
using System.IO;

namespace Rock.CMS
{
	public partial class SiteDomainService
	{
        /// <summary>
        /// Gets the site domains that are contained by the specified domain
        /// </summary>
        /// <param name="domain">The full domain.</param>
        /// <returns></returns>
        public Rock.CMS.SiteDomain GetByDomainContained( string domain )
        {
            return Repository.FirstOrDefault( t => domain.ToLower().Contains( t.Domain.ToLower() ) );
        }
    }
}