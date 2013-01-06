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

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// 
    /// </summary>
    public partial class OneTimeGift : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );                       
            
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;
            


            base.OnLoad( e );
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
            pnlGift.Visible = false;
            //pnlPayment.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnNext_Click( object sender, EventArgs e )
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
                        
            
            //pnlDetails.Visible = false;
            pnlGift.Visible = true;
        }

        #endregion

        #region Internal Methods
        
        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="campusId">The campus id.</param>
        protected void ShowNext( int campusId )
        {
            pnlGift.Visible = false;
            //pnlDetails.Visible = false;

            CampusService campusService = new CampusService();
            Campus campus = campusService.Get( campusId );

            if ( campus != null )
            {
                //lActionTitle.Text = ActionTitle.Edit( Campus.FriendlyTypeName );
                //hfCampusId.Value = campus.Id.ToString();
                //tbCampusName.Text = campus.Name;
            }
            else
            {
                //lActionTitle.Text = ActionTitle.Add( Campus.FriendlyTypeName );
                //hfCampusId.Value = 0.ToString();
                //tbCampusName.Text = string.Empty;
            }
        }

        #endregion
    }
}