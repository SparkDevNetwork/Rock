// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to run quick SQL queries on a schedule
    /// </summary>
    [WorkflowTypeField( "eRA Entry Workflow", "The workflow type to launch when a family becomes an eRA.", key: "EraEntryWorkflow")]
    [WorkflowTypeField( "eRA Exit Workflow", "The workflow type to launch when a family exits from being an eRA.", key: "EraExitWorkflow" )]
    [DisallowConcurrentExecution]
    public class CalculatePersonAnalytics : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public CalculatePersonAnalytics()
        {
        }

        /// <summary>
        /// Job that will run quick SQL queries on a schedule.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            Guid? entryWorkflowType = dataMap.GetString( "EraEntryWorkflow" ).AsGuidOrNull();
            Guid? exitWorkflowType = dataMap.GetString( "EraExitWorkflow" ).AsGuidOrNull();

            // configuration
            //

            // giving
            int entryGivingCountShort = 1;
            int entryGivingCountLong = 4;
            int exitGivingCount = 1;

            // attendance
            int entryAttendanceCount = 8;
            int exitAttendanceCountShort = 1;
            int exitAttendanceCountLong = 8;

            // get era dataset from stored proc
            var rockContext = new RockContext();

            var attributeValueService = new AttributeValueService( rockContext );
            var eraAttribute = AttributeCache.Read( SystemGuid.Attribute.PERSON_ERA_CURRENTLY_AN_ERA.AsGuid() );

            var results = rockContext.Database.SqlQuery<EraResult>( "spCrm_PersonAnalyticsEraDataset" ).ToList();

            foreach (var result in results )
            {
                // if era ensure it still meets requirements
                if ( result.IsEra )
                {
                    if (result.ExitGiftCountDuration < exitGivingCount && result.ExitAttendanceCountDurationShort < exitAttendanceCountShort && result.ExitAttendanceCountDurationLong < exitAttendanceCountLong )
                    {
                        // exit era (delete attribute value from each person in family)
                        var family = new GroupService( rockContext ).Queryable( "Members" ).AsNoTracking().Where( m => m.Id == result.FamilyId ).FirstOrDefault();

                        if ( family != null ) {
                            foreach ( var person in family.Members.Select( m => m.Person ) ) {
                                var eraAttributeValue = attributeValueService.Queryable().Where( v => v.AttributeId == eraAttribute.Id && v.EntityId == person.Id ).FirstOrDefault();

                                if ( eraAttributeValue != null )
                                {
                                    attributeValueService.Delete( eraAttributeValue );
                                    rockContext.SaveChanges();
                                }
                            }

                            // launch exit workflow
                            if ( exitWorkflowType.HasValue )
                            {
                                var headOfHouse = family.Members.OrderBy( m => m.GroupRole.Order ).ThenBy( m => m.Person.Gender ).FirstOrDefault().Person;
                                LaunchWorkflow( exitWorkflowType.Value, headOfHouse );
                            }
                        }
                    }
                }
                else
                {
                    // entered era
                    var family = new GroupService( rockContext ).Queryable( "Members" ).AsNoTracking().Where( m => m.Id == result.FamilyId ).FirstOrDefault();

                    if ( family != null )
                    {
                        foreach ( var person in family.Members.Select( m => m.Person ) )
                        {
                            var eraAttributeValue = attributeValueService.Queryable().Where( v => v.AttributeId == eraAttribute.Id && v.EntityId == person.Id ).FirstOrDefault();

                            if ( eraAttributeValue == null )
                            {
                                eraAttributeValue = new AttributeValue();
                                eraAttributeValue.EntityId = person.Id;
                                eraAttributeValue.AttributeId = eraAttribute.Id;
                                attributeValueService.Add( eraAttributeValue );
                            }
                            eraAttributeValue.Value = bool.TrueString;
                            rockContext.SaveChanges();
                        }

                        // launch entry workflow
                        if ( entryWorkflowType.HasValue )
                        {
                            var headOfHouse = family.Members.OrderBy( m => m.GroupRole.Order ).ThenBy( m => m.Person.Gender ).FirstOrDefault().Person;
                            LaunchWorkflow( entryWorkflowType.Value, headOfHouse );
                        }
                    }
                }

                // update stats
            }

        }

        private void LaunchWorkflow(Guid workflowTypeGuid, Person headOfHouse )
        {
            using ( var rockContext = new RockContext() )
            {
                var workflowTypeService = new WorkflowTypeService( rockContext );
                var workflowType = workflowTypeService.Get( workflowTypeGuid );
                if ( workflowType != null )
                {
                    var workflow = Rock.Model.Workflow.Activate( workflowType, "Workflow Name" );
                    workflow.SetAttributeValue( "FromPhone", "" );

                    //List<string> workflowErrors;
                    //var processed = new Rock.Model.WorkflowService( rockContext ).Process( workflow, out workflowErrors );
                }
            }
        }

        /// <summary>
        /// eRA Result Class
        /// </summary>
        public class EraResult
        {            
            /// <summary>
            /// Gets or sets the family identifier.
            /// </summary>
            /// <value>
            /// The family identifier.
            /// </value>
            [Key]
            public int FamilyId { get; set; }
            /// <summary>
            /// Gets or sets the entry gift duration short. Default 6 weeks.
            /// </summary>
            /// <value>
            /// The entry gift duration short.
            /// </value>
            public int EntryGiftCountDurationShort { get; set; }
            /// <summary>
            /// Gets or sets the entry gift duration long. Default 52 weeks.
            /// </summary>
            /// <value>
            /// The entry gift duration long.
            /// </value>
            public int EntryGiftCountDurationLong { get; set; }
            /// <summary>
            /// Gets or sets the duration of the exit gift. Default 8 weeks.
            /// </summary>
            /// <value>
            /// The duration of the exit gift.
            /// </value>
            public int ExitGiftCountDuration { get; set; }
            /// <summary>
            /// Gets or sets the duration of the entry attendance. Default 16 weeks.
            /// </summary>
            /// <value>
            /// The duration of the entry attendance.
            /// </value>
            public int EntryAttendanceCountDuration { get; set; }
            /// <summary>
            /// Gets or sets the exit attendance duration short. Default 4 weeks.
            /// </summary>
            /// <value>
            /// The exit attendance duration short.
            /// </value>
            public int ExitAttendanceCountDurationShort { get; set; }
            /// <summary>
            /// Gets or sets the exit attendance duration long. Default 16 weeks.
            /// </summary>
            /// <value>
            /// The exit attendance duration long.
            /// </value>
            public int ExitAttendanceCountDurationLong { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether this instance is era.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is era; otherwise, <c>false</c>.
            /// </value>
            public bool IsEra { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="EraResult"/> class.
            /// </summary>
            public EraResult() { }
            
        }
    }
}
