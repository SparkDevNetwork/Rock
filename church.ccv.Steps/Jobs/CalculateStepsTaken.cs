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

        StringBuilder _resultMessages;

        int _personAnalyticsCategoryId;

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
            _resultMessages = new StringBuilder();

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

            const string ATTRIBUTE_GLOBAL_COACHING_GROUPTYPE_IDS = "CoachingGroupTypeIds";
            const string ATTRIBUTE_GLOBAL_CONNECTION_GROUPTYPE_IDS = "ConnectionGroupTypeIds";
            const string ATTRIBUTE_GLOBAL_SERVING_GROUPTYPE_IDS = "ServingGroupTypeIds";

            List<int> _coachingGroupTypeIds = new List<int>();
            List<int> _connectionGroupTypeIds = new List<int>();
            List<int> _servingGroupTypeIds = new List<int>();


            _personAnalyticsCategoryId = CategoryCache.Read( Rock.SystemGuid.Category.HISTORY_PERSON_ANALYTICS.AsGuid() ).Id;

            try
            {
                _coachingGroupTypeIds = GlobalAttributesCache.Read().GetValue( ATTRIBUTE_GLOBAL_COACHING_GROUPTYPE_IDS ).Split( ',' ).Select( int.Parse ).ToList();
                _connectionGroupTypeIds = GlobalAttributesCache.Read().GetValue( ATTRIBUTE_GLOBAL_CONNECTION_GROUPTYPE_IDS ).Split( ',' ).Select( int.Parse ).ToList();
                _servingGroupTypeIds = GlobalAttributesCache.Read().GetValue( ATTRIBUTE_GLOBAL_SERVING_GROUPTYPE_IDS ).Split( ',' ).Select( int.Parse ).ToList();
            }
            catch { }

            ProcessBaptisms();
            ProcessMembership();
            ProcessWorship();
            ProcessGiving();
            ProcessGroupTypes( _servingGroupTypeIds, _servingMeasureId, false ); // serving
            ProcessGroupTypes( _connectionGroupTypeIds, _connectionMeasureId, false ); // connection
            ProcessGroupTypes( _coachingGroupTypeIds, _coachingMeasureId, true ); // coaching

            // delete any duplicate steps (this can occur if the job failed mid-course in the past)
            var dedupe = @"DELETE FROM [_church_ccv_Steps_StepTaken] WHERE [Id] IN(
SELECT MIN(Id) FROM [_church_ccv_Steps_StepTaken] st
INNER JOIN
	(SELECT PersonAliasId, stepmeasureid FROM (
		SELECT PersonAliasId, stepmeasureid, count(*) AS DUPECOUNT
		  FROM [_church_ccv_Steps_StepTaken]
		  group by PersonAliasId, stepmeasureid
		  ) rs
		WHERE rs.DUPECOUNT > 1) i ON i.PersonAliasId = st.PersonAliasId AND i.StepMeasureId = st.StepMeasureId
