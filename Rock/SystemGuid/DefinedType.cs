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
    public class DefinedType
    {
        /// <summary>
        /// Guid for the types of Benevolence Request status (e.g. pending, active, answered, etc.)
        /// </summary>
        public const string BENEVOLENCE_REQUEST_STATUS = "2787B088-D607-4D69-84FF-850A6891EE34";

        /// <summary>
        /// The types of results for a Benevolence Request
        /// </summary>
        public const string BENEVOLENCE_RESULT_TYPE = "35FC0225-3DAC-48B4-BDF7-AFDE104FB60E";

        /// <summary>
        /// The types of communication supported (i.e. email, sms, twitter, app-push, etc)
        /// </summary>
        public const string COMMUNICATION_MEDIUM = "DC8A841C-E91D-4BD4-A6A7-0DE765308E8F";

        /// <summary>
        /// The domains that are safe to send from
        /// </summary>
        public const string COMMUNICATION_SAFE_SENDER_DOMAINS = "DB91D0E9-DCA6-45A9-8276-AEF032BE8AED";

        /// <summary>
        /// The list of values that SMS messages can be sent from.  Depending on provider, these may
        /// be phone numbers or short codes
        /// </summary>
        public const string COMMUNICATION_SMS_FROM = "611BDE1F-7405-4D16-8626-CCFEDB0E62BE";

        /// <summary>
        /// The list of phone country code formats and how to format their numbers 
        /// </summary>
        public const string COMMUNICATION_PHONE_COUNTRY_CODE = "45E9EF7C-91C7-45AB-92C1-1D6219293847";

        /// <summary>
        /// Guid for check-in search type
        /// </summary>
        public const string CHECKIN_SEARCH_TYPE = "1EBCDB30-A89A-4C14-8580-8289EC2C7742";

        /// <summary>
        /// Guid for Device Type
        /// </summary>
        public const string DEVICE_TYPE = "0368B637-327A-4F5E-80C2-832079E482EE";

        /// <summary>
        /// Guid for the DISC Results of personalities.
        /// </summary>
        public const string DISC_RESULTS_TYPE = "F06DDAD8-6058-4182-AD0A-B523BB7A2D78";

        /// <summary>
        /// Guid for External Application
        /// </summary>
        public const string EXTERNAL_APPLICATION = "1FAC459C-5F62-4E7C-8933-61FF9FE2DFEF";

        /// <summary>
        /// Guid for Financial Currency Type
        /// </summary>
        public const string FINANCIAL_ACCOUNT_TYPE = "752DA126-471F-4221-8503-5297593C99FF";
		
        /// <summary>
        /// Guid for Financial Currency Type
        /// </summary>
        public const string FINANCIAL_CURRENCY_TYPE =  "1D1304DE-E83A-44AF-B11D-0C66DD600B81"; 

        /// <summary>
        /// Guid for Financial Credit Card Type
        /// </summary>
        public const string FINANCIAL_CREDIT_CARD_TYPE =  "2BD4FFB0-6C7F-4890-8D08-00F0BB7B43E9";

        /// <summary>
        /// Guid for Financial Frequency 
        /// </summary>
        public const string FINANCIAL_FREQUENCY = "1F645CFB-5BBD-4465-B9CA-0D2104A1479B";

        /// <summary>
        /// Guid for Financial Source Type
        /// </summary>
        public const string FINANCIAL_SOURCE_TYPE =  "4F02B41E-AB7D-4345-8A97-3904DDD89B01";

        /// <summary>
        /// Guid for Financial Transaction Type
        /// </summary>
        public const string FINANCIAL_TRANSACTION_REFUND_REASON = "61FE3A58-9F4F-472F-A4E0-5116EB90A323";

        /// <summary>
        /// Guid for Financial Transaction Type
        /// </summary>
        public const string FINANCIAL_TRANSACTION_TYPE = "FFF62A4B-5D88-4DEB-AF8F-8E6178E41FE5";

        /// <summary>
        /// Guid for the types of Group Locations (such as Home, Main Office, etc)
        /// </summary>
        public const string GROUP_LOCATION_TYPE =  "2E68D37C-FB7B-4AA5-9E09-3785D52156CB";

        /// <summary>
        /// Guid for GroupType Purpose
        /// </summary>
        public const string GROUPTYPE_PURPOSE = "B23F1E45-BC26-4E82-BEB3-9B191FE5CCC3";

        /// <summary>
        /// Guid for countries
        /// </summary>
        public const string LOCATION_COUNTRIES = "D7979EA1-44E9-46E2-BF37-DDAF7F741378";

        /// <summary>
        /// Guid for the types of States that can be tied to a Location's address.
        /// </summary>
        public const string LOCATION_ADDRESS_STATE = "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB";

        /// <summary>
        /// Guid for the types of named locations (such as Campus, Building, Room, etc)
        /// </summary>
        public const string LOCATION_TYPE = "3285DCEF-FAA4-43B9-9338-983F4A384ABA";

        /// <summary>
        /// Guid for Marketing Campaign Audience Type
        /// </summary>
        public const string MARKETING_CAMPAIGN_AUDIENCE_TYPE =  "799301A3-2026-4977-994E-45DC68502559";

        /// <summary>
        /// Metric Source Type
        /// </summary>
        public const string METRIC_SOURCE_TYPE = "D6F323FF-6EF2-4DA7-A82C-61399AC1D798";
        
        /// <summary>
        /// Guid for the types of Person Records (such as person, business, etc.)
        /// </summary>
        public const string PERSON_RECORD_TYPE =  "26be73a6-a9c5-4e94-ae00-3afdcf8c9275";

        /// <summary>
        /// Guid for the types of Person Record Statuses (such as active, inactive, pending, etc.)
        /// </summary>
        public const string PERSON_RECORD_STATUS =  "8522badd-2871-45a5-81dd-c76da07e2e7e";

        /// <summary>
        /// Guid for the types of Person Record Status Reasons (such as deceased, moved, etc.)
        /// </summary>
        public const string PERSON_RECORD_STATUS_REASON =  "e17d5988-0372-4792-82cf-9e37c79f7319";

        /// <summary>
        /// Guid for the person'S connection status (such as member, attendee, participant, etc.)
        /// </summary>
        public const string PERSON_CONNECTION_STATUS =  "2e6540ea-63f0-40fe-be50-f2a84735e600";

        /// <summary>
        /// Guid for the reasons a person record needs to be reviewed
        /// </summary>
        public const string PERSON_REVIEW_REASON = "7680C445-AD69-4E5D-94F0-CBAA96DB0FF8";

        /// <summary>
        /// Guid for the types of Person Titles (such as Mr., Mrs., Dr., etc.)
        /// </summary>
        public const string PERSON_TITLE =  "4784cd23-518b-43ee-9b97-225bf6e07846";

        /// <summary>
        /// Guid for the types of Person Suffixes (such as Jr., Sr., etc.)
        /// </summary>
        public const string PERSON_SUFFIX =  "16f85b3c-b3e8-434c-9094-f3d41f87a740";

        /// <summary>
        /// Guid for the types of Person Marital Statuses (such as Married, Single, Divorced, Widowed, etc.)
        /// </summary>
        public const string PERSON_MARITAL_STATUS =  "b4b92c3f-a935-40e1-a00b-ba484ead613b";

        /// <summary>
        /// Guid for the types of Person phone numbers (such as Primary, Secondary, etc.)
        /// </summary>
        public const string PERSON_PHONE_TYPE =  "8345DD45-73C6-4F5E-BEBD-B77FC83F18FD";

        /// <summary>
        /// Guid for the types of possible check-in system ability levels (such as Infant, Crawler, etc.)
        /// </summary>
        public const string PERSON_ABILITY_LEVEL_TYPE = "7BEEF4D4-0860-4913-9A3D-857634D1BF7C";

        /// <summary>
        /// Guid for the types of map styles
        /// </summary>
        public const string MAP_STYLES = "4EF89471-C049-49ED-AB50-677F689A4E4E";

        /// <summary>
        /// Guid for the types of chart styles
        /// </summary>
        public const string CHART_STYLES = "FC684FD7-FE68-493F-AF38-1656FBF67E6B";

        /// <summary>
        /// Guid for the button html
        /// </summary>
        public const string BUTTON_HTML = "407A3A73-A3EF-4970-B856-2A33F62AC72E";

        /// <summary>
        /// The REST allowed domains
        /// </summary>
        public const string REST_API_ALLOWED_DOMAINS = "DF7C8DF7-49F9-4858-9E5D-20842AF65AD8";

        /// <summary>
        /// The workflow note type
        /// </summary>
        public const string WORKFLOW_NOTE_TYPE = "FDC7A191-717E-4CA6-9DCF-A2B5BB09C782";

        /// <summary>
        /// The school grades defined type which has values that determine which grade gext to display
        /// </summary>
        public const string SCHOOL_GRADES = "24E5A79F-1E62-467A-AD5D-0D10A2328B4D";

    }
}