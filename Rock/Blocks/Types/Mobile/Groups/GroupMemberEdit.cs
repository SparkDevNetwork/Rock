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
using System.Text;

using Rock.Attribute;
using Rock.Common.Mobile;
using Rock.Common.Mobile.Blocks.Content;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Groups
{
    /// <summary>
    /// Displays custom XAML content on the page.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Group Member Edit" )]
    [Category( "Mobile > Groups" )]
    [Description( "Edits a member of a group." )]
    [IconCssClass( "fa fa-user-cog" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [BooleanField( "Show Header",
        Description = "If enabled, a 'Group Member Edit' header will be displayed.",
        IsRequired = true,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.ShowHeader,
        Order = 0 )]

    [BooleanField( "Allow Role Change",
        Description = "",
        IsRequired = true,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.AllowRoleChange,
        Order = 1 )]

    [BooleanField( "Allow Member Status Change",
        Description = "",
        IsRequired = true,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.AllowMemberStatusChange,
        Order = 2 )]

    [BooleanField( "Allow Communication Preference Change",
        Description = "",
        IsRequired = true,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.AllowCommunicationPreferenceChange,
        Order = 3 )]

    [BooleanField( "Allow Note Edit",
        Description = "",
        IsRequired = true,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.AllowNoteEdit,
        Order = 4 )]

    [AttributeCategoryField( "Attribute Category",
        Description = "Category of attributes to show and allow editing on.",
        IsRequired = false,
        EntityType = typeof( GroupMember ),
        Key = AttributeKeys.AttributeCategory,
        Order = 5 )]

    [LinkedPage( "Member Detail Page",
        Description = "The group member page to return to, if not set then the edit page is popped off the navigation stack.",
        IsRequired = false,
        Key = AttributeKeys.MemberDetailPage,
        Order = 6 )]

    [BooleanField( "Enable Delete",
        Description = "Will show or hide the delete button. This will either delete or archive the member depending on the group type configuration.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.EnableDelete,
        Order = 7 )]

    [MobileNavigationActionField( "Delete Navigation Action",
        Description = "The action to perform after the group member is deleted from the group.",
        IsRequired = false,
        DefaultValue = MobileNavigationActionFieldAttribute.PopSinglePageValue,
        Key = AttributeKeys.DeleteNavigationAction,
        Order = 8 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_GROUPS_GROUP_MEMBER_EDIT_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "514B533A-8970-4628-A4C8-35388CD869BC" )]
    public class GroupMemberEdit : RockBlockType
    {
        /// <summary>
        /// The block setting attribute keys for the GroupMemberEdit block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The show header
            /// </summary>
            public const string ShowHeader = "EnableHeader";

            /// <summary>
            /// The allow role change
            /// </summary>
            public const string AllowRoleChange = "AllowRoleChange";

            /// <summary>
            /// The allow member status change
            /// </summary>
            public const string AllowMemberStatusChange = "AllowMemberStatusChange";

            /// <summary>
            /// The allow note edit
            /// </summary>
            public const string AllowNoteEdit = "AllowNoteEdit";

            /// <summary>
            /// The allow communication preference change key.
            /// </summary>
            public const string AllowCommunicationPreferenceChange = "AllowCommunicationPreferenceChange";

            /// <summary>
            /// The attribute category
            /// </summary>
            public const string AttributeCategory = "AttributeCategory";

            /// <summary>
            /// The member detail page
            /// </summary>
            public const string MemberDetailPage = "MemberDetailsPage";

            /// <summary>
            /// The enable delete attribute key.
            /// </summary>
            public const string EnableDelete = "EnableDelete";

            /// <summary>
            /// The delete navigation action attribute key.
            /// </summary>
            public const string DeleteNavigationAction = "DeleteNavigationAction";
        }

        /// <summary>
        /// The page parameter keys for the GroupMemberEdit block.
        /// </summary>
        public static class PageParameterKeys
        {
            /// <summary>
            /// The group member identifier
            /// </summary>
            public const string GroupMemberGuid = "GroupMemberGuid";
        }

        #region Attribute Properties

        /// <summary>
        /// Gets a value indicating whether [show header].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show header]; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowHeader => GetAttributeValue( AttributeKeys.ShowHeader ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether [allow role change].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow role change]; otherwise, <c>false</c>.
        /// </value>
        protected bool AllowRoleChange => GetAttributeValue( AttributeKeys.AllowRoleChange ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether to allow communication preference change in the group member edit block.
        /// </summary>
        /// <value><c>true</c> if [allow communication preference change]; otherwise, <c>false</c>.</value>
        protected bool AllowCommunicationPreferenceChange => GetAttributeValue( AttributeKeys.AllowCommunicationPreferenceChange ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether [allow member status change].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow member status change]; otherwise, <c>false</c>.
        /// </value>
        protected bool AllowMemberStatusChange => GetAttributeValue( AttributeKeys.AllowMemberStatusChange ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether [allow note edit].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow note edit]; otherwise, <c>false</c>.
        /// </value>
        protected bool AllowNoteEdit => GetAttributeValue( AttributeKeys.AllowNoteEdit ).AsBoolean();

        /// <summary>
        /// Gets the attribute category.
        /// </summary>
        /// <value>
        /// The attribute category.
        /// </value>
        protected Guid? AttributeCategory => GetAttributeValue( AttributeKeys.AttributeCategory ).AsGuidOrNull();

        /// <summary>
        /// Gets the member detail page.
        /// </summary>
        /// <value>
        /// The member detail page.
        /// </value>
        protected Guid? MemberDetailPage => GetAttributeValue( AttributeKeys.MemberDetailPage ).AsGuidOrNull();

        /// <summary>
        /// Gets a value indicating whether the delete button should be shown.
        /// </summary>
        /// <value><c>true</c> if the delete button should be shown; otherwise, <c>false</c>.</value>
        protected bool EnableDelete => GetAttributeValue( AttributeKeys.EnableDelete ).AsBoolean();

        /// <summary>
        /// Gets the delete navigation action.
        /// </summary>
        /// <value>The delete navigation action.</value>
        internal MobileNavigationAction DeleteNavigationAction => GetAttributeValue( AttributeKeys.DeleteNavigationAction ).FromJsonOrNull<MobileNavigationAction>() ?? new MobileNavigationAction();

        #endregion

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
            //
            // Indicate that we are a dynamic content providing block.
            //
            return new
            {
                // Rock Mobile Shell below v1.2.1
                Content = ( string ) null,
                DynamicContent = true,

                // Rock Mobile Shell v1.2.1 and later
                SupportedFeatures = new[] { "ClientLogic" },
                AllowRoleChange = AllowRoleChange,
                AllowMemberStatusChange = AllowMemberStatusChange,
                AllowNoteEdit = AllowNoteEdit,
                MemberDetailPage = MemberDetailPage,
                DeleteNavigationAction = DeleteNavigationAction,
                AllowCommunicationPreferenceChange = AllowCommunicationPreferenceChange
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the editable attributes.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        private List<AttributeCache> GetEditableAttributes( IHasAttributes entity )
        {
            if ( AttributeCategory.HasValue )
            {
                var category = CategoryCache.Get( AttributeCategory.Value );

                if ( category != null )
                {
                    return entity.Attributes.Values
                        .Where( a => a.CategoryIds.Contains( category.Id ) )
                        .Where( a => a.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name )
                        .ToList();
                }
            }

            return new List<AttributeCache>();
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the member data that describes the group member to be edited.
        /// </summary>
        /// <param name="groupMemberGuid">The group member unique identifier.</param>
        /// <returns>The result of the action.</returns>
        [BlockAction]
        public BlockActionResult GetMemberData( Guid groupMemberGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupMemberService = new GroupMemberService( rockContext );

                var member = groupMemberService
                    .Queryable()
                    .Include( m => m.Group.GroupType )
                    .FirstOrDefault( m => m.Guid == groupMemberGuid );

                if ( member == null )
                {
                    return ActionBadRequest( "We couldn't find that member." );
                }
                else if ( !member.Group.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) && !member.Group.IsAuthorized( Authorization.MANAGE_MEMBERS, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( "You are not authorized to edit members of this group." );
                }

                // Get the header content to send down.
                string headerContent = ShowHeader
                    ? @"<StackLayout><Label StyleClass=""h2, title1, text-interface-strongest, bold"" Text=""Group Member Edit"" /></StackLayout>"
                    : string.Empty;

                // Get all the attribute fields.
                member.LoadAttributes( rockContext );
                var fields = GetEditableAttributes( member )
                    .Select( a => new MobileField
                    {
                        AttributeGuid = a.Guid,
                        Key = a.Key,
                        Title = a.Name,
                        IsRequired = a.IsRequired,
                        ConfigurationValues = a.QualifierValues.ToDictionary( kvp => kvp.Key, kvp => kvp.Value.Value ),
                        FieldTypeGuid = a.FieldType.Guid,
#pragma warning disable CS0618 // Type or member is obsolete: Required for Mobile Shell v2 support
                        RockFieldType = a.FieldType.Class,
#pragma warning restore CS0618 // Type or member is obsolete: Required for Mobile Shell v2 support
                        Value = member.GetAttributeValue( a.Key )
                    } )
                    .ToList();

                // Get all the roles that can be set for this member.
                var roles = GroupTypeCache.Get( member.Group.GroupTypeId )
                    .Roles
                    // We want to filter out the roles that the Person already has a record for (besides the original).
                    .Where( r => !PersonHasGroupMemberRole( member.PersonId, member.GroupId, r.Id ) || r.Id == member.GroupRoleId )
                    .OrderBy( r => r.Order )
                    .Select( r => new ListItemViewModel
                    {
                        Value = r.Guid.ToString(),
                        Text = r.Name
                    } )
                    .ToList();

                // Configure the delete/archive options.
                bool canDelete = EnableDelete;
                bool canArchive = false;

                if ( canDelete )
                {
                    var groupMemberHistoricalService = new GroupMemberHistoricalService( rockContext );
                    var roleIdsWithGroupSync = member.Group.GroupSyncs.Select( a => a.GroupTypeRoleId ).ToList();

                    if ( roleIdsWithGroupSync.Contains( member.GroupRoleId ) )
                    {
                        canDelete = false;
                    }
                    else if ( member.Group.GroupType.EnableGroupHistory == true && groupMemberHistoricalService.Queryable().Any( a => a.GroupMemberId == member.Id ) )
                    {
                        canDelete = false;
                        canArchive = true;
                    }
                }

                return ActionOk( new GetMemberDataResult
                {
                    HeaderContent = headerContent,
                    Roles = roles,
                    Fields = fields,
                    CanDelete = canDelete,
                    CanArchive = canArchive,
                    Name = member.Person.FullName,
                    RoleGuid = member.GroupRole.Guid,
                    MemberStatus = member.GroupMemberStatus,
                    Note = member.Note,
                    CommunicationPreference = member.CommunicationPreference,
                    HasMultipleRoles = groupMemberService.GetByGroupIdAndPersonId( member.GroupId, member.PersonId ).Count() > 1
                } );
            }
        }

        /// <summary>
        /// Updates and saves the member with the data provided.
        /// </summary>
        /// <param name="groupMemberGuid">The group member unique identifier.</param>
        /// <param name="groupMemberData">The group member data.</param>
        /// <returns>Empty OK response if save was successful; otherwise returns an error code.</returns>
        [BlockAction]
        public BlockActionResult SaveMember( Guid groupMemberGuid, MemberDataViewModel groupMemberData )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupMemberService = new GroupMemberService( rockContext );
                var member = groupMemberService.Get( groupMemberGuid );

                if ( member == null || ( !member.Group.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) && !member.Group.IsAuthorized( Authorization.MANAGE_MEMBERS, RequestContext.CurrentPerson ) ) )
                {
                    return ActionBadRequest( "You are not authorized to edit members of this group." );
                }

                member.LoadAttributes( rockContext );

                // Verify and save the member role.
                if ( AllowRoleChange && groupMemberData.RoleGuid.HasValue )
                {
                    var groupRole = GroupTypeCache.Get( member.Group.GroupTypeId )
                        .Roles
                        .FirstOrDefault( r => r.Guid == groupMemberData.RoleGuid.Value );

                    if ( groupRole == null )
                    {
                        return ActionBadRequest( "Invalid data." );
                    }

                    member.GroupRoleId = groupRole.Id;
                }

                // Verify and save the communication preference.
                if ( AllowCommunicationPreferenceChange )
                {
                    foreach ( var groupMemberOccurrence in groupMemberService.GetByGroupIdAndPersonId( member.GroupId, member.PersonId ) )
                    {
                        groupMemberOccurrence.CommunicationPreference = groupMemberData.CommunicationPreference;
                    }
                }

                // Verify and save the member status.
                if ( AllowMemberStatusChange )
                {
                    if ( !groupMemberData.MemberStatus.HasValue )
                    {
                        return ActionBadRequest( "Invalid data." );
                    }

                    member.GroupMemberStatus = groupMemberData.MemberStatus.Value;
                }

                // Verify and save the note.
                if ( AllowNoteEdit )
                {
                    if ( groupMemberData.Note == null )
                    {
                        return ActionBadRequest( "Invalid data." );
                    }

                    member.Note = groupMemberData.Note;
                }

                // Save all the attribute values.
                if ( groupMemberData.FieldValues != null )
                {
                    member.LoadAttributes();
                    var attributes = GetEditableAttributes( member );
                    foreach ( var attribute in attributes )
                    {
                        if ( !groupMemberData.FieldValues.TryGetValue( attribute.Key, out var value ) )
                        {
                            continue;
                        }

                        member.SetAttributeValue( attribute.Key, value );
                    }
                }

                // Save all changes to database.
                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    member.SaveAttributeValues( rockContext );
                } );

                return ActionOk();
            }
        }

        /// <summary>
        /// Removes the member from the group.
        /// </summary>
        /// <param name="groupMemberGuid">The group member unique identifier.</param>
        /// <returns>A result that describes if the operation was successful or not.</returns>
        [BlockAction]
        public BlockActionResult RemoveMember( Guid groupMemberGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupMemberService = new GroupMemberService( rockContext );
                var member = groupMemberService.Queryable()
                    .Include( m => m.Group.GroupType )
                    .FirstOrDefault( m => m.Guid == groupMemberGuid );

                if ( member == null || ( !member.Group.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) && !member.Group.IsAuthorized( Authorization.MANAGE_MEMBERS, RequestContext.CurrentPerson ) ) )
                {
                    return ActionBadRequest( "You are not authorized to edit members of this group." );
                }

                var groupMemberHistoricalService = new GroupMemberHistoricalService( rockContext );
                bool archive = false;

                if ( member.Group.GroupType.EnableGroupHistory == true && groupMemberHistoricalService.Queryable().Any( a => a.GroupMemberId == member.Id ) )
                {
                    // if the group has GroupHistory enabled, and this group
                    // member has group member history snapshots, then we only
                    // archive.
                    archive = true;
                }
                else if ( !groupMemberService.CanDelete( member, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                int groupId = member.GroupId;

                if ( archive )
                {
                    // NOTE: Delete will AutoArchive, but since we know that we
                    // need to archive, we can call .Archive directly
                    groupMemberService.Archive( member, RequestContext.CurrentPerson?.PrimaryAliasId, true );
                }
                else
                {
                    groupMemberService.Delete( member, true );
                }

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion

        #region Legacy (Rock Shell < v1.2.1)

        #region Methods

        /// <summary>
        /// Builds the content to be displayed on the block.
        /// </summary>
        /// <returns>A string containing the XAML content to be displayed.</returns>
        private string BuildContent()
        {
            string content = @"
<StackLayout>
    ##HEADER##

    ##FIELDS##
    
    <Rock:Validator x:Name=""vForm"">
        ##VALIDATORS##
    </Rock:Validator>
    
    <Rock:NotificationBox x:Name=""nbError"" NotificationType=""Error"" />
    
    <Button StyleClass=""btn,btn-primary"" Text=""Save"" Margin=""24 0 0 0"" Command=""{Binding Callback}"">
        <Button.CommandParameter>
            <Rock:CallbackParameters Name=""Save"" Validator=""{x:Reference vForm}"" Notification=""{x:Reference nbError}"">
                ##PARAMETERS##
            </Rock:CallbackParameters>
        </Button.CommandParameter>
    </Button>

    <Button StyleClass=""btn,btn-link"" Text=""Cancel"" ##CANCEL## />
</StackLayout>";

            var groupMemberGuid = RequestContext.GetPageParameter( PageParameterKeys.GroupMemberGuid ).AsGuid();
            var parameters = new Dictionary<string, string>();
            string fieldsContent;

            using ( var rockContext = new RockContext() )
            {
                var member = new GroupMemberService( rockContext ).Get( groupMemberGuid );

                if ( member == null )
                {
                    return "<Rock:NotificationBox HeaderText=\"Error\" Text=\"We couldn't find that member.\" NotificationType=\"Error\" />";
                }
                else if ( !member.Group.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) && !member.Group.IsAuthorized( Authorization.MANAGE_MEMBERS, RequestContext.CurrentPerson ) )
                {
                    return "<Rock:NotificationBox HeaderText=\"Error\" Text=\"You are not authorized to edit members of this group.\" NotificationType=\"Error\" />";
                }

                member.LoadAttributes( rockContext );
                var attributes = GetEditableAttributes( member );

                fieldsContent = BuildCommonFields( member, parameters );
                fieldsContent += MobileHelper.GetEditAttributesXaml( member, attributes, parameters );
            }

            var validatorsContent = parameters.Keys.Select( a => $"<x:Reference>{a}</x:Reference>" );
            var parametersContent = parameters.Select( a => $"<Rock:Parameter Name=\"{a.Key}\" Value=\"{{Binding {a.Value}, Source={{x:Reference {a.Key}}}}}\" />" );

            if ( MemberDetailPage.HasValue )
            {
                content = content.Replace( "##CANCEL##", $"Command=\"{{Binding ReplacePage}}\" CommandParameter=\"{MemberDetailPage}?GroupMemberGuid={groupMemberGuid}\"" );
            }
            else
            {
                content = content.Replace( "##CANCEL##", "Command=\"{Binding PopPage}\"" );
            }

            if ( ShowHeader )
            {
                content = content.Replace( "##HEADER##", @"<Label StyleClass=""h2"" Text=""Group Member Edit"" />
<Rock:Divider />" );
            }
            else
            {
                content = content.Replace( "##HEADER##", string.Empty );
            }

            return content.Replace( "##FIELDS##", fieldsContent )
                .Replace( "##VALIDATORS##", string.Join( string.Empty, validatorsContent ) )
                .Replace( "##PARAMETERS##", string.Join( string.Empty, parametersContent ) );
        }

        /// <summary>
        /// Whether or not the person provided has a record with the specified row.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="groupId"></param>
        /// <param name="groupTypeRoleId">The group type role identifier.</param>
        /// <returns><c>true</c> if the person has a member occurrence with that role, <c>false</c> otherwise.</returns>
        private bool PersonHasGroupMemberRole( int personId, int groupId, int groupTypeRoleId )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupMemberRecords = new GroupMemberService( rockContext ).GetByGroupIdAndPersonId( groupId, personId );
                return groupMemberRecords.Any( gm => gm.GroupRoleId == groupTypeRoleId );
            }
        }

        /// <summary>
        /// Builds the common fields.
        /// </summary>
        /// <param name="member">The group.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>A string containing the XAML that represents the common Group fields.</returns>
        private string BuildCommonFields( GroupMember member, Dictionary<string, string> parameters )
        {
            var sb = new StringBuilder();

            sb.AppendLine( MobileHelper.GetReadOnlyFieldXaml( "Name", member.Person.FullName ) );

            if ( AllowRoleChange )
            {
                var roles = GroupTypeCache.Get( member.Group.GroupTypeId ).Roles
                    .Where( r => PersonHasGroupMemberRole( member.PersonId, member.GroupId, member.GroupRoleId ) );

                if ( roles.Any() )
                {
                    var items = roles.Select( a => new KeyValuePair<string, string>( a.Id.ToString(), a.Name ) );
                    sb.AppendLine( MobileHelper.GetSingleFieldXaml( MobileHelper.GetDropDownFieldXaml( "role", "Role", member.GroupRoleId.ToString(), true, items ) ) );
                    parameters.Add( "role", "SelectedValue" );
                }

            }
            else
            {
                sb.AppendLine( MobileHelper.GetReadOnlyFieldXaml( "Role", member.GroupRole.Name ) );
            }

            if ( AllowMemberStatusChange )
            {
                var items = Enum.GetNames( typeof( GroupMemberStatus ) )
                    .Select( a => new KeyValuePair<string, string>( a, a ) );

                sb.AppendLine( MobileHelper.GetSingleFieldXaml( MobileHelper.GetDropDownFieldXaml( "memberstatus", "Member Status", member.GroupMemberStatus.ToString(), true, items ) ) );
                parameters.Add( "memberstatus", "SelectedValue" );
            }
            else
            {
                sb.AppendLine( MobileHelper.GetReadOnlyFieldXaml( "Member Status", member.GroupMemberStatus.ToString() ) );
            }

            if ( AllowNoteEdit )
            {
                sb.AppendLine( MobileHelper.GetSingleFieldXaml( MobileHelper.GetTextEditFieldXaml( "note", "Note", member.Note, true, false, true ) ) );
                parameters.Add( "note", "Text" );
            }
            else
            {
                sb.AppendLine( MobileHelper.GetReadOnlyFieldXaml( "Note", member.Note ) );
            }

            return sb.ToString();
        }

        /// <summary>
        /// Saves the group member.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The response to send back to the client.</returns>
        private CallbackResponse SaveGroupMember( Dictionary<string, object> parameters )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupMemberGuid = RequestContext.GetPageParameter( PageParameterKeys.GroupMemberGuid ).AsGuid();
                var member = new GroupMemberService( rockContext ).Get( groupMemberGuid );

                if ( member == null || ( !member.Group.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) && !member.Group.IsAuthorized( Authorization.MANAGE_MEMBERS, RequestContext.CurrentPerson ) ) )
                {
                    return new CallbackResponse
                    {
                        Error = "You are not authorized to edit members of this group."
                    };
                }

                member.LoadAttributes( rockContext );

                //
                // Verify and save all the property values.
                //
                if ( AllowRoleChange )
                {
                    member.GroupRoleId = ( ( string ) parameters["role"] ).AsInteger();
                }

                if ( AllowMemberStatusChange )
                {
                    member.GroupMemberStatus = ( ( string ) parameters["memberstatus"] ).ConvertToEnum<GroupMemberStatus>();
                }

                if ( AllowNoteEdit )
                {
                    member.Note = ( string ) parameters["note"];
                }

                //
                // Save all the attribute values.
                //
                MobileHelper.UpdateEditAttributeValues( member, parameters, GetEditableAttributes( member ) );

                //
                // Save all changes to database.
                //
                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    member.SaveAttributeValues( rockContext );
                } );
            }

            if ( MemberDetailPage.HasValue )
            {
                return new CallbackResponse
                {
                    Command = "ReplacePage",
                    CommandParameter = $"{MemberDetailPage}?GroupMemberGuid={RequestContext.GetPageParameter( PageParameterKeys.GroupMemberGuid )}"
                };
            }
            else
            {
                return new CallbackResponse
                {
                    Command = "PopPage",
                    CommandParameter = "true"
                };
            }
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the current configuration for this block.
        /// </summary>
        /// <returns>A collection of string/string pairs.</returns>
        [BlockAction]
        public object GetInitialContent()
        {
            return new CallbackResponse
            {
                Content = BuildContent()
            };
        }

        /// <summary>
        /// Gets the dynamic XAML content that should be rendered based upon the request.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        [BlockAction]
        public object GetCallbackContent( string command, Dictionary<string, object> parameters )
        {
            if ( command == "Save" )
            {
                return SaveGroupMember( parameters );
            }
            else
            {
                return new CallbackResponse
                {
                    Error = "Invalid command received."
                };
            }
        }

        #endregion

        #endregion

        #region Action View Models

        /// <summary>
        /// Properties describing a successful member data result in the <see cref="GroupMemberEdit" /> block.
        /// </summary>
        internal class GetMemberDataResult
        {
            /// <summary>
            /// Gets or sets the content of the header.
            /// </summary>
            /// <value>The content of the header.</value>
            public string HeaderContent { get; set; }

            /// <summary>
            /// Gets or sets the roles.
            /// </summary>
            /// <value>The roles.</value>
            public List<ListItemViewModel> Roles { get; set; }

            /// <summary>
            /// Gets or sets the fields.
            /// </summary>
            /// <value>The fields.</value>
            public List<MobileField> Fields { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance can delete.
            /// </summary>
            /// <value><c>true</c> if this instance can delete; otherwise, <c>false</c>.</value>
            public bool CanDelete { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance can archive.
            /// </summary>
            /// <value><c>true</c> if this instance can archive; otherwise, <c>false</c>.</value>
            public bool CanArchive { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the role unique identifier.
            /// </summary>
            /// <value>The role unique identifier.</value>
            public Guid RoleGuid { get; set; }

            /// <summary>
            /// Gets or sets the member status.
            /// </summary>
            /// <value>The member status.</value>
            public GroupMemberStatus MemberStatus { get; set; }

            /// <summary>
            /// Gets or sets the note.
            /// </summary>
            /// <value>The note.</value>
            public string Note { get; set; }

            /// <summary>
            /// Gets or sets the communication preference.
            /// </summary>
            /// <value>The communication preference.</value>
            public CommunicationType CommunicationPreference { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance has multiple roles.
            /// </summary>
            /// <value><c>true</c> if this instance has multiple roles; otherwise, <c>false</c>.</value>
            public bool HasMultipleRoles { get; set; }
        }

        /// <summary>
        /// Describes the member data to be saved.
        /// </summary>
        public class MemberDataViewModel
        {
            /// <summary>
            /// Gets or sets the role unique identifier.
            /// </summary>
            /// <value>The role unique identifier.</value>
            public Guid? RoleGuid { get; set; }

            /// <summary>
            /// Gets or sets the member status.
            /// </summary>
            /// <value>The member status.</value>
            public GroupMemberStatus? MemberStatus { get; set; }

            /// <summary>
            /// Gets or sets the note.
            /// </summary>
            /// <value>The note.</value>
            public string Note { get; set; }

            /// <summary>
            /// Gets or sets the field values.
            /// </summary>
            /// <value>The field values.</value>
            public Dictionary<string, string> FieldValues { get; set; }

            /// <summary>
            /// Gets or sets the communication preference.
            /// </summary>
            /// <value>The communication preference.</value>
            public CommunicationType CommunicationPreference { get; set; }
        }

        internal class ListItemViewModel
        {
            public string Value { get; set; }

            public string Text { get; set; }
        }

        #endregion
    }
}