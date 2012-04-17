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

        protected string entity = string.Empty;
        protected string entityQualifierColumn = string.Empty;
        protected string entityQualifierValue = string.Empty;

        private bool canConfigure = false;
        private Rock.Core.AttributeRepository attributeRepository = new Rock.Core.AttributeRepository();

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

                canConfigure = PageInstance.Authorized( "Configure", CurrentUser );

                BindFilter();

                if ( canConfigure )
                {
                    rGrid.DataKeyNames = new string[] { "id" };
                    rGrid.GridRebind += new GridRebindEventHandler( rGrid_GridRebind );
                    rGrid.Actions.EnableAdd = true;
                    rGrid.Actions.ClientAddScript = "editAttribute(0)";

                    string script = string.Format( @"
        Sys.Application.add_load(function () {{
            $('td.grid-icon-cell.delete a').click(function(){{
                return confirm('Are you sure you want to delete this setting?');
                }});
        }});
    ", rGrid.ClientID );

                    this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", rGrid.ClientID ), script, true );
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

        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            Rock.Core.Attribute attribute = attributeRepository.Get( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
            if ( attribute != null )
            {
                Rock.Web.Cache.Attribute.Flush( attribute.Id );

                attributeRepository.Delete( attribute, CurrentPersonId );
                attributeRepository.Save( attribute, CurrentPersonId );
            }

            BindGrid();
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

            AttributeRepository attributeRepository = new AttributeRepository();
            var items = attributeRepository.AsQueryable().
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
            var queryable = attributeRepository.AsQueryable().
                Where( a => a.Entity == entity &&
                    ( a.EntityQualifierColumn ?? string.Empty ) == entityQualifierColumn &&
                    ( a.EntityQualifierValue ?? string.Empty ) == entityQualifierValue );

            if ( ddlCategoryFilter.SelectedValue != "[All]" )
                queryable = queryable.
                    Where( a => a.Category == ddlCategoryFilter.SelectedValue );

            rGrid.DataSource = queryable.
                OrderBy( a => a.Category ).
                ThenBy( a => a.Key ).
                ToList();

            rGrid.DataBind();
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