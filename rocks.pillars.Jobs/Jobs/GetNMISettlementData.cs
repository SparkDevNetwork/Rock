// <copyright>
// Copyright Pillars Inc.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;

using Quartz;

using RestSharp;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace rocks.pillars.Jobs.Jobs
{
    /// <summary>
    /// Job that executes CSharp code.
    /// </summary>

    [FinancialGatewayField(
        "NMI Gateway",
        Key = "NMIGateway",
        Description = "The NMI Gateway.",
        IsRequired = false,
        Order = 0 )]

    [DisallowConcurrentExecution]
    public class GetNMISettlementData : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            // Use concurrent safe data structures to track the count and errors
            var errors = new List<string>();

            int txnsTotal = 0;
            int txnsUpdated = 0;

            JobDataMap dataMap = context.JobDetail.JobDataMap;

            using ( var rockContext = new RockContext() )
            {
                var targetGatewayQuery = new FinancialGatewayService( rockContext ).Queryable().Where( g => g.IsActive ).AsNoTracking();

                var targetGatewayGuid = dataMap.GetString( "NMIGateway" ).AsGuidOrNull();
                if ( !targetGatewayGuid.HasValue )
                {
                    errors.Add( "Invalid Gateway Setting" );
                }

                var financialGateway = new FinancialGatewayService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( g =>
                        g.IsActive &&
                        g.Guid == targetGatewayGuid.Value
                    )
                    .FirstOrDefault();


                if ( financialGateway != null )
                {
                    financialGateway.LoadAttributes( rockContext );

                    string nmiUrl = financialGateway.GetAttributeValue( "QueryUrl" );
                    string securityKey = financialGateway.GetAttributeValue( "SecurityKey" );

                    var txnService = new FinancialTransactionService( rockContext );
                    var txns = txnService.Queryable().AsNoTracking()
                        .Where( t =>
                            t.FinancialGatewayId == financialGateway.Id &&
                            t.SettledGroupId == null )
                        .ToList();
                    txnsTotal = txns.Count();

                    int offset = 0;
                    var txnBatch = txns.Skip( offset ).Take( 25 ).Select( t => t.TransactionCode ).ToList();
                    while ( txnBatch.Any() )
                    {
                        string errorMessage = string.Empty;
                        txnsUpdated += ProcessTransactions( financialGateway.Id, nmiUrl, securityKey, txnBatch, out errorMessage );
                        if ( errorMessage.IsNotNullOrWhiteSpace() )
                        {
                            errors.Add( $"Error occurred processing txns {offset}-{offset+25}: {errorMessage}" );
                        }

                        offset += 25;
                        txnBatch = txns.Skip( offset ).Take( 25 ).Select( t => t.TransactionCode ).ToList();
                    }
                }

                // Set the results for the job log
                context.Result = $"{txnsUpdated:N0} of {txnsTotal:N0} transactions updated";
            }

            if (errors.Any() )
            {
                ThrowErrors( context, errors );
            }
        }

        private int ProcessTransactions( int gatewayId, string nmiUrl, string securityKey, List<string> transactionCodes, out string errorMessage )
        {
            errorMessage = string.Empty;
            int updated = 0;

            var txnIds = transactionCodes.AsDelimited( "," );

            var restClient = new RestClient( nmiUrl );
            var restRequest = new RestRequest( Method.POST );
            restRequest.AddParameter( "security_key", securityKey );
            restRequest.AddParameter( "transaction_id", txnIds );

            var response = restClient.Execute( restRequest );

            if ( response == null )
            {
                errorMessage = "Empty response returned From gateway.";
                return updated;
            }

            if ( response.StatusCode != HttpStatusCode.OK )
            {
                errorMessage = $"Status code of {response.StatusCode} returned From gateway.";
                return updated;
            }

            XDocument xdoc = GetXmlResponse( response );
            if ( xdoc == null)
            {
                errorMessage = $"Could not parse XML response returned From gateway.";
                return updated;
            }

            var errorResponse = xdoc.Root.Element( "error_response" );
            if ( errorResponse != null )
            {
                errorMessage = errorResponse.Value;
                return updated;
            }

            using ( var rockContext = new RockContext() )
            {
                var financialTransactions = new FinancialTransactionService( rockContext )
                    .Queryable()
                    .Where( t => transactionCodes.Contains( t.TransactionCode ) )
                    .ToList();

                foreach ( var xTxn in xdoc.Root.Elements( "transaction" ) )
                {
                    var txnId = GetXElementValue( xTxn, "transaction_id" );

                    foreach ( var xAction in xTxn.Elements("action") )
                    {
                        DateTime? actionDate = ParseDateValue( GetXElementValue( xAction, "date" ) );
                        string actionType = GetXElementValue( xAction, "action_type" );

                        if ( actionType == "settle" )
                        {
                            var financialTransaction = financialTransactions.Where( t => t.TransactionCode == txnId ).FirstOrDefault();
                            if ( financialTransaction != null )
                            {
                                financialTransaction.IsSettled = true;
                                financialTransaction.SettledDate = actionDate;
                                financialTransaction.SettledGroupId = GetXElementValue( xAction, "processor_batch_id" ).Trim();
                                updated++;
                            }
                        }
                    }
                }

                rockContext.SaveChanges();
            }

            return updated;
        }

        private XDocument GetXmlResponse( IRestResponse response )
        {
            if ( response.Content.Trim().Length > 0 &&
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

        private void ThrowErrors( IJobExecutionContext jobExecutionContext, IEnumerable<string> errors )
        {
            var sb = new StringBuilder();

            if ( !jobExecutionContext.Result.ToStringSafe().IsNullOrWhiteSpace() )
            {
                sb.AppendLine();
            }

            sb.AppendLine( string.Format( "{0} Errors: ", errors.Count() ) );

            foreach ( var error in errors )
            {
                sb.AppendLine( error );
            }

            var errorMessage = sb.ToString();
            jobExecutionContext.Result += errorMessage;

            var exception = new Exception( errorMessage );
            var httpContext = HttpContext.Current;
            ExceptionLogService.LogException( exception, httpContext );

            throw exception;
        }

    }

}
