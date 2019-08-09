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
using System.Web.UI;
using Rock.Attribute;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.PersonProfile;
using Rock.Security;

namespace RockWeb.Blocks.Crm
{
    [DisplayName( "Badge List" )]
    [Category( "CRM" )]
    [Description( "Shows a list of all badges." )]

    [LinkedPage( "Detail Page" )]
    public partial class BadgeList : RockBlock, ICustomGridColumns
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gBadge.DataKeyNames = new string[] { "Id" };
            gBadge.Actions.ShowAdd = true;
            gBadge.Actions.AddClick += gBadge_Add;
            gBadge.GridReorder += gBadge_GridReorder;
            gBadge.GridRebind += gBadge_GridRebind;
            gBadge.RowItemText = "Badge";

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gBadge.Actions.ShowAdd = canAddEditDelete;
            gBadge.IsDeleteEnabled = canAddEditDelete;

            var securityField = gBadge.ColumnsOfType<SecurityField>().FirstOrDefault();
            if ( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Badge ) ).Id;
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
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events (main grid)

        /// <summary>
        /// Handles the Add event of the gBadge control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gBadge_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "BadgeId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gBadge control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gBadge_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "BadgeId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gBadge control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gBadge_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var badgeService = new BadgeService( rockContext );
            var badge = badgeService.Get( e.RowKeyId );

            if ( badge != null )
            {
                string errorMessage;
                if ( !badgeService.CanDelete( badge, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                badgeService.Delete( badge );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        void gBadge_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var service = new BadgeService( rockContext );
            var badges = service.Queryable().OrderBy( b => b.Order );
            service.Reorder( badges.ToList(), e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gBadge control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gBadge_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            gBadge.DataSource = new BadgeService( new RockContext() )
                .Queryable().OrderBy( b => b.Order ).ToList();
            gBadge.DataBind();
        }

        #endregion
    }
}