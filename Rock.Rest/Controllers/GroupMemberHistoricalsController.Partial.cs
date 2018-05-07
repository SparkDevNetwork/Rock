using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/GroupMemberHistoricals/GetGroupHistoricalSummary" )]
        public List<GroupMemberHistoricalService.GroupHistoricalSummary> GetGroupHistoricalSummary( int personId, DateTime? startDateTime = null, DateTime? stopDateTime = null )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupMemberHistoricalService = new GroupMemberHistoricalService( rockContext );
                return groupMemberHistoricalService.GetGroupHistoricalSummary( personId, startDateTime, stopDateTime );
            }
        }
    }
}
