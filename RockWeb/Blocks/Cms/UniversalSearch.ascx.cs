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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.UniversalSearch;
using Rock.UniversalSearch.IndexModels;
using System.Data.Entity;
using Nest;
using Rock.UniversalSearch.IndexComponents;
using Newtonsoft.Json.Linq;
using Rock.UniversalSearch.IndexModels;
using System.Text;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Universal Search" )]
    [Category( "CMS" )]
    [Description( "A block to search for all indexable entity types in Rock." )]
    
    public partial class UniversalSearch : Rock.Web.UI.RockBlock
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
                var entities = new EntityTypeService( new RockContext() ).Queryable().AsNoTracking().ToList();

                var indexableEntities = entities.Where( i => i.IsIndexingSupported == true ).ToList();

                cblEntities.DataTextField = "FriendlyName";
                cblEntities.DataValueField = "Id";
                cblEntities.DataSource = indexableEntities;
                cblEntities.DataBind();
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

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

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
            List<int> entities = cblEntities.SelectedValuesAsInt;

            var results = client.Search( term, SearchType.ExactMatch, cblEntities.SelectedValuesAsInt );

            StringBuilder formattedResults = new StringBuilder();
            formattedResults.Append( "<ul class='list-unstyled'>" );

            foreach ( var result in results as IEnumerable<SearchResultModel> )
            {
                var formattedResult = result.Document.FormatSearchResult( CurrentPerson );

                if ( formattedResult.IsViewAllowed )
                {
                    formattedResults.Append( string.Format( "<li class='margin-b-md'><i class='{2}'></i> {1} <br />Score {0}</li>", result.Score, formattedResult.FormattedResult, result.Document.IconCssClass ));
                }
            }

            formattedResults.Append( "</ul>" );

            lResults.Text = formattedResults.ToString();
        }

        #endregion
    }
}