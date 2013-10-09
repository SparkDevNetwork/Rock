//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [GroupField( "Group", "Either pick a specific group or choose <none> to have group be determined by the siteId page parameter" )]
    [LinkedPage("Detail Page")]
    public partial class LayoutList : RockBlock, IDimmableBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gLayouts.DataKeyNames = new string[] { "Id" };
            gLayouts.Actions.AddClick += gLayouts_AddClick;
            gLayouts.Actions.ShowAdd = true;
            gLayouts.IsDeleteEnabled = true;
            gLayouts.GridRebind += gLayouts_GridRebind;

            SecurityField securityField = gLayouts.Columns[4] as SecurityField;
            securityField.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Layout ) ).Id;
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
                BindLayoutsGrid();
            }
        }

        #endregion

        #region Layouts Grid

        /// <summary>
        /// Handles the Click event of the DeleteLayout control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void DeleteLayout_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                LayoutService layoutService = new LayoutService();
                Layout layout = layoutService.Get( e.RowKeyId );
                if ( layout != null )
                {
                    string errorMessage;
                    if ( !layoutService.CanDelete( layout, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    int siteId = layout.SiteId;

                    layoutService.Delete( layout, CurrentPersonId );
                    layoutService.Save( layout, CurrentPersonId );

                    LayoutCache.Flush( e.RowKeyId );
                }
            } );

            BindLayoutsGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gLayouts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gLayouts_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "layoutId", 0, "siteId", hfSiteId.ValueAsInt() );
        }

        /// <summary>
        /// Handles the Edit event of the gLayouts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gLayouts_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "layoutId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the GridRebind event of the gLayouts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gLayouts_GridRebind( object sender, EventArgs e )
        {
            BindLayoutsGrid();
        }

        /// <summary>
        /// Binds the group members grid.
        /// </summary>
        protected void BindLayoutsGrid()
        {
            pnlLayouts.Visible = false;

            int siteId = PageParameter( "siteId" ).AsInteger() ?? 0;
            if ( siteId == 0 )
            {
                // quit if the siteId can't be determined
                return;
            }

            hfSiteId.SetValue( siteId );

            pnlLayouts.Visible = true;

            LayoutService layoutService = new LayoutService();

            // Add any missing layouts
            layoutService.RegisterLayouts( Request.MapPath( "~" ), SiteCache.Read( siteId ), CurrentPersonId );

            var qry = layoutService.Queryable().Where( a => a.SiteId.Equals( siteId ) );

            SortProperty sortProperty = gLayouts.SortProperty;

            if ( sortProperty != null )
            {
                gLayouts.DataSource = qry.Sort( sortProperty ).ToList();
            }
            else
            {
                gLayouts.DataSource = qry.OrderBy( l => l.Name ).ToList();
            }

            gLayouts.DataBind();
        }

        protected string GetFilePath( string fileName )
        {
            string virtualPath = fileName;

            var siteCache = SiteCache.Read( hfSiteId.ValueAsInt() );
            if ( siteCache != null )
            {
                virtualPath = string.Format( "~/Themes/{0}/Layouts/{1}.aspx", siteCache.Theme, fileName );

                if ( !File.Exists( Request.MapPath( virtualPath ) ) )
                {
                    virtualPath = virtualPath += " <span class='label label-danger'>Missing</span>";
                }
            }

            return virtualPath;
        }

        #endregion

        #region IDimmableBlock

        /// <summary>
        /// Sets the dimmed.
        /// </summary>
        /// <param name="dimmed">if set to <c>true</c> [dimmed].</param>
        public void SetDimmed( bool dimmed )
        {
            pnlLayouts.Disabled = dimmed;
            gLayouts.Enabled = !dimmed;
        }

        #endregion
    }
}