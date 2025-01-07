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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Utility.Settings.DataAutomation;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// Communication Settings.
    /// </summary>
    [DisplayName( "Communication Settings" )]
    [Category( "Communication" )]
    [Description( "Block used to set values specific to communication." )]
    [Rock.SystemGuid.BlockTypeGuid( "ED6447A6-F7E0-4680-BFD1-B45527C17156" )]
    public partial class CommunicationSettings : RockBlock
    {
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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindApprovalEmailTemplates();
            }

            base.OnLoad( e );
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
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            SaveSettings();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Bind the email templates
        /// </summary>
        private void BindApprovalEmailTemplates()
        {
            var SystemCommunications = new SystemCommunicationService( new RockContext() ).Queryable().OrderBy( e => e.Title );

            ddlApprovalEmailTemplate.Items.Clear();

            if ( SystemCommunications.Any() )
            {
                foreach ( var SystemCommunication in SystemCommunications )
                {
                    ddlApprovalEmailTemplate.Items.Add( new ListItem( SystemCommunication.Title, SystemCommunication.Guid.ToString() ) );
                }
            }

            ddlApprovalEmailTemplate.SetValue( Rock.Web.SystemSettings.GetValue( SystemSetting.COMMUNICATION_SETTING_APPROVAL_TEMPLATE ).AsGuidOrNull() );
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        private void SaveSettings()
        {
            // Save General
            Rock.Web.SystemSettings.SetValue( SystemSetting.COMMUNICATION_SETTING_APPROVAL_TEMPLATE, ddlApprovalEmailTemplate.SelectedValue );
        }

        #endregion
    }
}