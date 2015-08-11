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
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using Quartz;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to update Group Members' Group Requirement statuses for requirements that are calculated from SQL or Dataview
    /// </summary>
    [DisallowConcurrentExecution]
    public class CalculateGroupRequirements : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public CalculateGroupRequirements()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            var rockContext = new RockContext();
            var groupRequirementService = new GroupRequirementService( rockContext );
            var groupMemberRequirementService = new GroupMemberRequirementService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );

            // we only need to consider group requirements that are based on a DataView or SQL
            var groupRequirementQry = groupRequirementService.Queryable()
                .Where( a => a.GroupRequirementType.RequirementCheckType != RequirementCheckType.Manual )
                .AsNoTracking();

            var calculationExceptions = new List<Exception>();

            foreach ( var groupRequirement in groupRequirementQry.Include( i => i.GroupRequirementType ).AsNoTracking().ToList() )
            {
                try
                {
                    var groupMemberQry = groupMemberService.Queryable().Where( a => a.GroupId == groupRequirement.GroupId ).AsNoTracking();
                    var personQry = groupMemberQry.Select( a => a.Person );
                    var currentDateTime = RockDateTime.Now;
                    var expireDaysCount = groupRequirement.GroupRequirementType.ExpireInDays.Value;
                    var qryGroupMemberRequirementsAlreadyOK = groupMemberRequirementService.Queryable().Where( a => a.GroupRequirementId == groupRequirement.Id );

                    if ( groupRequirement.GroupRequirementType.CanExpire && groupRequirement.GroupRequirementType.ExpireInDays.HasValue )
                    {
                        // Expirable: don't recalculate members that already met the requirement within the expiredays
                        qryGroupMemberRequirementsAlreadyOK = qryGroupMemberRequirementsAlreadyOK.Where( a => a.RequirementMetDateTime.HasValue && SqlFunctions.DateDiff( "day", a.RequirementMetDateTime, currentDateTime ) < expireDaysCount );
                    }
                    else
                    {
                        // No Expiration: don't recalculate members that already met the requirement
                        qryGroupMemberRequirementsAlreadyOK = qryGroupMemberRequirementsAlreadyOK.Where( a => a.RequirementMetDateTime.HasValue );
                    }

                    personQry = personQry.Where( a => !qryGroupMemberRequirementsAlreadyOK.Any( r => r.GroupMember.PersonId == a.Id ) );

                    var results = groupRequirement.PersonQueryableMeetsGroupRequirement( rockContext, personQry, groupRequirement.GroupRoleId ).ToList();
                    foreach ( var result in results )
                    {
                        // use a fresh rockContext per Update so that ChangeTracker doesn't get bogged down
                        var rockContextUpdate = new RockContext();
                        groupRequirement.UpdateGroupMemberRequirementResult( rockContextUpdate, result.PersonId, result.MeetsGroupRequirement );
                        rockContextUpdate.SaveChanges();
                    }
                }
                catch ( Exception ex )
                {
                    calculationExceptions.Add( new Exception( string.Format( "Exception when calculating group requirement: ", groupRequirement ), ex ) );
                }
            }

            if ( calculationExceptions.Any() )
            {
                throw new AggregateException( "One or more group requirement calculations failed ", calculationExceptions );
            }
        }
    }
}
