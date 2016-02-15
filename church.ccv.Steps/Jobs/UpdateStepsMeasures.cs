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

            // process all missing sundays
            if ( lastProcessedSunday != RockDateTime.Now.SundayDate().AddDays( -7 ) )
            {
                lastProcessedSunday = lastProcessedSunday.Value.AddDays( 7 );

                while ( lastProcessedSunday <= RockDateTime.Now )
                {
                    string processingMessages = string.Empty;
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

                // load counts for campuses
                var campusAdultCounts = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.Group.GroupTypeId == _familyGroupTypeId
                                                    && m.Person.RecordStatusValueId == _activeRecordStatusId
                                                    && m.GroupRoleId == _adultFamilyRoleId )
                                            .GroupBy( m => m.Group.CampusId )
                                            .Select( a => new CampusAdultCount
                                            {
                                                    CampusId = a.Key.Value,
                                                    AdultCount = a.Count()
                                                } )
                                            .ToList();

                // load neighborhood group areas
                var neighborhoodAreas = new GroupService( rockContext ).Queryable().AsNoTracking()
                                            .Where( g =>
                                                 g.GroupTypeId == _neighborhoodAreaGroupTypeId
                                                 && g.IsActive == true )
                                             .Select( g => new NeighborhoodArea
                                             {
                                                 AreaId = g.Id,
                                                 PastorPersonAliasId = g.Members.Where( m => m.GroupRole.IsLeader ).Select( m => m.Person.Aliases.FirstOrDefault().Id ).FirstOrDefault(),
                                                 CampusId = g.CampusId
                                             } )
                                             .ToList();
                    
                // get details for each area
                foreach(var area in neighborhoodAreas )
                {
                    area.FamilyIds = new GroupService( rockContext ).GetGeofencedFamilies( area.AreaId )
                                        .Where( f =>
                                             f.CampusId == area.CampusId
                                             && f.IsActive )
                                        .Select( f => f.Id )
                                        .ToList();

                    area.AdultCount = new GroupMemberService( rockContext ).Queryable()
                                            .Where( m =>
                                                     m.GroupMemberStatus == GroupMemberStatus.Active
                                                     && m.GroupRoleId == _adultFamilyRoleId
                                                     && area.FamilyIds.Contains( m.GroupId ) )
                                             .Count();
                }

                var pastorSummary = neighborhoodAreas
                                        .GroupBy( n => n.PastorPersonAliasId )
                                        .Select( grp => new PastorSummary
                                        {
                                            PastorPersonAliasId = grp.Key,
                                            FamilyIds = grp.SelectMany( ff => ff.FamilyIds ).Distinct().ToList(),
                                            AdultCount = grp.Sum( ff => ff.AdultCount )
                                        } )
                                        .ToList();

                // clear out object to reduce memory
                neighborhoodAreas = null;

                // process measures
                ProcessBaptisms(sundayDate, metricValues, campusAdultCounts, pastorSummary );
                ProcessMembership( sundayDate, metricValues, campusAdultCounts, pastorSummary );
                ProcessAttending( sundayDate, metricValues, campusAdultCounts, pastorSummary );
                ProcessGiving( sundayDate, metricValues, campusAdultCounts, pastorSummary );

                ProcessCoaching( sundayDate, metricValues, campusAdultCounts, pastorSummary );
                ProcessConnection( sundayDate, metricValues, campusAdultCounts, pastorSummary );
                ProcessServing( sundayDate, metricValues, campusAdultCounts, pastorSummary );
                ProcessSharing( sundayDate, metricValues, campusAdultCounts, pastorSummary );
            }

            return wasSuccessful;
        }

        // attribute based measures
        private void ProcessBaptisms( DateTime sundayDate, List<MetricValue> metricValues, List<CampusAdultCount> campusAdultCounts, List<PastorSummary>pastorSummary )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                StepMeasureValueService stepMeasureValueService = new StepMeasureValueService( rockContext );

                var baptismAttributeId = new AttributeService( rockContext ).Queryable()
                                            .Where( a => 
                                                        a.Key == ATTRIBUTE_PERSON_DATE_OF_BAPTISM
                                                        && a.EntityType.Name == "Rock.Model.Person" )
                                            .Select( a => a.Id )
                                            .FirstOrDefault();

                var baptismAttributes = new AttributeValueService( rockContext ).Queryable().AsNoTracking()
                                            .Where( a => 
                                                a.AttributeId == baptismAttributeId
                                                && a.ValueAsDateTime != null 
                                                && a.ValueAsDateTime <= sundayDate)
                                            .Select( a => a.EntityId );

                var baptismCountsByCampusAll = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.Group.GroupTypeId == _familyGroupTypeId
                                                    && baptismAttributes.Contains( m.PersonId )
                                                    && m.Person.RecordStatusValueId == _activeRecordStatusId )
                                            .GroupBy( m => m.Group.CampusId )
                                            .Select( r => new
                                            {
                                                CampusId = r.Key,
                                                BaptismCount = r.Count()
                                            })
                                            .ToList();

                var baptismCountsByCampusAdults = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.Group.GroupTypeId == _familyGroupTypeId
                                                    && baptismAttributes.Contains( m.PersonId )
                                                    && m.Person.RecordStatusValueId == _activeRecordStatusId
                                                    && m.GroupRoleId == _adultFamilyRoleId)
                                            .GroupBy( m => m.Group.CampusId )
                                            .Select( r => new
                                            {
                                                CampusId = r.Key,
                                                BaptismCount = r.Count()
                                            })
                                            .ToList();

                foreach ( int campusId in _campusIds )
                {
                    // get counts 
                    int weekendAttendance = metricValues.Where( m => m.EntityId == campusId ).Select( m => m.YValue ).FirstOrDefault() != null ? Decimal.ToInt16( metricValues.Where( m => m.EntityId == campusId ).Select( m => m.YValue ).FirstOrDefault().Value ) : 0;
                    int activeAdults = campusAdultCounts.Where( c => c.CampusId == campusId ).Select( c => c.AdultCount ).Count() != 0 ? campusAdultCounts.Where( c => c.CampusId == campusId ).Select( c => c.AdultCount ).FirstOrDefault() : 0;
                    int baptismCountAll = baptismCountsByCampusAll.Where( b => b.CampusId == campusId ).Select( b => b.BaptismCount ).Count() != 0 ? baptismCountsByCampusAll.Where( b => b.CampusId == campusId ).Select( b => b.BaptismCount ).FirstOrDefault() : 0;
                    int baptismCountAdults = baptismCountsByCampusAdults.Where( b => b.CampusId == campusId ).Select( b => b.BaptismCount ).Count() != 0 ? baptismCountsByCampusAdults.Where( b => b.CampusId == campusId ).Select( b => b.BaptismCount ).FirstOrDefault() : 0;

                    // save baptism all count / weekend attendance
                    StepMeasureValue measureValueAll = new StepMeasureValue();
                    measureValueAll.StepMeasureId = _baptismMeasureId;
                    measureValueAll.SundayDate = sundayDate;
                    measureValueAll.Value = baptismCountAll;
                    measureValueAll.CampusId = campusId;
                    measureValueAll.WeekendAttendance = weekendAttendance;

                    stepMeasureValueService.Add( measureValueAll );

                    // save baptism adult count / campus adult count
                    StepMeasureValue measureValueAdults = new StepMeasureValue();
                    measureValueAdults.StepMeasureId = _baptismMeasureId;
                    measureValueAdults.SundayDate = sundayDate;
                    measureValueAdults.Value = baptismCountAdults;
                    measureValueAdults.CampusId = campusId;
                    measureValueAdults.ActiveAdults = activeAdults;

                    stepMeasureValueService.Add( measureValueAdults );
                }

                rockContext.SaveChanges();

                foreach(var pastor in pastorSummary ){
                    // get baptisms
                    var baptismCount = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                    pastor.FamilyIds.Contains(m.GroupId)
                                                    && baptismAttributes.Contains( m.PersonId )
                                                    && m.Person.RecordStatusValueId == _activeRecordStatusId
                                                    && m.GroupRoleId == _adultFamilyRoleId )
                                            .Count();

                    StepMeasureValue measureValueAdults = new StepMeasureValue();
                    measureValueAdults.StepMeasureId = _baptismMeasureId;
                    measureValueAdults.SundayDate = sundayDate;
                    measureValueAdults.Value = baptismCount;
                    measureValueAdults.PastorPersonAliasId = pastor.PastorPersonAliasId;
                    measureValueAdults.ActiveAdults = pastor.AdultCount;

                    stepMeasureValueService.Add( measureValueAdults );
                }

                rockContext.SaveChanges();
            }
        }

        private void ProcessMembership( DateTime sundayDate, List<MetricValue> metricValues, List<CampusAdultCount> campusAdultCounts, List<PastorSummary> pastorSummary )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                StepMeasureValueService stepMeasureValueService = new StepMeasureValueService( rockContext );

                var membershipAttributeId = new AttributeService( rockContext ).Queryable()
                                            .Where( a =>
                                                        a.Key == ATTRIBUTE_PERSON_DATE_OF_MEMBERSHIP
                                                        && a.EntityType.Name == "Rock.Model.Person" )
                                            .Select( a => a.Id )
                                            .FirstOrDefault();

                var membershipAttributes = new AttributeValueService( rockContext ).Queryable().AsNoTracking()
                                            .Where( a =>
                                                a.AttributeId == membershipAttributeId
                                                && a.ValueAsDateTime != null
                                                && a.ValueAsDateTime <= sundayDate )
                                            .Select( a => a.EntityId );

                var membershipCountsByCampusAll = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.Group.GroupTypeId == _familyGroupTypeId
                                                    && membershipAttributes.Contains( m.PersonId )
                                                    && m.Person.RecordStatusValueId == _activeRecordStatusId )
                                            .GroupBy( m => m.Group.CampusId )
                                            .Select( r => new
                                            {
                                                CampusId = r.Key,
                                                MembershipCount = r.Count()
                                            } )
                                            .ToList();

                var membershipCountsByCampusAdults = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.Group.GroupTypeId == _familyGroupTypeId
                                                    && membershipAttributes.Contains( m.PersonId )
                                                    && m.Person.RecordStatusValueId == _activeRecordStatusId
                                                    && m.GroupRoleId == _adultFamilyRoleId )
                                            .GroupBy( m => m.Group.CampusId )
                                            .Select( r => new
                                            {
                                                CampusId = r.Key,
                                                MembershipCount = r.Count()
                                            } )
                                            .ToList();

                foreach ( int campusId in _campusIds )
                {
                    // get counts 
                    int weekendAttendance = metricValues.Where( m => m.EntityId == campusId ).Select( m => m.YValue ).FirstOrDefault() != null ? Decimal.ToInt16( metricValues.Where( m => m.EntityId == campusId ).Select( m => m.YValue ).FirstOrDefault().Value ) : 0;
                    int activeAdults = campusAdultCounts.Where( c => c.CampusId == campusId ).Select( c => c.AdultCount ).Count() != 0 ? campusAdultCounts.Where( c => c.CampusId == campusId ).Select( c => c.AdultCount ).FirstOrDefault() : 0;
                    int membershipCountAll = membershipCountsByCampusAll.Where( b => b.CampusId == campusId ).Select( b => b.MembershipCount ).Count() != 0 ? membershipCountsByCampusAll.Where( b => b.CampusId == campusId ).Select( b => b.MembershipCount ).FirstOrDefault() : 0;
                    int membershipCountAdults = membershipCountsByCampusAdults.Where( b => b.CampusId == campusId ).Select( b => b.MembershipCount ).Count() != 0 ? membershipCountsByCampusAdults.Where( b => b.CampusId == campusId ).Select( b => b.MembershipCount ).FirstOrDefault() : 0;

                    // save membership all count / weekend attendance
                    StepMeasureValue measureValueAll = new StepMeasureValue();
                    measureValueAll.StepMeasureId = _membershipMeasureId;
                    measureValueAll.SundayDate = sundayDate;
                    measureValueAll.Value = membershipCountAll;
                    measureValueAll.CampusId = campusId;
                    measureValueAll.WeekendAttendance = weekendAttendance;

                    stepMeasureValueService.Add( measureValueAll );

                    // save membership adult count / campus adult count
                    StepMeasureValue measureValueAdults = new StepMeasureValue();
                    measureValueAdults.StepMeasureId = _membershipMeasureId;
                    measureValueAdults.SundayDate = sundayDate;
                    measureValueAdults.Value = membershipCountAdults;
                    measureValueAdults.CampusId = campusId;
                    measureValueAdults.ActiveAdults = activeAdults;

                    stepMeasureValueService.Add( measureValueAdults );
                }

                rockContext.SaveChanges();

                foreach ( var pastor in pastorSummary )
                {
                    // get memberships
                    var membershipCount = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                    pastor.FamilyIds.Contains( m.GroupId )
                                                    && membershipAttributes.Contains( m.PersonId )
                                                    && m.Person.RecordStatusValueId == _activeRecordStatusId
                                                    && m.GroupRoleId == _adultFamilyRoleId )
                                            .Count();

                    StepMeasureValue measureValueAdults = new StepMeasureValue();
                    measureValueAdults.StepMeasureId = _membershipMeasureId;
                    measureValueAdults.SundayDate = sundayDate;
                    measureValueAdults.Value = membershipCount;
                    measureValueAdults.PastorPersonAliasId = pastor.PastorPersonAliasId;
                    measureValueAdults.ActiveAdults = pastor.AdultCount;

                    stepMeasureValueService.Add( measureValueAdults );
                }

                rockContext.SaveChanges();
            }
        }

        private void ProcessAttending( DateTime sundayDate, List<MetricValue> metricValues, List<CampusAdultCount> campusAdultCounts, List<PastorSummary> pastorSummary )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                StepMeasureValueService stepMeasureValueService = new StepMeasureValueService( rockContext );

                var attendingAttributeId = new AttributeService( rockContext ).Queryable()
                                            .Where( a =>
                                                        a.Key == ATTRIBUTE_PERSON_ERA
                                                        && a.EntityType.Name == "Rock.Model.Person" )
                                            .Select( a => a.Id )
                                            .FirstOrDefault();

                var eraAttributes = new AttributeValueService( rockContext ).Queryable().AsNoTracking()
                                            .Where( a =>
                                                a.AttributeId == attendingAttributeId
                                                && a.Value == "true" )
                                            .Select( a => a.EntityId );

                var attendingCountsByCampusAll = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.Group.GroupTypeId == _familyGroupTypeId
                                                    && eraAttributes.Contains( m.PersonId )
                                                    && m.Person.RecordStatusValueId == _activeRecordStatusId )
                                            .GroupBy( m => m.Group.CampusId )
                                            .Select( r => new
                                            {
                                                CampusId = r.Key,
                                                AttendingCount = r.Count()
                                            } )
                                            .ToList();

                var attendingCountsByCampusAdults = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.Group.GroupTypeId == _familyGroupTypeId
                                                    && eraAttributes.Contains( m.PersonId )
                                                    && m.Person.RecordStatusValueId == _activeRecordStatusId
                                                    && m.GroupRoleId == _adultFamilyRoleId )
                                            .GroupBy( m => m.Group.CampusId )
                                            .Select( r => new
                                            {
                                                CampusId = r.Key,
                                                AttendingCount = r.Count()
                                            } )
                                            .ToList();

                foreach ( int campusId in _campusIds )
                {
                    // get counts 
                    int weekendAttendance = metricValues.Where( m => m.EntityId == campusId ).Select( m => m.YValue ).FirstOrDefault() != null ? Decimal.ToInt16( metricValues.Where( m => m.EntityId == campusId ).Select( m => m.YValue ).FirstOrDefault().Value ) : 0;
                    int activeAdults = campusAdultCounts.Where( c => c.CampusId == campusId ).Select( c => c.AdultCount ).Count() != 0 ? campusAdultCounts.Where( c => c.CampusId == campusId ).Select( c => c.AdultCount ).FirstOrDefault() : 0;
                    int attendingCountAll = attendingCountsByCampusAll.Where( b => b.CampusId == campusId ).Select( b => b.AttendingCount ).Count() != 0 ? attendingCountsByCampusAll.Where( b => b.CampusId == campusId ).Select( b => b.AttendingCount ).FirstOrDefault() : 0;
                    int attendingCountAdults = attendingCountsByCampusAdults.Where( b => b.CampusId == campusId ).Select( b => b.AttendingCount ).Count() != 0 ? attendingCountsByCampusAdults.Where( b => b.CampusId == campusId ).Select( b => b.AttendingCount ).FirstOrDefault() : 0;

                    // save attending all count / weekend attendance
                    StepMeasureValue measureValueAll = new StepMeasureValue();
                    measureValueAll.StepMeasureId = _attendingMeasureId;
                    measureValueAll.SundayDate = sundayDate;
                    measureValueAll.Value = attendingCountAll;
                    measureValueAll.CampusId = campusId;
                    measureValueAll.WeekendAttendance = weekendAttendance;

                    stepMeasureValueService.Add( measureValueAll );

                    // save attending adult count / campus adult count
                    StepMeasureValue measureValueAdults = new StepMeasureValue();
                    measureValueAdults.StepMeasureId = _attendingMeasureId;
                    measureValueAdults.SundayDate = sundayDate;
                    measureValueAdults.Value = attendingCountAdults;
                    measureValueAdults.CampusId = campusId;
                    measureValueAdults.ActiveAdults = activeAdults;

                    stepMeasureValueService.Add( measureValueAdults );
                }

                rockContext.SaveChanges();

                foreach ( var pastor in pastorSummary )
                {
                    // get attending
                    var attendingCount = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                    pastor.FamilyIds.Contains( m.GroupId )
                                                    && eraAttributes.Contains( m.PersonId )
                                                    && m.Person.RecordStatusValueId == _activeRecordStatusId
                                                    && m.GroupRoleId == _adultFamilyRoleId )
                                            .Count();

                    StepMeasureValue measureValueAdults = new StepMeasureValue();
                    measureValueAdults.StepMeasureId = _attendingMeasureId;
                    measureValueAdults.SundayDate = sundayDate;
                    measureValueAdults.Value = attendingCount;
                    measureValueAdults.PastorPersonAliasId = pastor.PastorPersonAliasId;
                    measureValueAdults.ActiveAdults = pastor.AdultCount;

                    stepMeasureValueService.Add( measureValueAdults );
                }

                rockContext.SaveChanges();
            }
        }

        private void ProcessGiving( DateTime sundayDate, List<MetricValue> metricValues, List<CampusAdultCount> campusAdultCounts, List<PastorSummary> pastorSummary )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                StepMeasureValueService stepMeasureValueService = new StepMeasureValueService( rockContext );

                var givingAttributeId = new AttributeService( rockContext ).Queryable()
                                            .Where( a =>
                                                        a.Key == ATTRIBUTE_PERSON_GIVING_IN_LAST_12_MONTHS
                                                        && a.EntityType.Name == "Rock.Model.Person" )
                                            .Select( a => a.Id )
                                            .FirstOrDefault();

                decimal titheThreshold = GlobalAttributesCache.Read().GetValue( ATTRIBUTE_GLOBAL_TITHE_THRESHOLD ).AsDecimal();

                var givingAttributes = new AttributeValueService( rockContext ).Queryable().AsNoTracking()
                                            .Where( a =>
                                                a.AttributeId == givingAttributeId
                                                && a.ValueAsNumeric >= titheThreshold )
                                            .Select( a => a.EntityId );

                var givingCountsByCampusAll = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.Group.GroupTypeId == _familyGroupTypeId
                                                    && givingAttributes.Contains( m.PersonId )
                                                    && m.Person.RecordStatusValueId == _activeRecordStatusId )
                                            .GroupBy( m => m.Group.CampusId )
                                            .Select( r => new
                                            {
                                                CampusId = r.Key,
                                                GivingCount = r.Count()
                                            } )
                                            .ToList();

                var givingCountsByCampusAdults = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.Group.GroupTypeId == _familyGroupTypeId
                                                    && givingAttributes.Contains( m.PersonId )
                                                    && m.Person.RecordStatusValueId == _activeRecordStatusId
                                                    && m.GroupRoleId == _adultFamilyRoleId )
                                            .GroupBy( m => m.Group.CampusId )
                                            .Select( r => new
                                            {
                                                CampusId = r.Key,
                                                GivingCount = r.Count()
                                            } )
                                            .ToList();

                foreach ( int campusId in _campusIds )
                {
                    // get counts 
                    int weekendAttendance = metricValues.Where( m => m.EntityId == campusId ).Select( m => m.YValue ).FirstOrDefault() != null ? Decimal.ToInt16( metricValues.Where( m => m.EntityId == campusId ).Select( m => m.YValue ).FirstOrDefault().Value ) : 0;
                    int activeAdults = campusAdultCounts.Where( c => c.CampusId == campusId ).Select( c => c.AdultCount ).Count() != 0 ? campusAdultCounts.Where( c => c.CampusId == campusId ).Select( c => c.AdultCount ).FirstOrDefault() : 0;
                    int givingCountAll = givingCountsByCampusAll.Where( b => b.CampusId == campusId ).Select( b => b.GivingCount ).Count() != 0 ? givingCountsByCampusAll.Where( b => b.CampusId == campusId ).Select( b => b.GivingCount ).FirstOrDefault() : 0;
                    int givingCountAdults = givingCountsByCampusAdults.Where( b => b.CampusId == campusId ).Select( b => b.GivingCount ).Count() != 0 ? givingCountsByCampusAdults.Where( b => b.CampusId == campusId ).Select( b => b.GivingCount ).FirstOrDefault() : 0;

                    // save giving all count / weekend attendance
                    StepMeasureValue measureValueAll = new StepMeasureValue();
                    measureValueAll.StepMeasureId = _givingMeasureId;
                    measureValueAll.SundayDate = sundayDate;
                    measureValueAll.Value = givingCountAll;
                    measureValueAll.CampusId = campusId;
                    measureValueAll.WeekendAttendance = weekendAttendance;

                    stepMeasureValueService.Add( measureValueAll );

                    // save giving adult count / campus adult count
                    StepMeasureValue measureValueAdults = new StepMeasureValue();
                    measureValueAdults.StepMeasureId = _givingMeasureId;
                    measureValueAdults.SundayDate = sundayDate;
                    measureValueAdults.Value = givingCountAdults;
                    measureValueAdults.CampusId = campusId;
                    measureValueAdults.ActiveAdults = activeAdults;

                    stepMeasureValueService.Add( measureValueAdults );
                }

                rockContext.SaveChanges();

                foreach ( var pastor in pastorSummary )
                {
                    // get giving
                    var givingCount = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                    pastor.FamilyIds.Contains( m.GroupId )
                                                    && givingAttributes.Contains( m.PersonId )
                                                    && m.Person.RecordStatusValueId == _activeRecordStatusId
                                                    && m.GroupRoleId == _adultFamilyRoleId )
                                            .Count();

                    StepMeasureValue measureValueAdults = new StepMeasureValue();
                    measureValueAdults.StepMeasureId = _givingMeasureId;
                    measureValueAdults.SundayDate = sundayDate;
                    measureValueAdults.Value = givingCount;
                    measureValueAdults.PastorPersonAliasId = pastor.PastorPersonAliasId;
                    measureValueAdults.ActiveAdults = pastor.AdultCount;

                    stepMeasureValueService.Add( measureValueAdults );
                }

                rockContext.SaveChanges();
            }
        }


        // group based measures
        private void ProcessCoaching( DateTime sundayDate, List<MetricValue> metricValues, List<CampusAdultCount> campusAdultCounts, List<PastorSummary> pastorSummary )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                StepMeasureValueService stepMeasureValueService = new StepMeasureValueService( rockContext );

                var coaches = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                    .Where( m =>
                                             _coachingGroupTypeIds.Contains( m.Group.GroupTypeId )
                                             && m.GroupRole.IsLeader
                                             && m.Group.IsActive
                                             && m.GroupMemberStatus == GroupMemberStatus.Active )
                                    .Select( m => m.PersonId );

                // all coaches should be adults
                var coachCountsByCampusAll = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.Group.GroupTypeId == _familyGroupTypeId
                                                    && coaches.Contains( m.PersonId )
                                                    && m.Person.RecordStatusValueId == _activeRecordStatusId )
                                            .GroupBy( m => m.Group.CampusId )
                                            .Select( r => new
                                            {
                                                CampusId = r.Key,
                                                CoachCount = r.Count()
                                            } )
                                            .ToList();

                foreach ( int campusId in _campusIds )
                {
                    // get counts 
                    int weekendAttendance = metricValues.Where( m => m.EntityId == campusId ).Select( m => m.YValue ).FirstOrDefault() != null ? Decimal.ToInt16( metricValues.Where( m => m.EntityId == campusId ).Select( m => m.YValue ).FirstOrDefault().Value ) : 0;
                    int activeAdults = campusAdultCounts.Where( c => c.CampusId == campusId ).Select( c => c.AdultCount ).Count() != 0 ? campusAdultCounts.Where( c => c.CampusId == campusId ).Select( c => c.AdultCount ).FirstOrDefault() : 0;
                    int coachCountAll = coachCountsByCampusAll.Where( b => b.CampusId == campusId ).Select( b => b.CoachCount ).Count() != 0 ? coachCountsByCampusAll.Where( b => b.CampusId == campusId ).Select( b => b.CoachCount ).FirstOrDefault() : 0;
                    
                    // save coach all count / weekend attendance
                    StepMeasureValue measureValueAll = new StepMeasureValue();
                    measureValueAll.StepMeasureId = _coachingMeasureId;
                    measureValueAll.SundayDate = sundayDate;
                    measureValueAll.Value = coachCountAll;
                    measureValueAll.CampusId = campusId;
                    measureValueAll.WeekendAttendance = weekendAttendance;

                    stepMeasureValueService.Add( measureValueAll );

                    // save coach adult count / campus adult count
                    StepMeasureValue measureValueAdults = new StepMeasureValue();
                    measureValueAdults.StepMeasureId = _coachingMeasureId;
                    measureValueAdults.SundayDate = sundayDate;
                    measureValueAdults.Value = coachCountAll;
                    measureValueAdults.CampusId = campusId;
                    measureValueAdults.ActiveAdults = activeAdults;

                    stepMeasureValueService.Add( measureValueAdults );
                }

                rockContext.SaveChanges();

                foreach ( var pastor in pastorSummary )
                {
                    // get connections
                    var coachCount = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                    pastor.FamilyIds.Contains( m.GroupId )
                                                    && coaches.Contains( m.PersonId )
                                                    && m.Person.RecordStatusValueId == _activeRecordStatusId )
                                            .Count();

                    StepMeasureValue measureValueAdults = new StepMeasureValue();
                    measureValueAdults.StepMeasureId = _coachingMeasureId;
                    measureValueAdults.SundayDate = sundayDate;
                    measureValueAdults.Value = coachCount;
                    measureValueAdults.PastorPersonAliasId = pastor.PastorPersonAliasId;
                    measureValueAdults.ActiveAdults = pastor.AdultCount;

                    stepMeasureValueService.Add( measureValueAdults );
                }

                rockContext.SaveChanges();
            }
        }

        private void ProcessConnection( DateTime sundayDate, List<MetricValue> metricValues, List<CampusAdultCount> campusAdultCounts, List<PastorSummary> pastorSummary )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                StepMeasureValueService stepMeasureValueService = new StepMeasureValueService( rockContext );

                var connectionMembers = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                    .Where( m =>
                                             _connectionGroupTypeIds.Contains( m.Group.GroupTypeId )
                                             && m.Group.IsActive
                                             && m.GroupMemberStatus == GroupMemberStatus.Active )
                                    .Select( m => m.PersonId );

                // all connection group members
                var connectionCountsByCampusAll = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.Group.GroupTypeId == _familyGroupTypeId
                                                    && connectionMembers.Contains( m.PersonId )
                                                    && m.Person.RecordStatusValueId == _activeRecordStatusId )
                                            .GroupBy( m => m.Group.CampusId )
                                            .Select( r => new
                                            {
                                                CampusId = r.Key,
                                                ConnectionCount = r.Count()
                                            } )
                                            .ToList();

                // adult connection group members
                var connectionCountsByCampusAdults = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.Group.GroupTypeId == _familyGroupTypeId
                                                    && connectionMembers.Contains( m.PersonId )
                                                    && m.Person.RecordStatusValueId == _activeRecordStatusId
                                                    && m.GroupRoleId == _adultFamilyRoleId )
                                            .GroupBy( m => m.Group.CampusId )
                                            .Select( r => new
                                            {
                                                CampusId = r.Key,
                                                ConnectionCount = r.Count()
                                            } )
                                            .ToList();
                                            

                foreach ( int campusId in _campusIds )
                {
                    // get counts 
                    int weekendAttendance = metricValues.Where( m => m.EntityId == campusId ).Select( m => m.YValue ).FirstOrDefault() != null ? Decimal.ToInt16( metricValues.Where( m => m.EntityId == campusId ).Select( m => m.YValue ).FirstOrDefault().Value ) : 0;
                    int activeAdults = campusAdultCounts.Where( c => c.CampusId == campusId ).Select( c => c.AdultCount ).Count() != 0 ? campusAdultCounts.Where( c => c.CampusId == campusId ).Select( c => c.AdultCount ).FirstOrDefault() : 0;
                    int connectionCountAll = connectionCountsByCampusAll.Where( b => b.CampusId == campusId ).Select( b => b.ConnectionCount ).Count() != 0 ? connectionCountsByCampusAll.Where( b => b.CampusId == campusId ).Select( b => b.ConnectionCount ).FirstOrDefault() : 0;
                    int connectionCountAdults = connectionCountsByCampusAdults.Where( b => b.CampusId == campusId ).Select( b => b.ConnectionCount ).Count() != 0 ? connectionCountsByCampusAdults.Where( b => b.CampusId == campusId ).Select( b => b.ConnectionCount ).FirstOrDefault() : 0;

                    // save connection all count / weekend attendance
                    StepMeasureValue measureValueAll = new StepMeasureValue();
                    measureValueAll.StepMeasureId = _connectionMeasureId;
                    measureValueAll.SundayDate = sundayDate;
                    measureValueAll.Value = connectionCountAll;
                    measureValueAll.CampusId = campusId;
                    measureValueAll.WeekendAttendance = weekendAttendance;

                    stepMeasureValueService.Add( measureValueAll );

                    // save coach adult count / campus adult count
                    StepMeasureValue measureValueAdults = new StepMeasureValue();
                    measureValueAdults.StepMeasureId = _connectionMeasureId;
                    measureValueAdults.SundayDate = sundayDate;
                    measureValueAdults.Value = connectionCountAdults;
                    measureValueAdults.CampusId = campusId;
                    measureValueAdults.ActiveAdults = activeAdults;

                    stepMeasureValueService.Add( measureValueAdults );
                }

                rockContext.SaveChanges();

                foreach ( var pastor in pastorSummary )
                {
                    // get connections
                    var connectionCount = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                    pastor.FamilyIds.Contains( m.GroupId )
                                                    && connectionMembers.Contains( m.PersonId )
                                                    && m.Person.RecordStatusValueId == _activeRecordStatusId )
                                            .Count();

                    StepMeasureValue measureValueAdults = new StepMeasureValue();
                    measureValueAdults.StepMeasureId = _connectionMeasureId;
                    measureValueAdults.SundayDate = sundayDate;
                    measureValueAdults.Value = connectionCount;
                    measureValueAdults.PastorPersonAliasId = pastor.PastorPersonAliasId;
                    measureValueAdults.ActiveAdults = pastor.AdultCount;

                    stepMeasureValueService.Add( measureValueAdults );
                }

                rockContext.SaveChanges();
            }
        }

        private void ProcessServing( DateTime sundayDate, List<MetricValue> metricValues, List<CampusAdultCount> campusAdultCounts, List<PastorSummary> pastorSummary )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                StepMeasureValueService stepMeasureValueService = new StepMeasureValueService( rockContext );

                var servingMembers = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                    .Where( m =>
                                             _servingGroupTypeIds.Contains( m.Group.GroupTypeId )
                                             && m.Group.IsActive
                                             && m.GroupMemberStatus == GroupMemberStatus.Active )
                                    .Select( m => m.PersonId );

                // all serving group members
                var servingCountsByCampusAll = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.Group.GroupTypeId == _familyGroupTypeId
                                                    && servingMembers.Contains( m.PersonId )
                                                    && m.Person.RecordStatusValueId == _activeRecordStatusId )
                                            .GroupBy( m => m.Group.CampusId )
                                            .Select( r => new
                                            {
                                                CampusId = r.Key,
                                                ServingCount = r.Count()
                                            } )
                                            .ToList();

                // adult serving group members
                var servingCountsByCampusAdults = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                    m.Group.GroupTypeId == _familyGroupTypeId
                                                    && servingMembers.Contains( m.PersonId )
                                                    && m.Person.RecordStatusValueId == _activeRecordStatusId
                                                    && m.GroupRoleId == _adultFamilyRoleId )
                                            .GroupBy( m => m.Group.CampusId )
                                            .Select( r => new
                                            {
                                                CampusId = r.Key,
                                                ServingCount = r.Count()
                                            } )
                                            .ToList();


                foreach ( int campusId in _campusIds )
                {
                    // get counts 
                    int weekendAttendance = metricValues.Where( m => m.EntityId == campusId ).Select( m => m.YValue ).FirstOrDefault() != null ? Decimal.ToInt16( metricValues.Where( m => m.EntityId == campusId ).Select( m => m.YValue ).FirstOrDefault().Value ) : 0;
                    int activeAdults = campusAdultCounts.Where( c => c.CampusId == campusId ).Select( c => c.AdultCount ).Count() != 0 ? campusAdultCounts.Where( c => c.CampusId == campusId ).Select( c => c.AdultCount ).FirstOrDefault() : 0;
                    int servingCountAll = servingCountsByCampusAll.Where( b => b.CampusId == campusId ).Select( b => b.ServingCount ).Count() != 0 ? servingCountsByCampusAll.Where( b => b.CampusId == campusId ).Select( b => b.ServingCount ).FirstOrDefault() : 0;
                    int servingCountAdults = servingCountsByCampusAdults.Where( b => b.CampusId == campusId ).Select( b => b.ServingCount ).Count() != 0 ? servingCountsByCampusAdults.Where( b => b.CampusId == campusId ).Select( b => b.ServingCount ).FirstOrDefault() : 0;

                    // save serving all count / weekend attendance
                    StepMeasureValue measureValueAll = new StepMeasureValue();
                    measureValueAll.StepMeasureId = _servingMeasureId;
                    measureValueAll.SundayDate = sundayDate;
                    measureValueAll.Value = servingCountAll;
                    measureValueAll.CampusId = campusId;
                    measureValueAll.WeekendAttendance = weekendAttendance;

                    stepMeasureValueService.Add( measureValueAll );

                    // save serving adult count / campus adult count
                    StepMeasureValue measureValueAdults = new StepMeasureValue();
                    measureValueAdults.StepMeasureId = _servingMeasureId;
                    measureValueAdults.SundayDate = sundayDate;
                    measureValueAdults.Value = servingCountAdults;
                    measureValueAdults.CampusId = campusId;
                    measureValueAdults.ActiveAdults = activeAdults;

                    stepMeasureValueService.Add( measureValueAdults );
                }

                rockContext.SaveChanges();

                foreach ( var pastor in pastorSummary )
                {
                    // get serving team memebers
                    var servingCount = new GroupMemberService( rockContext ).Queryable().AsNoTracking()
                                            .Where( m =>
                                                    pastor.FamilyIds.Contains( m.GroupId )
                                                    && servingMembers.Contains( m.PersonId )
                                                    && m.Person.RecordStatusValueId == _activeRecordStatusId )
                                            .Count();

                    StepMeasureValue measureValueAdults = new StepMeasureValue();
                    measureValueAdults.StepMeasureId = _servingMeasureId;
                    measureValueAdults.SundayDate = sundayDate;
                    measureValueAdults.Value = servingCount;
                    measureValueAdults.PastorPersonAliasId = pastor.PastorPersonAliasId;
                    measureValueAdults.ActiveAdults = pastor.AdultCount;

                    stepMeasureValueService.Add( measureValueAdults );
                }

                rockContext.SaveChanges();
            }
        }

        private void ProcessSharing( DateTime sundayDate, List<MetricValue> metricValues, List<CampusAdultCount> campusAdultCounts, List<PastorSummary> pastorSummary )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                StepMeasureValueService stepMeasureValueService = new StepMeasureValueService( rockContext );

                // not implemented but since it's active with IsTba we need values

                foreach ( int campusId in _campusIds )
                {
                    // get counts 
                    int weekendAttendance = metricValues.Where( m => m.EntityId == campusId ).Select( m => m.YValue ).FirstOrDefault() != null ? Decimal.ToInt16( metricValues.Where( m => m.EntityId == campusId ).Select( m => m.YValue ).FirstOrDefault().Value ) : 0;
                    int activeAdults = campusAdultCounts.Where( c => c.CampusId == campusId ).Select( c => c.AdultCount ).Count() != 0 ? campusAdultCounts.Where( c => c.CampusId == campusId ).Select( c => c.AdultCount ).FirstOrDefault() : 0;
                    int sharingCountAll =  0;
                    int sharingCountAdults = 0;

                    // save serving all count / weekend attendance
                    StepMeasureValue measureValueAll = new StepMeasureValue();
                    measureValueAll.StepMeasureId = _sharingMeasureId;
                    measureValueAll.SundayDate = sundayDate;
                    measureValueAll.Value = sharingCountAll;
                    measureValueAll.CampusId = campusId;
                    measureValueAll.WeekendAttendance = weekendAttendance;

                    stepMeasureValueService.Add( measureValueAll );

                    // save serving adult count / campus adult count
                    StepMeasureValue measureValueAdults = new StepMeasureValue();
                    measureValueAdults.StepMeasureId = _sharingMeasureId;
                    measureValueAdults.SundayDate = sundayDate;
                    measureValueAdults.Value = sharingCountAdults;
                    measureValueAdults.CampusId = campusId;
                    measureValueAdults.ActiveAdults = activeAdults;

                    stepMeasureValueService.Add( measureValueAdults );
                }

                rockContext.SaveChanges();

                foreach ( var pastor in pastorSummary )
                {
                    StepMeasureValue measureValueAdults = new StepMeasureValue();
                    measureValueAdults.StepMeasureId = _sharingMeasureId;
                    measureValueAdults.SundayDate = sundayDate;
                    measureValueAdults.Value = 0;
                    measureValueAdults.PastorPersonAliasId = pastor.PastorPersonAliasId;
                    measureValueAdults.ActiveAdults = pastor.AdultCount;

                    stepMeasureValueService.Add( measureValueAdults );
                }

                rockContext.SaveChanges();
            }
        }

        #region Utilities
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
        /// Pastor Summary
        /// </summary>
        public class PastorSummary
        {
            /// <summary>
            /// Gets or sets the pastor person alias identifier.
            /// </summary>
            /// <value>
            /// The pastor person alias identifier.
            /// </value>
            public int? PastorPersonAliasId { get; set; }
            /// <summary>
            /// Gets or sets the family ids.
            /// </summary>
            /// <value>
            /// The family ids.
            /// </value>
            public List<int> FamilyIds { get; set; }
            /// <summary>
            /// Gets or sets the adult count.
            /// </summary>
            /// <value>
            /// The adult count.
            /// </value>
            public int? AdultCount { get; set; }
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
            /// Gets or sets the family ids.
            /// </summary>
            /// <value>
            /// The family ids.
            /// </value>
            public List<int> FamilyIds { get; set; }
            /// <summary>
            /// Gets or sets the pastor person alias identifier.
            /// </summary>
            /// <value>
            /// The pastor person alias identifier.
            /// </value>
            public int? PastorPersonAliasId { get; set; }
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
            public int? AdultCount { get; set; }
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
