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
        /// The Job for migrating pre-v8.0 attendance records to occurrence records
        /// </summary>
        public const string MIGRATE_ATTENDANCE_OCCURRENCE = "98A2DCA5-5E2E-482A-A7CA-15DAD5B4EA65";

        /// <summary>
        /// The Job for migrating family check-in identifiers to person alternate ids
        /// </summary>
        public const string MIGRATE_FAMILY_CHECKIN_IDS = "E782C667-EF07-4AD2-86B7-01C1935AAF5B";

        /// <summary>
        /// The Job to run Post v8.0 Data Migrations
        /// </summary>
        public const string DATA_MIGRATIONS_80 = "AF760EF9-66BD-4A4D-AF95-749AA789ACAA";

        /// <summary>
        /// The Job to run Post v8.4 Data Migrations
        /// </summary>
        public const string DATA_MIGRATIONS_84 = "79FBDA04-ADFD-40D4-824F-E07D660F7858";

        /// <summary>
        /// The Job to run Post v7.4 Data Migrations
        /// </summary>
        public const string DATA_MIGRATIONS_74 = "FF760EF9-66BD-4A4D-AF95-749AA789ACAF";

        /// <summary>
        /// The Job to run Post v9 Data Migrations for DISC data
        /// </summary>
        public const string DATA_MIGRATIONS_90_DISC = "A839DFEC-B1A3-499C-9BB3-03241E8E5305";

        /// <summary>
        /// The Job to run Post v10.0 Data Migrations for AttributeValue.ValueAsNumeric
        /// </summary>
        public const string DATA_MIGRATIONS_100_ATTRIBUTEVALUE_VALUEASNUMERIC = "0A7573C9-D977-4A7E-BDD6-66DD36CBF6F3";

        /// <summary>
        /// The Job to run Post v10.0 Data Migrations for SundayDate
        /// </summary>
        public const string DATA_MIGRATIONS_100_SUNDAYDATE = "CC263453-B290-4393-BB91-1C1C87CAE291";

        /// <summary>
        /// The Job to run Post v9 Data Migrations to convert Scheduled Transaction Notes to History
        /// </summary>
        public const string DATA_MIGRATIONS_90_SCHEDULEDTRANSACTIONNOTESTOHISTORY = "6707AA98-7CF8-4258-A75A-0881CD68B0D9";

        /// <summary>
        /// The Job to run Post v9 Data Migrations
        /// </summary>
        public const string DATA_MIGRATIONS_90 = "3F279016-C7D1-490F-835D-8FFE6D943A32";

        /// <summary>
        /// The Job to run Post v10.3 Data Migrations for Spiritual Gifts Assessment updates
        /// </summary>
        public const string DATA_MIGRATIONS_103_SPIRITUAL_GIFTS = "B16F889F-3349-4CA9-976D-7EF098DD8BC6";

        /// <summary>
        /// The Job to run Post v11 Data Migrations to update Date Keys on several tables
        /// </summary>
        public const string DATA_MIGRATIONS_110_POPULATE_DATE_KEYS = "E56FD4FC-02F8-4A46-A91D-E86C2B635870";

        /// <summary>
        /// The job to run Post V11 to create an index on <seealso cref="Rock.Model.CommunicationRecipient.ResponseCode"/>
        /// </summary>
        public const string DATA_MIGRATIONS_110_COMMUNICATIONRECIPIENT_RESPONSECODE_INDEX = "131F9418-777B-4A34-A19B-EB9A65893602";

        /// <summary>
        /// The Job to Migrate pre-v8.0 History Summary Data
        /// </summary>
        public const string MIGRATE_HISTORY_SUMMARY_DATA = "CF2221CC-1E0A-422B-B0F7-5D81AF1DDB14";

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
        /// The collect hosting metrcis job - collect metrics regarding database connections, Etc.
        /// </summary>
        public const string COLLECT_HOSTING_METRICS = "36FA38CA-9DB0-40A8-BABD-5411121B4809";
    }
}