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
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Security;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// User control for managing attribute categories 
    /// </summary>
    [DisplayName( "Attribute Categories" )]
    [Category( "Core" )]
    [Description( "Allows attribute categories to be managed." )]
    public partial class AttributeCategories : RockBlock
    {
        #region Fields

        bool _canConfigure = false;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _canConfigure = IsUserAuthorized( Authorization.ADMINISTRATE );

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

            if ( _canConfigure )
            {
                rGrid.DataKeyNames = new string[] { "Id" };
                rGrid.Actions.ShowAdd = true;

                rGrid.Actions.AddClick += rGrid_Add;
                rGrid.GridReorder += rGrid_GridReorder;
                rGrid.GridRebind += rGrid_GridRebind;
                rGrid.RowDataBound += rGrid_RowDataBound;

                modalDetails.SaveClick += modalDetails_SaveClick;
                modalDetails.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdValue.ClientID );
            }
            else
            {
                nbMessage.Text = "You are not authorized to configure this page";
                nbMessage.Visible = true;
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
                if ( _canConfigure )
                {
                    BindFilter();
                    BindGrid();
                }
            }
            else
            {
                if ( !string.IsNullOrWhiteSpace( hfIdValue.Value ) )
                {
                    modalDetails.Show();
                }
            }


            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "EntityType", entityTypeFilter.SelectedValue );
            BindGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "EntityType":

                    if ( e.Value != "" )
                    {
                        if ( e.Value == "0" )
                        {
                            e.Value = "None (Global Attributes)";
                        }
                        else
                        {
                            e.Value = EntityTypeCache.Read( int.Parse( e.Value ) ).FriendlyName;
                        }
                    }
                    break;
            }

        }

        /// <summary>
        /// Handles the Edit event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var service = new CategoryService( rockContext );

            var category = service.Get( e.RowKeyId );
            if ( category != null )
            {
                string errorMessage = string.Empty;
                if ( service.CanDelete( category, out errorMessage ) )
                {

                    service.Delete( category );

                    rockContext.SaveChanges();
                }
                else
                {
                    nbMessage.Text = errorMessage;
                    nbMessage.Visible = true;
                }
            }

            BindFilter();
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGrid_Add( object sender, EventArgs e )
        {
            ShowEdit( null );
        }

        /// <summary>
        /// Handles the GridRebind event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                Literal lEntityType = e.Row.FindControl( "lEntityType" ) as Literal;
                if ( lEntityType != null )
                {
                    lEntityType.Text = "None (Global Attributes)";

                    int categoryId = (int)rGrid.DataKeys[e.Row.RowIndex].Value;
                    var category = CategoryCache.Read( categoryId );

                    int entityTypeId = int.MinValue;
                    if ( category != null &&
                        !string.IsNullOrWhiteSpace( category.EntityTypeQualifierValue ) &&
                        int.TryParse( category.EntityTypeQualifierValue, out entityTypeId ) &&
                        entityTypeId > 0 )
                    {
                        var entityType = EntityTypeCache.Read( entityTypeId );
                        if ( entityType != null )
                        {
                            lEntityType.Text = entityType.FriendlyName;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the GridReorder event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void rGrid_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var categories = GetCategories( rockContext );
            if ( categories != null )
            {
                new CategoryService( rockContext ).Reorder( categories.ToList(), e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the modalDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void modalDetails_SaveClick( object sender, EventArgs e )
        {
            int categoryId = 0;
            if ( hfIdValue.Value != string.Empty && !int.TryParse( hfIdValue.Value, out categoryId ) )
            {
                categoryId = 0;
            }

            var rockContext = new RockContext();
            var service = new CategoryService( rockContext );
            Category category = null;

            if ( categoryId != 0 )
            {
                CategoryCache.Flush( categoryId );
                category = service.Get( categoryId );
            }

            if ( category == null )
            {
                category = new Category();
                category.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Attribute ) ).Id;
                category.EntityTypeQualifierColumn = "EntityTypeId";

                var lastCategory = GetUnorderedCategories( category.EntityTypeId )
                    .OrderByDescending( c => c.Order ).FirstOrDefault();
                category.Order = lastCategory != null ? lastCategory.Order + 1 : 0;

                service.Add( category );
            }

            category.Name = tbName.Text;
            category.Description = tbDescription.Text;

            string QualifierValue = null;
            if ( ( entityTypePicker.SelectedEntityTypeId ?? 0 ) != 0 )
            {
                QualifierValue = entityTypePicker.SelectedEntityTypeId.ToString();
            }
            category.EntityTypeQualifierValue = QualifierValue;

            category.IconCssClass = tbIconCssClass.Text;
            category.HighlightColor = tbHighlightColor.Text;

            List<int> orphanedBinaryFileIdList = new List<int>();

            if ( category.IsValid )
            {
                BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                foreach ( int binaryFileId in orphanedBinaryFileIdList )
                {
                    var binaryFile = binaryFileService.Get( binaryFileId );
                    if ( binaryFile != null )
                    {
                        // marked the old images as IsTemporary so they will get cleaned up later
                        binaryFile.IsTemporary = true;
                    }
                }

                rockContext.SaveChanges();

                hfIdValue.Value = string.Empty;
                modalDetails.Hide();

                BindFilter();
                BindGrid();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            // Exclude the categories for block and service job attributes, since they are controlled through code attribute decorations
            var exclusions = new List<Guid>();
            exclusions.Add( Rock.SystemGuid.EntityType.BLOCK.AsGuid() );
            exclusions.Add( Rock.SystemGuid.EntityType.SERVICE_JOB.AsGuid() );

            var rockContext = new RockContext();
            var entityTypes = new EntityTypeService( rockContext ).GetEntities()
                .Where( t => !exclusions.Contains( t.Guid ) )
                .OrderBy( t => t.FriendlyName )
                .ToList();

            entityTypePicker.EntityTypes = entityTypes;

            // Load Entity Type Filter
            var attributeEntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Attribute ) ).Id;
            var categoryEntities = new CategoryService( rockContext ).Queryable()
                .Where( c =>
                    c.EntityTypeId == attributeEntityTypeId &&
                    c.EntityTypeQualifierColumn == "EntityTypeId" &&
                    c.EntityTypeQualifierValue != null )
                .Select( c => c.EntityTypeQualifierValue )
                .ToList()
                .Select( c => c.AsInteger() );

            entityTypeFilter.EntityTypes = entityTypes.Where( e => categoryEntities.Contains( e.Id ) ).ToList();
            entityTypeFilter.SetValue( rFilter.GetUserPreference( "EntityType" ) );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            int? entityTypeId = entityTypeFilter.SelectedValueAsInt( false );

            nbOrdering.Visible = entityTypeId.HasValue;

            rGrid.Columns.OfType<ReorderField>().FirstOrDefault().Visible = entityTypeId.HasValue;
            rGrid.DataSource = GetCategories().ToList();
            rGrid.DataBind();
        }

        private IQueryable<Category> GetCategories( RockContext rockContext = null )
        {
            int? entityTypeId = entityTypeFilter.SelectedValueAsInt( false );

            rockContext = rockContext ?? new RockContext();
            var unorderedCategories = GetUnorderedCategories( entityTypeId, rockContext );

            if ( entityTypeId.HasValue )
            {
                return unorderedCategories.OrderBy( a => a.Order ).ThenBy( a => a.Name );
            }
            else
            {
                return unorderedCategories.OrderBy( a => a.Name );
            }
        }

        private IQueryable<Category> GetUnorderedCategories( int? entityTypeId, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            var attributeEntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Attribute ) ).Id;
            var queryable = new CategoryService( rockContext ).Queryable()
                .Where( c => 
                    c.EntityTypeId == attributeEntityTypeId && 
                    c.EntityTypeQualifierColumn == "EntityTypeId" );

            if ( entityTypeId.HasValue )
            {
                var stringValue = entityTypeId.Value.ToString();
                queryable = queryable.Where( c => 
                    ( entityTypeId.Value == 0 && c.EntityTypeQualifierValue == null ) ||
                    ( entityTypeId.Value != 0 && c.EntityTypeQualifierValue != null && c.EntityTypeQualifierValue == stringValue ) );
            }
            else
            {
                // Exclude the categories for block and service job attributes, since they are controlled through code attribute decorations
                var exclusions = new List<Guid>();
                exclusions.Add( Rock.SystemGuid.EntityType.BLOCK.AsGuid() );
                exclusions.Add( Rock.SystemGuid.EntityType.SERVICE_JOB.AsGuid() );

                var entities = new EntityTypeService( rockContext ).GetEntities()
                    .Where( t => !exclusions.Contains( t.Guid ) )
                    .Select( e => e.Id )
                    .ToList()
                    .Select( e => e.ToString() )
                    .ToList();

                queryable = queryable.Where( c => 
                    c.EntityTypeQualifierValue == null ||
                    entities.Contains( c.EntityTypeQualifierValue ) );
            }

            return queryable;
        }


        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        protected void ShowEdit( int? categoryId )
        {
            Category category = null;
            if ( categoryId.HasValue )
            {
                category = new CategoryService( new RockContext() ).Get( categoryId.Value );
            }

            if ( category != null )
            {
                tbName.Text = category.Name;
                tbDescription.Text = category.Description;
                tbIconCssClass.Text = category.IconCssClass;
                tbHighlightColor.Text = category.HighlightColor;
                entityTypePicker.SelectedEntityTypeId = category.EntityTypeQualifierValue.AsIntegerOrNull();
            }
            else
            {
                tbName.Text = string.Empty;
                tbDescription.Text = string.Empty;
                tbIconCssClass.Text = string.Empty;
                entityTypePicker.SelectedEntityTypeId = entityTypeFilter.SelectedValueAsInt( false );
            }

            hfIdValue.Value = categoryId.ToString();
            modalDetails.Show();
        }

        #endregion
    }
}