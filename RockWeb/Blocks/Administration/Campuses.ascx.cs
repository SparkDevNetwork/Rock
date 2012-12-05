//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using Rock;
using Rock.Constants;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
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

            if ( CurrentPage.IsAuthorized( "Administrate", CurrentPerson ) )
            {
                gCampuses.DataKeyNames = new string[] { "id" };
                gCampuses.Actions.IsAddEnabled = true;
                gCampuses.Actions.AddClick += gCampuses_Add;
                gCampuses.GridRebind += gCampuses_GridRebind;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            if ( CurrentPage.IsAuthorized( "Administrate", CurrentPerson ) )
            {
                if ( !Page.IsPostBack )
                {
                    BindGrid();
                }
            }
            else
            {
                gCampuses.Visible = false;
                nbMessage.Text = WarningMessage.NotAuthorizedToEdit( Campus.FriendlyTypeName );
                nbMessage.Visible = true;
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
            ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gCampuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gCampuses_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( (int)gCampuses.DataKeys[e.RowIndex]["id"] );
        }

        /// <summary>
        /// Handles the Delete event of the gCampuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gCampuses_Delete( object sender, RowEventArgs e )
        {
            CampusService campusService = new CampusService();
            Campus campus = campusService.Get( (int)gCampuses.DataKeys[e.RowIndex]["id"] );
            if ( CurrentBlock != null )
            {
                campusService.Delete( campus, CurrentPersonId );
                campusService.Save( campus, CurrentPersonId );
            }

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

        #region Edit Events

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

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            Campus campus;
            CampusService campusService = new CampusService();

            int campusId = int.Parse( hfCampusId.Value ); ;

            if ( campusId == 0 )
            {
                campus = new Campus();
                campusService.Add( campus, CurrentPersonId );
            }
            else
            {
                campus = campusService.Get( campusId );
            }

            campus.Name = tbCampusName.Text;

            // check for duplicates
            if ( campusService.Queryable().Count( a => a.Name.Equals( campus.Name, StringComparison.OrdinalIgnoreCase ) && !a.Id.Equals( campus.Id ) ) > 0 )
            {
                nbMessage.Text = WarningMessage.DuplicateFoundMessage( "name", Campus.FriendlyTypeName );
                nbMessage.Visible = true;
                return;
            }

            if ( !campus.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            campusService.Save( campus, CurrentPersonId );
            BindGrid();
            pnlDetails.Visible = false;
            pnlList.Visible = true;
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

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="campusId">The campus id.</param>
        protected void ShowEdit( int campusId )
        {
            pnlList.Visible = false;
            pnlDetails.Visible = true;

            CampusService campusService = new CampusService();
            Campus campus = campusService.Get( campusId );

            if ( campus != null )
            {
                lActionTitle.Text = ActionTitle.Edit( Campus.FriendlyTypeName );
                hfCampusId.Value = campus.Id.ToString();
                tbCampusName.Text = campus.Name;
            }
            else
            {
                lActionTitle.Text = ActionTitle.Add( Campus.FriendlyTypeName );
                hfCampusId.Value = 0.ToString();
                tbCampusName.Text = string.Empty;
            }
        }

        #endregion
    }
}