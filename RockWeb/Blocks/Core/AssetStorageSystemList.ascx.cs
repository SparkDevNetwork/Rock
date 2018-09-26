using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Asset Storage System List" )]
    [Category( "Core" )]
    [Description( "Block for viewing list of asset storage systems." )]
    [LinkedPage( "Detail Page" )]
    public partial class AssetStorageSystemList : RockBlock, ICustomGridColumns
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            bool canEdit = IsUserAuthorized( Authorization.EDIT );

            rGridAssetStorageSystem.DataKeyNames = new string[] { "Id" };
            rGridAssetStorageSystem.Actions.ShowAdd = canEdit;
            rGridAssetStorageSystem.Actions.AddClick += rGridAssetStorageSystem_AddClick;
            rGridAssetStorageSystem.GridRebind += rGridAssetStorageSystem_GridRebind;
            rGridAssetStorageSystem.IsDeleteEnabled = canEdit;
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
                BindGrid();
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the rGridAssetStorageSystem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGridAssetStorageSystem_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", new Dictionary<string, string> { { "assetStorageSystemId", e.RowKeyValue.ToString() } } );
        }

        /// <summary>
        /// Handles the DeleteClick event of the rGridAssetStorageSystem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGridAssetStorageSystem_DeleteClick( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var assetStorageSystemService = new AssetStorageSystemService( rockContext );

            var assetStorageSystem = assetStorageSystemService.Get( e.RowKeyId );
            if ( assetStorageSystem != null )
            {
                assetStorageSystemService.Delete( assetStorageSystem );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the rGridAssetStorageSystem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGridAssetStorageSystem_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", new Dictionary<string, string> { { "assetStorageSystemId", "0" } } );
        }

        /// <summary>
        /// Handles the GridRebind event of the rGridAssetStorageSystem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGridAssetStorageSystem_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var assetStorageSystemService = new AssetStorageSystemService( rockContext );

                var qry = assetStorageSystemService.Queryable( "EntityType" ).AsNoTracking();

                SortProperty sortProperty = rGridAssetStorageSystem.SortProperty;
                if ( sortProperty != null )
                {
                    qry = qry.Sort( sortProperty );
                }
                else
                {
                    qry = qry.OrderBy( g => g.Name );
                }

                rGridAssetStorageSystem.DataSource = qry.ToList();
                rGridAssetStorageSystem.DataBind();
            }
        }

        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        /// <param name="entityTypeObject">The entity type object.</param>
        /// <returns></returns>
        protected string GetComponentName( object entityTypeObject )
        {
            var entityType = entityTypeObject as EntityType;
            if ( entityType != null )
            {
                string name = Rock.Storage.AssetStorage.AssetStorageContainer.GetComponentName( entityType.Name );
                if ( !string.IsNullOrWhiteSpace( name ) )
                {
                    return name.SplitCase();
                }
            }

            return string.Empty;
        }
    }
}