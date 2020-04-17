using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

using com.bemaservices.RemoteCheckDeposit;
using com.bemaservices.RemoteCheckDeposit.Model;
using System.ComponentModel;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;
using System.Text;
using System.IO;

namespace RockWeb.Plugins.com_bemaservices.RemoteCheckDeposit
{
    [DisplayName( "Remote Deposit Export" )]
    [Category( "BEMA Services > Remote Check Deposit" )]
    [Description( "Exports batch data for use remote deposit with a bank." )]
    public partial class RemoteDepositExport : RockBlock
    {
        // Dictionaries to cache values for performance
        //private static Dictionary<int, FinancialAccount> _financialAccountLookup;
        enum IsDeposited { Yes, No }

        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            //gBatches.Actions.ShowExcelExport = false;
            gBatches.Actions.ShowMergeTemplate = false;
            gBatches.ShowHeaderWhenEmpty = true;
            gBatches.ShowActionsInHeader = true;

            //var lbSelectBatches = new LinkButton
            //{
            //    ID = "lbSelectBatches",
            //    CssClass = "btn btn-default btn-sm",
            //    Text = "<i class='fa fa-download'></i>"
            //};
            //lbSelectBatches.Click += lbSelectBatches_Click;
            //gBatches.Actions.AddCustomActionControl( lbSelectBatches );
            gBatches.DataKeyNames = new string[] { "Id" };
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                var fileFormatService = new ImageCashLetterFileFormatService( new RockContext() );
                var fileFormats = fileFormatService.Queryable().Where( f => f.IsActive == true );

                ddlFileFormat.Items.Clear();
                ddlFileFormat.Items.Add( new ListItem() );
                foreach ( var fileFormat in fileFormats )
                {
                    ddlFileFormat.Items.Add( new ListItem( fileFormat.Name, fileFormat.Id.ToString() ) );
                }

                BindBatchesFilter();
                BindBatchesGrid();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the batches grid.
        /// </summary>
        private void BindBatchesGrid()
        {
            try
            {
                var rockContext = new RockContext();

                //_financialAccountLookup = new FinancialAccountService( rockContext ).Queryable().AsNoTracking().ToList().ToDictionary( k => k.Id, v => v );

                var financialBatchQry = GetQuery( rockContext )
                    .AsNoTracking()
                    .Include( b => b.Campus );

                gBatches.SetLinqDataSource( financialBatchQry );
                //gBatches.ObjectList = ( ( List<FinancialBatch> ) gBatches.DataSource ).ToDictionary( k => k.Id.ToString(), v => v as object );
                //gBatches.EntityTypeId = EntityTypeCache.Get<FinancialBatch>().Id;

                gBatches.DataBind();
            }
            catch ( Exception ex )
            {
                nbWarningMessage.Text = ex.Message;
            }
        }

        /// <summary>
        /// Binds the batches filter.
        /// </summary>
        private void BindBatchesFilter()
        {
            string titleFilter = gfBatches.GetUserPreference( "Title" );
            tbTitle.Text = !string.IsNullOrWhiteSpace( titleFilter ) ? titleFilter : string.Empty;

            ddlStatus.BindToEnum<BatchStatus>();
            ddlStatus.Items.Insert( 0, Rock.Constants.All.ListItem );
            string statusFilter = gfBatches.GetUserPreference( "Status" );
            if ( string.IsNullOrWhiteSpace( statusFilter ) )
            {
                statusFilter = BatchStatus.Closed.ConvertToInt().ToString();
            }
            ddlStatus.SetValue( statusFilter );

            var campuses = CampusCache.All();
            campCampus.Campuses = campuses;
            campCampus.Visible = campuses.Any();
            campCampus.SetValue( gfBatches.GetUserPreference( "Campus" ) );

            drpBatchDate.DelimitedValues = gfBatches.GetUserPreference( "Date Range" );

            ddlDeposited.BindToEnum<IsDeposited>();
            ddlDeposited.Items.Insert(0, Rock.Constants.All.ListItem);
            string depositedFilter = gfBatches.GetUserPreference("Deposited");
            if ( string.IsNullOrWhiteSpace( depositedFilter ) )
            {
                depositedFilter = IsDeposited.No.ConvertToInt().ToString();
            }
            ddlDeposited.SetValue( depositedFilter );
        }

