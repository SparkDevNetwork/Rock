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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Nest;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.UniversalSearch;
using Rock.Web.UI;
using Rock.Web.Cache;
using Newtonsoft.Json.Linq;
using Rock.UniversalSearch.IndexModels;
using System.Reflection;
using Rock.Web.UI.Controls;
using Rock.Web;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Universal Search" )]
    [Category( "CMS" )]
    [Description( "A block to search for all indexable entity types in Rock." )]

    [BooleanField( "Show Filters", "Toggles the display of the model filter which allows the user to select which models to search on.", true, "CustomSetting" )]
    [TextField( "Enabled Models", "The models that should be enabled for searching.", true,  category: "CustomSetting" )]
    [IntegerField("Results Per Page", "The number of results to show per page.", true, 20, category: "CustomSetting" )]
    [EnumField("Search Type", "The type of search to perform.", typeof(SearchType), true, "0", category: "CustomSetting" )]
    [TextField("Base Field Filters", "These field filters will always be enabled and will not be changeable by the individual. Uses tha same syntax as the lava command.", false, category: "CustomSetting" )]
    [BooleanField("Show Refined Search", "Determines whether the refinded search should be shown.", true, category: "CustomSetting" )]
    [BooleanField("Show Scores", "Enables the display of scores for help with debugging.", category: "CustomSetting" )]
    [CodeEditorField("Lava Result Template", "Custom Lava results template to use instead of the standard results.", CodeEditorMode.Lava, category:"CustomSetting", defaultValue: @"<ul>{% for result in Results %}
    <li><i class='fa {{ result.IconCssClass }}'></i> {{ result.DocumentName }} <small>(Score {{ result.Score }} )</small> </li>
{% endfor %}</ul>" )]
    [BooleanField("Use Custom Results", "Determines if the custom results should be displayed.", category: "CustomSetting" )]
    [LavaCommandsField("Custom Results Commands", "The custom Lava fields to allow.", category: "CustomSetting" )]
    [CodeEditorField( "Search Input Pre-HTML", "Custom Lava to place before the search input (for styling).", CodeEditorMode.Lava, category: "CustomSetting", key: "PreHtml" )]
    [CodeEditorField( "Search Input Post-HTML", "Custom Lava to place after the search input (for styling).", CodeEditorMode.Lava, category: "CustomSetting", key: "PostHtml" )]
    public partial class UniversalSearch : RockBlockCustomSettings
    {
        #region Fields

        private const int _defaultItemsPerPage = 20;

        private int _currentPageNum = 0;
        private int _itemsPerPage = _defaultItemsPerPage;

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/jquery.lazyload.min.js" ) );

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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                // check if this is a redirect from the smart search of a document
                if(PageParameter( "DocumentType" ).IsNotNullOrWhitespace() && PageParameter( "DocumentId" ).IsNotNullOrWhitespace() )
                {
                    RedirectToDocument( PageParameter( "DocumentType" ), PageParameter( "DocumentId" ) );
                }

                ConfigureSettings();

                // if this is from the smart search apply some additional configuration
                var showRefineSearch = PageParameter( "ShowRefineSearch" ).AsBoolean();
                if ( !string.IsNullOrWhiteSpace( PageParameter( "SmartSearch" ) ) )
                {
                    lbRefineSearch.Visible = showRefineSearch;
                }

                if ( !string.IsNullOrWhiteSpace(PageParameter( "Q" ) ) )
                {
                    tbSearch.Text = PageParameter( "Q" );
                    Search();
                }

                // add pre/post html
                var preHtml = GetAttributeValue( "PreHtml" );
                var postHtml = GetAttributeValue( "PostHtml" );

                if (preHtml.IsNotNullOrWhitespace() || postHtml.IsNotNullOrWhitespace() )
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                    lPreHtml.Text = preHtml.ResolveMergeFields( mergeFields );
                    lPostHtml.Text = postHtml.ResolveMergeFields( mergeFields );
                }
            }
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
            SetAttributeValue( "ShowFilters", cbShowFilter.Checked.ToString() );

            SetAttributeValue( "EnabledModels", string.Join(",", cblEnabledModels.SelectedValues ) );

            SetAttributeValue( "SearchType", ddlSearchType.SelectedValue );

            SetAttributeValue( "ResultsPerPage", tbResultsPerPage.Text );

            SetAttributeValue( "BaseFieldFilters", tbBaseFieldFilters.Text );

            SetAttributeValue( "ShowRefinedSearch", cbShowRefinedSearch.Checked.ToString() );

            SetAttributeValue( "ShowScores", cbShowScores.Checked.ToString() );

            SetAttributeValue( "UseCustomResults", cbUseCustomResults.Checked.ToString() );

            SetAttributeValue( "LavaResultTemplate", ceCustomResultsTemplate.Text );

            SetAttributeValue( "PreHtml", cePreHtml.Text );

            SetAttributeValue( "PostHtml", cePostHtml.Text );

            SetAttributeValue( "CustomResultsCommands", string.Join( ",", cblLavaCommands.SelectedValues ) );

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
            Response.Redirect( BuildUrl(0, false ) );
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
            var indexDocumentEntityType = EntityTypeCache.Read( documentType );

            var indexDocumentType = indexDocumentEntityType.GetEntityType();

            var client = IndexContainer.GetActiveComponent();

            if ( indexDocumentType != null ) {
                var document = client.GetDocumentById( indexDocumentType, documentId.AsInteger() );

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
        private void Search( )
        {
            var term = PageParameter( "Q" );

            lResults.Text = string.Empty;

            var searchType = GetAttributeValue( "SearchType" ).ConvertToEnum<SearchType>();

            // get listing of selected entities to search on
            List<int> selectedEntities = GetSearchEntities();

            // get listing of filters to apply
            List<FieldValue> fieldValues = GetFieldFilters( selectedEntities );

            SearchFieldCriteria fieldCriteria = new SearchFieldCriteria();
            fieldCriteria.FieldValues = fieldValues;
            
            var client = IndexContainer.GetActiveComponent();

            long totalResultsAvailable = 0;

            var results = client.Search( term, searchType, selectedEntities, fieldCriteria, _itemsPerPage, _currentPageNum * _itemsPerPage, out totalResultsAvailable );

            if ( GetAttributeValue( "UseCustomResults" ).AsBoolean() )
            {
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                mergeFields.Add( "Results", results );

                lResults.Text = GetAttributeValue( "LavaResultTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "CustomResultsCommands" ) );
            }
            else
            {
                StringBuilder formattedResults = new StringBuilder();
                formattedResults.Append( "<ul class='list-unstyled'>" );

                var showScores = GetAttributeValue( "ShowScores" ).AsBoolean();

                // for speed we will get the common merge fields and pass them to the formatter so it does not have to be done repeattedly in the loop (it's a bit expensive)
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

            // pageination
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
                    pagination.Append( String.Format( "<li><a href='{0}'><span><span aria-hidden='true'>&laquo;</span></span></a></li>", BuildUrl( -1 ) ) );
                }

                var paginationOffset = 5;
                var startPage = 1;
                var endPage = paginationOffset * 2;

                if ( _currentPageNum >= paginationOffset )
                {
                    startPage = _currentPageNum - paginationOffset;
                    endPage = _currentPageNum + paginationOffset;
                }

                if ( (endPage * _itemsPerPage) > totalResultsAvailable )
                {
                    endPage = (int)Math.Ceiling( (double)totalResultsAvailable / _itemsPerPage );
                }

                if ( endPage == 1 )
                {
                    pnlPagination.Visible = false;
                    return;
                }

                for ( int i = startPage; i <= endPage; i++ )
                {
                    if ( (_currentPageNum + 1) == i )
                    {
                        pagination.Append( string.Format( "<li class='active'><span>{0} </span></li>", i ) );
                    }
                    else
                    {
                        pagination.Append( string.Format( "<li><a href='{1}'><span>{0} </span></a></li>", i, BuildUrl( (i - _currentPageNum) - 1 ) ) );
                    }
                }

                // next button
                if ( _currentPageNum == endPage )
                {
                    pagination.Append( "<li class='disabled'><span><span aria-hidden='true'>&raquo;</span></span></li>" );
                }
                else
                {
                    pagination.Append( String.Format( "<li><a href='{0}'><span><span aria-hidden='true'>&raquo;</span></span></a></li>", BuildUrl( 1 ) ) );
                }

                lPagination.Text = pagination.ToString();
            }
        }

        /// <summary>
        /// Gets the search entities.
        /// </summary>
        /// <returns></returns>
        private List<int> GetSearchEntities()
        {
            List<int> selectedEntities = new List<int>();

            if ( PageParameter( "SmartSearch" ).IsNotNullOrWhitespace() )
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
                if ( selectedEntities.Count == 0 && GetAttributeValue( "EnabledModels" ).IsNotNullOrWhitespace() )
                {
                    selectedEntities = GetAttributeValue( "EnabledModels" ).Split( ',' ).Select( int.Parse ).ToList();
                }
            }

            return selectedEntities;
        }

        /// <summary>
        /// Gets the field filters.
        /// </summary>
        /// <returns></returns>
        private List<FieldValue> GetFieldFilters(List<int> selectedEntities)
        {
            List<FieldValue> fieldValues = new List<FieldValue>();

            if ( PageParameter( "SmartSearch" ).IsNotNullOrWhitespace() )
            {
                // get the field critiera
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
                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "BaseFieldFilters" ) ) )
                {
                    foreach ( var filterField in GetAttributeValue( "BaseFieldFilters" ).ToKeyValuePairList() )
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
                        var htmlContainer = (HtmlGenericContainer)control;
                        var childControls = htmlContainer.Controls;

                        foreach ( var childControl in childControls )
                        {
                            if ( childControl is RockCheckBoxList )
                            {
                                var filterControl = (RockCheckBoxList)childControl;
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
        private string BuildUrl(int pageOffset = 0, Boolean respectSmartSearch = true) // respect smart search flag allows a search that started as a smart search to be converted to a normal search via refining the search
        {
            var pageReference = new PageReference();
            pageReference.PageId = CurrentPageReference.PageId;
            pageReference.RouteId = CurrentPageReference.RouteId;

            if (!string.IsNullOrWhiteSpace(tbSearch.Text) )
            {
                pageReference.Parameters.AddOrReplace("Q", tbSearch.Text);
            }

            if ( PageParameter("SmartSearch").IsNotNullOrWhitespace() && respectSmartSearch )
            {
                pageReference.Parameters.AddOrReplace( "SmartSearch", "true" );
            }

            if (cblModelFilter.SelectedValues.Count > 0 )
            {
                pageReference.Parameters.AddOrReplace( "Models", string.Join( ",", cblModelFilter.SelectedValues ) );
            }

            // add dynamic filters
            var selectedEntities = cblModelFilter.SelectedValuesAsInt;

            // if no entities from the UI get from the block config
            if ( selectedEntities.Count == 0 && GetAttributeValue( "EnabledModels" ).IsNotNullOrWhitespace() )
            {
                selectedEntities = GetAttributeValue( "EnabledModels" ).Split( ',' ).Select( int.Parse ).ToList();
            }

            if ( selectedEntities.Count > 0 )
            {
                foreach ( var control in phFilters.Controls )
                {
                    if (control is HtmlGenericContainer )
                    {
                        var htmlContainer = (HtmlGenericContainer)control;
                        var childControls = htmlContainer.Controls;

                        foreach(var childControl in childControls )
                        {
                            if ( childControl is RockCheckBoxList )
                            {
                                var filterControl = (RockCheckBoxList)childControl;
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

            if (_currentPageNum != 0 || pageOffset != 0 )
            {
                pageReference.Parameters.AddOrReplace( "CurrentPage", ( ( _currentPageNum + 1)  + pageOffset ).ToString() );
            }

            if( !string.IsNullOrWhiteSpace( PageParameter( "SearchType" ) ) )
            {
                pageReference.Parameters.AddOrReplace( "SearchType", PageParameter( "SearchType" ) );
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
        private ModelFieldFilterConfig GetIndexFilterConfig(Type entityType )
        {
            if ( entityType != null )
            {
                object classInstance = Activator.CreateInstance( entityType, null );
                MethodInfo bulkItemsMethod = entityType.GetMethod( "GetIndexFilterConfig" );

                if ( classInstance != null && bulkItemsMethod != null )
                {
                    return (ModelFieldFilterConfig)bulkItemsMethod.Invoke( classInstance, null );
                }
            }

            return new ModelFieldFilterConfig();
        }

        /// <summary>
        /// Supportses the index field filtering.
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
                    return (bool)bulkItemsMethod.Invoke( classInstance, null );
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
            lbRefineSearch.Visible = GetAttributeValue( "ShowRefinedSearch" ).AsBoolean();

            // model selector
            var enabledModelIds = new List<int>();
            if ( GetAttributeValue( "EnabledModels" ).IsNotNullOrWhitespace() )
            {
                enabledModelIds = GetAttributeValue( "EnabledModels" ).Split( ',' ).Select( int.Parse ).ToList();
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

            cblModelFilter.Visible = GetAttributeValue( "ShowFilters" ).AsBoolean();

            // if only one model is selected then hide the type checkbox
            if (cblModelFilter.Items.Count == 1 )
            {
                cblModelFilter.Visible = false;
            }

            hrSeparator.Visible = cblModelFilter.Visible;

            ddlSearchType.BindToEnum<SearchType>();
            ddlSearchType.SelectedValue = GetAttributeValue( "SearchType" );

            // override the block setting if passed in the query string
            if ( !string.IsNullOrWhiteSpace( PageParameter( "SearchType" ) ) )
            {
                ddlSearchType.SelectedValue = PageParameter( "SearchType" );
            }

            // set setting values from query string
            if ( !string.IsNullOrWhiteSpace( PageParameter( "Models" ) ) )
            {
                var queryStringModels = PageParameter( "Models" ).Split( ',' ).Select( s => s.Trim() ).ToList();

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

            if ( !string.IsNullOrWhiteSpace( PageParameter( "ItemsPerPage" ) ) )
            {
                _itemsPerPage = PageParameter( "ItemsPerPage" ).AsInteger();
            }

            if ( !string.IsNullOrWhiteSpace( PageParameter( "CurrentPage" ) ) )
            {
                _currentPageNum = PageParameter( "CurrentPage" ).AsInteger() -1;
            }

            if ( !string.IsNullOrWhiteSpace( PageParameter( "RefinedSearch" ) ) )
            {
                pnlRefineSearch.Visible = PageParameter( "RefinedSearch" ).AsBoolean();

                if ( pnlRefineSearch.Visible )
                {
                    lbRefineSearch.Text = "Hide Refined Search";
                }
            }

            _itemsPerPage = GetAttributeValue( "ResultsPerPage" ).AsInteger();
        }

        /// <summary>
        /// Loads the custom filters.
        /// </summary>
        private void LoadCustomFilters()
        {
            var enabledModelIds = new List<int>();
            if ( GetAttributeValue( "EnabledModels" ).IsNotNullOrWhitespace() )
            {
                enabledModelIds = GetAttributeValue( "EnabledModels" ).Split( ',' ).Select( int.Parse ).ToList();
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

            if ( GetAttributeValue( "CustomResultsCommands" ) != null)
            {
                selectedCommands = GetAttributeValue( "CustomResultsCommands" ).Split( ',' ).ToList();
            }

            foreach ( var command in selectedCommands )
            {
                var item = cblLavaCommands.Items.FindByText( command );
                if ( item != null )
                {
                    item.Selected = true;
                }
            }

            cbShowFilter.Checked = GetAttributeValue( "ShowFilters" ).AsBoolean();
                 
            var enabledModelIds = new List<int>();

            if ( GetAttributeValue( "EnabledModels" ).IsNotNullOrWhitespace() )
            {
                enabledModelIds = GetAttributeValue( "EnabledModels" ).Split( ',' ).Select( int.Parse ).ToList();
            }

            var entities = EntityTypeCache.All();
            var indexableEntities = entities.Where( i => i.IsIndexingSupported == true && enabledModelIds.Contains( i.Id ) ).ToList();
            cblEnabledModels.DataValueField = "Id";
            cblEnabledModels.DataTextField = "FriendlyName";
            cblEnabledModels.DataSource = entities.Where( i => i.IsIndexingSupported == true && i.IsIndexingEnabled == true ).ToList();
            cblEnabledModels.DataBind();

            cblEnabledModels.SetValues( enabledModelIds );

            cbShowRefinedSearch.Checked = GetAttributeValue( "ShowRefinedSearch" ).AsBoolean();

            cbShowScores.Checked = GetAttributeValue( "ShowScores" ).AsBoolean();

            cbUseCustomResults.Checked = GetAttributeValue( "UseCustomResults" ).AsBoolean();
            ceCustomResultsTemplate.Text = GetAttributeValue( "LavaResultTemplate" );

            cePreHtml.Text = GetAttributeValue( "PreHtml" );
            cePostHtml.Text = GetAttributeValue( "PostHtml" );

            tbBaseFieldFilters.Text = GetAttributeValue( "BaseFieldFilters" );

            tbResultsPerPage.Text = GetAttributeValue( "ResultsPerPage" );

            upnlContent.Update();
        }

        #endregion
    }
}
 