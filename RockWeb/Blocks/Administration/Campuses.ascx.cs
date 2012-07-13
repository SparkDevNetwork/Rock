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
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    public partial class Campuses : Rock.Web.UI.Block
    {
        #region Fields
        
        Rock.CRM.CampusService campusService = new Rock.CRM.CampusService();
        
        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            if ( PageInstance.Authorized( "Configure", CurrentUser ) )
            {
                gCampuses.DataKeyNames = new string[] { "id" };
                gCampuses.Actions.EnableAdd = true;
                gCampuses.Actions.AddClick += gCampuses_Add;
                gCampuses.GridRebind += gCampuses_GridRebind;
            }

            string script = @"
        Sys.Application.add_load(function () {
            $('td.grid-icon-cell.delete a').click(function(){
                return confirm('Are you sure you want to delete this campus?');
                });
        });
    ";
            this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", gCampuses.ClientID ), script, true );

            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )

        {
            nbMessage.Visible = false;

            if ( PageInstance.Authorized( "Configure", CurrentUser ) )
            {
                if ( !Page.IsPostBack )
                    BindGrid();
            }
            else
            {
                gCampuses.Visible = false;
                nbMessage.Text = "You are not authorized to edit campuses";
                nbMessage.Visible = true;
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        protected void gCampuses_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( ( int )gCampuses.DataKeys[e.RowIndex]["id"] );
        }

        protected void gCampuses_Delete( object sender, RowEventArgs e )
        {
            Rock.CRM.Campus campus = campusService.Get( ( int )gCampuses.DataKeys[e.RowIndex]["id"] );
            if ( BlockInstance != null )
            {
                campusService.Delete( campus, CurrentPersonId );
                campusService.Save( campus, CurrentPersonId );
            }

            BindGrid();
        }

        void gCampuses_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        void gCampuses_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Edit Events

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            Rock.CRM.Campus campus;

            int campusId = 0;
            if ( !Int32.TryParse( hfCampusId.Value, out campusId ) )
                campusId = 0;

            if ( campusId == 0 )
            {
                campus = new Rock.CRM.Campus();
                campusService.Add( campus, CurrentPersonId );
            }
            else
            {
                campus = campusService.Get( campusId );
            }

            campus.Name = tbCampusName.Text;
            campusService.Save( campus, CurrentPersonId );

            BindGrid();

            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        #endregion

        #region Internal Methods

        private void BindGrid()
        {
            gCampuses.DataSource = campusService.Queryable().OrderBy( s => s.Name ).ToList();
            gCampuses.DataBind();
        }

        protected void ShowEdit( int campusId )
        {
            Rock.CRM.Campus campus = campusService.Get( campusId );

            if ( campus != null )
            {
                lAction.Text = "Edit";
                hfCampusId.Value = campus.Id.ToString();

                tbCampusName.Text = campus.Name;
            }
            else
            {
                lAction.Text = "Add";
                tbCampusName.Text = string.Empty;
            }

            pnlList.Visible = false;
            pnlDetails.Visible = true;
        }

        #endregion

    }
}