        /// <summary>
        /// Gets the query.  Set the timeout to 90 seconds in case the user
        /// has not set any filters and they've imported N years worth of
        /// batch data into Rock.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private IOrderedQueryable<FinancialBatch> GetQuery( RockContext rockContext )
        {
            var batchService = new FinancialBatchService( rockContext );
            rockContext.Database.CommandTimeout = 90;
            var qry = batchService.Queryable().AsNoTracking()
                .Where( b => b.BatchStartDateTime.HasValue );

            // filter by date
            string dateRangeValue = gfBatches.GetUserPreference( "Date Range" );
            if ( !string.IsNullOrWhiteSpace( dateRangeValue ) )
            {
                var drp = new DateRangePicker();
                drp.DelimitedValues = dateRangeValue;
                if ( drp.LowerValue.HasValue )
                {
                    qry = qry.Where( b => b.BatchStartDateTime >= drp.LowerValue.Value );
                }

                if ( drp.UpperValue.HasValue )
                {
                    var endOfDay = drp.UpperValue.Value.AddDays( 1 );
                    qry = qry.Where( b => b.BatchStartDateTime < endOfDay );
                }
            }

            // filter by status
            var status = gfBatches.GetUserPreference( "Status" ).ConvertToEnumOrNull<BatchStatus>();
            if ( status.HasValue )
            {
                qry = qry.Where( b => b.Status == status );
            }

            // filter by title
            string title = gfBatches.GetUserPreference( "Title" );
            if ( !string.IsNullOrEmpty( title ) )
            {
                qry = qry.Where( batch => batch.Name.Contains( title ) );
            }

            // filter by campus
            var campus = CampusCache.Get( gfBatches.GetUserPreference( "Campus" ).AsInteger() );
            if ( campus != null )
            {
                qry = qry.Where( b => b.CampusId == campus.Id );
            }

            var deposited = gfBatches.GetUserPreference( "Deposited" ).ConvertToEnumOrNull<IsDeposited>();

            if ( deposited.HasValue )
            {
                var attributeQry = new AttributeValueService(rockContext).Queryable().AsNoTracking().Where(av => av.Attribute.Key == "com.bemaservices.Deposited");
                // how to I query an attribute on linq???
                if ( deposited == IsDeposited.Yes )
                {
                    qry = qry.Where(b => attributeQry.Any( av => av.EntityId == b.Id && av.ValueAsBoolean == true ) );
                }
                else if ( deposited == IsDeposited.No )
                {
                    qry = qry.Where(b => !attributeQry.Any( av => av.EntityId == b.Id && av.ValueAsBoolean == true ) );
                }
            }

            IOrderedQueryable<FinancialBatch> sortedQry = null;

            SortProperty sortProperty = gBatches.SortProperty;
            if ( sortProperty != null )
            {
                switch ( sortProperty.Property )
                {
                    case "TransactionCount":
                        {
                            if ( sortProperty.Direction == SortDirection.Ascending )
                            {
                                sortedQry = qry.OrderBy( b => b.Transactions.Count() );
                            }
                            else
                            {
                                sortedQry = qry.OrderByDescending( b => b.Transactions.Count() );
                            }

                            break;
                        }

                    case "TransactionAmount":
                        {
                            if ( sortProperty.Direction == SortDirection.Ascending )
                            {
                                sortedQry = qry.OrderBy( b => b.Transactions.Sum( t => ( decimal? ) ( t.TransactionDetails.Sum( d => ( decimal? ) d.Amount ) ?? 0.0M ) ) ?? 0.0M );
                            }
                            else
                            {
                                sortedQry = qry.OrderByDescending( b => b.Transactions.Sum( t => ( decimal? ) ( t.TransactionDetails.Sum( d => ( decimal? ) d.Amount ) ?? 0.0M ) ) ?? 0.0M );
                            }

                            break;
                        }

                    default:
                        {
                            sortedQry = qry.Sort( sortProperty );
                            break;
                        }
                }
            }
            else
            {
                sortedQry = qry
                    .OrderByDescending( b => b.BatchStartDateTime )
                    .ThenBy( b => b.Name );
            }

            return sortedQry;
        }
        
