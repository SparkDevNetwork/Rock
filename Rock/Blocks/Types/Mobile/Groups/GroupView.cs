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
    /// Displays a page to allow the user to view the details about a group.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Group View" )]
    [Category( "Mobile > Groups" )]
    [Description( "Allows the user to view the details about a group." )]
    [IconCssClass( "fa fa-user-friends" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [LinkedPage( "Group Edit Page",
        Description = "The page that will allow editing of the group.",
        IsRequired = false,
        Key = AttributeKeys.GroupEditPage,
        Order = 0 )]

    [BooleanField( "Show Leader List",
        Description = "Specifies if the leader list should be shown, this value is made available to the Template as ShowLeaderList.",
        DefaultBooleanValue = true,
        IsRequired = true,
        Key = AttributeKeys.ShowLeaderList,
        Order = 1 )]

    [BlockTemplateField( "Template",
        Description = "The template to use when rendering the content.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_VIEW,
        IsRequired = true,
        DefaultValue = "",
        Key = AttributeKeys.Template,
        Order = 2 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_GROUPS_GROUP_VIEW_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "3F34AE03-9378-4363-A232-0318139C3BD3")]
    public class GroupView : RockBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the GroupView block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The group edit page attribute key.
            /// </summary>
            public const string GroupEditPage = "GroupEditPage";

            /// <summary>
            /// The show leader list attribute key.
            /// </summary>
            public const string ShowLeaderList = "ShowLeaderList";

            /// <summary>
            /// The template attribute key.
            /// </summary>
            public const string Template = "Template";
        }

        /// <summary>
        /// Gets the group edit page.
        /// </summary>
        /// <value>
        /// The group edit page.
        /// </value>
        protected Guid? GroupEditPage => GetAttributeValue( AttributeKeys.GroupEditPage ).AsGuidOrNull();

        /// <summary>
        /// Gets a value indicating whether the leader list should be shown.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the leader list should be shown; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowLeaderList => GetAttributeValue( AttributeKeys.ShowLeaderList ).AsBoolean();

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
        public static class PageParameterKeys
        {
            /// <summary>
            /// The group identifier key.
            /// </summary>
            public const string GroupGuid = "GroupGuid";
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
                var groupGuid = RequestContext.GetPageParameter( PageParameterKeys.GroupGuid ).AsGuid();
                var group = new GroupService( rockContext ).Get( groupGuid );

                //
                // Verify the group exists.
                //
                if ( group == null )
                {
                    return @"<StackLayout>
    <Rock:NotificationBox Text=""Group not Found."" NotificationType=""Error"" />
</StackLayout>";
                }

                //
                // Verify the user has access to view the group.
                //
                if ( !group.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    return @"<StackLayout>
    <Rock:NotificationBox Text=""You do not have permission to view this group."" NotificationType=""Error"" />
</StackLayout>";
                }

                var mergeFields = RequestContext.GetCommonMergeFields();

                mergeFields.Add( "Group", group );
                mergeFields.Add( "GroupEditPage", GroupEditPage.HasValue ? GroupEditPage.ToString() : string.Empty );
                mergeFields.Add( "ShowLeaderList", ShowLeaderList );

                //
                // Add in all attributes/values that the user is allowed to see.
                //
                group.LoadAttributes( rockContext );
                var attributes = group.Attributes
                    .Where( a => a.Value.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                    .Select( a =>
                    {
                        var rawValue = group.GetAttributeValue( a.Value.Key );
                        return new
                        {
                            a.Value.Key,
                            a.Value.Name,
                            Value = rawValue,
                            FormattedValue = a.Value.FieldType.Field.FormatValue( null, a.Value.EntityTypeId, group.Id, rawValue, a.Value.QualifierValues, false )
                        };
                    } )
                    .OrderBy( a => a.Name )
                    .ToList();
                mergeFields.Add( "VisibleAttributes", attributes );

                //
                // Add collection of allowed security actions.
                //
                var securityActions = new Dictionary<string, object>
                {
                    { "View", group.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) },
                    { "ManageMembers", group.IsAuthorized( Authorization.MANAGE_MEMBERS, RequestContext.CurrentPerson ) },
                    { "Edit", group.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) },
                    { "Administrate", group.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) }
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
