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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

using RestSharp;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.cc_newspring.Blocks.NMITransactions
{
    [DisplayName( "NMI Transaction Mismatch List" )]
    [Category( "Finance" )]
    [Description( "Builds a list of all financial transactions that are in the NMI system, but not in Rock." )]

    [ContextAware]
    [TextField( "Title", "Title to display above the grid. Leave blank to hide.", false, order: 1 )]
    [FinancialGatewayField("Financial Gateway", "The financial gateway", true, "", "", 0)]
    public partial class NMITransactionMismatch : Rock.Web.UI.RockBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            gfTransactions.ApplyFilterClick += gfTransactions_ApplyFilterClick;
            gfTransactions.ClearFilterClick += gfTransactions_ClearFilterClick;
            gfTransactions.DisplayFilterValue += gfTransactions_DisplayFilterValue;
            if ( !Page.IsPostBack)
            {
                string title = GetAttributeValue( "Title" );
                if ( string.IsNullOrWhiteSpace( title ) )
                {
                    title = "NMI Transaction Mismatch List";
                }

                lTitle.Text = title;
                BindFilter();
                BindGrid();
            }
        }

        private void gfTransactions_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
        }

        private void gfTransactions_ClearFilterClick( object sender, EventArgs e )
        {
            gfTransactions.DeleteUserPreferences();
            BindFilter();
        }

        private void gfTransactions_ApplyFilterClick( object sender, EventArgs e )
        {
            gfTransactions.SaveUserPreference( "Date Range", drpDates.DelimitedValues );
            BindGrid();
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            drpDates.DelimitedValues = gfTransactions.GetUserPreference( "Date Range" );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            String errorMessage = "";

            // Date Range
            DateTime endDate = DateTime.Today;
            DateTime startDate = endDate.AddDays( -6 );
            var drp = new DateRangePicker();
            drp.DelimitedValues = gfTransactions.GetUserPreference( "Date Range" );
            if ( drp.LowerValue.HasValue )
            {
                startDate = drp.LowerValue.Value;
            }

            if ( drp.UpperValue.HasValue )
            {
                endDate = drp.UpperValue.Value;
            }

            FinancialGateway financialGateway = null;
            Guid? gatewayGuid = GetAttributeValue( "FinancialGateway" ).AsGuidOrNull();
            if ( gatewayGuid.HasValue )
            {
                financialGateway = new FinancialGatewayService( rockContext ).Get( gatewayGuid.Value );
                if ( financialGateway != null )
                {
                    financialGateway.LoadAttributes( rockContext );
                }
            }

            if ( financialGateway != null )
            {
                var transactions = new List<Payment>();
                transactions = GetTransactions( financialGateway, startDate, endDate, out errorMessage );

                // Just a couple test payments since I can't get real ones yet
                var newPayment = new Payment();
                newPayment.TransactionCode = "123";
                newPayment.Amount = 5;
                newPayment.TransactionDateTime = DateTime.Now;
                transactions.Add( newPayment );
                var newPayment2 = new Payment();
                newPayment2.TransactionCode = "456";
                newPayment2.Amount = 5000;
                newPayment2.TransactionDateTime = DateTime.Now;
                transactions.Add( newPayment2 );

                transactions.ToList();

                
                // Get a list of the transactions in Rock for the time frame specified.
                var transactionService = new FinancialTransactionService(rockContext);
                var transactionsInRock = transactionService.Queryable();
                
                // You have a list of all the transactions from NMI for the time period requested.
                // You also have a list of all the transactions in Rock.
                // Next you need to filter out the transactions based on if they are in Rock or not.
                var result = transactions.Where( t => !transactionsInRock.Any( t2 => t2.TransactionCode == t.TransactionCode ) ).ToList();

                // Show the transactions that are left after the filtering in the grid.
                gTransactions.DataSource = result.Select( t => new
                {
                    t.TransactionDateTime,
                    t.Amount,
                    t.TransactionCode
                } ).ToList();
                gTransactions.DataBind();
            }
        }

        /// <summary>
        /// Gets the transactions from the payment gateway for the specified time period.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public List<Payment> GetTransactions( FinancialGateway financialGateway, DateTime startDate, DateTime endDate, out string errorMessage )
        {
            errorMessage = string.Empty;

            var txns = new List<Payment>();

            var restClient = new RestClient( GetAttributeValue( financialGateway, "QueryUrl" ) );
            var restRequest = new RestRequest( Method.GET );

            restRequest.AddParameter( "username", GetAttributeValue( financialGateway, "AdminUsername" ) );
            restRequest.AddParameter( "password", GetAttributeValue( financialGateway, "AdminPassword" ) );
            restRequest.AddParameter( "start_date", startDate.ToString( "yyyyMMddHHmmss" ) );
            restRequest.AddParameter( "end_date", endDate.ToString( "yyyyMMddHHmmss" ) );

            try
            {
                var response = restClient.Execute( restRequest );
                if ( response != null )
                {
                    if ( response.StatusCode == HttpStatusCode.OK )
                    {
                        var xdocResult = GetXmlResponse( response );
                        if ( xdocResult != null )
                        {
                            var errorResponse = xdocResult.Root.Element( "error_response" );
                            if ( errorResponse != null )
                            {
                                errorMessage = errorResponse.Value;
                            }
                            else
                            {
                                foreach ( var xTxn in xdocResult.Root.Elements( "transaction" ) )
                                {
                                    string subscriptionId = GetXElementValue( xTxn, "original_transaction_id" ).Trim();
                                    if ( !string.IsNullOrWhiteSpace( subscriptionId ) )
                                    {
                                        Payment payment = null;
                                        var statusMessage = new StringBuilder();
                                        foreach ( var xAction in xTxn.Elements( "action" ) )
                                        {
                                            DateTime? actionDate = ParseDateValue( GetXElementValue( xAction, "date" ) );
                                            string actionType = GetXElementValue( xAction, "action_type" );
                                            string responseText = GetXElementValue( xAction, "response_text" );
                                            if ( actionDate.HasValue )
                                            {
                                                statusMessage.AppendFormat( "{0} {1}: {2}; Status: {3}",
                                                    actionDate.Value.ToShortDateString(), actionDate.Value.ToShortTimeString(),
                                                    actionType.FixCase(), responseText );
                                                statusMessage.AppendLine();
                                            }
                                            if ( payment == null && actionType == "sale" && GetXElementValue( xAction, "source" ) == "recurring" )
                                            {
                                                decimal? txnAmount = GetXElementValue( xAction, "amount" ).AsDecimalOrNull();
                                                if ( txnAmount.HasValue && actionDate.HasValue )
                                                {
                                                    payment = new Payment();
                                                    payment.Status = GetXElementValue( xTxn, "condition" ).FixCase();
                                                    payment.IsFailure = payment.Status == "Failed";
                                                    payment.StatusMessage = GetXElementValue( xTxn, "response_text" );
                                                    payment.Amount = txnAmount.Value;
                                                    payment.TransactionDateTime = actionDate.Value;
                                                    payment.TransactionCode = GetXElementValue( xTxn, "transaction_id" );
                                                    payment.GatewayScheduleId = subscriptionId;
                                                }
                                            }
                                        }
                                        if ( payment != null )
                                        {
                                            payment.StatusMessage = statusMessage.ToString();
                                            txns.Add( payment );
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            errorMessage = "Invalid XML Document Returned From Gateway!";
                        }
                    }
                    else
                    {
                        errorMessage = string.Format( "invalid response from gateway: [{0}] {1}", response.StatusCode.ConvertToString(), response.ErrorMessage );
                    }
                }
                else
                {
                    errorMessage = "Null Response From Gateway!";
                }
            }
            catch ( WebException webException )
            {
                string message = GetResponseMessage( webException.Response.GetResponseStream() );
                throw new Exception( webException.Message + " - " + message );
            }

            return txns;
        }

        /// <summary>
        /// Gets the attribute value for the gateway 
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetAttributeValue( FinancialGateway financialGateway, string key )
        {
            if ( financialGateway.AttributeValues == null )
            {
                financialGateway.LoadAttributes();
            }

            var values = financialGateway.AttributeValues;
            if ( values != null && values.ContainsKey( key ) )
            {
                var keyValues = values[key];
                if ( keyValues != null )
                {
                    return keyValues.Value;
                }
            }

            return string.Empty;
        }

        private DateTime? ParseDateValue( string dateString )
        {
            if ( !string.IsNullOrWhiteSpace( dateString ) && dateString.Length >= 14 )
            {
                int year = dateString.Substring( 0, 4 ).AsInteger();
                int month = dateString.Substring( 4, 2 ).AsInteger();
                int day = dateString.Substring( 6, 2 ).AsInteger();
                int hour = dateString.Substring( 8, 2 ).AsInteger();
                int min = dateString.Substring( 10, 2 ).AsInteger();
                int sec = dateString.Substring( 12, 2 ).AsInteger();

                return new DateTime( year, month, day, hour, min, sec );
            }

            return DateTime.MinValue;

        }

        /// <summary>
        /// Gets the response message.
        /// </summary>
        /// <param name="responseStream">The response stream.</param>
        /// <returns></returns>
        private string GetResponseMessage( Stream responseStream )
        {
            Stream receiveStream = responseStream;
            Encoding encode = System.Text.Encoding.GetEncoding( "utf-8" );
            StreamReader readStream = new StreamReader( receiveStream, encode );

            StringBuilder sb = new StringBuilder();
            Char[] read = new Char[8192];
            int count = 0;
            do
            {
                count = readStream.Read( read, 0, 8192 );
                String str = new String( read, 0, count );
                sb.Append( str );
            }
            while ( count > 0 );

            return sb.ToString();
        }

        /// <summary>
        /// Gets the response as an XDocument
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        private XDocument GetXmlResponse( IRestResponse response )
        {
            if ( response.StatusCode == HttpStatusCode.OK &&
                response.Content.Trim().Length > 0 &&
                response.Content.Contains( "<?xml" ) )
            {
                return XDocument.Parse( response.Content );
            }

            return null;
        }

        private string GetXElementValue( XElement parentElement, string elementName )
        {
            var x = parentElement.Element( elementName );
            if ( x != null )
            {
                return x.Value;
            }
            return string.Empty;
        }

    }
}