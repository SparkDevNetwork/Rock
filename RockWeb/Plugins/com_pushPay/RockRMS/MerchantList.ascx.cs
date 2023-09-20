//
// Copyright (C) Spark Development Network - All Rights Reserved
//
using com.pushpay.RockRMS.ApiModel;
using com.pushpay.RockRMS.Model;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RockWeb.Plugins.com_pushPay.RockRMS
{
    /// <summary>
    /// Lists all packages or packages for a organizationan.
    /// </summary>
    [DisplayName( "Merchant Listing List" )]
    [Category( "Pushpay" )]
    [Description( "Lists all the merchant listings for a particular Pushpay account." )]

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage,
        Description = "Page used to view merchant listing details.",
        IsRequired = true,
        Order = 0)]

    [TextField( "Batch Name Prefix",
        Key = AttributeKey.BatchNamePrefix,
        Description = "The batch prefix name to use when creating a new batch",
        IsRequired = false,
        DefaultValue = "Pushpay", 
        Order = 1 )]

    [LinkedPage( "Batch Detail Page",
        Key = AttributeKey.BatchDetailPage,
        Description = "The page used to display details of a batch.",
        IsRequired = false, 
        Order = 2 )]

    [CampusField( "Default Campus", "The default campus to use for new people when their gift is not made a unique campus-specific fund.", true, "", "", 3, AttributeKey.DefaultCampus )]

    [DefinedValueField( "Connection Status",
        Key = AttributeKey.ConnectionStatus,
        Description = "The connection status to use for new individuals (default: 'Prospect'.)",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT,
        Order = 4 )]

    [DefinedValueField( "Record Status",
        Key = AttributeKey.RecordStatus,
        Description = "The record status to use for new individuals (default: 'Pending'.)",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING,
        Order = 5 )]

    [IntegerField( "Manual Download Payment Timeout (in minutes)",
        Key = AttributeKey.ManualDownloadPaymentTimeout,
        Description = "The length of time (in minutes) that a manual download will be permitted to run before it fails.",
        IsRequired = true,
        DefaultIntegerValue = 90,
        Order = 6 )]

    public partial class MerchantList : Rock.Web.UI.RockBlock
    {
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string BatchNamePrefix = "BatchNamePrefix";
            public const string BatchDetailPage = "BatchDetailPage";
            public const string DefaultCampus = "DefaultCampus";
            public const string ConnectionStatus = "ConnectionStatus";
            public const string RecordStatus = "RecordStatus";
            public const string ManualDownloadPaymentTimeout = "ManualDownloadPaymentTimeout";
        }

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
            NavigateToLinkedPage( AttributeKey.DetailPage, new Dictionary<string, string> { { "MerchantId", e.RowKeyId.ToString() } } );
        }

        /// <summary>
        /// Handles the Edit event of the gMerchants control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gMerchants_Edit( object sender, RowEventArgs e )
        {
            ddlMemoReferenceField.Items.Clear();
            ddlMemoReferenceField.Items.Add( new ListItem() );

            dvTransactionType.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE.AsGuid() ).Id;
            var dfltTrnType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );

            using ( var rockContext = new RockContext() )
            {
                var merchant = new MerchantService( rockContext ).Get( e.RowKeyId );
                if ( merchant != null )
                {
                    var referenceFields = JsonConvert.DeserializeObject<List<ReferenceDefinition>>( merchant.ReferenceFieldsJson );
                    if ( referenceFields != null )
                    {
                        foreach ( var field in referenceFields.OrderBy( f => f.Order ) )
                        {
                            ddlMemoReferenceField.Items.Add( new ListItem( field.Label, field.Id.ToString() ) );
                        }
                    }

                    hfMerchantId.Value = merchant.Id.ToString();
                    if ( merchant.MemoReferenceFieldId != null )
                    {
                        ddlMemoReferenceField.SetValue( merchant.MemoReferenceFieldId.Value.ToString() );
                    }
                    apDefaultAccount.SetValue( merchant.DefaultFinancialAccount );
                    cbActive.Checked = merchant.IsActive;
                    ceBatchSuffix.Text = merchant.BatchNameSuffix;
                    tbGivingUrl.Text = merchant.GivingBaseUrl;
                    dvTransactionType.SetValue( merchant.TransactionTypeId ?? dfltTrnType.Id );

                    mdEditMerchantSettings.Title = merchant.Name + " Settings";
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
                        long fieldId;
                        if ( long.TryParse( ddlMemoReferenceField.SelectedValue, out fieldId ) )
                        {
                            merchant.MemoReferenceFieldId = fieldId;
                        }
                        else
                        {
                            merchant.MemoReferenceFieldId = null;
                        }
                        merchant.MemoReferenceFieldName = ddlMemoReferenceField.SelectedItem != null ? ddlMemoReferenceField.SelectedItem.Text : string.Empty;
                        merchant.DefaultFinancialAccountId = apDefaultAccount.SelectedValueAsId();
                        merchant.IsActive = cbActive.Checked;
                        merchant.BatchNameSuffix = ceBatchSuffix.Text;
                        merchant.TransactionTypeId = dvTransactionType.SelectedValueAsInt();
                        merchant.GivingBaseUrl = tbGivingUrl.Text;

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
            int? defaultCampusId = null;
            var campus = CampusCache.All().FirstOrDefault( c => c.Guid.Equals( GetAttributeValue( AttributeKey.DefaultCampus ).AsGuid() ) );
            if ( campus != null )
            {
                defaultCampusId = campus.Id;
            }
            var connectionStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() );
            var recordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() );

            string batchNamePrefix = GetAttributeValue( AttributeKey.BatchNamePrefix );

            DateTime? startDateTime = drpDates.LowerValue;
            DateTime? endDateTime = drpDates.UpperValue;

            using ( var rockContext = new RockContext() )
            {
                int merchantId = hfMerchantId.ValueAsInt();
                var merchantService = new MerchantService( rockContext );
                var merchant = merchantService
                    .AsNoFilter()
                    .Include( m => m.Account )                 // These includes are necessary because the RockContext will be disposed in
                    .Include( m => m.DefaultFinancialAccount)  // this thread, but the Merchant record will be handed off to StartDownload() 
                    .Include( m => m.MerchantFunds )           // in a new background thread (and it cannot lazy load these properties).
                    .Include( m => m.MerchantFunds.Select( f => f.FinancialAccount ) )
                    .FirstOrDefault( m => m.Id == merchantId );

                if ( merchant == null || !startDateTime.HasValue || !endDateTime.HasValue )
                {
                    nbDownload.Text = "The merchant listing, start, and/or end times could not be determined.";
                    nbDownload.NotificationBoxType = NotificationBoxType.Warning;
                    nbDownload.Visible = true;
                    return;
                }

                if ( !merchant.IsActive || !merchant.Account.IsActive || merchant.DefaultFinancialAccount == null || !merchant.DefaultFinancialAccount.IsActive )
                {
                    nbDownload.Text = "The selected Merchant Listing is not active, or its account is not active, or it does not have an active default account!";
                    nbDownload.NotificationBoxType = NotificationBoxType.Danger;
                    nbDownload.Visible = true;
                    return;
                }

                var minDate = merchant.Account.ActiveDate ?? DateTime.MinValue;
                if ( startDateTime < minDate )
                {
                    nbDownload.Text = $"The start time cannot be prior to the account cutover date ({minDate.ToShortDateString()}).";
                    nbDownload.NotificationBoxType = NotificationBoxType.Danger;
                    nbDownload.Visible = true;
                    return;
                }


                var financialGateway = new FinancialGatewayService( rockContext ).Queryable()
                    .FirstOrDefault( g => g.EntityType != null && g.EntityType.Name == "com.pushpay.RockRMS.Gateway" );

                if ( financialGateway == null )
                {
                    nbDownload.Text = "The Pushpay financial gateway could not be loaded.";
                    nbDownload.NotificationBoxType = NotificationBoxType.Danger;
                    nbDownload.Visible = true;
                    return;
                }

                financialGateway.LoadAttributes( rockContext );

                DateTime start = startDateTime.Value;
                DateTime end = endDateTime.Value;

                var qryParam = new Dictionary<string, string>
                {
                    { "batchId", "9999" }
                };

                string batchUrlFormat = LinkedPageUrl( AttributeKey.BatchDetailPage, qryParam ).Replace( "9999", "{0}" );

                Task.Run( () => StartDownload( financialGateway, merchant, batchNamePrefix, batchUrlFormat, defaultCampusId,
                    recordStatus, connectionStatus, startDateTime.Value, endDateTime.Value, rebResultsEmail.Text ) );

                nbDownload.Text = $"Your download has been started.  The results will be sent to {rebResultsEmail.Text}.";
                nbDownload.NotificationBoxType = NotificationBoxType.Success;
                nbDownload.Visible = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gateway">The gateway.</param>
        /// <param name="merchant">The merchant.</param>
        /// <param name="batchNamePrefix">The batch name prefix.</param>
        /// <param name="batchUrlFormat">The batch URL format.</param>
        /// <param name="defaultCampusId">The default campus identifier.</param>
        /// <param name="defaultRecordStatus">The default record status.</param>
        /// <param name="defaultConnectionStatus">The default connection status.</param>
        /// <param name="fromDateTime">From date time.</param>
        /// <param name="toDateTime">To date time.</param>
        private async void StartDownload( FinancialGateway gateway, Merchant merchant, string batchNamePrefix, string batchUrlFormat,
            int? defaultCampusId, DefinedValueCache recordStatus, DefinedValueCache connectionStatus,
            DateTime fromDateTime, DateTime toDateTime, string recipientEmail )
        {
            var downloadId = Guid.NewGuid();
            var cancellationMinutes = GetAttributeValue( AttributeKey.ManualDownloadPaymentTimeout ).AsInteger();
            var cancellationTime = new TimeSpan( 0, cancellationMinutes, 0 );

            using ( var tokenSource = new CancellationTokenSource( cancellationTime ) )
            {
                try
                {
                    var cancellationToken = tokenSource.Token;

                    Task task = Task.Run( () => {

                        string resultSummary = MerchantService.ProcessPayments( gateway, merchant, batchNamePrefix, batchUrlFormat,
                            defaultCampusId, recordStatus, connectionStatus, fromDateTime, toDateTime, null, cancellationToken );
                        
                        if ( string.IsNullOrWhiteSpace( resultSummary ) )
                        {
                            resultSummary = "Your Pushpay download completed successfully, but there were not any transactions to download.";
                        }

                        SendResults( resultSummary, recipientEmail );

                    }, cancellationToken );

                    RockLogger.Log.Information( RockLogDomains.Finance, $"[Pushpay] Manual download started ({downloadId})." );

                    await task;

                    RockLogger.Log.Information( RockLogDomains.Finance, $"[Pushpay] Manual download succeeded ({downloadId})." );
                }
                catch ( OperationCanceledException )
                {
                    // Timeout exceeded.  Log and notify user.
                    RockLogger.Log.Information( RockLogDomains.Finance, $"[Pushpay] Manual download failed due to time out ({downloadId})." );
                    var resultSummary = "Your Pushpay download failed due to exceeding the timeout duration.";
                    SendResults( resultSummary, recipientEmail );
                }
                catch ( Exception ex )
                {
                    // Exception occurred.  Log and notify user, then re-throw the exception.
                    RockLogger.Log.Information( RockLogDomains.Finance, $"[Pushpay] Manual download failed due to exception ({downloadId}).  Exception: {ex.Message}" );
                    var resultSummary = $"Your Pushpay download failed due to an exception.  The exception was {ex.Message}.";
                    SendResults( resultSummary, recipientEmail );
                    throw ex;
                }
            }
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

        /// <summary>
        /// Handles the SaveClick event of the mdDownload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdDownload_SaveClick( object sender, EventArgs e )
        {
            HideDialog();
        }

        #endregion Events

        #region Private Methods

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
                                MemoField = m.MemoReferenceFieldName,
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

        /// <summary>
        /// Send manual download results by email.
        /// </summary>
        /// <param name="resultSummary">The message body.</param>
        /// <param name="resultsEmail">The email address of the recipient.</param>
        private void SendResults( string resultSummary, string resultsEmail )
        {
            var recipient = RockEmailMessageRecipient.CreateAnonymous( resultsEmail, new Dictionary<string, object>() );
            var message = new RockEmailMessage
            {
                Message = resultSummary
            };
            message.AddRecipient( recipient );
            message.Send();
        }

        #endregion Private Methods

    }
}