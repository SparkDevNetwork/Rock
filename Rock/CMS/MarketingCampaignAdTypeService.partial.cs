//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rock.Data;

namespace Rock.Cms
{
    public partial class MarketingCampaignAdTypeService : Service<MarketingCampaignAdType, MarketingCampaignAdTypeDto>
    {
        /// <summary>
        /// Determines whether this instance can delete the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified id; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( int id, out string errorMessage )
        {
            // partially code generated from Dev Tools/Sql/CodeGen_CanDelete.sql

            RockContext context = new RockContext();
            context.Database.Connection.Open();
            bool canDelete = true;
            errorMessage = string.Empty;

            using ( var cmdCheckRef = context.Database.Connection.CreateCommand() )
            {
                cmdCheckRef.CommandText = string.Format( "select count(*) from cmsMarketingCampaignAd where MarketingCampaignAdTypeId = {0} ", id );
                var result = cmdCheckRef.ExecuteScalar();
                int? refCount = result as int?;
                if ( refCount > 0 )
                {
                    canDelete = false;
                    errorMessage = "This Marketing Campaign Ad Type is assigned to a Marketing Campaign Ad.";
                }
            }

            return canDelete;
        }

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        public override bool Delete( MarketingCampaignAdType item, int? personId )
        {
            string message;
            if ( !CanDelete( item.Id, out message ) )
            {
                return false;
            }

            return base.Delete( item, personId );
        }
    }
}