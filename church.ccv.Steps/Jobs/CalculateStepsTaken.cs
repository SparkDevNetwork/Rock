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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

using church.ccv.Steps.Model;
using church.ccv.Datamart.Model;

namespace church.ccv.Steps
{
    /// <summary>
    /// Updates the steps taken for each person in the database
    /// </summary>
    
    
    [DisallowConcurrentExecution]
    [IntegerField( "Baptism Measure Id", "The Measure Id for baptisms.", category: "Measures", order: 1 )]
    [IntegerField( "Membership Measure Id", "The Measure Id for membership.", category: "Measures", order: 2 )]
    [IntegerField( "Attending Measure Id", "The Measure Id for attending.", category: "Measures", order: 3 )]
    [IntegerField( "Connection Measure Id", "The Measure Id for connection.", category: "Measures", order: 4 )]
    [IntegerField( "Giving Measure Id", "The Measure Id for giving.", category: "Measures", order: 5 )]
    [IntegerField( "Serving Measure Id", "The Measure Id for serving.", category: "Measures", order: 6 )]
    [IntegerField( "Sharing Measure Id", "The Measure Id for sharing.", category: "Measures", order: 7 )]
    [IntegerField( "Coaching Measure Id", "The Measure Id for coaching.", category: "Measures", order: 8 )]
    public class CalculateStepsTaken : IJob
    {
        const string ATTRIBUTE_PERSON_DATE_OF_MEMBERSHIP = "DateofMembership";
        const string ATTRIBUTE_GLOBAL_TITHE_THRESHOLD = "TitheThreshold";

        int _baptismMeasureId = -1;
        int _coachingMeasureId = -1;
        int _servingMeasureId = -1;
        int _connectionMeasureId = -1;
        int _membershipMeasureId = -1;
        int _givingMeasureId = -1;
        int _attendingMeasureId = -1;
        int _sharingMeasureId = -1;

        int _searchDateSpan = 365;

        DateTime? lastProcessedDate;

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public CalculateStepsTaken()
        {
            
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            context.Result = "";

            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // read job config
            _baptismMeasureId = dataMap.GetIntFromString( "BaptismMeasureId" );
            _coachingMeasureId = dataMap.GetIntFromString( "CoachingMeasureId" );
            _servingMeasureId = dataMap.GetIntFromString( "ServingMeasureId" );
            _connectionMeasureId = dataMap.GetIntFromString( "ConnectionMeasureId" );
            _membershipMeasureId = dataMap.GetIntFromString( "MembershipMeasureId" );
            _givingMeasureId = dataMap.GetIntFromString( "GivingMeasureId" );
            _attendingMeasureId = dataMap.GetIntFromString( "AttendingMeasureId" );
            _sharingMeasureId = dataMap.GetIntFromString( "SharingMeasureId" );

            // get time of last run
            lastProcessedDate = Rock.Web.SystemSettings.GetValue( "church_ccv_StepsTakenLastUpdate" ).AsDateTime();

            if ( !lastProcessedDate.HasValue )
            {
                lastProcessedDate = RockDateTime.Now.AddDays( -1 ); // if first run use yesterday
            }

            ProcessBaptisms();
            ProcessMembership();
            ProcessWorship();
            ProcessGiving();

            // set lastrun to now
            Rock.Web.SystemSettings.SetValue( "church_ccv_StepsTakenLastUpdate", RockDateTime.Now.ToString() );
        }

        private void ProcessBaptisms()
        {
            // since the baptism date could be added days or possibly weeks after the actual run date we will 
            // query for baptism dates in the last 365 days where there is not a steps taken record. This keeps
            // the performance up and also keeps from adding steps for baptisms that may have occurred at a 
            // church other than CCV.

            var baptismSearchDate = RockDateTime.Now.AddDays( _searchDateSpan * -1 );

            using ( RockContext rockContext = new RockContext() )
            {
                DatamartPersonService datamartPersonService = new DatamartPersonService( rockContext );

                StepTakenService stepTakenService = new StepTakenService( rockContext );
                var baptismStepPersonIds = stepTakenService.Queryable()
                                                .Where( s =>
                                                         s.StepMeasureId == _baptismMeasureId )
                                                 .Select( s => s.PersonAlias.PersonId );

                var newSteps = datamartPersonService.Queryable()
                                                .Where( p => 
                                                        p.IsBaptized == true 
                                                        && p.BaptismDate >= baptismSearchDate 
                                                        && !baptismStepPersonIds.Contains(p.PersonId))
                                                .ToList();

                foreach(var newStep in newSteps )
                {
                    var person = new PersonService( rockContext ).Get( newStep.PersonId );

                    if ( person.PrimaryAliasId.HasValue )
                    {
                        StepTaken step = new StepTaken();
                        step.DateTaken = newStep.BaptismDate.Value;
                        step.StepMeasureId = _baptismMeasureId;
                        step.PersonAliasId = person.PrimaryAliasId.Value;
                        step.CampusId = newStep.CampusId;

                        stepTakenService.Add( step );
                        rockContext.SaveChanges();
                    }
                }
            }
        }

