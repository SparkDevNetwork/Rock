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
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.AI
{
    /// <summary>
    /// Block for viewing a list of AI Providers.
    /// </summary>
    [DisplayName( "AI Provider List" )]
    [Category( "Core" )]
    [Description( "Block for viewing a list of AI Providers." )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Category = AttributeCategory.LinkedPages,
        Order = 1 )]

    #endregion

    [Rock.SystemGuid.BlockTypeGuid( "3C5CEC83-7AB8-470D-994B-52679E12CF7D" )]
    public partial class AiProviderList : RockBlock, ICustomGridColumns
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion

        #region Attribute Categories

        /// <summary>
        /// Keys to use for Block Attribute Categories
        /// </summary>
        private static class AttributeCategory
        {
            public const string LinkedPages = "Linked Pages";
        }

        #endregion

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string ProviderId = "ProviderId";
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var canEdit = IsUserAuthorized( Authorization.EDIT );

            rGridList.DataKeyNames = new string[] { "Id" };
            rGridList.Actions.ShowAdd = canEdit;
            rGridList.Actions.AddClick += rGridList_Add;
            rGridList.GridRebind += rGridLists_GridRebind;
            rGridList.IsDeleteEnabled = canEdit;
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

        #region Events

        /// <summary>
        /// Handles the Add event of the rGridList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGridList_Add( object sender, EventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( PageParameterKey.ProviderId, "0" );
            NavigateToLinkedPage( AttributeKey.DetailPage, parms );
        }

        /// <summary>
        /// Handles the Edit event of the rGridList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGridList_Edit( object sender, RowEventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( PageParameterKey.ProviderId, e.RowKeyValue.ToString() );
            NavigateToLinkedPage( AttributeKey.DetailPage, parms );
        }

        /// <summary>
        /// Handles the Delete event of the rGridList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGridList_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var service = new AIProviderService( rockContext );
            var entity = service.Get( e.RowKeyId );
            if ( entity != null )
            {
                string errorMessage;
                if ( !service.CanDelete( entity, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                service.Delete( entity );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the rGridList control.
        /// </summary>
        /// <param name="sendder">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGridLists_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the Provider list grid.
        /// </summary>
        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var aiProviderService = new AIProviderService( rockContext );
                var qry = aiProviderService.Queryable()
                    .Include( a => a.ProviderComponentEntityType )
                    .AsNoTracking();

                var sortProperty = rGridList.SortProperty;
                if ( sortProperty != null )
                {
                    qry = qry.Sort( sortProperty );
                }
                else
                {
                    qry = qry.OrderBy( g => g.Name );
                }

                var components = qry.ToList();

                rGridList.DataSource = components;
                rGridList.DataBind();
            }
        }

        protected string GetComponentDisplayName( object entityTypeObject )
        {
            var entityType = entityTypeObject as EntityType;
            if ( entityType != null )
            {
                var aiProviderEntityType = EntityTypeCache.Get( entityType.Guid );
                var name = Rock.Reflection.GetDisplayName( aiProviderEntityType.GetEntityType() );

                // If it has a DisplayName, use it as is
                if ( !string.IsNullOrWhiteSpace( name ) )
                {
                    return name;
                }
                else
                {
                    // Otherwise use the previous logic with SplitCase on the ComponentName
                    var componentName = Rock.AI.Provider.AIProviderContainer.GetComponentName( entityType.Name )
                        .ToStringSafe()
                        .SplitCase();
                    return componentName;
                }
            }

            return string.Empty;
        }

        #endregion
    }
}