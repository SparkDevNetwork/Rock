using System.Collections.Generic;
using System.Linq;
using Rock.Model;
using Rock.ViewModels.Blocks.Event.RegistrationEntry;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    internal static class ExtensionMethods
    {
        public static List<RegistrationEntryCostSummaryBag> AsRegistrationCostSummaryBagListOrNull( this IEnumerable<RegistrationCostSummaryInfo> costs )
        {
            return costs
                ?.Select( cost => cost.AsRegistrationCostSummaryBagOrNull() )
                // Remove null entries.
                .Where( cost =>  cost != null )
                .ToList();
        }

        public static RegistrationEntryCostSummaryBag AsRegistrationCostSummaryBagOrNull( this RegistrationCostSummaryInfo cost )
        {
            return cost == null
                ? null
                : new RegistrationEntryCostSummaryBag
                {
                    Cost = cost.Cost,
                    DefaultPaymentAmount = cost.DefaultPayment,
                    Description = cost.Description,
                    DiscountedCost = cost.DiscountedCost,
                    MinimumPaymentAmount = cost.MinPayment,
                    RegistrationRegistrantGuid = cost.RegistrationRegistrantGuid,
                    Type = cost.Type,
                };
        }

        public static RegistrationEntryPaymentPlanBag AsRegistrationPaymentPlanBag( this PaymentPlan paymentPlan )
        {
            var transactionFrequency = DefinedValueCache.Get( paymentPlan.TransactionFrequencyValueGuid );

            return paymentPlan == null
                ? null
                : new RegistrationEntryPaymentPlanBag
                {
                    FinancialScheduledTransactionGuid = paymentPlan.FinancialScheduledTransactionGuid,

                    AmountPerPayment = paymentPlan.AmountPerPayment,
                    StartDate = paymentPlan.StartDate,

                    RemainingPlannedAmount = paymentPlan.PlannedAmountRemaining,
                    RemainingNumberOfPayments = paymentPlan.NumberOfPaymentsRemaining,

                    ProcessedNumberOfPayments = paymentPlan.NumberOfPaymentsProcessed,
                    ProcessedPlannedAmount = paymentPlan.PlannedAmountProcessed,

                    TotalNumberOfPayments = paymentPlan.NumberOfPayments,
                    TotalPlannedAmount = paymentPlan.PlannedAmount,

                    TransactionFrequencyGuid = paymentPlan.TransactionFrequencyValueGuid,
                    TransactionFrequencyText = transactionFrequency.Value,
                };
        }
    }
}
