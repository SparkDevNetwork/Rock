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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Groups.GroupMemberList;
using Rock.Common.Mobile.Enums;
using Rock.Common.Mobile.ViewModel;
using Rock.Data;
using Rock.Mobile;
using Rock.Mobile.JsonFields;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Types.Mobile.Groups
{
    /// <summary>
    /// Displays a page to allow the user to view a list of members in a group.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Group Member List" )]
    [Category( "Mobile > Groups" )]
    [Description( "Allows the user to view a list of members in a group." )]
    [IconCssClass( "fa fa-users" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [LinkedPage( "Detail Page",
        Description = "The page that will display when a particular member/person in the list is pressed.",
        IsRequired = false,
        Key = AttributeKeys.GroupMemberDetailPage,
        Order = 0 )]

    [TextField( "Title Template",
        Description = "The value to use when rendering the title text. <span class='tip tip-lava'></span>",
        IsRequired = false,
        DefaultValue = "{{ Group.Name }} Group Roster",
        Key = AttributeKeys.TitleTemplate,
        Order = 1 )]

    [BooleanField( "Group By Person",
        Description = "If enabled, the merge field object provided will change to be 'People'. This an object containing a list of the individual people for each member occurrence, and a comma-delimited string list of every role the Person has.",
        IsRequired = false,
        DefaultBooleanValue = false,
        Key = AttributeKeys.GroupByPerson,
        Order = 2 )]

    [BlockTemplateField( "Template",
        Description = "The template to use when rendering the content.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_MEMBER_LIST,
        IsRequired = true,
        DefaultValue = "674CF1E3-561C-430D-B4A8-39957AC1BCF1",
        Key = AttributeKeys.Template,
        Order = 3 )]

    [TextField( "Additional Fields",
        Description = "",
        IsRequired = false,
        DefaultValue = "",
        Category = "CustomSetting",
        Key = AttributeKeys.AdditionalFields,
        Order = 4 )]

    [BooleanField( "Show Include Inactive Members Filter",
        Description = "If enabled then the 'Include Inactive' filter option will be shown.",
        IsRequired = false,
        Key = AttributeKeys.ShowInactiveMembersFilter,
        Category = "filter",
        DefaultBooleanValue = false,
        Order = 5 )]

    [BooleanField( "Show Group Role Type Filter",
        Description = "If enabled then the 'Group Type Role' filter option will be shown.",
        IsRequired = false,
        Key = AttributeKeys.ShowGroupRoleTypeFilter,
        Category = "filter",
        DefaultBooleanValue = false,
        Order = 6 )]

    [BooleanField( "Show Group Role Filter",
        Description = "If enabled then the 'Group Role' filter option will be shown.",
        IsRequired = false,
        DefaultBooleanValue = true,
        Category = "filter",
        Key = AttributeKeys.ShowGroupRoleFilter,
        Order = 7 )]

    [BooleanField( "Show Gender Filter",
        Description = "If enabled then the 'Gender' filter option will be shown.",
        IsRequired = false,
        DefaultBooleanValue = false,
        Category = "filter",
        Key = AttributeKeys.ShowGenderFilter,
        Order = 8 )]

    [BooleanField( "Show Unknown as Gender Filter Option",
        Description = "If enabled then 'Unknown' will be shown as a Gender filter option.",
        IsRequired = false,
        DefaultBooleanValue = true,
        Category = "filter",
        Key = AttributeKeys.ShowUnknownAsGenderFilterOption,
        Order = 9 )]

    [BooleanField( "Show Subgroup Filter",
        Description = "If enabled then the 'Subgroup' filter option will be shown.",
        IsRequired = false,
        DefaultBooleanValue = false,
        Category = "filter",
        Key = AttributeKeys.ShowChildGroupsFilter,
        Order = 10 )]

    [BooleanField( "Show Attendance Filter",
        Description = "If enabled then the 'Attendance' filter option will be shown.",
        IsRequired = false,
        DefaultBooleanValue = false,
        Key = AttributeKeys.ShowAttendanceFilter,
        Category = "filter",
        Order = 11 )]

    [IntegerField( "Attendance Filter Short Week Range",
        Description = "Displays a filter option that gives a variety of different options for attendance based on x number of weeks.",
        IsRequired = false,
        DefaultIntegerValue = 3,
        Key = AttributeKeys.AttendanceFilterShortWeekRange,
        Category = "filter",
        Order = 12 )]

    [IntegerField( "Attendance Filter Long Week Range",
        Description = "Displays a filter option that gives a variety of different options for attendance based on x number of weeks.",
        IsRequired = false,
        DefaultIntegerValue = 12,
        Key = AttributeKeys.AttendanceFilterLongWeekRange,
        Category = "filter",
        Order = 13 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_GROUPS_GROUP_MEMBER_LIST_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "5A6D2ADB-03A7-4B55-8EAA-26A37116BFF1" )]
    public class GroupMemberList : RockBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the GroupMemberList block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The block title template key.
            /// </summary>
            public const string TitleTemplate = "TitleTemplate";

            /// <summary>
            /// The on-click redirect page attribute key.
            /// </summary>
            public const string GroupMemberDetailPage = "GroupMemberDetailPage";

            /// <summary>
            /// The template key.
            /// </summary>
            public const string Template = "Template";

            /// <summary>
            /// The additional fields key.
            /// </summary>
            public const string AdditionalFields = "AdditionalFields";

            /// <summary>
            /// The show inactive filter key.
            /// </summary>
            public const string ShowInactiveMembersFilter = "ShowInactiveMembersFilter";

            /// <summary>
            /// The show group role type filter key.
            /// </summary>
            public const string ShowGroupRoleTypeFilter = "ShowGroupRoleTypeFilter";

            /// <summary>
            /// The show group role filter key.
            /// </summary>
            public const string ShowGroupRoleFilter = "ShowGroupRoleFilter";

            /// <summary>
            /// The show gender filter key.
            /// </summary>
            public const string ShowGenderFilter = "ShowGenderFilter";

            /// <summary>
            /// The show unknown as gender filter option key.
            /// </summary>
            public const string ShowUnknownAsGenderFilterOption = "ShowUnknownAsGenderFilterOption";

            /// <summary>
            /// The show child groups filter key.
            /// </summary>
            public const string ShowChildGroupsFilter = "ShowChildGroupsFilter";

            /// <summary>
            /// The show attendance filter key.
            /// </summary>
            public const string ShowAttendanceFilter = "ShowAttendanceFilter";

            /// <summary>
            /// The attendance filter short period filter key.
            /// </summary>
            public const string AttendanceFilterShortWeekRange = "AttendanceFilterShortWeekRange";

            /// <summary>
            /// The attendance filter long period filter key.
            /// </summary>
            public const string AttendanceFilterLongWeekRange = "AttendanceFilterLongWeekRange";

            /// <summary>
            /// The group by person attribute key.
            /// </summary>
            public const string GroupByPerson = "GroupByPerson";
        }

        /// <summary>
        /// Gets the title Lava template to use when rendering out the block title.
        /// </summary>
        /// <value>
        /// The title Lava template to use when rendering out the block title.
        /// </value>
        protected string TitleTemplate => GetAttributeValue( AttributeKeys.TitleTemplate );

        /// <summary>
        /// Gets the group member detail page.
        /// </summary>
        /// <value>
        /// The group member detail page.
        /// </value>
        protected Guid? GroupMemberDetailPage => GetAttributeValue( AttributeKeys.GroupMemberDetailPage ).AsGuidOrNull();

        /// <summary>
        /// Gets a value indicating whether to show the 'Include Inactive' filter option.
        /// </summary>
        /// <value><c>true</c> if [show inactive]; otherwise, <c>false</c>.</value>
        protected bool ShowInactiveMembersFilter => GetAttributeValue( AttributeKeys.ShowInactiveMembersFilter ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether to show the group role type filter option.
        /// </summary>
        /// <value><c>true</c> if [show group role type filter]; otherwise, <c>false</c>.</value>
        protected bool ShowGroupRoleTypeFilter => GetAttributeValue( AttributeKeys.ShowGroupRoleTypeFilter ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether to show the group role filter.
        /// </summary>
        /// <value><c>true</c> if [show group role filter]; otherwise, <c>false</c>.</value>
        protected bool ShowGroupRoleFilter => GetAttributeValue( AttributeKeys.ShowGroupRoleFilter ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether to show the gender filter option.
        /// </summary>
        /// <value><c>true</c> if [show gender filter]; otherwise, <c>false</c>.</value>
        protected bool ShowGenderFilter => GetAttributeValue( AttributeKeys.ShowGenderFilter ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether to show 'Unknown' as a gender filter option.
        /// </summary>
        /// <value><c>true</c> if [show unknown as gender filter option]; otherwise, <c>false</c>.</value>
        protected bool ShowUnknownAsGenderFilterOption => GetAttributeValue( AttributeKeys.ShowUnknownAsGenderFilterOption ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether to show the child group filter option.
        /// </summary>
        /// <value><c>true</c> if [show child group filter]; otherwise, <c>false</c>.</value>
        protected bool ShowChildGroupFilter => GetAttributeValue( AttributeKeys.ShowChildGroupsFilter ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether to show the attendance filter.
        /// </summary>
        /// <value><c>true</c> if [show attendance filter]; otherwise, <c>false</c>.</value>
        protected bool ShowAttendanceFilter => GetAttributeValue( AttributeKeys.ShowAttendanceFilter ).AsBoolean();

        /// <summary>
        /// Gets the attendance filter short period.
        /// </summary>
        /// <value>The attendance filter short period.</value>
        protected int? AttendanceFilterShortWeekRange => GetAttributeValue( AttributeKeys.AttendanceFilterShortWeekRange ).AsIntegerOrNull();

        /// <summary>
        /// Gets the attendance filter long period.
        /// </summary>
        /// <value>The attendance filter long period.</value>
        protected int? AttendanceFilterLongWeekRange => GetAttributeValue( AttributeKeys.AttendanceFilterLongWeekRange ).AsIntegerOrNull();

        /// <summary>
        /// Gets the template.
        /// </summary>
        /// <value>
        /// The template.
        /// </value>
        protected string Template => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKeys.Template ) );

        /// <summary>
        /// Gets a value indicating whether [group by person].
        /// </summary>
        /// <value><c>true</c> if [group by person]; otherwise, <c>false</c>.</value>
        protected bool GroupByPerson => GetAttributeValue( AttributeKeys.GroupByPerson ).AsBooleanOrNull() ?? false;

        #endregion

        /// <summary>
        /// The page parameter keys for the GroupMemberList block.
        /// </summary>
        public static class PageParameterKeys
        {
            /// <summary>
            /// The group identifier
            /// </summary>
            public const string GroupGuid = "GroupGuid";
        }

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 1 );

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Groups.GroupMemberList.Configuration
            {
                Template = Template,
                GroupMemberDetailPage = GroupMemberDetailPage,
                ShowGroupRoleTypeFilter = ShowGroupRoleTypeFilter,
                ShowGenderFilter = ShowGenderFilter,
                ShowInactiveMembersFilter = ShowInactiveMembersFilter,
                ShowAttendanceFilter = ShowAttendanceFilter,
                ShowGroupRoleFilter = ShowGroupRoleFilter,
                ShowChildGroupFilter = ShowChildGroupFilter,
                AttendanceFilterLongWeekRange = AttendanceFilterLongWeekRange,
                AttendanceFilterShortWeekRange = AttendanceFilterShortWeekRange,
                ShowUnknownAsGenderFilterOption = ShowUnknownAsGenderFilterOption,
                GroupByPerson = GroupByPerson
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the lava template from the list of fields.
        /// </summary>
        /// <returns></returns>
        private string CreateLavaTemplate()
        {
            var fields = GetAttributeValue( AttributeKeys.AdditionalFields ).FromJsonOrNull<List<FieldSetting>>() ?? new List<FieldSetting>();

            Dictionary<string, string> properties;

            if ( GroupByPerson )
            {
                properties = new Dictionary<string, string>
                {
                    { "Id", "Person.Id" },
                    { "Guid", "Person.Guid" },
                    { "FullName", "Person.FullName" },
                    { "FirstName", "Person.FirstName" },
                    { "NickName", "Person.NickName" },
                    { "LastName", "Person.LastName" },
                    { "Roles", "Roles" },
                    { "PhotoId", "Person.PhotoId" }
                };
            }
            else
            {
                properties = new Dictionary<string, string>
                {
                    { "Id", "Id" },
                    { "Guid", "Guid" },
                    { "PersonId", "PersonId" },
                    { "PersonGuid", "Person.Guid" },
                    { "FullName", "Person.FullName" },
                    { "FirstName", "Person.FirstName" },
                    { "NickName", "Person.NickName" },
                    { "LastName", "Person.LastName" },
                    { "GroupRole", "GroupRole.Name" },
                    { "PhotoId", "Person.PhotoId" }
                };
            }


            string publicApplicationRoot = GlobalAttributesCache.Value( "PublicApplicationRoot" ).RemoveTrailingForwardslash();

            //
            // Add a custom field for the PhotoUrl since it needs to be custom formatted.
            //
            fields.Add( new FieldSetting
            {
                Key = "PhotoUrl",
                FieldFormat = FieldFormat.String,
                Value = $@"{{{{ '{publicApplicationRoot}' | Append:item.Person.PhotoUrl }}}}"
            } );

            return MobileHelper.CreateItemLavaTemplate( properties, fields );
        }

        /// <summary>
        /// Filters the group members.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="filterBag">The filter bag.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>IEnumerable&lt;GroupMember&gt;.</returns>
        private static IEnumerable<GroupMember> FilterGroupMembers( Group group, FilterBag filterBag, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            var groupTypeCache = GroupTypeCache.Get( group.GroupTypeId );

            var members = new GroupMemberService( rockContext )
                .Queryable()
                .Where( gm => gm.Group.Guid == group.Guid );

            if ( !filterBag.IncludeInactive )
            {
                members = members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active );
            }

            // If they selected group roles to filter, and the amount of
            // selected group roles does not equal of group roles the amount the group already has.
            var filterByGroupRole = ( filterBag.SelectedGroupRoles?.Any() ?? false )
                && filterBag.SelectedGroupRoles.Count() != groupTypeCache.Roles.Count;

            if ( filterByGroupRole )
            {
                members = members.Where( m => filterBag.SelectedGroupRoles.Contains( m.GroupRole.Guid ) );
            }

            // Filter by gender, we first check if there are any selected
            // gender filters, if the amount of selected genders
            // is the same as the amount contained in the enum (all of them)
            // we shouldn't filter.
            var filterByGender = filterBag.SelectedGenders?.Any() ?? false && filterBag.SelectedGenders.Count() != Enum.GetNames( typeof( Rock.Common.Mobile.Enums.Gender ) ).Length;
            if ( filterByGender )
            {
                var genders = new List<Rock.Common.Mobile.Enums.Gender>();
                foreach ( var selectedGender in filterBag.SelectedGenders )
                {
                    genders.Add( selectedGender );
                }

                members = members.Where( gm => genders.Contains( ( Common.Mobile.Enums.Gender ) gm.Person.Gender ) );
            }

            // Same logic as the gender one above.
            var filterByGroupTypeRole = filterBag.SelectedGroupTypeRoles?.Any() ?? false
                && filterBag.SelectedGroupTypeRoles.Count() != Enum.GetNames( typeof( GroupRoleTypeSpecifier ) ).Length;

            // This will need to be updated if we ever add more specific
            // group role types. Since as of today we only utilize
            // the 'IsLeader' property.
            if ( filterByGroupTypeRole )
            {
                foreach ( var groupTypeRole in filterBag.SelectedGroupTypeRoles )
                {
                    switch ( groupTypeRole )
                    {
                        case GroupRoleTypeSpecifier.Leader:
                            members = members.Where( m => m.GroupRole.IsLeader );
                            break;
                        case GroupRoleTypeSpecifier.Member:
                            members = members.Where( m => !m.GroupRole.IsLeader );
                            break;
                    }
                }
            }

            // Filtering by child group.
            var filterByChildGroup = filterBag.SelectedChildGroups?.Any() ?? false && filterBag.SelectedChildGroups.Count() != group.Groups.Count;
            if ( filterByChildGroup )
            {
                members = members.Where( gm => gm.Person.Members.Any( innerGm => filterBag.SelectedChildGroups.Contains( innerGm.Group.Guid ) ) );
            }

            // Filtering by attendance.
            var filterByAttendance = filterBag.SelectedAttendance != null;
            if ( filterByAttendance )
            {
                var range = filterBag.SelectedAttendance.Range;
                var attendanceFilterType = filterBag.SelectedAttendance.FilterType;

                members = FilterMembersByAttendanceOption( members, group.Id, range, attendanceFilterType, rockContext );
            }

            return members.ToList();
        }

        /// <summary>
        /// Filters the members by attendance option.
        /// See <see cref="AttendanceFilterOptionType" />.
        /// </summary>
        /// <param name="groupMembers">The group members.</param>
        /// <param name="groupId"></param>
        /// <param name="amtOfWeeks">The amt of weeks.</param>
        /// <param name="option">The option.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>IQueryable&lt;GroupMember&gt;.</returns>
        private static IQueryable<GroupMember> FilterMembersByAttendanceOption( IQueryable<GroupMember> groupMembers, int groupId, int amtOfWeeks, AttendanceFilterOptionType option, RockContext rockContext )
        {
            // Switch our selected attendance option from the shell.
            switch ( option )
            {
                // If we want to retrieve group members where there is not an attendance for x number of weeks.
                case AttendanceFilterOptionType.NoAttendance:
                    return GroupMemberService.WhereMembersWithNoAttendanceForNumberOfWeeks( groupMembers, groupId, amtOfWeeks, rockContext );
                // If we want to retrieve group members who first attended within an x number of weeks.
                case AttendanceFilterOptionType.FirstAttended:
                    return GroupMemberService.WhereMembersWhoFirstAttendedWithinNumberOfWeeks( groupMembers, groupId, amtOfWeeks, rockContext );
                // If we want to retrieve group members who attended within an x number of weeks.
                case AttendanceFilterOptionType.Attended:
                    return GroupMemberService.WhereMembersWhoAttendedWithinNumberOfWeeks( groupMembers, groupId, amtOfWeeks, rockContext );
            }

            return groupMembers;
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the group details with filter options.
        /// </summary>
        /// <param name="filterBag">The filter bag.</param>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult GetGroupDetailsWithFilterOptions( FilterBag filterBag )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupGuid = RequestContext.GetPageParameter( PageParameterKeys.GroupGuid ).AsGuid();

                // Get our group from the Group Guid.
                var group = new GroupService( rockContext )
                    .Queryable()
                    .Include( g => g.Groups )
                    .Where( g => g.Guid == groupGuid )
                    .FirstOrDefault();

                if ( group == null )
                {
                    return ActionNotFound();
                }

                IEnumerable<GroupMember> groupMembers;
                int totalGroupMemberCount;

                // This really shouldn't ever be null, and in most cases
                // the shell just passes up an empty filter, but if it is
                // we just want to use all of the active group members.
                if ( filterBag == null )
                {
                    groupMembers = new GroupMemberService( rockContext )
                        .Queryable()
                        .Include( gm => gm.Person )
                        .Where( gm => gm.Group.Guid == groupGuid )
                        .ToList();

                    totalGroupMemberCount = groupMembers.Count();
                }
                // Otherwise, filter the group members according to our filter bag values.
                else
                {
                    groupMembers = FilterGroupMembers( group, filterBag, rockContext );

                    if ( filterBag.IncludeInactive )
                    {
                        totalGroupMemberCount = new GroupMemberService( rockContext )
                        .Queryable()
                        .Where( gm => gm.Group.Guid == groupGuid )
                        .Count();
                    }
                    else
                    {
                        totalGroupMemberCount = new GroupMemberService( rockContext )
                        .Queryable()
                        .Where( gm => gm.Group.Guid == groupGuid && gm.GroupMemberStatus == GroupMemberStatus.Active )
                        .Count();
                    }
                }

                var lavaTemplate = CreateLavaTemplate();
                var mergeFields = RequestContext.GetCommonMergeFields();

                if ( GroupByPerson )
                {
                    var groupedByPersonMembers = groupMembers.Select( gm => new
                    {
                        Person = gm.Person,
                        Roles = new GroupMemberService( rockContext ).GetByGroupIdAndPersonId( gm.GroupId, gm.PersonId ).Select( member => member.GroupRole.Name ).ToList().AsDelimited( ", " )
                    } ).Distinct();

                    totalGroupMemberCount = groupedByPersonMembers.Count();
                    mergeFields.Add( "Items", groupedByPersonMembers );
                }
                else
                {
                    mergeFields.Add( "Items", groupMembers );
                }

                mergeFields.Add( "Group", group );

                var title = TitleTemplate.ResolveMergeFields( mergeFields );
                var memberJson = lavaTemplate.ResolveMergeFields( mergeFields );

                // This is about 1,000x faster than .FromJsonDynamic() --dsh
                var members = Newtonsoft.Json.Linq.JToken.Parse( memberJson );

                var groupRoleTypes = new List<ListItemViewModel>()
                {
                    new ListItemViewModel
                    {
                        Text = "Is Leader",
                        Value = GroupRoleTypeSpecifier.Leader.ToString()
                    },
                    new ListItemViewModel
                    {
                        Text = "Is Member",
                        Value = GroupRoleTypeSpecifier.Member.ToString()
                    }
                };

                var groupTypeCache = GroupTypeCache.Get( group.GroupTypeId );

                // We need to send the shell a list of roles
                // that they can filter by.
                var groupRoles = groupTypeCache.Roles.Select( r => new ListItemViewModel
                {
                    Text = r.Name,
                    Value = r.Guid.ToString()
                } ).ToList();

                // We also need to send the shell a list of child groups
                // that they can filter by.
                var childGroups = group.Groups.Where( g => g.IsActive && g.IsPublic && !g.IsArchived )
                    .Select( g => new ListItemViewModel
                    {
                        Text = g.Name,
                        Value = g.Guid.ToString()
                    } ).ToList();

                var groupDetailBag = new GroupDetailBag
                {
                    GroupRoles = groupRoles,
                    GroupRoleTypes = groupRoleTypes,
                    ChildGroups = childGroups,
                    Members = members,
                    TotalGroupMemberCount = totalGroupMemberCount,
                    Title = title
                };

                return ActionOk( groupDetailBag );
            }
        }

        /// <summary>
        /// Gets the group details.
        /// </summary>
        /// <remarks>We only use this in shell version 4 and below. You should use the <see cref="GetGroupDetailsWithFilterOptions(FilterBag)"/></remarks>
        /// <returns></returns>
        [BlockAction]
        public object GetGroupDetails()
        {
            using ( var rockContext = new RockContext() )
            {
                var groupGuid = RequestContext.GetPageParameter( PageParameterKeys.GroupGuid ).AsGuid();
                var group = new GroupService( rockContext ).Get( groupGuid );

                var lavaTemplate = CreateLavaTemplate();

                var mergeFields = RequestContext.GetCommonMergeFields();
                mergeFields.Add( "Items", group.Members );
                mergeFields.Add( "Group", group );

                var title = TitleTemplate.ResolveMergeFields( mergeFields );
                var memberJson = lavaTemplate.ResolveMergeFields( mergeFields );

                // This is about 1,000x faster than .FromJsonDynamic() --dsh
                var members = Newtonsoft.Json.Linq.JToken.Parse( memberJson );

                return new
                {
                    Title = title,
                    Members = members
                };
            }
        }

        #endregion

        #region Custom Settings

        /// <summary>
        /// Defines the control that will provide additional Basic Settings tab content
        /// for the GroupMemberList block.
        /// </summary>
        /// <seealso cref="Rock.Web.RockCustomSettingsProvider" />
        [CustomSettingsBlockType( typeof( GroupMemberList ), SiteType.Mobile )]
        public class GroupMemberListCustomSettingsProvider : Rock.Web.RockCustomSettingsProvider
        {
            /// <summary>
            /// Gets the custom settings title. Used when displaying tabs or links to these settings.
            /// </summary>
            /// <value>
            /// The custom settings title.
            /// </value>
            public override string CustomSettingsTitle => "Basic Settings";

            /// <summary>
            /// Gets the custom settings control. The returned control will be added to the parent automatically.
            /// </summary>
            /// <param name="attributeEntity">The attribute entity.</param>
            /// <param name="parent">The parent control that will eventually contain the returned control.</param>
            /// <returns>
            /// A control that contains all the custom UI.
            /// </returns>
            public override Control GetCustomSettingsControl( IHasAttributes attributeEntity, Control parent )
            {
                var pnlContent = new Panel();

                var jfBuilder = new JsonFieldsBuilder
                {
                    Label = "Additional Fields",
                    SourceType = typeof( GroupMember ),
                    AvailableAttributes = AttributeCache.AllForEntityType<GroupMember>()
                        .Where( a => a.EntityTypeQualifierColumn.IsNullOrWhiteSpace() && a.EntityTypeQualifierValue.IsNullOrWhiteSpace() )
                        .ToList()
                };

                pnlContent.Controls.Add( jfBuilder );

                return pnlContent;
            }

            /// <summary>
            /// Update the custom UI to reflect the current settings found in the entity.
            /// </summary>
            /// <param name="attributeEntity">The attribute entity.</param>
            /// <param name="control">The control returned by GetCustomSettingsControl() method.</param>
            public override void ReadSettingsFromEntity( IHasAttributes attributeEntity, Control control )
            {
                var jfBuilder = ( JsonFieldsBuilder ) control.Controls[0];

                jfBuilder.FieldSettings = attributeEntity.GetAttributeValue( AttributeKeys.AdditionalFields )
                    .FromJsonOrNull<List<FieldSetting>>();
            }

            /// <summary>
            /// Update the entity with values from the custom UI.
            /// </summary>
            /// <param name="attributeEntity">The attribute entity.</param>
            /// <param name="control">The control returned by the GetCustomSettingsControl() method.</param>
            /// <param name="rockContext">The rock context to use when accessing the database.</param>
            public override void WriteSettingsToEntity( IHasAttributes attributeEntity, Control control, RockContext rockContext )
            {
                var jfBuilder = ( JsonFieldsBuilder ) control.Controls[0];

                attributeEntity.SetAttributeValue( AttributeKeys.AdditionalFields, jfBuilder.FieldSettings.ToJson() );
            }
        }

        #endregion
    }
}
