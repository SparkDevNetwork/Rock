using System;

using com.bemaservices.RemoteCheckDeposit.Attributes;

namespace com.bemaservices.RemoteCheckDeposit.Records
{
    public class CashLetterControl : Record
    {
        /// <summary>
        /// Field 2 Size 6 IntegerField
        /// </summary>
        [IntegerField( 2, 6 )]
        public int BundleCount { get; set; }

        /// <summary>
        /// Field 3 Size 8 IntegerField
        /// </summary>
        [IntegerField( 3, 8 )]
        public int ItemCount { get; set; }

        /// <summary>
        /// Field 4 Size 14 MoneyField
        /// </summary>
        [MoneyField( 4, 14 )]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Field 5 Size 9 IntegerField
        /// </summary>
        [IntegerField( 5, 9 )]
        public int? ImageCount { get; set; }

        /// <summary>
        /// Field 6 Size 18 TextField
        /// </summary>
        [TextField( 6, 18 )]
        public string ECEInstitutionName { get; set; }

        /// <summary>
        /// Field 7 DateField
        /// </summary>
        [DateField( 7 )]
        public DateTime? SettlementDate { get; set; }

        /// <summary>
        /// Field 8 Size 15 TextField
        /// </summary>
        [TextField( 8, 15 )]
        protected string Reserved { get; set; }

        /// <summary>
        /// Construct Record Type 90
        /// </summary>
        public CashLetterControl()
            : base( 90 )
        {
        }
    }
}
