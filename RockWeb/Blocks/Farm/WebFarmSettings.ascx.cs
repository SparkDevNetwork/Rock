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
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Bus.Queue;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Web;
using Rock.Web.UI;
using Rock.WebFarm;

namespace RockWeb.Blocks.Farm
{
    [DisplayName( "Web Farm Settings" )]
    [Category( "Farm" )]
    [Description( "Displays the details of the Web Farm." )]

    public partial class WebFarmSettings : RockBlock, IDetailBlock
    {
        #region Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string QueueKey = "QueueKey";
        }

        #endregion Keys

        #region View State

        /// <summary>
        /// Gets or sets a value indicating whether this instance is edit mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is edit mode; otherwise, <c>false</c>.
        /// </value>
        private bool IsEditMode
        {
            get
            {
                return CanEdit() && ViewState["IsEditMode"].ToStringSafe().AsBoolean();
            }
            set
            {
                ViewState["IsEditMode"] = value;
            }
        }

        #endregion View State

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            RockPage.AddScriptLink( "~/Scripts/Chartjs/Chart.min.js", true );

            InitializeSettingsNotification();
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
                RenderState();
            }
        }

        /// <summary>
        /// Initialize handlers for block configuration changes.
        /// </summary>
        /// <param name="triggerPanel"></param>
        private void InitializeSettingsNotification()
        {
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upUpdatePanel );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            IsEditMode = CanEdit();
            RenderState();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            SaveRecord();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            IsEditMode = false;
            RenderState();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            RenderState();
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Save the current record.
        /// </summary>
        /// <returns></returns>
        private void SaveRecord()
        {
            SystemSettings.SetValue( SystemSetting.WEBFARM_IS_ENABLED, cbIsActive.Checked.ToString() );
            SystemSettings.SetValue( SystemSetting.WEBFARM_KEY, tbWebFarmKey.Text );

            SystemSettings.SetValue(
                SystemSetting.WEBFARM_LEADERSHIP_POLLING_INTERVAL_LOWER_LIMIT_SECONDS,
                ( nbPollingMin.IntegerValue ?? RockWebFarm.DefaultValue.DefaultLeadershipPollingIntervalLowerLimitSeconds ).ToString() );

            SystemSettings.SetValue(
                SystemSetting.WEBFARM_LEADERSHIP_POLLING_INTERVAL_UPPER_LIMIT_SECONDS,
                ( nbPollingMax.IntegerValue ?? RockWebFarm.DefaultValue.DefaultLeadershipPollingIntervalUpperLimitSeconds ).ToString() );

            IsEditMode = false;
            RenderState();
        }

        /// <summary>
        /// This method satisfies the IDetailBlock requirement
        /// </summary>
        /// <param name="unused"></param>
        public void ShowDetail( int unused )
        {
            RenderState();
        }

        /// <summary>
        /// Shows the controls needed
        /// </summary>
        public void RenderState()
        {
            nbEditModeMessage.Text = string.Empty;

            var isEnabled = SystemSettings.GetValue( Rock.SystemKey.SystemSetting.WEBFARM_IS_ENABLED ).AsBoolean();
            var hasValidKey = Rock.WebFarm.RockWebFarm.HasValidKey();

            if ( isEnabled && hasValidKey )
            {
                hlActive.Text = "Active";
                hlActive.LabelType = Rock.Web.UI.Controls.LabelType.Success;
            }
            else
            {
                hlActive.Text = "Inactive";
                hlActive.LabelType = Rock.Web.UI.Controls.LabelType.Danger;
            }

            if ( IsEditMode )
            {
                ShowEditMode();
            }
            else if ( IsViewMode() )
            {
                ShowViewMode();
            }
        }

        /// <summary>
        /// Shows the mode where the user can edit an existing queue
        /// </summary>
        private void ShowEditMode()
        {
            if ( !IsEditMode )
            {
                return;
            }

            pnlEditDetails.Visible = true;
            pnlViewDetails.Visible = false;
            HideSecondaryBlocks( true );

            cbIsActive.Checked = SystemSettings.GetValue( SystemSetting.WEBFARM_IS_ENABLED ).AsBoolean();
            tbWebFarmKey.Text = SystemSettings.GetValue( SystemSetting.WEBFARM_KEY );
            nbPollingMin.IntegerValue = RockWebFarm.GetLowerPollingLimitSeconds();
            nbPollingMax.IntegerValue = RockWebFarm.GetUpperPollingLimitSeconds();
        }

        /// <summary>
        /// Shows the mode where the user is only viewing an existing streak type
        /// </summary>
        private void ShowViewMode()
        {
            if ( !IsViewMode() )
            {
                return;
            }

            var canEdit = CanEdit();
            btnEdit.Visible = canEdit;

            pnlEditDetails.Visible = false;
            pnlViewDetails.Visible = true;
            HideSecondaryBlocks( false );

            // Load values from system settings
            var minPolling = RockWebFarm.GetLowerPollingLimitSeconds();
            var maxPolling = RockWebFarm.GetUpperPollingLimitSeconds();

            var maskedKey = SystemSettings.GetValue( SystemSetting.WEBFARM_KEY ).Masked();

            if ( maskedKey.IsNullOrWhiteSpace() )
            {
                maskedKey = "None";
            }

            // Build the description list with the values
            var descriptionList = new DescriptionList();
            descriptionList.Add( "Key", string.Format( "{0}", maskedKey ) );
            descriptionList.Add( "Min Polling Limit", string.Format( "{0} seconds", minPolling ) );
            descriptionList.Add( "Max Polling Limit", string.Format( "{0} seconds", maxPolling ) );

            // Bind the grid data view models
            using ( var rockContext = new RockContext() )
            {
                var service = new WebFarmNodeService( rockContext );
                var query = service.Queryable().Select( wfn => new
                {
                    PollingIntervalSeconds = wfn.CurrentLeadershipPollingIntervalSeconds,
                    IsJobRunner = wfn.IsCurrentJobRunner,
                    IsActive = wfn.IsActive,
                    IsLeader = wfn.IsLeader,
                    NodeName = wfn.NodeName,
                    LastSeen = wfn.LastSeenDateTime
                } );

                rNodes.DataSource = query.ToList();
                rNodes.DataBind();
            }

            lDescription.Text = descriptionList.Html;
        }

        #endregion Internal Methods

        #region State Determining Methods

        /// <summary>
        /// Can the user edit the streak type
        /// </summary>
        /// <returns></returns>
        private bool CanEdit()
        {
            return UserCanAdministrate;
        }

        /// <summary>
        /// Is the block currently showing information about a streak type
        /// </summary>
        /// <returns></returns>
        private bool IsViewMode()
        {
            return !IsEditMode;
        }

        #endregion State Determining Methods
    }
}