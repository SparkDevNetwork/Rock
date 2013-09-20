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
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// User control for managing the attributes that are available for a specific entity
    /// </summary>
    [BooleanField("Configure Type", "Only show attributes for type specified below", true)]
    [EntityTypeField( "Entity", "Entity Name", false, "Applies To", 0 )]
    [TextField( "Entity Qualifier Column", "The entity column to evaluate when determining if this attribute applies to the entity", false, "", "Applies To", 1 )]
    [TextField( "Entity Qualifier Value", "The entity column value to evaluate.  Attributes will only apply to entities with this value", false, "", "Applies To", 2 )]
    [BooleanField( "Allow Setting of Values", "Should UI be available for setting values of the specified Entity ID?", false, "Set Values", 0 )]
    [IntegerField( "Entity Id", "The entity id that values apply to", false, 0, "Set Values", 1 )]

    public partial class Attributes : RockBlock
    {
        #region Fields

        protected bool _configuredType = true;
        protected int? _entityTypeId = null;
        protected string _entityQualifierColumn = string.Empty;
        protected string _entityQualifierValue = string.Empty;
        protected bool _displayValueEdit = false;
        protected int? _entityId = null;

        private bool _canConfigure = false;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if (!bool.TryParse( GetAttributeValue( "ConfigureType" ), out _configuredType))
            {
                _configuredType = true;
            }

            string entityTypeName = GetAttributeValue( "Entity" );
            _entityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( entityTypeName );
            _entityQualifierColumn = GetAttributeValue( "EntityQualifierColumn" );
            _entityQualifierValue = GetAttributeValue( "EntityQualifierValue" );
            _displayValueEdit = Convert.ToBoolean( GetAttributeValue( "AllowSettingofValues" ) );

            string entityIdString = GetAttributeValue( "EntityId" );
            if ( !string.IsNullOrWhiteSpace( entityIdString ) )
            {
                int entityIdint = 0;
                if ( int.TryParse( entityIdString, out entityIdint ) && entityIdint > 0 )
                {
                    _entityId = entityIdint;
                }
            }

            _canConfigure = CurrentPage.IsAuthorized( "Administrate", CurrentPerson );

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

            if ( _canConfigure )
            {
                rGrid.DataKeyNames = new string[] { "id" };
                rGrid.Actions.ShowAdd = true;

                rGrid.Actions.AddClick += rGrid_Add;
                rGrid.GridRebind += rGrid_GridRebind;
                rGrid.RowDataBound += rGrid_RowDataBound;

                rGrid.Columns[1].Visible = !_configuredType;
                rGrid.Columns[8].Visible = !_displayValueEdit;
                rGrid.Columns[9].Visible = _displayValueEdit;
                rGrid.Columns[10].Visible = _displayValueEdit;

                SecurityField securityField = rGrid.Columns[11] as SecurityField;
                securityField.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Attribute ) ).Id;

                modalDetails.SaveClick += modalDetails_SaveClick;
                modalDetails.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdValues.ClientID );

                string editAttributeId = Request.Form[hfIdValues.UniqueID];
                if ( Page.IsPostBack && editAttributeId != null && editAttributeId.Trim() != string.Empty )
                {
                    ShowEditValue( int.Parse( editAttributeId ), false );
                }

                if ( !_configuredType )
                {
                    ddlEntityType.Items.Clear();
                    ddlEntityType.Items.Add( new ListItem( "None (Global Attributes)", None.IdValue ) );

                    ddlAttrEntityType.Items.Clear();
                    ddlAttrEntityType.Items.Add( new ListItem( "None (Global Attribute)", None.IdValue ) );

                    new EntityTypeService().GetEntityListItems().ForEach( l =>
                    {
                        ddlEntityType.Items.Add( l );
                        ddlAttrEntityType.Items.Add( l );
                    } );
                }

                BindFilter();

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
            if ( !Page.IsPostBack && _canConfigure )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlEntityType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlEntityType_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindCategoryFilter();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            if ( !_configuredType )
            {
                rFilter.SaveUserPreference( "Entity Type", ddlEntityType.SelectedValue );
            }

            string categoryFilterValue = cpCategoriesFilter.SelectedValuesAsInt()
                .Where( v => v != 0 )
                .Select( c => c.ToString() )
                .ToList()
                .AsDelimited( "," );

            rFilter.SaveUserPreference( "Categories", categoryFilterValue );

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
                case "Categories":

                    var categories = new List<string>();

                    foreach ( var idVal in e.Value.SplitDelimitedValues() )
                    {
                        int id = int.MinValue;
                        if ( int.TryParse( idVal, out id ) )
                        {
                            if ( id != 0 )
                            {
                                var category = CategoryCache.Read( id );
                                if ( category != null )
                                {
                                    categories.Add( CategoryCache.Read( id ).Name );
                                }
                            }
                        }
                    }

                    e.Value = categories.AsDelimited( ", " );

                    break;

                case "Entity Type":

                    if ( _configuredType )
                    {
                        e.Value = "";
                    }

                    else
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

                default:
                    e.Value = "";
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
            ShowEdit( (int)rGrid.DataKeys[e.RowIndex]["id"] );
        }

        /// <summary>
        /// Handles the EditValue event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_EditValue( object sender, RowEventArgs e )
        {
            if ( _displayValueEdit )
            {
                ShowEditValue( (int)rGrid.DataKeys[e.RowIndex]["id"], true );
            }
        }

        /// <summary>
        /// Handles the Delete event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            var attributeService = new Rock.Model.AttributeService();

            Rock.Model.Attribute attribute = attributeService.Get( (int)rGrid.DataKeys[e.RowIndex]["id"] );
            if ( attribute != null )
            {
                Rock.Web.Cache.AttributeCache.Flush( attribute.Id );

                attributeService.Delete( attribute, CurrentPersonId );
                attributeService.Save( attribute, CurrentPersonId );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGrid_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
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
                int attributeId = (int)rGrid.DataKeys[e.Row.RowIndex].Value;

                var attribute = Rock.Web.Cache.AttributeCache.Read( attributeId );
                var fieldType = Rock.Web.Cache.FieldTypeCache.Read( attribute.FieldTypeId );

                Literal lCategories = e.Row.FindControl( "lCategories" ) as Literal;
                if ( lCategories != null )
                {
                    lCategories.Text = attribute.Categories.Select( c => c.Name ).ToList().AsDelimited( ", " );
                }

                Literal lEntityQualifier = e.Row.FindControl( "lEntityQualifier" ) as Literal;
                if ( lEntityQualifier != null )
                {
                    if ( attribute.EntityTypeId.HasValue )
                    {
                        string entityTypeName = EntityTypeCache.Read( attribute.EntityTypeId.Value ).FriendlyName;
                        if ( !string.IsNullOrWhiteSpace( attribute.EntityTypeQualifierColumn ) )
                        {
                            lEntityQualifier.Text = string.Format( "Where [{0}] = '{1}'", attribute.EntityTypeQualifierColumn, attribute.EntityTypeQualifierValue );
                        }
                        else
                        {
                            lEntityQualifier.Text = entityTypeName;
                        }
                    }
                    else
                    {
                        lEntityQualifier.Text = "Global Attribute";
                    }
                }

                Literal lDescription = e.Row.FindControl( "lDescription" ) as Literal;
                if ( lDescription != null )
                {
                    lDescription.Text = attribute.Description.Truncate( 100 );
                }

                if ( _displayValueEdit )
                {
                    Literal lValue = e.Row.FindControl( "lValue" ) as Literal;
                    if ( lValue != null )
                    {
                        AttributeValueService attributeValueService = new AttributeValueService();
                        var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attributeId, _entityId ).FirstOrDefault();
                        if ( attributeValue != null && !string.IsNullOrWhiteSpace( attributeValue.Value ) )
                        {
                            lValue.Text = fieldType.Field.FormatValue( lValue, attributeValue.Value, attribute.QualifierValues, true );
                        }
                        else
                        {
                            lValue.Text = string.Format( "<span class='muted'>{0}</span>", fieldType.Field.FormatValue( lValue, attribute.DefaultValue, attribute.QualifierValues, true ) );
                        }
                    }
                }
                else
                {
                    Literal lDefaultValue = e.Row.FindControl( "lDefaultValue" ) as Literal;
                    if ( lDefaultValue != null )
                    {
                        lDefaultValue.Text = fieldType.Field.FormatValue( lDefaultValue, attribute.DefaultValue, attribute.QualifierValues, true );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            using ( new UnitOfWorkScope() )
            {
                Rock.Model.Attribute attribute = null;

                RockTransactionScope.WrapTransaction( () =>
                {
                    var attributeService = new AttributeService();

                    // remove old qualifier values in case they changed
                    if ( edtAttribute.AttributeId.HasValue )
                    {
                        AttributeQualifierService attributeQualifierService = new AttributeQualifierService();
                        foreach ( var oldQualifier in attributeQualifierService.GetByAttributeId( edtAttribute.AttributeId.Value ).ToList() )
                        {
                            attributeQualifierService.Delete( oldQualifier, CurrentPersonId );
                            attributeQualifierService.Save( oldQualifier, CurrentPersonId );
                        }
                        attribute = attributeService.Get( edtAttribute.AttributeId.Value );
                    }

                    if ( attribute == null )
                    {
                        attribute = new Rock.Model.Attribute();
                        attributeService.Add( attribute, CurrentPersonId );
                    }

                    if ( _configuredType )
                    {
                        attribute.EntityTypeId = _entityTypeId;
                        attribute.EntityTypeQualifierColumn = _entityQualifierColumn;
                        attribute.EntityTypeQualifierValue = _entityQualifierValue;
                    }
                    else
                    {
                        attribute.EntityTypeId = ddlAttrEntityType.SelectedValueAsInt();
                        attribute.EntityTypeQualifierColumn = tbAttrQualifierField.Text;
                        attribute.EntityTypeQualifierValue = tbAttrQualifierValue.Text;
                    }

                    edtAttribute.GetAttributeProperties( attribute );

                    // Controls will show warnings
                    if ( !attribute.IsValid )
                    {
                        return;
                    }

                    attributeService.Save( attribute, CurrentPersonId );

                } );

                if ( attribute != null )
                {
                    Rock.Web.Cache.AttributeCache.Flush( attribute.Id );
                    if ( !_entityTypeId.HasValue && _entityQualifierColumn == string.Empty && _entityQualifierValue == string.Empty && !_entityId.HasValue )
                    {
                        Rock.Web.Cache.GlobalAttributesCache.Flush();
                    }
                }

            }

            BindGrid();

            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        #endregion

        /// <summary>
        /// Handles the SaveClick event of the modalDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void modalDetails_SaveClick( object sender, EventArgs e )
        {
            if ( _displayValueEdit )
            {
                int attributeId = 0;
                if ( hfIdValues.Value != string.Empty && !int.TryParse( hfIdValues.Value, out attributeId ) )
                {
                    attributeId = 0;
                }

                if ( attributeId != 0 && phEditControl.Controls.Count > 0 )
                {
                    var attribute = Rock.Web.Cache.AttributeCache.Read( attributeId );

                    AttributeValueService attributeValueService = new AttributeValueService();
                    var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attributeId, _entityId ).FirstOrDefault();
                    if ( attributeValue == null )
                    {
                        attributeValue = new Rock.Model.AttributeValue();
                        attributeValue.AttributeId = attributeId;
                        attributeValue.EntityId = _entityId;
                        attributeValueService.Add( attributeValue, CurrentPersonId );
                    }

                    var fieldType = Rock.Web.Cache.FieldTypeCache.Read( attribute.FieldType.Id );
                    attributeValue.Value = fieldType.Field.GetEditValue( phEditControl.Controls[0], attribute.QualifierValues );

                    attributeValueService.Save( attributeValue, CurrentPersonId );

                    Rock.Web.Cache.AttributeCache.Flush( attributeId );
                    if ( !_entityTypeId.HasValue && _entityQualifierColumn == string.Empty && _entityQualifierValue == string.Empty && !_entityId.HasValue )
                    {
                        Rock.Web.Cache.GlobalAttributesCache.Flush();
                    }
                }

                hfIdValues.Value = string.Empty;

                modalDetails.Hide();
            }

            BindGrid();
        }

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlEntityType.Visible = !_configuredType;
            ddlEntityType.SelectedValue = rFilter.GetUserPreference( "Entity Type" );
            BindCategoryFilter();
        }

        /// <summary>
        /// Binds the category filter.
        /// </summary>
        private void BindCategoryFilter()
        {
            int? entityTypeId = _configuredType ? _entityTypeId : ddlEntityType.SelectedValueAsInt();

            cpCategoriesFilter.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Attribute ) ).Id;
            cpCategoriesFilter.EntityTypeQualifierColumn = "EntityTypeId";
            cpCategoriesFilter.EntityTypeQualifierValue = entityTypeId.ToString();

            var selectedIDs = new List<int>();
            foreach ( var idVal in rFilter.GetUserPreference( "Categories" ).SplitDelimitedValues() )
            {
                int id = int.MinValue;
                if ( int.TryParse( idVal, out id ) )
                {
                    selectedIDs.Add( id );
                }
            }

            cpCategoriesFilter.SetValues( selectedIDs );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            IQueryable<Rock.Model.Attribute> query;

            AttributeService attributeService = new AttributeService();
            if ( _configuredType )
            {
                query = attributeService.Get( _entityTypeId, _entityQualifierColumn, _entityQualifierValue);
            }
            else
            {
                query = attributeService.GetByEntityTypeId( ddlEntityType.SelectedValueAsInt() );
            }

            List<int> selectedCategoryIds = cpCategoriesFilter.SelectedValuesAsInt().Where( v => v != 0).ToList();
            if ( selectedCategoryIds.Any() )
            {
                query = query.
                    Where( a => a.Categories.Any( c => selectedCategoryIds.Contains( c.Id ) ) );
            }

            SortProperty sortProperty = rGrid.SortProperty;
            if ( sortProperty != null )
            {
                query = query.
                    Sort( sortProperty );
            }
            else
            {
                query = query.
                    OrderBy( a => a.Key );
            }

            rGrid.DataSource = query.ToList();
            rGrid.DataBind();
        }


        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        protected void ShowEdit( int attributeId )
        {
            var attributeModel = new AttributeService().Get( attributeId );

            edtAttribute.AttributeEntityTypeId = _entityTypeId;

            if ( attributeModel == null )
            {
                attributeModel = new Rock.Model.Attribute();

                if ( !_configuredType )
                {
                    int entityTypeId = int.MinValue;
                    if ( int.TryParse( rFilter.GetUserPreference( "Entity Type" ), out entityTypeId ) && entityTypeId > 0 )
                    {
                        attributeModel.EntityTypeId = entityTypeId;
                    }
                }

                List<int> selectedCategoryIds = cpCategoriesFilter.SelectedValuesAsInt().ToList();
                new CategoryService().Queryable().Where( c => selectedCategoryIds.Contains( c.Id ) ).ToList().ForEach( c =>
                    attributeModel.Categories.Add( c ) );
                edtAttribute.ActionTitle = Rock.Constants.ActionTitle.Add( Rock.Model.Attribute.FriendlyTypeName );
            }
            else
            {
                edtAttribute.ActionTitle = Rock.Constants.ActionTitle.Edit( Rock.Model.Attribute.FriendlyTypeName );
            }

            Type type = null;
            if ( _entityTypeId.HasValue )
            {
                type = EntityTypeCache.Read( _entityTypeId.Value ).GetEntityType();
            }
            edtAttribute.SetAttributeProperties( attributeModel, type  );

            if ( _configuredType )
            {
                ddlAttrEntityType.Visible = false;
                tbAttrQualifierField.Visible = false;
                tbAttrQualifierValue.Visible = false;
            }
            else
            {
                ddlAttrEntityType.Visible = true;
                tbAttrQualifierField.Visible = true;
                tbAttrQualifierValue.Visible = true;

                ddlAttrEntityType.SelectedValue = ( attributeModel.EntityTypeId ?? 0 ).ToString();
                tbAttrQualifierField.Text = attributeModel.EntityTypeQualifierColumn;
                tbAttrQualifierValue.Text = attributeModel.EntityTypeQualifierValue;
            }

            pnlList.Visible = false;
            pnlDetails.Visible = true;
        }

        /// <summary>
        /// Shows the edit value.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        protected void ShowEditValue( int attributeId, bool setValues )
        {
            if ( _displayValueEdit )
            {
                var attribute = Rock.Web.Cache.AttributeCache.Read( attributeId );

                hfIdValues.Value = attribute.Id.ToString();
                lCaption.Text = attribute.Name;

                AttributeValueService attributeValueService = new AttributeValueService();
                var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attributeId, _entityId ).FirstOrDefault();

                var fieldType = Rock.Web.Cache.FieldTypeCache.Read( attribute.FieldType.Id );

                Control editControl = fieldType.Field.EditControl( attribute.QualifierValues, string.Format( "attribute_field_{0}", attribute.Id ) );
                editControl.ClientIDMode = ClientIDMode.AutoID;

                if ( setValues && attributeValue != null )
                {
                    fieldType.Field.SetEditValue( editControl, attribute.QualifierValues, attributeValue.Value );
                }

                phEditControl.Controls.Clear();
                phEditControl.Controls.Add( editControl );

                modalDetails.Show();
            }
        }

        #endregion
}
}