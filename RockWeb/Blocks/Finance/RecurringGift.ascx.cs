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
using Rock.Attribute;
using Rock.Constants;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// 
    /// </summary>    
    [BooleanField( 0, "Stack layout vertically", false, "UseStackedLayout", "", "Should giving UI be stacked vertically or horizontally?")]
    [BooleanField( 0, "Show Campus dropdown", false, "ShowCampusSelect", "", "Should giving be associated with a specific campus?" )]
    public partial class RecurringGift : RockBlock
    {
        #region Fields

        protected bool UseStackedLayout = false;
        protected bool ShowCampusSelect = false;
        protected string spanClass;

        #endregion

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

            UseStackedLayout = Convert.ToBoolean( GetAttributeValue( "UseStackedLayout" ) );
            ShowCampusSelect = Convert.ToBoolean( GetAttributeValue( "ShowCampusSelect" ) );

            base.OnLoad( e );
        }

        #endregion


        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnNext_Click( object sender, EventArgs e )
        {
            
            pnlDetails.Visible = false;
            pnlConfirm.Visible = true;

        }

        /// <summary>
        /// Handles the Click event of the btnBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnBack_Click( object sender, EventArgs e )
        {

            pnlConfirm.Visible = false;
            pnlDetails.Visible = true;            

        }

        #endregion

    }
}