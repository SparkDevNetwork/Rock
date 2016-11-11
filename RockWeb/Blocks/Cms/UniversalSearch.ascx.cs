// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    [EnumField("Search Type", "The type of search to perform.", typeof(SearchType), true, "2", category: "CustomSetting" )]
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

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
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
                // load indexable entity list
                var entities = EntityTypeCache.All();

                var indexableEntities = entities.Where( i => i.IsIndexingSupported == true ).ToList();

                cblEnabledModels.DataTextField = "FriendlyName";
                cblEnabledModels.DataValueField = "Id";
                cblEnabledModels.DataSource = indexableEntities;
                cblEnabledModels.DataBind();

                // load indexable group types
                if (entities.Any( t => t.Guid == Rock.SystemGuid.EntityType.GROUP.AsGuid() ) )
                {
                    var indexableGroupTypes = GroupTypeCache.All().AsQueryable().Where( g => g.IsIndexEnabled == true ).ToList();

                    if (indexableGroupTypes.Count != 0 )
                    {
                        cblGroupTypes.DataTextField = "Name";
                        cblGroupTypes.DataValueField = "Name";
                        cblGroupTypes.DataSource = indexableGroupTypes;
                        cblGroupTypes.DataBind();
                    }
                    else
                    {
                        cblGroupTypes.Visible = false;
                    }
                }
                else
                {
                    cblGroupTypes.Visible = false;
                }

                // load content channels
                if ( entities.Any( t => t.Guid == Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM.AsGuid()) )
                {
                    var indexableChannels = new ContentChannelService( new RockContext() ).Queryable().Where( c => c.IsIndexEnabled == true ).ToList();

                    if (indexableChannels.Count != 0 )
                    {
                        cblContentChannelTypes.DataTextField = "Name";
                        cblContentChannelTypes.DataValueField = "Name";
                        cblContentChannelTypes.DataSource = indexableChannels;
                        cblContentChannelTypes.DataBind();
                    }

                }
                else
                {
                    cblContentChannelTypes.Visible = false;
                }

                ConfigureSettings();

                if ( !string.IsNullOrWhiteSpace(PageParameter( "Q" ) ) )
                {
                    Search();
                    tbSearch.Text = PageParameter( "Q" );
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

            SaveAttributeValues();

            ConfigureSettings();

            mdEdit.Hide();
            pnlEditModal.Visible = false;
            upnlContent.Update();
        }

        /// <summary>
        /// Handles the Click event of the btnSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSearch_Click( object sender, EventArgs e )
        {
            Response.Redirect( BuildUrl() );
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
                lbRefineSearch.Text = "Hide Advanced Options";
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

            // configure models/entities
            List<int> entities = new List<int>();

            if ( entities.Count == 0 )
            {
                entities = cblModelFilter.SelectedValuesAsInt;
            }

            List<FieldValue> fieldValues = new List<FieldValue>();

            if ( cblGroupTypes.SelectedValues.Count > 0 )
            {
                foreach(string groupType in cblGroupTypes.SelectedValues )
                {
                    fieldValues.Add( new FieldValue {  Field = "groupTypeName", Value = groupType } );
                }
            }

            if ( cblContentChannelTypes.SelectedValues.Count > 0 )
            {
                foreach ( string channel in cblContentChannelTypes.SelectedValues )
                {
                    fieldValues.Add( new FieldValue { Field = "contentChannel", Value = channel } );
                }
            }

            // if person model is selected add a filter criteria to prevent blocking by other filters
            if ( entities.Contains( 15 ) )
            {
                fieldValues.Add( new FieldValue { Field = "indexModelType", Value = "Rock.UniversalSearch.IndexModels.PersonIndex" } );
            }

            SearchFieldCriteria fieldCriteria = new SearchFieldCriteria();
            fieldCriteria.FieldValues = fieldValues;


            var client = IndexContainer.GetActiveComponent();

            

            long totalResultsAvailable = 0;

            var results = client.Search( term, searchType, entities, fieldCriteria, _itemsPerPage, _currentPageNum * _itemsPerPage, out totalResultsAvailable );

            StringBuilder formattedResults = new StringBuilder();
            formattedResults.Append( "<ul class='list-unstyled'>" );

            foreach ( var result in results)
            {
                var formattedResult = result.FormatSearchResult( CurrentPerson );

                if ( formattedResult.IsViewAllowed )
                {
                    formattedResults.Append( string.Format( "{0} <div class='pull-right'><small>{1}</small></div><hr />", formattedResult.FormattedResult, result.Score ));
                }
            }

            formattedResults.Append( "</ul>" );

            tbSearch.Text = term;

            lResults.Text = formattedResults.ToString();
        }

        /// <summary>
        /// Builds the URL.
        /// </summary>
        /// <returns></returns>
        private string BuildUrl(int pageOffset = 0)
        {
            StringBuilder url = new StringBuilder();

            var appendChar = string.Empty;

            url.Append( Request.Path + "?" );

            if (!string.IsNullOrWhiteSpace(tbSearch.Text) )
            {
                url.Append( string.Format("Q={0}", tbSearch.Text) );
                appendChar = "&";
            }

            if (cblModelFilter.SelectedValues.Count > 0 )
            {
                url.Append( string.Format( "{0}Models={1}", appendChar, string.Join(",", cblModelFilter.SelectedValues ) ) );
                appendChar = "&";
            }

            if ( cblGroupTypes.SelectedValues.Count > 0 )
            {
                url.Append( string.Format( "{0}GroupTypes={1}", appendChar, string.Join( ",", cblGroupTypes.SelectedValues ) ) );
                appendChar = "&";
            }

            if ( cblContentChannelTypes.SelectedValues.Count > 0 )
            {
                url.Append( string.Format( "{0}ContentChannels={1}", appendChar, string.Join( ",", cblContentChannelTypes.SelectedValues ) ) );
                appendChar = "&";
            }

            if (_itemsPerPage != _defaultItemsPerPage )
            {
                url.Append( string.Format( "{0}ItemsPerPage={1}", appendChar, _itemsPerPage ) );
                appendChar = "&";
            }

            if (_currentPageNum != 0 )
            {
                url.Append( string.Format( "{0}CurrentPage={1}", appendChar, _currentPageNum + pageOffset ) );
                appendChar = "&";
            }

            if( !string.IsNullOrWhiteSpace( PageParameter( "SearchType" ) ) )
            {
                url.Append( string.Format( "{0}SearchType={1}", appendChar, PageParameter( "SearchType" ) ) );
                appendChar = "&";
            }

            if ( pnlRefineSearch.Visible )
            {
                url.Append( string.Format( "{0}RefinedSearch={1}", appendChar, "True" ) );
                appendChar = "&";
            }

            return url.ToString().TrimEnd('&');
        }

        /// <summary>
        /// Configures the settings.
        /// </summary>
        private void ConfigureSettings()
        {
            // model selector
            var enabledModelIds = GetAttributeValue( "EnabledModels" ).Split( ',' ).Select( int.Parse ).ToList();

            var entities = EntityTypeCache.All();
            var indexableEntities = entities.Where( i => i.IsIndexingSupported == true &&  enabledModelIds.Contains( i.Id )).ToList();

            cblModelFilter.DataTextField = "FriendlyName";
            cblModelFilter.DataValueField = "Id";
            cblModelFilter.DataSource = indexableEntities;
            cblModelFilter.DataBind();

            cblModelFilter.Visible = GetAttributeValue( "ShowFilters" ).AsBoolean();

            // hide content channel selector if content channel items not enabled
            cblContentChannelTypes.Visible = indexableEntities.Any( e => e.Guid == Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM.AsGuid() );

            // hide group type selector if groups not enabled
            cblGroupTypes.Visible = indexableEntities.Any( e => e.Guid == Rock.SystemGuid.EntityType.GROUP.AsGuid() );

            // if only one model is selected then hide the type checkbox
            if (cblModelFilter.Items.Count == 1 )
            {
                cblModelFilter.Visible = false;
            }

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

            if ( !string.IsNullOrWhiteSpace( PageParameter( "GroupTypes" ) ) )
            {
                var queryStringGroupTypes = PageParameter( "GroupTypes" ).Split( ',' ).Select( s => s.Trim() ).ToList();

                foreach ( ListItem item in cblGroupTypes.Items )
                {
                    if ( queryStringGroupTypes.Contains( item.Value ) )
                    {
                        item.Selected = true;
                    }
                    else
                    {
                        item.Selected = false;
                    }
                }
            }

            if ( !string.IsNullOrWhiteSpace( PageParameter( "ContentChannels" ) ) )
            {
                var queryStringContentChannel = PageParameter( "ContentChannels" ).Split( ',' ).Select( s => s.Trim() ).ToList();

                foreach ( ListItem item in cblContentChannelTypes.Items )
                {
                    if ( queryStringContentChannel.Contains( item.Value ) )
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
                _currentPageNum = PageParameter( "CurrentPage" ).AsInteger();
            }

            if ( !string.IsNullOrWhiteSpace( PageParameter( "RefinedSearch" ) ) )
            {
                pnlRefineSearch.Visible = PageParameter( "RefinedSearch" ).AsBoolean();
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

            cbShowFilter.Checked = GetAttributeValue( "ShowFilters" ).AsBoolean();

            var enabledModelIds = GetAttributeValue( "EnabledModels" ).Split( ',' ).Select( int.Parse ).ToList();
            
            cblEnabledModels.SetValues( enabledModelIds );

            upnlContent.Update();
        }

        #endregion
    }
}