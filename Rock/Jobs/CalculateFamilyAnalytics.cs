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
    public class CalculateFamilyAnalytics : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public CalculateFamilyAnalytics()
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
            int exitGivingCount = 1;

            // attendance
            int exitAttendanceCountShort = 1;
            int exitAttendanceCountLong = 8;

            // get era dataset from stored proc
            var resultContext = new RockContext();

            
            var eraAttribute = AttributeCache.Read( SystemGuid.Attribute.PERSON_ERA_CURRENTLY_AN_ERA.AsGuid() );
            var eraStartAttribute = AttributeCache.Read( SystemGuid.Attribute.PERSON_ERA_START_DATE.AsGuid() );
            var eraEndAttribute = AttributeCache.Read( SystemGuid.Attribute.PERSON_ERA_END_DATE.AsGuid() );

            resultContext.Database.CommandTimeout = 1200;

            var results = resultContext.Database.SqlQuery<EraResult>( "spCrm_FamilyAnalyticsEraDataset" ).ToList();

            int personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
            int attributeEntityTypeId = EntityTypeCache.Read( "Rock.Model.Attribute" ).Id;
            int eraAttributeId = AttributeCache.Read( SystemGuid.Attribute.PERSON_ERA_CURRENTLY_AN_ERA.AsGuid() ).Id;
            int personAnalyticsCategoryId = CategoryCache.Read( SystemGuid.Category.HISTORY_PERSON_ANALYTICS.AsGuid() ).Id;

            foreach (var result in results )
            {
                // create new rock context for each family (https://weblog.west-wind.com/posts/2014/Dec/21/Gotcha-Entity-Framework-gets-slow-in-long-Iteration-Loops)
                RockContext updateContext = new RockContext();
                var attributeValueService = new AttributeValueService( updateContext );
                var historyService = new HistoryService( updateContext );

                // if era ensure it still meets requirements
                if ( result.IsEra )
                {
                    if (result.ExitGiftCountDuration < exitGivingCount && result.ExitAttendanceCountDurationShort < exitAttendanceCountShort && result.ExitAttendanceCountDurationLong < exitAttendanceCountLong )
                    {
                        // exit era (delete attribute value from each person in family)
                        var family = new GroupService( updateContext ).Queryable( "Members, Members.Person" ).AsNoTracking().Where( m => m.Id == result.FamilyId ).FirstOrDefault();

                        if ( family != null ) {
                            foreach ( var person in family.Members.Select( m => m.Person ) ) {

                                // remove the era flag
                                var eraAttributeValue = attributeValueService.Queryable().Where( v => v.AttributeId == eraAttribute.Id && v.EntityId == person.Id ).FirstOrDefault();
                                if ( eraAttributeValue != null )
                                {
                                    attributeValueService.Delete( eraAttributeValue );
                                }

                                // set end date
                                var eraEndAttributeValue = attributeValueService.Queryable().Where( v => v.AttributeId == eraEndAttribute.Id && v.EntityId == person.Id ).FirstOrDefault();
                                if ( eraEndAttributeValue == null )
                                {
                                    eraEndAttributeValue = new AttributeValue();
                                    eraEndAttributeValue.EntityId = person.Id;
                                    eraEndAttributeValue.AttributeId = eraEndAttribute.Id;
                                    attributeValueService.Add( eraEndAttributeValue );
                                }
                                eraEndAttributeValue.Value = RockDateTime.Now.ToString();

                                // add a history record
                                if ( personAnalyticsCategoryId != 0 && personEntityTypeId != 0 && attributeEntityTypeId != 0 && eraAttributeId != 0 )
                                {
                                    History historyRecord = new History();
                                    historyService.Add( historyRecord );
                                    historyRecord.EntityTypeId = personEntityTypeId;
                                    historyRecord.EntityId = person.Id;
                                    historyRecord.CreatedDateTime = RockDateTime.Now;
                                    historyRecord.CreatedByPersonAliasId = person.PrimaryAliasId;
                                    historyRecord.Caption = "eRA";
                                    historyRecord.Summary = "Exited eRA Status";
                                    historyRecord.Verb = "EXITED";
                                    historyRecord.RelatedEntityTypeId = attributeEntityTypeId;
                                    historyRecord.RelatedEntityId = eraAttributeId;
                                    historyRecord.CategoryId = personAnalyticsCategoryId;
                                }

                                updateContext.SaveChanges();
                            }

                            // launch exit workflow
                            if ( exitWorkflowType.HasValue )
                            {
                                LaunchWorkflow( exitWorkflowType.Value, family );
                            }
                        }
                    }
                }
                else
                {
                    // entered era
                    var family = new GroupService( updateContext ).Queryable( "Members" ).AsNoTracking().Where( m => m.Id == result.FamilyId ).FirstOrDefault();

                    if ( family != null )
                    {
                        foreach ( var person in family.Members.Select( m => m.Person ) )
                        {
                            // set era attribute to true
                            var eraAttributeValue = attributeValueService.Queryable().Where( v => v.AttributeId == eraAttribute.Id && v.EntityId == person.Id ).FirstOrDefault();
                            if ( eraAttributeValue == null )
                            {
                                eraAttributeValue = new AttributeValue();
                                eraAttributeValue.EntityId = person.Id;
                                eraAttributeValue.AttributeId = eraAttribute.Id;
                                attributeValueService.Add( eraAttributeValue );
                            }
                            eraAttributeValue.Value = bool.TrueString;

                            // add start date
                            var eraStartAttributeValue = attributeValueService.Queryable().Where( v => v.AttributeId == eraStartAttribute.Id && v.EntityId == person.Id ).FirstOrDefault();
                            if (eraStartAttributeValue == null )
                            {
                                eraStartAttributeValue = new AttributeValue();
                                eraStartAttributeValue.EntityId = person.Id;
                                eraStartAttributeValue.AttributeId = eraStartAttribute.Id;
                                attributeValueService.Add( eraStartAttributeValue );
                            }
                            eraStartAttributeValue.Value = RockDateTime.Now.ToString();

                            // delete end date if it exists
                            var eraEndAttributeValue = attributeValueService.Queryable().Where( v => v.AttributeId == eraEndAttribute.Id && v.EntityId == person.Id ).FirstOrDefault();
                            if ( eraEndAttributeValue != null )
                            {
                                attributeValueService.Delete( eraEndAttributeValue );
                            }

                            // add a history record
                            if ( personAnalyticsCategoryId != 0 && personEntityTypeId != 0 && attributeEntityTypeId != 0 && eraAttributeId != 0 )
                            {
                                History historyRecord = new History();
                                historyService.Add( historyRecord );
                                historyRecord.EntityTypeId = personEntityTypeId;
                                historyRecord.EntityId = person.Id;
                                historyRecord.CreatedDateTime = RockDateTime.Now;
                                historyRecord.CreatedByPersonAliasId = person.PrimaryAliasId;
                                historyRecord.Caption = "eRA";
                                historyRecord.Summary = "Entered eRA Status";
                                historyRecord.Verb = "ENTERED";
                                historyRecord.RelatedEntityTypeId = attributeEntityTypeId;
                                historyRecord.RelatedEntityId = eraAttributeId;
                                historyRecord.CategoryId = personAnalyticsCategoryId;
                            }

                            updateContext.SaveChanges();
                        }

                        // launch entry workflow
                        if ( entryWorkflowType.HasValue )
                        {
                            LaunchWorkflow( entryWorkflowType.Value, family );
                        }
                    }
                }

                // update stats
            }

            // load giving attributes
            resultContext.Database.ExecuteSqlCommand( "spCrm_FamilyAnalyticsGiving" );

            // load attendance attributes
            resultContext.Database.ExecuteSqlCommand( "spCrm_FamilyAnalyticsAttendance" );
        }

        /// <summary>
        /// Launches the workflow.
        /// </summary>
        /// <param name="workflowTypeGuid">The workflow type unique identifier.</param>
        /// <param name="family">The family.</param>
        private void LaunchWorkflow( Guid workflowTypeGuid, Group family )
        {
            int adultRoleId = GroupTypeCache.Read( SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() ).Roles.Where(r => r.Guid == SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid()).FirstOrDefault().Id;

            var headOfHouse = family.Members.Where( m => m.GroupRoleId == adultRoleId ).OrderBy( m => m.Person.Gender ).FirstOrDefault();

            // don't launch a workflow if no adult is present in the family
            if ( headOfHouse != null && headOfHouse.Person != null && headOfHouse.Person.PrimaryAlias != null ) {
                var spouse = family.Members.Where( m => m.GroupRoleId == adultRoleId && m.PersonId != headOfHouse.Person.Id ).FirstOrDefault();

                using ( var rockContext = new RockContext() )
                {
                    var workflowTypeService = new WorkflowTypeService( rockContext );
                    var workflowType = workflowTypeService.Get( workflowTypeGuid );
                    if ( workflowType != null )
                    {
                        var workflowService = new WorkflowService( rockContext );
                        var workflow = Rock.Model.Workflow.Activate( workflowType, headOfHouse.Person.FullName, rockContext );
                        workflowService.Add( workflow );
                        rockContext.SaveChanges();

                        workflow.SetAttributeValue( "Family", family.Guid );
                        workflow.SetAttributeValue( "HeadOfHouse", headOfHouse.Person.PrimaryAlias.Guid );

                        if ( family.Campus != null )
                        {
                            workflow.SetAttributeValue( "Campus", family.Campus.Guid );
                        }

                        if ( spouse != null && spouse.Person != null && spouse.Person.PrimaryAlias != null )
                        {
                            workflow.SetAttributeValue( "Spouse", spouse.Person.PrimaryAlias.Guid );
                        }

                        workflow.SaveAttributeValues();
                    }
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
