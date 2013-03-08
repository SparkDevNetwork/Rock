//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// User controls for managing defined types and their values
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

            // assign type values grid actions
            gDefinedValues.DataKeyNames = new string[] { "id" };
            gDefinedValues.Actions.IsAddEnabled = true;
            gDefinedValues.Actions.AddClick += gDefinedValues_Add;
            gDefinedValues.GridRebind += gDefinedValues_GridRebind;

            // assign attributes grid actions
            gDefinedTypeAttributes.DataKeyNames = new string[] { "Guid" };
            gDefinedTypeAttributes.Actions.IsAddEnabled = true;
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

            if ( pnlDefinedValueEditor.Visible )
            {
                if ( !string.IsNullOrWhiteSpace( hfDefinedTypeId.Value ) )
                {
                    var definedValue = new DefinedValue { DefinedTypeId = hfDefinedTypeId.ValueAsInt() };
                    definedValue.LoadAttributes();
                    phDefinedValueAttributes.Controls.Clear();
                    Rock.Attribute.Helper.AddEditControls( definedValue, phDefinedValueAttributes, false );
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
                Rock.Web.Cache.DefinedTypeCache.Flush( definedTypeId );
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

            ShowReadonlyDetails( definedType );
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

            string descriptionFormat = "<dt>{0}</dt><dd>{1}</dd>";
            lblMainDetails.Text = @"
<div class='span5'>
    <dl>";

            lblMainDetails.Text += string.Format( descriptionFormat, "Name", definedType.Name );
            lblMainDetails.Text += string.Format( descriptionFormat, "Description", definedType.Description );

            lblMainDetails.Text += @"
    </dl>
</div>
<div class='span4'>
    <dl>";

            lblMainDetails.Text += string.Format( descriptionFormat, "Category", definedType.Category );
            definedType.FieldType = definedType.FieldType ?? new FieldTypeService().Get( definedType.FieldTypeId ?? 0 );
            lblMainDetails.Text += string.Format( descriptionFormat, "FieldType", definedType.FieldType.Name );

            lblMainDetails.Text += @"
    </dl>
</div>";

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
            BindDefinedValuesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveValue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveValue_Click( object sender, EventArgs e )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                DefinedValueService valueService = new DefinedValueService();

                DefinedValue definedValue;

                int definedValueId = hfDefinedValueId.ValueAsInt();
                if ( definedValueId == 0 )
                {
                    definedValue = new DefinedValue();
                    definedValue.IsSystem = false;
                    definedValue.DefinedTypeId = definedValueId;
                    valueService.Add( definedValue, CurrentPersonId );
                }
                else
                {
                    Rock.Web.Cache.AttributeCache.Flush( definedValueId );
                    definedValue = valueService.Get( definedValueId );
                }

                definedValue.Name = tbValueName.Text;
                definedValue.Description = tbValueDescription.Text;

                valueService.Save( definedValue, CurrentPersonId );
            }

            BindDefinedValuesGrid();
            pnlValues.Visible = true;
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
            string actionTitle;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                actionTitle = ActionTitle.Add( "attribute for defined type " + tbTypeName.Text );
            }
            else
            {
                AttributeService attributeService = new AttributeService();
                attribute = attributeService.Get( attributeGuid );
                actionTitle = ActionTitle.Edit( "attribute for defined type " + tbTypeName.Text );
            }

            edtDefinedTypeAttributes.EditAttribute( attribute, actionTitle );
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

                Rock.Web.Cache.AttributeCache.Flush( attribute.Id );
                attributeService.Delete( attribute, CurrentPersonId );
                attributeService.Save( attribute, CurrentPersonId );
            }

            BindDefinedTypeAttributesGrid();
            BindDefinedValuesGrid();
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
            Attribute attribute;
            AttributeService attributeService = new AttributeService();
            if ( edtDefinedTypeAttributes.AttributeId.Equals( 0 ) )
            {
                attribute = new Attribute();
            }
            else
            {
                attribute = attributeService.Get( edtDefinedTypeAttributes.AttributeId );
            }

            edtDefinedTypeAttributes.GetAttributeValues( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            RockTransactionScope.WrapTransaction( () =>
            {
                if ( attribute.Id.Equals( 0 ) )
                {
                    attribute.EntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( new DefinedValue().TypeName ).Id;
                    attribute.EntityTypeQualifierColumn = "DefinedTypeId";
                    attribute.EntityTypeQualifierValue = hfDefinedTypeId.Value;
                    attributeService.Add( attribute, CurrentPersonId );
                }

                Rock.Web.Cache.AttributeCache.Flush( attribute.Id );
                attributeService.Save( attribute, CurrentPersonId );
            } );

            pnlDetails.Visible = true;
            pnlDefinedTypeAttributes.Visible = false;

            BindDefinedTypeAttributesGrid();
            BindDefinedValuesGrid();
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

            int definedTypeId = hfDefinedTypeId.ValueAsInt();

            var qryDefinedTypeAttributes = attributeService.GetByEntityTypeId( new DefinedValue().TypeId ).AsQueryable()
                .Where( a => a.EntityTypeQualifierColumn.Equals( "DefinedTypeId", StringComparison.OrdinalIgnoreCase )
                && a.EntityTypeQualifierValue.Equals( definedTypeId.ToString() ) );

            gDefinedTypeAttributes.DataSource = qryDefinedTypeAttributes.OrderBy( a => a.Name ).ToList();
            gDefinedTypeAttributes.DataBind();
        }

        #endregion
        #region Edit Values

        /// <summary>
        /// Handles the Add event of the gDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gDefinedValues_Add( object sender, EventArgs e )
        {
            gDefinedValues_ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gDefinedValues_Edit( object sender, RowEventArgs e )
        {
            int definedValueId = (int)e.RowKeyValue;
            gDefinedValues_ShowEdit( definedValueId );
        }

        /// <summary>
        /// Handles the Delete event of the gDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gDefinedValues_Delete( object sender, RowEventArgs e )
        {
            var valueService = new DefinedValueService();

            DefinedValue value = valueService.Get( (int)e.RowKeyValue );

            if ( value != null )
            {
                valueService.Delete( value, CurrentPersonId );
                valueService.Save( value, CurrentPersonId );
            }

            BindDefinedValuesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gDefinedValues_GridRebind( object sender, EventArgs e )
        {
            BindDefinedValuesGrid();
        }

        /// <summary>
        /// Binds the defined values grid.
        /// </summary>
        protected void BindDefinedValuesGrid()
        {
            AttributeService attributeService = new AttributeService();

            int definedTypeId = hfDefinedTypeId.ValueAsInt();
            
            // add attributes with IsGridColumn to grid
            var qryDefinedTypeAttributes = attributeService.GetByEntityTypeId( new DefinedValue().TypeId ).AsQueryable()
                .Where( a => a.EntityTypeQualifierColumn.Equals( "DefinedTypeId", StringComparison.OrdinalIgnoreCase )
                && a.EntityTypeQualifierValue.Equals( definedTypeId.ToString() ) );

            qryDefinedTypeAttributes = qryDefinedTypeAttributes.Where( a => a.IsGridColumn );

            List<Attribute> gridItems = qryDefinedTypeAttributes.ToList();

            foreach ( var item in gDefinedValues.Columns.OfType<AttributeField>().ToList() )
            {
                gDefinedValues.Columns.Remove( item );
            }

            foreach ( var item in gridItems.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
            {
                string dataFieldExpression = item.Key;
                bool columnExists = gDefinedValues.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                if ( !columnExists )
                {
                    AttributeField boundField = new AttributeField();
                    boundField.DataField = dataFieldExpression;
                    boundField.HeaderText = item.Name;
                    boundField.SortExpression = string.Empty;
                    int insertPos = gDefinedValues.Columns.IndexOf(gDefinedValues.Columns.OfType<DeleteField>().First());
                    gDefinedValues.Columns.Insert(insertPos, boundField );
                }
            }

            var queryable = new DefinedValueService().Queryable().Where( a => a.DefinedTypeId == definedTypeId );

            SortProperty sortProperty = gDefinedValues.SortProperty;
            if ( sortProperty != null )
            {
                queryable = queryable.Sort( sortProperty );
            }
            else
            {
                queryable = queryable.OrderBy( a => a.Id );
            }

            var result = queryable.ToList();

            gDefinedValues.DataSource = result;
            gDefinedValues.DataBind();
        }

        /// <summary>
        /// Shows the edit value.
        /// </summary>
        /// <param name="valueId">The value id.</param>
        protected void gDefinedValues_ShowEdit( int valueId )
        {
            pnlDetails.Visible = false;
            pnlDefinedValueEditor.Visible = true;
            DefinedValue definedValue;
            if ( valueId.Equals( 0 ) )
            {
                definedValue = new DefinedValue { Id = 0 };
                definedValue.DefinedTypeId = hfDefinedTypeId.ValueAsInt();
                lActionTitleDefinedValue.Text = ActionTitle.Add( "defined value for " + tbTypeName.Text );
            }
            else
            {
                definedValue = new DefinedValueService().Get( valueId );
                lActionTitleDefinedValue.Text = ActionTitle.Edit( "defined value for " + tbTypeName.Text );
            }

            hfDefinedValueId.SetValue( definedValue.Id );
            tbValueName.Text = definedValue.Name;
            tbValueDescription.Text = definedValue.Description;
            definedValue.LoadAttributes();
            phDefinedValueAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( definedValue, phDefinedValueAttributes, true );
        }

        /// <summary>
        /// Handles the Click event of the btnSaveDefinedValue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveDefinedValue_Click( object sender, EventArgs e )
        {
            DefinedValue definedValue;
            DefinedValueService definedValueService = new DefinedValueService();

            int definedValueId = hfDefinedValueId.ValueAsInt();

            if ( definedValueId.Equals( 0 ) )
            {
                definedValue = new DefinedValue { Id = 0 };
                definedValue.DefinedTypeId = hfDefinedTypeId.ValueAsInt();
                definedValue.IsSystem = false;
            }
            else
            {
                definedValue = definedValueService.Get( definedValueId );
            }

            definedValue.Name = tbValueName.Text;
            definedValue.Description = tbValueDescription.Text;
            definedValue.LoadAttributes();
            Rock.Attribute.Helper.GetEditValues( phDefinedValueAttributes, definedValue );
            Rock.Attribute.Helper.SetErrorIndicators( phDefinedValueAttributes, definedValue );

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !definedValue.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            RockTransactionScope.WrapTransaction( () =>
            {
                if ( definedValue.Id.Equals( 0 ) )
                {
                    definedValueService.Add( definedValue, CurrentPersonId );
                }

                definedValueService.Save( definedValue, CurrentPersonId );
                Rock.Attribute.Helper.SaveAttributeValues( definedValue, CurrentPersonId );
            } );

            pnlDetails.Visible = true;
            pnlDefinedValueEditor.Visible = false;
            BindDefinedValuesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelDefinedValue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelDefinedValue_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = true;
            pnlDefinedValueEditor.Visible = false;
        }

        #endregion
    }
}