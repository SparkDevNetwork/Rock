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

namespace Rock.Model
{
    /// <summary>
    /// Service/data access class for <see cref="MakretingCampaignAdType"/> entity objects
    /// </summary>
    public partial class MarketingCampaignAdTypeService
    {
        /// <summary>
        /// Deletes a specified <see cref="MarketingCampaignAdType"/>.
        /// </summary>
        /// <param name="item">The <see cref="MarketingCampaignAdType"/> to delete</param>
        /// <param name="personId">An <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> that is performing the deletion.</param>
        /// <returns>A <see cref="System.Boolean"/> flag that indicates if the deletion was completed successfully.</returns>
        public override bool Delete( MarketingCampaignAdType item, int? personId )
        {
            string message;
            if ( !CanDelete( item, out message ) )
            {
                return false;
            }

            return base.Delete( item, personId );
        }
    }
}