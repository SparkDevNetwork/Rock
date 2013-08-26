//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    [EntityTypeField( "Entity Type", "The type of entity to associate category with" )]
    [TextField( "Entity Type Qualifier Property", "", false )]
    [TextField( "Entity Type Qualifier Value", "", false )]
    public partial class CategoryDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        private int entityTypeId = 0;
        private string entityTypeQualifierProperty = string.Empty;
        private string entityTypeQualifierValue = string.Empty;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string entityTypeName = GetAttributeValue( "EntityType" );
            entityTypeId = EntityTypeCache.Read(entityTypeName).Id;
            entityTypeQualifierProperty = GetAttributeValue( "EntityTypeQualifierProperty" );
            entityTypeQualifierValue = GetAttributeValue( "EntityTypeQualifierValue" );
            
            btnDelete.Attributes["onclick"] = string.Format( "javascript: return confirmDelete(event, '{0}');", Category.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Category ) ).Id;
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
                string itemId = PageParameter( "categoryId" );
                string parentCategoryId = PageParameter( "parentCategoryId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    if ( string.IsNullOrWhiteSpace( parentCategoryId ) )
                    {
                        ShowDetail( "categoryId", int.Parse( itemId ) );
                    }
                    else
                    {
                        ShowDetail( "categoryId", int.Parse( itemId ), int.Parse( parentCategoryId ) );
                    }
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfCategoryId.Value.Equals( "0" ) )
            {
                // Cancelling on Add.  Return to tree view with parent category selected
                var qryParams = new Dictionary<string, string>();

                string parentCategoryId = PageParameter( "parentCategoryId" );
                if ( !string.IsNullOrWhiteSpace( parentCategoryId ) )
                {
                    qryParams["CategoryId"] = parentCategoryId;
                }
                NavigateToPage( this.CurrentPage.Guid, qryParams );
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                CategoryService service = new CategoryService();
                Category category = service.Get( hfCategoryId.ValueAsInt() );
                ShowReadonlyDetails( category );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            CategoryService service = new CategoryService();
            Category category = service.Get( hfCategoryId.ValueAsInt() );
            ShowEditDetails( category );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            int? parentCategoryId = null;

            var categoryService = new CategoryService();
            var category = categoryService.Get( int.Parse( hfCategoryId.Value ) );

            if ( category != null )
            {
                string errorMessage;
                if ( !categoryService.CanDelete( category, out errorMessage ) )
                {
                    ShowReadonlyDetails( category );
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                }
                else
                {
                    parentCategoryId = category.ParentCategoryId;

                    categoryService.Delete( category, CurrentPersonId );
                    categoryService.Save( category, CurrentPersonId );

                    // reload page, selecting the deleted category's parent
                    var qryParams = new Dictionary<string, string>();
                    if ( parentCategoryId != null )
                    {
                        qryParams["CategoryId"] = parentCategoryId.ToString();
                    }

                    NavigateToPage( this.CurrentPage.Guid, qryParams );
                }
            }
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            Category category;
            CategoryService categoryService = new CategoryService();

            int categoryId = hfCategoryId.ValueAsInt();

            if ( categoryId == 0 )
            {
                category = new Category();
                category.IsSystem = false;
                category.EntityTypeId = entityTypeId;
                category.EntityTypeQualifierColumn = entityTypeQualifierProperty;
                category.EntityTypeQualifierValue = entityTypeQualifierValue;
            }
            else
            {
                category = categoryService.Get( categoryId );
            }

            category.Name = tbName.Text;
            category.ParentCategoryId = cpParentCategory.SelectedValueAsInt();
            category.IconCssClass = tbIconCssClass.Text;
            category.IconSmallFileId = imgIconSmall.BinaryFileId;
            category.IconLargeFileId = imgIconLarge.BinaryFileId;

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !category.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            RockTransactionScope.WrapTransaction( () =>
            {
                if ( category.Id.Equals( 0 ) )
                {
                    categoryService.Add( category, CurrentPersonId );
                }

                categoryService.Save( category, CurrentPersonId );
            } );

            var qryParams = new Dictionary<string, string>();
            qryParams["CategoryId"] = category.Id.ToString();
            NavigateToPage( this.CurrentPage.Guid, qryParams );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            ShowDetail( itemKey, itemKeyValue, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        /// <param name="parentCategoryId">The parent category id.</param>
        public void ShowDetail( string itemKey, int itemKeyValue, int? parentCategoryId )
        {
            pnlDetails.Visible = false;
            if ( !itemKey.Equals( "categoryId" ) )
            {
                return;
            }

            var categoryService = new CategoryService();
            Category category = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                category = categoryService.Get( itemKeyValue );
            }
            else
            {
                category = new Category { Id = 0, IsSystem = false, ParentCategoryId = parentCategoryId};
                category.EntityTypeId = entityTypeId;
                category.EntityTypeQualifierColumn = entityTypeQualifierProperty;
                category.EntityTypeQualifierValue = entityTypeQualifierValue;
            }

            if ( category == null || !category.IsAuthorized( "View", CurrentPerson ) )
            {
                return;
            }

            pnlDetails.Visible = true;
            hfCategoryId.Value = category.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !category.IsAuthorized( "Edit", CurrentPerson ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Category.FriendlyTypeName );
            }

            if ( category.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( Category.FriendlyTypeName );
            }

            btnSecurity.Visible = category.IsAuthorized( "Administrate", CurrentPerson );
            btnSecurity.Title = category.Name;
            btnSecurity.EntityId = category.Id;

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                ShowReadonlyDetails( category );
            }
            else
            {
                btnEdit.Visible = true;
                string errorMessage = string.Empty;
                btnDelete.Visible = categoryService.CanDelete(category, out errorMessage);
                if ( category.Id > 0 )
                {
                    ShowReadonlyDetails( category );
                }
                else
                {
                    ShowEditDetails( category );
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="category">The category.</param>
        private void ShowEditDetails( Category category )
        {
            if ( category.Id > 0 )
            {
                lActionTitle.Text = ActionTitle.Edit( Category.FriendlyTypeName );
            }
            else
            {
                lActionTitle.Text = ActionTitle.Add( Category.FriendlyTypeName );
            }

            SetEditMode( true );
            
            tbName.Text = category.Name;

            if ( category.EntityTypeId != 0 )
            {
                var entityType = EntityTypeCache.Read( category.EntityTypeId );
                lblEntityTypeName.Text = entityType.Name;
            }
            else
            {
                lblEntityTypeName.Text = string.Empty;
            }

            cpParentCategory.EntityTypeId = category.EntityTypeId;
            cpParentCategory.EntityTypeQualifierColumn = category.EntityTypeQualifierColumn;
            cpParentCategory.EntityTypeQualifierValue = category.EntityTypeQualifierValue;
            cpParentCategory.SetValue( category.ParentCategoryId );

            lblEntityTypeQualifierColumn.Visible = !string.IsNullOrWhiteSpace( category.EntityTypeQualifierColumn );
            lblEntityTypeQualifierColumn.Text = category.EntityTypeQualifierColumn;
            lblEntityTypeQualifierValue.Visible = !string.IsNullOrWhiteSpace( category.EntityTypeQualifierValue );
            lblEntityTypeQualifierValue.Text = category.EntityTypeQualifierValue;
            tbIconCssClass.Text = category.IconCssClass;
            imgIconSmall.BinaryFileId = category.IconSmallFileId.HasValue ? category.IconSmallFileId.Value : None.Id;
            imgIconLarge.BinaryFileId = category.IconLargeFileId.HasValue ? category.IconLargeFileId.Value : None.Id;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="category">The category.</param>
        private void ShowReadonlyDetails( Category category )
        {
            SetEditMode( false );

            string categoryIconHtml = string.Empty;
            if ( !string.IsNullOrWhiteSpace( category.IconCssClass ) )
            {
                categoryIconHtml = string.Format( "<i class='{0} icon-large' ></i>", category.IconCssClass );
            }
            else
            {
                var appPath = System.Web.VirtualPathUtility.ToAbsolute( "~" );
                string imageUrlFormat = "<img src='" + appPath + "GetImage.ashx?id={0}&width=50&height=50' />";
                if ( category.IconLargeFileId != null )
                {
                    categoryIconHtml = string.Format( imageUrlFormat, category.IconLargeFileId );
                }
                else if ( category.IconSmallFileId != null )
                {
                    categoryIconHtml = string.Format( imageUrlFormat, category.IconSmallFileId );
                }
            }

            hfCategoryId.SetValue( category.Id );
            lCategoryIconHtml.Text = categoryIconHtml;
            lReadOnlyTitle.Text = category.Name;

            lblMainDetails.Text = new DescriptionList()
                .Add("Entity Type", category.EntityType.Name)
                .Html;
            
        }

        #endregion
    }
}