        private void ProcessMembership()
        {
            // membership is like baptism in that it can take a while for the dates to be entered
            // unlike baptism though the date is not stored in the data mart tables so we'll need
            // to read it out of the attributes table

            var membershipSearchDate = RockDateTime.Now.AddDays( _searchDateSpan * -1 ); 

            using (RockContext rockContext = new RockContext() )
            {
                var personEntityType = EntityTypeCache.Read( "Rock.Model.Person" );
                var membershipDateAttribute = new AttributeService( rockContext ).Queryable().AsNoTracking().Where( a => a.Key == ATTRIBUTE_PERSON_DATE_OF_MEMBERSHIP && a.EntityTypeId == personEntityType.Id ).FirstOrDefault();

                AttributeValueService attributeValueService = new AttributeValueService( rockContext );
                StepTakenService stepTakenService = new StepTakenService( rockContext );

                var membershipStepPersonIds = stepTakenService.Queryable()
                                                .Where( s =>
                                                         s.StepMeasureId == _membershipMeasureId )
                                                 .Select( s => s.PersonAlias.PersonId );

                var newSteps = attributeValueService.Queryable()
                                                .Where( a =>
                                                    a.AttributeId == membershipDateAttribute.Id 
                                                    && a.ValueAsDateTime >= membershipSearchDate
                                                    && a.EntityId != null
                                                    && !membershipStepPersonIds.Contains( a.EntityId.Value ) )
                                                .ToList();

                foreach ( var newStep in newSteps )
                {
                    var person = new PersonService( rockContext ).Get( newStep.EntityId.Value );

                    if ( person.PrimaryAliasId.HasValue )
                    {
                        StepTaken step = new StepTaken();
                        step.DateTaken = newStep.ValueAsDateTime.Value;
                        step.StepMeasureId = _membershipMeasureId;
                        step.PersonAliasId = person.PrimaryAliasId.Value;

                        var campus = person.GetCampus();
                        if (campus != null )
                        {
                            step.CampusId = campus.Id;
                        }
                        
                        stepTakenService.Add( step );
                        rockContext.SaveChanges();
                    }
                }

            }
        }

        private void ProcessWorship()
        {
            var attendSearchDate = RockDateTime.Now.AddDays( _searchDateSpan * -1 );

            using ( RockContext rockContext = new RockContext() )
            {

                // this query could take a while so set the timeout up (10min)
                rockContext.Database.CommandTimeout = 600;

                DatamartERAService datamartEraService = new DatamartERAService( rockContext );
                DatamartEraLossService datamartEraLossService = new DatamartEraLossService( rockContext );
                StepTakenService stepTakenService = new StepTakenService( rockContext );

                var previousEraRecord = datamartEraService.Queryable();
                var lossExists = datamartEraLossService.Queryable().Where( l => l.LossDate > RockDateTime.Now ).Select( l => l.FamilyId);

                // key the most recent date someone became an era (in our search range) where they didn't have a previous loss in at least a year
                var newEras = datamartEraService.Queryable()
                                    .Where( e =>
                                         e.WeekendDate >= attendSearchDate
                                         && e.RegularAttendee != previousEraRecord.Where( p => p.FamilyId == e.FamilyId && p.WeekendDate < e.WeekendDate ).OrderByDescending( p => p.WeekendDate ).Select( p => p.RegularAttendee ).FirstOrDefault()
                                         && !lossExists.Contains( e.FamilyId )
                                         && e.RegularAttendee == true
                                    )
                                    .GroupBy(e => e.FamilyId)
                                    .Select(f => new { FamilyId = f.Key, WeekendDate = f.Max( e => e.WeekendDate ) } )
                                    .ToList();

                foreach(var newEra in newEras )
                {
                    // add a step for each adult in the family
                    Guid adultRoleGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
                    var familyMembers = new GroupMemberService( rockContext ).Queryable()
                                            .Where(f => f.GroupId == newEra.FamilyId && f.GroupRole.Guid == adultRoleGuid )
                                            .Select(m => m.Person);

                    foreach(var person in familyMembers ) {
                            StepTaken step = new StepTaken();
                            step.DateTaken = newEra.WeekendDate;
                            step.StepMeasureId = _attendingMeasureId;
                            step.PersonAliasId = person.PrimaryAliasId.Value;

                            var campus = person.GetCampus();
                            if ( campus != null )
                            {
                                step.CampusId = campus.Id;
                            }

                        stepTakenService.Add( step );
                            rockContext.SaveChanges();
                    }
                }
            }
        }

