//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

namespace Rock.SystemGuid
{
    /// <summary>
    /// Static Guids used by the Rock ChMS application
    /// </summary>
    public static class DefinedValue
    {
        #region CheckIn Search Type

        /// <summary>
        /// Phone number search type
        /// 
        /// </summary>
        public const string CHECKIN_SEARCH_TYPE_PHONE_NUMBER = "F3F66040-C50F-4D13-9652-780305FFFE23";

        /// <summary>
        /// Barcode Search Type
        /// </summary>
        public const string CHECKIN_SEARCH_TYPE_BARCODE = "9A66BFCD-0F16-4EAE-BE35-B3FAF4B817BE";

        #endregion

        #region Device Type

        /// <summary>
        /// Check-in Kiosk device type
        /// </summary>
        public const string DEVICE_TYPE_CHECKIN_KIOSK = "BC809626-1389-4543-B8BB-6FAC79C27AFD";

        /// <summary>
        /// Giving Kiosk device type
        /// </summary>
        public const string DEVICE_TYPE_GIVING_KIOSK = "64A1DBE5-10AD-42F1-A9BA-646A781D4112";

        /// <summary>
        /// Printer device type
        /// </summary>
        public const string DEVICE_TYPE_PRINTER = "8284B128-E73B-4863-9FC2-43E6827B65E6";

        /// <summary>
        /// Digital signage device type
        /// </summary>
        public const string DEVICE_TYPE_DIGITAL_SIGNAGE = "01B585B1-389D-4C86-A9DA-267A8564699D";

        #endregion

        #region Note Type

        /// <summary>
        /// Manually entered note.
        /// </summary>
        public const string NOTE_TYPE_MANUAL_NOTE = "4318E9AC-B669-4AF7-AF88-EF580FC43C6A";

        #endregion

        #region Person Phone Type

        /// <summary>
        /// Person Primary Phone
        /// </summary>
        public const string PERSON_PHONE_TYPE_PRIMARY = "407E7E45-7B2E-4FCD-9605-ECB1339F2453";

        /// <summary>
        /// Person Secondary Phone
        /// </summary>
        public const string PERSON_PHONE_TYPE_SECONDARY = "AA8732FB-2CEA-4C76-8D6D-6AAA2C6A4303";

        #endregion

        #region Person Record Status

        /// <summary>
        /// Active Record Status
        /// </summary>
        public const string PERSON_RECORD_STATUS_ACTIVE = "618F906C-C33D-4FA3-8AEF-E58CB7B63F1E";

        /// <summary>
        /// Inactive Record Status
        /// </summary>
        public const string PERSON_RECORD_STATUS_INACTIVE = "1DAD99D5-41A9-4865-8366-F269902B80A4";

        /// <summary>
        /// Pending Record Status
        /// </summary>
        public const string PERSON_RECORD_STATUS_PENDING = "283999EC-7346-42E3-B807-BCE9B2BABB49";

        #endregion

        #region Person Record Type

        /// <summary>
        /// Person Record Type
        /// </summary>
        public const string PERSON_RECORD_TYPE_PERSON = "36CF10D6-C695-413D-8E7C-4546EFEF385E";

        /// <summary>
        /// Business Record Type
        /// </summary>
        public const string PERSON_RECORD_TYPE_BUSINESS = "BF64ADD3-E70A-44CE-9C4B-E76BBED37550";

        #endregion

    }
}