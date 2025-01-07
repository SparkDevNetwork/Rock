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

using System.ComponentModel;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs.PostUpdateJobs
{
    /// <summary>
    /// Run once job for v17 to update the newly added PrimaryAliasGuid columns on the Person table.
    /// </summary>
    /// <seealso cref="Rock.Jobs.RockJob" />
    [DisplayName( "Rock Update Helper v17.0 - Update Person PrimaryAliasGuid column." )]
    [Description( "This job updates all empty PrimaryAliasGuid columns on the Person table with their corresponding PersonAliasGuid values." )]
    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]
    public class PostV17UpdatePersonPrimaryAliasGuid : PostUpdateJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc />
        public override void Execute()
        {
            // get the configured timeout, or default to 240 minutes if it is blank
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;
            var jobMigration = new JobMigration( commandTimeout );
            jobMigration.Sql( @"
DECLARE @batchId INT
DECLARE @batchSize INT
DECLARE @results INT
DECLARE @personsWithoutPrimaryAliasGuid INT = (SELECT COUNT(*)
	FROM dbo.[Person]
	LEFT JOIN dbo.[PersonAlias]
	ON Person.Id = PersonAlias.AliasPersonId
	WHERE PersonAlias.AliasPersonGuid IS NULL);
SET @results = 1
SET @batchSize = 5000
SET @batchId = 0
-- Run batches until all persons without a PrimaryAliasGuid have had their PrimaryAliasGuid set.
WHILE ((SELECT COUNT(*) FROM dbo.[Person] WHERE PrimaryAliasGuid IS NULL) > @personsWithoutPrimaryAliasGuid)
	BEGIN
		UPDATE dbo.[Person] 
        SET PrimaryAliasGuid = (SELECT TOP 1 [Guid] FROM dbo.[PersonAlias] WHERE AliasPersonId = [Person].Id)
        WHERE ([Person].Id > @batchId AND [Person].Id <= @batchId + @batchSize)
	
		-- next batch
		SET @batchId = @batchId + @batchSize
	END
" );
            DeleteJob();
        }

        /// <summary>
        /// Deletes the job.
        /// </summary>
        private void DeleteJob()
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( GetJobId() );
                if ( job != null )
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges();
                }
            }
        }
    }
}
