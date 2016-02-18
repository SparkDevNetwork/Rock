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
        const string ATTRIBUTE_PERSON_DATE_OF_BAPTISM = "BaptismDate";
        const string ATTRIBUTE_PERSON_ERA = "CurrentlyanERA";
        const string ATTRIBUTE_PERSON_GIVING_IN_LAST_12_MONTHS = "GivingInLast12Months";
        const string ATTRIBUTE_PERSON_DATE_OF_MEMBERSHIP = "DateofMembership";

        const string ATTRIBUTE_GLOBAL_TITHE_THRESHOLD = "TitheThreshold";
        const string ATTRIBUTE_GLOBAL_COACHING_GROUPTYPE_IDS = "CoachingGroupTypeIds";
        const string ATTRIBUTE_GLOBAL_CONNECTION_GROUPTYPE_IDS = "ConnectionGroupTypeIds";
        const string ATTRIBUTE_GLOBAL_SERVING_GROUPTYPE_IDS = "ServingGroupTypeIds";
        const string ATTRIBUTE_GLOBAL_MEMBERSHIP_VALUE_ID = "MembershipValueId";

        List<int> _campusIds = new List<int>();
        int _attendanceMetricId = -1;
        string _warningEmailAddresses = string.Empty;
        Guid _warningEmailTemplate = Guid.Empty;

        int _activeRecordStatusId = -1;
        int _familyGroupTypeId = -1;
        int _adultFamilyRoleId = -1;

        int _baptismMeasureId = -1;
        int _coachingMeasureId = -1;
        int _servingMeasureId = -1;
        int _connectionMeasureId = -1;
        int _membershipMeasureId = -1;
        int _givingMeasureId = -1;
        int _attendingMeasureId = -1;
        int _sharingMeasureId = -1;

        List<int> _filteredConnectionStatuses = new List<int>();

        int _neighborhoodAreaGroupTypeId = -1;

        List<int> _coachingGroupTypeIds = new List<int>();
        List<int> _connectionGroupTypeIds = new List<int>();
        List<int> _servingGroupTypeIds = new List<int>();

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

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            context.Result = "";

            JobDataMap dataMap = context.JobDetail.JobDataMap;

            var rockContext = new RockContext();

            _filteredConnectionStatuses = new List<int>( new int[] { 146, 65 } ); // Member = 65, Attendee = 146, Visitor = 66

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

            _activeRecordStatusId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE ).Id;
            _familyGroupTypeId = GroupTypeCache.Read(Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid()).Id;

            Guid familyMemberAdultRoleGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
            _adultFamilyRoleId = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Roles.Where( r => r.Guid == familyMemberAdultRoleGuid ).Select( r => r.Id ).FirstOrDefault();

            _neighborhoodAreaGroupTypeId = GroupTypeCache.Read( dataMap.GetString( "NeighborhoodAreaGroupType" ).AsGuid() ).Id;

            try
            {
                _coachingGroupTypeIds = GlobalAttributesCache.Read().GetValue( ATTRIBUTE_GLOBAL_COACHING_GROUPTYPE_IDS ).Split( ',' ).Select( int.Parse ).ToList();
                _connectionGroupTypeIds = GlobalAttributesCache.Read().GetValue( ATTRIBUTE_GLOBAL_CONNECTION_GROUPTYPE_IDS ).Split( ',' ).Select( int.Parse ).ToList();
                _servingGroupTypeIds = GlobalAttributesCache.Read().GetValue( ATTRIBUTE_GLOBAL_SERVING_GROUPTYPE_IDS ).Split( ',' ).Select( int.Parse ).ToList();
            }
            catch { }

            // get the last sunday date processed
            StepMeasureValueService stepMeasureValueService = new StepMeasureValueService( rockContext );

            var lastProcessedSunday = stepMeasureValueService.Queryable().Select( m => m.SundayDate ).Max();

            // if first run set it to previous sunday
            if ( !lastProcessedSunday.HasValue )
            {
                lastProcessedSunday = RockDateTime.Now.SundayDate().AddDays( -14 );
            }

            bool sundayProcessed = false;

            // process all missing sundays
            if ( lastProcessedSunday != RockDateTime.Now.SundayDate().AddDays( -7 ) )
            {
                lastProcessedSunday = lastProcessedSunday.Value.AddDays( 7 );

                while ( lastProcessedSunday <= RockDateTime.Now )
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

            using ( RockContext rockContext = new RockContext() ) {
                // check if the metrics we need exist, if not send warning email and stop
                List<MetricValue> metricValues = new MetricValueService( rockContext ).Queryable().Where( m => m.MetricValueDateTime == sundayDate && m.MetricId == _attendanceMetricId ).ToList();

                // ensure that all campuses have metrics
                if (!ContainsAllItems(_campusIds, metricValues.Select(m => m.EntityId.Value ).ToList() ) )
                {
                    // send warning email
                    if ( !string.IsNullOrWhiteSpace( _warningEmailAddresses ) && _warningEmailTemplate != Guid.Empty)
                    {
                        List<RecipientData> recipients = new List<RecipientData>();

                        List<string> emails = _warningEmailAddresses.Split( ',' ).ToList();

                        var mergeObjects = new Dictionary<string, object>();
                        mergeObjects.Add( "SundayDate", sundayDate );

                        foreach (var email in emails )
                        {
                            recipients.Add( new RecipientData( email, mergeObjects ) );
                        }

                        Email.Send( _warningEmailTemplate, recipients );

                        message = string.Format( "Could not process {0} due to missing metric values.", sundayDate.ToShortDateString() );
                    }
                    return false;
                }

                DatamartNeighborhoodService datamartNeighborhoodService = new DatamartNeighborhoodService(rockContext);
                var neighborhoodAreas = datamartNeighborhoodService.Queryable()
                                            .Where( n => n.NeighborhoodPastorId != null )
                                            .Select( n => new NeighborhoodArea
                                            {
                                                AreaId = n.NeighborhoodId.Value,
                                                //AdultCount = n.AdultCount.Value, -- adult count in the neighborhood datamart isn't currently accurate 2/17/2016
                                                PastorPersonId = n.NeighborhoodPastorId,
                                            } )
                                            .ToList();
                
                // fill in missing pieces from the datamart
                foreach(var area in neighborhoodAreas )
                {
                    area.PastorPersonAliasId = new PersonAliasService( rockContext ).Queryable().Where( a => a.PersonId == area.PastorPersonId ).Select( a => a.Id ).FirstOrDefault();
                    area.CampusId = new GroupService( rockContext ).Queryable().Where( g => g.Id == area.AreaId ).Select( g => g.CampusId ).FirstOrDefault();
                    area.AdultCount = new DatamartPersonService( rockContext ).Queryable().Where( p => p.NeighborhoodId == area.AreaId && p.FamilyRole == "Adult" && _filteredConnectionStatuses.Contains( p.ConnectionStatusValueId.Value ) ).Count();
                }

                // load counts for campuses
                var campusAdultCounts = new DatamartPersonService( rockContext ).Queryable()
                                                .Where(p => p.FamilyRole == "Adult" && _filteredConnectionStatuses.Contains( p.ConnectionStatusValueId.Value ) )
                                                .GroupBy( p => p.CampusId )
                                                .Select( a => new CampusAdultCount
                                                {
                                                    CampusId = a.Key.Value,
                                                    AdultCount = a.Count()
                                                } )
                                                .ToList();

                // process measures
                ProcessBaptisms(sundayDate, metricValues, campusAdultCounts, neighborhoodAreas );
                ProcessMembership( sundayDate, metricValues, campusAdultCounts, neighborhoodAreas );
                ProcessAttending( sundayDate, metricValues, campusAdultCounts, neighborhoodAreas );
                ProcessGiving( sundayDate, metricValues, campusAdultCounts, neighborhoodAreas );

                ProcessCoaching( sundayDate, metricValues, campusAdultCounts, neighborhoodAreas );
                ProcessConnection( sundayDate, metricValues, campusAdultCounts, neighborhoodAreas );
                ProcessServing( sundayDate, metricValues, campusAdultCounts, neighborhoodAreas );
                ProcessSharing( sundayDate, metricValues, campusAdultCounts, neighborhoodAreas );
            }

            return wasSuccessful;
        }

        // attribute based measures
        private void ProcessBaptisms( DateTime sundayDate, List<MetricValue> metricValues, List<CampusAdultCount> campusAdultCounts, List<NeighborhoodArea> neighborhoodAreas )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                DatamartPersonService datamartPersonService = new DatamartPersonService( rockContext );

                var measureCountsByCampusAll = datamartPersonService.Queryable()
                                                .Where( p => p.IsBaptized == true && _filteredConnectionStatuses.Contains( p.ConnectionStatusValueId.Value ) )
                                                .GroupBy( p => p.CampusId)
                                                .Select( r => new CampusMeasure
                                                {
                                                    CampusId = r.Key.Value,
                                                    MeasureValue = r.Count()
                                                } )
                                                .ToList();

                var measureCountsByCampusAdults = datamartPersonService.Queryable()
                                                .Where( p => p.IsBaptized == true && p.FamilyRole == "Adult" && _filteredConnectionStatuses.Contains( p.ConnectionStatusValueId.Value ) )
                                                .GroupBy( p => p.CampusId )
                                                .Select( r => new CampusMeasure
                                                {
                                                    CampusId = r.Key.Value,
                                                    MeasureValue = r.Count()
                                                } )
                                                .ToList();

                List<AreaMeasure> areaMeasures = new List<AreaMeasure>();
                foreach(var area in neighborhoodAreas ){
                    // get baptisms
                    var areaMeasureCount = datamartPersonService.Queryable()
                                                .Where( p => 
                                                    p.IsBaptized == true 
                                                    && p.FamilyRole == "Adult" 
                                                    && p.NeighborhoodId == area.AreaId 
                                                    && _filteredConnectionStatuses.Contains(p.ConnectionStatusValueId.Value))
                                                .Count();

                    AreaMeasure neighborhoodMeasure = new AreaMeasure();
                    neighborhoodMeasure.PastorPersonAliasId = area.PastorPersonAliasId;
                    neighborhoodMeasure.AdultCount = area.AdultCount;
                    neighborhoodMeasure.MeasureCount = areaMeasureCount;

                    areaMeasures.Add( neighborhoodMeasure );
                }

                SaveMetrics( _baptismMeasureId, sundayDate, metricValues, campusAdultCounts, measureCountsByCampusAll, measureCountsByCampusAdults, areaMeasures );
            }
        }
        private void ProcessMembership( DateTime sundayDate, List<MetricValue> metricValues, List<CampusAdultCount> campusAdultCounts, List<NeighborhoodArea> neighborhoodAreas )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                DatamartPersonService datamartPersonService = new DatamartPersonService( rockContext );

                int membershipConnectionId = DefinedValueCache.Read( GlobalAttributesCache.Read().GetValue( ATTRIBUTE_GLOBAL_MEMBERSHIP_VALUE_ID ) ).Id; 

                var measureCountsByCampusAll = datamartPersonService.Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.ConnectionStatusValueId == membershipConnectionId )
                                            .GroupBy( m => m.CampusId )
                                            .Select( r => new CampusMeasure
                                            {
                                                CampusId = r.Key.Value,
                                                MeasureValue = r.Count()
                                            } )
                                            .ToList();

                var measureCountsByCampusAdults = datamartPersonService.Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.ConnectionStatusValueId == membershipConnectionId && m.FamilyRole == "Adult" )
                                            .GroupBy( m => m.CampusId )
                                            .Select( r => new CampusMeasure
                                            {
                                                CampusId = r.Key.Value,
                                                MeasureValue = r.Count()
                                            } )
                                            .ToList();



                List<AreaMeasure> areaMeasures = new List<AreaMeasure>();
                foreach ( var area in neighborhoodAreas )
                {
                    // get membership
                    var areaMeasureCount = datamartPersonService.Queryable()
                                                .Where( p => p.ConnectionStatusValueId == membershipConnectionId && p.FamilyRole == "Adult" && p.NeighborhoodId == area.AreaId )
                                                .Count();

                    AreaMeasure neighborhoodMeasure = new AreaMeasure();
                    neighborhoodMeasure.PastorPersonAliasId = area.PastorPersonAliasId;
                    neighborhoodMeasure.AdultCount = area.AdultCount;
                    neighborhoodMeasure.MeasureCount = areaMeasureCount;

                    areaMeasures.Add( neighborhoodMeasure );
                }

                SaveMetrics(_membershipMeasureId, sundayDate, metricValues, campusAdultCounts, measureCountsByCampusAll, measureCountsByCampusAdults, areaMeasures );
            }
        }
        
        private void ProcessAttending( DateTime sundayDate, List<MetricValue> metricValues, List<CampusAdultCount> campusAdultCounts, List<NeighborhoodArea> neighborhoodAreas )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                DatamartPersonService datamartPersonService = new DatamartPersonService( rockContext );

                var measureCountsByCampusAll = datamartPersonService.Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.IsEra == true && _filteredConnectionStatuses.Contains( m.ConnectionStatusValueId.Value ) )
                                            .GroupBy( m => m.CampusId )
                                            .Select( r => new CampusMeasure
                                            {
                                                CampusId = r.Key.Value,
                                                MeasureValue = r.Count()
                                            } )
                                            .ToList();

                var measureCountsByCampusAdults = datamartPersonService.Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.IsEra == true && m.FamilyRole == "Adult" && _filteredConnectionStatuses.Contains( m.ConnectionStatusValueId.Value ) )
                                            .GroupBy( m => m.CampusId )
                                            .Select( r => new CampusMeasure
                                            {
                                                CampusId = r.Key.Value,
                                                MeasureValue = r.Count()
                                            } )
                                            .ToList();

                List<AreaMeasure> areaMeasures = new List<AreaMeasure>();
                foreach ( var area in neighborhoodAreas )
                {
                    // get attending
                    var areaMeasureCount = datamartPersonService.Queryable()
                                                .Where( p => p.IsEra == true && p.FamilyRole == "Adult" && p.NeighborhoodId == area.AreaId && _filteredConnectionStatuses.Contains( p.ConnectionStatusValueId.Value ) )
                                                .Count();

                    AreaMeasure neighborhoodMeasure = new AreaMeasure();
                    neighborhoodMeasure.PastorPersonAliasId = area.PastorPersonAliasId;
                    neighborhoodMeasure.AdultCount = area.AdultCount;
                    neighborhoodMeasure.MeasureCount = areaMeasureCount;

                    areaMeasures.Add( neighborhoodMeasure );
                }

                SaveMetrics( _attendingMeasureId, sundayDate, metricValues, campusAdultCounts, measureCountsByCampusAll, measureCountsByCampusAdults, areaMeasures );
            }
        }
        
        private void ProcessGiving( DateTime sundayDate, List<MetricValue> metricValues, List<CampusAdultCount> campusAdultCounts, List<NeighborhoodArea> neighborhoodAreas )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                DatamartPersonService datamartPersonService = new DatamartPersonService( rockContext );

                decimal titheThreshold = GlobalAttributesCache.Read().GetValue( ATTRIBUTE_GLOBAL_TITHE_THRESHOLD ).AsDecimal();

                var measureCountsByCampusAll = datamartPersonService.Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.GivingLast12Months >= titheThreshold )
                                            .GroupBy( m => m.CampusId )
                                            .Select( r => new CampusMeasure
                                            {
                                                CampusId = r.Key.Value,
                                                MeasureValue = r.Count()
                                            } )
                                            .ToList();

                var measureCountsByCampusAdults = datamartPersonService.Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.GivingLast12Months >= titheThreshold && m.FamilyRole == "Adult" )
                                            .GroupBy( m => m.CampusId )
                                            .Select( r => new CampusMeasure
                                            {
                                                CampusId = r.Key.Value,
                                                MeasureValue = r.Count()
                                            } )
                                            .ToList();



                List<AreaMeasure> areaMeasures = new List<AreaMeasure>();
                foreach ( var area in neighborhoodAreas )
                {
                    // get attending
                    var areaMeasureCount = datamartPersonService.Queryable()
                                                .Where( p => p.GivingLast12Months >= titheThreshold && p.FamilyRole == "Adult" && p.NeighborhoodId == area.AreaId )
                                                .Count();

                    AreaMeasure neighborhoodMeasure = new AreaMeasure();
                    neighborhoodMeasure.PastorPersonAliasId = area.PastorPersonAliasId;
                    neighborhoodMeasure.AdultCount = area.AdultCount;
                    neighborhoodMeasure.MeasureCount = areaMeasureCount;

                    areaMeasures.Add( neighborhoodMeasure );
                }

                SaveMetrics( _givingMeasureId, sundayDate, metricValues, campusAdultCounts, measureCountsByCampusAll, measureCountsByCampusAdults, areaMeasures );
            }
        }

        // group based measures
        private void ProcessCoaching( DateTime sundayDate, List<MetricValue> metricValues, List<CampusAdultCount> campusAdultCounts, List<NeighborhoodArea> neighborhoodAreas )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                DatamartPersonService datamartPersonService = new DatamartPersonService( rockContext );

                var measureCountsByCampusAll = datamartPersonService.Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.IsCoaching == true && _filteredConnectionStatuses.Contains( m.ConnectionStatusValueId.Value ) )
                                            .GroupBy( m => m.CampusId )
                                            .Select( r => new CampusMeasure
                                            {
                                                CampusId = r.Key.Value,
                                                MeasureValue = r.Count()
                                            } )
                                            .ToList();

                var measureCountsByCampusAdults = datamartPersonService.Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.IsCoaching == true && m.FamilyRole == "Adult" && _filteredConnectionStatuses.Contains( m.ConnectionStatusValueId.Value ) )
                                            .GroupBy( m => m.CampusId )
                                            .Select( r => new CampusMeasure
                                            {
                                                CampusId = r.Key.Value,
                                                MeasureValue = r.Count()
                                            } )
                                            .ToList();

                List<AreaMeasure> areaMeasures = new List<AreaMeasure>();
                foreach ( var area in neighborhoodAreas )
                {
                    // get attending
                    var areaMeasureCount = datamartPersonService.Queryable()
                                                .Where( p => p.IsCoaching == true && p.FamilyRole == "Adult" && p.NeighborhoodId == area.AreaId && _filteredConnectionStatuses.Contains( p.ConnectionStatusValueId.Value ) )
                                                .Count();

                    AreaMeasure neighborhoodMeasure = new AreaMeasure();
                    neighborhoodMeasure.PastorPersonAliasId = area.PastorPersonAliasId;
                    neighborhoodMeasure.AdultCount = area.AdultCount;
                    neighborhoodMeasure.MeasureCount = areaMeasureCount;

                    areaMeasures.Add( neighborhoodMeasure );
                }

                SaveMetrics( _coachingMeasureId, sundayDate, metricValues, campusAdultCounts, measureCountsByCampusAll, measureCountsByCampusAdults, areaMeasures );

            }
        }

        private void ProcessConnection( DateTime sundayDate, List<MetricValue> metricValues, List<CampusAdultCount> campusAdultCounts, List<NeighborhoodArea> neighborhoodAreas )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                DatamartPersonService datamartPersonService = new DatamartPersonService( rockContext );

                var measureCountsByCampusAll = datamartPersonService.Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.InNeighborhoodGroup == true && _filteredConnectionStatuses.Contains( m.ConnectionStatusValueId.Value ) )
                                            .GroupBy( m => m.CampusId )
                                            .Select( r => new CampusMeasure
                                            {
                                                CampusId = r.Key.Value,
                                                MeasureValue = r.Count()
                                            } )
                                            .ToList();

                var measureCountsByCampusAdults = datamartPersonService.Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.InNeighborhoodGroup == true && m.FamilyRole == "Adult" && _filteredConnectionStatuses.Contains( m.ConnectionStatusValueId.Value ) )
                                            .GroupBy( m => m.CampusId )
                                            .Select( r => new CampusMeasure
                                            {
                                                CampusId = r.Key.Value,
                                                MeasureValue = r.Count()
                                            } )
                                            .ToList();

                List<AreaMeasure> areaMeasures = new List<AreaMeasure>();
                foreach ( var area in neighborhoodAreas )
                {
                    // get attending
                    var areaMeasureCount = datamartPersonService.Queryable()
                                                .Where( p => p.InNeighborhoodGroup == true && p.FamilyRole == "Adult" && p.NeighborhoodId == area.AreaId && _filteredConnectionStatuses.Contains( p.ConnectionStatusValueId.Value ) )
                                                .Count();

                    AreaMeasure neighborhoodMeasure = new AreaMeasure();
                    neighborhoodMeasure.PastorPersonAliasId = area.PastorPersonAliasId;
                    neighborhoodMeasure.AdultCount = area.AdultCount;
                    neighborhoodMeasure.MeasureCount = areaMeasureCount;

                    areaMeasures.Add( neighborhoodMeasure );
                }

                SaveMetrics( _connectionMeasureId, sundayDate, metricValues, campusAdultCounts, measureCountsByCampusAll, measureCountsByCampusAdults, areaMeasures );
            }
        }

        private void ProcessServing( DateTime sundayDate, List<MetricValue> metricValues, List<CampusAdultCount> campusAdultCounts, List<NeighborhoodArea> neighborhoodAreas )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                DatamartPersonService datamartPersonService = new DatamartPersonService( rockContext );

                var measureCountsByCampusAll = datamartPersonService.Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.IsServing == true && _filteredConnectionStatuses.Contains( m.ConnectionStatusValueId.Value ) )
                                            .GroupBy( m => m.CampusId )
                                            .Select( r => new CampusMeasure
                                            {
                                                CampusId = r.Key.Value,
                                                MeasureValue = r.Count()
                                            } )
                                            .ToList();

                var measureCountsByCampusAdults = datamartPersonService.Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.IsServing == true && m.FamilyRole == "Adult" && _filteredConnectionStatuses.Contains( m.ConnectionStatusValueId.Value ) )
                                            .GroupBy( m => m.CampusId )
                                            .Select( r => new CampusMeasure
                                            {
                                                CampusId = r.Key.Value,
                                                MeasureValue = r.Count()
                                            } )
                                            .ToList();

                List<AreaMeasure> areaMeasures = new List<AreaMeasure>();
                foreach ( var area in neighborhoodAreas )
                {
                    // get attending
                    var areaMeasureCount = datamartPersonService.Queryable()
                                                .Where( p => p.IsServing == true && p.FamilyRole == "Adult" && p.NeighborhoodId == area.AreaId && _filteredConnectionStatuses.Contains( p.ConnectionStatusValueId.Value ) )
                                                .Count();

                    AreaMeasure neighborhoodMeasure = new AreaMeasure();
                    neighborhoodMeasure.PastorPersonAliasId = area.PastorPersonAliasId;
                    neighborhoodMeasure.AdultCount = area.AdultCount;
                    neighborhoodMeasure.MeasureCount = areaMeasureCount;

                    areaMeasures.Add( neighborhoodMeasure );
                }

                SaveMetrics( _servingMeasureId, sundayDate, metricValues, campusAdultCounts, measureCountsByCampusAll, measureCountsByCampusAdults, areaMeasures );
            }
        }

        private void ProcessSharing( DateTime sundayDate, List<MetricValue> metricValues, List<CampusAdultCount> campusAdultCounts, List<NeighborhoodArea> neighborhoodAreas )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                // not implemented sending in values so graphs will work as tbd

                var measureCountsByCampusAll = campusAdultCounts.Select( a => new CampusMeasure { CampusId = a.CampusId, MeasureValue = 0 } ).ToList();
                var measureCountsByCampusAdults = campusAdultCounts.Select( a => new CampusMeasure { CampusId = a.CampusId, MeasureValue = 0 } ).ToList();

                var areaMeasures = neighborhoodAreas.Select( a => new AreaMeasure { PastorPersonAliasId = a.PastorPersonAliasId, AdultCount = a.AdultCount, MeasureCount = 0 } ).ToList();

                SaveMetrics( _sharingMeasureId, sundayDate, metricValues, campusAdultCounts, measureCountsByCampusAll, measureCountsByCampusAdults, areaMeasures );
            }
        }

        #region Utilities

        private void SaveMetrics( int measureId, DateTime sundayDate, List<MetricValue> metricValues, List<CampusAdultCount> campusAdultCounts, List<CampusMeasure> campusAdultMeasures, List<CampusMeasure> campusAllMeasures, List<AreaMeasure> areaMeasures )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                StepMeasureValueService stepMeasureValueService = new StepMeasureValueService( rockContext );

                foreach ( int campusId in _campusIds )
                {
                    // get counts 
                    int weekendAttendance = metricValues.Where( m => m.EntityId == campusId ).Select( m => m.YValue ).FirstOrDefault() != null ? Decimal.ToInt16( metricValues.Where( m => m.EntityId == campusId ).Select( m => m.YValue ).FirstOrDefault().Value ) : 0;
                    int activeAdults = campusAdultCounts.Where( c => c.CampusId == campusId ).Select( c => c.AdultCount ).Count() != 0 ? campusAdultCounts.Where( c => c.CampusId == campusId ).Select( c => c.AdultCount ).FirstOrDefault() : 0;
                    int measureCountAll = campusAllMeasures.Where( b => b.CampusId == campusId ).Select( b => b.MeasureValue ).Count() != 0 ? campusAllMeasures.Where( b => b.CampusId == campusId ).Select( b => b.MeasureValue ).FirstOrDefault() : 0;
                    int measureCountAdults = campusAdultMeasures.Where( b => b.CampusId == campusId ).Select( b => b.MeasureValue ).Count() != 0 ? campusAdultMeasures.Where( b => b.CampusId == campusId ).Select( b => b.MeasureValue ).FirstOrDefault() : 0;

                    // save baptism all count / weekend attendance
                    StepMeasureValue measureValueAll = new StepMeasureValue();
                    measureValueAll.StepMeasureId = measureId;
                    measureValueAll.SundayDate = sundayDate;
                    measureValueAll.Value = measureCountAll;
                    measureValueAll.CampusId = campusId;
                    measureValueAll.WeekendAttendance = weekendAttendance;

                    stepMeasureValueService.Add( measureValueAll );

                    // save baptism adult count / campus adult count
                    StepMeasureValue measureValueAdults = new StepMeasureValue();
                    measureValueAdults.StepMeasureId = measureId;
                    measureValueAdults.SundayDate = sundayDate;
                    measureValueAdults.Value = measureCountAdults;
                    measureValueAdults.CampusId = campusId;
                    measureValueAdults.ActiveAdults = activeAdults;

                    stepMeasureValueService.Add( measureValueAdults );
                }

                rockContext.SaveChanges();

                var pastorMeasures = areaMeasures.GroupBy( n => n.PastorPersonAliasId )
                                        .Select( p => new
                                        {
                                            PastorPersonAliasId = p.Key,
                                            AdultCount = p.Sum( k => k.AdultCount ),
                                            MeasureValue = p.Sum( k => k.MeasureCount )
                                        } )
                                        .ToList();

                foreach ( var pastor in pastorMeasures )
                {
                    StepMeasureValue measureValueAdults = new StepMeasureValue();
                    measureValueAdults.StepMeasureId = measureId;
                    measureValueAdults.SundayDate = sundayDate;
                    measureValueAdults.Value = pastor.MeasureValue.HasValue ? pastor.MeasureValue.Value : 0;
                    measureValueAdults.PastorPersonAliasId = pastor.PastorPersonAliasId;
                    measureValueAdults.ActiveAdults = pastor.AdultCount;

                    stepMeasureValueService.Add( measureValueAdults );
                }
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

        /// <summary>
        /// Campus Measure
        /// </summary>
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
        
        /// <summary>
        /// Area Measuer
        /// </summary>
        public class AreaMeasure
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
            /// Gets or sets the measure count.
            /// </summary>
            /// <value>
            /// The measure count.
            /// </value>
            public int? MeasureCount { get; set; }
        }

        /// <summary>
        /// Neighborhood Area Summary Class
        /// </summary>
        public class NeighborhoodArea
        {
            /// <summary>
            /// Gets or sets the area identifier.
            /// </summary>
            /// <value>
            /// The area identifier.
            /// </value>
            public int AreaId { get; set; }
            /// <summary>
            /// Gets or sets the pastor person alias identifier.
            /// </summary>
            /// <value>
            /// The pastor person alias identifier.
            /// </value>
            public int? PastorPersonAliasId { get; set; }
            /// <summary>
            /// Gets or sets the pastor person alias identifier.
            /// </summary>
            /// <value>
            /// The pastor person alias identifier.
            /// </value>
            public int? PastorPersonId { get; set; }
            /// <summary>
            /// Gets or sets the campus identifier.
            /// </summary>
            /// <value>
            /// The campus identifier.
            /// </value>
            public int? CampusId { get; set; }
            /// <summary>
            /// Gets or sets the adult count.
            /// </summary>
            /// <value>
            /// The adult count.
            /// </value>
            public int AdultCount { get; set; }
        }

        /// <summary>
        /// Campus Attenance Summary
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
        #endregion
        #endregion
    }
}
