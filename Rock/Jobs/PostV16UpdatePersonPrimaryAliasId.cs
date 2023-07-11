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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using System;
using System.ComponentModel;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v16 to update the newly added PrimaryAliasId columns on the Person table.
    /// </summary>
    /// <seealso cref="Rock.Jobs.RockJob" />
    [DisplayName( "Rock Update Helper v16.0 - Update Person PrimaryAliasId column." )]
    [Description( "This job update all empty PrimaryAliasId columns on the Person table with their corresponding PersonAliasId values." )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]
    public class PostV16UpdatePersonPrimaryAliasId : RockJob
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
DECLARE @personsWithoutPrimaryAlias INT = (SELECT COUNT(*)
	FROM Person
	LEFT JOIN PersonAlias
	ON Person.Id = PersonAlias.AliasPersonId
	WHERE PersonAlias.AliasPersonId IS NULL);

SET @results = 1
SET @batchSize = 5000
SET @batchId = 0

WHILE ((SELECT COUNT(*) FROM Person WHERE PrimaryAliasId IS NULL) > @personsWithoutPrimaryAlias)
	BEGIN
		UPDATE Person 
        SET PrimaryAliasId = (SELECT TOP 1 Id FROM [PersonAlias] WHERE AliasPersonId = Person.Id)
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
