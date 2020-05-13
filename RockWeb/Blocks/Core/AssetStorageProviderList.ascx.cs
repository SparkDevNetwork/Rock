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
    [DisplayName( "Asset Storage Provider List" )]
    [Category( "Core" )]
    [Description( "Block for viewing list of asset storage providers." )]
    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage )]

    public partial class AssetStorageProviderList : RockBlock, ICustomGridColumns
    {
        public static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            bool canEdit = IsUserAuthorized( Authorization.EDIT );

            rGridAssetStorageProvider.DataKeyNames = new string[] { "Id" };
            rGridAssetStorageProvider.Actions.ShowAdd = canEdit;
            rGridAssetStorageProvider.Actions.AddClick += rGridAssetStorageProvider_AddClick;
            rGridAssetStorageProvider.GridRebind += rGridAssetStorageProvider_GridRebind;
            rGridAssetStorageProvider.IsDeleteEnabled = canEdit;
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
        /// Handles the RowSelected event of the rGridAssetStorageProvider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGridAssetStorageProvider_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, new Dictionary<string, string> { { "assetStorageProviderId", e.RowKeyValue.ToString() } } );
        }

        /// <summary>
        /// Handles the DeleteClick event of the rGridAssetStorageProvider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGridAssetStorageProvider_DeleteClick( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var assetStorageProviderService = new AssetStorageProviderService( rockContext );

            var assetStorageProvider = assetStorageProviderService.Get( e.RowKeyId );
            if ( assetStorageProvider != null )
            {
                assetStorageProviderService.Delete( assetStorageProvider );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the rGridAssetStorageProvider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGridAssetStorageProvider_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, new Dictionary<string, string> { { "assetStorageProviderId", "0" } } );
        }

        /// <summary>
        /// Handles the GridRebind event of the rGridAssetStorageProvider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGridAssetStorageProvider_GridRebind( object sender, EventArgs e )
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
                var assetStorageProviderService = new AssetStorageProviderService( rockContext );

                var qry = assetStorageProviderService.Queryable( "EntityType" ).AsNoTracking();

                SortProperty sortProperty = rGridAssetStorageProvider.SortProperty;
                if ( sortProperty != null )
                {
                    qry = qry.Sort( sortProperty );
                }
                else
                {
                    qry = qry.OrderBy( g => g.Name );
                }

                rGridAssetStorageProvider.DataSource = qry.ToList();
                rGridAssetStorageProvider.DataBind();
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