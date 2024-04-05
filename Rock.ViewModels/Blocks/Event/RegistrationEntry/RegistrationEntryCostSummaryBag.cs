using Rock.Model;

namespace Rock.ViewModels.Blocks.Event.RegistrationEntry
{
    /// <summary>
    /// Contains the cost summary information for the Registration Entry block.
    /// </summary>
    public class RegistrationEntryCostSummaryBag
    {
        /// <summary>
        /// Gets or sets the cost summary type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public RegistrationCostSummaryType Type { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets the discounted cost.
        /// </summary>
        /// <value>
        /// The discounted cost.
        /// </value>
        public decimal DiscountedCost { get; set; }

        /// <summary>
        /// Gets or sets the minimum payment amount.
        /// </summary>
        /// <value>
        /// The minimum payment.
        /// </value>
        public decimal MinimumPaymentAmount { get; set; }

        /// <summary>
        /// Gets or sets the default payment amount.
        /// </summary>
        /// <value>
        /// The default payment.
        /// </value>
        public decimal? DefaultPaymentAmount { get; set; }
    }
}
