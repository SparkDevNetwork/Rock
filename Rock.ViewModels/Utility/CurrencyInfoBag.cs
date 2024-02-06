namespace Rock.ViewModels.Utility
{
    /// <summary>
    /// Defines the attribute values of the Currency defined by the Organization Currency Code Global Attribute.
    /// </summary>
    public class CurrencyInfoBag
    {
        /// <summary>
        /// Gets or sets the symbol of the currency
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Gets or sets the number of decimal places that the currency needs to display
        /// </summary>
        public int DecimalPlaces { get; set; }

        /// <summary>
        /// Gets or sets if the currency code needs to be displayed on the left or the right side.
        /// </summary>
        public string SymbolLocation { get; set; }
    }
}
