// <copyright>
// Copyright 2013 by the Spark Development Network
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
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.Widgets
{
    [DisplayName( "Search Result List" )]
    [Category( "com_centralaz > Widgets" )]
    [Description( "A lava block for searching the external website using OpenSearchServer." )]
    [TextField( "IP Address", "The IP address of the server running OpenSearchServer", true, "10.200.4.113:9090", order: 1 )]
    [TextField( "Index Name", "The name of the index that is being searched", true, "RockIndex", order: 2 )]
    [TextField( "Searcher Username", "The username of the OpenSearchServer user", true, "searcher", order: 3 )]
    [TextField( "Api Key", "The API key of the OpenSearchServer user", true, "2b6b1d68b17b6090a1fffd1be4f9a20e", order: 4 )]
    [CodeEditorField( "Search Query", "The search query to use to return search results.", CodeEditorMode.Text, CodeEditorTheme.Rock, 400, true, "{\r\n    \"query\": \"QUERY\",\r\n    \"start\": 0,\r\n    \"rows\": 10,\r\n    \"lang\": \"ENGLISH\",\r\n    \"operator\": \"AND\",\r\n    \"collapsing\": {\r\n        \"max\": 2,\r\n        \"mode\": \"OFF\",\r\n        \"type\": \"OPTIMIZED\"\r\n    },\r\n    \"returnedFields\": [\r\n        \"url\"\r\n    ],\r\n    \"snippets\": [\r\n        {\r\n            \"field\": \"title\",\r\n            \"tag\": \"em\",\r\n            \"separator\": \"...\",\r\n            \"maxSize\": 200,\r\n            \"maxNumber\": 1,\r\n            \"fragmenter\": \"NO\"\r\n        },\r\n        {\r\n            \"field\": \"content\",\r\n            \"tag\": \"em\",\r\n            \"separator\": \"...\",\r\n            \"maxSize\": 200,\r\n            \"maxNumber\": 1,\r\n           \"fragmenter\": \"SENTENCE\"\r\n        }\r\n    ],\r\n    \"enableLog\": false,\r\n    \"searchFields\": [\r\n        {\r\n            \"field\": \"title\",\r\n            \"mode\": \"TERM_AND_PHRASE\",\r\n            \"boost\": 10\r\n        },\r\n        {\r\n            \"field\": \"content\",\r\n            \"mode\": \"TERM_AND_PHRASE\",\r\n            \"boost\": 1\r\n        },\r\n        {\r\n            \"field\": \"titleExact\",\r\n            \"mode\": \"TERM_AND_PHRASE\",\r\n            \"boost\": 10\r\n        },\r\n        {\r\n            \"field\": \"contentExact\",\r\n            \"mode\": \"TERM_AND_PHRASE\",\r\n            \"boost\": 1\r\n        }\r\n    ]\r\n}", "", 5 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the search results.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, true, @"{% include '~/Plugins/com_centralaz/Widgets/Lava/SearchResultList.lava' %}", "", 6 )]
    [BooleanField( "Enable Debug", "Display a list of merge fields available for lava.", false, "", 7 )]
    public partial class SearchResultList : RockBlock
    {

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
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
                tbSearch.Text = PageParameter( "SearchTerm" );
                LoadContent( PageParameter( "SearchTerm" ) );
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
            LoadContent( PageParameter( "SearchTerm" ) );
        }

        /// <summary>
        /// Handles the Click event of the btnButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnButton_Click( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams["SearchTerm"] = tbSearch.Text;
            NavigateToPage( RockPage.Guid, qryParams);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the content.
        /// </summary>
        /// <param name="query">The query.</param>
        public void LoadContent( string query )
        {
            var mergeFields = new Dictionary<string, object>();

            var results = GetResults( query );

            mergeFields.Add( "Results", results );
            mergeFields.Add( "CurrentPerson", CurrentPerson );
            mergeFields.Add( "CurrentUser", CurrentUser );

            var globalAttributeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );
            globalAttributeFields.ToList().ForEach( d => mergeFields.Add( d.Key, d.Value ) );

            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );

            // show debug info
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Rock.Security.Authorization.EDIT ) )
            {
                lDebug.Visible = true;
                lDebug.Text = mergeFields.lavaDebugInfo();
            }
        }

        /// <summary>
        /// Gets the results.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public dynamic GetResults( string query )
        {
            dynamic data = new ExpandoObject();
            if ( !string.IsNullOrWhiteSpace( query ) )
            {
                var converter = new ExpandoObjectConverter();

                string json = GetAttributeValue( "SearchQuery" );
                var payload = System.Text.RegularExpressions.Regex.Replace( json, "QUERY", query );
                var restClient = new RestClient( string.Format( "http://{0}/services/rest/index/{1}/search/field?login={2}&key={3}", GetAttributeValue( "IPAddress" ), GetAttributeValue( "IndexName" ), GetAttributeValue( "SearcherUsername" ), GetAttributeValue( "ApiKey" ) ) );
                var restRequest = new RestRequest( Method.POST );
                restRequest.RequestFormat = DataFormat.Json;
                restRequest.AddHeader( "Accept", "application/json" );
                restRequest.AddParameter( "application/json", payload, ParameterType.RequestBody );
                var restResponse = restClient.Execute( restRequest );
                if ( restResponse.StatusCode == HttpStatusCode.OK )
                {
                    data = JsonConvert.DeserializeObject<ExpandoObject>( restResponse.Content, converter );
                }
            }
            else
            {
                data.successful = false;
            }

            return data;
        }

        #endregion
    }
}