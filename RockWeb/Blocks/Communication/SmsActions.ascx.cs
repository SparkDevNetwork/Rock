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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Communication.SmsActions;

namespace RockWeb.Blocks.Communication
{
    [DisplayName( "SMS Actions" )]
    [Category( "Communication" )]
    [Description( "Configures the SMS Actions that run when an incoming message is received." )]
    public partial class SmsActions : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/dragula.min.js" );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindComponents();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the AddClick event of the Actions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Actions_AddClick( object sender, EventArgs e )
        {
        }

        #endregion

        #region Internal Methods

        private void BindComponents()
        {
            var components = SmsActionContainer.Instance.Components.Values
                .Select( a => a.Value )
                .Select( a => new
                {
                    a.Title,
                    a.Description,
                    a.IconCssClass,
                    Id = a.TypeName
                } );

            rptrComponents.DataSource = components;
            rptrComponents.DataBind();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
        }

        #endregion

        protected void rptrComponents_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var actionComponent = SmsActionContainer.GetComponent( e.CommandArgument.ToString() );

            if ( actionComponent != null )
            {
                var rockContext = new RockContext();
                var smsActionService = new SmsActionService( rockContext );

                int? lastOrder = smsActionService.Queryable()
                    .OrderByDescending( a => a.Order )
                    .Select( a => a.Order )
                    .FirstOrDefault();

                var action = new SmsAction
                {
                    Title = actionComponent.Title,
                    IsActive = true,
                    Order = lastOrder ?? 0,
                    SmsActionComponentEntityTypeId = actionComponent.TypeId
                };

                smsActionService.Add( action );
                rockContext.SaveChanges();
            }
        }
    }
}