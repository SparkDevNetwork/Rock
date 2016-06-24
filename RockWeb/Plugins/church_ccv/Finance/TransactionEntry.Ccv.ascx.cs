using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Security;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Communication;

namespace RockWeb.Plugins.church_ccv.Finance
{
    [DisplayName( "Transaction Entry (CCV)" )]
    [Category( "CCV > Finance" )]
    [Description( "Custom CCV block that provides an optimized experience for giving" )]

    [FinancialGatewayField( "Credit Card Gateway", "The payment gateway to use for Credit Card transactions", false, "", "", 0, "CCGateway" )]
    [AccountsField( "Accounts", "The accounts to display.  By default all active accounts with a Public Name will be displayed", false, "", "", 6 )]
    public partial class TransactionEntryCcv : Rock.Web.UI.RockBlock
    {
        /// <summary>
        /// Gets or sets the giving funds json.
        /// </summary>
        /// <value>
        /// The giving funds json.
        /// </value>
        public string GivingFundsJSON { get; set; }

        /// <summary>
        /// Gets or sets the campus fund locations json.
        /// </summary>
        /// <value>
        /// The campus fund locations json.
        /// </value>
        public string CampusFundLocationsJSON { get; set; }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            this.RockPage.AddCSSLink( ResolveUrl( "~/Plugins/church_ccv/Finance/styles/main.css" ) );
            this.RockPage.AddCSSLink( ResolveUrl( "~/Plugins/church_ccv/Finance/styles/vendor.css" ) );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += TransactionEntryCcv_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upPayment );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the TransactionEntryCcv control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void TransactionEntryCcv_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !this.IsPostBack )
            {
                ShowDetail();
            }
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            LoadDropDowns();
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        protected void LoadDropDowns()
        {
            var rockContext = new RockContext();

            var selectedAccountGuids = GetAttributeValue( "Accounts" ).SplitDelimitedValues().ToList().AsGuidList();
            bool showAll = !selectedAccountGuids.Any();
            var today = RockDateTime.Today;

            var accountsQry = new FinancialAccountService( rockContext ).Queryable()
                    .Where( f =>
                        f.IsActive &&
                        f.IsPublic.HasValue &&
                        f.IsPublic.Value &&
                        ( f.StartDate == null || f.StartDate <= today ) &&
                        ( f.EndDate == null || f.EndDate >= today ) );


            if ( selectedAccountGuids.Any() )
            {
                accountsQry = accountsQry.Where( a => selectedAccountGuids.Contains( a.Guid ) );
            }

            var accounts = accountsQry
                    .OrderBy( f => f.Order )
                    .ThenBy( f => f.Name )
                    .Select( a => new
                    {
                        AccountId = a.Id.ToString(),
                        Name = a.Name,
                        CampusId = a.CampusId
                    } ).ToList();

            var campusFundLocations = CampusCache.All().OrderBy( a => a.Name ).Select( a =>
                new
                {
                    campusId = a.Id,
                    account = accounts.Where( c => c.CampusId == a.Id ).Select( x => new
                        {
                            Id = x.AccountId,
                            Text = x.Name
                        } ).FirstOrDefault(),
                    longitude = a.LocationId.HasValue ? a.Location.Longitude : (double?)null,
                    latitude = a.LocationId.HasValue ? a.Location.Latitude : (double?)null,
                }
             );

            // these are needed for the Location Detector and FundSetter js to work
            this.GivingFundsJSON = accounts.Select( a => new { value = a.AccountId, text = a.Name } ).ToJson();
            this.CampusFundLocationsJSON = campusFundLocations.ToJson();

            ddlAccounts.Items.Clear();
            foreach ( var account in accounts )
            {
                ddlAccounts.Items.Add( new ListItem( account.Name, account.AccountId.ToString() ) );
            }
        }
    }
}
