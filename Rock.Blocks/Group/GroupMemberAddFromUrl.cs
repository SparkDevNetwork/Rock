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
using System.ComponentModel;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupMemberAddFromUrl;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Adds a person to a group based on inputs from the URL query string.
    /// </summary>
    [DisplayName( "Group Member Add From URL" )]
    [Category( "Groups" )]
    [Description( "Adds a person to a group based on inputs from the URL query string." )]

    #region Block Attributes

    [GroupField(
        "Default Group",
        Description = "The default group to use if one is not passed through the query string (optional).",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.DefaultGroup )]

    [GroupRoleField( null,
        "Default Group Member Role",
        Description = "The default role to use if one is not passed through the query string (optional).",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.DefaultGroupMemberRole )]

    [EnumField(
        "Group Member Status",
        Description = "The status to use when adding a person to the group.",
        EnumSourceType = typeof( GroupMemberStatus ),
        IsRequired = true,
        DefaultValue = "Active",
        Order = 2,
        Key = AttributeKey.GroupMemberStatus )]

    [CodeEditorField(
        "Success Message",
        Description = "Lava template to display when person has been added to the group.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 300,
        IsRequired = true,
        DefaultValue = @"<div class='alert alert-success'>
    {{ Person.NickName }} has been added to the group '{{ Group.Name }}' with the role of {{ Role.Name }}.
</div>",
        Order = 3,
        Key = AttributeKey.SuccessMessage )]

    [CodeEditorField(
        "Already In Group Message",
        Description = "Lava template to display when person is already in the group with that role.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 300,
        IsRequired = true,
        DefaultValue = @"<div class='alert alert-warning'>
    {{ Person.NickName }} is already in the group '{{ Group.Name }}' with the role of {{ Role.Name }}.
</div>",
        Order = 4,
        Key = AttributeKey.AlreadyInGroupMessage )]

    [GroupTypesField(
        "Limit Group Type",
        Description = "To ensure that people cannot modify the URL and try adding themselves to standard Rock security groups with known Id numbers you can limit which Group Type that are considered valid during add.",
        IsRequired = false,
        Order = 5,
        Key = AttributeKey.LimitGroupType )]

    [BooleanField(
        "Enable Passing Group Id",
        Description = "If enabled, allows the ability to pass in a group's Id (GroupId=) instead of the Guid.",
        DefaultBooleanValue = true,
        Order = 6,
        Key = AttributeKey.EnablePassingGroupId )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "FEB8132D-8ED9-447D-A0D9-574A3EE0A98E" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "1A7F3BF3-B1EF-451D-A4F1-7A1B7EBC8EE9" )]
    [Rock.SystemGuid.BlockTypeGuid( "42CF3822-A70C-4E07-9394-21607EED7018" )]
    public class GroupMemberAddFromUrl : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DefaultGroup = "DefaultGroup";
            public const string DefaultGroupMemberRole = "DefaultGroupMemberRole";
            public const string SuccessMessage = "SuccessMessage";
            public const string AlreadyInGroupMessage = "AlreadyInGroupMessage";
            public const string GroupMemberStatus = "GroupMemberStatus";
            public const string LimitGroupType = "LimitGroupType";
            public const string EnablePassingGroupId = "EnablePassingGroupId";
        }

        /*
            7/10/26 - MSE

            The query-string keys below intentionally keep their original,
            entity-specific names (GroupGuid, GroupId, GroupMemberRoleId, PersonGuid)
            rather than the modern single-parameter convention. This block is linked
            from external URLs (emails, bulletins) that already exist in the wild, so
            renaming the parameters would break those links.

            Reason: Preserve the established external-link contract for this block.
        */
        private static class PageParameterKey
        {
            public const string GroupGuid = "GroupGuid";
            public const string GroupId = "GroupId";
            public const string GroupMemberRoleId = "GroupMemberRoleId";
            public const string PersonGuid = "PersonGuid";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<GroupMemberAddFromUrlBag, GroupMemberAddFromUrlOptionsBag>();

            box.Bag = new GroupMemberAddFromUrlBag
            {
                Content = ProcessAddRequest()
            };

            return box;
        }

        /// <summary>
        /// Resolves the group, role, and person from the URL, adds the person to the
        /// group, and returns the HTML content to display to the user.
        /// </summary>
        /// <returns>The rendered HTML message (a Lava result or an alert).</returns>
        private string ProcessAddRequest()
        {
            var groupService = new GroupService( RockContext );

            var group = GetGroup( groupService );
            if ( group == null )
            {
                return Alert( "Could not determine the group to add to." );
            }

            var groupType = GroupTypeCache.Get( group.GroupTypeId );

            // Restrict which group types may be targeted so a modified URL cannot add
            // a person to an arbitrary security group with a known Id.
            var limitGroupTypeValue = GetAttributeValue( AttributeKey.LimitGroupType );
            if ( limitGroupTypeValue.IsNotNullOrWhiteSpace() )
            {
                var allowedGroupTypeGuids = limitGroupTypeValue.SplitDelimitedValues();
                var isGroupTypeAllowed = groupType != null
                    && allowedGroupTypeGuids.Contains( groupType.Guid.ToString(), StringComparer.OrdinalIgnoreCase );

                if ( !isGroupTypeAllowed )
                {
                    return Alert( "Invalid group specified." );
                }
            }

            var groupMemberRole = GetGroupMemberRole( group );
            if ( groupMemberRole == null )
            {
                return Alert( "Could not determine the group role to use for the add." );
            }

            var personGuid = PageParameter( PageParameterKey.PersonGuid ).AsGuidOrNull();
            if ( !personGuid.HasValue )
            {
                return Alert( "A valid person identifier was not found in the page address." );
            }

            // Ensure the resolved role actually belongs to the target group's type.
            if ( groupMemberRole.GroupTypeId != group.GroupTypeId )
            {
                return Alert( "The group you have provided does not have the group member role configured." );
            }

            var person = new PersonService( RockContext ).Get( personGuid.Value );
            if ( person == null )
            {
                return Alert( "A person could not be found for the identifier provided." );
            }

            var groupMemberStatus = GetAttributeValue( AttributeKey.GroupMemberStatus ).ConvertToEnum<GroupMemberStatus>( GroupMemberStatus.Active );

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "GroupMemberStatus", groupMemberStatus.ToString() );
            mergeFields.Add( "Group", group );
            mergeFields.Add( "Person", person );
            mergeFields.Add( "Role", groupMemberRole );

            var groupMemberService = new GroupMemberService( RockContext );

            // Only a current (non-archived) membership counts as "already in the group".
            // Deceased members are included to match the original block's behavior, but
            // archived members are intentionally excluded here so they fall through and are
            // restored below rather than being blocked with the "already in group" message.
            var isAlreadyInGroup = groupMemberService.Queryable( includeDeceased: true )
                .Any( m => m.GroupId == group.Id
                    && m.PersonId == person.Id
                    && m.GroupRoleId == groupMemberRole.Id );

            if ( isAlreadyInGroup )
            {
                return GetAttributeValue( AttributeKey.AlreadyInGroupMessage ).ResolveMergeFields( mergeFields );
            }

            // Add the person to the group. If a matching archived membership already exists,
            // it is restored (un-archived) instead of inserting a duplicate record.
            var groupMember = groupMemberService.AddOrRestoreGroupMember( group, person.Id, groupMemberRole.Id );
            groupMember.GroupMemberStatus = groupMemberStatus;

            /*
                7/10/26 - MSE

                Only pre-validate brand-new members (Id == 0). Restored members already have
                an Id and should not run IsValidGroupMember here — GroupMember.SaveHook
                intentionally bypasses requirement/capacity checks when un-archiving so the
                person can rejoin and show the "does not meet" indicator in the member list.

                Reason: Match SaveHook un-archive behavior for AddOrRestoreGroupMember restores.
            */
            if ( groupMember.Id == 0 && !groupMember.IsValidGroupMember( RockContext ) )
            {
                var validationMessage = groupMember.ValidationResults
                    .Select( r => r.ErrorMessage )
                    .ToList()
                    .AsDelimited( "<br />" );

                return Alert( validationMessage );
            }

            try
            {
                RockContext.SaveChanges();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                return Alert( "An error occurred adding this person to the group. Please try again later." );
            }

            return GetAttributeValue( AttributeKey.SuccessMessage ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Resolves the target group from the URL parameters or the block's default group setting.
        /// </summary>
        /// <param name="groupService">The group service to query with.</param>
        /// <returns>The resolved <see cref="Model.Group"/>, or <c>null</c> if none could be determined.</returns>
        private Model.Group GetGroup( GroupService groupService )
        {
            // 1. Group passed by Guid on the URL.
            var groupGuid = PageParameter( PageParameterKey.GroupGuid ).AsGuidOrNull();
            if ( groupGuid.HasValue )
            {
                var group = groupService.Get( groupGuid.Value );
                if ( group != null )
                {
                    return group;
                }
            }

            // 2. Group passed by Id/IdKey on the URL, only when the block permits it.
            var isPassingGroupIdEnabled = GetAttributeValue( AttributeKey.EnablePassingGroupId ).AsBoolean( true );
            if ( isPassingGroupIdEnabled )
            {
                var groupIdKey = PageParameter( PageParameterKey.GroupId );
                if ( groupIdKey.IsNotNullOrWhiteSpace() )
                {
                    var group = groupService.Get( groupIdKey, allowIntegerIdentifier: true );
                    if ( group != null )
                    {
                        return group;
                    }
                }
            }

            // 3. Fall back to the block's default group setting.
            var defaultGroupGuid = GetAttributeValue( AttributeKey.DefaultGroup ).AsGuidOrNull();
            if ( defaultGroupGuid.HasValue )
            {
                return groupService.Get( defaultGroupGuid.Value );
            }

            return null;
        }

        /// <summary>
        /// Resolves the group member role from the URL parameter, the block's default role
        /// setting, or the group type's configured default role, in that order.
        /// </summary>
        /// <param name="group">The target group.</param>
        /// <returns>The resolved <see cref="GroupTypeRole"/>, or <c>null</c> if none could be determined.</returns>
        private GroupTypeRole GetGroupMemberRole( Model.Group group )
        {
            var roleService = new GroupTypeRoleService( RockContext );

            // 1. Role passed on the URL (Id, IdKey, or Guid). Integer Ids are always
            //    allowed here, matching both the original WebForms block and the GroupId
            //    handling above, so existing external links that pass a numeric role Id keep
            //    working even on sites that have disabled predictable Ids.
            var roleKey = PageParameter( PageParameterKey.GroupMemberRoleId );
            if ( roleKey.IsNotNullOrWhiteSpace() )
            {
                return roleService.Get( roleKey, allowIntegerIdentifier: true );
            }

            // 2. Default role from the block setting.
            var defaultRoleGuid = GetAttributeValue( AttributeKey.DefaultGroupMemberRole ).AsGuidOrNull();
            if ( defaultRoleGuid.HasValue )
            {
                return roleService.Get( defaultRoleGuid.Value );
            }

            // 3. The group type's configured default role.
            var defaultRoleId = GroupTypeCache.Get( group.GroupTypeId )?.DefaultGroupRoleId;
            return defaultRoleId.HasValue ? roleService.Get( defaultRoleId.Value ) : null;
        }

        /// <summary>
        /// Wraps a message in a warning alert for display.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <returns>The alert HTML.</returns>
        private string Alert( string message )
        {
            return $"<div class='alert alert-warning'>{message}</div>";
        }

        #endregion Methods
    }
}
