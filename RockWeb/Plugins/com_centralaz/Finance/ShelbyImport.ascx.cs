// <copyright>
// Copyright by Central Christian Church
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Microsoft.AspNet.SignalR;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using System.Diagnostics;

namespace RockWeb.Plugins.com_centralaz.Finance
{
    /// <summary>
    /// Block to import a system with Shelby contributions data into Rock.
    /// 
    /// In a nutshell it essentially works like this:
    ///  - User is asked to create/confirm Shelby Fund matching to Rock Accounts
    ///     - The Shelby PurCounter is added to a dictionary map (PurCounter -> FinancialAccount.Id)
    ///  - List of unique persons who contributed in Shelby is generated
    ///     - For each, a match is found or a new person/family record is created
    ///     - the Shelby NameCounter is added to a dictionary map (NameCounter -> Person.PersonAliasId)
    ///  - For each distinct batch (related to a contribution; select distinct BatchNu from [Shelby].[CNHst]) in Shelby
    ///         SELECT * FROM [Shelby].[CNBat] WHERE BatchNu IN (SELECT distinct BatchNu from [Shelby].[CNHst])
    ///     - Find matching or add FinancialBatch in Rock; if found, skip to next batch; when adding:
    ///     - TBD the Shelby BatchNu is added to a dictionary map (BatchNu -> FinancialBatch.Id)
    ///     - For each contribution in Shelby
    ///         - If CNHst.Counter same as previous, use previous FinancialTransaction (don't create a new one)
    ///         - else, create FinancialTransaction
    ///         - Create FinancialTransactionDetail
    ///         - Create FinancialPaymentDetail 
    ///         - Add FinancialPaymentDetail to FinancialTransaction.FinancialPaymentDetail
    ///         - Add FinancialTransactionDetail to FinancialTransaction.TransactionDetails
    ///         - Save
    ///     - commit transaction and move to next batch
    /// </summary>
    [DisplayName( "Shelby Import" )]
    [Category( "com_centralaz > Finance" )]
    [Description( "Finance block to import contribution data from a Shelby database. It will add new people if a match cannot be made, import the batches, and financial transactions." )]
    [DefinedValueField( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2", "", 0 )]

    [TextField( "Batch Name Prefix", "The prefix that should be used for the name of the batches that are created. Note: the Shelby Batch Number will be appended to the name.", true, "Glendale Shelby", order: 0 )]
    [LinkedPage( "Batch Detail Page", "The page used to display the contributions for a specific batch", true, "", "Linked Pages", 0 )]
    [LinkedPage( "Contribution Detail Page", "The page used to display the contribution transaction details", true, "",  "Linked Pages", 1 )]
    [TextField( "Who Mapping", "These map the names in the Shelby Who* columns to Rock personAliasIds. Delimit them with commas or semicolons, and write them in the format 'Shelby_who=Rock_value'.", false, "", "Data Mapping", 3 )]
    [TextField( "Fund Account Mappings", "The mapping between Shelby Funds and Rock Accounts. A comma delimited list of 'Shelby_value=Rock_value'.", false, "", "Data Mapping", 1 )]
    [GroupLocationTypeField( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, "Address Type", "The location type to use for a new person's address.", false,
        Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME, "", 11 )]
    [EncryptedTextField( "Shelby DB DataSource", "", true, @"ACC02\Shelby", "Remote Shelby DB", 0 )]
    [EncryptedTextField( "Shelby DB Catalog", "", true, "ShelbyDB", "Remote Shelby DB", 1 )]
    [EncryptedTextField( "Shelby DB UserId", "", true, "RockConversion", "Remote Shelby DB", 2 )]
    [EncryptedTextField( "Shelby DB Password", "", true, "", "Remote Shelby DB", 3 )]
    public partial class ShelbyImport : Rock.Web.UI.RockBlock
    {
        #region Fields
        /// <summary>
        /// This holds the reference to the RockMessageHub SignalR Hub context.
        /// </summary>
        private IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<RockMessageHub>();

        private static readonly string FUND_ACCOUNT_MAPPINGS = "FundAccountMappings";
        private List<ShelbyContribution> _errorElements = new List<ShelbyContribution>();
        private Stopwatch _stopwatch;
        private int _personRecordTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
        private int _personStatusPending = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() ).Id;
        private int _transactionTypeIdContribution = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() ).Id;

        private Regex reOnlyDigits = new Regex( @"^[0-9-\/\.]+$" );

        // Home Number type phone
        private int _homePhoneDefinedValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ).Id;

        // Connection Status
        private int connectionStatusDefinedValueId = -1;

