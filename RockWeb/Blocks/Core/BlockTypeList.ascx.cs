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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Blocks;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Slingshot.Core.Data;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Block Type List" )]
    [Category( "Core" )]
    [Description( "Lists all the block types registered in Rock." )]

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.BlockTypeGuid( "78A31D91-61F6-42C3-BB7D-676EDC72F35F" )]
    public partial class BlockTypeList : RockBlock, ICustomGridColumns
    {
        public static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

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
                tbNameFilter.Text = gfSettings.GetFilterPreference( "Name" );
                tbPathFilter.Text = gfSettings.GetFilterPreference( "Path" );
                ddlCategoryFilter.SetValue( gfSettings.GetFilterPreference( "Category" ) );
                cbExcludeSystem.Checked = !string.IsNullOrWhiteSpace( gfSettings.GetFilterPreference( "Exclude System" ) );
                cbShowObsidianOnly.Checked = !string.IsNullOrWhiteSpace( gfSettings.GetFilterPreference( "Show Obsidian Only" ) );

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
            gfSettings.SetFilterPreference( "Name", tbNameFilter.Text );
            gfSettings.SetFilterPreference( "Path", tbPathFilter.Text );
            gfSettings.SetFilterPreference( "Category", ddlCategoryFilter.SelectedValue );
            gfSettings.SetFilterPreference( "Exclude System", cbExcludeSystem.Checked ? "Yes" : string.Empty );
            gfSettings.SetFilterPreference( "Show Obsidian Only", cbShowObsidianOnly.Checked ? "Yes" : string.Empty );
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gBlockTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gBlockTypes_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "BlockTypeId", 0 );
        }

        /// <summary>
        /// Handles the EditRow event of the gBlockTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gBlockTypes_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "BlockTypeId", e.RowKeyId );
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
            BlockTypeInfoRow blockTypeInfoRow = e.Row.DataItem as BlockTypeInfoRow;
            if ( blockTypeInfoRow != null )
            {
                if ( blockTypeInfoRow.Path.IsNotNullOrWhiteSpace() )
                {
                    string blockPath = Request.MapPath( blockTypeInfoRow.Path );
                    if ( !System.IO.File.Exists( blockPath ) )
                    {
                        e.Row.Cells[4].Text = "<span class='label label-danger'>Missing</span>";
                    }
                    else
                    {
                        e.Row.Cells[4].Text = "<span class='label label-success'>Found</span>";
                    }
                }
                else if ( blockTypeInfoRow.EntityTypeId.HasValue )
                {
                    e.Row.Cells[4].Text = string.Format( "<span class='label label-info'>{0}</span>", blockTypeInfoRow.EntityName );
                }
                else
                {
                    e.Row.Cells[4].Text = "<span class='label label-danger'>Unknown</span>";
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
                .Where( b => b.Category != null && b.Category != "" )
                .OrderBy( b => b.Category )
                .Select( b => b.Category )
                .Distinct() )
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

            var blockTypes = blockTypeService.Queryable().AsNoTracking();
            
            // Exclude system blocks if checked.
            if ( !string.IsNullOrWhiteSpace( gfSettings.GetFilterPreference( "Exclude System" ) ) )
            {
                blockTypes = blockTypes.Where( b => b.IsSystem == false );
            }

            // Filter by Name
            string nameFilter = gfSettings.GetFilterPreference( "Name" );
            if ( !string.IsNullOrEmpty( nameFilter.Trim() ) )
            {
                blockTypes = blockTypes.Where( b => b.Name.Contains( nameFilter.Trim() ) );
            }

            // Filter by Path
            string path = gfSettings.GetFilterPreference( "Path" );
            if ( !string.IsNullOrEmpty( path.Trim() ) )
            {
                blockTypes = blockTypes.Where( b => b.Path.Contains( path.Trim() ) );
            }

            string category = gfSettings.GetFilterPreference( "Category" );
            if ( !string.IsNullOrWhiteSpace( category ) )
            {
                blockTypes = blockTypes.Where( b => b.Category == category );
            }

            // A dictionary of WebSite blocks where the boolean value indicates if it's Obsidian or not.
            var obsidianBlockTypesDict = BlockTypeService.BlockTypesToDisplay( SiteType.Web )
                .ToDictionary( q => q.Id, q => typeof( IRockObsidianBlockType ).IsAssignableFrom( q.EntityType?.GetEntityType() ) );

            var selectQry = blockTypes.Select( a =>
                new BlockTypeInfoRow
                {
                    Id = a.Id,
                    Name = a.Name,
                    Category = a.Category,
                    Description = a.Description,
                    Path = a.Path,
                    EntityTypeId = a.EntityTypeId,
                    BlocksCount = a.Blocks.Count(),
                    IsSystem = a.IsSystem
                } ).AsEnumerable()
                    // We have to operate on the AsEnumerable so we can perform the dictionary lookup outside of LINQ to Entities
                    .Select( item => new BlockTypeInfoRow
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Category = item.Category,
                        Description = item.Description,
                        Path = item.Path,
                        EntityTypeId = item.EntityTypeId,
                        BlocksCount = item.BlocksCount,
                        IsSystem = item.IsSystem,
                        EntityName = (item.EntityTypeId.HasValue ? EntityTypeCache.Get( item.EntityTypeId.Value )?.Name : string.Empty ),
                        IsObsidian = ( obsidianBlockTypesDict.ContainsKey( item.Id ) ? obsidianBlockTypesDict[item.Id] : false)
                    } );

            // Filter by Obsidian. (Filtering by IsObsidian has to happen after the selectQry is built.)
            if ( !string.IsNullOrWhiteSpace( gfSettings.GetFilterPreference( "Show Obsidian Only" ) ) )
            {
                selectQry = selectQry.Where( a => a.IsObsidian );
            }

            if ( sortProperty != null )
            {
                if ( sortProperty.Property == "Status" )
                {
                    // special case:  See if the file exists and sort by that
                    if ( sortProperty.Direction == System.Web.UI.WebControls.SortDirection.Ascending )
                    {
                        gBlockTypes.DataSource = selectQry.ToList().OrderBy( a => System.IO.File.Exists( Request.MapPath( a.Path ) ) ).ThenBy( a => a.EntityName ).ThenBy( a => a.Name ).ToList();
                    }
                    else
                    {
                        gBlockTypes.DataSource = selectQry.ToList().OrderBy( a => !System.IO.File.Exists( Request.MapPath( a.Path ) ) ).ThenByDescending( a => a.EntityName ).ThenByDescending( a => a.Name ).ToList();
                    }
                }
                else
                {
                    // Sort by property name dynamically
                    if ( sortProperty.Direction == System.Web.UI.WebControls.SortDirection.Ascending )
                    {
                        gBlockTypes.DataSource = selectQry.OrderBy( a => a.GetType()
                            .GetProperty( sortProperty.Property )
                            .GetValue( a, null ) ).ThenBy( a => a.Name ).ToList();
                        }
                    else
                    {
                        gBlockTypes.DataSource = selectQry.OrderByDescending( a => a.GetType()
                            .GetProperty( sortProperty.Property )
                            .GetValue( a, null ) ).ThenBy( a => a.Name ).ToList();
                    }
                }
            }
            else
            {
                gBlockTypes.DataSource = selectQry.OrderBy( b => b.Name ).ToList();
            }

            gBlockTypes.EntityTypeId = EntityTypeCache.Get<Rock.Model.BlockType>().Id;
            gBlockTypes.DataBind();
        }

        public class BlockTypeInfoRow : RockDynamic
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string EntityName { get; set; }

            public string Category { get; set; }

            public string Description { get; set; }

            public string Path { get; set; }

            public int BlocksCount { get; set; }

            public bool IsObsidian { get; set; }

            public bool IsSystem { get; set; }

            public int? EntityTypeId { get; internal set; }
        }

        #endregion

    }
}