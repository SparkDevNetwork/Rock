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
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Block Type List" )]
    [Category( "Core" )]
    [Description( "Lists all the block types registered in Rock." )]

    [LinkedPage("Detail Page")]
    public partial class BlockTypeList : RockBlock
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;

            gBlockTypes.RowItemText = "Block Type";
            gBlockTypes.DataKeyNames = new string[] { "Id" };
            gBlockTypes.Actions.ShowAdd = true;
            gBlockTypes.Actions.AddClick += gBlockTypes_Add;
            gBlockTypes.GridRebind += gBlockTypes_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gBlockTypes.Actions.ShowAdd = canAddEditDelete;
            gBlockTypes.IsDeleteEnabled = canAddEditDelete;

            BindFilter();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                tbNameFilter.Text = gfSettings.GetUserPreference( "Name" );
                tbPathFilter.Text = gfSettings.GetUserPreference( "Path" );
                ddlCategoryFilter.SetValue( gfSettings.GetUserPreference( "Category" ) );
                cbExcludeSystem.Checked = !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( "Exclude System" ) );

                BlockTypeService.RegisterBlockTypes( Request.MapPath( "~" ), Page );

                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Apply Filter event for the GridFilter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SaveUserPreference( "Name", tbNameFilter.Text );
            gfSettings.SaveUserPreference( "Path", tbPathFilter.Text );
            gfSettings.SaveUserPreference( "Category", ddlCategoryFilter.SelectedValue);
            gfSettings.SaveUserPreference( "Exclude System", cbExcludeSystem.Checked ? "Yes" : string.Empty );
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gBlockTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gBlockTypes_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "blockTypeId", 0 );
        }

        /// <summary>
        /// Handles the EditRow event of the gBlockTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gBlockTypes_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "blockTypeId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gBlockTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gBlockTypes_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            BlockTypeService blockTypeService = new BlockTypeService( rockContext );
            BlockType blockType = blockTypeService.Get( e.RowKeyId );
            if ( blockType != null )
            {
                string errorMessage;
                if ( !blockTypeService.CanDelete( blockType, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                blockTypeService.Delete( blockType );
                rockContext.SaveChanges();
                Rock.Web.Cache.BlockTypeCache.Flush( blockType.Id );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gBlockTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gBlockTypes_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gBlockTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs" /> instance containing the event data.</param>
        protected void gBlockTypes_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.DataItem != null )
            {
                string blockPath = Request.MapPath( e.Row.DataItem.GetPropertyValue( "Path" ) as string );
                if ( !System.IO.File.Exists( blockPath ) )
                {
                    e.Row.Cells[4].Text = "<span class='label label-danger'>Missing</span>";
                }
                else
                {
                    e.Row.Cells[4].Text = "<span class='label label-success'>Found</span>";
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRefreshAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRefreshAll_Click( object sender, EventArgs e )
        {
            BlockTypeService.RegisterBlockTypes( Request.MapPath( "~" ), Page, true );
            BindGrid();
        }
        
        #endregion

        #region Methods

        private void BindFilter()
        {
            ddlCategoryFilter.Items.Clear();
            ddlCategoryFilter.Items.Add( string.Empty );
            foreach ( string category in new BlockTypeService( new RockContext() ).Queryable()
                .Where( b => b.Category != null && b.Category != "")
                .OrderBy( b=> b.Category)
                .Select( b => b.Category)
                .Distinct())
            {
                ddlCategoryFilter.Items.Add( category );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            BlockTypeService blockTypeService = new BlockTypeService( new RockContext() );
            SortProperty sortProperty = gBlockTypes.SortProperty;

            var blockTypes = blockTypeService.Queryable();

            // Exclude system blocks if checked.
            if ( !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference("Exclude System") ) )
            {
                blockTypes = blockTypes.Where( b => b.IsSystem == false );
            }

            // Filter by Name
            string nameFilter = gfSettings.GetUserPreference( "Name" );
            if ( !string.IsNullOrEmpty( nameFilter.Trim() ) )
            {
                blockTypes = blockTypes.Where( b => b.Name.Contains( nameFilter.Trim() ) );
            }

            // Filter by Path
            string path = gfSettings.GetUserPreference( "Path" );
            if ( !string.IsNullOrEmpty( path.Trim() ) )
            {
                blockTypes = blockTypes.Where( b => b.Path.Contains( path.Trim() ) );
            }

            string category = gfSettings.GetUserPreference( "Category" );
            if (!string.IsNullOrWhiteSpace(category))
            {
                blockTypes = blockTypes.Where( b => b.Category == category );
            }

            var selectQry = blockTypes.Select( a =>
                new
                {
                    a.Id,
                    a.Name,
                    a.Category,
                    a.Description,
                    a.Path,
                    BlocksCount = a.Blocks.Count(),
                    a.IsSystem
                } );

            if ( sortProperty != null )
            {
                if ( sortProperty.Property == "Status" )
                {
                    // special case:  See if the file exists and sort by that
                    if ( sortProperty.Direction == System.Web.UI.WebControls.SortDirection.Ascending )
                    {
                        gBlockTypes.DataSource = selectQry.ToList().OrderBy( a => System.IO.File.Exists( Request.MapPath( a.Path ) ) ).ToList();
                    }
                    else
                    {
                        gBlockTypes.DataSource = selectQry.ToList().OrderBy( a => !System.IO.File.Exists( Request.MapPath( a.Path ) ) ).ToList();
                    }
                }
                else
                {
                    gBlockTypes.DataSource = selectQry.Sort( sortProperty ).ToList();
                }
            }
            else
            {
                gBlockTypes.DataSource = selectQry.OrderBy( b => b.Name ).ToList();
            }

            gBlockTypes.EntityTypeId = EntityTypeCache.Read<Rock.Model.BlockType>().Id;
            gBlockTypes.DataBind();
        }

        #endregion

}
}