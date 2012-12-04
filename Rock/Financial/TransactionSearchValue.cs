//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rock.Model;

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
        public int? CurrencyTypeId { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the credit card.
        /// </summary>
        /// <value>
        /// The type of the credit card.
        /// </value>
        public int? CreditCardTypeId { get; set; }
    
        /// <summary>
        /// Gets or sets the type of the source.
        /// </summary>
        /// <value>
        /// The type of the source.
        /// </value>
        public int? SourceTypeId { get; set; }

        /// <summary>
        /// Gets or sets the transaction code.
        /// </summary>
        /// <value>
        /// The transaction code.
        /// </value>
        public string TransactionCode { get; set; }
    }
}