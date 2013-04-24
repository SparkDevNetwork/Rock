using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
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
        }

        /// <summary>
        /// Posts the scanned.
        /// </summary>
        /// <param name="financialTransaction">The financial transaction.</param>
        /// <param name="checkMicr">The check micr.</param>
        /// <returns></returns>
        [Authenticate]
		[HttpPost]
        public HttpResponseMessage PostScanned( [FromBody]FinancialTransaction financialTransaction)
        {
            financialTransaction.CheckMicrEncrypted = Encryption.EncryptString( financialTransaction.AttributeValues["checkMicrPlainText"][0].Value );
            return this.Post( financialTransaction );
        }
    }
}
