﻿// <copyright>
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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// This job will update the AnalyticsSourcePostalCode table with geographical and census data.
    /// </summary>
    [DisplayName( "Update Analytics Source PostalCode" )]
    [Description( "Job to update the AnalyticsSourcePostalCode table with geographical and census data." )]

    #region Job Attributes

    [IntegerField(
        "SQL Command Timeout",
        Key = AttributeKey.CommandTimeoutSeconds,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. Leave blank to use the default for this job (300 seconds). ",
        IsRequired = false,
        DefaultIntegerValue = 300,
        Category = "General",
        Order = 1 )]

    [IntegerField(
        "Number of Attempts",
        Key = AttributeKey.FailedAttempts,
        Description = "The current number of attempts taken to download the ZipCode data.",
        IsRequired = false,
        DefaultIntegerValue = 0,
        Category = "General",
        Order = 2 )]

    [IntegerField(
        "Maximum Number Of Attempts",
        Key = AttributeKey.MaximumNumberOfAttempts,
        Description = "The maximum number of attempts to take in downloading the ZipCode data.",
        IsRequired = false,
        DefaultIntegerValue = 5,
        Category = "General",
        Order = 3 )]

    #endregion

    public class UpdateAnalyticsSourcePostalCode : RockJob
    {
        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string CommandTimeoutSeconds = "CommandTimeoutSeconds";
            public const string FailedAttempts = "FailedAttempts";
            public const string MaximumNumberOfAttempts = "MaximumNumberOfAttempts";
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            var failedAttempts = GetAttributeValue( AttributeKey.FailedAttempts ).AsInteger();
            var maximumAttempts = GetAttributeValue( AttributeKey.MaximumNumberOfAttempts ).AsInteger();

            if ( failedAttempts >= maximumAttempts )
            {
                this.Result = $"Aborted job after several ({failedAttempts}) failed attempts";
                throw new RockJobWarningException( this.Result );
            }

            // If no attempts have been made try downloading and saving the data.
            if ( failedAttempts == 0 )
            {
                GenerateData( failedAttempts );
            }
            else
            {
                UpdateLastStatusMessage( "Reading Census data." );
                var rockContext = new Rock.Data.RockContext();
                var censusData = AnalyticsSourcePostalCode.GetZipCodeCensusData();
                var query = rockContext.Set<AnalyticsSourcePostalCode>().AsQueryable();
                List<AnalyticsSourcePostalCode.ZipCodeBoundary> boundaryData = null;
                int batchSize = 100;

                // If an attempt has been made try and figure where we left off previously by comparing the last database entry PostalCode with the last Census data PostalCode
                var lastDatabaseEntryPostalCode = query.OrderByDescending( az => az.PostalCode ).Select( az => az.PostalCode ).FirstOrDefault();
                var lastEntry = censusData.OrderByDescending( az => az.PostalCode ).FirstOrDefault();

                // If we have no data try downloading and saving the data from scratch.
                if ( lastEntry == null )
                {
                    GenerateData( failedAttempts );
                }
                else
                {
                    // Else try picking up from where we last left off.
                    if ( lastEntry.PostalCode.AsInteger() > lastDatabaseEntryPostalCode.AsInteger() )
                    {
                        // Get the index of the last record saved to the database.
                        var lastDatabaseEntry = censusData.Find( az => az.PostalCode == lastDatabaseEntryPostalCode );
                        var skipCount = censusData.IndexOf( lastDatabaseEntry ) + 1;

                        // Get the records left after the last record was saved successfully to the database.                     
                        var remainingCount = censusData.Count - skipCount;

                        // Calculate the number of batches needed to save the remaining records.
                        var batches = ( int ) Math.Ceiling( ( double ) remainingCount / batchSize );
                        for ( int i = 0; i < batches; i++ )
                        {
                            // Renew context to clear saved entries from memory, this is done to speed up the process since we are 
                            // dealing with a large dataset and the context noticeably slows down when tracking several records.
                            rockContext = new RockContext();
                            rockContext.Configuration.AutoDetectChangesEnabled = false;
                            rockContext.Configuration.ValidateOnSaveEnabled = false;

                            // Save the current batch.
                            var currentBatch = censusData.Skip( skipCount ).Take( batchSize ).ToList();

                            rockContext.Set<AnalyticsSourcePostalCode>().AddRange( currentBatch );
                            rockContext.SaveChanges();

                            skipCount += batchSize;
                        }
                    }

                    // If we successfully downloaded the boundary data.
                    if ( boundaryData?.Any() == true )
                    {
                        // Calculate the number of batches needed to save the updated records.
                        var updateBatches = ( int ) Math.Ceiling( ( double ) censusData.Count / batchSize );
                        for ( int i = 0; i < updateBatches; i++ )
                        {
                            rockContext = new RockContext();
                            rockContext.Configuration.ValidateOnSaveEnabled = false;

                            // Get and update the current batch
                            var currentBatch = rockContext.Set<AnalyticsSourcePostalCode>().OrderBy( az => az.PostalCode ).Skip( i * batchSize ).Take( batchSize ).ToList();

                            foreach ( var analyticsPostalCode in currentBatch )
                            {
                                var zipCodeBoundary = boundaryData.Find( z => z.ZipCode == analyticsPostalCode.PostalCode );

                                if ( zipCodeBoundary != null )
                                {
                                    analyticsPostalCode.State = zipCodeBoundary.State ?? string.Empty;
                                    analyticsPostalCode.SquareMiles = zipCodeBoundary.SquareMiles;
                                    analyticsPostalCode.City = zipCodeBoundary.City ?? string.Empty;
                                    analyticsPostalCode.GeoFence = zipCodeBoundary.GeoFence;
                                }
                            }

                            rockContext.SaveChanges();

                            UpdateLastStatusMessage( $"Saving PostalCode Data ({i * batchSize}/{censusData.Count})." );
                        }
                    }

                    failedAttempts = 0;
                    UpdateLastStatusMessage( $"Saved PostalCode Data ({rockContext.Set<AnalyticsSourcePostalCode>().Count()}/{censusData.Count})." );
                    SetAttributeValue( AttributeKey.FailedAttempts, failedAttempts.ToString() );
                    rockContext.Dispose();
                }
            }

            if ( failedAttempts == 0 )
            {
                AnalyticsSourcePostalCode.DeleteCensusDataFile();
                DeleteJob();
            }
        }

        /// <summary>
        /// Generates and saves the AnalyticsSourcePostalCode data.
        /// </summary>
        /// <param name="failedAttempts">The number failed attempts.</param>
        private void GenerateData( int failedAttempts )
        {
            try
            {
                // remove all the rows in preparation for rebuild
                AnalyticsSourcePostalCode.ClearTable();

                UpdateLastStatusMessage( "Reading PostalCode census data." );
                var zipCodeCensusData = AnalyticsSourcePostalCode.GetZipCodeCensusData();

                UpdateLastStatusMessage( "Saving AnalyticsSourcePostalCode." );
                AnalyticsSourcePostalCode.SaveBoundaryAndCensusData( zipCodeCensusData );

                UpdateLastStatusMessage( $"Updated {zipCodeCensusData.Count:N0} PostalCode values." );

                failedAttempts = 0;
                SetAttributeValue( AttributeKey.FailedAttempts, failedAttempts.ToString() );
            }
            catch ( Exception )
            {
                failedAttempts++;
                SetAttributeValue( AttributeKey.FailedAttempts, failedAttempts.ToString() );
                throw;
            }
        }

        /// <summary>
        /// Saves the attribute the post update job along with the value to the database so that it is persisted across restarts.
        /// </summary>
        /// <param name="key">The key of the attribute to be saved</param>
        /// <param name="value">The value of the attribute to be saved</param>
        internal void SetAttributeValue( string key, string value )
        {
            ServiceJob.SetAttributeValue( key, value );
            ServiceJob.SaveAttributeValues();
        }

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
