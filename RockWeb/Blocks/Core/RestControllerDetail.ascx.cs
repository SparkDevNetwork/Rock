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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Web.UI;
using Rock;
using Rock.Attribute;
using System.Web.Http.Controllers;

namespace RockWeb.Blocks.Administration
{
    [DisplayName( "Rest Controller Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of the given rest controller." )]

    [LinkedPage("Detail Page")]
    public partial class RestControllerDetail : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                string itemId = PageParameter( "controllerType" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "controllerType", itemId );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, string itemKeyValue )
        {
            var config = GlobalConfiguration.Configuration;

            HttpControllerDescriptor controllerDescription = config.Services.GetHttpControllerSelector().GetControllerMapping().Where( a => a.Value.ControllerType.FullName == itemKeyValue ).Select( s => s.Value).FirstOrDefault();

            lReadOnlyTitle.Text = controllerDescription.ControllerName.FormatAsHtmlTitle();
            lDescription.Text = controllerDescription.ControllerType.FullName;


            var explorer = config.Services.GetApiExplorer();

            // list Actions for the selected control
            var apiList = explorer.ApiDescriptions
                .Where( a => a.ActionDescriptor.ControllerDescriptor.ControllerType.FullName == itemKeyValue )
                .OrderBy( a => a.HttpMethod.Method ).ThenBy( a => a.RelativePath )
                .Select( a => new
                {
                    a.ID,
                    a.HttpMethod,
                    a.RelativePath,
                    a.ActionDescriptor.ControllerDescriptor
                })
                .ToList();

            gApiActions.DataSource = apiList;
            gApiActions.DataBind();

            // show detail of the controller

        }

        /// <summary>
        /// Handles the RowSelected event of the gApiActions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gApiActions_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {

        }
}
}