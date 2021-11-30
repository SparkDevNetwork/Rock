using System;

namespace Rock.Stripe
{
    public class PaymentIntentRequest
    {
        public Guid GatewayGuid { get; set; }

        public decimal Amount { get; set; }
    }
}
