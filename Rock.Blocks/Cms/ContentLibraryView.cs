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
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Cms.ContentLibrary;
using Rock.Cms.ContentLibrary.IndexDocuments;
using Rock.Cms.ContentLibrary.Search;
using Rock.Data;
using Rock.Enums.Blocks.Cms.ContentLibraryView;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.ContentLibraryView;
using Rock.ViewModels.Cms;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the search results of a particular content library.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianDetailBlockType" />

    [DisplayName( "Content Library View" )]
    [Category( "CMS" )]
    [Description( "Displays the search results of a particular content library." )]
    [IconCssClass( "fa fa-book-open" )]

    #region Block Attributes

    [TextField( "Content Library",
        Description = "The content library to use when searching.",
        Category = "CustomSetting",
        Key = AttributeKey.ContentLibrary )]

    [BooleanField( "Show Filters Panel",
        Description = "Determines if the filters panel should be visible.",
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
        Description = "This will group the results by library source.",
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
        DefaultValue = @"<div>
    <h2><i class=""{{ SourceEntity.IconCssClass""></i> {{ SourceName }}</h2>
    <div class=""result-items""></div>
    <div class=""actions"">
       <a href=""#"" class=""btn btn-default show-more"">Show More</a>
    </div>
</div>",
        Category = "CustomSetting",
        Key = AttributeKey.ResultsTemplate )]

    [TextField( "Item Template",
        Description = "The lava template to use to render a single result.",
        DefaultValue = @"<div class=""result-item"">
    <div>{{ results.Title }}</div>
</div>",
        Category = "CustomSetting",
        Key = AttributeKey.ItemTemplate )]

    [TextField( "Pre-Search Template",
        Description = "The lava template to use to render the content displayed before a search happens. This will not be used if Search on Load is enabled.",
        DefaultValue = "",
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
    public class ContentLibraryView : RockObsidianBlockType, IHasCustomActions
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ContentLibrary = "ContentLibrary";

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

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var contentLibrary = ContentLibraryCache.Get( GetAttributeValue( AttributeKey.ContentLibrary ).AsGuid() );

                if ( contentLibrary == null )
                {
                    return new ContentLibraryInitializationBox
                    {
                        ErrorMessage = "Block is not properly configured."
                    };
                }

                var searchOnLoad = GetAttributeValue( AttributeKey.SearchOnLoad ).AsBoolean();
                var preSearchTemplate = GetAttributeValue( AttributeKey.PreSearchTemplate );
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

                return new ContentLibraryInitializationBox
                {
                    ShowFullTextSearch = GetAttributeValue( AttributeKey.ShowFullTextSearch ).AsBoolean(),
                    ShowFiltersPanel = GetAttributeValue( AttributeKey.ShowFiltersPanel ).AsBoolean(),
                    ShowSort = GetAttributeValue( AttributeKey.ShowSort ).AsBoolean(),
                    EnabledSortOrders = GetAttributeValue( AttributeKey.EnabledSortOrders ).SplitDelimitedValues().ToList(),
                    TrendingTerm = GetAttributeValue( AttributeKey.TrendingTerm ),
                    Filters = GetSearchFilters( contentLibrary ),
                    PreSearchContent = preSearchContent,
                    SearchOnLoad = searchOnLoad,
                    InitialResults = initialSearchResults
                };
            }
        }

        /// <summary>
        /// Gets the search filters that will be used by the specified content library.
        /// </summary>
        /// <param name="contentLibrary">The content library whose search filters need to be rendered.</param>
        /// <returns>A list of <see cref="SearchFilterBag"/> objects that describe the search filters configured on the content library.</returns>
        private List<SearchFilterBag> GetSearchFilters( ContentLibraryCache contentLibrary )
        {
            var filters = new List<SearchFilterBag>();
            var attributeValues = contentLibrary.FilterSettings.AttributeValues ?? new Dictionary<string, List<ListItemBag>>();

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
                    if ( !contentLibrary.FilterSettings.YearSearchEnabled )
                    {
                        continue;
                    }

                    filters.Add( new SearchFilterBag
                    {
                        Control = contentLibrary.FilterSettings.YearSearchFilterControl,
                        Label = contentLibrary.FilterSettings.YearSearchLabel,
                        IsMultipleSelection = contentLibrary.FilterSettings.YearSearchFilterIsMultipleSelection,
                        HeaderMarkup = filterOption.HeaderMarkup,
                        Items = GetYearItemBags( contentLibrary )
                    } );

                    continue;
                }

                // Check if this is an attribute search filter.
                if ( filterOption.SourceKey.StartsWith( "attr_" ) )
                {
                    var attrKey = filterOption.SourceKey.Substring( 5 );

                    if ( !contentLibrary.FilterSettings.AttributeFilters.TryGetValue( attrKey, out var filterSettings ) )
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
            }

            return filters;
        }

        /// <summary>
        /// Get the list item bags that will represent the available options in
        /// the Year search filter.
        /// </summary>
        /// <returns>A list of <see cref="ListItemBag"/> objects.</returns>
        private static List<ListItemBag> GetYearItemBags( ContentLibraryCache contentLibrary )
        {
            var now = RockDateTime.Now;
            var items = new List<ListItemBag>();
            var filterValues = contentLibrary.FilterSettings.FieldValues ?? new Dictionary<string, List<ListItemBag>>();

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
        /// Gets the content libraries the current person is authorized to see.
        /// </summary>
        /// <returns>A list of content library cache instances.</returns>
        private List<ContentLibraryCache> GetContentLibraries()
        {
            return ContentLibraryCache.All()
                .Where( cl => cl.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson ) )
                .ToList();
        }

        /// <summary>
        /// Gets the library bag items from the list of content libraries. These
        /// will be sent to the client for use during the custom settings modal.
        /// </summary>
        /// <param name="libraries">The list of content libraries to be represented by bags.</param>
        /// <returns>The bags that represent the content libraries.</returns>
        private List<ContentLibraryListItemBag> GetContentLibraryBagItems( List<ContentLibraryCache> libraries )
        {
            return libraries
                .Select( l => new ContentLibraryListItemBag
                {
                    Value = l.Guid.ToString(),
                    Text = l.Name,
                    Filters = GetActiveLibraryFilterNames( l )
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the library filter names that are configured and active for
        /// the given content library.
        /// </summary>
        /// <param name="library">The content library whose filter names are being requested.</param>
        /// <returns>A list of filter names.</returns>
        private List<ListItemBag> GetActiveLibraryFilterNames( ContentLibraryCache library )
        {
            var filterSettings = library.FilterSettings;

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

            return names;
        }

        /// <summary>
        /// Gets all the attribute keys that are valid for use in a search query.
        /// The resulting keys are all lower cased.
        /// </summary>
        /// <param name="contentLibrary">The content library whose filters keys are being requested.</param>
        /// <returns>A list of attribute keys in lowercase form.</returns>
        private List<string> GetActiveLibraryFilterAttributeKeys( ContentLibraryCache contentLibrary )
        {
            return GetActiveLibraryFilterNames( contentLibrary )
                .Where( f => f.Value.StartsWith( "attr_" ) )
                .Select( f => f.Value.Substring( 5 ).ToLower() )
                .ToList();
        }

        /// <summary>
        /// Checks if the query string has any filter parameters specified
        /// that can be used in a search.
        /// </summary>
        /// <returns><c>true</c> if the query string contains search filters; otherwise <c>false</c>.</returns>
        private bool HasQueryStringFilters()
        {
            var contentLibrary = ContentLibraryCache.Get( GetAttributeValue( AttributeKey.ContentLibrary ).AsGuid() );

            if ( contentLibrary == null )
            {
                return false;
            }

            if ( RequestContext.GetPageParameter( "q" ).IsNotNullOrWhiteSpace() )
            {
                return true;
            }

            var validKeys = GetActiveLibraryFilterNames( contentLibrary )
                .Select( f => f.Value.StartsWith( "attr_" ) ? f.Value.Substring( 5 ) : f.Value )
                .ToList();

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
        /// Performs the initial page-load search request. This uses the query
        /// string parameters to build the search query. It will return null
        /// if something prevented the results from being retrieved.
        /// </summary>
        /// <remarks>This has a built-in timeout of 2.5 seconds to prevent the page load from being delayed due to an index engine issue.</remarks>
        /// <returns>The search results.</returns>
        private async Task<SearchResultBag> PerformInitialSearchAsync()
        {
            var contentLibrary = ContentLibraryCache.Get( GetAttributeValue( AttributeKey.ContentLibrary ).AsGuid() );

            if ( contentLibrary == null )
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
            var validKeys = GetActiveLibraryFilterNames( contentLibrary )
                .Select( f => f.Value.StartsWith( "attr_" ) ? f.Value.Substring( 5 ) : f.Value )
                .ToList();

            foreach ( var key in validKeys )
            {
                var filterValue = RequestContext.GetPageParameter( key );

                if ( filterValue.IsNotNullOrWhiteSpace() )
                {
                    query.Filters.AddOrReplace( key.ToLower(), filterValue );
                }
            }

            var searchTask = PerformSearchAsync( query, contentLibrary );

            // Wait at most 2.5 seconds for the search to complete. We don't want
            // to hold up the page loading for longer than that.
            if ( await Task.WhenAny( searchTask, Task.Delay( 2500 ) ) == searchTask )
            {
                return await searchTask;
            }

            return null;
        }

        /// <summary>
        /// Performs the search specified in the query against the content library.
        /// </summary>
        /// <param name="query">The query that represents which documents we are interested in.</param>
        /// <param name="contentLibrary">The content library that will be searched for results.</param>
        /// <returns>The search results.</returns>
        private async Task<SearchResultBag> PerformSearchAsync( SearchQueryBag query, ContentLibraryCache contentLibrary )
        {
            // Get the search query to be run.
            var searchQuery = GetSearchQueryFromBag( query, contentLibrary );

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
                AppendAllSourcesToSearchQuery( searchQuery, contentLibrary );

                resultBag.ResultSources.Add( await GetSearchResultsForSourceAsync( searchQuery, null, query.Offset ?? 0, maxResults, order ) );
            }
            else if ( query.SourceGuid.HasValue && query.SourceGuid.Value != Guid.Empty )
            {
                var source = ContentLibrarySourceCache.Get( query.SourceGuid.Value );

                if ( source == null || source.ContentLibraryId != contentLibrary.Id )
                {
                    return null;
                }

                resultBag.ResultSources.Add( await GetSearchResultsForSourceAsync( searchQuery, source, query.Offset ?? 0, maxResults, order ) );
            }
            else
            {
                var sources = ContentLibrarySourceCache.All()
                    .Where( s => s.ContentLibraryId == contentLibrary.Id )
                    .ToList();

                foreach ( var source in sources )
                {
                    resultBag.ResultSources.Add( await GetSearchResultsForSourceAsync( searchQuery, source, query.Offset ?? 0, maxResults, order ) );
                }
            }

            return resultBag;
        }

        /// <summary>
        /// Gets the search query from bag. This does not include the source,
        /// only the search text and filters.
        /// </summary>
        /// <param name="query">The query bag to use when constructing the real query.</param>
        /// <param name="contentLibrary">The content library this query will be for.</param>
        /// <returns>A new <see cref="SearchQuery"/> instance that represents the search text and filters.</returns>
        private SearchQuery GetSearchQueryFromBag( SearchQueryBag query, ContentLibraryCache contentLibrary )
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
                AppendFiltersToSearchQuery( query.Filters, searchQuery, contentLibrary );
            }

            AppendPersonalizationToSearchQuery( searchQuery, contentLibrary );
            AppendTrendingToSearchQuery( searchQuery, contentLibrary );

            return searchQuery;
        }

        /// <summary>
        /// Appends the filter required to limit results to all sources of the
        /// content library.
        /// </summary>
        /// <param name="searchQuery">The search query to be updated.</param>
        /// <param name="contentLibrary">The content library to filter results to.</param>
        private static void AppendAllSourcesToSearchQuery( SearchQuery searchQuery, ContentLibraryCache contentLibrary )
        {
            var sources = ContentLibrarySourceCache.All()
                .Where( s => s.ContentLibraryId == contentLibrary.Id )
                .ToList();

            // Build a criteria search to find all documents of all sources that
            // belong to this library.
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
        /// <param name="contentLibrary">The content library that describes the configuration.</param>
        private void AppendPersonalizationToSearchQuery( SearchQuery searchQuery, ContentLibraryCache contentLibrary )
        {
            // If nothing to do, don't modify the query.
            if ( !contentLibrary.EnableSegments && !contentLibrary.EnableRequestFilters )
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

            // Add all the personalization segments this person matches.
            if ( contentLibrary.EnableSegments )
            {
                personalizationQuery.Add( new SearchField
                {
                    Name = nameof( IndexDocumentBase.Segments ),
                    Value = "1",
                    Boost = GetAttributeValue( AttributeKey.SegmentBoostAmount ).AsDoubleOrNull() ?? 1.0d
                } );
            }

            // Add all the request filters this request matches.
            if ( contentLibrary.EnableRequestFilters )
            {
                personalizationQuery.Add( new SearchField
                {
                    Name = nameof( IndexDocumentBase.RequestFilters ),
                    Value = "1",
                    Boost = GetAttributeValue( AttributeKey.RequestFilterBoostAmount ).AsDoubleOrNull() ?? 1.0d
                } );
            }

            searchQuery.Add( personalizationQuery );
        }

        /// <summary>
        /// Appends the trending boost values to the search query.
        /// </summary>
        /// <param name="searchQuery">The search query to be updated.</param>
        /// <param name="contentLibrary">The content library that describes the configuration.</param>
        private void AppendTrendingToSearchQuery( SearchQuery searchQuery, ContentLibraryCache contentLibrary )
        {
            // If there is nothing to do, then don't modify the query.
            if ( !contentLibrary.TrendingEnabled )
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
        /// <param name="contentLibrary">The content library this query will be run against.</param>
        private void AppendFiltersToSearchQuery( Dictionary<string, string> filters, SearchQuery searchQuery, ContentLibraryCache contentLibrary )
        {
            if ( filters == null )
            {
                return;
            }

            // Get all the attribute keys that are valid to be used for filtering.
            var validAttributeKeys = GetActiveLibraryFilterAttributeKeys( contentLibrary );

            foreach ( var filterPair in filters )
            {
                // If no value was specified, do not filter.
                if ( filterPair.Value.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                var filterKey = filterPair.Key.ToLower();

                // If this is the Year filter, special processing is done.
                if ( contentLibrary.FilterSettings.YearSearchEnabled && contentLibrary.FilterSettings.YearSearchLabel.ToLower() == filterKey )
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

                // Find the attribute filter for this query filter.
                var attributeFilterKey = contentLibrary.FilterSettings.AttributeFilters
                    .Where( kvp => kvp.Value.Label.ToLower() == filterKey )
                    .Select( kvp => kvp.Key )
                    .FirstOrDefault();

                // Make sure the filter is a valid key and that it is enabled.
                if ( attributeFilterKey == null || !validAttributeKeys.Contains( filterKey ) )
                {
                    continue;
                }

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
                            Value = value
                        } );
                    }

                    searchQuery.Add( valueQuery );
                }
            }
        }

        /// <summary>
        /// Get search results for a single content library source.
        /// </summary>
        /// <param name="searchQuery">The base search query to use when searching.</param>
        /// <param name="source">The source to load results for or <c>null</c> if all results from the library should be loaded.</param>
        /// <param name="offset">The starting offset in the search results to return.</param>
        /// <param name="maxResults">The maximum number of results to return.</param>
        /// <param name="order">The order in which results will be returned.</param>
        /// <returns>A new <see cref="SearchResultSourceBag"/> that contains the results from this query.</returns>
        private async Task<SearchResultSourceBag> GetSearchResultsForSourceAsync( SearchQuery searchQuery, ContentLibrarySourceCache source, int offset, int maxResults, SearchOrder order )
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

            // Run the search.
            var results = await ContentIndexContainer.GetActiveComponent().SearchAsync( searchQuery, searchOptions );

            // Start preparing the result.
            var resultBag = new SearchResultSourceBag
            {
                SourceGuid = source?.Guid ?? Guid.Empty,
                HasMore = results.TotalResultsAvailable > ( maxResults + offset ),
                Results = new List<string>()
            };

            // Always include the source result template just in case something
            // weird happens and they start loading items past offset 0 first.
            var resultsTemplate = GetAttributeValue( AttributeKey.ResultsTemplate );
            var sourceEntityType = source.EntityType.GetEntityType();
            var sourceEntity = Reflection.GetIEntityForEntityType( sourceEntityType, source.EntityId );
            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "SourceType", sourceEntityType.Name );
            mergeFields.Add( "SourceName", sourceEntity.ToString() );
            mergeFields.Add( "SourceEntity", sourceEntity );
            resultBag.Template = resultsTemplate.ResolveMergeFields( mergeFields );

            // Merge the results with the Lava template.
            var itemTemplate = GetAttributeValue( AttributeKey.ItemTemplate );
            mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "SourceType", sourceEntityType.Name );
            mergeFields.Add( "SourceName", sourceEntity.ToString() );
            mergeFields.Add( "SourceEntity", sourceEntity );

            foreach ( var result in results.Documents )
            {
                mergeFields.AddOrReplace( "Item", result );

                resultBag.Results.Add( itemTemplate.ResolveMergeFields( mergeFields ) );
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

            if ( BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                actions.Add( new BlockCustomActionBag
                {
                    IconCssClass = "fa fa-edit",
                    Tooltip = "Settings",
                    ComponentFileUrl = "/Obsidian/Blocks/CMS/contentLibraryViewCustomSettings"
                } );
            }

            return actions;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Performs the search using the information specified in the query.
        /// </summary>
        /// <param name="query">The query that contains the search request.</param>
        /// <returns>The content for the search results.</returns>
        [BlockAction]
        public async Task<BlockActionResult> Search( SearchQueryBag query )
        {
            var contentLibrary = ContentLibraryCache.Get( GetAttributeValue( AttributeKey.ContentLibrary ).AsGuid() );

            if ( contentLibrary == null )
            {
                return ActionInternalServerError( "Block is not properly configured." );
            }

            var resultBag = await PerformSearchAsync( query, contentLibrary );

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
                    ContentLibraryItems = GetContentLibraryBagItems( GetContentLibraries() )
                };

                var filters = GetAttributeValue( AttributeKey.Filters ).FromJsonOrNull<List<FilterOptionsBag>>() ?? new List<FilterOptionsBag>();

                var settings = new CustomSettingsBag
                {
                    ContentLibrary = GetAttributeValue( AttributeKey.ContentLibrary ).AsGuidOrNull(),
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
                    ItemTemplate = GetAttributeValue( AttributeKey.ItemTemplate ),
                    PreSearchTemplate = GetAttributeValue( AttributeKey.PreSearchTemplate ),
                    BoostMatchingSegments = GetAttributeValue( AttributeKey.BoostMatchingSegments ).AsBoolean(),
                    BoostMatchingRequestFilters = GetAttributeValue( AttributeKey.BoostMatchingRequestFilters ).AsBoolean(),
                    SegmentBoostAmount = GetAttributeValue( AttributeKey.SegmentBoostAmount ).AsDecimalOrNull(),
                    RequestFilterBoostAmount = GetAttributeValue( AttributeKey.RequestFilterBoostAmount ).AsDecimalOrNull()
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

                // Setting the content library requires additional validation checks.
                var errorActionResult = box.IfValidProperty( nameof( box.Settings.ContentLibrary ), () =>
                {
                    if ( !box.Settings.ContentLibrary.HasValue )
                    {
                        return ActionBadRequest( "Content Library is required." );
                    }

                    var libraryCache = new ContentLibraryService( rockContext ).Get( box.Settings.ContentLibrary.Value );

                    if ( libraryCache == null )
                    {
                        return ActionNotFound( "Content library was not found." );
                    }
                    else if ( !libraryCache.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson ) )
                    {
                        return ActionForbidden( "You do not have access to that content library." );
                    }

                    block.SetAttributeValue( AttributeKey.ContentLibrary, box.Settings.ContentLibrary.Value.ToString() );

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

                block.SaveAttributeValues( rockContext );

                return ActionOk();
            }
        }

        #endregion
    }
}
