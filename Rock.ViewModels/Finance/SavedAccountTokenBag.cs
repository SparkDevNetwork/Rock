namespace Rock.ViewModels.Finance
{
    /// <summary>
    /// A bag used to add a saved account using a payment method token.
    /// </summary>
    public class SavedAccountTokenBag
    {
        /// <summary>
        /// The token for the payment method.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The currency type value identifier.
        /// </summary>
        public string CurrencyTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the first street line.
        /// </summary>
        /// <value>
        /// The first street line.
        /// </value>
        public string Street1 { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        /// <value>
        /// The postal code.
        /// </value>
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        public string Country { get; set; }
    }
}
