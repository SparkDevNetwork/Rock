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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.Jobs
{
    /// <summary>
    /// A run once job after a new installation. The purpose of this job is to populate generated datasets after an initial installation using RockInstaller that are too large to include in the installer.
    /// </summary>
    [DisplayName( "Runs data updates that need to occur after a new installation." )]
    [Description( "This job will take care of any data migrations that need to occur after a new installation. After all the operations are done, this job will delete itself." )]
    [IntegerField( "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of Attribute Values, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 3600 )]
    public class PostInstallDataMigrations : RockJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }
        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            // get the configured timeout, or default to 60 minutes if it is blank
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 3600;

            InsertAnalyitcsSourceDateData( commandTimeout );
            InsertIdentityVerificationCodeData( commandTimeout );
            InsertAnalyticsSourceZipCodeData( commandTimeout );

            DeleteJob( this.ServiceJobId );
        }

        private static void DeleteJob( int jobId )
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( jobId );

                if ( job != null )
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges();
                    return;
                }
            }
        }

        private void InsertAnalyitcsSourceDateData( int commandTimeout )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;
                if ( !rockContext.AnalyticsSourceDates.AsQueryable().Any() )
                {
                    var analyticsStartDate = new DateTime( RockDateTime.Today.AddYears( -150 ).Year, 1, 1 );
                    var analyticsEndDate = new DateTime( RockDateTime.Today.AddYears( 101 ).Year, 1, 1 ).AddDays( -1 );
                    Rock.Model.AnalyticsSourceDate.GenerateAnalyticsSourceDateData( 1, false, analyticsStartDate, analyticsEndDate );
                }
            }
        }

        private void InsertAnalyticsSourceZipCodeData( int commandTimeout )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;
                if ( !rockContext.AnalyticsSourceZipCodes.AsQueryable().Any() )
                {
                    Rock.Model.AnalyticsSourceZipCode.GenerateAnalyticsSourceZipCodeData();
                }
            }
        }

        private void InsertIdentityVerificationCodeData( int commandTimeout )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;
                rockContext.Database.ExecuteSqlCommand( "DELETE FROM IdentityVerificationCode" );

                InsertIdentityVerificationCodeDataChunk( rockContext, 1001, 250000 );
                InsertIdentityVerificationCodeDataChunk( rockContext, 250000, 500000 );
                InsertIdentityVerificationCodeDataChunk( rockContext, 500000, 750000 );
                InsertIdentityVerificationCodeDataChunk( rockContext, 750000, 998998 );
            }
        }

        /// <summary>
        /// Inserts the identity verification code data chunk. This is to avoid filling the transaction log on servers with limited resources (e.g. winhost)
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="start">The start.</param>
        /// <param name="finish">The finish.</param>
        private void InsertIdentityVerificationCodeDataChunk( RockContext rockContext, int start, int finish )
        {
            try
            {
                rockContext.Database.ExecuteSqlCommand( $@"
                    WITH x AS (SELECT n FROM (VALUES (0),(1),(2),(3),(4),(5),(6),(7),(8),(9)) v(n))
                    SELECT RIGHT('000000' + CAST([ones].n + 10*[tens].n + 100*[hundreds].n + 1000*[thousands].n + 10000*[tenthousand].n  + 100000*[hundredthousand].n AS NVARCHAR(10)), 6) AS [Code]
                    INTO #Codes
                    FROM x [ones]
                        , x [tens]
                        , x [hundreds]
                        , x [thousands]
                        , x [tenthousand]
                        , x [hundredthousand]
                    ORDER BY 1
                    
                    INSERT INTO [IdentityVerificationCode] ([Code], [GUID], [CreatedDateTime], [ModifiedDateTime])
                    SELECT DISTINCT [Code], NEWID(), GETDATE(), GETDATE()
                    FROM #Codes
                    WHERE [Code] NOT LIKE '%000%'
		                AND [Code] NOT LIKE '%111%'
		                AND [Code] NOT LIKE '%222%'
		                AND [Code] NOT LIKE '%333%'
		                AND [Code] NOT LIKE '%444%'
		                AND [Code] NOT LIKE '%555%'
		                AND [Code] NOT LIKE '%666%'
		                AND [Code] NOT LIKE '%777%'
		                AND [Code] NOT LIKE '%888%'
		                AND [Code] NOT LIKE '%999%'
                        AND CAST([Code] AS INT) BETWEEN {start} AND {finish}

                    DROP TABLE #Codes" );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new Exception( $"Error inserting IdentityVerificationCodes {start} to {finish}.", ex )  );
                throw;
            }
        }
    }
}
