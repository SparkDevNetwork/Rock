// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;

namespace Rock.SystemGuid
{
    /// <summary>
    /// Static Guids used by the Rock application
    /// </summary>
    public static class DefinedValue
    {
        #region Attendance

        /// <summary>
        /// The attendance record came from a legacy kiosk.
        /// </summary>
        public const string ATTENDANCE_SOURCE_LEGACY_KIOSK = "0B25A627-679B-4B73-AA84-305DEF24815A";

        /// <summary>
        /// The attendance record came from a kiosk.
        /// </summary>
        public const string ATTENDANCE_SOURCE_KIOSK = "9A21D7EB-BCB5-4466-B62F-70EC5008B6B9";

        /// <summary>
        /// The attendance record came from a mobile check-in.
        /// </summary>
        public const string ATTENDANCE_SOURCE_MOBILE = "972E8841-A3C9-4DDD-BD24-A414C0290331";

        #endregion

        #region Block Template

        /// <summary>
        /// The block template for the mobile notes block.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_GROUP_MEMBERS = "89322C87-CA36-4169-9361-FDA4EFEF07C1";

        /// <summary>
        /// The block template for the mobile notes block.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_NOTES = "37F44005-C4D8-4527-BA86-B6F8B72BF243";

        /// <summary>
        /// The block template for the mobile my notes block.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_MY_NOTES = "652A4E12-6C15-407D-9620-ED39190899A5";

        /// <summary>
        /// The block template for the mobile group schedule preference landing page.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_SIGNUP_LANDING_PAGE = "7F89AE4D-BD8B-49ED-B8EC-F883D43505F2";

        /// <summary>
        /// The block template for the mobile group schedule preference landing page.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_PREFERENCE_LANDING_PAGE = "D39CCB65-2444-48E4-9DE1-7A01AB20CB61";

        /// <summary>
        /// The block template for mobile group schedule unavailability block. 
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_UNAVAILABILITY = "48EE4803-66BE-43A0-A7CF-E2D669DB2D21";

        /// <summary>
        /// The block template for mobile group schedule toolbox block.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_TOOLBOX = "7E11BEF1-F6F7-49E3-8804-90AC1CB9AD25";

        /// <summary>
        /// The block template for mobile group schedule toolbox decline modal.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_TOOLBOX_DECLINE_MODAL = "68ACFE34-C1D9-40C6-9AB1-9E6F4DA846EE";

        /// <summary>
        /// The block template for mobile answer to prayer block.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_ANSWER_TO_PRAYER = "D13256E3-D9ED-45C2-8EF7-C4AABCF4B2B7";

        /// <summary>
        /// The block template for the event template on the mobile calendar event list.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_CALENDAR_EVENT_LIST = "248587C7-5CE3-46B7-8728-2E03E725D0B2";

        /// <summary>
        /// The block template for the event template on the mobile calendar item view.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_CALENDAR_EVENT_ITEM_OCCURRENCE_VIEW = "128F7350-97FD-4ECA-9C79-D02DE0C434EB";

        /// <summary>
        /// The block template for the mobile communication view
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_COMMUNICATION_VIEW = "0D588D84-111C-4350-98DE-460C194F5DE5";

        /// <summary>
        /// The block template for the Mobile > Connection > Connection Type List block.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_TYPE_LIST = "FECAD08A-570F-434E-BEED-412C4F2A3159";

        /// <summary>
        /// The block template for the Mobile > Connection > Connection Opportunity List block.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_OPPORTUNITY_LIST = "167465D9-E8D2-413C-BA99-FDA529B14E6C";

        /// <summary>
        /// The block template for the Mobile > Connection > Connection Request List block.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_REQUEST_LIST = "4A45926C-5571-414C-8C69-8203FDBF4AE7";

        /// <summary>
        /// The block template for the Mobile > Connection > Connection Request Detail block.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_REQUEST_DETAIL = "44538702-5157-489A-B256-217F5D1F53F4";

        /// <summary>
        /// The block template for <see cref="Rock.Blocks.Types.Mobile.Core.Search"/> block.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_CORE_SEARCH = "CFEB7FA6-0F19-496B-8DCC-4D200AEEE866";

        /// <summary>
        /// The block template mobile daily challenge entry
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_DAILY_CHALLENGE_ENTRY = "30ECA4B6-0869-4656-A4CD-B8729CB29E76";

        /// <summary>
        /// The block template mobile event item occurrence list by audience
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_EVENT_ITEM_OCCURRENCE_LIST_BY_AUDIENCE = "26944B71-7B69-4943-8EC2-3506F728D943";

        /// <summary>
        /// The block template for the mobile group member list.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_GROUP_MEMBER_LIST = "E5618730-9E50-4BDA-9E13-D27697F83980";

        /// <summary>
        /// The block template for the mobile group member view.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_GROUP_MEMBER_VIEW = "09053C7C-9374-4489-8A7B-71F02E3E7D89";

        /// <summary>
        /// The block template for the mobile group view.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_GROUP_VIEW = "559346FB-C684-42CF-8F4C-CF4A1C278AD6";

        /// <summary>
        /// The block template for the mobile group finder.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_GROUPS_GROUP_FINDER = "1B6D540A-2E40-44F6-8AE9-2857DE913459";

        /// <summary>
        /// The block template for the live experience occurrences block.
        /// </summary>
        public const string BLOCK_TEMPLATE_LIVE_EXPERIENCE_OCCURRENCES = "95837896-BB45-47FA-8517-F1C0EFE2A89C";

        /// <summary>
        /// The block template for mobile my prayer requests.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_MY_PRAYER_REQUESTS = "198F3006-5F0F-48AB-9EA0-2FA56F633753";

        /// <summary>
        /// The block template for Mobile > Prayer > Prayer Card View block.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_PRAYER_PRAYER_CARD_VIEW = "DCFECA8C-CDF5-4863-851E-7EFEE6758EE9";

        /// <summary>
        /// The block template for the mobile prayer session block.
        /// </summary>
        public const string BLOCK_TEMPLATE_MOBILE_PRAYER_SESSION = "6F1F6BAB-B403-48D1-BF6A-52B16361279C";

        #endregion Block Template

        #region Button Styles

        /// <summary>
        /// Button HTML - Primary 
        /// </summary>
        public const string BUTTON_HTML_PRIMARY = "FDC397CD-8B4A-436E-BEA1-BCE2E6717C03";

        /// <summary>
        /// Button HTML - Secondary 
        /// </summary>
        public const string BUTTON_HTML_SECONDARY = "8CF6E927-4FA5-4241-991C-391038B79631";

        /// <summary>
        /// Button HTML - Deny 
        /// </summary>
        public const string BUTTON_HTML_DENY = "D6B809A9-C1CC-4EBB-816E-33D8C1E53EA4";

