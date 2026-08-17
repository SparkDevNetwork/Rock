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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.UniversalSearch;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.UniversalSearch;
using Rock.ViewModels.Cms;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Provides a search experience across all indexable entity types in Rock.
    /// </summary>
    [DisplayName( "Universal Search" )]
    [Category( "CMS" )]
    [Description( "A block to search for all indexable entity types in Rock." )]
    [IconCssClass( "ti ti-search" )]

    #region Block Attributes

    [BooleanField(
        "Show Filters",
        Description = "Toggles the display of the model filter which allows the user to select which models to search on.",
        DefaultBooleanValue = true,
        Category = "CustomSetting",
        Order = 0,
        Key = AttributeKey.ShowFilters )]

    [TextField(
        "Enabled Models",
        Description = "The models that should be enabled for searching.",
        IsRequired = true,
        Category = "CustomSetting",
        Order = 1,
        Key = AttributeKey.EnabledModels )]

    [IntegerField(
        "Results Per Page",
        Description = "The number of results to show per page.",
        IsRequired = true,
        DefaultIntegerValue = DefaultItemsPerPage,
        Category = "CustomSetting",
        Order = 2,
        Key = AttributeKey.ResultsPerPage )]

    [EnumField(
        "Search Type",
        Description = "The type of search to perform.",
        EnumSourceType = typeof( SearchType ),
        IsRequired = true,
        DefaultValue = "0",
        Category = "CustomSetting",
        Order = 3,
        Key = AttributeKey.SearchType )]

    [TextField(
        "Base Field Filters",
        Description = "These field filters will always be enabled and will not be changeable by the individual. Uses the same syntax as the lava command.",
        IsRequired = false,
        Category = "CustomSetting",
        Order = 4,
        Key = AttributeKey.BaseFieldFilters )]

    [BooleanField(
        "Show Refined Search",
        Description = "Determines whether the refined search should be shown.",
        DefaultBooleanValue = true,
        Category = "CustomSetting",
        Order = 5,
        Key = AttributeKey.ShowRefinedSearch )]

    [BooleanField(
        "Show Scores",
        Description = "Enables the display of scores for help with debugging.",
        Category = "CustomSetting",
        Order = 6,
        Key = AttributeKey.ShowScores )]

    [CodeEditorField(
        "Lava Result Template",
        Description = "Custom Lava results template to use instead of the standard results.",
        EditorMode = CodeEditorMode.Lava,
        DefaultValue = DefaultLavaResultTemplate,
        Category = "CustomSetting",
        Order = 7,
        Key = AttributeKey.LavaResultTemplate )]

    [BooleanField(
        "Use Custom Results",
        Description = "Determines if the custom results should be displayed.",
        Category = "CustomSetting",
        Order = 8,
        Key = AttributeKey.UseCustomResults )]

    [LavaCommandsField(
        "Custom Results Commands",
        Description = "The custom Lava fields to allow.",
        Category = "CustomSetting",
        Order = 9,
        Key = AttributeKey.CustomResultsCommands )]

    [CodeEditorField(
        "Search Input Pre-HTML",
        Description = "Custom Lava to place before the search input (for styling).",
        EditorMode = CodeEditorMode.Lava,
        Category = "CustomSetting",
        Order = 10,
        Key = AttributeKey.PreHtml )]

    [CodeEditorField(
        "Search Input Post-HTML",
        Description = "Custom Lava to place after the search input (for styling).",
        EditorMode = CodeEditorMode.Lava,
        Category = "CustomSetting",
        Order = 11,
        Key = AttributeKey.PostHtml )]

    #endregion Block Attributes

    [ConfigurationChangedReload( Rock.Enums.Cms.BlockReloadMode.Block )]
    [Rock.SystemGuid.EntityTypeGuid( "2C08C1AF-E118-48B0-A25F-C6B3221EBB4A" )]
    [Rock.SystemGuid.BlockTypeGuid( "FDF1BBFF-7A7B-4F4E-BF34-831203B0FEAC" )]
    public class UniversalSearch : RockBlockType, IHasCustomActions
    {
        #region Constants

        private const int DefaultItemsPerPage = 20;

        private const string DefaultLavaResultTemplate = @"<ul>{% for result in Results %}
    <li><i class='ti {{ result.IconCssClass }}'></i> {{ result.DocumentName }} <small>(Score {{ result.Score }} )</small> </li>
{% endfor %}</ul>";

        #endregion Constants

        #region Keys

        private static class AttributeKey
        {
            public const string ShowFilters = "ShowFilters";
            public const string EnabledModels = "EnabledModels";
            public const string ResultsPerPage = "ResultsPerPage";
            public const string SearchType = "SearchType";
            public const string BaseFieldFilters = "BaseFieldFilters";
            public const string ShowRefinedSearch = "ShowRefinedSearch";
            public const string ShowScores = "ShowScores";
            public const string LavaResultTemplate = "LavaResultTemplate";
            public const string UseCustomResults = "UseCustomResults";
            public const string CustomResultsCommands = "CustomResultsCommands";
            public const string PreHtml = "PreHtml";
            public const string PostHtml = "PostHtml";
        }

        private static class PageParameterKey
        {
            public const string DocumentType = "DocumentType";
            public const string DocumentId = "DocumentId";
            public const string ShowRefineSearch = "ShowRefineSearch";
            public const string SmartSearch = "SmartSearch";
            public const string Query = "Q";
            public const string SearchType = "SearchType";
            public const string Models = "Models";
            public const string CurrentPage = "CurrentPage";
            public const string RefinedSearch = "RefinedSearch";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var bag = new UniversalSearchBag
            {
                Query = PageParameter( PageParameterKey.Query ),
                SearchTypeValue = ( int ) GetEffectiveSearchType(),
                ResultsPerPage = GetResultsPerPage(),
                ShowRefinedSearchToggle = GetShowRefinedSearchToggle(),
                IsRefinedSearchVisible = PageParameter( PageParameterKey.RefinedSearch ).AsBoolean(),
                IsSmartSearchRequest = PageParameter( PageParameterKey.SmartSearch ).IsNotNullOrWhiteSpace()
            };

            if ( TryGetRedirectUrl( out var redirectUrl, out var redirectError ) )
            {
                bag.RedirectUrl = redirectUrl;
                RequestContext.Response.RedirectToUrl( redirectUrl );
                return bag;
            }

            if ( redirectError.IsNotNullOrWhiteSpace() )
            {
                bag.ErrorMessage = redirectError;
                return bag;
            }

            bag.AvailableModels = GetAvailableModels();
            bag.ShowModelFilter = GetAttributeValue( AttributeKey.ShowFilters ).AsBoolean() && bag.AvailableModels.Count > 1;
            bag.Filters = GetAvailableFilters();
            bag.SelectedModelIds = GetSelectedModelIdsFromPageParameters();
            bag.SelectedFilters = GetSelectedFiltersFromPageParameters( bag.Filters );
            bag.PreHtml = ResolveConfiguredHtml( AttributeKey.PreHtml );
            bag.PostHtml = ResolveConfiguredHtml( AttributeKey.PostHtml );

            if ( bag.Query.IsNotNullOrWhiteSpace() )
            {
                bag.InitialResults = ExecuteSearch( new UniversalSearchRequestBag
                {
                    Query = bag.Query,
                    SelectedEntityTypeIds = bag.SelectedModelIds.Select( v => v.AsInteger() ).Where( v => v > 0 ).ToList(),
                    SelectedFilters = bag.SelectedFilters,
                    CurrentPage = GetCurrentPageNumber(),
                    SearchTypeValue = bag.SearchTypeValue,
                    IsSmartSearchRequest = bag.IsSmartSearchRequest
                } );
            }

            return bag;
        }

        /// <summary>
        /// Executes the requested search and formats the results for display.
        /// </summary>
        /// <param name="bag">The request information to use.</param>
        /// <returns>A bag containing the rendered results.</returns>
        private UniversalSearchResultsBag ExecuteSearch( UniversalSearchRequestBag bag )
        {
            var resultBag = new UniversalSearchResultsBag
            {
                CurrentPage = Math.Max( 0, bag.CurrentPage ),
                ItemsPerPage = GetResultsPerPage()
            };

            if ( bag.Query.IsNullOrWhiteSpace() )
            {
                return resultBag;
            }

            if ( TryGetRedirectUrlFromQuery( bag.Query, out var redirectUrl ) )
            {
                resultBag.RedirectUrl = redirectUrl;
                //RequestContext.Response.RedirectToUrl( redirectUrl );
                return resultBag;
            }

            var client = IndexContainer.GetActiveComponent();

            if ( client == null )
            {
                resultBag.WarningMessage = "No indexing service is currently configured.";
                return resultBag;
            }

            var selectedEntities = GetSelectedEntityIds( bag.SelectedEntityTypeIds, bag.IsSmartSearchRequest );
            var fieldValues = GetSelectedFieldValues( selectedEntities, bag.SelectedFilters, bag.IsSmartSearchRequest );
            var fieldCriteria = new SearchFieldCriteria
            {
                FieldValues = fieldValues
            };

            try
            {
                var results = client.Search(
                    bag.Query,
                    GetSearchTypeFromValue( bag.SearchTypeValue ),
                    selectedEntities,
                    fieldCriteria,
                    resultBag.ItemsPerPage,
                    resultBag.CurrentPage * resultBag.ItemsPerPage,
                    out var totalResultsAvailable );

                resultBag.TotalResultsAvailable = totalResultsAvailable;
                resultBag.ResultsHtml = FormatResults( results );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                resultBag.WarningMessage = "An error occurred while searching.";
            }

            return resultBag;
        }

        /// <summary>
        /// Formats the search result collection into display markup.
        /// </summary>
        /// <param name="results">The results to format.</param>
        /// <returns>The rendered result markup.</returns>
        private string FormatResults( List<Rock.UniversalSearch.IndexModels.IndexModelBase> results )
        {
            results = results ?? new List<Rock.UniversalSearch.IndexModels.IndexModelBase>();
            var mergeFields = new Dictionary<string, object>();

            if ( GetAttributeValue( AttributeKey.UseCustomResults ).AsBoolean() )
            {
                mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                mergeFields.Add( "Results", results );

                return GetAttributeValue( AttributeKey.LavaResultTemplate )
                    .ResolveMergeFields( mergeFields, GetAttributeValue( AttributeKey.CustomResultsCommands ) );
            }

            var formattedResults = new StringBuilder();
            formattedResults.Append( "<ul class='list-unstyled'>" );

            var showScores = GetAttributeValue( AttributeKey.ShowScores ).AsBoolean();
            mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, RequestContext.CurrentPerson );

            foreach ( var result in results )
            {
                var formattedResult = result.FormatSearchResult( RequestContext.CurrentPerson, null, mergeFields );

                if ( !formattedResult.IsViewAllowed )
                {
                    continue;
                }

                formattedResults.Append( formattedResult.FormattedResult );

                if ( showScores )
                {
                    formattedResults.AppendFormat( "<div class='pull-right'><small>{0}</small></div>", result.Score );
                }

                formattedResults.Append( "<hr />" );
            }

            formattedResults.Append( "</ul>" );

            return formattedResults.ToString();
        }

        /// <summary>
        /// Gets the entity type options that can be searched.
        /// </summary>
        /// <returns>The collection of entity type options.</returns>
        private List<ListItemBag> GetAvailableModels()
        {
            return GetIndexableEntities()
                .Select( e => new ListItemBag
                {
                    Text = e.FriendlyName,
                    Value = e.Id.ToString()
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the available refine search filters.
        /// </summary>
        /// <returns>The collection of filters that can be displayed.</returns>
        private List<UniversalSearchFilterBag> GetAvailableFilters()
        {
            var filters = new List<UniversalSearchFilterBag>();

            foreach ( var entity in GetIndexableEntities() )
            {
                var entityType = entity.GetEntityType();

                if ( !SupportsIndexFieldFiltering( entityType ) )
                {
                    continue;
                }

                var filterOptions = GetIndexFilterConfig( entityType );

                if ( filterOptions?.FilterValues == null || filterOptions.FilterValues.Count == 0 )
                {
                    continue;
                }

                filters.Add( new UniversalSearchFilterBag
                {
                    EntityTypeId = entity.Id,
                    FieldName = filterOptions.FilterField,
                    Label = filterOptions.FilterLabel,
                    Items = filterOptions.FilterValues
                        .Where( v => v != null )
                        .Distinct()
                        .Select( v => new ListItemBag
                        {
                            Text = v,
                            Value = v
                        } )
                        .ToList()
                } );
            }

            return filters;
        }

        /// <summary>
        /// Gets the entity identifiers that should participate in the search.
        /// </summary>
        /// <param name="selectedEntityTypeIds">The entity type identifiers selected by the individual.</param>
        /// <param name="isSmartSearchRequest"><c>true</c> if smart search settings should be used; otherwise <c>false</c>.</param>
        /// <returns>The entity identifiers to search.</returns>
        private List<int> GetSelectedEntityIds( List<int> selectedEntityTypeIds, bool isSmartSearchRequest )
        {
            if ( isSmartSearchRequest )
            {
                // get entities from smart search config
                var searchEntitiesSetting = Rock.Web.SystemSettings.GetValue( "core_SmartSearchUniversalSearchEntities" );

                if ( searchEntitiesSetting.IsNotNullOrWhiteSpace() )
                {
                    return searchEntitiesSetting.Split( ',' ).Select( int.Parse ).ToList();
                }
            }

            selectedEntityTypeIds = selectedEntityTypeIds ?? new List<int>();

            if ( selectedEntityTypeIds.Count > 0 )
            {
                return selectedEntityTypeIds;
            }

            // if no entities from the UI get from the block config
            if ( GetAttributeValue( AttributeKey.EnabledModels ).IsNotNullOrWhiteSpace() )
            {
                return GetAttributeValue( AttributeKey.EnabledModels ).Split( ',' ).Select( int.Parse ).ToList();
            }

            return new List<int>();
        }

        /// <summary>
        /// Gets the selected field values to apply during the search.
        /// </summary>
        /// <param name="selectedEntities">The entity identifiers that are included in the search.</param>
        /// <param name="selectedFilters">The selected field values keyed by filter field.</param>
        /// <param name="isSmartSearchRequest"><c>true</c> if smart search settings should be used; otherwise <c>false</c>.</param>
        /// <returns>The field values to apply.</returns>
        private List<FieldValue> GetSelectedFieldValues( List<int> selectedEntities, Dictionary<string, List<string>> selectedFilters, bool isSmartSearchRequest )
        {
            var fieldValues = new List<FieldValue>();

            if ( isSmartSearchRequest )
            {
                // get the field criteria
                var fieldCriteriaSetting = Rock.Web.SystemSettings.GetValue( "core_SmartSearchUniversalSearchFieldCriteria" );

                if ( fieldCriteriaSetting.IsNotNullOrWhiteSpace() )
                {
                    foreach ( var queryString in fieldCriteriaSetting.ToKeyValuePairList() )
                    {
                        foreach ( var value in queryString.Value.ToString().Split( ',' ) )
                        {
                            fieldValues.Add( new FieldValue
                            {
                                Field = queryString.Key,
                                Value = value
                            } );
                        }
                    }
                }
            }
            else
            {
                // add any base field filters from block settings
                if ( GetAttributeValue( AttributeKey.BaseFieldFilters ).IsNotNullOrWhiteSpace() )
                {
                    foreach ( var filterField in GetAttributeValue( AttributeKey.BaseFieldFilters ).ToKeyValuePairList() )
                    {
                        foreach ( var value in filterField.Value.ToString().Split( ',' ) )
                        {
                            fieldValues.Add( new FieldValue
                            {
                                Field = filterField.Key,
                                Value = value
                            } );
                        }
                    }
                }

                selectedFilters = selectedFilters ?? new Dictionary<string, List<string>>();

                // get dynamic filters
                foreach ( var filter in GetAvailableFilters() )
                {
                    if ( !selectedEntities.Contains( filter.EntityTypeId ) )
                    {
                        continue;
                    }

                    if ( !selectedFilters.TryGetValue( filter.FieldName, out var values ) || values == null )
                    {
                        continue;
                    }

                    foreach ( var value in values.Where( v => v.IsNotNullOrWhiteSpace() ) )
                    {
                        fieldValues.Add( new FieldValue
                        {
                            Field = filter.FieldName,
                            Value = value
                        } );
                    }
                }
            }

            // Check for the existence of field criteria. If any exist check the entity list for models that
            // do not support field criteria and add a filter for modelconfig with 'nofilters'.
            // This keeps them from being excluded.
            if ( fieldValues.Count > 0 )
            {
                fieldValues.Add( new FieldValue { Field = "modelConfiguration", Value = "nofilters", Boost = 3 } );
            }

            return fieldValues;
        }

        /// <summary>
        /// Gets the selected model identifiers from the page parameters.
        /// </summary>
        /// <returns>The selected model identifiers.</returns>
        private List<string> GetSelectedModelIdsFromPageParameters()
        {
            var models = PageParameter( PageParameterKey.Models );

            if ( models.IsNullOrWhiteSpace() )
            {
                return new List<string>();
            }

            return models.Split( ',' ).Select( s => s.Trim() ).Where( s => s.IsNotNullOrWhiteSpace() ).ToList();
        }

        /// <summary>
        /// Gets the selected filter values from the page parameters.
        /// </summary>
        /// <param name="filters">The filter definitions to inspect.</param>
        /// <returns>The selected filter values keyed by field name.</returns>
        private Dictionary<string, List<string>> GetSelectedFiltersFromPageParameters( IEnumerable<UniversalSearchFilterBag> filters )
        {
            var selectedFilters = new Dictionary<string, List<string>>( StringComparer.OrdinalIgnoreCase );

            foreach ( var filter in filters )
            {
                var filterValue = PageParameter( filter.FieldName );

                if ( filterValue.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                selectedFilters[filter.FieldName] = filterValue
                    .Split( ',' )
                    .Select( v => v.Trim() )
                    .Where( v => v.IsNotNullOrWhiteSpace() )
                    .ToList();
            }

            return selectedFilters;
        }

        /// <summary>
        /// Resolves a configured Lava HTML field into markup.
        /// </summary>
        /// <param name="attributeKey">The attribute key that contains the Lava markup.</param>
        /// <returns>The resolved markup.</returns>
        private string ResolveConfiguredHtml( string attributeKey )
        {
            var html = GetAttributeValue( attributeKey );

            if ( html.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            return html.ResolveMergeFields( Rock.Lava.LavaHelper.GetCommonMergeFields( null ) );
        }

        /// <summary>
        /// Gets the configured search type, honoring the page parameter when valid.
        /// </summary>
        /// <returns>The effective search type.</returns>
        private SearchType GetEffectiveSearchType()
        {
            var searchType = GetAttributeValue( AttributeKey.SearchType ).ConvertToEnum<SearchType>();
            var queryStringSearchType = PageParameter( PageParameterKey.SearchType );

            if ( queryStringSearchType.IsNotNullOrWhiteSpace() )
            {
                var queryStringValue = queryStringSearchType.ConvertToEnumOrNull<SearchType>();

                if ( queryStringValue.HasValue )
                {
                    searchType = queryStringValue.Value;
                }
            }

            return searchType;
        }

        /// <summary>
        /// Gets a value indicating whether the refine search toggle should be shown.
        /// </summary>
        /// <returns><c>true</c> if the toggle should be shown; otherwise <c>false</c>.</returns>
        private bool GetShowRefinedSearchToggle()
        {
            if ( PageParameter( PageParameterKey.SmartSearch ).IsNotNullOrWhiteSpace() )
            {
                return PageParameter( PageParameterKey.ShowRefineSearch ).AsBoolean();
            }

            return GetAttributeValue( AttributeKey.ShowRefinedSearch ).AsBoolean();
        }

        /// <summary>
        /// Gets the configured results per page value.
        /// </summary>
        /// <returns>The number of results to show per page.</returns>
        private int GetResultsPerPage()
        {
            return GetAttributeValue( AttributeKey.ResultsPerPage ).AsIntegerOrNull() ?? DefaultItemsPerPage;
        }

        /// <summary>
        /// Gets the zero-based current page number from the page parameters.
        /// </summary>
        /// <returns>The zero-based current page number.</returns>
        private int GetCurrentPageNumber()
        {
            var currentPage = PageParameter( PageParameterKey.CurrentPage ).AsIntegerOrNull();

            if ( !currentPage.HasValue || currentPage.Value <= 1 )
            {
                return 0;
            }

            return currentPage.Value - 1;
        }

        /// <summary>
        /// Gets the list of indexable entities allowed by the block configuration.
        /// </summary>
        /// <returns>The collection of indexable entities.</returns>
        private List<EntityTypeCache> GetIndexableEntities()
        {
            var enabledModelIds = new List<int>();

            if ( GetAttributeValue( AttributeKey.EnabledModels ).IsNotNullOrWhiteSpace() )
            {
                enabledModelIds = GetAttributeValue( AttributeKey.EnabledModels ).Split( ',' ).Select( int.Parse ).ToList();
            }

            var indexableEntities = EntityTypeCache.All().Where( e => e.IsIndexingEnabled == true ).ToList();

            // If enabled entities setting is set, further filter by those.
            if ( enabledModelIds.Count > 0 )
            {
                indexableEntities = indexableEntities.Where( e => enabledModelIds.Contains( e.Id ) ).ToList();
            }

            return indexableEntities;
        }

        /// <summary>
        /// Gets the index filter configuration from the provided entity type.
        /// </summary>
        /// <param name="entityType">The entity type to inspect.</param>
        /// <returns>The filter configuration for the entity type.</returns>
        private ModelFieldFilterConfig GetIndexFilterConfig( Type entityType )
        {
            if ( entityType != null )
            {
                var classInstance = Activator.CreateInstance( entityType, null );
                var bulkItemsMethod = entityType.GetMethod( "GetIndexFilterConfig" );

                if ( classInstance != null && bulkItemsMethod != null )
                {
                    return ( ModelFieldFilterConfig ) bulkItemsMethod.Invoke( classInstance, null );
                }
            }

            return new ModelFieldFilterConfig();
        }

        /// <summary>
        /// Determines whether the specified entity type supports index field filtering.
        /// </summary>
        /// <param name="entityType">The entity type to inspect.</param>
        /// <returns><c>true</c> if field filtering is supported; otherwise <c>false</c>.</returns>
        private bool SupportsIndexFieldFiltering( Type entityType )
        {
            if ( entityType != null )
            {
                var classInstance = Activator.CreateInstance( entityType, null );
                var bulkItemsMethod = entityType.GetMethod( "SupportsIndexFieldFiltering" );

                if ( classInstance != null && bulkItemsMethod != null )
                {
                    return ( bool ) bulkItemsMethod.Invoke( classInstance, null );
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the effective <see cref="SearchType"/> from the provided integer value.
        /// </summary>
        /// <param name="searchTypeValue">The integer value to convert.</param>
        /// <returns>The resolved search type.</returns>
        private SearchType GetSearchTypeFromValue( int searchTypeValue )
        {
            return Enum.IsDefined( typeof( SearchType ), searchTypeValue )
                ? ( SearchType ) searchTypeValue
                : GetEffectiveSearchType();
        }

        /// <summary>
        /// Attempts to resolve a redirect URL from the provided query text.
        /// </summary>
        /// <param name="query">The query text to inspect.</param>
        /// <param name="redirectUrl">The resolved redirect URL when found.</param>
        /// <returns><c>true</c> if the query identified a specific document; otherwise <c>false</c>.</returns>
        private bool TryGetRedirectUrlFromQuery( string query, out string redirectUrl )
        {
            redirectUrl = null;

            if ( query.IsNullOrWhiteSpace() )
            {
                return false;
            }

            var queryParts = query.SplitDelimitedValues( "/" );

            if ( queryParts.Length != 2 )
            {
                return false;
            }

            var indexDocumentEntityType = EntityTypeCache.Get( queryParts[0], createNew: false );

            if ( indexDocumentEntityType == null )
            {
                return false;
            }

            return TryResolveDocumentUrl( queryParts[0], queryParts[1], out redirectUrl, out _ );
        }
        /// <summary>
        /// Attempts to determine whether the current request should redirect to a specific indexed document.
        /// </summary>
        /// <param name="redirectUrl">The URL to redirect to when successful.</param>
        /// <param name="errorMessage">The error message to display when a redirect target could not be resolved.</param>
        /// <returns><c>true</c> if a redirect URL was found; otherwise <c>false</c>.</returns>
        private bool TryGetRedirectUrl( out string redirectUrl, out string errorMessage )
        {
            redirectUrl = null;
            errorMessage = null;

            var documentType = PageParameter( PageParameterKey.DocumentType );
            var documentId = PageParameter( PageParameterKey.DocumentId );

            if ( documentType.IsNotNullOrWhiteSpace() && documentId.IsNotNullOrWhiteSpace() )
            {
                return TryResolveDocumentUrl( documentType, documentId, out redirectUrl, out errorMessage );
            }

            var query = PageParameter( PageParameterKey.Query );

            if ( query.IsNullOrWhiteSpace() )
            {
                return false;
            }

            var queryParts = query.SplitDelimitedValues( "/" );

            if ( queryParts.Length != 2 )
            {
                return false;
            }

            var indexDocumentEntityType = EntityTypeCache.Get( queryParts[0], createNew: false );

            if ( indexDocumentEntityType == null )
            {
                return false;
            }

            return TryResolveDocumentUrl( queryParts[0], queryParts[1], out redirectUrl, out errorMessage );
        }

        /// <summary>
        /// Attempts to resolve the public URL for the specified indexed document.
        /// </summary>
        /// <param name="documentType">The indexed document type name.</param>
        /// <param name="documentId">The indexed document identifier.</param>
        /// <param name="documentUrl">The resolved URL when successful.</param>
        /// <param name="errorMessage">The error message to display when unsuccessful.</param>
        /// <returns><c>true</c> if the URL was resolved; otherwise <c>false</c>.</returns>
        private bool TryResolveDocumentUrl( string documentType, string documentId, out string documentUrl, out string errorMessage )
        {
            documentUrl = null;
            errorMessage = null;

            var indexDocumentEntityType = EntityTypeCache.Get( documentType );
            var indexDocumentType = indexDocumentEntityType?.GetEntityType();
            var client = IndexContainer.GetActiveComponent();

            if ( client == null )
            {
                errorMessage = "No indexing service is currently configured.";
                return false;
            }

            if ( indexDocumentType == null )
            {
                errorMessage = "Invalid document type.";
                return false;
            }

            var document = client.GetDocumentById( indexDocumentType, documentId );
            documentUrl = document?.GetDocumentUrl();

            if ( documentUrl.IsNullOrWhiteSpace() )
            {
                errorMessage = "No URL is available for the provided index document.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the settings that will be displayed in the custom settings modal.
        /// </summary>
        /// <returns>The current custom settings values.</returns>
        private CustomSettingsBag GetCustomSettingsBag()
        {
            return new CustomSettingsBag
            {
                ShowFilters = GetAttributeValue( AttributeKey.ShowFilters ).AsBoolean(),
                EnabledModels = GetAttributeValue( AttributeKey.EnabledModels ).SplitDelimitedValues().ToList(),
                ResultsPerPage = GetAttributeValue( AttributeKey.ResultsPerPage ).AsIntegerOrNull(),
                SearchType = GetAttributeValue( AttributeKey.SearchType ),
                BaseFieldFilters = GetAttributeValue( AttributeKey.BaseFieldFilters ),
                ShowRefinedSearch = GetAttributeValue( AttributeKey.ShowRefinedSearch ).AsBoolean(),
                ShowScores = GetAttributeValue( AttributeKey.ShowScores ).AsBoolean(),
                UseCustomResults = GetAttributeValue( AttributeKey.UseCustomResults ).AsBoolean(),
                LavaResultTemplate = GetAttributeValue( AttributeKey.LavaResultTemplate ),
                CustomResultsCommands = GetAttributeValue( AttributeKey.CustomResultsCommands ).SplitDelimitedValues().ToList(),
                PreHtml = GetAttributeValue( AttributeKey.PreHtml ),
                PostHtml = GetAttributeValue( AttributeKey.PostHtml )
            };
        }

        /// <summary>
        /// Gets the options required to render the custom settings modal.
        /// </summary>
        /// <returns>The custom settings option values.</returns>
        private CustomSettingsOptionsBag GetCustomSettingsOptions()
        {
            return new CustomSettingsOptionsBag
            {
                SearchTypeItems = GetSearchTypeItems(),
                EnabledModelItems = GetEnabledModelItems(),
                LavaCommandItems = GetLavaCommandItems()
            };
        }

        /// <summary>
        /// Gets the search type items that can be selected in the custom settings modal.
        /// </summary>
        /// <returns>The selectable search types.</returns>
        private List<ListItemBag> GetSearchTypeItems()
        {
            return Enum.GetValues( typeof( SearchType ) )
                .Cast<SearchType>()
                .Select( searchType => new ListItemBag
                {
                    Text = searchType.ToString().SplitCase(),
                    Value = ( ( int ) searchType ).ToString()
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the enabled model items that can be selected in the custom settings modal.
        /// </summary>
        /// <returns>The selectable enabled model items.</returns>
        private List<ListItemBag> GetEnabledModelItems()
        {
            return EntityTypeCache.All()
                .Where( entityType => entityType.IsIndexingSupported == true && entityType.IsIndexingEnabled == true )
                .OrderBy( entityType => entityType.FriendlyName )
                .Select( entityType => new ListItemBag
                {
                    Text = entityType.FriendlyName,
                    Value = entityType.Id.ToString()
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the Lava command items that can be selected in the custom settings modal.
        /// </summary>
        /// <returns>The selectable Lava command items.</returns>
        private List<ListItemBag> GetLavaCommandItems()
        {
            var commands = new List<string> { "All" };
            commands.AddRange( Rock.Lava.LavaHelper.GetLavaCommands() );

            return commands
                .Distinct()
                .Select( command => new ListItemBag
                {
                    Text = command,
                    Value = command
                } )
                .ToList();
        }
        #endregion Methods

        #region IHasCustomAdministrateActions

        /// <inheritdoc/>
        List<BlockCustomActionBag> IHasCustomActions.GetCustomActions( bool canEdit, bool canAdministrate )
        {
            var actions = new List<BlockCustomActionBag>();

            if ( canAdministrate )
            {
                actions.Add( new BlockCustomActionBag
                {
                    IconCssClass = "ti ti-edit",
                    Tooltip = "Settings",
                    ComponentFileUrl = "/Obsidian/Blocks/CMS/universalSearchCustomSettings.obs"
                } );
            }

            return actions;
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

        #region Block Actions

        /// <summary>
        /// Gets the custom settings that can be edited for this block.
        /// </summary>
        /// <returns>A box containing the current settings and available options.</returns>
        [BlockAction]
        public BlockActionResult GetCustomSettings()
        {
            if ( !BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to edit block settings." );
            }

            return ActionOk( new CustomSettingsBox<CustomSettingsBag, CustomSettingsOptionsBag>
            {
                Settings = GetCustomSettingsBag(),
                Options = GetCustomSettingsOptions(),
                SecurityGrantToken = GetSecurityGrantToken()
            } );
        }

        /// <summary>
        /// Saves the updates to the custom settings values for this block.
        /// </summary>
        /// <param name="box">The box that contains the setting values.</param>
        /// <returns>A response that indicates whether the save was successful.</returns>
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

                var enabledModels = box.Settings.EnabledModels ?? new List<string>();
                block.SetAttributeValue( AttributeKey.EnabledModels, string.Join( ",", enabledModels ) );

                var errorActionResult = box.IfValidProperty( nameof( box.Settings.ResultsPerPage ), () =>
                {
                    if ( !box.Settings.ResultsPerPage.HasValue || box.Settings.ResultsPerPage.Value < 1 )
                    {
                        return ActionBadRequest( "Results Per Page must be at least 1." );
                    }

                    block.SetAttributeValue( AttributeKey.ResultsPerPage, box.Settings.ResultsPerPage.Value.ToString() );
                    return null;
                }, null );

                if ( errorActionResult != null )
                {
                    return errorActionResult;
                }

                errorActionResult = box.IfValidProperty( nameof( box.Settings.SearchType ), () =>
                {
                    var searchTypeValue = box.Settings.SearchType.AsIntegerOrNull();

                    if ( !searchTypeValue.HasValue || !Enum.IsDefined( typeof( SearchType ), searchTypeValue.Value ) )
                    {
                        return ActionBadRequest( "Search Type is invalid." );
                    }

                    block.SetAttributeValue( AttributeKey.SearchType, box.Settings.SearchType );
                    return null;
                }, null );

                if ( errorActionResult != null )
                {
                    return errorActionResult;
                }

                box.IfValidProperty( nameof( box.Settings.ShowFilters ),
                    () => block.SetAttributeValue( AttributeKey.ShowFilters, box.Settings.ShowFilters.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.BaseFieldFilters ),
                    () => block.SetAttributeValue( AttributeKey.BaseFieldFilters, box.Settings.BaseFieldFilters ) );

                box.IfValidProperty( nameof( box.Settings.ShowRefinedSearch ),
                    () => block.SetAttributeValue( AttributeKey.ShowRefinedSearch, box.Settings.ShowRefinedSearch.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.ShowScores ),
                    () => block.SetAttributeValue( AttributeKey.ShowScores, box.Settings.ShowScores.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.UseCustomResults ),
                    () => block.SetAttributeValue( AttributeKey.UseCustomResults, box.Settings.UseCustomResults.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.LavaResultTemplate ),
                    () => block.SetAttributeValue( AttributeKey.LavaResultTemplate, box.Settings.LavaResultTemplate ) );

                box.IfValidProperty( nameof( box.Settings.CustomResultsCommands ),
                    () => block.SetAttributeValue( AttributeKey.CustomResultsCommands, string.Join( ",", box.Settings.CustomResultsCommands ?? new List<string>() ) ) );

                box.IfValidProperty( nameof( box.Settings.PreHtml ),
                    () => block.SetAttributeValue( AttributeKey.PreHtml, box.Settings.PreHtml ) );

                box.IfValidProperty( nameof( box.Settings.PostHtml ),
                    () => block.SetAttributeValue( AttributeKey.PostHtml, box.Settings.PostHtml ) );

                block.SaveAttributeValues( rockContext );

                return ActionOk();
            }
        }

        /// <summary>
        /// Performs a Universal Search query and returns the rendered results.
        /// </summary>
        /// <param name="bag">The search request information.</param>
        /// <returns>A bag containing the rendered results.</returns>
        [BlockAction]
        public BlockActionResult Search( UniversalSearchRequestBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "Search request was not provided." );
            }

            return ActionOk( ExecuteSearch( bag ) );
        }

        #endregion Block Actions

    }
}