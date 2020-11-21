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

    [EntityTypeField( "Entity Type",
        Description = "The entity type to manage categories for.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.EntityType )]

    [TextField( "Entity Qualifier Column",
        Description = "Column to evaluate to determine entities that this category applies to.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.EntityQualifierColumn )]

    [TextField( "Entity Qualifier Value",
        Description = "The value of the column that this category applies to.",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.EntityQualifierValue )]

    [BooleanField( "Enable Hierarchy",
        Description = "When set allows you to drill down through the category hierarchy.",
        DefaultValue = "true",
        Order = 3,
        Key = AttributeKey.EnableHierarchy )]

    public partial class Categories : RockBlock, ICustomGridColumns
    {
        public static class AttributeKey
        {
            public const string EntityType = "EntityType";
            public const string EntityQualifierColumn = "EntityQualifierColumn";
            public const string EntityQualifierValue = "EntityQualifierValue";
            public const string EnableHierarchy = "EnableHierarchy";
        }

        #region Fields

        int _entityTypeId = 0;
        string _entityCol = string.Empty;
        string _entityVal = string.Empty;
        bool _hasEntityTypeBlockSetting = false;

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

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            int parentCategoryId = int.MinValue;
            if ( int.TryParse( PageParameter( "CategoryId" ), out parentCategoryId ) )
            {
                _parentCategoryId = parentCategoryId;
            }

            gCategories.DataKeyNames = new string[] { "Id" };
            gCategories.Actions.ShowAdd = true;

            gCategories.Actions.AddClick += gCategories_Add;
            gCategories.GridReorder += gCategories_GridReorder;
            gCategories.GridRebind += gCategories_GridRebind;
            gCategories.RowDataBound += gCategories_RowDataBound;

            mdDetails.SaveClick += mdDetails_SaveClick;
            mdDetails.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdValue.ClientID );

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

            _hasEntityTypeBlockSetting = !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.EntityType ) );

            SetDisplay();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( GetAttributeValue( AttributeKey.EnableHierarchy ).AsBoolean() )
            {
                gCategories.RowSelected += gCategories_Select;
            }

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
                    if ( category == null )
                    {
                        category = new Category
                        {
                            EntityTypeId = _entityTypeId,
                            EntityTypeQualifierColumn = _entityCol,
                            EntityTypeQualifierValue = _entityVal
                        };
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
            var entityTypeName = string.Empty;

            Guid entityTypeGuid = Guid.Empty;
            Guid.TryParse( GetAttributeValue( AttributeKey.EntityType ), out entityTypeGuid );
            var entityType = EntityTypeCache.Get( entityTypeGuid );
            if ( entityType != null )
            {
                entityTypeName = entityType.FriendlyName;
            }

            int parentCategoryId = int.MinValue;
            CategoryCache category = null;
            if ( int.TryParse( PageParameter( "CategoryId" ), out parentCategoryId ) )
            {
                category = CategoryCache.Get( parentCategoryId );

                if ( entityType == null && category != null )
                {
                    entityType = EntityTypeCache.Get( category.EntityTypeId.Value );

                    if ( entityType != null )
                    {
                        entityTypeName = entityType.FriendlyName;
                    }
                }
            }

            if ( entityType == null && category == null )
            {
                return base.GetBreadCrumbs( pageReference );
            }

            var breadCrumbs = new List<BreadCrumb>();

            // add categories to the breadcrumbs
            while ( category != null )
            {
                var parms = new Dictionary<string, string>();
                parms.Add( "CategoryId", category.Id.ToString() );
                breadCrumbs.Add( new BreadCrumb( category.Name, new PageReference( pageReference.PageId, 0, parms ) ) );

                category = category.ParentCategory;
            }

            string rootPageTitle = string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.EntityQualifierColumn ) ) ?
                entityTypeName + " Categories" : this.RockPage.PageTitle;

            breadCrumbs.Add( new BreadCrumb( rootPageTitle, new PageReference( pageReference.PageId ) ) );

            breadCrumbs.Reverse();

            return breadCrumbs;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            SetDisplay();

            if ( _canConfigure )
            {
                BindGrid();
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( AttributeKey.EntityType, entityTypeFilter.SelectedValue );
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
                case AttributeKey.EntityType:
                    {
                        if ( e.Value != "" )
                        {
                            if ( e.Value != "0" )
                                e.Value = EntityTypeCache.Get( int.Parse( e.Value ) ).FriendlyName;
                        }
                        break;
                    }
            }
        }

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

        /// <summary>
        /// Handles the RowDataBound event of the gCategories control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void gCategories_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                //
                // Disable the edit button if it is a system category.
                //
                if ( ( ( bool ) e.Row.DataItem.GetPropertyValue( "IsSystem" ) ) == true )
                {
                    int? idx = gCategories.GetColumnIndexByFieldType( typeof( EditField ) );

                    if ( idx.HasValue )
                    {
                        var cell = e.Row.Cells[idx.Value];

                        if ( cell.Controls.Count > 0 && cell.Controls[0] is LinkButton )
                        {
                            ( ( LinkButton ) cell.Controls[0] ).Enabled = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the GridReorder event of the gCategories control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        void gCategories_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var categories = GetCategories( rockContext );
            if ( categories != null )
            {
                var changedIds = new CategoryService( rockContext ).Reorder( categories.ToList(), e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
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

                if ( _hasEntityTypeBlockSetting )
                {
                    category.EntityTypeId = _entityTypeId;
                    category.EntityTypeQualifierColumn = _entityCol;
                    category.EntityTypeQualifierValue = _entityVal;
                }
                else
                {
                    category.EntityTypeId = entityTypePicker.SelectedEntityTypeId ?? 0;
                    category.EntityTypeQualifierColumn = tbEntityQualifierField.Text;
                    category.EntityTypeQualifierValue = tbEntityQualifierValue.Text;
                }

                var lastCategory = GetUnorderedCategories()
                    .OrderByDescending( c => c.Order ).FirstOrDefault();
                category.Order = lastCategory != null ? lastCategory.Order + 1 : 0;

                service.Add( category );
            }

            category.Name = tbName.Text;
            category.Description = tbDescription.Text;
            category.ParentCategoryId = catpParentCategory.SelectedValueAsInt();
            category.IconCssClass = tbIconCssClass.Text;
            category.HighlightColor = cpHighlight.Value;

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

                hfIdValue.Value = string.Empty;
                mdDetails.Hide();

                BindGrid();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the display.
        /// </summary>
        private void SetDisplay()
        {
            var securityField = gCategories.ColumnsOfType<SecurityField>().FirstOrDefault();
            if ( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( Category ) ).Id;
            }

            _canConfigure = IsUserAuthorized( Authorization.ADMINISTRATE );
            if ( _canConfigure )
            {
                Guid entityTypeGuid = Guid.Empty;
                if ( Guid.TryParse( GetAttributeValue( AttributeKey.EntityType ), out entityTypeGuid ) )
                {
                    var entityType = EntityTypeCache.Get( entityTypeGuid );
                    if ( entityType != null )
                    {
                        securityField.Visible = entityType.IsSecured;

                        _entityTypeId = entityType.Id;
                        _entityCol = GetAttributeValue( AttributeKey.EntityQualifierColumn );
                        _entityVal = GetAttributeValue( AttributeKey.EntityQualifierValue );

                        catpParentCategory.EntityTypeId = _entityTypeId;

                        pnlList.Visible = true;
                        nbMessage.Visible = false;
                    }
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
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
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

            entityTypePicker.IncludeGlobalOption = false;
            entityTypeFilter.IncludeGlobalOption = false;

            entityTypePicker.EntityTypes = entityTypes;
            entityTypeFilter.EntityTypes = entityTypes;

            entityTypeFilter.SetValue( rFilter.GetUserPreference( AttributeKey.EntityType ) );

            if ( _hasEntityTypeBlockSetting )
            {
                pnlEntityInfo.Visible = false;

                var entityTypeField = gCategories.ColumnsOfType<RockBoundField>().FirstOrDefault( a => a.DataField == AttributeKey.EntityType );
                if ( entityTypeField != null )
                {
                    entityTypeField.Visible = false;
                }

                var entityQualifierField = gCategories.ColumnsOfType<RockBoundField>().FirstOrDefault( a => a.DataField == "EntityQualifierField" );
                if ( entityQualifierField != null )
                {
                    entityQualifierField.Visible = false;
                }

                var entityQualifierValue = gCategories.ColumnsOfType<RockBoundField>().FirstOrDefault( a => a.DataField == AttributeKey.EntityQualifierValue );
                if ( entityQualifierValue != null )
                {
                    entityQualifierValue.Visible = false;
                }

                catpParentCategory.Visible = false;

                rFilter.Visible = false;
            }

            gCategories.DataSource = GetCategories()
                .Select( c => new
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IconCssClass = c.IconCssClass,
                    ChildCount = c.ChildCategories.Count(),
                    EntityType = c.EntityType.Name,
                    EntityQualifierField = c.EntityTypeQualifierColumn,
                    EntityQualifierValue = c.EntityTypeQualifierValue,
                    IsSystem = c.IsSystem
                } ).ToList();

            gCategories.EntityTypeId = EntityTypeCache.Get<Rock.Model.Category>().Id;
            gCategories.DataBind();
        }

        private IEnumerable<Category> GetCategories( RockContext rockContext = null )
        {
            return GetUnorderedCategories( rockContext )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name );
        }

        private IEnumerable<Category> GetUnorderedCategories( RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            var queryable = new CategoryService( rockContext )
                .Queryable();

            if ( _hasEntityTypeBlockSetting )
            {
                queryable = queryable.Where( c => c.EntityTypeId == _entityTypeId );
            }
            else
            {
                int? filterEntityTypeId = entityTypeFilter.SelectedValueAsInt();
                if ( filterEntityTypeId.HasValue )
                {
                    queryable = queryable.Where( c => c.EntityTypeId == filterEntityTypeId.Value );
                }
            }

            var queryableFiltered = queryable.ToList()
                .Where( c =>
                    ( c.EntityTypeQualifierColumn ?? "" ) == ( _entityCol ?? "" ) &&
                    ( c.EntityTypeQualifierValue ?? "" ) == ( _entityVal ?? "" ) );

            if ( _parentCategoryId.HasValue )
            {
                queryableFiltered = queryableFiltered.Where( c => c.ParentCategoryId == _parentCategoryId );
            }
            else
            {
                queryableFiltered = queryableFiltered.Where( c => c.ParentCategoryId == null );
            }

            return queryableFiltered;
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
                // if there is a parent category set the entity type and qualifiers and hide the settings
                if ( _parentCategoryId.HasValue )
                {
                    var parentCategory = CategoryCache.Get( _parentCategoryId.Value );
                    entityTypePicker.SelectedEntityTypeId = parentCategory.EntityTypeId;
                    tbEntityQualifierField.Text = parentCategory.EntityTypeQualifierColumn;
                    tbEntityQualifierValue.Text = parentCategory.EntityTypeQualifierValue;
                    pnlEntityInfo.Visible = false;
                }

                category = new Category
                {
                    Id = 0,
                    EntityTypeId = _entityTypeId,
                    EntityTypeQualifierColumn = _entityCol,
                    EntityTypeQualifierValue = _entityVal,
                    ParentCategoryId = _parentCategoryId,
                };
            }
            else
            {
                entityTypePicker.SelectedEntityTypeId = category.EntityTypeId;
                tbEntityQualifierField.Text = category.EntityTypeQualifierColumn;
                tbEntityQualifierValue.Text = category.EntityTypeQualifierValue;
            }


            tbName.Text = category.Name;
            tbDescription.Text = category.Description;
            catpParentCategory.SetValue( category.ParentCategoryId );
            tbIconCssClass.Text = category.IconCssClass;
            cpHighlight.Value = category.HighlightColor;

            category.LoadAttributes();
            phAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( category, phAttributes, true, BlockValidationGroup );

            hfIdValue.Value = categoryId.ToString();
            mdDetails.Show();
        }

        #endregion

    }
}