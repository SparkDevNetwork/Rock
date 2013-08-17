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
    public partial class AttributeCategories : RockBlock
    {
        bool _canConfigure = false;

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Load Entity Type Filter
            ddlEntityType.Items.Add( new ListItem( "All", "" ) );
            ddlEntityType.Items.Add( new ListItem( "None (Global Attributes)", "None" ) );
            new EntityTypeService().GetEntityListItems().ForEach( l => ddlEntityType.Items.Add( l ) );

            _canConfigure = CurrentPage.IsAuthorized( "Administrate", CurrentPerson );

            BindFilter();
            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

            if ( _canConfigure )
            {
                rGrid.DataKeyNames = new string[] { "id" };
                rGrid.Actions.ShowAdd = true;

                rGrid.Actions.AddClick += rGrid_Add;
                rGrid.GridRebind += rGrid_GridRebind;
                rGrid.RowDataBound += rGrid_RowDataBound;

                modalDetails.SaveClick += modalDetails_SaveClick;
                modalDetails.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdValue.ClientID );
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
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "EntityType", ddlEntityType.SelectedValue );
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
                case "EntityType":

                    if ( e.Value == "" )
                    {
                        e.Value = "All";
                    }
                    else if ( e.Value == "None" )
                    {
                        e.Value = "None (Global Attributes)";
                    }
                    else
                    {
                        e.Value = EntityTypeCache.Read( int.Parse(ddlEntityType.SelectedValue) ).FriendlyName;
                    }

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
        /// Handles the Delete event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            var service = new CategoryService();

            var category = service.Get( (int)rGrid.DataKeys[e.RowIndex]["id"] );
            if ( category != null )
            {
                string errorMessage = string.Empty;
                if ( service.CanDelete( category, out errorMessage ) )
                {

                    service.Delete( category, CurrentPersonId );
                    service.Save( category, CurrentPersonId );
                }
                else
                {
                    nbMessage.Text = errorMessage;
                    nbMessage.Visible = true;
                }
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
                Literal lEntityType = e.Row.FindControl( "lEntityType" ) as Literal;
                if ( lEntityType != null )
                {
                    lEntityType.Text = "None (Global Attributes)";

                    int categoryId = (int)rGrid.DataKeys[e.Row.RowIndex].Value;
                    var category = CategoryCache.Read( categoryId );

                    int entityTypeId = int.MinValue;
                    if ( category != null &&
                        !string.IsNullOrWhiteSpace( category.EntityTypeQualifierValue ) &&
                        int.TryParse( category.EntityTypeQualifierValue, out entityTypeId ) &&
                        entityTypeId > 0 )
                    {
                        var entityType = EntityTypeCache.Read( entityTypeId );
                        if ( entityType != null )
                        {
                            lEntityType.Text = entityType.FriendlyName;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the modalDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void modalDetails_SaveClick( object sender, EventArgs e )
        {
            int categoryId = 0;
            if ( hfIdValue.Value != string.Empty && !int.TryParse( hfIdValue.Value, out categoryId ) )
            {
                categoryId = 0;
            }

            var service = new CategoryService();
            Category category = null;

            if ( categoryId != 0 )
            {
                category = service.Get( categoryId );
            }

            if ( category == null )
            {
                string QualifierValue = null;
                if ( ddlEntityType.SelectedValue != "" &&
                    ddlEntityType.SelectedValue != "None" )
                {
                    QualifierValue = ddlEntityType.SelectedValue;
                }

                category = new Category();
                category.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Attribute ) ).Id;
                category.EntityTypeQualifierColumn = "EntityTypeId";
                category.EntityTypeQualifierValue = QualifierValue;
                service.Add( category, CurrentPersonId );
            }

            category.Name = tbName.Text;

            service.Save( category, CurrentPersonId );

            hfIdValue.Value = string.Empty;

            modalDetails.Hide();

            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
             
            ddlEntityType.SelectedValue = rFilter.GetUserPreference( "EntityType" );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            string selectedValue = ddlEntityType.SelectedValue;

            var attributeEntityTypeId = EntityTypeCache.Read(typeof(Rock.Model.Attribute)).Id;
            var queryable = new CategoryService().Queryable()
                .Where( c => c.EntityTypeId == attributeEntityTypeId);

            if ( !string.IsNullOrWhiteSpace( selectedValue ) )
            {
                if ( selectedValue == "None" )
                {
                    queryable = queryable
                        .Where( c =>
                            c.EntityTypeQualifierColumn == "EntityTypeId" &&
                            c.EntityTypeQualifierValue == null );
                }
                else
                {
                    queryable = queryable
                        .Where( c =>
                            c.EntityTypeQualifierColumn == "EntityTypeId" &&
                            c.EntityTypeQualifierValue == selectedValue );
                }
            }

            SortProperty sortProperty = rGrid.SortProperty;
            if ( sortProperty != null )
            {
                queryable = queryable.
                    Sort( sortProperty );
            }
            else
            {
                queryable = queryable.
                    OrderBy( a => a.Name );
            }

            rGrid.DataSource = queryable.ToList();
            rGrid.DataBind();
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        protected void ShowEdit( int categoryId )
        {
            var category = new CategoryService().Get(categoryId);
            tbName.Text = category != null ? category.Name : string.Empty;
            modalDetails.Show();
        }

        #endregion
}
}