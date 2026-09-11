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

    [BooleanField( "Show Date Added",
        Description = "Should the date that person was added to the group be displayed for each group member?",
        Key = AttributeKey.ShowDateAdded,
        DefaultBooleanValue = false,
        IsRequired = false,
        Order = 3 )]

    [BooleanField( "Show Note Column",
        Description = "Should the note be displayed as a separate grid column (instead of displaying a note icon under person's name)?",
        Key = AttributeKey.ShowNoteColumn,
        DefaultBooleanValue = false,
        IsRequired = false,
        Order = 4 )]

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
            public const string ShowDateAdded = "ShowDateAdded";
            public const string ShowNoteColumn = "ShowNoteColumn";
        }

        private static class PageParameterKey
        {
            public const string CampusId = "CampusId";
            public const string GroupId = "GroupId";
            public const string GroupMemberId = "GroupMemberId";
        }

        private static class NavigationUrlKey
        {
            public const string AddPage = "AddPage";
            public const string DetailPage = "DetailPage";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the campus supplied to the page.
        /// </summary>
        private CampusCache Campus => CampusCache.Get( PageParameter( PageParameterKey.CampusId ), !PageCache.Layout.Site.DisablePredictableIds );

        #endregion Properties

        #region RockListBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new ListBlockBox<GroupMemberListOptionsBag>
            {
                GridDefinition = GetGridBuilder().BuildDefinition(),
                Options = GetBoxOptions(),
                IsAddEnabled = CanEdit(),
                NavigationUrls = GetBoxNavigationUrls()
            };
        }

        /// <inheritdoc/>
        protected override bool IsAllowedToCreateEntitySet( GridEntitySetBag entitySetBag )
        {
            return GetGroup() != null;
        }

        /// <inheritdoc/>
        protected override bool IsAllowedToCreateCommunication( GridCommunicationBag communicationBag )
        {
            return GetGroup() != null;
        }

        /// <inheritdoc/>
        protected override IQueryable<GroupMemberRow> GetListQueryable( RockContext rockContext )
        {
            var group = GetGroup();

            if ( group?.GroupType?.Roles.Any() != true )
            {
                return new List<GroupMemberRow>().AsQueryable();
            }

            var groupId = group.Id;

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
                        PhotoId = gm.Person.PhotoId,
                        Age = gm.Person.Age,
                        Gender = gm.Person.Gender,
                        RecordTypeValueId = gm.Person.RecordTypeValueId,
                        AgeClassification = gm.Person.AgeClassification
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
                person.PhotoUrl = Rock.Model.Person.GetPersonPhotoUrl(
                    initials,
                    person.PhotoId,
                    person.Age,
                    person.Gender,
                    person.RecordTypeValueId,
                    person.AgeClassification );
            }

            return rows;
        }

        /// <inheritdoc/>
        protected override GridBuilder<GroupMemberRow> GetGridBuilder()
        {
            return new GridBuilder<GroupMemberRow>()
                .WithBlock( this )
                .AddTextField( "idKey", r => r.GroupMember.IdKey )
                .AddTextField( "personIdKey", r => r.Person.IdKey )
                .AddField( "person", r => new PersonFieldBag
                {
                    IdKey = r.Person.IdKey,
                    NickName = r.Person.NickName,
                    LastName = r.Person.LastName,
                    PhotoUrl = r.Person.PhotoUrl
                } )
                .AddTextField( "role", r => r.RoleName )
                .AddDateTimeField( "dateAdded", r => r.GroupMember.DateTimeAdded )
                .AddTextField( "note", r => r.GroupMember.Note )
                .AddTextField( "status", r => r.GroupMember.GroupMemberStatus.ConvertToString() );
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
            var groupType = GetGroup()?.GroupType;

            if ( groupType == null )
            {
                return options;
            }

            var blockTitle = GetAttributeValue( AttributeKey.BlockTitle );

            options.Title = blockTitle.IsNotNullOrWhiteSpace()
                ? blockTitle
                : $"{groupType.GroupTerm} {groupType.GroupMemberTerm.Pluralize()}";

            options.ItemTerm = $"{groupType.GroupTerm} {groupType.GroupMemberTerm}";

            // A group type with no roles cannot hold members, so the warning stands in place of the grid.
            if ( !groupType.Roles.Any() )
            {
                options.WarningMessage = $"{groupType.GroupMemberTerm.Pluralize()} cannot be added to this {groupType.GroupTerm} because the '{groupType.Name}' group type does not have any roles defined.";

                return options;
            }

            options.IsGridVisible = true;
            options.IsDateAddedColumnVisible = GetAttributeValue( AttributeKey.ShowDateAdded ).AsBoolean();
            options.IsNoteColumnVisible = GetAttributeValue( AttributeKey.ShowNoteColumn ).AsBoolean();

            return options;
        }

        /// <summary>
        /// Gets the group this block lists the members of, resolved from the Group block setting, then the GroupId page
        /// parameter, then the team group of the campus named by the CampusId page parameter. The group is only
        /// returned when the current person may view it.
        /// </summary>
        /// <returns>The group, or <c>null</c> when none resolved or it cannot be viewed.</returns>
        private GroupCache GetGroup()
        {
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

            return group?.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) == true ? group : null;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var group = GetGroup();

            if ( group == null )
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
                { PageParameterKey.GroupId, group.IdKey }
            };

            var campus = Campus;

            if ( campus != null )
            {
                detailParameters.Add( PageParameterKey.CampusId, campus.IdKey );
                addParameters.Add( PageParameterKey.CampusId, campus.IdKey );
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, detailParameters ),
                [NavigationUrlKey.AddPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, addParameters )
            };
        }

        /// <summary>
        /// Determines whether the current person may add and remove members of this group.
        /// </summary>
        /// <returns><c>true</c> when the current person may manage this group's members.</returns>
        private bool CanEdit()
        {
            var group = GetGroup();
            var currentPerson = GetCurrentPerson();

            return group != null
                && (
                    BlockCache.IsAuthorized( Authorization.EDIT, currentPerson )
                    || group.IsAuthorized( Authorization.EDIT, currentPerson )
                    || group.IsAuthorized( Authorization.MANAGE_MEMBERS, currentPerson )
                );
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
            /// Gets or sets the identifier of the person's photo.
            /// </summary>
            public int? PhotoId { get; set; }

            /// <summary>
            /// Gets or sets the person's age.
            /// </summary>
            public int? Age { get; set; }

            /// <summary>
            /// Gets or sets the person's gender.
            /// </summary>
            public Gender Gender { get; set; }

            /// <summary>
            /// Gets or sets the person's record type defined value identifier.
            /// </summary>
            public int? RecordTypeValueId { get; set; }

            /// <summary>
            /// Gets or sets the person's age classification.
            /// </summary>
            public AgeClassification AgeClassification { get; set; }

            /// <summary>
            /// Gets or sets the URL of the person's photo.
            /// </summary>
            public string PhotoUrl { get; set; }
        }

        #endregion Helper Classes
    }
}