        /// <summary>
        /// Button HTML - Approve 
        /// </summary>
        public const string BUTTON_HTML_APPROVE = "C88FEF94-95B9-444A-BC93-58E983F3C047";

        /// <summary>
        /// Button HTML - Success 
        /// </summary>
        public const string BUTTON_HTML_SUCCESS = "53CA2CB9-8BFA-450C-A3AA-FD3F3FD3BC8A";

        /// <summary>
        /// Button HTML - Info 
        /// </summary>
        public const string BUTTON_HTML_INFO = "3C026B37-29D4-47CB-BB6E-DA43AFE779FE";

        /// <summary>
        /// Button HTML - Warning 
        /// </summary>
        public const string BUTTON_HTML_WARNING = "F03C9591-C497-4E27-A714-6A482E745141";

        /// <summary>
        /// Button HTML - Danger 
        /// </summary>
        public const string BUTTON_HTML_DANGER = "9B329020-E074-4326-8831-9DD534F491DF";

        /// <summary>
        /// Button HTML - Default 
        /// </summary>
        public const string BUTTON_HTML_DEFAULT = "638BEEE0-2F8F-4706-B9A4-5BAB70386697";

        /// <summary>
        /// Button HTML - Cancel 
        /// </summary>
        public const string BUTTON_HTML_CANCEL = "5683E775-B9F3-408C-80AC-94DE0E51CF3A";

        #endregion Button Styles

        #region Campus

        /// <summary>
        /// The campus status pending
        /// </summary>
        public const string CAMPUS_STATUS_PENDING = "008209F5-144E-4282-92C3-944A4AC78700";

        /// <summary>
        /// The campus status open
        /// </summary>
        public const string CAMPUS_STATUS_OPEN = "10696FD8-D0C7-486F-B736-5FB3F5D69F1A";

        /// <summary>
        /// The campus status closed
        /// </summary>
        public const string CAMPUS_STATUS_CLOSED = "99D2C9CB-B6DC-49C1-B626-B76BD398B63A";

        /// <summary>
        /// The campus type physical
        /// </summary>
        public const string CAMPUS_TYPE_PHYSICAL = "5A61507B-79CB-4DA2-AF43-6F82260203B3";

        /// <summary>
        /// The campus type on-line
        /// </summary>
        public const string CAMPUS_TYPE_ONLINE = "10101010-2DB4-4C95-B07D-C400E412289B";

        #endregion Campus

        #region Communication Medium Type

        /// <summary>
        /// Email communication
        /// </summary>
        public const string COMMUNICATION_MEDIUM_EMAIL = "FC51461D-0C31-4C6B-A7C8-B3E8482C1055";

        #endregion

        #region Check-in Search Type

        /// <summary>
        /// Phone number search type
        /// </summary>
        public const string CHECKIN_SEARCH_TYPE_PHONE_NUMBER = "F3F66040-C50F-4D13-9652-780305FFFE23";

        /// <summary>
        /// Name Search Type
        /// </summary>
        public const string CHECKIN_SEARCH_TYPE_NAME = "071D6DAA-3063-463A-B8A1-7D9A1BE1BB31";

        /// <summary>
        /// Power Search Type
        /// </summary>
        public const string CHECKIN_SEARCH_TYPE_NAME_AND_PHONE = "93773B0A-6E7F-1AA0-4F1D-9A4D6ACE930F";

        /// <summary>
        /// Scanned Id Search Type. This type is always supported and if a BarCode or Bio id is scanned, the attendance will be updated to reflect this type OS search was used.
        /// </summary>
        public const string CHECKIN_SEARCH_TYPE_SCANNED_ID = "7668CE15-E372-47EE-8FF8-6FEE09F7C858";

        /// <summary>
        /// Family Id Search Type. This type is for future (face recognition) support and is not currently being used.
        /// </summary>
        public const string CHECKIN_SEARCH_TYPE_FAMILY_ID = "111385BB-DAEB-4CE3-A945-0B50DC15EE02";

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
        /// Proxy device type
        /// </summary>
        [RockObsolete( "1.16.7" )]
        [Obsolete( "Use DEVICE_TYPE_CLOUD_PRINT_PROXY instead." )]
        public const string DEVICE_TYPE_PROXY = "EF5BF148-C9E0-4F96-8152-BB23CD85D845";

        /// <summary>
        /// Cloud Print Proxy device type
        /// </summary>
        public const string DEVICE_TYPE_CLOUD_PRINT_PROXY = "EF5BF148-C9E0-4F96-8152-BB23CD85D845";

        #endregion

        #region Entity Set Purpose

        /// <summary>
        /// Entity Set for the purpose of doing a Person Merge request
        /// </summary>
        public const string ENTITY_SET_PURPOSE_PERSON_MERGE_REQUEST = "214EB26F-5493-4540-B2EF-F0887C8FBB9E";

        #endregion

        #region External Link Types

        /// <summary>
        /// Barnes and Noble External Link Type
        /// </summary>
        public const string EXTERNAL_LINK_TYPE_BARNES_AND_NOBLE = "ACAC304A-A760-4313-BD7D-F272C2A8BC5B";

        /// <summary>
        /// Amazon External Link Type
        /// </summary>
        public const string EXTERNAL_LINK_TYPE_AMAZON = "BB3E2119-3CF1-4B20-958E-304BFFF120B1";

        /// <summary>
        /// ChristianBook External Link Type
        /// </summary>
        public const string EXTERNAL_LINK_TYPE_CHRISTIAN_BOOK = "EE74B3D2-1B5A-4436-9908-ABC63FF59309";

        /// <summary>
        /// YouTube Video External Link Type
        /// </summary>
        public const string EXTERNAL_LINK_TYPE_YOUTUBE_VIDEO = "FE7A6DE2-206C-420F-B67E-7139BB9B8B6D";

        #endregion

        #region Financial Currency Type

        /// <summary>
        /// Cash
        /// </summary>
        public const string CURRENCY_TYPE_CASH = "F3ADC889-1EE8-4EB6-B3FD-8C10F3C8AF93";

        /// <summary>
        /// Check
        /// </summary>
        public const string CURRENCY_TYPE_CHECK = "8B086A19-405A-451F-8D44-174E92D6B402";

        /// <summary>
        /// Credit Card
        /// </summary>
        public const string CURRENCY_TYPE_CREDIT_CARD = "928A2E04-C77B-4282-888F-EC549CEE026A";

        /// <summary>
        /// ACH
        /// </summary>
        public const string CURRENCY_TYPE_ACH = "DABEE8FD-AEDF-43E1-8547-4C97FA14D9B6";

        /// <summary>
        /// Non-Cash
        /// </summary>
        public const string CURRENCY_TYPE_NONCASH = "7950FF66-80EE-E8AB-4A77-4A13EDEB7513";

        /// <summary>
        /// Unknown Currency Type
        /// </summary>
        public const string CURRENCY_TYPE_UNKNOWN = "56C9AE9C-B5EB-46D5-9650-2EF86B14F856";

