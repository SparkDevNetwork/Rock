//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class FinancialTransactionsController : IHasCustomRoutes
    {
        /// <summary>
        /// Adds the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public void AddRoutes( System.Web.Routing.RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "FinancialTransactionGetMicr",
                routeTemplate: "api/FinancialTransactions/PostScanned",
                defaults: new
                {
                    controller = "FinancialTransactions",
                    action = "PostScanned"
                } );

            routes.MapHttpRoute(
                name: "GetContributionPersonGroupAddress",
                routeTemplate: "api/FinancialTransactions/GetContributionPersonGroupAddress",
                defaults: new
                {
                    controller = "FinancialTransactions",
                    action = "GetContributionPersonGroupAddress"
                } );

            routes.MapHttpRoute(
                name: "GetContributionTransactionsGroup",
                routeTemplate: "api/FinancialTransactions/GetContributionTransactions/{groupId}",
                defaults: new
                {
                    controller = "FinancialTransactions",
                    action = "GetContributionTransactions"
                } );

            routes.MapHttpRoute(
                name: "GetContributionTransactionsPerson",
                routeTemplate: "api/FinancialTransactions/GetContributionTransactions/{groupId}/{personId}",
                defaults: new
                {
                    controller = "FinancialTransactions",
                    action = "GetContributionTransactions"
                } );
        }

        /// <summary>
        /// Posts the scanned.
        /// </summary>
        /// <param name="financialTransaction">The financial transaction.</param>
        /// <param name="checkMicr">The check micr.</param>
        /// <returns></returns>
        [Authenticate]
        [HttpPost]
        public HttpResponseMessage PostScanned( [FromBody]FinancialTransactionScannedCheck financialTransactionScannedCheck )
        {
            financialTransactionScannedCheck.CheckMicrEncrypted = Encryption.EncryptString( financialTransactionScannedCheck.ScannedCheckMicr );
            FinancialTransaction financialTransaction = FinancialTransaction.FromJson( financialTransactionScannedCheck.ToJson() );
            return this.Post( financialTransaction );
        }

        /// <summary>
        /// Gets the contribution person group address.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException">
        /// </exception>
        [Authenticate]
        [HttpPost]
        public DataSet GetContributionPersonGroupAddress( [FromBody]Rock.Net.RestParameters.ContributionStatementOptions options )
        {
            var user = CurrentUser();
            if ( user == null )
            {
                // unable to determine user
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }

            if ( !new FinancialTransaction().IsAuthorized( "View", user.Person ) )
            {
                // user can't view FinancialTransactions
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }

            Service service = new Service();
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add( "startDate", options.StartDate );
            parameters.Add( "endDate", options.EndDate );
            if ( options.AccountIds != null )
            {
                parameters.Add( "accountIds", options.AccountIds.AsDelimited( "," ) );
            }
            else
            {
                parameters.Add( "accountIds", DBNull.Value );
            }

            if ( options.PersonId.HasValue )
            {
                parameters.Add( "personId", options.PersonId );
            }
            else
            {
                parameters.Add( "personId", DBNull.Value );
            }

            parameters.Add( "orderByZipCode", options.OrderByZipCode );
            var result = service.GetDataSet( "sp_get_contribution_person_group_address", System.Data.CommandType.StoredProcedure, parameters );

            if ( result.Tables.Count > 0 )
            {
                result.Tables[0].TableName = "contribution_person_group_address";
            }

            return result;
        }

        /// <summary>
        /// Gets the contribution transactions.
        /// </summary>
        /// <param name="groupId">The group unique identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        [Authenticate]
        [HttpPost]
        public DataSet GetContributionTransactions( int groupId, [FromBody]Rock.Net.RestParameters.ContributionStatementOptions options )
        {
            return GetContributionTransactions( groupId, null, options );
        }

        /// <summary>
        /// Gets the contribution transactions.
        /// </summary>
        /// <param name="personId">The person unique identifier.</param>
        /// <param name="groupId">The group unique identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [Authenticate]
        [HttpPost]
        public DataSet GetContributionTransactions( int groupId, int? personId, [FromBody]Rock.Net.RestParameters.ContributionStatementOptions options )
        {
            // get data from Rock database
            FinancialTransactionService financialTransactionService = new FinancialTransactionService();

            var qry = Get();

            qry = qry
                .Where( a => a.TransactionDateTime >= options.StartDate )
                .Where( a => a.TransactionDateTime < options.EndDate )
                .OrderBy( a => a.TransactionDateTime );

            if ( personId.HasValue )
            {
                // get transactions for a specific person
                qry = qry.Where( a => a.AuthorizedPersonId == personId.Value );
            }
            else 
            {
                // get transactions for all the persons in the specified group that have specified that group as their GivingGroup
                GroupMemberService groupMemberService = new GroupMemberService();
                var personIdList = groupMemberService.GetByGroupId( groupId ).Where( a => a.Person.GivingGroupId == groupId ).Select( s => s.PersonId ).ToList();

                qry = qry.Where( a => personIdList.Contains( a.AuthorizedPersonId.Value ) );
            }

            if ( options.AccountIds != null )
            {
                qry = qry.Where( a => options.AccountIds.Contains( a.TransactionDetails.FirstOrDefault().AccountId ) );
            }

            var selectQry = qry.Select( a => new 
            {
                a.TransactionDateTime,
                CurrencyTypeValueName = a.CurrencyTypeValue.Name,
                a.Summary,
                AccountId = a.TransactionDetails.FirstOrDefault().Account.Id,
                AccountName = a.TransactionDetails.FirstOrDefault().Account.Name,
                a.Amount
            } );

            DataTable dataTable = new DataTable( "contribution_transactions" );
            dataTable.Columns.Add( "TransactionDateTime" );
            dataTable.Columns.Add( "CurrencyTypeValueName" );
            dataTable.Columns.Add( "Summary" );
            dataTable.Columns.Add( "AccountId" );
            dataTable.Columns.Add( "AccountName" );
            dataTable.Columns.Add( "Amount" );

            foreach ( var fieldItems in selectQry.ToList() )
            {
                var itemArray = new object[] {
                    fieldItems.TransactionDateTime,
                    fieldItems.CurrencyTypeValueName,
                    fieldItems.Summary,
                    fieldItems.AccountId,
                    fieldItems.AccountName,
                    fieldItems.Amount
                };

                dataTable.Rows.Add( itemArray );
            }

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add( dataTable );

            return dataSet;
        }
    }
}
