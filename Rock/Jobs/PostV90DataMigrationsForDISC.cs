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

namespace Rock.Jobs
{
    /// <summary>
    /// A run once job for Disc in V9.0
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [DisplayName( "Rock Update Helper v9.0 - DISC" )]
    [Description( "This job will take care of any data migrations that need to occur after updating to v. After all the operations are done, this job will delete itself." )]
    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for each SQL command to complete. Leave blank to use the default for this job (3600 seconds). Note that some of the tasks might take a while on larger databases, so you might need to set it higher.", false, 60 * 60, "General", 7, "CommandTimeout" )]
    public class PostV90DataMigrationsForDISC : IJob
    {
        private int? _commandTimeout = null;

        /// <summary>
        /// Executes the specified context. When updating large data sets SQL will burn a lot of time updating the indexes. If performing multiple inserts/updates
        /// consider dropping the related indexes first and re-creating them once the operation is complete.
        /// Put all index creation method calls at the end of this method.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // get the configured timeout, or default to 60 minutes if it is blank
            _commandTimeout = dataMap.GetString( "CommandTimeout" ).AsIntegerOrNull() ?? 3600;
            UpdateDiscAdaptiveScore();
            UpdateDiscNaturalScore();
            DeleteJob( context.GetJobId() );
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

        /// <summary>
        /// Update the Disc Adaptive Score
        /// </summary>
        public void UpdateDiscAdaptiveScore()
        {
            //Adaptive D
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                var attribute = new AttributeService( rockContext ).Get( new Guid( "EDE5E199-37BE-424F-A788-5CDCC064157C" ) );
                var attributeValues = new AttributeValueService( rockContext ).Queryable().Where( b => b.AttributeId == attribute.Id && b.Value != string.Empty );
                attributeValues.ToList().ForEach( a => {
                    a.Value = DiscService.GetAdaptiveScoreValue( DiscService.AttributeKeys.AdaptiveD, Convert.ToInt32( Math.Round( a.Value.AsDecimal() * 0.28m, 0 ) ) ).ToString();

                } );
                rockContext.SaveChanges();
            }

            //Adaptive S
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                var attribute = new AttributeService( rockContext ).Get( new Guid( "2512DAC6-BBC4-4D0E-A01D-E92F94C534BD" ) );
                var attributeValues = new AttributeValueService( rockContext ).Queryable().Where( b => b.AttributeId == attribute.Id && b.Value != string.Empty );
                attributeValues.ToList().ForEach( a => {
                    a.Value = DiscService.GetAdaptiveScoreValue( DiscService.AttributeKeys.AdaptiveS, Convert.ToInt32( Math.Round(  a.Value.AsDecimal() * 0.28m, 0 ) ) ).ToString();

                } );
                rockContext.SaveChanges();
            }

            //Adaptive I
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                var attribute = new AttributeService( rockContext ).Get( new Guid( "7F0A1794-0150-413B-9AE1-A6B0D6373DA6" ) );
                var attributeValues = new AttributeValueService( rockContext ).Queryable().Where( b => b.AttributeId == attribute.Id && b.Value != string.Empty );
                attributeValues.ToList().ForEach( a => {
                    a.Value = DiscService.GetAdaptiveScoreValue( DiscService.AttributeKeys.AdaptiveI, Convert.ToInt32( Math.Round(  a.Value.AsDecimal() * 0.28m, 0 ) ) ).ToString();

                } );
                rockContext.SaveChanges();
            }

            //Adaptive C
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                var attribute = new AttributeService( rockContext ).Get( new Guid( "4A2E1539-4ECC-40B9-9EBD-C0C84EC8DA36" ) );
                var attributeValues = new AttributeValueService( rockContext ).Queryable().Where( b => b.AttributeId == attribute.Id && b.Value != string.Empty );
                attributeValues.ToList().ForEach( a => {
                    a.Value = DiscService.GetAdaptiveScoreValue( DiscService.AttributeKeys.AdaptiveC, Convert.ToInt32( Math.Round( a.Value.AsDecimal() * 0.28m , 0 ) ) ).ToString();

                } );
                rockContext.SaveChanges();
            }

        }

        /// <summary>
        /// Update the Disc Natural Score
        /// </summary>
        public void UpdateDiscNaturalScore()
        {
            //Natural D
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                var attribute = new AttributeService( rockContext ).Get( new Guid( "86670F7D-07BA-4ECE-9BB9-9D94B5FB5F26" ) );
                var attributeValues = new AttributeValueService( rockContext ).Queryable().Where( b => b.AttributeId == attribute.Id && b.Value != string.Empty );
                attributeValues.ToList().ForEach( a => {
                    a.Value = DiscService.GetNaturalScoreValue( DiscService.AttributeKeys.NaturalD, Convert.ToInt32( Math.Round( ( 27 - a.Value.AsDecimal() * 0.78m ), 0 ) ) ).ToString();

                } );
                rockContext.SaveChanges();
            }

            //Natural S
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                var attribute = new AttributeService( rockContext ).Get( new Guid( "FA4341B4-28C7-409E-A101-548BB5759BE6" ) );
                var attributeValues = new AttributeValueService( rockContext ).Queryable().Where( b => b.AttributeId == attribute.Id && b.Value != string.Empty );
                attributeValues.ToList().ForEach( a => {
                    a.Value = DiscService.GetNaturalScoreValue( DiscService.AttributeKeys.NaturalS, Convert.ToInt32( Math.Round( ( 27 - a.Value.AsDecimal() * 0.78m ), 0 ) ) ).ToString();

                } );
                rockContext.SaveChanges();
            }

            //Natural I
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                var attribute = new AttributeService( rockContext ).Get( new Guid( "3EFF4FEF-EE4C-40E2-8DBD-80F3276852DA" ) );
                var attributeValues = new AttributeValueService( rockContext ).Queryable().Where( b => b.AttributeId == attribute.Id && b.Value != string.Empty );
                attributeValues.ToList().ForEach( a => {
                    a.Value = DiscService.GetNaturalScoreValue( DiscService.AttributeKeys.NaturalI, Convert.ToInt32( Math.Round( ( 26 - a.Value.AsDecimal() * 0.78m ), 0 ) ) ).ToString();

                } );
                rockContext.SaveChanges();
            }

            //Natural C
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                var attribute = new AttributeService( rockContext ).Get( new Guid( "3A10ECFB-8CAB-4CCA-8B29-298756CD3251" ) );
                var attributeValues = new AttributeValueService( rockContext ).Queryable().Where( b => b.AttributeId == attribute.Id && b.Value != string.Empty );
                attributeValues.ToList().ForEach( a => {
                    a.Value = DiscService.GetNaturalScoreValue( DiscService.AttributeKeys.NaturalC, Convert.ToInt32( Math.Round( ( 26 - a.Value.AsDecimal() * 0.78m ), 0 ) ) ).ToString();

                } );
                rockContext.SaveChanges();
            }

        }
    }
}
