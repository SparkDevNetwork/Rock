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

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Universal Search" )]
    [Category( "CMS" )]
    [Description( "A block to search for all indexable entity types in Rock." )]

    [BooleanField( "Show Model Filter", "Toggles the display of the model filter which allows the user to select which models to search on.", true, "CustomSetting" )]
    public partial class UniversalSearch : RockBlockCustomSettings
    {
        #region Fields

        // used for private variables

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

            var sm = ScriptManager.GetCurrent( Page );
            sm.Navigate += sm_Navigate;

            if ( !Page.IsPostBack )
            {
                // load indexable entity list
                var entities = EntityTypeCache.All();

                var indexableEntities = entities.Where( i => i.IsIndexingSupported == true ).ToList();

                cblModelFilter.DataTextField = "FriendlyName";
                cblModelFilter.DataValueField = "Id";
                cblModelFilter.DataSource = indexableEntities;
                cblModelFilter.DataBind();

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
            }
        }

        private void sm_Navigate( object sender, HistoryEventArgs e )
        {
            var state = e.State["search"];
            Search( state );
            tbSearch.Text = state;
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
            ShowView();
        }

        private void btnTrigger_Click( object sender, EventArgs e )
        {
            throw new NotImplementedException();
        }
        protected void lbSave_Click( object sender, EventArgs e )
        {
            SetAttributeValue( "ShowModelFilter", cbShowModelFilter.Checked.ToString() );
            SaveAttributeValues();

            ShowView();

            mdEdit.Hide();
            pnlEditModal.Visible = false;
            upnlContent.Update();
        }

        protected void btnSearch_Click( object sender, EventArgs e )
        {
            this.AddHistory( "search", tbSearch.Text );
            Search( tbSearch.Text );
        }

        private void Search(string term )
        {
            lResults.Text = string.Empty;

            var client = IndexContainer.GetActiveComponent();

            Rock.UniversalSearch.IndexComponents.Elasticsearch search = new Rock.UniversalSearch.IndexComponents.Elasticsearch();
            ElasticClient _client = search.Client;

            //ISearchResponse<dynamic> results = null;
            List<int> entities = cblModelFilter.SelectedValuesAsInt;

            var results = client.Search( term, SearchType.ExactMatch, cblModelFilter.SelectedValuesAsInt );

            StringBuilder formattedResults = new StringBuilder();
            formattedResults.Append( "<ul class='list-unstyled'>" );

            foreach ( var result in results as IEnumerable<SearchResultModel> )
            {
                var formattedResult = result.Document.FormatSearchResult( CurrentPerson );

                if ( formattedResult.IsViewAllowed )
                {
                    formattedResults.Append( string.Format( "{0} <hr />", formattedResult.FormattedResult ));
                }
            }

            formattedResults.Append( "</ul>" );

            lResults.Text = formattedResults.ToString();
        }

        private void ShowView()
        {
            cblModelFilter.Visible = GetAttributeValue( "ShowModelFilter" ).AsBoolean();
        }

        protected override void ShowSettings()
        {
            pnlEditModal.Visible = true;
            upnlContent.Update();
            mdEdit.Show();

            cbShowModelFilter.Checked = GetAttributeValue( "ShowModelFilter" ).AsBoolean();

            upnlContent.Update();
        }

        #endregion
    }
}