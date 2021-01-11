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
using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Utility;
using Rock.Utility.Settings.SparkData;
using Rock.Utility.SparkDataApi;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Tasks;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// Spark Data Settings
    /// </summary>
    [DisplayName( "Spark Data Settings" )]
    [Category( "Administration" )]
    [Description( "Block used to set values specific to Spark Data (NCOA, Etc)." )]
    public partial class SparkDataSettings : RockBlock
    {
        #region private variables

        private SparkDataConfig _sparkDataConfig = new SparkDataConfig();

        #endregion

        #region Base Control Methods

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
            dvpNcoaPersonDataView.EntityTypeId = EntityTypeCache.GetId<Rock.Model.Person>();

            var inactiveRecordReasonDt = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON.AsGuid() );
            dvpNcoaInactiveRecordReason.DefinedTypeId = inactiveRecordReasonDt.Id;
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
                BindControls();
                GetSettings();
            }
        }

        #endregion

        #region Events

        #region Spark Data Events

        /// <summary>
        /// Handles the BlockUpdated event of the SystemConfiguration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        /// <summary>
        /// Handles the Click event of the btnSaveLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveLogin_Click( object sender, EventArgs e )
        {
            // Get Spark Data
            _sparkDataConfig = Ncoa.GetSettings();

            _sparkDataConfig.GlobalNotificationApplicationGroupId = grpNotificationGroupLogin.GroupId;
            _sparkDataConfig.SparkDataApiKey = txtSparkDataApiKeyLogin.Text;

            Rock.Web.SystemSettings.SetValue( SystemSetting.SPARK_DATA, _sparkDataConfig.ToJson() );

            GetSettings();
        }

        /// <summary>
        /// Handles the Click event of the btnUpdateSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdateSettings_Click( object sender, EventArgs e )
        {
            pnlSparkDataEdit.Visible = true;
            pnlSignIn.Visible = false;
            pnlAccountStatus.Visible = false;
            pwNcoaConfiguration.Visible = false;
            bbtnNcoaSaveConfig.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the btnCancelEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelEdit_Click( object sender, EventArgs e )
        {
            GetSettings();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveEdit_Click( object sender, EventArgs e )
        {
            // Get Spark Data
            _sparkDataConfig = Ncoa.GetSettings();

            _sparkDataConfig.GlobalNotificationApplicationGroupId = grpNotificationGroupEdit.GroupId;
            _sparkDataConfig.SparkDataApiKey = txtSparkDataApiKeyEdit.Text;

            Rock.Web.SystemSettings.SetValue( SystemSetting.SPARK_DATA, _sparkDataConfig.ToJson() );

            GetSettings();
        }

        #endregion

        #region NCOA Events

        /// <summary>
        /// Handles saving all the data set by the user to the web.config.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void bbtnNcoaSaveConfig_Click( object sender, EventArgs e )
        {
            SaveSettings();
        }

        /// <summary>
        /// Handles the CheckedChanged event when enabling/disabling the NCOA option.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void cbNcoaConfiguration_CheckedChanged( object sender, EventArgs e )
        {
            _sparkDataConfig = Ncoa.GetSettings();

            _sparkDataConfig.NcoaSettings.IsEnabled = cbNcoaConfiguration.Checked;

            Rock.Web.SystemSettings.SetValue( SystemSetting.SPARK_DATA, _sparkDataConfig.ToJson() );

            // Save job active status
            using ( var rockContext = new RockContext() )
            {
                var ncoaJob = new ServiceJobService( rockContext ).Get( Rock.SystemGuid.ServiceJob.GET_NCOA.AsGuid() );
                if ( ncoaJob != null )
                {
                    ncoaJob.IsActive = cbNcoaConfiguration.Checked;
                    rockContext.SaveChanges();
                }
            }

            SetPanels();
        }

        /// <summary>
        /// Handles the CheckedChanged event when enabling/disabling the recurring enabled control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbNcoaRecurringEnabled_CheckedChanged( object sender, EventArgs e )
        {
            nbNcoaRecurrenceInterval.Enabled = cbNcoaRecurringEnabled.Checked;

            bbtnNcoaSaveConfig.Enabled = true;
            SetStartNcoaEnabled();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbNcoaAcceptTerms control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbNcoaAcceptTerms_CheckedChanged( object sender, EventArgs e )
        {
            // Update Spark Data settings
            _sparkDataConfig = Ncoa.GetSettings();

            _sparkDataConfig.NcoaSettings.IsAcceptedTerms = cbNcoaAcceptTerms.Checked;
            Rock.Web.SystemSettings.SetValue( SystemSetting.SPARK_DATA, _sparkDataConfig.ToJson() );

            // Update if Run Manually button is enabled
            SetStartNcoaEnabled();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbNcoaAckPrice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbNcoaAckPrice_CheckedChanged( object sender, EventArgs e )
        {
            // Update Spark Data settings
            _sparkDataConfig = Ncoa.GetSettings();

            _sparkDataConfig.NcoaSettings.IsAckPrice = cbNcoaAckPrice.Checked;
            Rock.Web.SystemSettings.SetValue( SystemSetting.SPARK_DATA, _sparkDataConfig.ToJson() );

            // Update if Run Manually button is enabled
            SetStartNcoaEnabled();
        }

        /// <summary>
        /// Handles the Click event of the btnStartNcoa control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnStartNcoa_Click( object sender, EventArgs e )
        {
            var addresses = new Ncoa().GetAddresses( dvpNcoaPersonDataView.SelectedValue.AsIntegerOrNull() );
            if ( addresses == null || addresses.Count < SparkDataConfig.NCOA_MIN_ADDRESSES )
            {
                mdGridWarning.Show( string.Format( "Only {0} addresses were selected to be processed. NCOA will not run because it is below the minimum of {1} addresses.", addresses.Count, SparkDataConfig.NCOA_MIN_ADDRESSES ), ModalAlertType.Information );
            }
            else
            {
                lbNcoaCount.Text = addresses.Count.ToString() + " Addresses will be processed by NCOA.";
                mdRunNcoa.Show();
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the mdRunNcoa control. This is the Start NCOA dialog. Save = Start
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdRunNcoa_SaveClick( object sender, EventArgs e)
        {
            Ncoa ncoa = new Ncoa();
            var sparkDataConfig = Ncoa.GetSettings();
            sparkDataConfig.NcoaSettings.PersonFullName = CurrentPerson != null ? CurrentPerson.FullName : null;
            sparkDataConfig.NcoaSettings.CurrentReportStatus = "Start";
            Ncoa.SaveSettings( sparkDataConfig );
            using ( RockContext rockContext = new RockContext() )
            {
                ServiceJob job = new ServiceJobService( rockContext ).Get( Rock.SystemGuid.ServiceJob.GET_NCOA.AsGuid() );
                if ( job != null )
                {
                    new ProcessRunJobNow.Message { JobId = job.Id }.Send();

                    mdGridWarning.Show( string.Format( "The '{0}' job has been started.", job.Name ), ModalAlertType.Information );
                    lbStartNcoa.Enabled = false;
                }
            }

            mdRunNcoa.Hide();
        }

        /// <summary>
        /// Handles the TextChanged event of the nbNcoaMinMoveDistance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void nbNcoaMinMoveDistance_TextChanged( object sender, EventArgs e )
        {
            SetNcoaEditNotSaved();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbNcoa48MonAsPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbNcoa48MonAsPrevious_CheckedChanged( object sender, EventArgs e )
        {
            SetNcoaEditNotSaved();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbNcoaInvalidAddressAsPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbNcoaInvalidAddressAsPrevious_CheckedChanged( object sender, EventArgs e )
        {
            SetNcoaEditNotSaved();
        }

        /// <summary>
        /// Handles the TextChanged event of the nbNcoaRecurrenceInterval control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void nbNcoaRecurrenceInterval_TextChanged( object sender, EventArgs e )
        {
            SetNcoaEditNotSaved();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the dvpNcoaInactiveRecordReason control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dvpNcoaInactiveRecordReason_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetNcoaEditNotSaved();
        }

        /// <summary>
        /// Handles the SelectItem event of the dvpNcoaPersonDataView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dvpNcoaPersonDataView_SelectItem( object sender, EventArgs e )
        {
            SetNcoaEditNotSaved();
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Binds the controls.
        /// </summary>
        private void BindControls()
        {
        }

        /// <summary>
        /// Enables the data automation panels and sets the titles.
        /// </summary>
        private void SetPanels()
        {
            SetPanel( pwNcoaConfiguration, pnlNcoaConfiguration, "National Change of Address (NCOA)", cbNcoaConfiguration.Checked );
            SetStartNcoaEnabled();
        }

        /// <summary>
        /// Enables a data automation panel and sets the title.
        /// </summary>
        /// <param name="panelWidget">The panel widget.</param>
        /// <param name="panel">The panel.</param>
        /// <param name="title">The title.</param>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        private void SetPanel( PanelWidget panelWidget, Panel panel, string title, bool enabled )
        {
            panel.Enabled = enabled;
            var enabledLabel = string.Empty;
            if ( enabled )
            {
                enabledLabel = "<span class='label label-success'>Enabled</span> <span class='label label-success'>$</span>";
            }
            else
            {
                enabledLabel = "<span class='label label-warning'>Disabled</span> <span class='label label-warning'>$</span>";
            }

            panelWidget.Title = string.Format( "<h3 class='panel-title pull-left margin-r-sm'>{0}</h3> {1} ", title, enabledLabel );
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        private void GetSettings()
        {
            // Get Spark Data settings
            _sparkDataConfig = Ncoa.GetSettings();
            if ( _sparkDataConfig.SparkDataApiKey.IsNullOrWhiteSpace() )
            {
                pnlSparkDataEdit.Visible = false;
                pnlSignIn.Visible = true;
                pnlAccountStatus.Visible = false;
                pwNcoaConfiguration.Visible = false;
                bbtnNcoaSaveConfig.Visible = false;

                txtSparkDataApiKeyLogin.Text = _sparkDataConfig.SparkDataApiKey;
                grpNotificationGroupLogin.GroupId = _sparkDataConfig.GlobalNotificationApplicationGroupId;
            }
            else
            {
                pnlSparkDataEdit.Visible = false;
                pnlSignIn.Visible = false;
                pnlAccountStatus.Visible = true;
                pwNcoaConfiguration.Visible = true;
                bbtnNcoaSaveConfig.Visible = true;

                // Get NCOA configuration settings
                nbNcoaMinMoveDistance.Text = Rock.Web.SystemSettings.GetValue( SystemSetting.NCOA_MINIMUM_MOVE_DISTANCE_TO_INACTIVATE );
                cbNcoa48MonAsPrevious.Checked = Rock.Web.SystemSettings.GetValue( SystemSetting.NCOA_SET_48_MONTH_AS_PREVIOUS ).AsBoolean();
                cbNcoaInvalidAddressAsPrevious.Checked = Rock.Web.SystemSettings.GetValue( SystemSetting.NCOA_SET_INVALID_AS_PREVIOUS ).AsBoolean();

                txtSparkDataApiKeyEdit.Text = _sparkDataConfig.SparkDataApiKey;
                grpNotificationGroupEdit.GroupId = _sparkDataConfig.GlobalNotificationApplicationGroupId;

                // Get NCOA settings
                if ( _sparkDataConfig.NcoaSettings == null )
                {
                    _sparkDataConfig.NcoaSettings = new NcoaSettings();
                }

                dvpNcoaPersonDataView.SetValue( _sparkDataConfig.NcoaSettings.PersonDataViewId );
                cbNcoaRecurringEnabled.Checked = _sparkDataConfig.NcoaSettings.RecurringEnabled;
                nbNcoaRecurrenceInterval.Enabled = _sparkDataConfig.NcoaSettings.RecurringEnabled;
                nbNcoaRecurrenceInterval.Text = _sparkDataConfig.NcoaSettings.RecurrenceInterval.ToStringSafe();
                cbNcoaAcceptTerms.Checked = _sparkDataConfig.NcoaSettings.IsAcceptedTerms;
                cbNcoaAckPrice.Checked = _sparkDataConfig.NcoaSettings.IsAckPrice;
                if ( _sparkDataConfig.NcoaSettings.InactiveRecordReasonId.HasValue )
                {
                    dvpNcoaInactiveRecordReason.SetValue( _sparkDataConfig.NcoaSettings.InactiveRecordReasonId.Value );
                }

                nbNcoaCreditCard.Visible = false;

                if ( _sparkDataConfig.NcoaSettings.CurrentReportStatus == null )
                {
                    _sparkDataConfig.NcoaSettings.CurrentReportStatus = string.Empty;
                }

                if ( _sparkDataConfig.SparkDataApiKey.IsNullOrWhiteSpace() )
                {
                    pnlSignIn.Visible = true;
                    pnlSparkDataEdit.Visible = false;

                }
                else
                {
                    pnlSignIn.Visible = false;
                    bool accountValid = false;

                    SparkDataApi sparkDataApi = new SparkDataApi();
                    try
                    {
                        var accountStatus = sparkDataApi.CheckAccount( _sparkDataConfig.SparkDataApiKey );
                        switch ( accountStatus )
                        {
                            case SparkDataApi.AccountStatus.AccountNoName:
                                hlAccountStatus.LabelType = LabelType.Warning;
                                hlAccountStatus.Text = "Account does not have a name";
                                break;
                            case SparkDataApi.AccountStatus.AccountNotFound:
                                hlAccountStatus.LabelType = LabelType.Warning;
                                hlAccountStatus.Text = "Account not found";
                                break;
                            case SparkDataApi.AccountStatus.Disabled:
                                hlAccountStatus.LabelType = LabelType.Warning;
                                hlAccountStatus.Text = "Disabled";
                                break;
                            case SparkDataApi.AccountStatus.EnabledCardExpired:
                                hlAccountStatus.LabelType = LabelType.Danger;
                                hlAccountStatus.Text = "Enabled - Card expired";
                                break;
                            case SparkDataApi.AccountStatus.EnabledNoCard:
                                hlAccountStatus.LabelType = LabelType.Warning;
                                hlAccountStatus.Text = "Enabled - No card on file";
                                nbNcoaCreditCard.Visible = true;
                                break;
                            case SparkDataApi.AccountStatus.EnabledCard:
                                hlAccountStatus.LabelType = LabelType.Success;
                                hlAccountStatus.Text = "Enabled - Card on file";
                                accountValid = true;
                                break;
                            case SparkDataApi.AccountStatus.InvalidSparkDataKey:
                                hlAccountStatus.LabelType = LabelType.Warning;
                                hlAccountStatus.Text = "Invalid Spark Data Key";
                                break;
                            case SparkDataApi.AccountStatus.EnabledCardNoExpirationDate:
                                hlAccountStatus.LabelType = LabelType.Warning;
                                hlAccountStatus.Text = "Enabled - Card expiration date not on file";
                                break;
                        }

                        string cost = sparkDataApi.GetPrice( "CF20766E-80F9-E282-432F-6A9E19F0BFF1" );
                        cbNcoaAckPrice.Text = cbNcoaAckPrice.Text.Replace( "$xx", "$" + cost );
                    }
                    catch
                    {
                        hlAccountStatus.LabelType = LabelType.Danger;
                        hlAccountStatus.Text = "Error connecting to Spark server";
                    }

                    cbNcoaConfiguration.Checked = _sparkDataConfig.NcoaSettings.IsEnabled && accountValid;
                    cbNcoaConfiguration.Enabled = accountValid;
                    SetStartNcoaEnabled();
                    SetPanels();
                }
            }
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        private void SaveSettings()
        {
            // NCOA Configuration
            Rock.Web.SystemSettings.SetValue( SystemSetting.NCOA_MINIMUM_MOVE_DISTANCE_TO_INACTIVATE, nbNcoaMinMoveDistance.Text );
            Rock.Web.SystemSettings.SetValue( SystemSetting.NCOA_SET_48_MONTH_AS_PREVIOUS, cbNcoa48MonAsPrevious.Checked.ToString() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.NCOA_SET_INVALID_AS_PREVIOUS, cbNcoaInvalidAddressAsPrevious.Checked.ToString() );

            // Get Spark Data
            _sparkDataConfig = Ncoa.GetSettings();

            _sparkDataConfig.NcoaSettings.PersonDataViewId = dvpNcoaPersonDataView.SelectedValue.AsIntegerOrNull();
            _sparkDataConfig.NcoaSettings.RecurringEnabled = cbNcoaRecurringEnabled.Checked;
            _sparkDataConfig.NcoaSettings.RecurrenceInterval = nbNcoaRecurrenceInterval.Text.AsInteger();
            _sparkDataConfig.NcoaSettings.IsEnabled = cbNcoaConfiguration.Checked;
            _sparkDataConfig.NcoaSettings.IsAckPrice = cbNcoaAckPrice.Checked;
            _sparkDataConfig.NcoaSettings.IsAcceptedTerms = cbNcoaAcceptTerms.Checked;
            _sparkDataConfig.NcoaSettings.InactiveRecordReasonId = dvpNcoaInactiveRecordReason.SelectedValueAsId();

            Rock.Web.SystemSettings.SetValue( SystemSetting.SPARK_DATA, _sparkDataConfig.ToJson() );

            bbtnNcoaSaveConfig.Enabled = false;
            SetStartNcoaEnabled();
        }

        #region NCOA Methods

        /// <summary>
        /// Sets the Run Manually start NCOA button enabled state.
        /// </summary>
        private void SetStartNcoaEnabled()
        {
            if ( _sparkDataConfig == null )
            {
                _sparkDataConfig = Ncoa.GetSettings();
            }

            if ( _sparkDataConfig.NcoaSettings.CurrentReportStatus.Contains( "Pending" ) )
            {
                lbStartNcoa.Enabled = false;
            }
            else
            {
                lbStartNcoa.Enabled = cbNcoaAcceptTerms.Checked &&
                    cbNcoaAckPrice.Checked &&
                    cbNcoaConfiguration.Checked &&
                    !bbtnNcoaSaveConfig.Enabled &&
                    _sparkDataConfig.NcoaSettings.IsValid();
            }
        }

        /// <summary>
        /// Sets the NCOA state to edit not saved.
        /// </summary>
        private void SetNcoaEditNotSaved()
        {
            bbtnNcoaSaveConfig.Enabled = true;
            SetStartNcoaEnabled();
        }

        #endregion

        #endregion
    }
}