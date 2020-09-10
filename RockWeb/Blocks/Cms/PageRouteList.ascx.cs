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
using System.Linq;
using System.Web.Routing;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.ComponentModel;
using Rock.Security;
using Rock.Web.Cache;
using System.Web.UI.WebControls;
using System.Data.Entity;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Route List")]
    [Category("CMS")]
    [Description("Displays a list of page routes.")]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    #endregion
    public partial class PageRouteList : RockBlock, ICustomGridColumns
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Attribute Keys

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gPageRoutes.DataKeyNames = new string[] { "Id" };
            gPageRoutes.Actions.ShowAdd = true;
            gPageRoutes.Actions.AddClick += gPageRoutes_Add;
            gPageRoutes.GridRebind += gPageRoutes_GridRebind;
            gFilter.ApplyFilterClick += gFilter_ApplyFilterClick;
            gFilter.DisplayFilterValue += gFilter_DisplayFilterValue;
            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gPageRoutes.Actions.ShowAdd = canAddEditDelete;
            gPageRoutes.IsDeleteEnabled = canAddEditDelete;

            AddAttributeColumns();

            var deleteField = new DeleteField();
            gPageRoutes.Columns.Add( deleteField );
            deleteField.Click += gPageRoutes_Delete;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }
        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gFilter.SaveUserPreference( "Site", ddlSite.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gPageRoutes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gPageRoutes_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "PageRouteId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gPageRoutes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPageRoutes_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "PageRouteId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gPageRoutes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPageRoutes_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            PageRouteService pageRouteService = new PageRouteService( rockContext );
            PageRoute pageRoute = pageRouteService.Get( e.RowKeyId );

            if ( pageRoute != null )
            {
                string errorMessage;
                if ( !pageRouteService.CanDelete( pageRoute, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                pageRouteService.Delete( pageRoute );

                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Displays the text of the gFilter control
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Site":
                    int? siteId = e.Value.AsIntegerOrNull();
                    if ( siteId.HasValue )
                    {
                        var site = SiteCache.Get( siteId.Value );
                        if ( site != null )
                        {
                            e.Value = site.Name;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gPageRoutes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gPageRoutes_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Adds columns for any page route attributes marked as Show In Grid
        /// </summary>
        protected void AddAttributeColumns()
        {
            // Remove attribute columns
            foreach ( var column in gPageRoutes.Columns.OfType<AttributeField>().ToList() )
            {
                gPageRoutes.Columns.Remove( column );
            }

            int entityTypeId = new PageRoute().TypeId;
            foreach ( var attribute in new AttributeService( new RockContext() ).Queryable().AsNoTracking()
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.IsGridColumn
                   )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name ) )
            {
                string dataFieldExpression = attribute.Key;
                bool columnExists = gPageRoutes.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                if ( !columnExists )
                {
                    AttributeField boundField = new AttributeField();
                    boundField.DataField = dataFieldExpression;
                    boundField.AttributeId = attribute.Id;
                    boundField.HeaderText = attribute.Name;

                    var attributeCache = Rock.Web.Cache.AttributeCache.Get( attribute.Id );
                    if ( attributeCache != null )
                    {
                        boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                    }

                    gPageRoutes.Columns.Add( boundField );
                }
            }
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlSite.Items.Clear();

            foreach ( SiteCache site in new SiteService( new RockContext() ).Queryable().AsNoTracking().OrderBy( s => s.Name ).Select( a => a.Id ).ToList().Select( a => SiteCache.Get( a ) ) )
            {
                ddlSite.Items.Add( new ListItem( site.Name, site.Id.ToString() ) );
            }
            ddlSite.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
            ddlSite.SetValue( gFilter.GetUserPreference( "Site" ) );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            PageRouteService pageRouteService = new PageRouteService( new RockContext() );
            SortProperty sortProperty = gPageRoutes.SortProperty;
            gPageRoutes.EntityTypeId = EntityTypeCache.Get<PageRoute>().Id;

            var queryable = pageRouteService.Queryable().AsNoTracking();

            int? siteId = gFilter.GetUserPreference( "Site" ).AsIntegerOrNull();
            if ( siteId.HasValue )
            {
                queryable = queryable.Where( d => d.Page.Layout.SiteId == siteId.Value );
            }

            if ( sortProperty != null )
            {
                gPageRoutes.DataSource = queryable.Sort( sortProperty ).ToList();
            }
            else
            {
                gPageRoutes.DataSource = queryable.OrderBy( p => p.Route ).ToList();
            }

            gPageRoutes.DataBind();
        }

        #endregion
    }
}