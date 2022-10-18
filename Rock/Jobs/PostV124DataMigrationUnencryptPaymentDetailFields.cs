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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// A run once job for V12.4
    /// </summary>
    [DisplayName( "Rock Update Helper v12.4 - Decrypt expiration month / year and name on card fields." )]
    [Description( "This job will decrypt the expiration month / year and the name on card fields." )]

    [IntegerField(
        "Command Timeout",
        AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete.",
        IsRequired = false,
        DefaultIntegerValue = 60 * 60 )]
    public class PostV124DataMigrationUnencryptPaymentDetailFields : RockJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            // Get the configured timeout, or default to 60 minutes if it is blank
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 3600;

            // Get a list of all FinancialPaymentDetails that need to have the Encrypted fields decrypted into the plain text fields
#pragma warning disable 612, 618
            var financialPaymentDetailIdsToUpdate = new FinancialPaymentDetailService( new RockContext() )
                .Queryable()
                .Where( pd =>
                    ( pd.ExpirationMonth == null && pd.ExpirationMonthEncrypted != null )
                    || ( pd.NameOnCardEncrypted != null && pd.NameOnCard == null ) )
                .OrderByDescending( a => a.Id )
                .Select( a => a.Id )
                .ToList();
#pragma warning restore 612, 618

            var runtime = System.Diagnostics.Stopwatch.StartNew();
            var lastProgressUpdate = DateTime.MinValue;
            double recordsProcessed = 0;
            var totalRecords = financialPaymentDetailIdsToUpdate.Count();

            // Load the FinancialPayemntDetail record for each of the financialPaymentDetailIdsToUpdate
            // and convert the encrypted fields to plain text field.
            foreach ( var financialPaymentDetailId in financialPaymentDetailIdsToUpdate )
            {
                using ( var rockContext = new RockContext() )
                {
                    var financialPaymentDetail = new FinancialPaymentDetailService( rockContext ).Get( financialPaymentDetailId );

                    if ( financialPaymentDetail != null )
                    {
                        // Tell EF that the whole FinancialPaymentDetail record has been modified.
                        // This will ensure that all the logic for setting the field data
                        // is processed, and that all the appropriate PostSaveChanges, etc is done.
                        rockContext.Entry( financialPaymentDetail ).State = EntityState.Modified;
                        rockContext.SaveChanges();
                    }

                    var processTime = runtime.ElapsedMilliseconds;
                    recordsProcessed++;
                    var recordsPerMillisecond = recordsProcessed / processTime;
                    var recordsRemaining = totalRecords - recordsProcessed;
                    var minutesRemaining = recordsRemaining / recordsPerMillisecond / 1000 / 60;

                    if ( RockDateTime.Now - lastProgressUpdate > TimeSpan.FromSeconds( 10 ) )
                    {
                        // Update the status every 10 seconds so that the progress can be shown.
                        this.UpdateLastStatusMessage( $"Processing {recordsProcessed} of {totalRecords} records. Approximately {minutesRemaining:N0} minutes remaining." );
                        lastProgressUpdate = RockDateTime.Now;
                    }
                }
            }

            this.UpdateLastStatusMessage( $"Processed {recordsProcessed} of {totalRecords} records. " );

            // Now that all the rows that need to have been decrypted have been processed, the job can be deleted.
            ServiceJobService.DeleteJob( this.ServiceJobId );
        }
    }
}
