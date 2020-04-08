// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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
