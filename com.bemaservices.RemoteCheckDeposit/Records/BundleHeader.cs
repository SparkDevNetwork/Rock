using System;

using com.bemaservices.RemoteCheckDeposit.Attributes;

namespace com.bemaservices.RemoteCheckDeposit.Records
{
    public class BundleHeader : Record
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
        /// Field 4 Size 9 TextField
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
        [DateField( 6 )]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Field 7 Size 10 TextField
        /// </summary>
        [TextField( 7, 10 )]
        public string ID { get; set; }

        /// <summary>
        /// Field 8 Size 4 TextField
        /// </summary>
        [TextField( 8, 4 )]
        public string SequenceNumber { get; set; }

        /// <summary>
        /// Field 9 Size 2 TextField
        /// </summary>
        [TextField( 9, 2 )]
        public string CycleNumber { get; set; }

        /// <summary>
        /// Field 10 Size 9 TextField
        /// </summary>
        [TextField( 10, 9 )]
        public string ReturnLocationRoutingNumber { get; set; }
        
        /// <summary>
        /// Field 11 Size 5 TextField
        /// </summary>
        [TextField( 11, 5 )]
        public string UserField { get; set; }

        /// <summary>
        /// Field 12 Size 12 TextField
        /// </summary>
        [TextField( 12, 12 )]
        public string Reserved { get; set; }

        /// <summary>
        /// Construct Record Type 20
        /// </summary>
        public BundleHeader()
            : base( 20 )
        {
        }
    }
}
