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
using System.Linq;

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// A run once job for V10.3
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [DisplayName( "Rock Update Helper v10.3 - Spiritual Gifts Update" )]
    [Description( "This job will take care of any data migrations to Spiritual Gifts Assessment results that need to occur after updating to v10.3. After all the operations are done, this job will delete itself." )]

    [IntegerField(
        "Command Timeout",
        AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 60 * 60 )]
    public class PostV103DataMigrationsSpiritualGifts : IJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        private static class SystemSettingKey
        {
            public const string ConversionCompleted = "core_SpiritualGiftsConversionCompleted";
        }

        private int _commandTimeout;

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // Get the configured timeout, or default to 60 minutes if it is blank
            _commandTimeout = dataMap.GetString( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 3600;

            if ( !IsAlreadyConverted() )
            {
                ConvertSpiritualGiftsAssessmentResults();

                Web.SystemSettings.SetValue( SystemSettingKey.ConversionCompleted, RockDateTime.Now.ToString( "o" ) );
            }

            DeleteJob( context.GetJobId() );
        }

        /// <summary>
        /// Determines whether Spiritual Gifts Assessment results have already been converted.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if already converted; otherwise, <c>false</c>.
        /// </returns>
        private bool IsAlreadyConverted()
        {
            var conversionCompleted = Web.SystemSettings.GetValue( SystemSettingKey.ConversionCompleted ).AsDateTime() ?? DateTime.MinValue;

            return conversionCompleted != DateTime.MinValue;
        }

        /// <summary>
        /// Converts the assessment results.
        /// </summary>
        private void ConvertSpiritualGiftsAssessmentResults()
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = _commandTimeout;

                var spiritualGiftsGuid = SystemGuid.AssessmentType.GIFTS.AsGuidOrNull();
                if ( !spiritualGiftsGuid.HasValue )
                {
                    return;
                }

                int? assessmentTypeId = AssessmentTypeCache.GetId( spiritualGiftsGuid.Value );
                if ( !assessmentTypeId.HasValue )
                {
                    return;
                }

                // Load all completed Spiritual Gifts Assessments that have AssessmentResultData
                var assessmentService = new AssessmentService( rockContext );
                var assessments = assessmentService.Queryable( "PersonAlias.Person" )
                    .Where( a => a.AssessmentTypeId == assessmentTypeId.Value )
                    .Where( a => a.Status == AssessmentRequestStatus.Complete )
                    .Where( a => !string.IsNullOrEmpty( a.AssessmentResultData ) )
                    .ToList();

                foreach ( Assessment assessment in assessments )
                {
                    if ( assessment.PersonAlias?.Person == null )
                    {
                        continue;
                    }

                    // Deserialize the stored JSON
                    var resultData = assessment.AssessmentResultData.FromJsonOrNull<SpiritualGiftsService.AssessmentResultData>();
                    if ( resultData == null || resultData.Result == null )
                    {
                        continue;
                    }

                    // Re-score the origninal Assessment responses
                    SpiritualGiftsService.AssessmentResults results = SpiritualGiftsService.GetResult( resultData.Result );
                    if ( results == null )
                    {
                        continue;
                    }

                    // Re-save the Assessment result AttributeValues
                    SpiritualGiftsService.SaveAssessmentResults( assessment.PersonAlias.Person, results );

                    // Add the scores to the Assessment's data object
                    resultData.ResultScores = results.SpiritualGiftScores;
                    assessment.AssessmentResultData = resultData.ToJson();
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Deletes the job.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        public static void DeleteJob( int jobId )
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
    }
}
