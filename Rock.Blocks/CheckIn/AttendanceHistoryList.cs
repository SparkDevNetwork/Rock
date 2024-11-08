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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.CheckIn.AttendanceHistoryList;
using Rock.ViewModels.Utility;
using Rock.Web.UI;

namespace Rock.Blocks.CheckIn
{
    /// <summary>
    /// Displays a list of attendances.
    /// </summary>
    [DisplayName( "Attendance History" )]
    [Category( "Check-in" )]
    [Description( "Block for displaying the attendance history of a person or a group." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [BooleanField( "Filter Attendance By Default",
        Key = AttributeKey.FilterAttendanceByDefault,
        Description = "Sets the default display of Attended to Did Attend instead of [All]",
        DefaultBooleanValue = false )]

    [SystemGuid.EntityTypeGuid( "8b678dc2-25e0-4589-bc3e-765be9729bc8" )]
    [SystemGuid.BlockTypeGuid( "68d2abbc-3c43-4450-973f-071d1715c0c9" )]
    [CustomizedGrid]
    [ContextAware]
    public class AttendanceHistoryList : RockEntityListBlockType<Attendance>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string FilterAttendanceByDefault = "FilterAttendanceByDefault";
        }

        private static class PreferenceKey
        {
            public const string FilterDateRangeStart = "filter-date-range-start";
            public const string FilterDateRangeEnd = "filter-date-range-end";
            public const string FilterGroup = "filter-group";
            public const string FilterPerson = "filter-person";
            public const string FilterSchedule = "filter-schedule";
            public const string FilterDidAttend = "filter-did-attend";
        }

        #endregion Keys

        #region Fields

        private Person _person;
        private Rock.Model.Group _group;
        private List<CheckinAreaPath> _checkInAreaPaths;
        private Dictionary<int, string> _locationPaths;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the date from which to filter the results.
        /// </summary>
        /// <value>
        /// The filter date range start.
        /// </value>
        protected DateTime? FilterDateRangeStart => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterDateRangeStart )
            .AsDateTime();

