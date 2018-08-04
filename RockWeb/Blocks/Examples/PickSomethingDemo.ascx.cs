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
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.UI;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Examples
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Pick Something Demo" )]
    [Category( "Examples" )]
    [Description( "Demonstrates using the Rock:ItemFromBlockPicker" )]
    public partial class PickSomethingDemo : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            //pExample.BlockTypePath = "~/Blocks/Utility/PickSomething.ascx";
            pModalExample.PickerBlock.PickerSettings["HeaderText"] = "Hello World 1234";

            pPickFromFileBrowser.PickerBlock.PickerSettings["RootFolder"] = "~/Content";
        }

        /// <summary>
        /// Handles the SelectItem event of the pModalExample control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void pModalExample_SelectItem( object sender, EventArgs e )
        {
            lModalPickedResult.Text = "You Picked: " + pModalExample.SelectedValue;
        }

        /// <summary>
        /// Handles the SelectItem event of the pInlineExample control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void pInlineExample_SelectItem( object sender, EventArgs e )
        {

        }

        protected void pPickFromFileBrowser_SelectItem( object sender, EventArgs e )
        {
            lFileBrowserPickedResult.Text = "You Picked: " + pPickFromFileBrowser.SelectedValue;
        }
    }
}
