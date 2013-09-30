//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// User controls for managing defined types
    /// </summary>
    public partial class DefinedTypeDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // assign attributes grid actions
            gDefinedTypeAttributes.DataKeyNames = new string[] { "Guid" };
            gDefinedTypeAttributes.Actions.ShowAdd = true;
            gDefinedTypeAttributes.Actions.AddClick += gDefinedTypeAttributes_Add;
            gDefinedTypeAttributes.GridRebind += gDefinedTypeAttributes_GridRebind;
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
                string itemId = PageParameter( "definedTypeId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "definedTypeId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSaveType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveType_Click( object sender, EventArgs e )
        {
            DefinedType definedType = null;
            DefinedTypeService typeService = new DefinedTypeService();

            int definedTypeId = hfDefinedTypeId.ValueAsInt();

            if ( definedTypeId == 0 )
            {
                definedType = new DefinedType();
                definedType.IsSystem = false;
                definedType.Order = 0;
                typeService.Add( definedType, CurrentPersonId );
            }
            else
            {
                DefinedTypeCache.Flush( definedTypeId );
                definedType = typeService.Get( definedTypeId );
            }

            definedType.Name = tbTypeName.Text;
            definedType.Category = tbTypeCategory.Text;
            definedType.Description = tbTypeDescription.Text;
            definedType.FieldTypeId = int.Parse( ddlTypeFieldType.SelectedValue );

            if ( !definedType.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            RockTransactionScope.WrapTransaction( () =>
            {
                typeService.Save( definedType, CurrentPersonId );

                // get it back to make sure we have a good Id
                definedType = typeService.Get( definedType.Guid );
            } );

            var qryParams = new Dictionary<string, string>();
            qryParams["definedTypeId"] = definedType.Id.ToString();
            NavigateToPage( this.CurrentPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancelType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelType_Click( object sender, EventArgs e )
        {
            if ( hfDefinedTypeId.IsZero() )
            {
                // Cancelling on Add.  Return to Grid
                NavigateToParentPage();
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                DefinedTypeService definedTypeService = new DefinedTypeService();
                DefinedType definedType = definedTypeService.Get( hfDefinedTypeId.ValueAsInt() );
                ShowReadonlyDetails( definedType );
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="definedType">Type of the defined.</param>
        private void ShowReadonlyDetails( DefinedType definedType )
        {
            SetEditMode( false );

            hfDefinedTypeId.SetValue( definedType.Id );
            tbTypeName.Text = definedType.Name;

            definedType.FieldType = definedType.FieldType ?? new FieldTypeService().Get( definedType.FieldTypeId ?? 0 );

            lblMainDetails.Text = new DescriptionList()
                .Add("Name", definedType.Name)
                .Add("Description", definedType.Description)
                .Add("Category", definedType.Category)
                .Add("FieldType", definedType.FieldType.Name)
                .Html;

            definedType.LoadAttributes();
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="definedType">Type of the defined.</param>
        private void ShowEditDetails( DefinedType definedType )
        {
            if ( definedType.Id > 0 )
            {
                lActionTitle.Text = ActionTitle.Edit( DefinedType.FriendlyTypeName );
            }
            else
            {
                lActionTitle.Text = ActionTitle.Add( DefinedType.FriendlyTypeName );
            }

            SetEditMode( true );

            tbTypeName.Text = definedType.Name;
            tbTypeCategory.Text = definedType.Category;
            tbTypeDescription.Text = definedType.Description;
            ddlTypeFieldType.SetValue( definedType.FieldTypeId );
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            DefinedTypeService definedTypeService = new DefinedTypeService();
            DefinedType definedType = definedTypeService.Get( hfDefinedTypeId.ValueAsInt() );
            ShowEditDetails( definedType );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            vsDetails.Enabled = editable;
            fieldsetViewDetails.Visible = !editable;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( "definedTypeId" ) )
            {
                return;
            }

            pnlDetails.Visible = true;
            DefinedType definedType = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                definedType = new DefinedTypeService().Get( itemKeyValue );
            }
            else
            {
                definedType = new DefinedType { Id = 0 };
            }

            hfDefinedTypeId.SetValue( definedType.Id );

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( DefinedType.FriendlyTypeName );
            }

            if ( definedType.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( DefinedType.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                ShowReadonlyDetails( definedType );
            }
            else
            {
                btnEdit.Visible = true;
                if ( definedType.Id > 0 )
                {
                    ShowReadonlyDetails( definedType );
                }
                else
                {
                    ShowEditDetails( definedType );
                }
            }

            BindDefinedTypeAttributesGrid();            
        }
                
        #endregion

        #region DefinedTypeAttributes Grid and Picker

        /// <summary>
        /// Handles the Add event of the gDefinedTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gDefinedTypeAttributes_Add( object sender, EventArgs e )
        {
            gDefinedTypeAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gDefinedTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gDefinedTypeAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            gDefinedTypeAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Gs the defined type attributes_ show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        protected void gDefinedTypeAttributes_ShowEdit( Guid attributeGuid )
        {
            pnlDetails.Visible = false;
            vsDetails.Enabled = false;
            pnlDefinedTypeAttributes.Visible = true;

            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                edtDefinedTypeAttributes.ActionTitle = ActionTitle.Add( "attribute for defined type " + tbTypeName.Text );
            }
            else
            {
                AttributeService attributeService = new AttributeService();
                attribute = attributeService.Get( attributeGuid );
                edtDefinedTypeAttributes.ActionTitle = ActionTitle.Edit( "attribute for defined type " + tbTypeName.Text );
            }

            edtDefinedTypeAttributes.SetAttributeProperties( attribute, typeof( DefinedValue ) );
        }

        /// <summary>
        /// Handles the Delete event of the gDefinedTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gDefinedTypeAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            AttributeService attributeService = new AttributeService();
            Attribute attribute = attributeService.Get( attributeGuid );

            if ( attribute != null )
            {
                string errorMessage;
                if ( !attributeService.CanDelete( attribute, out errorMessage ) )
                {
                    mdGridWarningAttributes.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                AttributeCache.Flush( attribute.Id );
                attributeService.Delete( attribute, CurrentPersonId );
                attributeService.Save( attribute, CurrentPersonId );
            }

            BindDefinedTypeAttributesGrid();            
        }

        /// <summary>
        /// Handles the GridRebind event of the gDefinedTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gDefinedTypeAttributes_GridRebind( object sender, EventArgs e )
        {
            BindDefinedTypeAttributesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveDefinedTypeAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveDefinedTypeAttribute_Click( object sender, EventArgs e )
        {
            Attribute attribute = null;
            
            AttributeService attributeService = new AttributeService();
            if ( edtDefinedTypeAttributes.AttributeId.HasValue )
            {
                attribute = attributeService.Get( edtDefinedTypeAttributes.AttributeId.Value );
            }

            if (attribute == null)
            {
                attribute = new Attribute();
            }

            edtDefinedTypeAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            RockTransactionScope.WrapTransaction( () =>
            {
                if ( attribute.Id.Equals( 0 ) )
                {
                    attribute.EntityTypeId = EntityTypeCache.Read( typeof( DefinedValue ) ).Id;
                    attribute.EntityTypeQualifierColumn = "DefinedTypeId";
                    attribute.EntityTypeQualifierValue = hfDefinedTypeId.Value;
                    attributeService.Add( attribute, CurrentPersonId );
                }

                AttributeCache.Flush( attribute.Id );
                attributeService.Save( attribute, CurrentPersonId );
            } );

            pnlDetails.Visible = true;
            pnlDefinedTypeAttributes.Visible = false;

            BindDefinedTypeAttributesGrid();            
        }

        /// <summary>
        /// Handles the Click event of the btnCancelDefinedTypeAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelDefinedTypeAttribute_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = true;
            pnlDefinedTypeAttributes.Visible = false;
        }

        /// <summary>
        /// Binds the defined type attributes grid.
        /// </summary>
        private void BindDefinedTypeAttributesGrid()
        {
            AttributeService attributeService = new AttributeService();

            string qualifierValue = hfDefinedTypeId.Value;
            var qryDefinedTypeAttributes = attributeService.GetByEntityTypeId( new DefinedValue().TypeId ).AsQueryable()
                .Where( a => a.EntityTypeQualifierColumn.Equals( "DefinedTypeId", StringComparison.OrdinalIgnoreCase )
                && a.EntityTypeQualifierValue.Equals( qualifierValue ) );

            gDefinedTypeAttributes.DataSource = qryDefinedTypeAttributes.OrderBy( a => a.Name ).ToList();
            gDefinedTypeAttributes.DataBind();
        }

        #endregion       
    }
}