        /// <summary>
                 /// Formats the value as currency.
                 /// </summary>
                 /// <param name="value">The value.</param>
                 /// <returns></returns>
        protected string FormatValueAsCurrency( decimal value )
        {
            return value.FormatAsCurrency();
        }

        /// <summary>
        /// Validates the selection to ensure that all MICR numbers are valid.
        /// </summary>
        protected bool ValidateSelection( bool showAll = false )
        {
            var rockContext = new RockContext();
            var batchIds = hfBatchIds.Value.SplitDelimitedValues().AsIntegerList();

            //
            // Check for a bad MICR scan.
            //
            var transactions = new FinancialBatchService( rockContext ).Queryable()
                .Where( b => batchIds.Contains( b.Id ) )
                .SelectMany( b => b.Transactions )
                .ToList()
                .Where( t => string.IsNullOrEmpty( t.CheckMicrHash ) || !Micr.IsValid( Rock.Security.Encryption.DecryptString( t.CheckMicrEncrypted ) ) ||
                        t.Images.Count < 2 )
                .OrderBy( t => t.Id )
                .ToList();

            if ( transactions.Count > 0 || showAll )
            {
                string routingNumber;
                string accountNumber;
                string checkNumber;
                List<MicrRow> micrRowList = new List<MicrRow>();

                foreach ( var transaction in new FinancialBatchService( rockContext ).Queryable()
                    .Where( b => batchIds.Contains( b.Id ) )
                    .SelectMany( b => b.Transactions )
                    .ToList() )
                {
                    routingNumber = "";
                    accountNumber = "";
                    checkNumber = "";
                    try
                    {
                        var micr = new Micr( Rock.Security.Encryption.DecryptString( transaction.CheckMicrEncrypted ) );
                        routingNumber = micr.GetRoutingNumber();
                        accountNumber = micr.GetAccountNumber();
                        checkNumber = micr.GetCheckNumber();
                    }
                    catch { /* Intentionally left blank */ }

                    List<string> errors = new List<string>();
                    bool isValid = Micr.IsValid(Rock.Security.Encryption.DecryptString(transaction.CheckMicrEncrypted), out errors );
                    bool hasImages = transaction.Images.Count == 2;

                    if ( !hasImages )
                    {
                        errors.Add("Transaction does not contain two images (front and back of check) ");
                    }

                    MicrRow micrRow = new MicrRow()
                    {
                        TransactionId = transaction.Id,
                        RoutingNumber = routingNumber,
                        AccountNumber = accountNumber,
                        CheckNumber = checkNumber,
                        Amount = transaction.TotalAmount,
                        ImageUrl = transaction.Images.FirstOrDefault().IsNotNull() ? transaction.Images.First().BinaryFile.Url : "",
                        IsValid = ( isValid && hasImages ),
                        IsValidMessage = errors.AsDelimited("<br/>")
                    };

                    micrRowList.Add( micrRow );
                }

                rptMicrDetail.DataSource = micrRowList;
                rptMicrDetail.DataBind();

                pnlBatches.Visible = false;
                pnlFixMicr.Visible = true;

                return false;
            }
            else
            {
                //ShowOptions();
                return true;
            }
        }

        protected void ShowOptions()
        {
            var rockContext = new RockContext();
            var batchIds = hfBatchIds.Value.SplitDelimitedValues().AsIntegerList();

            decimal total = new FinancialBatchService( rockContext ).Queryable()
                .Where( b => batchIds.Contains( b.Id ) )
                .ToList()
                .Sum( b => b.Transactions.Sum( t => t.TotalAmount ) );

            lTotalDeposit.Text = total.FormatAsCurrency();

            dpBusinessDate.SelectedDateTime = RockDateTime.Now.SundayDate().AddDays( -7 );

            pnlBatches.Visible = false;
            pnlFixMicr.Visible = false;
            pnlOptions.Visible = true;
        }

