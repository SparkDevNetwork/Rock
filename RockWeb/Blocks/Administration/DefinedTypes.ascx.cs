//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Collections.Generic;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// User controls for managing defined types and their values
    /// </summary>
    public partial class DefinedTypes : RockBlock
    {
        #region Fields
        
        private int? _entityTypeId = null;
        private string _entityTypeName = string.Empty;
        private string _entityQualifier = string.Empty;
        protected bool _displayValueEdit = false;
        private bool _canConfigure = false;        
                
        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            try
            {
                _entityTypeName = AttributeValue( "DefinedType" );
                _entityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( _entityTypeName );
                _entityQualifier = "DefinedTypeId";
                _canConfigure = CurrentPage.IsAuthorized( "Administrate", CurrentPerson );

                BindFilter();
                tFilter.ApplyFilterClick += tFilter_ApplyFilterClick;

                if ( _canConfigure )
                {                    
                    //assign types grid actions
                    rGridType.DataKeyNames = new string[] { "id" };
                    rGridType.GridRebind += new GridRebindEventHandler( rGridType_GridRebind );
                    rGridType.Actions.IsAddEnabled = true;
                    rGridType.Actions.AddClick += rGridType_Add;
                    rGridType.GridRebind += rGridType_GridRebind;

                    //assign type values grid actions
                    rGridValue.DataKeyNames = new string[] { "id" };
                    rGridValue.GridRebind += new GridRebindEventHandler( rGridValue_GridRebind );
                    rGridValue.Actions.IsAddEnabled = true;
                    rGridValue.Actions.AddClick += rGridValue_Add;
                    rGridValue.GridRebind += rGridValue_GridRebind;

                    //assign attributes grid actions
                    rGridAttribute.DataKeyNames = new string[] { "id" };
                    rGridAttribute.GridRebind += new GridRebindEventHandler( rGridAttribute_GridRebind );
                    rGridAttribute.Actions.IsAddEnabled = true;
                    rGridAttribute.Actions.AddClick += rGridAttribute_Add;
                    rGridAttribute.GridRebind += rGridAttribute_GridRebind;

                    modalAttributes.SaveClick += btnSaveAttribute_Click;
                    modalAttributes.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdAttribute.ClientID );

                    modalValues.SaveClick += btnSaveValue_Click;
                    modalValues.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdValue.ClientID );

                    this.Page.ClientScript.RegisterStartupScript( this.GetType(), 
                        string.Format( "grid-confirm-delete-{0}", CurrentBlock.Id ), @"
                        Sys.Application.add_load(function () {{
                            $('td.grid-icon-cell.delete a').click(function(){{
                                return confirm('Are you sure you want to delete this value?');
                            }});
                        }});", true 
                    );

                }
                else
                {
                    DisplayError( "You are not authorized to configure this page" );
                }
            }
            catch ( SystemException ex )
            {
                DisplayError( ex.Message );
            }

            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack && _canConfigure )
                rGridType_Bind();
 
            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the tFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void tFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            tFilter.SaveUserPreference( "Category", ddlCategoryFilter.SelectedValue );

            rGridType_Bind();
        }

        protected void btnSaveType_Click( object sender, EventArgs e )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                DefinedTypeService typeService = new DefinedTypeService();

                DefinedType definedType;

                int typeId = (( hfIdType.Value ) != null && hfIdType.Value != String.Empty ) ? Int32.Parse( hfIdType.Value ) : 0;

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
                definedType.FieldTypeId = Int32.Parse( ddlTypeFieldType.SelectedValue );
                
                typeService.Save( definedType, CurrentPersonId );
            }

            rGridType_Bind();

            pnlTypeDetails.Visible = false;
            pnlTypes.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnCancelType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelType_Click( object sender, EventArgs e )
        {
            pnlTypeDetails.Visible = false;
            pnlTypes.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnSaveAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveAttribute_Click( object sender, EventArgs e )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                var attributeService = new AttributeService();
                var attributeQualifierService = new AttributeQualifierService();

                Rock.Model.Attribute attribute;

                int attributeId = ( ( hfIdAttribute.Value ) != null && hfIdAttribute.Value != String.Empty ) ? Int32.Parse( hfIdAttribute.Value ) : 0;
                if ( attributeId == 0 )
                {
                    attribute = new Rock.Model.Attribute();
                    attribute.IsSystem = false;
                    attribute.EntityTypeId = _entityTypeId;
                    attribute.EntityTypeQualifierColumn = _entityQualifier;
                    attribute.EntityTypeQualifierValue = hfIdType.Value;
                    attributeService.Add( attribute, CurrentPersonId );                    
                }
                else
                {
                    Rock.Web.Cache.AttributeCache.Flush( attributeId );
                    attribute = attributeService.Get( attributeId );
                }

                attribute.Key = tbAttributeKey.Text;
                attribute.Name = tbAttributeName.Text;
                attribute.Category = tbAttributeCategory.Text;
                attribute.Description = tbAttributeDescription.Text;
                attribute.FieldTypeId = Int32.Parse( ddlAttributeFieldType.SelectedValue );
                attribute.DefaultValue = tbAttributeDefaultValue.Text;
                attribute.IsGridColumn = cbAttributeGridColumn.Checked;
                attribute.IsRequired = cbAttributeRequired.Checked;

                attributeService.Save( attribute, CurrentPersonId );                
            }

            rGridAttribute_Bind( hfIdType.Value );

            modalAttributes.Hide();
        }

        /// <summary>
        /// Handles the Click event of the btnCloseAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCloseAttribute_Click( object sender, EventArgs e )
        {
            pnlAttributes.Visible = false;
            pnlTypes.Visible = true;
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

                int valueId = ( ( hfIdValue.Value ) != null && hfIdValue.Value != String.Empty ) ? Int32.Parse( hfIdValue.Value ) : 0;
                if ( valueId == 0 )
                {
                    definedValue = new DefinedValue();
                    definedValue.IsSystem = false;
                    definedValue.DefinedTypeId = Int32.Parse( hfIdType.Value );
                    valueService.Add( definedValue, CurrentPersonId );    
                }
                else
                {
                    Rock.Web.Cache.AttributeCache.Flush( valueId );
                    definedValue = valueService.Get( valueId );
                }

                definedValue.Name = tbValueName.Text;
                definedValue.Description = tbValueDescription.Text;     
                /* 
				
				
				Parse attribute values from controls list
				
				
				
				
				
				*/

                if ( phAttrControl.Controls.Count > 0 )
                { 
                    
                }

                valueService.Save( definedValue, CurrentPersonId );
            }

            rGridValue_Bind( hfIdType.Value );

            modalValues.Hide();
            pnlValues.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnCloseValue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCloseValue_Click( object sender, EventArgs e )
        {
            pnlValues.Visible = false;
            pnlTypes.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnRefresh control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnRefresh_Click( object sender, EventArgs e )
        {
            BindFilter();
            rGridType_Bind();
            rGridValue_Bind( hfIdType.Value );
            rGridAttribute_Bind( hfIdType.Value );
        }

        #endregion

        #region Edit Methods       
        /// <summary>
        /// Handles the Add event of the rGridType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGridType_Add( object sender, EventArgs e  )
        {
            ShowEditType( 0 );
        }

        /// <summary>
        /// Handles the EditValue event of the rGridType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CommandEventArgs" /> instance containing the event data.</param>
        protected void rGridType_EditValue( object sender, CommandEventArgs e )
        {
            
            hfIdType.Value = e.CommandArgument.ToString();            
            rGridValue_Bind( hfIdType.Value );            
            
            pnlTypes.Visible = false;
            pnlValues.Visible = true;
        }

        /// <summary>
        /// Handles the Edit event of the rGridType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGridType_Edit( object sender, RowEventArgs e )
        {
            hfIdType.Value = rGridType.DataKeys[e.RowIndex]["id"].ToString();
            ShowEditType( Int32.Parse(hfIdType.Value) );
        }

        /// <summary>
        /// Handles the EditAttribute event of the rGridType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGridType_EditAttribute( object sender, RowEventArgs e ) 
        {
            hfIdType.Value = rGridType.DataKeys[e.RowIndex]["id"].ToString();
            rGridAttribute_Bind( hfIdType.Value );

            pnlTypes.Visible = false;
            pnlAttributes.Visible = true;        
        }

        /// <summary>
        /// Handles the Delete event of the rGridType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGridType_Delete( object sender, RowEventArgs e )
        {
            DefinedType type = new DefinedTypeService().Get( (int)rGridType.DataKeys[e.RowIndex]["id"] );

            var valueService = new DefinedValueService();
            var typeService = new DefinedTypeService();


            if ( type != null )
            {
                // if this DefinedType has DefinedValues, delete them
                var hasDefinedValues = valueService
                .GetByDefinedTypeId( type.Id )
                .ToList();

                foreach ( var value in hasDefinedValues )
                {
                    valueService.Delete( value, CurrentPersonId );
                    valueService.Save( value, CurrentPersonId );
                }

                typeService.Delete( type, CurrentPersonId );
                typeService.Save( type, CurrentPersonId );
            }

            rGridType_Bind();
        }

        /// <summary>
        /// Handles the GridRebind event of the rGridType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void rGridType_GridRebind( object sender, EventArgs e )
        {
            rGridType_Bind();
        }

        /// <summary>
        /// Handles the Add event of the rGridAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGridAttribute_Add( object sender, EventArgs e )
        {
            ShowEditAttribute( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the rGridAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGridAttribute_Edit( object sender, RowEventArgs e )
        {
            hfIdAttribute.Value = rGridAttribute.DataKeys[e.RowIndex]["id"].ToString();
            ShowEditAttribute( Int32.Parse(hfIdAttribute.Value) );
            modalAttributes.Show();
        }

        /// <summary>
        /// Handles the Delete event of the rGridAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGridAttribute_Delete( object sender, RowEventArgs e )
        {
            var attributeService = new AttributeService();

            Rock.Model.Attribute attribute = attributeService.Get( (int)rGridAttribute.DataKeys[e.RowIndex]["id"] );
            if ( attribute != null )
            {
                attributeService.Delete( attribute, CurrentPersonId );
                attributeService.Save( attribute, CurrentPersonId );
            }

            rGridAttribute_Bind( hfIdType.Value );
        }

        /// <summary>
        /// Handles the GridRebind event of the rGridAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void rGridAttribute_GridRebind( object sender, EventArgs e )
        {
            rGridAttribute_Bind( hfIdType.Value );
        }

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
            hfIdValue.Value = rGridValue.DataKeys[e.RowIndex]["id"].ToString();
            ShowEditValue( Int32.Parse(hfIdValue.Value) );
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

            rGridValue_Bind( hfIdType.Value );
        }

        /// <summary>
        /// Handles the GridRebind event of the rGridValue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void rGridValue_GridRebind( object sender, EventArgs e )
        {
            rGridValue_Bind( hfIdType.Value );
        }
                                
        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            if ( ddlCategoryFilter.SelectedItem == null )
            {
                ddlCategoryFilter.Items.Clear();
                ddlCategoryFilter.Items.Add( "[All]" );

                DefinedTypeService typeService = new DefinedTypeService();
                var items = typeService.Queryable().
                    Where( a => a.Category != "" && a.Category != null ).
                    OrderBy( a => a.Category ).
                    Select( a => a.Category ).
                    Distinct().ToList();

                foreach ( var item in items )
                    ddlCategoryFilter.Items.Add( item );
            }
        }

        /// <summary>
        /// Binds the grid for defined types.
        /// </summary>
        private void rGridType_Bind()
        {
            var queryable = new DefinedTypeService().Queryable().
                Where( a => a.Category != "" && a.Category != null );
            
            if ( ddlCategoryFilter.SelectedValue != "[All]" )
                queryable = queryable.
                    Where( a => a.Category == ddlCategoryFilter.SelectedValue );

            SortProperty sortProperty = rGridType.SortProperty;
            if ( sortProperty != null )
                queryable = queryable.
                    Sort( sortProperty );
            else
                queryable = queryable.
                    OrderBy( a => a.Category );

            rGridType.DataSource = queryable.ToList();
            rGridType.DataBind();            
        }

        /// <summary>
        /// Binds the grid for defined values.
        /// </summary>
        /// <param name="typeId">The type id.</param>
        protected void rGridValue_Bind( string typeId )
        {
            int definedTypeId = Int32.Parse( typeId );
            
            var queryable = new DefinedValueService().Queryable().
                Where( a => a.DefinedTypeId == definedTypeId );

            SortProperty sortProperty = rGridValue.SortProperty;
            if ( sortProperty != null )
                queryable = queryable.
                    Sort( sortProperty );
            else
                queryable = queryable.
                    OrderBy( a => a.Id);

            rGridValue.DataSource = queryable.ToList();

            rGridValue.DataBind();
        }

        /// <summary>
        /// Binds the grid for type attributes.
        /// </summary>
        /// <param name="typeId">The type id.</param>
        protected void rGridAttribute_Bind( string typeId )
        {
            var queryable = new AttributeService().Queryable().
                Where( a => a.EntityTypeId == _entityTypeId &&
                ( a.EntityTypeQualifierColumn ?? string.Empty ) == _entityQualifier &&
                ( a.EntityTypeQualifierValue ?? string.Empty ) == typeId );

            SortProperty sortProperty = rGridAttribute.SortProperty;
            if ( sortProperty != null )
                queryable = queryable.
                    Sort( sortProperty );
            else 
                queryable = queryable.
                OrderBy( a => a.Category ).
                ThenBy( a => a.Key );

            rGridAttribute.DataSource = queryable.ToList();
            rGridAttribute.DataBind();
        }

        /// <summary>
        /// Shows the type of the edit.
        /// </summary>
        /// <param name="typeId">The type id.</param>
        protected void ShowEditType( int typeId )
        {
            var typeModel = new DefinedTypeService().Get( typeId );

            if ( typeModel != null )
            {
                var type = Rock.Web.Cache.DefinedTypeCache.Read( typeModel );
                lType.Text = "Editing " + typeModel.Name;
                hfIdType.Value = typeId.ToString();
                tbTypeName.Text = typeModel.Name;
                tbTypeCategory.Text = typeModel.Category;
                tbTypeDescription.Text = typeModel.Description;
                if (typeModel.FieldTypeId != null)
                    ddlTypeFieldType.SelectedValue = typeModel.FieldTypeId.ToString();
            }
            else
            {
                lType.Text = "Adding Defined Type";
                hfIdType.Value = string.Empty;
                tbTypeName.Text = string.Empty;
                tbTypeCategory.Text = string.Empty;
                tbTypeDescription.Text = string.Empty;
            }
                        
            pnlTypes.Visible = false;
            pnlTypeDetails.Visible = true;
        }

        /// <summary>
        /// Shows the edit attribute.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        protected void ShowEditAttribute( int attributeId )
        {
            var attributeModel = new AttributeService().Get( attributeId );

            if ( attributeModel != null )
            {
                var attribute = Rock.Web.Cache.AttributeCache.Read( attributeModel );
                hfIdAttribute.Value = attributeId.ToString();
                tbAttributeKey.Text = attribute.Key;
                tbAttributeName.Text = attribute.Name;
                tbAttributeCategory.Text = attribute.Category;
                tbAttributeDescription.Text = attribute.Description;
                tbAttributeDefaultValue.Text = attribute.DefaultValue;
                cbAttributeGridColumn.Checked = attribute.IsGridColumn;
                cbAttributeRequired.Checked = attribute.IsRequired;
                //if ( attribute.FieldType.Id != null )
                //{
                    ddlAttributeFieldType.SelectedValue = attribute.FieldType.Id.ToString();
                //}
            }
            else
            {
                hfIdAttribute.Value = string.Empty;
                tbAttributeKey.Text = string.Empty;
                tbAttributeName.Text = string.Empty;
                tbAttributeCategory.Text = string.Empty;
                tbAttributeDescription.Text = string.Empty;
                tbAttributeDefaultValue.Text = string.Empty;                
            }

            modalAttributes.Show();
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
            var value = Rock.Web.Cache.DefinedValueCache.Read( valueModel );
            var attributeList = attributeService
                .Get( _entityTypeId, _entityQualifier, hfIdType.Value )
                .ToArray();

            //int i = 0;
            // add control for each attribute associated with the defined type
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
                tbValueName.Text = value.Name;
                tbValueDescription.Text = value.Description;                
            }
            else
            {
                hfIdValue.Value = string.Empty;
                tbValueName.Text = string.Empty;
                tbValueDescription.Text = string.Empty;
            }
            
            modalValues.Show();
        }

        /// <summary>
        /// Displays the error messages.
        /// </summary>
        /// <param name="message">The message.</param>
        private void DisplayError( string message )
        {
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;

            pnlTypes.Visible = false;
            pnlValues.Visible = false;
            pnlAttributes.Visible = false;
        }

        #endregion
    }
}