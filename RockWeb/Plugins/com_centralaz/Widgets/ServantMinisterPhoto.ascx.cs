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
using System.Linq;
using System.Web.UI;
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

    [DisplayName( "Servant Minister Photo" )]
    [Category( "com_centralaz > widgets" )]
    [Description( "Allows servant ministers to upload a profile picture." )]

    [BooleanField( "Enable Debug", "Display a list of merge fields available for lava.", false, "", 5 )]

    public partial class ServantMinisterPhoto : Rock.Web.UI.RockBlock
    {
        #region Base ControlMethods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {

                // BindData();

            }

        }


        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            //  BindData();
        }

        #endregion

        #region Events

        #endregion

        #region Methods


        #endregion

        protected void btnUpload_Click( object sender, EventArgs e )
        {
         //   var dataUrl = canvas;
        }
    }
}