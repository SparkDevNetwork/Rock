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
using System.Text;
using System.Threading.Tasks;
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

            string accountIdCommaList = options.AccountIds != null ? options.AccountIds.AsDelimited(",") : null;

            Service service = new Service();
            Dictionary<string, object> parameters = new Dictionary<string,object>();
            parameters.Add( "startDate", options.StartDate );
            parameters.Add( "endDate", options.EndDate );
            parameters.Add( "accountIds", accountIdCommaList );
            parameters.Add( "personId", options.PersonId );
            parameters.Add( "orderByZipCode", options.OrderByZipCode );
            var result = service.GetDataSet( "sp_get_contribution_person_group_address", System.Data.CommandType.StoredProcedure, parameters );

            return result;
        }
    }
}