        /// <summary>
        /// Other Currency Type
        /// </summary>
        public const string CURRENCY_TYPE_OTHER = "0FDF0BB3-B483-4C0A-9DFF-A35ABE3B688D";

        /// <summary>
        /// Apple Pay Currency Type
        /// </summary>
        public const string CURRENCY_TYPE_APPLE_PAY = "D42C4DF7-1AE9-4DDE-ADA2-774B866B798C";

        /// <summary>
        /// Android Pay Currency Type
        /// </summary>
        public const string CURRENCY_TYPE_ANDROID_PAY = "6151F6E0-3223-46BA-A59E-E091BE4AF75C";

        #endregion Financial Currency Type

        #region Credit Card Types

        /// <summary>
        /// The credit card type AMEX
        /// </summary>
        public const string CREDITCARD_TYPE_AMEX = "696A54E3-352C-49FB-88A1-BCDBD81AA9EC";

        /// <summary>
        /// The credit card type diners club
        /// </summary>
        public const string CREDITCARD_TYPE_DINERS_CLUB = "1A9A4DB9-AFF3-4773-875C-C10346BD1CA7";

        /// <summary>
        /// The credit card type discover
        /// </summary>
        public const string CREDITCARD_TYPE_DISCOVER = "4B746601-E9EB-4660-BA13-C0B66B24E248";

        /// <summary>
        /// The credit card type JCB
        /// </summary>
        public const string CREDITCARD_TYPE_JCB = "4DD7F0C2-F6B7-4510-90E6-287ADC25FD05";

        /// <summary>
        /// The credit card type MasterCard
        /// </summary>
        public const string CREDITCARD_TYPE_MASTERCARD = "6373A4B6-4DCA-4EB6-9ADE-B30E8A7F8621";

        /// <summary>
        /// The credit card type visa
        /// </summary>
        public const string CREDITCARD_TYPE_VISA = "FC66B5F8-634F-4800-A60D-436964D27B64";

        #endregion

        #region Financial Non-Cash Asset Type

        /// <summary>
        /// Non-Cash Asset Type: Property.
        /// </summary>
        public const string NONCASH_ASSET_PROPERTY = "FF4E8D66-CFF2-4A96-AA30-19721884C373";

        /// <summary>
        /// Non-Cash Asset Type: Stocks and Bonds.
        /// </summary>
        public const string NONCASH_ASSET_STOCKSANDBONDS = "B29D7D89-357F-47F9-BE7B-52AFF3892007";

        /// <summary>
        /// Non-Cash Asset Type: Vehicles.
        /// </summary>
        public const string NONCASH_ASSET_VEHICLES = "C1DCBE74-88FE-4876-8943-5783499CBBE0";

        /// <summary>
        /// Non-Cash Asset Type: Other.
        /// </summary>
        public const string NONCASH_ASSET_OTHER = "3086AF9A-108B-47C8-B299-CECF53B9D1DF";

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

        /// <summary>
        /// A Transaction Type of Receipt
        /// NOTE: This is not a core transaction type. It'll get created if needed when importing transactions from Slingshot
        /// </summary>
        public const string TRANSACTION_TYPE_RECEIPT = "F57AAF36-F208-4A85-A078-E2B1F91798EB";

        #endregion

        #region Financial Source

        /// <summary>
        /// The financial source of bank check
        /// </summary>
        public const string FINANCIAL_SOURCE_TYPE_BANK_CHECK = "61E46A46-7399-4817-A6EC-3D8495E2316E";

        /// <summary>
        /// The financial source of Website
        /// </summary>
        public const string FINANCIAL_SOURCE_TYPE_WEBSITE = "7D705CE7-7B11-4342-A58E-53617C5B4E69";

        /// <summary>
        /// The financial source of Kiosk
        /// </summary>
        public const string FINANCIAL_SOURCE_TYPE_KIOSK = "260EEA80-821A-4F79-973F-49DF79C955F7";

        /// <summary>
        /// The financial source of Mobile Application
        /// </summary>
        public const string FINANCIAL_SOURCE_TYPE_MOBILE_APPLICATION = "8ADCEC72-63FC-4F08-A4CC-72BCE470172C";

        /// <summary>
        /// The financial source of On-Site
        /// </summary>
        public const string FINANCIAL_SOURCE_TYPE_ONSITE_COLLECTION = "BE7ECF50-52BC-4774-808D-574BA842DB98";

        /// <summary>
        /// The financial source of Text-to-Give (Give action of the SMS actions)
        /// </summary>
        public const string FINANCIAL_SOURCE_TYPE_SMS_GIVE = "8BA95E24-D291-499E-A535-4DCAC365689B";

        #endregion

        #region Group Location Type

        /// <summary>
        /// Address Type - Home 
        /// </summary>
        public const string GROUP_LOCATION_TYPE_HOME = "8C52E53C-2A66-435A-AE6E-5EE307D9A0DC";

        /// <summary>
        /// Address Type - Work
        /// </summary>
        public const string GROUP_LOCATION_TYPE_WORK = "E071472A-F805-4FC4-917A-D5E3C095C35C";

        /// <summary>
        /// Address Type - Previous
        /// </summary>
        public const string GROUP_LOCATION_TYPE_PREVIOUS = "853D98F1-6E08-4321-861B-520B4106CFE0";

        /// <summary>
        /// Meeting Location Type
        /// </summary>
        public const string GROUP_LOCATION_TYPE_MEETING_LOCATION = "96D540F5-071D-4BBD-9906-28F0A64D39C4";

        /// <summary>
        /// Some other type of Group Location
        /// NOTE: This is not a core group location type. It'll get created if needed when importing locations from Slingshot.
        /// </summary>
        public const string GROUP_LOCATION_TYPE_OTHER = "D49965C7-5254-4D2D-BC77-F390375F0C44";

        #endregion

        #region Group Type Purpose

        /// <summary>
        /// Group Type Purpose of Check-in Template (Weekly Service Check-in, etc)
        /// </summary>
        public const string GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE = "4A406CB0-495B-4795-B788-52BDFDE00B01";

        /// <summary>
        /// Group Type Purpose of Check-in Filter
        /// </summary>
        public const string GROUPTYPE_PURPOSE_CHECKIN_FILTER = "6BCED84C-69AD-4F5A-9197-5C0F9C02DD34";

        #endregion

        #region Group RSVP

        /// <summary>
        /// Group RSVP Decline Reason - Illness.
        /// </summary>
        public const string GROUP_RSVP_DECLINE_REASON_ILLNESS = "15B4124E-9717-4161-9974-86C4925053E3";

        /// <summary>
        /// Group RSVP Decline Reason - Vacation.
        /// </summary>
        public const string GROUP_RSVP_DECLINE_REASON_VACATION = "759E0745-727B-493D-A44C-3A042A11B761";

