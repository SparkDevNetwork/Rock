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
using HtmlAgilityPack;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Universal Smart Search Results" )]
    [Category( "CMS" )]
    [Description( "A block that displays smart search results for the universal search provider." )]

    public partial class UniversalSmartSearchResults : RockBlock
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

            if ( !Page.IsPostBack )
            {
                if ( !string.IsNullOrWhiteSpace( PageParameter("SearchTerm") ) )
                {
                    Search( PageParameter( "SearchTerm" ) );
                }
                else if ( !string.IsNullOrWhiteSpace( PageParameter( "DocumentType" ) ) && !string.IsNullOrWhiteSpace( PageParameter( "DocumentId" ) ) )
                {
                    Redirect( PageParameter( "DocumentType" ), PageParameter( "DocumentId" ) );
                }
                else
                {
                    lResults.Text = "<div class='alert alert-warning'>No search criteria was provided.</div>";
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
            
        }

        /// <summary>
        /// Redirects the specified search term.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        private void Redirect(string documentType, string documentId )
        {
            var indexDocumentEntityType = EntityTypeCache.Read( documentType );

            var indexDocumentType = indexDocumentEntityType.GetEntityType();

            var client = IndexContainer.GetActiveComponent();
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

        /// <summary>
        /// Searches the specified term.
        /// </summary>
        /// <param name="term">The term.</param>
        private void Search(string term )
        {
            lResults.Text = string.Empty;

            StringBuilder formattedResults = new StringBuilder();

            List<FieldValue> fieldValues = new List<FieldValue>();
            List<int> entityIds = new List<int>();

            var searchEntitiesSetting = Rock.Web.SystemSettings.GetValue( "core_SmartSearchUniversalSearchEntities" );

            if ( !string.IsNullOrWhiteSpace( searchEntitiesSetting ) )
            {
                entityIds = searchEntitiesSetting.Split( ',' ).Select( int.Parse ).ToList();
            }

            // get the field critiera
            var fieldCriteriaSetting = Rock.Web.SystemSettings.GetValue( "core_SmartSearchUniversalSearchFieldCriteria" );
            SearchFieldCriteria fieldCriteria = new SearchFieldCriteria();

            if ( !string.IsNullOrWhiteSpace( fieldCriteriaSetting ) )
            {
                foreach ( var queryString in fieldCriteriaSetting.ToKeyValuePairList() )
                {
                    // check that multiple values were not passed
                    var values = queryString.Value.ToString().Split( ',' );

                    foreach ( var value in values )
                    {

                        fieldCriteria.FieldValues.Add( new FieldValue { Field = queryString.Key, Value = value } );
                    }
                }
            }

            var client = IndexContainer.GetActiveComponent();
            var results = client.Search( term, SearchType.Wildcard, entityIds, fieldCriteria );

            
            foreach ( var result in results)
            {
                var formattedResult = result.FormatSearchResult( CurrentPerson );

                if ( formattedResult.IsViewAllowed )
                {
                    formattedResults.Append( string.Format( "{0} <hr />", formattedResult.FormattedResult ));
                }
            }

            lResults.Text = formattedResults.ToString();
        }

        #endregion
    }
}