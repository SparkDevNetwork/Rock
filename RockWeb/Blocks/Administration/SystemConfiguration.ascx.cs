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
using System.IO;
using System.Linq;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Configuration;
using Microsoft.Web.XmlTransform;
using Rock.SystemKey;
using System.Threading.Tasks;

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
        #region Fields

        // used for private variables

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
            BindTimeZones();

            BindOtherAppSettings();
            BindMaxFileSize();

            BindExperimentalSettings();
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

            rockWebConfig.Save();

            if ( !SaveMaxAllowedContentLength() )
            {
                nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbMessage.Title = "Error";
                nbMessage.Text = "An error occurred which prevented the 'MaxAllowedContentLength' to be saved in the web.config";
            }
            else
            {
                nbMessage.NotificationBoxType = NotificationBoxType.Success;
                nbMessage.Title = "Success";
                nbMessage.Text = "You will need to reload this page to continue.";
            }
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
                // requestLengthDiskThreshold is in bytes and the MaxRequestLength must not be less than this value.
                //numbMaxSize.MinimumValue = Math.Round(section.RequestLengthDiskThreshold * 10.48576, 0 ).ToString();
                //numbMaxSize.ToolTip = string.Format( "between {0} and {1} MB", section.RequestLengthDiskThreshold, numbMaxSize.MaximumValue );
            }
        }

        /// <summary>
        /// Transform the web.config to inject the maximum allowed content length
        /// into the requestLimits tag of the requestFiltering section of the web.config.
        /// </summary>
        /// <returns>true if the transform was successful; false otherwise.</returns>
        protected bool SaveMaxAllowedContentLength()
        {
            string webConfig = System.Web.HttpContext.Current.Server.MapPath( Path.Combine( "~", "web.config" ) );
            bool isSuccess = false;

            using ( XmlTransformableDocument document = new XmlTransformableDocument() )
            {
                document.PreserveWhitespace = true;
                document.Load( webConfig );

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

                using ( XmlTransformation transform = new XmlTransformation( transformString, false, null ) )
                {
                    isSuccess = transform.Apply( document );
                    document.Save( webConfig );
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
                nbStartDayOfWeekSaveMessage.Text = string.Format("Start Day of Week is now set to <strong>{0}</strong>. ", dowpStartingDayOfWeek.SelectedDayOfWeek.ConvertToString());
            }
        }
    }
}