GROUP BY st.PersonAliasId, st.stepmeasureid)";

            new RockContext().Database.ExecuteSqlCommand( dedupe );

            // set lastrun to now
            Rock.Web.SystemSettings.SetValue( "church_ccv_StepsTakenLastUpdate", RockDateTime.Now.ToString() );

            if ( _resultMessages.Length > 0 )
            {
                context.Result = _resultMessages.ToString().ReplaceLastOccurrence( ",", "" );
            }
        }

        private void ProcessBaptisms()
        {
            // since the baptism date could be added days or possibly weeks after the actual run date we will 
            // query for baptism dates in the last 365 days where there is not a steps taken record. This keeps
            // the performance up and also keeps from adding steps for baptisms that may have occurred at a 
            // church other than CCV.

            int stepCounter = 0;

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
                                                        && p.BaptismDate <= RockDateTime.Now
                                                        && !baptismStepPersonIds.Contains(p.PersonId))
                                                 .GroupBy( p => p.PersonId) // currently the era person table has duplicates for the same person id :(
                                                 .Select( g => g.FirstOrDefault())
                                                .ToList();

                foreach(var newStep in newSteps )
                {
                    using ( RockContext updateConext = new RockContext() )
                    {
                        var person = new PersonService( updateConext ).Get( newStep.PersonId );

                        if ( person != null && person.PrimaryAliasId.HasValue )
                        {
                            StepTakenService stepTakenUpdateService = new StepTakenService( updateConext );

                            StepTaken step = new StepTaken();
                            step.DateTaken = newStep.BaptismDate.Value;
                            step.StepMeasureId = _baptismMeasureId;
                            step.PersonAliasId = person.PrimaryAliasId.Value;
                            step.CampusId = newStep.CampusId;

                            stepTakenUpdateService.Add( step );
                            updateConext.SaveChanges();

                            stepCounter++;
                        }
                    }
                }
            }

            if (stepCounter > 0 )
            {
                _resultMessages.Append( string.Format( "Baptism Steps {0},", stepCounter ));
            }
        }

        private void ProcessMembership()
        {
            // membership is like baptism in that it can take a while for the dates to be entered
            // unlike baptism though the date is not stored in the data mart tables so we'll need
            // to read it out of the attributes table

            int stepCounter = 0;

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
                                                    && a.ValueAsDateTime <= RockDateTime.Now
                                                    && a.EntityId != null
                                                    && !membershipStepPersonIds.Contains( a.EntityId.Value ) )
                                                .ToList();

                foreach ( var newStep in newSteps )
                {
                    using ( RockContext updateConext = new RockContext() )
                    {
                        var person = new PersonService( updateConext ).Get( newStep.EntityId.Value );

                        if ( person != null && person.PrimaryAliasId.HasValue )
                        {
                            StepTakenService stepTakenUpdateService = new StepTakenService( updateConext );

                            StepTaken step = new StepTaken();
                            step.DateTaken = newStep.ValueAsDateTime.Value;
                            step.StepMeasureId = _membershipMeasureId;
                            step.PersonAliasId = person.PrimaryAliasId.Value;

                            var campus = person.GetCampus();
                            if ( campus != null )
                            {
                                step.CampusId = campus.Id;
                            }

                            stepTakenUpdateService.Add( step );
                            updateConext.SaveChanges();

                            stepCounter++;
                        }
                    }
                }
            }

            if ( stepCounter > 0 )
            {
                _resultMessages.Append( string.Format( "Membership Steps {0},", stepCounter ) );
            }
        }

        private void ProcessWorship()
        {
            int stepCounter = 0;

            var lastYearDate = RockDateTime.Now.AddYears( -1 );

            using ( RockContext rockContext = new RockContext() )
            {
                DatamartPersonService datamartPersonService = new DatamartPersonService( rockContext );

                var newSteps = new HistoryService(rockContext).Queryable()
                                                .Where( h =>
                                                        h.Caption == "eRA"
                                                        && h.Verb == "ENTERED"
                                                        && h.CreatedDateTime > lastProcessedDate
                                                 )
                                                .ToList();

                foreach ( var newStep in newSteps )
                {
                    // check if era has had an exit in the last 12 months
                    var hasExitedIn12Months = new HistoryService( rockContext ).Queryable()
                                                .Where( h =>
                                                    h.Caption == "eRA"
                                                    && h.Verb == "EXITED"
                                                    && h.CreatedDateTime < lastYearDate
                                                )
                                                .Any();

                    if ( !hasExitedIn12Months )
                    {
                        using ( RockContext updateConext = new RockContext() )
                        {
                            var person = new PersonService( updateConext ).Get( newStep.EntityId );

                            if ( person != null && person.PrimaryAliasId.HasValue )
                            {
                                StepTakenService stepTakenUpdateService = new StepTakenService( updateConext );

                                StepTaken step = new StepTaken();
                                if ( newStep.CreatedDateTime.HasValue )
                                {
                                    step.DateTaken = newStep.CreatedDateTime.Value;
                                }
                                else
                                {
                                    step.DateTaken = RockDateTime.Now;
                                }
                                step.StepMeasureId = _attendingMeasureId;
                                step.PersonAliasId = person.PrimaryAliasId.Value;

                                var campus = person.GetCampus();
                                if ( campus != null )
                                {
                                    step.CampusId = campus.Id;
                                }

                                stepTakenUpdateService.Add( step );
                                updateConext.SaveChanges();

                                stepCounter++;
                            }
                        }
                    }
                }
            }

            if ( stepCounter > 0 )
            {
                _resultMessages.Append( string.Format( "Attending (eRA) Steps {0},", stepCounter ) );
            }
        }

        private void ProcessGiving()
        {
            // To get the give step you must give more than $250 in 12 months. If you drop below this amount you won’t get 
            // another give step unless you remain below $250 for more than 12 months.
            //
            // To be able to calculate this logic we need to write to the person's history when the cross into and out of 
            // the giving threshold.

            int stepCounter = 0;
            int? personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
            int? attributeEntityTypeId = EntityTypeCache.Read( "Rock.Model.Attribute" ).Id;
            int? givingAmountAttributeId = AttributeCache.Read( "1F96525E-68CA-47C4-A793-E9C0BDAEF18F".AsGuid() ).Id;

            decimal titheThreshold = GlobalAttributesCache.Read().GetValue( ATTRIBUTE_GLOBAL_TITHE_THRESHOLD ).AsDecimal();

            if ( personEntityTypeId != null && attributeEntityTypeId != null && givingAmountAttributeId != null )
            {
                using ( RockContext rockContext = new RockContext() )
                {
                    rockContext.Database.CommandTimeout = 1200;

                    var lastYearDate = RockDateTime.Now.AddYears( -1 );

                    // get most recent history for giving
                    var historyRecords = new HistoryService( rockContext ).Queryable()
                                        .Where( h =>
                                             h.EntityTypeId == personEntityTypeId
                                             && h.RelatedEntityTypeId == attributeEntityTypeId
                                             && h.RelatedEntityId == givingAmountAttributeId
                                             && h.Caption == "Giving"
                                         )
                                         .GroupBy( h => h.EntityId )
                                         .Select( g => g.OrderByDescending( h => h.CreatedDateTime ).Select( h => new { h.EntityId, h.Verb } ).FirstOrDefault() )
                                         .ToList();

                    // get people giving over limit
                    DatamartPersonService datamartPersonService = new DatamartPersonService( rockContext );
                    var givers = datamartPersonService.Queryable()
                                                .Where( p =>
                                                        p.GivingLast12Months > titheThreshold
                                                )
                                                .GroupBy( p => p.PersonId ) // currently the person datamart has dupes :(
                                                .Select( g => g.FirstOrDefault() )
                                                .ToList();

                    // get people needing a start history record
                    var needsStartDate = givers.Where( p => !historyRecords.Any( h => h.EntityId == p.PersonId && h.Verb == "STARTED" ) );

                    foreach ( var itemStart in needsStartDate )
                    {
                        using ( RockContext updateContext = new RockContext() )
                        {
                            var person = new PersonService( updateContext ).Get( itemStart.PersonId );
                            if ( person != null && person.PrimaryAliasId.HasValue )
                            {
                                HistoryService historyService = new HistoryService( updateContext );
                                History history = new History();
                                historyService.Add( history );
                                history.EntityTypeId = personEntityTypeId.Value;
                                history.EntityId = itemStart.PersonId;
                                history.RelatedEntityTypeId = attributeEntityTypeId;
                                history.RelatedEntityId = givingAmountAttributeId;
                                history.Caption = "Giving";
                                history.Summary = "Started Giving";
                                history.Verb = "STARTED";
                                history.CreatedDateTime = RockDateTime.Now;
                                history.CreatedByPersonAliasId = person.PrimaryAliasId;
                                history.CategoryId = _personAnalyticsCategoryId;

                                updateContext.SaveChanges();

                                // check if there was a stop in the last 12 months, if not write step
                                var hasStopInLastYear = new HistoryService( rockContext ).Queryable()
                                                            .Where( h =>
                                                                         h.Caption == "Giving"
                                                                         && h.Verb == "STOPPED"
                                                                         && h.CreatedDateTime > lastYearDate
                                                                         && h.EntityId == person.Id
                                                                   )
                                                                   .Any();

                                if ( !hasStopInLastYear )
                                {
                                    StepTakenService stepTakenUpdateService = new StepTakenService( updateContext );

                                    StepTaken step = new StepTaken();
                                    step.DateTaken = RockDateTime.Now;
                                    step.StepMeasureId = _givingMeasureId;
                                    step.PersonAliasId = person.PrimaryAliasId.Value;

                                    var campus = person.GetCampus();
                                    if ( campus != null )
                                    {
                                        step.CampusId = campus.Id;
                                    }

                                    stepTakenUpdateService.Add( step );
                                    updateContext.SaveChanges();

                                    stepCounter++;
                                }
                            }
                        }
                    }

                    // get people needing a stop history record
                    var needsStoppedDate = historyRecords.Where( h => h.Verb == "STARTED" && !givers.Any( m => m.PersonId == h.EntityId ) );

                    foreach ( var itemStop in needsStoppedDate )
                    {
                        using ( RockContext updateContext = new RockContext() )
                        {
                            var person = new PersonService( updateContext ).Get( itemStop.EntityId );
                            if ( person.PrimaryAliasId.HasValue )
                            {
                                HistoryService historyService = new HistoryService( updateContext );
                                History history = new History();
                                historyService.Add( history );
                                history.EntityTypeId = personEntityTypeId.Value;
                                history.EntityId = itemStop.EntityId;
                                history.RelatedEntityTypeId = attributeEntityTypeId;
                                history.RelatedEntityId = givingAmountAttributeId;
                                history.Caption = "Giving";
                                history.Summary = "Stopped Giving";
                                history.Verb = "STOPPED";
                                history.CreatedDateTime = RockDateTime.Now;
                                history.CreatedByPersonAliasId = person.PrimaryAliasId;
                                history.CategoryId = _personAnalyticsCategoryId;

                                updateContext.SaveChanges();
                            }
                        }
                    }
                }
            }

            if ( stepCounter > 0 )
            {
                _resultMessages.Append( string.Format( "Giving Steps {0},", stepCounter ) );
            }
        }

        private void ProcessGroupTypes(List<int> groupTypes, int measureId, bool leaderRequired = false)
        {
            // get list of history STARTS for the group type(s) since our last run where there was no STOP in the last 12 months

            int stepCounter = 0;

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
                        bool isLeader = false;
                        if ( leaderRequired )
                        {
                            // check is the person is a leader in a group of this type
                            isLeader = new GroupMemberService( rockContext ).Queryable()
                                            .Where( m =>
                                                 groupTypes.Contains( m.Group.GroupTypeId )
                                                 && m.GroupRole.IsLeader
                                                 && m.GroupMemberStatus == GroupMemberStatus.Active
                                                 && m.PersonId == start.EntityId
                                            ).Any();
                        }

                        if ( !leaderRequired || isLeader )
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

                                stepCounter++;
                            }
                        }
                    }
                }
            }

            if ( stepCounter > 0 )
            {
                _resultMessages.Append( string.Format( "Group Type ({1}) Steps {0},", stepCounter, string.Join( ",", groupTypes.Select( n => n.ToString() ).ToArray() ) ) );
            }
        }
    }
}
