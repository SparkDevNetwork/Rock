using System.Collections.Generic;
using System.Linq;

namespace Rock.Model
{
    public partial class GroupLocationHistoricalService
    {
        /// <summary>
        /// Sets the group location identifier to null for group location identifier.
        /// Also sets the ExpireDateTime to the current time and CurrentRowIndicator to false.
        /// Changes are made on the Context, the caller is responsible to save changes.
        /// </summary>
        /// <param name="groupLocationId">The group location identifier.</param>
        /// <returns></returns>
        public bool SetGroupLocationIdToNullForGroupLocationId( int groupLocationId )
        {
            var rockContext = this.Context as Rock.Data.RockContext;
            List<GroupLocationHistorical> groupLocationHistoricalList =
                Queryable()
                .Where( h => h.GroupLocationId == groupLocationId )
                .ToList();

            foreach( var groupLocationHistorical in groupLocationHistoricalList )
            {
                groupLocationHistorical.GroupLocationId = null;
                groupLocationHistorical.ExpireDateTime = RockDateTime.Now;
                groupLocationHistorical.CurrentRowIndicator = false;
            }

            return true;
        }
    }
}
