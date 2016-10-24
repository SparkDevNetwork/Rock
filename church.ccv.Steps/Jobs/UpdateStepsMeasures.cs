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
using church.ccv.Actions.Data;
using church.ccv.Actions.Models;
using church.ccv.Datamart.Model;
using church.ccv.Steps.Model;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace church.ccv.Steps
{
    /// <summary>
    /// Sends eRA potential loss email
    /// </summary>
    [TextField("Warning Email Addresses", "Comma-separated list of email addresses to warn when required metrics are not present.", false)]
    [IntegerField("Weekend Attendance Metric Id", "The metric id of the metric that will provide the weekend attendance counts.")]
    [SystemEmailField( "Warning Email Template", "The system email to use when metrics don't exist.", false )]
    [GroupTypeField("Neighborhood Area Group Type")]

    [IntegerField("Baptism Measure Id", "The Measure Id for baptisms.", category:"Measures", order: 1)]
    [IntegerField( "Membership Measure Id", "The Measure Id for membership.", category: "Measures", order: 2 )]
    [IntegerField( "Attending Measure Id", "The Measure Id for attending.", category: "Measures", order: 3 )]
    [IntegerField( "Connection Measure Id", "The Measure Id for connection.", category: "Measures", order: 4 )]
    [IntegerField( "Giving Measure Id", "The Measure Id for giving.", category: "Measures", order: 5 )]
    [IntegerField( "Serving Measure Id", "The Measure Id for serving.", category: "Measures", order: 6 )]
    [IntegerField( "Sharing Measure Id", "The Measure Id for sharing.", category: "Measures", order: 7 )]
    [IntegerField( "Coaching Measure Id", "The Measure Id for coaching.", category: "Measures", order: 8 )]
    
    [DisallowConcurrentExecution]
    public class UpdateStepMeasures : IJob
    {
        const int _studentLowerGrade = 7;
        const int _studentUpperGrade = 12;
        
        List<int> _campusIds = new List<int>();
        int _attendanceMetricId = -1;
        string _warningEmailAddresses = string.Empty;
        Guid _warningEmailTemplate = Guid.Empty;

        int _baptismMeasureId = -1;
        int _coachingMeasureId = -1;
        int _servingMeasureId = -1;
        int _connectionMeasureId = -1;
        int _membershipMeasureId = -1;
        int _givingMeasureId = -1;
        int _attendingMeasureId = -1;
        int _sharingMeasureId = -1;

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public UpdateStepMeasures()
        {
            // determine the last Sunday that was run
        }
        
        public void Execute( IJobExecutionContext context )
        {
            // set a working date here so that we can change it easily for debugging.
            DateTime workingDate = RockDateTime.Now;

            context.Result = "";

            JobDataMap dataMap = context.JobDetail.JobDataMap;

            var rockContext = new RockContext();

            // load list of active campuses
            _campusIds = new CampusService( rockContext ).Queryable().Where( c => c.IsActive == true ).Select( c => c.Id ).ToList();
            _attendanceMetricId = dataMap.GetIntFromString( "WeekendAttendanceMetricId" );
            _warningEmailAddresses = dataMap.GetString( "WarningEmailAddresses" );
            _warningEmailTemplate = dataMap.GetString( "WarningEmailTemplate" ).AsGuid();

            _baptismMeasureId = dataMap.GetIntFromString( "BaptismMeasureId" );
            _coachingMeasureId = dataMap.GetIntFromString( "CoachingMeasureId" );
            _servingMeasureId = dataMap.GetIntFromString( "ServingMeasureId" );
            _connectionMeasureId = dataMap.GetIntFromString( "ConnectionMeasureId" );
            _membershipMeasureId = dataMap.GetIntFromString( "MembershipMeasureId" );
            _givingMeasureId = dataMap.GetIntFromString( "GivingMeasureId" );
            _attendingMeasureId = dataMap.GetIntFromString( "AttendingMeasureId" );
            _sharingMeasureId = dataMap.GetIntFromString( "SharingMeasureId" );
                        
            // get the last sunday date processed
            StepMeasureValueService stepMeasureValueService = new StepMeasureValueService( rockContext );

            var lastProcessedSunday = stepMeasureValueService.Queryable().Select( m => m.SundayDate ).Max();

            // if first run set it to previous sunday
            if ( !lastProcessedSunday.HasValue )
            {
                lastProcessedSunday = workingDate.SundayDate().AddDays( -14 );
            }

            bool sundayProcessed = false;

            // process all missing sundays
            if ( lastProcessedSunday != workingDate.SundayDate().AddDays( -7 ) )
            {
                lastProcessedSunday = lastProcessedSunday.Value.AddDays( 7 );

                while ( lastProcessedSunday <= workingDate )
                {
                    string processingMessages = string.Empty;

                    sundayProcessed = true;
                    
                    var wasSuccessful = ProcessWeek( lastProcessedSunday.Value, out processingMessages );

                    if ( !wasSuccessful )
                    {
                        context.Result += " " +  processingMessages;
                        break;
                    }

                    if ( string.IsNullOrWhiteSpace( context.Result.ToString() ) )
                    {
                        context.Result = "Processed: " + lastProcessedSunday.Value.ToShortDateString();
                    } else
                    {
                        context.Result += ", " + lastProcessedSunday.Value.ToShortDateString();
                    }

                    lastProcessedSunday = lastProcessedSunday.Value.AddDays( 7 );
                }

                if ( string.IsNullOrWhiteSpace( context.Result.ToString() ) )
                {
                    context.Result = "No Sundays required processing";
                }
            }

            if (sundayProcessed == false )
            {
                context.Result = "No Sundays required processing. We are up to date.";
            }
        }

        private bool ProcessWeek(DateTime sundayDate, out string message)
        {
            message = string.Empty;

            bool wasSuccessful = true;

            using ( RockContext rockContext = new RockContext( ) )
            {
                // check if the metrics we need exist, if not send warning email and stop
                List<MetricValue> metricValues = new MetricValueService( rockContext ).Queryable().Where( m => m.MetricValueDateTime == sundayDate && m.MetricId == _attendanceMetricId ).ToList();

                int entityTypeCampusId = EntityTypeCache.GetId<Campus>() ?? 0;

                // ensure that all campuses have metrics
                var metricCampusIds = metricValues.SelectMany(m => m.MetricValuePartitions ).Where(a => a.MetricPartition.EntityTypeId == entityTypeCampusId && a.EntityId.HasValue).Select(a => a.EntityId.Value).ToList();
                if ( !ContainsAllItems( _campusIds, metricCampusIds ) )
                {
                    // send warning email
                    if ( !string.IsNullOrWhiteSpace( _warningEmailAddresses ) && _warningEmailTemplate != Guid.Empty )
                    {
                        List<RecipientData> recipients = new List<RecipientData>();

                        List<string> emails = _warningEmailAddresses.Split( ',' ).ToList();

                        var mergeObjects = new Dictionary<string, object>();
                        mergeObjects.Add( "SundayDate", sundayDate );

                        foreach ( var email in emails )
                        {
                            recipients.Add( new RecipientData( email, mergeObjects ) );
                        }

                        Email.Send( _warningEmailTemplate, recipients );

                        message = string.Format( "Could not process {0} due to missing metric values.", sundayDate.ToShortDateString( ) );
                    }
                    return false;
                }

                // get the pastor responsible for each region
                DatamartNeighborhoodService datamartNeighborhoodService = new DatamartNeighborhoodService(rockContext);
                var regionList = datamartNeighborhoodService.Queryable()
                                            .Where( n => n.NeighborhoodPastorId != null )
                                            .Select( n => new Region
                                            {
                                                RegionId = n.NeighborhoodId.Value,
                                                PastorPersonId = n.NeighborhoodPastorId,
                                            } )
                                            .ToList();

                // fill in missing pieces from the datamart
                foreach ( var region in regionList )
                {
                    region.PastorPersonAliasId = new PersonAliasService( rockContext ).Queryable( ).Where( a => a.PersonId == region.PastorPersonId ).Select( a => a.Id ).FirstOrDefault( );
                    region.CampusId = new GroupService( rockContext ).Queryable( ).Where( g => g.Id == region.RegionId ).Select( g => g.CampusId ).FirstOrDefault( );
                }

                // perform our actual metrics analysis
                using ( ActionsContext actionsContext = new ActionsContext( ) )
                {
                    ActionsService<ActionsHistory_Adult> adultActionsService = new ActionsService<ActionsHistory_Adult>( actionsContext );
                    ActionsService<ActionsHistory_Student> studentActionsService = new ActionsService<ActionsHistory_Student>( actionsContext );
                    
                    // Given the sunday date, take the most recent available date that's on or before sundayDate and not older than a week.
                    // This ensures we use the most recent data available, but don't keep using it week after week.
                    DateTime? latestDate = null;
                    try
                    {
                        // grab the most recent history date available
                        DateTime newestHistoryDate = adultActionsService.Queryable( ).Where( ah => ah.Date <= sundayDate ).Max( ah => ah.Date );
                        if( (sundayDate - newestHistoryDate).TotalDays <= 6 )
                        {
                            latestDate = newestHistoryDate;
                        }
                    }
                    catch
                    {
                        // don't worry about catching, we'll just fail below
                    }

                    if( latestDate.HasValue )
                    {
                        // begin by getting queries that contain only the rows of action history we care about, organized by campus and region, adult and student
                    
                        // Adults organized by campus
                        var actionsHistory_Campus_Adult = adultActionsService.Queryable( ).Where( ah => ah.Date == latestDate )
                                                                                          .GroupBy( ah => ah.CampusId );

                        // Adults organized by region
                        var actionsHistory_Region_Adult = adultActionsService.Queryable( ).Where( ah => ah.Date == latestDate );
                
                        // get the number of active adults per campus
                        var campusTotals_Adult = actionsHistory_Campus_Adult.Select( wg => new CampusAdultCount
                                                                             {
                                                                                 CampusId = wg.Key,
                                                                                 AdultCount = wg.Sum( w => w.TotalPeople )
                                                                             } )
                                                                            .ToList( );


                        // Students organized by campus
                        var actionsHistory_Campus_Student = studentActionsService.Queryable( ).Where( ah => ah.Date == latestDate )
                                                                                              .GroupBy( ah => ah.CampusId );
                
                        // get the number of actlive students per campus
                        var campusTotals_Student = actionsHistory_Campus_Student.Select( wg => new CampusStudentCount
                                                                                 {
                                                                                     CampusId = wg.Key,
                                                                                     StudentCount = wg.Sum( w => w.TotalPeople )
                                                                                 } )
                                                                                .ToList( );

                        // store values that will be passed thru to SaveMetrics in a wrapper class.
                        SaveMetrics_Dependencies metricDependencies = new SaveMetrics_Dependencies( )
                        {
                            SundayDate = sundayDate,
                            MetricValues = metricValues,
                            CampusTotals_Adult = campusTotals_Adult,
                            CampusTotals_Student = campusTotals_Student
                        };
                    
                        // run the analytics for each action
                        ProcessBaptisms( metricDependencies, actionsHistory_Campus_Adult, actionsHistory_Region_Adult, actionsHistory_Campus_Student, regionList );
                        ProcessMembership( metricDependencies, actionsHistory_Campus_Adult, actionsHistory_Region_Adult, actionsHistory_Campus_Student, regionList );
                        ProcessAttending( metricDependencies, actionsHistory_Campus_Adult, actionsHistory_Region_Adult, actionsHistory_Campus_Student, regionList );
                        ProcessGiving( metricDependencies, actionsHistory_Campus_Adult, actionsHistory_Region_Adult, actionsHistory_Campus_Student, regionList );
                        ProcessCoaching( metricDependencies, actionsHistory_Campus_Adult, actionsHistory_Region_Adult, actionsHistory_Campus_Student, regionList );
                        ProcessConnection( metricDependencies, actionsHistory_Campus_Adult, actionsHistory_Region_Adult, actionsHistory_Campus_Student, regionList );
                        ProcessServing( metricDependencies, actionsHistory_Campus_Adult, actionsHistory_Region_Adult, actionsHistory_Campus_Student, regionList );
                        ProcessSharing( metricDependencies, actionsHistory_Campus_Adult, actionsHistory_Region_Adult, actionsHistory_Campus_Student, regionList );
                    }
                    else
                    {
                        wasSuccessful = false;
                        message = string.Format( "Could not process {0} due to missing Actions History.", sundayDate.ToShortDateString( ) );
                    }
                }
            }

            return wasSuccessful;
        }

        // Simple wrapper class to store variables only SaveMetrics cares about.
        // saves us from passing around like 9 variables on the stack
        internal class SaveMetrics_Dependencies
        {
            public DateTime SundayDate;
            public List<MetricValue> MetricValues;
            public List<CampusAdultCount> CampusTotals_Adult;
            public List<CampusStudentCount> CampusTotals_Student;
        }
        
        // attribute based measures
        private void ProcessBaptisms( SaveMetrics_Dependencies metricDependencies, 
                                      IQueryable<IGrouping<int, ActionsHistory_Adult>> actionsHistory_Campus_Adult, 
                                      IQueryable<ActionsHistory_Adult> actionsHistory_Region_Adult,
                                      IQueryable<IGrouping<int, ActionsHistory_Student>> actionsHistory_Campus_Student, 
                                      List<Region> regionList )
        {
            // sum the number of adults baptised per-campus
            var measureCountsByCampusAdults = actionsHistory_Campus_Adult.Select( ah => new CampusMeasure
                                                                            {
                                                                                CampusId = ah.Key,
                                                                                MeasureValue = ah.Sum( c => c.Baptised )
                                                                            }).ToList( );

            // Regions require two steps.
            //1. take the region ID, total people, and total people baptised
            var adultRegionMeasureCounts = actionsHistory_Region_Adult.Select( ah => new
                                                                            {
                                                                                RegionId = ah.RegionId,
                                                                                AdultCount = ah.TotalPeople,
                                                                                AdultMeasureCount = ah.Baptised
                                                                            }).ToList( );
            
            // 2. now resolve the PastorPersonAliasId from the RegionId, and store the results in the measure object SaveMetrics wants.
            var measureCountsByRegionAdult = adultRegionMeasureCounts.Select( am => new RegionMeasure
            {
                PastorPersonAliasId = regionList.Where( a => a.RegionId == am.RegionId ).SingleOrDefault( ).PastorPersonAliasId,
                AdultCount = am.AdultCount,
                AdultMeasureCount = am.AdultMeasureCount
            }).ToList( );



            // sum the number of students baptised per-campus
            var measureCountsByCampusStudents = actionsHistory_Campus_Student.Select( ah => new CampusMeasure
                                                                                    {
                                                                                        CampusId = ah.Key,
                                                                                        MeasureValue = ah.Sum( c => c.Baptised )
                                                                                    }).ToList( );
            // NOTE: Even tho students are in Regions, they aren't evaluated by region. They're evaluated according to NG groups and whoever is over that group.
            // Someday we'll need to add support for that.
                
                
            // TOTAL ADULTS + STUDENTS (Used for Weekend Attendance)
            // to get this, join the tables on the campus ID, and then create a CampusMeasure object with the sum of the campus totals for adult/student
            var measureCountsByCampusAll = measureCountsByCampusAdults
                                            .Join( measureCountsByCampusStudents,
                                                    adult => adult.CampusId, student => student.CampusId, ( a, s ) =>
                                                    new CampusMeasure
                                                    {
                                                        CampusId = a.CampusId,
                                                        MeasureValue = a.MeasureValue + s.MeasureValue
                                                    } )
                                            .ToList();

            SaveMetrics( _baptismMeasureId, metricDependencies, measureCountsByCampusAll, measureCountsByCampusAdults, measureCountsByCampusStudents, measureCountsByRegionAdult );
        }

        private void ProcessMembership( SaveMetrics_Dependencies metricDependencies, 
                                        IQueryable<IGrouping<int, ActionsHistory_Adult>> actionsHistory_Campus_Adult, 
                                        IQueryable<ActionsHistory_Adult> actionsHistory_Region_Adult,
                                        IQueryable<IGrouping<int, ActionsHistory_Student>> actionsHistory_Campus_Student, 
                                        List<Region> regionList )
        {
            // sum the number of adults performing the action per-campus
            var measureCountsByCampusAdults = actionsHistory_Campus_Adult.Select( ah => new CampusMeasure
                                                                            {
                                                                                CampusId = ah.Key,
                                                                                MeasureValue = ah.Sum( c => c.Member )
                                                                            }).ToList( );

            // Regions require two steps.
            //1. take the region ID, total people, and total people performing the action
            var adultRegionMeasureCounts = actionsHistory_Region_Adult.Select( ah => new
                                                                            {
                                                                                RegionId = ah.RegionId,
                                                                                AdultCount = ah.TotalPeople,
                                                                                AdultMeasureCount = ah.Member
                                                                            }).ToList( );
            
            // 2. now resolve the PastorPersonAliasId from the RegionId, and store the results in the measure object SaveMetrics wants.
            var measureCountsByRegionAdult = adultRegionMeasureCounts.Select( am => new RegionMeasure
            {
                PastorPersonAliasId = regionList.Where( a => a.RegionId == am.RegionId ).SingleOrDefault( ).PastorPersonAliasId,
                AdultCount = am.AdultCount,
                AdultMeasureCount = am.AdultMeasureCount
            }).ToList( );



            // sum the number of students performing the action per-campus
            var measureCountsByCampusStudents = actionsHistory_Campus_Student.Select( ah => new CampusMeasure
                                                                                    {
                                                                                        CampusId = ah.Key,
                                                                                        MeasureValue = ah.Sum( c => c.Member )
                                                                                    }).ToList( );
            // NOTE: Even tho students are in Regions, they aren't evaluated by region. They're evaluated according to NG groups and whoever is over that group.
            // Someday we'll need to add support for that.
                
                
            // TOTAL ADULTS + STUDENTS (Used for Weekend Attendance)
            // to get this, join the tables on the campus ID, and then create a CampusMeasure object with the sum of the campus totals for adult/student
            var measureCountsByCampusAll = measureCountsByCampusAdults
                                            .Join( measureCountsByCampusStudents,
                                                    adult => adult.CampusId, student => student.CampusId, ( a, s ) =>
                                                    new CampusMeasure
                                                    {
                                                        CampusId = a.CampusId,
                                                        MeasureValue = a.MeasureValue + s.MeasureValue
                                                    } )
                                            .ToList();

            SaveMetrics( _membershipMeasureId, metricDependencies, measureCountsByCampusAll, measureCountsByCampusAdults, measureCountsByCampusStudents, measureCountsByRegionAdult );
        }
        
        private void ProcessAttending( SaveMetrics_Dependencies metricDependencies, 
                                       IQueryable<IGrouping<int, ActionsHistory_Adult>> actionsHistory_Campus_Adult, 
                                       IQueryable<ActionsHistory_Adult> actionsHistory_Region_Adult,
                                       IQueryable<IGrouping<int, ActionsHistory_Student>> actionsHistory_Campus_Student, 
                                       List<Region> regionList )
        {
            // sum the number of adults performing the action per-campus
            var measureCountsByCampusAdults = actionsHistory_Campus_Adult.Select( ah => new CampusMeasure
                                                                            {
                                                                                CampusId = ah.Key,
                                                                                MeasureValue = ah.Sum( c => c.ERA )
                                                                            }).ToList( );

            // Regions require two steps.
            //1. take the region ID, total people, and total people performing the action
            var adultRegionMeasureCounts = actionsHistory_Region_Adult.Select( ah => new
                                                                            {
                                                                                RegionId = ah.RegionId,
                                                                                AdultCount = ah.TotalPeople,
                                                                                AdultMeasureCount = ah.ERA
                                                                            }).ToList( );
            
            // 2. now resolve the PastorPersonAliasId from the RegionId, and store the results in the measure object SaveMetrics wants.
            var measureCountsByRegionAdult = adultRegionMeasureCounts.Select( am => new RegionMeasure
            {
                PastorPersonAliasId = regionList.Where( a => a.RegionId == am.RegionId ).SingleOrDefault( ).PastorPersonAliasId,
                AdultCount = am.AdultCount,
                AdultMeasureCount = am.AdultMeasureCount
            }).ToList( );



            // sum the number of students performing the action per-campus
            var measureCountsByCampusStudents = actionsHistory_Campus_Student.Select( ah => new CampusMeasure
                                                                                    {
                                                                                        CampusId = ah.Key,
                                                                                        MeasureValue = ah.Sum( c => c.ERA )
                                                                                    }).ToList( );
            // NOTE: Even tho students are in Regions, they aren't evaluated by region. They're evaluated according to NG groups and whoever is over that group.
            // Someday we'll need to add support for that.
                
                
            // TOTAL ADULTS + STUDENTS (Used for Weekend Attendance)
            // to get this, join the tables on the campus ID, and then create a CampusMeasure object with the sum of the campus totals for adult/student
            var measureCountsByCampusAll = measureCountsByCampusAdults
                                            .Join( measureCountsByCampusStudents,
                                                    adult => adult.CampusId, student => student.CampusId, ( a, s ) =>
                                                    new CampusMeasure
                                                    {
                                                        CampusId = a.CampusId,
                                                        MeasureValue = a.MeasureValue + s.MeasureValue
                                                    } )
                                            .ToList();

            SaveMetrics( _attendingMeasureId, metricDependencies, measureCountsByCampusAll, measureCountsByCampusAdults, measureCountsByCampusStudents, measureCountsByRegionAdult );
        }
        
        private void ProcessGiving( SaveMetrics_Dependencies metricDependencies, 
                                    IQueryable<IGrouping<int, ActionsHistory_Adult>> actionsHistory_Campus_Adult, 
                                    IQueryable<ActionsHistory_Adult> actionsHistory_Region_Adult,
                                    IQueryable<IGrouping<int, ActionsHistory_Student>> actionsHistory_Campus_Student, 
                                    List<Region> regionList )
        {
            // sum the number of adults performing the action per-campus
            var measureCountsByCampusAdults = actionsHistory_Campus_Adult.Select( ah => new CampusMeasure
                                                                            {
                                                                                CampusId = ah.Key,
                                                                                MeasureValue = ah.Sum( c => c.Give )
                                                                            }).ToList( );

            // Regions require two steps.
            //1. take the region ID, total people, and total people performing the action
            var adultRegionMeasureCounts = actionsHistory_Region_Adult.Select( ah => new
                                                                            {
                                                                                RegionId = ah.RegionId,
                                                                                AdultCount = ah.TotalPeople,
                                                                                AdultMeasureCount = ah.Give
                                                                            }).ToList( );
            
            // 2. now resolve the PastorPersonAliasId from the RegionId, and store the results in the measure object SaveMetrics wants.
            var measureCountsByRegionAdult = adultRegionMeasureCounts.Select( am => new RegionMeasure
            {
                PastorPersonAliasId = regionList.Where( a => a.RegionId == am.RegionId ).SingleOrDefault( ).PastorPersonAliasId,
                AdultCount = am.AdultCount,
                AdultMeasureCount = am.AdultMeasureCount
            }).ToList( );



            // sum the number of students performing the action per-campus
            var measureCountsByCampusStudents = actionsHistory_Campus_Student.Select( ah => new CampusMeasure
                                                                                    {
                                                                                        CampusId = ah.Key,
                                                                                        MeasureValue = ah.Sum( c => c.Give )
                                                                                    }).ToList( );
            // NOTE: Even tho students are in Regions, they aren't evaluated by region. They're evaluated according to NG groups and whoever is over that group.
            // Someday we'll need to add support for that.
                
                
            // TOTAL ADULTS + STUDENTS (Used for Weekend Attendance)
            // to get this, join the tables on the campus ID, and then create a CampusMeasure object with the sum of the campus totals for adult/student
            var measureCountsByCampusAll = measureCountsByCampusAdults
                                            .Join( measureCountsByCampusStudents,
                                                    adult => adult.CampusId, student => student.CampusId, ( a, s ) =>
                                                    new CampusMeasure
                                                    {
                                                        CampusId = a.CampusId,
                                                        MeasureValue = a.MeasureValue + s.MeasureValue
                                                    } )
                                            .ToList();

            SaveMetrics( _givingMeasureId, metricDependencies, measureCountsByCampusAll, measureCountsByCampusAdults, measureCountsByCampusStudents, measureCountsByRegionAdult );
        }
        
        private void ProcessCoaching( SaveMetrics_Dependencies metricDependencies, 
                                      IQueryable<IGrouping<int, ActionsHistory_Adult>> actionsHistory_Campus_Adult, 
                                      IQueryable<ActionsHistory_Adult> actionsHistory_Region_Adult,
                                      IQueryable<IGrouping<int, ActionsHistory_Student>> actionsHistory_Campus_Student, 
                                      List<Region> regionList )
        {
            // sum the number of adults performing the action per-campus
            var measureCountsByCampusAdults = actionsHistory_Campus_Adult.Select( ah => new CampusMeasure
                                                                            {
                                                                                CampusId = ah.Key,
                                                                                MeasureValue = ah.Sum( c => c.Teaching )
                                                                            }).ToList( );

            // Regions require two steps.
            //1. take the region ID, total people, and total people performing the action
            var adultRegionMeasureCounts = actionsHistory_Region_Adult.Select( ah => new
                                                                            {
                                                                                RegionId = ah.RegionId,
                                                                                AdultCount = ah.TotalPeople,
                                                                                AdultMeasureCount = ah.Teaching
                                                                            }).ToList( );
            
            // 2. now resolve the PastorPersonAliasId from the RegionId, and store the results in the measure object SaveMetrics wants.
            var measureCountsByRegionAdult = adultRegionMeasureCounts.Select( am => new RegionMeasure
            {
                PastorPersonAliasId = regionList.Where( a => a.RegionId == am.RegionId ).SingleOrDefault( ).PastorPersonAliasId,
                AdultCount = am.AdultCount,
                AdultMeasureCount = am.AdultMeasureCount
            }).ToList( );



            // sum the number of students performing the action per-campus
            var measureCountsByCampusStudents = actionsHistory_Campus_Student.Select( ah => new CampusMeasure
                                                                                    {
                                                                                        CampusId = ah.Key,
                                                                                        MeasureValue = ah.Sum( c => c.Teaching )
                                                                                    }).ToList( );
            // NOTE: Even tho students are in Regions, they aren't evaluated by region. They're evaluated according to NG groups and whoever is over that group.
            // Someday we'll need to add support for that.
                
                
            // TOTAL ADULTS + STUDENTS (Used for Weekend Attendance)
            // to get this, join the tables on the campus ID, and then create a CampusMeasure object with the sum of the campus totals for adult/student
            var measureCountsByCampusAll = measureCountsByCampusAdults
                                            .Join( measureCountsByCampusStudents,
                                                    adult => adult.CampusId, student => student.CampusId, ( a, s ) =>
                                                    new CampusMeasure
                                                    {
                                                        CampusId = a.CampusId,
                                                        MeasureValue = a.MeasureValue + s.MeasureValue
                                                    } )
                                            .ToList();

            SaveMetrics( _coachingMeasureId, metricDependencies, measureCountsByCampusAll, measureCountsByCampusAdults, measureCountsByCampusStudents, measureCountsByRegionAdult );
        }

        private void ProcessConnection( SaveMetrics_Dependencies metricDependencies, 
                                        IQueryable<IGrouping<int, ActionsHistory_Adult>> actionsHistory_Campus_Adult, 
                                        IQueryable<ActionsHistory_Adult> actionsHistory_Region_Adult,
                                        IQueryable<IGrouping<int, ActionsHistory_Student>> actionsHistory_Campus_Student, 
                                        List<Region> regionList )
        {
            // sum the number of adults performing the action per-campus
            var measureCountsByCampusAdults = actionsHistory_Campus_Adult.Select( ah => new CampusMeasure
                                                                            {
                                                                                CampusId = ah.Key,
                                                                                MeasureValue = ah.Sum( c => c.PeerLearning )
                                                                            }).ToList( );

            // Regions require two steps.
            //1. take the region ID, total people, and total people performing the action
            var adultRegionMeasureCounts = actionsHistory_Region_Adult.Select( ah => new
                                                                            {
                                                                                RegionId = ah.RegionId,
                                                                                AdultCount = ah.TotalPeople,
                                                                                AdultMeasureCount = ah.PeerLearning
                                                                            }).ToList( );
            
            // 2. now resolve the PastorPersonAliasId from the RegionId, and store the results in the measure object SaveMetrics wants.
            var measureCountsByRegionAdult = adultRegionMeasureCounts.Select( am => new RegionMeasure
            {
                PastorPersonAliasId = regionList.Where( a => a.RegionId == am.RegionId ).SingleOrDefault( ).PastorPersonAliasId,
                AdultCount = am.AdultCount,
                AdultMeasureCount = am.AdultMeasureCount
            }).ToList( );



            // sum the number of students performing the action per-campus
            var measureCountsByCampusStudents = actionsHistory_Campus_Student.Select( ah => new CampusMeasure
                                                                                    {
                                                                                        CampusId = ah.Key,
                                                                                        MeasureValue = ah.Sum( c => c.PeerLearning )
                                                                                    }).ToList( );
            // NOTE: Even tho students are in Regions, they aren't evaluated by region. They're evaluated according to NG groups and whoever is over that group.
            // Someday we'll need to add support for that.
                
                
            // TOTAL ADULTS + STUDENTS (Used for Weekend Attendance)
            // to get this, join the tables on the campus ID, and then create a CampusMeasure object with the sum of the campus totals for adult/student
            var measureCountsByCampusAll = measureCountsByCampusAdults
                                            .Join( measureCountsByCampusStudents,
                                                    adult => adult.CampusId, student => student.CampusId, ( a, s ) =>
                                                    new CampusMeasure
                                                    {
                                                        CampusId = a.CampusId,
                                                        MeasureValue = a.MeasureValue + s.MeasureValue
                                                    } )
                                            .ToList();

            SaveMetrics( _connectionMeasureId, metricDependencies, measureCountsByCampusAll, measureCountsByCampusAdults, measureCountsByCampusStudents, measureCountsByRegionAdult );
        }

        private void ProcessServing( SaveMetrics_Dependencies metricDependencies, 
                                     IQueryable<IGrouping<int, ActionsHistory_Adult>> actionsHistory_Campus_Adult, 
                                     IQueryable<ActionsHistory_Adult> actionsHistory_Region_Adult,
                                     IQueryable<IGrouping<int, ActionsHistory_Student>> actionsHistory_Campus_Student, 
                                     List<Region> regionList )
        {
            // sum the number of adults performing the action per-campus
            var measureCountsByCampusAdults = actionsHistory_Campus_Adult.Select( ah => new CampusMeasure
                                                                            {
                                                                                CampusId = ah.Key,
                                                                                MeasureValue = ah.Sum( c => c.Serving )
                                                                            }).ToList( );

            // Regions require two steps.
            //1. take the region ID, total people, and total people performing the action
            var adultRegionMeasureCounts = actionsHistory_Region_Adult.Select( ah => new
                                                                            {
                                                                                RegionId = ah.RegionId,
                                                                                AdultCount = ah.TotalPeople,
                                                                                AdultMeasureCount = ah.Serving
                                                                            }).ToList( );
            
            // 2. now resolve the PastorPersonAliasId from the RegionId, and store the results in the measure object SaveMetrics wants.
            var measureCountsByRegionAdult = adultRegionMeasureCounts.Select( am => new RegionMeasure
            {
                PastorPersonAliasId = regionList.Where( a => a.RegionId == am.RegionId ).SingleOrDefault( ).PastorPersonAliasId,
                AdultCount = am.AdultCount,
                AdultMeasureCount = am.AdultMeasureCount
            }).ToList( );



            // sum the number of students performing the action per-campus
            var measureCountsByCampusStudents = actionsHistory_Campus_Student.Select( ah => new CampusMeasure
                                                                                    {
                                                                                        CampusId = ah.Key,
                                                                                        MeasureValue = ah.Sum( c => c.Serving )
                                                                                    }).ToList( );
            // NOTE: Even tho students are in Regions, they aren't evaluated by region. They're evaluated according to NG groups and whoever is over that group.
            // Someday we'll need to add support for that.
                
                
            // TOTAL ADULTS + STUDENTS (Used for Weekend Attendance)
            // to get this, join the tables on the campus ID, and then create a CampusMeasure object with the sum of the campus totals for adult/student
            var measureCountsByCampusAll = measureCountsByCampusAdults
                                            .Join( measureCountsByCampusStudents,
                                                    adult => adult.CampusId, student => student.CampusId, ( a, s ) =>
                                                    new CampusMeasure
                                                    {
                                                        CampusId = a.CampusId,
                                                        MeasureValue = a.MeasureValue + s.MeasureValue
                                                    } )
                                            .ToList();

            SaveMetrics( _servingMeasureId, metricDependencies, measureCountsByCampusAll, measureCountsByCampusAdults, measureCountsByCampusStudents, measureCountsByRegionAdult );
        }

        private void ProcessSharing( SaveMetrics_Dependencies metricDependencies, 
                                     IQueryable<IGrouping<int, ActionsHistory_Adult>> actionsHistory_Campus_Adult, 
                                     IQueryable<ActionsHistory_Adult> actionsHistory_Region_Adult,
                                     IQueryable<IGrouping<int, ActionsHistory_Student>> actionsHistory_Campus_Student, 
                                     List<Region> regionList )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                // not implemented sending in values so graphs will work as tbd
                
                var measureCountsByCampusAdults = new List<CampusMeasure>( );
                var measureCountsByRegionAdult = new List<RegionMeasure>( );

                var measureCountsByCampusStudents = new List<CampusMeasure>( );
                
                var measureCountsByCampusAll = new List<CampusMeasure>( );

                SaveMetrics( _sharingMeasureId, metricDependencies, measureCountsByCampusAll, measureCountsByCampusAdults, measureCountsByCampusStudents, measureCountsByRegionAdult );
            }
        }

        #region Utilities

        private void SaveMetrics( int measureId, SaveMetrics_Dependencies metricDependencies, List<CampusMeasure> campusAllMeasures, List<CampusMeasure> campusAdultMeasures, List<CampusMeasure> campusStudentMeasures, List<RegionMeasure> regionAdultMeasures )
        {
            int entityTypeCampusId = EntityTypeCache.GetId<Campus>() ?? 0;

            using ( RockContext rockContext = new RockContext() )
            {
                StepMeasureValueService stepMeasureValueService = new StepMeasureValueService( rockContext );

                foreach ( int campusId in _campusIds )
                {
                    // get counts 
                    int weekendAttendance = metricDependencies.MetricValues.Where( m => m.MetricValuePartitions.Any( x => x.MetricPartition.EntityTypeId == entityTypeCampusId && x.EntityId == campusId )).Select( m => m.YValue ).FirstOrDefault() != null ? 
                        Decimal.ToInt16( metricDependencies.MetricValues.Where( m => m.MetricValuePartitions.Any( x => x.MetricPartition.EntityTypeId == entityTypeCampusId && x.EntityId == campusId )).Select( m => m.YValue ).FirstOrDefault().Value ) : 0;

                    int activeAdults = metricDependencies.CampusTotals_Adult.Where( c => c.CampusId == campusId ).Select( c => c.AdultCount ).Count() != 0 ? metricDependencies.CampusTotals_Adult.Where( c => c.CampusId == campusId ).Select( c => c.AdultCount ).FirstOrDefault() : 0;
                    int activeStudents = metricDependencies.CampusTotals_Student.Where( c => c.CampusId == campusId ).Select( c => c.StudentCount ).Count() != 0 ? metricDependencies.CampusTotals_Student.Where( c => c.CampusId == campusId ).Select( c => c.StudentCount ).FirstOrDefault() : 0;

                    int measureCountAll = campusAllMeasures.Where( b => b.CampusId == campusId ).Select( b => b.MeasureValue ).Count() != 0 ? campusAllMeasures.Where( b => b.CampusId == campusId ).Select( b => b.MeasureValue ).FirstOrDefault() : 0;
                    int measureCountAdults = campusAdultMeasures.Where( b => b.CampusId == campusId ).Select( b => b.MeasureValue ).Count() != 0 ? campusAdultMeasures.Where( b => b.CampusId == campusId ).Select( b => b.MeasureValue ).FirstOrDefault() : 0;
                    int measureCountStudents = campusStudentMeasures.Where( b => b.CampusId == campusId ).Select( b => b.MeasureValue ).Count() != 0 ? campusStudentMeasures.Where( b => b.CampusId == campusId ).Select( b => b.MeasureValue ).FirstOrDefault() : 0;

                    // save metric all count / weekend attendance
                    StepMeasureValue measureValueAll = new StepMeasureValue();
                    measureValueAll.StepMeasureId = measureId;
                    measureValueAll.SundayDate = metricDependencies.SundayDate;
                    measureValueAll.Value = measureCountAll;
                    measureValueAll.CampusId = campusId;
                    measureValueAll.WeekendAttendance = weekendAttendance;

                    stepMeasureValueService.Add( measureValueAll );

                    // save metric adult count / campus adult count
                    StepMeasureValue measureValueAdults = new StepMeasureValue();
                    measureValueAdults.StepMeasureId = measureId;
                    measureValueAdults.SundayDate = metricDependencies.SundayDate;
                    measureValueAdults.Value = measureCountAdults;
                    measureValueAdults.CampusId = campusId;
                    measureValueAdults.ActiveAdults = activeAdults;

                    stepMeasureValueService.Add( measureValueAdults );

                    // save metric student count / campus student count
                    StepMeasureValue measureValueStudents = new StepMeasureValue();
                    measureValueStudents.StepMeasureId = measureId;
                    measureValueStudents.SundayDate = metricDependencies.SundayDate;
                    measureValueStudents.Value = measureCountStudents;
                    measureValueStudents.CampusId = campusId;
                    measureValueStudents.ActiveStudents = activeStudents;

                    stepMeasureValueService.Add( measureValueStudents );
                }

                rockContext.SaveChanges();

                var pastorMeasures = regionAdultMeasures.GroupBy( n => n.PastorPersonAliasId )
                                        .Select( p => new
                                        {
                                            PastorPersonAliasId = p.Key,
                                            AdultCount = p.Sum( k => k.AdultCount ),
                                            AdultMeasureValue = p.Sum( k => k.AdultMeasureCount ),
                                        } )
                                        .ToList();

                foreach ( var pastor in pastorMeasures )
                {
                    // store adults
                    StepMeasureValue measureValueAdults = new StepMeasureValue();
                    measureValueAdults.StepMeasureId = measureId;
                    measureValueAdults.SundayDate = metricDependencies.SundayDate;
                    measureValueAdults.Value = pastor.AdultMeasureValue.HasValue ? pastor.AdultMeasureValue.Value : 0;
                    measureValueAdults.PastorPersonAliasId = pastor.PastorPersonAliasId;
                    measureValueAdults.ActiveAdults = pastor.AdultCount;

                    stepMeasureValueService.Add( measureValueAdults );
                }

                // note: Student's don't yet have a measureable leader

                rockContext.SaveChanges();
            }
        }



        #region Uitlity Methods
        private bool ContainsAllItems( List<int> a, List<int> b )
        {
            if ((b.Except( a ).Any()) || (a.Except( b ).Any())) // if after removing items from each other's list there are remaining items then not all metics exist
            {
                return false;
            }

            return true;
        }
        #endregion

        #region Utility Classes
        
        public class CampusMeasure
        {
            /// <summary>
            /// Gets or sets the campus identifier.
            /// </summary>
            /// <value>
            /// The campus identifier.
            /// </value>
            public int CampusId { get; set; }
            /// <summary>
            /// Gets or sets the measure value.
            /// </summary>
            /// <value>
            /// The measure value.
            /// </value>
            public int MeasureValue { get; set; }
        }
        
        public class RegionMeasure
        {
            /// <summary>
            /// Gets or sets the pastor person alias identifier.
            /// </summary>
            /// <value>
            /// The pastor person alias identifier.
            /// </value>
            public int? PastorPersonAliasId { get; set; }

            /// <summary>
            /// Gets or sets the adult count.
            /// </summary>
            /// <value>
            /// The adult count.
            /// </value>
            public int? AdultCount { get; set; }

            /// <summary>
            /// Gets or sets the adult measure count.
            /// </summary>
            /// <value>
            /// The measure count.
            /// </value>
            public int? AdultMeasureCount { get; set; }
        }

        /// <summary>
        /// Region Summary Class
        /// </summary>
        public class Region
        {
            public int RegionId { get; set; }
            
            public int? PastorPersonAliasId { get; set; }
            
            public int? PastorPersonId { get; set; }
            
            public int? CampusId { get; set; }
        }

        /// <summary>
        /// Campus Adult Attenance Summary
        /// </summary>
        public class CampusAdultCount
        {
            /// <summary>
            /// Gets or sets the campus identifier.
            /// </summary>
            /// <value>
            /// The campus identifier.
            /// </value>
            public int CampusId { get; set; }
            /// <summary>
            /// Gets or sets the attendance.
            /// </summary>
            /// <value>
            /// The attendance.
            /// </value>
            public int AdultCount { get; set; }
        }

        /// <summary>
        /// Campus Student Attenance Summary
        /// </summary>
        public class CampusStudentCount
        {
            /// <summary>
            /// Gets or sets the campus identifier.
            /// </summary>
            /// <value>
            /// The campus identifier.
            /// </value>
            public int CampusId { get; set; }
            /// <summary>
            /// Gets or sets the attendance.
            /// </summary>
            /// <value>
            /// The attendance.
            /// </value>
            public int StudentCount { get; set; }
        }
        #endregion
        #endregion
    }
}
