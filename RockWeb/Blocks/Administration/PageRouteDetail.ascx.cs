//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using System.Web.UI;
using Rock;
using Rock.Constants;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PageRouteDetail : RockBlock, IDetailBlock
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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                string itemId = PageParameter( "pageRouteId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "pageRouteId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }
        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( "pageRouteId" ) )
            {
                return;
            }

            pnlDetails.Visible = true;

            PageRoute pageRoute = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                pageRoute = new PageRouteService().Get( itemKeyValue );
                lActionTitle.Text = ActionTitle.Edit( PageRoute.FriendlyTypeName );
            }
            else
            {
                pageRoute = new PageRoute { Id = 0 };
                lActionTitle.Text = ActionTitle.Add( PageRoute.FriendlyTypeName );
            }

            hfPageRouteId.Value = pageRoute.Id.ToString();
            ppPage.SetValue( pageRoute.Page );
            tbRoute.Text = pageRoute.Route;

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( PageRoute.FriendlyTypeName );
            }

            if ( pageRoute.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( PageRoute.FriendlyTypeName );
            }

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( PageRoute.FriendlyTypeName );
                btnCancel.Text = "Close";
            }

            ppPage.Enabled = !readOnly;
            tbRoute.ReadOnly = readOnly;
            btnSave.Visible = !readOnly;
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            PageRoute pageRoute;
            PageRouteService pageRouteService = new PageRouteService();

            int pageRouteId = int.Parse( hfPageRouteId.Value );

            if ( pageRouteId == 0 )
            {
                pageRoute = new PageRoute();
                pageRouteService.Add( pageRoute, CurrentPersonId );
            }
            else
            {
                pageRoute = pageRouteService.Get( pageRouteId );
            }

            pageRoute.Route = tbRoute.Text.Trim();
            int selectedPageId = int.Parse( ppPage.SelectedValue );
            pageRoute.PageId = selectedPageId;

            if ( !pageRoute.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            pageRouteService.Save( pageRoute, CurrentPersonId );

            // new or updated route
            var existingRoute = RouteTable.Routes.OfType<Route>().FirstOrDefault( a => a.RouteId() == pageRoute.Id );
            if ( existingRoute != null )
            {
                RouteTable.Routes.Remove( existingRoute );
            }

            RouteTable.Routes.AddPageRoute( pageRoute );

            NavigateToParentPage();
        }

        #endregion
    }
}