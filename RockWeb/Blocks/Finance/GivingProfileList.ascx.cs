//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
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

    /// <summary>
    /// Lists scheduled transactions for current or selected user (if context for person is not configured, will display for currently logged in person).
    /// </summary>
    [DisplayName("Giving Profile List")]
    [Category("Financial")]
    [Description("Lists scheduled transactions for current or selected user (if context for person is not configured, will display for currently logged in person).")]

    [LinkedPage("Edit Page")]
    [LinkedPage("Add Page")]
    [ContextAware( typeof( Person ) )]
    public partial class GivingProfileList : Rock.Web.UI.RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets the target person.
        /// </summary>
        /// <value>
        /// The target person.
        /// </value>
        protected Person TargetPerson { get; private set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;
            gfSettings.DisplayFilterValue += gfSettings_DisplayFilterValue;

            bool canEdit = RockPage.IsAuthorized( "Edit", CurrentPerson );

            rGridGivingProfile.DataKeyNames = new string[] { "id" };
            rGridGivingProfile.Actions.ShowAdd = canEdit;
            rGridGivingProfile.IsDeleteEnabled = canEdit;

            rGridGivingProfile.Actions.AddClick += rGridGivingProfile_Add;
            rGridGivingProfile.GridRebind += rGridGivingProfile_GridRebind;

            TargetPerson = ContextEntity<Person>();
            if (TargetPerson == null)
            {
                TargetPerson = CurrentPerson;
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
                cbIncludeInactive.Checked = !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( "Include Inactive" ) );

                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SaveUserPreference( "Include Inactive", cbIncludeInactive.Checked ? "Yes" : "");
            BindGrid();
        }

        /// <summary>
        /// Gfs the settings_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void gfSettings_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if (e.Key != "Include Inactive")
            {
                e.Value = string.Empty;
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the rGridGivingProfile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGridGivingProfile_Edit( object sender, RowEventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "Txn", rGridGivingProfile.DataKeys[e.RowIndex]["id"].ToString() );
            parms.Add( "Person", TargetPerson.UrlEncodedKey );
            NavigateToLinkedPage( "EditPage", parms );
        }

        /// <summary>
        /// Handles the Add event of the gridFinancialGivingProfile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGridGivingProfile_Add( object sender, EventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "Person", TargetPerson.UrlEncodedKey );
            NavigateToLinkedPage( "AddPage", parms );
        }
        
        /// <summary>
        /// Handles the Delete event of the grdFinancialGivingProfile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGridGivingProfile_Delete( object sender, RowEventArgs e )
        {
            // TODO: Can't just delete profile, need to inactivate it on gateway
            //var scheduledTransactionService = new FinancialScheduledTransactionService();

            //FinancialScheduledTransaction profile = scheduledTransactionService.Get( (int)rGridGivingProfile.DataKeys[e.RowIndex]["id"] );
            //if ( profile != null )
            //{
            //    scheduledTransactionService.Delete( profile, CurrentPersonId );
            //    scheduledTransactionService.Save( profile, CurrentPersonId );
            //}

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

                
        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            bool includeInactive = !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( "Include Inactive" ) );

            rGridGivingProfile.DataSource = new FinancialScheduledTransactionService()
                .Get(TargetPerson.Id, TargetPerson.GivingGroupId, includeInactive).ToList();

            rGridGivingProfile.DataBind();
        }

        /// <summary>
        /// Shows the detail form.
        /// </summary>
        /// <param name="id">The id.</param>
        protected void ShowDetailForm( int id )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "Txn", id.ToString() );
            parms.Add( "Person", TargetPerson.UrlEncodedKey );
            NavigateToLinkedPage( "DetailPage", parms );
        }

        #endregion
        
    }        
}
