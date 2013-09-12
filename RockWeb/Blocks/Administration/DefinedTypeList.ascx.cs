//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// User controls for managing defined types and their values
    /// </summary>
    [LinkedPage("Detail Page")]
    public partial class DefinedTypeList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            BindFilter();
            tFilter.ApplyFilterClick += tFilter_ApplyFilterClick;

            gDefinedType.DataKeyNames = new string[] { "id" };
            gDefinedType.Actions.ShowAdd = true;
            gDefinedType.Actions.AddClick += gDefinedType_Add;
            gDefinedType.GridRebind += gDefinedType_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
            gDefinedType.Actions.ShowAdd = canAddEditDelete;
            gDefinedType.IsDeleteEnabled = canAddEditDelete;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                gDefinedType_Bind();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Filter Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the tFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void tFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            tFilter.SaveUserPreference( "Category", ddlCategoryFilter.SelectedValue );

            gDefinedType_Bind();
        }

        #endregion

        #region Grid Events (main grid)

        /// <summary>
        /// Handles the Add event of the gDefinedType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gDefinedType_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "definedTypeId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gDefinedType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gDefinedType_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "definedTypeId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gDefinedType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gDefinedType_Delete( object sender, RowEventArgs e )
        {
            var definedValueService = new DefinedValueService();
            var definedTypeService = new DefinedTypeService();

            DefinedType type = definedTypeService.Get( e.RowKeyId );

            if ( type != null )
            {
                string errorMessage;
                if ( !definedTypeService.CanDelete( type, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                // if this DefinedType has DefinedValues, see if they can be deleted
                var definedValues = definedValueService.GetByDefinedTypeId( type.Id ).ToList();

                foreach ( var value in definedValues )
                {
                    if ( !definedValueService.CanDelete( value, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }
                }

                RockTransactionScope.WrapTransaction( () =>
                {
                    foreach ( var value in definedValues )
                    {
                        definedValueService.Delete( value, CurrentPersonId );
                        definedValueService.Save( value, CurrentPersonId );
                    }

                    definedTypeService.Delete( type, CurrentPersonId );
                    definedTypeService.Save( type, CurrentPersonId );
                } );
            }

            gDefinedType_Bind();
        }

        /// <summary>
        /// Handles the GridRebind event of the gDefinedType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gDefinedType_GridRebind( object sender, EventArgs e )
        {
            gDefinedType_Bind();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlCategoryFilter.Items.Clear();
            ddlCategoryFilter.Items.Add( Rock.Constants.All.Text );

            var items = new DefinedTypeService().Queryable()
                .Where( a => a.Category != string.Empty)
                .OrderBy( a => a.Category )
                .Select( a => a.Category )
                .Distinct()
                .ToList();

            foreach ( var item in items )
            {
                ListItem li = new ListItem( item );
                li.Selected = ( !Page.IsPostBack && tFilter.GetUserPreference( "Category" ) == item );
                ddlCategoryFilter.Items.Add( li );
            }
        }

        /// <summary>
        /// Binds the grid for defined types.
        /// </summary>
        private void gDefinedType_Bind()
        {
            var queryable = new DefinedTypeService().Queryable().Select( a =>
                new
                {
                    a.Id,
                    a.Category,
                    a.Name,
                    a.Description,
                    FieldTypeName = a.FieldType.Name
                } );

            string categoryFilter = tFilter.GetUserPreference( "Category" );
            if ( !string.IsNullOrWhiteSpace(categoryFilter) && categoryFilter != Rock.Constants.All.Text )
            {
                queryable = queryable.Where( a => a.Category == categoryFilter );
            }

            SortProperty sortProperty = gDefinedType.SortProperty;
            if ( sortProperty != null )
            {
                queryable = queryable.Sort( sortProperty );
            }
            else
            {
                queryable = queryable.OrderBy( a => a.Category ).ThenBy( a => a.Name );
            }

            gDefinedType.DataSource = queryable.ToList();
            gDefinedType.DataBind();
        }

        #endregion
    }
}