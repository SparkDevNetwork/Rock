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
    /// Guids for EntityTypes
    /// </summary>
    public static class EntityType
    {
        /// <summary>
        /// The achievement attempt
        /// </summary>
        public const string ACHIEVEMENT_ATTEMPT = "5C144B51-3D2E-4BC2-B6C7-7E4CB890E15F";

        /// <summary>
        /// The achievement type
        /// </summary>
        public const string ACHIEVEMENT_TYPE = "0E99356C-0DEA-4F24-944E-21CD5FA83B9E";

        /// <summary>
        /// The achievement type prerequisite
        /// </summary>
        public const string ACHIEVEMENT_TYPE_PREREQUISITE = "5362DB19-B8E1-4378-A66A-FB097CE3AB90";

        /// <summary>
        /// The  guid for the Rock.Model.AssessmentType entity.
        /// </summary>
        public const string ASSESSMENT_TYPE = "D17A28AC-F529-4AB0-A790-C21F9E74AC89";

        /// <summary>
        /// The guid for the Rock.Model.Attendance entity.
        /// </summary>
        public const string ATTENDANCE = "4CCB856F-51E0-4E48-B94A-1705EFBA6C9E";

        /// <summary>
        /// The guid for the Rock.Model.Attribute entity.
        /// </summary>
        public const string ATTRIBUTE = "5997C8D3-8840-4591-99A5-552919F90CBD";

        /// <summary>
        /// The database authentication provider
        /// </summary>
        public const string AUTHENTICATION_DATABASE = "4E9B798F-BB68-4C0E-9707-0928D15AB020";

        /// <summary>
        /// The pin authentication provider
        /// </summary>
        public const string AUTHENTICATION_PIN = "1FB5A259-F45C-4857-AF3D-3B9E32DB0EEE";

        /// <summary>
        /// The passwordless authentication provider
        /// </summary>
        public const string AUTHENTICATION_PASSWORDLESS = "2D037783-09BD-48BA-8C1D-626B0BC82495";

        /// <summary>
        /// The guid for the Rock.Model.Badge entity
        /// </summary>
        public const string BADGE = "99300129-6F4C-45B2-B486-71123F046289";

        /// <summary>
        /// The benevolence request
        /// </summary>
        public const string BENEVOLENCE_REQUEST = "CF0CE5C1-9286-4310-9B50-10D040F8EBD2";

        /// <summary>
        /// The guid for the <seealso cref="Rock.Model.BenevolenceType"/> entity.
        /// </summary>
        public const string BENEVOLENCE_TYPE = "9DB5D35A-F2DF-4AFF-AB9F-06C2EB587C0D";

        /// <summary>
        /// The Block entity type
        /// </summary>
        public const string BLOCK = "D89555CA-9AE4-4D62-8AF1-E5E463C1EF65";

        /// <summary>
        /// The campus
        /// </summary>
        public const string CAMPUS = "00096BED-9587-415E-8AD4-4E076AE8FBF0";

        /// <summary>
        /// The checkr provider
        /// </summary>
        public const string CHECKR_PROVIDER = "8D9DE88A-C649-47B2-BA5C-92A24F60AE61";

        /// <summary>
        /// The content channel type
        /// </summary>
        public const string CONTENT_CHANNEL = "44484685-477E-4668-89A6-84F29739EB68";

        /// <summary>
        /// The content channel item type
        /// </summary>
        public const string CONTENT_CHANNEL_ITEM = "BF12AE64-21FB-433B-A8A4-E40E8C426DDA";

        /// <summary>
        /// The guid for the email communication medium
        /// </summary>
        public const string COMMUNICATION_MEDIUM_EMAIL = "5A653EBE-6803-44B4-85D2-FB7B8146D55D";

        /// <summary>
        /// The guid for the push notification communication medium
        /// </summary>
        public const string COMMUNICATION_MEDIUM_PUSH_NOTIFICATION = "3638C6DF-4FF3-4A52-B4B8-AFB754991597";

        /// <summary>
        /// The guid for the SMS communication medium
        /// </summary>
        public const string COMMUNICATION_MEDIUM_SMS = "4BC02764-512A-4A10-ACDE-586F71D8A8BD";

        /// <summary>
        /// The guid for Rock.Model.CommunicationTemplate
        /// </summary>
        public const string COMMUNICATION_TEMPLATE = "A9493AFE-4316-4651-800D-5028E4C7444D";

        /// <summary>
        /// The guid for the Rock.Model.ConnectionActivityType entity
        /// </summary>
        public const string CONNECTION_ACTIVITY_TYPE = "97B143F0-CB9D-4652-8FF1-FF2FA1EA4945";

        /// <summary>
        /// The guid for the Rock.Model.ConnectionOpportunity entity
        /// </summary>
        public const string CONNECTION_OPPORTUNITY = "79F64363-BC90-4109-9D31-A5EEB397CB2F";

        /// <summary>
        /// The guid for the Rock.Model.ConnectionOpportunityCampus entity
        /// </summary>
        public const string CONNECTION_OPPORTUNITY_CAMPUS = "E656E8B3-12AB-476E-AA63-5F9B76F64A08";

        /// <summary>
        /// The guid for the Rock.Model.ConnectionOpportunityGroup entity
        /// </summary>
        public const string CONNECTION_OPPORTUNITY_GROUP = "CD3F425C-9B36-4433-9C38-D58DE42C9F65";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.ConnectionOpportunityConnectorGroup"/> 
        /// </summary>
        public const string CONNECTION_OPPORTUNITY_CONNECTOR_GROUP = "2ADBE499-C9EC-479B-B33B-6E92BDE09FD1";

        /// <summary>
        /// The guid for the Rock.Model.ConnectionRequest entity
        /// </summary>
        public const string CONNECTION_REQUEST = "36B0D0C7-8125-48FA-9DA2-729AAA65F718";

        /// <summary>
        /// The guid for the Rock.Model.ConnectionRequestActivity entity
        /// </summary>
        public const string CONNECTION_REQUEST_ACTIVITY = "3248F40D-7661-42CC-AD9B-EF63322937B7";

        /// <summary>
        /// The guid for the Rock.Model.ConnectionRequestWorkflow entity
        /// </summary>
        public const string CONNECTION_REQUEST_WORKFLOW = "C69D1C9F-5521-4C83-8FE9-5044ECC2CE65";

        /// <summary>
        /// The guid for the Rock.Model.ConnectionStatus entity
        /// </summary>
        public const string CONNECTION_STATUS = "F3840C8B-63BF-4F98-AC4A-9336896E589B";

        /// <summary>
        /// The guid for the Rock.Model.ConnectionType entity
        /// </summary>
        public const string CONNECTION_TYPE = "B1E52EAD-65BD-4C4D-BCCD-73368067621D";

        /// <summary>
        /// The guid for the Rock.Model.ConnectionWorkflow entity
        /// </summary>
        public const string CONNECTION_WORKFLOW = "4EB8711F-7301-4699-A223-0505A7CEB20A";

        /// <summary>
        /// The guid for the Rock.Model.ContentCollection entity
        /// </summary>
        public const string CONTENT_COLLECTION = "AD7B9219-1B47-4164-9DD1-90F0AF588CB8";

        /// <summary>
        /// The guid for the Rock.Model.ContentCollectionSource entity
        /// </summary>
        public const string CONTENT_COLLECTION_SOURCE = "46BD0E73-14B3-499D-B8BE-C0EF6BDCD733";

        /// <summary>
        /// The guid for the Rock.Model.ContentTopic entity
        /// </summary>
        public const string CONTENT_TOPIC = "CD4BE244-147E-4A90-9137-B35FC35B5A52";

        /// <summary>
        /// The guid for the Rock.Model.ContentTopicDomain entity
        /// </summary>
        public const string CONTENT_TOPIC_DOMAIN = "8C26A4D9-9C0B-4433-AF31-6C7094BEFB51";

        /// <summary>
        /// The guid for the Rock.Model.DataView entity.
        /// </summary>
        public const string DATAVIEW = "57F8FA29-DCF1-4F74-8553-87E90F234139";

        /// <summary>
        /// The guid for the Rock.Model.DefinedType entity.
        /// </summary>
        public const string DEFINED_TYPE = "6028D502-79F4-4A74-9323-525E90F900C7";

        /// <summary>
        /// The guid for Rock.Model.DefinedValue entity
        /// </summary>
        public const string DEFINED_VALUE = "53D4BF38-C49E-4A52-8B0E-5E016FB9574E";

        /// <summary>
        /// The guid for Rock.Model.EntityType entity.
        /// </summary>
        public const string ENTITY_TYPE = "A2277FBA-D09F-4D07-B0AB-1C650C25A7A7";

        /// <summary>
        /// The guid for Rock.Model.EventCalendar entity.
        /// </summary>
        public const string EVENT_CALENDAR = "E67D8D6D-4FE6-48D5-A940-A39213047314";

        /// <summary>
        /// The guid for the Rock.Model.FinancialAccount entity.
        /// </summary>
        public const string FINANCIAL_ACCOUNT = "798BCE48-6AA7-4983-9214-F9BCEFB4521D";

        /// <summary>
        /// The guid for the Rock.Model.FinancialBatch entity.
        /// </summary>
        public const string FINANCIAL_BATCH = "BDD09C8E-2C52-4D08-9062-BE7D52D190C2";

        /// <summary>
        /// The guid for the Rock.Model.FinancialScheduledTransaction entity.
        /// </summary>
        public const string FINANCIAL_SCHEDULED_TRANSACTION = "76824E8A-CCC4-4085-84D9-8AF8C0807E20";

        /// <summary>
        /// The guid for the Rock.Model.FinancialTransaction entity.
        /// </summary>
        public const string FINANCIAL_TRANSACTION = "2C1CB26B-AB22-42D0-8164-AEDEE0DAE667";

        /// <summary>
        /// The guid for the Rock.Model.FinancialTransactionDetail entity.
        /// </summary>
        public const string FINANCIAL_TRANSACTION_DETAIL = "AC4AC28B-8E7E-4D7E-85DB-DFFB4F3ADCCE";

        /// <summary>
        /// The guid for the Rock.Model.Group entity.
        /// </summary>
        public const string GROUP = "9BBFDA11-0D22-40D5-902F-60ADFBC88987";

        /// <summary>
        /// The guid for the Rock.Model.GroupMember entity.
        /// </summary>
        public const string GROUP_MEMBER = "49668B95-FEDC-43DD-8085-D2B0D6343C48";

        /// <summary>
        /// The guid for the <see cref="Rock.Model.History">Rock.Model.History</see> entity.
        /// </summary>
        public const string HISTORY = "546D5F43-1184-47C9-8265-2D7BF4E1BCA5";

        /// <summary>
        /// The HTTP module component
        /// </summary>
        public const string HTTP_MODULE_COMPONENT = "EDE69F48-5E05-4260-B360-DA37DFD1AB83";

        /// <summary>
        /// The guid for <see cref="Rock.Model.Interaction"/>
        /// </summary>
        public const string INTERACTION = "3BB4B095-2DE4-4009-8FA2-705BF284F7B7";

        /// <summary>
        /// The guid for the <see cref="Rock.Model.InteractionEntity"/> entity.
        /// </summary>
        public const string INTERACTION_ENTITY = "AB3AC547-CCEF-4662-9646-64F16813DAC4";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.InteractiveExperience"/>
        /// </summary>
        public const string INTERACTIVE_EXPERIENCE = "3D90E693-476E-4DFC-B958-A28D1DD370BF";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.InteractiveExperienceAction"/>
        /// </summary>
        public const string INTERACTIVE_EXPERIENCE_ACTION = "8635E7E7-3576-47FF-92DE-30A69EB5D011";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.InteractiveExperienceAnswer"/>
        /// </summary>
        public const string INTERACTIVE_EXPERIENCE_ANSWER = "D11DA9D4-8887-4EC2-B396-78556926DE89";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.InteractiveExperienceOccurrence"/>
        /// </summary>
        public const string INTERACTIVE_EXPERIENCE_OCCURRENCE = "2D1263A1-A3E7-4568-AA4B-C1234824188D";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.InteractiveExperienceSchedule"/>
        /// </summary>
        public const string INTERACTIVE_EXPERIENCE_SCHEDULE = "D23B4DCF-545A-490F-AEAD-BA78A8FB4028";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.InteractiveExperienceScheduleCampus"/>
        /// </summary>
        public const string INTERACTIVE_EXPERIENCE_SCHEDULE_CAMPUS = "ABEF4137-F25B-4B2E-AF01-2CEFF704FC11";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.LavaShortcode"/> (well known as of v14)
        /// </summary>
        public const string LAVA_SHORTCODE = "7574A473-3326-4973-8DF6-C7BF5F64EB36";

        /// <summary>
        /// The EntityType Guid for LavaShortcodeCategory 'joiner' table <see cref="Rock.Model.LavaShortcodeConfiguration"/>
        /// </summary>
        public const string LAVA_SHORTCODE_CATEGORY = "6CFE20AD-F883-4F53-A678-0D048406299D";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.Layout"/>
        /// </summary>
        public const string LAYOUT = "9DACC861-FED4-47FC-946D-D6A120FF6D56";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.LearningActivity"/>
        /// </summary>
        public const string LEARNING_ACTIVITY = "E82F540C-F483-4D4A-898A-3AE7FF76F75A";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.LearningActivityCompletion"/>
        /// </summary>
        public const string LEARNING_ACTIVITY_COMPLETION = "14D1295A-CE9E-4FCB-A63C-6DF04DB5E9B1";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.LearningClass"/>
        /// </summary>
        public const string LEARNING_CLASS = "EB41E4E1-64B1-4AA1-8F66-F0DFD81557D9";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.LearningClassAnnouncement"/>
        /// </summary>
        public const string LEARNING_CLASS_ANNOUNCEMENT = "D2CE59D3-55E1-4275-9EA1-38C18A05A32B";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.LearningClassContentPage"/>
        /// </summary>
        public const string LEARNING_CLASS_CONTENT_PAGE = "72A061C4-B7E6-4A91-A4F2-80551F772D46";
        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.LearningCourse"/>
        /// </summary>
        public const string LEARNING_COURSE = "21870376-4A6E-4402-ACE3-42AA4441FC2E";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.LearningCourseRequirement"/>
        /// </summary>
        public const string LEARNING_COURSE_REQUIREMENT = "576F54E1-CE70-43ED-A7D8-5169529C70E9";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.LearningGradingSystem"/>
        /// </summary>
        public const string LEARNING_GRADING_SYSTEM = "0B693CAF-3718-4913-B7AC-61D31B4DF099";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.LearningGradingSystem"/>
        /// </summary>
        public const string LEARNING_GRADING_SYSTEM_SCALE = "14BA9B5B-6B4A-4462-ACDF-898DCEC9EC0D";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.LearningParticipant"/>
        /// </summary>
        public const string LEARNING_PARTICIPANT = "03195758-1770-4794-9487-7A4AA02930A7";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.LearningProgram"/>
        /// </summary>
        public const string LEARNING_PROGRAM = "AFD89885-6923-4FA1-B6FE-A1DF8D821BBC";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.LearningProgramCompletion"/>
        /// </summary>
        public const string LEARNING_PROGRAM_COMPLETION = "B82813A7-E70E-4F04-8568-0D84EFB484B2";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.LearningSemester"/>
        /// </summary>
        public const string LEARNING_SEMESTER = "F9A40931-0ACE-4B50-A507-0D1D75F92BC4";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.MediaAccount"/> 
        /// </summary>
        public const string MEDIA_ACCOUNT = "CD35F034-AC18-40D5-B703-6BF16D79C51C";

        /// <summary>
        /// The guid for the Rock.Model.MetricCategory entity
        /// </summary>
        public const string METRICCATEGORY = "3D35C859-DF37-433F-A20A-0FFD0FCB9862";

        /// <summary>
        /// The guid for the Rock.Model.MergeTemplate entity
        /// </summary>
        public const string MERGE_TEMPLATE = "CD1DB988-6891-4B0F-8D1B-B0A311A3BC3E";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Prayer.AnswerToPrayer"/>.
        /// </summary>
        public const string MOBILE_ANSWER_TO_PRAYER_BLOCK_TYPE = "759AFCA0-9E0B-4A22-A402-CD4499F2A457";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Content
        /// </summary>
        public const string MOBILE_CONTENT_BLOCK_TYPE = "B9ADB0A5-62B0-4D74-BDFF-1AA959788602";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.ContentChannelItemList
        /// </summary>
        public const string MOBILE_CONTENT_CHANNEL_ITEM_LIST_BLOCK_TYPE = "6DBF59D6-EB40-43C8-8859-F38254EC3F6D";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.ContentChannelItemView
        /// </summary>
        public const string MOBILE_CONTENT_CHANNEL_ITEM_VIEW_BLOCK_TYPE = "44A8B647-E0A7-42E7-9A75-276310F7E7BB";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Cms.DailyChallengeEntry"/>
        /// </summary>
        public const string MOBILE_CMS_DAILY_CHALLENGE_ENTRY = "E9BC058A-CFE4-498B-A7E7-DD38DC74B30E";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.LavaItemList
        /// </summary>
        public const string MOBILE_LAVA_ITEM_LIST_BLOCK_TYPE = "60AD6D70-8A2A-4CC1-97D5-199300AF77EE";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Login
        /// </summary>
        public const string MOBILE_LOGIN_BLOCK_TYPE = "6CE2D3D7-18D8-49FF-8C39-0CA98EB5DEB4";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Core.Notes
        /// </summary>
        public const string MOBILE_CORE_NOTES_BLOCK_TYPE = "2FED71D1-4A60-4EB5-B971-530B5D1FC041";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Connection.ConnectionTypeList"/>.
        /// </summary>
        public const string MOBILE_CONNECTION_CONNECTION_TYPE_LIST_BLOCK_TYPE = "F30667AC-5FAA-429C-AD7F-D4B7C0C5C293";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Connection.ConnectionOpportunityList"/>.
        /// </summary>
        public const string MOBILE_CONNECTION_CONNECTION_OPPORTUNITY_LIST_BLOCK_TYPE = "CB151D80-DBEF-4A1E-A816-3DF4CD2DE45A";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Connection.ConnectionRequestList"/>.
        /// </summary>
        public const string MOBILE_CONNECTION_CONNECTION_REQUEST_LIST_BLOCK_TYPE = "E1C52624-898F-46EF-98B0-3C06B6D81654";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Connection.ConnectionRequestDetail"/>.
        /// </summary>
        public const string MOBILE_CONNECTION_CONNECTION_REQUEST_DETAIL_BLOCK_TYPE = "6C9B8E2A-A0B1-4E52-B419-250A77AFBCBF";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Connection.AddConnectionRequest"/>.
        /// </summary>
        public const string MOBILE_CONNECTION_ADD_CONNECTION_REQUEST = "F41E7BE3-2854-40FF-82C8-1FDEA12B8B2F";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Prayer.MyPrayerRequests"/>.
        /// </summary>
        public const string MOBILE_MY_PRAYER_REQUESTS_BLOCK_TYPE = "E644DE6A-44CA-48AC-BF33-5429DA8052C6";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.ProfileDetails
        /// </summary>
        public const string MOBILE_PROFILE_DETAILS_BLOCK_TYPE = "A1ED4948-0778-4E13-B434-E97795DDB68B";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Register
        /// </summary>
        public const string MOBILE_REGISTER_BLOCK_TYPE = "4459357F-E422-45D1-855D-C4681101F848";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.WorkflowEntry
        /// </summary>
        public const string MOBILE_WORKFLOW_ENTRY_BLOCK_TYPE = "02D2DBA8-5300-4367-B15B-E37DFB3F7D1E";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Cms.Hero
        /// </summary>
        public const string MOBILE_CMS_HERO_BLOCK_TYPE = "49BE78CD-2D19-44C4-A6BF-4F3B5D3F97C8";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Cms.StructuredContentView
        /// </summary>
        public const string MOBILE_CMS_STRUCTUREDCONTENTVIEW_BLOCK_TYPE = "219660C4-8F32-46DA-B8E3-A7A6FA0D6B76";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Core.Search"/>.
        /// </summary>
        public const string MOBILE_CORE_SEARCH_BLOCK_TYPE = "F81015CD-EBA9-4358-B930-4F1AB29AF879";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Core.SmartSearch" />.
        /// </summary>
        public const string MOBILE_CORE_SMART_SEARCH_BLOCK_TYPE = "45BE4816-3F5B-4AD1-BA89-819325D7E8CF";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Core.QuickNote" />.
        /// </summary>
        public const string MOBILE_CORE_QUICK_NOTE_BLOCK_TYPE = "9AA328FB-8FBB-4C5D-A898-C9B355051ADD";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Core.MyNotes" />.
        /// </summary>
        public const string MOBILE_CORE_MY_NOTES_BLOCK_TYPE = "1CCC09C4-2994-4009-813F-2F4B86C13BFE";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Events.CalendarEventList
        /// </summary>
        public const string MOBILE_EVENTS_CALENDAREVENTLIST_BLOCK_TYPE = "6FB9F1F4-5F24-4A22-A6EB-A7FA499179A9";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Events.CalendarView
        /// </summary>
        public const string MOBILE_EVENTS_CALENDARVIEW_BLOCK_TYPE = "5A26F32F-892E-4E76-B64A-0F54A77C863D";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Events.LiveExperience
        /// </summary>
        public const string MOBILE_EVENTS_LIVEEXPERIENCE_BLOCK_TYPE = "FC408E41-872D-4B71-A08C-513D7500E980";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Communication.CommunicationView
        /// </summary>
        public const string MOBILE_COMMUNICATION_COMMUNICATIONVIEW_BLOCK_TYPE = "4AF5FCEF-CBF6-486B-A04D-920E31356B7F";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Communication.CommunicationEntry
        /// </summary>
        public const string MOBILE_COMMUNICATION_COMMUNICATIONENTRY_BLOCK_TYPE = "9A952F9F-F619-4063-B1BB-CFB2E6983C01";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Events.CalendarEventItemOccurrenceView
        /// </summary>
        public const string MOBILE_EVENTS_CALENDAREVENTITEMOCCURRENCEVIEW_BLOCK_TYPE = "04C43693-C524-4679-9F65-047F94A74CAB";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Events.CommunicationListSubscribe
        /// </summary>
        public const string MOBILE_EVENTS_COMMUNICATION_LIST_SUBSCRIBE_BLOCK_TYPE = "C4B81A58-6380-4C38-85E8-0536E584310E";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Events.EventItemOccurrenceListByAudienceLava
        /// </summary>
        public const string MOBILE_EVENTS_EVENTITEMOCCURRENCELISTBYAUDIENCELAVA_BLOCK_TYPE = "95BAF1B3-5B4B-430C-BDCC-268142C708BD";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Events.PrayerSession
        /// </summary>
        public const string MOBILE_EVENTS_PRAYER_SESSION_BLOCK_TYPE = "BCAF9B7B-2ADE-496B-9303-150F495851FC";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Events.PrayerSessionSetup
        /// </summary>
        public const string MOBILE_EVENTS_PRAYER_SESSION_SETUP_BLOCK_TYPE = "51431866-FF92-433C-8B0F-0F6BBAD9BCE7";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Groups.GroupRegistration
        /// </summary>
        [Obsolete( "Use MOBILE_GROUPS_GROUP_REGISTRATION_BLOCK_TYPE instead." )]
        [RockObsolete( "1.13" )]
        public const string MOBILE_GROUPS_GROUP_ADD_TO_GROUP = "E0664BDC-9583-44F2-AC8D-23AE48603EAB";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Groups.GroupAttendanceEntry
        /// </summary>
        public const string MOBILE_GROUPS_GROUP_ATTENDANCE_ENTRY_BLOCK_TYPE = "1655E6A9-2BD6-4FA0-8886-D64DCA177FBB";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Groups.GroupEdit
        /// </summary>
        public const string MOBILE_GROUPS_GROUP_EDIT_BLOCK_TYPE = "DE46759A-CE15-4F27-9FC8-154CD30D4637";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Groups.GroupFinder"/>
        /// </summary>
        public const string MOBILE_GROUPS_GROUP_FINDER_BLOCK_TYPE = "15492F6A-344A-484E-AA26-A5E667CBD502";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Groups.GroupMemberEdit
        /// </summary>
        public const string MOBILE_GROUPS_GROUP_MEMBER_EDIT_BLOCK_TYPE = "61208516-9051-4E0E-AC46-6C8E1F104F3A";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Groups.GroupMemberList
        /// </summary>
        public const string MOBILE_GROUPS_GROUP_MEMBER_LIST_BLOCK_TYPE = "70652D98-9285-4707-8F46-B7FC48B6503D";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Groups.GroupMemberView
        /// </summary>
        public const string MOBILE_GROUPS_GROUP_MEMBER_VIEW_BLOCK_TYPE = "3213DCBC-C5EC-4DD2-BB78-19B3636AE842";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Groups.GroupRegistration"/>
        /// </summary>
        public const string MOBILE_GROUPS_GROUP_REGISTRATION_BLOCK_TYPE = "E0664BDC-9583-44F2-AC8D-23AE48603EAB";

        /// <summary>
        /// The GUID for entity <see cref="Rock.Blocks.Types.Mobile.Groups.GroupSchedulePreference"/>
        /// </summary>
        public const string MOBILE_GROUPS_GROUP_SCHEDULE_PREFERENCE = "36B341A9-07FC-43A1-970A-E5D4CDB83E6A";

        /// <summary>
        /// The GUID for entity <see cref="Rock.Blocks.Types.Mobile.Groups.GroupScheduleSignUp"/>
        /// </summary>
        public const string MOBILE_GROUPS_GROUP_SCHEDULE_SIGNUP = "DE3893A7-C353-4D19-A3FA-8B8EA99984F9";

        /// <summary>
        /// The GUID for entity <see cref="Rock.Blocks.Types.Mobile.Groups.GroupScheduleToolbox"/>
        /// </summary>
        public const string MOBILE_GROUPS_GROUP_SCHEDULE_TOOLBOX = "DCC6C7C8-1040-4F0E-94ED-90450E2E9610";

        /// <summary>
        /// The GUID for entity <see cref="Rock.Blocks.Types.Mobile.Groups.GroupScheduleUnavailability"/>
        /// </summary>
        public const string MOBILE_GROUPS_GROUP_SCHEDULE_UNAVAILABILITY = "300E3105-A9C9-4C86-8BD1-CC041B6953C6";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Groups.GroupView
        /// </summary>
        public const string MOBILE_GROUPS_GROUP_VIEW_BLOCK_TYPE = "564C4D86-C9DF-48D0-84B6-DD3FCC1A5158";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.Prayer.PrayerRequestDetails
        /// </summary>
        public const string MOBILE_PRAYER_PRAYER_REQUEST_DETAILS_BLOCK_TYPE = "F8E56BC0-E9D1-44A4-9900-46589A1FB784";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Prayer.PrayerCardView"/>.
        /// </summary>
        public const string MOBILE_PRAYER_PRAYER_CARD_VIEW_BLOCK_TYPE = "0D0F1D7E-2D75-451B-95EE-0610B8F26BBF";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Security.OnboardPerson"/>.
        /// </summary>
        public const string MOBILE_SECURITY_ONBOARD_PERSON = "C9B7F36A-F70A-4ABF-9422-B18E579F927F";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Reminders.ReminderList"/>.
        /// </summary>
        public const string MOBILE_REMINDERS_REMINDER_LIST = "35B1CA9F-DCD9-453F-892E-33E0E09E7CB3";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Reminders.ReminderDashboard"/>.
        /// </summary>
        public const string MOBILE_REMINDERS_REMINDER_DASHBOARD = "AD29BE7E-00B2-4AE3-8DA4-756C348E7AFA";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Reminders.ReminderEdit"/>.
        /// </summary>
        public const string MOBILE_REMINDERS_REMINDER_EDIT = "A07DA3CE-4598-4177-AD47-B0D1EBFB1E7A";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Crm.PersonProfile"/>.
        /// </summary>
        public const string MOBILE_CRM_PERSON_PROFILE = "A1EEA3BD-7B40-47A9-82D4-7187290C917C";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Core.AttributeValues"/>.
        /// </summary>
        public const string MOBILE_CORE_ATTRIBUTE_VALUES = "6751AC1E-C467-4416-9F02-0B9A0D1FAC2D";

        /// <summary>
        /// The GUID for the entity <see cref="Rock.Blocks.Types.Mobile.Crm.GroupMembers"/>.
        /// </summary>
        public const string MOBILE_CRM_GROUP_MEMBERS = "592242ED-7536-49EA-94DE-7B4EBA7E87A6";

        /// <summary>
        /// The GUID for the entity Rock.Blocks.Types.Mobile.CheckIn.CheckIn.
        /// </summary>
        public const string MOBILE_CHECKIN_CHECKIN = "BC0A4B6C-9F6D-4D39-8FFE-B6F9FA4B2F49";

        /// <summary>
        /// The obsidian event registration entry
        /// </summary>
        public const string OBSIDIAN_EVENT_REGISTRATION_ENTRY = "06AAC065-BF89-483D-B671-80F0F72779A6";

        /// <summary>
        /// The obsidian event control gallery
        /// </summary>
        public const string OBSIDIAN_EXAMPLE_CONTROL_GALLERY = "7B916FEC-9395-4877-9856-427419C50AB5";

        /// <summary>
        /// The obsidian event field type gallery
        /// </summary>
        public const string OBSIDIAN_EXAMPLE_FIELD_TYPE_GALLERY = "82F9C803-C998-46B2-B354-783D4D1E3B43";

        /// <summary>
        /// The obsidian form builder detail
        /// </summary>
        public const string OBSIDIAN_FORM_BUILDER_DETAIL_BLOCK_TYPE = "BB44D8ED-DEDC-4C9B-A30D-C636019BC960";

        /// <summary>
        /// The obsidian form template detail
        /// </summary>
        public const string OBSIDIAN_FORM_TEMPLATE_DETAIL_BLOCK_TYPE = "4A08BC88-AD45-4106-BDD7-184A14B39B9A";

        /// <summary>
        /// The guid for the Rock.Model.Note entity
        /// </summary>
        public const string NOTE = "53DC1E78-14A5-44DE-903F-6A2CB02164E7";

        /// <summary>
        /// The guid for the Rock.Model.Page entity
        /// </summary>
        public const string PAGE = "E104DCDF-247C-4CED-A119-8CC51632761F";

        /// <summary>
        /// The guid for the Rock.Model.Person entity
        /// </summary>
        public const string PERSON = "72657ED8-D16E-492E-AC12-144C5E7567E7";

        /// <summary>
        /// The guid for the Rock.Model.PersonAlias entity
        /// </summary>
        public const string PERSON_ALIAS = "90F5E87B-F0D5-4617-8AE9-EB57E673F36F";

        /// <summary>
        /// The guid for the Rock.Workflow.Action.PersonGetCampusTeamMember entity
        /// </summary>
        public const string PERSON_GET_CAMPUS_TEAM_MEMBER = "6A4F7FEC-3D49-4A31-882C-2D10DB84231E";

        /// <summary>
        /// The GUID for the Rock.Follow.Event.PersonNoteAdded entity
        /// </summary>
        public const string PERSON_NOTE_ADDED = "C4AB0F1B-E036-4D14-BFB7-30BAF12D648A";

        /// <summary>
        /// The GUID for the Rock.Follow.Event.PersonPrayerRequest entity
        /// </summary>
        public const string PERSON_PRAYER_REQUEST = "DAE05FAE-A26F-465A-836C-BAA0EFA1267B";

        /// <summary>
        /// The GUID for the Rock.Model.PersonPreference entity
        /// </summary>
        public const string PERSON_PREFERENCE = "FDCF766C-F36B-403B-89F3-7030DA65507E";

        /// <summary>
        /// The guid for the Rock.Model.PersonSignal entity
        /// </summary>
        public const string PERSON_SIGNAL = "0FFF77A1-E92D-4A05-8B36-1D2B6D46660F";

        /// <summary>
        /// The protect my ministry provider
        /// </summary>
        public const string PROTECT_MY_MINISTRY_PROVIDER = "C16856F4-3C6B-4AFB-A0B8-88A303508206";

        /// <summary>
        /// The guid for the Rock.Model.Registration entity
        /// </summary>
        public const string REGISTRATION = "D2F294C6-E161-4A56-85C7-CD74D535F61A";

        /// <summary>
        /// The guid for the Rock.Model.RegistrationTemplate entity
        /// </summary>
        public const string REGISTRATION_TEMPLATE = "A01E3E99-A8AD-4C6C-BAAC-98795738BA70";

        /// <summary>
        /// The guid for the Rock.Model.Reminder entity
        /// </summary>
        public const string REMINDER = "46CC0D74-BE46-4D5D-A6F1-0811645721AC";

        /// <summary>
        /// The guid for the Rock.Model.ReminderType entity
        /// </summary>
        public const string REMINDER_TYPE = "B2B0B6F3-0E3B-40CF-BA93-FBB99D50788C";

        /// <summary>
        /// The LiquidSelect DataSelect field for Reporting
        /// </summary>
        public const string REPORTING_DATASELECT_LIQUIDSELECT = "C130DC52-CA31-45EE-A4F2-6C53A838EF3D";

        /// <summary>
        /// The guid for the Rock.Model.Schedule entity
        /// </summary>
        public const string SCHEDULE = "0B2C38A7-D79C-4F85-9757-F1B045D32C8A";

        /// <summary>
        /// The guid for the Rock.Search.Group.Name search component.
        /// </summary>
        public const string SEARCH_COMPONENT_GROUP_NAME = "94825231-DC38-4DC0-A1D3-64B4AD6A87F0";

        /// <summary>
        /// The guid for the Rock.Search.Person.Name search component.
        /// </summary>
        public const string SEARCH_COMPONENT_PERSON_NAME = "3B1D679A-290F-4A53-8E11-159BF0517A19";

        /// <summary>
        /// The guid for the Rock.Search.Person.Email search component.
        /// </summary>
        public const string SEARCH_COMPONENT_PERSON_EMAIL = "00095C10-72C9-4C82-844E-AE8B146DE4F1";

        /// <summary>
        /// The guid for the Rock.Search.Person.Phone search component.
        /// </summary>
        public const string SEARCH_COMPONENT_PERSON_PHONE = "5F92ECC3-4EBD-4C41-A691-C03F1DA4F7BF";

        /// <summary>
        /// The guid for the Rock.Workflow.Action.SendEmail entity
        /// </summary>
        public const string SEND_EMAIL = "66197B01-D1F0-4924-A315-47AD54E030DE";

        /// <summary>
        /// The Service Job entity type
        /// </summary>
        public const string SERVICE_JOB = "52766196-A72F-4F60-997A-78E19508843D";

        /// <summary>
        /// The Signal Type entity type
        /// </summary>
        public const string SIGNAL_TYPE = "0BA03B9B-E974-4526-9B21-5037424B6D16";

        /// <summary>
        /// The Step Flow entity type
        /// </summary>
        public const string STEP_FLOW = "308D8252-7712-4A45-8DE4-737C3EEAEA8F";

        /// <summary>
        /// The guid for the database storage provider entity
        /// </summary>
        public const string STORAGE_PROVIDER_DATABASE = "0AA42802-04FD-4AEC-B011-FEB127FC85CD";

        /// <summary>
        /// The guid for <see cref="Rock.Model.Streak"/>
        /// </summary>
        public const string STREAK = "D953B0A5-0065-4624-8844-10010DE01E5C";

        /// <summary>
        /// The guid for the system communication entity
        /// </summary>
        public const string SYSTEM_COMMUNICATION = "D0CAD7C0-10FE-41EF-B89D-E6F0D22456C4";

        /// <summary>
        /// The guid for the file-system storage provider entity (Rock.Storage.Provider.FileSystem)
        /// </summary>
        public const string STORAGE_PROVIDER_FILESYSTEM = "A97B6002-454E-4890-B529-B99F8F2F376A";

        /// <summary>
        /// The asset storage 'Amazon S3' component (Rock.Storage.AssetStorage.AmazonS3Component)
        /// </summary>
        public const string STORAGE_ASSETSTORAGE_AMAZONS3 = "FFE9C4A0-7AB7-48CA-8938-EC73DEC134E8";

        /// <summary>
        /// The asset storage 'Azure Cloud Storage' component (Rock.Storage.AssetStorage.AzureCloudStorageComponent)
        /// </summary>
        public const string STORAGE_ASSETSTORAGE_AZURECLOUD = "1576800F-BFD2-4309-A2C9-AE6DF6C0A1A5";

        /// <summary>
        /// The asset storage 'Google Cloud Storage' component (Rock.Storage.AssetStorage.GoogleCloudStorageComponent)
        /// </summary>
        public const string STORAGE_ASSETSTORAGE_GOOGLECLOUD = "71344FA8-4210-4B6C-ADC1-9F63C4CA15CA";

        /// <summary>
        /// The asset storage file-system component (Rock.Storage.AssetStorage.FileSystemComponent)
        /// </summary>
        public const string STORAGE_ASSETSTORAGE_FILESYSTEM = "FFEA94EA-D394-4C1A-A3AE-23E6C50F047A";

        /// <summary>
        /// The EntityType Guid for Tag.
        /// </summary>
        public const string TAG = "d34258d0-d366-4efb-aa76-84b059fb5434";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.Workflow"/>
        /// </summary>
        public const string WORKFLOW = "3540E9A7-FE30-43A9-8B0A-A372B63DFC93";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.WorkflowActivity"/>
        /// </summary>
        public const string WORKFLOW_ACTIVITY = "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.WorkflowType"/>
        /// </summary>
        public const string WORKFLOW_TYPE = "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.WorkflowActionType"/>
        /// </summary>
        public const string WORKFLOW_ACTION_TYPE = "23E3273A-B137-48A3-9AFF-C8DC832DDCA6";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.WorkflowFormBuilderTemplate"/>.
        /// </summary>
        public const string WORKFLOW_FORM_BUILDER_TEMPLATE = "65fa3078-9d42-4857-b78a-f32a05f7a4c1";

        /// <summary>
        /// The guid for the Test Financial Gateway entity type
        /// </summary>
        public const string FINANCIAL_GATEWAY_TEST_GATEWAY = "C22B0247-7C9F-411B-A1F5-0051FCBAC199";

        /// <summary>
        /// The guid for the Step entity type
        /// </summary>
        public const string STEP = "8EADB0DC-17F4-4541-A46E-53F89E21A622";

        /// <summary>
        /// The guid for the Step program entity
        /// </summary>
        public const string STEP_PROGRAM = "E89F9528-A74E-41B7-8B65-B56B4CE7A122";

        /// <summary>
        /// The MyWell financial gateway
        /// </summary>
        public const string MYWELL_FINANCIAL_GATEWAY = "E81ED723-E807-4BDE-ADF1-AB9686241637";

        /// <summary>
        /// The SMS Conversation Action
        /// </summary>
        public const string SMS_ACTION_CONVERSATION = "E808A9FD-06A7-4FB2-AD01-C826A53B0ABB";

        /// <summary>
        /// Rock.Model.Site EntityType guid
        /// </summary>
        public const string SITE = "7244C10B-5D87-467B-A7F5-12DC29910CA8";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Achievement.Component.AccumulativeAchievement"/> 
        /// </summary>
        public const string ACCUMULATIVE_ACHIEVEMENT_COMPONENT = "05D8CD17-E07D-4927-B9C4-5018F7C4B715";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Achievement.Component.StreakAchievement"/> 
        /// </summary>
        public const string STREAK_ACHIEVEMENT_COMPONENT = "174F0AFF-3A5E-4A20-AE8B-D8D83D43BACD";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Achievement.Component.StepProgramAchievement"/> 
        /// </summary>
        public const string STEP_PROGRAM_ACHIEVEMENT_COMPONENT = "7140BAE3-89E9-423E-A691-6E13544203CA";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Achievement.Component.InteractionSourcedAccumulativeAchievement"/> 
        /// </summary>
        public const string INTERACTION_SOURCED_ACHIEVEMENT_COMPONENT = "1F2B13BE-EFAA-4D4E-B2D2-D221B51AEA67";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.MediaFolder"/> 
        /// </summary>
        public const string MEDIA_FOLDER = "B28FC79F-9FEE-4BE4-801D-96B9246E6043";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.PersistedDataset"/> 
        /// </summary>
        public const string PERSISTED_DATASET = "9C3064C0-CF9C-4549-9A80-022514B7FF83";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.PersonalLinkSection"/> 
        /// </summary>
        public const string PERSONAL_LINK_SECTION = "42411FC0-7ACD-42E9-A8A1-BCEA78AF0AAF";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.WorkflowLog"/>
        /// </summary>
        public const string WORKFLOW_LOG = "332CFF36-D637-414D-B732-370B330B8D73";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.Location"/> 
        /// </summary>
        public const string LOCATION = "0D6410AD-C83C-47AC-AF3D-616D09EDF63B";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AnalyticsDimCampus"/> 
        /// </summary>
        public const string ANALYTICS_DIM_CAMPUS = "DCEB0575-1351-4CFF-BA4F-410BA2D638CB";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AnalyticsDimFamilyCurrent"/> 
        /// </summary>
        public const string ANALYTICS_DIM_FAMILY_CURRENT = "B78878C9-4EB7-4EE4-BB85-D00CCA83BCEA";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AnalyticsDimFamilyHeadOfHousehold"/> 
        /// </summary>
        public const string ANALYTICS_DIM_FAMILY_HEAD_OF_HOUSEHOLD = "89730008-FD3F-49BE-9084-6CC5EA4DC4B3";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AnalyticsDimFamilyHistorical"/> 
        /// </summary>
        public const string ANALYTICS_DIM_FAMILY_HISTORICAL = "D906B981-9603-4B5F-9009-31F6EDDE9DC3";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AnalyticsDimFinancialAccount"/> 
        /// </summary>
        public const string ANALYTICS_DIM_FINANCIAL_ACCOUNT = "893F38F8-FBF8-4157-B718-6009298ABC91";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AnalyticsDimFinancialBatch"/> 
        /// </summary>
        public const string ANALYTICS_DIM_FINANCIAL_BATCH = "F970FF85-F3ED-41BD-90D9-5511BACED928";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AnalyticsDimPersonCurrent"/> 
        /// </summary>
        public const string ANALYTICS_DIM_PERSON_CURRENT = "30CBF82B-4C90-4767-9C2D-622308439BF2";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AnalyticsDimPersonHistorical"/> 
        /// </summary>
        public const string ANALYTICS_DIM_PERSON_HISTORICAL = "050AAA2B-43EA-4952-936C-70638D3BCC0D";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AnalyticsFactAttendance"/> 
        /// </summary>
        public const string ANALYTICS_FACT_ATTENDANCE = "3FCC0DF5-4299-4971-860C-8CB591DA75D8";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AnalyticsFactFinancialTransaction"/> 
        /// </summary>
        public const string ANALYTICS_FACT_FINANCIAL_TRANSACTION = "6447497F-C40F-41B9-AB5B-A1F14F80DD18";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AnalyticsSourceAttendance"/> 
        /// </summary>
        public const string ANALYTICS_SOURCE_ATTENDANCE = "BCE52831-6FEF-4521-9E4A-AE5C29F20E2F";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AnalyticsSourceCampus"/> 
        /// </summary>
        public const string ANALYTICS_SOURCE_CAMPUS = "9DE61413-6D38-4F14-AE1B-DB927E07CE56";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AnalyticsSourceFamilyHistorical"/> 
        /// </summary>
        public const string ANALYTICS_SOURCE_FAMILY_HISTORICAL = "C9941E89-EC9D-41FF-A892-5016730F22C1";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AnalyticsSourceFinancialTransaction"/> 
        /// </summary>
        public const string ANALYTICS_SOURCE_FINANCIAL_TRANSACTION = "68E1BB08-B30B-49E2-993E-0B5352BB97C5";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AnalyticsSourceGivingUnit"/> 
        /// </summary>
        public const string ANALYTICS_SOURCE_GIVING_UNIT = "05103BCB-B164-4591-9129-F949A58C04B1";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AnalyticsSourcePersonHistorical"/> 
        /// </summary>
        public const string ANALYTICS_SOURCE_PERSON_HISTORICAL = "FC84E469-7E8F-4202-89C3-F27DD41BC132";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.Assessment"/> 
        /// </summary>
        public const string ASSESSMENT = "6DCD8FF0-4BFD-4AF7-8F4F-E387934775A3";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AssetStorageProvider"/> 
        /// </summary>
        public const string ASSET_STORAGE_PROVIDER = "E0B4BE77-B29F-4BD4-AE45-CF833AC3A482";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AttendanceCheckInSession"/> 
        /// </summary>
        public const string ATTENDANCE_CHECK_IN_SESSION = "D60A20A7-98A2-45B5-BF7E-A327E4090940";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AttendanceCode"/> 
        /// </summary>
        public const string ATTENDANCE_CODE = "D5ADC6D3-9AA0-4AC6-9762-A703EE684934";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AttendanceOccurrence"/> 
        /// </summary>
        public const string ATTENDANCE_OCCURRENCE = "0F6FD7F1-7AF5-4135-843F-E34948D4EA28";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AttributeMatrix"/> 
        /// </summary>
        public const string ATTRIBUTE_MATRIX = "028228F0-B1D9-4DE5-9E6A-F898C34DDAB8";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AttributeMatrixItem"/> 
        /// </summary>
        public const string ATTRIBUTE_MATRIX_ITEM = "3C9D5021-0484-4846-AEF6-B6216D26C3C8";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AttributeMatrixTemplate"/> 
        /// </summary>
        public const string ATTRIBUTE_MATRIX_TEMPLATE = "20B90F35-70C1-4ADC-A908-A4254C15373D";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AttributeQualifier"/> 
        /// </summary>
        public const string ATTRIBUTE_QUALIFIER = "EC7EB9AC-8B52-4A3D-8587-4A08050780CC";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AttributeValue"/> 
        /// </summary>
        public const string ATTRIBUTE_VALUE = "D2BDCCF0-D3F4-4F29-B286-DA5B7BFA41C6";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AttributeValueHistorical"/> 
        /// </summary>
        public const string ATTRIBUTE_VALUE_HISTORICAL = "D940AA57-D977-4B75-B4BE-7C2EB40B26A4";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.Audit"/> 
        /// </summary>
        public const string AUDIT = "5DE389E2-4E25-4BB0-8292-67A94ECB379B";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AuditDetail"/> 
        /// </summary>
        public const string AUDIT_DETAIL = "00A4323F-4A05-4E6A-8480-35D8B7BB6615";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.Auth"/> 
        /// </summary>
        public const string AUTH = "84855F4E-9865-4F1B-B420-939318272004";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AuthAuditLog"/> 
        /// </summary>
        public const string AUTH_AUDIT_LOG = "6AC9997F-B238-4A67-933E-D80E1551704D";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AuthClaim"/> 
        /// </summary>
        public const string AUTH_CLAIM = "A6924EE0-509B-461E-8127-DBF5C4FE30DA";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AuthClient"/> 
        /// </summary>
        public const string AUTH_CLIENT = "CBD66C3A-959A-4A0B-926C-C3ADE43066B1";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AuthScope"/> 
        /// </summary>
        public const string AUTH_SCOPE = "8926FE3B-5AB6-4E48-8191-09EB1682F743";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.BackgroundCheck"/> 
        /// </summary>
        public const string BACKGROUND_CHECK = "6D3A2293-BA7E-44D9-B5B3-5B85DA0EECAC";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.BenevolenceRequestDocument"/> 
        /// </summary>
        public const string BENEVOLENCE_REQUEST_DOCUMENT = "3D627F51-E262-454B-95A0-2EF97103BCE1";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.BenevolenceResult"/> 
        /// </summary>
        public const string BENEVOLENCE_RESULT = "A4929A2D-5B83-4535-A1D4-8A2C84FBA581";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.BenevolenceWorkflow"/> 
        /// </summary>
        public const string BENEVOLENCE_WORKFLOW = "1F27A8E2-C40A-4C8D-9F22-55BC24B98D80";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.BinaryFile"/> 
        /// </summary>
        public const string BINARY_FILE = "9BB1A349-5998-47C1-97D5-D6CC00275662";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.BinaryFileData"/> 
        /// </summary>
        public const string BINARY_FILE_DATA = "EA647542-1E0F-4DAE-9537-65E3FFEE8792";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.BinaryFileType"/> 
        /// </summary>
        public const string BINARY_FILE_TYPE = "62AF597F-F193-412B-94EA-291CF713327D";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.BlockType"/> 
        /// </summary>
        public const string BLOCK_TYPE = "04768EDF-C0CD-4950-B629-4D2370B57C99";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.CampusSchedule"/> 
        /// </summary>
        public const string CAMPUS_SCHEDULE = "134B8879-CD7C-4872-8065-882AA3721C2D";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.CampusTopic"/> 
        /// </summary>
        public const string CAMPUS_TOPIC = "0FFDCB0B-B435-4E66-9085-2750534E706A";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.Category"/> 
        /// </summary>
        public const string CATEGORY = "1D68154E-EC76-44C8-9813-7736B27AECF9";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.Communication"/> 
        /// </summary>
        public const string COMMUNICATION = "C4CCBD91-1264-48BF-BC33-92751C8948B5";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.CommunicationAttachment"/> 
        /// </summary>
        public const string COMMUNICATION_ATTACHMENT = "4271BFB1-AF05-4B6C-8931-032EB02DD760";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.CommunicationRecipient"/> 
        /// </summary>
        public const string COMMUNICATION_RECIPIENT = "3EC89B90-6692-451E-A48F-0D2ADEBA05BC";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.CommunicationResponse"/> 
        /// </summary>
        public const string COMMUNICATION_RESPONSE = "DB449144-6045-4B11-AA55-ECF286B117A9";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.CommunicationResponseAttachment"/> 
        /// </summary>
        public const string COMMUNICATION_RESPONSE_ATTACHMENT = "2F34E79E-F158-4693-85C4-87FF75D3AFE4";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.CommunicationTemplateAttachment"/> 
        /// </summary>
        public const string COMMUNICATION_TEMPLATE_ATTACHMENT = "0DEA0BC5-2AF2-4E06-92CF-DCCD4D3FF011";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.ConnectionOpportunityGroupConfig"/> 
        /// </summary>
        public const string CONNECTION_OPPORTUNITY_GROUP_CONFIG = "59756122-B779-4A4E-9CE7-6A4468AA9524";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.ConnectionStatusAutomation"/> 
        /// </summary>
        public const string CONNECTION_STATUS_AUTOMATION = "ACF794C7-F12D-42DF-85C5-089A28993EE6";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.ContentChannelItemAssociation"/> 
        /// </summary>
        public const string CONTENT_CHANNEL_ITEM_ASSOCIATION = "7C86EED3-C3F9-4B25-887B-F732FE3C35F0";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.ContentChannelItemSlug"/> 
        /// </summary>
        public const string CONTENT_CHANNEL_ITEM_SLUG = "2FF2F1C3-A440-4D24-8FB8-2B0D9436EA75";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.ContentChannelType"/> 
        /// </summary>
        public const string CONTENT_CHANNEL_TYPE = "D5992F79-7FB8-49FF-82AB-E8CB2CEC1E74";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.DataViewFilter"/> 
        /// </summary>
        public const string DATA_VIEW_FILTER = "507E646B-9943-4DD6-8FB7-8BA9F95E6BD0";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.Device"/> 
        /// </summary>
        public const string DEVICE = "C06EE1FE-AF12-410A-A364-7A366CD72414";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.Document"/> 
        /// </summary>
        public const string DOCUMENT = "7CEFA340-9D98-4B2C-B462-313C61944B6C";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.DocumentType"/> 
        /// </summary>
        public const string DOCUMENT_TYPE = "18CF366F-46B6-49CA-B557-BCABD6BBD175";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.EntityCampusFilter"/> 
        /// </summary>
        public const string ENTITY_CAMPUS_FILTER = "A736A9FB-F2A5-4458-B126-FAD6BD3F3B78";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.EntityIntent"/> 
        /// </summary>
        public const string ENTITY_INTENT = "15CE39A3-193F-44E5-80C7-DE47DA3EAF97";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.EntitySet"/> 
        /// </summary>
        public const string ENTITY_SET = "50E3F9C8-4010-41AF-8F61-08308DC44640";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.EntitySetItem"/> 
        /// </summary>
        public const string ENTITY_SET_ITEM = "1F9D13F9-BF55-48BE-BEA9-0939CD6FDA5B";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.EventCalendarContentChannel"/> 
        /// </summary>
        public const string EVENT_CALENDAR_CONTENT_CHANNEL = "B8631058-DAC3-4164-9A50-9E732B0C3882";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.EventCalendarItem"/> 
        /// </summary>
        public const string EVENT_CALENDAR_ITEM = "E37FB26F-03F6-48DA-8E96-F412616F5EE4";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.EventItem"/> 
        /// </summary>
        public const string EVENT_ITEM = "6A58AD11-3491-84AE-4896-8F39906EA65E";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.EventItemAudience"/> 
        /// </summary>
        public const string EVENT_ITEM_AUDIENCE = "22394315-E21B-40AF-AFDA-75D9F5DAD721";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.EventItemOccurrence"/> 
        /// </summary>
        public const string EVENT_ITEM_OCCURRENCE = "71632E1A-1E7F-42B9-A630-EC99F375303A";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.EventItemOccurrenceChannelItem"/> 
        /// </summary>
        public const string EVENT_ITEM_OCCURRENCE_CHANNEL_ITEM = "378A9559-BD86-45A8-B218-2C5D4CF3D770";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.EventItemOccurrenceGroupMap"/> 
        /// </summary>
        public const string EVENT_ITEM_OCCURRENCE_GROUP_MAP = "1479D2B7-65C0-4E98-9E70-0848422FA00C";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.ExceptionLog"/> 
        /// </summary>
        public const string EXCEPTION_LOG = "F61A9F8A-6DA5-49C6-BC8E-5545C5EEDA21";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.FieldType"/> 
        /// </summary>
        public const string FIELD_TYPE = "54018EB6-868C-477D-8B6A-455A6115B30B";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.FinancialGateway"/> 
        /// </summary>
        public const string FINANCIAL_GATEWAY = "122EFE60-84A6-4C7A-A852-30E4BD89A662";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.FinancialPaymentDetail"/> 
        /// </summary>
        public const string FINANCIAL_PAYMENT_DETAIL = "F3659077-43FD-4805-BC42-BC8A3F9C3008";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.FinancialPersonBankAccount"/> 
        /// </summary>
        public const string FINANCIAL_PERSON_BANK_ACCOUNT = "EC1AE861-BDFE-4A08-9741-2E1D2293456F";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.FinancialPersonSavedAccount"/> 
        /// </summary>
        public const string FINANCIAL_PERSON_SAVED_ACCOUNT = "F5244E64-53DB-4707-A398-D248616A776D";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.FinancialPledge"/> 
        /// </summary>
        public const string FINANCIAL_PLEDGE = "CE8060E6-21E7-49F5-BFBE-F632C816C232";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.FinancialScheduledTransactionDetail"/> 
        /// </summary>
        public const string FINANCIAL_SCHEDULED_TRANSACTION_DETAIL = "A206615F-3FB5-48DF-B606-86AE8716FD57";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.FinancialStatementTemplate"/> 
        /// </summary>
        public const string FINANCIAL_STATEMENT_TEMPLATE = "85442202-13BC-46C5-B1E9-43018DEA20F1";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.FinancialTransactionAlert"/> 
        /// </summary>
        public const string FINANCIAL_TRANSACTION_ALERT = "0E60F95E-70B5-4A06-9BEE-80ED9F12F25A";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.FinancialTransactionAlertType"/> 
        /// </summary>
        public const string FINANCIAL_TRANSACTION_ALERT_TYPE = "2E237B04-5B2A-40F1-8CD3-52673C104305";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.FinancialTransactionImage"/> 
        /// </summary>
        public const string FINANCIAL_TRANSACTION_IMAGE = "78DCA7EE-C5FE-49AE-9995-0E254CC8E2A2";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.FinancialTransactionRefund"/> 
        /// </summary>
        public const string FINANCIAL_TRANSACTION_REFUND = "7616E2AF-CEF2-44A5-B27F-3E70EC8B70FA";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.Following"/> 
        /// </summary>
        public const string FOLLOWING = "D9AD7A30-92F2-467B-A3F9-37CA246F90BD";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.FollowingEventNotification"/> 
        /// </summary>
        public const string FOLLOWING_EVENT_NOTIFICATION = "21BA6002-1FB0-45C7-BCA6-D6629EC8AB52";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.FollowingEventSubscription"/> 
        /// </summary>
        public const string FOLLOWING_EVENT_SUBSCRIPTION = "2EEA3DF1-1FBE-472C-85AF-6D952DFC4684";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.FollowingEventType"/> 
        /// </summary>
        public const string FOLLOWING_EVENT_TYPE = "8A0D208B-762D-403A-A972-3A0F079866D4";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.FollowingSuggested"/> 
        /// </summary>
        public const string FOLLOWING_SUGGESTED = "EC14B80A-3776-4134-884D-031A89C0EF03";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.FollowingSuggestionType"/> 
        /// </summary>
        public const string FOLLOWING_SUGGESTION_TYPE = "CC7DF118-86A1-4F90-82D8-0DAE9CD37343";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.GroupDemographicType"/> 
        /// </summary>
        public const string GROUP_DEMOGRAPHIC_TYPE = "9AE7A87B-E274-4FF5-BEFD-55CCF603CE13";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.GroupDemographicValue"/> 
        /// </summary>
        public const string GROUP_DEMOGRAPHIC_VALUE = "C9CED7B0-88BF-40D1-83D1-A58B3C57A2E1";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.GroupHistorical"/> 
        /// </summary>
        public const string GROUP_HISTORICAL = "422A2EF2-9D74-4308-8CDB-D5FA4B6A01FF";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.GroupLocation"/> 
        /// </summary>
        public const string GROUP_LOCATION = "26248EE7-09F3-4578-A1D6-47E01D91D6EF";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.GroupLocationHistorical"/> 
        /// </summary>
        public const string GROUP_LOCATION_HISTORICAL = "03128778-5E7D-4FE4-9C7A-929936E06F90";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.GroupLocationHistoricalSchedule"/> 
        /// </summary>
        public const string GROUP_LOCATION_HISTORICAL_SCHEDULE = "3BC646E4-CA5E-47D6-BC6D-4BBFAAEDAD8B";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.GroupMemberAssignment"/> 
        /// </summary>
        public const string GROUP_MEMBER_ASSIGNMENT = "22BF14ED-E882-4BB0-9328-D12545BF5F61";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.GroupMemberHistorical"/> 
        /// </summary>
        public const string GROUP_MEMBER_HISTORICAL = "233EA15D-8FEE-40FE-9772-D369D34E3A8D";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.GroupMemberRequirement"/> 
        /// </summary>
        public const string GROUP_MEMBER_REQUIREMENT = "FF1B2C4B-0F2D-4D9B-9E85-7336CCC24A62";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.GroupMemberScheduleTemplate"/> 
        /// </summary>
        public const string GROUP_MEMBER_SCHEDULE_TEMPLATE = "D84ED719-B659-433C-BFA0-E798E52C6B24";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.GroupMemberWorkflowTrigger"/> 
        /// </summary>
        public const string GROUP_MEMBER_WORKFLOW_TRIGGER = "3CE3406A-1FFE-4CCA-A8D5-916EEF800D76";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.GroupRequirement"/> 
        /// </summary>
        public const string GROUP_REQUIREMENT = "CFC7DE86-222E-4669-83C2-A3F5B04CB5D6";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.GroupRequirementType"/> 
        /// </summary>
        public const string GROUP_REQUIREMENT_TYPE = "8E67E852-D1BF-485C-9898-09F19998CC40";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.GroupScheduleExclusion"/> 
        /// </summary>
        public const string GROUP_SCHEDULE_EXCLUSION = "047D57EE-1B06-455F-86EA-D96B8325C77D";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.GroupSync"/> 
        /// </summary>
        public const string GROUP_SYNC = "1C011499-1122-4429-9AFA-6578798E18A9";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.GroupType"/> 
        /// </summary>
        public const string GROUP_TYPE = "0DD30B04-01CF-4B38-8E83-BE661E2F7286";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.GroupTypeRole"/> 
        /// </summary>
        public const string GROUP_TYPE_ROLE = "D155C373-9E47-4C6A-BADD-792F31AF5FBA";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.HtmlContent"/> 
        /// </summary>
        public const string HTML_CONTENT = "FB30EC4C-7DCC-41A4-94AB-E728A8CE537B";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.IdentityVerification"/> 
        /// </summary>
        public const string IDENTITY_VERIFICATION = "C76071B0-0C2F-4A3F-88BF-08B2E006C614";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.IdentityVerificationCode"/> 
        /// </summary>
        public const string IDENTITY_VERIFICATION_CODE = "3FCB8972-C319-4262-9D6E-3D60E1C4E463";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.InteractionChannel"/> 
        /// </summary>
        public const string INTERACTION_CHANNEL = "08606092-5FF5-4A34-A7A6-3DEE43F2843A";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.InteractionComponent"/> 
        /// </summary>
        public const string INTERACTION_COMPONENT = "ACE6145B-57D6-4694-972F-EC43AF776DE7";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.InteractionDeviceType"/> 
        /// </summary>
        public const string INTERACTION_DEVICE_TYPE = "C1A24534-C77F-41A8-BCA9-73ABA57348E3";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.InteractionSession"/> 
        /// </summary>
        public const string INTERACTION_SESSION = "338025DE-C16F-47BB-BA31-6DE0C59E59AA";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.InteractionSessionLocation"/> 
        /// </summary>
        public const string INTERACTION_SESSION_LOCATION = "790EC7AC-7443-466C-A07E-F702D86B9E1B";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.MediaElement"/> 
        /// </summary>
        public const string MEDIA_ELEMENT = "F4506B5D-F22C-4D3F-8205-FE48A9B7584B";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.MetaFirstNameGenderLookup"/> 
        /// </summary>
        public const string META_FIRST_NAME_GENDER_LOOKUP = "FA96E086-811E-4FE0-BECA-F7A593F9FB05";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.MetaLastNameLookup"/> 
        /// </summary>
        public const string META_LAST_NAME_LOOKUP = "0FB0F456-6999-426D-9C86-973D54749565";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.MetaNickNameLookup"/> 
        /// </summary>
        public const string META_NICK_NAME_LOOKUP = "5FCBE899-3756-4F64-8540-CE37EA9EED45";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.Metric"/> 
        /// </summary>
        public const string METRIC = "1C5489C6-82F9-4967-8425-52545CE8AF90";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.MetricPartition"/> 
        /// </summary>
        public const string METRIC_PARTITION = "82ED733F-BAD8-4D8D-BCB7-10A6433F452A";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.MetricValue"/> 
        /// </summary>
        public const string METRIC_VALUE = "DD0E6F39-3E07-44D0-BE7B-B1AB75AFED2D";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.MetricValuePartition"/> 
        /// </summary>
        public const string METRIC_VALUE_PARTITION = "FEC12664-C5F2-44E1-9805-1137068AC755";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.NcoaHistory"/> 
        /// </summary>
        public const string NCOA_HISTORY = "1F20AC90-C57E-4DD1-A71B-06312110E56F";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.NoteAttachment"/> 
        /// </summary>
        public const string NOTE_ATTACHMENT = "D090C50E-2FE1-4284-9631-19D06F4AD8B0";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.NoteType"/> 
        /// </summary>
        public const string NOTE_TYPE = "337EED57-D4AB-4EED-BBDB-0CB3A467DBCC";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.NoteWatch"/> 
        /// </summary>
        public const string NOTE_WATCH = "A5C129C2-E64D-4B72-B94D-DBA6DA6AC2E3";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.Notification"/> 
        /// </summary>
        public const string NOTIFICATION = "6DB6D280-9740-41FD-B8BD-0AA29E12F4B6";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.NotificationRecipient"/> 
        /// </summary>
        public const string NOTIFICATION_RECIPIENT = "2ECE2776-0FE2-429D-B655-AB56A2D6EE0B";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.PageContext"/> 
        /// </summary>
        public const string PAGE_CONTEXT = "5C56B915-8A8B-4747-9D84-EBAF0BACC9A1";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.PageRoute"/> 
        /// </summary>
        public const string PAGE_ROUTE = "42C14361-67B2-472C-95BE-EA8A9C511837";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.PageShortLink"/> 
        /// </summary>
        public const string PAGE_SHORT_LINK = "83D8C6DF-1D53-438B-93B2-75A2038BBEE6";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.PersonalDevice"/> 
        /// </summary>
        public const string PERSONAL_DEVICE = "E9CD3369-E087-4809-9952-F2DCD6B8816B";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.PersonalizationSegment"/> 
        /// </summary>
        public const string PERSONALIZATION_SEGMENT = "368A3581-C8C4-4960-901A-9587864226F3";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.PersonalLink"/> 
        /// </summary>
        public const string PERSONAL_LINK = "F858CF72-ECCC-4DC6-AD72-7B82467B3466";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.PersonalLinkSectionOrder"/> 
        /// </summary>
        public const string PERSONAL_LINK_SECTION_ORDER = "648325DC-1555-44B7-AFA4-1971E57F7E11";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.PersonDuplicate"/> 
        /// </summary>
        public const string PERSON_DUPLICATE = "20B2B2B6-38C3-4302-9200-63DD4C78687B";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.PersonPreviousName"/> 
        /// </summary>
        public const string PERSON_PREVIOUS_NAME = "80AC80E1-F769-4E34-9937-E2FEEF2B60EE";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.PersonScheduleExclusion"/> 
        /// </summary>
        public const string PERSON_SCHEDULE_EXCLUSION = "07204F06-C09C-4B37-921A-C31C042938B9";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.PersonSearchKey"/> 
        /// </summary>
        public const string PERSON_SEARCH_KEY = "914FE998-4F61-4005-ACCC-A8D0433CAD47";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.PersonToken"/> 
        /// </summary>
        public const string PERSON_TOKEN = "846B2BCE-7101-46B9-B89C-CD68073712CE";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.PersonViewed"/> 
        /// </summary>
        public const string PERSON_VIEWED = "AF13DF44-4EE7-4492-AEE4-6BD2A62F9C76";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.PhoneNumber"/> 
        /// </summary>
        public const string PHONE_NUMBER = "AD7E27CB-036D-40C6-B352-81B38BFAE798";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.PluginMigration"/> 
        /// </summary>
        public const string PLUGIN_MIGRATION = "F239557E-C7A8-4D1F-82CC-55CDD0ACA3C8";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.PrayerRequest"/> 
        /// </summary>
        public const string PRAYER_REQUEST = "F13C8FD2-7702-4C79-A6A9-86440DD5DE13";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.RegistrationInstance"/> 
        /// </summary>
        public const string REGISTRATION_INSTANCE = "5CD9C0C8-C047-61A0-4E36-0FDB8496F066";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.RegistrationRegistrant"/> 
        /// </summary>
        public const string REGISTRATION_REGISTRANT = "8A25E5CE-1B4F-4825-BCEA-216167836305";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.RegistrationRegistrantFee"/> 
        /// </summary>
        public const string REGISTRATION_REGISTRANT_FEE = "AB66F63A-3E79-438F-8C52-C9A8C70A0511";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.RegistrationSession"/> 
        /// </summary>
        public const string REGISTRATION_SESSION = "6846744B-2623-4EF3-A20F-8027C4839094";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.RegistrationTemplateDiscount"/> 
        /// </summary>
        public const string REGISTRATION_TEMPLATE_DISCOUNT = "88D94ECB-FCEE-4A00-ACB9-FF90BDBA7A17";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.RegistrationTemplateFee"/> 
        /// </summary>
        public const string REGISTRATION_TEMPLATE_FEE = "2DB3A441-6CA1-49D1-BB25-C744E2FFA457";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.RegistrationTemplateFeeItem"/> 
        /// </summary>
        public const string REGISTRATION_TEMPLATE_FEE_ITEM = "CC1DB3FF-A145-49CE-ADC0-B8960EBF37D4";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.RegistrationTemplateForm"/> 
        /// </summary>
        public const string REGISTRATION_TEMPLATE_FORM = "2F0B3A6A-4E47-45A8-A331-7234CE711356";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.RegistrationTemplateFormField"/> 
        /// </summary>
        public const string REGISTRATION_TEMPLATE_FORM_FIELD = "A773CAA2-2211-416B-BDD7-D907085B4441";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.RegistrationTemplatePlacement"/> 
        /// </summary>
        public const string REGISTRATION_TEMPLATE_PLACEMENT = "CCE05820-5854-47A4-ACE3-05DF48479939";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.RelatedEntity"/> 
        /// </summary>
        public const string RELATED_ENTITY = "BD29E403-BA47-4688-BE29-45A38CE8BD03";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.RemoteAuthenticationSession"/> 
        /// </summary>
        public const string REMOTE_AUTHENTICATION_SESSION = "2FDEE857-08BE-47F3-8E86-B2027F545EE8";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.Report"/> 
        /// </summary>
        public const string REPORT = "F1F22D3E-FEFA-4C84-9FFA-9E8ACE60FCE7";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.ReportField"/> 
        /// </summary>
        public const string REPORT_FIELD = "6B541BAA-44B7-48BA-937A-543866905689";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.RequestFilter"/> 
        /// </summary>
        public const string REQUEST_FILTER = "97FAC672-37A4-4185-B1D4-C68426C625B1";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.RestAction"/> 
        /// </summary>
        public const string REST_ACTION = "D4F7F055-5351-4ADF-9F8D-4802CAD6CC9D";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.RestController"/> 
        /// </summary>
        public const string REST_CONTROLLER = "65CDFD5B-A9AA-48FA-8D22-669612D5EA7D";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.ScheduleCategoryExclusion"/> 
        /// </summary>
        public const string SCHEDULE_CATEGORY_EXCLUSION = "E04681EB-7A85-441B-B794-82B025FFB5D4";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.ServiceJobHistory"/> 
        /// </summary>
        public const string SERVICE_JOB_HISTORY = "D6A7C6E0-004F-4F38-9DCA-16E645F5EDF4";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.ServiceLog"/> 
        /// </summary>
        public const string SERVICE_LOG = "B5C26A04-E374-495A-AEC6-171C2D2CBD60";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.SignatureDocument"/> 
        /// </summary>
        public const string SIGNATURE_DOCUMENT = "C1724719-1C03-4D0C-8A66-E3545138F57F";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.SignatureDocumentTemplate"/> 
        /// </summary>
        public const string SIGNATURE_DOCUMENT_TEMPLATE = "3F9828CC-8224-4AB0-98A5-6D60001EBE32";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.SiteDomain"/> 
        /// </summary>
        public const string SITE_DOMAIN = "4C4CD7DD-427C-45EC-9651-F8250D3CAB5F";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.SmsAction"/> 
        /// </summary>
        public const string SMS_ACTION = "1F5E26BE-0ED4-4250-8FFC-1DED5E9EACF0";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.SmsPipeline"/> 
        /// </summary>
        public const string SMS_PIPELINE = "64DA3A06-FD39-4E5B-8126-38404FB0092A";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.StepProgramCompletion"/> 
        /// </summary>
        public const string STEP_PROGRAM_COMPLETION = "B7A9C37D-2B04-4FD3-91BD-DFCA50B3CC8C";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.StepStatus"/> 
        /// </summary>
        public const string STEP_STATUS = "6C270D6A-F126-445B-93F0-5079A968BF4E";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.StepType"/> 
        /// </summary>
        public const string STEP_TYPE = "5E795620-9F16-49D2-9030-947C0E348A8E";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.StepTypePrerequisite"/> 
        /// </summary>
        public const string STEP_TYPE_PREREQUISITE = "F2181FCD-1423-4937-9137-099154E1C3EC";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.StepWorkflow"/> 
        /// </summary>
        public const string STEP_WORKFLOW = "9E164DCB-2B3C-49DB-A3DA-E25E24BB23B9";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.StepWorkflowTrigger"/> 
        /// </summary>
        public const string STEP_WORKFLOW_TRIGGER = "55C1D610-D42F-4E08-9CD9-1EC9801BC4E3";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.StreakType"/> 
        /// </summary>
        public const string STREAK_TYPE = "66203975-2A7A-4000-870E-76457DF3C920";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.StreakTypeExclusion"/> 
        /// </summary>
        public const string STREAK_TYPE_EXCLUSION = "1F00C782-F8A2-4CFA-B7DF-E5B3B6D36069";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.SystemEmail"/> 
        /// </summary>
        public const string SYSTEM_EMAIL = "B21FD119-893E-46C0-B42D-E4CDD5C8C49D";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.TaggedItem"/> 
        /// </summary>
        public const string TAGGED_ITEM = "AB97403A-206E-4E0C-AC42-856A010FA6DD";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.Theme"/> 
        /// </summary>
        public const string THEME = "D02B63B9-E945-45BC-9D41-2C3E72B85F46";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.UserLogin"/> 
        /// </summary>
        public const string USER_LOGIN = "0FA592F1-728C-4885-BE38-60ED6C0D834F";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.WebFarmNode"/> 
        /// </summary>
        public const string WEB_FARM_NODE = "2338D5C3-E808-408F-B000-E8A7D8A4858C";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.WebFarmNodeLog"/> 
        /// </summary>
        public const string WEB_FARM_NODE_LOG = "E00B4559-8E53-4B21-8B29-DB2E6DD74C50";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.WebFarmNodeMetric"/> 
        /// </summary>
        public const string WEB_FARM_NODE_METRIC = "3194A5E3-F352-4CD0-A585-F1DD37008B9C";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.WorkflowAction"/> 
        /// </summary>
        public const string WORKFLOW_ACTION = "9CBF4BEC-5653-47F9-8E87-0D31C6CA5947";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.WorkflowActionForm"/> 
        /// </summary>
        public const string WORKFLOW_ACTION_FORM = "FDAB9AEB-B2AA-4FB5-A35D-83254A9B014C";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.WorkflowActionFormAttribute"/> 
        /// </summary>
        public const string WORKFLOW_ACTION_FORM_ATTRIBUTE = "E147611F-D1AB-4C34-A1F8-84A118BAFDE3";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.WorkflowActionFormSection"/> 
        /// </summary>
        public const string WORKFLOW_ACTION_FORM_SECTION = "90AF7254-87A2-42CB-B2F6-D4D53D7E30A0";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.WorkflowActivityType"/> 
        /// </summary>
        public const string WORKFLOW_ACTIVITY_TYPE = "E5FBDBA2-9539-4679-B948-2E06C1BB1E3F";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.WorkflowTrigger"/> 
        /// </summary>
        public const string WORKFLOW_TRIGGER = "3781C82A-7F40-4D88-B3DB-1B9589D73D3D";

        /// <summary>
        /// The Test Redirection Gateway entity type
        /// </summary>
        public const string FINANCIAL_GATEWAY_TEST_REDIRECTION_GATEWAY = "AB3C5BF6-4D99-4289-84AF-3EF798849705";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.Snippet"/>
        /// </summary>
        public const string SNIPPET = "93548852-201B-4EF6-AF27-BBF535A2CC2B";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.SnippetType"/> 
        /// </summary>
        public const string SNIPPET_TYPE = "FD4C72DE-6B5D-4EB5-9438-385E2E15AF05";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.SystemPhoneNumber"/> 
        /// </summary>
        public const string SYSTEM_PHONE_NUMBER = "66D62A9F-13CD-4160-8653-211B2A4ABF16";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.NotificationMessage"/> 
        /// </summary>
        public const string NOTIFICATION_MESSAGE = "239ADD2E-2DBF-46A7-BD28-4A2A201D4E7B";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.NotificationMessageType"/> 
        /// </summary>
        public const string NOTIFICATION_MESSAGE_TYPE = "36FB1038-8836-429F-BAD4-04D32892D6D0";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Web.HttpModules.Observability"/> 
        /// </summary>
        public const string HTTP_MODULE_OBSERVABILITY = "FE7A8295-9383-4FD8-9FB2-FF77A8042462";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AdaptiveMessage"/> 
        /// </summary>
        public const string ADAPTIVE_MESSAGE = "63D98F58-DA81-46AE-AE0C-662A7BFAA7D0";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AdaptiveMessageAdaptation"/> 
        /// </summary>
        public const string ADAPTIVE_MESSAGE_ADAPTATION = "39753CCE-184A-4F14-AE80-08241DE8FC2E";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AdaptiveMessageAdaptationSegment"/> 
        /// </summary>
        public const string ADAPTIVE_MESSAGE_ADAPTATION_SEGMENT = "40EDF82B-EFF5-4253-A129-965F7BC90033";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AdaptiveMessageCategory"/> 
        /// </summary>
        public const string ADAPTIVE_MESSAGE_CATEGORY = "D47BDA25-03A3-46EE-A0A6-F8B220E39E4A";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.CheckInLabel"/> 
        /// </summary>
        public const string CHECK_IN_LABEL = "8B651EB1-492F-46D0-821B-CA7355C6E6E7";

        /// <summary>
        /// The EntityType Guid for <see cref="Rock.Model.AIProvider"/> 
        /// </summary>
        public const string AI_PROVIDER = "945A994F-F15E-43AC-B503-A54BDE70F77F";
    }
}
