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

namespace Rock.SystemKey
{
    /// <summary>
    /// This class holds Rock's well known System Setting keys.
    /// </summary>
    public class SystemSetting
    {
        /// <summary>
        /// This system setting's guid represents a unique identifier for each installation of Rock.
        /// The value it stores is the current version of Rock for that installation.
        /// </summary>
        public const string ROCK_INSTANCE_ID = "RockInstanceId";

        /// <summary>
        /// Holds the System Setting key for the sample data load date/time.
        /// </summary>
        public const string SAMPLEDATA_DATE = "com.rockrms.sampledata.datetime";

        /// <summary>
        /// Percent Confidence threshold for automatically setting gender based on a name
        /// </summary>
        public const string GENDER_AUTO_FILL_CONFIDENCE = "core_GenderAutoFillConfidence";

        /// <summary>
        /// The minimum distance in miles person/family has to have moved before automatically inactivating their record
        /// </summary>
        public const string NCOA_MINIMUM_MOVE_DISTANCE_TO_INACTIVATE = "core_MinimumMoveDistanceToInactivate";

        /// <summary>
        /// Should a NCOA 48 month move request change a family's address to 'previous'
        /// </summary>
        public const string NCOA_SET_48_MONTH_AS_PREVIOUS = "core_Set48monthAsPrevious";

        /// <summary>
        /// Should a NCOA invalid address change a family's address to 'previous'
        /// </summary>
        public const string NCOA_SET_INVALID_AS_PREVIOUS = "core-SetinvalidAsAddress";

        /// <summary>
        /// Settings for how people/families should be reactivated
        /// </summary>
        public const string DATA_AUTOMATION_REACTIVATE_PEOPLE = "core_DataAutomationReactivatePeople";

        /// <summary>
        /// Settings for how people/families should be inactivated
        /// </summary>
        public const string DATA_AUTOMATION_INACTIVATE_PEOPLE = "core_DataAutomationInactivatePeople";

        /// <summary>
        /// Settings for if/when a family's campus should be updated
        /// </summary>
        public const string DATA_AUTOMATION_CAMPUS_UPDATE = "core_DataAutomationUpdateFamilyCampus";

        /// <summary>
        /// Settings for if/when adult children should be moved to their own family
        /// </summary>
        public const string DATA_AUTOMATION_ADULT_CHILDREN = "core_DataAutomationAdultChildren";

        /// <summary>
        /// Settings for Updating Person Connection Status
        /// </summary>
        public const string DATA_AUTOMATION_UPDATE_PERSON_CONNECTION_STATUS = "core_DataAutomationUpdatePersonConnectionStatus";

        /// <summary>
        /// Settings for Updating Family Status
        /// </summary>
        public const string DATA_AUTOMATION_UPDATE_FAMILY_STATUS = "core_DataAutomationUpdateFamilyStatus";

        /// <summary>
        /// The default background check provider
        /// </summary>
        public const string DEFAULT_BACKGROUND_CHECK_PROVIDER = "core_DefaultBackgroundCheckProvider";

        /// <summary>
        /// The font awesome pro key
        /// </summary>
        public const string FONT_AWESOME_PRO_KEY = "core_FontAwesomeProKey";

        /// <summary>
        /// Enable multi time zone support. Default is false
        /// </summary>
        public const string ENABLE_MULTI_TIME_ZONE_SUPPORT = "core_EnableMultiTimeZoneSupport";

        /// <summary>
        /// The day of the week that is considered the 'Start Day' (First Day Of week). This is used to compute 'SundayDate'. Default is Monday.
        /// </summary>
        public const string START_DAY_OF_WEEK = "core_StartDayOfWeek";

        /// <summary>
        /// Enable a redis cache cluster
        /// </summary>
        public const string REDIS_ENABLE_CACHE_CLUSTER = "EnableRedisCacheCluster";

        /// <summary>
        /// The redis connection string
        /// </summary>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use REDIS_ENDPOINT_LIST, REDIS_PASSWORD, and REDIS_DATABASE_NUMBER instead.", true )]
        public const string REDIS_CONNECTION_STRING = "RedisConnectionString";

        /// <summary>
        /// Comma separated list of Redis endpoints (e.g. server.com:6379)
        /// </summary>
        public const string REDIS_ENDPOINT_LIST = "RedisEndpointList";

        /// <summary>
        /// The redis password
        /// </summary>
        public const string REDIS_PASSWORD = "RedisPassword";

        /// <summary>
        /// The redis database index number
        /// </summary>
        public const string REDIS_DATABASE_NUMBER = "RedisDatabaseNumber";

        /// <summary>
        /// Settings for Spark Data NCOA
        /// </summary>
        public const string SPARK_DATA_NCOA = "core_SparkDataNcoa";

        /// <summary>
        /// Settings for Spark Data
        /// </summary>
        public const string SPARK_DATA = "core_SparkData";

        /// <summary>
        /// The rock logging settings
        /// </summary>
        public const string ROCK_LOGGING_SETTINGS = "core_LoggingConfig";

        /// <summary>
        /// Enable system.diagnostics ADO.NET Off-By-Default Performance Counters
        /// </summary>
        public const string SYSTEM_DIAGNOSTICS_ENABLE_ADO_NET_PERFORMANCE_COUNTERS = "core_EnableAdoNetPerformanceCounters";

        /// <summary>
        /// Settings for Do Not Disturb Start
        /// </summary>
        [Obsolete( "This functionality is no longer used." )]
        [RockObsolete( "1.9" )]
        public const string DO_NOT_DISTURB_START = "core_DoNotDisturbStart";

        /// <summary>
        /// Settings for Do Not Disturb End
        /// </summary>
        [Obsolete( "This functionality is no longer used." )]
        [RockObsolete( "1.9" )]
        public const string DO_NOT_DISTURB_END = "core_DoNotDisturbEnd";

        /// <summary>
        /// Settings for Do Not Disturb Active
        /// </summary>
        [Obsolete( "This functionality is no longer used." )]
        [RockObsolete( "1.9" )]
        public const string DO_NOT_DISTURB_ACTIVE = "core_DoNotDisturbActive";
    }
}