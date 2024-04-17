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
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Cms.ContentCollection;
using Rock.Cms.ContentCollection.IndexDocuments;
using Rock.Cms.ContentCollection.Search;
using Rock.Data;
using Rock.Enums.Blocks.Cms.ContentCollectionView;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.ContentCollectionView;
using Rock.ViewModels.Cms;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the search results of a particular content collection.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Content Collection View" )]
    [Category( "CMS" )]
    [Description( "Displays the search results of a particular content collection." )]
    [IconCssClass( "fa fa-book-open" )]
    [SupportedSiteTypes( Model.SiteType.Web, Model.SiteType.Mobile )]

    #region Block Attributes

    [TextField( "Content Collection",
        Description = "The content collection to use when searching.",
        Category = "CustomSetting",
        Key = AttributeKey.ContentCollection )]

    [BooleanField( "Show Filters",
        Description = "Determines if the filters should be visible.",
        DefaultBooleanValue = true,
        Category = "CustomSetting",
        Key = AttributeKey.ShowFiltersPanel )]

    [BooleanField( "Show Full-Text Search",
        Description = "Determines if the full-text search box is visible.",
        DefaultBooleanValue = true,
        Category = "CustomSetting",
        Key = AttributeKey.ShowFullTextSearch )]

    [BooleanField( "Show Sort",
        Description = "Determines if the custom sorting options are displayed.",
        DefaultBooleanValue = true,
        Category = "CustomSetting",
        Key = AttributeKey.ShowSort )]

    [IntegerField( "Number Of Results",
        Description = "The number of results to include.",
        DefaultIntegerValue = 10,
        IsRequired = true,
        Category = "CustomSetting",
        Key = AttributeKey.NumberOfResults )]

    [BooleanField( "Search On Load",
        Description = "Determines if search results will be displayed when the block loads.",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Key = AttributeKey.SearchOnLoad )]

    [BooleanField( "Group Results By Source",
        Description = "This will group the results by collection source.",
        DefaultBooleanValue = false,
        Category = "CustomSetting",
        Key = AttributeKey.GroupResultsBySource )]

    [TextField( "Enabled Sort Orders",
        Description = "The sort order options to be made available to the individual.",
        DefaultValue = SortOrdersKey.Newest + "," + SortOrdersKey.Oldest + "," + SortOrdersKey.Relevance + "," + SortOrdersKey.Trending,
        Category = "CustomSetting",
        Key = AttributeKey.EnabledSortOrders )]

    [TextField( "Trending Term",
        Description = "The term to use for the trending sort option.",
        DefaultValue = "Trending",
        Category = "CustomSetting",
        Key = AttributeKey.TrendingTerm )]

    [TextField( "Filters",
        Description = "The configured filter settings for this block instance.",
        DefaultValue = "",
        Category = "CustomSetting",
        Key = AttributeKey.Filters )]

    [TextField( "Results Template",
        Description = "The lava template to use to render the results container. It must contain an element with the class 'result-items'.",
        DefaultValue = @"<div class=""panel panel-default"">
    {% if SourceEntity and SourceEntity != empty %}
        <div class=""panel-heading"">
            <h2 class=""panel-title"">
                <i class=""{{ SourceEntity.IconCssClass }}""></i> {{ SourceName }}
            </h2>
        </div>
    {% endif %}
    <div class=""list-group"">
        <div class=""result-items""></div>
    </div>
</div>
<div class=""actions"">
   <a href=""#"" class=""btn btn-default js-more"">Show More</a>
</div>",
        Category = "CustomSetting",
        Key = AttributeKey.ResultsTemplate )]

    [TextField( "Group Header Template",
        Description = "The lava template to use to render the group headers. This will display above each content collection source.",
        DefaultValue = DefaultMobileGroupHeaderTemplate,
        Category = "CustomSetting",
        Key = AttributeKey.GroupHeaderTemplate )]

    [TextField( "Item Template",
        Description = "The lava template to use to render a single result.",
        DefaultValue = DefaultTemplateMarker,
        Category = "CustomSetting",
        Key = AttributeKey.ItemTemplate )]

    [TextField( "Pre-Search Template",
        Description = "The lava template to use to render the content displayed before a search happens. This will not be used if Search on Load is enabled.",
        DefaultValue = DefaultTemplateMarker,
        Category = "CustomSetting",
        Key = AttributeKey.PreSearchTemplate )]

    [BooleanField( "Boost Matching Segments",
        Description = "Determines if matching personalization segments should receive an additional boost.",
        DefaultBooleanValue = true,
        Category = "CustomSetting",
        Key = AttributeKey.BoostMatchingSegments )]

    [BooleanField( "Boost Matching Request Filters",
        Description = "Determines if matching personalization request filters should receive an additional boost.",
        DefaultBooleanValue = true,
        Category = "CustomSetting",
        Key = AttributeKey.BoostMatchingRequestFilters )]

    [DecimalField( "Segment Boost Amount",
        Description = "The amount of boost to apply to matches on personalization segments.",
        DefaultValue = null,
        Category = "CustomSetting",
        Key = AttributeKey.SegmentBoostAmount )]

    [DecimalField( "Request Filter Boost Amount",
        Description = "The amount of boost to apply to matches on personalization request filters.",
        DefaultValue = null,
        Category = "CustomSetting",
        Key = AttributeKey.SegmentBoostAmount )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "16C3A9D7-DD61-4971-8FE0-EEE09AEF703F" )]
    [Rock.SystemGuid.BlockTypeGuid( "CC387575-3530-4CD6-97E0-1F449DCA1869" )]
    public class ContentCollectionView : RockBlockType, IHasCustomActions
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ContentCollection = "ContentCollection";

            public const string ShowFiltersPanel = "ShowFiltersPanel";

            public const string ShowFullTextSearch = "ShowFullTextSearch";

            public const string ShowSort = "ShowSort";

            public const string NumberOfResults = "NumberOfResults";

            public const string SearchOnLoad = "SearchOnLoad";

            public const string GroupResultsBySource = "GroupResultsBySource";

            public const string EnabledSortOrders = "EnabledSortOrders";

            public const string TrendingTerm = "TrendingTerm";

            public const string Filters = "Filters";

            public const string ResultsTemplate = "ResultsTemplate";

            public const string GroupHeaderTemplate = "GroupHeaderTemplate";

            public const string ItemTemplate = "ItemTemplate";

            public const string PreSearchTemplate = "PreSearchTemplate";

            public const string BoostMatchingSegments = "BoostMatchingSegments";

            public const string BoostMatchingRequestFilters = "BoostMatchingRequestFilters";

            public const string SegmentBoostAmount = "SegmentBoostAmount";

            public const string RequestFilterBoostAmount = "RequestFilterBoostAmount";
        }

        private static class SortOrdersKey
        {
            public const string Relevance = "relevance";

            public const string Newest = "newest";

            public const string Oldest = "oldest";

            public const string Trending = "trending";

            public const string Alphabetical = "alphabetical";
        }

        #endregion Keys

        #region Constants

        /// <summary>
        /// The marker that we use internally to swap out the default template.
        /// </summary>
        private const string DefaultTemplateMarker = "## INTERNAL DEFAULT TEMPLATE MARKER";

        /// <summary>
        /// The default template for the web item.
        /// </summary>
        private readonly string DefaultWebItemTemplate = @"<div class=""result-item"">
    <h4 class=""mt-0"">{{ Item.Name }}</h4>
    <div class=""mb-3"">
    {{ Item.Content | StripHtml | Truncate:300 }}
    </div>
    <a href=""#"" class=""stretched-link"">Read More</a>
