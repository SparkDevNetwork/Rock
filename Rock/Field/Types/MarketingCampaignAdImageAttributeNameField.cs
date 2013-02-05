//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    public class MarketingCampaignAdImageAttributeNameField : SelectFromListFieldType
    {
        /// <summary>
        /// Gets the list source.
        /// </summary>
        /// <value>
        /// The list source.
        /// </value>
        internal override Dictionary<string, string> ListSource
        {
            get
            {
                var service = new AttributeService();
                int entityTypeId = EntityTypeCache.GetId(typeof(MarketingCampaignAd).FullName) ?? 0;
                var qry = service.GetByEntityTypeId(entityTypeId).Where( a => a.FieldType.Guid.Equals( SystemGuid.FieldType.IMAGE ) );
                Dictionary<string, string> result = new Dictionary<string, string>();
                foreach ( var item in qry.ToList() )
                {
                    string key = item.Key;

                    // intentionally make Name = Key to avoid problems if the same key is used with different names
                    string name = item.Key;
                    if ( !result.ContainsKey( key ) )
                    {
                        result.Add( key, name );
                    }
                }
                return result;
            }
        }
    }
}
