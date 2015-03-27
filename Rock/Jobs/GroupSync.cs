// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Linq;
using System.Text;
using System.Web;
using System.Data.Entity;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to run quick SQL queries on a schedule
    /// </summary>
    [DisallowConcurrentExecution]
    public class GroupSync : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public GroupSync()
        {
        }

        /// <summary>
        /// Job that will sync groups.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            int targetIndex = 0;

            try
            {                
                // get groups set to sync
                RockContext rockContext = new RockContext();
                var groupsThatSync = new GroupService( rockContext ).Queryable().Where( g => g.SyncDataViewId != null ).ToList();

                foreach ( var syncGroup in groupsThatSync )
                {
                    var syncSource = new DataViewService(rockContext).Get(syncGroup.SyncDataViewId.Value);

                    // ensure this is a person dataview
                    bool isPersonDataSet = syncSource.EntityTypeId == EntityTypeCache.Read( typeof( Rock.Model.Person ) ).Id;

                    if ( isPersonDataSet )
                    {
                        SortProperty sortById = new SortProperty();
                        sortById.Property = "Id";
                        sortById.Direction = System.Web.UI.WebControls.SortDirection.Ascending;
                        List<string> errorMessages = new List<string>();

                        var sourceItems = syncSource.GetQuery( sortById, 180, out errorMessages );
                        var targetItems = syncGroup.Members.OrderBy( g => g.Id ).ToList();

                        if (targetItems== null) {
                            targetIndex = -1;
                        }

                        foreach( var sourceItem in sourceItems) {
                            if ( (sourceItem.Id > targetItems[targetIndex].Id) && targetIndex != -1 )
                            {
                                // remove target item

                                targetIndex++;
                                if ( targetItems.Count < targetIndex )
                                {
                                    targetIndex = -1;
                                }
                            }
                            else if (sourceItem.Id < targetItems[targetIndex].Id)
                            {
                                // add source to target
                            }
                        }

                    }
                }

            }
            catch ( System.Exception ex )
            {
                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( ex, context2 );
                throw ex;
            }
        }

    }
}
