//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

using Rock.CheckIn;
using Rock.Model;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Kiosk REST API
    /// </summary>
    public partial class KioskController : ApiController
    {
        public virtual KioskStatus Get( int kioskId )
        {
            return Get( kioskId, null );
        }

        public virtual KioskStatus Get( int kioskId, List<int> groupTypeIds )
        {
            if (KioskCache.Kiosks.ContainsKey(kioskId))
            {
                // Clone the kioskstatus, but only copy group types that have been selected by admin
                var masterKioskStatus = KioskCache.Kiosks[kioskId];
                var configuredkioskStatus = new KioskStatus();
                configuredkioskStatus.FromDictionary( masterKioskStatus.ToDictionary() );

                foreach ( var groupType in masterKioskStatus.GroupTypes )
                {
                    if ( groupTypeIds.Contains( groupType.Value.Id ) )
                    {
                        configuredkioskStatus.GroupTypes.Add( groupType.Key, groupType.Value );
                    }
                }

                return configuredkioskStatus;
            }
            
            return null;
        }
    }
}
