using System;

using com.bemaservices.RemoteCheckDeposit.Attributes;

namespace com.bemaservices.RemoteCheckDeposit.Records
{
    public class CheckDetailAddendumA : Record
    {
        [IntegerField( 2, 1 )]
        public int RecordNumber { get; set; }

        [TextField( 3, 9 )]
        public string BankOfFirstDepositRoutingNumber { get; set; }

        [DateField( 4 )]
        public DateTime BankOfFirstDepositBusinessDate { get; set; }

        [TextField( 5, 15 )]
        public string BankOfFirstDepositItemSequenceNumber { get; set; }

        [TextField( 6, 18 )]
        public string BankOfFirstDepositAccountNumber { get; set; }

        [TextField( 7, 5 )]
        public string BankOfFirstDepositBranch { get; set; }

        [TextField( 8, 15 )]
        public string PayeeName { get; set; }

        [TextField( 9, 1 )]
        public string TruncationIndicator { get; set; }

        [TextField( 10, 1 )]
        public string BankOfFirstDepositConversionIndicator { get; set; }

        [TextField( 11, 1 )]
        public string BankOfFirstDepositCorrectionIndicator { get; set; }

        [TextField( 12, 1 )]
        public string UserField { get; set; }

        [TextField( 13, 3 )]
        public string Reserved { get; set; }

        public CheckDetailAddendumA()
            : base( 26 )
        {
        }
    }
}
