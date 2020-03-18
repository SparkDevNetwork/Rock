
using com.bemaservices.RemoteCheckDeposit.Attributes;

namespace com.bemaservices.RemoteCheckDeposit.Records
{
    /// <summary>
    /// Type 25 Record Type
    /// </summary>
    public class CheckDetail : Record
    {
        /// <summary>
        /// Field 2 Size 15 FieldJustification.Right TextField
        /// </summary>
        [TextField( 2, 15, FieldJustification.Right )]
        public string AuxiliaryOnUs { get; set; }

        /// <summary>
        /// Field 3 Size 1 TextField
        /// </summary>
        [TextField( 3, 1 )]
        public string ExternalProcessingCode { get; set; }

        /// <summary>
        /// Field 4 Size 8 TextField
        /// </summary>
        [TextField( 4, 8 )]
        public string PayorBankRoutingNumber { get; set; }

        /// <summary>
        /// Field 5 Size 1 TextField
        /// </summary>
        [TextField( 5, 1 )]
        public string PayorBankRoutingNumberCheckDigit { get; set; }

        /// <summary>
        /// Field 6 Size 20 FieldJustification.Right TextField
        /// </summary>
        [TextField( 6, 20, FieldJustification.Right )]
        public string OnUs { get; set; }

        /// <summary>
        /// Field 7 Size 10 MoneyField
        /// </summary>
        [MoneyField( 7, 10 )]
        public decimal ItemAmount { get; set; }

        /// <summary>
        /// Field 8 Size 15 FieldJustificastion.Right TextField
        /// </summary>
        [TextField( 8, 15, FieldJustification.Right )]
        public string ClientInstitutionItemSequenceNumber { get; set; }

        /// <summary>
        /// Field 9 Size 1 TextField
        /// </summary>
        [TextField( 9, 1 )]
        public string DocumentationTypeIndicator { get; set; }

        /// <summary>
        /// Field 10 Size 1 TextField
        /// </summary>
        [TextField( 10, 1 )]
        public string ElectronicReturnAcceptanceIndicator { get; set; }

        /// <summary>
        /// Field 11 Size 1 IntegerField
        /// </summary>
        [IntegerField( 11, 1 )]
        public int? MICRValidIndicator { get; set; }

        /// <summary>
        /// Field 12 Size 1 TextField
        /// </summary>
        [TextField( 12, 1 )]
        public string BankOfFirstDepositIndicator { get; set; }

        /// <summary>
        /// Field 13 Size 2 IntegerField
        /// </summary>
        [IntegerField( 13, 2 )]
        public int CheckDetailRecordAddendumCount { get; set; }

        /// <summary>
        /// Field 14 Size 1 TextField
        /// </summary>
        [TextField( 14, 1 )]
        public string CorrectionIndicator { get; set; }

        /// <summary>
        /// Field 15 Size 1 TextField
        /// </summary>
        [TextField( 15, 1 )]
        public string ArchiveTypeIndicator { get; set; }

        /// <summary>
        /// Construct Record Type 25
        /// </summary>
        public CheckDetail()
            : base( 25 )
        {
        }
    }
}
