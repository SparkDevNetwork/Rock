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

namespace Rock.SystemGuid
{
    /// <summary>
    /// System Blocks.  NOTE: Some of these are referenced in Migrations to avoid string-typos.
    /// </summary>
    public class BlockType
    {
        /// <summary>
        /// Gets the Badges display block guid
        /// </summary>
        public const string BADGES = "FC8AF928-C4AF-40C7-A667-4B24390F03A1";

        /// <summary>
        /// Gets the Plugin Manager guid
        /// </summary>
        public const string PLUGIN_MANAGER  = "F80268E6-2625-4565-AA2E-790C5E40A119";

        /// <summary>
        /// HTML Content Block Type Guid
        /// </summary>
        public const string HTML_CONTENT = "19B61D65-37E3-459F-A44F-DEF0089118A3";

        /// <summary>
        /// Page Menu Block Type Guid
        /// </summary>
        public const string PAGE_MENU = "CACB9D1A-A820-4587-986A-D66A69EE9948";

        #region Communication Block Types

        /// <summary>
        /// Communication Detail Block Type Guid
        /// </summary>
        public const string COMMUNICATION_DETAIL = "CEDC742C-0AB3-487D-ABC2-77A0A443AEBF";

        /// <summary>
        /// Communication Entry (Simple) Block Type Guid
        /// </summary>
        public const string COMMUNICATION_ENTRY = "D9834641-7F39-4CFA-8CB2-E64068127565";

        /// <summary>
        /// The Communication Entry (Wizard) Block Type Guid
        /// </summary>
        public const string COMMUNICATION_ENTRY_WIZARD = "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0";

        /// <summary>
        /// System Communication List Block Type Guid
        /// </summary>
        public const string SYSTEM_COMMUNICATION_LIST = "13BD5FCC-8F03-46B4-B193-E9C0987D2F20";

        /// <summary>
        /// System Communication Detail Block Type Guid
        /// </summary>
        public const string SYSTEM_COMMUNICATION_DETAIL = "8713F91A-8738-453D-AF13-3ED57F74137E";

        #endregion

        /// <summary>
        /// The connection opportunity signup Block Type GUID
        /// </summary>
        public const string CONNECTION_OPPORTUNITY_SIGNUP = "C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9";

        /// <summary>
        /// Content Channel View Block Type Guid
        /// </summary>
        public const string CONTENT_CHANNEL_VIEW = "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F";

        /// <summary>
        /// Content Channel Item View Block Type Guid
        /// </summary>
        public const string CONTENT_CHANNEL_ITEM_VIEW = "63659EBE-C5AF-4157-804A-55C7D565110E";

        /// <summary>
        /// Content Channel Navigation Block Type Guid
        /// </summary>
        public const string CONTENT_CHANNEL_NAVIGATION = "0E023AE3-BF08-48E0-93F8-08C32EB5CAFA";

        /// <summary>
        /// Content Component Block Type Guid
        /// </summary>
        public const string CONTENT_COMPONENT = "AD802CA1-842C-47F0-B5E9-739FE2B4A2BD";

        /// <summary>
        /// The PowerBI Account Registration Block Type Guid
        /// </summary>
        public const string POWERBI_ACCOUNT_REGISTRATION = "EA20D87E-ED46-3DAA-4C4D-4156C399B1C2";

        #region Event Block Types

        /// <summary>
        /// Event category Registration Group Placement block type guid
        /// </summary>
        public const string EVENT_REGISTRATION_GROUP_PLACEMENT = "9AF434D2-FB9B-43D7-8550-DD0B92B7A70A";

        /// <summary>
        /// Event category Registration Template Detail block type guid
        /// </summary>
        public const string EVENT_REGISTRATION_TEMPLATE_DETAIL = "91354899-304E-44C7-BD0D-55F42E6505D3";

        #endregion

        #region Finance Block Types

        /// <summary>
        /// The Convert Business Block Type Guid
        /// </summary>
        public const string CONVERT_BUSINESS = "115A7725-6760-4E86-8171-57F4A3CF6909";

        #endregion

        /// <summary>
        /// The bulk update block type guid
        /// </summary>
        public const string BULK_UPDATE = "A844886D-ED6F-4367-9C6F-667401201ED0";

        #region Assessment Block Types

        /// <summary>
        /// The assessment list block type guid
        /// </summary>
        public const string ASSESSMENT_LIST = "0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4";

        /// <summary>
        /// The conflict profile block type guid
        /// </summary>
        public const string CONFLICT_PROFILE = "91473D2F-607D-4260-9C6A-DD3762FE472D";

        /// <summary>
        /// The disc block type guid
        /// </summary>
        public const string DISC = "A161D12D-FEA7-422F-B00E-A689629680E4";

        /// <summary>
        /// The eq inventory block type guid
        /// </summary>
        public const string EQ_INVENTORY = "040CFD6D-5155-4BC9-BAEE-A53219A7BECE";

        /// <summary>
        /// The gifts assessment block type guid
        /// </summary>
        public const string GIFTS_ASSESSMENT = "A7E86792-F0ED-46F2-988D-25EBFCD1DC96";

        /// <summary>
        /// The motivators block type guid
        /// </summary>
        public const string MOTIVATORS = "18CF8DA8-5DE0-49EC-A279-D5507CFA5713";

        #endregion Assessment Block Types

        #region Groups Block Types

        /// <summary>
        /// Groups category Group Member Detail block type guid
        /// </summary>
        public const string GROUPS_GROUP_MEMBER_DETAIL = "AAE2E5C3-9279-4AB0-9682-F4D19519D678";

        /// <summary>
        /// Groups category Group Member List block type guid
        /// </summary>
        public const string GROUPS_GROUP_MEMBER_LIST = "88B7EFA9-7419-4D05-9F88-38B936E61EDD";

        #endregion

        #region RSVP Block Types

        /// <summary>
        /// RSVP category List block type guid.
        /// </summary>
        public const string RSVP_LIST = "16CE8B41-FD1B-43F2-8C8E-4E878470E8BD";

        /// <summary>
        /// RSVP category Detail block type guid.
        /// </summary>
        public const string RSVP_DETAIL = "2BC5CC6B-3618-4848-BCD9-1796AA35E7FD";

        /// <summary>
        /// RSVP category Response block type guid.
        /// </summary>
        public const string RSVP_RESPONSE = "EEFD83FB-6EE1-44F4-A012-7569F979CD6B";

        #endregion

        /// <summary>
        /// The log viewer
        /// </summary>
        public const string LOG_VIEWER = "6059FC03-E398-4359-8632-909B63FFA550";

        /// <summary>
        /// The phone number lookup block type.
        /// </summary>
        public const string PHONE_NUMBER_LOOKUP = "51BB37DA-6F3E-40EC-B80E-D381E13E01B2";
    }
}