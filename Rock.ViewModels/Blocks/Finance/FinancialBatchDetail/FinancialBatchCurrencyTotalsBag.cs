using Rock.ViewModels.Utility;


namespace Rock.ViewModels.Blocks.Finance.FinancialBatchDetail
{
    /// <summary>
    /// AddressStandardizationResultBag
    /// Implements the <see cref="IViewModel" />
    /// </summary>
    /// <seealso cref="IViewModel" />
    public class FinancialBatchCurrencyTotalsBag: IViewModel
    {
        /// <summary>
        /// Gets or sets the name of the account to be displayed in the Accounts Total section in the view mode
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the currency to be displayed in the Accounts Total section in the view mode
        /// </summary>
        public decimal Currency { get; set; }
    }
}
