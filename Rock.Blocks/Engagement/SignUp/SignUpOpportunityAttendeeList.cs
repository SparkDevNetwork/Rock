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
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.SignUp.SignUpOpportunityAttendeeList;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement.SignUp
{
    /// <summary>
    /// Displays the attendees for a sign-up project opportunity.
    /// </summary>
    [DisplayName( "Sign-Up Opportunity Attendee List" )]
    [Category( "Engagement > Sign-Up" )]
    [Description( "Lists all the group members for the selected group, location and schedule." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Group Member Detail Page",
        Key = AttributeKey.GroupMemberDetailPage,
        Description = "Page used for viewing an attendee's group member detail for this Sign-Up project. Clicking a row in the grid will take you to this page.",
        IsRequired = true,
        Order = 0 )]

    [LinkedPage( "Person Profile Page",
        Key = AttributeKey.PersonProfilePage,
        Description = "Page used for viewing a person's profile. If set, a view profile button will show for each group member.",
        IsRequired = false,
        Order = 1 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "2B2B9FF8-96EA-4565-85D7-E6E50C5219FE" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "9038C76C-2815-4C9F-B31C-1CB5C1196450" )]
    [Rock.SystemGuid.BlockTypeGuid( "EE652767-5070-4EAB-8BB7-BB254DD01B46" )]
    [CustomizedGrid]
    public class SignUpOpportunityAttendeeList : RockListBlockType<SignUpOpportunityAttendeeList.AttendeeRow>, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string GroupMemberId = "GroupMemberId";
            public const string LocationId = "LocationId";
            public const string PersonId = "PersonId";
            public const string ScheduleId = "ScheduleId";
        }

        private static class AttributeKey
        {
            public const string GroupMemberDetailPage = "GroupMemberDetailPage";
            public const string PersonProfilePage = "PersonProfilePage";
        }

        private static class NavigationUrlKey
        {
            public const string GroupMemberDetailPage = "GroupMemberDetailPage";
            public const string PersonProfilePage = "PersonProfilePage";
        }

        private static class PreferenceKey
        {
            public const string FilterCampus = "filter-campus";
            public const string FilterGender = "filter-gender";
        }

        #endregion Keys

        #region Fields

        private int? _groupId;
        private int? _locationId;
        private int? _scheduleId;

        private Rock.Model.Group _group;

        private GroupLocation _groupLocation;

        private string _validationErrorMessage;
        private bool _isValidationErrorMessageBuilt;

        private List<AttributeCache> _memberAttributes;
        private List<AttributeCache> _opportunityAttributes;

        private PersonPreferenceCollection _personPreferences;

        private HashSet<int> _groupMemberIdsWithGroupHistory = new HashSet<int>();
        private HashSet<int> _groupSyncRoleIds = new HashSet<int>();
        private Dictionary<int, List<string>> _unmetRequirementNamesByGroupMemberId = new Dictionary<int, List<string>>();
        private Dictionary<int, string> _homePhoneByPersonId = new Dictionary<int, string>();
        private Dictionary<int, string> _cellPhoneByPersonId = new Dictionary<int, string>();
        private Dictionary<int, Location> _homeLocationByPersonId = new Dictionary<int, Location>();

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the group (project) identifier resolved from the page parameter.
        /// </summary>
        private int GroupId
        {
            get
            {
                if ( !_groupId.HasValue )
                {
                    _groupId = new GroupService( RockContext )
                        .GetSelect( PageParameter( PageParameterKey.GroupId ), g => g.Id, !PageCache.Layout.Site.DisablePredictableIds );
                }

                return _groupId.Value;
            }
        }

        /// <summary>
        /// Gets the location identifier resolved from the page parameter.
        /// </summary>
        private int LocationId
        {
            get
            {
                if ( !_locationId.HasValue )
                {
                    _locationId = new LocationService( RockContext )
                        .GetSelect( PageParameter( PageParameterKey.LocationId ), l => l.Id, !PageCache.Layout.Site.DisablePredictableIds );
                }

                return _locationId.Value;
            }
        }

        /// <summary>
        /// Gets the schedule identifier resolved from the page parameter.
        /// </summary>
        private int ScheduleId
        {
            get
            {
                if ( !_scheduleId.HasValue )
                {
                    _scheduleId = new ScheduleService( RockContext )
                        .GetSelect( PageParameter( PageParameterKey.ScheduleId ), s => s.Id, !PageCache.Layout.Site.DisablePredictableIds );
                }

                return _scheduleId.Value;
            }
        }

        /// <summary>
        /// Gets the prefix applied to person preference keys so saved filters are scoped
        /// to a single opportunity instead of being shared by every opportunity viewed
        /// on this page.
        /// </summary>
        private string PreferenceKeyPrefix => $"{GroupId}-{LocationId}-{ScheduleId}-";

        /// <summary>
        /// Gets the person preferences for this block.
        /// </summary>
        private PersonPreferenceCollection PersonPreferences
        {
            get
            {
                if ( _personPreferences == null )
                {
                    _personPreferences = GetBlockPersonPreferences();
                }

                return _personPreferences;
            }
        }

        /// <summary>
        /// Gets the campus identifier of the "Family Campus" filter, or <c>null</c> when
        /// the filter is not set.
        /// </summary>
        private int? FilterCampusId
        {
            get
            {
                var campusGuid = PersonPreferences
                    .GetValue( $"{PreferenceKeyPrefix}{PreferenceKey.FilterCampus}" )
                    .FromJsonOrNull<ListItemBag>()
                    ?.Value
                    ?.AsGuidOrNull();

                return campusGuid.HasValue
                    ? CampusCache.Get( campusGuid.Value )?.Id
                    : null;
            }
        }

        /// <summary>
        /// Gets the genders selected in the "Gender" filter.
        /// </summary>
        private List<Gender> FilterGenders
        {
            get
            {
                var value = PersonPreferences.GetValue( $"{PreferenceKeyPrefix}{PreferenceKey.FilterGender}" ) ?? string.Empty;

                return value
                    .Split( new[] { '|' }, StringSplitOptions.RemoveEmptyEntries )
                    .Select( g => g.ConvertToEnumOrNull<Gender>() )
                    .Where( g => g.HasValue )
                    .Select( g => g.Value )
                    .ToList();
            }
        }

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<SignUpOpportunityAttendeeListOptionsBag>();
            var builder = GetGridBuilder();

            var canManageMembers = CanManageMembers();

            box.IsAddEnabled = canManageMembers;
            box.IsDeleteEnabled = canManageMembers;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the block.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private SignUpOpportunityAttendeeListOptionsBag GetBoxOptions()
        {
            var options = new SignUpOpportunityAttendeeListOptionsBag();

            var errorMessage = GetValidationErrorMessage();
            if ( errorMessage.IsNotNullOrWhiteSpace() )
            {
                options.ErrorMessage = errorMessage;
                return options;
            }

            var group = GetGroup();
            var groupLocation = GetGroupLocation();
            var schedule = groupLocation.Schedules.First( s => s.Id == ScheduleId );
            var config = groupLocation.GroupLocationScheduleConfigs.FirstOrDefault( c => c.ScheduleId == ScheduleId );

            options.OpportunityName = GetOpportunityName( group, schedule, config );
            options.LocationName = groupLocation.Location.ToString();
            options.FriendlySchedule = schedule.ToFriendlyScheduleText( true );
            options.SlotsMinimum = config?.MinimumCapacity;
            options.SlotsDesired = config?.DesiredCapacity;
            options.SlotsMaximum = config?.MaximumCapacity;
            options.SlotsFilled = GetSlotsFilledCount();
            options.GroupTypeName = group.GroupType?.Name;
            options.CampusName = group.Campus?.Name;
            options.IsGroupInactive = !group.IsActive;
            options.PreferenceKeyPrefix = PreferenceKeyPrefix;
            options.ProjectName = group.Name;

            return options;
        }

        /// <summary>
        /// Gets the display name for this opportunity. Preference is given to the name
        /// provided at the opportunity (schedule configuration) level, falling back to
        /// the name provided at the project (group) level. When the schedule is a named
        /// schedule its name is appended.
        /// </summary>
        /// <param name="group">The project (group) this opportunity belongs to.</param>
        /// <param name="schedule">The opportunity's schedule.</param>
        /// <param name="config">The opportunity's schedule configuration, if one exists.</param>
        /// <returns>The display name for this opportunity.</returns>
        private string GetOpportunityName( Rock.Model.Group group, Schedule schedule, GroupLocationScheduleConfig config )
        {
            var title = config?.ConfigurationName.IsNotNullOrWhiteSpace() == true
                ? config.ConfigurationName
                : group?.Name;

            var scheduleName = schedule?.ScheduleType == ScheduleType.Named && schedule.Name.IsNotNullOrWhiteSpace()
                ? schedule.Name
                : string.Empty;

            var separator = title.IsNotNullOrWhiteSpace() && scheduleName.IsNotNullOrWhiteSpace()
                ? " - "
                : string.Empty;

            return $"{title}{separator}{scheduleName}";
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.GroupMemberDetailPage] = this.GetLinkedPageUrl( AttributeKey.GroupMemberDetailPage, new Dictionary<string, string>
                {
                    { PageParameterKey.GroupId, GroupId.ToString() },
                    { PageParameterKey.LocationId, LocationId.ToString() },
                    { PageParameterKey.ScheduleId, ScheduleId.ToString() },
                    { PageParameterKey.GroupMemberId, "((Key))" }
                } ),
                [NavigationUrlKey.PersonProfilePage] = this.GetLinkedPageUrl( AttributeKey.PersonProfilePage, PageParameterKey.PersonId, "((Key))" )
            };
        }

        /// <summary>
        /// Gets the message describing why this block cannot be displayed, or <c>null</c>
        /// when the page parameters represent a valid, viewable sign-up opportunity.
        /// </summary>
        /// <returns>An error message or <c>null</c>.</returns>
        private string GetValidationErrorMessage()
        {
            if ( _isValidationErrorMessageBuilt )
            {
                return _validationErrorMessage;
            }

            _isValidationErrorMessageBuilt = true;
            _validationErrorMessage = BuildValidationErrorMessage();

            return _validationErrorMessage;
        }

        /// <summary>
        /// Builds the message describing why this block cannot be displayed, or <c>null</c>
        /// when the page parameters represent a valid, viewable sign-up opportunity.
        /// </summary>
        /// <returns>An error message or <c>null</c>.</returns>
        private string BuildValidationErrorMessage()
        {
            var missingIds = new List<string>();
            if ( GroupId <= 0 )
            {
                missingIds.Add( "Group ID" );
            }

            if ( LocationId <= 0 )
            {
                missingIds.Add( "Location ID" );
            }

            if ( ScheduleId <= 0 )
            {
                missingIds.Add( "Schedule ID" );
            }

            if ( missingIds.Any() )
            {
                return $"The following required ID{( missingIds.Count > 1 ? "s were" : " was" )} not provided: {string.Join( ", ", missingIds )}.";
            }

            var group = GetGroup();
            if ( group == null )
            {
                return "The selected group does not exist or it has been archived.";
            }

            if ( !group.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) )
            {
                return EditModeMessage.NotAuthorizedToView( Rock.Model.Group.FriendlyTypeName );
            }

            var signUpGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP )?.Id ?? 0;
            if ( group.GroupTypeId != signUpGroupTypeId && group.GroupType?.InheritedGroupTypeId != signUpGroupTypeId )
            {
                return "The selected group is not of a type that can be edited as a sign-up group.";
            }

            var groupLocation = GetGroupLocation();
            if ( groupLocation?.Location == null || !groupLocation.Schedules.Any( s => s.Id == ScheduleId ) )
            {
                return "The selected sign-up opportunity does not exist.";
            }

            return null;
        }

        /// <summary>
        /// Determines if the current person is allowed to manage the members of this
        /// sign-up project.
        /// </summary>
        /// <returns><c>true</c> if the current person can manage members; otherwise, <c>false</c>.</returns>
        private bool CanManageMembers()
        {
            var group = GetGroup();
            var currentPerson = GetCurrentPerson();

            return group != null
                && (
                    group.IsAuthorized( Authorization.EDIT, currentPerson )
                    || group.IsAuthorized( Authorization.MANAGE_MEMBERS, currentPerson )
                    || group.IsAuthorized( Authorization.SCHEDULE, currentPerson )
                );
        }

        /// <summary>
        /// Gets the sign-up project (group) for this opportunity, loading it from the
        /// database on first access.
        /// </summary>
        /// <returns>The project (group) or <c>null</c> when not found.</returns>
        private Rock.Model.Group GetGroup()
        {
            if ( _group == null )
            {
                var groupId = GroupId;
                if ( groupId > 0 )
                {
                    _group = new GroupService( RockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Include( g => g.Campus )
                        .Include( g => g.GroupSyncs )
                        .Include( g => g.GroupType )
                        .Include( g => g.ParentGroup ) // ParentGroup may be needed for a proper authorization check.
                        .FirstOrDefault( g => g.Id == groupId );
                }
            }

            return _group;
        }

        /// <summary>
        /// Gets the group location for this opportunity, loading it from the database
        /// on first access.
        /// </summary>
        /// <returns>The group location or <c>null</c> when not found.</returns>
        private GroupLocation GetGroupLocation()
        {
            if ( _groupLocation == null )
            {
                if ( GroupId > 0 && LocationId > 0 )
                {
                    var groupId = GroupId;
                    var locationId = LocationId;

                    _groupLocation = new GroupLocationService( RockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Include( gl => gl.Location )
                        .Include( gl => gl.Schedules )
                        .Include( gl => gl.GroupLocationScheduleConfigs )
                        .FirstOrDefault( gl => gl.GroupId == groupId && gl.LocationId == locationId );
                }
            }

            return _groupLocation;
        }

        /// <summary>
        /// Gets the count of slots currently filled for this opportunity. Deceased
        /// individuals are excluded from the count, and saved grid filters are
        /// intentionally ignored.
        /// </summary>
        /// <returns>The count of slots currently filled.</returns>
        private int GetSlotsFilledCount()
        {
            var groupId = GroupId;
            var locationId = LocationId;
            var scheduleId = ScheduleId;

            return new GroupMemberAssignmentService( RockContext )
                .Queryable()
                .Count( gma =>
                    gma.GroupMember.GroupId == groupId
                    && gma.LocationId == locationId
                    && gma.ScheduleId == scheduleId
                    && !gma.GroupMember.Person.IsDeceased );
        }

        /// <inheritdoc/>
        protected override IQueryable<AttendeeRow> GetListQueryable( RockContext rockContext )
        {
            if ( GetValidationErrorMessage().IsNotNullOrWhiteSpace() )
            {
                return new List<AttendeeRow>().AsQueryable();
            }

            var groupId = GroupId;
            var locationId = LocationId;
            var scheduleId = ScheduleId;

            var qry = new GroupMemberAssignmentService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gma =>
                    gma.GroupMember.GroupId == groupId
                    && gma.LocationId == locationId
                    && gma.ScheduleId == scheduleId );

            // Filter by the family campus of each attendee.
            var campusId = FilterCampusId;
            var familyGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY )?.Id;
            if ( campusId.HasValue && familyGroupTypeId.HasValue )
            {
                var familyMembersQry = new GroupMemberService( rockContext )
                    .Queryable()
                    .Where( gm => gm.Group.GroupTypeId == familyGroupTypeId.Value && gm.Group.CampusId == campusId.Value );

                qry = qry.Where( gma => familyMembersQry.Any( fm => fm.PersonId == gma.GroupMember.PersonId ) );
            }

            // Filter by gender.
            var genders = FilterGenders;
            if ( genders.Any() )
            {
                qry = qry.Where( gma => genders.Contains( gma.GroupMember.Person.Gender ) );
            }

            return qry.Select( gma => new AttendeeRow
            {
                GroupMemberAssignment = gma,
                GroupMember = gma.GroupMember,
                RoleName = gma.GroupMember.GroupRole.Name,
                Person = new PersonProjection
                {
                    Id = gma.GroupMember.Person.Id,
                    NickName = gma.GroupMember.Person.NickName,
                    LastName = gma.GroupMember.Person.LastName,
                    SuffixValueId = gma.GroupMember.Person.SuffixValueId,
                    PhotoId = gma.GroupMember.Person.PhotoId,
                    Age = gma.GroupMember.Person.Age,
                    BirthDate = gma.GroupMember.Person.BirthDate,
                    Email = gma.GroupMember.Person.Email,
                    Gender = gma.GroupMember.Person.Gender,
                    RecordTypeValueId = gma.GroupMember.Person.RecordTypeValueId,
                    RecordStatusValueId = gma.GroupMember.Person.RecordStatusValueId,
                    AgeClassification = gma.GroupMember.Person.AgeClassification,
                    TopSignalColor = gma.GroupMember.Person.TopSignalColor,
                    TopSignalIconCssClass = gma.GroupMember.Person.TopSignalIconCssClass,
                    IsDeceased = gma.GroupMember.Person.IsDeceased
                }
            } );
        }

        /// <inheritdoc/>
        protected override IQueryable<AttendeeRow> GetOrderedListQueryable( IQueryable<AttendeeRow> queryable, RockContext rockContext )
        {
            return queryable
                .OrderBy( a => a.Person.LastName )
                .ThenBy( a => a.Person.AgeClassification )
                .ThenBy( a => a.Person.Gender );
        }

        /// <inheritdoc/>
        protected override List<AttendeeRow> GetListItems( IQueryable<AttendeeRow> queryable, RockContext rockContext )
        {
            var attendees = queryable.ToList();

            // Load attribute values for the grid-selected attributes.
            GridAttributeLoader.LoadFor( attendees, a => a.GroupMember, GetMemberAttributes(), rockContext );
            GridAttributeLoader.LoadFor( attendees, a => a.GroupMemberAssignment, GetOpportunityAttributes(), rockContext );

            foreach ( var attendee in attendees )
            {
                attendee.Person.IdKey = IdHasher.Instance.GetHash( attendee.Person.Id );
                attendee.Person.Initials = $"{attendee.Person.NickName.Truncate( 1, false )}{attendee.Person.LastName.Truncate( 1, false )}";
                attendee.Person.FullName = Rock.Model.Person.FormatFullName(
                    attendee.Person.NickName,
                    attendee.Person.LastName,
                    attendee.Person.SuffixValueId,
                    attendee.Person.RecordTypeValueId
                );
                attendee.Person.FullNameReversed = Rock.Model.Person.FormatFullNameReversed(
                    attendee.Person.LastName,
                    attendee.Person.NickName,
                    attendee.Person.SuffixValueId,
                    attendee.Person.RecordTypeValueId
                );
                attendee.Person.PhotoUrl = Rock.Model.Person.GetPersonPhotoUrl(
                    attendee.Person.Initials,
                    attendee.Person.PhotoId,
                    attendee.Person.Age,
                    attendee.Person.Gender,
                    attendee.Person.RecordTypeValueId,
                    attendee.Person.AgeClassification
                );
            }

            BuildRowSupportData( attendees, rockContext );

            return attendees;
        }

        /// <summary>
        /// Builds the lookups that supply computed grid field values (group history,
        /// group sync, unmet group requirements, phone numbers and home addresses).
        /// All data is fetched with one query per lookup to avoid per-row queries.
        /// </summary>
        /// <param name="attendees">The materialized attendee rows.</param>
        /// <param name="rockContext">The database context.</param>
        private void BuildRowSupportData( List<AttendeeRow> attendees, RockContext rockContext )
        {
            var group = GetGroup();
            if ( group == null || !attendees.Any() )
            {
                return;
            }

            var groupId = GroupId;

            /*
                6/10/26 - MSE

                The phone and home-location lookups use the unexecuted attendee
                queryable as their Contains source so EF generates an IN (subquery)
                instead of an IN clause with one parameter per attendee, which can
                exceed SQL parameter and batch size limits for a very large
                opportunity.

                Reason: Keep the per-person lookups safe for very large attendee lists.
            */
            var personIdQry = GetListQueryable( rockContext ).Select( a => a.Person.Id );

            _groupSyncRoleIds = new HashSet<int>( group.GroupSyncs.Select( s => s.GroupTypeRoleId ) );

            // Group member history only matters when the group type has it enabled,
            // since that is the only case where deletes become archives.
            var groupTypeCache = GroupTypeCache.Get( group.GroupTypeId );
            if ( groupTypeCache?.EnableGroupHistory == true )
            {
                _groupMemberIdsWithGroupHistory = new HashSet<int>(
                    new GroupMemberHistoricalService( rockContext )
                        .Queryable()
                        .Where( h => h.GroupId == groupId )
                        .Select( h => h.GroupMemberId )
                );
            }

            // Take note of any group members not yet meeting group requirements. The
            // requirements check is expensive, so only perform it when the group (or
            // its group type) actually has requirements defined.
            var hasGroupRequirements = new GroupRequirementService( rockContext )
                .Queryable()
                .Any( gr =>
                    ( gr.GroupId.HasValue && gr.GroupId == groupId )
                    || ( gr.GroupTypeId.HasValue && gr.GroupTypeId == group.GroupTypeId ) );

            if ( hasGroupRequirements )
            {
                _unmetRequirementNamesByGroupMemberId = new GroupService( rockContext )
                    .GroupMembersNotMeetingRequirements( group, true, true )
                    .ToDictionary(
                        kvp => kvp.Key.Id,
                        kvp => kvp.Value
                            .Select( statusKvp => statusKvp.Key?.GroupRequirement?.GroupRequirementType )
                            .Where( requirementType => requirementType != null )
                            .Select( requirementType => requirementType.NegativeLabel.IsNotNullOrWhiteSpace()
                                ? requirementType.NegativeLabel
                                : requirementType.Name )
                            .Where( name => name.IsNotNullOrWhiteSpace() )
                            .ToList()
                    );
            }

            // Preload the phone numbers for all attendees.
            var homePhoneTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() );
            var cellPhoneTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );

            var phoneNumbers = new PhoneNumberService( rockContext )
                .Queryable()
                .Where( n => personIdQry.Contains( n.PersonId ) && n.NumberTypeValueId.HasValue )
                .Select( n => new
                {
                    n.PersonId,
                    n.NumberTypeValueId,
                    n.NumberFormatted
                } )
                .ToList();

            _homePhoneByPersonId = phoneNumbers
                .Where( n => n.NumberTypeValueId == homePhoneTypeId )
                .GroupBy( n => n.PersonId )
                .ToDictionary( g => g.Key, g => g.Select( n => n.NumberFormatted ).FirstOrDefault() );

            _cellPhoneByPersonId = phoneNumbers
                .Where( n => n.NumberTypeValueId == cellPhoneTypeId )
                .GroupBy( n => n.PersonId )
                .ToDictionary( g => g.Key, g => g.Select( n => n.NumberFormatted ).FirstOrDefault() );

            // Preload the mapped home locations for all attendees.
            var familyGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY )?.Id;
            var homeLocationTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME )?.Id;

            if ( familyGroupTypeId.HasValue && homeLocationTypeId.HasValue )
            {
                _homeLocationByPersonId = new GroupMemberService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( gm => personIdQry.Contains( gm.PersonId ) && gm.Group.GroupTypeId == familyGroupTypeId.Value )
                    .Select( gm => new
                    {
                        gm.PersonId,
                        GroupOrder = gm.Group.Order,
                        HomeLocation = gm.Group.GroupLocations
                            .Where( gl => gl.GroupLocationTypeValueId == homeLocationTypeId.Value && gl.IsMappedLocation )
                            .Select( gl => gl.Location )
                            .FirstOrDefault()
                    } )
                    .ToList()
                    .GroupBy( x => x.PersonId )
                    .ToDictionary( g => g.Key, g => g.OrderBy( x => x.GroupOrder ).Select( x => x.HomeLocation ).FirstOrDefault() );
            }
        }

        /// <summary>
        /// Gets the group member attributes that should be included on the grid. This
        /// includes attributes inherited from the group type as well as attributes
        /// qualified to this specific group.
        /// </summary>
        /// <returns>A list of <see cref="AttributeCache"/> objects.</returns>
        private List<AttributeCache> GetMemberAttributes()
        {
            if ( _memberAttributes == null )
            {
                EnsureGridAttributes();
            }

            return _memberAttributes;
        }

        /// <summary>
        /// Gets the group member assignment (opportunity) attributes that should be
        /// included on the grid.
        /// </summary>
        /// <returns>A list of <see cref="AttributeCache"/> objects.</returns>
        private List<AttributeCache> GetOpportunityAttributes()
        {
            if ( _opportunityAttributes == null )
            {
                EnsureGridAttributes();
            }

            return _opportunityAttributes;
        }

        /// <summary>
        /// Builds the member and opportunity attribute lists that should be included
        /// on the grid: those marked to show in grid that the current person may view.
        /// </summary>
        private void EnsureGridAttributes()
        {
            _memberAttributes = new List<AttributeCache>();
            _opportunityAttributes = new List<AttributeCache>();

            var groupId = GroupId;
            if ( groupId <= 0 )
            {
                return;
            }

            var currentPerson = GetCurrentPerson();

            /*
                6/10/26 - MSE

                The grid builder throws when two fields share a name, so we drop
                duplicate keys within a source (e.g. an inherited and a group-qualified
                member attribute). Member and opportunity attributes may share a key
                legitimately, so keys are claimed per source and GetGridBuilder prefixes
                each source's field names to keep them unique.

                Reason: Skip same-source duplicate keys while still showing member and opportunity attributes that share a key.
            */
            var claimedMemberKeys = new HashSet<string>();
            var claimedOpportunityKeys = new HashSet<string>();

            void AddIfAvailable( List<AttributeCache> target, HashSet<string> claimedKeys, AttributeCache attribute )
            {
                if ( attribute.IsAuthorized( Authorization.VIEW, currentPerson ) && claimedKeys.Add( attribute.Key ) )
                {
                    target.Add( attribute );
                }
            }

            foreach ( var attribute in new GroupMember { GroupId = groupId }.GetInheritedAttributes( RockContext )
                .Where( a => a.IsGridColumn )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name ) )
            {
                AddIfAvailable( _memberAttributes, claimedMemberKeys, attribute );
            }

            var attributeService = new AttributeService( RockContext );
            var groupQualifier = groupId.ToString();

            var memberEntityTypeId = EntityTypeCache.Get<GroupMember>().Id;
            foreach ( var attribute in attributeService.GetByEntityTypeQualifier( memberEntityTypeId, "GroupId", groupQualifier, true )
                .Where( a => a.IsGridColumn )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToAttributeCacheList() )
            {
                AddIfAvailable( _memberAttributes, claimedMemberKeys, attribute );
            }

            var assignmentEntityTypeId = EntityTypeCache.Get<GroupMemberAssignment>().Id;
            foreach ( var attribute in attributeService.GetByEntityTypeQualifier( assignmentEntityTypeId, "GroupId", groupQualifier, true )
                .Where( a => a.IsGridColumn )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToAttributeCacheList() )
            {
                AddIfAvailable( _opportunityAttributes, claimedOpportunityKeys, attribute );
            }
        }

        /// <inheritdoc/>
        protected override GridBuilder<AttendeeRow> GetGridBuilder()
        {
            var inactiveRecordStatusId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );
            var isGroupHistoryEnabled = GroupTypeCache.Get( GetGroup()?.GroupTypeId ?? 0 )?.EnableGroupHistory == true;

            return new GridBuilder<AttendeeRow>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.GroupMemberAssignment.IdKey )
                .AddField( "person", a => new PersonFieldBag
                {
                    IdKey = a.Person.IdKey,
                    NickName = a.Person.NickName,
                    LastName = a.Person.LastName,
                    PhotoUrl = a.Person.PhotoUrl
                } )
                .AddTextField( "fullName", a => a.Person.FullName )
                .AddTextField( "personIdKey", a => a.Person.IdKey )
                .AddTextField( "groupMemberIdKey", a => a.GroupMember.IdKey )
                .AddTextField( "roleName", a => a.RoleName )
                .AddTextField( "memberStatus", a => a.GroupMember.GroupMemberStatus.ConvertToString() )
                .AddTextField( "signalMarkup", a => Rock.Model.Person.GetSignalMarkup( a.Person.TopSignalColor, a.Person.TopSignalIconCssClass ) )
                .AddTextField( "note", a => a.GroupMember.Note )
                .AddField( "hasUnmetGroupRequirements", a => _unmetRequirementNamesByGroupMemberId.ContainsKey( a.GroupMember.Id ) )
                .AddField( "unmetGroupRequirements", a => _unmetRequirementNamesByGroupMemberId.GetValueOrNull( a.GroupMember.Id ) )
                .AddField( "isDeceased", a => a.Person.IsDeceased )
                .AddField( "isActive", a => !inactiveRecordStatusId.HasValue || a.Person.RecordStatusValueId != inactiveRecordStatusId.Value )
                .AddField( "isMemberInactive", a => a.GroupMember.GroupMemberStatus == GroupMemberStatus.Inactive )
                .AddField( "isDeleteDisabled", a => _groupSyncRoleIds.Contains( a.GroupMember.GroupRoleId ) )
                .AddField( "isArchiveExpected", a => isGroupHistoryEnabled && _groupMemberIdsWithGroupHistory.Contains( a.GroupMember.Id ) )
                .AddTextField( "exportFullNameReversed", a => a.Person.FullNameReversed )
                .AddTextField( "nickName", a => a.Person.NickName )
                .AddTextField( "lastName", a => a.Person.LastName )
                .AddDateTimeField( "birthDate", a => a.Person.BirthDate )
                .AddField( "age", a => a.Person.Age )
                .AddTextField( "email", a => a.Person.Email )
                .AddTextField( "recordStatus", a => DefinedValueCache.GetName( a.Person.RecordStatusValueId ) )
                .AddTextField( "gender", a => a.Person.Gender.ConvertToString() )
                .AddTextField( "homePhone", a => _homePhoneByPersonId.GetValueOrNull( a.Person.Id ) )
                .AddTextField( "cellPhone", a => _cellPhoneByPersonId.GetValueOrNull( a.Person.Id ) )
                .AddTextField( "homeAddress", a => _homeLocationByPersonId.GetValueOrNull( a.Person.Id )?.FormattedAddress )
                .AddField( "latitude", a => _homeLocationByPersonId.GetValueOrNull( a.Person.Id )?.Latitude )
                .AddField( "longitude", a => _homeLocationByPersonId.GetValueOrNull( a.Person.Id )?.Longitude )
                .AddAttributeFieldsFrom( a => a.GroupMember, GetMemberAttributes() )
                // Opportunity attributes get their own field key prefix so an attribute
                // key shared with a member attribute cannot produce a duplicate grid
                // field name (see the note in EnsureGridAttributes).
                .AddAttributeFieldsFrom( a => a.GroupMemberAssignment, GetOpportunityAttributes(), "attr_opportunity_" );
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<IBreadCrumb>();

            // The page parameters may be supplied as an IdKey, Guid, or integer, so
            // each is resolved to its integer identifier the same way the block body
            // resolves them.
            var groupId = new GroupService( RockContext )
                .GetSelect( pageReference.GetPageParameter( PageParameterKey.GroupId ), g => g.Id );
            var locationId = new LocationService( RockContext )
                .GetSelect( pageReference.GetPageParameter( PageParameterKey.LocationId ), l => l.Id );
            var scheduleId = new ScheduleService( RockContext )
                .GetSelect( pageReference.GetPageParameter( PageParameterKey.ScheduleId ), s => s.Id );

            if ( groupId > 0 && locationId > 0 && scheduleId > 0 )
            {
                var opportunityInfo = new GroupLocationService( RockContext )
                    .Queryable()
                    .Where( gl => gl.GroupId == groupId && gl.LocationId == locationId )
                    .Select( gl => new
                    {
                        GroupName = gl.Group.Name,
                        ConfigurationName = gl.GroupLocationScheduleConfigs
                            .Where( c => c.ScheduleId == scheduleId )
                            .Select( c => c.ConfigurationName )
                            .FirstOrDefault()
                    } )
                    .FirstOrDefault();

                // Prefer the name provided at the opportunity level, falling back
                // to the name provided at the group (project) level.
                var opportunityName = opportunityInfo?.ConfigurationName.IsNotNullOrWhiteSpace() == true
                    ? opportunityInfo.ConfigurationName
                    : opportunityInfo?.GroupName;

                if ( opportunityName.IsNotNullOrWhiteSpace() )
                {
                    var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageReference.Parameters );
                    breadCrumbs.Add( new BreadCrumbLink( $"{opportunityName} Attendee List", breadCrumbPageRef ) );
                }
            }

            return new BreadCrumbResult
            {
                BreadCrumbs = breadCrumbs
            };
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Deletes the specified attendee (group member assignment). When this was the
        /// group member's last remaining assignment, the underlying group member record
        /// is also archived (when group history is enabled) or deleted (when allowed).
        /// </summary>
        /// <param name="key">The identifier of the group member assignment to be deleted.</param>
        /// <returns>An action result containing the updated slots filled count.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            if ( !CanManageMembers() )
            {
                return ActionForbidden( "Not authorized to delete attendees." );
            }

            var groupMemberAssignmentService = new GroupMemberAssignmentService( RockContext );
            var assignment = groupMemberAssignmentService
                .GetQueryableByKey( key, !PageCache.Layout.Site.DisablePredictableIds )
                .Include( gma => gma.GroupMember )
                .FirstOrDefault();

            // Ensure the assignment belongs to this opportunity so a crafted key cannot
            // delete attendees the current person is not authorized to manage.
            if ( assignment == null
                || assignment.GroupMember?.GroupId != GroupId
                || assignment.LocationId != LocationId
                || assignment.ScheduleId != ScheduleId )
            {
                return ActionBadRequest( "Unable to delete attendee." );
            }

            if ( !groupMemberAssignmentService.CanDelete( assignment, out var assignmentErrorMessage ) )
            {
                return ActionBadRequest( assignmentErrorMessage );
            }

            var groupMemberId = assignment.GroupMemberId;
            groupMemberAssignmentService.Delete( assignment );

            // When no other assignments remain for this group member, try to archive or
            // delete the group member record as well.
            var hasRemainingAssignments = groupMemberAssignmentService
                .Queryable()
                .Any( gma => gma.GroupMemberId == groupMemberId && gma.Id != assignment.Id );

            if ( !hasRemainingAssignments )
            {
                var groupMemberService = new GroupMemberService( RockContext );
                var groupMember = groupMemberService.Get( groupMemberId );

                if ( groupMember != null )
                {
                    var groupTypeCache = GroupTypeCache.Get( groupMember.GroupTypeId );
                    var hasGroupHistory = new GroupMemberHistoricalService( RockContext )
                        .Queryable()
                        .Any( h => h.GroupMemberId == groupMemberId );

                    if ( groupTypeCache?.EnableGroupHistory == true && hasGroupHistory )
                    {
                        groupMemberService.Archive( groupMember, GetCurrentPerson()?.PrimaryAliasId, false );
                    }
                    else if ( groupMemberService.CanDelete( groupMember, out _ ) )
                    {
                        groupMemberService.Delete( groupMember );
                    }
                    // Otherwise the attendee record is still removed; there is no need
                    // to surface an error just because the underlying group member
                    // record must remain.
                }
            }

            RockContext.SaveChanges();

            return ActionOk( GetSlotsFilledCount() );
        }

        /// <summary>
        /// Checks if the current person is allowed to create the specified entity set.
        /// Entity sets back the person merge and bulk update grid actions, so they may
        /// only be created by someone who can view this sign-up opportunity. In WebForms
        /// these actions were implicitly gated because the grid was never rendered for an
        /// unauthorized person; this gate provides the same protection for the directly
        /// invokable block action.
        /// </summary>
        /// <param name="entitySetBag">The entity set bag that will be created.</param>
        /// <returns><c>true</c> if the operation is allowed; otherwise, <c>false</c>.</returns>
        protected override bool IsAllowedToCreateEntitySet( GridEntitySetBag entitySetBag )
        {
            return GetValidationErrorMessage().IsNullOrWhiteSpace();
        }

        /// <summary>
        /// Checks if the current person is allowed to create the specified
        /// communication. Communications may only be created by someone who can
        /// view this sign-up opportunity.
        /// </summary>
        /// <param name="communicationBag">The communication bag that will be created.</param>
        /// <returns><c>true</c> if the operation is allowed; otherwise, <c>false</c>.</returns>
        protected override bool IsAllowedToCreateCommunication( GridCommunicationBag communicationBag )
        {
            return GetValidationErrorMessage().IsNullOrWhiteSpace();
        }

        /// <inheritdoc/>
        public override BlockActionResult CreateGridCommunication( GridCommunicationBag communication )
        {
            var group = GetGroup();

            if ( communication?.Recipients?.Any() == true && group != null )
            {
                var groupId = GroupId;
                var locationId = LocationId;
                var scheduleId = ScheduleId;

                // Look up this opportunity's assignment for each person, preferring an
                // active membership when a person somehow has more than one.
                var assignmentInfoByPersonId = new GroupMemberAssignmentService( RockContext )
                    .Queryable()
                    .Where( gma =>
                        gma.GroupMember.GroupId == groupId
                        && gma.LocationId == locationId
                        && gma.ScheduleId == scheduleId )
                    .Select( gma => new
                    {
                        gma.GroupMember.PersonId,
                        AssignmentId = gma.Id,
                        gma.GroupMember.GroupMemberStatus
                    } )
                    .ToList()
                    .GroupBy( a => a.PersonId )
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderBy( a => a.GroupMemberStatus == GroupMemberStatus.Inactive ? 1 : 0 ).First()
                    );

                /*
                    6/10/26 - MSE

                    Stamp each recipient with a GroupMemberAssignment merge field so a
                    communication can reference the recipient's sign-up assignment in
                    Lava as "GroupMemberAssignment" (resolved at send time by
                    CommunicationRecipient). Recipient keys come from the client, so we
                    only keep keys that resolve to an attendee of this opportunity and
                    drop inactive members.

                    Reason: Give communications per-recipient access to the sign-up assignment, restricted to this opportunity's active attendees.
                */
                var mergeFieldKey = EntityMergeFieldIdHelper.GetMergeFieldId<GroupMemberAssignment>(
                    new[]
                    {
                        new EntityMergeFieldQualifier( "GroupId", groupId.ToString() ),
                        new EntityMergeFieldQualifier( "GroupTypeId", group.GroupTypeId.ToString() ),
                        new EntityMergeFieldQualifier( "LocationId", locationId.ToString() ),
                        new EntityMergeFieldQualifier( "ScheduleId", scheduleId.ToString() )
                    } );

                var recipients = new List<GridEntitySetItemBag>();

                foreach ( var recipient in communication.Recipients )
                {
                    var personId = IdHasher.Instance.GetId( recipient.EntityKey );

                    if ( !personId.HasValue || !assignmentInfoByPersonId.TryGetValue( personId.Value, out var assignmentInfo ) )
                    {
                        continue;
                    }

                    if ( assignmentInfo.GroupMemberStatus == GroupMemberStatus.Inactive )
                    {
                        continue;
                    }

                    if ( recipient.AdditionalMergeValues == null )
                    {
                        recipient.AdditionalMergeValues = new Dictionary<string, object>();
                    }

                    recipient.AdditionalMergeValues[mergeFieldKey] = assignmentInfo.AssignmentId;

                    recipients.Add( recipient );
                }

                communication.Recipients = recipients;

                // Make the merge field available in the communication editor's picker.
                if ( communication.MergeFields == null )
                {
                    communication.MergeFields = new List<string>();
                }

                if ( !communication.MergeFields.Contains( mergeFieldKey ) )
                {
                    communication.MergeFields.Add( mergeFieldKey );
                }
            }

            return base.CreateGridCommunication( communication );
        }

        #endregion Block Actions

        #region Helper Classes

        /// <summary>
        /// A single attendee row displayed on the grid.
        /// </summary>
        public class AttendeeRow
        {
            /// <summary>
            /// Gets or sets the group member assignment that ties the group member to
            /// this opportunity's location and schedule.
            /// </summary>
            public GroupMemberAssignment GroupMemberAssignment { get; set; }

            /// <summary>
            /// Gets or sets the group member.
            /// </summary>
            public GroupMember GroupMember { get; set; }

            /// <summary>
            /// Gets or sets the attendee's person information.
            /// </summary>
            public PersonProjection Person { get; set; }

            /// <summary>
            /// Gets or sets the name of the group member's role.
            /// </summary>
            public string RoleName { get; set; }
        }

        /// <summary>
        /// The subset of person data needed to render and export an attendee row.
        /// </summary>
        public class PersonProjection
        {
            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the person's hashed identifier key.
            /// </summary>
            public string IdKey { get; set; }

            /// <summary>
            /// Gets or sets the person's nick name.
            /// </summary>
            public string NickName { get; set; }

            /// <summary>
            /// Gets or sets the person's last name.
            /// </summary>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets the person's name suffix defined value identifier.
            /// </summary>
            public int? SuffixValueId { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the person's photo.
            /// </summary>
            public int? PhotoId { get; set; }

            /// <summary>
            /// Gets or sets the person's age.
            /// </summary>
            public int? Age { get; set; }

            /// <summary>
            /// Gets or sets the person's birth date.
            /// </summary>
            public DateTime? BirthDate { get; set; }

            /// <summary>
            /// Gets or sets the person's email address.
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// Gets or sets the person's gender.
            /// </summary>
            public Gender Gender { get; set; }

            /// <summary>
            /// Gets or sets the person's record type defined value identifier.
            /// </summary>
            public int? RecordTypeValueId { get; set; }

            /// <summary>
            /// Gets or sets the person's record status defined value identifier.
            /// </summary>
            public int? RecordStatusValueId { get; set; }

            /// <summary>
            /// Gets or sets the person's age classification.
            /// </summary>
            public AgeClassification AgeClassification { get; set; }

            /// <summary>
            /// Gets or sets the color of the person's highest priority signal.
            /// </summary>
            public string TopSignalColor { get; set; }

            /// <summary>
            /// Gets or sets the icon of the person's highest priority signal.
            /// </summary>
            public string TopSignalIconCssClass { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the person is deceased.
            /// </summary>
            public bool IsDeceased { get; set; }

            /// <summary>
            /// Gets or sets the person's initials.
            /// </summary>
            public string Initials { get; set; }

            /// <summary>
            /// Gets or sets the person's full name.
            /// </summary>
            public string FullName { get; set; }

            /// <summary>
            /// Gets or sets the person's full name with last name first.
            /// </summary>
            public string FullNameReversed { get; set; }

            /// <summary>
            /// Gets or sets the URL of the person's photo.
            /// </summary>
            public string PhotoUrl { get; set; }
        }

        #endregion Helper Classes
    }
}
