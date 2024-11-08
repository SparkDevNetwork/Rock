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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.UniversalSearch;
using Rock.Web.UI;
using Rock.Web.Cache;
using System.Reflection;
using Rock.Web.UI.Controls;
using Rock.Web;

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "Universal Search" )]
    [Category( "CMS" )]
    [Description( "A block to search for all indexable entity types in Rock." )]

    [BooleanField( "Show Filters",
        Description = "Toggles the display of the model filter which allows the user to select which models to search on.",
        DefaultBooleanValue = true,
        Category = "CustomSetting",
        Key = AttributeKey.ShowFilters )]

    [TextField( "Enabled Models",
        Description = "The models that should be enabled for searching.",
        IsRequired = true,
        Category = "CustomSetting",
        Key = AttributeKey.EnabledModels )]

    [IntegerField( "Results Per Page",
        Description = "The number of results to show per page.",
        IsRequired = true,
        DefaultIntegerValue = 20,
        Category = "CustomSetting",
        Key = AttributeKey.ResultsPerPage )]

    [EnumField( "Search Type",
        Description = "The type of search to perform.",
        EnumSourceType = typeof( SearchType ),
        IsRequired = true,
        DefaultValue = "0",
        Category = "CustomSetting",
        Key = AttributeKey.SearchType )]

    [TextField( "Base Field Filters",
        Description = "These field filters will always be enabled and will not be changeable by the individual. Uses the same syntax as the lava command.",
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.BaseFieldFilters )]

    [BooleanField( "Show Refined Search",
        Description = "Determines whether the refined search should be shown.",
        DefaultBooleanValue = true,
        Category = "CustomSetting",
        Key = AttributeKey.ShowRefinedSearch )]

    [BooleanField( "Show Scores",
        Description = "Enables the display of scores for help with debugging.",
        Category = "CustomSetting",
        Key = AttributeKey.ShowScores )]

    [CodeEditorField( "Lava Result Template",
        Description = "Custom Lava results template to use instead of the standard results.",
        EditorMode = CodeEditorMode.Lava,
        Category = "CustomSetting",
        DefaultValue = DefaultLavaResultTemplate,
        Key = AttributeKey.LavaResultTemplate )]

    [BooleanField( "Use Custom Results",
        Description = "Determines if the custom results should be displayed.",
        Category = "CustomSetting",
        Key = AttributeKey.UseCustomResults )]

    [LavaCommandsField( "Custom Results Commands",
        Description = "The custom Lava fields to allow.",
        Category = "CustomSetting",
        Key = AttributeKey.CustomResultsCommands )]

    [CodeEditorField( "Search Input Pre-HTML",
        Description = "Custom Lava to place before the search input (for styling).",
        EditorMode = CodeEditorMode.Lava,
        Category = "CustomSetting",
        Key = AttributeKey.PreHtml )]

    [CodeEditorField( "Search Input Post-HTML",
        Description = "Custom Lava to place after the search input (for styling).",
        EditorMode = CodeEditorMode.Lava,
        Category = "CustomSetting",
        Key = AttributeKey.PostHtml )]
    [Rock.SystemGuid.BlockTypeGuid( "FDF1BBFF-7A7B-4F4E-BF34-831203B0FEAC" )]
    public partial class UniversalSearch : RockBlockCustomSettings
    {
        #region AttributeKeys
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

        #endregion AttributeKeys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string DocumentType = "DocumentType";
            public const string DocumentId = "DocumentId";
            public const string ShowRefineSearch = "ShowRefineSearch";
            public const string SmartSearch = "SmartSearch";
            public const string Q = "Q";
            public const string SearchType = "SearchType";
            public const string Models = "Models";
            public const string ItemsPerPage = "ItemsPerPage";
            public const string CurrentPage = "CurrentPage";
            public const string RefinedSearch = "RefinedSearch";
        }

        #endregion PageParameterKeys

        #region Fields

        private const int DefaultItemsPerPage = 20;

        private int _currentPageNum = 0;
        private int _itemsPerPage = DefaultItemsPerPage;
        private const string DefaultLavaResultTemplate = @"<ul>{% for result in Results %}
    <li><i class='fa {{ result.IconCssClass }}'></i> {{ result.DocumentName }} <small>(Score {{ result.Score }} )</small> </li>
{% endfor %}</ul>";

        #endregion

        #region Properties

        //// used for public / protected properties

        #endregion

        #region Base Control Methods

        // overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/jquery.lazyload.min.js" );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            LoadCustomFilters();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                // Check if this is a request for a specific document from the results list of smart search.
                // Smart search may specify either a search term or a specific document in the "Q" query parameter,
                // because the Search component [Result URL] setting is used for both types of request.
                if ( PageParameter( PageParameterKey.DocumentType ).IsNotNullOrWhiteSpace() && PageParameter( PageParameterKey.DocumentId ).IsNotNullOrWhiteSpace() )
                {
                    // First, check if the document parameters are specified as part of the request route.
                    // This is a legacy request format that should be avoided, because it fails to process search requests containing special characters such as "/".
                    RedirectToDocument( PageParameter( PageParameterKey.DocumentType ), PageParameter( PageParameterKey.DocumentId ) );
                }
                else if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.Q ) ) )
                {
                    // Parse the "Q" parameter to determine if it is a specific document reference.
                    var queryParts = PageParameter( PageParameterKey.Q ).SplitDelimitedValues("/");
                    if ( queryParts.Length == 2 )
                    { 
                        var documentType = queryParts[0];

                        var indexDocumentEntityType = EntityTypeCache.Get( documentType, createNew:false );
                        if ( indexDocumentEntityType != null )
                        {
                            var documentId = queryParts[1];
                            RedirectToDocument( documentType, documentId );
                        }
                    }
                }

                ConfigureSettings();

                // if this is from the smart search apply some additional configuration
                var showRefineSearch = PageParameter( PageParameterKey.ShowRefineSearch ).AsBoolean();
                if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.SmartSearch ) ) )
                {
                    lbRefineSearch.Visible = showRefineSearch;
                }

                if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.Q ) ) )
                {
                    tbSearch.Text = PageParameter( PageParameterKey.Q );
                    Search();
                }

                // add pre/post html
                var preHtml = GetAttributeValue( AttributeKey.PreHtml );
                var postHtml = GetAttributeValue( AttributeKey.PostHtml );

                if ( preHtml.IsNotNullOrWhiteSpace() || postHtml.IsNotNullOrWhiteSpace() )
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                    lPreHtml.Text = preHtml.ResolveMergeFields( mergeFields );
                    lPostHtml.Text = postHtml.ResolveMergeFields( mergeFields );
                }
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ConfigureSettings();
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            SetAttributeValue( AttributeKey.ShowFilters, cbShowFilter.Checked.ToString() );
            SetAttributeValue( AttributeKey.EnabledModels, string.Join( ",", cblEnabledModels.SelectedValues ) );
            SetAttributeValue( AttributeKey.SearchType, ddlSearchType.SelectedValue );
            SetAttributeValue( AttributeKey.ResultsPerPage, tbResultsPerPage.Text );
            SetAttributeValue( AttributeKey.BaseFieldFilters, tbBaseFieldFilters.Text );
            SetAttributeValue( AttributeKey.ShowRefinedSearch, cbShowRefinedSearch.Checked.ToString() );
            SetAttributeValue( AttributeKey.ShowScores, cbShowScores.Checked.ToString() );
            SetAttributeValue( AttributeKey.UseCustomResults, cbUseCustomResults.Checked.ToString() );
            SetAttributeValue( AttributeKey.LavaResultTemplate, ceCustomResultsTemplate.Text );
            SetAttributeValue( AttributeKey.PreHtml, cePreHtml.Text );
            SetAttributeValue( AttributeKey.PostHtml, cePostHtml.Text );
            SetAttributeValue( AttributeKey.CustomResultsCommands, string.Join( ",", cblLavaCommands.SelectedValues ) );

            SaveAttributeValues();

            ConfigureSettings();

            mdEdit.Hide();
            pnlEditModal.Visible = false;

            Search();

            upnlContent.Update();
        }

        /// <summary>
        /// Handles the Click event of the btnSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSearch_Click( object sender, EventArgs e )
        {
            string url = BuildUrl( 0, false );
            Response.Redirect( url, false );
            Context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Handles the Click event of the lbRefineSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRefineSearch_Click( object sender, EventArgs e )
        {
            if ( pnlRefineSearch.Visible )
            {
                pnlRefineSearch.Visible = false;
                lbRefineSearch.Text = "Refine Search";
            }
            else
            {
                pnlRefineSearch.Visible = true;
                lbRefineSearch.Text = "Hide Refined Search";
            }
        }

        /// <summary>
        /// Redirects to document.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="documentId">The document identifier.</param>
        private void RedirectToDocument( string documentType, string documentId )
        {
            var indexDocumentEntityType = EntityTypeCache.Get( documentType );

            var indexDocumentType = indexDocumentEntityType.GetEntityType();

            var client = IndexContainer.GetActiveComponent();

            if ( indexDocumentType != null )
            {
                var document = client.GetDocumentById( indexDocumentType, documentId );

                var documentUrl = document.GetDocumentUrl();

                if ( !string.IsNullOrWhiteSpace( documentUrl ) )
                {
                    Response.Redirect( documentUrl );
                }
                else
                {
                    lResults.Text = "<div class='alert alert-warning'>No url is available for the provided index document.</div>";
                }
            }
            else
            {
                lResults.Text = "<div class='alert alert-warning'>Invalid document type.</div>";
            }
        }

        /// <summary>
        /// Performs the search
        /// </summary>
        private void Search()
        {
            var term = PageParameter( PageParameterKey.Q );

            lResults.Text = string.Empty;

            var searchType = GetAttributeValue( AttributeKey.SearchType ).ConvertToEnum<SearchType>();

            // get listing of selected entities to search on
            List<int> selectedEntities = GetSearchEntities();

            // get listing of filters to apply
            List<FieldValue> fieldValues = GetFieldFilters( selectedEntities );

            SearchFieldCriteria fieldCriteria = new SearchFieldCriteria();
            fieldCriteria.FieldValues = fieldValues;

            var client = IndexContainer.GetActiveComponent();

            if ( client == null )
            {
                nbWarnings.Text = "No indexing service is currently configured.";
            }
            else
            {
                long totalResultsAvailable = 0;

                List<Rock.UniversalSearch.IndexModels.IndexModelBase> results;

                try
                {

                    results = client.Search( term, searchType, selectedEntities, fieldCriteria, _itemsPerPage, _currentPageNum * _itemsPerPage, out totalResultsAvailable );
                }
                catch ( Exception ex )
                {
                    nbWarnings.Text = "An Error Occurred";
                    nbWarnings.Details = ex.Message;
                    return;
                }

                if ( GetAttributeValue( AttributeKey.UseCustomResults ).AsBoolean() )
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                    mergeFields.Add( "Results", results );

                    lResults.Text = GetAttributeValue( AttributeKey.LavaResultTemplate ).ResolveMergeFields( mergeFields, GetAttributeValue( AttributeKey.CustomResultsCommands ) );
                }
                else
                {
                    StringBuilder formattedResults = new StringBuilder();
                    formattedResults.Append( "<ul class='list-unstyled'>" );

                    var showScores = GetAttributeValue( AttributeKey.ShowScores ).AsBoolean();

                    // for speed we will get the common merge fields and pass them to the formatter so it does not have to be done repeatedly in the loop (it's a bit expensive)
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, CurrentPerson );

                    foreach ( var result in results )
                    {
                        var formattedResult = result.FormatSearchResult( CurrentPerson, null, mergeFields );

                        if ( formattedResult.IsViewAllowed )
                        {
                            formattedResults.Append( string.Format( "{0}", formattedResult.FormattedResult ) );

                            if ( showScores )
                            {
                                formattedResults.Append( string.Format( "<div class='pull-right'><small>{0}</small></div>", result.Score ) );
                            }

                            formattedResults.Append( "<hr />" );
                        }
                    }

                    formattedResults.Append( "</ul>" );

                    tbSearch.Text = term;
                    lResults.Text = formattedResults.ToString();
                }

                // pagination
                if ( totalResultsAvailable > 0 )
                {
                    StringBuilder pagination = new StringBuilder();

                    // previous button
                    if ( _currentPageNum == 0 )
                    {
                        pagination.Append( "<li class='disabled'><span><span aria-hidden='true'>&laquo;</span></span></li>" );
                    }
                    else
                    {
                        pagination.Append( string.Format( "<li><a href='{0}'><span><span aria-hidden='true'>&laquo;</span></span></a></li>", BuildUrl( -1 ) ) );
                    }

                    var paginationOffset = 5;
                    var startPage = 1;
                    var endPage = paginationOffset * 2;

                    if ( _currentPageNum >= paginationOffset )
                    {
                        startPage = _currentPageNum - paginationOffset;
                        endPage = _currentPageNum + paginationOffset;
                    }

                    if ( ( endPage * _itemsPerPage ) > totalResultsAvailable )
                    {
                        endPage = ( int ) Math.Ceiling( ( double ) totalResultsAvailable / _itemsPerPage );
                    }

                    if ( endPage == 1 )
                    {
                        pnlPagination.Visible = false;
                        return;
                    }

                    for ( int i = startPage; i <= endPage; i++ )
                    {
                        if ( ( _currentPageNum + 1 ) == i )
                        {
                            pagination.Append( string.Format( "<li class='active'><span>{0} </span></li>", i ) );
                        }
                        else
                        {
                            pagination.Append( string.Format( "<li><a href='{1}'><span>{0} </span></a></li>", i, BuildUrl( ( i - _currentPageNum ) - 1 ) ) );
                        }
                    }

                    // next button
                    if ( _currentPageNum == endPage )
                    {
                        pagination.Append( "<li class='disabled'><span><span aria-hidden='true'>&raquo;</span></span></li>" );
                    }
                    else
                    {
                        pagination.Append( string.Format( "<li><a href='{0}'><span><span aria-hidden='true'>&raquo;</span></span></a></li>", BuildUrl( 1 ) ) );
                    }

                    lPagination.Text = pagination.ToString();
                }
            }
        }

        /// <summary>
        /// Gets the search entities.
        /// </summary>
        /// <returns></returns>
        private List<int> GetSearchEntities()
        {
            List<int> selectedEntities = new List<int>();

            if ( PageParameter( PageParameterKey.SmartSearch ).IsNotNullOrWhiteSpace() )
            {
                // get entities from smart search config
                var searchEntitiesSetting = Rock.Web.SystemSettings.GetValue( "core_SmartSearchUniversalSearchEntities" );

                if ( !string.IsNullOrWhiteSpace( searchEntitiesSetting ) )
                {
                    selectedEntities = searchEntitiesSetting.Split( ',' ).Select( int.Parse ).ToList();
                }
            }
            else
            {
                selectedEntities = cblModelFilter.SelectedValuesAsInt;

                // if no entities from the UI get from the block config
                if ( selectedEntities.Count == 0 && GetAttributeValue( AttributeKey.EnabledModels ).IsNotNullOrWhiteSpace() )
                {
                    selectedEntities = GetAttributeValue( AttributeKey.EnabledModels ).Split( ',' ).Select( int.Parse ).ToList();
                }
            }

            return selectedEntities;
        }

        /// <summary>
        /// Gets the field filters.
        /// </summary>
        /// <returns></returns>
        private List<FieldValue> GetFieldFilters( List<int> selectedEntities )
        {
            List<FieldValue> fieldValues = new List<FieldValue>();

            if ( PageParameter( PageParameterKey.SmartSearch ).IsNotNullOrWhiteSpace() )
            {
                // get the field criteria
                var fieldCriteriaSetting = Rock.Web.SystemSettings.GetValue( "core_SmartSearchUniversalSearchFieldCriteria" );

                if ( !string.IsNullOrWhiteSpace( fieldCriteriaSetting ) )
                {
                    foreach ( var queryString in fieldCriteriaSetting.ToKeyValuePairList() )
                    {
                        // check that multiple values were not passed
                        var values = queryString.Value.ToString().Split( ',' );

                        foreach ( var value in values )
                        {
                            fieldValues.Add( new FieldValue { Field = queryString.Key, Value = value } );
                        }
                    }
                }
            }
            else
            {
                // add any base field filters from block settings
                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.BaseFieldFilters ) ) )
                {
                    foreach ( var filterField in GetAttributeValue( AttributeKey.BaseFieldFilters ).ToKeyValuePairList() )
                    {
                        // check that multiple values were not passed as a comma separated string
                        var values = filterField.Value.ToString().Split( ',' );

                        foreach ( var value in values )
                        {
                            fieldValues.Add( new FieldValue { Field = filterField.Key, Value = value } );
                        }
                    }
                }

                // get dynamic filters
                foreach ( var control in phFilters.Controls )
                {
                    if ( control is HtmlGenericContainer )
                    {
                        var htmlContainer = ( HtmlGenericContainer ) control;
                        var childControls = htmlContainer.Controls;

                        foreach ( var childControl in childControls )
                        {
                            if ( childControl is RockCheckBoxList )
                            {
                                var filterControl = ( RockCheckBoxList ) childControl;
                                var entityId = filterControl.Attributes["entity-id"].AsInteger();
                                var fieldName = filterControl.Attributes["entity-filter-field"];

                                if ( selectedEntities.Contains( entityId ) )
                                {
                                    foreach ( var selectedItem in filterControl.SelectedValues )
                                    {
                                        fieldValues.Add( new FieldValue { Field = fieldName, Value = selectedItem } );
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // check for the existence of field criteria. if any exist check the entity list for models that do not support field criteria and
            // add a filter for modelconfig with 'nofilters'. This keeps them from being excluded
            if ( fieldValues.Count > 0 )
            {
                fieldValues.Add( new FieldValue() { Field = "modelConfiguration", Value = "nofilters", Boost = 3 } );
            }

            return fieldValues;
        }

        /// <summary>
        /// Builds the URL.
        /// </summary>
        /// <returns></returns>
        private string BuildUrl( int pageOffset = 0, bool respectSmartSearch = true ) // respect smart search flag allows a search that started as a smart search to be converted to a normal search via refining the search
        {
            var pageReference = new PageReference();
            pageReference.PageId = CurrentPageReference.PageId;
            pageReference.RouteId = CurrentPageReference.RouteId;

            if ( !string.IsNullOrWhiteSpace( tbSearch.Text ) )
            {
                pageReference.Parameters.AddOrReplace( "Q", tbSearch.Text );
            }

            if ( PageParameter( PageParameterKey.SmartSearch ).IsNotNullOrWhiteSpace() && respectSmartSearch )
            {
                pageReference.Parameters.AddOrReplace( "SmartSearch", "true" );
            }

            if ( cblModelFilter.SelectedValues.Count > 0 )
            {
                pageReference.Parameters.AddOrReplace( "Models", string.Join( ",", cblModelFilter.SelectedValues ) );
            }

            // add dynamic filters
            var selectedEntities = cblModelFilter.SelectedValuesAsInt;

            // if no entities from the UI get from the block config
            if ( selectedEntities.Count == 0 && GetAttributeValue( AttributeKey.EnabledModels ).IsNotNullOrWhiteSpace() )
            {
                selectedEntities = GetAttributeValue( AttributeKey.EnabledModels ).Split( ',' ).Select( int.Parse ).ToList();
            }

            if ( selectedEntities.Count > 0 )
            {
                foreach ( var control in phFilters.Controls )
                {
                    if ( control is HtmlGenericContainer )
                    {
                        var htmlContainer = ( HtmlGenericContainer ) control;
                        var childControls = htmlContainer.Controls;

                        foreach ( var childControl in childControls )
                        {
                            if ( childControl is RockCheckBoxList )
                            {
                                var filterControl = ( RockCheckBoxList ) childControl;
                                var entityId = filterControl.Attributes["entity-id"].AsInteger();
                                var fieldName = filterControl.Attributes["entity-filter-field"];

                                if ( selectedEntities.Contains( entityId ) )
                                {
                                    pageReference.Parameters.AddOrReplace( fieldName, string.Join( ",", filterControl.SelectedValues ) );
                                }
                            }
                        }
                    }
                }
            }

            if ( _currentPageNum != 0 || pageOffset != 0 )
            {
                pageReference.Parameters.AddOrReplace( "CurrentPage", ( ( _currentPageNum + 1 ) + pageOffset ).ToString() );
            }

            if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.SearchType ) ) )
            {
                pageReference.Parameters.AddOrReplace( "SearchType", PageParameter( PageParameterKey.SearchType ) );
            }

            if ( pnlRefineSearch.Visible )
            {
                pageReference.Parameters.AddOrReplace( "RefinedSearch", "True" );
            }

            return pageReference.BuildUrl();
        }

        /// <summary>
        /// Gets the index filter configuration.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        private ModelFieldFilterConfig GetIndexFilterConfig( Type entityType )
        {
            if ( entityType != null )
            {
                object classInstance = Activator.CreateInstance( entityType, null );
                MethodInfo bulkItemsMethod = entityType.GetMethod( "GetIndexFilterConfig" );

                if ( classInstance != null && bulkItemsMethod != null )
                {
                    return ( ModelFieldFilterConfig ) bulkItemsMethod.Invoke( classInstance, null );
                }
            }

            return new ModelFieldFilterConfig();
        }

        /// <summary>
        /// Supports the index field filtering.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        private bool SupportsIndexFieldFiltering( Type entityType )
        {
            if ( entityType != null )
            {
                object classInstance = Activator.CreateInstance( entityType, null );
                MethodInfo bulkItemsMethod = entityType.GetMethod( "SupportsIndexFieldFiltering" );

                if ( classInstance != null && bulkItemsMethod != null )
                {
                    return ( bool ) bulkItemsMethod.Invoke( classInstance, null );
                }
            }

            return false;
        }

        /// <summary>
        /// Configures the settings.
        /// </summary>
        private void ConfigureSettings()
        {
            // toggle refine search view toggle button
            lbRefineSearch.Visible = GetAttributeValue( AttributeKey.ShowRefinedSearch ).AsBoolean();

            // model selector
            var enabledModelIds = new List<int>();
            if ( GetAttributeValue( AttributeKey.EnabledModels ).IsNotNullOrWhiteSpace() )
            {
                enabledModelIds = GetAttributeValue( AttributeKey.EnabledModels ).Split( ',' ).Select( int.Parse ).ToList();
            }

            var entities = EntityTypeCache.All();
            var indexableEntities = entities.Where( i => i.IsIndexingEnabled == true ).ToList();

            // if enabled entities setting is set further filter by those
            if ( enabledModelIds.Count > 0 )
            {
                indexableEntities = indexableEntities.Where( i => enabledModelIds.Contains( i.Id ) ).ToList();
            }

            cblModelFilter.DataTextField = "FriendlyName";
            cblModelFilter.DataValueField = "Id";
            cblModelFilter.DataSource = indexableEntities;
            cblModelFilter.DataBind();

            cblModelFilter.Visible = GetAttributeValue( AttributeKey.ShowFilters ).AsBoolean();

            // if only one model is selected then hide the type checkbox
            if ( cblModelFilter.Items.Count == 1 )
            {
                cblModelFilter.Visible = false;
            }

            hrSeparator.Visible = cblModelFilter.Visible;

            ddlSearchType.BindToEnum<SearchType>();
            ddlSearchType.SelectedValue = GetAttributeValue( AttributeKey.SearchType );

            // override the block setting if passed in the query string
            if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.SearchType ) ) )
            {
                ddlSearchType.SelectedValue = PageParameter( PageParameterKey.SearchType );
            }

            // set setting values from query string
            if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.Models ) ) )
            {
                var queryStringModels = PageParameter( PageParameterKey.Models ).Split( ',' ).Select( s => s.Trim() ).ToList();

                foreach ( ListItem item in cblModelFilter.Items )
                {
                    if ( queryStringModels.Contains( item.Value ) )
                    {
                        item.Selected = true;
                    }
                    else
                    {
                        item.Selected = false;
                    }
                }
            }

            if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.ItemsPerPage ) ) )
            {
                _itemsPerPage = PageParameter( PageParameterKey.ItemsPerPage ).AsInteger();
            }

            if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.CurrentPage ) ) )
            {
                _currentPageNum = PageParameter( PageParameterKey.CurrentPage ).AsInteger() - 1;
            }

            if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.RefinedSearch ) ) )
            {
                pnlRefineSearch.Visible = PageParameter( PageParameterKey.RefinedSearch ).AsBoolean();

                if ( pnlRefineSearch.Visible )
                {
                    lbRefineSearch.Text = "Hide Refined Search";
                }
            }

            _itemsPerPage = GetAttributeValue( AttributeKey.ResultsPerPage ).AsInteger();
        }

        /// <summary>
        /// Loads the custom filters.
        /// </summary>
        private void LoadCustomFilters()
        {
            var enabledModelIds = new List<int>();
            if ( GetAttributeValue( AttributeKey.EnabledModels ).IsNotNullOrWhiteSpace() )
            {
                enabledModelIds = GetAttributeValue( AttributeKey.EnabledModels ).Split( ',' ).Select( int.Parse ).ToList();
            }

            var entities = EntityTypeCache.All();
            var indexableEntities = entities.Where( i => i.IsIndexingEnabled == true ).ToList();

            // if select entities are configured further filter by them
            if ( enabledModelIds.Count > 0 )
            {
                indexableEntities = indexableEntities.Where( i => enabledModelIds.Contains( i.Id ) ).ToList();
            }

            foreach ( var entity in indexableEntities )
            {
                var entityType = entity.GetEntityType();

                if ( SupportsIndexFieldFiltering( entityType ) )
                {
                    var filterOptions = GetIndexFilterConfig( entityType );

                    RockCheckBoxList filterConfig = new RockCheckBoxList();
                    filterConfig.Label = filterOptions.FilterLabel;
                    filterConfig.CssClass = "js-entity-id-" + entity.Id.ToString();
                    filterConfig.CssClass += " js-entity-filter-field";
                    filterConfig.RepeatDirection = RepeatDirection.Horizontal;
                    filterConfig.Attributes.Add( "entity-id", entity.Id.ToString() );
                    filterConfig.Attributes.Add( "entity-filter-field", filterOptions.FilterField );
                    filterConfig.DataSource = filterOptions.FilterValues.Where( i => i != null );
                    filterConfig.DataBind();

                    // set any selected values from the query string
                    if ( !string.IsNullOrWhiteSpace( PageParameter( filterOptions.FilterField ) ) )
                    {
                        List<string> selectedValues = PageParameter( filterOptions.FilterField ).Split( ',' ).ToList();

                        foreach ( ListItem item in filterConfig.Items )
                        {
                            if ( selectedValues.Contains( item.Value ) )
                            {
                                item.Selected = true;
                            }
                        }
                    }

                    if ( filterOptions.FilterValues.Count > 0 )
                    {
                        HtmlGenericContainer filterWrapper = new HtmlGenericContainer( "div", "col-md-6" );
                        filterWrapper.Controls.Add( filterConfig );
                        phFilters.Controls.Add( filterWrapper );
                    }
                }
            }
        }

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            pnlEditModal.Visible = true;
            upnlContent.Update();
            mdEdit.Show();

            cblLavaCommands.Items.Clear();
            cblLavaCommands.Items.Add( "All" );

            // load the lava commands control
            foreach ( var command in Rock.Lava.LavaHelper.GetLavaCommands() )
            {
                cblLavaCommands.Items.Add( command );
            }

            List<string> selectedCommands = new List<string>();

            if ( GetAttributeValue( AttributeKey.CustomResultsCommands ) != null )
            {
                selectedCommands = GetAttributeValue( AttributeKey.CustomResultsCommands ).Split( ',' ).ToList();
            }

            foreach ( var command in selectedCommands )
            {
                var item = cblLavaCommands.Items.FindByText( command );
                if ( item != null )
                {
                    item.Selected = true;
                }
            }

            cbShowFilter.Checked = GetAttributeValue( AttributeKey.ShowFilters ).AsBoolean();

            var enabledModelIds = new List<int>();
            if ( GetAttributeValue( AttributeKey.EnabledModels ).IsNotNullOrWhiteSpace() )
            {
                enabledModelIds = GetAttributeValue( AttributeKey.EnabledModels ).Split( ',' ).Select( int.Parse ).ToList();
            }

            var entities = EntityTypeCache.All();
            var indexableEntities = entities.Where( i => i.IsIndexingSupported == true && enabledModelIds.Contains( i.Id ) ).ToList();

            cblEnabledModels.DataValueField = "Id";
            cblEnabledModels.DataTextField = "FriendlyName";
            cblEnabledModels.DataSource = entities.Where( i => i.IsIndexingSupported == true && i.IsIndexingEnabled == true ).ToList();
            cblEnabledModels.DataBind();
            cblEnabledModels.SetValues( enabledModelIds );

            cbShowRefinedSearch.Checked = GetAttributeValue( AttributeKey.ShowRefinedSearch ).AsBoolean();
            cbShowScores.Checked = GetAttributeValue( AttributeKey.ShowScores ).AsBoolean();
            cbUseCustomResults.Checked = GetAttributeValue( AttributeKey.UseCustomResults ).AsBoolean();
            ceCustomResultsTemplate.Text = GetAttributeValue( AttributeKey.LavaResultTemplate );
            cePreHtml.Text = GetAttributeValue( AttributeKey.PreHtml );
            cePostHtml.Text = GetAttributeValue( AttributeKey.PostHtml );
            tbBaseFieldFilters.Text = GetAttributeValue( AttributeKey.BaseFieldFilters );
            tbResultsPerPage.Text = GetAttributeValue( AttributeKey.ResultsPerPage );

            upnlContent.Update();
        }

        #endregion
    }
}