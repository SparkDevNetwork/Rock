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
using System.ComponentModel;
using Rock.Data;
using Rock.Model;
using System.Linq;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Updates attribute values of SlidingDateRangeFieldType RoundTrip format.
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisplayName( "Rock Update Helper v14.1 - Update SlidingDateRange Attribute Values." )]
    [Description( "Updates attribute values of SlidingDateRangeFieldType to RoundTrip format." )]

    [IntegerField(
        "Command Timeout",
        Description = "Maximum amount of time (in seconds) to wait for the SQL Query to complete. Leave blank to use the default for this job (3600). Note, it could take several minutes, so you might want to set it at 3600 (60 minutes) or higher",
        IsRequired = false,
        DefaultIntegerValue = AttributeDefaults.CommandTimeout,
        Category = "General",
        Order = 1,
        Key = AttributeKey.CommandTimeout )]
    public class PostV141DataMigrationsUpdateSlidingDateRangeFormat : RockJob
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
            // get the configured timeout, or default to 60 minutes if it is blank
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? AttributeDefaults.CommandTimeout;
            var isProcessingComplete = false;
            var batchSize = 1000;
            var totalBatchSize = 0;
            var currentBatch = 1;

            var fieldType = FieldTypeCache.Get( Rock.SystemGuid.FieldType.SLIDING_DATE_RANGE.AsGuid() );
            var batchSizeQueryContext = new RockContext();
            batchSizeQueryContext.Database.CommandTimeout = commandTimeout;
            totalBatchSize = new AttributeValueService( batchSizeQueryContext )
                .Queryable()
                .Where( a => a.Attribute.FieldTypeId == fieldType.Id )
                .Select( a => a.Id )
                .Count();
            var runtime = System.Diagnostics.Stopwatch.StartNew();
            var lastProcessedAttributeId = 0;

            while ( !isProcessingComplete )
            {
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.CommandTimeout = commandTimeout;
                    var attributes = new AttributeValueService( rockContext )
                        .Queryable()
                        .Where( a => a.Attribute.FieldTypeId == fieldType.Id && a.Id > lastProcessedAttributeId )
                        .OrderBy( a => a.Id )
                        .Take( batchSize )
                        .ToList();

                    foreach ( var attribute in attributes )
                    {
                        attribute.Value = RockDateTimeHelper.ConvertDateRangeDelimitedValueToRoundTripFormat( attribute.Value );
                        lastProcessedAttributeId = attribute.Id;
                    }

                    rockContext.SaveChanges();

                    var processTime = runtime.ElapsedMilliseconds;
                    var recordsProcessed = (double) ( batchSize * currentBatch ) + attributes.Count;
                    var recordsPerMillisecond = recordsProcessed / processTime;
                    var recordsRemaining = totalBatchSize - recordsProcessed;
                    var minutesRemaining = recordsRemaining / recordsPerMillisecond / 1000 / 60;
                    UpdateLastStatusMessage( $"Processing {recordsProcessed} of {totalBatchSize} records. Approximately {minutesRemaining:N0} minutes remaining." );
                    currentBatch++;
                    isProcessingComplete = attributes.Count < batchSize;
                }
            }

            ServiceJobService.DeleteJob( GetJobId() );
        }
    }
}
