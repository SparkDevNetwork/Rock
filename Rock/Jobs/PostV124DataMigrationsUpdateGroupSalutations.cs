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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// A run once job for V12.4
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [DisplayName( "Rock Update Helper v12.4 - Update Group Salutation fields on Rock.Model.Group." )]
    [Description( "Updates Group Salutation fields on Rock.Model.Group." )]

    public class PostV124DataMigrationsUpdateGroupSalutations : IJob
    {
        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            var familyGroupTypeId = GroupTypeCache.GetFamilyGroupType().Id;

            var personIdListWithFamilyId = new PersonService( new RockContext() ).Queryable( true, true ).Where( a => a.PrimaryFamilyId.HasValue ).Select( a => new { a.Id, a.PrimaryFamilyId } ).ToArray();
            var recordsUpdated = 0;

            // we only need one person from each family (and it doesn't matter who)
            var personIdList = personIdListWithFamilyId.GroupBy( a => a.PrimaryFamilyId.Value ).Select( s => s.FirstOrDefault()?.Id ).Where( a => a.HasValue ).Select( s => s.Value ).ToList();

            foreach ( var personId in personIdList )
            {
                using ( var rockContext = new RockContext() )
                {
                    recordsUpdated += PersonService.UpdateGroupSalutations( personId, rockContext );
                }
            }

            ServiceJobService.DeleteJob( context.GetJobId() );
        }
    }
}
