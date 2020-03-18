
using com.bemaservices.RemoteCheckDeposit.Attributes;

namespace com.bemaservices.RemoteCheckDeposit.Records
{
    public class FileControl : Record
    {
        /// <summary>
        /// Field 2 Size 6 Integer Field
        /// </summary>
        [IntegerField( 2, 6 )]
        public int CashLetterCount { get; set; }

        /// <summary>
        /// Field 3 Size 8 IntegerField
        /// </summary>
        [IntegerField( 3, 8 )]
        public int TotalRecordCount { get; set; }

        /// <summary>
        /// Field 4 Size 8 IntegerField
        /// </summary>
        [IntegerField( 4, 8 )]
        public int TotalItemCount { get; set; }

        /// <summary>
        /// Field 5 Size 16 MoneyField
        /// </summary>
        [MoneyField( 5, 16 )]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Field 6 Size 14 TextField
        /// </summary>
        [TextField( 6, 14 )]
        public string ImmediateOriginContactName { get; set; }

        /// <summary>
        /// Field 7 Size 10 TextField
        /// </summary>
        [TextField( 7, 10 )]
        public string ImmediateOriginContactPhoneNumber { get; set; }

        /// <summary>
        /// Field 8 Size 16 TextField
        /// </summary>
        [TextField( 8, 16 )]
        protected string Reserved { get; set; }

        /// <summary>
        /// Construct Record Type 99
        /// </summary>
        public FileControl()
            : base( 99 )
        {
        }
    }
}
