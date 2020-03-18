using System;

using com.bemaservices.RemoteCheckDeposit.Attributes;

namespace com.bemaservices.RemoteCheckDeposit.Records
{
    public class CashLetterHeaderRecordType10 : Record
    {
        /// <summary>
        /// Field 2 Size 2 IntegerField
        /// </summary>
        [IntegerField( 2, 2 )]
        public int CollectionTypeIndicator { get; set; }

        /// <summary>
        /// Field 3 Size 9 IntegerField
        /// </summary>
        [IntegerField( 3, 9 )]
        public int DestinationRoutingNumber { get; set; }

        /// <summary>
        /// Field 4 Size 9 IntegerField
        /// </summary>
        [IntegerField( 4, 9 )]
        public int BankOfAmericaClientId { get; set; }

        /// <summary>
        /// Field 5 DateField
        /// </summary>
        [DateField( 5 )]
        public DateTime CashLetterBusinessDate { get; set; }

        /// <summary>
        /// Field 6 DateField
        /// </summary>
        [DateField( 6 )]
        public DateTime CashLetterCreationDate { get; set; } 

        /// <summary>
        /// Field 7 Size 4 IntegerField
        /// </summary>
        [TextField( 7, 4 )]
        public string CashLetterCreationTime { get; set; }

        /// <summary>
        /// Field 8 Size 1 TextField
        /// </summary>
        [TextField( 8, 1 )]
        public string CashLetterRecordTypeIndicator { get; set; }

        /// <summary>
        /// Field 9 Size 1 TextField
        /// </summary>
        [TextField( 9, 1 )]
        public string CashLetterDocumentationTypeIndicator { get; set; }

        /// <summary>
        /// Field 10 Size 8 TextField
        /// </summary>
        [TextField( 10, 8 )]
        public string CashLetterId { get; set; }

        /// <summary>
        /// Field 11 Size 14 TextField
        /// </summary>
        [TextField( 11, 14 )]
        public string OriginatorContactName { get; set; }

        /// <summary>
        /// Field 12 Size 10 TextField
        /// </summary>
        [TextField( 12, 10, FieldJustification.Right )]
        public string OriginatorContactPhoneNumber { get; set; }

        /// <summary>
        /// Field 13 Size 1 TextField
        /// </summary>
        [TextField( 13, 1 )]
        public string FedWorkType { get; set; }

        /// <summary>
        /// Field 14 Size 2 TextField
        /// </summary>
        [TextField(14, 2)]
        public string UserField { get; set; }

        /// <summary>
        /// Field 15 Size 1 TextField
        /// </summary>
        [TextField(15, 1)]
        protected string Reserved { get; set; }

        /// <summary>
        /// Construct Record Type 10
        /// </summary>
        public CashLetterHeaderRecordType10() : 
            base( 10 )
        {
        }
    }
}
