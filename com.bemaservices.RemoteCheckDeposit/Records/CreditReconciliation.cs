using com.bemaservices.RemoteCheckDeposit.Attributes;

/// <summary>
/// Record Type 61 Credit Reconciliation for alternate Credit Records (Bank of America, Regions, etc)
/// </summary>
namespace com.bemaservices.RemoteCheckDeposit.Records
{
    /// <summary>
    /// Credit Reconciliation (Type 61).  This could be a 1 off but this is from BOA documentation.  Let's hope we dont have to get
    /// into a habit of doing these like this.
    /// </summary>
    public class CreditReconciliation : Record
    {
        /// <summary>
        /// Field 2 Size 1 IntegerField
        /// </summary>
        [IntegerField( 2, 1 )]
        public int RecordUsageIndicator { get; set; }

        /// <summary>
        /// Field 3 Size 15 TextField
        /// </summary>
        [TextField( 3, 15, FieldJustification.Right )]
        public string AuxiliaryOnUs { get; set; }

        /// <summary>
        /// Field 4 Size 1 TextField
        /// </summary>
        [TextField( 4, 1 )]
        public string ExternalProcessingCode { get; set; }

        /// <summary>
        /// Field 5 Size 9 TextField
        /// </summary>
        [IntegerField( 5, 9 )]
        public int PostingAccountRoutingNumber { get; set; }

        /// <summary>
        /// Field 6 Size 20
        /// </summary>
        [TextField( 6, 20, FieldJustification.Right )]
        public string PostingAccountBankOnUs { get; set; }
        
        /// <summary>
        /// Field 7 Size 10 MoneyField
        /// </summary>
        [MoneyField( 7, 10 )]
        public decimal ItemAmount { get; set; }

        /// <summary>
        /// Field 8 Size 15 TextField
        /// </summary>
        [TextField( 8, 15 )]
        public string ECEInstitutionSequenceNumber { get; set; }

        /// <summary>
        /// Field 9 Size 1 TextField
        /// </summary>
        [TextField( 9, 1 )]
        public string DocumentationTypeIndicator { get; set; }

        /// <summary>
        /// Field 10 Size 1 IntegerField
        /// </summary>
        [IntegerField( 10, 1 )]
        public int TypeOfAccountCode { get; set; }

        /// <summary>
        /// Field 11 Size 2 IntegerField
        /// </summary>
        [IntegerField( 11, 2 )]
        public int SourceOfWork { get; set; }
        
        /// <summary>
        /// Field 12 Size 3 TextField
        /// </summary>
        [TextField( 12, 3 )]
        public string Reserved { get; set; }

        /// <summary>
        /// Construct Record Type 61
        /// </summary>
        public CreditReconciliation() 
            : base(61)
        {
        }
    }
}
