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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Connection.ConnectionOpportunitySearch;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Connection
{
    /// <summary>
    /// Allows users to search for a connection opportunity to join. The matching
    /// opportunities are rendered with a Lava template.
    /// </summary>
    [DisplayName( "Connection Opportunity Search" )]
    [Category( "Connection" )]
    [Description( "Allows users to search for an opportunity to join" )]

    #region Block Attributes

    [CodeEditorField( "Lava Template",
        Description = "Lava template to use to display the list of opportunities.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = @"{% include '~~/Assets/Lava/OpportunitySearch.lava' %}",
        Order = 0,
        Key = AttributeKey.LavaTemplate )]

    [BooleanField( "Enable Campus Context",
        Description = "If the page has a campus context its value will be used as a filter",
        DefaultBooleanValue = true,
        Order = 1,
        Key = AttributeKey.EnableCampusContext )]

    [BooleanField( "Set Page Title",
        Description = "Determines if the block should set the page title with the connection type name.",
        DefaultBooleanValue = false,
        Order = 2,
        Key = AttributeKey.SetPageTitle )]

    [BooleanField( "Display Name Filter",
        Description = "Display the name filter",
        DefaultBooleanValue = false,
        Order = 3,
        Key = AttributeKey.DisplayNameFilter )]

    [BooleanField( "Display Campus Filter",
        Description = "Display the campus filter",
        DefaultBooleanValue = true,
        Order = 4,
        Key = AttributeKey.DisplayCampusFilter )]

    [BooleanField( "Display Inactive Campuses",
        Description = "Include inactive campuses in the Campus Filter",
        DefaultBooleanValue = true,
        Order = 5,
        Key = AttributeKey.DisplayInactiveCampuses )]

    [DefinedValueField( "Campus Types",
        Description = "This setting filters the list of campuses by type that are displayed in the campus drop-down.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        Order = 6,
        Key = AttributeKey.CampusTypes )]

    [DefinedValueField( "Campus Statuses",
        Description = "This setting filters the list of campuses by statuses that are displayed in the campus drop-down.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        Order = 7,
        Key = AttributeKey.CampusStatuses )]

    [BooleanField( "Display Attribute Filters",
        Description = "Display the attribute filters",
        DefaultBooleanValue = true,
        Order = 8,
        Key = AttributeKey.DisplayAttributeFilters )]

    [LinkedPage( "Detail Page",
        Description = "The page used to view a connection opportunity.",
        Order = 9,
        Key = AttributeKey.DetailPage )]

    [IntegerField( "Connection Type Id",
        Description = "The Id of the connection type whose opportunities are displayed.",
        IsRequired = true,
        DefaultIntegerValue = 1,
        Order = 10,
        Key = AttributeKey.ConnectionTypeId )]

    [BooleanField( "Show Search",
        Description = "Determines if the search fields should be displayed. Sometimes listing all the options is enough.",
        DefaultBooleanValue = true,
        Order = 11,
        Key = AttributeKey.ShowSearch )]

    [TextField( "Campus Label",
        IsRequired = true,
        DefaultValue = "Campuses",
        Order = 12,
        Key = AttributeKey.CampusLabel )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "C35F3F70-9EB6-4827-A95F-63F9B7C1D0A9" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "6E9F6E6B-5425-4689-A9DB-656294FF3425" )]
    [Rock.SystemGuid.BlockTypeGuid( "C0D58DEE-D266-4AA8-8750-414A3CC26C07" )]

    public class ConnectionOpportunitySearch : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string LavaTemplate = "LavaTemplate";
            public const string EnableCampusContext = "EnableCampusContext";
            public const string SetPageTitle = "SetPageTitle";
            public const string DisplayNameFilter = "DisplayNameFilter";
            public const string DisplayCampusFilter = "DisplayCampusFilter";
            public const string DisplayInactiveCampuses = "DisplayInactiveCampuses";
            public const string DisplayAttributeFilters = "DisplayAttributeFilters";
            public const string DetailPage = "DetailPage";
            public const string ConnectionTypeId = "ConnectionTypeId";
            public const string ShowSearch = "ShowSearch";
            public const string CampusLabel = "CampusLabel";
            public const string CampusTypes = "CampusTypes";
            public const string CampusStatuses = "CampusStatuses";
        }

        private static class PageParameterKey
        {
            public const string PageId = "PageId";
        }

        private static class PersonPreferenceKey
        {
            public const string SearchName = "search-name";
            public const string CampusGuids = "campus-guids";
            public const string AttributeFilterValues = "attribute-filter-values";
        }

        #endregion Keys

        #region Fields

        // The initialization box and the initial HTML content are built in the
        // same request, so the values they share are computed once per instance.
        private List<ListItemBag> _campusItems;
        private List<AttributeCache> _searchAttributes;
        private ConnectionOpportunitySearchBag _initialSearchBag;

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<ConnectionOpportunitySearchBag, ConnectionOpportunitySearchOptionsBag>
            {
                Options = new ConnectionOpportunitySearchOptionsBag
                {
                    IsSearchShown = GetAttributeValue( AttributeKey.ShowSearch ).AsBoolean(),
                    IsNameFilterShown = GetAttributeValue( AttributeKey.DisplayNameFilter ).AsBoolean(),
                    IsCampusFilterShown = GetAttributeValue( AttributeKey.DisplayCampusFilter ).AsBoolean(),
                    CampusLabel = GetAttributeValue( AttributeKey.CampusLabel ),
                    CampusItems = GetCampusItems(),
                    AttributeFilters = GetSearchAttributes().Select( PublicAttributeHelper.GetPublicAttributeForEdit ).ToList()
                },
                Bag = GetInitialSearchBag()
            };

            if ( GetAttributeValue( AttributeKey.SetPageTitle ).AsBoolean() )
            {
                SetSearchPageTitle( GetConnectionTypeId() );
            }

            return box;
        }

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            return RenderResults( GetInitialSearchBag() );
        }

        /// <summary>
        /// Gets the identifier of the connection type whose opportunities are searched.
        /// </summary>
        /// <returns>The connection type identifier configured on the block.</returns>
        private int GetConnectionTypeId()
        {
            return GetAttributeValue( AttributeKey.ConnectionTypeId ).AsInteger();
        }

        /// <summary>
        /// Gets the campuses available in the campus filter after applying the
        /// configured campus type and status restrictions.
        /// </summary>
        /// <returns>The campus filter items, or an empty list when the campus filter is hidden.</returns>
        private List<ListItemBag> GetCampusItems()
        {
            if ( _campusItems != null )
            {
                return _campusItems;
            }

            if ( !GetAttributeValue( AttributeKey.DisplayCampusFilter ).AsBoolean() )
            {
                _campusItems = new List<ListItemBag>();

                return _campusItems;
            }

            var campusTypeIds = GetDefinedValueIds( AttributeKey.CampusTypes );
            var campusStatusIds = GetDefinedValueIds( AttributeKey.CampusStatuses );
            var includeInactive = GetAttributeValue( AttributeKey.DisplayInactiveCampuses ).AsBoolean();

            _campusItems = CampusCache.All( includeInactive )
                .Where( c => !campusTypeIds.Any() || ( c.CampusTypeValueId.HasValue && campusTypeIds.Contains( c.CampusTypeValueId.Value ) ) )
                .Where( c => !campusStatusIds.Any() || ( c.CampusStatusValueId.HasValue && campusStatusIds.Contains( c.CampusStatusValueId.Value ) ) )
                .Select( c => new ListItemBag
                {
                    Value = c.Guid.ToString(),
                    Text = c.Name
                } )
                .ToList();

            return _campusItems;
        }

        /// <summary>
        /// Gets the identifiers of the defined values selected in a multi-value
        /// defined value block attribute.
        /// </summary>
        /// <param name="attributeKey">The key of the block attribute.</param>
        /// <returns>The selected defined value identifiers.</returns>
        private List<int> GetDefinedValueIds( string attributeKey )
        {
            return GetAttributeValues( attributeKey )
                .AsGuidOrNullList()
                .Where( g => g.HasValue )
                .Select( g => DefinedValueCache.GetId( g.Value ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();
        }

        /// <summary>
        /// Gets the searchable opportunity attributes of the connection type that
        /// can be displayed as filters.
        /// </summary>
        /// <returns>The attributes to filter by, or an empty list when attribute filters are hidden.</returns>
        private List<AttributeCache> GetSearchAttributes()
        {
            if ( _searchAttributes != null )
            {
                return _searchAttributes;
            }

            var connectionTypeId = GetConnectionTypeId();

            if ( !GetAttributeValue( AttributeKey.DisplayAttributeFilters ).AsBoolean() || ConnectionTypeCache.Get( connectionTypeId ) == null )
            {
                _searchAttributes = new List<AttributeCache>();

                return _searchAttributes;
            }

            var entityTypeId = EntityTypeCache.Get<ConnectionOpportunity>().Id;

            _searchAttributes = AttributeCache.GetByEntityTypeQualifier( entityTypeId, nameof( ConnectionOpportunity.ConnectionTypeId ), connectionTypeId.ToString(), false )
                .Where( a => a.AllowSearch && a.FieldType?.Field?.HasFilterControl() == true )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();

            return _searchAttributes;
        }

        /// <summary>
        /// Gets the filter selections to show when the block first loads. These are
        /// the selections remembered from the previous search, with the campus
        /// context taking precedence over any remembered campus selection.
        /// </summary>
        /// <returns>The initial filter selections.</returns>
        private ConnectionOpportunitySearchBag GetInitialSearchBag()
        {
            if ( _initialSearchBag != null )
            {
                return _initialSearchBag;
            }

            var preferences = GetBlockPersonPreferences();

            // Only selections that can still be displayed are restored so hidden
            // choices never filter the results.
            var availableCampusGuids = GetAvailableCampusGuids();
            var campusGuids = preferences.GetValue( PersonPreferenceKey.CampusGuids )
                .SplitDelimitedValues()
                .AsGuidList()
                .Where( g => availableCampusGuids.Contains( g ) )
                .ToList();

            if ( GetAttributeValue( AttributeKey.EnableCampusContext ).AsBoolean() )
            {
                var contextCampus = RequestContext.GetContextEntity<Campus>();

                if ( contextCampus != null && availableCampusGuids.Contains( contextCampus.Guid ) )
                {
                    campusGuids = new List<Guid> { contextCampus.Guid };
                }
            }

            var availableAttributeKeys = new HashSet<string>( GetSearchAttributes().Select( a => a.Key ) );
            var attributeFilterValues = preferences.GetValue( PersonPreferenceKey.AttributeFilterValues )
                .FromJsonOrNull<Dictionary<string, PublicComparisonValueBag>>()
                ?.Where( kvp => availableAttributeKeys.Contains( kvp.Key ) )
                .ToDictionary( kvp => kvp.Key, kvp => kvp.Value )
                ?? new Dictionary<string, PublicComparisonValueBag>();

            _initialSearchBag = new ConnectionOpportunitySearchBag
            {
                SearchName = preferences.GetValue( PersonPreferenceKey.SearchName ),
                CampusGuids = campusGuids,
                AttributeFilterValues = attributeFilterValues
            };

            return _initialSearchBag;
        }

        /// <summary>
        /// Gets the unique identifiers of the campuses offered in the campus filter.
        /// </summary>
        /// <returns>The available campus unique identifiers.</returns>
        private HashSet<Guid> GetAvailableCampusGuids()
        {
            return new HashSet<Guid>( GetCampusItems().Select( c => c.Value.AsGuid() ) );
        }

        /// <summary>
        /// Remembers the filter selections so they can be restored the next time
        /// the block is loaded.
        /// </summary>
        /// <param name="searchBag">The filter selections to remember.</param>
        private void SaveSearchPreferences( ConnectionOpportunitySearchBag searchBag )
        {
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( PersonPreferenceKey.SearchName, searchBag.SearchName ?? string.Empty );
            preferences.SetValue( PersonPreferenceKey.CampusGuids, ( searchBag.CampusGuids ?? new List<Guid>() ).AsDelimited( "|" ) );
            preferences.SetValue( PersonPreferenceKey.AttributeFilterValues, ( searchBag.AttributeFilterValues ?? new Dictionary<string, PublicComparisonValueBag>() ).ToJson() );
            preferences.Save();
        }

        /// <summary>
        /// Renders the configured Lava template with the opportunities that match
        /// the filter selections.
        /// </summary>
        /// <param name="searchBag">The filter selections.</param>
        /// <returns>The rendered HTML.</returns>
        private string RenderResults( ConnectionOpportunitySearchBag searchBag )
        {
            var connectionTypeId = GetConnectionTypeId();
            var connectionOpportunityService = new ConnectionOpportunityService( RockContext );

            /*
                8/27/26 - MSE

                This query is intentionally left tracked. The opportunities are
                handed to an administrator supplied Lava template, which is free to
                walk navigation properties such as ConnectionOpportunityCampuses or
                ConnectionOpportunityGroups. Adding AsNoTracking() detaches the
                entities, so those navigation properties silently resolve to null or
                an empty collection instead of lazy loading the way they did in the
                WebForms block.

                The connection type is still included up front because the
                authorization check below inherits from it for every opportunity.

                Reason: Keep lazy loading available to the configurable Lava template.
            */
            var qry = connectionOpportunityService.Queryable()
                .Include( o => o.ConnectionType )
                .Where( o => o.ConnectionTypeId == connectionTypeId && o.IsActive && o.ConnectionType.IsActive );

            qry = ApplyNameFilter( qry, searchBag );
            qry = ApplyCampusFilter( qry, searchBag );
            qry = ApplyAttributeFilters( qry, searchBag, connectionOpportunityService );

            var opportunities = qry
                .ToList()
                .Where( o => o.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .OrderBy( o => o.Order )
                .ThenBy( o => o.Name )
                .ToList();

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "CampusContext", RequestContext.GetContextEntity<Campus>() );
            mergeFields.Add( "DetailPage", GetDetailPageUrl() );

            // Resolve any Lava embedded in the summary and description before
            // exposing the opportunities to the template.
            foreach ( var opportunity in opportunities )
            {
                opportunity.Summary = opportunity.Summary.ResolveMergeFields( mergeFields );
                opportunity.Description = opportunity.Description.ResolveMergeFields( mergeFields );
            }

            mergeFields.Add( "Opportunities", opportunities );

            return GetAttributeValue( AttributeKey.LavaTemplate ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Limits the opportunities to those whose name or public name matches any
        /// of the entered search terms.
        /// </summary>
        /// <param name="qry">The opportunity query.</param>
        /// <param name="searchBag">The filter selections.</param>
        /// <returns>The filtered query.</returns>
        private IQueryable<ConnectionOpportunity> ApplyNameFilter( IQueryable<ConnectionOpportunity> qry, ConnectionOpportunitySearchBag searchBag )
        {
            if ( !GetAttributeValue( AttributeKey.DisplayNameFilter ).AsBoolean() || searchBag.SearchName.IsNullOrWhiteSpace() )
            {
                return qry;
            }

            var searchTerms = searchBag.SearchName.ToLower().SplitDelimitedValues( true );

            // A term matches when it is found in the name or when the name is found
            // in the term. The WebForms block matched Name only; PublicName was added
            // on purpose because it is the name the templates display to the searcher.
            return qry.Where( o => searchTerms.Any( t => o.Name.ToLower().Contains( t )
                || t.Contains( o.Name.ToLower() )
                || o.PublicName.ToLower().Contains( t )
                || t.Contains( o.PublicName.ToLower() ) ) );
        }

        /// <summary>
        /// Limits the opportunities to those offered at any of the selected campuses.
        /// </summary>
        /// <param name="qry">The opportunity query.</param>
        /// <param name="searchBag">The filter selections.</param>
        /// <returns>The filtered query.</returns>
        private IQueryable<ConnectionOpportunity> ApplyCampusFilter( IQueryable<ConnectionOpportunity> qry, ConnectionOpportunitySearchBag searchBag )
        {
            if ( !GetAttributeValue( AttributeKey.DisplayCampusFilter ).AsBoolean() || searchBag.CampusGuids == null || !searchBag.CampusGuids.Any() )
            {
                return qry;
            }

            // Only campuses offered in the filter may be searched, just as the
            // WebForms check box list could only post the campuses it displayed.
            var availableCampusGuids = GetAvailableCampusGuids();

            var campusIds = searchBag.CampusGuids
                .Where( g => availableCampusGuids.Contains( g ) )
                .Select( g => CampusCache.Get( g )?.Id )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            if ( !campusIds.Any() )
            {
                return qry;
            }

            return qry.Where( o => o.ConnectionOpportunityCampuses.Any( c => campusIds.Contains( c.CampusId ) ) );
        }

        /// <summary>
        /// Limits the opportunities to those whose attribute values satisfy the
        /// selected attribute filters. Each field type converts its public filter
        /// value into the private filter values the expression builder expects.
        /// </summary>
        /// <param name="qry">The opportunity query.</param>
        /// <param name="searchBag">The filter selections.</param>
        /// <param name="connectionOpportunityService">The service used to build the attribute expressions.</param>
        /// <returns>The filtered query.</returns>
        private IQueryable<ConnectionOpportunity> ApplyAttributeFilters( IQueryable<ConnectionOpportunity> qry, ConnectionOpportunitySearchBag searchBag, ConnectionOpportunityService connectionOpportunityService )
        {
            if ( searchBag.AttributeFilterValues == null || !searchBag.AttributeFilterValues.Any() )
            {
                return qry;
            }

            var parameterExpression = connectionOpportunityService.ParameterExpression;

            foreach ( var attribute in GetSearchAttributes() )
            {
                if ( !searchBag.AttributeFilterValues.TryGetValue( attribute.Key, out var filterValue ) || filterValue == null )
                {
                    continue;
                }

                // Leave the comparison unset when the client did not send one so
                // checkbox-style field types keep their natural "any of" filter.
                ComparisonType? comparisonType = filterValue.ComparisonType.HasValue
                    ? ( ComparisonType ) filterValue.ComparisonType.Value
                    : ( ComparisonType? ) null;

                /*
                    8/27/26 - MSE

                    Empty values are intentionally passed through to the field type
                    rather than skipped here. AttributeFilterExpression owns the
                    empty-value rules: IsBlank and IsNotBlank filter without a value,
                    EqualTo and NotEqualTo with no value become IsBlank and IsNotBlank
                    on field types that support them, and everything else yields a
                    NoAttributeFilterExpression that is dropped below. This is the
                    same path the WebForms ApplyAttributeQueryFilter took.

                    The value is discarded for IsBlank and IsNotBlank, as WebForms
                    GetFilterValues did. The filter control hides the value input for
                    those comparisons but keeps whatever was typed before, and field
                    types that compare the string Value column (Time, Social Media
                    Account) would otherwise match that stale text instead of blanks.

                    Reason: Preserve the WebForms attribute filter semantics for blank values.
                */
                var isBlankComparison = comparisonType == ComparisonType.IsBlank
                    || comparisonType == ComparisonType.IsNotBlank;

                var publicComparisonValue = new ComparisonValue
                {
                    ComparisonType = comparisonType,
                    Value = isBlankComparison ? string.Empty : filterValue.Value
                };

                var filterValues = attribute.FieldType.Field
                    .GetPrivateFilterValue( publicComparisonValue, attribute.ConfigurationValues )
                    .FromJsonOrNull<List<string>>();

                if ( filterValues == null || !filterValues.Any() )
                {
                    continue;
                }

                var entityField = EntityHelper.GetEntityFieldForAttribute( attribute );

                if ( entityField == null )
                {
                    continue;
                }

                var expression = ExpressionHelper.GetAttributeExpression( connectionOpportunityService, parameterExpression, entityField, filterValues );

                if ( expression == null || expression is NoAttributeFilterExpression )
                {
                    continue;
                }

                qry = qry.Where( parameterExpression, expression );
            }

            return qry;
        }

        /// <summary>
        /// Builds the detail page URL, carrying the current page parameters so the
        /// detail page receives the same context this page was opened with.
        /// </summary>
        /// <returns>The detail page URL, or an empty string when no detail page is configured.</returns>
        private string GetDetailPageUrl()
        {
            var queryParams = RequestContext.GetPageParameters()
                .Where( p => !p.Key.Equals( PageParameterKey.PageId, StringComparison.OrdinalIgnoreCase ) )
                .ToDictionary( p => p.Key, p => p.Value );

            return this.GetLinkedPageUrl( AttributeKey.DetailPage, queryParams );
        }

        /// <summary>
        /// Sets the page and browser titles to the connection type name.
        /// </summary>
        /// <param name="connectionTypeId">The connection type identifier.</param>
        private void SetSearchPageTitle( int connectionTypeId )
        {
            var title = ConnectionTypeCache.Get( connectionTypeId )?.Name;

            if ( title.IsNullOrWhiteSpace() )
            {
                title = "Connection";
            }

            var siteName = PageCache?.Layout?.Site?.Name;

            RequestContext.Response.SetPageTitle( title );
            RequestContext.Response.SetBrowserTitle( siteName.IsNotNullOrWhiteSpace() ? $"{title} | {siteName}" : title );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Searches for the opportunities that match the filter selections and
        /// returns the rendered results.
        /// </summary>
        /// <param name="bag">The filter selections.</param>
        /// <returns>The HTML produced by the configured Lava template.</returns>
        [BlockAction]
        public BlockActionResult Search( ConnectionOpportunitySearchBag bag )
        {
            bag = bag ?? new ConnectionOpportunitySearchBag();

            SaveSearchPreferences( bag );

            return ActionOk( RenderResults( bag ) );
        }

        #endregion Block Actions
    }
}
