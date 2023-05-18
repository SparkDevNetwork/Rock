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
    /// Run once job for v15 to perform additional steps for System Phone Numbers.
    /// </summary>
    [DisplayName( "Rock Update Helper v15.0 - System Phone Numbers" )]
    [Description( "This job will add indexes related to SystemPhoneNumber conversion." )]

    [IntegerField(
    "Command Timeout",
    Key = AttributeKey.CommandTimeout,
    Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.",
    IsRequired = false,
    DefaultIntegerValue = 14400 )]
    public class PostV15DataMigrationsSystemPhoneNumbers : RockJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            // get the configured timeout, or default to 240 minutes if it is blank
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;
            var jobMigration = new JobMigration( commandTimeout );
            var migrationHelper = new MigrationHelper( jobMigration );

            CreateIndexes( migrationHelper );
            SetInitialColumnValues( jobMigration );

            DeleteJob();
        }

        /// <summary>
        /// Creates the indexes on existing tables.
        /// </summary>
        /// <param name="migrationHelper">The migration helper.</param>
        private void CreateIndexes( MigrationHelper migrationHelper )
        {
            migrationHelper.CreateIndexIfNotExists( "Communication", new[] { "SmsFromSystemPhoneNumberId" }, Array.Empty<string>() );
            migrationHelper.CreateIndexIfNotExists( "CommunicationResponse", new[] { "RelatedSmsFromSystemPhoneNumberId" }, Array.Empty<string>() );
        }

        private void SetInitialColumnValues( JobMigration jobMigration )
        {
            // Set the initial value on SystemCommunication.SmsFromSystemPhoneNumberId.
            jobMigration.Sql( @"
UPDATE [SC]
    SET [SC].[SmsFromSystemPhoneNumberId] = [SPN].[Id]
FROM [SystemCommunication] AS [SC]
INNER JOIN [DefinedValue] AS [DV] ON [DV].[Id] = [SC].[SMSFromDefinedValueId]
INNER JOIN [SystemPhoneNumber] AS [SPN] ON [SPN].[Guid] = [DV].[Guid]
WHERE [SC].[SMSFromDefinedValueId] IS NOT NULL
  AND [SC].[SmsFromSystemPhoneNumberId] IS NULL
" );

            // Set the initial value on CommunicationTemplate.SmsFromSystemPhoneNumberId.
            jobMigration.Sql( @"
UPDATE [CT]
    SET [CT].[SmsFromSystemPhoneNumberId] = [SPN].[Id]
FROM [CommunicationTemplate] AS [CT]
INNER JOIN [DefinedValue] AS [DV] ON [DV].[Id] = [CT].[SMSFromDefinedValueId]
INNER JOIN [SystemPhoneNumber] AS [SPN] ON [SPN].[Guid] = [DV].[Guid]
WHERE [CT].[SMSFromDefinedValueId] IS NOT NULL
  AND [CT].[SmsFromSystemPhoneNumberId] IS NULL
" );

            // Set the initial value on Communication.SmsFromSystemPhoneNumberId.
            jobMigration.Sql( @"
UPDATE [C]
    SET [C].[SmsFromSystemPhoneNumberId] = [SPN].[Id]
FROM [Communication] AS [C]
INNER JOIN [DefinedValue] AS [DV] ON [DV].[Id] = [C].[SMSFromDefinedValueId]
INNER JOIN [SystemPhoneNumber] AS [SPN] ON [SPN].[Guid] = [DV].[Guid]
WHERE [C].[SMSFromDefinedValueId] IS NOT NULL
  AND [C].[SmsFromSystemPhoneNumberId] IS NULL
" );

            // Set the initial value on CommunicationResponse.RelatedSmsFromSystemPhoneNumberId.
            jobMigration.Sql( @"
UPDATE [CR]
    SET [CR].[RelatedSmsFromSystemPhoneNumberId] = [SPN].[Id]
FROM [CommunicationResponse] AS [CR]
INNER JOIN [DefinedValue] AS [DV] ON [DV].[Id] = [CR].[RelatedSmsFromDefinedValueId]
INNER JOIN [SystemPhoneNumber] AS [SPN] ON [SPN].[Guid] = [DV].[Guid]
WHERE [CR].[RelatedSmsFromDefinedValueId] IS NOT NULL
  AND [CR].[RelatedSmsFromSystemPhoneNumberId] IS NULL
" );
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
