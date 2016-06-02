// <copyright>
// Copyright by Central Christian Church
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
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.Widgets
{
    [DisplayName( "Take Profile Photo" )]
    [Category( "com_centralaz > widgets" )]
    [Description( "Allows a profile picture to be uploaded using a device's camera (if authorized)." )]

    public partial class TakeProfilePhoto : Rock.Web.UI.RockBlock
    {
        #region Base ControlMethods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            RockPage.AddCSSLink( ResolveRockUrl( "~/Plugins/com_centralaz/Widgets/Styles/TakeProfilePhoto.css" ) );
            RockPage.AddScriptLink( "~/Plugins/com_centralaz/Widgets/Scripts/exif.js" );
            RockPage.AddScriptLink( "~/Plugins/com_centralaz/Widgets/Scripts/binaryajax.js" );

            string cameraIcon = ResolveRockUrl( "~/Plugins/com_centralaz/Widgets/Assets/Icons/camera-icon-128x128.png" );
            HtmlMeta metaDescription = new HtmlMeta();
            metaDescription.Name = "viewport";
            metaDescription.Content = "width=device-width, initial-scale=0.75, user-scalable=no";
            RockPage.Header.Controls.Add( metaDescription );

            RockPage.Header.Controls.Add( new LiteralControl( string.Format( "<link rel=\"icon\" sizes=\"128x128\" href=\"{0}\">", cameraIcon) ) );
            RockPage.Header.Controls.Add( new LiteralControl( string.Format( "<link rel=\"apple-touch-icon\" sizes=\"128x128\" href=\"{0}\">", cameraIcon  ) ) );
            RockPage.Header.Controls.Add( new LiteralControl( string.Format( "<link rel=\"apple-touch-icon-precomposed\" sizes=\"128x128\" href=\"{0}\">", cameraIcon ) ) );
            //Page.Header.Controls.Add( new LiteralControl( "<meta name=\"mobile-web-app-capable\" content=\"yes\">" ) );

            if ( CurrentPerson == null )
            {
                divLoginWarning.Visible = true;
                divLoginWarning.Attributes.Add( "class", "alert alert-error" );
            }
            else
            {
                divLoginWarning.Attributes.Add( "class", "muted" );
                divLoginWarning.Visible = true;
                divLoginWarning.InnerHtml = string.Format( "Greetings {0} <small>(otherwise, please logout)</small>", CurrentPerson.FullName );
            }

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        #endregion

        #region Events
        
        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            //  BindData();
        }

        protected void btnUpload_Click( object sender, EventArgs e )
        {
         //   var dataUrl = canvas;
        }

        #endregion

        #region Methods

        #endregion
    }
}