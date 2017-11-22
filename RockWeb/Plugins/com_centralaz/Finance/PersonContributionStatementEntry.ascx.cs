// <copyright>
// Copyright by the Spark Development Network
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
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Web.UI;
using Newtonsoft.Json.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.MergeTemplates;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.Finance
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Person Contribution Statement Template Entry" )]
    [Category( "com_centralaz > Finance" )]
    [Description( "Used for merging contribution data into output documents, such as Word, Html, using a pre-defined template." )]
    [IntegerField( "Database Timeout", "The number of seconds to wait before reporting a database timeout.", false, 180, "", 0 )]
    [BinaryFileTypeField( "File Type", "The file type used to save the contribution statements.", true, "FC7218EE-EA28-4EA4-8C3D-F30750A2FE59" )]
    public partial class PersonContributionStatementEntry : RockBlock
    {
        #region Properties            
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the account1.
        /// </summary>
        /// <value>
        /// The account1.
        /// </value>
        public FinancialAccount Account1 { get; set; }

        /// <summary>
        /// Gets or sets the account2.
        /// </summary>
        /// <value>
        /// The account2.
        /// </value>
        public FinancialAccount Account2 { get; set; }

        /// <summary>
        /// Gets or sets the account3.
        /// </summary>
        /// <value>
        /// The account3.
        /// </value>
        public FinancialAccount Account3 { get; set; }

        /// <summary>
        /// Gets or sets the account4.
        /// </summary>
        /// <value>
        /// The account4.
        /// </value>
        public FinancialAccount Account4 { get; set; }

        /// <summary>
        /// Gets or sets the database timeout.
        /// </summary>
        /// <value>
        /// The database timeout.
        /// </value>
        public int? DatabaseTimeout { get; set; }

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        public Person Person { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            //// set postback timeout to whatever the DatabaseTimeout is plus an extra 5 seconds so that page doesn't timeout before the database does
            //// note: this only makes a difference on Postback, not on the initial page visit
            int databaseTimeout = GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull() ?? 180;
            var sm = ScriptManager.GetCurrent( this.Page );
            if ( sm.AsyncPostBackTimeout < databaseTimeout + 5 )
            {
                sm.AsyncPostBackTimeout = databaseTimeout + 5;
            }
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
                ShowDetail();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            //
        }

        /// <summary>
        /// Handles the Click event of the btnMerge control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMerge_Click( object sender, EventArgs e )
        {
            nbNotification.Visible = false;

            var rockContext = new RockContext();

            MergeTemplate mergeTemplate = new MergeTemplateService( rockContext ).Get( mtpMergeTemplate.SelectedValue.AsInteger() );
            if ( mergeTemplate == null )
            {
                nbWarningMessage.Text = "Unable to get merge template";
                nbWarningMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbWarningMessage.Visible = true;
                return;
            }

            MergeTemplateType mergeTemplateType = this.GetMergeTemplateType( rockContext, mergeTemplate );
            if ( mergeTemplateType == null )
            {
                nbWarningMessage.Text = "Unable to get merge template type";
                nbWarningMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbWarningMessage.Visible = true;
                return;
            }

            BinaryFileType binaryFileType = new BinaryFileTypeService( rockContext ).Get( GetAttributeValue( "FileType" ).AsGuid() );
            if ( binaryFileType == null )
            {
                nbWarningMessage.Text = "Unable to get file type";
                nbWarningMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbWarningMessage.Visible = true;
                return;
            }

            if ( !ppPerson.PersonAliasId.HasValue )
            {
                nbWarningMessage.Text = "No person selected";
                nbWarningMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbWarningMessage.Visible = true;
                return;
            }

            Person = new PersonAliasService( rockContext ).GetPerson( ppPerson.PersonAliasId.Value );
            if ( Person == null )
            {
                nbWarningMessage.Text = "Could not find person.";
                nbWarningMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbWarningMessage.Visible = true;
                return;
            }

            // Get the accounts that we want to list independently
            var accountService = new FinancialAccountService( rockContext );

            FinancialAccount account1 = new FinancialAccount();
            if ( apAccount1.SelectedValueAsId().HasValue )
            {
                Account1 = accountService.Get( apAccount1.SelectedValueAsInt().Value );
            }

            FinancialAccount account2 = new FinancialAccount();
            if ( apAccount2.SelectedValueAsId().HasValue )
            {
                Account2 = accountService.Get( apAccount2.SelectedValueAsInt().Value );
            }

            FinancialAccount account3 = new FinancialAccount();
            if ( apAccount3.SelectedValueAsId().HasValue )
            {
                Account3 = accountService.Get( apAccount3.SelectedValueAsInt().Value );
            }

            FinancialAccount account4 = new FinancialAccount();
            if ( apAccount4.SelectedValueAsId().HasValue )
            {
                Account4 = accountService.Get( apAccount4.SelectedValueAsInt().Value );
            }

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpDates.DelimitedValues );

            DatabaseTimeout = GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull();
            StartDate = dateRange.Start;
            EndDate = dateRange.End;

            SetBlockUserPreference( "MergeTemplate", mtpMergeTemplate.SelectedValue );
            SetBlockUserPreference( "Account1", apAccount1.SelectedValue );
            SetBlockUserPreference( "Account2", apAccount2.SelectedValue );
            SetBlockUserPreference( "Account3", apAccount3.SelectedValue );
            SetBlockUserPreference( "Account4", apAccount4.SelectedValue );
            SetBlockUserPreference( "Date Range", drpDates.DelimitedValues );

            try
            {
                var fileName = String.Format( "{0}_{1}_{2}_ContributionStatement.html", DateTime.Now.ToString( "MMddyyyy" ), Person.LastName, Person.NickName );
                var mergeFields = GetMergeFields( rockContext );

                BinaryFile outputBinaryFileDoc = null;

                outputBinaryFileDoc = mergeTemplateType.CreateDocument( mergeTemplate, new List<object>(), mergeFields );

                if ( mergeTemplateType.Exceptions != null && mergeTemplateType.Exceptions.Any() )
                {
                    if ( mergeTemplateType.Exceptions.Count == 1 )
                    {
                        this.LogException( mergeTemplateType.Exceptions[0] );
                    }
                    else if ( mergeTemplateType.Exceptions.Count > 50 )
                    {
                        this.LogException( new AggregateException( string.Format( "Exceptions merging template {0}. See InnerExceptions for top 50.", mergeTemplate.Name ), mergeTemplateType.Exceptions.Take( 50 ).ToList() ) );
                    }
                    else
                    {
                        this.LogException( new AggregateException( string.Format( "Exceptions merging template {0}. See InnerExceptions", mergeTemplate.Name ), mergeTemplateType.Exceptions.ToList() ) );
                    }
                }

                var uri = new UriBuilder( outputBinaryFileDoc.Url );
                var qry = System.Web.HttpUtility.ParseQueryString( uri.Query );
                qry["attachment"] = true.ToTrueFalse();
                uri.Query = qry.ToString();
                Response.Redirect( uri.ToString(), false );
                Context.ApplicationInstance.CompleteRequest();
            }
            catch ( Exception ex )
            {
                this.LogException( ex );
                if ( ex is System.FormatException )
                {
                    nbWarningMessage.Text = "Error loading the merge template. Please verify that the merge template file is valid.";
                }
                else
                {
                    nbWarningMessage.Text = "An error occurred while merging";
                }

                nbWarningMessage.Details = ex.Message;
                nbWarningMessage.Visible = true;
            }

            nbNotification.Visible = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            nbNotification.Visible = false;
            var delimitedDateValues = GetBlockUserPreference( "Date Range" );

            if ( !String.IsNullOrWhiteSpace( delimitedDateValues ) )
            {
                drpDates.DelimitedValues = delimitedDateValues;
            }
            else
            {
                int year = DateTime.Now.Year;
                drpDates.DateRangeModeStart = new DateTime( year, 1, 1 );
                drpDates.DateRangeModeEnd = new DateTime( year, 12, 31 );
            }

            if ( !String.IsNullOrWhiteSpace( GetBlockUserPreference( "MergeTemplate" ) ) )
            {
                mtpMergeTemplate.SetValue( GetBlockUserPreference( "MergeTemplate" ).AsIntegerOrNull() );
            }

            if ( !String.IsNullOrWhiteSpace( GetBlockUserPreference( "Account1" ) ) )
            {
                apAccount1.SetValue( GetBlockUserPreference( "Account1" ).AsIntegerOrNull() );
            }

            if ( !String.IsNullOrWhiteSpace( GetBlockUserPreference( "Account2" ) ) )
            {
                apAccount2.SetValue( GetBlockUserPreference( "Account2" ).AsIntegerOrNull() );
            }

            if ( !String.IsNullOrWhiteSpace( GetBlockUserPreference( "Account3" ) ) )
            {
                apAccount3.SetValue( GetBlockUserPreference( "Account3" ).AsIntegerOrNull() );
            }

            if ( !String.IsNullOrWhiteSpace( GetBlockUserPreference( "Account4" ) ) )
            {
                apAccount4.SetValue( GetBlockUserPreference( "Account4" ).AsIntegerOrNull() );
            }
        }

        private Dictionary<string, object> GetMergeFields( RockContext rockContext, int? fetchCount = null )
        {
            var databaseTimeout = GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull();
            if ( databaseTimeout.HasValue )
            {
                rockContext.Database.CommandTimeout = databaseTimeout.Value;
            }

            // Get all transactions tied to the Giving Ids
            List<TransactionSummary> transactionList = new List<TransactionSummary>();
            List<AddressSummary> addressList = new List<AddressSummary>();
            Dictionary<string, object> parameters = GetSqlParameters();
            DataSet result = null;
            if ( Person.RecordTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() )
            {
                result = DbService.GetDataSet( "spFinance_GetGivingGroupTransactionsForAPerson", System.Data.CommandType.StoredProcedure, parameters );
            }
            else if ( Person.RecordTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() )
            {
                result = DbService.GetDataSet( "spFinance_GetGivingGroupTransactionsForABusiness", System.Data.CommandType.StoredProcedure, parameters );
            }

            if ( result != null )
            {
                if ( result.Tables.Count > 1 )
                {
                    var transactionDataTable = result.Tables[0];
                    foreach ( var row in transactionDataTable.Rows.OfType<DataRow>().ToList() )
                    {
                        transactionList.Add( new TransactionSummary
                        {
                            GivingId = row.ItemArray[0] as string,
                            TransactionCode = row.ItemArray[1] as string,
                            TransactionDateTime = row.ItemArray[2] as DateTime?,
                            Account1Amount = row.ItemArray[3] as decimal?,
                            Account2Amount = row.ItemArray[4] as decimal?,
                            Account3Amount = row.ItemArray[5] as decimal?,
                            Account4Amount = row.ItemArray[6] as decimal?,
                            OtherAmount = row.ItemArray[7] as decimal?,
                            TotalTransactionAmount = row.ItemArray[8] as decimal?
                        } );
                    }

                    var addressDataTable = result.Tables[01];
                    foreach ( var row in addressDataTable.Rows.OfType<DataRow>().ToList() )
                    {
                        addressList.Add( new AddressSummary
                        {
                            GivingId = row.ItemArray[0] as string,
                            AddressNames = row.ItemArray[1] as string,
                            Street1 = row.ItemArray[3] as string,
                            Street2 = row.ItemArray[4] as string,
                            City = row.ItemArray[5] as string,
                            State = row.ItemArray[6] as string,
                            PostalCode = row.ItemArray[7] as string
                        } );
                    }
                }
            }

            var givingTransactionList = transactionList.GroupBy( t => t.GivingId ).Select( g => new GroupedTransaction
            {
                Key = g.Key,
                Account1Total = g.Sum( t => t.Account1Amount ),
                Account2Total = g.Sum( t => t.Account2Amount ),
                Account3Total = g.Sum( t => t.Account3Amount ),
                Account4Total = g.Sum( t => t.Account4Amount ),
                OtherTotal = g.Sum( t => t.OtherAmount ),
                TransactionTotal = g.Sum( t => t.TotalTransactionAmount ),
                Transactions = g.ToList()
            } ).ToList();

            // Dump everything into a lava object
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null );
            mergeFields.Add( "Transactions", givingTransactionList );
            mergeFields.Add( "Addresses", addressList );
            mergeFields.Add( "Account1", Account1 );
            mergeFields.Add( "Account2", Account2 );
            mergeFields.Add( "Account3", Account3 );
            mergeFields.Add( "Account4", Account4 );
            mergeFields.Add( "StartDate", StartDate.ToString() );
            mergeFields.Add( "EndDate", EndDate.ToString() );
            return mergeFields;
        }

        private Dictionary<string, object> GetSqlParameters()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if ( StartDate.HasValue )
            {
                parameters.Add( "startDate", StartDate.Value );
            }
            else
            {
                parameters.Add( "startDate", DateTime.MinValue );
            }
            if ( EndDate.HasValue )
            {
                parameters.Add( "endDate", EndDate.Value );
            }
            else
            {
                parameters.Add( "endDate", DateTime.MaxValue );
            }

            parameters.Add( "account1Id", Account1.Id );
            parameters.Add( "account2Id", Account2.Id );
            parameters.Add( "account3Id", Account3.Id );
            parameters.Add( "account4Id", Account4.Id );

            parameters.Add( "givingId", Person.GivingId );

            return parameters;
        }

        /// <summary>
        /// Gets the type of the merge template.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="mergeTemplate">The merge template.</param>
        /// <returns></returns>
        private MergeTemplateType GetMergeTemplateType( RockContext rockContext, MergeTemplate mergeTemplate )
        {
            mergeTemplate = new MergeTemplateService( rockContext ).Get( mtpMergeTemplate.SelectedValue.AsInteger() );
            if ( mergeTemplate == null )
            {
                return null;
            }

            return mergeTemplate.GetMergeTemplateType();
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Grouped Transactions
        /// </summary>
        /// <seealso cref="Rock.Data.Entity{com.centralaz.Finance.Transactions.GenerateContributionStatementTransaction.GroupedTransaction}" />
        public class GroupedTransaction : Entity<GroupedTransaction>
        {
            /// <summary>
            /// Gets or sets the key.
            /// </summary>
            /// <value>
            /// The key.
            /// </value>
            [LavaInclude]
            public String Key { get; set; }

            /// <summary>
            /// Gets or sets the account1 total.
            /// </summary>
            /// <value>
            /// The account1 total.
            /// </value>
            [LavaInclude]
            public decimal? Account1Total { get; set; }

            /// <summary>
            /// Gets or sets the account2 total.
            /// </summary>
            /// <value>
            /// The account2 total.
            /// </value>
            [LavaInclude]
            public decimal? Account2Total { get; set; }

            /// <summary>
            /// Gets or sets the account3 total.
            /// </summary>
            /// <value>
            /// The account3 total.
            /// </value>
            [LavaInclude]
            public decimal? Account3Total { get; set; }

            /// <summary>
            /// Gets or sets the account4 total.
            /// </summary>
            /// <value>
            /// The account4 total.
            /// </value>
            [LavaInclude]
            public decimal? Account4Total { get; set; }

            /// <summary>
            /// Gets or sets the other total.
            /// </summary>
            /// <value>
            /// The other total.
            /// </value>
            [LavaInclude]
            public decimal? OtherTotal { get; set; }

            /// <summary>
            /// Gets or sets the transaction total.
            /// </summary>
            /// <value>
            /// The transaction total.
            /// </value>
            [LavaInclude]
            public decimal? TransactionTotal { get; set; }

            /// <summary>
            /// Gets or sets the transactions.
            /// </summary>
            /// <value>
            /// The transactions.
            /// </value>
            [LavaInclude]
            public List<TransactionSummary> Transactions { get; set; }

        }

        /// <summary>
        /// The transaction Summary
        /// </summary>
        [DotLiquid.LiquidType( "GivingId", "TransactionCode", "TransactionDateTime", "Account1Amount", "Account2Amount", "Account3Amount", "Account4Amount", "OtherAmount", "TotalTransactionAmount" )]
        public class TransactionSummary
        {
            /// <summary>
            /// Gets or sets the giving identifier.
            /// </summary>
            /// <value>
            /// The giving identifier.
            /// </value>
            public String GivingId { get; set; }

            /// <summary>
            /// Gets or sets the transaction code.
            /// </summary>
            /// <value>
            /// The transaction code.
            /// </value>
            public String TransactionCode { get; set; }

            /// <summary>
            /// Gets or sets the transaction date time.
            /// </summary>
            /// <value>
            /// The transaction date time.
            /// </value>
            public DateTime? TransactionDateTime { get; set; }

            /// <summary>
            /// Gets or sets the account1 amount.
            /// </summary>
            /// <value>
            /// The account1 amount.
            /// </value>
            public decimal? Account1Amount { get; set; }

            /// <summary>
            /// Gets or sets the account2 amount.
            /// </summary>
            /// <value>
            /// The account2 amount.
            /// </value>
            public decimal? Account2Amount { get; set; }

            /// <summary>
            /// Gets or sets the account3 amount.
            /// </summary>
            /// <value>
            /// The account3 amount.
            /// </value>
            public decimal? Account3Amount { get; set; }

            /// <summary>
            /// Gets or sets the account4 amount.
            /// </summary>
            /// <value>
            /// The account4 amount.
            /// </value>
            public decimal? Account4Amount { get; set; }

            /// <summary>
            /// Gets or sets the other amount.
            /// </summary>
            /// <value>
            /// The other amount.
            /// </value>
            public decimal? OtherAmount { get; set; }

            /// <summary>
            /// Gets or sets the total transaction amount.
            /// </summary>
            /// <value>
            /// The total transaction amount.
            /// </value>
            public decimal? TotalTransactionAmount { get; set; }
        }

        /// <summary>
        /// The address summary for lava
        /// </summary>
        [DotLiquid.LiquidType( "GivingId", "AddressNames", "Street1", "Street2", "City", "State", "PostalCode" )]
        public class AddressSummary
        {
            /// <summary>
            /// Gets or sets the giving identifier.
            /// </summary>
            /// <value>
            /// The giving identifier.
            /// </value>
            public String GivingId { get; set; }

            /// <summary>
            /// Gets or sets the names.
            /// </summary>
            /// <value>
            /// The names.
            /// </value>
            public String AddressNames { get; set; }

            /// <summary>
            /// Gets or sets the street1.
            /// </summary>
            /// <value>
            /// The street1.
            /// </value>
            public String Street1 { get; set; }

            /// <summary>
            /// Gets or sets the street2.
            /// </summary>
            /// <value>
            /// The street2.
            /// </value>
            public String Street2 { get; set; }

            /// <summary>
            /// Gets or sets the city.
            /// </summary>
            /// <value>
            /// The city.
            /// </value>
            public String City { get; set; }

            /// <summary>
            /// Gets or sets the state.
            /// </summary>
            /// <value>
            /// The state.
            /// </value>
            public String State { get; set; }

            /// <summary>
            /// Gets or sets the postal code.
            /// </summary>
            /// <value>
            /// The postal code.
            /// </value>
            public String PostalCode { get; set; }
        }
        #endregion
    }
}