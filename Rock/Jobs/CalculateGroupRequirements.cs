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
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;

using Quartz;

using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to update Group Members' Group Requirement statuses for requirements that are calculated from SQL or DataView
    /// </summary>
    [DisplayName( "Calculate Group Requirements" )]
    [Description( "Calculate Group Requirements for group members that are in groups that have group requirements." )]

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
            var groupService = new GroupService( rockContext );

            // we only need to consider group requirements that are based on a DataView or SQL
            var groupRequirementQry = groupRequirementService.Queryable()
                .Where( a => a.GroupRequirementType.RequirementCheckType != RequirementCheckType.Manual )
                .AsNoTracking();

            var calculationExceptions = new List<Exception>();
            List<int> groupRequirementsCalculatedPersonIds = new List<int>();

            foreach ( var groupRequirement in groupRequirementQry.Include( i => i.GroupRequirementType ).Include( a => a.GroupRequirementType.DataView ).Include( a => a.GroupRequirementType.WarningDataView ).AsNoTracking().ToList() )
            {
                // Only calculate group requirements for Active groups (if an inactive group becomes active again, this job will take care of re-calculating the requirements again)
                var groupQuery = groupService.Queryable().Where( a => a.IsActive );
                if ( groupRequirement.GroupId.HasValue )
                {
                    groupQuery = groupQuery.Where( g => g.Id == groupRequirement.GroupId );
                }
                else if ( groupRequirement.GroupTypeId.HasValue )
                {
                    groupQuery = groupQuery.Where( g => g.GroupTypeId == groupRequirement.GroupTypeId );
                }
                else
                {
                    // shouldn't happen, but Group Requirement doesn't have a groupId or a GroupTypeId
                    break;
                }

                var groupList = groupQuery.Select( a => new { a.Id, a.Name } ).ToList();
                var groupCount = groupList.Count();
                foreach ( var group in groupList )
                {
                    context.UpdateLastStatusMessage( $"Calculating group requirement '{groupRequirement.GroupRequirementType.Name}' for {group.Name}" );
                    try
                    {
                        var currentDateTime = RockDateTime.Now;
                        var qryGroupMemberRequirementsAlreadyOK = groupMemberRequirementService.Queryable().Where( a => a.GroupRequirementId == groupRequirement.Id && a.GroupMember.GroupId == group.Id );

                        if ( groupRequirement.GroupRequirementType.CanExpire && groupRequirement.GroupRequirementType.ExpireInDays.HasValue )
                        {
                            // Group requirement can expire: don't recalculate members that already met the requirement within the expire days (unless they are flagged with a warning)
                            var expireDaysCount = groupRequirement.GroupRequirementType.ExpireInDays.Value;
                            qryGroupMemberRequirementsAlreadyOK = qryGroupMemberRequirementsAlreadyOK.Where( a => !a.RequirementWarningDateTime.HasValue && a.RequirementMetDateTime.HasValue && SqlFunctions.DateDiff( "day", a.RequirementMetDateTime, currentDateTime ) < expireDaysCount );
                        }
                        else
                        {
                            // No Expiration: don't recalculate members that already met the requirement
                            qryGroupMemberRequirementsAlreadyOK = qryGroupMemberRequirementsAlreadyOK.Where( a => a.RequirementMetDateTime.HasValue );
                        }

                        var groupMemberQry = groupMemberService.Queryable();

                        if ( groupRequirement.GroupId.HasValue )
                        {
                            groupMemberQry = groupMemberQry.Where( g => g.GroupId == groupRequirement.GroupId );
                        }
                        else if ( groupRequirement.GroupTypeId.HasValue )
                        {
                            groupMemberQry = groupMemberQry.Where( g => ( g.Group.GroupTypeId == groupRequirement.GroupTypeId ) && g.GroupId == group.Id );
                        }
                        else
                        {
                            // shouldn't happen, but Group Requirement doesn't have a groupId or a GroupTypeId
                            break;
                        }


                        var personQry = groupMemberQry.Where( a => !qryGroupMemberRequirementsAlreadyOK.Any( r => r.GroupMemberId == a.Id ) ).Select( a => a.Person );


                        var results = groupRequirement.PersonQueryableMeetsGroupRequirement( rockContext, personQry, group.Id, groupRequirement.GroupRoleId ).ToList();

                        groupRequirementsCalculatedPersonIds.AddRange( results.Select( a => a.PersonId ).Distinct() );
                        foreach ( var result in results )
                        {
                            try
                            {
                                // use a fresh rockContext per result so that ChangeTracker doesn't get bogged down
                                using ( var rockContextUpdate = new RockContext() )
                                {
                                    groupRequirement.UpdateGroupMemberRequirementResult( rockContextUpdate, result.PersonId, group.Id, result.MeetsGroupRequirement );
                                    rockContextUpdate.SaveChanges();
                                }
                            }
                            catch ( Exception ex )
                            {
                                calculationExceptions.Add( new Exception( $"Exception when updating group requirement result: {groupRequirement} for person.Id { result.PersonId }" , ex ) );
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        calculationExceptions.Add( new Exception( string.Format( "Exception when calculating group requirement: {0} ", groupRequirement ), ex ) );
                    }
                }
            }

            context.UpdateLastStatusMessage( $"{groupRequirementQry.Count()} group member requirements re-calculated for {groupRequirementsCalculatedPersonIds.Distinct().Count()} people" );

            if ( calculationExceptions.Any() )
            {
                throw new AggregateException( "One or more group requirement calculations failed ", calculationExceptions );
            }
        }
    }
}
