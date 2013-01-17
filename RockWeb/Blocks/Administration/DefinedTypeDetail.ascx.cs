//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
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
        /// <summary>
        /// Gets or sets the state of the defined type attributes.
        /// </summary>
        /// <value>
        /// The state of the defined type attributes.
        /// </value>
        private ViewStateList<Attribute> DefinedTypeAttributesState
        {
            get
            {
                return ViewState["DefinedTypeAttributesState"] as ViewStateList<Attribute>;
            }

            set
            {
                ViewState["DefinedTypeAttributesState"] = value;
            }
        }


        #region Fields

        private int? _entityTypeId = null;
        private string _entityTypeName = string.Empty;
        private string _entityQualifier = string.Empty;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _entityTypeName = GetAttributeValue( "DefinedType" );
            _entityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( _entityTypeName );
            _entityQualifier = "DefinedTypeId";

            // assign type values grid actions
            rGridValue.DataKeyNames = new string[] { "id" };
            rGridValue.Actions.IsAddEnabled = true;
            rGridValue.Actions.AddClick += rGridValue_Add;
            rGridValue.GridRebind += rGridValue_GridRebind;

            // assign attributes grid actions
            gDefinedTypeAttributes.DataKeyNames = new string[] { "id" };
            gDefinedTypeAttributes.Actions.IsAddEnabled = true;
            gDefinedTypeAttributes.Actions.AddClick += gDefinedTypeAttributes_Add;
            gDefinedTypeAttributes.GridRebind += gDefinedTypeAttributes_GridRebind;

            modalValues.SaveClick += btnSaveValue_Click;
            modalValues.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdValue.ClientID );
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
                    pnlTypeDetails.Visible = false;
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
            AttributeService attributeService = new AttributeService();

            int typeId = int.Parse( hfIdType.Value );

            if ( typeId == 0 )
            {
                definedType = new DefinedType();
                definedType.IsSystem = false;
                definedType.Order = 0;
                typeService.Add( definedType, CurrentPersonId );
            }
            else
            {
                Rock.Web.Cache.DefinedTypeCache.Flush( typeId );
                definedType = typeService.Get( typeId );
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

                // get it back to make sure we have a good Id for it for the Attributes
                definedType = typeService.Get( definedType.Guid );
                hfIdType.Value = definedType.Id.ToString();

                /* Take care of Defined Type Attributes */

                // delete DefinedTypeAttributes that are no longer configured in the UI
                var definedTypeAttributesQry = attributeService.GetByEntityTypeId( new DefinedType().TypeId ).AsQueryable()
                    .Where( a => a.EntityTypeQualifierColumn.Equals( "Id", StringComparison.OrdinalIgnoreCase )
                    && a.EntityTypeQualifierValue.Equals( definedType.Id.ToString() ) );

                var deletedDefinedTypeAttributes = from attr in definedTypeAttributesQry
                                                   where !( from d in DefinedTypeAttributesState
                                                            select d.Guid ).Contains( attr.Guid )
                                                   select attr;

                deletedDefinedTypeAttributes.ToList().ForEach( a =>
                {
                    var attr = attributeService.Get( a.Guid );
                    attributeService.Delete( attr, CurrentPersonId );
                    attributeService.Save( attr, CurrentPersonId );
                } );

                // add/update the DefinedTypeAttributes that are assigned in the UI
                foreach ( var attributeState in DefinedTypeAttributesState )
                {
                    Attribute attribute = definedTypeAttributesQry.FirstOrDefault( a => a.Guid.Equals( attributeState.Guid ) );
                    if ( attribute == null )
                    {
                        attribute = attributeState.Clone() as Rock.Model.Attribute;
                        attributeService.Add( attribute, CurrentPersonId );
                    }
                    else
                    {
                        attributeState.Id = attribute.Id;
                        attribute.FromDictionary( attributeState.ToDictionary() );
                    }

                    attribute.EntityTypeQualifierColumn = "Id";
                    attribute.EntityTypeQualifierValue = definedType.Id.ToString();
                    attribute.EntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( new DefinedType().TypeName ).Id;
                    attributeService.Save( attribute, CurrentPersonId );
                }
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
            if ( hfIdType.Value.Equals( "0" ) )
            {
                // Cancelling on Add.  Return to Grid
                NavigateToParentPage();
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                DefinedTypeService definedTypeService = new DefinedTypeService();
                DefinedType definedType = definedTypeService.Get( int.Parse( hfIdType.Value ) );
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
            Rock.Attribute.Helper.AddDisplayControls( definedType, phDefinedTypeAttributesReadOnly );
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

            DefinedTypeAttributesState = new ViewStateList<Attribute>();

            tbTypeName.Text = definedType.Name;
            tbTypeCategory.Text = definedType.Category;
            tbTypeDescription.Text = definedType.Description;
            ddlTypeFieldType.SetValue( definedType.FieldTypeId );

            AttributeService attributeService = new AttributeService();

            var qryDefinedTypeAttributes = attributeService.GetByEntityTypeId( new DefinedType().TypeId ).AsQueryable()
                .Where( a => a.EntityTypeQualifierColumn.Equals( "Id", StringComparison.OrdinalIgnoreCase )
                && a.EntityTypeQualifierValue.Equals( definedType.Id.ToString() ) );

            DefinedTypeAttributesState.AddAll( qryDefinedTypeAttributes.ToList() );
            BindDefinedTypeAttributesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            DefinedTypeService definedTypeService = new DefinedTypeService();
            DefinedType definedType = definedTypeService.Get( int.Parse( hfIdType.Value ) );
            ShowEditDetails( definedType );
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

            pnlTypeDetails.Visible = true;
            DefinedType definedType = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                definedType = new DefinedTypeService().Get( itemKeyValue );
            }
            else
            {
                definedType = new DefinedType { Id = 0 };
            }

            hfIdType.Value = definedType.Id.ToString();

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

            rGridValue_Bind( int.Parse( hfIdType.Value ) );
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

                int valueId = int.Parse( hfIdValue.Value );
                if ( valueId == 0 )
                {
                    definedValue = new DefinedValue();
                    definedValue.IsSystem = false;
                    definedValue.DefinedTypeId = int.Parse( hfIdType.Value );
                    valueService.Add( definedValue, CurrentPersonId );
                }
                else
                {
                    Rock.Web.Cache.AttributeCache.Flush( valueId );
                    definedValue = valueService.Get( valueId );
                }

                definedValue.Name = tbValueName.Text;
                definedValue.Description = tbValueDescription.Text;

                valueService.Save( definedValue, CurrentPersonId );
            }
           
            rGridValue_Bind( int.Parse( hfIdType.Value ) );

            modalValues.Hide();
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
            pnlTypeDetails.Visible = false;
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
                attribute = DefinedTypeAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
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
            DefinedTypeAttributesState.RemoveEntity( attributeGuid );

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
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtDefinedTypeAttributes.GetAttributeValues( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            DefinedTypeAttributesState.RemoveEntity( attribute.Guid );
            DefinedTypeAttributesState.Add( attribute );

            pnlTypeDetails.Visible = true;
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
            pnlTypeDetails.Visible = true;
            pnlDefinedTypeAttributes.Visible = false;
        }

        /// <summary>
        /// Binds the defined type attributes grid.
        /// </summary>
        private void BindDefinedTypeAttributesGrid()
        {
            gDefinedTypeAttributes.DataSource = DefinedTypeAttributesState.OrderBy( a => a.Name ).ToList();
            gDefinedTypeAttributes.DataBind();
        }

        #endregion
        #region Edit Values

        /// <summary>
        /// Handles the Add event of the rGridValue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGridValue_Add( object sender, EventArgs e )
        {
            ShowEditValue( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the rGridValue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGridValue_Edit( object sender, RowEventArgs e )
        {
            int definedValueId = (int)e.RowKeyValue;
            hfIdValue.Value = definedValueId.ToString();
            ShowEditValue( definedValueId );
        }

        /// <summary>
        /// Handles the Delete event of the rGridValue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGridValue_Delete( object sender, RowEventArgs e )
        {
            var valueService = new DefinedValueService();

            DefinedValue value = valueService.Get( (int)rGridValue.DataKeys[e.RowIndex]["id"] );

            if ( value != null )
            {
                valueService.Delete( value, CurrentPersonId );
                valueService.Save( value, CurrentPersonId );
            }

            rGridValue_Bind( int.Parse( hfIdType.Value ) );
        }

        /// <summary>
        /// Handles the GridRebind event of the rGridValue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGridValue_GridRebind( object sender, EventArgs e )
        {
            rGridValue_Bind( int.Parse( hfIdType.Value ) );
        }

        /// <summary>
        /// Rs the grid value_ bind.
        /// </summary>
        /// <param name="definedTypeId">The defined type id.</param>
        protected void rGridValue_Bind( int definedTypeId )
        {
            var queryable = new DefinedValueService().Queryable().Where( a => a.DefinedTypeId == definedTypeId );

            SortProperty sortProperty = rGridValue.SortProperty;
            if ( sortProperty != null )
            {
                queryable = queryable.Sort( sortProperty );
            }
            else
            {
                queryable = queryable.OrderBy( a => a.Id );
            }

            rGridValue.DataSource = queryable.ToList();
            rGridValue.DataBind();
        }

        /// <summary>
        /// Shows the edit value.
        /// </summary>
        /// <param name="valueId">The value id.</param>
        protected void ShowEditValue( int valueId )
        {
            phAttrControl.Controls.Clear();

            AttributeValueService attributeValueService = new AttributeValueService();
            AttributeService attributeService = new AttributeService();

            var valueModel = new DefinedValueService().Get( valueId );
            var attributeList = attributeService
                .Get( _entityTypeId, _entityQualifier, hfIdType.Value )
                .ToArray();

            // TODO add control for each attribute associated with the defined type
            foreach ( var attribute in attributeList )
            {
                var fieldType = Rock.Web.Cache.FieldTypeCache.Read( attribute.FieldType );
                if ( fieldType != null )
                {
                    var configControls = fieldType.Field.ConfigurationControls();

                    /*
                    
                    var ctrlGroup = new HtmlGenericControl( "div" );
                    phAttrControl.Controls.Add( ctrlGroup );
                    ctrlGroup.AddCssClass( "control-group" );

                    var lbl = new Label();
                    ctrlGroup.Controls.Add( lbl );
                    lbl.AddCssClass( "control-label" );
                    lbl.Text = attribute.Name;

                    var ctrls = new HtmlGenericControl( "div" );
                    ctrlGroup.Controls.Add( ctrls );
                    ctrls.AddCssClass( "controls" );

                    Control control = configControls[i];
                    ctrls.Controls.Add( control );
                    control.ID = "configControl_" + attribute.Key;
                    i++;
                     
                     
                    */
                }
            }

            if ( valueModel != null )
            {
                hfIdValue.Value = valueId.ToString();
                tbValueName.Text = valueModel.Name;
                tbValueDescription.Text = valueModel.Description;
            }
            else
            {
                hfIdValue.Value = string.Empty;
                tbValueName.Text = string.Empty;
                tbValueDescription.Text = string.Empty;
            }

            modalValues.Show();
        }

        #endregion
    }
}