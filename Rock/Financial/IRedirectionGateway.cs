using System;
using System.Collections.Generic;

namespace Rock.Financial
{
    /// <summary>
    /// Interface to implement if your gateway requires redirection.
    /// </summary>
    public interface IRedirectionGateway
    {
        /// <summary>
        /// Gets the merchant field label.
        /// </summary>
        /// <value>
        /// The merchant field label.
        /// </value>
        string MerchantFieldLabel { get; }

        /// <summary>
        /// Gets the fund field label.
        /// </summary>
        /// <value>
        /// The fund field label.
        /// </value>
        string FundFieldLabel { get; }

        /// <summary>
        /// Gets the merchants.
        /// </summary>
        /// <returns></returns>
        IEnumerable<KeyValuePair<string, string>> GetMerchants();

        /// <summary>
        /// Gets the merchant funds.
        /// </summary>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <returns></returns>
        IEnumerable<KeyValuePair<string, string>> GetMerchantFunds( string merchantId );

    }
}