        private void ProcessGiving()
        {
            // To get the give step you must give more than $250 in 12 months. If you drop below this amount you won’t get 
            // another give step unless you remain below $250 for more than 12 months.
            //
            // To be able to calculate this logic we need to write to the person's history when the cross into and out of 
            // the giving threshold.

            // STEP 1: Find all people giving over $250 whos last history is STOPPED and is older than 12 months, or they don't have a giving history of giving. 
            //         We'll give them a new step.

            var lastYearDate = RockDateTime.Now.AddYears( -1 );

            using ( RockContext rockContext = new RockContext() )
            {
                DatamartPersonService datamartPersonService = new DatamartPersonService( rockContext );

                decimal titheThreshold = GlobalAttributesCache.Read().GetValue( ATTRIBUTE_GLOBAL_TITHE_THRESHOLD ).AsDecimal();

                StepTakenService stepTakenService = new StepTakenService( rockContext );
                HistoryService historyService = new HistoryService( rockContext );

                var personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
                var attributeEntityTypeId = EntityTypeCache.Read( "Rock.Model.Attribute" ).Id;

                var olderThan12MonthsHistory = new HistoryService( rockContext ).Queryable()
                                                .Where( h =>
                                                    h.Caption == "Giving"
                                                    && h.Verb == "STOPPED"
                                                    && h.CreatedDateTime < lastYearDate
                                                )
                                                .Select( h => h.EntityId );

                var noHistory = new HistoryService( rockContext ).Queryable()
                                                .Where( h =>
                                                    h.Caption == "Giving"
                                                );

                var newSteps = datamartPersonService.Queryable()
                                                .Where( p =>
                                                        p.GivingLast12Months > titheThreshold
                                                        && ( olderThan12MonthsHistory.Contains( p.PersonId ) ) || !noHistory.Any(h => h.EntityId == p.PersonId) )
                                                .ToList();

                foreach ( var newStep in newSteps )
                {
                    var person = new PersonService( rockContext ).Get( newStep.PersonId );

                    if ( person.PrimaryAliasId.HasValue )
                    {
                        StepTaken step = new StepTaken();
                        step.DateTaken = RockDateTime.Now;
                        step.StepMeasureId = _givingMeasureId;
                        step.PersonAliasId = person.PrimaryAliasId.Value;

                        var campus = person.GetCampus();
                        if ( campus != null )
                        {
                            step.CampusId = campus.Id;
                        }

                        stepTakenService.Add( step );
                        rockContext.SaveChanges();
                    }
                }

                // STEP 2: Find all people giving over $250 whos last history is STOPPED, or they don't have a giving history of giving. 
                //         We'll mark them as STARTED
                var stoppedHistory = new HistoryService( rockContext ).Queryable()
                                                .Where( h =>
                                                    h.Caption == "Giving"
                                                    && h.Verb == "STOPPED"
                                                );

                var markStarted = datamartPersonService.Queryable()
                                                .Where( p =>
                                                        p.GivingLast12Months > titheThreshold
                                                        && (stoppedHistory.Any(h => h.EntityId == p.PersonId )) || !noHistory.Any( h => h.EntityId == p.PersonId ) )
                                                .ToList();

                foreach(var item in markStarted )
                {
                    var person = new PersonService( rockContext ).Get( item.PersonId );
                    if ( person.PrimaryAliasId.HasValue )
                    {
                        History history = new History();
                        historyService.Add( history );
                        history.EntityTypeId = personEntityTypeId;
                        history.EntityId = item.PersonId;
                        history.RelatedEntityTypeId = attributeEntityTypeId;
                        history.RelatedEntityId = 12;
                        history.Caption = "Giving";
                        history.Summary = "Started Giving (CCV)";
                        history.Verb = "STARTED";
                        history.CreatedDateTime = RockDateTime.Now;
                        history.CreatedByPersonAliasId = person.PrimaryAliasId;

                        rockContext.SaveChanges();
                    }
                }

                // STEP 3: Find all people giving less $250 whos last history is STARTED 
                //         We'll mark them as STOPPED

                var startedHistory = new HistoryService( rockContext ).Queryable()
                                                    .Where( h =>
                                                        h.Caption == "Giving"
                                                        && h.Verb == "STARTED"
                                                    );
                var markStopped = datamartPersonService.Queryable()
                                                .Where( p =>
                                                        p.GivingLast12Months <= titheThreshold
                                                        && startedHistory.Any( h => h.EntityId == p.PersonId )
                                                 )
                                                .ToList();

                foreach ( var item in markStopped )
                {
                    var person = new PersonService( rockContext ).Get( item.PersonId );
                    if ( person.PrimaryAliasId.HasValue )
                    {
                        History history = new History();
                        historyService.Add( history );
                        history.EntityTypeId = personEntityTypeId;
                        history.EntityId = item.PersonId;
                        history.RelatedEntityTypeId = attributeEntityTypeId;
                        history.RelatedEntityId = 12;
                        history.Caption = "Giving";
                        history.Summary = "Started Giving (CCV)";
                        history.Verb = "STOPPED";
                        history.CreatedDateTime = RockDateTime.Now;
                        history.CreatedByPersonAliasId = person.PrimaryAliasId;

                        rockContext.SaveChanges();
                    }
                }
            }


        }

