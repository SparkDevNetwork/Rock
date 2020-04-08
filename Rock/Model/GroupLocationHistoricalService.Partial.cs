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
