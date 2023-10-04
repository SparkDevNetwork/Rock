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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using com.minecartstudio.MinePass.Client;
using Rock;
using Rock.Transactions;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;
using com.minecartstudio.MinePass.Client.MinePassApi;
using com.minecartstudio.MinePass.Common;
using com.minecartstudio.MinePassCommon.Relevance;
using com.minecartstudio.MinePass.Common.Fields;

namespace RockWeb.Plugins.com_mineCartStudio.MinePass
{
    /// <summary>
    /// Displays the details of the given Wistia Account.
    /// </summary>
    [DisplayName( "Pass Tester" )]
    [Category( "Mine Cart Studio > Mine Pass" )]
    [Description( "Displays the details of the given Wistia Account." )]
    public partial class PassTester : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
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
                var pass = new MinePassApple();
                pass.OrganizationName = "Spark Development Network";
                pass.Description = "My sample description";
                pass.LogoText = "SparkDevNetwork";
                pass.ShowBarcode = true;
                pass.BarcodeValue = "http://rockrms.com";
                pass.BarCodeType = com.minecartstudio.MinePass.Common.Enums.MinePassAppleBarcodeType.Qr;
                pass.LogoUrl = "https://rockrms.blob.core.windows.net/rx2018/logo.png";
                pass.LogoRetinaUrl = "https://rockrms.blob.core.windows.net/rx2018/logo@2x.png";
                pass.IconUrl = "https://rockrms.blob.core.windows.net/rx2018/icon.png";
                pass.IconRetinaUrl = "https://rockrms.blob.core.windows.net/rx2018/icon@2x.png";
                //pass.LogoText = "RX 2018";
                pass.BackgroundColor = "#ee7725";
                pass.LabelColor = "#fed2b4";
                pass.ForegroundColor = "#ffffff";
                pass.PassStyle = com.minecartstudio.MinePass.Common.Enums.MinePassAppleStyle.Generic;

                var passLocationPanda = new MinePassAppleRelevantLocation();
                passLocationPanda.Latitude = "33.672603";
                passLocationPanda.Longitude = "-112.238160";
                passLocationPanda.Message = "Panda Village! Be careful of the baby ones.";
                pass.RelevantLocations.Add( passLocationPanda );

                var passLocationHome = new MinePassAppleRelevantLocation();
                passLocationHome.Latitude = "33.710206";
                passLocationHome.Longitude = "-112.282889";
                passLocationHome.Message = "Home Base! Relax you deserve it.";
                pass.RelevantLocations.Add( passLocationHome );

                var passLocationWork = new MinePassAppleRelevantLocation();
                passLocationWork.Latitude = "33.639827";
                passLocationWork.Longitude = "-112.279412";
                passLocationWork.Message = "Spark Development Network Global Headquarters! It's time for business.";
                pass.RelevantLocations.Add( passLocationWork );

                var fieldName = new MinePassAppleField();
                fieldName.Label = "Name";
                fieldName.Value = "Ted Decker";
                pass.PrimaryFields.Add( fieldName );

                var fieldOrganization = new MinePassAppleField();
                fieldOrganization.Label = "Organization";
                fieldOrganization.Value = "Spark Development Network";
                pass.SecondaryFields.Add( fieldOrganization );

                var fieldBack = new MinePassAppleField();
                fieldBack.Label = "Organization";
                fieldBack.Value = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque scelerisque lectus urna, ut pellentesque orci varius vitae. Curabitur viverra ornare tellus quis condimentum. Duis sit amet erat sodales, mollis quam nec, maximus velit. Pellentesque condimentum bibendum varius. Cras molestie dui ipsum, vitae condimentum leo interdum sed. Mauris finibus ante vel dictum vulputate. Aenean placerat varius augue nec maximus. Praesent quis auctor risus. Vivamus condimentum ac ipsum vitae suscipit. Etiam tincidunt, eros tincidunt aliquam gravida, massa augue posuere ante, eu rhoncus metus orci id leo. ";
                pass.BackFields.Add( fieldBack );

                cePassJson.Text = pass.ToJson( true );
            }

        }

        #endregion

        #region Events



        #endregion

        #region Methods



        #endregion

        protected void lbUploadPass_Click( object sender, EventArgs e )
        {
            var serialNumber = MinePassApiManager.UpdatePass( tbApiKey.Text, tbSerialNumber.Text, tbPassPerson.Text, tbTemplate.Text, cePassJson.Text );
            tbSerialNumber.Text = serialNumber;
        }
    }
}