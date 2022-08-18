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
using System.Linq;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.ServiceJobHistory"/> entity objects.
    /// </summary>
    public partial class ServiceJobHistoryService
    {
        /// <summary>
        /// Returns a queryable collection of all <see cref="Rock.Model.ServiceJobHistory">jobs history</see>
        /// </summary>
        /// <param name="serviceJobId">The service job identifier.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="stopDateTime">The stop date time.</param>
        /// <returns>A queryable collection of all <see cref="Rock.Model.ServiceJobHistory"/>jobs history</returns>
        public IQueryable<ServiceJobHistory> GetServiceJobHistory( int? serviceJobId, DateTime? startDateTime = null, DateTime? stopDateTime = null )
        {
            var ServiceJobHistoryQuery = this.AsNoFilter();

            if ( serviceJobId.HasValue )
            {
                ServiceJobHistoryQuery = ServiceJobHistoryQuery.Where( a => a.ServiceJobId == serviceJobId );
            }

            if ( startDateTime.HasValue )
            {
                ServiceJobHistoryQuery = ServiceJobHistoryQuery.Where( a => a.StartDateTime >= startDateTime.Value );
            }

            if ( stopDateTime.HasValue )
            {
                ServiceJobHistoryQuery = ServiceJobHistoryQuery.Where( a => a.StopDateTime < stopDateTime.Value );
            }

            return ServiceJobHistoryQuery.OrderBy( a => a.ServiceJobId ).ThenByDescending( a => a.StartDateTime );
        }

        /// <summary>
        /// Deletes job history items more than maximum.
        /// </summary>
        public void DeleteMoreThanMax()
        {
            ServiceJobService serviceJobService = new ServiceJobService( (RockContext)this.Context );
            var serviceJobIds = serviceJobService.AsNoFilter().Select( sj => sj.Id ).ToArray();
            foreach ( var serviceJobId in serviceJobIds)
            {
                DeleteMoreThanMax( serviceJobId );
            }
        }

        /// <summary>
        /// Deletes job history items more than maximum.
        /// </summary>
        /// <param name="serviceJobId">The service job identifier.</param>
        public void DeleteMoreThanMax( int serviceJobId )
        {
            var rockContext = this.Context as RockContext;
            var historyCount = new ServiceJobService( rockContext ).GetSelect( serviceJobId, s => s.HistoryCount );
            historyCount = historyCount <= 0 ? historyCount = 500 : historyCount;

            ServiceJobHistoryService serviceJobHistoryService = new ServiceJobHistoryService( rockContext );
            var serviceJobHistoryToDeleteQuery = serviceJobHistoryService.Queryable().Where( a => a.ServiceJobId == serviceJobId ).OrderByDescending( a => a.StartDateTime ).Skip( historyCount );
            rockContext.BulkDelete( serviceJobHistoryToDeleteQuery );
        }
    }
}
