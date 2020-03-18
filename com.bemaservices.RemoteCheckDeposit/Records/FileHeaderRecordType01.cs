
using System;
using com.bemaservices.RemoteCheckDeposit.Attributes;

namespace com.bemaservices.RemoteCheckDeposit.Records
{
    public class FileHeaderRecordType01 : Record
    {
        /// <summary>
        /// Field 2 Size 2 IntegerField
        /// </summary>
        [IntegerField(2, 2)]
        public int StandardLevel { get; set; }

        /// <summary>
        /// Field 3 Size 1 TextField
        /// </summary>
        [TextField(3, 1)]
        public string FileTypeIndicator { get; set; }

        /// <summary>
        /// Field 4 Size 9 TextField
        /// </summary>
        [TextField(4, 9)]
        public string ImmediateDestinationRoutingNumber { get; set; }

        /// <summary>
        /// Field 5 Size 9 TextField
        /// </summary>
        [TextField( 5, 9, FieldJustification.Right)]
        public string ImmediateOriginRoutingNumber { get; set; }

        /// <summary>
        /// Field 6 DateTimeField
        /// </summary>
        [DateTimeField(6)]
        public DateTime FileCreationDateTime { get; set; }

        /// <summary>
        /// Field 8 Size 1 TextField
        /// </summary>
        [TextField(8, 1)]
        public string ResendIndicator { get; set; }

        /// <summary>
        /// Field 9 Size 18 TextField
        /// </summary>
        [TextField(9, 18)]
        public string ImmediateDestinationName { get; set; }

        /// <summary>
        /// Field 10 Size 18 TextField
        /// </summary>
        [TextField(10, 18)]
        public string ImmediateOriginName { get; set; }

        /// <summary>
        /// Field 11 Size 1 TextField
        /// </summary>
        [TextField(11, 1)]
        public string FileIdModifier { get; set; }

        /// <summary>
        /// Field 12 Size 2 TextField
        /// </summary>
        [TextField(12, 2)]
        public string CountryCode { get; set; }

        /// <summary>
        /// Field 13 Size 4 TextField
        /// </summary>
        [TextField(13, 4)]
        public string UserField { get; set; }

        /// <summary>
        /// Field 14 Size 1 TextField
        /// </summary>
        [TextField(14, 1)]
        protected string Reserved { get; set; }


        /// <summary>
        /// Construct Record Type 01
        /// </summary>
        public FileHeaderRecordType01()
            : base(01)
        {
        }
    }
}
