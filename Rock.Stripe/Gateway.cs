using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Attribute;
using Rock.Financial;
using Rock.Model;

using Stripe;

namespace Rock.Stripe
{
    [DisplayName( "Stripe Gateway" )]
    [Description( "" )]

    [Export( typeof( GatewayComponent ) )]
    [ExportMetadata( "ComponentName", "Stripe Gateway" )]

    [TextField( "Secret Key",
        Key = AttributeKey.SecretKey,
        Description = "The Secret API Key to use.",
        IsRequired = true,
        IsPassword = true,
        DefaultValue = "",
        Order = 0 )]

    [TextField( "Publishable Key",
        Key = AttributeKey.PublishableKey,
        Description = "The Public API Key to use.",
        IsRequired = true,
        DefaultValue = "",
        Order = 1 )]

    public class Gateway : GatewayComponent, IObsidianHostedGatewayComponent
    {
        internal static class AttributeKey
        {
            public const string SecretKey = "SecretKey";

            public const string PublishableKey = "PublishableKey";
        }

        #region IObsidianFinancialGateway

        /// <inheritdoc/>
        public string GetObsidianControlFileUrl( FinancialGateway financialGateway )
        {
            return "/Obsidian/Controls/stripeGatewayControl.js";
        }

        /// <inheritdoc/>
        public object GetObsidianControlSettings( FinancialGateway financialGateway, HostedPaymentInfoControlOptions options )
        {
            return new
            {
                GatewayGuid = financialGateway.Guid,
                PublicKey = GetAttributeValue( financialGateway, AttributeKey.PublishableKey )
            };
        }

        #endregion

        public string CreateCustomerAccount( FinancialGateway financialGateway, ReferencePaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = null;

            return Guid.NewGuid().ToString();
        }

        /// <inheritdoc/>
        public bool TryGetPaymentTokenFromParameters( FinancialGateway financialGateway, IDictionary<string, string> parameters, out string paymentToken )
        {
            return parameters.TryGetValue( "payment_intent", out paymentToken );
        }

        /// <inheritdoc/>
        public bool IsPaymentTokenCharged( FinancialGateway financialGateway, string paymentToken )
        {
            return true;
        }

        /// <inheritdoc/>
        public FinancialTransaction FetchPaymentTokenTransaction( Data.RockContext rockContext, FinancialGateway financialGateway, int? fundId, string paymentToken )
        {
            if ( financialGateway.Attributes == null )
            {
                financialGateway.LoadAttributes( rockContext );
            }

            var client = new StripeClient( financialGateway.GetAttributeValue( AttributeKey.SecretKey ) );
            var paymentIntentService = new PaymentIntentService( client );

            var intent = paymentIntentService.Get( paymentToken );

            throw new NotImplementedException();
        }

        public override FinancialTransaction Charge( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override FinancialTransaction Credit( FinancialTransaction origTransaction, decimal amount, string comment, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override FinancialScheduledTransaction AddScheduledPayment( FinancialGateway financialGateway, PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override bool UpdateScheduledPayment( FinancialScheduledTransaction transaction, PaymentInfo paymentInfo, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override bool CancelScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override bool ReactivateScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override bool GetScheduledPaymentStatus( FinancialScheduledTransaction transaction, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override List<Payment> GetPayments( FinancialGateway financialGateway, DateTime startDate, DateTime endDate, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override string GetReferenceNumber( FinancialTransaction transaction, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override string GetReferenceNumber( FinancialScheduledTransaction scheduledTransaction, out string errorMessage )
        {
            throw new NotImplementedException();
        }
    }
}
