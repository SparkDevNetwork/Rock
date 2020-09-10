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
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Examples
{
    /// <summary>
    /// A sample block that uses many of the Rock Modal controls.
    /// </summary>
    [DisplayName( "Rock Modal Gallery" )]
    [Category( "Examples" )]
    [Description( "Allows you to see and try various Rock Modal controls." )]

    public partial class RockModalGallery : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            mdModalDialog.SaveClick += mdModalDialog_SaveClick;
        }

        /// <summary>
        /// Handles the SaveClick event of the mdModalDialog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void mdModalDialog_SaveClick( object sender, EventArgs e )
        {
            mdModalDialog.Hide();
        }

        /// <summary>
        /// Handles the ServerClick event of the btnModalAlert control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnModalAlert_ServerClick( object sender, EventArgs e )
        {
            maModalAlert.Show( "This is the message", ModalAlertType.Alert );
        }

        /// <summary>
        /// Handles the ServerClick event of the btnModalDialog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnModalDialog_ServerClick( object sender, EventArgs e )
        {
            mdModalDialog.Show();
        }

        /// <summary>
        /// Handles the ServerClick event of the btnModalDialogNested control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnModalDialogNested_ServerClick( object sender, EventArgs e )
        {
            mdModalDialogNested.Show();
        }

        /// <summary>
        /// Handles the ServerClick event of the btnModalAlertNested control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnModalAlertNested_ServerClick( object sender, EventArgs e )
        {
            maModalAlert.Show( "This is the nested message", ModalAlertType.Warning );
        }
    }
}