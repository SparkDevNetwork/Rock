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
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.MyWell;

namespace RockWeb.Webhooks
{
    /// <summary>
    /// </summary>
    public class MyWellCardSync : IHttpHandler
    {
        /// <summary>
        /// Enables processing of HTTP Web requests by a custom <see langword="HttpHandler" /> that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, <see langword="Request" />, <see langword="Response" />, <see langword="Session" />, and <see langword="Server" />) used to service HTTP requests.</param>
        public void ProcessRequest( HttpContext context )
        {
            // see https://sandbox.gotnpgateway.com/docs/webhooks/#core-webhook-response-format for example payload
            HttpRequest request = context.Request;
            var response = context.Response;
            response.ContentType = "text/plain";

            // Signature https://sandbox.gotnpgateway.com/docs/webhooks/#security
            // see signature @ https://sandbox.fluidpay.com/merchant/settings/webhooks/search
            var postedSignature = request.Headers["Signature"];

            string postedData = string.Empty;
            using ( var reader = new StreamReader( request.InputStream ) )
            {
                postedData = reader.ReadToEnd();
            }

            var cardSyncWebhookResponse = postedData.FromJsonOrNull<CardSyncWebhookResponse>();

            if ( cardSyncWebhookResponse == null )
            {
                response.StatusCode = ( int ) HttpStatusCode.BadRequest;
                response.StatusDescription = "Unable to determine response format.";
                return;
            }

            var paymentMethodData = cardSyncWebhookResponse.PaymentMethodData;

            if ( paymentMethodData == null )
            {
                response.StatusCode = ( int ) HttpStatusCode.BadRequest;
                response.StatusDescription = "Unable to determine payment method 'data'.";
                return;
            }

            var rockContext = new RockContext();
            FinancialPersonSavedAccountService financialPersonSavedAccountService = new FinancialPersonSavedAccountService( rockContext );
            var financialPersonSavedAccountQuery = financialPersonSavedAccountService.Queryable()
                .Where( a => a.GatewayPersonIdentifier == paymentMethodData.RecordId || a.FinancialPaymentDetail.GatewayPersonIdentifier == paymentMethodData.RecordId );

            var savedAccounts = financialPersonSavedAccountQuery.Include( a => a.FinancialPaymentDetail ).Include( a => a.FinancialGateway ).ToList();

            // There probably is only one saved account for the GatewayPersonIdentifier, but just in case, we'll loop thru.
            foreach ( var savedAccount in savedAccounts )
            {
                var financialGateway = savedAccount.FinancialGateway;
                var myWellGateway = savedAccount.FinancialGateway?.GetGatewayComponent() as MyWellGateway;
                if ( myWellGateway == null )
                {
                    ExceptionLogService.LogException( new MyWellGatewayException( $"Unable to determine Gateway for CardSync GatewayPersonIdentifier: {paymentMethodData.RecordId} and FinancialGatewayId: {savedAccount.FinancialGatewayId}" ) );
                    
                    response.StatusCode = ( int ) HttpStatusCode.BadRequest;
                    response.StatusDescription = $"Unable to find matching financial gateway record for recordId: {paymentMethodData.RecordId}";
                    return;
                }

                financialGateway.LoadAttributes();
                var validSignature = myWellGateway.VerifySignature( financialGateway, postedSignature, postedData );
                if ( !validSignature )
                {
                    ExceptionLogService.LogException( new MyWellGatewayException( $"Invalid WebHook signature included in header. (PostedData for RecordId: {paymentMethodData.RecordId} and FinancialGatewayId: {savedAccount.FinancialGatewayId})" ) );
                    response.StatusCode = ( int ) HttpStatusCode.Forbidden;
                    response.StatusDescription = "Invalid WebHook signature included in header";
                    return;
                }

                var financialPaymentDetail = savedAccount.FinancialPaymentDetail;
                if ( financialPaymentDetail == null )
                {
                    // shouldn't happen
                    continue;
                }

                if ( paymentMethodData.ExpirationDate.IsNotNullOrWhiteSpace() && paymentMethodData.ExpirationDate.Length == 5 )
                {
                    // now that we validated this is a 5 char string (MM/YY), extract MM and YY as integers 
                    financialPaymentDetail.ExpirationMonth = paymentMethodData.ExpirationDate.Substring( 0, 2 ).AsIntegerOrNull();
                    financialPaymentDetail.ExpirationYear = paymentMethodData.ExpirationDate.Substring( 3, 2 ).AsIntegerOrNull();

                    // ToDo: See if they send us the CreditCardType (visa, mastercard)
                    // ?? financialPaymentDetail.CreditCardTypeValueId
                }

                financialPaymentDetail.AccountNumberMasked = paymentMethodData.MaskedNumber;
                rockContext.SaveChanges();
            }

            // NOTE: If it takes us more than 5 seconds to respond, they'll retry.
            // Otherwise, if we respond with a 200, they'll assume we processed it.
            // See https://sandbox.gotnpgateway.com/docs/webhooks/#acknowledge-and-retry for additional details
            response.StatusCode = ( int ) HttpStatusCode.OK;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}