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
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupMemberList;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Lists all the members of the given group.
    /// </summary>
    [DisplayName( "Group Member List" )]
    [Category( "Groups" )]
    [Description( "Lists all the members of the given group." )]
    [IconCssClass( "ti ti-users" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]
    [CustomizedGrid]

    #region Block Attributes

    [TextField( "Block Title",
        Description = "The text used in the title/header bar for this block.",
        Key = AttributeKey.BlockTitle,
        DefaultValue = "Group Members",
        IsRequired = true,
        Order = 0 )]

    [LinkedPage( "Detail Page",
        Description = "Page used for viewing and adding a group member.",
        Key = AttributeKey.DetailPage,
        IsRequired = true,
        Order = 1 )]

    [GroupField( "Group",
        Description = "Either pick a specific group or leave blank to have group be determined by the GroupId or CampusId page parameter.",
        Key = AttributeKey.Group,
        IsRequired = false,
        Order = 2 )]

    [LinkedPage( "Registration Page",
        Description = "Page used for viewing the registration(s) associated with a particular group member.",
        Key = AttributeKey.RegistrationPage,
        IsRequired = false,
        Order = 3 )]

    [BooleanField( "Show First/Last Attendance",
        Description = "If the group allows attendance, should the first and last attendance date be displayed for each group member?",
        Key = AttributeKey.ShowAttendance,
        DefaultBooleanValue = false,
        IsRequired = false,
        Order = 4 )]

    [BooleanField( "Show Date Added",
        Description = "Should the date that person was added to the group be displayed for each group member?",
        Key = AttributeKey.ShowDateAdded,
        DefaultBooleanValue = false,
        IsRequired = false,
        Order = 5 )]

    [BooleanField( "Show Note Column",
        Description = "Should the note be displayed as a separate grid column (instead of displaying a note icon under person's name)?",
        Key = AttributeKey.ShowNoteColumn,
        DefaultBooleanValue = false,
        IsRequired = false,
        Order = 6 )]

    [BooleanField( "Display Gender Column",
        Description = "Should the gender be displayed for each group member?",
        Key = AttributeKey.DisplayGenderColumn,
        DefaultBooleanValue = false,
        IsRequired = false,
        Order = 7 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "4C42FF33-4758-4AFC-A0F3-3F51DE43D2B6" )]
    [Rock.SystemGuid.BlockTypeGuid( "858F3FD2-77CF-4DC6-AF72-EC1F3221EA74" )]
    //// was [Rock.SystemGuid.BlockTypeGuid( "858F3FD2-77CF-4DC6-AF72-EC1F3221EA74" )]
    //[Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.GROUPS_GROUP_MEMBER_LIST )]
    public class GroupMemberList : RockListBlockType<GroupMemberList.GroupMemberRow>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string BlockTitle = "BlockTitle";
            public const string DetailPage = "DetailPage";
            public const string Group = "Group";
            public const string RegistrationPage = "RegistrationPage";
            public const string ShowAttendance = "ShowAttendance";
            public const string ShowDateAdded = "ShowDateAdded";
            public const string ShowNoteColumn = "ShowNoteColumn";
            public const string DisplayGenderColumn = "DisplayGenderColumn";
        }

        private static class PageParameterKey
        {
            public const string CampusId = "CampusId";
            public const string GroupId = "GroupId";
            public const string GroupMemberId = "GroupMemberId";
            public const string RegistrationId = "RegistrationId";
        }

        private static class NavigationUrlKey
        {
            public const string AddPage = "AddPage";
            public const string DetailPage = "DetailPage";
            public const string RegistrationPage = "RegistrationPage";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The group whose members are being listed. Access via <see cref="Group"/>. <c>null</c> when no group
        /// resolves or the current person cannot view it.
        /// </summary>
        private GroupCache _group;

        /// <summary>
        /// Indicates whether <see cref="_group"/> has been resolved, so a missing or unauthorized group is not
        /// resolved again.
        /// </summary>
        private bool _isGroupLoaded;

        /// <summary>
        /// The registrations each listed member was added through, keyed by group member identifier.
        /// </summary>
        private Dictionary<int, List<ListItemBag>> _registrationsByGroupMemberId = new Dictionary<int, List<ListItemBag>>();

        /// <summary>
        /// The earliest and latest dates each person attended this group, keyed by person identifier.
        /// </summary>
        private Dictionary<int, DateRange> _attendanceRangeByPersonId = new Dictionary<int, DateRange>();

        /// <summary>
        /// The formatted home phone number of each listed person, keyed by person identifier.
        /// </summary>
        private Dictionary<int, string> _homePhoneByPersonId = new Dictionary<int, string>();

        /// <summary>
        /// The formatted mobile phone number of each listed person, keyed by person identifier.
        /// </summary>
        private Dictionary<int, string> _cellPhoneByPersonId = new Dictionary<int, string>();

        /// <summary>
        /// The mapped home location of each listed person, keyed by person identifier.
        /// </summary>
        private Dictionary<int, Location> _homeLocationByPersonId = new Dictionary<int, Location>();

        /// <summary>
        /// The identifiers of the listed people who have signed the group's required signature document.
        /// </summary>
        private HashSet<int> _signedPersonIds = new HashSet<int>();

        /// <summary>
        /// The identifiers of the listed people holding more than one active role in the group.
        /// </summary>
        private HashSet<int> _multipleRolePersonIds = new HashSet<int>();

        /// <summary>
        /// The group member attributes shown as grid columns. Access via <see cref="GetGridAttributes"/>.
        /// </summary>
        private List<AttributeCache> _gridAttributes;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the group this block lists the members of, resolved from the Group block setting, then the GroupId
        /// page parameter, then the team group of the campus named by the CampusId page parameter. Resolved once per
        /// request. <c>null</c> when no group resolves or the current person cannot view it.
        /// </summary>
        private GroupCache Group
        {
            get
            {
                if ( _isGroupLoaded )
                {
                    return _group;
                }

                var groupGuid = GetAttributeValue( AttributeKey.Group ).AsGuidOrNull();
                GroupCache group;

                if ( groupGuid.HasValue )
                {
                    group = GroupCache.Get( groupGuid.Value );
                }
                else
                {
                    group = GroupCache.Get( PageParameter( PageParameterKey.GroupId ), !PageCache.Layout.Site.DisablePredictableIds );

                    if ( group == null )
                    {
                        var teamGroupId = Campus?.TeamGroupId;

                        group = teamGroupId.HasValue ? GroupCache.Get( teamGroupId.Value ) : null;
                    }
                }

                _group = group?.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) == true ? group : null;
                _isGroupLoaded = true;

                return _group;
            }
        }

        /// <summary>
        /// Gets the campus supplied to the page.
        /// </summary>
        private CampusCache Campus => CampusCache.Get( PageParameter( PageParameterKey.CampusId ), !PageCache.Layout.Site.DisablePredictableIds );

        /// <summary>
        /// Gets a value indicating whether the First Attended and Last Attended columns are shown, which takes both
        /// the block setting and a group type that records attendance.
        /// </summary>
        private bool IsAttendanceShown => GetAttributeValue( AttributeKey.ShowAttendance ).AsBoolean() && Group?.GroupType?.TakesAttendance == true;

        /// <summary>
        /// Gets a value indicating whether people who have not signed the group's required signature document
        /// should be flagged, which takes a group that requires one.
        /// </summary>
        private bool IsUnsignedShown => Group?.RequiredSignatureDocumentTemplateId.HasValue == true;

        /// <summary>
        /// Gets the identifier of the Inactive person record status.
        /// </summary>
        private int? InactiveRecordStatusValueId => DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE )?.Id;

        #endregion Properties

        #region RockListBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new ListBlockBox<GroupMemberListOptionsBag>
            {
                GridDefinition = GetGridBuilder().BuildDefinition(),
                Options = GetBoxOptions(),
                IsAddEnabled = IsAddEnabled(),
                NavigationUrls = GetBoxNavigationUrls()
            };
        }

        /// <inheritdoc/>
        protected override bool IsAllowedToCreateEntitySet( GridEntitySetBag entitySetBag )
        {
            return Group != null;
        }

        /// <inheritdoc/>
        protected override bool IsAllowedToCreateCommunication( GridCommunicationBag communicationBag )
        {
            return Group != null;
        }

        /// <inheritdoc/>
        protected override IQueryable<GroupMemberRow> GetListQueryable( RockContext rockContext )
        {
            if ( Group == null )
            {
                return new List<GroupMemberRow>().AsQueryable();
            }

            var groupId = Group.Id;

            return new GroupMemberService( rockContext )
                .Queryable( true )
                .AsNoTracking()
                .Where( gm => gm.GroupId == groupId )
                .Select( gm => new GroupMemberRow
                {
                    GroupMember = gm,
                    RoleName = gm.GroupRole.Name,
                    RoleOrder = gm.GroupRole.Order,
                    Person = new PersonProjection
                    {
                        Id = gm.Person.Id,
                        NickName = gm.Person.NickName,
                        FirstName = gm.Person.FirstName,
                        LastName = gm.Person.LastName,
                        SuffixValueId = gm.Person.SuffixValueId,
                        PhotoId = gm.Person.PhotoId,
                        Age = gm.Person.Age,
                        BirthDate = gm.Person.BirthDate,
                        Email = gm.Person.Email,
                        Gender = gm.Person.Gender,
                        IsDeceased = gm.Person.IsDeceased,
                        RecordTypeValueId = gm.Person.RecordTypeValueId,
                        RecordStatusValueId = gm.Person.RecordStatusValueId,
                        ConnectionStatusValueId = gm.Person.ConnectionStatusValueId,
                        MaritalStatusValueId = gm.Person.MaritalStatusValueId,
                        AgeClassification = gm.Person.AgeClassification,
                        TopSignalColor = gm.Person.TopSignalColor,
                        TopSignalIconCssClass = gm.Person.TopSignalIconCssClass
                    }
                } );
        }

        /// <inheritdoc/>
        protected override IQueryable<GroupMemberRow> GetOrderedListQueryable( IQueryable<GroupMemberRow> queryable, RockContext rockContext )
        {
            return queryable
                .OrderBy( r => r.RoleOrder )
                .ThenBy( r => r.Person.LastName )
                .ThenBy( r => r.Person.FirstName );
        }

        /// <inheritdoc/>
        protected override List<GroupMemberRow> GetListItems( IQueryable<GroupMemberRow> queryable, RockContext rockContext )
        {
            var rows = queryable.ToList();

            foreach ( var person in rows.Select( r => r.Person ) )
            {
                var initials = $"{person.NickName.Truncate( 1, false )}{person.LastName.Truncate( 1, false )}";

                person.IdKey = IdHasher.Instance.GetHash( person.Id );
                person.FullNameReversed = Rock.Model.Person.FormatFullNameReversed(
                    person.LastName,
                    person.NickName,
                    person.SuffixValueId,
                    person.RecordTypeValueId );
                person.PhotoUrl = Rock.Model.Person.GetPersonPhotoUrl(
                    initials,
                    person.PhotoId,
                    person.Age,
                    person.Gender,
                    person.RecordTypeValueId,
                    person.AgeClassification );
            }

            GridAttributeLoader.LoadFor( rows, r => r.GroupMember, GetGridAttributes(), rockContext );

            BuildRowSupportData( rows, rockContext );

            return rows;
        }

        /// <inheritdoc/>
        protected override GridBuilder<GroupMemberRow> GetGridBuilder()
        {
            // Custom grid columns resolve their Lava against the group member, so "Row" means the same thing it did
            // in the legacy block rather than the projected row this grid is built from.
            var blockOptions = new GridBuilderGridOptions<GroupMemberRow>
            {
                LavaObject = row => row.GroupMember
            };

            return new GridBuilder<GroupMemberRow>()
                .WithBlock( this, blockOptions )
                .AddTextField( "idKey", r => r.GroupMember.IdKey )
                .AddTextField( "personIdKey", r => r.Person.IdKey )
                .AddField( "person", r => new PersonFieldBag
                {
                    IdKey = r.Person.IdKey,
                    NickName = r.Person.NickName,
                    LastName = r.Person.LastName,
                    PhotoUrl = r.Person.PhotoUrl,
                    ConnectionStatus = Group?.GroupType?.ShowConnectionStatus == true ? DefinedValueCache.GetValue( r.Person.ConnectionStatusValueId ) : null
                } )
                .AddTextField( "exportFullNameReversed", r => r.Person.FullNameReversed )
                .AddTextField( "maritalStatus", r => DefinedValueCache.GetValue( r.Person.MaritalStatusValueId ) )
                .AddTextField( "connectionStatus", r => DefinedValueCache.GetValue( r.Person.ConnectionStatusValueId ) )
                .AddTextField( "gender", r => r.Person.Gender.ConvertToString() )
                .AddField( "registrations", r => _registrationsByGroupMemberId.GetValueOrNull( r.GroupMember.Id ) )
                .AddTextField( "role", r => r.RoleName )
                .AddDateTimeField( "dateAdded", r => r.GroupMember.DateTimeAdded )
                .AddDateTimeField( "firstAttended", r => _attendanceRangeByPersonId.GetValueOrNull( r.Person.Id )?.Start )
                .AddDateTimeField( "lastAttended", r => _attendanceRangeByPersonId.GetValueOrNull( r.Person.Id )?.End )
                .AddTextField( "note", r => r.GroupMember.Note )
                .AddTextField( "status", r => r.GroupMember.GroupMemberStatus.ConvertToString() )
                .AddTextField( "signalColor", r => r.Person.TopSignalColor )
                .AddTextField( "signalIconCssClass", r => r.Person.TopSignalIconCssClass )
                .AddField( "hasMultipleRoles", r => _multipleRolePersonIds.Contains( r.Person.Id ) )
                .AddField( "isUnsigned", r => IsUnsignedShown && !_signedPersonIds.Contains( r.Person.Id ) )
                .AddField( "isInactive", r => r.GroupMember.GroupMemberStatus == GroupMemberStatus.Inactive )
                .AddField( "isPersonInactive", r => r.Person.RecordStatusValueId.HasValue && r.Person.RecordStatusValueId == InactiveRecordStatusValueId )
                .AddTextField( "nickName", r => r.Person.NickName )
                .AddTextField( "lastName", r => r.Person.LastName )
                .AddDateTimeField( "birthDate", r => r.Person.BirthDate )
                .AddField( "age", r => r.Person.Age )
                .AddTextField( "email", r => r.Person.Email )
                .AddField( "recordStatusValueId", r => r.Person.RecordStatusValueId )
                .AddTextField( "recordStatus", r => DefinedValueCache.GetValue( r.Person.RecordStatusValueId ) )
                .AddField( "isDeceased", r => r.Person.IsDeceased )
                .AddTextField( "homePhone", r => _homePhoneByPersonId.GetValueOrNull( r.Person.Id ) )
                .AddTextField( "cellPhone", r => _cellPhoneByPersonId.GetValueOrNull( r.Person.Id ) )
                .AddTextField( "homeAddress", r => _homeLocationByPersonId.GetValueOrNull( r.Person.Id )?.FormattedAddress )
                .AddField( "latitude", r => _homeLocationByPersonId.GetValueOrNull( r.Person.Id )?.Latitude )
                .AddField( "longitude", r => _homeLocationByPersonId.GetValueOrNull( r.Person.Id )?.Longitude )
                .AddAttributeFieldsFrom( r => r.GroupMember, GetGridAttributes() );
        }

        #endregion RockListBlockType Implementation

        #region Private Methods

        /// <summary>
        /// Gets the box options required for the component to render the block.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private GroupMemberListOptionsBag GetBoxOptions()
        {
            var options = new GroupMemberListOptionsBag();
            var groupType = Group?.GroupType;

            if ( groupType == null )
            {
                return options;
            }

            var blockTitle = GetAttributeValue( AttributeKey.BlockTitle );

            options.Title = blockTitle.IsNotNullOrWhiteSpace()
                ? blockTitle
                : $"{groupType.GroupTerm} {groupType.GroupMemberTerm.Pluralize()}";

            options.ItemTerm = $"{groupType.GroupTerm} {groupType.GroupMemberTerm}";
            options.ExportTitle = Group.Name;
            options.IsGridVisible = true;

            // A group type with no roles cannot hold members. The warning sits above the grid, which stays in place
            // and simply has no rows.
            if ( !groupType.Roles.Any() )
            {
                options.WarningMessage = $"{groupType.GroupMemberTerm.Pluralize()} cannot be added to this {groupType.GroupTerm} because the '{groupType.Name}' group type does not have any roles defined.";
            }

            options.IsDateAddedColumnVisible = GetAttributeValue( AttributeKey.ShowDateAdded ).AsBoolean();
            options.IsNoteColumnVisible = GetAttributeValue( AttributeKey.ShowNoteColumn ).AsBoolean();
            options.IsGenderColumnVisible = GetAttributeValue( AttributeKey.DisplayGenderColumn ).AsBoolean();
            options.IsMaritalStatusColumnVisible = groupType.ShowMaritalStatus;
            options.IsAttendanceColumnVisible = IsAttendanceShown;

            return options;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            if ( Group == null )
            {
                return new Dictionary<string, string>();
            }

            var detailParameters = new Dictionary<string, string>
            {
                { PageParameterKey.GroupMemberId, "((Key))" }
            };

            var addParameters = new Dictionary<string, string>
            {
                { PageParameterKey.GroupMemberId, "0" },
                { PageParameterKey.GroupId, Group.IdKey }
            };

            var campus = Campus;

            if ( campus != null )
            {
                detailParameters.Add( PageParameterKey.CampusId, campus.IdKey );
                addParameters.Add( PageParameterKey.CampusId, campus.IdKey );
            }

            var registrationParameters = new Dictionary<string, string>
            {
                { PageParameterKey.RegistrationId, "((Key))" }
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, detailParameters ),
                [NavigationUrlKey.AddPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, addParameters ),
                [NavigationUrlKey.RegistrationPage] = this.GetLinkedPageUrl( AttributeKey.RegistrationPage, registrationParameters )
            };
        }

        /// <summary>
        /// Determines whether the current person may add and remove members of this group.
        /// </summary>
        /// <returns><c>true</c> when the current person may manage this group's members.</returns>
        private bool CanEdit()
        {
            var currentPerson = GetCurrentPerson();

            return Group != null
                && (
                    BlockCache.IsAuthorized( Authorization.EDIT, currentPerson )
                    || Group.IsAuthorized( Authorization.EDIT, currentPerson )
                    || Group.IsAuthorized( Authorization.MANAGE_MEMBERS, currentPerson )
                );
        }

        /// <summary>
        /// Determines whether the Add button should be offered. A group type with no roles has nothing to add a
        /// member as.
        /// </summary>
        /// <returns><c>true</c> when a member may be added.</returns>
        private bool IsAddEnabled()
        {
            return CanEdit() && Group?.GroupType?.Roles.Any() == true;
        }

        /// <summary>
        /// Gets the group member attributes shown as grid columns: those flagged to show in the grid, active, and
        /// VIEW-authorized, qualified either to this group or to a group type in its inheritance chain. Resolved
        /// once per request.
        /// </summary>
        /// <returns>The attributes to render as columns, in display order.</returns>
        private List<AttributeCache> GetGridAttributes()
        {
            if ( _gridAttributes != null )
            {
                return _gridAttributes;
            }

            _gridAttributes = new List<AttributeCache>();

            if ( Group == null )
            {
                return _gridAttributes;
            }

            var candidates = new List<AttributeCache>();

            candidates.AddRange( AttributeCache.GetOrderedGridAttributes( EntityTypeCache.Get<GroupMember>().Id, "GroupId", Group.Id.ToString() ) );

            // GroupTypeId is set so the inherited lookup resolves from cache rather than querying for it. The order
            // it returns runs from the most distant group type down to this one, which the dedupe below relies on.
            candidates.AddRange( new GroupMember { GroupId = Group.Id, GroupTypeId = Group.GroupTypeId }
                .GetInheritedAttributes( RockContext )
                .Where( a => a.IsGridColumn && a.IsActive ) );

            // Two attributes sharing a key would throw in the grid builder. Rock keeps the first one in this order
            // as the attribute for that key, so the column matches the definition the rest of Rock resolves to.
            _gridAttributes = candidates
                .Where( a => a.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) )
                .GroupBy( a => a.Key )
                .Select( g => g.First() )
                .ToList();

            return _gridAttributes;
        }

        /// <summary>
        /// Builds the lookups the member query cannot project: registrations, first and last attendance, signed
        /// documents, the people holding multiple active roles, phone numbers, and home addresses. Each takes a
        /// single query covering the whole list, never one per member, and the multiple-role set takes none.
        /// </summary>
        /// <param name="rows">The materialized group member rows.</param>
        /// <param name="rockContext">The database context.</param>
        private void BuildRowSupportData( List<GroupMemberRow> rows, RockContext rockContext )
        {
            if ( Group == null || !rows.Any() )
            {
                return;
            }

            var groupId = Group.Id;

            // Contains over an unexecuted queryable so EF emits an IN (subquery) rather than one parameter per member.
            var listQueryable = GetListQueryable( rockContext );
            var groupMemberIdQuery = listQueryable.Select( r => r.GroupMember.Id );
            var personIdQuery = listQueryable.Select( r => r.Person.Id );

            _registrationsByGroupMemberId = new RegistrationRegistrantService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( r => r.GroupMemberId.HasValue
                    && groupMemberIdQuery.Contains( r.GroupMemberId.Value )
                    && r.Registration != null
                    && r.Registration.RegistrationInstance != null )
                .Select( r => new
                {
                    GroupMemberId = r.GroupMemberId.Value,
                    RegistrationId = r.Registration.Id,
                    RegistrationName = r.Registration.RegistrationInstance.Name
                } )
                .Distinct()
                .ToList()
                .GroupBy( r => r.GroupMemberId )
                .ToDictionary(
                    g => g.Key,
                    g => g.Select( r => new ListItemBag
                    {
                        Value = IdHasher.Instance.GetHash( r.RegistrationId ),
                        Text = r.RegistrationName
                    } ).ToList() );

            if ( IsAttendanceShown )
            {
                _attendanceRangeByPersonId = new AttendanceService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( a => a.Occurrence.GroupId == groupId && a.DidAttend == true )
                    .GroupBy( a => a.PersonAlias.PersonId )
                    .Select( g => new
                    {
                        PersonId = g.Key,
                        FirstAttended = g.Min( a => a.StartDateTime ),
                        LastAttended = g.Max( a => a.StartDateTime )
                    } )
                    .ToList()
                    .ToDictionary( a => a.PersonId, a => new DateRange( a.FirstAttended, a.LastAttended ) );
            }

            if ( IsUnsignedShown )
            {
                var templateId = Group.RequiredSignatureDocumentTemplateId.Value;

                _signedPersonIds = new SignatureDocumentService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( d => d.SignatureDocumentTemplateId == templateId
                        && d.Status == SignatureDocumentStatus.Signed
                        && d.BinaryFileId.HasValue
                        && d.AppliesToPersonAlias != null
                        && personIdQuery.Contains( d.AppliesToPersonAlias.PersonId ) )
                    .Select( d => d.AppliesToPersonAlias.PersonId )
                    .Distinct()
                    .ToHashSet();
            }

            if ( Group.GroupType?.IsSchedulingEnabled == true )
            {
                _multipleRolePersonIds = rows
                    .Where( r => r.GroupMember.GroupMemberStatus == GroupMemberStatus.Active )
                    .GroupBy( r => r.Person.Id )
                    .Where( g => g.Count() > 1 )
                    .Select( g => g.Key )
                    .ToHashSet();
            }

            var homePhoneTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() );
            var cellPhoneTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );

            var phoneNumbers = new PhoneNumberService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( n => personIdQuery.Contains( n.PersonId ) && n.NumberTypeValueId.HasValue )
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

            var familyGroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            var homeLocationTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );

            if ( !familyGroupTypeId.HasValue || !homeLocationTypeId.HasValue )
            {
                return;
            }

            // The lowest ordered family wins when a person belongs to more than one.
            _homeLocationByPersonId = new GroupMemberService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gm => personIdQuery.Contains( gm.PersonId ) && gm.Group.GroupTypeId == familyGroupTypeId.Value )
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

        #endregion Private Methods

        #region Helper Classes

        /// <summary>
        /// A single group member row displayed on the grid.
        /// </summary>
        public class GroupMemberRow
        {
            /// <summary>
            /// Gets or sets the group member.
            /// </summary>
            public GroupMember GroupMember { get; set; }

            /// <summary>
            /// Gets or sets the name of the group member's role.
            /// </summary>
            public string RoleName { get; set; }

            /// <summary>
            /// Gets or sets the display order of the group member's role, which drives the default sort.
            /// </summary>
            public int RoleOrder { get; set; }

            /// <summary>
            /// Gets or sets the group member's person information.
            /// </summary>
            public PersonProjection Person { get; set; }
        }

        /// <summary>
        /// The subset of person data needed to render a group member row.
        /// </summary>
        public class PersonProjection
        {
            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the person's hashed identifier.
            /// </summary>
            public string IdKey { get; set; }

            /// <summary>
            /// Gets or sets the person's nick name.
            /// </summary>
            public string NickName { get; set; }

            /// <summary>
            /// Gets or sets the person's first name, which the default sort falls back to.
            /// </summary>
            public string FirstName { get; set; }

            /// <summary>
            /// Gets or sets the person's last name.
            /// </summary>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets the person's suffix defined value identifier.
            /// </summary>
            public int? SuffixValueId { get; set; }

            /// <summary>
            /// Gets or sets the person's name as "Last, First", which is the name written to the export.
            /// </summary>
            public string FullNameReversed { get; set; }

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
            /// Gets or sets a value indicating whether the person is deceased.
            /// </summary>
            public bool IsDeceased { get; set; }

            /// <summary>
            /// Gets or sets the person's record type defined value identifier.
            /// </summary>
            public int? RecordTypeValueId { get; set; }

            /// <summary>
            /// Gets or sets the person's record status defined value identifier.
            /// </summary>
            public int? RecordStatusValueId { get; set; }

            /// <summary>
            /// Gets or sets the person's connection status defined value identifier.
            /// </summary>
            public int? ConnectionStatusValueId { get; set; }

            /// <summary>
            /// Gets or sets the person's marital status defined value identifier.
            /// </summary>
            public int? MaritalStatusValueId { get; set; }

            /// <summary>
            /// Gets or sets the person's age classification.
            /// </summary>
            public AgeClassification AgeClassification { get; set; }

            /// <summary>
            /// Gets or sets the color of the person's highest priority active signal.
            /// </summary>
            public string TopSignalColor { get; set; }

            /// <summary>
            /// Gets or sets the icon of the person's highest priority active signal.
            /// </summary>
            public string TopSignalIconCssClass { get; set; }

            /// <summary>
            /// Gets or sets the URL of the person's photo.
            /// </summary>
            public string PhotoUrl { get; set; }
        }

        #endregion Helper Classes
    }
}