        protected void ExportBatches()
        {
            using ( var rockContext = new RockContext() )
            {
                var batchIds = hfBatchIds.Value.SplitDelimitedValues().AsIntegerList();
                var batches = new FinancialBatchService( rockContext ).Queryable()
                    .AsNoTracking()
                    .Where( b => batchIds.Contains( b.Id ) )
                    .ToList();
                var fileFormat = new ImageCashLetterFileFormatService( rockContext ).Get( ddlFileFormat.SelectedValue.AsInteger() );
                var component = FileFormatTypeContainer.GetComponent( fileFormat.EntityType.Name );
                List<string> errorMessages;

                fileFormat.LoadAttributes( rockContext );

                // Create a counter for each day's exports
                string counterKey = "com.bemaservices.RemoteCheckDeposit.CounterKey";
                string valueFromKey = Rock.Web.SystemSettings.GetValue(counterKey);
                int counter = 1;
                if ( valueFromKey.IsNotNullOrWhiteSpace() && valueFromKey.Split('|').Count() == 2 )
                {
                    var keyArray = valueFromKey.Split('|');
                    if ( keyArray[0].AsDateTime() >= RockDateTime.Today )
                    {
                        counter = keyArray[1].AsInteger() + 1;
                    }
                }
                //Set key to DateTime of Today with a counter of 1
                string keyString = RockDateTime.Today.ToString() + "|" + counter.ToString();
                Rock.Web.SystemSettings.SetValue(counterKey, keyString);

                //
                // Get the final filename for the export.
                //
                var mergeFields = new Dictionary<string, object>
                {
                    {  "FileFormat", fileFormat },
                    { "Counter", counter }
                };
                var filename = fileFormat.FileNameTemplate.ResolveMergeFields( mergeFields );

                //
                // Construct the export options.
                //
                var options = new ExportOptions( fileFormat, batches );
                options.BusinessDateTime = dpBusinessDate.SelectedDateTime.Value;
                options.ExportDateTime = RockDateTime.Now;

                //
                // Perform the export.
                //
                Stream stream = null;
                try
                {
                    stream = component.ExportBatches( options, out errorMessages );
                    foreach ( var batch in batches )
                    {
                        batch.LoadAttributes();
                        batch.SetAttributeValue( "com.bemaservices.Deposited", "True" );
                        batch.SaveAttributeValue( "com.bemaservices.Deposited" );
                    }
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                    nbWarningMessage.Text = ex.Message;
                    return;
                }

                //
                // Save the data to the database.
                //
                var binaryFileService = new BinaryFileService( rockContext );
                var binaryFileTypeService = new BinaryFileTypeService( rockContext );
                var binaryFile = new BinaryFile
                {
                    BinaryFileTypeId = binaryFileTypeService.Get( Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid() ).Id,
                    IsTemporary = true,
                    FileName = filename,
                    MimeType = "application/octet-stream", //"octet/stream",
                    ContentStream = stream
                };

                binaryFileService.Add( binaryFile );
                rockContext.SaveChanges();

                //
                // Present download link.
                //
                pnlOptions.Visible = false;
                pnlSuccess.Visible = true;
                hlDownload.NavigateUrl = ResolveUrl( string.Format( "~/GetFile.ashx?Id={0}&attachment=True", binaryFile.Id ) );
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the lbExport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbExport_Click( object sender, EventArgs e )
        {
            if ( ValidateSelection() )
            {
                ExportBatches();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSelectBatches control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSelectBatches_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var batchIds = gBatches.SelectedKeys.Cast<int>().ToList();

            //if ( !batchIds.Any() )
            //{
            //    batchIds = GetQuery( rockContext ).Select( b => b.Id ).ToList();
            //}
            hfBatchIds.Value = string.Join( ",", batchIds.Select( i => i.ToString() ) );

            //if ( ValidateSelection() )
            //{
            //    ShowOptions();
            //}

            ValidateSelection( true );
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfBatches control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfBatches_ApplyFilterClick( object sender, EventArgs e )
        {
            gfBatches.SaveUserPreference( "Date Range", drpBatchDate.DelimitedValues );
            gfBatches.SaveUserPreference( "Title", tbTitle.Text );
            gfBatches.SaveUserPreference( "Status", ddlStatus.SelectedValue );
            gfBatches.SaveUserPreference( "Campus", campCampus.SelectedValue );
            gfBatches.SaveUserPreference( "Deposited", ddlDeposited.SelectedValue );

            BindBatchesGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfBatches control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfBatches_ClearFilterClick( object sender, EventArgs e )
        {
            gfBatches.DeleteUserPreferences();
            BindBatchesFilter();
        }

        /// <summary>
        /// Gets the batches display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfBatches_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Date Range":
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }

                case "Status":
                    {
                        var status = e.Value.ConvertToEnumOrNull<BatchStatus>();
                        e.Value = status.HasValue ? status.ConvertToString() : string.Empty;
                        break;
                    }

                case "Campus":
                    {
                        var campus = CampusCache.Get( e.Value.AsInteger() );
                        e.Value = campus != null ? campus.Name : string.Empty;
                        break;
                    }

                case "Deposited":
                    {
                        var deposited = e.Value.ConvertToEnumOrNull<IsDeposited>();
                        e.Value = deposited.HasValue ? deposited.ConvertToString() : string.Empty;
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gBatches control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gBatches_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindBatchesGrid();
        }

        /// <summary>
        /// Handles the RowCreated event of the gBatches control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gBatches_RowCreated( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow && e.Row.DataItem != null )
            {
                var batch = e.Row.DataItem as FinancialBatch;
                if ( batch != null )
                {
                    if ( batch.Attributes == null )
                    {
                        batch.LoadAttributes();
                    }

                    var batchRow = new BatchRow
                    {
                        Id = batch.Id,
                        BatchStartDateTime = batch.BatchStartDateTime.Value,
                        Name = batch.Name,
                        AccountingSystemCode = batch.AccountingSystemCode,
                        TransactionCount = batch.Transactions.Count(),
                        ControlAmount = batch.ControlAmount,
                        CampusName = batch.Campus != null ? batch.Campus.Name : "",
                        Status = batch.Status,
                        UnMatchedTxns = batch.Transactions.Any( t => !t.AuthorizedPersonAliasId.HasValue ),
                        BatchNote = batch.Note,
                        AccountSummaryList = batch.Transactions
                        .SelectMany( t => t.TransactionDetails )
                        .GroupBy( d => d.AccountId )
                        .Select( s => new BatchAccountSummary
                        {
                            AccountId = s.Key,
                            Amount = s.Sum( d => ( decimal? ) d.Amount ) ?? 0.0M,
                            AccountOrder = s.Select( d => d.Account.Order ).FirstOrDefault(),
                            AccountName = s.Select( d => d.Account.Name ).FirstOrDefault()
                        } )
                        .ToList(),
                        Deposited = batch.GetAttributeValue( "com.bemaservices.Deposited" ).AsBoolean()
                    };

                    e.Row.DataItem = batchRow;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            pnlFixMicr.Visible = false;
            pnlOptions.Visible = false;
            pnlBatches.Visible = true;

            BindBatchesGrid();
        }

        /// <summary>
        /// Handles the Click event of the lbFinished control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbFinished_Click( object sender, EventArgs e )
        {
            NavigateToCurrentPage();
        }

        /// <summary>
        /// Handles the Click event of the lbFixMicr control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbFixMicr_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var transactionService = new FinancialTransactionService( rockContext );

                //Loop through each check transaction and format details
                foreach ( RepeaterItem item in rptMicrDetail.Items )
                {
                    var hfFixMicrId = ( item.FindControl( "hfFixMicrId" ) as HiddenField );
                    var tbRoutingNumber = ( item.FindControl( "tbRoutingNumber" ) as RockTextBox );
                    var tbAccountNumber = ( item.FindControl( "tbAccountNumber" ) as RockTextBox );
                    var tbCheckNumber = ( item.FindControl( "tbCheckNumber" ) as RockTextBox );
                    var tbAmount = ( item.FindControl( "tbAmount" ) as CurrencyBox );

                    var transaction = transactionService.Get( hfFixMicrId.Value.AsInteger() );
                    var micrText = string.Format( "d{0}d{1}c{2}", tbRoutingNumber.Text, tbAccountNumber.Text, tbCheckNumber.Text );
                    transaction.CheckMicrEncrypted = Rock.Security.Encryption.EncryptString( micrText );
                    transaction.CheckMicrHash = Rock.Security.Encryption.GetSHA1Hash( micrText );

                    rockContext.SaveChanges();
                }

                if ( ValidateSelection() )
                {
                    ShowOptions();
                }
            }
            
        }

        #endregion

        #region Support Classes

        public class BatchAccountSummary
        {
            public int AccountId { get; set; }
            public int AccountOrder { get; set; }

            public string AccountName { get; set; }

            public decimal Amount { get; set; }

            public override string ToString()
            {
                return string.Format( "{0}: {1}", AccountName, Amount.FormatAsCurrency() );
            }
        }

        public class BatchRow
        {
            public int Id { get; set; }
            public DateTime BatchStartDateTime { get; set; }
            public string Name { get; set; }
            public string AccountingSystemCode { get; set; }
            public int TransactionCount { get; set; }

            public decimal TransactionAmount
            {
                get
                {
                    return AccountSummaryList.Select( a => a.Amount ).Sum();
                }
            }

            public decimal ControlAmount { get; set; }
            public List<BatchAccountSummary> AccountSummaryList
            {
                get
                {
                    return _accountSummaryList.OrderBy( a => a.AccountOrder ).ToList();
                }
                set
                {
                    _accountSummaryList = value;
                }
            }

            private List<BatchAccountSummary> _accountSummaryList;
            public string CampusName { get; set; }
            public BatchStatus Status { get; set; }
            public bool UnMatchedTxns { get; set; }
            public string BatchNote { get; set; }

            public decimal Variance
            {
                get
                {
                    return TransactionAmount - ControlAmount;
                }
            }

            public string AccountSummaryText
            {
                get
                {
                    var summary = new List<string>();
                    AccountSummaryList.ForEach( a => summary.Add( a.ToString() ) );
                    return summary.AsDelimited( Environment.NewLine );
                }
            }

            public string AccountSummaryHtml
            {
                get
                {
                    var summary = new List<string>();
                    AccountSummaryList.ForEach( a => summary.Add( a.ToString() ) );
                    return "<small>" + summary.AsDelimited( "<br/>" ) + "</small>";
                }
            }

            public string StatusText
            {
                get
                {
                    return Status.ConvertToString();
                }
            }


            public string StatusLabelClass
            {
                get
                {
                    switch ( Status )
                    {
                        case BatchStatus.Closed:
                            return "label label-default";
                        case BatchStatus.Open:
                            return "label label-info";
                        case BatchStatus.Pending:
                            return "label label-warning";
                    }

                    return string.Empty;
                }
            }

            public string Notes
            {
                get
                {
                    var notes = new StringBuilder();

                    switch ( Status )
                    {
                        case BatchStatus.Open:
                            {
                                if ( UnMatchedTxns )
                                {
                                    notes.Append( "<span class='label label-warning'>Unmatched Transactions</span><br/>" );
                                }

                                break;
                            }
                    }

                    notes.Append( BatchNote );

                    return notes.ToString();
                }
            }

            public bool Deposited { get; set; }
        }

        public class MicrRow
        {
            public int TransactionId { get; set; }

            public string RoutingNumber { get; set; }

            public string AccountNumber { get; set; }

            public string CheckNumber { get; set; }

            public decimal Amount { get; set; }

            public string ImageUrl { get; set; }

            public bool? IsValid { get; set; }

            public string IsValidMessage { get; set; }
        }

        #endregion
    }
}
