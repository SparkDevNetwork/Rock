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

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v20.1 to add an index that keeps claiming the next communication recipient fast during large sends.
    /// </summary>
    [DisplayName( "Rock Update Helper v20.1 - Add Communication Recipient Claim Index" )]
    [Description( "This job will add an index to keep large communications sending at a steady speed." )]

    [IntegerField( "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]

    public class PostV201AddCommunicationRecipientClaimIndex : RockJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc />
        public override void Execute()
        {
            // Get the configured timeout, or default to 240 minutes if it is blank.
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;
            var jobMigration = new JobMigration( commandTimeout );

            // If the index already exists there is nothing to build, so skip the module check below.
            var isIndexPresent = jobMigration.SqlScalar( @"
SELECT COUNT(*)
FROM sys.indexes
WHERE [name] = N'IX_Unsent_CommunicationId_MediumEntityTypeId_Status'
    AND [object_id] = OBJECT_ID(N'[dbo].[CommunicationRecipient]');" ).ToStringSafe().AsInteger() > 0;

            if ( isIndexPresent )
            {
                DeleteJob();
                return;
            }

            /*
                10/30/2026 - MSE

                Once a table has any filtered index, SQL Server rejects every insert, update and delete on it
                (Msg 1934) from a session or module running with QUOTED_IDENTIFIER or ANSI_NULLS OFF. So before
                adding the filtered index below, skip the build if any SQL module that mentions the table was
                created with either setting OFF. The job then ends with a warning and is not deleted, so it
                stays visible and tries again on the next run.

                This check mostly duplicates protection that already exists: the v19.0 filtered index
                IX_UnsubscribeDateTime_SendDateTime (PostV19AddCommunicationUnsubscribeReportIndex) puts the
                same rule on this table, so any such module already fails wherever that index exists. It only
                adds real protection where that index is missing. It also cannot see ad hoc sessions (sqlcmd
                without -I, bcp without -q, SQL Agent steps, ETL tools); those must use the right SET options
                themselves.

                Reason: Do not break writes from legacy modules that use QUOTED_IDENTIFIER or ANSI_NULLS OFF.
            */
            var incompatibleModuleCount = jobMigration.SqlScalar( @"
SELECT COUNT(*)
FROM sys.sql_modules AS [m]
WHERE [m].[definition] LIKE N'%CommunicationRecipient%'
    AND ([m].[uses_quoted_identifier] = 0 OR [m].[uses_ansi_nulls] = 0);" ).ToStringSafe().AsInteger();

            if ( incompatibleModuleCount > 0 )
            {
                throw new RockJobWarningException( $"Skipped adding the IX_Unsent_CommunicationId_MediumEntityTypeId_Status index because {incompatibleModuleCount} SQL module(s) that reference CommunicationRecipient were created with QUOTED_IDENTIFIER or ANSI_NULLS OFF. Recreate them with both settings ON and run this job again." );
            }

            /*
                10/30/2026 - MSE

                Communication.GetNextPending claims one recipient per call. This index holds only Pending (0)
                and Sending (5) recipients, so each claim seeks straight to the next one to send instead of
                reading past every recipient already sent. The filter must match the literal statuses used in
                GetNextPending; see the note there.

                The build is offline like every other post-update job. It holds a shared lock on the table while
                it scans it, which is why this job runs at 2 AM instead of 9 PM, when large sends often run.

                Reason: Keep the cost of each claim constant for large sends (Fixes #7092).
            */
            jobMigration.Sql( @"
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE NAME = N'IX_Unsent_CommunicationId_MediumEntityTypeId_Status' AND object_id = OBJECT_ID(N'[dbo].[CommunicationRecipient]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Unsent_CommunicationId_MediumEntityTypeId_Status] ON [dbo].[CommunicationRecipient] (
        [CommunicationId] ASC,
        [MediumEntityTypeId] ASC,
        [Status] ASC
    )
    INCLUDE ([ModifiedDateTime])
    WHERE [Status] IN (0, 5);
END" );

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
