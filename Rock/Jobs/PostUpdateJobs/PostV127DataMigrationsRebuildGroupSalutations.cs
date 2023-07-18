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
using System.ComponentModel;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// </summary>
    [DisplayName( "Rock Update Helper v12.7 - Rebuilds Group Salutation fields on Rock.Model.Group for all family groups." )]
    [Description( "Updates Group Salutation fields on Rock.Model.Group." )]
    public class PostV127DataMigrationsRebuildGroupSalutations : RockJob
    {
        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            var familyGroupTypeId = GroupTypeCache.GetFamilyGroupType().Id;

            var rockContext = new RockContext();

            // just in case there are Groups that have a null or empty Name, update them.
            var familiesWithoutNames = new GroupService( rockContext )
                .Queryable().Where( a => a.GroupTypeId == familyGroupTypeId )
                .Where( a => string.IsNullOrEmpty( a.Name ) );

            if ( familiesWithoutNames.Any() )
            {
                rockContext.BulkUpdate( familiesWithoutNames, g => new Group { Name = "Family" } );
            }

            // Re-calculates all GroupSalutation values on Family Groups.
            var familyIdList = new GroupService( rockContext )
                .Queryable().Where( a => a.GroupTypeId == familyGroupTypeId )
                .Select( a => a.Id ).ToList();

            foreach ( var familyId in familyIdList )
            {
                try
                {
                    using ( var rockContextUpdate = new RockContext() )
                    {
                        GroupService.UpdateGroupSalutations( familyId, rockContextUpdate );
                    }

                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( new Exception( $"Error running the job 'PostV127DataMigrationsRebuildGroupSalutations'. UpdateGroupSalutations failed for Group Id {familyId}", ex ) );
                }
            }


            ServiceJobService.DeleteJob( this.GetJobId() );
        }
    }
}
