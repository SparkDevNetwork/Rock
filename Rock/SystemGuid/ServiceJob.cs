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
        /// The Job to run Post v140 to add FK indexes on RegistrationRegistrant.RegistrationTemplateId, GroupMember.GroupTypeId, and ConnectionRequest.ConnectionTypeId.
        /// </summary>
        public const string DATA_MIGRATIONS_140_CREATE_FK_INDEXES = "D96BD1F7-6A4A-4DC0-B10D-40031F709573";

        /// <summary>
        /// The Job to run Post v12.4 Data Migrations to decrypt the expiration month / year and the name on card fields.
        /// </summary>
        public const string DATA_MIGRATIONS_124_DECRYPT_FINANCIAL_PAYMENT_DETAILS = "6C795E61-9DD4-4BE8-B9EB-E662E43B5E12";

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
    }
}