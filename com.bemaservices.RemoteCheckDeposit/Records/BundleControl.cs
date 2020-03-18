
using com.bemaservices.RemoteCheckDeposit.Attributes;

namespace com.bemaservices.RemoteCheckDeposit.Records
{
    public class BundleControl : Record
    {
        /// <summary>
        /// Field 2 Size 4 IntegerField
        /// </summary>
        [IntegerField( 2, 4 )]
        public int ItemCount { get; set; }

        /// <summary>
        /// Field 3 Size 12 MoneyField
        /// </summary>
        [MoneyField( 3, 12 )]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Field 4 Size 12 MoneyField
        /// </summary>
        [MoneyField( 4, 12 )]
        public decimal? MICRValidTotalAmount { get; set; }

        /// <summary>
        /// Field 5 Size 5 IntegerField
        /// </summary>
        [IntegerField( 5, 5 )]
        public int ImageCount { get; set; }

        /// <summary>
        /// Field 6 Size 20 TextField
        /// </summary>
        [TextField( 6, 20 )]
        public string UserField { get; set; }

        /// <summary>
        /// Field 7 Size 25 TextField
        /// </summary>
        [TextField( 7, 25 )]
        protected string Reserved { get; set; }

        /// <summary>
        /// Construct Record Type 70
        /// </summary>
        public BundleControl()
            : base( 70 )
        {
        }
    }
}
