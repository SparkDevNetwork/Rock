using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Text;

using Newtonsoft.Json;

using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile
{
    /// <summary>
    /// Lists content channel items for a given channel and allow the user to
    /// format how they are displayed with XAML.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    /// <seealso cref="Rock.Blocks.IRockMobileBlockType" />

    [DisplayName( "Content Channel Item List" )]
    [Category( "Mobile" )]
    [Description( "Lists content channel items for a given channel." )]
    [IconCssClass( "fa fa-th-list" )]

    #region Block Attributes
    
    [ContentChannelField(
        "Content Channel",
        Description = "The content channel to retrieve the items for.",
        Key = AttributeKeys.ContentChannel,
        Order = 1,
        Category = "CustomSetting" )]

    [TextField(
        "Page Size",
        Description = "The number of items to send per page.",
        Key = AttributeKeys.PageSize,
        DefaultValue = "50",
        Order = 2,
        Category = "CustomSetting" )]

    [BooleanField(
        "Include Following",
        Description = "Determines if following data should be sent along with the results.",
        Key = AttributeKeys.IncludeFollowing,
        Order = 3,
        Category = "CustomSetting" )]

    [LinkedPage( "Detail Page",
        Description = "The page to redirect to when selecting an item.",
        Key = AttributeKeys.DetailPage,
        IsRequired = false,
        Order = 4,
        Category = "CustomSetting" )]

    [TextField(
        "Field Settings",
        Description = "JSON object of the configured fields to show.",
        Key = AttributeKeys.FieldSettings,
        Order = 5,
        Category = "CustomSetting" )]

    [CodeEditorField(
        "List Data Template",
        Description = "The XAML for the lists data template.",
        Key = AttributeKeys.ListDataTemplate,
        Order = 0,
        EditorMode = Web.UI.Controls.CodeEditorMode.Xml,
        DefaultValue = defaultDataTemplate,
        Category = "custommobile")]

    [IntegerField( "Cache Duration",
        "The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.",
        false,
        86400,
        category: "custommobile",
        order: 1 )]

    #endregion

    public class ContentChannelItemList : RockBlockType, IRockMobileBlockType
    {
        /// <summary>
        /// The key names of all block attributes used by the ContentChannelItemList block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The lava template key
            /// </summary>
            public const string LavaTemplate = "LavaTemplate";

            /// <summary>
            /// The content channel key
            /// </summary>
            public const string ContentChannel = "ContentChannel";

            /// <summary>
            /// The field settings key
            /// </summary>
            public const string FieldSettings = "FieldSettings";

            /// <summary>
            /// The page size key
            /// </summary>
            public const string PageSize = "PageSize";

            /// <summary>
            /// The include following key
            /// </summary>
            public const string IncludeFollowing = "IncludeFollowing";

            /// <summary>
            /// The list data template key
            /// </summary>
            public const string ListDataTemplate = "ListDataTemplate";

            /// <summary>
            /// The cache duration key
            /// </summary>
            public const string CacheDuration = "CacheDuration";

            /// <summary>
            /// The detail page key
            /// </summary>
            public const string DetailPage = "DetailPage";
        }

        #region Constants

        private const string defaultDataTemplate = @"<StackLayout HeightRequest=""50"" WidthRequest=""200"" Orientation=""Horizontal"" Padding=""0,5,0,5"">
    <Label Text=""{Binding [Content]}"" />
</StackLayout>";

        #endregion

        #region IRockMobileBlockType Implementation

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        public string MobileBlockType => "Rock.Mobile.Blocks.CollectionViewList";

        /// <summary>
        /// Gets the required mobile application binary interface version.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version.
        /// </value>
        public int RequiredMobileAbiVersion => 1;

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public object GetMobileConfigurationValues()
        {
            return new Dictionary<string, object>
            {
                { "ItemSelectedPage", GetAttributeValue( AttributeKeys.DetailPage ) },
                { "ItemSelectedParameter", "ContentChannelItemId" },
                { "ItemSelectedKey", "Id" }
            };
        }

        #endregion

        #region Actions

        /// <summary>
        /// Gets the items that should be displayed on the given page.
        /// </summary>
        /// <param name="pageNumber">The page number whose items are returned, the first page being 0.</param>
        /// <returns>A JSON string that defines the items to be displayed.</returns>
        [BlockAction]
        public object GetItems( int pageNumber = 0 )
        {
            var contentChannelId = GetAttributeValue( AttributeKeys.ContentChannel ).AsInteger();
            var pageSize = GetAttributeValue( AttributeKeys.PageSize ).AsInteger();
            var includeFollowing = GetAttributeValue( AttributeKeys.IncludeFollowing ).AsBoolean();

            var skipNumber = pageNumber * pageSize;

            var rockContext = new RockContext();

            var results = new ContentChannelItemService( rockContext ).Queryable().AsNoTracking()
                            .Where( i => i.ContentChannelId == contentChannelId )
                            .OrderBy( i => i.Id )  // TODO make this a setting
                            .Skip( skipNumber )
                            .Take( pageSize )
                            .ToList();

            List<int> followedItemIds = new List<int>();

            // Get the ids of items that are followed by the current person
            if ( includeFollowing )
            {
                var currentPerson = GetCurrentPerson();

                if ( currentPerson != null )
                {
                    var resultIds = results.Select( r => r.Id ).ToList();
                    var contentChannelItemEntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.CONTENT_CHANNEL_ITEM.AsGuid() ).Id;

                    followedItemIds = new FollowingService( rockContext ).Queryable().AsNoTracking()
                                        .Where( f =>
                                            f.EntityTypeId == contentChannelItemEntityTypeId &&
                                            resultIds.Contains( f.EntityId ) &&
                                            f.PersonAlias.PersonId == currentPerson.Id )
                                        .Select( f => f.EntityId )
                                        .ToList();
                }
            }

            var lavaTemplate = CreateLavaTemplate( followedItemIds );

            var commonMergeFields = new CommonMergeFieldsOptions
            {
                GetLegacyGlobalMergeFields = false
            };

            var mergeFields = RequestContext.GetCommonMergeFields( null, commonMergeFields );
            mergeFields.Add( "Items", results );
            mergeFields.Add( "FollowedItemIds", followedItemIds );

            var output = lavaTemplate.ResolveMergeFields( mergeFields );

            return ActionOk( new StringContent( output, Encoding.UTF8, "application/json" ) );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates the lava template from the list of fields.
        /// </summary>
        /// <returns></returns>
        private string CreateLavaTemplate( List<int> followedItemIds )
        {
            var fieldSettingJson = GetAttributeValue( AttributeKeys.FieldSettings );
            var fields = JsonConvert.DeserializeObject<List<FieldSetting>>( fieldSettingJson );

            var contentChannelItemEntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.CONTENT_CHANNEL_ITEM.AsGuid() ).Id;

            var template = new StringBuilder();
            template.AppendLine( "[" );
            template.AppendLine( "    {% for item in Items %}" );
            template.AppendLine( "    {" );

            for ( int i = 0; i < fields.Count; i++ )
            {
                var field = fields[i];

                template.AppendLine( string.Format( @"        {{% jsonproperty name:'{0}' format:'{1}' %}}{2}{{% endjsonproperty %}},", field.Key, field.FieldFormat, field.Value ) );
            }

            // Append the fields we'd need for the following button
            template.AppendLine(                "    \"EntityId\": {{ item.Id }}," );
            template.AppendLine( string.Format( "    \"EntityTypeId\": {0}, ", contentChannelItemEntityTypeId ) );
            template.AppendLine(                "    \"IsFollowing\": {{ FollowedItemIds | Contains:item.Id }} " );

            template.Append( "    }" );
            template.AppendLine( "{% if forloop.last != true %},{% endif %}" );
            template.AppendLine( "    {% endfor %}" );
            template.AppendLine( "]" );

            return template.ToString();
        }

        #endregion

        #region Custom Settings

        /// <summary>
        /// Defines the control that will provide the Basic Settings tab content
        /// for the ContentChannelItemList block.
        /// </summary>
        /// <seealso cref="Rock.Web.RockCustomSettingsUserControlProvider" />
        [TargetType( typeof( ContentChannelItemList ) )]
        public class ContentChannelItemListCustomSettingsProvider : RockCustomSettingsUserControlProvider
        {
            /// <summary>
            /// Gets the path to the user control file.
            /// </summary>
            /// <value>
            /// The path to the user control file.
            /// </value>
            protected override string UserControlPath => "~/Blocks/Mobile/ContentChannelListSettings.ascx";

            /// <summary>
            /// Gets the custom settings title. Used when displaying tabs or links to these settings.
            /// </summary>
            /// <value>
            /// The custom settings title.
            /// </value>
            public override string CustomSettingsTitle => "Basic Settings";
        }

        #endregion

        #region POCOs

        /// <summary>
        /// POCO to store the settings for the fields
        /// </summary>
        public class FieldSetting
        {
            /// <summary>
            /// Creates an identifier based off the key. This is used for grid operations.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id
            {
                get
                {
                    return this.Key.GetHashCode();
                }
            }

            /// <summary>
            /// Gets or sets the field key.
            /// </summary>
            /// <value>
            /// The key.
            /// </value>
            public string Key { get; set; }

            /// <summary>
            /// Gets or sets the field value.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public string Value { get; set; }

            /// <summary>
            /// Gets or sets the name of the property.
            /// </summary>
            /// <value>
            /// The name of the property.
            /// </value>
            public string FieldName { get; set; }

            /// <summary>
            /// Gets or sets the field source.
            /// </summary>
            /// <value>
            /// The field source.
            /// </value>
            public FieldSource FieldSource { get; set; }

            /// <summary>
            /// Gets or sets the attribute format.
            /// </summary>
            /// <value>
            /// The attribute format.
            /// </value>
            public AttributeFormat AttributeFormat { get; set; }

            /// <summary>
            /// Gets or sets the field format.
            /// </summary>
            /// <value>
            /// The field format.
            /// </value>
            public FieldFormat FieldFormat { get; set; }
        }

        /// <summary>
        /// The source of the data for the field. The two types are properties on the item model and an attribute expression.
        /// </summary>
        public enum FieldSource
        {
            /// <summary>
            /// The source comes from a model property.
            /// </summary>
            Property = 0,

            /// <summary>
            /// The source comes from an attribute defined on the model.
            /// </summary>
            Attribute = 1,

            /// <summary>
            /// The source comes from a custom lava expression.
            /// </summary>
            LavaExpression = 2
        }

        /// <summary>
        /// The format to use for the attribute.
        /// </summary>
        public enum AttributeFormat
        {
            /// <summary>
            /// The attribute's friendly value should be used.
            /// </summary>
            FriendlyValue = 0,

            /// <summary>
            /// The attribute's raw value should be used.
            /// </summary>
            RawValue = 1
        }

        /// <summary>
        /// Determines the field's format. This will be used to properly format the Json sent to the client.
        /// </summary>
        public enum FieldFormat
        {
            /// <summary>
            /// The value will be formatted as a string.
            /// </summary>
            String = 0,

            /// <summary>
            /// The value will be formatted as a number.
            /// </summary>
            Number = 1,

            /// <summary>
            /// The value will be formatted as a datetime.
            /// </summary>
            Date = 2,

            /// <summary>
            /// The value will be formatted as a boolean.
            /// </summary>
            Boolean = 3
        }

        #endregion
    }
}
