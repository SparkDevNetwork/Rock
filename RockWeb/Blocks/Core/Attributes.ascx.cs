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
using System.Net;

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
    /// User control for managing the attributes that are available for a specific entity
    /// </summary>
    [DisplayName( "Attributes" )]
    [Category( "Core" )]
    [Description( "Allows for the managing of attribues." )]

    [BooleanField( "Configure Type", "Only show attributes for type specified below", true )]
    [EntityTypeField( "Entity", "Entity Name", false, "Applies To", 0 )]
    [TextField( "Entity Qualifier Column", "The entity column to evaluate when determining if this attribute applies to the entity", false, "", "Applies To", 1 )]
    [TextField( "Entity Qualifier Value", "The entity column value to evaluate.  Attributes will only apply to entities with this value", false, "", "Applies To", 2 )]
    [BooleanField( "Allow Setting of Values", "Should UI be available for setting values of the specified Entity ID?", false, "Advanced", 0 )]
    [IntegerField( "Entity Id", "The entity id that values apply to", false, 0, "Advanced", 1 )]
    [BooleanField( "Enable Show In Grid", "Should the 'Show In Grid' option be displayed when editing attributes?", false, "Advanced", 2 )]

    public partial class Attributes : RockBlock
    {
        #region Fields

        private bool _configuredType = true;
        private int? _entityTypeId = null;
        private string _entityQualifierColumn = string.Empty;
        private string _entityQualifierValue = string.Empty;
        private bool _displayValueEdit = false;
        private int? _entityId = null;
        private bool _canConfigure = false;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _configuredType = GetAttributeValue( "ConfigureType" ).AsBooleanOrNull() ?? true;
            edtAttribute.ShowInGridVisible = GetAttributeValue( "DisplayShowInGrid" ).AsBooleanOrNull() ?? false;

            Guid? entityTypeGuid = GetAttributeValue( "Entity" ).AsGuidOrNull();
            if ( entityTypeGuid.HasValue )
            {
                _entityTypeId = EntityTypeCache.Read( entityTypeGuid.Value ).Id;
            }

            _entityQualifierColumn = GetAttributeValue( "EntityQualifierColumn" );
            _entityQualifierValue = GetAttributeValue( "EntityQualifierValue" );
            _displayValueEdit = GetAttributeValue( "AllowSettingofValues" ).AsBooleanOrNull() ?? false;

            _entityId = GetAttributeValue( "EntityId" ).AsIntegerOrNull();
            if ( _entityId == 0 )
            {
                _entityId = null;
            }

            _canConfigure = IsUserAuthorized( Rock.Security.Authorization.ADMINISTRATE );

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

            if ( _canConfigure )
            {
                rGrid.DataKeyNames = new string[] { "Id" };
                rGrid.Actions.ShowAdd = true;

                rGrid.Actions.AddClick += rGrid_Add;
                rGrid.GridRebind += rGrid_GridRebind;
                rGrid.RowDataBound += rGrid_RowDataBound;

                rGrid.Columns[1].Visible = !_configuredType;   // qualifier

                rGrid.Columns[4].Visible = !_displayValueEdit; // default value / value
                rGrid.Columns[5].Visible = _displayValueEdit; // default value / value
                rGrid.Columns[6].Visible = _displayValueEdit;  // edit

                SecurityField securityField = rGrid.Columns[7] as SecurityField;
                securityField.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Attribute ) ).Id;

                mdAttribute.SaveClick += mdAttribute_SaveClick;
                mdAttributeValue.SaveClick += mdAttributeValue_SaveClick;

                if ( !_configuredType )
                {
                    var entityTypeList = new EntityTypeService( new RockContext() ).GetEntities().ToList();
                    ddlEntityType.EntityTypes = entityTypeList;
                    ddlAttrEntityType.EntityTypes = entityTypeList;
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
            if ( !Page.IsPostBack )
            {
                if ( _canConfigure )
                {
                    BindGrid();
                }
            }
            else
            {
                int? attributeId = hfIdValues.Value.AsIntegerOrNull();
                if ( attributeId.HasValue )
                {
                    ShowEditValue( attributeId.Value, false );
                }

                if ( hfActiveDialog.Value.ToUpper() == "ATTRIBUTEVALUE" )
                {
                    //
                }

                ShowDialog();
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
                        e.Value = string.Empty;
                    }
                    else
                    {
                        if ( e.Value == "0" )
                        {
                            e.Value = "None (Global Attributes)";
                        }
                        else
                        {
                            e.Value = EntityTypeCache.Read( e.Value.AsInteger() ).FriendlyName;
                        }
                    }

                    break;

                default:
                    e.Value = string.Empty;
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
        /// Handles the EditValue event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_RowSelected( object sender, RowEventArgs e )
        {
            if ( _displayValueEdit )
            {
                ShowEditValue( e.RowKeyId, true );
            }
            else
            {
                ShowEdit( e.RowKeyId );
            }
        }

        /// <summary>
        /// Handles the Delete event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var attributeService = new Rock.Model.AttributeService( rockContext );

            Rock.Model.Attribute attribute = attributeService.Get( e.RowKeyId );
            if ( attribute != null )
            {
                Rock.Web.Cache.AttributeCache.Flush( attribute.Id );

                attributeService.Delete( attribute );

                rockContext.SaveChanges();
            }

            AttributeCache.FlushEntityAttributes();

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

                if ( _displayValueEdit )
                {
                    Literal lValue = e.Row.FindControl( "lValue" ) as Literal;
                    if ( lValue != null )
                    {
                        AttributeValueService attributeValueService = new AttributeValueService( new RockContext() );
                        var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attributeId, _entityId );
                        if ( attributeValue != null && !string.IsNullOrWhiteSpace( attributeValue.Value ) )
                        {
                            lValue.Text = fieldType.Field.FormatValueAsHtml( lValue, attributeValue.Value, attribute.QualifierValues, true );
                        }
                        else
                        {
                            lValue.Text = string.Format( "<span class='text-muted'>{0}</span>", fieldType.Field.FormatValueAsHtml( lValue, attribute.DefaultValue, attribute.QualifierValues, true ) );
                        }
                    }
                }
                else
                {
                    Literal lDefaultValue = e.Row.FindControl( "lDefaultValue" ) as Literal;
                    if ( lDefaultValue != null )
                    {
                        lDefaultValue.Text = fieldType.Field.FormatValueAsHtml( lDefaultValue, attribute.DefaultValue, attribute.QualifierValues, true );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlAttrEntityType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlAttrEntityType_SelectedIndexChanged( object sender, EventArgs e )
        {
            edtAttribute.AttributeEntityTypeId = ddlAttrEntityType.SelectedValueAsInt( false );
            edtAttribute.CategoryIds = new List<int>();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void mdAttribute_SaveClick( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = null;

            if ( _configuredType )
            {
                attribute = Rock.Attribute.Helper.SaveAttributeEdits( edtAttribute, _entityTypeId, _entityQualifierColumn, _entityQualifierValue );
            }
            else
            {
                attribute = Rock.Attribute.Helper.SaveAttributeEdits( edtAttribute, ddlAttrEntityType.SelectedValueAsInt(), tbAttrQualifierField.Text, tbAttrQualifierValue.Text );
            }

            // Attribute will be null if it was not valid
            if ( attribute == null )
            {
                return;
            }

            AttributeCache.FlushEntityAttributes();

            HideDialog();

            BindGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAttributeValue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void mdAttributeValue_SaveClick( object sender, EventArgs e )
        {
            if ( _displayValueEdit )
            {
                int attributeId = 0;
                if ( hfIdValues.Value != string.Empty && !int.TryParse( hfIdValues.Value, out attributeId ) )
                {
                    attributeId = 0;
                }

                if ( attributeId != 0 && phEditControls.Controls.Count > 0 )
                {
                    var attribute = Rock.Web.Cache.AttributeCache.Read( attributeId );

                    var rockContext = new RockContext();
                    AttributeValueService attributeValueService = new AttributeValueService( rockContext );
                    var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attributeId, _entityId );
                    if ( attributeValue == null )
                    {
                        attributeValue = new Rock.Model.AttributeValue();
                        attributeValue.AttributeId = attributeId;
                        attributeValue.EntityId = _entityId;
                        attributeValueService.Add( attributeValue );
                    }

                    var fieldType = Rock.Web.Cache.FieldTypeCache.Read( attribute.FieldType.Id );
                    attributeValue.Value = fieldType.Field.GetEditValue( attribute.GetControl( phEditControls.Controls[0] ), attribute.QualifierValues );

                    rockContext.SaveChanges();

                    Rock.Web.Cache.AttributeCache.Flush( attributeId );
                    if ( !_entityTypeId.HasValue && _entityQualifierColumn == string.Empty && _entityQualifierValue == string.Empty && ( !_entityId.HasValue || _entityId.Value == 0 ) )
                    {
                        Rock.Web.Cache.GlobalAttributesCache.Flush();
                    }
                }

                hfIdValues.Value = string.Empty;

                HideDialog();
            }

            BindGrid();
        }

        #endregion

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

            if ( ( entityTypeId ?? 0 ).ToString() == rFilter.GetUserPreference( "Entity Type" ) )
            {
                foreach ( var idVal in rFilter.GetUserPreference( "Categories" ).SplitDelimitedValues() )
                {
                    int id = int.MinValue;
                    if ( int.TryParse( idVal, out id ) )
                    {
                        selectedIDs.Add( id );
                    }
                }
            }

            cpCategoriesFilter.SetValues( selectedIDs );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            IQueryable<Rock.Model.Attribute> query = null;

            AttributeService attributeService = new AttributeService( new RockContext() );
            if ( _configuredType )
            {
                query = attributeService.Get( _entityTypeId, _entityQualifierColumn, _entityQualifierValue );
            }
            else
            {
                int entityTypeId = int.MinValue;
                if ( int.TryParse( rFilter.GetUserPreference( "Entity Type" ), out entityTypeId ) )
                {
                    if ( entityTypeId > 0 )
                    {
                        query = attributeService.GetByEntityTypeId( entityTypeId );
                    }
                }
            }

            if ( query == null )
            {
                query = attributeService.GetByEntityTypeId( null );
            }

            var selectedCategoryIds = new List<int>();
            rFilter.GetUserPreference( "Categories" ).SplitDelimitedValues().ToList().ForEach( s => selectedCategoryIds.Add( int.Parse( s ) ) );
            if ( selectedCategoryIds.Any() )
            {
                query = query.Where( a => a.Categories.Any( c => selectedCategoryIds.Contains( c.Id ) ) );
            }

            SortProperty sortProperty = rGrid.SortProperty;
            if ( sortProperty != null )
            {
                query = query.Sort( sortProperty );
            }
            else
            {
                query = query.OrderBy( a => a.Key );
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
            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );
            var attributeModel = attributeService.Get( attributeId );

            if ( attributeModel == null )
            {
                mdAttribute.Title = "Add Attribute".FormatAsHtmlTitle();

                attributeModel = new Rock.Model.Attribute();
                attributeModel.FieldTypeId = FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT ).Id;

                if ( !_configuredType )
                {
                    int entityTypeId = int.MinValue;
                    if ( int.TryParse( rFilter.GetUserPreference( "Entity Type" ), out entityTypeId ) && entityTypeId > 0 )
                    {
                        attributeModel.EntityTypeId = entityTypeId;
                    }
                }
                else
                {
                    attributeModel.EntityTypeId = _entityTypeId;
                    attributeModel.EntityTypeQualifierColumn = _entityQualifierColumn;
                    attributeModel.EntityTypeQualifierValue = _entityQualifierValue;
                }

                List<int> selectedCategoryIds = cpCategoriesFilter.SelectedValuesAsInt().ToList();
                new CategoryService( rockContext ).Queryable().Where( c => selectedCategoryIds.Contains( c.Id ) ).ToList().ForEach( c =>
                    attributeModel.Categories.Add( c ) );
                edtAttribute.ActionTitle = Rock.Constants.ActionTitle.Add( Rock.Model.Attribute.FriendlyTypeName );
            }
            else
            {
                edtAttribute.ActionTitle = Rock.Constants.ActionTitle.Edit( Rock.Model.Attribute.FriendlyTypeName );
                mdAttribute.Title = ( "Edit " + attributeModel.Name ).FormatAsHtmlTitle();
            }

            Type type = null;
            if ( attributeModel.EntityTypeId.HasValue )
            {
                type = EntityTypeCache.Read( attributeModel.EntityTypeId.Value ).GetEntityType();
                edtAttribute.ReservedKeyNames = attributeService.Get( attributeModel.EntityTypeId, attributeModel.EntityTypeQualifierColumn, attributeModel.EntityTypeQualifierValue )
                    .Where( a => a.Id != attributeId )
                    .Select( a => a.Key )
                    .Distinct()
                    .ToList();
            }

            edtAttribute.SetAttributeProperties( attributeModel, type );
            edtAttribute.AttributeEntityTypeId = attributeModel.EntityTypeId;

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

                ddlAttrEntityType.SetValue( attributeModel.EntityTypeId.HasValue ? attributeModel.EntityTypeId.Value.ToString() : "0" );
                tbAttrQualifierField.Text = attributeModel.EntityTypeQualifierColumn;
                tbAttrQualifierValue.Text = attributeModel.EntityTypeQualifierValue;
            }

            ShowDialog( "Attribute", true );
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
                phEditControls.Controls.Clear();

                var attribute = Rock.Web.Cache.AttributeCache.Read( attributeId );
                if ( attribute != null )
                {
                    mdAttributeValue.Title = attribute.Name + " Value";

                    var attributeValue = new AttributeValueService( new RockContext() ).GetByAttributeIdAndEntityId( attributeId, _entityId );
                    string value = attributeValue != null && !string.IsNullOrWhiteSpace( attributeValue.Value ) ? attributeValue.Value : attribute.DefaultValue;
                    attribute.AddControl( phEditControls.Controls, value, string.Empty, setValues, true );

                    SetValidationGroup( phEditControls.Controls, mdAttributeValue.ValidationGroup );

                    if ( setValues )
                    {
                        hfIdValues.Value = attribute.Id.ToString();
                        ShowDialog( "AttributeValue", true );
                    }
                }
            }
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "ATTRIBUTE":
                    mdAttribute.Show();
                    break;
                case "ATTRIBUTEVALUE":
                    mdAttributeValue.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "ATTRIBUTE":
                    mdAttribute.Hide();
                    break;
                case "ATTRIBUTEVALUE":
                    mdAttributeValue.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion
}
}