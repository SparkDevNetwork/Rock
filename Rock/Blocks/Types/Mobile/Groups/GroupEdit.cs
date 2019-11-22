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
using System.Text;

using Rock.Attribute;
using Rock.Data;
using Rock.Mobile.Common.Blocks.Content;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Groups
{
    /// <summary>
    /// Displays custom XAML content on the page.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Group Edit" )]
    [Category( "Mobile > Groups" )]
    [Description( "Edits the basic settings of a group." )]
    [IconCssClass( "fa fa-users-cog" )]

    #region Block Attributes

    [BooleanField( "Show Group Name",
        Description = "",
        IsRequired = true,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.ShowGroupName,
        Order = 0 )]

    [BooleanField( "Enable Group Name Edit",
        Description = "",
        IsRequired = true,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.EnableGroupNameEdit,
        Order = 1 )]

    [BooleanField( "Show Description",
        Description = "",
        IsRequired = true,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.ShowDescription,
        Order = 2 )]

    [BooleanField( "Enable Description Edit",
        Description = "",
        IsRequired = true,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.EnableDescriptionEdit,
        Order = 3 )]

    [BooleanField( "Show Campus",
        Description = "",
        IsRequired = true,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.ShowCampus,
        Order = 4 )]

    [BooleanField( "Enable Campus Edit",
        Description = "",
        IsRequired = true,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.EnableCampusEdit,
        Order = 5 )]

    [BooleanField( "Show Group Capacity",
        Description = "",
        IsRequired = true,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.ShowGroupCapacity,
        Order = 6 )]

    [BooleanField( "Enable Group Capacity Edit",
        Description = "",
        IsRequired = true,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.EnableGroupCapacityEdit,
        Order = 7 )]

    [BooleanField( "Show Active Status",
        Description = "",
        IsRequired = true,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.ShowActiveStatus,
        Order = 8 )]

    [BooleanField( "Enable Active Status Edit",
        Description = "",
        IsRequired = true,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.EnableActiveStatusEdit,
        Order = 9 )]

    [BooleanField( "Show Public Status",
        Description = "",
        IsRequired = true,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.ShowPublicStatus,
        Order = 10 )]

    [BooleanField( "Enable Public Status Edit",
        Description = "",
        IsRequired = true,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.EnablePublicStatusEdit,
        Order = 11 )]

    [AttributeCategoryField( "Attribute Category",
        Description = "Category of attributes to show and allow editing on.",
        IsRequired = false,
        EntityType = typeof( Group ),
        Key = AttributeKeys.AttributeCategory,
        Order = 12 )]

    [LinkedPage( "Group Detail Page",
        Description = "The group detail page to return to, if not set then the edit page is popped off the navigation stack.",
        IsRequired = false,
        Key = AttributeKeys.GroupDetailPage,
        Order = 13 )]

    #endregion

    public class GroupEdit : RockMobileBlockType
    {
        /// <summary>
        /// The block setting attribute keys for the GroupEdit block.
        /// </summary>
        public static class AttributeKeys
        {
            public const string ShowGroupName = "ShowGroupName";

            public const string EnableGroupNameEdit = "EnableGroupNameEdit";

            public const string ShowDescription = "ShowDescription";

            public const string EnableDescriptionEdit = "EnableDescriptionEdit";

            public const string ShowCampus = "ShowCampus";

            public const string EnableCampusEdit = "EnableCampusEdit";

            public const string ShowGroupCapacity = "ShowGroupCapacity";

            public const string EnableGroupCapacityEdit = "EnableGroupCapacityEdit";

            public const string ShowActiveStatus = "ShowActiveStatus";

            public const string EnableActiveStatusEdit = "EnableActiveStatusEdit";

            public const string ShowPublicStatus = "ShowPublicStatus";

            public const string EnablePublicStatusEdit = "EnablePublicStatusEdit";

            public const string AttributeCategory = "AttributeCategory";

            public const string GroupDetailPage = "GroupDetailPage";
        }

        /// <summary>
        /// The page parameter keys for the GroupEdit block.
        /// </summary>
        public static class PagePArameterKeys
        {
            public const string GroupId = "GroupId";
        }

        #region Attribute Properties

        protected bool ShowGroupName => GetAttributeValue( AttributeKeys.ShowGroupName ).AsBoolean();

        protected bool EnableGroupNameEdit => GetAttributeValue( AttributeKeys.EnableGroupNameEdit ).AsBoolean();

        protected bool ShowDescription => GetAttributeValue( AttributeKeys.ShowDescription ).AsBoolean();

        protected bool EnableDescriptionEdit => GetAttributeValue( AttributeKeys.EnableDescriptionEdit ).AsBoolean();

        protected bool ShowCampus => GetAttributeValue( AttributeKeys.ShowCampus ).AsBoolean();

        protected bool EnableCampusEdit => GetAttributeValue( AttributeKeys.EnableCampusEdit ).AsBoolean();

        protected bool ShowGroupCapacity => GetAttributeValue( AttributeKeys.ShowGroupCapacity ).AsBoolean();

        protected bool EnableGroupCapacityEdit => GetAttributeValue( AttributeKeys.EnableGroupCapacityEdit ).AsBoolean();

        protected bool ShowActiveStatus => GetAttributeValue( AttributeKeys.ShowActiveStatus ).AsBoolean();

        protected bool EnableActiveStatusEdit => GetAttributeValue( AttributeKeys.EnableActiveStatusEdit ).AsBoolean();

        protected bool ShowPublicStatus => GetAttributeValue( AttributeKeys.ShowPublicStatus ).AsBoolean();

        protected bool EnablePublicStatusEdit => GetAttributeValue( AttributeKeys.EnablePublicStatusEdit ).AsBoolean();

        protected Guid? AttributeCategory => GetAttributeValue( AttributeKeys.AttributeCategory ).AsGuidOrNull();

        protected Guid? GroupDetailPage => GetAttributeValue( AttributeKeys.GroupDetailPage ).AsGuidOrNull();

        #endregion

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
        public override string MobileBlockType => "Rock.Mobile.Blocks.Group.GroupEdit";

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
            return new Rock.Mobile.Common.Blocks.Content.Configuration
            {
                Content = null,
                DynamicContent = true,
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the content to be displayed on the block.
        /// </summary>
        /// <returns>A string containing the XAML content to be displayed.</returns>
        private string BuildContent()
        {
            string content = @"
<StackLayout>
    <Label Text=""Group Details"" StyleClass=""heading1"" />
    <BoxView Color=""#888"" HeightRequest=""1"" Margin=""0 0 12 0"" />

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

    <Button StyleClass=""btn,btn-link"" Text=""Cancel"" Command=""{Binding PopPage}"" />
</StackLayout>";

            var parameters = new Dictionary<string, string>();
            string fieldsContent;

            using ( var rockContext = new RockContext() )
            {
                var groupId = RequestContext.GetPageParameter( PagePArameterKeys.GroupId ).AsInteger();
                var group = new GroupService( rockContext ).Get( groupId );

                if ( group == null )
                {
                    return "<Rock:NotificationBox HeaderText=\"Error\" Text=\"We couldn't find that group.\" NotificationType=\"Error\" />";
                }
                else if ( !group.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return "<Rock:NotificationBox HeaderText=\"Error\" Text=\"You are not authorized to edit this group.\" NotificationType=\"Error\" />";
                }

                group.LoadAttributes( rockContext );
                var attributes = GetEditableAttributes( group, rockContext );

                fieldsContent = BuildCommonFields( group, parameters );
                fieldsContent += BuildAttributeFields( group, attributes, parameters );
            }

            var validatorsContent = parameters.Keys.Select( a => $"<x:Reference>{a}</x:Reference>" );
            var parametersContent = parameters.Select( a => $"<Rock:ActionParameter Name=\"{a.Key}\" Value=\"{{Binding {a.Value}, Source={{x:Reference {a.Key}}}}}\" />" );

            return content.Replace( "##FIELDS##", fieldsContent )
                .Replace( "##VALIDATORS##", string.Join( string.Empty, validatorsContent ) )
                .Replace( "##PARAMETERS##", string.Join( string.Empty, parametersContent ) );
        }

        /// <summary>
        /// Builds the common fields.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>A string containing the XAML that represents the common Group fields.</returns>
        private string BuildCommonFields( Group group, Dictionary<string, string> parameters )
        {
            var sb = new StringBuilder();

            if ( ShowGroupName )
            {
                if ( EnableGroupNameEdit )
                {
                    AppendField( sb, $"<Rock:TextBox x:Name=\"name\" Label=\"Name\" IsRequired=\"true\" Text=\"{group.Name.EncodeXml( true )}\" />" );
                    parameters.Add( "name", "Text" );
                }
                else
                {
                    AppendLiteral( sb, "Name", group.Name );
                }
            }

            if ( ShowDescription )
            {
                if ( EnableDescriptionEdit )
                {
                    AppendField( sb, $"<Rock:TextBox x:Name=\"description\" Label=\"Description\" IsRequired=\"false\" Text=\"{group.Description.EncodeXml( true )}\" />" );
                    parameters.Add( "description", "Text" );
                }
                else
                {
                    AppendLiteral( sb, "Description", group.Name );
                }
            }

            if ( ShowCampus )
            {
                if ( EnableCampusEdit )
                {
                    AppendField( sb, $"<Rock:CampusPicker x:Name=\"campus\" Label=\"Campus\" IsRequired=\"{group.GroupType.GroupsRequireCampus}\" SelectedValue=\"{group.Campus?.Guid.ToStringSafe()}\" />" );
                    parameters.Add( "campus", "SelectedValue" );
                }
                else
                {
                    AppendLiteral( sb, "Campus", group.Campus.Name );
                }
            }

            if ( ShowGroupCapacity && group.GroupType.GroupCapacityRule != GroupCapacityRule.None )
            {
                if ( EnableGroupCapacityEdit )
                {
                    AppendField( sb, $"<Rock:NumberBox x:Name=\"capacity\" Label=\"Capacity\" IsRequired=\"false\" Text=\"{group.GroupCapacity}\" />" );
                    parameters.Add( "capacity", "Text" );
                }
                else
                {
                    AppendLiteral( sb, "Group Capacity", group.Name );
                }
            }

            if ( ShowActiveStatus )
            {
                if ( EnableActiveStatusEdit )
                {
                    AppendField( sb, $"<Rock:CheckBox x:Name=\"active\" Label=\"Is Active\" IsRequired=\"false\" IsChecked=\"{group.IsActive}\" />", false );
                    parameters.Add( "active", "IsChecked" );
                }
                else
                {
                    AppendLiteral( sb, "Is Active", group.IsActive ? "Yes" : "No" );
                }
            }

            if ( ShowPublicStatus )
            {
                if ( EnablePublicStatusEdit )
                {
                    AppendField( sb, $"<Rock:CheckBox x:Name=\"public\" Label=\"Is Public\" IsRequired=\"false\" IsChecked=\"{group.IsPublic}\" />", false );
                    parameters.Add( "public", "IsChecked" );
                }
                else
                {
                    AppendLiteral( sb, "Is Public", group.IsPublic ? "Yes" : "No" );
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Builds the attribute fields.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>A XAML string that contains any attribute fields as well as the header text.</returns>
        private string BuildAttributeFields( Group group, List<AttributeCache> attributes, Dictionary<string, string> parameters )
        {
            if ( !attributes.Any() )
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            sb.AppendLine( "<Label Text=\"Attributes\" StyleClass=\"heading1\" />" );
            sb.AppendLine( "<BoxView Color=\"#888\" HeightRequest=\"1\" Margin=\"0 0 12 0\" />" );

            foreach ( var attribute in attributes )
            {
                var fieldName = $"attribute_{attribute.Id}";
                var label = attribute.AbbreviatedName.IsNotNullOrWhiteSpace() ? attribute.AbbreviatedName : attribute.Name;
                var configurationValues = attribute.QualifierValues
                    .ToDictionary( a => a.Key, a => a.Value.Value )
                    .ToJson();

                AppendField( sb, $"<Rock:AttributeValueEditor x:Name=\"{fieldName}\" Label=\"{label}\" IsRequired=\"{attribute.IsRequired}\" FieldType=\"{attribute.FieldType.Class}\" ConfigurationValues=\"{{}}{configurationValues.EncodeXml( true )}\" Value=\"{group.GetAttributeValue( attribute.Key ).EncodeXml( true )}\" />" );
                parameters.Add( fieldName, "Value" );
            }

            return sb.ToString();
        }

        /// <summary>
        /// Appends the XAML field to the StringBuilder.
        /// </summary>
        /// <param name="sb">The string builder to append to.</param>
        /// <param name="field">The field.</param>
        /// <param name="wrapped">if set to <c>true</c> the SingleField wraps the field in a border.</param>
        private void AppendField( StringBuilder sb, string field, bool wrapped = true )
        {
            sb.AppendLine( $"<Rock:SingleField Wrapped=\"{wrapped}\">{field}</Rock:SingleField>" );
        }

        /// <summary>
        /// Appends the literal text field.
        /// </summary>
        /// <param name="sb">The string builder to append to..</param>
        /// <param name="label">The label of the field.</param>
        /// <param name="text">The text content of the field.</param>
        private void AppendLiteral( StringBuilder sb, string label, string text )
        {
            AppendField( sb, $"<Rock:Literal Label=\"{label.EncodeXml( true )}\" Text=\"{text.EncodeXml( true )}\" />", false );
        }

        /// <summary>
        /// Saves the group.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The response to send back to the client.</returns>
        private CallbackResponse SaveGroup( Dictionary<string, object> parameters )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupId = RequestContext.GetPageParameter( PagePArameterKeys.GroupId ).AsInteger();
                var group = new GroupService( rockContext ).Get( groupId );

                if ( group == null || !group.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return new CallbackResponse
                    {
                        Error = "You are not authorized to edit this group."
                    };
                }

                group.LoadAttributes( rockContext );

                //
                // Verify and save all the property values.
                //
                if ( ShowGroupName && EnableGroupNameEdit )
                {
                    group.Name = ( string ) parameters["name"];
                }

                if ( ShowDescription && EnableDescriptionEdit )
                {
                    group.Description = ( string ) parameters["description"];
                }

                if ( ShowCampus && EnableCampusEdit )
                {
                    group.CampusId = CampusCache.Get( ( ( string ) parameters["campus"] ).AsGuid() )?.Id;
                }

                if ( ShowGroupCapacity && EnableGroupCapacityEdit && group.GroupType.GroupCapacityRule != GroupCapacityRule.None )
                {
                    group.GroupCapacity = ( ( string ) parameters["capacity"] ).AsIntegerOrNull();
                }

                if ( ShowActiveStatus && EnableActiveStatusEdit )
                {
                    group.IsActive = ( bool ) parameters["active"];
                }

                if ( ShowPublicStatus && EnablePublicStatusEdit )
                {
                    group.IsPublic = ( bool ) parameters["public"];
                }

                //
                // Save all the attribute values.
                //
                var attributes = GetEditableAttributes( group, rockContext );
                if ( attributes.Any() )
                {
                    foreach ( var attribute in attributes )
                    {
                        var keyName = $"attribute_{attribute.Id}";
                        if ( parameters.ContainsKey( keyName ) )
                        {
                            group.SetAttributeValue( attribute.Key, parameters[keyName].ToStringSafe() );
                        }
                    }
                }

                //
                // Save all changes to database.
                //
                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    group.SaveAttributeValues( rockContext );
                } );
            }

            if ( GroupDetailPage.HasValue )
            {
                return new CallbackResponse
                {
                    Command = "ReplacePage",
                    CommandParameter = $"{GroupDetailPage}?GroupId={RequestContext.GetPageParameter( PagePArameterKeys.GroupId )}"
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

        /// <summary>
        /// Gets the editable attributes.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<AttributeCache> GetEditableAttributes( Group group, RockContext rockContext )
        {
            if ( AttributeCategory.HasValue )
            {
                var category = CategoryCache.Get( AttributeCategory.Value );

                if ( category != null )
                {
                    group.LoadAttributes( rockContext );

                    return group.Attributes.Values
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
        /// Gets the current configuration for this block.
        /// </summary>
        /// <returns>A collection of string/string pairs.</returns>
        [BlockAction]
        public object GetInitialContent()
        {
                return new Rock.Mobile.Common.Blocks.Content.Configuration
                {
                    Content = BuildContent()
                };
        }

        [BlockAction]
        [RockObsolete( "1.10.2" )]
        public object GetCurrentConfig()
        {
            return GetInitialContent();
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
                return SaveGroup( parameters );
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
    }
}
