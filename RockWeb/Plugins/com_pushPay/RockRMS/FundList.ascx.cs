//
// Copyright (C) Spark Development Network - All Rights Reserved
//
using com.pushpay.RockRMS.Model;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RockWeb.Plugins.com_pushPay.RockRMS
{
    /// <summary>
    /// Lists all packages or packages for a organizationan.
    /// </summary>
    [DisplayName( "Fund List" )]
    [Category( "Pushpay" )]
    [Description( "Lists all the fund options for a particular Pushpay Merchant Listing." )]

    public partial class FundList : Rock.Web.UI.RockBlock
    {

        public int? _merchantId { get; set; }

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gFunds.DataKeyNames = new string[] { "Id" };
            gFunds.GridRebind += gFunds_GridRebind;
            gFunds.Actions.ShowAdd = false;
            gFunds.IsDeleteEnabled = false;

            rptCampusAccounts.ItemDataBound += RptCampusAccounts_ItemDataBound;

            _merchantId = PageParameter( "MerchantId" ).AsIntegerOrNull();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack && _merchantId.HasValue )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        protected void gFunds_Edit( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var fund = new MerchantFundService( rockContext ).Get( e.RowKeyId );
                if ( fund != null )
                {
                    hfFundId.Value = fund.Id.ToString();
                    rblFundCampusOption.SetValue( ( fund.SetAccountsByCampus ?? false ).ToString() );

                    apRockAccount.SetValue( fund.FinancialAccount );

                    if ( fund.MerchantFundCampuses.Any() )
                    {
                        nbCampusAccountWarning.Visible = false;
                        rptCampusAccounts.DataSource = fund.MerchantFundCampuses;
                        rptCampusAccounts.DataBind();
                        rptCampusAccounts.Visible = true;
                    }
                    else
                    {
                        nbCampusAccountWarning.Visible = true;
                        rptCampusAccounts.Visible = false;
                    }

                    SetAccountsOptionDisplay();

                    mdEditFundSettings.Title = fund.Name;
                    mdEditFundSettings.Show();
                }
            }
        }

        protected void mdEditFundSettings_SaveClick( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var fundService = new MerchantFundService( rockContext );
                MerchantFund fund = null;

                int? fundId = hfFundId.Value.AsIntegerOrNull();
                if ( fundId.HasValue )
                {
                    fund = fundService.Get( fundId.Value );
                    if ( fund != null )
                    {
                        fund.SetAccountsByCampus = rblFundCampusOption.SelectedValue.AsBoolean();
                        if ( fund.SetAccountsByCampus ?? false )
                        {
                            foreach ( RepeaterItem item in rptCampusAccounts.Items )
                            {
                                HiddenField hfCampusValue = item.FindControl( "hfCampusValue" ) as HiddenField;
                                var apCampusAccount = item.FindControl( "apCampusAccount" ) as AccountPicker;
                                if ( hfCampusValue != null && apCampusAccount != null )
                                {
                                    var fundCampus = fund.MerchantFundCampuses.FirstOrDefault( c => c.CampusKey == hfCampusValue.Value );
                                    fundCampus.FinancialAccountId = apCampusAccount.SelectedValueAsInt();
                                }
                            }
                        }
                        else
                        {
                            fund.FinancialAccountId = apRockAccount.SelectedValueAsInt();
                            foreach ( var fundCampus in fund.MerchantFundCampuses )
                            {
                                fundCampus.FinancialAccountId = null;
                            }
                        }

                        rockContext.SaveChanges();
                    }
                }

                mdEditFundSettings.Hide();

                BindGrid();
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gFunds control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gFunds_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblFundCampusOption control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblFundCampusOption_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetAccountsOptionDisplay();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the RptCampusAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void RptCampusAccounts_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var merchantFundCampus = e.Item.DataItem as MerchantFundCampus;
            var apCampusAccount = e.Item.FindControl( "apCampusAccount" ) as AccountPicker;
            if ( merchantFundCampus != null && apCampusAccount != null )
            {
                apCampusAccount.SetValue( merchantFundCampus.FinancialAccountId );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            if ( _merchantId.HasValue )
            {
                var service = new MerchantService( new RockContext() );
                var sortProperty = gFunds.SortProperty;
                var merchant = service.Get( _merchantId.Value );
                if ( merchant != null )
                {
                    lMerchantName.Text = merchant.Name;

                    var qry = merchant.MerchantFunds.AsQueryable();

                    if ( sortProperty != null )
                    {
                        qry = qry.Sort( sortProperty );
                    }
                    else
                    {
                        qry = qry.OrderBy( c => c.Name );
                    }

                    gFunds.DataSource = qry.Select( f => new
                        {
                            f.Id,
                            f.Name,
                            FinancialAccount = ( f.SetAccountsByCampus ?? false ) ?
                                ( f.MerchantFundCampuses.Any( c => !c.FinancialAccountId.HasValue ) ? "<span class='label label-danger'>Not All Campuses Selected</span>" : "<span class='label label-success'>Campus Specific</span>" ) :
                                ( f.FinancialAccount != null ? f.FinancialAccount.Name : "<span class='label label-danger'>None</span>" ),
                        } ).ToList();
                    gFunds.DataBind();
                }
            }
        }

        private void SetAccountsOptionDisplay()
        {
            bool setAccountsByCampus = rblFundCampusOption.SelectedValue.AsBoolean();
            pnlSameAccount.Visible = !setAccountsByCampus;
            pnlCampusAccounts.Visible = setAccountsByCampus;
        }

        #endregion

    }
}