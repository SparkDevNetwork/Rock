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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;

namespace Rock.Model
{
    public partial class AssessmentService
    {
        /// <summary>
        /// Updates the last reminder date of the latest pending assessment of each type for the provided Person Alias ID.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        public void UpdateLastReminderDateForPersonAlias( int personAliasId )
        {
            var sqlParams = new Dictionary<string, object>();
            sqlParams.Add( "@lastReminderDate", DateTime.Now );
            sqlParams.Add( "@status", AssessmentRequestStatus.Pending );
            sqlParams.Add( "@personAliasId", personAliasId );

            string sql = $@"
                -- A pending assessment will stay in a pending status if it was never taken, even if a new one is requested.
                -- This query will take that into account.
                -- Uses ID instead of CreatedDate as the highest ID should also be the latest request. It also greatly simplifies the join.
                -- Get a list of Assessments for the latest pending assessments for the person alias ID
                WITH cte AS (
	                SELECT [AssessmentTypeId], [PersonAliasId], MAX( [Id] ) AS Id
	                FROM [Assessment]
	                WHERE [PersonAliasId] = @personAliasId AND [Status] = @status
	                GROUP BY [AssessmentTypeId], [PersonAliasId]
                )

                -- Update the latest pending assessments
                UPDATE [dbo].[Assessment]
                SET [LastReminderDate] = @lastReminderDate
                WHERE [Id] in (SELECT [Id] FROM cte)";

            DbService.ExecuteCommand( sql, System.Data.CommandType.Text, sqlParams, null );
        }

        /// <summary>
        /// Gets the latest assessment of each type for each person
        /// </summary>
        /// <returns></returns>
        public IQueryable<Assessment> GetLatestAssessments()
        {
            string sql = $@"
                -- A pending assessment will stay in a pending status if it was never taken, even if a new one is requested.
                -- This query will take that into account.
                -- Uses ID instead of CreatedDate as the highest ID should also be the latest request. It also simplifies the join.
                -- Get a list of the latest requested Assessment test of each type for each person

                SELECT [Assessment].[Id]
                FROM (
		                SELECT [AssessmentTypeId], [PersonAliasId], MAX( [Id] ) AS latestRequestId
		                FROM [Assessment]
		                GROUP BY [AssessmentTypeId], [PersonAliasId]
	                ) AS a1
                INNER JOIN [Assessment] ON [Assessment].[Id] = a1.latestRequestId";

            //var assessmentIds = this.ExecuteQuery( sql ).Select( a => a.Id ).ToList();
            var assessmentIds = this.Context.Database.SqlQuery<int>( sql );
            return base.Queryable().Where( a => assessmentIds.Contains( a.Id ) );
        }

        /// <summary>
        /// Gets the latest assessments for person. If the person does not have an alias ID it uses 0 and the queryable will not have any data.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IQueryable<Assessment> GetLatestAssessmentsForPerson( int personId )
        {
            return GetLatestAssessmentsForPersonAlias( new PersonAliasService( ( RockContext )this.Context ).GetPrimaryAliasId( personId ) ?? 0 );
        }

        /// <summary>
        /// Get the latest assessment of each type for a person alias.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns></returns>
        public IQueryable<Assessment> GetLatestAssessmentsForPersonAlias( int personAliasId )
        {
            System.Data.SqlClient.SqlParameter[] sqlParams = new System.Data.SqlClient.SqlParameter[]
            {
                new System.Data.SqlClient.SqlParameter( "@personAliasId", personAliasId )
            };

            string sql = $@"
                -- A pending assessment will stay in a pending status if it was never taken, even if a new one is requested.
                -- This query will take that into account.
                -- Uses ID instead of CreatedDate as the highest ID should also be the latest request. It also simplifies the join.
                -- Get a list of Assessments for the latest pending assessments for the person alias ID

                SELECT [Assessment].[Id]
                FROM (
		                SELECT [AssessmentTypeId], [PersonAliasId], MAX( [Id] ) AS latestRequestId
		                FROM [Assessment]
		                WHERE [PersonAliasId] = @personAliasId
		                GROUP BY [AssessmentTypeId], [PersonAliasId]
	                ) AS a1
                INNER JOIN [Assessment] ON [Assessment].[Id] = a1.latestRequestId";

            //var assessmentIds = this.ExecuteQuery( sql, sqlParams ).Select( a => a.Id ).ToList();
            var assessmentIds = this.Context.Database.SqlQuery<int>( sql, sqlParams );
            return base.Queryable().Where( a => assessmentIds.Contains( a.Id ) );
        }
    }
}
