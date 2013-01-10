//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class TransactionSearchValue
    {
        /// <summary>
        /// Gets or sets the date range.
        /// </summary>
        /// <value>
        /// The date range.
        /// </value>
        public RangeValue<DateTime?> DateRange { get; set; }
        
        /// <summary>
        /// Gets or sets the amount range.
        /// </summary>
        /// <value>
        /// The amount range.
        /// </value>
        public RangeValue<decimal?> AmountRange { get; set; }
        
        /// <summary>
        /// Gets or sets the fund.
        /// </summary>
        /// <value>
        /// The fund.
        /// </value>
        public int? FundId { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the currency.
        /// </summary>
        /// <value>
        /// The type of the currency.
        /// </value>
        public int? CurrencyTypeValueId { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the credit card.
        /// </summary>
        /// <value>
        /// The type of the credit card.
        /// </value>
        public int? CreditCardTypeValueId { get; set; }
    
        /// <summary>
        /// Gets or sets the type of the source.
        /// </summary>
        /// <value>
        /// The type of the source.
        /// </value>
        public int? SourceTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the transaction code.
        /// </summary>
        /// <value>
        /// The transaction code.
        /// </value>
        public string TransactionCode { get; set; }
    }
}