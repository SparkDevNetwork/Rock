// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web.UI;
using Rock;
using Rock.Model;
using Rock.Web.UI;


namespace RockWeb.Plugins.com_mineCartStudio.MinePass
{
    /// <summary>
    /// Displays the details of the given Wistia Account.
    /// </summary>
    [DisplayName( "Mine Pass Settings" )]
    [Category( "Mine Cart Studio > Mine Pass" )]
    [Description( "Configures the settings for the Mine Pass service." )]
    public partial class MinPassSettings : RockBlock
    {
        private string _apiSettingKey = null;
        
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _apiSettingKey = com.minecartstudio.MinePass.Client.SystemKey.SystemSetting.MINE_CART_API_KEY;
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
                ShowPanel();
            }

        }

        #endregion

        #region Events



        #endregion

        #region Methods



        #endregion

        private void ShowPanel()
        {
            var apiKey = Rock.Web.SystemSettings.GetValue( _apiSettingKey );

            pnlEdit.Visible = false;
            pnlStatus.Visible = false;

            if ( apiKey.IsNullOrWhiteSpace() )
            {
                pnlEdit.Visible = true;
                this.HideSecondaryBlocks( true );
            }
            else
            {
                pnlStatus.Visible = true;
                this.HideSecondaryBlocks( false );
            }
        }
        
        protected void btnSaveApiKey_Click( object sender, EventArgs e )
        {
            Rock.Web.SystemSettings.SetValue( _apiSettingKey, txtApiKey.Text );

            ShowPanel();
        }

        protected void btnEditSettings_Click( object sender, EventArgs e )
        {
            var apiKey = Rock.Web.SystemSettings.GetValue( _apiSettingKey );
            txtApiKey.Text = apiKey;

            pnlStatus.Visible = false;
            pnlEdit.Visible = true;
        }
    }
}