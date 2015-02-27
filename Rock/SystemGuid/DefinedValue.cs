// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
        #region Communication Medium Type

        /// <summary>
        /// Email communication
        /// </summary>
        public const string COMMUNICATION_MEDIUM_EMAIL = "FC51461D-0C31-4C6B-A7C8-B3E8482C1055";

        #endregion

        #region Check-in Search Type

        /// <summary>
        /// Phone number search type
        ///
        /// </summary>
        public const string CHECKIN_SEARCH_TYPE_PHONE_NUMBER = "F3F66040-C50F-4D13-9652-780305FFFE23";

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

        #region Financial Source

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

        #endregion

        #region Group Location Type

        /// <summary>
        /// Home location type
        /// </summary>
        public const string GROUP_LOCATION_TYPE_HOME = "8C52E53C-2A66-435A-AE6E-5EE307D9A0DC";

        /// <summary>
        /// Work Record Type
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

        #region Note Type

        /// <summary>
        /// Manually entered note.
        /// </summary>
        public const string NOTE_TYPE_MANUAL_NOTE = "4318E9AC-B669-4AF7-AF88-EF580FC43C6A";

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

        #endregion

        #region Person Review Reason

        /// <summary>
        /// Member Person Connection Status
        /// </summary>
        public const string PERSON_REVIEW_REASON_SELF_INACTIVATED = "D539C356-6856-4E94-80B4-8FEA869AF38B";

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

        #endregion

        #region Workflow Note Type

        /// <summary>
        /// A user-entered workflow note
        /// </summary>
        public const string WORKFLOW_NOTE_TYPE_USER_NOTE = "534489FB-E239-4C51-8F5D-9ECF85E9CDE2";

        /// <summary>
        /// A system-entered workflow note
        /// </summary>
        public const string WORKFLOW_NOTE_TYPE_SYSTEM_NOTE = "414E9F98-4709-4895-AEBA-E41773BB7EB8";

        #endregion
    }
}