        /// <summary>
        /// Group RSVP Decline Reason - Schedule Conflict.
        /// </summary>
        public const string GROUP_RSVP_DECLINE_REASON_SCHEDULE_CONFLICT = "D5989FA4-2CE0-4427-83E4-E43F30045B43";

        /// <summary>
        /// Group RSVP Decline Reason - Childcare Not Available.
        /// </summary>
        public const string GROUP_RSVP_DECLINE_REASON_CHILDCARE = "0E728945-3D74-4E03-9799-0CF5EE8D6B3D";

        /// <summary>
        /// Group RSVP Decline Reason - Other.
        /// </summary>
        public const string GROUP_RSVP_DECLINE_REASON_OTHER = "1634AC71-0054-4F3C-A76D-A75A48E6BA56";

        #endregion

        #region Library Content Types

        /// <summary>
        /// Article Library Content Type
        /// </summary>
        public const string LIBRARY_CONTENT_TYPE_ARTICLE = "8B66EBAA-9BE4-42C8-A106-655A2EFD6109";

        #endregion

        #region Library License Types

        /// <summary>
        /// Author Attribution Library License Type
        /// </summary>
        public const string LIBRARY_LICENSE_TYPE_AUTHOR_ATTRIBUTION = "9AED8DEE-F74D-4F38-AD45-2423170D31D2";

        /// <summary>
        /// Open Library License Type
        /// </summary>
        public const string LIBRARY_LICENSE_TYPE_OPEN = "54D8921D-A9E9-46DA-8B7C-433C163FD41A";

        /// <summary>
        /// Organization Attribution Library License Type
        /// </summary>
        public const string LIBRARY_LICENSE_TYPE_ORGANIZATION_ATTRIBUTION = "577F2BD5-BFDF-41B7-96A8-32C0F1E44905";

        #endregion

        #region Location Types

        /// <summary>
        /// Campus Location Type
        /// </summary>
        public const string LOCATION_TYPE_CAMPUS = "C0D7AE35-7901-4396-870E-3AAF472AAE88";

        /// <summary>
        /// Building Location Type
        /// </summary>
        public const string LOCATION_TYPE_BUILDING = "D9646A93-1667-4A44-82DA-12E1229B4695";

        /// <summary>
        /// Room Location Type
        /// </summary>
        public const string LOCATION_TYPE_ROOM = "107C6DA1-266D-4E1C-A443-1CD37064601D";

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
        /// Twice a month on the 1st and 15th
        /// </summary>
        public const string TRANSACTION_FREQUENCY_TWICEMONTHLY = "791C863D-2600-445B-98F8-3E5B66A3DEC4";

        /// <summary>
        /// A Frequency of First and Fifteenth
        /// </summary>
        public const string TRANSACTION_FREQUENCY_FIRST_AND_FIFTEENTH = "C752403C-0F88-45CD-B574-069355C01D77";

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

        #region Metrics

        /// <summary>
        /// Metric values come from a DataView
        /// </summary>
        public const string METRIC_SOURCE_VALUE_TYPE_DATAVIEW = "2EC60BCF-EF63-4CCC-A970-F152292765D0";

        /// <summary>
        /// Metric values are entered manually
        /// </summary>
        public const string METRIC_SOURCE_VALUE_TYPE_MANUAL = "1D6511D6-B15D-4DED-B3C4-459CD2A7EC0E";

        /// <summary>
        /// Metric values are populated from custom SQL
        /// </summary>
        public const string METRIC_SOURCE_VALUE_TYPE_SQL = "6A1E1A1B-A636-4E12-B90C-D7FD1BDAE764";

        /// <summary>
        /// Metric values are populated from custom Lava
        /// </summary>
        public const string METRIC_SOURCE_VALUE_TYPE_LAVA = "2868A3E8-4632-4966-84CD-EDB8B775D66C";

        #endregion

        #region Person Marital Status

        /// <summary>
        /// Marital Status of Married
        /// </summary>
        public const string PERSON_MARITAL_STATUS_MARRIED = "5FE5A540-7D9F-433E-B47E-4229D1472248";

        /// <summary>
        /// Marital Status of Single
        /// </summary>
        public const string PERSON_MARITAL_STATUS_SINGLE = "F19FC180-FE8F-4B72-A59C-8013E3B0EB0D";

        /// <summary>
        /// Marital Status of Divorced
        /// </summary>
        public const string PERSON_MARITAL_STATUS_DIVORCED = "3B689240-24C2-434B-A7B9-A4A6CBA7928C";

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
        /// Record Status - Active
        /// </summary>
        public const string PERSON_RECORD_STATUS_ACTIVE = "618F906C-C33D-4FA3-8AEF-E58CB7B63F1E";

        /// <summary>
        /// Record Status - Inactive 
        /// </summary>
        public const string PERSON_RECORD_STATUS_INACTIVE = "1DAD99D5-41A9-4865-8366-F269902B80A4";

        /// <summary>
        /// Record Status - Pending
        /// </summary>
        public const string PERSON_RECORD_STATUS_PENDING = "283999EC-7346-42E3-B807-BCE9B2BABB49";

        #endregion

        #region Person Record Status Reason

        /// <summary>
        /// Record Status set to Inactive because person is Deceased 
        /// </summary>
        public const string PERSON_RECORD_STATUS_REASON_DECEASED = "05D35BC4-5816-4210-965F-1BF44F35A16A";

        /// <summary>
        /// Record Status set to Inactive because person has No Activity
        /// </summary>
        public const string PERSON_RECORD_STATUS_REASON_NO_ACTIVITY = "64014FE6-943D-4ACF-8014-FED9F9169AE8";

        /// <summary>
        /// Record Status set to Inactive because person moved
        /// </summary>
        public const string PERSON_RECORD_STATUS_REASON_MOVED = "3684CF84-F30F-4CE0-9EB9-D6AFFEA9B99B";

        #endregion

        #region Person Record Type

        /// <summary>
        /// Person Record Type - Person
        /// </summary>
        public const string PERSON_RECORD_TYPE_PERSON = "36CF10D6-C695-413D-8E7C-4546EFEF385E";

        /// <summary>
        /// Person Record Type - Business
        /// </summary>
        public const string PERSON_RECORD_TYPE_BUSINESS = "BF64ADD3-E70A-44CE-9C4B-E76BBED37550";

        /// <summary>
        /// Person Record Type - REST User
        /// </summary>
        public const string PERSON_RECORD_TYPE_RESTUSER = "E2261A84-831D-4234-9BE0-4D628BBE751E";

        /// <summary>
        /// Person Record Type - Nameless Person (usually due to SMS from unknown person)
        /// </summary>
        public const string PERSON_RECORD_TYPE_NAMELESS = "721300ED-1267-4DA0-B4F2-6C6B5B17B1C5";

        #endregion

        #region Person Connection Status

