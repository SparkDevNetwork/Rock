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
            return GetConfigurationTemplates( RockContext )
                .OrderBy( t => t.Name )
                .Select( t => new ConfigurationTemplateBag
                {
                    Guid = t.Guid,
                    Name = t.Name,
                    IconCssClass = t.IconCssClass
                } )
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
            var areas = new Dictionary<Guid, ConfigurationAreaBag>();
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
                templates = GetConfigurationTemplates( RockContext ).ToList();
            }

            if ( kiosk != null )
            {
                kioskGroupTypeIds = new HashSet<int>( GetKioskAreas( kiosk ).Select( gt => gt.Id ) );
            }

            // Go through each template and get all areas that belong to
            // it. Then either add them to the list of areas or update the
            // primary template guids of the existing area.
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

                    if ( areas.TryGetValue( areaGroupType.Guid, out var area ) )
                    {
                        area.PrimaryTemplateGuids.Add( cfg.Guid );
                    }
                    else
                    {
                        areas.Add( areaGroupType.Guid, new ConfigurationAreaBag
                        {
                            Guid = areaGroupType.Guid,
                            Name = areaGroupType.Name,
                            PrimaryTemplateGuids = new List<Guid> { cfg.Guid }
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
        /// Creates the check in session that will be used for the specified template.
        /// </summary>
        /// <param name="templateConfiguration">The configuration template.</param>
        /// <returns>An instance of <see cref="CheckInSession"/>.</returns>
        public virtual CheckInSession CreateSession( TemplateConfigurationData templateConfiguration )
        {
            return new CheckInSession( this, templateConfiguration );
        }

        /// <summary>
        /// Gets the recent attendance for a set of people.
        /// </summary>
        /// <param name="cutoffDateTime">Attendance records must start on or after this date and time.</param>
        /// <param name="personGuids">The person unique identifiers to query the database for.</param>
        /// <param name="rockContext">The database context to execute the query on.</param>
        /// <returns>A collection of <see cref="RecentAttendance"/> records.</returns>
        internal static List<RecentAttendance> GetRecentAttendance( DateTime cutoffDateTime, IEnumerable<Guid> personGuids, RockContext rockContext )
        {
            var attendanceService = new AttendanceService( rockContext );

            var personAttendanceQuery = attendanceService
                .Queryable()
                .Where( a => a.PersonAlias != null
                    && a.Occurrence.Group != null
                    && a.Occurrence.Schedule != null
                    && a.StartDateTime >= cutoffDateTime
                    && a.DidAttend.HasValue
                    && a.DidAttend.Value == true );

            // TODO: This should probably be changed to a raw SQL query for performance.
            // Because the list of personGuids will be changing constantly it
            // will still not be cached by EF.
            personAttendanceQuery = WhereContains( personAttendanceQuery, personGuids, a => a.PersonAlias.Person.Guid );

            return personAttendanceQuery
                .Select( a => new RecentAttendance
                {
                    AttendanceId = a.Id,
                    AttendanceGuid = a.Guid,
                    Status = a.CheckInStatus,
                    StartDateTime = a.StartDateTime,
                    EndDateTime = a.EndDateTime,
                    PersonGuid = a.PersonAlias.Person.Guid,
                    GroupTypeGuid = a.Occurrence.Group.GroupType.Guid,
                    GroupGuid = a.Occurrence.Group.Guid,
                    LocationGuid = a.Occurrence.Location.Guid,
                    ScheduleGuid = a.Occurrence.Schedule.Guid,
                    CampusGuid = a.CampusId.HasValue ? a.Campus.Guid : ( Guid? ) null
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the current attendance for a set of locations.
        /// </summary>
        /// <param name="startDateTime">Attendance records must start on this date.</param>
        /// <param name="locationIds">The location identifiers to load attendance data for.</param>
        /// <param name="rockContext">The database context to execute the query on.</param>
        /// <returns>A collection of <see cref="RecentAttendance"/> records.</returns>
        internal static List<RecentAttendance> GetCurrentAttendance( DateTime startDateTime, IEnumerable<int> locationIds, RockContext rockContext )
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

            // TODO: This should probably be changed to a raw SQL query for performance.
            // Because the list of locationGuids will be changing constantly it
            // will still not be cached by EF.
            personAttendanceQuery = WhereContains( personAttendanceQuery, locationIds, a => a.Occurrence.Location.Id );

            return personAttendanceQuery
                .Select( a => new RecentAttendance
                {
                    AttendanceId = a.Id,
                    AttendanceGuid = a.Guid,
                    Status = a.CheckInStatus,
                    StartDateTime = a.StartDateTime,
                    EndDateTime = a.EndDateTime,
                    PersonGuid = a.PersonAlias.Person.Guid,
                    GroupTypeGuid = a.Occurrence.Group.GroupType.Guid,
                    GroupGuid = a.Occurrence.Group.Guid,
                    LocationGuid = a.Occurrence.Location.Guid,
                    ScheduleGuid = a.Occurrence.Schedule.Guid,
                    CampusGuid = a.CampusId.HasValue ? a.Campus.Guid : ( Guid? ) null
                } )
                .ToList();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Gets the group type areas that are valid for the kiosk device. Only group
        /// types associated via group and location to the kiosk will be returned.
        /// </summary>
        /// <param name="kiosk">The kiosk device.</param>
        /// <returns>An enumeration of <see cref="GroupTypeCache" /> objects.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="kiosk"/> is <c>null</c>.</exception>
        protected virtual IEnumerable<GroupTypeCache> GetKioskAreas( DeviceCache kiosk )
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
        /// <param name="rockContext">The rock context to use if database access is required.</param>
        /// <returns>An enumeration of <see cref="GroupTypeCache"/> objects.</returns>
        /// <exception cref="Exception">Check-in Template Purpose was not found in the database, please check your installation.</exception>
        protected virtual IEnumerable<GroupTypeCache> GetConfigurationTemplates( RockContext rockContext )
        {
            var checkinTemplateTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid(), rockContext )?.Id;

            if ( !checkinTemplateTypeId.HasValue )
            {
                throw new Exception( "Check-in Template Purpose was not found in the database, please check your installation." );
            }

            return GroupTypeCache.All( rockContext )
                .Where( t => t.GroupTypePurposeValueId.HasValue && t.GroupTypePurposeValueId == checkinTemplateTypeId.Value );
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
        internal static IQueryable<T> WhereContains<T, V>( IQueryable<T> source, IEnumerable<V> values, Expression<Func<T, V>> expression )
        {
            Expression<Func<T, bool>> predicate = null;
            var parameter = expression.Parameters[0];

            foreach ( var value in values )
            {
                var equalExpr = Expression.Equal( expression.Body, Expression.Constant( value ) );
                var lambdaExpr = Expression.Lambda<Func<T, bool>>( equalExpr, parameter );

                predicate = predicate != null
                    ? predicate.Or( lambdaExpr )
                    : lambdaExpr;
            }

            if ( predicate != null )
            {
                return source.Where( predicate );
            }
            else
            {
                return source.Where( a => false );
            }
        }

        #endregion
    }
}
