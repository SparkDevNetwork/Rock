using System;

using com.bemaservices.RemoteCheckDeposit.Attributes;

namespace com.bemaservices.RemoteCheckDeposit.Records
{
    public class ImageViewData : Record
    {
        /// <summary>
        /// Field 2 Size 9 TextField
        /// </summary>
        [TextField( 2, 9 )]
        public string InstitutionRoutingNumber { get; set; }

        /// <summary>
        /// Field 3 DateField
        /// </summary>
        [DateField( 3 )]
        public DateTime BundleBusinessDate { get; set; }

        /// <summary>
        /// Field 4 Size 2 TextField
        /// </summary>
        [TextField( 4, 2 )]
        public string CycleNumber { get; set; }

        /// <summary>
        /// Field 5 Size 15 TextField FieldJustifiation.Right
        /// </summary>
        [TextField( 5, 15, FieldJustification.Right )]
        public string ClientInstitutionItemSequenceNumber { get; set; }

        /// <summary>
        /// Field 6 Size 16 TextField
        /// </summary>
        [TextField( 6, 16 )]
        public string SecurityOriginatorName { get; set; }

        /// <summary>
        /// Field 7 Size 16 TextField
        /// </summary>
        [TextField( 7, 16 )]
        public string SecurityAuthenticatorName { get; set; }

        /// <summary>
        /// Field 8 Size 16 TextField
        /// </summary>
        [TextField( 8, 16 )]
        public string SecurityKeyName { get; set; }

        /// <summary>
        /// Field 9 Size 1 IntegerField
        /// </summary>
        [IntegerField( 9, 1 )]
        public int? ClippingOrigin { get; set; }

        /// <summary>
        /// Field 10 Size 4 IntegerField
        /// </summary>
        [IntegerField( 10, 4 )]
        public int? ClippingCoordinateH1 { get; set; }

        /// <summary>
        /// Field 11 Size 4 IntegerField
        /// </summary>
        [IntegerField( 11, 4 )]
        public int? ClippingCoordinateH2 { get; set; }
        
        /// <summary>
        /// Field 12 Size 4 IntegerField
        /// </summary>
        [IntegerField( 12, 4 )]
        public int? ClippingCoordinateV1 { get; set; }

        /// <summary>
        /// Field 13 Size 4 IntegerField
        /// </summary>
        [IntegerField( 13, 4 )]
        public int? ClippingCoordinateV2 { get; set; }

        /// <summary>
        /// Field 14 Size 4 Variable(x)
        /// </summary>
        [VariableTextField( 14, 4 )]
        public string ImageReferenceKey { get; set; }

        /// <summary>
        /// Field 16 Size 5 VariableBinary
        /// </summary>
        [VariableBinaryField( 16, 5 )]
        public byte[] DigitalSignature { get; set; }

        /// <summary>
        /// Field 17 Size 7 VariableBinaryField
        /// </summary>
        [VariableBinaryField( 17, 7 )]
        public byte[] ImageData { get; set; }

        /// <summary>
        /// Construct Record Type 52
        /// </summary>
        public ImageViewData()
            : base( 52 )
        {
        }
    }
}
