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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Rock.Data;
using Rock.Model;
using Rock.Observability;
using Rock.Utility;
using Rock.ViewModels.CheckIn;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// Primary entry point to the check-in system. This provides a single
    /// place to interface with check-in so that all logic is centralized
    /// and not duplicated.
    /// </summary>
    internal class CheckInDirector
    {
        #region Properties

        /// <summary>
        /// The context to use when accessing the database.
        /// </summary>
        /// <value>The database context.</value>
        public RockContext RockContext { get; }

        /// <summary>
        /// Gets the conversion provider to be used with this instance.
        /// </summary>
        /// <value>The conversion provider.</value>
        public virtual DefaultConversionProvider ConversionProvider { get; }

        /// <summary>
        /// The instance that will provide label rendering with this instance.
        /// </summary>
        public virtual DefaultLabelProvider LabelProvider { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInDirector"/> class.
        /// </summary>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="rockContext"/> is <c>null</c>.</exception>
        public CheckInDirector( RockContext rockContext )
        {
            if ( rockContext == null )
            {
                throw new ArgumentNullException( nameof( rockContext ) );
            }

            RockContext = rockContext;
            ConversionProvider = new DefaultConversionProvider( RockContext );
            LabelProvider = new DefaultLabelProvider( RockContext );
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the configuration template bags for all valid check-in
        /// configurations.
        /// </summary>
        /// <returns>A colleciton of <see cref="ConfigurationTemplateBag"/> objects.</returns>
        public virtual List<ConfigurationTemplateBag> GetConfigurationTemplateBags()
        {
            return GetConfigurationTemplates()
                .OrderBy( t => t.Name )
                .Select( GetConfigurationTemplateBag )
                .Where( c => c != null )
                .ToList();
        }

        /// <summary>
        /// Gets the check in area summary bags for all valid check-in areas. If
        /// a <paramref name="kiosk"/> or <paramref name="checkinTemplate"/> are
        /// provided then they will be used to filter the results to only areas
        /// valid for those items.
        /// </summary>
        /// <param name="kiosk">The optional kiosk to filter the results for.</param>
        /// <param name="checkinTemplate">The optional check-in template to filter all areas to.</param>
        /// <returns>A collection of <see cref="ConfigurationAreaBag"/> objects.</returns>
        public virtual List<ConfigurationAreaBag> GetCheckInAreaSummaries( DeviceCache kiosk, GroupTypeCache checkinTemplate )
        {
            var areas = new Dictionary<string, ConfigurationAreaBag>();
            List<GroupTypeCache> templates;
            HashSet<int> kioskGroupTypeIds = null;

            // If the caller specified a template, then we return areas for
            // only that primary template. Otherwise we include areas from
            // all templates.
            if ( checkinTemplate != null )
            {
                templates = new List<GroupTypeCache> { checkinTemplate };
            }
            else
            {
                templates = GetConfigurationTemplates().ToList();
            }

            if ( kiosk != null )
            {
                kioskGroupTypeIds = new HashSet<int>( GetKioskAreas( kiosk ).Select( gt => gt.Id ) );
            }

            // Go through each template and get all areas that belong to
            // it. Then either add them to the list of areas or update the
            // primary template ids of the existing area.
            foreach ( var cfg in templates )
            {
                foreach ( var areaGroupType in cfg.GetDescendentGroupTypes() )
                {
                    // Only include group types that actually take attendance.
                    if ( !areaGroupType.TakesAttendance )
                    {
                        continue;
                    }

                    // If a kiosk was specified, limit the results to areas
                    // that are valid for the kiosk.
                    if ( kioskGroupTypeIds != null && !kioskGroupTypeIds.Contains( areaGroupType.Id ) )
                    {
                        continue;
                    }

                    if ( areas.TryGetValue( areaGroupType.IdKey, out var area ) )
                    {
                        area.PrimaryTemplateIds.Add( cfg.IdKey );
                    }
                    else
                    {
                        areas.Add( areaGroupType.IdKey, new ConfigurationAreaBag
                        {
                            Id = areaGroupType.IdKey,
                            Name = areaGroupType.Name,
                            PrimaryTemplateIds = new List<string> { cfg.IdKey }
                        } );
                    }
                }
            }

            return new List<ConfigurationAreaBag>( areas.Values );
        }

        /// <summary>
        /// <para>
        /// Gets all the check-in opportunities that are possible for the kiosk or
        /// locations. 
        /// </para>
        /// <para>
        /// If you provide an array of locations they will be used, otherwise
        /// the locations of the kiosk will be used. If you provide a kiosk
        /// then it will be used to determine the current timestamp when
        /// checking if locations are open or not.
        /// </para>
        /// </summary>
        /// <param name="possibleAreas">The possible areas that are to be considered when generating the opportunities.</param>
        /// <param name="kiosk">The optional kiosk to use.</param>
        /// <param name="locations">The list of locations to use.</param>
        /// <returns>An instance of <see cref="OpportunityCollection"/> that describes the available opportunities.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="possibleAreas"/> is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="kiosk"/> - Kiosk must be specified unless locations are specified.</exception>
        public OpportunityCollection GetAllOpportunities( IReadOnlyCollection<GroupTypeCache> possibleAreas, DeviceCache kiosk, IReadOnlyCollection<NamedLocationCache> locations )
        {
            using ( var activity = ObservabilityHelper.StartActivity( "Get All Opportunities" ) )
            {
                if ( kiosk == null && locations == null )
                {
                    throw new ArgumentNullException( nameof( kiosk ), "Kiosk must be specified unless locations are specified." );
                }

                if ( possibleAreas == null )
                {
                    throw new ArgumentNullException( nameof( possibleAreas ) );
                }

                return OpportunityCollection.Create( possibleAreas, kiosk, locations, RockContext );
            }
        }

        /// <summary>
        /// <para>
        /// Gets the kiosk status given the check-in areas and location information.
        /// </para>
        /// <para>
        /// If you provide an array of locations they will be used, otherwise
        /// the locations of the kiosk will be used. If you provide a kiosk
        /// then it will be used to determine the current timestamp when
        /// checking if locations are open or not.
        /// </para>
        /// </summary>
        /// <param name="possibleAreas">The possible areas that are to be considered when generating the opportunities.</param>
        /// <param name="kiosk">The optional kiosk to use.</param>
        /// <param name="locations">The list of locations to use.</param>
        /// <returns>An instance of <see cref="KioskStatusBag"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="possibleAreas"/> is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="kiosk"/> - Kiosk must be specified unless locations are specified.</exception>
        public KioskStatusBag GetKioskStatus( IReadOnlyCollection<GroupTypeCache> possibleAreas, DeviceCache kiosk, IReadOnlyCollection<NamedLocationCache> locations )
        {
            using ( var activity = ObservabilityHelper.StartActivity( "Get Kiosk State" ) )
            {
                if ( kiosk == null && locations == null )
                {
                    throw new ArgumentNullException( nameof( kiosk ), "Kiosk must be specified unless locations are specified." );
                }

                if ( possibleAreas == null )
                {
                    throw new ArgumentNullException( nameof( possibleAreas ) );
                }

                // Get the primary campus for this kiosk.
                var kioskCampusId = kiosk?.GetCampusId();
                var kioskCampus = kioskCampusId.HasValue ? CampusCache.Get( kioskCampusId.Value, RockContext ) : null;

                // Get the current timestamp as well as today's date for filtering
                // in later logic.
                var now = kioskCampus?.CurrentDateTime ?? RockDateTime.Now;
                var serverNow = RockDateTime.Now;
                var today = now.Date;

                // Get all areas that don't have exclusions for today.
                var activeAreas = possibleAreas
                    .Where( a => !a.GroupScheduleExclusions.Any( e => today >= e.Start && today <= e.End ) )
                    .ToList();

                // Get all area identifiers as a HashSet for faster lookups.
                var activeAreaIds = new HashSet<int>( activeAreas.Select( a => a.Id ) );

                // If they did not provide a set of locations then get them from
                // the kiosk, including closed locations.
                if ( locations == null )
                {
                    locations = kiosk.GetAllLocations().ToList();
                }

                // Get all the group locations for active locations. This also
                // filters down to only groups in an active area.
                var activeGroupLocations = locations
                    .Where( l => l.IsActive )
                    .SelectMany( l => GroupLocationCache.AllForLocationId( l.Id ).Select( gl => new
                    {
                        GroupLocation = gl,
                        Location = l
                    } ) )
                    .DistinctBy( glc => glc.GroupLocation.Id )
                    .Where( glc => activeAreaIds.Contains( GroupCache.Get( glc.GroupLocation.GroupId, RockContext )?.GroupTypeId ?? 0 ) )
                    .ToList();

                // Get all the schedules that are associated with these locations.
                var schedules = activeGroupLocations
                    .SelectMany( gl => gl.GroupLocation.ScheduleIds )
                    .Distinct()
                    .Select( id => NamedScheduleCache.Get( id, RockContext ) )
                    .Where( s => s != null )
                    .ToList();

                // Find all active schedules with any check-in windows today.
                var validCheckInSchedules = schedules.Where( s => s.IsActive )
                    .Select( s => new
                    {
                        Schedule = s,
                        CheckInTimes = s.GetCheckInTimes( now )
                    } )
                    .Where( s => s.CheckInTimes.Any() )
                    .ToList();

                // Determine the next time in the future (today) that check-in will
                // open.
                var nextStartDateTime = validCheckInSchedules
                    .SelectMany( s => s.CheckInTimes )
                    .Where( s => s.CheckInStart > now )
                    .OrderBy( s => s.CheckInStart )
                    .FirstOrDefault()
                    ?.CheckInStart;

                // Determine the next time in the future that check-in will close.
                var nextStopDateTime = validCheckInSchedules
                    .SelectMany( s => s.CheckInTimes )
                    .Where( t => t.CheckInEnd > now )
                    .OrderBy( t => t.CheckInEnd )
                    .FirstOrDefault()
                    ?.CheckInEnd;

                // Determine if check-in is currently active at this moment.
                var isCheckInActive = validCheckInSchedules.SelectMany( s => s.CheckInTimes )
                    .Any( t => t.CheckInStart <= now && t.CheckInEnd > now );

                // Determine if the there are any locations that are open.
                var hasOpenLocations = activeGroupLocations.Any();

                return new KioskStatusBag
                {
                    IsCheckInActive = isCheckInActive,
                    HasOpenLocations = hasOpenLocations,
                    NextStartDateTime = nextStartDateTime,
                    NextStopDateTime = nextStopDateTime,
                    CampusCurrentDateTime = now,
                    ServerCurrentDateTime = serverNow,
                    LocationIds = locations.Select( l => l.IdKey ).ToList(),
                    ScheduleIds = schedules.Select( s => s.IdKey ).ToList()
                };
            }
        }

        /// <summary>
        /// Creates the check in session that will be used for the specified template.
        /// </summary>
        /// <param name="templateConfiguration">The configuration template.</param>
        /// <returns>An instance of <see cref="CheckInSession"/>.</returns>
        public virtual CheckInSession CreateSession( TemplateConfigurationData templateConfiguration )
        {
            return new CheckInSession( this, templateConfiguration );
        }

        /// <summary>
        /// Gets the recent attendance for a set of people. This does not
        /// include pending attendance records.
        /// </summary>
        /// <param name="cutoffDateTime">Attendance records must start on or after this date and time.</param>
        /// <param name="personIds">The person identifiers to query the database for.</param>
        /// <param name="rockContext">The database context to execute the query on.</param>
        /// <returns>A collection of <see cref="RecentAttendance"/> records.</returns>
        public static List<RecentAttendance> GetRecentAttendance( DateTime cutoffDateTime, IReadOnlyList<string> personIds, RockContext rockContext )
        {
            var attendanceService = new AttendanceService( rockContext );

            var personAttendanceQuery = attendanceService
                .Queryable()
                .Where( a => a.PersonAlias != null
                    && a.Occurrence.Group != null
                    && a.Occurrence.Schedule != null
                    && a.StartDateTime >= cutoffDateTime
                    && a.DidAttend.HasValue
                    && a.DidAttend.Value == true
                    && a.CheckInStatus != Enums.Event.CheckInStatus.Pending );

            var personIdNumbers = personIds
                .Select( a => IdHasher.Instance.GetId( a ) )
                .Where( a => a.HasValue )
                .Select( a => a.Value )
                .ToList();

            personAttendanceQuery = WhereContains( personAttendanceQuery, personIdNumbers, a => a.PersonAlias.PersonId );

            return personAttendanceQuery
                .Select( a => new
                {
                    AttendanceId = a.Id,
                    Status = a.CheckInStatus,
                    a.StartDateTime,
                    a.EndDateTime,
                    PersonId = a.PersonAlias.Person.Id,
                    GroupTypeId = a.Occurrence.Group.GroupType.Id,
                    GroupId = a.Occurrence.Group.Id,
                    LocationId = a.Occurrence.Location.Id,
                    ScheduleId = a.Occurrence.Schedule.Id,
                    CampusId = a.CampusId.HasValue ? a.CampusId.Value : ( int? ) null
                } )
                .ToList()
                .Select( a => new RecentAttendance
                {
                    AttendanceId = IdHasher.Instance.GetHash( a.AttendanceId ),
                    Status = a.Status,
                    StartDateTime = a.StartDateTime,
                    EndDateTime = a.EndDateTime,
                    PersonId = IdHasher.Instance.GetHash( a.PersonId ),
                    GroupTypeId = IdHasher.Instance.GetHash( a.GroupTypeId ),
                    GroupId = IdHasher.Instance.GetHash( a.GroupId ),
                    LocationId = IdHasher.Instance.GetHash( a.LocationId ),
                    ScheduleId = IdHasher.Instance.GetHash( a.ScheduleId ),
                    CampusId = a.CampusId.HasValue
                        ? IdHasher.Instance.GetHash( a.CampusId.Value )
                        : null
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the current attendance for a set of locations. This includes
        /// pending attendance records.
        /// </summary>
        /// <param name="startDateTime">Attendance records must start on this date.</param>
        /// <param name="locationIds">The location identifiers to load attendance data for.</param>
        /// <param name="rockContext">The database context to execute the query on.</param>
        /// <returns>A collection of <see cref="RecentAttendance"/> records.</returns>
        public static List<RecentAttendance> GetCurrentAttendance( DateTime startDateTime, IReadOnlyList<int> locationIds, RockContext rockContext )
        {
            var attendanceService = new AttendanceService( rockContext );

            var personAttendanceQuery = attendanceService.Queryable()
                .Where( a =>
                    a.Occurrence.OccurrenceDate == startDateTime.Date
                    && a.Occurrence.LocationId.HasValue
                    && a.Occurrence.GroupId.HasValue
                    && a.Occurrence.ScheduleId.HasValue
                    && a.PersonAliasId.HasValue
                    && a.DidAttend.HasValue
                    && a.DidAttend.Value == true
                    && !a.EndDateTime.HasValue );

            personAttendanceQuery = WhereContains( personAttendanceQuery, locationIds, a => a.Occurrence.Location.Id );

            return personAttendanceQuery
                .Select( a => new
                {
                    AttendanceId = a.Id,
                    Status = a.CheckInStatus,
                    a.StartDateTime,
                    a.EndDateTime,
                    PersonId = a.PersonAlias.Person.Id,
                    GroupTypeId = a.Occurrence.Group.GroupType.Id,
                    GroupId = a.Occurrence.Group.Id,
                    LocationId = a.Occurrence.Location.Id,
                    ScheduleId = a.Occurrence.Schedule.Id,
                    CampusId = a.CampusId.HasValue ? a.CampusId.Value : ( int? ) null
                } )
                .ToList()
                .Select( a => new RecentAttendance
                {
                    AttendanceId = IdHasher.Instance.GetHash( a.AttendanceId ),
                    Status = a.Status,
                    StartDateTime = a.StartDateTime,
                    EndDateTime = a.EndDateTime,
                    PersonId = IdHasher.Instance.GetHash( a.PersonId ),
                    GroupTypeId = IdHasher.Instance.GetHash( a.GroupTypeId ),
                    GroupId = IdHasher.Instance.GetHash( a.GroupId ),
                    LocationId = IdHasher.Instance.GetHash( a.LocationId ),
                    ScheduleId = IdHasher.Instance.GetHash( a.ScheduleId ),
                    CampusId = a.CampusId.HasValue
                        ? IdHasher.Instance.GetHash( a.CampusId.Value )
                        : null
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the group type areas that are valid for the kiosk device. Only group
        /// types associated via group and location to the kiosk will be returned.
        /// </summary>
        /// <param name="kiosk">The kiosk device.</param>
        /// <returns>An enumeration of <see cref="GroupTypeCache" /> objects.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="kiosk"/> is <c>null</c>.</exception>
        public virtual ICollection<GroupTypeCache> GetKioskAreas( DeviceCache kiosk )
        {
            if ( kiosk == null )
            {
                throw new ArgumentNullException( nameof( kiosk ) );
            }

            // Get all locations for the device.
            var locationIds = new HashSet<int>( kiosk.GetAllLocationIds() );

            // Get all the group locations associated with those locations.
            var groupLocations = locationIds
                .SelectMany( id => GroupLocationCache.AllForLocationId( id, RockContext ) )
                .DistinctBy( glc => glc.Id )
                .ToList();

            // Get the distinct group types for those group locations that have
            // attendance enabled.
            return groupLocations
                .Select( gl => GroupCache.Get( gl.GroupId, RockContext )?.GroupTypeId )
                .Where( id => id.HasValue )
                .Distinct()
                .Select( id => GroupTypeCache.Get( id.Value, RockContext ) )
                .Where( gt => gt != null && gt.TakesAttendance )
                .ToList();
        }

        /// <summary>
        /// Gets the configuration template group types that are defined in the system.
        /// </summary>
        /// <returns>An enumeration of <see cref="GroupTypeCache"/> objects.</returns>
        /// <exception cref="Exception">Check-in Template Purpose was not found in the database, please check your installation.</exception>
        public virtual IEnumerable<GroupTypeCache> GetConfigurationTemplates()
        {
            var checkinTemplateTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid(), RockContext )?.Id;

            if ( !checkinTemplateTypeId.HasValue )
            {
                throw new Exception( "Check-in Template Purpose was not found in the database, please check your installation." );
            }

            return GroupTypeCache.All( RockContext )
                .Where( t => t.GroupTypePurposeValueId.HasValue && t.GroupTypePurposeValueId == checkinTemplateTypeId.Value );
        }

        /// <summary>
        /// Gets the configuration template bag from the group type.
        /// </summary>
        /// <param name="groupType">The group type that represents the configuration template.</param>
        /// <returns>An instance of <see cref="ConfigurationTemplateBag"/> if <paramref name="groupType"/> was a check-in template; otherwise <c>null</c>.</returns>
        public ConfigurationTemplateBag GetConfigurationTemplateBag( GroupTypeCache groupType )
        {
            var configuration = groupType.GetCheckInConfiguration( RockContext );

            if ( configuration == null )
            {
                return null;
            }

            return new ConfigurationTemplateBag
            {
                Id = groupType.IdKey,
                Name = groupType.Name,
                IconCssClass = groupType.IconCssClass,
                AbilityLevelDetermination = configuration.AbilityLevelDetermination,
                KioskCheckInType = configuration.KioskCheckInType,
                FamilySearchType = configuration.FamilySearchType,
                IsAutoSelect = configuration.KioskCheckInType == Enums.CheckIn.KioskCheckInMode.Family
                    && configuration.AutoSelect == Enums.CheckIn.AutoSelectMode.PeopleAndAreaGroupLocation,
                IsLocationCountDisplayed = configuration.IsLocationCountDisplayed,
                IsOverrideAvailable = configuration.IsOverrideAvailable,
                IsPhotoHidden = configuration.IsPhotoHidden,
                IsSupervisorEnabled = configuration.IsSupervisorEnabled,
                MaximumPhoneNumberLength = configuration.MaximumPhoneNumberLength,
                MinimumPhoneNumberLength = configuration.MinimumPhoneNumberLength,
                PhoneSearchType = configuration.PhoneSearchType
            };
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// <para>
        /// Adds a where clause that can replicates a Contains() call on the
        /// values. If your LINQ statement has a real Contains() call then it
        /// will not be cached by EF - meaning EF will generate the SQL each
        /// time instead of using a cached SQL statement. This is very costly at
        /// about 15-20ms or more each time this happens.
        /// </para>
        /// <para>
        /// This method will do the same but generate individual x == 1 OR x == 2
        /// statements - which do get translated to an IN statement in SQL.
        /// </para>
        /// <para>
        /// Because the EF cache will be no good if any of the values in the
        /// clause change, this method is only helpful if <paramref name="values"/>
        /// is fairly consistent. If it is going to change with nearly every
        /// query then this does not provide any performance improvement.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of queryable.</typeparam>
        /// <typeparam name="V">The type of the value to be checked.</typeparam>
        /// <param name="source">The source queryable.</param>
        /// <param name="values">The values that <paramref name="expression"/> must match one of.</param>
        /// <param name="expression">The expression to the property.</param>
        /// <returns>A new queryable with the updated where clause.</returns>
        internal static IQueryable<T> WhereContains<T, V>( IQueryable<T> source, IReadOnlyList<V> values, Expression<Func<T, V>> expression )
        {
            Expression<Func<T, bool>> predicate = null;
            var parameter = expression.Parameters[0];

            if ( values.Count == 0 )
            {
                return source.Where( a => false );
            }

            if ( values.Count <= 5 )
            {
                /*
                     2024-06-26 - DSH

                     This behaves the same way as the C# compiler. When you
                     write something like this:
                       var myValue = 5;
                       list.Where( a => a.Id == myValue )
                     it gets translated into something like this:
                       private sealed class CompilerGenerated
                       {
                           public int myValue;
                       }
                       var compilerGenerated = new CompilerGenerated();
                       compilerGenerated.myValue = 5;
                       list.Where( Expression.Field( Expression.Constant( compilerGenerated ), "myValue" ) )
                     
                     So we are doing the same in a somewhat dynamic way. This makes
                     the expression cacheable by entity framework so it doesn't need
                     to rebuild the SQL every time. It also makes these values
                     parameters so SQL can re-use the same query plan.
                     
                     NOTE: We limit to 5 because that is fairly safe for check-in.
                     It is unlikely to need more than 5 achievement types, or
                     person ids, etc. in a Where() check. There are two reasons
                     for the limit:
                     
                     1. We have a limited number of SQL parameters in a query. We
                        don't want to use them all up here and then have the query
                        blow up on the developer when they add an unrelated where
                        clause somewhere else.
                     2. The cache will only be valid for the same number of parameters.
                        So using this on up to 5 values means we could fill up 5 slots
                        in the EF and SQL cache, since EF especially is pretty limited
                        we don't want to waste cache slots.
                 
                 */

                var valueHelper = new WhereContainsValues<V>();
                var valueHelperExpr = Expression.Constant( valueHelper );

                for (int i = 0; i < values.Count; i++ )
                {
                    Expression valuePropExpr;

                    if ( i == 0 )
                    {
                        valueHelper.P0 = values[0];
                        valuePropExpr = Expression.Field( valueHelperExpr, "P0" );
                    }
                    else if ( i == 1 )
                    {
                        valueHelper.P1 = values[1];
                        valuePropExpr = Expression.Field( valueHelperExpr, "P1" );
                    }
                    else if ( i == 2 )
                    {
                        valueHelper.P2 = values[2];
                        valuePropExpr = Expression.Field( valueHelperExpr, "P2" );
                    }
                    else if ( i == 3 )
                    {
                        valueHelper.P3 = values[3];
                        valuePropExpr = Expression.Field( valueHelperExpr, "P3" );
                    }
                    else
                    {
                        valueHelper.P4 = values[4];
                        valuePropExpr = Expression.Field( valueHelperExpr, "P4" );
                    }

                    var equalExpr = Expression.Equal( expression.Body, valuePropExpr );
                    var lambdaExpr = Expression.Lambda<Func<T, bool>>( equalExpr, parameter );

                    predicate = predicate != null
                        ? predicate.Or( lambdaExpr )
                        : lambdaExpr;
                }
            }
            else
            {
                // If we have more than 5 values just build a query with
                // constant values.
                foreach ( var value in values )
                {
                    var equalExpr = Expression.Equal( expression.Body, Expression.Constant( value ) );
                    var lambdaExpr = Expression.Lambda<Func<T, bool>>( equalExpr, parameter );

                    predicate = predicate != null
                        ? predicate.Or( lambdaExpr )
                        : lambdaExpr;
                }
            }

            return source.Where( predicate );
        }

        private class WhereContainsValues<T>
        {
            public T P0;

            public T P1;

            public T P2;

            public T P3;

            public T P4;
        }

        #endregion
    }
}
