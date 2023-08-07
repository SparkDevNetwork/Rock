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
using System.Data.SqlClient;
using System.IO;
using System.Linq;

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Updates the ValueAs___ AttributeValue columns that were destroyed and re-created
    /// during the upgrade to 14.1.
    /// </summary>
    [DisplayName( "Rock Update Helper v14.1 - Update ValueAs Columns" )]
    [Description( "Updates the ValueAs___ AttributeValue columns that were converted away from computed columns." )]

    [IntegerField(
        "Command Timeout",
        Description = "Maximum amount of time (in seconds) to wait for the SQL Query to complete. Leave blank to use the default for this job (3600). Note, it could take several minutes, so you might want to set it at 3600 (60 minutes) or higher",
        IsRequired = false,
        DefaultIntegerValue = AttributeDefaults.CommandTimeout,
        Category = "General",
        Order = 1,
        Key = AttributeKey.CommandTimeout )]
    public class PostV141UpdateValueAsColumns : RockJob
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The command timeout
            /// </summary>
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// Attribute value defaults
        /// </summary>
        private static class AttributeDefaults
        {
            /// <summary>
            /// The command timeout
            /// </summary>
            public const int CommandTimeout = 60 * 60;
        }

        #endregion Keys

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        public override void Execute()
        {
            // get the configured timeout, or default to 3600 minutes if it is blank
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 3600;
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();

            var jobMigration = new JobMigration( commandTimeout );
            var migrationHelper = new MigrationHelper( jobMigration );

            int highestAttributeValueId = 0;

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;
                highestAttributeValueId = new AttributeValueService( rockContext ).Queryable()
                    .Max( av => av.Id );
            }

            int baseAttributeValueId = 0;
            int chunkSize = 100_000;

            // Update all the attribute values in chunks to reduce the risk
            // of SQL timeouts.
            while ( baseAttributeValueId <= highestAttributeValueId )
            {
                int maxValueId = Math.Min( highestAttributeValueId, baseAttributeValueId + chunkSize );

                UpdateAttributeValuesInRange( baseAttributeValueId, maxValueId, commandTimeout );

                baseAttributeValueId = maxValueId + 1;
            }

            // Do one more chunk to catch any attribute values that were created
            // while we were already processing.
            UpdateAttributeValuesInRange( baseAttributeValueId, null, commandTimeout );

            // Log how long it took us to run.
            var logMessage = $"{RockDateTime.Now:MM/dd/yyyy HH:mm:ss.fff},[{stopWatch.Elapsed.TotalMilliseconds,5:#} ms],{this.GetType().FullName}";
            WriteToLog( logMessage );

            ServiceJobService.DeleteJob( GetJobId() );
        }

        /// <summary>
        /// Updates the attributes between inclusive baseAttributeValueId
        /// and inclusive highestAttributeValueId.
        /// </summary>
        /// <param name="baseAttributeValueId">The lower attribute value identifier.</param>
        /// <param name="highestAttributeValueId">The upper attribute value identifier.</param>
        /// <param name="commandTimeout">The number of seconds to allow the command to execute.</param>
        private void UpdateAttributeValuesInRange( int baseAttributeValueId, int? highestAttributeValueId, int commandTimeout )
        {
            var booleanQuery = @"
UPDATE [AttributeValue]
SET [ValueAsBoolean] = (case when [Value] IS NULL OR [Value]='' OR len([Value])>len('false') then NULL when lower([Value])='1' OR lower([Value])='y' OR lower([Value])='t' OR lower([Value])='yes' OR lower([Value])='true' then CONVERT([bit],(1)) else CONVERT([bit],(0)) end)
WHERE [Id] >= @LowerId
  AND (@UpperId IS NULL OR [Id] <= @UpperId)
";

            // Note: We use a LEFT(40) in this query to prevent SQL data
            // truncation errors on really long attribute values. We don't
            // truncate to 36 (the length of a guid) so that it will still fail
            // if it looks like a Guid, but has other data after it.

            var personIdQuery = @"
UPDATE [AV]
SET [AV].[ValueAsPersonId] = [P].[Id]
FROM [AttributeValue] AS [AV]
LEFT OUTER JOIN [PersonAlias] AS [PA] ON [PA].[Guid] = TRY_CAST(LEFT([AV].[Value], 40) AS uniqueidentifier)
LEFT OUTER JOIN [Person] AS [P] ON [P].[Id] = [PA].[PersonId]
INNER JOIN [Attribute] AS [A] ON [A].[Id] = [AV].[AttributeId]
INNER JOIN [FieldType] AS [FT] ON [FT].[Id] = [A].[FieldTypeId]
WHERE [AV].[Id] >= @LowerId
  AND (@UpperId IS NULL OR [AV].[Id] <= @UpperId)
  AND [FT].[Guid] = @PersonFieldTypeGuid
";

            var lowerId = new SqlParameter( "@LowerId", baseAttributeValueId );
            var upperId = new SqlParameter( "@UpperId", highestAttributeValueId.HasValue ? ( object ) highestAttributeValueId.Value : DBNull.Value );
            var personFieldTypeGuid = new SqlParameter( "@PersonFieldTypeGuid", SystemGuid.FieldType.PERSON.AsGuid() );

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                rockContext.Database.ExecuteSqlCommand( booleanQuery, lowerId, upperId );
                rockContext.Database.ExecuteSqlCommand( personIdQuery, lowerId, upperId, personFieldTypeGuid );
            }
        }

        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="message">The message.</param>
        private void WriteToLog( string message )
        {
            if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
            {
                System.Diagnostics.Debug.WriteLine( message );
            }

            try
            {
                string directory = AppDomain.CurrentDomain.BaseDirectory;
                directory = Path.Combine( directory, "App_Data", "Logs" );

                if ( !Directory.Exists( directory ) )
                {
                    Directory.CreateDirectory( directory );
                }

                string filePath = Path.Combine( directory, "MigrationLog.csv" );
                using ( var logFile = new StreamWriter( filePath, true ) )
                {
                    logFile.WriteLine( message );
                }
            }
            catch
            {
                // Intentionally ignored, don't error if we couldn't log.
            }
        }
    }
}