        /// <summary>
        /// Gets the date to which to filter the results.
        /// </summary>
        /// <value>
        /// The filter date range end.
        /// </value>
        protected DateTime? FilterDateRangeEnd => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterDateRangeEnd )
            .AsDateTime();

        /// <summary>
        /// Gets the group the results should be associated with.
        /// </summary>
        /// <value>
        /// The group filter.
        /// </value>
        protected Guid? FilterGroup => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterGroup )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        /// <summary>
        /// Gets the person the results should be associated with.
        /// </summary>
        /// <value>
        /// The person filter.
        /// </value>
        protected Guid? FilterPerson => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterPerson )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        /// <summary>
        /// Gets the schedule of occurrences with which the results should be filtered by.
        /// </summary>
        /// <value>
        /// The filter schedule.
        /// </value>
        protected Guid? FilterSchedule => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterSchedule )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        /// <summary>
        /// Gets the filter indicating whether only results with DidAttend equal true or not.
        /// </summary>
        /// <value>
        /// The DidAttend filter.
        /// </value>
        protected string FilterDidAttend => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterDidAttend );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<AttendanceHistoryListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private AttendanceHistoryListOptionsBag GetBoxOptions()
        {
            var isValidContext = InitializeContextEntities();

            var options = new AttendanceHistoryListOptionsBag()
            {
                IsGroupColumnVisible = _group == null,
                IsPersonColumnVisible = _person == null,
                IsValidContextEntity = isValidContext,
                GroupItems = GetGroupItems(),
                FilterAttendanceByDefault = GetAttributeValue( AttributeKey.FilterAttendanceByDefault ).AsBoolean()
            };

            return options;
        }

        /// <summary>
        /// Gets the list of groups to potentially filter from.
        /// </summary>
        private List<ListItemBag> GetGroupItems()
        {
            var items = new List<ListItemBag>();

            using ( var rockContext = new RockContext() )
            {
                if ( _group == null && _person != null )
                {
                    // only list groups that this person has attended before
                    var groupIdsAttended = new AttendanceService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( a =>
                            a.PersonAlias != null &&
                            a.PersonAlias.PersonId == _person.Id )
                        .Select( a => a.Occurrence.GroupId )
                        .Distinct()
                        .ToList();

                    foreach ( var group in new GroupService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( g => groupIdsAttended.Contains( g.Id ) )
                        .OrderBy( g => g.Name )
                        .Select( g => new { g.Name, g.Guid } ).ToList() )
                    {
                        items.Add( new ListItemBag() { Text = group.Name, Value = group.Guid.ToString() } );
                    }
                }
            }

           return items;
        }

        /// <inheritdoc/>
        protected override IQueryable<Attendance> GetListQueryable( RockContext rockContext )
        {
            var attendanceService = new AttendanceService( rockContext );
            var queryable = attendanceService.Queryable();
            InitializeContextEntities();

            if ( _person != null )
            {
                queryable = queryable.Where( a => a.PersonAlias.PersonId == _person.Id );

                if ( _group != null )
                {
                    queryable = queryable.Where( a => a.Occurrence.GroupId == _group.Id );
                }
                else
                {
                    if ( FilterGroup.HasValue )
                    {
                        queryable = queryable.Where( a => a.Occurrence.Group.Guid == FilterGroup.Value );
                    }
                }
            }
            else
            {
                if ( FilterPerson.HasValue )
                {
                    queryable = queryable.Where( a => a.PersonAlias.Person.Guid == FilterPerson.Value );
                }
            }

            // Filter by Date Range
            if ( FilterDateRangeStart.HasValue )
            {
                queryable = queryable.Where( t => t.StartDateTime >= FilterDateRangeStart.Value );
            }
            if ( FilterDateRangeEnd.HasValue )
            {
                var upperDate = FilterDateRangeEnd.Value.Date.AddDays( 1 );
                queryable = queryable.Where( t => t.StartDateTime < upperDate );
            }

            // Filter by Schedule
            if ( FilterSchedule.HasValue )
            {
                queryable = queryable.Where( h => h.Occurrence.Schedule.Guid == FilterSchedule.Value );
            }

            // Filter by DidAttend
            if ( FilterDidAttend.IsNotNullOrWhiteSpace() )
            {
                var didAttend = FilterDidAttend.AsBoolean();
                queryable = queryable.Where( a => a.DidAttend == didAttend );
            }
            else if ( GetAttributeValue( AttributeKey.FilterAttendanceByDefault ).AsBoolean() )
            {
                queryable = queryable.Where( a => a.DidAttend == true );
            }

            return queryable;
        }

        /// <inheritdoc/>
        protected override IQueryable<Attendance> GetOrderedListQueryable( IQueryable<Attendance> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( a => a.StartDateTime );
        }

        /// <inheritdoc/>
        protected override List<Attendance> GetListItems( IQueryable<Attendance> queryable, RockContext rockContext )
        {
            var listItems = base.GetListItems( queryable, rockContext );

            // Filter out attendance records where the current user does not have View permission for the Group.
            var securedAttendanceItems = listItems
                .AsEnumerable()
                .Where( a => ( a.Occurrence.Group?.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) == true ) || a.Occurrence.Group == null )
                .ToList();

            // build a lookup for _checkInAreaPaths
            _checkInAreaPaths = new GroupTypeService( rockContext ).GetAllCheckinAreaPaths().ToList();

            // build a lookup for _locationPaths
            var locationIdList = securedAttendanceItems.Select( a => a.Occurrence.LocationId )
                .Distinct()
                .ToList();

            _locationPaths = new Dictionary<int, string>();
            var qryLocations = new LocationService( rockContext )
                .Queryable()
                .Where( l => locationIdList.Contains( l.Id ) );

            foreach ( var location in qryLocations )
            {
                var parentLocation = location.ParentLocation;
                var locationNames = new List<string>();
                while ( parentLocation != null )
                {
                    locationNames.Add( parentLocation.Name );
                    parentLocation = parentLocation.ParentLocation;
                }

                string locationPath = string.Empty;
                if ( locationNames.Any() )
                {
                    locationNames.Reverse();
                    locationPath = locationNames.AsDelimited( " > " );
                }

                _locationPaths.TryAdd( location.Id, locationPath );
            }

            return listItems;
        }

        /// <inheritdoc/>
        protected override GridBuilder<Attendance> GetGridBuilder()
        {
            return new GridBuilder<Attendance>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "location", a => a.Occurrence.Location?.Name ?? "" )
                .AddTextField( "locationPath", a => GetLocationPath( a.Occurrence.LocationId ) )
                .AddTextField( "campus", a => a.Campus?.Name ?? "" )
                .AddTextField( "schedule", a => a.Occurrence.Schedule?.Name ?? "" )
                .AddTextField( "groupName", a => a.Occurrence.Group?.Name ?? "" )
                .AddTextField( "checkInAreaPath", a => CheckInAreaPath( a.Occurrence.Group?.GroupTypeId ) )
                .AddPersonField( "person", a => a.PersonAlias?.Person )
                .AddDateTimeField( "startDateTime", a => a.StartDateTime )
                .AddDateTimeField( "endDateTime", a => a.EndDateTime )
                .AddField( "didAttend", a => a.DidAttend );
        }

        /// <summary>
        /// Gets the location path from the cached <see cref="_locationPaths"/>
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <returns></returns>
        private string GetLocationPath( int? locationId )
        {
            return locationId.HasValue && _locationPaths.ContainsKey( locationId.Value ) ? _locationPaths[locationId.Value] : string.Empty;
        }

        /// <summary>
        /// Gets the CheckInAreaPath path from the cached <see cref="_checkInAreaPaths"/>
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        private string CheckInAreaPath( int? groupTypeId )
        {
            string groupTypePath = null;

            if ( groupTypeId.HasValue )
            {
                var path = _checkInAreaPaths?.Find( a => a.GroupTypeId == groupTypeId.Value );
                if ( path != null )
                {
                    groupTypePath = path.Path;
                }
            }

            return groupTypePath;
        }

        /// <summary>
        /// Initializes the context entities and returns a boolean value indicating whether or not the block has a valid context entity
        /// based on the configuration of the <see cref="Rock.Web.UI.ContextAwareAttribute"/> attribute.
        /// </summary>
        /// <returns></returns>
        private bool InitializeContextEntities()
        {
            var contextEntityType = GetContextEntityType();
            if ( contextEntityType == typeof( Person ) )
            {
                _person = RequestContext.GetContextEntity<Person>();
                return _person != null;
            }
            else if ( contextEntityType == typeof( Rock.Model.Group ) )
            {
                _group = RequestContext.GetContextEntity<Rock.Model.Group>();
                return _group != null;
            }

            return true;
        }

        #endregion
    }
}
