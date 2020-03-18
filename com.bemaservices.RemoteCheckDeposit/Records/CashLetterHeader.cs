using System;

using com.bemaservices.RemoteCheckDeposit.Attributes;

namespace com.bemaservices.RemoteCheckDeposit.Records
{
    public class CashLetterHeader : Record
    {
        /// <summary>
        /// Field 2 Size 2 IntegerField
        /// </summary>
        [IntegerField( 2, 2 )]
        public int CollectionTypeIndicator { get; set; }

        /// <summary>
        /// Field 3 Size 9 TextField
        /// </summary>
        [TextField( 3, 9 )]
        public string DestinationRoutingNumber { get; set; }

        /// <summary>
        /// Field 4 Size 9 Type N
        /// </summary>
        [TextField( 4, 9 )]
        public string ClientInstitutionRoutingNumber { get; set; }

        /// <summary>
        /// Field 5 DateField
        /// </summary>
        [DateField( 5 )]
        public DateTime BusinessDate { get; set; }

        /// <summary>
        /// Field 6 DateField
        /// </summary>
        [DateTimeField( 6 )]
        public DateTime CreationDateTime { get; set; }

        /// <summary>
        /// Field 8 Size 1 TextField
        /// </summary>
        [TextField( 8, 1 )]
        public string RecordTypeIndicator { get; set; }

        /// <summary>
        /// Field 9 Size 1 TextField
        /// </summary>
        [TextField( 9, 1 )]
        public string DocumentationTypeIndicator { get; set; }

        /// <summary>
        /// Field 10 Size 8 TextField
        /// </summary>
        [TextField( 10, 8 )]
        public string ID { get; set; }

        /// <summary>
        /// Field 11 Size 14 TextField
        /// </summary>
        [TextField( 11, 14 )]
        public string OriginatorContactName { get; set; }

        /// <summary>
        /// Field 12 Size 10 TextField
        /// </summary>
        [TextField( 12, 10 )]
        public string OriginatorContactPhoneNumber { get; set; }

        /// <summary>
        /// Field 13 Size 1 TextField
        /// </summary>
        [TextField( 13, 1 )]
        public string WorkType { get; set; }

        /// <summary>
        /// Field 14 Size 2 TextField
        /// </summary>
        [TextField( 14, 2 )]
        public string UserField { get; set; }

        /// <summary>
        /// Field 15 Size 1 TextField
        /// </summary>
        [TextField( 15, 1 )]
        protected string Reserved { get; set; }

        /// <summary>
        /// Construct Record Type 10
        /// </summary>
        public CashLetterHeader()
            : base( 10 )
        {
        }
    }
}