</div>";

        /// <summary>
        /// The default template for the mobile item.
        /// </summary>
        private readonly string DefaultMobileItemTemplate = @"<Grid RowDefinitions=""Auto, Auto, Auto""
      ColumnDefinitions=""*, Auto""
      StyleClass=""px-16, gap-col-12"">
    
    <Grid.Behaviors>
        <Rock:AddCssClassWhenTrueBehavior Value=""{Binding IsLastItem}""
                                          ClassName=""mb-16"" />
    </Grid.Behaviors>
    
    <Label Text=""{{ Item.Name | Escape }}""
           StyleClass=""body, bold, text-interface-stronger, mt-16""
           MaxLines=""2""
           LineBreakMode=""TailTruncation""
           Grid.Row=""0""
           Grid.Column=""0"" />
           
    <Label Text=""{{ Item.Content | StripHtml | Trim | Escape }}""
           StyleClass=""footnote, text-interface-strong""
           MaxLines=""2""
           LineBreakMode=""TailTruncation""
           Grid.Row=""1"" 
           Grid.Column=""0"" />
    
    <Rock:Icon IconClass=""chevron-right""
               StyleClass=""text-interface-medium""
               Grid.Row=""0"" 
               Grid.RowSpan=""3""
               Grid.Column=""1""
               VerticalOptions=""Center"" />
               
    <BoxView HeightRequest=""1""
             StyleClass=""mt-16, text-interface-soft""
             IsVisible=""{Binding IsLastItem, Converter={Rock:InverseBooleanConverter}}""
             Grid.Row=""2"" 
             Grid.Column=""0"" 
             Grid.ColumnSpan=""2"" />
</Grid>";

        /// <summary>
        /// The default template for the pre-search content.
        /// </summary>
        private readonly string DefaultWebPreSearchTemplate = @"<div class=""panel panel-default"">
    <div class=""panel-body"">Discover content that matches your preferences.</div>
</div>";

        /// <summary>
        /// The default template for a mobile group header.
        /// </summary>
        private const string DefaultMobileGroupHeaderTemplate = @"<Grid x:Name=""sourcesLayout"" 
      RowDefinitions=""*"">
       <FlexLayout BindableLayout.ItemsSource=""{Binding Sources}""
                   Wrap=""Wrap""
                   JustifyContent=""Start""
                   AlignItems=""Start"">
              <BindableLayout.ItemTemplate>
                     <DataTemplate>
                            <Border HeightRequest=""36""
                                    StrokeShape=""RoundRectangle 22""
                                    StyleClass=""px-16, py-4""
                                    MarginRight=""4""
                                    MarginBottom=""4"">
                                
                                <!-- The Content Collection Source Name -->
                                <Label Text=""{Binding Name}""
                                       StyleClass=""body""
                                       VerticalOptions=""Center"">
                                    <Label.Behaviors>
                                        <!-- Ensure that text-interface-softest applies to the selected tag. -->
                                        <Rock:AddCssClassWhenTrueBehavior ClassName=""text-interface-softest""
                                                                      FalseClassName=""text-interface-strong""
                                                                      Value=""{Binding IsSelected}"" />
                                    </Label.Behaviors>
                                </Label>
                                
                                <Border.Behaviors>
                                   <!-- Ensure that bg-primary-strong applies to the selected tag. -->
                                    <Rock:AddCssClassWhenTrueBehavior ClassName=""bg-primary-strong""
                                                                      FalseClassName=""bg-interface-soft""
                                                                      Value=""{Binding IsSelected}"" />
                                </Border.Behaviors>
                                        
                                <Border.GestureRecognizers>
                                    <TapGestureRecognizer Command=""{Binding BindingContext.UpdateSelectedSource, Source={x:Reference Name=sourcesLayout}}""
                                                          CommandParameter=""{Binding Guid}"" />
                                </Border.GestureRecognizers>
                            </Border>
                     </DataTemplate>
              </BindableLayout.ItemTemplate>
       </FlexLayout>
