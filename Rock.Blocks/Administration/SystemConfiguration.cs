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
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Rock.Attribute;
using Rock.Configuration;
using Rock.Model;
using Rock.Observability;
using Rock.SystemKey;
using Rock.ViewModels.Blocks.Administration.SystemConfiguration;
using Rock.ViewModels.Utility;
using Rock.Web.Cache.NonEntities;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Administration
{
    /// <summary>
    /// Used for making configuration changes to configurable items in the web.config.
    /// </summary>
    /// <seealso cref="RockBlockType" />
    [DisplayName( "System Configuration" )]
    [Category( "Administration" )]
    [Description( "Used for making configuration changes to configurable items in the web.config." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [SystemGuid.EntityTypeGuid( "7ECDCE1B-D63F-42AA-88B6-7C5585E1F33A" )]
    [SystemGuid.BlockTypeGuid( "3855B15B-C903-446A-AE5B-891AB52851CB" )]
    public class SystemConfiguration : RockBlockType
    {
        #region Keys

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Fields

        private System.Configuration.Configuration _webConfig;

        #endregion

        #region Default Settings

        /// <summary>
        /// Default setting values
        /// </summary>
        private static class SettingDefault
        {
            /// <summary>
            /// The visitor cookie timeout days
            /// </summary>
            public const int VisitorCookieTimeoutDays = 365;

            /// <summary>
            /// The login cookie timeout (in minutes)
            /// </summary>
            public const int LoginCookieTimeoutMinutes = 43200;

            /// <summary>
            /// The personalization cookie length in minutes
            /// </summary>
            public const int PersonalizationCookieCacheLengthMinutes = 5;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = GetInitializationBox();

            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        /// <summary>
        /// Gets the initialization box.
        /// </summary>
        /// <returns></returns>
        private SystemConfigurationInitializationBox GetInitializationBox()
        {
            var box = new SystemConfigurationInitializationBox()
            {
                ExperimentalSettingsConfigurationBag = InitializeExperimentalSettingsBag(),
                FamilyRulesSettingsConfigurationBag = InitializeFamilyRulesConfigurationBag(),
                GeneralConfigurationBag = InitializeGeneralConfigurationBag(),
                ObservabilityConfigurationBag = InitializeObservabilityBag(),
                UiSettingsConfigurationBag = InitializeUiSettingsConfigurationBag(),
                WebConfigConfigurationBag = InitializeWebConfigConfigurationBag(),
                ObservabilityEndpointProtocols = ObservabilityHelper.GetOpenTelemetryExporterProtocolsAsListItemBag(),
                TimeZones = GetTimeZones()
            };

            return box;
        }

        /// <summary>
        /// Initializes the web configuration bag.
        /// </summary>
        /// <returns></returns>
        private WebConfigConfigurationBag InitializeWebConfigConfigurationBag()
        {
            var config = GetWebConfig();
            var appsettings = config.AppSettings.Settings;

            return new WebConfigConfigurationBag()
            {
                AzureSignalRAccessKey = appsettings["AzureSignalRAccessKey"]?.Value,
                AzureSignalREndpoint = appsettings["AzureSignalREndpoint"]?.Value,
                EnableDatabasePerformanceCounters = Rock.Web.SystemSettings.GetValue( SystemSetting.SYSTEM_DIAGNOSTICS_ENABLE_ADO_NET_PERFORMANCE_COUNTERS ).AsBoolean(),
                EnableRunJobsInIISContext = appsettings["RunJobsInIISContext"]?.Value.AsBooleanOrNull() ?? false,
                LoginCookiePersistenceLength = GetLoginCookieTimeout(),
                MaxUploadFileSize = GetMaxFileSize(),
                ObservabilityServiceName = appsettings["ObservabilityServiceName"]?.Value,
                TimeZone = RockDateTime.OrgTimeZoneInfo.Id
            };
        }

        /// <summary>
        /// Initializes the UI settings configuration bag.
        /// </summary>
        /// <returns></returns>
        private UiSettingsConfigurationBag InitializeUiSettingsConfigurationBag()
        {
            return new UiSettingsConfigurationBag()
            {
                CaptchaSecretKey = Rock.Web.SystemSettings.GetValue( SystemSetting.CAPTCHA_SECRET_KEY ),
                CaptchaSiteKey = Rock.Web.SystemSettings.GetValue( SystemSetting.CAPTCHA_SITE_KEY ),
                EthnicityLabel = Rock.Web.SystemSettings.GetValue( SystemSetting.PERSON_ETHNICITY_LABEL ),
                RaceLabel = Rock.Web.SystemSettings.GetValue( SystemSetting.PERSON_RACE_LABEL ),
                SmsOptInMessage = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.SMS_OPT_IN_MESSAGE_LABEL )
            };
        }

        /// <summary>
        /// Initializes the observability bag.
        /// </summary>
        /// <returns></returns>
        private ObservabilityConfigurationBag InitializeObservabilityBag()
        {
            return new ObservabilityConfigurationBag()
            {
                EnableObservability = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENABLED ).AsBoolean(),
                Endpoint = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENDPOINT ),
                EndpointHeaders = GetKeyValueListItems( Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENDPOINT_HEADERS ) ),
                EndpointProtocol = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENDPOINT_PROTOCOL ),
                IncludeQueryStatements = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_INCLUDE_QUERY_STATEMENTS ).AsBoolean(),
                MaximumAttributeLength = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_MAX_ATTRIBUTE_LENGTH ).AsIntegerOrNull(),
                SpanCountLimit = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_SPAN_COUNT_LIMIT ).AsIntegerOrNull(),
                TargetedQueries = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_TARGETED_QUERIES ).SplitDelimitedValues( "|", StringSplitOptions.RemoveEmptyEntries ).ToList()
            };
        }

        /// <summary>
        /// Initializes the general configuration bag.
        /// </summary>
        /// <returns></returns>
        private GeneralConfigurationBag InitializeGeneralConfigurationBag()
        {
            return new GeneralConfigurationBag()
            {
                EnableKeepAlive = Rock.Web.SystemSettings.GetValue( SystemSetting.ENABLE_KEEP_ALIVE ).AsBoolean(),
                IncludeBusinessInPersonPicker = Rock.Web.SystemSettings.GetValue( SystemSetting.ALWAYS_SHOW_BUSINESS_IN_PERSONPICKER ).AsBoolean(),
                IsMultipleTimeZoneSupportEnabled = Rock.Web.SystemSettings.GetValue( SystemSetting.ENABLE_MULTI_TIME_ZONE_SUPPORT ).AsBoolean(),
                PDFExternalRenderEndpoint = Rock.Web.SystemSettings.GetValue( SystemSetting.PDF_EXTERNAL_RENDER_ENDPOINT ),
                PersonalizationCookieCacheLengthMinutes = Rock.Web.SystemSettings.GetValue( SystemSetting.PERSONALIZATION_SEGMENT_COOKIE_AFFINITY_DURATION_MINUTES ).AsIntegerOrNull() ?? SettingDefault.PersonalizationCookieCacheLengthMinutes,
                VisitorCookiePersistenceLengthDays = Rock.Web.SystemSettings.GetValue( SystemSetting.VISITOR_COOKIE_PERSISTENCE_DAYS ).AsIntegerOrNull() ?? SettingDefault.VisitorCookieTimeoutDays
            };
        }

        /// <summary>
        /// Initializes the family rules configuration bag.
        /// </summary>
        /// <returns></returns>
        private FamilyRulesSettingsConfigurationBag InitializeFamilyRulesConfigurationBag()
        {
            return new FamilyRulesSettingsConfigurationBag()
            {
                EnableBibleStrictSpouse = Rock.Web.SystemSettings.GetValue( SystemSetting.BIBLE_STRICT_SPOUSE ).AsBoolean( true )
            };
        }

        /// <summary>
        /// Initializes the experimental settings bag.
        /// </summary>
        /// <returns></returns>
        private ExperimentalSettingsConfigurationBag InitializeExperimentalSettingsBag()
        {
            return new ExperimentalSettingsConfigurationBag()
            {
                SecurityGrantTokenDuration = Math.Max( Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.DEFAULT_SECURITY_GRANT_TOKEN_DURATION )?.AsIntegerOrNull() ?? 4320, 60 ),
                StartingDayOfWeek = ( ( int ) RockDateTime.FirstDayOfWeek ).ToString(),
            };
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <summary>
        /// Converts the parameters value from a list of ListItemBag items to a | delimited kpv sting
        /// </summary>
        /// <param name="items">The key value list items.</param>
        /// <returns></returns>
        private string JoinKeyValueListItems( List<ListItemBag> items )
        {
            var sb = new StringBuilder();

            foreach ( var listItemBag in items )
            {
                sb.Append( listItemBag.Text ).Append( '^' ).Append( listItemBag.Value ).Append( '|' );
            }

            return sb.ToString().Trim( '|' );
        }

        /// <summary>
        /// Gets the Key Value pair values from the delimited string.
        /// </summary>
        /// <param name="keyValueItemsString">The parameters.</param>
        /// <returns></returns>
        private List<ListItemBag> GetKeyValueListItems( string keyValueItemsString )
        {
            var listBagItems = new List<ListItemBag>();
            foreach ( var value in keyValueItemsString.SplitDelimitedValues( "|", StringSplitOptions.RemoveEmptyEntries ) )
            {
                var kvp = value.SplitDelimitedValues( "^" );
                if ( kvp.Length == 2 )
                {
                    listBagItems.Add( new ListItemBag { Text = kvp[0], Value = kvp[1] } );
                }
                else
                {
                    listBagItems.Add( new ListItemBag { Text = kvp[0] } );
                }
            }

            return listBagItems;
        }

        /// <summary>
        /// Gets the login cookie timeout.
        /// </summary>
        /// <returns></returns>
        private int GetLoginCookieTimeout()
        {
            // Access the authentication section
            var authenticationSection = GetSection( "authentication" );
            if ( authenticationSection == null )
            {
                return SettingDefault.LoginCookieTimeoutMinutes;
            }

            var xml = authenticationSection.SectionInformation.GetRawXml();

            // Parse to an XElement since it's easier to read attributes and work with.
            var root = XElement.Parse( xml );
            var element = root?.Element( "forms" );

            if ( element != null )
            {
                var timeoutValue = element.Attribute( "timeout" )?.Value;
                return int.TryParse( timeoutValue, out var value ) ? value : SettingDefault.LoginCookieTimeoutMinutes;
            }

            return SettingDefault.LoginCookieTimeoutMinutes;
        }

        /// <summary>
        /// Gets the maximum file size value as defined by the web config, if a value is found it is convert to its equivalent MB value.
        /// </summary>
        /// <returns></returns>
        private int? GetMaxFileSize()
        {
            var httpRuntimeSection = GetSection( "httpRuntime" );

            if ( httpRuntimeSection == null )
            {
                return null;
            }

            var xml = httpRuntimeSection.SectionInformation.GetRawXml();

            // Parse to an XElement since it's easier to read attributes and work with.
            var element = XElement.Parse( xml );

            if ( element != null )
            {
                var maxRequestLengthValue = element.Attribute( "maxRequestLength" )?.Value;

                if ( int.TryParse( maxRequestLengthValue, out var maxRequestLength ) )
                {
                    return maxRequestLength / 1024;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the specified section from the 'system.web' section group in the web config.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="config">The web config.</param>
        /// <returns></returns>
        private ConfigurationSection GetSection( string section, System.Configuration.Configuration config = null )
        {
            config = config ?? GetWebConfig();

            // Access the system.web section
            var systemWebGroup = config.GetSectionGroup( "system.web" );
            if ( systemWebGroup?.Sections == null )
            {
                return null;
            }

            // Access the section
            return systemWebGroup.Sections[section];
        }

        /// <summary>
        /// Gets the web configuration.
        /// </summary>
        /// <returns></returns>
        private System.Configuration.Configuration GetWebConfig()
        {
            // Open the Web.config file
            if ( _webConfig == null )
            {
                var map = new ExeConfigurationFileMap { ExeConfigFilename = GetWebConfigPath() };
                _webConfig = ConfigurationManager.OpenMappedExeConfiguration( map, ConfigurationUserLevel.None );
            }

            return _webConfig;
        }

        /// <summary>
        /// Gets the web configuration path.
        /// </summary>
        /// <returns></returns>
        private string GetWebConfigPath()
        {
            return Path.Combine( System.AppDomain.CurrentDomain.BaseDirectory, "web.config" );
        }

        /// <summary>
        /// Saves the timeout value as an attribute in the authentication section of the web config.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="config">The configuration.</param>
        private void SaveTimeout( WebConfigConfigurationBag bag, System.Configuration.Configuration config )
        {
            var authenticationSection = GetSection( "authentication", config );

            if ( authenticationSection != null )
            {
                // Convert section to xml string.
                var xml = authenticationSection.SectionInformation.GetRawXml();

                // Convert xml string to XElement so we can update the attributes on the desired elements.
                var root = XElement.Parse( xml );
                var element = root?.Element( "forms" );

                if ( element != null )
                {
                    var loginCookieTimeoutMinutes = ( ( int ) TimeSpan.FromMinutes( bag.LoginCookiePersistenceLength ?? SettingDefault.LoginCookieTimeoutMinutes ).TotalMinutes ).ToString();
                    var currentValue = element.Attribute( "timeout" )?.Value;

                    if ( currentValue != loginCookieTimeoutMinutes )
                    {
                        element.SetAttributeValue( "timeout", loginCookieTimeoutMinutes );

                        var updatedXml = root.ToString();
                        authenticationSection.SectionInformation.SetRawXml( updatedXml );

                        ConfigurationManager.RefreshSection( authenticationSection.SectionInformation.Name );
                    }
                }
            }
        }

        /// <summary>
        /// Saves the maximum length of the request as an attribute in the 'httpRuntime' of the web config section.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="config">The configuration.</param>
        private void SaveMaxRequestLength( WebConfigConfigurationBag bag, System.Configuration.Configuration config )
        {
            var httpRuntimeSection = GetSection( "httpRuntime", config );

            if ( httpRuntimeSection != null )
            {
                // Convert section to xml string.
                var xml = httpRuntimeSection.SectionInformation.GetRawXml();

                // Convert xml string to XElement so we can update the attributes on the desired elements.
                var element = XElement.Parse( xml );

                if ( element != null )
                {
                    var maxUploadFileSize = ( ( bag.MaxUploadFileSize ?? 1 ) * 1024 ).ToString();
                    var currentValue = element.Attribute( "maxRequestLength" )?.Value;

                    if ( maxUploadFileSize != currentValue )
                    {
                        element.SetAttributeValue( "maxRequestLength", maxUploadFileSize );

                        var updatedXml = element.ToString();
                        httpRuntimeSection.SectionInformation.SetRawXml( updatedXml );

                        ConfigurationManager.RefreshSection( httpRuntimeSection.SectionInformation.Name );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the time zones.
        /// </summary>
        /// <returns></returns>
        private List<ListItemBag> GetTimeZones()
        {
            return TimeZoneInfo.GetSystemTimeZones().Select( tz => new ListItemBag() { Text = tz.DisplayName, Value = tz.Id } ).ToList();
        }

        /// <summary>
        /// Creates a new instance of a <see cref="SaveConfigurationResponseBag"/> displaying the success text with a success alert.
        /// </summary>
        /// <returns></returns>
        private SaveConfigurationResponseBag GetSuccessResponseBag( string successMessage = null )
        {
            return new SaveConfigurationResponseBag() { AlertType = "success", SuccessMessage = successMessage };
        }

        /// <summary>
        /// Creates a new instance of a <see cref="SaveConfigurationResponseBag"/> displaying the error message with a warning alert.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private SaveConfigurationResponseBag GetWarningResponseBag( string errorMessage = null )
        {
            return new SaveConfigurationResponseBag() { AlertType = "warning", ErrorMessage = errorMessage };
        }

        /// <summary>
        /// Creates a new instance of a <see cref="SaveConfigurationResponseBag"/> displaying the error message with a danger alert.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private SaveConfigurationResponseBag GetErrorResponseBag( string errorMessage = null )
        {
            return new SaveConfigurationResponseBag() { AlertType = "danger", ErrorMessage = errorMessage };
        }

        /// <summary>
        /// Transform the web.config to inject the maximum allowed content length
        /// into the requestLimits tag of the requestFiltering section of the web.config.
        /// </summary>
        /// <returns>true if the transform was successful; false otherwise.</returns>
        protected bool SaveMaxAllowedContentLength( int maxFileUploadSize )
        {
            int maxContentLengthBytes = maxFileUploadSize * 1048576;

            string transformString = string.Format( @"<?xml version='1.0'?>
<configuration xmlns:xdt='http://schemas.microsoft.com/XML-Document-Transform'>
  <system.webServer>
    <security>
      <requestFiltering>
        <requestLimits maxAllowedContentLength='{0}' xdt:Transform='SetAttributes(maxAllowedContentLength)'/>
      </requestFiltering>
    </security>
  </system.webServer>
</configuration>", maxContentLengthBytes );

            return XmlHelper.TransformXmlDocument( GetWebConfigPath(), transformString );
        }

        /// <summary>
        /// Transform the web.config to inject system.diagnostics configuration and save related system settings.
        /// </summary>
        /// <returns>true if the transform was successful; false otherwise.</returns>
        private bool SaveSystemDiagnosticsSettings( bool adoNetPerformanceCountersAreEnabled )
        {
            var transformSb = new StringBuilder( @"<?xml version='1.0'?>
<configuration xmlns:xdt='http://schemas.microsoft.com/XML-Document-Transform'>
  <system.diagnostics xdt:Transform='InsertIfMissing'>
    <switches xdt:Transform='InsertIfMissing'>
      <add name='ConnectionPoolPerformanceCounterDetail' xdt:Transform='RemoveAll' xdt:Locator='Match(name)' />" );

            if ( adoNetPerformanceCountersAreEnabled )
            {
                transformSb.Append( @"
      <add name='ConnectionPoolPerformanceCounterDetail' value='4' xdt:Transform='Insert' />" );
            }

            transformSb.Append( @"
    </switches>
  </system.diagnostics>
</configuration>" );

            if ( !XmlHelper.TransformXmlDocument( GetWebConfigPath(), transformSb.ToString() ) )
            {
                return false;
            }

            // Update the "core_EnableAdoNetPerformanceCounters" System Setting
            Rock.Web.SystemSettings.SetValue( SystemSetting.SYSTEM_DIAGNOSTICS_ENABLE_ADO_NET_PERFORMANCE_COUNTERS, adoNetPerformanceCountersAreEnabled.ToString() );

            // Toggle the "Collect Hosting Metrics" ServiceJob's IsActive status if necessary
            var serviceJob = new ServiceJobService( RockContext ).Get( Rock.SystemGuid.ServiceJob.COLLECT_HOSTING_METRICS.AsGuid() );

            if ( serviceJob != null && serviceJob.IsActive != adoNetPerformanceCountersAreEnabled )
            {
                serviceJob.IsActive = !serviceJob.IsActive;

                RockContext.SaveChanges();
            }

            return true;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Saves the general configuration.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <returns></returns>
        [BlockAction( "SaveGeneralConfiguration" )]
        public BlockActionResult SaveGeneralConfiguration( GeneralConfigurationBag bag )
        {
            Rock.Web.SystemSettings.SetValue( SystemSetting.ENABLE_MULTI_TIME_ZONE_SUPPORT, bag.IsMultipleTimeZoneSupportEnabled.ToString() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.ALWAYS_SHOW_BUSINESS_IN_PERSONPICKER, bag.IncludeBusinessInPersonPicker.ToString() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.ENABLE_KEEP_ALIVE, bag.EnableKeepAlive.ToString() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.PDF_EXTERNAL_RENDER_ENDPOINT, bag.PDFExternalRenderEndpoint );
            Rock.Web.SystemSettings.SetValue( SystemSetting.VISITOR_COOKIE_PERSISTENCE_DAYS, bag.VisitorCookiePersistenceLengthDays?.ToString() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.PERSONALIZATION_SEGMENT_COOKIE_AFFINITY_DURATION_MINUTES, bag.PersonalizationCookieCacheLengthMinutes?.ToString() );

            return ActionOk( GetSuccessResponseBag( "Settings saved successfully." ) );
        }

        /// <summary>
        /// Saves the UI settings configuration.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <returns></returns>
        [BlockAction( "SaveUiSettingsConfiguration" )]
        public BlockActionResult SaveUiSettingsConfiguration( UiSettingsConfigurationBag bag )
        {
            // Save Race and Ethnicity label values
            Rock.Web.SystemSettings.SetValue( SystemSetting.PERSON_RACE_LABEL, bag.RaceLabel );
            Rock.Web.SystemSettings.SetValue( SystemSetting.PERSON_ETHNICITY_LABEL, bag.EthnicityLabel );

            // Save Captcha keys
            Rock.Web.SystemSettings.SetValue( SystemSetting.CAPTCHA_SITE_KEY, bag.CaptchaSiteKey );
            Rock.Web.SystemSettings.SetValue( SystemSetting.CAPTCHA_SECRET_KEY, bag.CaptchaSecretKey );

            Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.SMS_OPT_IN_MESSAGE_LABEL, bag.SmsOptInMessage );

            return ActionOk( GetSuccessResponseBag( "Settings saved successfully." ) );
        }

        /// <summary>
        /// Saves the observability configuration.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <returns></returns>
        [BlockAction( "SaveObservabilityConfiguration" )]
        public BlockActionResult SaveObservabilityConfiguration( ObservabilityConfigurationBag bag )
        {
            if ( bag.EnableObservability && bag.Endpoint.IsNullOrWhiteSpace() )
            {
                return ActionOk( GetWarningResponseBag( "To enable observability, please provide a valid service endpoint. (e.g. https://otlp.nr-data.net:4317)" ) );
            }

            Rock.Web.SystemSettings.SetValue( SystemSetting.OBSERVABILITY_ENABLED, bag.EnableObservability.ToString() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.OBSERVABILITY_ENDPOINT_PROTOCOL, bag.EndpointProtocol );
            Rock.Web.SystemSettings.SetValue( SystemSetting.OBSERVABILITY_ENDPOINT_HEADERS, JoinKeyValueListItems( bag.EndpointHeaders ) );
            Rock.Web.SystemSettings.SetValue( SystemSetting.OBSERVABILITY_ENDPOINT, bag.Endpoint );
            Rock.Web.SystemSettings.SetValue( SystemSetting.OBSERVABILITY_SPAN_COUNT_LIMIT, bag.SpanCountLimit?.ToString() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.OBSERVABILITY_MAX_ATTRIBUTE_LENGTH, bag.MaximumAttributeLength.ToString() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.OBSERVABILITY_INCLUDE_QUERY_STATEMENTS, bag.IncludeQueryStatements.ToString() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.OBSERVABILITY_TARGETED_QUERIES, string.Join( "|", bag.TargetedQueries ) );

            // Clear the targeted query hash cache
            DbCommandObservabilityCache.ClearTargetedQueryHashes();

            // Update the provider
            ObservabilityHelper.ReconfigureObservability();

            return ActionOk( GetSuccessResponseBag( "Settings saved successfully." ) );
        }

        /// <summary>
        /// Saves the experimental configuration.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <returns></returns>
        [BlockAction( "SaveExperimentalConfiguration" )]
        public BlockActionResult SaveExperimentalConfiguration( ExperimentalSettingsConfigurationBag bag )
        {
            var startingDayOfWeek = bag.StartingDayOfWeek.ConvertToEnumOrNull<DayOfWeek>();
            var response = GetSuccessResponseBag();

            if ( startingDayOfWeek != RockDateTime.FirstDayOfWeek )
            {
                Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.START_DAY_OF_WEEK, startingDayOfWeek.ConvertToInt().ToString() );
                Task.Run( () =>
                {
                    try
                    {
                        RockDateTimeHelper.UpdateSundayDateData();
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( new Exception( "An error occurred applying the Start Of Week setting", ex ) );
                    }
                } );

                response.SuccessMessage = string.Format( "Start Day of Week is now set to <strong>{0}</strong>. ", startingDayOfWeek.ConvertToString() );
            }

            var oldSecurityGrantTokenDuration = Math.Max( Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.DEFAULT_SECURITY_GRANT_TOKEN_DURATION ).AsInteger(), 60 );
            var newSecurityGrantTokenDuration = Math.Max( bag.SecurityGrantTokenDuration ?? 0, 60 );

            if ( oldSecurityGrantTokenDuration != newSecurityGrantTokenDuration )
            {
                Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.DEFAULT_SECURITY_GRANT_TOKEN_DURATION, newSecurityGrantTokenDuration.ToString() );
                response.SecondaryMessageAlertType = nameof( NotificationBoxType.Success ).ToLower();
                response.SecondaryMessage = "Security grant token duration has been successfully updated.";
            }

            return ActionOk( response );
        }

        /// <summary>
        /// Saves the web configuration.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <returns></returns>
        [BlockAction( "SaveWebConfigConfiguration" )]
        public BlockActionResult SaveWebConfigConfiguration( WebConfigConfigurationBag bag )
        {
            var config = GetWebConfig();

            config.AppSettings.Settings["OrgTimeZone"].Value = bag.TimeZone;
            config.AppSettings.Settings["RunJobsInIISContext"].Value = bag.EnableRunJobsInIISContext.ToString();
            config.AppSettings.Settings["AzureSignalREndpoint"].Value = bag.AzureSignalREndpoint;
            config.AppSettings.Settings["AzureSignalRAccessKey"].Value = bag.AzureSignalRAccessKey;
            config.AppSettings.Settings["ObservabilityServiceName"].Value = bag.ObservabilityServiceName;
            ConfigurationManager.RefreshSection( config.AppSettings.SectionInformation.Name );

            SaveMaxRequestLength( bag, config );

            SaveTimeout( bag, config );

            config.Save( ConfigurationSaveMode.Modified );

            string errorMessage = null;
            string errorMessageTemplate = "An error occurred which prevented the {0} from being saved within the web.config.";

            if ( !SaveMaxAllowedContentLength( bag.MaxUploadFileSize ?? 1 ) )
            {
                errorMessage = string.Format( errorMessageTemplate, "'MaxAllowedContentLength'" );
            }

            if ( errorMessage == null && !SaveSystemDiagnosticsSettings( bag.EnableDatabasePerformanceCounters ) )
            {
                errorMessage = string.Format( errorMessageTemplate, "system.diagnostics configuration" );
            }

            if ( !string.IsNullOrEmpty( errorMessage ) )
            {
                return ActionOk( GetErrorResponseBag( errorMessage ) );
            }
            else
            {
                return ActionOk( GetSuccessResponseBag( "You will need to reload this page to continue." ) );
            }
        }

        /// <summary>
        /// Saves the family rules settings configuration.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <returns></returns>
        [BlockAction( "SaveFamilyRulesSettingsConfiguration" )]
        public BlockActionResult SaveFamilyRulesSettingsConfiguration( FamilyRulesSettingsConfigurationBag bag )
        {
            Rock.Web.SystemSettings.SetValue( SystemSetting.BIBLE_STRICT_SPOUSE, bag.EnableBibleStrictSpouse.ToString() );

            return ActionOk( GetSuccessResponseBag( "Settings saved successfully." ) );
        }

        /// <summary>
        /// Revokes the security grants.
        /// </summary>
        /// <returns></returns>
        [BlockAction( "RevokeSecurityGrants" )]
        public BlockActionResult RevokeSecurityGrants()
        {
            Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.SECURITY_GRANT_TOKEN_EARLIEST_DATE, RockDateTime.Now.ToString( "O" ) );

            return ActionOk( GetSuccessResponseBag( "All existing security grant tokens have been revoked." ) );
        }

        #endregion Block Actions
    }
}