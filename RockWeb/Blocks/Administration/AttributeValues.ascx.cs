//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Core;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// User control for managing the attributes that are available for a specific entity
    /// </summary>
    [BlockProperty( 0, "Entity", "Applies To", "Entity Name", false, "" )]
    [BlockProperty( 1, "Entity Qualifier Column", "Applies To", "The entity column to evaluate when determining if this attribute applies to the entity", false, "" )]
    [BlockProperty( 2, "Entity Qualifier Value", "Applies To", "The entity column value to evaluate.  Attributes will only apply to entities with this value", false, "" )]
    [BlockProperty( 3, "Entity Id", "Entity", "The entity id that values apply to", false, "" )]
    public partial class AttributeValues : Rock.Web.UI.RockBlock
    {
        #region Fields

        protected int? _entityTypeId = null;
        protected string _entityQualifierColumn = string.Empty;
        protected string _entityQualifierValue = string.Empty;
        protected int? _entityId = null;

        private bool _canConfigure = false;

        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            try
            {
                string entityTypeName = AttributeValue( "Entity" );
                if ( string.IsNullOrWhiteSpace( entityTypeName ) )
                {
                    entityTypeName = PageParameter( "Entity" );
                }
                _entityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( entityTypeName );

                _entityQualifierColumn = AttributeValue( "EntityQualifierColumn" );
                if ( string.IsNullOrWhiteSpace( _entityQualifierColumn ) )
                {
                    _entityQualifierColumn = PageParameter( "EntityQualifierColumn" );
                }

                _entityQualifierValue = AttributeValue( "EntityQualifierValue" );
                if ( string.IsNullOrWhiteSpace( _entityQualifierValue ) )
                {
                    _entityQualifierValue = PageParameter( "EntityQualifierValue" );
                }

                string entityIdString = AttributeValue( "EntityId" );
                if ( string.IsNullOrWhiteSpace( entityIdString ) )
                {
                    entityIdString = PageParameter( "EntityId" );
                }
                if ( !string.IsNullOrWhiteSpace( entityIdString ) )
                {
                    int entityIdint = 0;
                    if (Int32.TryParse(entityIdString, out entityIdint))
                        _entityId = entityIdint;
                }

                _canConfigure = CurrentPage.IsAuthorized( "Configure", CurrentPerson );

                BindFilter();
                rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

                if ( _canConfigure )
                {
                    rGrid.DataKeyNames = new string[] { "id" };
                    rGrid.ShowActionRow = false;

                    rGrid.GridRebind += rGrid_GridRebind;
                    rGrid.RowDataBound += rGrid_RowDataBound;
                    modalDetails.SaveClick += modalDetails_SaveClick;

                    modalDetails.OnCancelScript = string.Format( "$('#{0}').val('');", hfId.ClientID );

                    string editAttributeId = Request.Form[hfId.UniqueID];
                    if ( Page.IsPostBack && editAttributeId != null && editAttributeId.Trim() != string.Empty )
                        ShowEdit( Int32.Parse( editAttributeId ), false );
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
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        protected void ddlCategoryFilter_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindGrid();
        }

        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserValue( "Category", ddlCategoryFilter.SelectedValue );
            BindGrid();
        }

        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( ( int )rGrid.DataKeys[e.RowIndex]["id"], true );
        }

        void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        void modalDetails_SaveClick( object sender, EventArgs e )
        {
            int attributeId = 0;
            if ( hfId.Value != string.Empty && !Int32.TryParse( hfId.Value, out attributeId ) )
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
                    attributeValue = new Rock.Core.AttributeValue();
                    attributeValue.AttributeId = attributeId;
                    attributeValue.EntityId = _entityId;
                    attributeValueService.Add( attributeValue, CurrentPersonId );
                }

                var fieldType = Rock.Web.Cache.FieldTypeCache.Read( attribute.FieldType.Id );
                attributeValue.Value = fieldType.Field.GetEditValue( phEditControl.Controls[0], attribute.QualifierValues );

                attributeValueService.Save(attributeValue, CurrentPersonId);

                Rock.Web.Cache.AttributeCache.Flush( attributeId );
                if ( !_entityTypeId.HasValue && _entityQualifierColumn == string.Empty && _entityQualifierValue == string.Empty && !_entityId.HasValue )
                {
                    Rock.Web.Cache.GlobalAttributesCache.Flush();
                }
            }

            modalDetails.Hide();

            BindGrid();
        }

        void rGrid_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                Literal lValue = e.Row.FindControl( "lValue" ) as Literal;

                if ( lValue != null )
                {
                    int attributeId = ( int )rGrid.DataKeys[e.Row.RowIndex].Value;

                    var attribute = Rock.Web.Cache.AttributeCache.Read( attributeId );
                    var fieldType = Rock.Web.Cache.FieldTypeCache.Read( attribute.FieldTypeId );

                    AttributeValueService attributeValueService = new AttributeValueService();
                    var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attributeId, _entityId ).FirstOrDefault();
                    if ( attributeValue != null )
                    {
                        lValue.Text = fieldType.Field.FormatValue( lValue, attributeValue.Value, attribute.QualifierValues, true );
                    }
                }
            }
        }

        #endregion

        #region Methods

        private void BindFilter()
        {
            ddlCategoryFilter.Items.Clear();
            ddlCategoryFilter.Items.Add( "[All]" );

            AttributeService attributeService = new AttributeService();
            var items = attributeService.Get( _entityTypeId, _entityQualifierColumn, _entityQualifierValue )
                .Where( a => a.Category != "" && a.Category != null )
                .OrderBy( a => a.Category )
                .Select( a => a.Category )
                .Distinct()
                .ToList();

            foreach ( var item in items )
            {
                ListItem li = new ListItem( item );
                li.Selected = ( !Page.IsPostBack && rFilter.GetUserValue( "Category" ) == item );
                ddlCategoryFilter.Items.Add( li );
            }
        }

        private void BindGrid()
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                AttributeService attributeService = new AttributeService();
                var queryable = attributeService.Get( _entityTypeId, _entityQualifierColumn, _entityQualifierValue );

                if ( ddlCategoryFilter.SelectedValue != "[All]" )
                    queryable = queryable.
                        Where( a => a.Category == ddlCategoryFilter.SelectedValue );
                {
                    rGrid.DataSource = queryable.
                        OrderBy( a => a.Category ).
                        ThenBy( a => a.Name ).
                        Select( a => new
                        {
                            a.Id,
                            a.Category,
                            a.Name,
                            a.Description,
                            a.DefaultValue
                        } ).
                        ToList();
                }

                rGrid.DataBind();
            }

            hfId.Value = string.Empty;
        }

        protected void ShowEdit( int attributeId, bool setValues )
        {
            var attribute = Rock.Web.Cache.AttributeCache.Read(attributeId);

            hfId.Value = attribute.Id.ToString();
            lCaption.Text = attribute.Name;

            AttributeValueService attributeValueService = new AttributeValueService();
            var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attributeId, _entityId ).FirstOrDefault();

            var fieldType = Rock.Web.Cache.FieldTypeCache.Read( attribute.FieldType.Id );

            Control editControl = fieldType.Field.EditControl( attribute.QualifierValues );
            if ( setValues && attributeValue != null )
            {
                fieldType.Field.SetEditValue( editControl, attribute.QualifierValues, attributeValue.Value );
            }

            phEditControl.Controls.Clear();
            phEditControl.Controls.Add( editControl );

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