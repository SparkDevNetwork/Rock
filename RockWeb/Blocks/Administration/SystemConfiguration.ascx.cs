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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Web.XmlTransform;

using Rock;
using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.SystemKey;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "System Configuration" )]
    [Category( "Administration" )]
    [Description( "Used for making configuration changes to configurable items in the web.config." )]
    public partial class SystemConfiguration : Rock.Web.UI.RockBlock
    {
        #region Defaults

        /// <summary>
        /// Default setting values
        /// </summary>
        private static class SettingDefault
        {
            /// <summary>
            /// The cookie timeout
            /// </summary>
            public const int CookieTimeout = 43200;
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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetails();
            }

            lTitle.Text = ( "Edit System Configuration" ).FormatAsHtmlTitle();
        }

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            BindGeneralConfiguration();

            BindLoggingSettings();

            BindTimeZones();

            BindOtherAppSettings();
            BindMaxFileSize();
            BindCookieTimeout();

            BindExperimentalSettings();

            BindSystemDiagnosticsSettings();
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

            var section = ( System.Web.Configuration.SystemWebSectionGroup ) rockWebConfig.GetSectionGroup( "system.web" );
            section.HttpRuntime.MaxRequestLength = int.Parse( numbMaxSize.Text ) * 1024;
            section.Authentication.Forms.Timeout = TimeSpan.FromMinutes( numCookieTimeout.IntegerValue ?? SettingDefault.CookieTimeout );

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

        protected void btnLoggingSave_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            nbLoggingMessage.Visible = true;

            var logConfig = new RockLogSystemSettings
            {
                LogLevel = rblVerbosityLevel.SelectedValue.ConvertToEnum<RockLogLevel>( RockLogLevel.Off ),
                DomainsToLog = cblDomainsToLog.SelectedValues,
                MaxFileSize = txtMaxFileSize.Text.AsInteger(),
                NumberOfLogFiles = txtFilesToRetain.Text.AsInteger()
            };

            Rock.Web.SystemSettings.SetValue( SystemSetting.ROCK_LOGGING_SETTINGS, logConfig.ToJson() );

            nbLoggingMessage.NotificationBoxType = NotificationBoxType.Success;
            nbLoggingMessage.Title = string.Empty;
            nbLoggingMessage.Text = "Setting saved successfully.";
        }

        protected void btnLoggingFlush_Click( object sender, EventArgs e )
        {
            nbLoggingMessage.Visible = true;

            RockLogger.Log.Close();

            nbLoggingMessage.NotificationBoxType = NotificationBoxType.Success;
            nbLoggingMessage.Title = string.Empty;
            nbLoggingMessage.Text = "The buffered logs were successfully flushed out to the log file.";
        }
        #endregion

        #region Methods

        /// <summary>
        /// Bind thee general configuration
        /// </summary>
        private void BindGeneralConfiguration()
        {
            cbEnableMultipleTimeZone.Checked = Rock.Web.SystemSettings.GetValue( SystemSetting.ENABLE_MULTI_TIME_ZONE_SUPPORT ).AsBoolean();
        }

        private void BindLoggingSettings()
        {
            var logLevel = Enum.GetNames( typeof( RockLogLevel ) );
            rblVerbosityLevel.DataSource = logLevel;
            rblVerbosityLevel.DataBind();

            var rockConfig = Rock.Web.SystemSettings.GetValue( SystemSetting.ROCK_LOGGING_SETTINGS ).FromJsonOrNull<RockLogSystemSettings>();

            if ( rockConfig == null )
            {
                return;
            }

            rblVerbosityLevel.SelectedValue = rockConfig.LogLevel.ToString();
            txtFilesToRetain.Text = rockConfig.NumberOfLogFiles.ToString();
            txtMaxFileSize.Text = rockConfig.MaxFileSize.ToString();

            var definedValues = new DefinedValueService( new Rock.Data.RockContext() ).GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.LOGGING_DOMAINS.AsGuid() );

            cblDomainsToLog.DataSource = definedValues.ToList();
            cblDomainsToLog.DataTextField = "Value";
            cblDomainsToLog.DataValueField = "Value";
            cblDomainsToLog.DataBind();

            cblDomainsToLog.SetValues( rockConfig.DomainsToLog );
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
        /// Binds the cookie timeout.
        /// </summary>
        private void BindCookieTimeout()
        {
            var rockWebConfig = WebConfigurationManager.OpenWebConfiguration( "~" );
            var systemWebSection = ( SystemWebSectionGroup ) rockWebConfig.GetSectionGroup( "system.web" );

            if ( systemWebSection == null )
            {
                numCookieTimeout.IntegerValue = SettingDefault.CookieTimeout;
                return;
            }

            numCookieTimeout.IntegerValue = ( int ) systemWebSection.Authentication.Forms.Timeout.TotalMinutes;
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
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the btnSaveStartDayOfWeek control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveStartDayOfWeek_Click( object sender, EventArgs e )
        {
            if ( dowpStartingDayOfWeek.SelectedDayOfWeek != RockDateTime.FirstDayOfWeek )
            {
                Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.START_DAY_OF_WEEK, dowpStartingDayOfWeek.SelectedDayOfWeek.ConvertToInt().ToString() );
                Task.Run( () =>
                {
                    try
                    {
                        RockDateTime.UpdateSundayDateData();
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
        }
    }
}