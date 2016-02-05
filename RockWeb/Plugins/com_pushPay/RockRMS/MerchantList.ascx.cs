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
    [DisplayName( "Merchant Listing List" )]
    [Category( "Pushpay" )]
    [Description( "Lists all the merchant listings for a particular Pushpay account." )]

    [LinkedPage( "Detail Page", "Page used to view merchant listing details.", true, "", "", 0)]
    [TextField( "Batch Name Prefix", "The batch prefix name to use when creating a new batch", false, "Pushpay", "", 1 )]
    [LinkedPage( "Batch Detail Page", "The page used to display details of a batch.", false, "", "", 2 )]
    [CampusField( "Default Campus", "The deafult campus to use for new people when their gift is not made a unique campus-specific fund.", true, "", "", 3)]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2", "", 4 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS, "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING, "", 5 )]
    public partial class MerchantList : Rock.Web.UI.RockBlock
    {

        public int? _accountId { get; set; }

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gMerchants.DataKeyNames = new string[] { "Id" };
            gMerchants.GridRebind += gMerchants_GridRebind;
            gMerchants.Actions.ShowAdd = false;
            gMerchants.IsDeleteEnabled = false;

            _accountId = PageParameter( "AccountId" ).AsIntegerOrNull();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                if ( _accountId.HasValue )
                {
                    BindGrid();
                }
            }
            else
            {
                ShowDialog();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the RowSelected event of the gMerchants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gMerchants_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", new Dictionary<string, string> { { "MerchantId", e.RowKeyId.ToString() } } );
        }

        /// <summary>
        /// Handles the Edit event of the gMerchants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gMerchants_Edit( object sender, RowEventArgs e )
        {
            ddlFundReferenceField.Items.Clear();

            using ( var rockContext = new RockContext() )
            {
                var merchant = new MerchantService( rockContext ).Get( e.RowKeyId );
                if ( merchant != null )
                {
                    var referenceFields = JsonConvert.DeserializeObject<List<ReferenceDefinition>>( merchant.ReferenceFieldsJson );
                    if ( referenceFields != null )
                    {
                        foreach ( var field in referenceFields.Where( f => f.HasChoices )
                            .OrderBy( f => f.Order ) )
                        {
                            ddlFundReferenceField.Items.Add( new ListItem( field.Label, field.Id.ToString() ) );
                        }
                    }

                    hfMerchantId.Value = merchant.Id.ToString();
                    ddlFundReferenceField.SetValue( merchant.FundReferenceFieldId );
                    apDefaultAccount.SetValue( merchant.DefaultFinancialAccount );
                    cbActive.Checked = merchant.IsActive;

                    ShowDialog( "SETTINGS", true );
                }
            }
        }

        /// <summary>
        /// Handles the Download event of the gMerchants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gMerchants_Download( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var merchant = new MerchantService( rockContext ).Get( e.RowKeyId );
                if ( merchant != null )
                {
                    hfMerchantId.Value = merchant.Id.ToString();

                    if ( merchant.LastDownloadToDate.HasValue )
                    { 
                        drpDates.LowerValue = merchant.LastDownloadToDate.Value.Date;
                        drpDates.UpperValue = RockDateTime.Today.Date;
                    }
                    else
                    {
                        drpDates.LowerValue = null;
                        drpDates.UpperValue = null;
                    }

                    nbDownload.Text = string.Empty;
                    nbDownload.Visible = false;

                    ShowDialog( "DOWNLOAD", true );
                }
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the mdEditMerchantSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdEditMerchantSettings_SaveClick( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var merchantService = new MerchantService( rockContext );
                var fundService = new MerchantFundService( rockContext );

                Merchant merchant = null;

                int? merchantId = hfMerchantId.Value.AsIntegerOrNull();
                if ( merchantId.HasValue )
                {
                    merchant = merchantService.Get( merchantId.Value );
                    if ( merchant != null )
                    {
                        merchant.FundReferenceFieldId = ddlFundReferenceField.SelectedValueAsInt();
                        merchant.FundReferenceFieldName = ddlFundReferenceField.SelectedItem != null ? ddlFundReferenceField.SelectedItem.Text : string.Empty;
                        merchant.DefaultFinancialAccountId = apDefaultAccount.SelectedValueAsId();
                        merchant.IsActive = cbActive.Checked;

                        rockContext.SaveChanges();

                        Merchant.RefreshFunds( merchant.Id );
                    }
                }
            }

            HideDialog();
            BindGrid();

        }

        /// <summary>
        /// Handles the Click event of the btnDownload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDownload_Click( object sender, EventArgs e )
        {
            int? defaultcampusId = null;
            var campus = CampusCache.All().FirstOrDefault( c => c.Guid.Equals( GetAttributeValue( "DefaultCampus" ).AsGuid() ) );
            if ( campus != null )
            {
                defaultcampusId = campus.Id;
            }
            var connectionStatus = DefinedValueCache.Read( GetAttributeValue( "ConnectionStatus" ).AsGuid() );
            var recordStatus = DefinedValueCache.Read( GetAttributeValue( "RecordStatus" ).AsGuid() );

            string batchNamePrefix = GetAttributeValue( "BatchNamePrefix" );

            DateTime? startDateTime = drpDates.LowerValue;
            DateTime? endDateTime = drpDates.UpperValue;

            using ( var rockContext = new RockContext() )
            {
                var merchantService = new MerchantService( rockContext );
                var merchant = merchantService.Get( hfMerchantId.ValueAsInt() );
                if ( merchant != null && startDateTime.HasValue && endDateTime.HasValue )
                {
                    if ( merchant.IsActive && 
                        merchant.Account.IsActive  &&
                        merchant.DefaultFinancialAccount != null && 
                        merchant.DefaultFinancialAccount.IsActive )
                    {
                        var financialGateway = new FinancialGatewayService( rockContext ).Get( com.pushpay.RockRMS.SystemGuid.FinancialGateway.PUSHPAY_GATEWAY.AsGuid() );
                        if ( financialGateway != null )
                        {
                            DateTime start = startDateTime.Value;
                            DateTime end = endDateTime.Value;

                            var qryParam = new Dictionary<string, string>();
                            qryParam.Add( "batchId", "9999" );
                            string batchUrlFormat = LinkedPageUrl( "BatchDetailPage", qryParam ).Replace( "9999", "{0}" );

                            string resultSummary = MerchantService.ProcessPayments( financialGateway, merchant, batchNamePrefix, batchUrlFormat,
                                defaultcampusId, recordStatus, connectionStatus, startDateTime.Value, endDateTime.Value );

                            if ( !string.IsNullOrWhiteSpace( resultSummary ) )
                            {
                                nbDownload.Text = string.Format( "<ul>{0}</ul>", resultSummary );
                                nbDownload.NotificationBoxType = NotificationBoxType.Success;
                            }
                            else
                            {
                                nbDownload.Text = string.Format( "There were not any transactions downloaded.", resultSummary );
                                nbDownload.NotificationBoxType = NotificationBoxType.Warning;
                            }
                        }
                        else
                        {
                            nbDownload.Text = "The Pushpay financial gateway could not be loaded.";
                            nbDownload.NotificationBoxType = NotificationBoxType.Danger;
                        }
                    }
                    else
                    {
                        nbDownload.Text = "The selected Merchant Listing is not active, or it's account is not active, or it does not have an active default account!";
                        nbDownload.NotificationBoxType = NotificationBoxType.Danger;
                    }

                }
                else
                {
                    nbDownload.Text = "The merchant listing, start, and/or end times could not be determined.";
                    nbDownload.NotificationBoxType = NotificationBoxType.Warning;
                }
            }

            nbDownload.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnRefresh control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRefresh_Click( object sender, EventArgs e )
        {
            Account.RefreshMerchants( _accountId );
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gMerchants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gMerchants_GridRebind( object sender, EventArgs e )
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
            if ( _accountId.HasValue )
            {
                var service = new AccountService( new RockContext() );
                var sortProperty = gMerchants.SortProperty;

                var account = service.Get( _accountId.Value );
                if ( account != null )
                {
                    lAccountName.Text = account.Name;

                    var qry = account.Merchants.AsQueryable();

                    if ( sortProperty != null )
                    {
                        qry = qry.Sort( sortProperty );
                    }
                    else
                    {
                        qry = qry.OrderBy( c => c.Name );
                    }

                    gMerchants.DataSource = qry
                        .ToList()
                        .Select( m => new
                            {
                                m.Id,
                                m.Name,
                                FundField = m.FundReferenceFieldId.HasValue ?
                                    m.FundReferenceFieldName :
                                    "<span data-toggle='tooltip' title='A reference Field has not been selected for the funds' class='label label-danger'>None</span>",
                                FundFieldName = m.FundReferenceFieldId.HasValue ?
                                    m.FundReferenceFieldName : string.Empty,
                                DefaultAccount = m.DefaultFinancialAccount != null ?
                                    m.DefaultFinancialAccount.Name :
                                    "<span data-toggle='tooltip' title='A default account has not been selected for the funds' class='label label-danger'>None</span>",
                                m.IsActive,
                                Funds = m.FundHtmlBadge
                            } )
                        .ToList();
                    gMerchants.DataBind();
                }
            }
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "SETTINGS":
                    mdEditMerchantSettings.Show();
                    break;
                case "DOWNLOAD":
                    mdDownload.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "SETTINGS":
                    mdEditMerchantSettings.Hide();
                    break;
                case "DOWNLOAD":
                    mdDownload.Hide();
                    break;
            }

            hfMerchantId.Value = string.Empty;
            hfActiveDialog.Value = string.Empty;
        }


        #endregion

        protected void mdDownload_SaveClick( object sender, EventArgs e )
        {
            HideDialog();
        }
}
}