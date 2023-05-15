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
using System.Threading.Tasks;using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// A run once job for V12.4
    /// </summary>
    [DisplayName( "Rock Update Helper v12.4 - Update Group Salutation fields on Rock.Model.Group." )]
    [Description( "Updates Group Salutation fields on Rock.Model.Group." )]

    public class PostV124DataMigrationsUpdateGroupSalutations : RockJob
    {
        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            var familyGroupTypeId = GroupTypeCache.GetFamilyGroupType().Id;

            var personIdListWithFamilyId = new PersonService( new RockContext() ).Queryable( true, true ).Where( a => a.PrimaryFamilyId.HasValue ).Select( a => new { a.Id, a.PrimaryFamilyId } ).ToArray();
            var recordsUpdated = 0;

            // we only need one person from each family (and it doesn't matter who)
            var personIdList = personIdListWithFamilyId.GroupBy( a => a.PrimaryFamilyId.Value ).Select( s => s.FirstOrDefault()?.Id ).Where( a => a.HasValue ).Select( s => s.Value ).ToList();

            foreach ( var personId in personIdList )
            {
                try
                {
                    using ( var rockContext = new RockContext() )
                    {
                        // Ensure the person's primary family has a group name set set one if it doesn't.
                        CheckFamilyGroupName( personId, rockContext );

                        // Update the Group Salutations
                        recordsUpdated += PersonService.UpdateGroupSalutations( personId, rockContext );
                    }
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( new Exception( $"Error running the job 'PostV124DataMigrationsUpdateGroupSalutations'. UpdateGroupSalutations failed for person ID {personId}", ex ) );
                }
            }

            ServiceJobService.DeleteJob( this.ServiceJobId );
        }

        /// <summary>
        /// Checks the primary family of the person to ensure it has a group name.
        /// If not then the string "{LastName} Family" is used where the LastName is the LastName of the GivingLeader.
        /// If no GivingLeader is found then the string "Family" is used.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        private void CheckFamilyGroupName( int personId, RockContext rockContext )
        {
            // Get the person
            var personService = new PersonService( rockContext );
            var person = personService.Get( personId );
            var group = person.PrimaryFamily;
            if ( group.Name.IsNotNullOrWhiteSpace() )
            {
                return;
            }

            // The group doesn't have a name so we'll need to create one
            group.Name = "Family";
            var givingLeader = personService.Get( person.GivingLeaderId );
            if ( givingLeader != null )
            {
                group.Name = $"{givingLeader.LastName} Family";
            }

            rockContext.SaveChanges();
        }
    }
}