        /// <summary>
        /// Person Connection Status - Member
        /// </summary>
        public const string PERSON_CONNECTION_STATUS_MEMBER = "41540783-D9EF-4C70-8F1D-C9E83D91ED5F";

        /// <summary>
        /// Person Connection Status - Attendee
        /// </summary>
        public const string PERSON_CONNECTION_STATUS_ATTENDEE = "39F491C5-D6AC-4A9B-8AC0-C431CB17D588";

        /// <summary>
        /// Person Connection Status - Visitor
        /// </summary>
        public const string PERSON_CONNECTION_STATUS_VISITOR = "B91BA046-BC1E-400C-B85D-638C1F4E0CE2";

        /// <summary>
        /// Person Connection Status - Participant
        /// </summary>
        public const string PERSON_CONNECTION_STATUS_PARTICIPANT = "8EBC0CEB-474D-4C1B-A6BA-734C3A9AB061";

        /// <summary>
        /// Person Connection Status - Prospect
        /// </summary>
        public const string PERSON_CONNECTION_STATUS_PROSPECT = "368DD475-242C-49C4-A42C-7278BE690CC2";

        /// <summary>
        /// Person Connection Status - Web Prospect
        /// </summary>
        [Obsolete( "This is the same as the old Web Prospect but it is renamed to just Prospect." )]
        [RockObsolete( "1.13" )]
        public const string PERSON_CONNECTION_STATUS_WEB_PROSPECT = "368DD475-242C-49C4-A42C-7278BE690CC2";

        #endregion

        #region Person Review Reason

        /// <summary>
        /// Person needs review due to being Self-Inactivated
        /// </summary>
        public const string PERSON_REVIEW_REASON_SELF_INACTIVATED = "D539C356-6856-4E94-80B4-8FEA869AF38B";

        #endregion

        #region Personal Device Type

        /// <summary>
        /// Personal Device Type - Mobile
        /// </summary>
        public const string PERSONAL_DEVICE_TYPE_MOBILE = "5A8F264F-3BE6-4F15-912A-3CE93A98E8F6";

        /// <summary>
        /// Personal Device Type - Computer
        /// </summary>
        public const string PERSONAL_DEVICE_TYPE_COMPUTER = "828ADECE-EFE7-49DF-BA8C-B3F132509A95";

        /// <summary>
        /// Personal Device Type - TV
        /// </summary>
        public const string PERSONAL_DEVICE_TYPE_TV = "CA45FC83-2B1C-51AC-4B46-F3427F57116B";
        #endregion

        #region Personal Device Platform

        /// <summary>
        /// Other platform
        /// </summary>
        public const string PERSONAL_DEVICE_PLATFORM_OTHER = "13091A2D-C483-46FA-B489-E5D230B353C8";

        /// <summary>
        /// IOS platform
        /// </summary>        
        public const string PERSONAL_DEVICE_PLATFORM_IOS = "F00515E7-4EF3-480D-A45D-372CE3D80E69";

        /// <summary>
        /// Android platform
        /// </summary>       
        public const string PERSONAL_DEVICE_PLATFORM_ANDROID = "63464BB8-83E2-4914-B922-5075311758F9";

        #endregion

        #region Structure Content Editor

        /// <summary>
        /// Default Structure Content Editor
        /// </summary>
        public const string STRUCTURE_CONTENT_EDITOR_DEFAULT = "09B25845-B879-4E69-87E9-003F9380B8DD";

        /// <summary>
        /// Structure Content Editor Message Notes
        /// </summary>
        public const string STRUCTURE_CONTENT_EDITOR_MESSAGE_NOTES = "31C63FB9-1365-4EEF-851D-8AB9A188A06C";

        #endregion

        #region Map Styles

        /// <summary>
        /// Google map style
        /// </summary>
        public const string MAP_STYLE_GOOGLE = "BFC46259-FB66-4427-BF05-2B030A582BEA";

        /// <summary>
        /// The standard Rock map style
        /// </summary>
        public const string MAP_STYLE_ROCK = "FDC5D6BA-A818-4A06-96B1-9EF31B4087AC";

        #endregion

        #region Chart Styles

        /// <summary>
        /// Flot Chart Style
        /// </summary>
        public const string CHART_STYLE_FLOT = "B45DA8E1-B9A6-46FD-9A2B-E8440D7D6AAC";

        /// <summary>
        /// Rock Chart Style
        /// </summary>
        public const string CHART_STYLE_ROCK = "2ABB2EA0-B551-476C-8F6B-478CD08C2227";

        #endregion

        #region Liquid Templates

        /// <summary>
        /// Default RSS Channel Template
        /// </summary>
        public const string DEFAULT_RSS_CHANNEL = "D6149581-9EFC-40D8-BD38-E92C0717BEDA";

        /// <summary>
        /// The default iCal description
        /// </summary>
        public const string DEFAULT_ICAL_DESCRIPTION = "DCBA4862-73E9-49B5-8AD5-08E17BE68025";

        #endregion

        #region Benevolence
        /// <summary>
        /// Benevolence Pending
        /// </summary>
        public const string BENEVOLENCE_PENDING = "67B24629-62A9-436A-A98C-30A454642153";

        /// <summary>
        /// Benevolence Approved
        /// </summary>
        public const string BENEVOLENCE_APPROVED = "18D3A2DA-F2BA-49AE-83EB-7E60DCD18A3B";

        /// <summary>
        /// Benevolence Denied
        /// </summary>
        public const string BENEVOLENCE_DENIED = "3720671E-DA48-405F-A6D5-5E2D47436F9A";
        #endregion

        #region Interactions

        /// <summary>
        /// Interaction Channel Type: Website
        /// </summary>
        public const string INTERACTIONCHANNELTYPE_WEBSITE = "E503E77D-CF35-E09F-41A2-B213184F48E8";

        /// <summary>
        /// Interaction Channel Type: UrlShortener
        /// </summary>
        public const string INTERACTIONCHANNELTYPE_URLSHORTENER = "371066D5-C5F9-4783-88C8-D9AC8DC67468";

        /// <summary>
        /// Interaction Channel Type: Communication
        /// </summary>
        public const string INTERACTIONCHANNELTYPE_COMMUNICATION = "55004F5C-A8ED-7CB7-47EE-5988E9F8E0A8";

        /// <summary>
        /// Interaction Channel Type: Content Channel
        /// </summary>
        public const string INTERACTIONCHANNELTYPE_CONTENTCHANNEL = "F1A19D09-E010-EEB3-465A-940A6F023CEB";

        /// <summary>
        /// Interaction Channel Type: Content Channel
        /// </summary>
        public const string INTERACTIONCHANNELTYPE_WIFI_PRESENCE = "338CB800-C556-46CD-849D-8AE58FC7CB0E";