</Grid>";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var contentCollection = ContentCollectionCache.Get( GetAttributeValue( AttributeKey.ContentCollection ).AsGuid() );

                if ( contentCollection == null )
                {
                    return new ContentCollectionInitializationBox
                    {
                        ErrorMessage = "Block is not properly configured."
                    };
                }

                var searchOnLoad = GetAttributeValue( AttributeKey.SearchOnLoad ).AsBoolean();
                var preSearchTemplate = GetPreSearchTemplate();
                var mergeFields = RequestContext.GetCommonMergeFields();
                var preSearchContent = preSearchTemplate.ResolveMergeFields( mergeFields );
                SearchResultBag initialSearchResults = null;

                if ( searchOnLoad || HasQueryStringFilters() )
                {
                    try
                    {
                        var searchTask = Task.Run( PerformInitialSearchAsync );
                        searchTask.Wait();
                        initialSearchResults = searchTask.Result;
                    }
                    catch
                    {
                        initialSearchResults = null;
                    }
                }

                return new ContentCollectionInitializationBox
                {
                    ShowFullTextSearch = GetAttributeValue( AttributeKey.ShowFullTextSearch ).AsBoolean(),
                    ShowFiltersPanel = GetAttributeValue( AttributeKey.ShowFiltersPanel ).AsBoolean(),
                    ShowSort = GetAttributeValue( AttributeKey.ShowSort ).AsBoolean(),
                    EnabledSortOrders = GetAttributeValue( AttributeKey.EnabledSortOrders ).SplitDelimitedValues().ToList(),
                    TrendingTerm = GetAttributeValue( AttributeKey.TrendingTerm ),
                    Filters = GetSearchFilters( contentCollection ),
                    PreSearchContent = preSearchContent,
                    SearchOnLoad = searchOnLoad,
                    InitialResults = initialSearchResults
                };
            }
        }

        /// <summary>
        /// Gets the search filters that will be used by the specified content collection.
        /// </summary>
        /// <param name="contentCollection">The content collection whose search filters need to be rendered.</param>
        /// <returns>A list of <see cref="SearchFilterBag"/> objects that describe the search filters configured on the content collection.</returns>
        private List<SearchFilterBag> GetSearchFilters( ContentCollectionCache contentCollection )
        {
            var filters = new List<SearchFilterBag>();
            var attributeValues = contentCollection.FilterSettings.AttributeValues ?? new Dictionary<string, List<ListItemBag>>();
            var fieldValues = contentCollection.FilterSettings.FieldValues ?? new Dictionary<string, List<ListItemBag>>();

            // Get the filters configured on the block custom settings.
            var filterOptions = GetAttributeValue( AttributeKey.Filters ).FromJsonOrNull<List<FilterOptionsBag>>() ?? new List<FilterOptionsBag>();

            foreach ( var filterOption in filterOptions )
            {
                // If the filter is not set to show or not valid then skip it.
                if ( !filterOption.Show || filterOption.SourceKey.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                // Check if this is the special "Year" filter.
                if ( filterOption.SourceKey == "Year" )
                {
                    if ( !contentCollection.FilterSettings.YearSearchEnabled )
                    {
                        continue;
                    }

                    filters.Add( new SearchFilterBag
                    {
                        Control = contentCollection.FilterSettings.YearSearchFilterControl,
                        Label = contentCollection.FilterSettings.YearSearchLabel,
                        IsMultipleSelection = contentCollection.FilterSettings.YearSearchFilterIsMultipleSelection,
                        HeaderMarkup = filterOption.HeaderMarkup,
                        Items = GetYearItemBags( contentCollection )
                    } );

                    continue;
                }

                // Check if this is an attribute search filter.
                if ( filterOption.SourceKey.StartsWith( "attr_" ) )
                {
                    var attrKey = filterOption.SourceKey.Substring( 5 );

                    if ( !contentCollection.FilterSettings.AttributeFilters.TryGetValue( attrKey, out var filterSettings ) )
                    {
                        continue;
                    }

                    filters.Add( new SearchFilterBag
                    {
                        Control = filterSettings.FilterControl,
                        Label = filterSettings.Label,
                        IsMultipleSelection = filterSettings.IsMultipleSelection,
                        HeaderMarkup = filterOption.HeaderMarkup,
                        Items = attributeValues.ContainsKey( attrKey ) ? attributeValues[attrKey] : new List<ListItemBag>()
                    } );
                }

                if ( filterOption.SourceKey.StartsWith( "cust_" ) )
                {
                    var key = filterOption.SourceKey.Substring( 5 );

                    if ( !contentCollection.FilterSettings.CustomFieldFilters.TryGetValue( key, out var filterSettings ) )
                    {
                        continue;
                    }

                    filters.Add( new SearchFilterBag
                    {
                        Control = filterSettings.FilterControl,
                        Label = filterSettings.Label,
                        IsMultipleSelection = filterSettings.IsMultipleSelection,
                        HeaderMarkup = filterOption.HeaderMarkup,
                        Items = fieldValues.ContainsKey( key ) ? fieldValues[key] : new List<ListItemBag>()
                    } );
                }
            }

            return filters;
        }

        /// <summary>
        /// Get the list item bags that will represent the available options in
        /// the Year search filter.
        /// </summary>
        /// <param name="contentCollection">The content collection whose available year values should be returned.</param>
        /// <returns>A list of <see cref="ListItemBag"/> objects.</returns>
        private static List<ListItemBag> GetYearItemBags( ContentCollectionCache contentCollection )
        {
            var now = RockDateTime.Now;
            var items = new List<ListItemBag>();
            var filterValues = contentCollection.FilterSettings.FieldValues ?? new Dictionary<string, List<ListItemBag>>();

            if ( !filterValues.TryGetValue( nameof( IndexDocumentBase.Year ), out var yearValues ) )
            {
                return items;
            }

            var yearNumericValues = yearValues
                .Select( y => y.Value.AsIntegerOrNull() )
                .Where( y => y.HasValue )
                .Select( y => y.Value )
                .ToList();

            if ( !yearNumericValues.Any() )
            {
                return items;
            }

            var startAtYear = yearNumericValues.Min();

            // Include options from the starting year up to and including this year.
            for ( int year = startAtYear; year <= now.Year; year++ )
            {
                items.Add( new ListItemBag
                {
                    Value = year.ToString(),
                    Text = year.ToString()
                } );
            }

            return items;
        }

        /// <summary>
        /// Gets the content collections the current person is authorized to see.
        /// </summary>
        /// <returns>A list of content collection cache instances.</returns>
        private List<ContentCollectionCache> GetContentCollections()
        {
            return ContentCollectionCache.All()
                .Where( cl => cl.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson ) )
                .ToList();
        }

        /// <summary>
        /// Gets the collection bag items from the list of content collections. These
        /// will be sent to the client for use during the custom settings modal.
        /// </summary>
        /// <param name="collections">The list of content collections to be represented by bags.</param>
        /// <returns>The bags that represent the content collections.</returns>
        private List<ContentCollectionListItemBag> GetContentCollectionBagItems( List<ContentCollectionCache> collections )
        {
            return collections
                .Select( l => new ContentCollectionListItemBag
                {
                    Value = l.Guid.ToString(),
                    Text = l.Name,
                    Filters = GetActiveCollectionFilterNames( l )
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the collection filter names that are configured and active for
        /// the given content collection.
        /// </summary>
        /// <param name="collection">The content collection whose filter names are being requested.</param>
        /// <returns>A list of filter names.</returns>
        private List<ListItemBag> GetActiveCollectionFilterNames( ContentCollectionCache collection )
        {
            var filterSettings = collection.FilterSettings;

            if ( filterSettings == null )
            {
                return new List<ListItemBag>();
            }

            var names = new List<ListItemBag>();

            if ( filterSettings.YearSearchEnabled )
            {
                names.Add( new ListItemBag
                {
                    Value = "Year",
                    Text = filterSettings.YearSearchLabel
                } );
            }

            if ( filterSettings.AttributeFilters != null )
            {
                foreach ( var kvp in filterSettings.AttributeFilters )
                {
                    if ( kvp.Value.IsEnabled )
                    {
                        names.Add( new ListItemBag
                        {
                            Value = $"attr_{kvp.Key}",
                            Text = kvp.Value.Label
                        } );
                    }
                }
            }

            if ( filterSettings.CustomFieldFilters != null )
            {
                foreach ( var kvp in filterSettings.CustomFieldFilters )
                {
                    if ( kvp.Value.IsEnabled )
                    {
                        names.Add( new ListItemBag
                        {
                            Value = $"cust_{kvp.Key}",
                            Text = kvp.Value.Label
                        } );
                    }
                }
            }

            return names;
        }

        /// <summary>
        /// Gets all the attribute keys that are valid for use in a search query.
        /// The resulting keys are all lower cased.
        /// </summary>
        /// <param name="contentCollection">The content collection whose filters keys are being requested.</param>
        /// <returns>A list of attribute keys in lowercase form.</returns>
        private List<string> GetActiveCollectionFilterKeys( ContentCollectionCache contentCollection )
        {
            return GetActiveCollectionFilterNames( contentCollection )
                .Where( f => f.Value.StartsWith( "attr_" ) || f.Value.StartsWith( "cust_" ) )
                .Select( f => f.Text.ToLower() )
                .ToList();
        }

        /// <summary>
        /// Checks if the query string has any filter parameters specified
        /// that can be used in a search.
        /// </summary>
        /// <returns><c>true</c> if the query string contains search filters; otherwise <c>false</c>.</returns>
        private bool HasQueryStringFilters()
        {
            var contentCollection = ContentCollectionCache.Get( GetAttributeValue( AttributeKey.ContentCollection ).AsGuid() );

            if ( contentCollection == null )
            {
                return false;
            }

            if ( RequestContext.GetPageParameter( "q" ).IsNotNullOrWhiteSpace() )
            {
                return true;
            }

            var validKeys = GetActiveCollectionFilterKeys( contentCollection );

            foreach ( var key in validKeys )
            {
                if ( RequestContext.GetPageParameter( key ).IsNotNullOrWhiteSpace() )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the pre-search template for the content collection view.
        /// </summary>
        /// <returns></returns>
        private string GetPreSearchTemplate()
        {
            var itemTemplate = GetAttributeValue( AttributeKey.PreSearchTemplate );

            var useMobileTemplate = PageCache.Layout?.Site?.SiteType == Model.SiteType.Mobile;

            //
            // We want to set the default value of the item template depending on
            // the site type. This is kind of a cheesy wasy to do it, considering this
            // block is unique in the aspect that all of the settings are "custom".
            // This is a temporary solution until we can figure out a better way to
            // set default values depending on the site type.
            //
            if ( itemTemplate == DefaultTemplateMarker )
            {
                if ( useMobileTemplate )
                {
                    return string.Empty;
                }
                else
                {
                    return DefaultWebPreSearchTemplate;
                }
            }

            return itemTemplate;
        }

        /// <summary>
        /// Gets the item template for the content collection view.
        /// </summary>
        /// <returns></returns>
        private string GetItemTemplate()
        {
            var itemTemplate = GetAttributeValue( AttributeKey.ItemTemplate );

            var useMobileTemplate = PageCache.Layout?.Site?.SiteType == Model.SiteType.Mobile;

            //
            // We want to set the default value of the item template depending on
            // the site type. This is kind of a cheesy wasy to do it, considering this
            // block is unique in the aspect that all of the settings are "custom".
            // This is a temporary solution until we can figure out a better way to
            // set default values depending on the site type.
            //
            if ( itemTemplate == DefaultTemplateMarker )
            {
                if ( useMobileTemplate )
                {
                    return DefaultMobileItemTemplate;
                }
                else
                {
                    return DefaultWebItemTemplate;
                }
            }

            return itemTemplate;
        }

        /// <summary>
        /// Performs the initial page-load search request. This uses the query
        /// string parameters to build the search query. It will return null
        /// if something prevented the results from being retrieved.
        /// </summary>
        /// <remarks>This has a built-in timeout of 2.5 seconds to prevent the page load from being delayed due to an index engine issue.</remarks>
        /// <returns>The search results.</returns>
        private async Task<SearchResultBag> PerformInitialSearchAsync()
        {
            var contentCollection = ContentCollectionCache.Get( GetAttributeValue( AttributeKey.ContentCollection ).AsGuid() );

            if ( contentCollection == null )
            {
                return null;
            }

            // Construct a search query bag with all the information from the
            // query string.
            var query = new SearchQueryBag
            {
                Text = RequestContext.GetPageParameter( "q" ),
                Order = RequestContext.GetPageParameter( "s" )?.ConvertToEnumOrNull<SearchOrder>() ?? SearchOrder.Relevance,
                Filters = new Dictionary<string, string>()
            };

            // Add in all the query string filters.
            var validKeys = GetActiveCollectionFilterKeys( contentCollection );

            foreach ( var key in validKeys )
            {
                var filterValue = RequestContext.GetPageParameter( key );

                if ( filterValue.IsNotNullOrWhiteSpace() )
                {
                    query.Filters.AddOrReplace( key.ToLower(), filterValue );
                }
            }

            var searchTask = PerformSearchAsync( query, contentCollection );

            // Wait at most 2.5 seconds for the search to complete. We don't want
            // to hold up the page loading for longer than that.
            if ( await Task.WhenAny( searchTask, Task.Delay( 2500 ) ) == searchTask )
            {
                return await searchTask;
            }

            return null;
        }

        /// <summary>
        /// Performs the search specified in the query against the content collection.
        /// </summary>
        /// <param name="query">The query that represents which documents we are interested in.</param>
        /// <param name="contentCollection">The content collection that will be searched for results.</param>
        /// <returns>The search results.</returns>
        private async Task<SearchResultBag> PerformSearchAsync( SearchQueryBag query, ContentCollectionCache contentCollection )
        {
            // Get the search query to be run.
            var searchQuery = GetSearchQueryFromBag( query, contentCollection );

            if ( searchQuery == null )
            {
                return null;
            }

            var order = query.Order ?? SearchOrder.Relevance;
            var maxResults = GetAttributeValue( AttributeKey.NumberOfResults ).AsInteger();
            var groupResults = GetAttributeValue( AttributeKey.GroupResultsBySource ).AsBoolean();

            var resultBag = new SearchResultBag
            {
                ResultSources = new List<SearchResultSourceBag>()
            };

            if ( !groupResults )
            {
                AppendAllSourcesToSearchQuery( searchQuery, contentCollection );

                resultBag.ResultSources.Add( await GetSearchResultsForSourceAsync( searchQuery, null, query.Offset ?? 0, maxResults, order ) );
            }
            else if ( query.SourceGuid.HasValue && query.SourceGuid.Value != Guid.Empty )
            {
                var source = ContentCollectionSourceCache.Get( query.SourceGuid.Value );

                if ( source == null || source.ContentCollectionId != contentCollection.Id )
                {
                    return null;
                }

                resultBag.ResultSources.Add( await GetSearchResultsForSourceAsync( searchQuery, source, query.Offset ?? 0, maxResults, order ) );
            }
            else
            {
                var sources = ContentCollectionSourceCache.All()
                    .Where( s => s.ContentCollectionId == contentCollection.Id )
                    .ToList();

                foreach ( var source in sources )
                {
                    resultBag.ResultSources.Add( await GetSearchResultsForSourceAsync( searchQuery, source, query.Offset ?? 0, maxResults, order ) );
                }
            }

            resultBag.TotalResultCount = resultBag.ResultSources.Sum( rs => rs.TotalResultCount );

            return resultBag;
        }

        /// <summary>
        /// Gets the search query from bag. This does not include the source,
        /// only the search text and filters.
        /// </summary>
        /// <param name="query">The query bag to use when constructing the real query.</param>
        /// <param name="contentCollection">The content collection this query will be for.</param>
        /// <returns>A new <see cref="SearchQuery"/> instance that represents the search text and filters.</returns>
        private SearchQuery GetSearchQueryFromBag( SearchQueryBag query, ContentCollectionCache contentCollection )
        {
            var searchQuery = new SearchQuery
            {
                IsAllMatching = true
            };

            // Only include the full text search if it was enabled in the block
            // settings and they provided a value.
            if ( GetAttributeValue( AttributeKey.ShowFullTextSearch ).AsBoolean() && query.Text.IsNotNullOrWhiteSpace() )
            {
                searchQuery.Add( new SearchTerm
                {
                    Text = query.Text
                } );
            }

            // Append the filters only if we are showing the filters panel.
            if ( GetAttributeValue( AttributeKey.ShowFiltersPanel ).AsBoolean() )
            {
                AppendFiltersToSearchQuery( query.Filters, searchQuery, contentCollection );
            }

            AppendPersonalizationToSearchQuery( searchQuery, contentCollection );
            AppendTrendingToSearchQuery( searchQuery, contentCollection );

            return searchQuery;
        }

        /// <summary>
        /// Appends the filter required to limit results to all sources of the
        /// content collection.
        /// </summary>
        /// <param name="searchQuery">The search query to be updated.</param>
        /// <param name="contentCollection">The content collection to filter results to.</param>
        private static void AppendAllSourcesToSearchQuery( SearchQuery searchQuery, ContentCollectionCache contentCollection )
        {
            var sources = ContentCollectionSourceCache.All()
                .Where( s => s.ContentCollectionId == contentCollection.Id )
                .ToList();

            // Build a criteria search to find all documents of all sources that
            // belong to this collection.
            var sourceQuery = new SearchQuery
            {
                IsAllMatching = false
            };

            foreach ( var source in sources )
            {
                sourceQuery.Add( new SearchField
                {
                    Name = nameof( IndexDocumentBase.SourceId ),
                    Value = source.Id.ToString()
                } );
            }

            searchQuery.Add( sourceQuery );
        }

        /// <summary>
        /// Appends the personalization boosting values to the search query.
        /// </summary>
        /// <param name="searchQuery">The search query to be updated.</param>
        /// <param name="contentCollection">The content collection that describes the configuration.</param>
        private void AppendPersonalizationToSearchQuery( SearchQuery searchQuery, ContentCollectionCache contentCollection )
        {
            // If nothing to do, don't modify the query.
            if ( !contentCollection.EnableSegments && !contentCollection.EnableRequestFilters )
            {
                return;
            }

            var personalizationQuery = new SearchQuery
            {
                IsAllMatching = false
            };

            // Add a rule that always matches so that if none of the personalization
            // rules match we still get results.
            personalizationQuery.Add( new SearchAnyMatch() );
            var hasPersonalization = false;

            // Add all the personalization segments this person matches.
            if ( contentCollection.EnableSegments )
            {
                var segmentIds = RequestContext.PersonalizationSegmentIds;

                foreach ( var segmentId in segmentIds )
                {
                    personalizationQuery.Add( new SearchField
                    {
                        Name = nameof( IndexDocumentBase.Segments ),
                        Value = segmentId.ToString(),
                        Boost = GetAttributeValue( AttributeKey.SegmentBoostAmount ).AsDoubleOrNull() ?? 1.0d
                    } );

                    hasPersonalization = true;
                }
            }

            // Add all the request filters this request matches.
            if ( contentCollection.EnableRequestFilters )
            {
                var filterIds = RequestContext.PersonalizationRequestFilterIds;

                foreach ( var filterId in filterIds )
                {
                    personalizationQuery.Add( new SearchField
                    {
                        Name = nameof( IndexDocumentBase.RequestFilters ),
                        Value = filterId.ToString(),
                        Boost = GetAttributeValue( AttributeKey.RequestFilterBoostAmount ).AsDoubleOrNull() ?? 1.0d
                    } );

                    hasPersonalization = true;
                }
            }

            if ( hasPersonalization )
            {
                searchQuery.Add( personalizationQuery );
            }
        }

        /// <summary>
        /// Appends the trending boost values to the search query.
        /// </summary>
        /// <param name="searchQuery">The search query to be updated.</param>
        /// <param name="contentCollection">The content collection that describes the configuration.</param>
        private void AppendTrendingToSearchQuery( SearchQuery searchQuery, ContentCollectionCache contentCollection )
        {
            // If there is nothing to do, then don't modify the query.
            if ( !contentCollection.TrendingEnabled )
            {
                return;
            }

            var trendingQuery = new SearchQuery
            {
                IsAllMatching = false
            };

            // Add a rule that always matches so that if the trending boost
            // doesn't match we still get results.
            trendingQuery.Add( new SearchAnyMatch() );

            // This will apply a small boost to trending items, later this should
            // probably be made a configurable boost value.
            trendingQuery.Add( new SearchField
            {
                Name = nameof( IndexDocumentBase.IsTrending ),
                Value = "true"
            } );

            searchQuery.Add( trendingQuery );
        }

        /// <summary>
        /// Appends the filters to the search query.
        /// </summary>
        /// <param name="filters">The filters values to use when updating the query.</param>
        /// <param name="searchQuery">The search query to be updated.</param>
        /// <param name="contentCollection">The content collection this query will be run against.</param>
        private void AppendFiltersToSearchQuery( Dictionary<string, string> filters, SearchQuery searchQuery, ContentCollectionCache contentCollection )
        {
            if ( filters == null )
            {
                return;
            }

            // Get all the keys that are valid to be used for filtering.
            var validKeys = GetActiveCollectionFilterKeys( contentCollection );

            foreach ( var filterPair in filters )
            {
                // If no value was specified, do not filter.
                if ( filterPair.Value.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                var filterKey = filterPair.Key.ToLower();

                // If this is the Year filter, special processing is done.
                if ( contentCollection.FilterSettings.YearSearchEnabled && contentCollection.FilterSettings.YearSearchLabel.ToLower() == filterKey )
                {
                    var years = filterPair.Value.Split( ',' );

                    if ( years.Length > 0 )
                    {
                        var yearCriteria = new SearchQuery
                        {
                            IsAllMatching = false
                        };

                        foreach ( var year in years )
                        {
                            yearCriteria.Add( new SearchField
                            {
                                Name = nameof( IndexDocumentBase.Year ),
                                Value = year
                            } );
                        }

                        searchQuery.Add( yearCriteria );
                    }

                    continue;
                }

                if ( !validKeys.Contains( filterKey ) )
                {
                    continue;
                }

                // Find the attribute filter for this query filter.
                var attributeFilterKey = contentCollection.FilterSettings?.AttributeFilters
                    .Where( kvp => kvp.Value.Label.ToLower() == filterKey )
                    .Select( kvp => kvp.Key )
                    .FirstOrDefault();

                // See if this was an attribute filter.
                if ( attributeFilterKey != null )
                {
                    // Apply the search for the attribute filter.
                    var values = filterPair.Value.Replace( ".", "" ).Split( ',' );

                    if ( values.Length > 0 )
                    {
                        var valueQuery = new SearchQuery
                        {
                            IsAllMatching = false
                        };

                        foreach ( var value in values )
                        {
                            valueQuery.Add( new SearchField
                            {
                                Name = $"{attributeFilterKey}ValueRaw",
                                Value = value,
                                IsPhrase = true
                            } );
                        }

                        searchQuery.Add( valueQuery );
                    }
                }

                var customFieldFilterKey = contentCollection.FilterSettings?.CustomFieldFilters
                    .Where( kvp => kvp.Value.Label.ToLower() == filterKey )
                    .Select( kvp => kvp.Key )
                    .FirstOrDefault();

                // See if this was a custom field filter.
                if ( customFieldFilterKey != null )
                {
                    // Apply the search for the custom field filter.
                    var values = filterPair.Value.Replace( ".", "" ).Split( ',' );

                    if ( values.Length > 0 )
                    {
                        var valueQuery = new SearchQuery
                        {
                            IsAllMatching = false
                        };

                        foreach ( var value in values )
                        {
                            valueQuery.Add( new SearchField
                            {
                                Name = $"{customFieldFilterKey}ValueRaw",
                                Value = value,
                                IsPhrase = true
                            } );
                        }

                        searchQuery.Add( valueQuery );
                    }
                }
            }
        }

        /// <summary>
        /// Get search results for a single content collection source.
        /// </summary>
        /// <param name="searchQuery">The base search query to use when searching.</param>
        /// <param name="source">The source to load results for or <c>null</c> if all results from the collection should be loaded.</param>
        /// <param name="offset">The starting offset in the search results to return.</param>
        /// <param name="maxResults">The maximum number of results to return.</param>
        /// <param name="order">The order in which results will be returned.</param>
        /// <returns>A new <see cref="SearchResultSourceBag"/> that contains the results from this query.</returns>
        private async Task<SearchResultSourceBag> GetSearchResultsForSourceAsync( SearchQuery searchQuery, ContentCollectionSourceCache source, int offset, int maxResults, SearchOrder order )
        {
            // Clone the search query so we don't corrupt it.
            searchQuery = searchQuery.Clone();

            if ( source != null )
            {
                // Append the source filter to the query.
                searchQuery.Add( new SearchField
                {
                    Name = nameof( IndexDocumentBase.SourceId ),
                    Value = source.Id.ToString()
                } );
            }

            var searchOptions = new SearchOptions
            {
                Offset = offset,
                MaxResults = maxResults
            };

            // Define the search order to return results in.
            switch ( order )
            {
                case SearchOrder.Newest:
                case SearchOrder.Oldest:
                    searchOptions.Order = SearchSortOrder.RelevantDate;
                    searchOptions.IsDescending = order == SearchOrder.Newest;
                    break;

                case SearchOrder.Alphabetical:
                    searchOptions.Order = SearchSortOrder.Alphabetical;
                    searchOptions.IsDescending = order == SearchOrder.Newest;
                    break;

                case SearchOrder.Trending:
                    searchOptions.Order = SearchSortOrder.Trending;
                    searchOptions.IsDescending = true;
                    break;

                default:
                    break;
            }

            // Start preparing the result.
            var resultBag = new SearchResultSourceBag
            {
                SourceGuid = source?.Guid ?? Guid.Empty,
                Results = new List<string>()
            };

            // Always include the source result template just in case something
            // weird happens and they start loading items past offset 0 first.

            string resultsTemplate;

            if ( PageCache.Layout?.Site?.SiteType == Model.SiteType.Mobile )
            {
                resultsTemplate = GetAttributeValue( AttributeKey.GroupHeaderTemplate );
            }
            else
            {
                resultsTemplate = GetAttributeValue( AttributeKey.ResultsTemplate );
            }

            var mergeFields = RequestContext.GetCommonMergeFields();

            if ( source != null )
            {
                var sourceEntityType = source.EntityType.GetEntityType();
                var sourceEntity = Reflection.GetIEntityForEntityType( sourceEntityType, source.EntityId );
                mergeFields.Add( "SourceType", sourceEntityType.Name );
                mergeFields.Add( "SourceName", sourceEntity.ToString() );
                mergeFields.Add( "SourceEntity", sourceEntity );
            }

            resultBag.Template = resultsTemplate.ResolveMergeFields( mergeFields );

            // Run the search.
            var activeComponent = ContentIndexContainer.GetActiveComponent();
            if ( activeComponent != null )
            {
                var results = await activeComponent.SearchAsync( searchQuery, searchOptions );

                resultBag.HasMore = results.TotalResultsAvailable > ( maxResults + offset );
                resultBag.TotalResultCount = results.TotalResultsAvailable;

                // Merge the results with the Lava template.
                var itemTemplate = GetItemTemplate();

                foreach ( var result in results.Documents )
                {
                    mergeFields.AddOrReplace( "Item", result );

                    resultBag.Results.Add( itemTemplate.ResolveMergeFields( mergeFields ) );
                }
            }

            return resultBag;
        }

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            using ( var rockContext = new RockContext() )
            {
                return GetSecurityGrantToken();
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken()
        {
            return new Rock.Security.SecurityGrant()
                .ToToken();
        }

        #endregion

        #region IHasCustomAdministrateActions

        /// <inheritdoc/>
        List<BlockCustomActionBag> IHasCustomActions.GetCustomActions( bool canEdit, bool canAdministrate )
        {
            var actions = new List<BlockCustomActionBag>();

            if ( canAdministrate )
            {
                actions.Add( new BlockCustomActionBag
                {
                    IconCssClass = "fa fa-edit",
                    Tooltip = "Settings",
                    ComponentFileUrl = "/Obsidian/Blocks/CMS/contentCollectionViewCustomSettings.obs"
                } );
            }

            return actions;
        }

        #endregion

        #region Mobile

        /// <summary>
        /// Gets the configuration values for the mobile shell.
        /// </summary>
        /// <returns></returns>
        public override object GetMobileConfigurationValues()
        {
            var contentCollectionGuid = GetAttributeValue( AttributeKey.ContentCollection ).AsGuidOrNull();

            if ( contentCollectionGuid == null )
            {
                return null;
            }

            var contentCollection = ContentCollectionCache.Get( contentCollectionGuid.Value );
            if ( contentCollection == null )
            {
                return null;
            }

            var groupResultsBySource = GetAttributeValue( AttributeKey.GroupResultsBySource ).AsBoolean();
            var mobileSources = new List<MobileContentSource>();

            if ( groupResultsBySource )
            {
                var sources = ContentCollectionSourceCache.All()
                    .Where( s => s.ContentCollectionId == contentCollection.Id )
                    .ToList();

                foreach( var source in sources )
                {
                    var sourceEntityType = source.EntityType.GetEntityType();
                    var sourceEntity = Reflection.GetIEntityForEntityType( sourceEntityType, source.EntityId );

                    if ( sourceEntity == null )
                    {
                        continue;
                    }

                    mobileSources.Add( new MobileContentSource
                    {
                        Name = sourceEntity.ToString(),
                        Guid = source.Guid
                    } );
                }
            }

            return new
            {
                ContentCollection = contentCollectionGuid,
                ShowFilters = GetAttributeValue( AttributeKey.ShowFiltersPanel ).AsBoolean(),
                ShowFullTextSearch = GetAttributeValue( AttributeKey.ShowFullTextSearch ).AsBoolean(),
                ShowSort = GetAttributeValue( AttributeKey.ShowSort ).AsBoolean(),
                NumberOfResults = GetAttributeValue( AttributeKey.NumberOfResults ).AsIntegerOrNull(),
                SearchOnLoad = GetAttributeValue( AttributeKey.SearchOnLoad ).AsBoolean(),
                GroupResultsBySource = GetAttributeValue( AttributeKey.GroupResultsBySource ).AsBoolean(),
                EnabledSortOrders = GetAttributeValue( AttributeKey.EnabledSortOrders ).SplitDelimitedValues().ToList(),
                TrendingTerm = GetAttributeValue( AttributeKey.TrendingTerm ),
                GroupHeaderTemplate = GetAttributeValue( AttributeKey.GroupHeaderTemplate ),
                ItemTemplate = GetItemTemplate(),
                PreSearchTemplate = GetPreSearchTemplate(),
                Filters = GetSearchFilters( contentCollection ),
                BoostMatchingSegments = GetAttributeValue( AttributeKey.BoostMatchingSegments ).AsBoolean(),
                BoostMatchingRequestFilters = GetAttributeValue( AttributeKey.BoostMatchingRequestFilters ).AsBoolean(),
                SegmentBoostAmount = GetAttributeValue( AttributeKey.SegmentBoostAmount ).AsDecimalOrNull(),
                RequestFilterBoostAmount = GetAttributeValue( AttributeKey.RequestFilterBoostAmount ).AsDecimalOrNull(),
                CollectionSources = mobileSources
            };
        }

        /// <summary>
        /// A class that represents a content source to be bundled
        /// into the mobile configuration.
        /// </summary>
        private class MobileContentSource
        {
            /// <summary>
            /// Gets or sets the name of the source.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the GUID of the source.
            /// </summary>
            public Guid Guid { get; set; }
        }

        #endregion

        #region Block Actions

        [BlockAction]
        public async Task<BlockActionResult> PerformInitialSearch()
        {
            var result = await PerformInitialSearchAsync();

            if ( result == null )
            {
                return ActionBadRequest( "Search request was not valid." );
            }

            return ActionOk( result );
        }

        /// <summary>
        /// Performs the search using the information specified in the query.
        /// </summary>
        /// <param name="query">The query that contains the search request.</param>
        /// <returns>The content for the search results.</returns>
        [BlockAction]
        public async Task<BlockActionResult> Search( SearchQueryBag query )
        {
            var contentCollection = ContentCollectionCache.Get( GetAttributeValue( AttributeKey.ContentCollection ).AsGuid() );

            if ( contentCollection == null )
            {
                return ActionInternalServerError( "Block is not properly configured." );
            }

            var resultBag = await PerformSearchAsync( query, contentCollection );

            if ( resultBag == null )
            {
                return ActionBadRequest( "Search request was not valid." );
            }

            return ActionOk( resultBag );
        }

        /// <summary>
        /// Gets the values and all other required details that will be needed
        /// to display the custom settings modal.
        /// </summary>
        /// <returns>A box that contains the custom settings values and additional data.</returns>
        [BlockAction]
        public BlockActionResult GetCustomSettings()
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden( "Not authorized to edit block settings." );
                }

                var options = new CustomSettingsOptionsBag
                {
                    ContentCollectionItems = GetContentCollectionBagItems( GetContentCollections() )
                };

                var filters = GetAttributeValue( AttributeKey.Filters ).FromJsonOrNull<List<FilterOptionsBag>>() ?? new List<FilterOptionsBag>();

                var settings = new CustomSettingsBag
                {
                    ContentCollection = GetAttributeValue( AttributeKey.ContentCollection ).AsGuidOrNull(),
                    ShowFiltersPanel = GetAttributeValue( AttributeKey.ShowFiltersPanel ).AsBoolean(),
                    ShowFullTextSearch = GetAttributeValue( AttributeKey.ShowFullTextSearch ).AsBoolean(),
                    ShowSort = GetAttributeValue( AttributeKey.ShowSort ).AsBoolean(),
                    NumberOfResults = GetAttributeValue( AttributeKey.NumberOfResults ).AsIntegerOrNull(),
                    SearchOnLoad = GetAttributeValue( AttributeKey.SearchOnLoad ).AsBoolean(),
                    GroupResultsBySource = GetAttributeValue( AttributeKey.GroupResultsBySource ).AsBoolean(),
                    EnabledSortOrders = GetAttributeValue( AttributeKey.EnabledSortOrders ).SplitDelimitedValues().ToList(),
                    TrendingTerm = GetAttributeValue( AttributeKey.TrendingTerm ),
                    Filters = filters,
                    ResultsTemplate = GetAttributeValue( AttributeKey.ResultsTemplate ),
                    ItemTemplate = GetItemTemplate(),
                    PreSearchTemplate = GetPreSearchTemplate(),
                    BoostMatchingSegments = GetAttributeValue( AttributeKey.BoostMatchingSegments ).AsBoolean(),
                    BoostMatchingRequestFilters = GetAttributeValue( AttributeKey.BoostMatchingRequestFilters ).AsBoolean(),
                    SegmentBoostAmount = GetAttributeValue( AttributeKey.SegmentBoostAmount ).AsDecimalOrNull(),
                    RequestFilterBoostAmount = GetAttributeValue( AttributeKey.RequestFilterBoostAmount ).AsDecimalOrNull(),
                    GroupHeaderTemplate = GetAttributeValue( AttributeKey.GroupHeaderTemplate ),
                    SiteType = ( PageCache?.Layout?.Site?.SiteType ?? Model.SiteType.Web ).ToString().ToLower()
                };

                return ActionOk( new CustomSettingsBox<CustomSettingsBag, CustomSettingsOptionsBag>
                {
                    Settings = settings,
                    Options = options,
                    SecurityGrantToken = GetSecurityGrantToken()
                } );
            }
        }

        /// <summary>
        /// Saves the updates to the custom setting values for this block.
        /// </summary>
        /// <param name="box">The box that contains the setting values.</param>
        /// <returns>A response that indicates if the save was successful or not.</returns>
        [BlockAction]
        public BlockActionResult SaveCustomSettings( CustomSettingsBox<CustomSettingsBag, CustomSettingsOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden( "Not authorized to edit block settings." );
                }

                var block = new BlockService( rockContext ).Get( BlockId );
                block.LoadAttributes( rockContext );

                // Setting the content collection requires additional validation checks.
                var errorActionResult = box.IfValidProperty( nameof( box.Settings.ContentCollection ), () =>
                {
                    if ( !box.Settings.ContentCollection.HasValue )
                    {
                        return ActionBadRequest( "Content Collection is required." );
                    }

                    var collectionCache = new ContentCollectionService( rockContext ).Get( box.Settings.ContentCollection.Value );

                    if ( collectionCache == null )
                    {
                        return ActionNotFound( "Content collection was not found." );
                    }
                    else if ( !collectionCache.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson ) )
                    {
                        return ActionForbidden( "You do not have access to that content collection." );
                    }

                    block.SetAttributeValue( AttributeKey.ContentCollection, box.Settings.ContentCollection.Value.ToString() );

                    return null;
                }, null );

                if ( errorActionResult != null )
                {
                    return errorActionResult;
                }

                box.IfValidProperty( nameof( box.Settings.ShowFiltersPanel ),
                    () => block.SetAttributeValue( AttributeKey.ShowFiltersPanel, box.Settings.ShowFiltersPanel.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.ShowFullTextSearch ),
                    () => block.SetAttributeValue( AttributeKey.ShowFullTextSearch, box.Settings.ShowFullTextSearch.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.ShowSort ),
                    () => block.SetAttributeValue( AttributeKey.ShowSort, box.Settings.ShowSort.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.NumberOfResults ),
                    () => block.SetAttributeValue( AttributeKey.NumberOfResults, box.Settings.NumberOfResults.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.SearchOnLoad ),
                    () => block.SetAttributeValue( AttributeKey.SearchOnLoad, box.Settings.SearchOnLoad.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.GroupResultsBySource ),
                    () => block.SetAttributeValue( AttributeKey.GroupResultsBySource, box.Settings.GroupResultsBySource.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.EnabledSortOrders ),
                    () => block.SetAttributeValue( AttributeKey.EnabledSortOrders, string.Join( ",", box.Settings.EnabledSortOrders ) ) );

                box.IfValidProperty( nameof( box.Settings.TrendingTerm ),
                    () => block.SetAttributeValue( AttributeKey.TrendingTerm, box.Settings.TrendingTerm ) );

                box.IfValidProperty( nameof( box.Settings.Filters ),
                    () => block.SetAttributeValue( AttributeKey.Filters, box.Settings.Filters.ToJson() ) );

                box.IfValidProperty( nameof( box.Settings.ResultsTemplate ),
                    () => block.SetAttributeValue( AttributeKey.ResultsTemplate, box.Settings.ResultsTemplate ) );

                box.IfValidProperty( nameof( box.Settings.ItemTemplate ),
                    () => block.SetAttributeValue( AttributeKey.ItemTemplate, box.Settings.ItemTemplate ) );

                box.IfValidProperty( nameof( box.Settings.PreSearchTemplate ),
                    () => block.SetAttributeValue( AttributeKey.PreSearchTemplate, box.Settings.PreSearchTemplate ) );

                box.IfValidProperty( nameof( box.Settings.BoostMatchingSegments ),
                    () => block.SetAttributeValue( AttributeKey.BoostMatchingSegments, box.Settings.BoostMatchingSegments.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.BoostMatchingRequestFilters ),
                    () => block.SetAttributeValue( AttributeKey.BoostMatchingRequestFilters, box.Settings.BoostMatchingRequestFilters.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.SegmentBoostAmount ),
                    () => block.SetAttributeValue( AttributeKey.SegmentBoostAmount, box.Settings.SegmentBoostAmount.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.RequestFilterBoostAmount ),
                    () => block.SetAttributeValue( AttributeKey.RequestFilterBoostAmount, box.Settings.RequestFilterBoostAmount.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.GroupHeaderTemplate ),
                    () => block.SetAttributeValue( AttributeKey.GroupHeaderTemplate, box.Settings.GroupHeaderTemplate ) );

                block.SaveAttributeValues( rockContext );

                return ActionOk();
            }
        }

        #endregion
    }
}