        // Shelby Marital statuses: U, W, M, D, P, S
        private DefinedTypeCache _maritalStatusDefinedType = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() );

        // Holds the Fund Account Mappings block attribute as a dictionary (Shelby Purpose Counter -> Rock Account Id)
        private Dictionary<String, String> _fundAccountMappingDictionary = new Dictionary<string, string>();

        // Holds the Shelby NameCounter to Rock PersonAliasId map
        private static Dictionary<int, int> _shelbyPersonMappingDictionary = new Dictionary<int, int>();

        // Holds a record of the completed ShelbyContribution counters
        private static Dictionary<int, bool> _shelbyContributionsCompleted = new Dictionary<int, bool>();

        // Holds the Shelby Batch to Rock Batch map
        private Dictionary<int, int> _shelbyBatchMappingDictionary = new Dictionary<int, int>();

        // Holds the mapping between Shelby who and Rock PersonAliasIds
        private Dictionary<string, int> _shelbyWhoMappingDictionary = new Dictionary<string, int>();
        
        private Dictionary<int, string> _accountNames = null;
        private Dictionary<int, string> AllAccounts
        {
            get
            {
                if ( _accountNames == null )
                {
                    _accountNames = new Dictionary<int, string>();
                    _accountNames.Add( -1, "" );
                    new FinancialAccountService( new RockContext() ).Queryable()
                        .OrderBy( a => a.Order )
                        .Select( a => new { a.Id, a.Name } )
                        .ToList()
                        .ForEach( a => _accountNames.Add( a.Id, a.Name ) );
                }
                return _accountNames;
            }
        }
        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gBatchList.GridRebind += gBatchList_GridRebind;
            gBatchList.RowDataBound += gBatchList_RowDataBound;
            //gErrors.GridRebind += gErrors_GridRebind;
            //gErrors.RowDataBound += gErrors_RowDataBound;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            ScriptManager scriptManager = ScriptManager.GetCurrent( Page );
            scriptManager.RegisterPostBackControl( lbImport );

            RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.2.0.min.js", fingerprint: false );

            _fundAccountMappingDictionary = Regex.Matches( GetAttributeValue( FUND_ACCOUNT_MAPPINGS ), @"\s*(.*?)\s*=\s*(.*?)\s*(;|,|$)" )
                .OfType<Match>()
                .ToDictionary( m => m.Groups[1].Value, m => m.Groups[2].Value );
            connectionStatusDefinedValueId = DefinedValueCache.Read( GetAttributeValue( "ConnectionStatus" ).AsGuid() ).Id;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            Session.Timeout = 120;
            // Set timeout for up to 60 minutes (twice as long as the installer)
            Server.ScriptTimeout = 3600;
            ScriptManager.GetCurrent( Page ).AsyncPostBackTimeout = 3600;

            if ( !Page.IsPostBack )
            {
                VerifyOrSetAccountMappings();

                if ( string.IsNullOrEmpty( GetAttributeValue( "ContributionDetailPage" ) )  || string.IsNullOrEmpty( GetAttributeValue( "BatchDetailPage" ) ) )
                {
                    nbMessage.Text = "Invalid block settings.";
                    return;
                }

                tbBatchName.Text = GetAttributeValue( "BatchNamePrefix" );
                BindCampusPicker();
                BindGrid();
                BindErrorGrid();
            }

            if ( _shelbyPersonMappingDictionary != null )
            {
                lSessionStats.Text = string.Format( "people ({0}) ", _shelbyPersonMappingDictionary.Count );
            }
            if ( _shelbyContributionsCompleted != null )
            {
                lSessionStats.Text += string.Format( "contributions ({0})", _shelbyContributionsCompleted.Count );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbImport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbImport_Click( object sender, EventArgs e )
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            // clear any old errors:
            _errorElements = new List<ShelbyContribution>();
            nbMessage.Text = "";
            pnlErrors.Visible = false;
            int importedPeople = 0;
            int importedBatches = 0;
            int importedTransactions = 0;

            try
            {
                _shelbyWhoMappingDictionary = Regex.Matches( GetAttributeValue( "WhoMapping" ), @"\s*(.*?)\s*=\s*(.*?)\s*(;|,|$)" )
                .OfType<Match>()
                .ToDictionary( m => m.Groups[1].Value, m => m.Groups[2].Value.AsInteger() );

                _hubContext.Clients.All.showLog();
                importedPeople = ProcessPeople();

                // Don't continue if there were errors.
                if ( _errorElements.Count == 0 )
                {
                    importedBatches = ProcessBatches();
                    importedTransactions = ProcessTransactions();
                }

                BindGrid();
                pnlConfiguration.Visible = false;
            }
            catch ( Exception ex )
            {
                nbMessage.Text = string.Format( "Error: {0}", ex.Message );
                pnlErrors.Visible = true;
                BindErrorGrid();
            }

            _shelbyBatchMappingDictionary.Clear();
            _fundAccountMappingDictionary.Clear();
            //_shelbyPersonMappingDictionary.Clear();
            _shelbyWhoMappingDictionary.Clear();

            _shelbyBatchMappingDictionary = null;
            _fundAccountMappingDictionary = null;
            //_shelbyPersonMappingDictionary = null;
            _shelbyWhoMappingDictionary = null;

            if ( _errorElements.Count > 0 )
            {
                nbMessage.Text = "Errors found.";
                pnlErrors.Visible = true;
                BindErrorGrid();
            }
            else
            {
                nbSuccessMessage.Text = string.Format( "Imported {0} people, {1} batches and {2} transactions.", importedPeople, importedBatches, importedTransactions );
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gBatchList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gErrors control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gErrors_GridRebind( object sender, EventArgs e )
        {
            BindErrorGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gErrors control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gErrors_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var errorItem = e.Row.DataItem as ShelbyContribution;
                if ( errorItem != null )
                {
                    // TODO?
                }
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gBatchList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gBatchList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                FinancialTransactionDetail financialTransactionDetail = e.Row.DataItem as FinancialTransactionDetail;
                if ( financialTransactionDetail != null )
                {
                    Literal lTransactionID = e.Row.FindControl( "lTransactionID" ) as Literal;
                    if ( lTransactionID != null )
                    {
                        Dictionary<string, string> dictionaryInfo = new Dictionary<string, string>();
                        dictionaryInfo.Add( "transactionId", financialTransactionDetail.TransactionId.ToString() );
                        string url = LinkedPageUrl( "ContributionDetailPage", dictionaryInfo );
                        String theString = String.Format( "<a href=\"{0}\">{1}</a>", url, financialTransactionDetail.TransactionId.ToString() );
                        lTransactionID.Text = theString;
                    }

                    Literal lFullName = e.Row.FindControl( "lFullName" ) as Literal;
                    if ( lFullName != null )
                    {
                        String url = ResolveUrl( string.Format( "~/Person/{0}", financialTransactionDetail.Transaction.AuthorizedPersonAlias.PersonId ) );
                        String theString = String.Format( "<a href=\"{0}\">{1}</a>", url, financialTransactionDetail.Transaction.AuthorizedPersonAlias.Person.FullName );
                        lFullName.Text = theString;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            nbMessage.Text = "";
            if ( string.IsNullOrEmpty( GetAttributeValue( "ContributionDetailPage" ) ) || string.IsNullOrEmpty( GetAttributeValue( "BatchDetailPage" ) ) )
            {
                nbMessage.Text = "Invalid block settings.";
                return;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Formats the value as currency (called from markup)
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public string FormatValueAsCurrency( decimal value )
        {
            return value.FormatAsCurrency();
        }

        /// <summary>
        /// List of unique persons who contributed in Shelby is generated
        ///     - For each, a match is found or a new person/family record is created
        ///     - the Shelby NameCounter is added to a dictionary map (NameCounter -> Person.Id)
        /// </summary>
        private int ProcessPeople()
        {
            if ( Session["ShelbyImport:shelbyPersonMappingDictionary"] != null )
            {
                _shelbyPersonMappingDictionary = Session["ShelbyImport:shelbyPersonMappingDictionary"] as Dictionary<int, int>;
            }

            int totalCount = 0;
            int counter = 0;
            try
            {
                RockContext rockContext = new RockContext();
                //rockContext.Configuration.AutoDetectChangesEnabled = false;
                PersonService personService = new PersonService( rockContext );
                var connectionString = GetConnectionString();
                using ( SqlConnection connection = new SqlConnection( connectionString ) )
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;

                    // First count the total people
                    command.CommandText = @"SELECT COUNT(1) as 'Count'
FROM [Shelby].[NANames] N WITH(NOLOCK)
LEFT JOIN [Shelby].[NAAddresses] A WITH(NOLOCK) ON A.AddressCounter = N.MainAddress
LEFT JOIN [Shelby].[NAPhones] P WITH(NOLOCK) ON P.[NameCounter] = N.[NameCounter] AND P.[PhoneCounter] = 1
WHERE N.NameCounter IN ( SELECT H.NameCounter FROM [Shelby].[CNHst] H WITH(NOLOCK) )";
                    using ( SqlDataReader reader = command.ExecuteReader() )
                    {
                        if ( reader.HasRows )
                        {
                            while ( reader.Read() )
                            {
                                totalCount = ( int ) reader["Count"];
                            }
                        }
                    }

                    command.CommandText = @"SELECT P.[PhoneNu], N.[NameCounter], N.[EMailAddress], N.[Gender], N.[Salutation], N.[FirstMiddle], N.[LastName], N.[MaritalStatus], A.[Adr1], A.[Adr2], A.[City], A.[State], A.[PostalCode]
FROM [Shelby].[NANames] N WITH(NOLOCK)
LEFT JOIN [Shelby].[NAAddresses] A WITH(NOLOCK) ON A.[AddressCounter] = N.[MainAddress]
LEFT JOIN [Shelby].[NAPhones] P WITH(NOLOCK) ON P.[NameCounter] = N.[NameCounter] AND P.[PhoneCounter] = 1
WHERE N.[NameCounter] IN ( SELECT H.[NameCounter] FROM [Shelby].[CNHst] H WITH(NOLOCK) )
ORDER BY N.[NameCounter]";

                    using ( SqlDataReader reader = command.ExecuteReader() )
                    {
                        if ( reader.HasRows )
                        {
                            while ( reader.Read() )
                            {
                                counter++;
                                var shelbyPerson = new ShelbyPerson( reader );
                                int nameCounter = shelbyPerson.NameCounter;
                                int? personAliasId = null;

                                // Throw away the context and get a new one periodically to improve performance
                                // change detection.
                                if ( counter % 100 == 0 )
                                {
                                    rockContext = new RockContext();
                                    rockContext.Configuration.AutoDetectChangesEnabled = false;
                                    personService = new PersonService( rockContext );
                                }

                                // Skip it if the name counter is already mapped
                                if ( !_shelbyPersonMappingDictionary.ContainsKey( nameCounter ) )
                                {
                                    personAliasId = FindOrCreateNewPerson( personService, shelbyPerson, counter );

                                    if ( personAliasId != null )
                                    {
                                        _shelbyPersonMappingDictionary.AddOrReplace( nameCounter, personAliasId.Value );
                                    }
                                    else
                                    {
                                        var shelbyContribution = new ShelbyContribution();
                                        shelbyContribution.ERROR = string.Format( "Person with Shelby NameCounter {0} could not be found.", nameCounter );
                                        _errorElements.Add( shelbyContribution );
                                        break;
                                    }
                                }
                                NotifyClientProcessingUsers( counter, totalCount );
                            }
                        }
                        Session["ShelbyImport:shelbyPersonMappingDictionary"] = _shelbyPersonMappingDictionary;
                        reader.Close();
                    }
                }
            }
            catch ( Exception ex )
            {
                nbMessage.Text = string.Format( "Your database block settings are not valid or the remote database server is offline or mis-configured. {0}<br/><pre>{1}</pre>", ex.Message, ex.StackTrace );
            }

            return counter;
        }

        /// <summary>
        /// Process the batches from the remote Shelby DB.
        /// 
        ///  - For each distinct batch (related to a contribution; select distinct BatchNu from [Shelby].[CNHst]) in Shelby
        ///         SELECT * FROM [Shelby].[CNBat] WHERE BatchNu IN (SELECT distinct BatchNu from [Shelby].[CNHst])
        ///     - Find matching or add FinancialBatch in Rock; if found, skip to next batch; when adding:
        ///         - Set the FinancialBatch.ForeignKey to the CNBat.BatchNu
        ///         - Set the FinancialBatch.ControlAmount to the CNBat.Total
        ///         - Set the FinancialBatch.Note to the CNBat.NuContr
        ///         - Set the FinancialBatch.CreatedByPersonAliasId to the CNBat.WhoSetup
        ///         - Set the FinancialBatch.CreatedDateTime to the CNBat.WhenSetup
        ///     - TBD the Shelby BatchNu is added to a dictionary map (BatchNu -> FinancialBatch.Id)
        ///     - commit batch transaction and move to next batch
        /// </summary>
        private int ProcessBatches()
        {
            int totalCount = 0;
            int counter = 0;
            try
            {
                RockContext rockContext = new RockContext();
                rockContext.Configuration.AutoDetectChangesEnabled = false;
                FinancialBatchService batchService = new FinancialBatchService( rockContext );
                var connectionString = GetConnectionString();
                using ( SqlConnection connection = new SqlConnection( connectionString ) )
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;

                    // First count the total
                    command.CommandText = string.Format( @"SELECT COUNT(1) as 'Count' FROM [Shelby].[CNBat] WITH(NOLOCK) WHERE [BatchNu] BETWEEN {0} AND {1} AND [BatchNu] IN (SELECT distinct BatchNu from [Shelby].[CNHst] WITH(NOLOCK))", nreBatchRange.LowerValue, nreBatchRange.UpperValue );
                    using ( SqlDataReader reader = command.ExecuteReader() )
                    {
                        if ( reader.HasRows )
                        {
                            while ( reader.Read() )
                            {
                                totalCount = ( int ) reader["Count"];
                            }
                        }
                    }

                    command.CommandText = string.Format( @"SELECT [BatchNu], [NuContr], [Total], [WhenPosted], [WhenSetup], [WhoSetup] FROM [Shelby].[CNBat] WITH(NOLOCK) WHERE [BatchNu] BETWEEN {0} AND {1} AND [BatchNu] IN (SELECT distinct [BatchNu] from [Shelby].[CNHst] WITH(NOLOCK)) ORDER BY [BatchNu]", nreBatchRange.LowerValue, nreBatchRange.UpperValue );

                    using ( SqlDataReader reader = command.ExecuteReader() )
                    {
                        if ( reader.HasRows )
                        {
                            while ( reader.Read() )
                            {
                                counter++;
                                var shelbyBatch = new ShelbyBatch( reader );

                                int? rockBatchId = FindOrCreateNewBatch( batchService, shelbyBatch );
                                NotifyClientProcessingBatches( counter, totalCount );

                                if ( rockBatchId != null )
                                {
                                    _shelbyBatchMappingDictionary.AddOrReplace( shelbyBatch.BatchNu, rockBatchId.Value );
                                }
                            }
                        }

                        reader.Close();
                    }
                }
            }
            catch ( Exception ex )
            {
                nbMessage.Text = string.Format( "Your database block settings are not valid or the remote database server is offline or mis-configured. {0}<br/><pre>{1}</pre>", ex.Message, ex.StackTrace );
            }

            return counter;
        }

        /// <summary>
        /// Finds and returns the matching batchId or creates a new one and returns the batchId: 
        ///   - Find matching or add FinancialBatch in Rock; if found, skip to next batch; when adding:
        ///   - Set the FinancialBatch.ForeignKey to the CNBat.BatchNu
        ///   - Set the FinancialBatch.ControlAmount to the CNBat.Total
        ///   - Set the FinancialBatch.Note to the CNBat.NuContr
        ///   - Set the FinancialBatch.CreatedByPersonAliasId to the CNBat.WhoSetup
        ///   - Set the FinancialBatch.CreatedDateTime to the CNBat.WhenSetup
        /// </summary>
        private int? FindOrCreateNewBatch( FinancialBatchService batchService, ShelbyBatch shelbyBatch )
        {
            string batchNu = shelbyBatch.BatchNu.ToStringSafe();
            var exactBatch = batchService.Queryable().Where( p => p.ForeignKey == batchNu ).FirstOrDefault();

            if ( exactBatch != null )
            {
                return exactBatch.Id;
            }

            var financialBatch = new FinancialBatch();
            financialBatch.Name = tbBatchName.Text + " " + batchNu;
            financialBatch.BatchStartDateTime = shelbyBatch.WhenSetup; // Confirmed by Michele A.
            //financialBatch.BatchStartDateTime = shelbyBatch.WhenPosted;
            financialBatch.ControlAmount = shelbyBatch.Total;
            financialBatch.ForeignKey = batchNu;
            financialBatch.Note = shelbyBatch.NuContr.ToStringSafe();
            financialBatch.CreatedDateTime = shelbyBatch.WhenSetup;
            
            if ( _shelbyWhoMappingDictionary.ContainsKey( shelbyBatch.WhoSetup ) )
            {
                financialBatch.CreatedByPersonAliasId = _shelbyWhoMappingDictionary[ shelbyBatch.WhoSetup ];
            }

            int? campusId = cpCampus.SelectedCampusId;

            if ( campusId != null )
            {
                financialBatch.CampusId = campusId;
            }
            else
            {
                var campuses = CampusCache.All();
                financialBatch.CampusId = campuses.FirstOrDefault().Id;
            }

            batchService.Add( financialBatch );
            RockContext rockContext = (RockContext) batchService.Context;
            rockContext.ChangeTracker.DetectChanges();
            rockContext.SaveChanges( disablePrePostProcessing: true );

            return financialBatch.Id;
        }

        /// <summary>
        /// Process transactions.
        ///     - For each contribution in Shelby
        ///         - If CNHst.Counter same as previous, use previous FinancialTransaction (don't create a new one)
        ///         - else, create FinancialTransaction
        ///             - Set the FinancialTransaction.TransactionTypeValueId = 53 (Contribution)
        ///             - Set the FinancialTransaction.SourceTypeValueId = (10=Website, 511=Kiosk, 512=Mobile Application, 513=On-Site Collection, 593=Bank Checks)
        ///             - Set the FinancialTransaction.Summary to the CNHst.Memo
        ///             - Set the FinancialTransactionDetail.TransactionCode to the CNHst.CheckNu
        ///         - Create FinancialTransactionDetail
        ///             - Set Amount = [CNHstDet].Amount
        ///             - Set AccountId = (lookup PurCounter AccountPurpose dictionary)
        ///             
        ///         - Create FinancialPaymentDetail 
        ///             - Set CurrencyTypeValueId =  (6=Cash, 9=Check, 156=Credit Card, 157=ACH, 1493=Non-Cash, 1554=Debit)
        ///                 - 6 if CheckNu="CASH"
        ///                 - 1493 if CheckNu="ONLINE", "GIVING*CENTER", "ONLINEGIVING", "PAYPAL"
        ///                 - 9 if CheckNu is all numbers (or "CHECK")
        ///                 
        ///         - Add FinancialPaymentDetail to FinancialTransaction.FinancialPaymentDetail
        ///         - Add FinancialTransactionDetail to FinancialTransaction.TransactionDetails
        ///         - Save
        /// </summary>
        private int ProcessTransactions()
        {
            int totalCount = 0;
            int counter = 0;
            int previousTransactionCounter = -1;
            var shelbyContributionsSet = new Queue<ShelbyContribution>();
            
            if ( Session["ShelbyImport:shelbyContributionsCompleted"] != null )
            {
                _shelbyContributionsCompleted = Session["ShelbyImport:shelbyContributionsCompleted"] as Dictionary<int, bool>;
            }

            try
            {
                RockContext rockContext = new RockContext();
                rockContext.Configuration.AutoDetectChangesEnabled = false;
                FinancialTransactionService transactionService = new FinancialTransactionService( rockContext );
                var connectionString = GetConnectionString();
                using ( SqlConnection connection = new SqlConnection( connectionString ) )
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;

                    // First count the total
                    command.CommandText = string.Format( @"SELECT COUNT(1) as 'Count' FROM [ShelbyDB].[Shelby].[CNHst] H WITH(NOLOCK) INNER JOIN [ShelbyDB].[Shelby].[CNHstDet] D WITH(NOLOCK) ON D.[HstCounter] = H.[Counter]
WHERE [BatchNu] BETWEEN {0} and {1}", nreBatchRange.LowerValue, nreBatchRange.UpperValue );
                    using ( SqlDataReader reader = command.ExecuteReader() )
                    {
                        if ( reader.HasRows )
                        {
                            while ( reader.Read() )
                            {
                                totalCount = ( int ) reader["Count"];
                            }
                        }
                    }

                    command.CommandText = string.Format( @"SELECT
	H.[Counter]
	,H.[Amount]
	,H.[BatchNu]
	,H.[CheckNu]
	,H.[CNDate]
	,H.[Memo]
	,H.[NameCounter]
	,H.[WhenSetup]
	,H.[WhenUpdated]
	,H.[WhoSetup]
	,H.[WhoUpdated]
	,H.[CheckType]
    ,D.[Counter] as 'DetailCounter'
	,D.[PurCounter]
	,P.[Descr]
	,D.[Amount] as 'PurAmount'
  FROM [ShelbyDB].[Shelby].[CNHst] H WITH(NOLOCK)
  INNER JOIN [ShelbyDB].[Shelby].[CNHstDet] D WITH(NOLOCK) ON D.[HstCounter] = H.[Counter]
  INNER JOIN [ShelbyDB].[Shelby].[CNPur] P WITH(NOLOCK) ON P.[Counter]= D.[PurCounter]
  WHERE [BatchNu] BETWEEN {0} AND {1}
  ORDER BY H.[Counter]
", nreBatchRange.LowerValue, nreBatchRange.UpperValue );
                    ;

                    using ( SqlDataReader reader = command.ExecuteReader() )
                    {
                        if ( reader.HasRows )
                        {
                            while ( reader.Read() )
                            {
                                counter++;
                                var shelbyContribution = new ShelbyContribution( reader );

                                try
                                {
                                    // If we're on the first item, then the "previous" is this first one...
                                    if ( previousTransactionCounter == -1 )
                                    {
                                        previousTransactionCounter = shelbyContribution.Counter;
                                    }

                                    // Is the next item just another detail record for the same transaction?
                                    // If so, just add it to the set and move to the next record...
                                    if ( previousTransactionCounter == shelbyContribution.Counter )
                                    {
                                        shelbyContributionsSet.Enqueue( shelbyContribution );
                                    }
                                    else
                                    {
                                        // Otherwise, process the set...
                                        // But skip it if the counter is already recorded/saved (recorded via Session).
                                        if ( !_shelbyContributionsCompleted.ContainsKey( shelbyContribution.Counter ) )
                                        {
                                            // Otherwise we finish/write the previous set, and then clear the set and move to the next item
                                            FindOrCreateTransaction( transactionService, shelbyContributionsSet, counter );
                                            _shelbyContributionsCompleted.Add( shelbyContribution.Counter, true );
                                        }
                                        else
                                        {
                                            // Clear the set and add the current record to the set.
                                            shelbyContributionsSet.Clear();
                                        }

                                        shelbyContributionsSet.Enqueue( shelbyContribution );
                                        previousTransactionCounter = shelbyContribution.Counter;
                                    }

                                    NotifyClientProcessingTransactions( counter, totalCount );

                                }
                                catch ( Exception ex )
                                {
                                    shelbyContribution.ERROR = ex.Message;
                                    _errorElements.Add( shelbyContribution );
                                }
                            }

                            // Check the last set and finish/write it...
                           FindOrCreateTransaction( transactionService, shelbyContributionsSet, counter );
                           rockContext.SaveChanges( disablePrePostProcessing: true );
                        }

                        Session["ShelbyImport:shelbyContributionsCompleted"] = _shelbyContributionsCompleted;
                        reader.Close();
                    }
                }
            }
            catch ( Exception ex )
            {
                nbMessage.Text = string.Format( "Your database block settings are not valid or the remote database server is offline or mis-configured. {0}<br/><pre>{1}</pre>", ex.Message, ex.StackTrace );
            }

            return counter;
        }

        /// <summary>
        /// Finds and returns the matching financial transaction or creates a new one and returns it: 
        ///     - For each new transaction and detail in the set...
        ///         - If CNHst.Counter same as previous, use previous FinancialTransaction (don't create a new one)
        ///         - else, create FinancialTransaction
        ///             - Set the FinancialTransaction.TransactionTypeValueId = 53 (Contribution)
        ///             - Set the FinancialTransaction.SourceTypeValueId = (10=Website, 511=Kiosk, 512=Mobile Application, 513=On-Site Collection, 593=Bank Checks)
        ///             - Set the FinancialTransaction.Summary to the CNHst.Memo
        ///             - Set the FinancialTransaction.TransactionCode to the CNHst.CheckNu
        ///         - Create FinancialTransactionDetail
        ///             - Set Amount = [CNHstDet].Amount
        ///             - Set AccountId = (lookup PurCounter AccountPurpose dictionary)
        ///             
        ///         - Create FinancialPaymentDetail 
        ///             - Set CurrencyTypeValueId =  (6=Cash, 9=Check, 156=Credit Card, 157=ACH, 1493=Non-Cash, 1554=Debit)
        ///                 - 6 if CheckNu="CASH"
        ///                 - 1493 if CheckNu="ONLINE", "GIVING*CENTER", "ONLINEGIVING", "PAYPAL"
        ///                 - 9 if CheckNu is all numbers (or "CHECK")
        ///                 
        ///         - Add FinancialPaymentDetail to FinancialTransaction.FinancialPaymentDetail
        ///         - Add FinancialTransactionDetail to FinancialTransaction.TransactionDetails
        ///         - Save
        /// </summary>
        private void FindOrCreateTransaction( FinancialTransactionService transactionService, Queue<ShelbyContribution> shelbyContributionsSet, int count )
        {
            if ( shelbyContributionsSet.Count == 0 )
            {
                return;
            }

            var shelbyContribution = shelbyContributionsSet.Dequeue();
            try
            {
                string counter = shelbyContribution.Counter.ToStringSafe();
                FinancialTransaction financialTransaction = transactionService.Queryable().Where( p => p.ForeignKey == counter ).FirstOrDefault();

                if ( financialTransaction == null )
                {
                    financialTransaction = new FinancialTransaction();
                    //financialTransaction.TotalAmount = shelbyContribution.Amount;
                    financialTransaction.TransactionTypeValueId = _transactionTypeIdContribution;
                    financialTransaction.Summary = shelbyContribution.Memo;
                    financialTransaction.TransactionCode = shelbyContribution.CheckNu;
                    financialTransaction.ProcessedDateTime = Rock.RockDateTime.Now;
                    financialTransaction.TransactionDateTime = shelbyContribution.CNDate;
                    financialTransaction.CreatedDateTime = shelbyContribution.WhenSetup;
                    financialTransaction.ModifiedDateTime = shelbyContribution.WhenUpdated;
                    financialTransaction.ForeignKey = shelbyContribution.Counter.ToStringSafe();
                    financialTransaction.AuthorizedPersonAliasId = _shelbyPersonMappingDictionary[shelbyContribution.NameCounter];
                    financialTransaction.BatchId = _shelbyBatchMappingDictionary[shelbyContribution.BatchNu];

                    if ( _shelbyWhoMappingDictionary.ContainsKey( shelbyContribution.WhoSetup ) )
                    {
                        financialTransaction.CreatedByPersonAliasId = _shelbyWhoMappingDictionary[shelbyContribution.WhoSetup];
                    }

                    if ( _shelbyWhoMappingDictionary.ContainsKey( shelbyContribution.WhoUpdated ) )
                    {
                        financialTransaction.ModifiedByPersonAliasId = _shelbyWhoMappingDictionary[shelbyContribution.WhoUpdated];
                    }

                    // Hardcoded junk...
                    if ( shelbyContribution.CheckNu.Contains( "cash" ) )
                    {
                        financialTransaction.SourceTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_ONSITE_COLLECTION.AsGuid() ).Id;
                    }
                    else if ( shelbyContribution.CheckNu.Contains( "kiosk" ) )
                    {
                        financialTransaction.SourceTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_KIOSK.AsGuid() ).Id;
                    }
                    else if ( shelbyContribution.CheckNu.StartsWith( "on" ) )
                    {
                        financialTransaction.SourceTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE.AsGuid() ).Id;
                    }
                    else
                    {
                        financialTransaction.SourceTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_ONSITE_COLLECTION.AsGuid() ).Id;
                    }

                    // set up the necessary Financial Payment Detail record
                    if ( financialTransaction.FinancialPaymentDetail == null )
                    {
                        financialTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
                        financialTransaction.FinancialPaymentDetail.ForeignKey = shelbyContribution.DetailCounter.ToStringSafe();
                        financialTransaction.FinancialPaymentDetail.CreatedDateTime = shelbyContribution.WhenSetup;
                        financialTransaction.FinancialPaymentDetail.ModifiedDateTime = shelbyContribution.WhenUpdated;

                        // Now find the matching tender type...
                        // Get the tender type and put in cache if we've not encountered it before.
                        if ( shelbyContribution.CheckNu.Contains( "cash" ) )
                        {
                            financialTransaction.FinancialPaymentDetail.CurrencyTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CASH.AsGuid() ).Id;
                        }
                        else if ( reOnlyDigits.IsMatch( shelbyContribution.CheckNu ) )
                        {
                            financialTransaction.FinancialPaymentDetail.CurrencyTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid() ).Id;
                        }
                        else
                        {
                            var nonCash = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_NONCASH.AsGuid() );
                            // For some reason our production system does not have the above system GUID.
                            if ( nonCash == null )
                            {
                                nonCash = DefinedValueCache.Read( "6E4DA648-EF54-4375-A9FF-B675E6239E78".AsGuid() );
                            }

                            if ( nonCash != null )
                            {
                                financialTransaction.FinancialPaymentDetail.CurrencyTypeValueId = nonCash.Id;
                            }
                        }
                    }

                    FinancialTransactionDetail transactionDetail = new FinancialTransactionDetail();
                    transactionDetail.AccountId = _fundAccountMappingDictionary[shelbyContribution.PurCounter.ToString()].AsInteger();
                    transactionDetail.ForeignKey = shelbyContribution.DetailCounter.ToStringSafe();
                    transactionDetail.Amount = shelbyContribution.PurAmount;
                    financialTransaction.TransactionDetails.Add( transactionDetail );
                }
                else
                {
                    // this should not happen because we're dealing with the whole set at once.
                    // but I don't think it's an error -- it just means the record already existed.  It
                    // could be that they are running it a second time.
                    // Drain the queue and move to the next set
                    shelbyContributionsSet.Clear();
                    return;
                }

                while ( shelbyContributionsSet.Count > 0 )
                {
                    shelbyContribution = shelbyContributionsSet.Dequeue();
                    FinancialTransactionDetail transactionDetail = new FinancialTransactionDetail();
                    transactionDetail.AccountId = _fundAccountMappingDictionary[shelbyContribution.PurCounter.ToString()].AsInteger();
                    transactionDetail.ForeignKey = shelbyContribution.DetailCounter.ToStringSafe();
                    transactionDetail.Amount = shelbyContribution.PurAmount;
                    financialTransaction.TransactionDetails.Add( transactionDetail );
                }

                // Now save/write the whole set...
                transactionService.Add( financialTransaction );
                RockContext rockContext = ( RockContext ) transactionService.Context;
                if ( count % 200 == 0 )
                {
                    rockContext.ChangeTracker.DetectChanges();
                    rockContext.SaveChanges( disablePrePostProcessing: true );
                }
            }
            catch (Exception ex )
            {
                shelbyContribution.ERROR = ex.Message + " : " + ex.StackTrace + " : " + ex.InnerException + " : " + string.Join( "^^^", ex.Messages().ToArray() );
                _errorElements.Add( shelbyContribution );
            }
        }

        /// <summary>
        /// Finds a matching person or creates a new person in the db. When new people are created:
        ///   - it will store the person's Shelby NameCounter to the Rock person's ForeignKey field
        ///   - it will use the selected campus as the person's/family campus.
        /// </summary>
        /// <param name="personService">The person service.</param>
        /// <param name="shelbyPerson">The shelby person.</param>
        /// <returns>The person's PersonAliasId</returns>
        private int? FindOrCreateNewPerson( PersonService personService, ShelbyPerson shelbyPerson, int count )
        {
            int? personAliasId = null;
            string firstName = ( shelbyPerson.Salutation != string.Empty ) ? shelbyPerson.Salutation : shelbyPerson.FirstMiddle;
            string namecounter = shelbyPerson.NameCounter.ToStringSafe();

            var exactPerson = personService.Queryable().Where( p => p.ForeignKey == namecounter ).AsNoTracking().FirstOrDefault();

            if ( exactPerson != null )
            {
                personAliasId = exactPerson.PrimaryAliasId;
            }

            if ( personAliasId == null && firstName != string.Empty && shelbyPerson.LastName != string.Empty )
            {
                var people = personService.GetByFirstLastName( firstName, shelbyPerson.LastName, true, true ).AsNoTracking();

                // find any matches?
                if ( people.Any() )
                {
                    // If there's only one match, use it...
                    if ( people.Count() == 1 )
                    {
                        personAliasId = people.FirstOrDefault().PrimaryAliasId;
                    }
                    // otherwise, do any have the same email?
                    else if ( shelbyPerson.EmailAddress != string.Empty )
                    {
                        var peopleWithEmail = people.Where( p => p.Email == ( string ) shelbyPerson.EmailAddress );
                        if ( peopleWithEmail != null && peopleWithEmail.Count() == 1 )
                        {
                            var match = peopleWithEmail.FirstOrDefault();
                            personAliasId = match.PrimaryAliasId;
                        }
                    }
                }
            }

            // If no match was found, try matching just by email address
            if ( personAliasId == null && shelbyPerson.EmailAddress != string.Empty )
            {
                var people = personService.GetByEmail( shelbyPerson.EmailAddress, true, true ).AsNoTracking();
                if ( people.Any() && people.Count() == 1 )
                {
                    personAliasId = people.FirstOrDefault().PrimaryAliasId;
                }
            }

            // If no match was still found, add a new person/family
            if ( personAliasId == null )
            {
                var person = new Person();
                person.IsSystem = false;
                person.IsEmailActive = true;

                person.RecordTypeValueId = _personRecordTypeId;
                person.RecordStatusValueId = _personStatusPending;

                person.Email = shelbyPerson.EmailAddress;
                person.EmailPreference = EmailPreference.EmailAllowed;

                person.FirstName = firstName;
                person.LastName = shelbyPerson.LastName;
                switch ( shelbyPerson.Gender )
                {
                    case "M":
                        person.Gender = Gender.Male;
                        break;
                    case "F":
                        person.Gender = Gender.Female;
                        break;
                    default:
                        person.Gender = Gender.Unknown;
                        break;
                }
                person.MaritalStatusValueId = FindMatchingMaritalStatus( shelbyPerson.MaritalStatus );
                person.ConnectionStatusValueId = connectionStatusDefinedValueId;
                person.ForeignKey = namecounter;
                if ( !string.IsNullOrWhiteSpace( shelbyPerson.HomePhone ) )
                {
                    var phoneNumber = new PhoneNumber
                    {
                        NumberTypeValueId = _homePhoneDefinedValueId,
                        Number = PhoneNumber.CleanNumber( shelbyPerson.HomePhone )
                    };

                    // Format number since default SaveChanges() is not being used.
                    phoneNumber.NumberFormatted = PhoneNumber.FormattedNumber( phoneNumber.CountryCode, phoneNumber.Number );

                    person.PhoneNumbers.Add( phoneNumber );
                }

                RockContext rockContext = (RockContext) personService.Context;
                Rock.Model.Group familyGroup = PersonService.SaveNewPerson( person, rockContext );

                // Here you MUST detect changes before you save otherwise things like the ID and PrimaryAliasId
                // won't be resolved.
                rockContext.ChangeTracker.DetectChanges();
                rockContext.SaveChanges( disablePrePostProcessing: true );
                personAliasId = person.PrimaryAliasId;

                if ( familyGroup != null )
                {
                    familyGroup.CampusId = cpCampus.SelectedCampusId;
                    GroupService.AddNewGroupAddress(
                        rockContext,
                        familyGroup,
                        GetAttributeValue( "AddressType" ),
                        shelbyPerson.Address1, shelbyPerson.Address2, shelbyPerson.City, shelbyPerson.State, shelbyPerson.PostalCode, "US",
                        true );
                }
            }

            return personAliasId;
        }

        private int FindMatchingMaritalStatus( string theValue )
        {
            var theDefinedValue = _maritalStatusDefinedType.DefinedValues.FirstOrDefault( a => a.Value.StartsWith( theValue, StringComparison.CurrentCultureIgnoreCase ) );
            // use the unknown value if we didn't find a match.
            if ( string.IsNullOrWhiteSpace( theValue ) || theDefinedValue == null )
            {
                theDefinedValue = _maritalStatusDefinedType.DefinedValues.FirstOrDefault( a => String.Equals( a.Value, "Unknown", StringComparison.CurrentCultureIgnoreCase ) );
            }

            return theDefinedValue.Id;
        }

        /// <summary>
        /// Connects to the remote Shelby db and generates a list of unique Funds (Id/Name) and binds
        /// it to the AccountMap repeater.  That is then used to map Shelby Fund Ids to Rock Accounts.
        /// </summary>
        private void VerifyOrSetAccountMappings()
        {
            var list = new ListItemCollection();
            try
            {
                var connectionString = GetConnectionString();
                using ( SqlConnection connection = new SqlConnection( connectionString ) )
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT [Counter], [Descr] FROM [ShelbyDB].[Shelby].[CNPur] P WHERE P.[Counter] IN (SELECT DISTINCT D.[PurCounter] FROM [ShelbyDB].[Shelby].[CNHstDet] D) ORDER BY [Counter]";

                    SqlDataReader reader = command.ExecuteReader();
                    if ( reader.HasRows )
                    {
                        while ( reader.Read() )
                        {
                            list.Add( new ListItem( reader["Descr"].ToStringSafe(), reader["Counter"].ToStringSafe() ) );
                        }
                        rptAccountMap.DataSource = list;
                        rptAccountMap.DataBind();
                    }
                    else
                    {
                    }
                    reader.Close();
                }
            }
            catch ( Exception ex )
            {
                nbMessage.Text = string.Format( "Your database block settings are not valid or the remote database server is offline or mis-configured. {0}", ex.StackTrace ) ;
            }
        }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <returns></returns>
        private string GetConnectionString()
        {
            var dataSource = Encryption.DecryptString( GetAttributeValue( "ShelbyDBDataSource" ) );
            var catalog = Encryption.DecryptString( GetAttributeValue( "ShelbyDBCatalog" ) );
            var userId = Encryption.DecryptString( GetAttributeValue( "ShelbyDBUserId" ) );
            var pass = Encryption.DecryptString( GetAttributeValue( "ShelbyDBPassword" ) );

            return string.Format( @"Data Source={0};Initial Catalog={1}; User Id={2}; password={3};", dataSource, catalog, userId, pass );
        }

        /// <summary>
        /// Binds the campus picker.
        /// </summary>
        private void BindCampusPicker()
        {
            // load campus dropdown
            var campuses = CampusCache.All();
            cpCampus.Campuses = campuses;
            cpCampus.Visible = campuses.Count > 1;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            try
            {
                RockContext rockContext = new RockContext();
                FinancialBatchService financialBatchService = new FinancialBatchService( rockContext );
                var qry = financialBatchService.Queryable()
                                        .Where( fb => fb.ForeignKey != null ).AsNoTracking();

                var batchRowQry = qry.Select( b => new BatchRow
                {
                    Id = b.Id,
                    BatchStartDateTime = b.BatchStartDateTime.Value,
                    Name = b.Name,
                    AccountingSystemCode = b.AccountingSystemCode,
                    TransactionCount = b.Transactions.Count(),
                    TransactionAmount = b.Transactions.Sum( t => ( decimal? ) ( t.TransactionDetails.Sum( d => ( decimal? ) d.Amount ) ?? 0.0M ) ) ?? 0.0M,
                    ControlAmount = b.ControlAmount,
                    CampusName = b.Campus != null ? b.Campus.Name : "",
                    Status = b.Status,
                    UnMatchedTxns = b.Transactions.Any( t => !t.AuthorizedPersonAliasId.HasValue ),
                    BatchNote = b.Note,
                    AccountSummaryList = b.Transactions
                                        .SelectMany( t => t.TransactionDetails )
                                        .GroupBy( d => d.AccountId )
                                        .Select( s => new BatchAccountSummary
                                        {
                                            AccountId = s.Key,
                                            AccountOrder = s.Max( d => d.Account.Order ),
                                            AccountName = s.Max( d => d.Account.Name ),
                                            Amount = s.Sum( d => ( decimal? ) d.Amount ) ?? 0.0M
                                        } )
                                        .OrderBy( s => s.AccountOrder )
                                        .ToList()
                } );

                gBatchList.SetLinqDataSource( batchRowQry.AsNoTracking() );
                gBatchList.EntityTypeId = EntityTypeCache.Read<Rock.Model.FinancialBatch>().Id;
                gBatchList.DataBind();

                gBatchList.Actions.ShowExcelExport = false;
                pnlGrid.Visible = gBatchList.Rows.Count > 0;

            }
            catch ( Exception ex )
            {
                nbWarningMessage.Text = ex.Message;
            }

        }

        /// <summary>
        /// Binds the error grid.
        /// </summary>
        private void BindErrorGrid()
        {
            //RockContext rockContext = new RockContext();
            //FinancialTransactionDetailService financialTransactionDetailService = new FinancialTransactionDetailService( rockContext );

            if ( _errorElements.Count > 0 )
            {
                gErrors.DataSource = _errorElements;
            }

            gErrors.DataBind();

            if ( gErrors.Rows.Count > 0 )
            {
                pnlErrors.Visible = true;
                gErrors.Visible = true;
            }
        }

        private void NotifyClientProcessingUsers( int count, int total )
        {
            NotifyClientProcessing( "People", "shelbyImport-processingUsers", string.Empty, count, total );
        }

        private void NotifyClientProcessingBatches( int count, int total )
        {
            NotifyClientProcessing( "Batches", "shelbyImport-processingBatches", "progress-bar-info", count, total );

        }

        private void NotifyClientProcessingTransactions( int count, int total )
        {
            NotifyClientProcessing( "Transactions", "shelbyImport-processingTransactions", "progress-bar-success", count, total );
        }

        private void NotifyClientProcessing( string itemTitle, string htmlId, string progressBarclass, int count, int total )
        {
            var ts = _stopwatch.Elapsed;
            double percent = ( double ) count / total * 100;
            var x = string.Format( @"Processing {2} {3}...
                <div class='progress'>
                  <div class='progress-bar {4}' role='progressbar' aria-valuenow='{0:0}' aria-valuemin='0' aria-valuemax='100' style='min-width: 2em; width: {0:0}%;'>{1}</div>
                </div>
                <div class='pull-right'>{5:00}:{6:00}</div>", percent, count, total, itemTitle, progressBarclass, ts.Minutes, ts.Seconds );
            _hubContext.Clients.All.receiveNotification( htmlId, x );
        }
        #endregion

        /// <summary>
        /// Handles the ItemDataBound event of the rptAccountMap control which creates a DropDownList 
        /// with the Rock Account as the selected value which has been matched to the corresponding
        /// Shelby Fund.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptAccountMap_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var item = ( ListItem ) e.Item.DataItem;

                Literal litFundName = ( Literal ) e.Item.FindControl( "litFundName" );
                HiddenField hfFundId = ( HiddenField ) e.Item.FindControl( "hfFundId" );

                litFundName.Text = item.Text;
                hfFundId.Value = item.Value;
                string accountId = string.Empty;

                RockDropDownList list = ( RockDropDownList ) e.Item.FindControl( "rdpAcccounts" );
                if ( list != null )
                {
                    if ( _fundAccountMappingDictionary.ContainsKey( hfFundId.Value ) )
                    {
                        accountId = _fundAccountMappingDictionary[hfFundId.Value];
                        // Make sure it's still in the list before we try to select it.
                        if ( AllAccounts.ContainsKey( accountId.AsInteger() ) )
                        {
                            list.SelectedValue = accountId;
                        }
                    }
                    else
                    {
                        list.SelectedIndex = -1;
                    }

                    list.DataSource = AllAccounts;
                    list.DataValueField = "Key";
                    list.DataTextField = "Value";
                    list.DataBind();
                }
            }
        }

        protected void rdpAcccounts_SelectedIndexChanged( object sender, EventArgs e )
        {
            // Get the selected Rock Account Id
            RockDropDownList rdpAcccounts = ( RockDropDownList ) sender;

            var clientId = rdpAcccounts.ClientID;
            int controlIndex = GetControlIndex( clientId );
            int mapIndex = controlIndex - 1;

            // Get the Shelby Fund name 
            Literal litFundName = ( Literal ) rptAccountMap.Items[mapIndex].FindControl( "litFundName" );
            
            // Get the Shelby Fund Id
            HiddenField hfFundId = ( HiddenField ) rptAccountMap.Items[mapIndex].FindControl( "hfFundId" );

            // Save the value in the Dictonary and save it to the block's attribute
            
            // Add/Update the new value in the dictionary
            _fundAccountMappingDictionary.AddOrReplace( hfFundId.Value, rdpAcccounts.SelectedValue );

            // Turn the Dictionary back to a string and store it in the block's attribute value
            var newValue = String.Join( ",", _fundAccountMappingDictionary.Select( kvp =>String.Format( "{0}={1}", kvp.Key, kvp.Value ) ) );
            SetAttributeValue( FUND_ACCOUNT_MAPPINGS, newValue );
            SaveAttributeValues();

            // Update the onscreen status to show the user the value has been saved.
            Literal litAccontSaveStatus = ( Literal ) rptAccountMap.Items[mapIndex].FindControl( "litAccontSaveStatus" );
            litAccontSaveStatus.Text = string.Format( "<span class='text-success'><i class='fa fa-check'></i> saved</span>", litFundName.Text );
        }

        private int GetControlIndex( String controlID )
        {
            Regex regex = new Regex( "([0-9]+)", RegexOptions.RightToLeft );
            Match match = regex.Match( controlID );

            return Convert.ToInt32( match.Value );
        }

        protected void tbBatchName_TextChanged( object sender, EventArgs e )
        {
            RockTextBox tbBatchName = ( RockTextBox ) sender;
            SetAttributeValue( "BatchName", tbBatchName.Text );
            SaveAttributeValues();
        }

        #region Helper Classes

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
            public decimal TransactionAmount { get; set; }
            public decimal ControlAmount { get; set; }
            public List<BatchAccountSummary> AccountSummaryList { get; set; }
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
        }

        class ShelbyPerson
        {
            public int NameCounter;
            public string FirstMiddle;
            public string Salutation;
            public string LastName;
            public string Gender;
            public string MaritalStatus;
            public string Address1;
            public string Address2;
            public string City;
            public string State;
            public string PostalCode;
            public string EmailAddress;
            public string HomePhone;

            public ShelbyPerson( SqlDataReader reader )
            {
                NameCounter = ( int ) reader["NameCounter"];

                EmailAddress = reader["EMailAddress"].ToStringSafe();
                if ( ! EmailAddress.IsValidEmail() )
                {
                    EmailAddress = string.Empty;
                }

                Gender = reader["Gender"].ToStringSafe();
                Salutation = reader["Salutation"].ToStringSafe();
                FirstMiddle = reader["FirstMiddle"].ToStringSafe();
                LastName = reader["LastName"].ToStringSafe();
                MaritalStatus = reader["MaritalStatus"].ToStringSafe();
                Address1 =   reader["Adr1"].ToStringSafe();
                Address2 =  reader["Adr2"].ToStringSafe();
                City = reader["City"].ToStringSafe();
                State = reader["State"].ToStringSafe();
                PostalCode = reader["PostalCode"].ToStringSafe();
                HomePhone = reader["PhoneNu"].ToStringSafe();
            }
        }


        /// <summary>
        /// Class that represents a Shelby Batch ([BatchNu], [NuContr], [Total], [WhenPosted], [WhenSetup], [WhoSetup] )
        /// </summary>
        class ShelbyBatch
        {
            public int BatchNu;
            public int NuContr;
            public Decimal Total;
            public DateTime WhenPosted;
            public DateTime WhenSetup;
            public string WhoSetup;
            public int RockBatchId;

            public ShelbyBatch( SqlDataReader reader )
            {
                BatchNu = ( int ) reader["BatchNu"];
                NuContr = ( Int16 ) reader["NuContr"];
                Total = ( Decimal ) reader["Total"];
                WhenSetup = ( DateTime ) reader["WhenSetup"];
                WhenPosted = ( DateTime ) reader["WhenPosted"];
                WhoSetup = reader["WhoSetup"].ToStringSafe();
            }

        }

        /// <summary>
        /// Represents a Shelby Contribution History record
        /// 	H.[Counter], D.[Counter] as 'DetailCounter', H.[Amount], H.[BatchNu], H.[CheckNu], H.[CNDate], H.[Memo], H.[NameCounter], 
        /// 	H.[WhenSetup], H.[WhenUpdated], H.[WhoSetup], H.[WhoUpdated], H.[CheckType], D.[PurCounter],
        /// 	P.[Descr], D.[Amount] as 'PurAmount'
        /// </summary>
        public class ShelbyContribution
        {
            public int Counter { get; set; }
            public int DetailCounter { get; set; }
            public Decimal Amount { get; set; }
            public int BatchNu { get; set; }
            public string CheckNu { get; set; }
            public DateTime CNDate { get; set; }
            public string Memo { get; set; }
            public int NameCounter { get; set; }
            public DateTime WhenSetup { get; set; }
            public DateTime WhenUpdated { get; set; }
            public string WhoSetup { get; set; }
            public string WhoUpdated { get; set; }
            public string CheckType { get; set; }
            public int PurCounter { get; set; }
            public string Descr { get; set; }
            public Decimal PurAmount { get; set; }
            public string ERROR { get; set; }

            public ShelbyContribution()
            { }

            public ShelbyContribution( SqlDataReader reader )
            {
                Counter = ( int ) reader["Counter"];
                DetailCounter = ( int ) reader["DetailCounter"];
                Amount = ( Decimal ) reader["Amount"];
                BatchNu = ( int ) reader["BatchNu"];
                CheckNu = (( string ) reader["CheckNu"]).ToLower();
                CNDate = ( DateTime ) reader["CNDate"];
                Memo = ( string ) reader["Memo"];
                NameCounter = ( int ) reader["NameCounter"];
                WhenSetup = ( DateTime ) reader["WhenSetup"];
                WhenUpdated = ( DateTime ) reader["WhenUpdated"];
                WhoSetup = reader["WhoSetup"].ToStringSafe();
                WhoUpdated = reader["WhoUpdated"].ToStringSafe();
                CheckType = reader["CheckType"].ToStringSafe();
                PurCounter = ( int ) reader["PurCounter"];
                WhoUpdated = reader["WhoUpdated"].ToStringSafe();
                PurAmount = ( Decimal ) reader["PurAmount"];
            }
        }
        #endregion

        protected void lbClearSession_Click( object sender, EventArgs e )
        {
            _shelbyContributionsCompleted.Clear();
            _shelbyBatchMappingDictionary.Clear();
            _fundAccountMappingDictionary.Clear();
            _shelbyPersonMappingDictionary.Clear();
            _shelbyWhoMappingDictionary.Clear();

            _shelbyContributionsCompleted = null;
            _shelbyBatchMappingDictionary = null;
            _fundAccountMappingDictionary = null;
            _shelbyPersonMappingDictionary = null;
            _shelbyWhoMappingDictionary = null;

            Session["ShelbyImport:shelbyPersonMappingDictionary"] = null;
            Session["ShelbyImport:shelbyContributionsCompleted"] = null;
        }
    }
}