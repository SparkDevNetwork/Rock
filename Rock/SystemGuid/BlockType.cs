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

using System;

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
        public const string PLUGIN_MANAGER = "F80268E6-2625-4565-AA2E-790C5E40A119";

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

        /// <summary>
        /// Mass Push Notifications Block Type Guid
        /// </summary>
        public const string MASS_PUSH_NOTIFICATIONS = "D886DB44-0D0F-46D3-86AE-C959C520B0FD";

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

        /// <summary>
        /// The financial account search block type
        /// </summary>
        public const string FINANCIAL_ACCOUNT_SEARCH = "867E32A8-786E-4090-BE2B-6BE780440D44";

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

        /// <summary>
        /// The oidc authorize
        /// </summary>
        public const string OIDC_AUTHORIZE = "D9E2BE51-6AC2-43D6-BE63-9E5EC571BD95";

        /// <summary>
        /// The oidc logout
        /// </summary>
        public const string OIDC_LOGOUT = "32F2171C-4CD2-48A0-AAD0-AE681CB0D2DD";

        /// <summary>
        /// The oidc scope list
        /// </summary>
        public const string OIDC_SCOPE_LIST = "0E407FC8-B5B9-488E-81E4-8EA5F7CFCAB3";

        /// <summary>
        /// The oidc scope detail
        /// </summary>
        public const string OIDC_SCOPE_DETAIL = "AA4368BD-00FA-4AB9-9591-CFD64BE6C9EA";

        /// <summary>
        /// The oidc claims
        /// </summary>
        public const string OIDC_CLAIMS = "142BE80B-5FB2-459D-AE5C-E371C79538F6";

        /// <summary>
        /// The oidc client list
        /// </summary>
        public const string OIDC_CLIENT_LIST = "616D1A98-067D-43B8-B7F5-41FB12FB894E";

        /// <summary>
        /// The oidc client detail
        /// </summary>
        public const string OIDC_CLIENT_DETAIL = "312EAD0E-4068-4211-8410-2EB45B7D8BAB";

        /// <summary>
        /// The notes
        /// </summary>
        public const string NOTES = "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3";

        /// <summary>
        /// The group attendance detail
        /// </summary>
        public const string GROUP_ATTENDANCE_DETAIL = "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B";

        /// <summary>
        /// The group attendance list
        /// </summary>
        public const string GROUP_ATTENDANCE_LIST = "5C547728-38C2-420A-8602-3CDAAC369247";

        /// <summary>
        /// The attendance self entry
        /// </summary>
        public const string ATTENDANCE_SELF_ENTRY = "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3";

        /// <summary>
        /// Workflow Entry
        /// </summary>
        public const string WORKFLOW_ENTRY = "A8BD05C8-6F89-4628-845B-059E686F089A";

        /// <summary>
        /// The Checkin Manager En Route
        /// </summary>
        public const string CHECK_IN_MANAGER_EN_ROUTE = "BC86F18C-9F38-4CA3-8CF9-5A837CBC700D";

        #region Mobile Block Types

        /// <summary>
        /// The mobile Connection &gt; <see cref="Rock.Blocks.Types.Mobile.Connection.ConnectionTypeList">Connection Type List</see> block type.
        /// </summary>
        public const string MOBILE_CONNECTION_CONNECTION_TYPE_LIST = "31E1FCCF-C4B1-4D84-992C-DEACAF3697CF";

        /// <summary>
        /// The mobile Connection &gt; <see cref="Rock.Blocks.Types.Mobile.Connection.ConnectionOpportunityList">Connection Opportunity List</see> block type.
        /// </summary>
        public const string MOBILE_CONNECTION_CONNECTION_OPPORTUNITY_LIST = "0015A574-C10A-4530-897C-F7B7C3D9393E";

        /// <summary>
        /// The mobile Connection &gt; <see cref="Rock.Blocks.Types.Mobile.Connection.ConnectionRequestList">Connection Request List</see> block type.
        /// </summary>
        public const string MOBILE_CONNECTION_CONNECTION_REQUEST_LIST = "612E9E13-434F-4E47-958D-37E1C3EEF304";

        /// <summary>
        /// The mobile Connection &gt; <see cref="Rock.Blocks.Types.Mobile.Connection.ConnectionRequestDetail">Connection Request Detail</see> block type.
        /// </summary>
        public const string MOBILE_CONNECTION_CONNECTION_REQUEST_DETAIL = "EF537CC9-5E53-4832-A473-0D5EA439C296";

        /// <summary>
        /// The mobile Connection &gt; <see cref="Rock.Blocks.Types.Mobile.Connection.AddConnectionRequest">Add Connection Request</see> block type.
        /// </summary>
        public const string MOBILE_CONNECTION_ADD_CONNECTION_REQUEST = "1380115A-B3F0-49BC-A6BC-432A59DC27A2";

        /// <summary>
        /// The mobile Core &gt; <see cref="Rock.Blocks.Types.Mobile.Core.Search"/> block type.
        /// </summary>
        public const string MOBILE_CORE_SEARCH = "41174BEA-6567-430C-AAD4-A89A5CF70FB0";

        /// <summary>
        /// The mobile Groups &gt; <see cref="Rock.Blocks.Types.Mobile.Groups.GroupRegistration">Group Registration</see> block type.
        /// </summary>
        public const string MOBILE_GROUPS_GROUP_REGISTRATION = "8A42E4FA-9FE1-493C-B6D8-7A766D96E912";

        /// <summary>
        /// The mobile groups <see cref="Rock.Blocks.Types.Mobile.Groups.GroupSchedulePreference">Group Schedule Preference</see> block type.
        /// </summary>
        public const string MOBILE_GROUPS_GROUP_SCHEDULE_PREFERENCE = "F6D0A258-F97E-4561-B881-ACBF985F89DC";

        /// <summary>
        /// The mobile groups <see cref="Rock.Blocks.Types.Mobile.Groups.GroupScheduleSignUp">Group Schedule Sign Up</see> block type.
        /// </summary>
        public const string MOBILE_GROUPS_GROUP_SCHEDULE_SIGNUP = "CA27CB14-22FD-4DE6-9C3B-0EAA0AA84708";

        /// <summary>
        /// The mobile groups <see cref="Rock.Blocks.Types.Mobile.Groups.GroupSchedulePreference">Group Schedule Preference</see> block type.
        /// </summary>
        public const string MOBILE_GROUPS_GROUP_SCHEDULE_TOOLBOX = "E00F3C6D-D007-4408-8A41-AD2A6AB29D6E";

        /// <summary>
        /// The mobile groups <see cref="Rock.Blocks.Types.Mobile.Groups.GroupScheduleUnavailability">Group Schedule Unavailability</see> block type.
        /// </summary>
        public const string MOBILE_GROUPS_GROUP_SCHEDULE_UNAVAILABILITY = "AEFF246D-A514-4D46-801E-D717E1D1D209";

        /// <summary>
        /// The mobile Groups &gt; <see cref="Rock.Blocks.Types.Mobile.Groups.GroupFinder">Group Finder</see> block type.
        /// </summary>
        public const string MOBILE_GROUPS_GROUP_FINDER = "BAC6671E-4D6F-4428-A6FA-69B8BEADF55C";

        /// <summary>
        /// The mobile Prayer &gt; Prayer Card View block type.
        /// </summary>
        public const string MOBILE_PRAYER_PRAYER_CARD_VIEW = "CA75C558-9345-47E7-99AF-D8191D31D00D";

        /// <summary>
        /// The mobile Reminders &gt; <see cref="Rock.Blocks.Types.Mobile.Reminders.ReminderList">Reminder List</see> block type.
        /// </summary>
        public const string MOBILE_REMINDERS_REMINDER_LIST = "E3FD3E7B-BF9D-4008-B71D-DF857DC20D7B";

        /// <summary>
        /// The mobile Reminders &gt; <see cref="Rock.Blocks.Types.Mobile.Reminders.ReminderList">Reminder Dashboard</see> block type.
        /// </summary>
        public const string MOBILE_REMINDERS_REMINDER_DASHBOARD = "223F5122-C93A-44CD-BFB7-AF990A2B6B65";

        /// <summary>
        /// The mobile Reminders &gt; <see cref="Rock.Blocks.Types.Mobile.Reminders.ReminderList">Reminder Edit</see> block type.
        /// </summary>
        public const string MOBILE_REMINDERS_REMINDER_EDIT = "BA26C29E-660C-470D-9FEA-5830DB15E935";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Crm.PersonProfile"/>.
        /// </summary>
        public const string MOBILE_CRM_PERSON_PROFILE = "F97E2359-BB2D-4534-821D-870F853CA5CC";

        /// <summary>
        /// The mobile attribute values &gt; <see cref="Rock.Blocks.Types.Mobile.Core.AttributeValues">Attribute Values</see> block type.
        /// </summary>
        public const string MOBILE_CORE_ATTRIBUTE_VALUES = "DF110543-C295-4DD9-B06E-82640AC63610";

        /// <summary>
        /// The mobile group members &gt; <see cref="Rock.Blocks.Types.Mobile.Crm.GroupMembers">Group Members</see> block type.
        /// </summary>
        public const string MOBILE_CRM_GROUP_MEMBERS  = "1F1E7598-8D51-4750-8D61-E5791A226FDB";

        #endregion

        #region Obsidian Block Types

        /// <summary>
        /// The obsidian event registration entry
        /// </summary>
        public const string OBSIDIAN_EVENT_REGISTRATION_ENTRY = "0252E237-0684-4426-9E5C-D454A13E152A";

        /// <summary>
        /// The obsidian event control gallery
        /// </summary>
        public const string OBSIDIAN_EXAMPLE_CONTROL_GALLERY = "3ED1B4B2-FD1C-4E4B-B4B9-2DE4E6EF8915";

        /// <summary>
        /// The obsidian event field type gallery
        /// </summary>
        public const string OBSIDIAN_EXAMPLE_FIELD_TYPE_GALLERY = "B9C209C2-ABB8-4B48-A68E-944572007B03";

        #endregion Obsidian Block Types

        #region Reporting Block Types

        /// <summary>
        /// Data Views with persistence enabled block Guid
        /// </summary>
        public const string PERSISTED_DATA_VIEW_LIST = "3C4FAFAE-41D1-4FF2-B6DC-FF99CD4DABBE";

        /// <summary>
        /// The dataview search results block type
        /// </summary>
        public const string DATAVIEW_SEARCH_RESULTS = "BFB625F7-75CA-48FE-9C82-90E47374242B";

        /// <summary>
        /// The report search results block type
        /// </summary>
        public const string REPORT_SEARCH_RESULTS = "13955B32-11F4-4606-8C31-4C6E5324C81A";

        #endregion Reporting Block Types

        #region Steps Block Types

        /// <summary>
        /// The Step Flow
        /// </summary>
        public const string STEP_FLOW = "2B4E0128-BCDF-48BF-AEC9-85001169DA3E";
        #endregion Steps Block Types

        #region Reporting Block Types

        /// <summary>
        /// The reporting dashboard liquid/lava dashboard widget.
        /// </summary>
        public const string REPORTING_LAVA_DASHBOARD_WIDGET = "AC19A4F3-2E88-487E-8E88-377C1C20DBD5";

        #endregion Reporting Block Types

        #region Reminder Block Types

        /// <summary>
        /// The reminder edit block.
        /// </summary>
        public const string REMINDER_EDIT = "7CE0E052-1400-4AC1-8AE0-ABF770A1B11E";

        /// <summary>
        /// The reminder links block.
        /// </summary>
        public const string REMINDER_LINKS = "EC59B6D6-5CA1-4367-9109-CDDC92357D35";

        /// <summary>
        /// The reminder list block.
        /// </summary>
        public const string REMINDER_LIST = "FC8DC018-C702-4A23-81BA-DF9DD6008CB6";

        /// <summary>
        /// The reminder types block.
        /// </summary>
        public const string REMINDER_TYPES = "E4161700-6882-4A50-B362-8E4C8F37C79D";

        #endregion Reminder Block Types

        /// <summary>
        /// The contribution statement lava (Legacy)
        /// </summary>
        [Obsolete( "Use ContributionStatementGenerator instead" )]
        [RockObsolete( "1.12.4" )]
        public const string CONTRIBUTION_STATEMENT_LAVA_LEGACY = "AF986B72-ADD9-4E05-971F-1DE4EBED8667";
    }
}