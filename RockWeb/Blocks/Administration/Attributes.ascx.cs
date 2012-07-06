//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Web.UI;

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
        private Rock.Core.AttributeService _attributeService = new Rock.Core.AttributeService();

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

                _canConfigure = PageInstance.Authorized( "Configure", CurrentUser );

                BindFilter();

                if ( _canConfigure )
                {
                    rGrid.DataKeyNames = new string[] { "id" };
                    rGrid.Actions.EnableAdd = true;

                    rGrid.Actions.AddClick += rGrid_Add;
                    rGrid.GridRebind += rGrid_GridRebind;

                    ddlFieldType.SelectedIndexChanged += new EventHandler<EventArgs>( ddlFieldType_SelectedIndexChanged );
                    modalDetails.SaveClick += modalDetails_SaveClick;

                    string script = string.Format( @"
        Sys.Application.add_load(function () {{
            $('#{0} td.grid-icon-cell.delete a').click(function(){{
                return confirm('Are you sure you want to delete this setting?');
                }});
        }});
    ", rGrid.ClientID );
                    this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", BlockInstance.Id ), script, true );

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
            Rock.Core.Attribute attribute = _attributeService.Get( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
            if ( attribute != null )
            {
                Rock.Web.Cache.Attribute.Flush( attribute.Id );

                _attributeService.Delete( attribute, CurrentPersonId );
                _attributeService.Save( attribute, CurrentPersonId );
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
            modalDetails.Show();
        }

        void modalDetails_SaveClick( object sender, EventArgs e )
        {
            Rock.Core.Attribute attribute;

            int attributeId = 0;
            if ( hfId.Value != string.Empty && !Int32.TryParse( hfId.Value, out attributeId) )
                attributeId = 0;

            if ( attributeId == 0 )
            {
                attribute = new Rock.Core.Attribute();
                attribute.System = false;
                attribute.Entity = _entity;
                attribute.EntityQualifierColumn = _entityQualifierColumn;
                attribute.EntityQualifierValue = _entityQualifierValue;
                _attributeService.Add( attribute, CurrentPersonId );
            }
            else
            {
                Rock.Web.Cache.Attribute.Flush( attributeId );
                attribute = _attributeService.Get( attributeId );
            }

            attribute.Key = tbKey.Text;
            attribute.Name = tbName.Text;
            attribute.Category = tbCategory.Text;
            attribute.Description = tbDescription.Text;
            attribute.FieldTypeId = ddlFieldType.FieldType.Id;

            attribute.AttributeQualifiers.Clear();
            foreach(var configValue in ddlFieldType.ConfigurationValues)
            {
                AttributeQualifier qualifier = new AttributeQualifier();
                qualifier.Key = configValue.Key;
                qualifier.Value = configValue.Value.Value;
                attribute.AttributeQualifiers.Add(qualifier);
            }

            attribute.DefaultValue = tbDefaultValue.Text;
            attribute.MultiValue = cbMultiValue.Checked;
            attribute.Required = cbRequired.Checked;

            _attributeService.Save( attribute, CurrentPersonId );

            BindGrid();
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
            var queryable = _attributeService.Queryable().
                Where( a => a.Entity == _entity &&
                    ( a.EntityQualifierColumn ?? string.Empty ) == _entityQualifierColumn &&
                    ( a.EntityQualifierValue ?? string.Empty ) == _entityQualifierValue );

            if ( ddlCategoryFilter.SelectedValue != "[All]" )
                queryable = queryable.
                    Where( a => a.Category == ddlCategoryFilter.SelectedValue );

            rGrid.DataSource = queryable.
                OrderBy( a => a.Category ).
                ThenBy( a => a.Key ).
                ToList();

            rGrid.DataBind();
        }

        protected void ShowEdit( int attributeId )
        {
            var attributeModel = _attributeService.Get( attributeId );

            if ( attributeModel != null )
            {
                var attribute = Rock.Web.Cache.Attribute.Read(attributeModel);

                modalDetails.Title = "Edit Attribute";
                hfId.Value = attribute.Id.ToString();

                tbKey.Text = attribute.Key;
                tbName.Text = attribute.Name;
                tbCategory.Text = attribute.Category;
                tbDescription.Text = attribute.Description;
                ddlFieldType.FieldType = attribute.FieldType;
                ddlFieldType.LabelText = "Field Type";
                ddlFieldType.ConfigurationValues = attribute.QualifierValues;
                tbDefaultValue.Text = attribute.DefaultValue;
                cbMultiValue.Checked = attribute.MultiValue;
                cbRequired.Checked = attribute.Required;
            }
            else
            {
                modalDetails.Title = "Add Attribute";
                hfId.Value = string.Empty;

                tbKey.Text = string.Empty;
                tbName.Text = string.Empty;
                tbCategory.Text = ddlCategoryFilter.SelectedValue != "[All]" ? ddlCategoryFilter.SelectedValue : string.Empty;
                tbDescription.Text = string.Empty;

                FieldTypeService fieldTypeService = new FieldTypeService();
                var fieldTypeModel = fieldTypeService.GetByName("Text").FirstOrDefault();
                if (fieldTypeModel != null)
                {
                    ddlFieldType.FieldType = Rock.Web.Cache.FieldType.Read( fieldTypeModel.Id);
                    ddlFieldType.ConfigurationValues = ddlFieldType.FieldType.Field.GetConfigurationValues(null);
                }

                tbDefaultValue.Text = string.Empty;
                cbMultiValue.Checked = false;
                cbRequired.Checked = false;
            }

            modalDetails.Show();
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