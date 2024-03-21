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
        /// Always show businesses in PersonPicker controls
        /// </summary>
        public const string ALWAYS_SHOW_BUSINESS_IN_PERSONPICKER = "core_AlwaysShowBusinessInPersonPicker";

        /// <summary>
        /// The setting to have Rock poll itself to keep it alive during times of inactivity. Usually this can stay disabled.
        /// </summary>
        public const string ENABLE_KEEP_ALIVE = "core_EnableKeepAlive";

        /// <summary>
        /// The PDF external render endpoint to use for the <see cref="Pdf.PdfGenerator"/> instead of the local chrome engine
        /// For example: wss://chrome.browserless.io?token=YOUR-API-TOKEN
        /// </summary>
        public const string PDF_EXTERNAL_RENDER_ENDPOINT = "core_PDFExternalRenderEndpoint";

        /// <summary>
        /// The number of days that a visitor cookie persists. Default is 365.
        /// </summary>
        public const string VISITOR_COOKIE_PERSISTENCE_DAYS = "core_VisitorCookiePersistenceDays";

        /// <summary>
        /// The day of the week that is considered the 'Start Day' (First Day Of week). This is used to compute 'SundayDate'. Default is Monday.
        /// </summary>
        public const string START_DAY_OF_WEEK = "core_StartDayOfWeek";

        /// <summary>
        /// (Obsolete) Enable a redis cache cluster
        /// </summary>
        [Obsolete( "No longer needed since we no longer support Redis." )]
        [RockObsolete( "1.15" )]
        public const string REDIS_ENABLE_CACHE_CLUSTER = "EnableRedisCacheCluster";

        /// <summary>
        /// (Obsolete) Comma separated list of Redis endpoints (e.g. server.com:6379)
        /// </summary>
        [Obsolete( "No longer needed since we no longer support Redis." )]
        [RockObsolete( "1.15" )]
        public const string REDIS_ENDPOINT_LIST = "RedisEndpointList";

        /// <summary>
        /// (Obsolete) The redis password
        /// </summary>
        [Obsolete( "No longer needed since we no longer support Redis." )]
        [RockObsolete( "1.15" )]
        public const string REDIS_PASSWORD = "RedisPassword";

        /// <summary>
        /// (Obsolete) The redis database index number
        /// </summary>
        [Obsolete( "No longer needed since we no longer support Redis." )]
        [RockObsolete( "1.15" )]
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
        /// Settings for Communication setting approval template
        /// </summary>
        public const string COMMUNICATION_SETTING_APPROVAL_TEMPLATE = "core_CommunicationSettingApprovalTemplate";

        /// <summary>
        /// Enable Cache Manager statistics and performance counters.
        /// </summary>
        public const string CACHE_MANAGER_ENABLE_STATISTICS = "CacheManagerEnableStatistics";

        /// <summary>
        /// The open id connect RSA keys
        /// </summary>
        public const string OPEN_ID_CONNECT_RSA_KEYS = "OpenIdConnectRsaKeys";

        /// <summary>
        /// The open identifier connect settings
        /// </summary>
        public const string OPEN_ID_CONNECT_SETTINGS = "OpenIdConnectSettings";

        /// <summary>
        /// The webfarm is enabled
        /// </summary>
        public const string WEBFARM_IS_ENABLED = "WEBFARM_IS_ENABLED";

        /// <summary>
        /// The webfarm key
        /// </summary>
        public const string WEBFARM_KEY = "WEBFARM_KEY";

        /// <summary>
        /// The webfarm leadership polling interval lower limit seconds
        /// </summary>
        public const string WEBFARM_LEADERSHIP_POLLING_INTERVAL_LOWER_LIMIT_SECONDS = "WEBFARM_LEADERSHIP_POLLING_INTERVAL_LOWER_LIMIT_SECONDS";

        /// <summary>
        /// The webfarm leadership polling interval upper limit seconds
        /// </summary>
        public const string WEBFARM_LEADERSHIP_POLLING_INTERVAL_UPPER_LIMIT_SECONDS = "WEBFARM_LEADERSHIP_POLLING_INTERVAL_UPPER_LIMIT_SECONDS";

        /// <summary>
        /// The webfarm leadership minimum polling difference seconds
        /// </summary>
        public const string WEBFARM_LEADERSHIP_MIN_POLLING_DIFFERENCE_SECONDS = "WEBFARM_LEADERSHIP_MIN_POLLING_DIFFERENCE_SECONDS";

        /// <summary>
        /// The webfarm leadership maximum wait seconds
        /// </summary>
        public const string WEBFARM_LEADERSHIP_MAX_WAIT_SECONDS = "WEBFARM_LEADERSHIP_MAX_WAIT_SECONDS";

        /// <summary>
        /// The RockCleanup Last Run DateTime
        /// </summary>
        public const string ROCK_CLEANUP_LAST_RUN_DATETIME = "core_RockCleanup_LastRunDateTime";

        /// <summary>
        /// Settings for Giving Automation
        /// </summary>
        public const string GIVING_AUTOMATION_CONFIGURATION = "core_GivingAutomationConfiguration";

        /// <summary>
        /// The organization currency code
        /// </summary>
        public const string ORGANIZATION_CURRENCY_CODE = "OrganizationStandardCurrencyCode";

        /// <summary>
        /// Lava Engine Type.
        /// </summary>
        public const string LAVA_ENGINE_LIQUID_FRAMEWORK = "core_LavaEngine_LiquidFramework";

        /// <summary>
        /// The statement generator configuration
        /// </summary>
        public const string STATEMENT_GENERATOR_CONFIG = "core_StatementGeneratorConfig";

        /// <summary>
        /// The default security grant token duration in minutes.
        /// </summary>
        public const string DEFAULT_SECURITY_GRANT_TOKEN_DURATION = "core_DefaultSecurityGrantTokenDuration";

        /// <summary>
        /// The security grant token earliest date. This provides support to revoke existing tokens.
        /// </summary>
        public const string SECURITY_GRANT_TOKEN_EARLIEST_DATE = "core_SecurityGrantTokenEarliestDate";

        /// <summary>
        /// Security Settings (Account Protection profiles)
        /// </summary>
        public const string ROCK_SECURITY_SETTINGS = "core_RockSecuritySettings";

        /// <summary>
        /// Job Settings for <see cref="Rock.Jobs.PopulateInteractionSessionData"/>
        /// </summary>
        public const string POPULATE_INTERACTION_SESSION_DATA_JOB_SETTINGS = "core_PopulateInteractionSessionDataJobSettings";

        /// <summary>
        /// Number of minutes old the ROCK_SEGMENT_FILTERS cookie can be before it is considered stale and will be re-fetched from the database.
        /// </summary>
        public const string PERSONALIZATION_SEGMENT_COOKIE_AFFINITY_DURATION_MINUTES = "core_PersonalizationSegmentCookieAffinityDurationMinutes";

        /// <summary>
        /// Label text for the PersonRacePicker
        /// </summary>
        public const string PERSON_RACE_LABEL = "core_PersonRaceLabel";

        /// <summary>
        /// Label text for the PersonEthnicityPicker
        /// </summary>
        public const string PERSON_ETHNICITY_LABEL = "core_PersonEthnicityLabel";

        /// <summary>
        /// Label text for the PersonGenderPicker
        /// </summary>
        public const string PERSON_GENDER_LABEL = "core_GenderLabel";

        /// <summary>
        /// The name of the standard Rock Context.
        /// </summary>
        [Obsolete( "Use RockInstanceConfig to get connection strings." )]
        [RockObsolete( "1.16.3" )]
        public const string ROCK_CONTEXT = "RockContext";

        /// <summary>
        /// The name of the standard Rock Context for Read Only queries.
        /// </summary>
        [Obsolete( "Use RockInstanceConfig to get connection strings." )]
        [RockObsolete( "1.16.3" )]
        public const string ROCK_CONTEXT_READ_ONLY = "RockContextReadOnly";

        /// <summary>
        /// The Captcha site key.
        /// </summary>
        public const string CAPTCHA_SITE_KEY = "core_CaptchaSiteKey";

        /// <summary>
        /// The Captcha secret key.
        /// </summary>
        public const string CAPTCHA_SECRET_KEY = "core_CaptchaSecretKey";

        /// <summary>
        /// The protocol to use for sending telemetry for observability.
        /// </summary>
        public const string OBSERVABILITY_ENDPOINT_PROTOCOL = "core_ObservabilityEndpointProtocol";

        /// <summary>
        /// The URL to use for sending telemetry for observability.
        /// </summary>
        public const string OBSERVABILITY_ENDPOINT = "core_ObservabilityEndpoint";

        /// <summary>
        /// Determines if observability is enabled.
        /// </summary>
        public const string OBSERVABILITY_ENABLED = "core_ObservabilityEnabled";

        /// <summary>
        /// The headers to send with telemetry requests for observability.
        /// </summary>
        public const string OBSERVABILITY_ENDPOINT_HEADERS = "core_ObservabilityEndpointHeaders";

        /// <summary>
        /// The protocol to use for sending telemetry for observability.
        /// </summary>
        public const string OBSERVABILITY_TARGETED_QUERIES = "core_ObservabilityTargetedQueries";

        /// <summary>
        /// The maximum number of spans that can be created for a single trace
        /// in observability.
        /// </summary>
        public const string OBSERVABILITY_SPAN_COUNT_LIMIT = "core_ObservabilitySpanCountLimit";

        /// <summary>
        /// The maximum length of any single attribute value in observability
        /// traces.
        /// </summary>
        public const string OBSERVABILITY_MAX_ATTRIBUTE_LENGTH = "core_ObservabilityMaxAttributeLength";

        /// <summary>
        /// The label text for the SMS Opt-In checkbox
        /// </summary>
        public const string SMS_OPT_IN_MESSAGE_LABEL = "core_SmsOptInMessageLabel";

        /// <summary>
        /// The content library data (JSON).
        /// </summary>
        public const string CONTENT_LIBRARY_DATA_JSON = "core_ContentLibraryDataJson";

        /// <summary>
        /// Determine if Same Sex Couples are allowed.
        /// </summary>
        public const string BIBLE_STRICT_SPOUSE = "core_BibleStrictSpouse";

        /// <summary>
        /// The start date for the analytics calendar dimension.
        /// </summary>
        public const string ANALYTICS_CALENDAR_DIMENSION_START_DATE = "core_AnalyticsCalendarDimensionStartDate";

        /// <summary>
        /// The end date for the analytics calendar dimension.
        /// </summary>
        public const string ANALYTICS_CALENDAR_DIMENSION_END_DATE = "core_AnalyticsCalendarDimensionEndDate";

        /// <summary>
        /// The fiscal start month for the analytics calendar dimension.
        /// </summary>
        public const string ANALYTICS_CALENDAR_DIMENSION_FISCAL_START_MONTH = "core_AnalyticsCalendarDimensionFiscalStartMonth";

        /// <summary>
        /// Whether the giving month should use Sunday date for the analytics calendar dimension.
        /// </summary>
        public const string ANALYTICS_CALENDAR_DIMENSION_GIVING_MONTH_USE_SUNDAY_DATE = "core_AnalyticsCalendarDimensionGivingMonthUseSundayDate";
    }
}