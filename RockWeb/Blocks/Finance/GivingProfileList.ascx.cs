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
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;
using System.Collections.Generic;

namespace RockWeb.Blocks.Finance
{
    [LinkedPage("Detail Page")]
    public partial class GivingProfileList : Rock.Web.UI.RockBlock
    {
        #region Fields
        private bool _canConfigure = false;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rFBFilter.ApplyFilterClick += rFBFilter_ApplyFilterClick;
            rFBFilter.DisplayFilterValue += rFBFilter_DisplayFilterValue;

            _canConfigure = CurrentPage.IsAuthorized( "Administrate", CurrentPerson );

            if ( _canConfigure )
            {
                rGridGivingProfile.DataKeyNames = new string[] { "id" };
                rGridGivingProfile.Actions.ShowAdd = true;

                rGridGivingProfile.Actions.AddClick += rGridGivingProfile_Add;
                rGridGivingProfile.GridRebind += rGridGivingProfile_GridRebind;
                rGridGivingProfile.GridReorder += rGridGivingProfile_GridReorder;

            }
            else
            {
                DisplayError( "You are not authorized to configure this page" );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the filter display for each saved user value
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void rFBFilter_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Date":
                    DateTime GivingProfileDate = DateTime.Now;
                    e.Value = GivingProfileDate.ToString();
                    break;
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFBFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFBFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFBFilter.SaveUserPreference( "Start Date", dtpGivingProfileDate.Text );
            
            BindGrid();
        }

        /// <summary>
        /// Handles the Delete event of the grdFinancialGivingProfile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGridGivingProfile_Delete( object sender, RowEventArgs e )
        {
            var scheduledTransactionService = new FinancialScheduledTransactionService();

            FinancialScheduledTransaction profile = scheduledTransactionService.Get( (int)rGridGivingProfile.DataKeys[e.RowIndex]["id"] );
            if ( profile != null )
            {
                scheduledTransactionService.Delete( profile, CurrentPersonId );
                scheduledTransactionService.Save( profile, CurrentPersonId );
            }

            BindGrid();
        }
        
        /// <summary>
        /// Handles the RowSelected event of the rGridGivingProfile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGridGivingProfile_Edit( object sender, RowEventArgs e )
        {
            ShowDetailForm( (int)rGridGivingProfile.DataKeys[e.RowIndex]["id"] );
        }

        /// <summary>
        /// Handles the Add event of the gridFinancialGivingProfile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGridGivingProfile_Add( object sender, EventArgs e )
        {
            BindFilter();
            ShowDetailForm( 0 );
        }
                
        #endregion

        #region Internal Methods
        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            DateTime fromDate;
            if ( !DateTime.TryParse( rFBFilter.GetUserPreference( "Start Date" ), out fromDate ) )
            {
                fromDate = DateTime.Today;
            }
            dtpGivingProfileDate.Text = fromDate.ToShortDateString();

        }

        /// <summary>
        /// Binds the defined type dropdown.
        /// </summary>
        /// <param name="ListControl">The list control.</param>
        /// <param name="definedTypeGuid">The defined type GUID.</param>
        /// <param name="userPreferenceKey">The user preference key.</param>
        private void BindDefinedTypeDropdown( ListControl ListControl, Guid definedTypeGuid, string userPreferenceKey )
        {
            ListControl.BindToDefinedType( DefinedTypeCache.Read( definedTypeGuid ) );
            //ListControl.Items.Insert( 0, new ListItem( All.Text, All.Id.ToString() ) );

            ListControl.SelectedValue = !string.IsNullOrWhiteSpace( rFBFilter.GetUserPreference( userPreferenceKey ) ) ?
                ListControl.SelectedValue = rFBFilter.GetUserPreference( userPreferenceKey ) : null;
        }

        /// <summary>
        /// Handles the GridReorder event of the grdFinancialGivingProfile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        private void rGridGivingProfile_GridReorder( object sender, GridReorderEventArgs e )
        {
            var profileService = new FinancialScheduledTransactionService();
            var queryable = profileService.Queryable();

            List<FinancialScheduledTransaction> items = queryable.ToList();
            profileService.Reorder( items, e.OldIndex, e.NewIndex, CurrentPersonId );
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the grdFinancialGivingProfile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void rGridGivingProfile_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var profiles = new FinancialScheduledTransactionService().Queryable();

            SortProperty sortProperty = rGridGivingProfile.SortProperty;

            if ( dtpGivingProfileDate.SelectedDateTime.HasValue )
            {
                profiles = profiles.Where( GivingProfile => GivingProfile.StartDate >= dtpGivingProfileDate.SelectedDateTime );
            }

            if ( sortProperty != null )
            {
                rGridGivingProfile.DataSource = profiles.Sort( sortProperty ).ToList();
            }
            else
            {
                rGridGivingProfile.DataSource = profiles.OrderBy( b => b.AuthorizedPersonId ).ToList();
            }

            rGridGivingProfile.DataBind();
        }

        /// <summary>
        /// Shows the detail form.
        /// </summary>
        /// <param name="id">The id.</param>
        protected void ShowDetailForm( int id )
        {
            NavigateToLinkedPage( "DetailPage", "GivingProfileId", id );
        }

        /// <summary>
        /// Displays the error.
        /// </summary>
        /// <param name="message">The message.</param>
        private void DisplayError( string message )
        {
            valSummaryTop.Controls.Clear();
            valSummaryTop.Controls.Add( new LiteralControl( message ) );
            valSummaryTop.Visible = true;
        }

        /// <summary>
        /// Handles the RowDataBound event of the grdFinancialGivingProfile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void rGridGivingProfile_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            FinancialScheduledTransaction profile = e.Row.DataItem as FinancialScheduledTransaction;
            if ( profile != null )
            {
                //do stuff here
              
            }
        }

        #endregion
        
    }        
}
