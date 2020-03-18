using System;
using com.bemaservices.RemoteCheckDeposit.Attributes;

namespace com.bemaservices.RemoteCheckDeposit.Records
{
    public class ImageViewDataRecordType52 : Record
    {
        /// <summary>
        /// Field 2 Size 9 IntegerField
        /// </summary>
        [IntegerField(2, 9)]
        public int BankOfAmericaClientId { get; set; }

        [DateField(3)]
        public DateTime BundleBusinessDate { get; set; }

        /// <summary>
        /// Field 4 Size 2 TextField
        /// </summary>
        [TextField(4, 2)]
        public string CycleNumber { get; set; }

        /// <summary>
        /// Field 5 Size 15 TextField
        /// </summary>
        [TextField(5, 15)]
        public string ItemSequenceNumber { get; set; }

        /// <summary>
        /// Field 6 Size 16 TextField
        /// </summary>
        [TextField(6, 16)]
        public string SecurityOriginatorName { get; set; }

        /// <summary>
        /// Field 7 Size 16 TextField
        /// </summary>
        [TextField(7, 16)]
        public string SecurityAuthenticatorName { get; set; }

        /// <summary>
        /// Field 8 Size 16 TextField
        /// </summary>
        [TextField(8, 16)]
        public string SecurityKeyName { get; set; }

        /// <summary>
        /// Field 9 Size 1 TextField
        /// </summary>
        [TextField(9, 1)]
        public string ClippingOrigin { get; set; }

        /// <summary>
        /// Field 10 Size 4 TextField
        /// </summary>
        [TextField(10, 4)]
        public string ClippingCoordinateH1 { get; set; }

        /// <summary>
        /// Field 11 Size 4 TextField
        /// </summary>
        [TextField(11, 4)]
        public string ClippingCoordinateH2 { get; set; }

        /// <summary>
        /// Field 12 Size 4 TextField
        /// </summary>
        [TextField(12, 4)]
        public string ClippingCoordinateV1 { get; set; }

        /// <summary>
        /// Field 13 Size 4 TextField
        /// </summary>
        [TextField(13, 4)]
        public string ClippingCoordinateV2 { get; set; }

        /// <summary>
        /// Field 14 Size 4 TextField
        /// </summary>
        ///////[TextField(14, 4)]
        ///////public string LengthOfImageReferenceKey { get; set; }

        /// <summary>
        /// Field 15 Size 4 VariableBinary
        /// </summary>
        [VariableBinaryField(15, 4)]
        public byte[] ImageReferenceKey { get; set; }

        /// <summary>
        /// Field 16 Size 5 VariableBinaryField
        /// </summary>
        //[TextField(16, 5)]
        //public string LengthOfDigitalSignature { get; set; }

        /// <summary>
        /// Field 17 Size 5 VariableBinaryField
        /// </summary>
        [VariableBinaryField(17, 5)]
        public byte[] DigitalSignature { get; set; }

        /// <summary>
        /// Field 18 Size 7 IntegerField
        /// </summary>
        //[IntegerField(18, 7)]
        //public int LengthOfImageData { get; set; }

        /// <summary>
        /// Field 19 Size 7 VariableBinaryField
        /// </summary>
        [VariableBinaryField(19, 7)]
        public byte[] ImageData { get; set; }

        /// <summary>
        /// Construct Record Type 52
        /// </summary>
        /// <param name="recordType"></param>
        public ImageViewDataRecordType52() :
            base(52)
        {
        }
    }
}