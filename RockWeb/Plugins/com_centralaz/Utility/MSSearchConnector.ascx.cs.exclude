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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;

using com.centralaz.Utility.MSSearch;
using System.Net;

namespace RockWeb.Plugins.com_CentralAZ.Utility
{
    /// <summary>
    /// The MSSearchConnector will communicate with your configured search server and return results
    /// for display.
    /// </summary>
    [DisplayName( "MS Search Connector" )]
    [Category( "Utility" )]
    [Description( "A search/search-results tool for use with MS Enterprise Search Server." )]

    [TextField( "Search Server URL", "The URL of your MS Search Service (eg, http://mss01/_vti_bin/search.asmx)", true )]
	[TextField( "WebService Account UserName", "The username to use in the Network Credentials for the WebService call.", true )]
	[TextField( "WebService Account Password", "The password to use in the Network Credentials for the WebService call.", true ) ]
	[TextField( "WebService Account Domain", "The domain to use in the Network Credentials for the WebService call.", true ) ]
	[IntegerField( "Return Results Page Size", "The number of items to display on each page of the result set (default = 10).", false, 10 )]
    public partial class MSSearchConnector : Rock.Web.UI.RockBlock
    {
        #region Fields

        private System.Data.DataSet queryResults;
        private DateTime _startTime = DateTime.Now;
        private DateTime _endTime = DateTime.Now;

        #endregion

        #region Properties

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

            // If the user provided text in the textbox use it...
            if ( txtSearch.Text.Length > 0 )
            {
                dgSearchResults.CurrentPageIndex = 0;
                Query();
            }
            // otherwise look for a search term in the query string "q"
            else if ( Request.QueryString["q"] != null )
            {
                txtSearch.Text = Request.QueryString["q"];
                dgSearchResults.CurrentPageIndex = 0;
                Query();
            }
            else
            {
                //divHeader.Visible = false;
            }

            txtSearch.Focus();
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
            BindData( 1 );
        }

        /// <summary>
        /// Called when changing the page on the datagrid's pager
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        protected void dgSearchResults_PageIndexChanged( object source, DataGridPageChangedEventArgs e )
        {
            dgSearchResults.CurrentPageIndex = e.NewPageIndex;
            BindData( 1 );
        }

        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)
        private void Query()
        {
            BindData( 1 );
        }


        /// <summary>
        /// This will make a search request and bind the search results to the 
        /// table.  In theory the search request could be done in chunks
        /// starting at the given startAt variable, however we're just going to
        /// fetch the entire result set when we do the query since we don't have
        /// 100k+ resulting matches.
        /// </summary>
        /// <param name="startAt"></param>
        private void BindData( int startAt )
        {
            dgSearchResults.PageSize = Convert.ToInt32( GetAttributeValue( "ReturnResultsPageSize" ) );

            try
            {
                string keywordString = txtSearch.Text;
                MSQueryRequest queryRequest = BuildQueryRequest( keywordString, true, startAt, null );
                MSQueryService queryService = new MSQueryService
                {
                    Url = GetAttributeValue( "SearchServerURL" ),
                    Credentials = new NetworkCredential( GetAttributeValue( "WebServiceAccountUserName" ), GetAttributeValue( "WebServiceAccountPassword" ), GetAttributeValue( "WebServiceAccountDomain" ) )
                };

                queryResults = queryService.QueryEx( queryRequest.ToString() );
                _endTime = DateTime.Now;
                if ( queryResults.Tables[0].Rows.Count > 0 )
                {
                    dgSearchResults.Visible = true;
                    nbNoResults.Visible = false;
                    DisplayHeader( queryResults.Tables[0].Rows.Count );
                    dgSearchResults.DataSource = queryResults.Tables[0];
                    dgSearchResults.DataBind();
                }
                else
                {
                    dgSearchResults.Visible = false;
                    divHeader.Visible = false;
                    nbNoResults.Visible = true;
                }
                nbErrorMessage.Visible = false;
            }
            catch (Exception ex )
            {
                LogException( ex );
                dgSearchResults.DataSource = null;
                dgSearchResults.DataBind();
                divHeader.Visible = false;
                dgSearchResults.Visible = false;
                nbErrorMessage.Visible = true;
            }
        }

        /// <summary>
        /// Builds a small header that shows how many results are showing, how many were found, and how long the search took.
        /// Example: Results 1 - 8 of about 8. (11.85 seconds)
        /// </summary>
        /// <param name="total"></param>
        private void DisplayHeader( int total )
        {
            divHeader.Visible = true;
            TimeSpan ts = _endTime.Subtract( _startTime );
            int startItem = dgSearchResults.CurrentPageIndex * dgSearchResults.PageSize + 1;
            int endItem = startItem + dgSearchResults.PageSize - 1;
            if ( endItem > total )
                endItem = total;
            divHeader.InnerHtml = "Results <b>" + startItem + "</b> - <b>" + endItem + "</b> of about <b>" + total + "</b>. (<b>" + ts.Seconds + "." + ts.Milliseconds + "</b> seconds)";
        }

        private static MSQueryRequest BuildQueryRequest( string text, bool isKeyword, int startAt, string target )
        {
            MSQueryRequest queryRequest = new MSQueryRequest
            {
                QueryText = text,
                QueryID = Guid.NewGuid()
            };

            // Decide what type of query we're doing
            queryRequest.MSQueryType = isKeyword ? MSQueryRequest.QueryType.Keyword : MSQueryRequest.QueryType.MsSql;

            // Set the number of results to return
            //queryRequest.Count = returnCount;
            queryRequest.StartAt = startAt;

            // Set the web service target, if needed
            if ( string.IsNullOrEmpty( target ) == false )
            {
                queryRequest.Target = target;
            }

            // Set query options, as appropriate
            queryRequest.EnableStemming = true; // checkBoxEnableStemming.Checked;
            queryRequest.IgnoreAllNoiseQuery = true; //checkBoxIgnoreAllNoiseQuery.Checked;
            queryRequest.ImplicitAndBehavior = true; //checkBoxImplicitAndBehavior.Checked;
            queryRequest.TrimDuplicates = true; //checkBoxTrimDuplicates.Checked;
            queryRequest.IncludeRelevantResults = true; // checkBoxIncludeRelevantResults.Checked;
            queryRequest.IncludeHighConfidenceResults = true; // checkBoxIncludeHighConfidenceResults.Checked;
            queryRequest.IncludeSpecialTermResults = true; //checkBoxIncludeSpecialTermResults.Checked;

            return queryRequest;
        }

        public void ShowMessage( string message )
        {
            //nbMessage.Text = message;
        }

        /// <summary>
        /// Used to format the size (bytes) into KB.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public string FormatSize( object size )
        {
            string sSize = "0 KB";
            try
            {
                long lSize = (Int64)size / 1024;
                sSize = lSize + " KB";
            }
            catch
            { }

            return sSize;
        }

        /// <summary>
        /// This will "bold" the search term in the search result fields.
        /// </summary>
        /// <param name="text">text from a search result field</param>
        /// <returns>the text with the search term bolded using HTML markup</returns>
        public string HighlightKeywords( object text )
        {
            string stringToSearch = (string)text;
            string replacedString = Regex.Replace( stringToSearch, txtSearch.Text, "<b>" + txtSearch.Text + "</b>", RegexOptions.IgnoreCase );
            return replacedString;
        }

        #endregion
    }
}