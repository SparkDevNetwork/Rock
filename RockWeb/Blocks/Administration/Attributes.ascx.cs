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
using Rock.Core;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// User control for managing the attributes that are available for a specific entity
    /// </summary>
    [Rock.Attribute.Property( 0, "Entity", "Applies To", "Entity Name", false, "" )]
    [Rock.Attribute.Property( 1, "Entity Qualifier Column", "Applies To", "The entity column to evaluate when determining if this attribute applies to the entity", false, "" )]
    [Rock.Attribute.Property( 2, "Entity Qualifier Value", "Applies To", "The entity column value to evaluate.  Attributes will only apply to entities with this value", false, "" )]
    public partial class Attributes : Rock.Web.UI.Block
    {
        #region Fields

        protected string _entity = string.Empty;
        protected string _entityQualifierColumn = string.Empty;
        protected string _entityQualifierValue = string.Empty;

        private bool _canConfigure = false;

        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            try
            {
                _entity = AttributeValue( "Entity" );
                if ( string.IsNullOrWhiteSpace( _entity ) )
                    _entity = PageParameter( "Entity" );

                _entityQualifierColumn = AttributeValue( "EntityQualifierColumn" );
                if ( string.IsNullOrWhiteSpace( _entityQualifierColumn ) )
                    _entityQualifierColumn = PageParameter( "EntityQualifierColumn" );

                _entityQualifierValue = AttributeValue( "EntityQualifierValue" );
                if ( string.IsNullOrWhiteSpace( _entityQualifierValue ) )
                    _entityQualifierValue = PageParameter( "EntityQualifierValue" );

                _canConfigure = PageInstance.IsAuthorized( "Configure", CurrentPerson );

                BindFilter();

                if ( _canConfigure )
                {
                    rGrid.DataKeyNames = new string[] { "id" };
                    rGrid.Actions.IsAddEnabled = true;

                    rGrid.Actions.AddClick += rGrid_Add;
                    rGrid.GridRebind += rGrid_GridRebind;

                    string script = string.Format( @"
        Sys.Application.add_load(function () {{
            $('#{0} td.grid-icon-cell.delete a').click(function(){{
                return confirm('Are you sure you want to delete this setting?');
                }});
        }});
    ", rGrid.ClientID );
                    this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", BlockInstance.Id ), script, true );

                    // Create the dropdown list for listing the available field types
                    var fieldTypeService = new Rock.Core.FieldTypeService();
                    var items = fieldTypeService.
                        Queryable().
                        Select( f => new { f.Id, f.Name } ).
                        OrderBy( f => f.Name );

                    ddlFieldType.AutoPostBack = true;
                    ddlFieldType.SelectedIndexChanged += new EventHandler( ddlFieldType_SelectedIndexChanged );
                    ddlFieldType.Items.Clear();
                    foreach ( var item in items )
                        ddlFieldType.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );
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
        }

        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack && _canConfigure )
                BindGrid();

            if ( Page.IsPostBack && hfId.Value != string.Empty )
                BuildConfigControls();

            base.OnLoad( e );
        }

        #endregion

        #region Events

        protected void ddlCategoryFilter_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindGrid();
        }

        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
        }

        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            var attributeService = new Rock.Core.AttributeService();

            Rock.Core.Attribute attribute = attributeService.Get( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
            if ( attribute != null )
            {
                Rock.Web.Cache.Attribute.Flush( attribute.Id );

                attributeService.Delete( attribute, CurrentPersonId );
                attributeService.Save( attribute, CurrentPersonId );
            }

            BindGrid();
        }

        protected void rGrid_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        void ddlFieldType_SelectedIndexChanged( object sender, EventArgs e )
        {
            int attributeId = 0;
            if ( hfId.Value != string.Empty && !Int32.TryParse( hfId.Value, out attributeId ) )
                attributeId = 0;

            if ( attributeId != 0 )
            {
                var attribute = Rock.Web.Cache.Attribute.Read ( attributeId );
                BuildConfigControls( attribute.QualifierValues );
            }
            else
                BuildConfigControls();
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                var attributeService = new Rock.Core.AttributeService();
                var attributeQualifierService = new Rock.Core.AttributeQualifierService();

                Rock.Core.Attribute attribute;

                int attributeId = 0;
                if ( hfId.Value != string.Empty && !Int32.TryParse( hfId.Value, out attributeId ) )
                    attributeId = 0;

                if ( attributeId == 0 )
                {
                    attribute = new Rock.Core.Attribute();
                    attribute.IsSystem = false;
                    attribute.Entity = _entity;
                    attribute.EntityQualifierColumn = _entityQualifierColumn;
                    attribute.EntityQualifierValue = _entityQualifierValue;
                    attributeService.Add( attribute, CurrentPersonId );
                }
                else
                {
                    Rock.Web.Cache.Attribute.Flush( attributeId );
                    attribute = attributeService.Get( attributeId );
                }

                attribute.Key = tbKey.Text;
                attribute.Name = tbName.Text;
                attribute.Category = tbCategory.Text;
                attribute.Description = tbDescription.Text;
                attribute.FieldTypeId = Int32.Parse(ddlFieldType.SelectedValue);

                var fieldType = Rock.Web.Cache.FieldType.Read(attribute.FieldTypeId);

                foreach ( var oldQualifier in attribute.AttributeQualifiers.ToList() )
                    attributeQualifierService.Delete( oldQualifier, CurrentPersonId );
                attribute.AttributeQualifiers.Clear();

                List<Control> configControls = new List<Control>();
                foreach ( var key in fieldType.Field.ConfigurationKeys() )
                    configControls.Add( phFieldTypeQualifiers.FindControl( "configControl_" + key ) );

                foreach ( var configValue in fieldType.Field.ConfigurationValues( configControls ) )
                {
                    AttributeQualifier qualifier = new AttributeQualifier();
                    qualifier.Key = configValue.Key;
                    qualifier.Value = configValue.Value.Value;
                    attribute.AttributeQualifiers.Add( qualifier );
                }

                attribute.DefaultValue = tbDefaultValue.Text;
                attribute.IsMultiValue = cbMultiValue.Checked;
                attribute.IsRequired = cbRequired.Checked;

                attributeService.Save( attribute, CurrentPersonId );

            }

            BindGrid();

            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        #endregion

        #region Methods

        private void BindFilter()
        {
            ddlCategoryFilter.Items.Clear();
            ddlCategoryFilter.Items.Add( "[All]" );

            AttributeService attributeService = new AttributeService();
            var items = attributeService.Queryable().
                Where( a => a.Entity == _entity &&
                    ( a.EntityQualifierColumn ?? string.Empty ) == _entityQualifierColumn &&
                    ( a.EntityQualifierValue ?? string.Empty ) == _entityQualifierValue &&
                    a.Category != "" && a.Category != null ).
                OrderBy( a => a.Category ).
                Select( a => a.Category ).
                Distinct().ToList();

            foreach ( var item in items )
                ddlCategoryFilter.Items.Add( item );
        }

        private void BindGrid()
        {
            var queryable = new Rock.Core.AttributeService().Queryable().
                Where( a => a.Entity == _entity &&
                    ( a.EntityQualifierColumn ?? string.Empty ) == _entityQualifierColumn &&
                    ( a.EntityQualifierValue ?? string.Empty ) == _entityQualifierValue );

            if ( ddlCategoryFilter.SelectedValue != "[All]" )
                queryable = queryable.
                    Where( a => a.Category == ddlCategoryFilter.SelectedValue );

            SortProperty sortProperty = rGrid.SortProperty;
            if ( sortProperty != null )
                queryable = queryable.
                    Sort( sortProperty );
            else
                queryable = queryable.
                    OrderBy( a => a.Category ).
                    ThenBy( a => a.Key );

            rGrid.DataSource = queryable.ToList();
            rGrid.DataBind();
        }

        protected void ShowEdit( int attributeId )
        {
            var attributeModel =  new Rock.Core.AttributeService().Get( attributeId );

            if ( attributeModel != null )
            {
                var attribute = Rock.Web.Cache.Attribute.Read(attributeModel);

                lAction.Text = "Edit";
                hfId.Value = attribute.Id.ToString();

                tbKey.Text = attribute.Key;
                tbName.Text = attribute.Name;
                tbCategory.Text = attribute.Category;
                tbDescription.Text = attribute.Description;
                ddlFieldType.SelectedValue = attribute.FieldType.Id.ToString();
                BuildConfigControls( attribute.QualifierValues );
                tbDefaultValue.Text = attribute.DefaultValue;
                cbMultiValue.Checked = attribute.IsMultiValue;
                cbRequired.Checked = attribute.IsRequired;
            }
            else
            {
                lAction.Text = "Add";
                hfId.Value = string.Empty;

                tbKey.Text = string.Empty;
                tbName.Text = string.Empty;
                tbCategory.Text = ddlCategoryFilter.SelectedValue != "[All]" ? ddlCategoryFilter.SelectedValue : string.Empty;
                tbDescription.Text = string.Empty;

                FieldTypeService fieldTypeService = new FieldTypeService();
                var fieldTypeModel = fieldTypeService.GetByName("Text").FirstOrDefault();
                if (fieldTypeModel != null)
                    ddlFieldType.SelectedValue = fieldTypeModel.Id.ToString();
                BuildConfigControls( );

                tbDefaultValue.Text = string.Empty;
                cbMultiValue.Checked = false;
                cbRequired.Checked = false;
            }

            pnlList.Visible = false;
            pnlDetails.Visible = true;
        }

        private void BuildConfigControls()
        {
            BuildConfigControls( null );
        }

        private void BuildConfigControls(Dictionary<string, Rock.Field.ConfigurationValue> qualifierValues)
        {
            var fieldType = Rock.Web.Cache.FieldType.Read( Int32.Parse( ddlFieldType.SelectedValue ) );
            if ( fieldType != null )
            {
                phFieldTypeQualifiers.Controls.Clear();
                var configControls = fieldType.Field.ConfigurationControls();

                int i = 0;
                foreach ( var configValue in fieldType.Field.ConfigurationValues( null ) )
                {
                    Label lbl = new Label();
                    phFieldTypeQualifiers.Controls.Add( lbl );
                    lbl.Text = configValue.Value.Name;

                    Control control = configControls[i];
                    phFieldTypeQualifiers.Controls.Add( control );
                    control.ID = "configControl_" + configValue.Key;

                    phFieldTypeQualifiers.Controls.Add( new LiteralControl( "<br/>" ) );

                    i++;
                }

                if ( qualifierValues != null )
                    fieldType.Field.SetConfigurationValues( configControls, qualifierValues );
            }
        }

        private void DisplayError( string message )
        {
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;
        }

        #endregion


    }
}