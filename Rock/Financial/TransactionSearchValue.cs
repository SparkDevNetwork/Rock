using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rock.Core;

namespace Rock.Financial
{
    public class TransactionSearchValue
    {
        public RangeValue<DateTime?> DateRange { get; set; }
        public RangeValue<decimal?> AmountRange { get; set; }
        public Fund Fund { get; set; }
        public DefinedValue CurrencyType { get; set; }
        public DefinedValue CreditCardType { get; set; }
        public DefinedValue SourceType { get; set; }
        public string TransactionCode { get; set; }
    }
}