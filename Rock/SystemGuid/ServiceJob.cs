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
    /// Service Job guids
    /// </summary>
    public static class ServiceJob
    {
        /// <summary>
        /// Gets the Job Pulse guid
        /// </summary>
        public const string JOB_PULSE = "CB24FF2A-5AD3-4976-883F-DAF4EFC1D7C7";

        /// <summary>
        /// The Job to run Post v12 to update interaction indexes.
        /// </summary>
        public const string DATA_MIGRATIONS_120_UPDATE_INTERACTION_INDEXES = "090CB437-F74B-49B0-8B51-BF2A491DD36D";

        /// <summary>
        /// The data migrations 120 add communication recipient index
        /// </summary>
        public const string DATA_MIGRATIONS_120_ADD_COMMUNICATIONRECIPIENT_INDEX = "AD7CAEAC-6C84-4B55-9E5A-FEE085C270E4";

        /// <summary>
        /// The data migrations 120 add communication get queued index
        /// </summary>
        public const string DATA_MIGRATIONS_120_ADD_COMMUNICATION_GET_QUEUED_INDEX = "BF3AADCC-B2A5-4EB9-A365-08C3F052A551";

        /// <summary>
        /// The Job to run Post v12.2 Data Migrations for adding PersonalDeviceId to Interaction index
        /// </summary>
        public const string DATA_MIGRATIONS_122_INTERACTION_PERSONAL_DEVICE_ID = "6BEDCC6F-620B-4DE0-AE9F-F6DB0E0153E4";

        /// <summary>
        /// The Job to run Post v12.4 Data Migrations for Update Group Salutation fields on Rock.Model.Group.
        /// </summary>
        public const string DATA_MIGRATIONS_124_UPDATE_GROUP_SALUTATIONS = "584F899B-B974-4847-9473-15099AADD577";

        /// <summary>
        /// The Job to run Post v12.5 Data Migrations for Update Step Program Completion
        /// </summary>
        public const string DATA_MIGRATIONS_125_UPDATE_STEP_PROGRAM_COMPLETION = "E7C54AAB-451E-4E89-8083-CF398D37416E";

        /// <summary>
        /// The Job to run Post v12.5 Data Migrations for Add SystemCommunicationId index to Communication
        /// </summary>
        public const string DATA_MIGRATIONS_125_ADD_COMMUNICATION_SYSTEM_COMMUNICATION_ID_INDEX = "DA54E879-44CE-433C-A472-54B57B11CB7B";

        /// <summary>
        /// The Job to run Post v12.7 Data Migrations for Rebuild Group Salutation fields on Rock.Model.Group.
        /// </summary>
        public const string DATA_MIGRATIONS_127_REBUILD_GROUP_SALUTATIONS = "FD32833A-6FC8-43E6-8D36-0C840DBE99F8";

        /// <summary>
        /// The Job to run Post v13.0 Data Migrations for Add InteractionComponentId index to Interaction
        /// </summary>
        public const string DATA_MIGRATIONS_130_ADD_INTERACTION_INTERACTION_COMPONENT_ID_INDEX = "1D7FADEC-2A8A-46FD-898E-58544E7FD9F2";

        /// <summary>
        /// The Job to run Post v13.3 Data Migrations for Adding InteractionSessionLocationId index to Interaction Session
        /// </summary>
        public const string DATA_MIGRATIONS_133_ADD_INTERACTION_SESSION_INTERACTION_SESSION_LOCATION_ID_INDEX = "219BF98C-C10C-4B19-86DB-C69D9B8705FC";

        /// <summary>
        /// The Job to run the Post v13.6 Data Migration to fix the eRA Start Date issue (#5072)
        /// </summary>
        public const string DATA_MIGRATIONS_136_FIX_INCORRECT_ERA_START_DATE = "C02ADF2E-A5C3-484F-9C7B-666AB7C5B333";

        /// <summary>
        /// The Job to Migrate pre-v8.0 History Summary Data
        /// </summary>
        public const string MIGRATE_HISTORY_SUMMARY_DATA = "CF2221CC-1E0A-422B-B0F7-5D81AF1DDB14";

        /// <summary>
        /// The Job to run Post v14.0 Data Migrations for Add missing Media Element interactions
        /// </summary>
        public const string DATA_MIGRATIONS_140_ADD_MISSING_MEDIA_ELEMENT_INTERACTIONS = "3E6817DA-CEE0-42F8-A30E-FF787719493C";

        /// <summary>
        /// The Job to run Post v14.0 Data Migrations to update current sessions
        /// </summary>
        public const string DATA_MIGRATIONS_140_UPDATE_CURRENT_SESSIONS = "53A6804F-5895-4E19-907D-916B5CF175AB";

        /// <summary>
        /// The Job to run Post v14.1 Data Migrations to update current sessions that might have 1900-01-01 set as the DurationLastCalculatedDateTime
        /// </summary>
        public const string DATA_MIGRATIONS_141_UPDATE_CURRENT_SESSIONS_1900 = "12925E67-1E4F-47E5-BB5E-DD411909F90E";

        /// <summary>
        /// The Job to run Post v14.1 Data Migrations to add some missing indexes
        /// </summary>
        public const string DATA_MIGRATIONS_141_ADD_MISSING_INDEXES = "B1970CD1-1DDD-46FC-B259-6D151D53374D";

        /// <summary>
        /// The Job to run Post v14.1 Data Migrations to update the ValueAs___ columns after migration.
        /// </summary>
        public const string DATA_MIGRATIONS_141_UPDATE_VALUEAS_ATTRIBUTE_VALUE_COLUMNS = "f7786b0a-e80a-4f19-b0c4-d4f85f4affa2";

        /// <summary>
        /// The Job to run Post v14.1 Data Migrations for Update AttributeValues of type SlidingDateRangeFieldType to RoundTrip format.
        /// </summary>
        public const string DATA_MIGRATIONS_141_UPDATE_SLIDING_DATE_RANGE_VALUE = "59D32B1D-5D9A-4B1E-836A-43BBE89BA004";

        /// <summary>
        /// The Job to run Post v14.1 Data Migrations to update current sessions that might have 1900-01-01 set as the DurationLastCalculatedDateTime
        /// </summary>
        public const string DATA_MIGRATIONS_141_RECREATE_METRIC_ANALYTICS_VIEWS = "8AF951F3-742B-433C-B7C0-BDF71B9A78FC";

        /// <summary>
        /// The Job to run Post v14.1 Data Migrations to replace all existing instances of the TransactionEntryBlock with a new instance of the UtilityPaymentEntry block.
        /// </summary>
        public const string DATA_MIGRATIONS_150_REPLACE_TRANSACTION_ENTRY_BLOCKS_WITH_UTILITY_PAYMENT_ENTRY_BLOCK = "8A013CC5-CB51-48F3-8AF8-767BDECACAFE";

        /// <summary>
        /// The Job to run Post v15.1 Data Migrations to cleanup duplicate mobile interaction entries.
        /// </summary>
        public const string DATA_MIGRATIONS_151_DUPLICATE_MOBILE_INTERACTIONS_CLEANUP = "D3D60B90-48D1-4718-905E-39638B44C665";

        /// <summary>
        /// The Job to run Post v15.0 Data Migrations to add a new mobile rest group and add existing mobile applications into that group.
        /// </summary>
        public const string DATA_MIGRATIONS_150_MOBILE_APPLICATION_USERS_REST_GROUP = "480E996E-6A31-40DB-AE98-BFF85CDED506";

        /// <summary>
        /// The Job to run Post v140 to add FK indexes on RegistrationRegistrant.RegistrationTemplateId, GroupMember.GroupTypeId, and ConnectionRequest.ConnectionTypeId.
        /// </summary>
        public const string DATA_MIGRATIONS_140_CREATE_FK_INDEXES = "D96BD1F7-6A4A-4DC0-B10D-40031F709573";

        /// <summary>
        /// The Job to run Post v15.1 Data Migrations for System Phone Numbers.
        /// </summary>
        public const string DATA_MIGRATIONS_150_SYSTEM_PHONE_NUMBERS = "6DFE731E-F28B-40B3-8383-84212A301214";

        /// <summary>
        /// The Job to run Post v15.2 Data Migrations for the AttributeValue.IX_ValueAsPersonId index.
        /// </summary>
        public const string DATA_MIGRATIONS_152_IX_VALUE_AS_PERSON_ID = "5DC19FB3-AB0B-48F3-817D-9023C65C5F8A";

        /// <summary>
        /// The Job to run Post v15.1 Replace Web Forms Blocks with Obsidian Blocks.
        /// </summary>
        public const string DATA_MIGRATIONS_150_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS = "EA00D1D4-709A-4102-863D-08471AA2C345";

        /// <summary>
        /// The Job to run Post v15.2 to replace web forms blocks with Obsidian blocks.
        /// </summary>
        public const string DATA_MIGRATIONS_152_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS = "4232194C-90AE-4B44-93E7-1E5DE984A9E1";

        /// <summary>
        /// The Job to run Post v15.4 to update the AgeBracket values to reflect the new values after spliting the 0 - 12 bracket.
        /// </summary>
        public const string DATA_MIGRATIONS_154_UPDATE_AGE_BRACKET_VALUES = "C1234A63-09A6-45C1-96D8-0DE03EC4A7A1";

        /// <summary>
        /// The Job to run Post v16.0 Move Person Preferences.
        /// </summary>
        public const string DATA_MIGRATIONS_160_MOVE_PERSON_PREFERENCES = "C8591D15-9D37-49D3-8DF8-1DB72EE42D29";

        /// <summary>
        /// The Job to run Post v16.0 Update InteractionSession SessionStartDateKey.
        /// </summary>
        public const string DATA_MIGRATIONS_160_UPDATE_INTERACTION_SESSION_SESSION_START_DATE_KEY = "EBAD6B4D-D928-41FD-A0DD-445060810504";

        /// <summary>
        /// The Job to run Post v16.0 Update InteractionSession InteractionChannelId.
        /// </summary>
        public const string DATA_MIGRATIONS_160_UPDATE_INTERACTION_SESSION_INTERACTION_CHANNEL_ID = "3BC5124D-0ED1-4D90-A9ED-D858FA4B5051";

        /// <summary>
        /// The Job to run v16.0 - Add New Indices To Interaction and InteractionSession.
        /// </summary>
        public const string DATA_MIGRATIONS_160_UPDATE_INTERACTION_SESSION_AND_INTERACTION_INDICES = "30A8FE3D-8C2B-413E-9B94-F4B9212904B1";

        /// <summary>
        /// The Job to run v16.0 - Add New Indices To Interaction and InteractionSession.
        /// </summary>
        public const string DATA_MIGRATIONS_160_POPULATE_INTERACTION_SESSION_DATA = "4C734B0E-8024-4600-99F9-B6CFEE9F8250";

        /// <summary>
        /// The Job to run Post v16.0 Update Person PrimaryPersonAliasId.
        /// </summary>
        public const string DATA_MIGRATIONS_160_UPDATE_PERSON_PRIMARY_PERSON_ALIAS_ID = "BC7564DC-594F-4B50-8988-1594849515F1";

        /// <summary>
        /// The Job to run Post v12.4 Data Migrations to decrypt the expiration month / year and the name on card fields.
        /// </summary>
        public const string DATA_MIGRATIONS_124_DECRYPT_FINANCIAL_PAYMENT_DETAILS = "6C795E61-9DD4-4BE8-B9EB-E662E43B5E12";

        /// <summary>
        /// The Job to run Post v16.0 Data Migrations to update the newly persisted WorkflowId column on Workflow entity with their correct values.
        /// </summary>
        public const string DATA_MIGRATIONS_160_UPDATE_WORKFLOWID_COLUMNS = "2222F9D2-4771-4B21-A630-E696DB0C329A";

        /// <summary>
        /// The Job to run Post v16.0 Data Migrations to update the note data to match the new formatting.
        /// </summary>
        public const string DATA_MIGRATIONS_160_UPDATE_NOTE_DATA = "3768889a-ba73-4cff-91f9-cc0f92780745";

        /// <summary>
        /// The Job to run Post v16.0 Data Migrations to update the media element default urls.
        /// </summary>
        public const string DATA_MIGRATIONS_160_UPDATE_MEDIA_ELEMENT_DEFAULT_URLS = "3f2a18ce-882d-4687-a4e4-b2a34af2777d";

        /// <summary>
        /// The Job to run Post v16.1 Data Migrations to swap Financial Batch List web forms block with obsidian block.
        /// </summary>
        public const string DATA_MIGRATIONS_161_SWAP_FINANCIAL_BATCH_LIST = "7750ECFD-26E3-49DE-8E90-1B1A6DCCC3FE";

        /// <summary>
        /// The Job to run Post v16.1 Data Migrations to swap AccountEntry and Login web forms blocks with obisdian blocks.
        /// </summary>
        public const string DATA_MIGRATIONS_161_CHOP_ACCOUNTENTRY_AND_LOGIN = "A65D26C1-229E-4198-B388-E269C3534BC0";

        /// <summary>
        /// The Job to get NCOA
        /// </summary>
        public const string GET_NCOA = "D2D6EA6C-F94A-39A0-481B-A23D08B887D6";

        /// <summary>
        /// The Job to Rebuild a Sequence. This job has been deleted and replaced with
        /// <see cref="Rock.Transactions.StreakTypeRebuildTransaction" />
        /// </summary>
        public const string REBUILD_STREAK = "BFBB9524-10E8-42CF-BCD3-0CC7D2B22C3A";

        /// <summary>
        /// The rock cleanup Job. <see cref="Rock.Jobs.RockCleanup"/>
        /// </summary>
        public const string ROCK_CLEANUP = "1A8238B1-038A-4295-9FDE-C6D93002A5D7";

        /// <summary>
        /// The steps automation job - add steps based on people in a dataview
        /// </summary>
        public const string STEPS_AUTOMATION = "97858941-0447-49D6-9E35-B03665FEE965";

        /// <summary>
        /// The collect hosting metrics job - collect metrics regarding database connections, Etc.
        /// </summary>
        public const string COLLECT_HOSTING_METRICS = "36FA38CA-9DB0-40A8-BABD-5411121B4809";

        /// <summary>
        /// The Job to send an email digest with an attendance summary of all child groups to regional group leaders
        /// </summary>
        public const string SEND_GROUP_ATTENDANCE_DIGEST = "9F9E9C3B-FC58-4939-A272-4FA86D44CE7B";

        /// <summary>
        /// A run once job after a new installation. The purpose of this job is to populate generated datasets after an initial installation using RockInstaller that are too large to include in the installer.
        /// </summary>
        public const string POST_INSTALL_DATA_MIGRATIONS = "322984F1-A7A0-4D1B-AE6F-D7F043F66EB3";

        /// <summary>
        /// The <seealso cref="Rock.Jobs.GivingAutomation"/> job.
        /// </summary>
        public const string GIVING_AUTOMATION = "B6DE0544-8C91-444E-B911-453D4CE71515";

        /// <summary>
        /// Use <see cref="GIVING_AUTOMATION" /> instead
        /// </summary>
        [Obsolete( "Use GIVING_AUTOMATION instead" )]
        [RockObsolete( "1.13" )]
        public const string GIVING_ANALYTICS = GIVING_AUTOMATION;

        /// <summary>
        /// The <see cref="Rock.Jobs.SyncMedia">media synchronize</see> job.
        /// </summary>
        public const string SYNC_MEDIA = "FB27C6DF-F8DB-41F8-83AF-BBE09E77A0A9";

        /// <summary>
        /// The Process Elevated Security Job. <see cref="Rock.Jobs.ProcessElevatedSecurity"/>
        /// </summary>
        public const string PROCESS_ELEVATED_SECURITY = "A1AF9D7D-E968-4AF6-B203-6BB4FD625714";

        /// <summary>
        /// The <see cref="Rock.Jobs.UpdatePersonalizationData" /> job.
        /// </summary>
        public const string UPDATE_PERSONALIZATION_DATA = "67CFE1FE-7C64-4328-8576-F1A4BFD0EA8B";

        /// <summary>
        /// The <see cref="Rock.Jobs.ProcessReminders"/> job.
        /// </summary>
        public const string PROCESS_REMINDERS = "3F697C80-4C33-4552-9038-D3470445EA40";

        /// <summary>
        /// The <see cref="Rock.Jobs.UpdatePersistedAttributeValues">Update Persisted Attribute Values</see> job.
        /// </summary>
        public const string UPDATE_PERSISTED_ATTRIBUTE_VALUE = "A7DDA4B0-BA1D-49F1-8749-5E7A9876AE70";

        /// <summary>
        /// The <see cref="Rock.Jobs.UpdateAnalyticsSourceZipCode" /> job.
        /// </summary>
        public const string UPDATE_ANALYTICS_SOURCE_ZIPCODE = "29731D97-699D-4D34-A9F4-50C7C33D5C48";

        /// <summary>
        /// The Post Update Data Migration Job to swap the Notes Block
        /// </summary>
        public const string DATA_MIGRATIONS_SWAP_NOTES_BLOCK = "8390C1AC-88D6-474A-AC05-8FFBD358F75D";

        /// <summary>
        /// The Post Update Data Migration Job to chop the Schedule Detail, Asset Storage Provider Detail, Page Short Link Detail, Streak Type Detail,
        /// Following Event Type Detail, Financial Batch Detail
        /// </summary>
        public const string DATA_MIGRATIONS_CHOP_BLOCKS_GROUP_1 = "54FACAE5-2175-4FE0-AC9F-5CDA957BCFB5";

        /// <summary>
        /// The post update data migration job to chop the Group Registration block
        /// </summary>
        public const string DATA_MIGRATIONS_160_CHOP_BLOCKS_GROUP_REGISTRATION = "72D9EC04-517A-4CA0-B631-9F9A41F1790D";

        /// <summary>
        /// The post update data migration job to swap the Group Schedule Toolbox V1.
        /// </summary>
        public const string DATA_MIGRATIONS_161_SWAP_BLOCK_GROUP_SCHEDULE_TOOLBOX_V1 = "22DBD648-79C0-40C7-B561-094E4E7637E5";

        /// <summary>
        /// The post update data migration job to chop the Group Schedule Toolbox V2.
        /// </summary>
        public const string DATA_MIGRATIONS_161_CHOP_BLOCK_GROUP_SCHEDULE_TOOLBOX_V2 = "7F989E9F-913C-45E4-9EB1-EC70AC220939";

        /// <summary>
        /// The post update data migration job to remove obsidian group schedule toolbox back buttons.
        /// </summary>
        public const string DATA_MIGRATIONS_161_REMOVE_OBSIDIAN_GROUP_SCHEDULE_TOOLBOX_BACK_BUTTONS = "781F2D3B-E5E4-41D5-9145-1D70DDB3EE04";

        /// <summary>
        /// The post update data migration job to chop the Login and Account Entry blocks.
        /// </summary>
        public const string DATA_MIGRATIONS_161_CHOP_SECURITY_BLOCKS = "A65D26C1-229E-4198-B388-E269C3534BC0";

        /// <summary>
        /// The post update data migration job to chop the Email Preference Entry block.
        /// </summary>
        public const string DATA_MIGRATIONS_162_CHOP_EMAIL_PREFERENCE_ENTRY = "AE07C80A-80A4-48FD-908C-56DDB1CAA322";

        /// <summary>
        /// The post update data migration job to remove legacy preference attributes.
        /// </summary>
        public const string DATA_MIGRATIONS_170_REMOVE_LEGACY_PREFERENCES = "46d98280-7611-4588-831d-6924e2be9da6";

        /// <summary>
        /// The <see cref="Rock.Jobs.UpdatePersistedDatasets" /> job.
        /// </summary>
        public const string UPDATE_PERSISTED_DATASETS = "B6D3B48A-039A-4A1C-87BE-3FC0152AB5DA";
    }
}