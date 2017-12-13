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
using com.centralaz.Finance.Utility;
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

            // Get all transactions tied to the Giving Id
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if ( Person.RecordTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() )
            {
                parameters = FinanceHelper.GetSqlParameters(
                startDate: StartDate,
                endDate: EndDate,
                account1Id: Account1.Id,
                account2Id: Account2.Id,
                account3Id: Account3.Id,
                account4Id: Account4.Id,
                givingId: Person.GivingId
                );
            }
            else if ( Person.RecordTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() )
            {
                parameters = FinanceHelper.GetSqlParameters(
                startDate: StartDate,
                endDate: EndDate,
                account1Id: Account1.Id,
                account2Id: Account2.Id,
                account3Id: Account3.Id,
                account4Id: Account4.Id,
                givingId: Person.GivingId,
                isBusiness: true
                );
            }

            var mergeFields = FinanceHelper.GetFinancialStatementTransactionsAndAddresses( parameters );
            mergeFields.Add( "Account1", Account1 );
            mergeFields.Add( "Account2", Account2 );
            mergeFields.Add( "Account3", Account3 );
            mergeFields.Add( "Account4", Account4 );
            mergeFields.Add( "StartDate", StartDate.ToString() );
            mergeFields.Add( "EndDate", EndDate.ToString() );
            return mergeFields;
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
    }
}