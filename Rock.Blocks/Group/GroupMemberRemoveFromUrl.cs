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

using System.ComponentModel;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupMemberRemoveFromUrl;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Removes a person from a group based on inputs from the URL query string.
    /// </summary>
    [DisplayName( "Group Member Remove From URL" )]
    [Category( "Groups" )]
    [Description( "Removes a person from a group based on inputs from the URL query string (GroupId, PersonGuid)." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [GroupField( "Default Group",
        Description = "The default group to use if one is not passed through the query string (optional).",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.DefaultGroup )]

    [CodeEditorField( "Success Message",
        Description = "Lava template to display when the person has been removed from the group.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 300,
        IsRequired = true,
        DefaultValue = @"<div class='alert alert-success'>
    {{ Person.NickName }} has been removed from the group '{{ Group.Name }}'.
</div>",
        Order = 1,
        Key = AttributeKey.SuccessMessage )]

    [CodeEditorField( "Not In Group Message",
        Description = "Lava template to display when the person is not in the group.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 300,
        IsRequired = true,
        DefaultValue = @"<div class='alert alert-warning'>
    {{ Person.NickName }} was not in the group '{{ Group.Name }}'.
</div>",
        Order = 2,
        Key = AttributeKey.NotInGroupMessage )]

    [BooleanField( "Warn When Not In Group",
        Description = "Determines if the 'Not In Group Message' should be shown if the person is not in the group. Otherwise the success message will be shown.",
        DefaultBooleanValue = true,
        Order = 3,
        Key = AttributeKey.WarnWhenNotInGroup )]

    [BooleanField( "Inactivate Instead of Remove",
        Description = "Inactivates the person in the group instead of removing them.",
        DefaultBooleanValue = false,
        Order = 4,
        Key = AttributeKey.Inactivate )]

    /*
        7/10/26 - MSE

        This setting was not on the original WebForms Remove From URL block. Added during
        the Obsidian conversion so public leave links can be restricted to safe group types
        (e.g. not security roles). Matches the "Limit Group Type" setting on Group Member
        Add From URL.

        Reason: Mirror GroupMemberAddFromURL block group-type guard for URL-based remove.
    */
    [GroupTypesField( "Limit Group Type",
        Description = "To ensure that people cannot modify the URL and try removing people from standard Rock security groups with known Id numbers you can limit which Group Types are considered valid during remove.",
        IsRequired = false,
        Order = 5,
        Key = AttributeKey.LimitGroupType )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "04942B88-F140-4BA3-8082-624D6744B0B3" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "DDCFA815-B44B-4034-B0F1-527AEBB4838C" )]
    [Rock.SystemGuid.BlockTypeGuid( "0159CE20-7B41-4D53-985C-81877ED75767" )]
    public class GroupMemberRemoveFromUrl : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DefaultGroup = "DefaultGroup";
            public const string SuccessMessage = "SuccessMessage";
            public const string NotInGroupMessage = "NotInGroupMessage";
            public const string WarnWhenNotInGroup = "WarnWhenNotInGroup";
            public const string Inactivate = "Inactivate";
            public const string LimitGroupType = "LimitGroupType";
        }

        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string GroupGuid = "GroupGuid";
            public const string PersonGuid = "PersonGuid";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<GroupMemberRemoveFromUrlBag, GroupMemberRemoveFromUrlOptionsBag>();

            box.Bag = new GroupMemberRemoveFromUrlBag
            {
                Content = ProcessRemoval()
            };

            return box;
        }

        /// <summary>
        /// Resolves the group and person from the request, removes (or inactivates) the
        /// person's group membership, and returns the resolved message to display.
        /// </summary>
        /// <returns>The rendered HTML content string.</returns>
        private string ProcessRemoval()
        {
            var groupService = new GroupService( RockContext );
            var group = GetGroupFromRequest( groupService );

            if ( group == null )
            {
                return "<div class='alert alert-warning'>Could not determine the group to remove from.</div>";
            }

            if ( !IsGroupTypeAllowed( group ) )
            {
                return "<div class='alert alert-warning'>Invalid group specified.</div>";
            }

            var personGuid = PageParameter( PageParameterKey.PersonGuid ).AsGuidOrNull();
            var person = personGuid.HasValue ? new PersonService( RockContext ).Get( personGuid.Value ) : null;

            if ( person == null )
            {
                return "<div class='alert alert-warning'>A person could not be found for the identifier provided.</div>";
            }

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Group", group );
            mergeFields.Add( "Person", person );

            var groupMemberService = new GroupMemberService( RockContext );
            var groupMembers = groupMemberService.Queryable()
                .Where( m => m.GroupId == group.Id && m.PersonId == person.Id )
                .ToList();

            if ( !groupMembers.Any() )
            {
                var warnWhenNotInGroup = GetAttributeValue( AttributeKey.WarnWhenNotInGroup ).AsBoolean();
                var notInGroupTemplate = warnWhenNotInGroup
                    ? GetAttributeValue( AttributeKey.NotInGroupMessage )
                    : GetAttributeValue( AttributeKey.SuccessMessage );

                return notInGroupTemplate.ResolveMergeFields( mergeFields );
            }

            var inactivateInsteadOfRemove = GetAttributeValue( AttributeKey.Inactivate ).AsBoolean();

            foreach ( var groupMember in groupMembers )
            {
                if ( inactivateInsteadOfRemove )
                {
                    groupMember.GroupMemberStatus = GroupMemberStatus.Inactive;
                }
                else
                {
                    groupMemberService.Delete( groupMember );
                }
            }

            RockContext.SaveChanges();

            return GetAttributeValue( AttributeKey.SuccessMessage ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Resolves the target group from the page parameters, falling back to the
        /// configured default group. The <c>GroupId</c> and <c>GroupGuid</c> parameters
        /// each accept an Id, IdKey, or Guid. Archived groups are treated as not found
        /// to match the WebForms block's <c>Queryable()</c> behavior.
        /// </summary>
        /// <param name="groupService">The group service used to resolve the group.</param>
        /// <returns>The resolved non-archived group, or <c>null</c> if one could not be determined.</returns>
        private Rock.Model.Group GetGroupFromRequest( GroupService groupService )
        {
            var allowIntegerIdentifier = !PageCache.Layout.Site.DisablePredictableIds;
            Rock.Model.Group group = null;

            var groupIdParam = PageParameter( PageParameterKey.GroupId );
            if ( !string.IsNullOrWhiteSpace( groupIdParam ) )
            {
                group = groupService.Get( groupIdParam, allowIntegerIdentifier );
            }
            else
            {
                var groupGuidParam = PageParameter( PageParameterKey.GroupGuid );
                if ( !string.IsNullOrWhiteSpace( groupGuidParam ) )
                {
                    group = groupService.Get( groupGuidParam, allowIntegerIdentifier );
                }
                else
                {
                    var defaultGroupGuid = GetAttributeValue( AttributeKey.DefaultGroup ).AsGuidOrNull();
                    if ( defaultGroupGuid.HasValue )
                    {
                        group = groupService.Get( defaultGroupGuid.Value );
                    }
                }
            }

            /*
                7/10/26 - MSE

                Service.Get uses AsNoFilter and can return archived groups. The WebForms
                block used GroupService.Queryable(), which excludes archived groups.
                Reject archived results so URL-based remove does not operate on soft-deleted groups.

                Reason: Match WebForms archived-group exclusion.
            */
            if ( group != null && group.IsArchived )
            {
                return null;
            }

            return group;
        }

        /// <summary>
        /// Determines whether the group's type is allowed by the Limit Group Type setting.
        /// When the setting is empty, all group types are allowed (same as Group Member Add From URL).
        /// </summary>
        /// <param name="group">The resolved group.</param>
        /// <returns><c>true</c> if the group type is allowed; otherwise <c>false</c>.</returns>
        private bool IsGroupTypeAllowed( Rock.Model.Group group )
        {
            var allowedGroupTypeGuids = GetAttributeValue( AttributeKey.LimitGroupType )
                .SplitDelimitedValues()
                .AsGuidList();

            // Empty setting means no restriction.
            if ( !allowedGroupTypeGuids.Any() )
            {
                return true;
            }

            var groupType = GroupTypeCache.Get( group.GroupTypeId );
            if ( groupType == null )
            {
                return false;
            }

            return allowedGroupTypeGuids.Contains( groupType.Guid );
        }

        #endregion Methods
    }
}
