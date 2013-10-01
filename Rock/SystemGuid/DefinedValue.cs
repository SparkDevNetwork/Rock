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
        #region Communication Channel Type

        /// <summary>
        /// Email communication
        /// </summary>
        public const string COMMUNICATION_CHANNEL_EMAIL = "FC51461D-0C31-4C6B-A7C8-B3E8482C1055";

        #endregion

        #region Check-in Search Type
         
        /// <summary>
        /// Phone number search type
        /// 
        /// </summary>
        public const string CHECKIN_SEARCH_TYPE_PHONE_NUMBER = "F3F66040-C50F-4D13-9652-780305FFFE23";

        /// <summary>
        /// Barcode Search Type
        /// </summary>
        public const string CHECKIN_SEARCH_TYPE_BARCODE = "9A66BFCD-0F16-4EAE-BE35-B3FAF4B817BE";

        /// <summary>
        /// Name Search Type
        /// </summary>
        public const string CHECKIN_SEARCH_TYPE_NAME = "071D6DAA-3063-463A-B8A1-7D9A1BE1BB31";
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

        #endregion

        #region Financial Currency Type

        /// <summary>
        /// Cash
        /// </summary>
        public const string CURRENCY_TYPE_CASH =	"F3ADC889-1EE8-4EB6-B3FD-8C10F3C8AF93";

        /// <summary>
        /// Check
        /// </summary>
        public const string CURRENCY_TYPE_CHECK =	"8B086A19-405A-451F-8D44-174E92D6B402";

        /// <summary>
        /// Credit Card
        /// </summary>
        public const string CURRENCY_TYPE_CREDIT_CARD = "928A2E04-C77B-4282-888F-EC549CEE026A";

        /// <summary>
        /// ACH
        /// </summary>
        public const string CURRENCY_TYPE_ACH = "DABEE8FD-AEDF-43E1-8547-4C97FA14D9B6";

        #endregion

        #region Financial Transaction Type

        /// <summary>
        /// A Contribution Transaction
        /// </summary>
        public const string TRANSACTION_TYPE_CONTRIBUTION = "2D607262-52D6-4724-910D-5C6E8FB89ACC";

        /// <summary>
        /// An Event Registration Transaction
        /// </summary>
        public const string TRANSACTION_TYPE_EVENT_REGISTRATION = "33CB96DD-8752-4BEE-A142-88DB7DE538F0";
               
        #endregion

        #region Financial Transaction Image Type

        /// <summary>
        /// Front of Check
        /// </summary>
        public const string TRANSACTION_IMAGE_TYPE_CHECK_FRONT = "A52EDD34-D3A2-420F-AF45-21B323FB21D6";

        /// <summary>
        /// Back of Check
        /// </summary>
        public const string TRANSACTION_IMAGE_TYPE_CHECK_BACK = "87D9347D-64E6-4DD4-8F05-2AA17419B5E8";

        /// <summary>
        /// Front of Envelope
        /// </summary>
        public const string TRANSACTION_IMAGE_TYPE_ENVELOPE_FRONT = "654ABEC4-7414-402F-BEA4-0AA833683AD6";

        /// <summary>
        /// Back of Envelope
        /// </summary>
        public const string TRANSACTION_IMAGE_TYPE_ENVELOPE_BACK = "746FBD46-AA4C-4A84-A7DA-080763CED187";

        #endregion

        #region Group Type Purpose

        /// <summary>
        /// Group Type Purpose of Check-in Template
        /// </summary>
        public const string GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE = "4A406CB0-495B-4795-B788-52BDFDE00B01";

        /// <summary>
        /// Group Type Purpose of Check-in Filter
        /// </summary>
        public const string GROUPTYPE_PURPOSE_CHECKIN_FILTER = "6BCED84C-69AD-4F5A-9197-5C0F9C02DD34";

        #endregion

        #region Transaction Frequency Type

        /// <summary>
        /// One Time
        /// </summary>
        public const string TRANSACTION_FREQUENCY_ONE_TIME = "82614683-7FB4-4F16-9087-6F85945A7B16";

        /// <summary>
        /// Weekly
        /// </summary>
        public const string TRANSACTION_FREQUENCY_WEEKLY = "35711E44-131B-4534-B0B2-F0A749292362";

        /// <summary>
        /// Every two weeks
        /// </summary>
        public const string TRANSACTION_FREQUENCY_BIWEEKLY = "72990023-0D43-4554-8D32-28461CAB8920";

        /// <summary>
        /// Twice a month
        /// </summary>
        public const string TRANSACTION_FREQUENCY_TWICEMONTHLY = "791C863D-2600-445B-98F8-3E5B66A3DEC4";

        /// <summary>
        /// Monthly
        /// </summary>
        public const string TRANSACTION_FREQUENCY_MONTHLY = "1400753C-A0F9-4A45-8A1D-81C98450BD1F";

        /// <summary>
        /// Monthly
        /// </summary>
        public const string TRANSACTION_FREQUENCY_QUARTERLY = "BF08EA03-C52A-4364-B142-12EBCA7CA14A";

        /// <summary>
        /// Twice a year (every 6 months)
        /// </summary>
        public const string TRANSACTION_FREQUENCY_TWICEYEARLY = "691BB8AB-5F96-4E88-847C-CB970D9E87FA";

        /// <summary>
        /// Yearly
        /// </summary>
        public const string TRANSACTION_FREQUENCY_YEARLY = "AC88C37A-901E-4CBB-947B-11348C208192";

        #endregion

        #region Note Type

        /// <summary>
        /// Manually entered note.
        /// </summary>
        public const string NOTE_TYPE_MANUAL_NOTE = "4318E9AC-B669-4AF7-AF88-EF580FC43C6A";

        #endregion

        #region Person Marital Status

        /// <summary>
        /// Marital Status of Married
        /// Note:  Single is also a DefinedValue for Marital Status but it is not IsSystem, so we won't include it as a const
        /// </summary>
        public const string PERSON_MARITAL_STATUS_MARRIED = "5FE5A540-7D9F-433E-B47E-4229D1472248";

        #endregion

        #region Person Phone Type

        /// <summary>
        /// Person Mobile Phone
        /// </summary>
        public const string PERSON_PHONE_TYPE_MOBILE = "407E7E45-7B2E-4FCD-9605-ECB1339F2453";

        /// <summary>
        /// Person Home Phone
        /// </summary>
        public const string PERSON_PHONE_TYPE_HOME = "AA8732FB-2CEA-4C76-8D6D-6AAA2C6A4303";

        /// <summary>
        /// Person Home Phone
        /// </summary>
        public const string PERSON_PHONE_TYPE_WORK = "2CC66D5A-F61C-4B74-9AF9-590A9847C13C";

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

        #region Location Type

        /// <summary>
        /// Home location type
        /// </summary>
        public const string LOCATION_TYPE_HOME = "8C52E53C-2A66-435A-AE6E-5EE307D9A0DC";

        /// <summary>
        /// Work Record Type
        /// </summary>
        public const string LOCATION_TYPE_WORK = "E071472A-F805-4FC4-917A-D5E3C095C35C";

        /// <summary>
        /// Work Record Type
        /// </summary>
        public const string LOCATION_TYPE_PREVIOUS = "853D98F1-6E08-4321-861B-520B4106CFE0";

        #endregion

        #region Person Status

        /// <summary>
        /// Member Person Status
        /// </summary>
        public const string PERSON_STATUS_MEMBER = "E8848110-CDE3-400E-B6CD-C2BD309FAF38";

        /// <summary>
        /// Visitor Person Status
        /// </summary>
        public const string PERSON_STATUS_VISITOR = "1B439C81-68B1-44F1-8DDF-0B555823D0F8";

        #endregion
    }
}