        /// <summary>
        /// Interaction Channel Type: System Events
        /// </summary>
        public const string INTERACTIONCHANNELTYPE_SYSTEM_EVENTS = "5919214F-9C59-4913-BE4E-0DFB6A05F528";

        /// <summary>
        /// Interaction Channel Type: Challenges
        /// </summary>
        public const string INTERACTIONCHANNELTYPE_CHALLENGES = "593607DC-4537-46E4-939F-60E0F74A1518";

        /// <summary>
        /// Interaction Channel Type: Challenge Progress
        /// </summary>
        public const string INTERACTIONCHANNELTYPE_CHALLENGE_PROGRESS = "6AA529BD-9FA5-43AD-A98E-B8DF5F997146";

        /// <summary>
        /// Interaction Channel Type: Interaction Intents
        /// </summary>
        public const string INTERACTIONCHANNELTYPE_INTERACTION_INTENTS = "A64DA89B-F634-4D58-ADC7-32243F84224B";

        /// <summary>
        /// The PBX CDR medium value
        /// </summary>
        public const string PBX_CDR_MEDIUM_VALUE = "B3904B57-62A2-57AC-43EA-94D4DEBA3D51";

        #endregion

        #region Person Search Keys

        /// <summary>
        /// Person Search Type: Email
        /// </summary>
        public const string PERSON_SEARCH_KEYS_EMAIL = "D6CFD200-B33B-4D01-B49F-24325E47D8B8";

        /// <summary>
        /// Person Search Type: Alternate Id
        /// </summary>
        public const string PERSON_SEARCH_KEYS_ALTERNATE_ID = "AD77CF28-E42B-44C2-8D5C-F6A0A6EE8022";

        #endregion

        #region Spiritual Gifts

        /// <summary>
        /// Spiritual Gifts: Administration
        /// </summary>
        public const string SPIRITUAL_GIFTS_ADMINISTRATION = "A276421D-F662-4723-99DA-6FDF3E9CFF7C";

        /// <summary>
        /// Spiritual Gifts: Apostleship
        /// </summary>
        public const string SPIRITUAL_GIFTS_APOSTLESHIP = "A2C7074E-AC97-4D89-9240-47A552CDC4C0";

        /// <summary>
        /// Spiritual Gifts: Discernment
        /// </summary>
        public const string SPIRITUAL_GIFTS_DISCERNMENT = "3EB352F3-F624-4ED6-A9EE-7951B71B1952";

        /// <summary>
        /// Spiritual Gifts: Encouragement
        /// </summary>
        public const string SPIRITUAL_GIFTS_ENCOURAGEMENT = "809F65A6-1759-472A-8B8B-F37009F476BF";

        /// <summary>
        /// Spiritual Gifts: Evangelism
        /// </summary>
        public const string SPIRITUAL_GIFTS_EVANGELISM = "0F8D41AA-7236-40BF-AA37-980BCCF4A881";

        /// <summary>
        /// Spiritual Gifts: Faith
        /// </summary>
        public const string SPIRITUAL_GIFTS_FAITH = "7B30E2BA-9461-4688-9B43-D2B774E33A18";

        /// <summary>
        /// Spiritual Gifts: Giving
        /// </summary>
        public const string SPIRITUAL_GIFTS_GIVING = "C4259D6E-675C-417B-9175-6D599C86A204";

        /// <summary>
        /// Spiritual Gifts: Helps
        /// </summary>
        public const string SPIRITUAL_GIFTS_HELPS = "13C40209-F41D-4C1D-83D3-2EC530588245";

        /// <summary>
        /// Spiritual Gifts: Hospitality
        /// </summary>
        public const string SPIRITUAL_GIFTS_HOSPITALITY = "98D5EE08-633D-4635-80CD-169449604D18";

        /// <summary>
        /// Spiritual Gifts: Knowledge
        /// </summary>
        public const string SPIRITUAL_GIFTS_KNOWLEDGE = "462A5D10-6DEA-43D7-96EF-8F82FF1E2E14";

        /// <summary>
        /// Spiritual Gifts: Leadership
        /// </summary>
        public const string SPIRITUAL_GIFTS_LEADERSHIP = "A1CB038C-AAFC-4745-A7D2-7C8BA5028F05";

        /// <summary>
        /// Spiritual Gifts: Mercy
        /// </summary>
        public const string SPIRITUAL_GIFTS_MERCY = "0894EDBA-8FC8-4433-877C-53351A06A8B7";

        /// <summary>
        /// Spiritual Gifts: Pastor-Shepherd
        /// </summary>
        public const string SPIRITUAL_GIFTS_PASTOR_SHEPHERD = "FC4F1B46-F0C3-45B0-9FD9-D15F4FD05A31";

        /// <summary>
        /// Spiritual Gifts: Pastor-Teacher
        /// </summary>
        public const string SPIRITUAL_GIFTS_PASTOR_TEACHER = "C7291F22-05F0-4EF9-A7C2-2CFEBFEBCB45";

        /// <summary>
        /// Spiritual Gifts: Prophecy
        /// </summary>
        public const string SPIRITUAL_GIFTS_PROPHECY = "4ADAEED1-D0E6-4DA4-A0BA-8E7D058075C4";

        /// <summary>
        /// Spiritual Gifts: Teaching
        /// </summary>
        public const string SPIRITUAL_GIFTS_TEACHING = "E8278791-2400-4DDA-AEAA-C6F11E0AC9D0";

        /// <summary>
        /// Spiritual Gifts: Wisdom
        /// </summary>
        public const string SPIRITUAL_GIFTS_WISDOM = "5F1F5A92-D981-4027-A4BC-C3642E784D0B";

        #endregion

        #region Conflict Profile

        /// <summary>
        /// Conflict Profile: Avoiding 
        /// </summary>
        public const string CONFLICT_PROFILE_AVOIDING = "663B0F4A-DE1F-46BE-8BDD-D7C98863DDC4";

        /// <summary>
        /// Conflict Profile: Compromising 
        /// </summary>
        public const string CONFLICT_PROFILE_COMPROMISING = "CF78D6B1-38AA-4FF7-9A4B-E900438FA85A";

        /// <summary>
        /// Conflict Profile: Resolving 
        /// </summary>
        public const string CONFLICT_PROFILE_RESOLVING = "DF7B1EB2-7E7E-4F91-BD26-C6DFD88E38DF";

        /// <summary>
        /// Conflict Profile: Winning 
        /// </summary>
        public const string CONFLICT_PROFILE_WINNING = "56300095-86AD-43FE-98D2-50829E9223C2";

        /// <summary>
        /// Conflict Profile: Yielding 
        /// </summary>
        public const string CONFLICT_PROFILE_YEILDING = "4AB06A6F-F5B1-4385-9365-199EA7969E50";

        #endregion

        #region Motivator

        /// <summary>
        /// Motivator: Believing 
        /// </summary>
        public const string MOTIVATOR_BELIEVING = "99F598E0-E0AC-4B4B-BEAF-589D41764EE1";

