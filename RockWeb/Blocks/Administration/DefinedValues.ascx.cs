//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Core;
using Rock.FieldTypes;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// User control for managing the attributes that are available for a specific entity
    /// </summary>
    [Rock.Attribute.Property( 0, "Entity", "Applies To", "Entity Name", false, "" )]
    [Rock.Attribute.Property( 1, "Entity Qualifier Column", "Applies To", "The entity column to evaluate when determining if this attribute applies to the entity", false, "" )]
    [Rock.Attribute.Property( 2, "Entity Qualifier Value", "Applies To", "The entity column value to evaluate.  Attributes will only apply to entities with this value", false, "" )]
    [Rock.Attribute.Property( 2, "Entity Id", "Entity", "The entity id that values apply to", false, "" )]
    public partial class AttributeValues : Rock.Web.UI.Block
    {
        #region Fields

        protected string entity = string.Empty;
        protected string entityQualifierColumn = string.Empty;
        protected string entityQualifierValue = string.Empty;
        protected string entityId = "null";

        private bool canConfigure = false;

        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            try
            {
                entity = AttributeValue( "Entity" );
                if ( string.IsNullOrWhiteSpace( entity ) )
                    entity = PageParameter( "Entity" );

                entityQualifierColumn = AttributeValue( "EntityQualifierColumn" );
                if ( string.IsNullOrWhiteSpace( entityQualifierColumn ) )
                    entityQualifierColumn = PageParameter( "EntityQualifierColumn" );

                entityQualifierValue = AttributeValue( "EntityQualifierValue" );
                if ( string.IsNullOrWhiteSpace( entityQualifierValue ) )
                    entityQualifierValue = PageParameter( "EntityQualifierValue" );

                entityId = AttributeValue( "EntityId" );
                if ( string.IsNullOrWhiteSpace( entityId ) )
                    entityId = PageParameter( "EntityId" );
                if ( string.IsNullOrWhiteSpace( entityId ) )
                    entityId = "null";

                BindFilter();

                canConfigure = PageInstance.Authorized( "Configure", CurrentUser );

                if ( canConfigure )
                {
                    rGrid.DataKeyNames = new string[] { "id" };
                    rGrid.ShowActionRow = false;
                    rGrid.GridRebind += new GridRebindEventHandler( rGrid_GridRebind );
                    rGrid.RowDataBound += new GridViewRowEventHandler( rGrid_RowDataBound );
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

        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack && canConfigure )
                BindGrid();

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        protected void ddlCategoryFilter_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindGrid();
        }

        void rGrid_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                Literal lValue = e.Row.FindControl( "lValue" ) as Literal;
                HtmlAnchor aEdit = e.Row.FindControl( "aEdit" ) as HtmlAnchor;

                if ( lValue != null && aEdit != null )
                {
                    int attributeId = ( int )rGrid.DataKeys[e.Row.RowIndex].Value;

                    AttributeService attributeService = new AttributeService();
                    var attribute = attributeService.Get( attributeId );
                    var fieldType = Rock.Web.Cache.FieldType.Read( attribute.FieldTypeId );

                    AttributeValueService attributeValueService = new AttributeValueService();

                    int? iEntityId = null;
                    if ( entityId != "null" )
                        try { iEntityId = Int32.Parse( entityId ); }
                        catch { }

                    // TODO: Need to add support for multiple values
                    var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attributeId, iEntityId ).FirstOrDefault();
                    if ( attributeValue != null )
                    {
                        string clientUpdateScript = fieldType.Field.ClientUpdateScript(
                            this.Page,
                            "0",
                            attributeValue.Value,
                            "attribute_value_" + BlockInstance.Id.ToString(),
                            hfAttributeValue.ClientID ) + "(\"" + attributeValue.Value.EscapeQuotes() + "\");";

                        lValue.Text = fieldType.Field.FormatValue( lValue, attributeValue.Value, true );
                        aEdit.Attributes.Add( "onclick", string.Format( "editValue({0}, {1}, '{2}', '{3}', '{4}');",
                            attributeId, attributeValue.Id, attribute.Name.EscapeQuotes(), attributeValue.Value.EscapeQuotes(), clientUpdateScript ) );
                    }
                    else
                    {
                        string clientUpdateScript = fieldType.Field.ClientUpdateScript(
                            this.Page,
                            "0",
                            string.Empty,
                            "attribute_value_" + BlockInstance.Id.ToString(),
                            hfAttributeValue.ClientID ) + "('');";

                        aEdit.Attributes.Add( "onclick", string.Format( "editValue({0}, 0, '{1}', '', \"{2}\");",
                            attributeId, attribute.Name.EscapeQuotes(), clientUpdateScript ) );
                    }
                }
            }
        }

        void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Edit Events

        protected void btnRefresh_Click( object sender, EventArgs e )
        {
            BindFilter();
            BindGrid();
        }

        #endregion

        #region Internal Methods

        private void BindFilter()
        {
            ddlCategoryFilter.Items.Clear();
            ddlCategoryFilter.Items.Add( "[All]" );

            AttributeService attributeService = new AttributeService();
            var items = attributeService.Queryable().
                Where( a => a.Entity == entity &&
                    ( a.EntityQualifierColumn ?? string.Empty ) == entityQualifierColumn &&
                    ( a.EntityQualifierValue ?? string.Empty ) == entityQualifierValue &&
                    a.Category != "" && a.Category != null ).
                OrderBy( a => a.Category ).
                Select( a => a.Category ).
                Distinct().ToList();
 
            foreach ( var item in items )
                ddlCategoryFilter.Items.Add( item );
        }

        private void BindGrid()
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                AttributeService attributeService = new AttributeService();

                var queryable = attributeService.
                    Queryable().
                    Where( a => a.Entity == entity &&
                        ( a.EntityQualifierColumn ?? string.Empty ) == entityQualifierColumn &&
                        ( a.EntityQualifierValue ?? string.Empty ) == entityQualifierValue );

                if ( ddlCategoryFilter.SelectedValue != "[All]" )
                    queryable = queryable.
                        Where( a => a.Category == ddlCategoryFilter.SelectedValue );

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

                rGrid.DataBind();
            }
        }

        private void DisplayError( string message )
        {
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;

            phContent.Visible = false;
        }

        #endregion


    }
}