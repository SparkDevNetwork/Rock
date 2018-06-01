using System;
using System.Collections.Generic;
using System.Web.Http;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class GroupMemberHistoricalsController
    {
        /// <summary>
        /// Gets a summary of a person's membership in groups that have group history enabled
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="stopDateTime">The stop date time.</param>
        /// <param name="groupTypeIds">The comma-delimited list of group type ids (leave blank for all)</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/GroupMemberHistoricals/GetGroupHistoricalSummary" )]
        public List<GroupMemberHistoricalService.GroupHistoricalSummary> GetGroupHistoricalSummary(
            int personId,
            DateTime? startDateTime = null,
            DateTime? stopDateTime = null,
            string groupTypeIds = ""
            )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupMemberHistoricalService = new GroupMemberHistoricalService( rockContext );
                List<int> groupTypeIdList = groupTypeIds?.SplitDelimitedValues().AsIntegerList();
                return groupMemberHistoricalService.GetGroupHistoricalSummary( personId, startDateTime, stopDateTime, groupTypeIdList );
            }
        }
    }
}
