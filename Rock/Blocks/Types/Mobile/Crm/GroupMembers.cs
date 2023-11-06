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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;
using Rock.Common.Mobile.Blocks.Crm.GroupMembers;
using Rock.Web.UI;
using Rock.Security;
using System;

namespace Rock.Blocks.Types.Mobile.Crm
{

    /// <summary>
    /// The Rock Mobile Group Members block.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Group Members" )]
    [Category( "Mobile > Crm" )]
    [IconCssClass( "fa fa-users" )]
    [Description( "Allows you to view the other members of a group person belongs to (e.g. Family groups)." )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [BlockTemplateField( "Members Template",
        Description = "The template to use when rendering the members. Provided with a merge field containing a list of groups and a value depicting whether the user is authorized to edit the group (for cases where there are multiple).",
        TemplateBlockValueGuid = Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_MEMBERS,
        DefaultValue = "13470DDB-5F8C-4EA2-93FD-B738F37C9AFC",
        IsRequired = true,
        Key = AttributeKey.MembersTemplate,
        Order = 0 )]

    [GroupTypeField(
        "Group Type",
        Key = AttributeKey.GroupType,
        Description = "The group type to display groups for (default is Family).",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY,
        Order = 1 )]

    [BooleanField(
        "Auto Create Group",
        Key = AttributeKey.AutoCreateGroup,
        Description = "If person doesn't belong to a group of this type, should one be created for them (default is Yes).",
        DefaultBooleanValue = true,
        Order = 2 )]

    [LinkedPage(
        "Group Edit Page",
        Key = AttributeKey.GroupEditPage,
        Description = "Page used to edit the members of the selected group.",
        IsRequired = false,
        Order = 3 )]

    #endregion

    [ContextAware( typeof( Rock.Model.Person ) )]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_CRM_GROUP_MEMBERS )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_CRM_GROUP_MEMBERS )]
    public class GroupMembers : RockBlockType
    {
        #region IRockMobileBlockType

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 5 );

        #endregion

        #region Keys

        /// <summary>
        /// The attribute keys for this block.
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The header template attribute key.
            /// </summary>
            public const string MembersTemplate = "MembersTemplate";

            /// <summary>
            /// The group type attribute key.
            /// </summary>
            public const string GroupType = "GroupType";

            /// <summary>
            /// The automatic create group attribute key.
            /// </summary>
            public const string AutoCreateGroup = "AutoCreateGroup";

            /// <summary>
            /// The group edit page attribute key.
            /// </summary>
            public const string GroupEditPage = "GroupEditPage";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the members template.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns>System.String.</returns>
        private string GetMembersTemplateInternal( Rock.Model.Person person )
        {
            var template = Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKey.MembersTemplate ) );
            var mergeFields = RequestContext.GetCommonMergeFields();

            // Get the group type, or the family group type as a default.
            var groupTypeGuid = GetAttributeValue( AttributeKey.GroupType ).AsGuidOrNull() ?? SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
            var groupType = GroupTypeCache.Get( groupTypeGuid );

            if ( groupType == null )
            {
                return null;
            }

            // If this is a Family GroupType and they belong to multiple families,
            // first make sure that the GroupMember.GroupOrder is set for this Person's Families.
            // This will ensure that other spots that rely on the GroupOrder provide consistent results.
            if ( groupType.Guid == SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() )
            {
                // We purposefully use a separate rockContext for this.
                using ( var rockContext = new RockContext() )
                {
                    var memberService = new GroupMemberService( rockContext );
                    var groupMemberGroups = memberService.Queryable( true )
                        .Where( m =>
                            m.PersonId == person.Id &&
                            m.Group.GroupTypeId == groupType.Id )
                        .OrderBy( m => m.GroupOrder ?? int.MaxValue ).ThenBy( m => m.Id )
                        .ToList();

                    if ( groupMemberGroups.Count > 1 && memberService.SetGroupMemberGroupOrder( groupMemberGroups ) )
                    {
                        rockContext.SaveChanges();
                    }
                }
            }

            using ( var rockContext = new RockContext() )
            {
                var memberService = new GroupMemberService( rockContext );

                // Load the list of groups for this person.
                var groups = memberService.Queryable( true )
                    .Where( m =>
                        m.PersonId == person.Id &&
                        m.Group.GroupTypeId == groupType.Id )
                    .OrderBy( m => m.GroupOrder ?? int.MaxValue ).ThenBy( m => m.Id )
                    .Select( m =>  m.Group )
                    .Include( g => g.Members.Select( m => m.Person ) ) // Include the Person in the query to save performance.
                    .AsNoTracking()
                    .ToList();

                var groupBags = groups.Select( g => new
                {
                    Group = g,
                    CanEdit = RequestContext.CurrentPerson != null && g.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
                } ).ToList();

                // If the group doesn't exist and configured, create one by default.
                if ( !groupBags.Any() && GetAttributeValue( AttributeKey.AutoCreateGroup ).AsBoolean( true ) )
                {
                    // Ensure that the person is in the group.
                    var groupService = new GroupService( rockContext );
                    var group = new Model.Group();
                    group.Name = person.LastName;
                    group.GroupTypeId = groupType.Id;
                    groupService.Add( group );
                    rockContext.SaveChanges();

                    var groupMember = new GroupMember();
                    groupMember.PersonId = person.Id;
                    groupMember.GroupRoleId = groupType.DefaultGroupRoleId.Value;
                    groupMember.GroupId = group.Id;
                    group.Members.Add( groupMember );
                    rockContext.SaveChanges();

                    var newGroup = new 
                    {
                        Group = groupService.Get( group.Id ),
                        CanEdit = true
                    };

                    groupBags.Add( newGroup );
                }

                mergeFields.Add( "Groups", groupBags );
                mergeFields.Add( "Person", person );

                var editPageGuid = GetAttributeValue( AttributeKey.GroupEditPage ).AsGuidOrNull();
                if ( editPageGuid != null )
                {
                    mergeFields.Add( "EditPage", editPageGuid );
                }

                return template.ResolveMergeFields( mergeFields );
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the members data.
        /// </summary>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult GetMembersData()
        {
            var person = RequestContext.GetContextEntity<Rock.Model.Person>();

            if ( person == null )
            {
                return ActionNotFound();
            }

            return ActionOk( new ResponseBag
            {
                MembersTemplate = GetMembersTemplateInternal( person )
            } );
        }

        #endregion
    }
}
