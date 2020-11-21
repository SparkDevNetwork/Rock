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
using Quartz;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.Utility.Settings;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to run quick SQL queries on a schedule
    /// </summary>
    [DisplayName( "Database Maintenance" )]
    [Description( "Performs routine SQL Server database maintenance." )]

    [BooleanField( "Run Integrity Check", "Determines if an integrity check should be performed.", true, order: 0 )]
    [BooleanField( "Run Index Rebuild", "Determines if indexes should be rebuilt.", true, order: 1 )]
    [BooleanField( "Run Statistics Update", "Determines if the statistics should be updated.", true, order: 2 )]
    [TextField( "Alert Email", "Email address to send alerts to errors occur (multiple address delimited with comma).", true, order: 3 )]

    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for each step to complete.", false, 900, "Advanced", 4, "CommandTimeout" )]
    [IntegerField( "Minimum Index Page Count", "The minimum size in pages that an index must be before it's considered for being re-built. Default value is 100.", false, 100, category: "Advanced", order: 5 )]
    [IntegerField( "Minimum Fragmentation Percentage", "The minimum fragmentation percentage for an index to be considered for re-indexing. If the fragmentation is below is amount nothing will be done. Default value is 10%.", false, 10, category: "Advanced", order: 6 )]
    [IntegerField( "Rebuild Threshold Percentage", "The threshold percentage where a REBUILD will be completed instead of a REORGANIZE. Default value is 30%.", false, 30, category: "Advanced", order: 7 )]
    [BooleanField( "Use ONLINE Index Rebuild", "Use the ONLINE option when rebuilding indexes. NOTE: This is only supported on SQL Enterprise and Azure SQL Database.", false, category: "Advanced", order: 8 )]
    [DisallowConcurrentExecution]
    public class DatabaseMaintenance : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public DatabaseMaintenance()
        {
        }

        private List<DatabaseMaintenanceTaskResult> _databaseMaintenanceTaskResults = new List<DatabaseMaintenanceTaskResult>();

        /// <summary>
        /// Job that will run quick SQL queries on a schedule.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext jobContext )
        {
            JobDataMap dataMap = jobContext.JobDetail.JobDataMap;

            // get job parms
            bool runIntegrityCheck = dataMap.GetBoolean( "RunIntegrityCheck" );
            bool runIndexRebuild = dataMap.GetBoolean( "RunIndexRebuild" );
            bool runStatisticsUpdate = dataMap.GetBoolean( "RunStatisticsUpdate" );

            int commandTimeout = dataMap.GetString( "CommandTimeout" ).AsInteger();
            bool integrityCheckPassed = false;
            bool integrityCheckIgnored = false;

            /*
             * DJL 2020-04-08
             * For Microsoft Azure, disable the Integrity Check and Statistics Update tasks.
             * Refer: https://azure.microsoft.com/en-us/blog/data-integrity-in-azure-sql-database/
             */
            if ( RockInstanceConfig.Database.Platform == RockInstanceDatabaseConfiguration.PlatformSpecifier.AzureSql )
            {
                runIntegrityCheck = false;
                runStatisticsUpdate = false;
            }

            // run integrity check
            if ( runIntegrityCheck )
            {
                try
                {
                    string alertEmail = dataMap.GetString( "AlertEmail" );
                    jobContext.UpdateLastStatusMessage( $"Integrity Check..." );
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
                        jobContext.UpdateLastStatusMessage( $"Rebuild Indexes..." );
                        RebuildFragmentedIndexes( jobContext, commandTimeout );
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
                        jobContext.UpdateLastStatusMessage( $"Update Statistics..." );
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

            jobContext.Result = jobSummaryBuilder.ToString();

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
        /// <param name="jobContext">The job context.</param>
        /// <param name="commandTimeoutSeconds">The command timeout seconds.</param>
        private void RebuildFragmentedIndexes( IJobExecutionContext jobContext, int commandTimeoutSeconds )
        {
            JobDataMap dataMap = jobContext.JobDetail.JobDataMap;
            int minimumIndexPageCount = dataMap.GetString( "MinimumIndexPageCount" ).AsInteger();
            int minimumFragmentationPercentage = dataMap.GetString( "MinimumFragmentationPercentage" ).AsInteger();
            int rebuildThresholdPercentage = dataMap.GetString( "RebuildThresholdPercentage" ).AsInteger();
            bool useONLINEIndexRebuild = dataMap.GetString( "UseONLINEIndexRebuild" ).AsBoolean();

            if ( useONLINEIndexRebuild
                 && !( RockInstanceConfig.Database.Platform == RockInstanceDatabaseConfiguration.PlatformSpecifier.AzureSql
                       || RockInstanceConfig.Database.Edition.Contains( "Enterprise" ) ) )
            {
                // Online index rebuild is only available for Azure SQL or SQL Enterprise.
                RockLogger.Log.Information( RockLogDomains.Jobs, "Database Maintenance - Online Index Rebuild option is selected but not available for the current database platform." );

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
                jobContext.UpdateLastStatusMessage( $"Rebuilding Index [{indexInfo.TableName}].[{indexInfo.IndexName}]" );
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

                    stopwatch.Stop();
                    databaseMaintenanceTaskResult.Elapsed = stopwatch.Elapsed;
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( new Exception( $"Error rebuilding index [{indexInfo.TableName}].[{indexInfo.IndexName}]", ex ) );
                    databaseMaintenanceTaskResult.Exception = ex;
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
