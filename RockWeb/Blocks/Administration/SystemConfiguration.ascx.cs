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
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

using Microsoft.Web.XmlTransform;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Observability;
using Rock.SystemKey;
using Rock.Web.Cache.NonEntities;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    [DisplayName( "System Configuration" )]
    [Category( "Administration" )]
    [Description( "Used for making configuration changes to configurable items in the web.config." )]
    [Rock.SystemGuid.BlockTypeGuid( "E2D423B8-10F0-49E2-B2A6-D62892379429" )]
    public partial class SystemConfiguration : Rock.Web.UI.RockBlock
    {
        #region Defaults

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

        #endregion Defaults

        #region Fields

        private static readonly string WebConfigPath = System.Web.HttpContext.Current.Server.MapPath( Path.Combine( "~", "web.config" ) );

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowDetails();
            }

            lTitle.Text = ( "Edit System Configuration" ).FormatAsHtmlTitle();

            base.OnLoad( e );
        }

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            BindGeneralConfiguration();
            BindTimeZones();
            BindOtherAppSettings();
            BindMaxFileSize();
            BindLoginCookieTimeout();
            BindExperimentalSettings();
            BindObservabilitySettings();
            BindSystemDiagnosticsSettings();
            BindUiSettings();
            BindFamilyRulesSettings();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the SystemConfiguration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            // nothing here yet...
        }

        /// <summary>
        /// Handles saving the general configuration set by the user to the database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnGeneralSave_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            nbGeneralMessage.Visible = true;

            // Save General
            Rock.Web.SystemSettings.SetValue( SystemSetting.ENABLE_MULTI_TIME_ZONE_SUPPORT, cbEnableMultipleTimeZone.Checked.ToString() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.ALWAYS_SHOW_BUSINESS_IN_PERSONPICKER, cbIncludeBusinessInPersonPicker.Checked.ToString() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.ENABLE_KEEP_ALIVE, cbEnableKeepAlive.Checked.ToString() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.PDF_EXTERNAL_RENDER_ENDPOINT, tbPDFExternalRenderEndpoint.Text );
            Rock.Web.SystemSettings.SetValue( SystemSetting.VISITOR_COOKIE_PERSISTENCE_DAYS, nbVisitorCookiePersistenceLengthDays.Text );
            Rock.Web.SystemSettings.SetValue( SystemSetting.PERSONALIZATION_SEGMENT_COOKIE_AFFINITY_DURATION_MINUTES, nbPersonalizationCookieCacheLengthMinutes.Text );

            nbGeneralMessage.NotificationBoxType = NotificationBoxType.Success;
            nbGeneralMessage.Title = string.Empty;
            nbGeneralMessage.Text = "Setting saved successfully.";
        }

        /// <summary>
        /// Handles saving all the data set by the user to the web.config.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnSaveConfig_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            nbMessage.Visible = true;

            Configuration rockWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration( "~" );
            rockWebConfig.AppSettings.Settings["OrgTimeZone"].Value = ddTimeZone.SelectedValue;
            rockWebConfig.AppSettings.Settings["RunJobsInIISContext"].Value = cbRunJobsInIISContext.Checked.ToString();
            rockWebConfig.AppSettings.Settings["AzureSignalREndpoint"].Value = rtbAzureSignalREndpoint.Text;
            rockWebConfig.AppSettings.Settings["AzureSignalRAccessKey"].Value = rtbAzureSignalRAccessKey.Text;
            rockWebConfig.AppSettings.Settings["ObservabilityServiceName"].Value = tbObservabilityServiceName.Text;

            var section = ( System.Web.Configuration.SystemWebSectionGroup ) rockWebConfig.GetSectionGroup( "system.web" );
            section.HttpRuntime.MaxRequestLength = int.Parse( numbMaxSize.Text ) * 1024;
            section.Authentication.Forms.Timeout = TimeSpan.FromMinutes( numLoginCookieTimeout.IntegerValue ?? SettingDefault.LoginCookieTimeoutMinutes );

            rockWebConfig.Save();

            string errorMessage = null;
            string errorMessageTemplate = "An error occurred which prevented the {0} from being saved within the web.config.";

            if ( !SaveMaxAllowedContentLength() )
            {
                errorMessage = string.Format( errorMessageTemplate, "'MaxAllowedContentLength'" );
            }

            if ( errorMessage == null && !SaveSystemDiagnosticsSettings() )
            {
                errorMessage = string.Format( errorMessageTemplate, "system.diagnostics configuration" );
            }

            if ( !string.IsNullOrEmpty( errorMessage ) )
            {
                nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbMessage.Title = "Error";
                nbMessage.Text = errorMessage;
            }
            else
            {
                nbMessage.NotificationBoxType = NotificationBoxType.Success;
                nbMessage.Title = "Success";
                nbMessage.Text = "You will need to reload this page to continue.";
            }
        }

        /// <summary>
        /// Handles saving the UI settings configuration
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnUiSettingSave_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            nbUiSettings.Visible = true;

            // Save Race and Ethnicity label values
            Rock.Web.SystemSettings.SetValue( SystemSetting.PERSON_RACE_LABEL, rtbPersonRaceLabel.Text );
            Rock.Web.SystemSettings.SetValue( SystemSetting.PERSON_ETHNICITY_LABEL, rtbPersonEthnicityLabel.Text );

            // Save Captcha keys
            Rock.Web.SystemSettings.SetValue( SystemSetting.CAPTCHA_SITE_KEY, rtbCaptchaSiteKey.Text );
            Rock.Web.SystemSettings.SetValue( SystemSetting.CAPTCHA_SECRET_KEY, rtbCaptchaSecretKey.Text );

            Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.SMS_OPT_IN_MESSAGE_LABEL, rtbSmsOptInMessage.Text );

            nbUiSettings.NotificationBoxType = NotificationBoxType.Success;
            nbUiSettings.Title = string.Empty;
            nbUiSettings.Text = "Settings saved successfully.";
        }

        protected void btnFamilyRules_Click( object sender, EventArgs e )
        {
            Rock.Web.SystemSettings.SetValue( SystemSetting.BIBLE_STRICT_SPOUSE, cbBibleStrictSpouse.Checked.ToString() );

            nbFamilyRulesMessage.NotificationBoxType = NotificationBoxType.Success;
            nbFamilyRulesMessage.Visible = true;
            nbFamilyRulesMessage.Title = string.Empty;
            nbFamilyRulesMessage.Text = "Settings saved successfully.";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the controls for the Observability settings
        /// </summary>
        private void BindObservabilitySettings()
        {
            ddlEndpointProtocol.BindToEnum<OpenTelemetry.Exporter.OtlpExportProtocol>();
            ddlEndpointProtocol.SelectedValue = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENDPOINT_PROTOCOL );

            cbEnableObservaility.Checked = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENABLED ).AsBoolean();

            urlObservabilityEndpoint.Text = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENDPOINT );

            kvlEndpointHeaders.Value = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_ENDPOINT_HEADERS );

            nbObservabilitySpanCountLimit.IntegerValue = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_SPAN_COUNT_LIMIT ).AsIntegerOrNull();

            nbObservabilityMaxAttributeLength.IntegerValue = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_MAX_ATTRIBUTE_LENGTH ).AsIntegerOrNull();

            cbObservabilityIncludeQueryStatements.Checked = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_INCLUDE_QUERY_STATEMENTS ).AsBoolean();

            vlTargetedQueries.Value = Rock.Web.SystemSettings.GetValue( SystemSetting.OBSERVABILITY_TARGETED_QUERIES );
        }

        /// <summary>
        /// Bind thee general configuration
        /// </summary>
        private void BindGeneralConfiguration()
        {
            cbEnableMultipleTimeZone.Checked = Rock.Web.SystemSettings.GetValue( SystemSetting.ENABLE_MULTI_TIME_ZONE_SUPPORT ).AsBoolean();
            cbIncludeBusinessInPersonPicker.Checked = Rock.Web.SystemSettings.GetValue( SystemSetting.ALWAYS_SHOW_BUSINESS_IN_PERSONPICKER ).AsBoolean();
            cbEnableKeepAlive.Checked = Rock.Web.SystemSettings.GetValue( SystemSetting.ENABLE_KEEP_ALIVE ).AsBoolean();
            tbPDFExternalRenderEndpoint.Text = Rock.Web.SystemSettings.GetValue( SystemSetting.PDF_EXTERNAL_RENDER_ENDPOINT );
            nbVisitorCookiePersistenceLengthDays.Text = ( Rock.Web.SystemSettings.GetValue( SystemSetting.VISITOR_COOKIE_PERSISTENCE_DAYS ).AsIntegerOrNull() ?? SettingDefault.VisitorCookieTimeoutDays ).ToString();
            nbPersonalizationCookieCacheLengthMinutes.Text = ( Rock.Web.SystemSettings.GetValue( SystemSetting.PERSONALIZATION_SEGMENT_COOKIE_AFFINITY_DURATION_MINUTES ).AsIntegerOrNull() ?? SettingDefault.PersonalizationCookieCacheLengthMinutes ).ToString();
        }

        /// <summary>
        /// Bind the available time zones and select the one that's configured in the
        /// web.config's OrgTimeZone setting.
        /// </summary>
        private void BindTimeZones()
        {
            foreach ( TimeZoneInfo timeZone in TimeZoneInfo.GetSystemTimeZones() )
            {
                ddTimeZone.Items.Add( new ListItem( timeZone.DisplayName, timeZone.Id ) );
            }

            ddTimeZone.SelectedValue = RockDateTime.OrgTimeZoneInfo.Id;
        }

        /// <summary>
        /// Bind the other settings that are in the appSettings section of the web.config.
        /// </summary>
        private void BindOtherAppSettings()
        {
            string runJobsInIISContext = ConfigurationManager.AppSettings["RunJobsInIISContext"];
            if ( !string.IsNullOrEmpty( runJobsInIISContext ) )
            {
                cbRunJobsInIISContext.Checked = bool.Parse( runJobsInIISContext );
            }
            rtbAzureSignalREndpoint.Text = ConfigurationManager.AppSettings["AzureSignalREndpoint"];
            rtbAzureSignalRAccessKey.Text = ConfigurationManager.AppSettings["AzureSignalRAccessKey"];
            tbObservabilityServiceName.Text = ConfigurationManager.AppSettings["ObservabilityServiceName"];
        }

        /// <summary>
        /// Bind the MaxRequestLength and maxAllowedContentLength values from the web.config
        /// into the number boxes on the form.
        /// </summary>
        private void BindMaxFileSize()
        {
            HttpRuntimeSection section = ConfigurationManager.GetSection( "system.web/httpRuntime" ) as HttpRuntimeSection;
            if ( section != null )
            {
                // MaxRequestLength is in KB, so let's convert to MB for the users sake.
                numbMaxSize.Text = ( section.MaxRequestLength / 1024 ).ToString();
            }
        }

        /// <summary>
        /// Binds the login cookie timeout.
        /// </summary>
        private void BindLoginCookieTimeout()
        {
            var rockWebConfig = WebConfigurationManager.OpenWebConfiguration( "~" );
            var systemWebSection = ( SystemWebSectionGroup ) rockWebConfig.GetSectionGroup( "system.web" );

            if ( systemWebSection == null )
            {
                numLoginCookieTimeout.IntegerValue = SettingDefault.LoginCookieTimeoutMinutes;
                return;
            }

            numLoginCookieTimeout.IntegerValue = ( int ) systemWebSection.Authentication.Forms.Timeout.TotalMinutes;
        }

        /// <summary>
        /// Binds the system diagnostics settings.
        /// </summary>
        private void BindSystemDiagnosticsSettings()
        {
            cbEnableAdoNetPerformanceCounters.Checked = Rock.Web.SystemSettings.GetValue( SystemSetting.SYSTEM_DIAGNOSTICS_ENABLE_ADO_NET_PERFORMANCE_COUNTERS ).AsBoolean();
        }

        /// <summary>
        /// Transform the web.config to inject the maximum allowed content length
        /// into the requestLimits tag of the requestFiltering section of the web.config.
        /// </summary>
        /// <returns>true if the transform was successful; false otherwise.</returns>
        protected bool SaveMaxAllowedContentLength()
        {
            int maxContentLengthBytes = int.Parse( numbMaxSize.Text ) * 1048576;

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

            return TransformWebConfig( transformString );
        }

        /// <summary>
        /// Transform the web.config to inject system.diagnostics configuration and save related system settings.
        /// </summary>
        /// <returns>true if the transform was successful; false otherwise.</returns>
        private bool SaveSystemDiagnosticsSettings()
        {
            var adoNetPerformanceCountersAreEnabled = cbEnableAdoNetPerformanceCounters.Checked;

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

            if ( !TransformWebConfig( transformSb.ToString() ) )
            {
                return false;
            }

            // Update the "core_EnableAdoNetPerformanceCounters" System Setting
            Rock.Web.SystemSettings.SetValue( SystemSetting.SYSTEM_DIAGNOSTICS_ENABLE_ADO_NET_PERFORMANCE_COUNTERS, adoNetPerformanceCountersAreEnabled.ToString() );

            // Toggle the "Collect Hosting Metrics" ServiceJob's IsActive status if necessary
            var rockContext = new RockContext();
            var serviceJob = new ServiceJobService( rockContext ).Get( Rock.SystemGuid.ServiceJob.COLLECT_HOSTING_METRICS.AsGuid() );

            if ( serviceJob != null && serviceJob.IsActive != adoNetPerformanceCountersAreEnabled )
            {
                serviceJob.IsActive = !serviceJob.IsActive;

                rockContext.SaveChanges();
            }

            return true;
        }

        /// <summary>
        /// Transform the web.config given the supplied transform string.
        /// </summary>
        /// <param name="transformString">The transform string used to instantiate a new <see cref="XmlTransformation"/>.</param>
        /// <returns>true if the transform was successful; false otherwise.</returns>
        private bool TransformWebConfig( string transformString )
        {
            bool isSuccess = false;

            using ( XmlTransformableDocument document = new XmlTransformableDocument() )
            {
                document.PreserveWhitespace = true;
                document.Load( WebConfigPath );

                using ( XmlTransformation transform = new XmlTransformation( transformString, false, null ) )
                {
                    isSuccess = transform.Apply( document );
                    document.Save( WebConfigPath );
                }
            }

            return isSuccess;
        }

        /// <summary>
        /// Binds the experimental settings.
        /// </summary>
        protected void BindExperimentalSettings()
        {
            nbStartDayOfWeekSaveMessage.NotificationBoxType = NotificationBoxType.Warning;
            nbStartDayOfWeekSaveMessage.Title = string.Empty;
            nbStartDayOfWeekSaveMessage.Text = "This is an experimental setting. Saving this will change how SundayDate is calculated and will also update existing data that keeps track of 'SundayDate'.";
            dowpStartingDayOfWeek.SelectedDayOfWeek = RockDateTime.FirstDayOfWeek;

            nbSecurityGrantTokenDuration.IntegerValue = Math.Max( Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.DEFAULT_SECURITY_GRANT_TOKEN_DURATION )?.AsIntegerOrNull() ?? 4320, 60 );
        }

        /// <summary>
        /// Binds the UI settings
        /// </summary>
        private void BindUiSettings()
        {
            rtbPersonRaceLabel.Text = Rock.Web.SystemSettings.GetValue( SystemSetting.PERSON_RACE_LABEL );
            rtbPersonEthnicityLabel.Text = Rock.Web.SystemSettings.GetValue( SystemSetting.PERSON_ETHNICITY_LABEL );

            rtbCaptchaSiteKey.Text = Rock.Web.SystemSettings.GetValue( SystemSetting.CAPTCHA_SITE_KEY );
            rtbCaptchaSecretKey.Text = Rock.Web.SystemSettings.GetValue( SystemSetting.CAPTCHA_SECRET_KEY );

            rtbSmsOptInMessage.Text = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.SMS_OPT_IN_MESSAGE_LABEL );
        }

        /// <summary>
        /// Binds the Family Rules Settings
        /// </summary>
        private void BindFamilyRulesSettings()
        {
            cbBibleStrictSpouse.Checked = Rock.Web.SystemSettings.GetValue( SystemSetting.BIBLE_STRICT_SPOUSE ).AsBoolean( true );
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the btnSaveExperimental control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveExperimental_Click( object sender, EventArgs e )
        {
            if ( dowpStartingDayOfWeek.SelectedDayOfWeek != RockDateTime.FirstDayOfWeek )
            {
                Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.START_DAY_OF_WEEK, dowpStartingDayOfWeek.SelectedDayOfWeek.ConvertToInt().ToString() );
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

                nbStartDayOfWeekSaveMessage.NotificationBoxType = NotificationBoxType.Success;
                nbStartDayOfWeekSaveMessage.Title = string.Empty;
                nbStartDayOfWeekSaveMessage.Text = string.Format( "Start Day of Week is now set to <strong>{0}</strong>. ", dowpStartingDayOfWeek.SelectedDayOfWeek.ConvertToString() );
            }

            var oldSecurityGrantTokenDuration = Math.Max( Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.DEFAULT_SECURITY_GRANT_TOKEN_DURATION ).AsInteger(), 60 );
            var newSecurityGrantTokenDuration = Math.Max( nbSecurityGrantTokenDuration.IntegerValue ?? 0, 60 );

            if ( oldSecurityGrantTokenDuration != newSecurityGrantTokenDuration )
            {
                Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.DEFAULT_SECURITY_GRANT_TOKEN_DURATION, newSecurityGrantTokenDuration.ToString() );
                nbSecurityGrantTokenDurationSaveMessage.Text = "Security grant token duration has been successfully updated.";
                nbSecurityGrantTokenDurationSaveMessage.Visible = true;
            }
        }

        protected void btnRevokeSecurityGrants_Click( object sender, EventArgs e )
        {
            Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.SECURITY_GRANT_TOKEN_EARLIEST_DATE, RockDateTime.Now.ToString( "O" ) );
            nbSecurityGrantTokenDurationSaveMessage.Text = "All existing security grant tokens have been revoked.";
            nbSecurityGrantTokenDurationSaveMessage.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnObservabilitySave control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnObservabilitySave_Click( object sender, EventArgs e )
        {
            if ( cbEnableObservaility.Checked && urlObservabilityEndpoint.Text.IsNullOrWhiteSpace() )
            {
                nbObservabilityMessages.NotificationBoxType = NotificationBoxType.Warning;
                nbObservabilityMessages.Text = "To enable observability, please provide a valid service endpoint. (e.g. https://otlp.nr-data.net:4317)";
                return;
            }

            Rock.Web.SystemSettings.SetValue( SystemSetting.OBSERVABILITY_ENABLED, cbEnableObservaility.Checked.ToString() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.OBSERVABILITY_ENDPOINT_PROTOCOL, ddlEndpointProtocol.SelectedValue );
            Rock.Web.SystemSettings.SetValue( SystemSetting.OBSERVABILITY_ENDPOINT_HEADERS, kvlEndpointHeaders.Value );
            Rock.Web.SystemSettings.SetValue( SystemSetting.OBSERVABILITY_ENDPOINT, urlObservabilityEndpoint.Text );
            Rock.Web.SystemSettings.SetValue( SystemSetting.OBSERVABILITY_SPAN_COUNT_LIMIT, nbObservabilitySpanCountLimit.Text );
            Rock.Web.SystemSettings.SetValue( SystemSetting.OBSERVABILITY_MAX_ATTRIBUTE_LENGTH, nbObservabilityMaxAttributeLength.Text );
            Rock.Web.SystemSettings.SetValue( SystemSetting.OBSERVABILITY_INCLUDE_QUERY_STATEMENTS, cbObservabilityIncludeQueryStatements.Checked.ToString() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.OBSERVABILITY_TARGETED_QUERIES, vlTargetedQueries.Value );

            nbObservabilityMessages.NotificationBoxType = NotificationBoxType.Success;
            nbObservabilityMessages.Title = string.Empty;
            nbObservabilityMessages.Text = "Settings saved successfully.";

            // Clear the targeted query hash cache
            DbCommandObservabilityCache.ClearTargetedQueryHashes();

            // Update the provider
            ObservabilityHelper.ConfigureObservability();
        }

        #endregion


    }
}