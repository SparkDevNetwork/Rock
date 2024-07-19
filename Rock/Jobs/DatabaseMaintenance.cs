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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;

using Rock.Attribute;
using Rock.Communication;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.Configuration;
using Rock.Logging;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to run quick SQL queries on a schedule
    /// </summary>
    [DisplayName( "Database Maintenance" )]
    [Description( "Performs routine SQL Server database maintenance." )]

    #region Job Attributes

    [BooleanField(
        "Run Integrity Check",
        Key = AttributeKey.RunIntegrityCheck,
        Description ="Determines if an integrity check should be performed. (Integrity checks are never run on Azure SQL databases, because Azure manages its own data integrity.)",
        DefaultBooleanValue = true,
        Order = 0 )]

    [BooleanField(
        "Run Index Rebuild",
        Key = AttributeKey.RunIndexRebuild,
        Description = "Determines if indexes should be rebuilt.",
        DefaultBooleanValue = true,
        Order = 1 )]

    [BooleanField(
        "Run Statistics Update",
        Key = AttributeKey.RunStatisticsUpdate,
        Description = "Determines if the statistics should be updated.",
        DefaultBooleanValue = true,
        Order = 2 )]

    [TextField(
        "Alert Email",
        Key = AttributeKey.AlertEmail,
        Description = "Email address to send alerts to errors occur (multiple address delimited with comma).",
        IsRequired = true,
        Order = 3 )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each step to complete.",
        IsRequired = false,
        DefaultIntegerValue = 900,
        Category = "Advanced",
        Order = 4 )]

    [IntegerField(
        "Minimum Index Page Count",
        Key = AttributeKey.MinimumIndexPageCount,
        Description = "The minimum size in pages that an index must be before it's considered for being re-built. Default value is 100.",
        IsRequired = false,
        DefaultIntegerValue = 100,
        Category = "Advanced",
        Order = 5 )]

    [IntegerField(
        "Minimum Fragmentation Percentage",
        Key = AttributeKey.MinimumFragmentationPercentage,
        Description = "The minimum fragmentation percentage for an index to be considered for re-indexing. If the fragmentation is below is amount nothing will be done. Default value is 10%.",
        IsRequired = false,
        DefaultIntegerValue = 10,
        Category = "Advanced",
        Order = 6 )]

    [IntegerField(
        "Rebuild Threshold Percentage",
        Key = AttributeKey.RebuildThresholdPercentage,
        Description = "The threshold percentage where a REBUILD will be completed instead of a REORGANIZE. Default value is 30%.",
        IsRequired = false,
        DefaultIntegerValue = 30,
        Category = "Advanced",
        Order = 7 )]

    [BooleanField(
        "Use ONLINE Index Rebuild",
        Key = AttributeKey.UseOnlineIndexRebuild,
        Description = "Use the ONLINE option when rebuilding indexes. NOTE: This is only supported on SQL Enterprise and Azure SQL Database.",
        DefaultBooleanValue = false,
        Category = "Advanced",
        Order = 8 )]

    #endregion

    public class DatabaseMaintenance : RockJob
    {
        #region Keys

        /// <summary>
        /// Keys to use for job Attributes.
        /// </summary>
        private static class AttributeKey
        {
            public const string RunIntegrityCheck = "RunIntegrityCheck";
            public const string RunIndexRebuild = "RunIndexRebuild";
            public const string RunStatisticsUpdate = "RunStatisticsUpdate";
            public const string AlertEmail = "AlertEmail";
            public const string CommandTimeout = "CommandTimeout";
            public const string MinimumIndexPageCount = "MinimumIndexPageCount";
            public const string MinimumFragmentationPercentage = "MinimumFragmentationPercentage";
            public const string RebuildThresholdPercentage = "RebuildThresholdPercentage";
            public const string UseOnlineIndexRebuild = "UseOnlineIndexRebuild";
        }

        #endregion

        private readonly IDatabaseConfiguration _databaseConfiguration;

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public DatabaseMaintenance()
        {
            _databaseConfiguration = RockApp.Current.GetDatabaseConfiguration();
        }

        private List<DatabaseMaintenanceTaskResult> _databaseMaintenanceTaskResults = new List<DatabaseMaintenanceTaskResult>();

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            // get job parms
            bool runIntegrityCheck = this.GetAttributeValue( "RunIntegrityCheck" ).AsBoolean();
            bool runIndexRebuild = this.GetAttributeValue( "RunIndexRebuild" ).AsBoolean();
            bool runStatisticsUpdate = this.GetAttributeValue( "RunStatisticsUpdate" ).AsBoolean();

            int commandTimeout = GetAttributeValue( "CommandTimeout" ).AsInteger();
            bool integrityCheckPassed = false;
            bool integrityCheckIgnored = false;

            /*
             * DJL 2020-04-08
             * For Microsoft Azure, disable the Integrity Check and Statistics Update tasks.
             * Refer: https://azure.microsoft.com/en-us/blog/data-integrity-in-azure-sql-database/
             */
            if ( _databaseConfiguration.Platform == DatabasePlatform.AzureSql )
            {
                runIntegrityCheck = false;

                /* 10/25/2021 - Shaun Cummings
                 * 
                 * The original change to make this job compliant with AzureSql best practices disabled both
                 * the integrity check and the update statistics tasks in this job (by setting
                 * "runStatisticsUpdate = false" in this code block).  Further review has failed to locate 
                 * the rationale for that decision and testing UPDATE STATISTICS on AzureSql databases 
                 * indicates that the command functions as expected in an AzureSql environment.
                 * 
                 * Large Rock instances were noticed to have issues with out of date indexes causing smiple
                 * queries to take an excessive amount of time to complete (in particular, updates to group
                 * location schedules could cause the UpdateGroupLocationHistorical() portion of the Process
                 * Group History job to timeout as the query processing time increased by two orders of
                 * magnitude).
                 * 
                 * Reason:  UPDATE STATISTICS should still be run on AzureSql instances to maintain proper
                 * index performance.
                 * 
                 * */
            }

            // run integrity check
            if ( runIntegrityCheck )
            {
                try
                {
                    string alertEmail = GetAttributeValue( "AlertEmail" );
                    this.UpdateLastStatusMessage( $"Integrity Check..." );
                    integrityCheckPassed = IntegrityCheck( commandTimeout, alertEmail );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex, HttpContext.Current );
                    throw;
                }
            }
            else
            {
                integrityCheckIgnored = true;
            }

            if ( integrityCheckPassed || integrityCheckIgnored )
            {
                // rebuild fragmented indexes
                if ( runIndexRebuild )
                {
                    try
                    {
                        this.UpdateLastStatusMessage( $"Rebuild Indexes..." );
                        RebuildFragmentedIndexes( commandTimeout );
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( ex, HttpContext.Current );
                        throw;
                    }
                }

                // update statistics
                if ( runStatisticsUpdate )
                {
                    try
                    {
                        this.UpdateLastStatusMessage( $"Update Statistics..." );
                        UpdateStatistics( commandTimeout );
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( ex, HttpContext.Current );
                        throw;
                    }
                }
            }

            StringBuilder jobSummaryBuilder = new StringBuilder();
            jobSummaryBuilder.AppendLine( "Summary:" );
            jobSummaryBuilder.AppendLine( string.Empty );
            foreach ( var databaseMaintenanceTaskResult in _databaseMaintenanceTaskResults )
            {
                jobSummaryBuilder.AppendLine( GetFormattedResult( databaseMaintenanceTaskResult ) );
            }

            if ( _databaseMaintenanceTaskResults.Any( a => a.HasException ) )
            {
                jobSummaryBuilder.AppendLine( "\n<i class='fa fa-circle text-warning'></i> Some jobs have errors. See exception log for details." );
            }

            this.Result = jobSummaryBuilder.ToString();

            var databaseMaintenanceExceptions = _databaseMaintenanceTaskResults.Where( a => a.HasException ).Select( a => a.Exception ).ToList();

            if ( databaseMaintenanceExceptions.Any() )
            {
                var exceptionList = new AggregateException( "One or more exceptions occurred in Database Maintenance.", databaseMaintenanceExceptions );
                throw new RockJobWarningException( "Database Maintenance completed with warnings", exceptionList );
            }
        }

        /// <summary>
        /// Get a job tasl result as a formatted string
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private string GetFormattedResult( DatabaseMaintenanceTaskResult result )
        {
            if ( result.HasException )
            {
                return $"<i class='fa fa-circle text-danger'></i> { result.Title}";
            }
            else
            {
                var icon = "<i class='fa fa-circle text-success'></i>";
                return $"{icon} {result.Title} ({result.Elapsed.TotalMilliseconds:N0}ms)";
            }
        }

        /// <summary>
        /// Checks the integrity of the database
        /// </summary>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <param name="alertEmail">The alert email.</param>
        /// <returns></returns>
        private bool IntegrityCheck( int commandTimeout, string alertEmail)
        {
            string databaseName = new RockContext().Database.Connection.Database;
            string integrityQuery = $"DBCC CHECKDB('{ databaseName }',NOINDEX) WITH PHYSICAL_ONLY, NO_INFOMSGS";
            bool checkPassed = true;

            Stopwatch stopwatch = Stopwatch.StartNew();

            // DBCC CHECKDB will return a count of how many issues there were
            int integrityErrorCount = DbService.ExecuteCommand( integrityQuery, System.Data.CommandType.Text, null, commandTimeout );
            stopwatch.Stop();

            var databaseMaintenanceTaskResult = new DatabaseMaintenanceTaskResult
            {
                Title = "Integrity Check",
                Elapsed = stopwatch.Elapsed
            };

            _databaseMaintenanceTaskResults.Add( databaseMaintenanceTaskResult );

            if ( integrityErrorCount > 0 )
            {
                // oh no...
                checkPassed = false;
                string errorMessage = $"Some errors were reported when running a database integrity check on your Rock database. We'd recommend running the command below under 'Admin Tools > Power Tools > SQL Command' to get further details. <p>DBCC CHECKDB ('{ databaseName }') WITH NO_INFOMSGS, ALL_ERRORMSGS</p>";

                var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, null );
                mergeFields.Add( "ErrorMessage", errorMessage );
                mergeFields.Add( "Errors", integrityErrorCount );

                databaseMaintenanceTaskResult.Exception = new Exception( errorMessage );

                if ( alertEmail.IsNotNullOrWhiteSpace() )
                {
                    var globalAttributes = GlobalAttributesCache.Get();
                    string emailHeader = globalAttributes.GetValue( "EmailHeader" );
                    string emailFooter = globalAttributes.GetValue( "EmailFooter" );
                    string messageBody = $"{emailHeader} {errorMessage} <p><small>This message was generated from the Rock Database Maintenance Job</small></p>{emailFooter}";

                    var emailMessage = new RockEmailMessage();
                    var alertEmailList = alertEmail.Split( ',' ).ToList();
                    var recipients = alertEmailList.Select( a => RockEmailMessageRecipient.CreateAnonymous( a, mergeFields ) ).ToList();
                    emailMessage.Subject = "Rock: Database Integrity Check Error";
                    emailMessage.Message = messageBody;
                    emailMessage.Send();
                }
            }

            return checkPassed;
        }

        /// <summary>
        /// Rebuilds the fragmented indexes.
        /// </summary>
        /// <param name="commandTimeoutSeconds">The command timeout seconds.</param>
        private void RebuildFragmentedIndexes( int commandTimeoutSeconds )
        {
            int minimumIndexPageCount = GetAttributeValue( "MinimumIndexPageCount" ).AsInteger();
            int minimumFragmentationPercentage = GetAttributeValue( "MinimumFragmentationPercentage" ).AsInteger();
            int rebuildThresholdPercentage = GetAttributeValue( "RebuildThresholdPercentage" ).AsInteger();
            bool useONLINEIndexRebuild = GetAttributeValue( "UseONLINEIndexRebuild" ).AsBoolean();

            if ( useONLINEIndexRebuild
                 && !( _databaseConfiguration.Platform == DatabasePlatform.AzureSql
                       || _databaseConfiguration.Edition.Contains( "Enterprise" ) ) )
            {
                // Online index rebuild is only available for Azure SQL or SQL Enterprise.
                Log( RockLogLevel.Info, "Online Index Rebuild option is selected but not available for the current database platform." );

                useONLINEIndexRebuild = false;
            }

            Dictionary<string, object> parms = new Dictionary<string, object>();
            parms.Add( "@PageCountLimit", minimumIndexPageCount );
            parms.Add( "@MinFragmentation", minimumFragmentationPercentage );
            parms.Add( "@MinFragmentationRebuild", rebuildThresholdPercentage );
            parms.Add( "@UseONLINEIndexRebuild", useONLINEIndexRebuild );

            var qry = @"
SELECT 
				dbschemas.[name] as [Schema], 
				dbtables.[name] as [Table], 
				dbindexes.[name] as [Index],
				dbindexes.[type_desc] as [IndexType],
				CONVERT(INT, indexstats.avg_fragmentation_in_percent) as [FragmentationPercent] ,
				indexstats.page_count as [PageCount]
			FROM 
				sys.dm_db_index_physical_stats (DB_ID(), NULL, NULL, NULL, NULL) AS indexstats
				INNER JOIN sys.tables dbtables on dbtables.[object_id] = indexstats.[object_id]
				INNER JOIN sys.schemas dbschemas on dbtables.[schema_id] = dbschemas.[schema_id]
				INNER JOIN sys.indexes AS dbindexes ON dbindexes.[object_id] = indexstats.[object_id]
				AND indexstats.index_id = dbindexes.index_id
			WHERE 
				indexstats.database_id = DB_ID()
                AND dbindexes.[name] IS NOT NULL
				AND indexstats.page_count > @PageCountLimit
				AND indexstats.avg_fragmentation_in_percent > @MinFragmentation
";

            var dataTable = DbService.GetDataTable( qry, System.Data.CommandType.Text, parms, commandTimeoutSeconds );

            var rebuildFillFactorOption = "FILLFACTOR = 80";

            var indexInfoList = dataTable.Rows.OfType<DataRow>().Select( row => new
            {
                SchemaName = row["Schema"].ToString(),
                TableName = row["Table"].ToString(),
                IndexName = row["Index"].ToString(),
                IndexType = row["IndexType"].ToString(),
                FragmentationPercent = row["FragmentationPercent"].ToString().AsIntegerOrNull()
            } );

            // let C# do the sorting.
            var sortedIndexInfoList = indexInfoList.OrderBy( a => a.TableName ).ThenBy( a => a.IndexName );

            foreach ( var indexInfo in sortedIndexInfoList )
            {
                this.UpdateLastStatusMessage( $"Rebuilding Index [{indexInfo.TableName}].[{indexInfo.IndexName}]" );
                Stopwatch stopwatch = Stopwatch.StartNew();

                DatabaseMaintenanceTaskResult databaseMaintenanceTaskResult = new DatabaseMaintenanceTaskResult
                {
                    Title = $"Rebuild [{indexInfo.TableName}].[{indexInfo.IndexName}]"
                };

                _databaseMaintenanceTaskResults.Add( databaseMaintenanceTaskResult );

                var rebuildSQL = $"ALTER INDEX [{indexInfo.IndexName}] ON [{indexInfo.SchemaName}].[{indexInfo.TableName}]";
                if ( indexInfo.FragmentationPercent > minimumFragmentationPercentage )
                {
                    var commandOption = rebuildFillFactorOption;
                    if ( useONLINEIndexRebuild && ( indexInfo.IndexType != "SPATIAL" && indexInfo.IndexType != "XML" ) )
                    {
                        commandOption = commandOption + $", ONLINE = ON";
                    }

                    rebuildSQL += $" REBUILD WITH ({commandOption})";
                }
                else
                {
                    rebuildSQL += $" REORGANIZE";
                }

                try
                {
                    DbService.ExecuteCommand( rebuildSQL, System.Data.CommandType.Text, null, commandTimeoutSeconds );
                }
                catch ( Exception ex )
                {
                    stopwatch.Stop();

                    ExceptionLogService.LogException( new Exception( $"Error rebuilding index [{indexInfo.TableName}].[{indexInfo.IndexName}] with Command: {rebuildSQL}. Elapsed time: {stopwatch.Elapsed}", ex ) );
                    databaseMaintenanceTaskResult.Exception = ex;
                }
                finally
                {
                    stopwatch.Stop();
                    databaseMaintenanceTaskResult.Elapsed = stopwatch.Elapsed;
                }
            }
        }

        /// <summary>
        /// Updates the statistics.
        /// </summary>
        /// <param name="commandTimeout">The command timeout.</param>
        private void UpdateStatistics( int commandTimeout )
        {
            // derived from http://www.sqlservercentral.com/scripts/Indexing/31823/
            // NOTE: Can't use sp_MSForEachtable because it isn't supported on AZURE (and it is undocumented)
            // NOTE: Can't use sp_updatestats because it requires membership in the sysadmin fixed server role, or ownership of the database (dbo)
            string statisticsQuery = @"
                DECLARE updatestats CURSOR
                FOR
                SELECT TABLE_NAME
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_SCHEMA = 'dbo'
                ORDER BY TABLE_NAME

                OPEN updatestats

                DECLARE @tablename NVARCHAR(max)
                DECLARE @Statement NVARCHAR(max)

                FETCH NEXT
                FROM updatestats
                INTO @tablename

                WHILE (@@FETCH_STATUS = 0)
                BEGIN
	                PRINT N'UPDATING STATISTICS [' + @tablename + ']'
	                SET @Statement = 'UPDATE STATISTICS [' + @tablename + ']'

	                EXEC sp_executesql @Statement

	                FETCH NEXT
	                FROM updatestats
	                INTO @tablename
                END

                CLOSE updatestats

                DEALLOCATE updatestats";

            Stopwatch stopwatch = Stopwatch.StartNew();
            DbService.ExecuteCommand( statisticsQuery, System.Data.CommandType.Text, null, commandTimeout );
            stopwatch.Stop();

            _databaseMaintenanceTaskResults.Add( new DatabaseMaintenanceTaskResult
            {
                Title = "Statistics Update",
                Elapsed = stopwatch.Elapsed
            } );
        }

        /// <summary>
        /// The result data from a DatabaseMaintenance task
        /// </summary>
        private class DatabaseMaintenanceTaskResult
        {
            /// <summary>
            /// Gets or sets the title.
            /// </summary>
            /// <value>
            /// The title.
            /// </value>
            public string Title { get; set; }

            /// <summary>
            /// Gets or sets the amount of time taken
            /// </summary>
            /// <value>
            /// The time.
            /// </value>
            public TimeSpan Elapsed { get; set; }

            public bool HasException => Exception != null;

            public Exception Exception { get; set; }
        }
    }
}