        private void ProcessGroupTypes(List<int> groupTypes, int measureId)
        {
            // get list of history STARTS for the group type(s) since our last run where there was no STOP in the last 12 months

            var groupTypeEntityTypeId = EntityTypeCache.Read( "Rock.Model.GroupType" ).Id;
            int personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;

            using (RockContext rockContext = new RockContext() )
            {
                var newStarts = new HistoryService( rockContext ).Queryable()
                                            .Where( h =>
                                                 h.EntityTypeId == personEntityTypeId
                                                 && h.RelatedEntityTypeId == groupTypeEntityTypeId
                                                 && h.RelatedEntityId.HasValue
                                                 && groupTypes.Contains( h.RelatedEntityId.Value )
                                                 && h.CreatedDateTime > lastProcessedDate.Value
                                             )
                                             .GroupBy( h => h.EntityId )
                                             .Select( g => g.OrderByDescending( h => h.CreatedDateTime ).FirstOrDefault() )
                                             .Where( h => h.Verb == "STARTED")
                                             .ToList();

                foreach(var start in newStarts )
                {
                    // check when the last stop was
                    var lastStop = new HistoryService( rockContext ).Queryable()
                                        .Where( h =>
                                                h.EntityTypeId == start.EntityTypeId
                                                && h.EntityId == start.EntityId
                                                && h.RelatedEntityTypeId == start.RelatedEntityTypeId
                                                && h.RelatedEntityId == start.RelatedEntityId
                                                && h.CreatedDateTime < start.CreatedDateTime
                                        )
                                        .GroupBy( h => h.EntityId )
                                        .Select( g => g.OrderByDescending( h => h.CreatedDateTime ).FirstOrDefault() )
                                        .Where( h => h.Verb == "STOPPED" )
                                        .FirstOrDefault();

                    if (lastStop == null || lastStop.CreatedDateTime < lastProcessedDate.Value.AddYears( -1 ) )
                    {
                        RockContext updateContext = new RockContext();
                        StepTakenService stepTakenService = new StepTakenService( updateContext );

                        Person person = new PersonService( rockContext ).Get( start.EntityId );

                        if ( person != null )
                        {
                            // create new step
                            StepTaken step = new StepTaken();
                            step.DateTaken = start.CreatedDateTime.Value;
                            step.StepMeasureId = measureId;
                            step.PersonAliasId = person.PrimaryAliasId.Value;
                            step.CampusId = person.GetCampus().Id;

                            stepTakenService.Add( step );
                            updateContext.SaveChanges();
                        }
                    }
                }
            }
        }
    }
}
