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
        #region Device Type

        /// <summary>
        /// Check-in Kiosk device type
        /// </summary>
        public static Guid DEVICE_TYPE_CHECKIN_KIOSK { get { return new Guid( "BC809626-1389-4543-B8BB-6FAC79C27AFD" ); } }

        /// <summary>
        /// Check-in Kiosk device type
        /// </summary>
        public static Guid DEVICE_TYPE_GIVING_KIOSK { get { return new Guid( "64A1DBE5-10AD-42F1-A9BA-646A781D4112" ); } }

        /// <summary>
        /// Check-in Kiosk device type
        /// </summary>
        public static Guid DEVICE_TYPE_PRINTER { get { return new Guid( "8284B128-E73B-4863-9FC2-43E6827B65E6" ); } }

        /// <summary>
        /// Check-in Kiosk device type
        /// </summary>
        public static Guid DEVICE_TYPE_DIGITAL_SIGNAGE { get { return new Guid( "01B585B1-389D-4C86-A9DA-267A8564699D" ); } }

        #endregion
        
        #region Person Phone Type

        /// <summary>
        /// Person Primary Phone
        /// </summary>
        public static Guid PERSON_PHONE_TYPE_PRIMARY { get { return new Guid( "407E7E45-7B2E-4FCD-9605-ECB1339F2453" ); } }

        /// <summary>
        /// Person Secondary Phone
        /// </summary>
        public static Guid PERSON_PHONE_TYPE_SECONDARY { get { return new Guid( "AA8732FB-2CEA-4C76-8D6D-6AAA2C6A4303" ); } }

        #endregion

        #region Person Record Status

        /// <summary>
        /// Active Record Status
        /// </summary>
        public static Guid PERSON_RECORD_STATUS_ACTIVE { get { return new Guid( "618F906C-C33D-4FA3-8AEF-E58CB7B63F1E" ); } }

        /// <summary>
        /// Inactive Record Status
        /// </summary>
        public static Guid PERSON_RECORD_STATUS_INACTIVE { get { return new Guid( "1DAD99D5-41A9-4865-8366-F269902B80A4" ); } }

        /// <summary>
        /// Pending Record Status
        /// </summary>
        public static Guid PERSON_RECORD_STATUS_PENDING { get { return new Guid( "283999EC-7346-42E3-B807-BCE9B2BABB49" ); } }

        #endregion

        #region Person Record Type

        /// <summary>
        /// Person Record Type
        /// </summary>
        public static Guid PERSON_RECORD_TYPE_PERSON { get { return new Guid( "36CF10D6-C695-413D-8E7C-4546EFEF385E" ); } }

        /// <summary>
        /// Business Record Type
        /// </summary>
        public static Guid PERSON_RECORD_TYPE_BUSINESS { get { return new Guid( "BF64ADD3-E70A-44CE-9C4B-E76BBED37550" ); } }

        #endregion

    }
}