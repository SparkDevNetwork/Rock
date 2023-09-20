using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using com.blueboxmoon.AcmeCertificate;

namespace RockWeb.Plugins.com_blueboxmoon.AcmeCertificate
{
    [DisplayName( "Acme Certificates" )]
    [Category( "Blue Box Moon > Acme Certificate" )]
    [Description( "Lists the certificate configuration." )]

    [LinkedPage( "Detail Page", order: 0 )]
    [TextField( "Redirect Override", "If you enter a value here it will be used as the redirect URL for Acme Challenges to other sites instead of the automatically determined one.", false, order: 1 )]
    public partial class AcmeCertificates : RockBlock, ISecondaryBlock
    {
        #region Base Control Method Overrides

        /// <summary>
        /// Handles the OnInit event of the block.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gCertificates.Actions.ShowAdd = true;
            gCertificates.Actions.AddClick += gCertificates_AddClick;
        }

        /// <summary>
        /// Handles the OnLoad event of the block.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                var account = AcmeHelper.LoadAccountData();

                pnlCertificates.Visible = !string.IsNullOrWhiteSpace( account.Key );

                BindGrid();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Bind the grid of certificates that are configured in the system.
        /// </summary>
        protected void BindGrid()
        {
            var rockContext = new RockContext();

            var groupTypeId = GroupTypeCache.Get( com.blueboxmoon.AcmeCertificate.SystemGuid.GroupType.ACME_CERTIFICATES ).Id;

            var groups = new GroupService( rockContext ).Queryable()
                .Where( g => g.GroupTypeId == groupTypeId );


            if ( gCertificates.SortProperty != null )
            {
                groups = groups.Sort( gCertificates.SortProperty );
            }
            else
            {
                groups = groups.OrderBy( g => g.Name );
            }


            var groupList = groups.ToList();
            groupList.ForEach( g => g.LoadAttributes( rockContext ) );
            var data = groupList.Select( g => new
            {
                g.Id,
                g.Name,
                LastRenewed = g.GetAttributeValue( "LastRenewed" ),
                Expires = g.GetAttributeValue( "Expires" ),
                Domains = string.Join( "<br />", g.GetAttributeValues( "Domains" ) )
            } );

            gCertificates.DataSource = data.ToList();
            gCertificates.DataBind();
        }

        /// <summary>
        /// Called when another block on the page requests secondary blocks to hide or become visible.
        /// </summary>
        /// <param name="visible">True if this block should become visible.</param>
        public void SetVisible( bool visible )
        {
            var account = AcmeHelper.LoadAccountData();

            pnlCertificates.Visible = visible && !string.IsNullOrWhiteSpace( account.Key );
        }

        #endregion

        #region Event Methods

        /// <summary>
        /// Handles the GridRebind event of the gCertificates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gCertificates_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gCertificates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gCertificates_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "Id", 0 );
        }

        /// <summary>
        /// Handles the Delete event of the gCertificates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCertificates_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );

            var group = groupService.Get( e.RowKeyId );
            groupService.Delete( group );

            rockContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the gCertificates control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCertificates_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "Id", e.RowKeyId );
        }

        #endregion
    }
}
