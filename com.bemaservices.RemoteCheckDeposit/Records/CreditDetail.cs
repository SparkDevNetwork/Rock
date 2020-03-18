using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using com.bemaservices.RemoteCheckDeposit.Attributes;

namespace com.bemaservices.RemoteCheckDeposit.Records
{
    public class CreditDetail : Record
    {
        [TextField( 2, 15, FieldJustification.Right )]
        public string AuxiliaryOnUs { get; set; }

        [TextField( 3, 1 )]
        public string ExternalProcessingCode { get; set; }

        [TextField( 4, 9 )]
        public string PayorRoutingNumber { get; set; }

        [TextField( 5, 20, FieldJustification.Right )]
        public string CreditAccountNumber { get; set; }

        [MoneyField( 6, 10 )]
        public decimal Amount { get; set; }

        [TextField( 7, 15, FieldJustification.Right )]
        public string InstitutionItemSequenceNumber { get; set; }

        [TextField( 8, 1 )]
        public string DocumentTypeIndicator { get; set; }

        [TextField( 9, 1 )]
        public string TypeOfAccountCode { get; set; }

        [TextField( 10, 1 )]
        public string SourceOfWorkCode { get; set; }

        [TextField( 11, 1 )]
        public string WorkType { get; set; }

        [TextField( 12, 1 )]
        public string DebitCreditIndicator { get; set; }

        [TextField( 13, 3 )]
        protected string Reserved { get; set; }

        public CreditDetail()
            : base( 61 )
        {
        }
    }
}
