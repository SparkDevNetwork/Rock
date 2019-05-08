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
using System.Data;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Dynamic;
using Rock.Chart;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PersonScheduleExclusionService
    {
        /// <summary>
        /// Determines whether [is exclusion date] [the specified person identifier].
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="date">The date.</param>
        /// <returns>
        ///   <c>true</c> if [is exclusion date] [the specified person identifier]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsExclusionDate( int personId, int groupId, DateTime date )
        {
            var exclusions = Queryable()
            .AsNoTracking()
            .Where ( e => e.PersonAlias.PersonId == personId )
            .Where ( e => e.GroupId == groupId || e.GroupId == null )
            .Where( e => e.StartDate >= DbFunctions.TruncateTime( date ) && e.EndDate <= DbFunctions.TruncateTime( date )  )
            .Select( e => e.Id )
            .ToList();

            return exclusions.Any();
        }
    }
}
