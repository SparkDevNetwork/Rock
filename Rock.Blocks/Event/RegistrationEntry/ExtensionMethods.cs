using System.Collections.Generic;
using System.Linq;
using Rock.Model;
using Rock.ViewModels.Blocks.Event.RegistrationEntry;

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
                    Type = cost.Type,
                };
        }
    }
}