        /// <summary>
        /// Motivator: Caring 
        /// </summary>
        public const string MOTIVATOR_CARING = "FFD7EF9C-5D68-40D2-A362-416B2D660D51";

        /// <summary>
        /// Motivator: Expressing 
        /// </summary>
        public const string MOTIVATOR_EXPRESSING = "FA70E27D-6642-4162-AF17-530F66B507E7";

        /// <summary>
        /// Motivator: Empowering 
        /// </summary>
        public const string MOTIVATOR_EMPOWERING = "C171D01E-C607-488B-A550-1E341081210B";

        /// <summary>
        /// Motivator: Engaging 
        /// </summary>
        public const string MOTIVATOR_ENGAGING = "5635E95B-3A07-43B7-837A-0F131EF1DA97";

        /// <summary>
        /// Motivator: Adapting 
        /// </summary>
        public const string MOTIVATOR_ADAPTING = "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89";

        /// <summary>
        /// Motivator: Gathering 
        /// </summary>
        public const string MOTIVATOR_GATHERING = "73087DD2-B892-4367-894F-8922477B2F10";

        /// <summary>
        /// Motivator: Innovating 
        /// </summary>
        public const string MOTIVATOR_INNOVATING = "D84E58E4-87FC-4CEB-B83E-A2C6D186366C";

        /// <summary>
        /// Motivator: Leading 
        /// </summary>
        public const string MOTIVATOR_LEADING = "6A2354C6-3FA4-4BAD-89A8-7359FEC48FE3";

        /// <summary>
        /// Motivator: Learning 
        /// </summary>
        public const string MOTIVATOR_LEARNING = "7EA44A56-58CB-4E40-9779-CC0A79772926";

        /// <summary>
        /// Motivator: Maximizing 
        /// </summary>
        public const string MOTIVATOR_MAXIMIZING = "3F678404-5844-494F-BDB0-DD9FEEBC98C9";

        /// <summary>
        /// Motivator: Organizing 
        /// </summary>
        public const string MOTIVATOR_ORGANIZING = "85459C0F-65A5-48F9-86F3-40B03F9C53E9";

        /// <summary>
        /// Motivator: Pacing 
        /// </summary>
        public const string MOTIVATOR_PACING = "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25";

        /// <summary>
        /// Motivator: Perceiving 
        /// </summary>
        public const string MOTIVATOR_PERCEIVING = "4C898A5C-B48E-4BAE-AB89-835F25A451BF";

        /// <summary>
        /// Motivator: Relating 
        /// </summary>
        public const string MOTIVATOR_RELATING = "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995";

        /// <summary>
        /// Motivator: Serving 
        /// </summary>
        public const string MOTIVATOR_SERVING = "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD";

        /// <summary>
        /// Motivator: Thinking 
        /// </summary>
        public const string MOTIVATOR_THINKING = "0D82DC77-334C-44B0-84A6-989910907DD4";

        /// <summary>
        /// Motivator: Transforming 
        /// </summary>
        public const string MOTIVATOR_TRANSFORMING = "2393C3CE-8E49-46FE-A75B-D5D624A37B49";

        /// <summary>
        /// Motivator: Uniting 
        /// </summary>
        public const string MOTIVATOR_UNITING = "D7601B56-7495-4D7B-A916-8C48F78675E3";

        /// <summary>
        /// Motivator: Persevering 
        /// </summary>
        public const string MOTIVATOR_PERSEVERING = "A027F6B2-56DD-4724-962D-F865606AEAB8";

        /// <summary>
        /// Motivator: Risk-Taking 
        /// </summary>
        public const string MOTIVATOR_RISKTAKING = "4D0A1A6D-3F5A-476E-A633-04EAEF457645";

        /// <summary>
        /// Motivator: Visioning 
        /// </summary>
        public const string MOTIVATOR_VISIONING = "EE1603BA-41AE-4CFA-B220-065768996501";

        /// <summary>
        /// Motivator: Growth Propensity 
        /// </summary>
        public const string MOTIVATOR_GROWTH_PROPENSITY = "605F3702-6AE7-4545-BEBE-23693E60031C";

        #endregion Motivator

        #region Motivator Theme

        /// <summary>
        /// Motivator Theme: Relational
        /// </summary>
        public const string MOTIVATOR_RELATIONAL_THEME = "840C414E-A261-4243-8302-6117E8949FE4";

        /// <summary>
        /// Motivator Theme: Directional 
        /// </summary>
        public const string MOTIVATOR_DIRECTIONAL_THEME = "112A35BE-3108-48D9-B057-125A788AB531";

        /// <summary>
        /// Motivator Theme: Intellectual 
        /// </summary>
        public const string MOTIVATOR_INTELLECTUAL_THEME = "58FEF15F-561D-420E-8937-6CF51D296F0E";

        /// <summary>
        /// Motivator Theme: Positional 
        /// </summary>
        public const string MOTIVATOR_POSITIONAL_THEME = "84322020-4E27-44EF-88F2-EAFDB7286A01";

        #endregion Motivator Theme

        #region Logging

        /// <summary>
        /// The logging domain CMS
        /// </summary>
        public const string LOGGING_DOMAIN_CMS = "C8A94BA4-9450-459C-B32E-E34C153AD02F";

        /// <summary>
        /// The logging domain event
        /// </summary>
        public const string LOGGING_DOMAIN_EVENT = "EF558F8A-08DA-4001-BBAA-83390613D20E";

        /// <summary>
        /// The logging domain reporting
        /// </summary>
        public const string LOGGING_DOMAIN_REPORTING = "A2960B6B-0DF5-458B-906C-F64521CB8C83";

        /// <summary>
        /// The logging domain communications
        /// </summary>
        public const string LOGGING_DOMAIN_COMMUNICATIONS = "8EE2657E-FF42-4E04-A5DA-15B14C8086B8";

        /// <summary>
        /// The logging domain finance
        /// </summary>
        public const string LOGGING_DOMAIN_FINANCE = "24230441-AE17-4F8B-ABF7-DA32CC5F2571";

        /// <summary>
        /// The logging domain steps
        /// </summary>
        public const string LOGGING_DOMAIN_STEPS = "0FE30F0C-E503-4DE6-A8EE-1E86B141967B";

        /// <summary>
        /// The logging domain connection
        /// </summary>
        public const string LOGGING_DOMAIN_CONNECTION = "E5125727-8361-4BBB-A615-28B3F045BA7E";

        /// <summary>
        /// The logging domain group
        /// </summary>
        public const string LOGGING_DOMAIN_GROUP = "134088B7-A12A-4ED8-AA18-E059FB5D17B5";

        /// <summary>
        /// The logging domain streaks
        /// </summary>
        public const string LOGGING_DOMAIN_STREAKS = "E7D3D04F-7BB8-4FE4-B2C2-924D8C0CA62B";

