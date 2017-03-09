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
using church.ccv.Actions.Data;
using church.ccv.Actions.Models;

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
        int _baptismMeasureId = -1;
        int _coachingMeasureId = -1;
        int _servingMeasureId = -1;
        int _connectionMeasureId = -1;
        int _membershipMeasureId = -1;
        int _givingMeasureId = -1;
        int _attendingMeasureId = -1;
        int _sharingMeasureId = -1;

        int _searchDateSpan = 60;

        DateTime? lastProcessedDate;

        StringBuilder _resultMessages;

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
            // How this WORKS:
            // We run and get the Action History rows matching: The date we last ran, and the date closest to today.
            // For each step, we grab only people who took that step in the past YEAR.
            // We compare the prevRun Rows against lastest Rows. If something went from false to true, they performed the action.
            // Then, we add that to the Steps table.
            // The next time it runs, we'll still see they performed the action, but they'll be filtered out by the last YEAR step filter.

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

            // get our contexts
            using ( RockContext rockContext = new RockContext( ) )
            {
                // some of these queries join large amounts of data and can take > 30 seconds, so give two minutes.
                rockContext.Database.CommandTimeout = 120;

                Service<ActionsHistory_Adult_Person> adultActionsService = new Service<ActionsHistory_Adult_Person>( rockContext );

                // get our most recent history date, and the date closest to when this job last ran
                DateTime? lastProcessedHistoryDate = null;
                DateTime? latestHistoryDate = null;

                try
                {
                    latestHistoryDate = adultActionsService.Queryable( ).AsNoTracking( ).Where( ah => ah.Date <= RockDateTime.Now ).Max( ah => ah.Date );
                    lastProcessedHistoryDate = adultActionsService.Queryable( ).AsNoTracking( ).Where( ah => ah.Date <= lastProcessedDate ).Max( ah => ah.Date );
                }
                catch
                {
                    // don't worry about catching, we'll just fail below
                }

                if ( latestHistoryDate.HasValue )
                {
                    if( lastProcessedHistoryDate.HasValue )
                    {
                        // First, get everyone we've recorded as having taken a step over the past year.
                        // We'll filter out anyone in this list so that we don't count them having taken any single step more than once a year.
                        DateTime oneYearAgo = new DateTime( RockDateTime.Now.Year - 1, RockDateTime.Now.Month, RockDateTime.Now.Day );
                        StepTakenService stepTakenService = new StepTakenService( rockContext );
                        IQueryable<StepTaken> pastYearStepsQuery = stepTakenService.Queryable( ).AsNoTracking( ).Where( s => s.DateTaken >= oneYearAgo );
                        
                        // Most of these require the history when we last processed, and the most recent history.
                        // we then compare those to see if anything went from 'false' to 'true', meaning they began one of the steps.
                        
                        // Note - this means that if this isn't run for a month, and someone started giving within that month, THIS JOB will consider
                        // their start date for giving to be NOW, because we're not grabbing the history date for exactly when they started giving.
                        // However, this job is designed to be run once a week, so it shouldn't matter
                        IQueryable<ActionsHistory_Adult_Person> lastProcessedHistory = adultActionsService.Queryable( ).Where( ah => ah.Date == lastProcessedHistoryDate ).AsNoTracking( );
                        IQueryable<ActionsHistory_Adult_Person> latestHistory = adultActionsService.Queryable( ).Where( ah => ah.Date == latestHistoryDate ).AsNoTracking( );

                        // Baptisms and Membership have actual dates that they occurred, so we can simply pass in the latest history
                        ProcessBaptisms( pastYearStepsQuery, latestHistory );
                        ProcessMembership( pastYearStepsQuery, latestHistory );
                        
                        ProcessWorship( pastYearStepsQuery, lastProcessedHistory, latestHistory );
                        ProcessGiving( pastYearStepsQuery, lastProcessedHistory, latestHistory );
                        ProcessServing( pastYearStepsQuery, lastProcessedHistory, latestHistory );
                        ProcessCoaching( pastYearStepsQuery, lastProcessedHistory, latestHistory );
                        ProcessConnected( pastYearStepsQuery, lastProcessedHistory, latestHistory );
                        
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

                        new RockContext( ).Database.ExecuteSqlCommand( dedupe );
                        
                        if ( _resultMessages.Length > 0 )
                        {
                            context.Result = _resultMessages.ToString( ).ReplaceLastOccurrence( ",", "" );
                        }
                        else
                        {
                            context.Result = "Up to date. No new steps taken.";
                        }
                    }
                    else
                    {
                        context.Result = "Not enough Actions History available. Ensure history is available on or before today, and try again.";
                    }

                    // Always set the last update to the latest history date. That way, if there was no "prior history" available,
                    // the current history will be treated as "prior history" the next time we run.
                    Rock.Web.SystemSettings.SetValue( "church_ccv_StepsTakenLastUpdate", latestHistoryDate.Value.ToString( ) );
                }
                else
                {
                    // if there's no history whatsoever for today or in the past, let them know. (This should NEVER happen)
                    context.Result = "No Actions History available. Ensure history is available on or before today, and try again.";
                }
            }
        }
        
        private void ProcessBaptisms( IQueryable<StepTaken> pastYearStepsQuery, IQueryable<ActionsHistory_Adult_Person> adultActions )
        {            
            // get baptisms in the past year
            IQueryable<int> baptismStepPersonAliasIds = pastYearStepsQuery.Where( s => s.StepMeasureId == _baptismMeasureId ).Select( s => s.PersonAlias.Id );

            // find all people who have had a baptism within the timeframe and are not already in our list.
            var baptismSearchDate = RockDateTime.Now.AddDays( _searchDateSpan * -1 );
            var newSteps = adultActions.Where( a =>
                                               a.Baptised.HasValue &&
                                               a.Baptised.Value >= baptismSearchDate &&
                                               a.Baptised.Value <= RockDateTime.Now &&
                                               !baptismStepPersonAliasIds.Contains( a.PersonAliasId ) )
                                       .Select( a => new NewStep
                                                     {
                                                         DateTaken = a.Baptised.Value,
                                                         PersonAliasId = a.PersonAliasId
                                                     } )
                                       .ToList( );
            

            int stepCounter = SaveSteps( _baptismMeasureId, newSteps );

            if ( stepCounter > 0 )
            {
                _resultMessages.Append( string.Format( "Baptism Steps {0}, ", stepCounter ) );
            }
        }

        private void ProcessMembership( IQueryable<StepTaken> pastYearStepsQuery, IQueryable<ActionsHistory_Adult_Person> adultActions )
        {
            // get a list of members in the past year
            IQueryable<int> membershipStepPersonAliasIds = pastYearStepsQuery.Where( s => s.StepMeasureId == _membershipMeasureId ).Select( s => s.PersonAlias.Id );

            // find all people who have become members within the timeframe and are not already in our list.
            var membershipSearchDate = RockDateTime.Now.AddDays( _searchDateSpan * -1 );
            var newSteps = adultActions.Where( a =>
                                               a.Member.HasValue &&
                                               a.Member.Value >= membershipSearchDate &&
                                               a.Member.Value <= RockDateTime.Now &&
                                               !membershipStepPersonAliasIds.Contains( a.PersonAliasId ) )
                                       .Select( a => new NewStep
                                                     {
                                                         DateTaken = a.Member.Value,
                                                         PersonAliasId = a.PersonAliasId
                                                     } )
                                       .ToList( );

            int stepCounter = SaveSteps( _membershipMeasureId, newSteps );

            if ( stepCounter > 0 )
            {
                _resultMessages.Append( string.Format( "Membership Steps {0}, ", stepCounter ) );
            }
        }

        private void ProcessWorship( IQueryable<StepTaken> pastYearStepsQuery, IQueryable<ActionsHistory_Adult_Person> prevRunHistory, IQueryable<ActionsHistory_Adult_Person> latestHistory )
        {
            // get a list of ERAs in the past year
            IQueryable<int> worshipStepPersonAliasIds = pastYearStepsQuery.Where( s => s.StepMeasureId == _attendingMeasureId ).Select( s => s.PersonAlias.Id );

            // find all people who weren't an ERA when we last ran, but now are.
            var newSteps = latestHistory.Join( prevRunHistory, ah => ah.PersonAliasId, ph => ph.PersonAliasId, ( ah, ph ) => 
                                               new
                                               {
                                                   PersonAliasId = ah.PersonAliasId,
                                                   DateTaken = ah.Date,
                                                   PrevERA = ph.ERA,
                                                   CurrERA = ah.ERA
                                               })
                                        .Where( a => worshipStepPersonAliasIds.Contains( a.PersonAliasId ) == false &&
                                                    a.PrevERA == false && 
                                                    a.CurrERA == true )
                                        .Select( s => new NewStep
                                        {
                                            PersonAliasId = s.PersonAliasId,
                                            DateTaken = s.DateTaken
                                        })
                                        .ToList( );

            int stepCounter = SaveSteps( _attendingMeasureId, newSteps );

            if ( stepCounter > 0 )
            {
                _resultMessages.Append( string.Format( "Worship Steps {0}, ", stepCounter ) );
            }
        }

        private void ProcessGiving( IQueryable<StepTaken> pastYearStepsQuery, IQueryable<ActionsHistory_Adult_Person> prevRunHistory, IQueryable<ActionsHistory_Adult_Person> latestHistory )
        {
            // get a list of givers in the past year
            IQueryable<int> giveStepPersonAliasIds = pastYearStepsQuery.Where( s => s.StepMeasureId == _givingMeasureId ).Select( s => s.PersonAlias.Id );
                                
            // find all people who weren't givers when we last ran, but now are.
            var newSteps = latestHistory.Join( prevRunHistory, ah => ah.PersonAliasId, ph => ph.PersonAliasId, ( ah, ph ) => 
                                               new
                                               {
                                                   PersonAliasId = ah.PersonAliasId,
                                                   DateTaken = ah.Date,
                                                   PrevGive = ph.Give,
                                                   CurrGive = ah.Give
                                               })
                                        .Where( a => giveStepPersonAliasIds.Contains( a.PersonAliasId ) == false &&
                                                     a.PrevGive == false && 
                                                     a.CurrGive == true )
                                        .Select( s => new NewStep
                                        {
                                            PersonAliasId = s.PersonAliasId,
                                            DateTaken = s.DateTaken
                                        })
                                        .ToList( );

            int stepCounter = SaveSteps( _givingMeasureId, newSteps );

            if ( stepCounter > 0 )
            {
                _resultMessages.Append( string.Format( "Giving Steps {0}, ", stepCounter ) );
            }
        }

        private void ProcessServing( IQueryable<StepTaken> pastYearStepsQuery, IQueryable<ActionsHistory_Adult_Person> prevRunHistory, IQueryable<ActionsHistory_Adult_Person> latestHistory )
        {
            // get a list of people serving in the past year
            IQueryable<int> servingStepPersonAliasIds = pastYearStepsQuery.Where( s => s.StepMeasureId == _servingMeasureId ).Select( s => s.PersonAlias.Id );
                                
            // find all people who weren't serving when we last ran, but now are.
            var newSteps = latestHistory.Join( prevRunHistory, ah => ah.PersonAliasId, ph => ph.PersonAliasId, ( ah, ph ) => 
                                               new
                                               {
                                                   PersonAliasId = ah.PersonAliasId,
                                                   DateTaken = ah.Date,
                                                   PrevServing = ph.Serving,
                                                   CurrServing = ah.Serving
                                               })
                                        .Where( a => servingStepPersonAliasIds.Contains( a.PersonAliasId ) == false &&
                                                     a.PrevServing == false && 
                                                     a.CurrServing == true )
                                        .Select( s => new NewStep
                                        {
                                            PersonAliasId = s.PersonAliasId,
                                            DateTaken = s.DateTaken
                                        })
                                        .ToList( );

            int stepCounter = SaveSteps( _servingMeasureId, newSteps );

            if ( stepCounter > 0 )
            {
                _resultMessages.Append( string.Format( "Serving Steps {0}, ", stepCounter ) );
            }
        }

        private void ProcessCoaching( IQueryable<StepTaken> pastYearStepsQuery, IQueryable<ActionsHistory_Adult_Person> prevRunHistory, IQueryable<ActionsHistory_Adult_Person> latestHistory )
        {
            // note - Coaching is the name of the Next Step, but the action driving it is called "Teaching"

            // get a list of people teaching in the past year
            IQueryable<int> coachingStepPersonAliasIds = pastYearStepsQuery.Where( s => s.StepMeasureId == _coachingMeasureId ).Select( s => s.PersonAlias.Id );
                                
            // find all people who weren't teaching when we last ran, but now are.
            var newSteps = latestHistory.Join( prevRunHistory, ah => ah.PersonAliasId, ph => ph.PersonAliasId, ( ah, ph ) => 
                                               new
                                               {
                                                   PersonAliasId = ah.PersonAliasId,
                                                   DateTaken = ah.Date,
                                                   PrevTeaching = ph.Teaching,
                                                   CurrTeaching = ah.Teaching
                                               })
                                        .Where( a => coachingStepPersonAliasIds.Contains( a.PersonAliasId ) == false &&
                                                     a.PrevTeaching == false && 
                                                     a.CurrTeaching == true )
                                        .Select( s => new NewStep
                                        {
                                            PersonAliasId = s.PersonAliasId,
                                            DateTaken = s.DateTaken
                                        })
                                        .ToList( );

            int stepCounter = SaveSteps( _coachingMeasureId, newSteps );

            if ( stepCounter > 0 )
            {
                _resultMessages.Append( string.Format( "Coaching Steps {0}, ", stepCounter ) );
            }
        }

        private void ProcessConnected( IQueryable<StepTaken> pastYearStepsQuery, IQueryable<ActionsHistory_Adult_Person> prevRunHistory, IQueryable<ActionsHistory_Adult_Person> latestHistory )
        {
            // note - Connected is the name of the Next Step, but the action driving it is called "Peer Learning"

            // get a list of people connected in the past year
            IQueryable<int> connectedStepPersonAliasIds = pastYearStepsQuery.Where( s => s.StepMeasureId == _connectionMeasureId).Select( s => s.PersonAlias.Id );
                                
            // find all people who weren't peer learning when we last ran, but now are.
            var newSteps = latestHistory.Join( prevRunHistory, ah => ah.PersonAliasId, ph => ph.PersonAliasId, ( ah, ph ) => 
                                               new
                                               {
                                                   PersonAliasId = ah.PersonAliasId,
                                                   DateTaken = ah.Date,
                                                   PrevPeerLearning = ph.PeerLearning,
                                                   CurrPeerLearning = ah.PeerLearning
                                               })
                                        .Where( a => connectedStepPersonAliasIds.Contains( a.PersonAliasId ) == false &&
                                                     a.PrevPeerLearning == false && 
                                                     a.CurrPeerLearning == true )
                                        .Select( s => new NewStep
                                        {
                                            PersonAliasId = s.PersonAliasId,
                                            DateTaken = s.DateTaken
                                        })
                                        .ToList( );

            int stepCounter = SaveSteps( _connectionMeasureId, newSteps );

            if ( stepCounter > 0 )
            {
                _resultMessages.Append( string.Format( "Connected Steps {0}, ", stepCounter ) );
            }
        }

        internal class NewStep
        {
            public DateTime DateTaken { get; set; }
            public int PersonAliasId { get; set; }
        }

        private int SaveSteps( int stepId, List<NewStep> newSteps )
        {
            int numSteps = 0;

            // add them as having taken this step
            using ( RockContext updateContext = new RockContext( ) )
            {
                StepTakenService stepTakenUpdateService = new StepTakenService( updateContext );

                foreach ( var newStep in newSteps )
                {
                    var person = new PersonAliasService( updateContext ).GetPerson( newStep.PersonAliasId );
                    if ( person != null && person.PrimaryAliasId.HasValue )
                    {
                        StepTaken step = new StepTaken();
                        step.DateTaken = newStep.DateTaken;
                        step.StepMeasureId = stepId;
                        step.PersonAliasId = newStep.PersonAliasId;

                        var campus = person.GetCampus();
                        if ( campus != null )
                        {
                            step.CampusId = campus.Id;
                        }

                        stepTakenUpdateService.Add( step );

                        numSteps++;
                    }
                }

                updateContext.SaveChanges( );
            }

            return numSteps;
        }
    }
}
