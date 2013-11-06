//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    [LinkedPage("Detail Page")] 
    public partial class Campuses : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gCampuses.DataKeyNames = new string[] { "id" };
            gCampuses.Actions.ShowAdd = true;
            gCampuses.Actions.AddClick += gCampuses_Add;
            gCampuses.GridRebind += gCampuses_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
            gCampuses.Actions.ShowAdd = canAddEditDelete;
            gCampuses.IsDeleteEnabled = canAddEditDelete;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gCampuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gCampuses_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "campusId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gCampuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gCampuses_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "campusId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gCampuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gCampuses_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                CampusService campusService = new CampusService();
                Campus campus = campusService.Get( (int)e.RowKeyValue );
                if ( campus != null )
                {
                    string errorMessage;
                    if ( !campusService.CanDelete( campus, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    campusService.Delete( campus, CurrentPersonId );
                    campusService.Save( campus, CurrentPersonId );
                }
            } );

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gCampuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gCampuses_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            CampusService campusService = new CampusService();
            SortProperty sortProperty = gCampuses.SortProperty;

            if ( sortProperty != null )
            {
                gCampuses.DataSource = campusService.Queryable().Sort( sortProperty ).ToList();
            }
            else
            {
                gCampuses.DataSource = campusService.Queryable().OrderBy( s => s.Name ).ToList();
            }

            gCampuses.DataBind();
        }

        #endregion
    }
}