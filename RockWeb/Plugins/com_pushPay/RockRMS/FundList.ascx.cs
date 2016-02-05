//
// Copyright (C) Spark Development Network - All Rights Reserved
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using com.pushpay.RockRMS;
using com.pushpay.RockRMS.ApiModel;
using com.pushpay.RockRMS.Model;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

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

                    apRockAccount.SetValue( fund.FinancialAccount );

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
                        fund.FinancialAccountId = apRockAccount.SelectedValueAsInt();
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
                            FinancialAccount = f.FinancialAccount != null ?
                                f.FinancialAccount.Name :
                                "<span class='label label-danger'>None</span>",
                            FinancialAccountName = f.FinancialAccount != null ?
                                f.FinancialAccount.Name : string.Empty
                        } ).ToList();
                    gFunds.DataBind();
                }
            }
        }

        #endregion

    }
}