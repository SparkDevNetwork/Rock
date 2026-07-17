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
using System.Linq;

using Rock.Attribute;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Lists the groups the current person is a member of, rendered with a configurable Lava template.
    /// </summary>
    [DisplayName( "Group List Personalized Lava" )]
    [Category( "Groups" )]
    [Description( "Lists all group that the person is a member of using a Lava template." )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ConfigurationChangedReload( Rock.Enums.Cms.BlockReloadMode.Page )]

    #region Block Attributes

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage,
        Description = "",
        IsRequired = false,
        Order = 0 )]

    [GroupField( "Parent Group",
        Key = AttributeKey.ParentGroup,
        Description = "If a group is chosen, only the groups under this group will be displayed.",
        IsRequired = false,
        Order = 1 )]

    [IntegerField( "Cache Duration",
        Key = AttributeKey.CacheDuration,
        Description = "Length of time in seconds to cache which groups are descendants of the parent group.",
        IsRequired = false,
        DefaultIntegerValue = 3600,
        Order = 2 )]

    [GroupTypesField( "Include Group Types",
        Key = AttributeKey.IncludeGroupTypes,
        Description = "The group types to display in the list.  If none are selected, all group types will be included.",
        IsRequired = false,
        Order = 3 )]

    [GroupTypesField( "Exclude Group Types",
        Key = AttributeKey.ExcludeGroupTypes,
        Description = "The group types to exclude from the list (only valid if including all groups).",
        IsRequired = false,
        Order = 4 )]

    [CodeEditorField( "Lava Template",
        Key = AttributeKey.LavaTemplate,
        Description = "The lava template to use to format the group list.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = "{% include '~~/Assets/Lava/GroupListSidebar.lava' %}",
        Order = 5 )]

    [BooleanField( "Display Inactive Groups",
        Key = AttributeKey.DisplayInactiveGroups,
        Description = "Include inactive groups in the lava results",
        DefaultBooleanValue = false,
        Order = 6 )]

    [CustomDropdownListField( "Initial Active Setting",
        Key = AttributeKey.InitialActiveSetting,
        Description = "Select whether to initially show all or just active groups in the lava.",
        ListSource = "0^All,1^Active",
        IsRequired = false,
        DefaultValue = "1",
        Order = 7 )]

    [TextField( "Inactive Parameter Name",
        Key = AttributeKey.InactiveParameterName,
        Description = "The page parameter name to toggle inactive groups.",
        IsRequired = false,
        DefaultValue = "showinactivegroups",
        Order = 8 )]

    [CustomCheckboxListField( "Cache Tags",
        Key = AttributeKey.CacheTags,
        Description = "Cached tags are used to link cached content so that it can be expired as a group.",
        ListSource = CACHE_TAG_LIST,
        IsRequired = false,
        Order = 9 )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Navigation )]
    [Rock.SystemGuid.EntityTypeGuid( "C705A204-FDE6-44BB-9E24-513F1E1BBED7" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "E79DAF8D-7BC2-4C4B-ADFA-A96B3044756E" )]
    [Rock.SystemGuid.BlockTypeGuid( "1B172C33-8672-4C98-A995-8E123FF316BD" )]
    public class GroupListPersonalizedLava : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string ParentGroup = "ParentGroup";
            public const string CacheDuration = "CacheDuration";
            public const string IncludeGroupTypes = "IncludeGroupTypes";
            public const string ExcludeGroupTypes = "ExcludeGroupTypes";
            public const string LavaTemplate = "LavaTemplate";
            public const string DisplayInactiveGroups = "DisplayInactiveGroups";
            public const string InitialActiveSetting = "InitialActiveSetting";
            public const string InactiveParameterName = "InactiveParameterName";
            public const string CacheTags = "CacheTags";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// SQL that provides the selectable values for the Cache Tags block setting.
        /// </summary>
        private const string CACHE_TAG_LIST = @"
            SELECT CAST([DefinedValue].[Value] AS VARCHAR) AS [Value], [DefinedValue].[Value] AS [Text]
            FROM [DefinedType]
            JOIN [DefinedValue] ON [DefinedType].[Id] = [DefinedValue].[DefinedTypeId]
            WHERE [DefinedType].[Guid] = 'BDF73089-9154-40C1-90E4-74518E9937DC'";

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            try
            {
                return GetGroupsHtml();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                return "<div class='alert alert-danger'>An error occurred while getting the group list.</div>";
            }
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        /// Resolves the configured Lava template against the current person's group involvements.
        /// </summary>
        /// <returns>The rendered HTML.</returns>
        private string GetGroupsHtml()
        {
            var displayInactiveGroups = GetAttributeValue( AttributeKey.DisplayInactiveGroups ).AsBoolean();
            var groups = GetGroupInvolvements( IsHidingInactiveGroups( displayInactiveGroups ) );

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Groups", groups );

            var linkedPages = new Dictionary<string, object>
            {
                [AttributeKey.DetailPage] = new PageReference( GetAttributeValue( AttributeKey.DetailPage ) ).Route
            };
            mergeFields.Add( "LinkedPages", linkedPages );

            if ( displayInactiveGroups )
            {
                mergeFields.Add( "ShowInactive", GetAttributeValue( AttributeKey.DisplayInactiveGroups ) );
                mergeFields.Add( "InitialActive", GetAttributeValue( AttributeKey.InitialActiveSetting ) );
                mergeFields.Add( "InactiveParameter", GetAttributeValue( AttributeKey.InactiveParameterName ) );
            }

            // Resolve theme and application relative URLs in the template before rendering.
            var template = GetAttributeValue( AttributeKey.LavaTemplate ) ?? string.Empty;
            var appRoot = RequestContext.ResolveRockUrl( "~/" );
            var themeRoot = RequestContext.ResolveRockUrl( "~~/" );
            template = template.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );

            return template.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Determines whether inactive groups and memberships should be hidden. When the block
        /// displays inactive groups, the configurable page parameter toggles the initial-active
        /// block setting.
        /// </summary>
        /// <param name="displayInactiveGroups">Whether the block is configured to display inactive groups.</param>
        /// <returns><c>true</c> if inactive groups should be hidden; otherwise <c>false</c>.</returns>
        private bool IsHidingInactiveGroups( bool displayInactiveGroups )
        {
            if ( !displayInactiveGroups )
            {
                return true;
            }

            var showInactiveGroups = PageParameter( GetAttributeValue( AttributeKey.InactiveParameterName ) ).AsBooleanOrNull();
            if ( showInactiveGroups.HasValue )
            {
                return !showInactiveGroups.Value;
            }

            return GetAttributeValue( AttributeKey.InitialActiveSetting ) == "1";
        }

        /// <summary>
        /// Gets the current person's group involvements that they are authorized to view,
        /// scoped and filtered by the block settings.
        /// </summary>
        /// <param name="hideInactive">Whether inactive groups and memberships should be excluded.</param>
        /// <returns>The list of group involvements for the Lava template.</returns>
        private List<GroupInvolvementInfo> GetGroupInvolvements( bool hideInactive )
        {
            var currentPerson = GetCurrentPerson();
            if ( currentPerson == null )
            {
                return new List<GroupInvolvementInfo>();
            }

            var qry = new GroupMemberService( RockContext )
                .Queryable( "Group" )
                .Where( m => m.PersonId == currentPerson.Id );

            var availableGroupIds = GetAvailableGroupIds();
            if ( availableGroupIds != null )
            {
                qry = qry.Where( m => availableGroupIds.Contains( m.GroupId ) );
            }

            if ( hideInactive )
            {
                qry = qry.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active );
                qry = qry.Where( m => m.Group.IsActive == true && !m.Group.IsArchived );
            }

            /*
                04/20/2022 - KA

                The GroupType filtering should use an if/else clause with the IncludeGroupTypes taking priority over the ExcludeGroupTypes
                (refer to ReminderService.GetReminderEntityTypesByPerson for how it should work). Thus if any GroupTypes are selected as
                part of the IncludeGroupTypes they should not be excluded even if they are selected as part of the ExcludeGroupTypes. This
                implementation has been left as it is because it would be too late/risky to change the behavior now since people/admins
                have already configured it and it is working the way it is working now.
            */

            /*
                7/13/26 - MSE

                These filters are gated on the configured guids rather than the resolved
                identifiers. If an include list only contains group types that no longer
                exist, the filter must still be applied (matching nothing) so the block
                lists no groups, which is how the WebForms guid-based filtering behaved.
                Gating on the resolved identifiers would skip the filter and list every
                group the person is a member of.

                Reason: Preserve WebForms behavior when configured group types no longer exist.
            */

            var includeGroupTypeGuids = GetAttributeValue( AttributeKey.IncludeGroupTypes ).SplitDelimitedValues().AsGuidList();
            if ( includeGroupTypeGuids.Any() )
            {
                var includeGroupTypeIds = GetGroupTypeIds( includeGroupTypeGuids );
                qry = qry.Where( m => includeGroupTypeIds.Contains( m.Group.GroupTypeId ) );
            }

            var excludeGroupTypeGuids = GetAttributeValue( AttributeKey.ExcludeGroupTypes ).SplitDelimitedValues().AsGuidList();
            if ( excludeGroupTypeGuids.Any() )
            {
                var excludeGroupTypeIds = GetGroupTypeIds( excludeGroupTypeGuids );
                qry = qry.Where( m => !excludeGroupTypeIds.Contains( m.Group.GroupTypeId ) );
            }

            var involvements = new List<GroupInvolvementInfo>();

            foreach ( var groupMember in qry.ToList() )
            {
                var groupType = GroupTypeCache.Get( groupMember.Group.GroupTypeId );
                var role = groupType?.Roles.FirstOrDefault( r => r.Id == groupMember.GroupRoleId );

                if ( role == null || !( role.CanView || role.CanTakeAttendance ) )
                {
                    continue;
                }

                if ( !groupMember.Group.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    continue;
                }

                involvements.Add( new GroupInvolvementInfo
                {
                    Group = groupMember.Group,
                    Role = role.Name,
                    IsLeader = role.IsLeader,
                    GroupType = groupType.Name
                } );
            }

            return involvements;
        }

        /// <summary>
        /// Gets the identifiers of the groups under the configured parent group, honoring the
        /// cache duration and cache tags block settings.
        /// </summary>
        /// <returns>The descendant group identifiers, or <c>null</c> when no parent group is configured.</returns>
        private List<int> GetAvailableGroupIds()
        {
            var parentGroupGuid = GetAttributeValue( AttributeKey.ParentGroup ).AsGuidOrNull();
            if ( !parentGroupGuid.HasValue )
            {
                return null;
            }

            var cacheDuration = GetAttributeValue( AttributeKey.CacheDuration ).AsInteger();
            var cacheKey = $"GroupListPersonalizedLava:{BlockId}:{parentGroupGuid.Value}";

            if ( cacheDuration > 0 && RockCache.Get( cacheKey ) is List<int> cachedGroupIds )
            {
                return cachedGroupIds;
            }

            var groupService = new GroupService( RockContext );
            var parentGroupId = groupService.GetId( parentGroupGuid.Value );

            /*
                7/13/26 - MSE

                The WebForms version of this block gathered descendant groups by recursively
                walking the Group.Groups navigation property, which included archived groups.
                GetAllDescendentGroupIds uses the descendants CTE, which always excludes
                archived groups. This difference is intentional: archived groups are treated
                as soft-deleted throughout Rock, so memberships in archived descendant groups
                should not be listed even when the block is showing inactive groups.

                Reason: Intentionally exclude archived descendant groups, unlike the WebForms block.
            */
            var availableGroupIds = parentGroupId.HasValue
                ? groupService.GetAllDescendentGroupIds( parentGroupId.Value, includeInactiveChildGroups: true )
                : new List<int>();

            if ( cacheDuration > 0 )
            {
                var cacheTags = GetAttributeValue( AttributeKey.CacheTags ) ?? string.Empty;
                RockCache.AddOrUpdate( cacheKey, null, availableGroupIds, TimeSpan.FromSeconds( cacheDuration ), cacheTags );
            }

            return availableGroupIds;
        }

        /// <summary>
        /// Resolves group type guids into group type identifiers, skipping any guids
        /// that no longer resolve to a group type.
        /// </summary>
        /// <param name="groupTypeGuids">The group type guids.</param>
        /// <returns>The group type identifiers.</returns>
        private List<int> GetGroupTypeIds( List<Guid> groupTypeGuids )
        {
            return groupTypeGuids
                .Select( guid => GroupTypeCache.Get( guid ) )
                .Where( groupType => groupType != null )
                .Select( groupType => groupType.Id )
                .ToList();
        }

        #endregion Private Methods

        #region Support Classes

        /// <summary>
        /// A summary of the current person's involvement in a single group for use in the Lava template.
        /// </summary>
        public class GroupInvolvementInfo : LavaDataObject
        {
            /// <summary>
            /// Gets or sets the group the person is a member of.
            /// </summary>
            public Rock.Model.Group Group { get; set; }

            /// <summary>
            /// Gets or sets the name of the person's role in the group.
            /// </summary>
            public string Role { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the person's role is a leader role.
            /// </summary>
            public bool IsLeader { get; set; }

            /// <summary>
            /// Gets or sets the name of the group's type.
            /// </summary>
            public string GroupType { get; set; }
        }

        #endregion Support Classes
    }
}