        /// <summary>
        /// The logging domain core
        /// </summary>
        public const string LOGGING_DOMAIN_CORE = "3B4F21DC-C169-453E-B119-8F33E122C84C";

        /// <summary>
        /// The logging domain jobs
        /// </summary>
        public const string LOGGING_DOMAIN_JOBS = "2741A4A3-8E57-4B58-ABCC-56C0A31C3A25";

        /// <summary>
        /// The logging domain workflow
        /// </summary>
        public const string LOGGING_DOMAIN_WORKFLOW = "FF8CBA1B-5DAF-4C0E-A7DA-A0277F7AA1E5";

        /// <summary>
        /// The logging domain CRM
        /// </summary>
        public const string LOGGING_DOMAIN_CRM = "5D737243-E7E0-4981-87A5-0787E52631C6";

        /// <summary>
        /// The logging domain prayer
        /// </summary>
        public const string LOGGING_DOMAIN_PRAYER = "F460BDE7-6FFC-4979-A3AC-7BBBED05C781";

        /// <summary>
        /// The logging domain mobile
        /// </summary>
        public const string LOGGING_DOMAIN_MOBILE = "77148fef-bc4f-4b16-9d4e-e1de28149751";

        /// <summary>
        /// The logging domain apple tv
        /// </summary>
        public const string LOGGING_DOMAIN_APPLE_TV = "41a3cbf1-769a-4d15-a13c-56d48b2009b8";
        /// <summary>
        /// The logging domain other
        /// </summary>
        public const string LOGGING_DOMAIN_OTHER = "83CFDA4F-5867-4D4B-853F-D9B535EC5131";

        #endregion

        #region "Checkin Attendance Types"

        /// <summary>
        /// The check in attendance type physical
        /// </summary>
        public const string CHECK_IN_ATTENDANCE_TYPE_PHYSICAL = "290CE89F-7DA0-41ED-82C1-1249A4A42BA4";

        /// <summary>
        /// The check in attendance type virtual
        /// </summary>
        public const string CHECK_IN_ATTENDANCE_TYPE_VIRTUAL = "DF1C89D0-7D8A-48C2-B251-DFF3376C2931";

        #endregion

        #region Schedule Types

        /// <summary>
        /// Schedule Type:  Weekend Service
        /// </summary>
        public const string SCHEDULE_TYPE_WEEKEND_SERVICE = "033FC160-2650-4C97-8075-0C0114FA7A8D";

        #endregion Schedule Types

        #region Map Markers

        /// <summary>
        /// The map marker pin
        /// </summary>
        public const string MAP_MARKER_PIN = "52DEB4F4-7D2C-4BB0-9A70-80CCA536E3EB";
        /// <summary>
        /// The map marker marker
        /// </summary>
        public const string MAP_MARKER_MARKER = "D9841DDB-BD34-45A7-97DC-141C8D6D9E84";
        /// <summary>
        /// The map marker marker with dot
        /// </summary>
        public const string MAP_MARKER_MARKER_WITH_DOT = "C0FF9C66-8232-41E8-A9C0-DC9D0ECAF932";
        /// <summary>
        /// The map marker circle
        /// </summary>
        public const string MAP_MARKER_CIRCLE = "66C27E88-6BC5-4B57-A981-0AD58481C66A";

        #endregion Map Markers

        #region Languages

        /// <summary>
        /// Translation Languages - English
        /// </summary>
        public const string LANGUAGE_ENGLISH = "DF0A29A7-A61E-E4A7-4F3D-58CFDD3D3871";

        /// <summary>
        /// Translation Languages - Spanish
        /// </summary>
        public const string LANGUAGE_SPANISH = "C93CB430-8554-E599-4F49-D7F3CED2B2C7";

        #endregion Languages

        #region Person Race

        /// <summary>
        /// Person Race - White
        /// </summary>
        public const string PERSON_RACE_WHITE = "52E12EBE-1FCE-4B95-A677-AEEEDE9B1745";

        /// <summary>
        /// Person Race - Black or African American
        /// </summary>
        public const string PERSON_RACE_BLACK_OR_AFRICAN_AMERICAN = "3760BA55-3D68-4F55-AEEF-0AC9F39D1730";

        /// <summary>
        /// Person Race - American Indian or Alaska Native
        /// </summary>
        public const string PERSON_RACE_AMERICAN_INDIAN_OR_ALASKAN_NATIVE = "C734961E-43A7-4FB9-999E-B60D694268B4";

        /// <summary>
        /// Person Race - Asian
        /// </summary>
        public const string PERSON_RACE_ASIAN = "FCDC15DF-B298-4067-AE8A-431E42DA6F7E";

        /// <summary>
        /// Person Race - Native Hawaiian or Pacific Islander
        /// </summary>
        public const string PERSON_RACE_NATIVE_HAWAIIAN_OR_PACIFIC_ISLANDER = "B46F3250-34A1-46E5-8171-9C8ED4FA0845";

        /// <summary>
        /// Person Race - Other
        /// </summary>
        public const string PERSON_RACE_OTHER = "E364D2DE-81A0-4F9C-8ECF-96CC68009251";

        #endregion

        #region Person Ethnicity

        /// <summary>
        /// Person Ethnicity - Hispanic or Latino
        /// </summary>
        public const string PERSON_ETHNICITY_HISPANIC_OR_LATINO = "05762BE9-32D4-4C30-9CF1-E1513C5C8360";

        /// <summary>
        /// Person Ethnicity - Not Hispanic or Latino
        /// </summary>
        public const string PERSON_ETHNICITY_NOT_HISPANIC_OR_LATINO = "2D1EF4CF-19E5-46BC-B4B1-591CFF57E0D8";

        #endregion

        #region Project Type

        /// <summary>
        /// Project Type - In-Person
        /// </summary>
        public const string PROJECT_TYPE_IN_PERSON = "FF3F0C5C-9775-4A09-9CCF-94902DB99BF6";

        /// <summary>
        /// Project Type = Project Due
        /// </summary>
        public const string PROJECT_TYPE_PROJECT_DUE = "C999D489-5B8F-4892-BCC3-90DFFBC524F5";

        #endregion

        #region Theme Purpose

        /// <summary>
        /// Check-in
        /// </summary>
        public const string THEME_PURPOSE_CHECKIN = "2BBB1A44-708E-4469-80DE-4AAE6227BEF8";

        /// <summary>
        /// Website Legacy
        /// </summary>
        public const string THEME_PURPOSE_WEBSITE_LEGACY = "4E1477FD-B105-4E4B-99BB-E5F1B964DC94";

        /// <summary>
        /// Website Nextgen
        /// </summary>
        public const string THEME_PURPOSE_WEBSITE_NEXTGEN = "B177E07F-7E07-4D7B-AFA7-9DE163797659";

        #endregion
    }
}