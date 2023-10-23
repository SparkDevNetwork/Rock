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
using Rock.Common.Mobile.Blocks.Content;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Blocks.Types.Mobile.Groups
{
    /// <summary>
    /// Displays a page to allow the user to view the details about a specified group member.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Group Member View" )]
    [Category( "Mobile > Groups" )]
    [Description( "Allows the user to view the details about a specific group member." )]
    [IconCssClass( "fa fa-user-tie" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [LinkedPage( "Group Member Edit Page",
        Description = "The page that will allow editing of a group member.",
        IsRequired = false,
        Key = AttributeKeys.GroupMemberEditPage,
        Order = 0 )]

    [BlockTemplateField( "Template",
        Description = "The template to use when rendering the content.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_MEMBER_VIEW,
        IsRequired = true,
        DefaultValue = "",
        Key = AttributeKeys.Template,
        Order = 1 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_GROUPS_GROUP_MEMBER_VIEW_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "6B3C23EA-A1C2-46FA-9F04-5B0BD004ED8B")]
    public class GroupMemberView : RockBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the GroupMemberView block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The group member edit page attribute key.
            /// </summary>
            public const string GroupMemberEditPage = "GroupMemberEditPage";

            /// <summary>
            /// The template key attribute key.
            /// </summary>
            public const string Template = "Template";
        }

        /// <summary>
        /// Gets the group member edit page.
        /// </summary>
        /// <value>
        /// The group member edit page.
        /// </value>
        protected Guid? GroupMemberEditPage => GetAttributeValue( AttributeKeys.GroupMemberEditPage ).AsGuidOrNull();

        /// <summary>
        /// Gets the template.
        /// </summary>
        /// <value>
        /// The template.
        /// </value>
        protected string Template => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKeys.Template ) );

        #endregion

        #region Page Parameter Keys

        /// <summary>
        /// The keys used in page parameters.
        /// </summary>
        private class PageParameterKeys
        {
            /// <summary>
            /// The member identifier key.
            /// </summary>
            public const string GroupMemberGuid = "GroupMemberGuid";
        }

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
            return new Rock.Common.Mobile.Blocks.Content.Configuration
            {
                DynamicContent = true
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the content to send to the client.
        /// </summary>
        /// <returns>A string containing XAML content.</returns>
        private string BuildContent()
        {
            using ( var rockContext = new RockContext() )
            {
                var memberGuid = RequestContext.GetPageParameter( PageParameterKeys.GroupMemberGuid ).AsGuid();
                var member = new GroupMemberService( rockContext ).Get( memberGuid );

                if ( member == null )
                {
                    return @"<StackLayout>
    <Rock:NotificationBox Text=""Group Member not Found."" NotificationType=""Error"" />
</StackLayout>";
                }

                //
                // Verify the user has access to view the group.
                //
                if ( !member.Group.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    return @"<StackLayout>
    <Rock:NotificationBox Text=""You do not have permission to view members of this group."" NotificationType=""Error"" />
</StackLayout>";
                }

                var mergeFields = RequestContext.GetCommonMergeFields();

                mergeFields.Add( "Member", member );
                mergeFields.Add( "GroupMemberEditPage", GroupMemberEditPage.HasValue ? GroupMemberEditPage.ToString() : string.Empty );

                //
                // Add in all attributes/values that the user is allowed to see.
                //
                member.LoadAttributes( rockContext );
                var attributes = member.Attributes
                    .Where( a => a.Value.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                    .Select( a =>
                    {
                        var rawValue = member.GetAttributeValue( a.Value.Key );
                        return new
                        {
                            a.Value.Key,
                            a.Value.Name,
                            Value = rawValue,
                            FormattedValue = a.Value.FieldType.Field.FormatValue( null, a.Value.EntityTypeId, member.Id, rawValue, a.Value.QualifierValues, false )
                        };
                    } )
                    .OrderBy( a => a.Name )
                    .ToList();
                mergeFields.Add( "VisibleAttributes", attributes);

                //
                // Add collection of allowed security actions.
                //
                var securityActions = new Dictionary<string, object>
                {
                    { "View", member.Group.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) },
                    { "ManageMembers", member.Group.IsAuthorized( Authorization.MANAGE_MEMBERS, RequestContext.CurrentPerson ) },
                    { "Edit", member.Group.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) },
                    { "Administrate", member.Group.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) }
                };
                mergeFields.Add( "AllowedActions", securityActions );

                return Template.ResolveMergeFields( mergeFields );
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

        #endregion
    }
}
