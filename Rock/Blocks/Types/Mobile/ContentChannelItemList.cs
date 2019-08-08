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
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

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

    [IntegerField( "Filter Id",
        Description = "The data filter that is used to filter items",
        Key = AttributeKeys.FilterId,
        IsRequired = false,
        DefaultIntegerValue = 0,
        Order = 6,
        Category = "CustomSetting" )]

    [BooleanField( "Query Parameter Filtering",
        Description = "Determines if block should evaluate the query string parameters for additional filter criteria.",
        Key = AttributeKeys.QueryParameterFiltering,
        IsRequired = false,
        Order = 7,
        Category = "CustomSetting" )]

    [BooleanField( "Show Children of Parent",
        Description = "If enabled the block will look for a passed ParentItemId parameter and if found filter for children of this parent item.",
        Key = AttributeKeys.ShowChildrenOfParent,
        IsRequired = false,
        Order = 8,
        DefaultBooleanValue = false,
        Category = "CustomSetting" )]

    [BooleanField( "Check Item Security",
        Description = "Determines if the security of each item should be checked. Recommend not checking security of each item unless required.",
        Key = AttributeKeys.CheckItemSecurity,
        IsRequired = true,
        Order = 9,
        DefaultBooleanValue = false,
        Category = "CustomSetting" )]

    [TextField( "Order",
        Description = "The specifics of how items should be ordered. This value is set through configuration and should not be modified here.",
        Key = AttributeKeys.Order,
        IsRequired = false,
        DefaultValue = "",
        Order = 10,
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
        Description = "The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.",
        IsRequired = false,
        DefaultIntegerValue = 86400,
        Category = "custommobile",
        Order = 1 )]

    #endregion

    public class ContentChannelItemList : RockBlockType, IRockMobileBlockType
    {
        /// <summary>
        /// The key names of all block attributes used by the ContentChannelItemList block.
        /// </summary>
        public static class AttributeKeys
        {
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
            /// The filter identifier key
            /// </summary>
            public const string FilterId = "FilterId";

            /// <summary>
            /// The query parameter filtering key
            /// </summary>
            public const string QueryParameterFiltering = "QueryParameterFiltering";

            /// <summary>
            /// The order key
            /// </summary>
            public const string Order = "Order";

            /// <summary>
            /// The cache duration key
            /// </summary>
            public const string CacheDuration = "CacheDuration";

            /// <summary>
            /// The detail page key
            /// </summary>
            public const string DetailPage = "DetailPage";

            /// <summary>
            /// The show children of parent key
            /// </summary>
            public const string ShowChildrenOfParent = "ShowChildrenOfParent";

            /// <summary>
            /// The check item security key
            /// </summary>
            public const string CheckItemSecurity = "CheckItemSecurity";
        }

        #region Constants

        private const string defaultDataTemplate = @"<StackLayout HeightRequest=""50"" WidthRequest=""200"" Orientation=""Horizontal"" Padding=""0,5,0,5"">
    <Label Text=""{Binding Content}"" />
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
            var checkSecurity = GetAttributeValue( AttributeKeys.CheckItemSecurity ).AsBoolean();
            var skipNumber = pageNumber * pageSize;

            var rockContext = new RockContext();
            var contentChannelItemService = new ContentChannelItemService( rockContext );
            var contentChannelItemAssociationService = new ContentChannelItemAssociationService( rockContext );

            var qry = contentChannelItemService.Queryable().AsNoTracking().Where( i => i.ContentChannelId == contentChannelId );

            //
            // Determine if we should be loading child items from a parent
            //
            var showChildrenOfParent = GetAttributeValue( AttributeKeys.ShowChildrenOfParent ).AsBoolean();
            var parentKeyPassed = RequestContext.GetPageParameters().ContainsKey("ParentItemId");

            if ( parentKeyPassed && showChildrenOfParent )
            {
                var parentItemId = RequestContext.GetPageParameters()["ParentItemId"].AsIntegerOrNull();

                if ( parentItemId.HasValue )
                {
                    var assoctaionsQry = contentChannelItemAssociationService.Queryable().Where( a => a.ContentChannelItemId == parentItemId );

                    qry = qry.Where( i => assoctaionsQry.Any( a => a.ChildContentChannelItemId == i.Id ) );
                }
            }

            //
            // Apply custom filtering.
            //
            qry = FilterResults( rockContext, contentChannelItemService, qry );

            //
            // Apply custom sorting to the results.
            //
            qry = SortResults( qry );

            var results = new List<ContentChannelItem>();

            //
            // Determine if we need to check the security of the items. Is can be slow, especially for channels with LOTS of items.
            //
            if ( checkSecurity )
            {
                // We have to take all items to check security to ensure we have enough to return the desired item count
                results = qry.ToList();
                foreach ( var item in results )
                {
                    if ( item.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                    {
                        results.Add( item );
                    }
                }

                // Take the final number of items requested.
                results = results.Skip( skipNumber )
                            .Take( pageSize )
                            .ToList();
            }
            else
            {
                // Just take the number requested
                results = qry.Skip( skipNumber )
                            .Take( pageSize )
                            .ToList();
            }

            // Load attributes
            foreach ( var item in results )
            {
                item.LoadAttributes( rockContext );
            }

            var followedItemIds = GetFollowedItemIds( rockContext, results );

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

        /// <summary>
        /// Filters the results.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="service">The service.</param>
        /// <param name="itemQry">The item qry.</param>
        /// <returns></returns>
        private IQueryable<ContentChannelItem> FilterResults( RockContext rockContext, ContentChannelItemService service, IQueryable<ContentChannelItem> itemQry )
        {
            var contentChannelId = GetAttributeValue( AttributeKeys.ContentChannel ).AsInteger();
            var itemType = typeof( Rock.Model.ContentChannelItem );
            var paramExpression = service.ParameterExpression;

            //
            // Apply Data Filter.
            //
            int? dataFilterId = GetAttributeValue( AttributeKeys.FilterId ).AsIntegerOrNull();
            if ( dataFilterId.HasValue )
            {
                var dataFilterService = new DataViewFilterService( rockContext );
                var dataFilter = dataFilterService.Queryable( "ChildFilters" ).FirstOrDefault( a => a.Id == dataFilterId.Value );
                var errorMessages = new List<string>();
                Expression whereExpression = dataFilter?.GetExpression( itemType, service, paramExpression, errorMessages );

                itemQry = itemQry.Where( paramExpression, whereExpression, null );
            }

            //
            // Apply page parameter filtering.
            //
            var pageParameters = RequestContext.GetPageParameters();
            if ( GetAttributeValue( AttributeKeys.QueryParameterFiltering ).AsBoolean() && pageParameters.Count > 0 )
            {
                var propertyFilter = new Rock.Reporting.DataFilter.PropertyFilter();

                foreach ( var kvp in pageParameters )
                {
                    var selection = new List<string>();

                    // Since there could be many matches by the key name for an attribute we have to construct the unique name used by EntityHelper.FindFromFilterSelection and use that
                    var attributeService = new AttributeService( rockContext );
                    var attributeGuid = attributeService
                        .Queryable()
                        .Where( a => a.EntityTypeQualifierColumn == "ContentChannelId" )
                        .Where( a => a.EntityTypeQualifierValue == contentChannelId.ToString() )
                        .Where( a => a.Key == kvp.Key )
                        .Select( a => a.Guid )
                        .FirstOrDefault();

                    string uniqueName = kvp.Key;
                    if ( attributeGuid != null )
                    {
                        uniqueName = string.Format( "Attribute_{0}_{1}", kvp.Key, attributeGuid.ToString().Replace( "-", string.Empty ) );
                    }

                    // Keep using uniquename for attributes since common keys (e.g. "category")will return mutliple values
                    selection.Add( uniqueName );

                    var entityField = Rock.Reporting.EntityHelper.FindFromFilterSelection( itemType, uniqueName, false, false );
                    if ( entityField != null )
                    {
                        string value = kvp.Value;
                        switch ( entityField.FieldType.Guid.ToString().ToUpper() )
                        {
                            case Rock.SystemGuid.FieldType.DAY_OF_WEEK:
                            case Rock.SystemGuid.FieldType.SINGLE_SELECT:
                                {
                                    selection.Add( value );
                                }
                                break;
                            case Rock.SystemGuid.FieldType.MULTI_SELECT:
                                {
                                    selection.Add( ComparisonType.Contains.ConvertToInt().ToString() );
                                    selection.Add( value );
                                }
                                break;
                            default:
                                {
                                    selection.Add( ComparisonType.EqualTo.ConvertToInt().ToString() );
                                    selection.Add( value );
                                }
                                break;
                        }

                        itemQry = itemQry.Where( paramExpression, propertyFilter.GetExpression( itemType, service, paramExpression, selection.ToJson() ) );
                    }
                }
            }

            return itemQry;
        }

        /// <summary>
        /// Gets the followed item ids.
        /// </summary>
        /// <param name="includeFollowing">if set to <c>true</c> [include following].</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="results">The results.</param>
        /// <returns></returns>
        private List<int> GetFollowedItemIds( RockContext rockContext, List<ContentChannelItem> results )
        {
            List<int> followedItemIds = new List<int>();

            //
            // Get the ids of items that are followed by the current person
            //
            if ( GetAttributeValue( AttributeKeys.IncludeFollowing ).AsBoolean() && RequestContext.CurrentPerson != null )
            {
                var resultIds = results.Select( r => r.Id ).ToList();
                var contentChannelItemEntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.CONTENT_CHANNEL_ITEM.AsGuid() ).Id;

                followedItemIds = new FollowingService( rockContext ).Queryable().AsNoTracking()
                    .Where( f =>
                        f.EntityTypeId == contentChannelItemEntityTypeId &&
                        resultIds.Contains( f.EntityId ) &&
                        f.PersonAlias.PersonId == RequestContext.CurrentPerson.Id )
                    .Select( f => f.EntityId )
                    .ToList();
            }

            return followedItemIds;
        }

        /// <summary>
        /// Sorts the results.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        private IQueryable<ContentChannelItem> SortResults( IQueryable<ContentChannelItem> items )
        {
            SortProperty sortProperty = null;

            string orderBy = GetAttributeValue( "Order" );

            if ( !string.IsNullOrWhiteSpace( orderBy ) )
            {
                var fieldDirection = new List<string>();
                var orderByPairs = orderBy.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries )
                    .Select( a => a.Split( '^' ) );

                foreach ( var itemPair in orderByPairs )
                {
                    if ( itemPair.Length == 2 && !string.IsNullOrWhiteSpace( itemPair[0] ) )
                    {
                        var sortDirection = SortDirection.Ascending;
                        if ( !string.IsNullOrWhiteSpace( itemPair[1] ) )
                        {
                            sortDirection = itemPair[1].ConvertToEnum<SortDirection>( SortDirection.Ascending );
                        }
                        fieldDirection.Add( itemPair[0] + ( sortDirection == SortDirection.Descending ? " desc" : "" ) );
                    }
                }

                sortProperty = new SortProperty
                {
                    Direction = SortDirection.Ascending,
                    Property = fieldDirection.AsDelimited( "," )
                };

                string[] columns = sortProperty.Property.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

                IOrderedQueryable<ContentChannelItem> orderedQry = null;

                for ( int columnIndex = 0; columnIndex < columns.Length; columnIndex++ )
                {
                    string column = columns[columnIndex].Trim();

                    var direction = sortProperty.Direction;
                    if ( column.ToLower().EndsWith( " desc" ) )
                    {
                        column = column.Left( column.Length - 5 );
                        direction = sortProperty.Direction == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
                    }

                    try
                    {
                        if ( column.StartsWith( "Attribute:" ) )
                        {
                            string attributeKey = column.Substring( 10 );

                            if ( direction == SortDirection.Ascending )
                            {
                                orderedQry = ( columnIndex == 0 ) ?
                                    items.OrderBy( i => i.AttributeValues.Where( v => v.Key == attributeKey ).FirstOrDefault().Value.SortValue ) :
                                    orderedQry.ThenBy( i => i.AttributeValues.Where( v => v.Key == attributeKey ).FirstOrDefault().Value.SortValue );
                            }
                            else
                            {
                                orderedQry = ( columnIndex == 0 ) ?
                                    items.OrderByDescending( i => i.AttributeValues.Where( v => v.Key == attributeKey ).FirstOrDefault().Value.SortValue ) :
                                    orderedQry.ThenByDescending( i => i.AttributeValues.Where( v => v.Key == attributeKey ).FirstOrDefault().Value.SortValue );
                            }
                        }
                        else
                        {
                            if ( direction == SortDirection.Ascending )
                            {
                                orderedQry = ( columnIndex == 0 ) ? items.OrderBy( column ) : orderedQry.ThenBy( column );
                            }
                            else
                            {
                                orderedQry = ( columnIndex == 0 ) ? items.OrderByDescending( column ) : orderedQry.ThenByDescending( column );
                            }
                        }
                    }
                    catch { }
                }

                try
                {
                    if ( orderedQry != null )
                    {
                        return orderedQry;
                    }
                }
                catch { }

            }

            // If we got here we did not have any sort requested, add a default by start date so pagination will work
            return items.OrderByDescending( i => i.StartDateTime);
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
