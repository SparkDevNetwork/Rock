﻿// <copyright>
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
namespace Rock.SystemGuid
{
    /// <summary>
    /// Static Guids used by the Rock application
    /// </summary>
    public static class DefinedValue
    {
        #region Campus

        public const string CAMPUS_STATUS_PENDING = "008209F5-144E-4282-92C3-944A4AC78700";
        public const string CAMPUS_STATUS_OPEN = "10696FD8-D0C7-486F-B736-5FB3F5D69F1A";
        public const string CAMPUS_STATUS_CLOSED = "99D2C9CB-B6DC-49C1-B626-B76BD398B63A";

        public const string CAMPUS_TYPE_PHYSICAL = "5A61507B-79CB-4DA2-AF43-6F82260203B3";
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
        /// Scanned Id Search Type. This type is always supported and if a barcode or bio id is scanned, the attendance will be updated to reflect this type os search was used.
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

        #endregion

        #region Entity Set Purpose

        /// <summary>
        /// Entity Set for the purpose of doing a Person Merge request
        /// </summary>
        public const string ENTITY_SET_PURPOSE_PERSON_MERGE_REQUEST = "214EB26F-5493-4540-B2EF-F0887C8FBB9E";

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
        public const string FINANCIAL_SOURCE_TYPE_KIOSK	= "260EEA80-821A-4F79-973F-49DF79C955F7";
        
        /// <summary>
        /// The financial source of Mobile Application
        /// </summary>
        public const string FINANCIAL_SOURCE_TYPE_MOBILE_APPLICATION	= "8ADCEC72-63FC-4F08-A4CC-72BCE470172C";
        
        /// <summary>
        /// The financial source of On-site Collection
        /// </summary>
        public const string FINANCIAL_SOURCE_TYPE_ONSITE_COLLECTION = "BE7ECF50-52BC-4774-808D-574BA842DB98";

        /// <summary>
        /// The financial source of Text-to-Give (Give action of the SMS actions)
        /// </summary>
        public const string FINANCIAL_SOURCE_TYPE_SMS_GIVE = "8BA95E24-D291-499E-A535-4DCAC365689B";

        #endregion

        #region Group Location Type

        /// <summary>
        /// Home location type
        /// </summary>
        public const string GROUP_LOCATION_TYPE_HOME = "8C52E53C-2A66-435A-AE6E-5EE307D9A0DC";

        /// <summary>
        /// Work location Type
        /// </summary>
        public const string GROUP_LOCATION_TYPE_WORK = "E071472A-F805-4FC4-917A-D5E3C095C35C";

        /// <summary>
        /// Previous Location Type
        /// </summary>
        public const string GROUP_LOCATION_TYPE_PREVIOUS = "853D98F1-6E08-4321-861B-520B4106CFE0";

        /// <summary>
        /// Meeting Location Type
        /// </summary>
        public const string GROUP_LOCATION_TYPE_MEETING_LOCATION = "96D540F5-071D-4BBD-9906-28F0A64D39C4";

        /// <summary>
        /// Some other type of Group Location
        /// NOTE: This is not a core group location type. It'll get created if needed when importing locations from Slinghshot.
        /// </summary>
        public const string GROUP_LOCATION_TYPE_OTHER = "D49965C7-5254-4D2D-BC77-F390375F0C44";

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

        #region Metrics

        /// <summary>
        /// Metric values come from a dataview
        /// </summary>
        public const string METRIC_SOURCE_VALUE_TYPE_DATAVIEW = "2EC60BCF-EF63-4CCC-A970-F152292765D0";

        /// <summary>
        /// Metric values are entered manually
        /// </summary>
        public const string METRIC_SOURCE_VALUE_TYPE_MANUAL = "1D6511D6-B15D-4DED-B3C4-459CD2A7EC0E";

        /// <summary>
        /// Metric values are populated from custom sql
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

        #region Person Record Status Reason

        /// <summary>
        /// Inactive record status reason of Deceased 
        /// </summary>
        public const string PERSON_RECORD_STATUS_REASON_DECEASED = "05D35BC4-5816-4210-965F-1BF44F35A16A";

        /// <summary>
        /// Inactive record status reason of No Activity 
        /// </summary>
        public const string PERSON_RECORD_STATUS_REASON_NO_ACTIVITY = "64014FE6-943D-4ACF-8014-FED9F9169AE8";

        /// <summary>
        /// Inactive record status reason of Moved
        /// </summary>
        public const string PERSON_RECORD_STATUS_REASON_MOVED = "3684CF84-F30F-4CE0-9EB9-D6AFFEA9B99B";

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

        /// <summary>
        /// Rest User Record Type
        /// </summary>
        public const string PERSON_RECORD_TYPE_RESTUSER = "E2261A84-831D-4234-9BE0-4D628BBE751E";

        #endregion

        #region Person Connection Status

        /// <summary>
        /// Member Person Connection Status
        /// </summary>
        public const string PERSON_CONNECTION_STATUS_MEMBER = "41540783-D9EF-4C70-8F1D-C9E83D91ED5F";

        /// <summary>
        /// Attendee Person Connection Status
        /// </summary>
        public const string PERSON_CONNECTION_STATUS_ATTENDEE = "39F491C5-D6AC-4A9B-8AC0-C431CB17D588";

        /// <summary>
        /// Visitor Person Connection Status
        /// TODO: some places have B91BA046-BC1E-400C-B85D-638C1F4E0CE2
        /// </summary>
        public const string PERSON_CONNECTION_STATUS_VISITOR = "B91BA046-BC1E-400C-B85D-638C1F4E0CE2";

        /// <summary>
        /// Participant Connection Status
        /// </summary>
        public const string PERSON_CONNECTION_STATUS_PARTICIPANT = "8EBC0CEB-474D-4C1B-A6BA-734C3A9AB061";

        /// <summary>
        /// Web Prospect Connection Status
        /// </summary>
        public const string PERSON_CONNECTION_STATUS_WEB_PROSPECT = "368DD475-242C-49C4-A42C-7278BE690CC2";

        #endregion

        #region Person Review Reason

        /// <summary>
        /// Member Person Connection Status
        /// </summary>
        public const string PERSON_REVIEW_REASON_SELF_INACTIVATED = "D539C356-6856-4E94-80B4-8FEA869AF38B";

        #endregion

        #region Personal Device Type

        /// <summary>
        /// Mobile Personal device type
        /// </summary>
        public const string PERSONAL_DEVICE_TYPE_MOBILE = "5A8F264F-3BE6-4F15-912A-3CE93A98E8F6";

        /// <summary>
        /// Computer personal device type
        /// </summary>
        public const string PERSONAL_DEVICE_TYPE_COMPUTER = "828ADECE-EFE7-49DF-BA8C-B3F132509A95";

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
        /// The default ical description
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
    }
}