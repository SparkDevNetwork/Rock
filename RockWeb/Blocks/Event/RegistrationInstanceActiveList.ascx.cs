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

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Attribute;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Block to display active Registration Instances.
    /// </summary>
    [DisplayName( "Registration Instance Active List" )]
    [Category( "Event" )]
    [Description( "Block to display active Registration Instances." )]

    [LinkedPage( "Detail Page" )]
    public partial class RegistrationInstanceActiveList : Rock.Web.UI.RockBlock
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gRegInstances.DataKeyNames = new string[] { "Id" };
            gRegInstances.GridRebind += gRegInstances_GridRebind;

            // hide this block if it determines it's on the event detail page
            if ( RockPage.PageParameter( "RegistrationTemplateId" ).IsNotNullOrWhitespace() || RockPage.PageParameter( "CategoryId" ).IsNotNullOrWhitespace() )
            {
                this.Visible = false;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindInstancesGrid();
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gRegInstances control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gRegInstances_GridRebind( object sender, EventArgs e )
        {
            BindInstancesGrid();
        }

        /// <summary>
        /// Handles the Edit event of the gInstances control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gRegInstances_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "RegistrationInstanceId", e.RowKeyId );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the instances grid.
        /// </summary>
        protected void BindInstancesGrid()
        {

            pnlInstances.Visible = true;

            using ( var rockContext = new RockContext() )
            {
                RegistrationInstanceService instanceService = new RegistrationInstanceService( rockContext );
                var qry = instanceService.Queryable()
                    .Where( i =>
                        i.StartDateTime <= RockDateTime.Now &&
                        i.EndDateTime > RockDateTime.Now &&
                        i.IsActive )
                    .OrderBy( i => i.StartDateTime );

                var sortProperty = gRegInstances.SortProperty;
                if ( sortProperty != null )
                {
                    qry = qry.Sort( sortProperty );
                }

                var qryResult = qry
                    .Select( i => new
                    {
                        i.Id,
                        i.Guid,
                        i.Name,
                        i.StartDateTime,
                        i.EndDateTime,
                        i.IsActive,
                        i.Details,
                        Registrants = i.Registrations.Where( r => !r.IsTemporary ).SelectMany( r => r.Registrants ).Count()
                    } );

                gRegInstances.SetLinqDataSource( qryResult );
                gRegInstances.DataBind();
            }

        }

        #endregion

    }
}