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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Block for managing categories for an specific entity type.
    /// </summary>
    [DisplayName( "Categories" )]
    [Category( "Core" )]
    [Description( "Block for managing categories for a specific, configured entity type." )]

    [EntityTypeField("Entity Type", "The entity type to manage categories for.")]
    public partial class Categories : RockBlock
    {
        #region Fields

        int _entityTypeId = 0;
        int? _parentCategoryId = null;
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
            if ( _canConfigure )
            {
                Guid entityTypeGuid = Guid.Empty;
                if ( Guid.TryParse( GetAttributeValue( "EntityType" ), out entityTypeGuid ) )
                {
                    var entityType = Rock.Web.Cache.EntityTypeCache.Read( entityTypeGuid );
                    if (entityType != null)
                    {
                        _entityTypeId = entityType.Id;

                        SecurityField securityField = gCategories.Columns[5] as SecurityField;
                        securityField.Visible = entityType.IsSecured;
                        securityField.EntityTypeId = EntityTypeCache.Read(typeof(Category)).Id;

                        catpParentCategory.EntityTypeId = _entityTypeId;

                        int parentCategoryId = int.MinValue;
                        if (int.TryParse(PageParameter("CategoryId"), out parentCategoryId))
                        {
                            _parentCategoryId = parentCategoryId;
                        }

                        gCategories.DataKeyNames = new string[] { "Id" };
                        gCategories.Actions.ShowAdd = true;

                        gCategories.Actions.AddClick += gCategories_Add;
                        gCategories.GridReorder += gCategories_GridReorder;
                        gCategories.GridRebind += gCategories_GridRebind;

                        mdDetails.SaveClick += mdDetails_SaveClick;
                        mdDetails.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdValue.ClientID );
                    }
                    else
                    {
                        pnlList.Visible = false;
                        nbMessage.Text = "Block has not been configured for a valid Enity Type.";
                        nbMessage.Visible = true;
                    }
                }
                else
                {
                    pnlList.Visible = false;
                    nbMessage.Text = "Block has not been configured for a valid Enity Type.";
                    nbMessage.Visible = true;
                }
            }
            else
            {
                pnlList.Visible = false;
                nbMessage.Text = "You are not authorized to configure this page.";
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
                    BindGrid();
                }
            }
            else
            {
                if ( !string.IsNullOrWhiteSpace( hfIdValue.Value ) )
                {
                    var rockContext = new RockContext();
                    Category category = new CategoryService( rockContext ).Get( hfIdValue.Value.AsInteger() );
                    if (category == null)
                    {
                        category = new Category { EntityTypeId = _entityTypeId };
                    }
                    category.LoadAttributes();
                    phAttributes.Controls.Clear();
                    Rock.Attribute.Helper.AddEditControls( category, phAttributes, false, BlockValidationGroup );

                    mdDetails.Show();
                }
            }


            base.OnLoad( e );
        }

        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            Guid entityTypeGuid = Guid.Empty;
            Guid.TryParse( GetAttributeValue( "EntityType" ), out entityTypeGuid );
            var entityType = EntityTypeCache.Read( entityTypeGuid );
            if ( entityType == null )
            {
                return base.GetBreadCrumbs( pageReference );
            }

            var breadCrumbs = new List<BreadCrumb>();


            int parentCategoryId = int.MinValue;
            if ( int.TryParse( PageParameter( "CategoryId" ), out parentCategoryId ) )
            {
                var category = CategoryCache.Read( parentCategoryId );
                while ( category != null )
                {
                    var parms = new Dictionary<string, string>();
                    parms.Add( "CategoryId", category.Id.ToString() );
                    breadCrumbs.Add( new BreadCrumb( category.Name, new PageReference( pageReference.PageId, 0, parms ) ) );

                    category = category.ParentCategory;
                }
            }

            breadCrumbs.Add( new BreadCrumb( entityType.FriendlyName + " Categories", new PageReference( pageReference.PageId ) ) );

            breadCrumbs.Reverse();

            return breadCrumbs;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Edit event of the gCategories control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gCategories_Select( object sender, RowEventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "CategoryId", e.RowKeyId.ToString() );
            Response.Redirect( new PageReference( CurrentPageReference.PageId, 0, parms ).BuildUrl(), false );
        }

        protected void gCategories_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( e.RowKeyId );
        }
        
        /// <summary>
        /// Handles the Delete event of the gCategories control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gCategories_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var service = new CategoryService( rockContext );

            var category = service.Get( e.RowKeyId );
            if ( category != null )
            {
                string errorMessage = string.Empty;
                if ( service.CanDelete( category, out errorMessage ) )
                {
                    int categoryId = category.Id;

                    service.Delete( category );
                    rockContext.SaveChanges();

                    CategoryCache.Flush( categoryId );
                }
                else
                {
                    nbMessage.Text = errorMessage;
                    nbMessage.Visible = true;
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gCategories control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gCategories_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the GridRebind event of the gCategories control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gCategories_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        void gCategories_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var categories = GetCategories( rockContext );
            if ( categories != null )
            {
                var changedIds = new CategoryService( rockContext ).Reorder( categories.ToList(), e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();

                foreach ( int id in changedIds )
                {
                    CategoryCache.Flush( id );
                }
            }

            BindGrid();
        }
        
        /// <summary>
        /// Handles the SaveClick event of the modalDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void mdDetails_SaveClick( object sender, EventArgs e )
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
                category = service.Get( categoryId );
            }

            if ( category == null )
            {
                category = new Category();
                category.EntityTypeId = _entityTypeId;
                var lastCategory = GetUnorderedCategories()
                    .OrderByDescending( c => c.Order ).FirstOrDefault();
                category.Order = lastCategory != null ? lastCategory.Order + 1 : 0;

                service.Add( category );
            }

            category.Name = tbName.Text;
            category.Description = tbDescription.Text;
            category.ParentCategoryId = catpParentCategory.SelectedValueAsInt();
            category.IconCssClass = tbIconCssClass.Text;
            category.HighlightColor = tbHighlightColor.Text;

            category.LoadAttributes( rockContext );
            Rock.Attribute.Helper.GetEditValues( phAttributes, category );

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

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    category.SaveAttributeValues( rockContext );
                } );

                CategoryCache.Flush( category.Id );

                hfIdValue.Value = string.Empty;
                mdDetails.Hide();

                BindGrid();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            gCategories.DataSource = GetCategories()
                .Select( c => new
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IconCssClass = c.IconCssClass,
                    ChildCount = c.ChildCategories.Count()
                } ).ToList();

            gCategories.EntityTypeId = EntityTypeCache.Read<Rock.Model.Category>().Id;
            gCategories.DataBind();
        }

        private IQueryable<Category> GetCategories( RockContext rockContext = null )
        {
            return GetUnorderedCategories( rockContext )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name );
        }

        private IQueryable<Category> GetUnorderedCategories( RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            var queryable = new CategoryService( rockContext ).Queryable().Where( c => c.EntityTypeId == _entityTypeId );
            if (_parentCategoryId.HasValue)
            {
                queryable = queryable.Where( c => c.ParentCategoryId == _parentCategoryId );
            }
            else
            {
                queryable = queryable.Where( c => c.ParentCategoryId == null );
            }

            return queryable;
        }


        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        protected void ShowEdit( int categoryId )
        {
            Category category = null;
            if ( categoryId > 0 )
            {
                category = new CategoryService( new RockContext() ).Get( categoryId );
            }

            if ( category == null )
            {
                category = new Category
                {
                    Id = 0,
                    EntityTypeId = _entityTypeId,
                    ParentCategoryId = _parentCategoryId,
                };
            }


            tbName.Text = category.Name;
            tbDescription.Text = category.Description;
            catpParentCategory.SetValue( category.ParentCategoryId );
            tbIconCssClass.Text = category.IconCssClass;
            tbHighlightColor.Text = category.HighlightColor;

            category.LoadAttributes();
            phAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( category, phAttributes, true, BlockValidationGroup );

            hfIdValue.Value = categoryId.ToString();
            mdDetails.Show();
        }

        #endregion

    }
}