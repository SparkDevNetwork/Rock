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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Attribute;
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
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Group Member List" )]
    [Category( "Mobile > Groups" )]
    [Description( "Allows the user to view a list of members in a group." )]
    [IconCssClass( "fa fa-users" )]

    #region Block Attributes

    [LinkedPage( "Group Member Detail Page",
        Description = "The page that will display the group member details when selecting a member.",
        IsRequired = false,
        Key = AttributeKeys.GroupMemberDetailPage,
        Order = 0 )]

    [TextField( "Title Template",
        Description = "The value to use when rendering the title text. <span class='tip tip-lava'></span>",
        IsRequired = false,
        DefaultValue = "{{ Group.Name }} Group Roster",
        Key = AttributeKeys.TitleTemplate,
        Order = 1 )]

    [BlockTemplateField( "Template",
        Description = "The template to use when rendering the content.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_MEMBER_LIST,
        IsRequired = true,
        DefaultValue = "674CF1E3-561C-430D-B4A8-39957AC1BCF1",
        Key = AttributeKeys.Template,
        Order = 2 )]

    [TextField( "Additional Fields",
        Description = "",
        IsRequired = false,
        DefaultValue = "",
        Category = "CustomSetting",
        Key = AttributeKeys.AdditionalFields,
        Order = 3 )]

    #endregion

    public class GroupMemberList : RockMobileBlockType
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
        /// Gets the template.
        /// </summary>
        /// <value>
        /// The template.
        /// </value>
        protected string Template => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKeys.Template ) );

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

        /// <summary>
        /// Gets the required mobile application binary interface version required to render this block.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version required to render this block.
        /// </value>
        public override int RequiredMobileAbiVersion => 1;

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        public override string MobileBlockType => "Rock.Mobile.Blocks.Groups.GroupMemberList";

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            return new
            {
                Template = Template,
                GroupMemberDetailPage = GroupMemberDetailPage
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

            var properties = new Dictionary<string, string>
            {
                { "Id", "Id" },
                { "Guid", "Guid" },
                { "PersonId", "PersonId" },
                { "FullName", "Person.FullName" },
                { "FirstName", "Person.FirstName" },
                { "NickName", "Person.NickName" },
                { "LastName", "Person.LastName" },
                { "GroupRole", "GroupRole.Name" },
                { "PhotoId", "Person.PhotoId" }
            };

            //
            // Add a custom field for the PhotoUrl since it needs to be custom formatted.
            //
            fields.Add( new FieldSetting
            {
                Key = "PhotoUrl",
                FieldFormat = FieldFormat.String,
                Value = "{{ 'Global' | Attribute:'PublicApplicationRoot' | Append:item.Person.PhotoUrl }}"
            } );

            return MobileHelper.CreateItemLavaTemplate( properties, fields );
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the group details.
        /// </summary>
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
        [TargetType( typeof( GroupMemberList ) )]
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
