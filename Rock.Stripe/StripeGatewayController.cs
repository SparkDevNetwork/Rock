using System.Web.Http;

using Rock.Model;

using Stripe;

namespace Rock.Stripe
{
    public class StripeGatewayController : Rock.Rest.ApiControllerBase
    {
        [Route( "api/v2/StripeGateway/createPaymentIntent" )]
        [HttpPost]
        public IHttpActionResult CreatePaymentIntent( [FromBody] PaymentIntentRequest request )
        {
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                var financialGateway = new FinancialGatewayService( rockContext )
                    .GetNoTracking( request.GatewayGuid );

                if ( financialGateway == null || !financialGateway.IsActive )
                {
                    return BadRequest();
                }

                financialGateway.LoadAttributes( rockContext );

                var client = new StripeClient( financialGateway.GetAttributeValue( Gateway.AttributeKey.SecretKey ) );
                var paymentIntentService = new PaymentIntentService( client );

                var paymentIntent = paymentIntentService.Create( new PaymentIntentCreateOptions
                {
                    Amount = ( long ) ( request.Amount * 100 ),
                    Currency = "USD", // TODO: This should be a configuration option on the gateway.
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                    },
                } );

                return Json( new
                {
                    clientSecret = paymentIntent.ClientSecret
                } );
            }
        }
    }
}
