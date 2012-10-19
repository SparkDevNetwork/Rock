using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rock.Core;

namespace Rock.Financial
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
        public RangeValue<DateTimeOffset?> DateRange { get; set; }
        
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
        public Fund Fund { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the currency.
        /// </summary>
        /// <value>
        /// The type of the currency.
        /// </value>
        public DefinedValue CurrencyType { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the credit card.
        /// </summary>
        /// <value>
        /// The type of the credit card.
        /// </value>
        public DefinedValue CreditCardType { get; set; }
    
        /// <summary>
        /// Gets or sets the type of the source.
        /// </summary>
        /// <value>
        /// The type of the source.
        /// </value>
        public DefinedValue SourceType { get; set; }

        /// <summary>
        /// Gets or sets the transaction code.
        /// </summary>
        /// <value>
        /// The transaction code.
        /// </value>
        public string TransactionCode { get; set; }
    }
}