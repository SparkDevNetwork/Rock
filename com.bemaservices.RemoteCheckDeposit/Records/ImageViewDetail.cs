using System;

using com.bemaservices.RemoteCheckDeposit.Attributes;

namespace com.bemaservices.RemoteCheckDeposit.Records
{
    public class ImageViewDetail : Record
    {
        /// <summary>
        /// Field 2 Size 1 IntegerField
        /// </summary>
        [IntegerField( 2, 1 )]
        public int ImageIndicator { get; set; }

        /// <summary>
        /// Field 3 Size 9 TextField
        /// </summary>
        [TextField( 3, 9 )]
        public string ImageCreatorRoutingNumber { get; set; }

        /// <summary>
        /// Field 4 DateField
        /// </summary>
        [DateField( 4 )]
        public DateTime ImageCreatorDate { get; set; }

        /// <summary>
        /// Field 5 Size 2 IntegerField
        /// </summary>
        [IntegerField( 5, 2 )]
        public int? ImageViewFormatIndicator { get; set; }

        /// <summary>
        /// Field 6 Size 2 IntegerField
        /// </summary>
        [IntegerField( 6, 2 )]
        public int? CompressionAlgorithmIdentifier { get; set; }

        /// <summary>
        /// Field 7 Size 7 IntegerField
        /// </summary>
        [IntegerField( 7, 7 )]
        public int DataSize { get; set; }

        /// <summary>
        /// Field 8 Size 1 IntegerField
        /// </summary>
        [IntegerField( 8, 1 )]
        public int SideIndicator { get; set; }

        /// <summary>
        /// Field 9 Size 2 IntegerField
        /// </summary>
        [IntegerField( 9, 2 )]
        public int ViewDescriptor { get; set; }

        /// <summary>
        /// Field 10 Size 1 IntegerField
        /// </summary>
        [IntegerField( 10, 1 )]
        public int? DigitalSignatureIndicator { get; set; }

        /// <summary>
        /// Field 11 Size 2 IntegerField
        /// </summary>
        [IntegerField( 11, 2 )]
        public int? DigitalSignatureMethod { get; set; }

        /// <summary>
        /// Field 12 Size 5 IntegerField
        /// </summary>
        [IntegerField( 12, 5 )]
        public int? SecurityKeySize { get; set; }

        /// <summary>
        /// Field 13 Size 7 IntegerField
        /// </summary>
        [IntegerField( 13, 7 )]
        public int? StartOfProtectedData { get; set; }

        /// <summary>
        /// Field 14 Size 7 IntegerField
        /// </summary>
        [IntegerField( 14, 7 )]
        public int? LengthOfProtectedData { get; set; }

        /// <summary>
        /// Field 15 Size 1 IntegerField
        /// </summary>
        [IntegerField( 15, 1 )]
        public int? ImageRecreateIndicator { get; set; }

        /// <summary>
        /// Field 16 Size 8 TextField
        /// </summary>
        [TextField( 16, 8 )]
        public string UserField { get; set; }

        /// <summary>
        /// Field 17 Size 15 TextField
        /// </summary>
        [TextField( 17, 15 )]
        public string Reserved { get; set; }

        /// <summary>
        /// Construct Record Type 50
        /// </summary>
        public ImageViewDetail()
            : base( 50 )
        {
        }
    }
}
