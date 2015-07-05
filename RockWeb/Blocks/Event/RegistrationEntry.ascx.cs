// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Humanizer;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Block used to register for registration instance.
    /// </summary>
    [DisplayName( "Registration Entry" )]
    [Category( "Event" )]
    [Description( "Block used to register for registration instance." )]

    public partial class RegistrationEntry : RockBlock
    {
        #region Fields

        // Page (query string) parameter names
        private const string REGISTRATION_ID_PARAM_NAME = "RegistrationId";
        private const string REGISTRANT_INSTANCE_ID_PARAM_NAME = "RegistrationInstanceId";

        // Viewstate keys
        private const string REGISTRATION_INSTANCE_STATE_KEY = "RegistrationInstanceState";
        private const string REGISTRATION_STATE_KEY = "RegistrationState";
        private const string CURRENT_PANEL_KEY = "CurrentPanel";
        private const string CURRENT_REGISTRANT_INDEX_KEY = "CurrentRegistrantIndex";
        private const string CURRENT_FORM_INDEX_KEY = "CurrentFormIndex";

        #endregion

        #region Properties

        // The selected registration instance 
        private RegistrationInstance RegistrationInstanceState { get; set; }

        // Info about each current registration
        private RegistrationInfo RegistrationState { get; set; }

        // The current panel to display ( HowMany
        private int CurrentPanel { get; set; }

        // The current registrant index
        private int CurrentRegistrantIndex { get; set; }

        // The current form index
        private int CurrentFormIndex { get; set; }

        /// <summary>
        /// Gets the registration template.
        /// </summary>
        private RegistrationTemplate RegistrationTemplate
        {
            get
            {
                return RegistrationInstanceState != null ? RegistrationInstanceState.RegistrationTemplate : null;
            }
        }

        /// <summary>
        /// Gets the number of forms for the current registration template.
        /// </summary>
        private int FormCount
        {
            get
            {
                if ( RegistrationTemplate != null && RegistrationTemplate.Forms != null )
                {
                    return RegistrationTemplate.Forms.Count;
                }

                return 0;
            }
        }        
        
        /// <summary>
        /// If the registration template allows multiple registrants per registration, returns the maximum allowed
        /// </summary>
        private int MaxRegistrants
        {
            get
            {
                if ( RegistrationTemplate != null && RegistrationTemplate.AllowMultipleRegistrants )
                {
                    if ( RegistrationTemplate.MaxRegistrants <= 0 )
                    {
                        return int.MaxValue;
                    }
                    return RegistrationTemplate.MaxRegistrants;
                }

                return 1;
            }
        }

        /// <summary>
        /// Gets the minimum number of registrants allowed. Most of the time this is one, except for an existing
        /// registration that has existing registrants. The minimum in this case is the number of existing registrants
        /// </summary>
        private int MinRegistrants
        {
            get
            {
                return RegistrationState != null ? RegistrationState.ExistingRegistrantsCount : 1;
            }
        }

        /// <summary>
        /// Gets the number of registrants for the current registration
        /// </summary>
        private int RegistrantCount
        {
            get
            {
                return RegistrationState != null ? RegistrationState.RegistrantCount : 0;
            }
        }

        /// <summary>
        /// Gets or sets the payment transaction code.
        /// </summary>
        protected string TransactionCode
        {
            get { return ViewState["TransactionCode"] as string ?? string.Empty; }
            set { ViewState["TransactionCode"] = value; }
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState[REGISTRATION_INSTANCE_STATE_KEY] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                SetRegistrationState();
            }
            else
            {
                RegistrationInstanceState = JsonConvert.DeserializeObject<RegistrationInstance>( json );
            }

            json = ViewState[REGISTRATION_STATE_KEY] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                RegistrationState = new RegistrationInfo();
            }
            else
            {
                RegistrationState = JsonConvert.DeserializeObject<RegistrationInfo>( json );
            }

            CurrentPanel = ViewState[CURRENT_PANEL_KEY] as int? ?? 0;
            CurrentRegistrantIndex = ViewState[CURRENT_REGISTRANT_INDEX_KEY] as int? ?? 0;
            CurrentFormIndex = ViewState[CURRENT_FORM_INDEX_KEY] as int? ?? 0;

            CreateDynamicControls( false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            RegisterClientScript();
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
                SetRegistrationState();

                if ( RegistrationTemplate != null )
                {
                    ShowHowMany();
                }
            }
            else
            {
                ParseDynamicControls();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState[REGISTRATION_INSTANCE_STATE_KEY] = JsonConvert.SerializeObject( RegistrationInstanceState, Formatting.None, jsonSetting );
            ViewState[REGISTRATION_STATE_KEY] = JsonConvert.SerializeObject( RegistrationState, Formatting.None, jsonSetting );

            ViewState[CURRENT_PANEL_KEY] = CurrentPanel;
            ViewState[CURRENT_REGISTRANT_INDEX_KEY] = CurrentRegistrantIndex;
            ViewState[CURRENT_FORM_INDEX_KEY] = CurrentFormIndex;

            return base.SaveViewState();
        }

        #endregion

        #region Events

        #region Navigation Events

        /// <summary>
        /// Handles the Click event of the lbHowManyNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbHowManyNext_Click( object sender, EventArgs e )
        {
            CurrentRegistrantIndex = 0;
            CurrentFormIndex = 0;

            SetRegistrantState( numHowMany.Value );

            ShowRegistrant();

            hfTriggerScroll.Value = "true";
        }

        /// <summary>
        /// Handles the Click event of the lbRegistrantPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRegistrantPrev_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == 1 )
            {
                CurrentFormIndex--;
                if ( CurrentFormIndex < 0 )
                {
                    CurrentRegistrantIndex--;
                    CurrentFormIndex = FormCount - 1;
                }
                ShowRegistrant();
            }
            else
            {
                ShowHowMany();
            }

            hfTriggerScroll.Value = "true";
        }

        /// <summary>
        /// Handles the Click event of the lbRegistrantNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRegistrantNext_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == 1 )
            {
                CurrentFormIndex++;
                if ( CurrentFormIndex >= FormCount )
                {
                    CurrentRegistrantIndex++;
                    CurrentFormIndex = 0;
                }
                ShowRegistrant();
            }
            else
            {
                ShowHowMany();
            }

            hfTriggerScroll.Value = "true";
        }

        /// <summary>
        /// Handles the Click event of the lbSummaryPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSummaryPrev_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == 2 )
            {
                CurrentRegistrantIndex = RegistrantCount - 1;
                CurrentFormIndex = FormCount - 1;
                ShowRegistrant();
            }
            else
            {
                ShowHowMany();
            }

            hfTriggerScroll.Value = "true";
        }

        /// <summary>
        /// Handles the Click event of the lbSummaryNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSummaryNext_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == 2 )
            {
                ShowSuccess();
            }
            else
            {
                ShowHowMany();
            }

            hfTriggerScroll.Value = "true";
        }

        #endregion

        #region Summary Panel Events 

        /// <summary>
        /// Handles the Click event of the lbDiscountApply control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDiscountApply_Click( object sender, EventArgs e )
        {
            if ( RegistrationState != null )
            {
                RegistrationState.DiscountCode = tbDiscountCode.Text;
                CreateDynamicControls( true );
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gFeeSummary control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gFeeSummary_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var costSummary = e.Row.DataItem as CostSummaryInfo;
                if ( costSummary != null )
                {
                    string typeCss = costSummary.Type.ConvertToString().ToLower();
                    e.Row.Cells[0].AddCssClass( typeCss + "-description" );
                    e.Row.Cells[1].AddCssClass( typeCss + "-amount" );
                    e.Row.Cells[2].AddCssClass( typeCss + "-discounted-amount" );
                }
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Model/State Methods

        /// <summary>
        /// Sets the registration state
        /// </summary>
        private void SetRegistrationState()
        {
            int? RegistrationInstanceId = PageParameter( REGISTRANT_INSTANCE_ID_PARAM_NAME ).AsIntegerOrNull();
            int? RegistrationId = PageParameter( REGISTRATION_ID_PARAM_NAME ).AsIntegerOrNull();

            // Not inside a "using" due to serialization needing context to still be active
            var rockContext = new RockContext();

            if ( RegistrationId.HasValue )
            {
                var registrationService = new RegistrationService( rockContext );
                var registration = registrationService
                    .Queryable( "Registrants.PersonAlias.Person,Registrants.GroupMember,RegistrationInstance.Account,RegistrationInstance.RegistrationTemplate.Fees,RegistrationInstance.RegistrationTemplate.Discounts,RegistrationInstance.RegistrationTemplate.Forms.Fields.Attribute,RegistrationInstance.RegistrationTemplate.FinancialGateway" )
                    .AsNoTracking()
                    .Where( r => r.Id == RegistrationId.Value )
                    .FirstOrDefault();
                if ( registration != null )
                {
                    RegistrationInstanceState = registration.RegistrationInstance;
                    RegistrationState = new RegistrationInfo(  registration );
                    RegistrationState.PreviousPaymentTotal = registrationService.GetTotalPayments( registration.Id );
                }
            }

            if ( RegistrationState == null && RegistrationInstanceId.HasValue )
            {
                RegistrationInstanceState = new RegistrationInstanceService( rockContext )
                    .Queryable( "Account,RegistrationTemplate.Fees,RegistrationTemplate.Discounts,RegistrationTemplate.Forms.Fields.Attribute,RegistrationTemplate.FinancialGateway" )
                    .AsNoTracking()
                    .Where( r => r.Id == RegistrationInstanceId.Value )
                    .FirstOrDefault();

                if ( RegistrationInstanceState != null )
                {
                    RegistrationState = new RegistrationInfo( CurrentPerson );
                }
            }

            if ( RegistrationState != null && !RegistrationState.Registrants.Any() )
            {
                SetRegistrantState( 1 );
            }
            
        }

        private Registration BuildRegistrationModel( RockContext rockContext )
        {
            Registration registration = null;

            if ( RegistrationState != null )
            {
                var registrationService = new RegistrationService( rockContext );
                if ( RegistrationState.RegistrationId.HasValue )
                {
                    registration = registrationService.Get( RegistrationState.RegistrationId.Value );
                }

                if ( registration == null )
                {
                    registration = new Registration();
                    registrationService.Add( registration );
                }

                registration.PersonAliasId = RegistrationState.PersonAliasId;
                registration.FirstName = RegistrationState.YourFirstName;
                registration.LastName = RegistrationState.YourLastName;
                registration.ConfirmationEmail = RegistrationState.ConfirmationEmail;
                registration.DiscountCode = RegistrationState.DiscountCode;

                // If PersonAliasId, FirstName, or LastName have changed, create a new person for registration
                if ( registration.PersonAlias == null ||
                    !RegistrationState.PersonAliasId.HasValue || 
                    registration.PersonAliasId != RegistrationState.PersonAliasId.Value ||
                    ( registration.PersonAlias.Person.NickName != RegistrationState.YourFirstName && 
                        registration.PersonAlias.Person.FirstName != RegistrationState.YourFirstName ) ||
                    registration.PersonAlias.Person.LastName != RegistrationState.YourLastName )
                {

                }
                    

            }

            return registration;
        }
        /// <summary>
        /// Adds (or removes) registrants to or from the registration. Only newly added registrants can
        /// can be removed. Any existing (saved) registrants cannot be removed from the registration
        /// </summary>
        /// <param name="registrantCount">The number of registrants that registration should have.</param>
        private void SetRegistrantState( int registrantCount )
        {
            if ( RegistrationState != null )
            {
                // While the number of registrants belonging to registration is less than the selected count, addd another registrant
                while ( RegistrationState.RegistrantCount < registrantCount )
                {
                    RegistrationState.Registrants.Add( new RegistrantInfo { Cost = RegistrationTemplate.Cost } );
                }

                // Get the number of registrants that needs to be removed
                int removeCount = RegistrationState.RegistrantCount - registrantCount;
                if ( removeCount > 0 )
                {
                    // If removing any, reverse the order of registrants, so that most recently added will be removed first
                    RegistrationState.Registrants.Reverse();

                    // Try to get the registrants to remove. Most recently added will be taken first
                    foreach ( var registrant in RegistrationState.Registrants.Where( r => !r.Existing ).Take( removeCount ).ToList() )
                    {
                        RegistrationState.Registrants.Remove( registrant );
                    }

                    // Reset the order after removing any registrants
                    RegistrationState.Registrants.Reverse();
                }
            }
        }

        #endregion

        //private bool ProcessPayment( out string errorMessage )
        //{
        //    var rockContext = new RockContext();
        //    if ( string.IsNullOrWhiteSpace( TransactionCode ) )
        //    {
        //        GatewayComponent gateway = null;
        //        if ( RegistrationTemplate != null && RegistrationTemplate.FinancialGateway != null )
        //        {
        //            gateway = RegistrationTemplate.FinancialGateway.GetGatewayComponent();
        //        }

        //        if ( gateway == null )
        //        {
        //            errorMessage = "There was a problem creating the payment gateway information";
        //            return false;
        //        }

        //        Person person = GetPerson( true );
        //        if ( person == null )
        //        {
        //            errorMessage = "There was a problem creating the person information";
        //            return false;
        //        }

        //        if ( !person.PrimaryAliasId.HasValue )
        //        {
        //            errorMessage = "There was a problem creating the person's primary alias";
        //            return false;
        //        }

        //        PaymentInfo paymentInfo = GetPaymentInfo();
        //        if ( paymentInfo == null )
        //        {
        //            errorMessage = "There was a problem creating the payment information";
        //            return false;
        //        }
        //        else
        //        {
        //            paymentInfo.FirstName = person.FirstName;
        //            paymentInfo.LastName = person.LastName;
        //        }

        //        if ( paymentInfo.CreditCardTypeValue != null )
        //        {
        //            CreditCardTypeValueId = paymentInfo.CreditCardTypeValue.Id;
        //        }

        //        PaymentSchedule schedule = GetSchedule();
        //        if ( schedule != null )
        //        {
        //            schedule.PersonId = person.Id;

        //            var scheduledTransaction = gateway.AddScheduledPayment( financialGateway, schedule, paymentInfo, out errorMessage );
        //            if ( scheduledTransaction != null )
        //            {
        //                scheduledTransaction.TransactionFrequencyValueId = schedule.TransactionFrequencyValue.Id;
        //                scheduledTransaction.AuthorizedPersonAliasId = person.PrimaryAliasId.Value;
        //                scheduledTransaction.FinancialGatewayId = financialGateway.Id;
        //                scheduledTransaction.CurrencyTypeValueId = paymentInfo.CurrencyTypeValue.Id;
        //                scheduledTransaction.CreditCardTypeValueId = CreditCardTypeValueId;

        //                var changeSummary = new StringBuilder();
        //                changeSummary.AppendFormat( "{0} starting {1}", schedule.TransactionFrequencyValue.Value, schedule.StartDate.ToShortDateString() );
        //                changeSummary.AppendLine();
        //                changeSummary.Append( paymentInfo.CurrencyTypeValue.Value );
        //                if ( paymentInfo.CreditCardTypeValue != null )
        //                {
        //                    changeSummary.AppendFormat( " - {0}", paymentInfo.CreditCardTypeValue.Value );
        //                }
        //                changeSummary.AppendFormat( " {0}", paymentInfo.MaskedNumber );
        //                changeSummary.AppendLine();

        //                foreach ( var account in SelectedAccounts.Where( a => a.Amount > 0 ) )
        //                {
        //                    var transactionDetail = new FinancialScheduledTransactionDetail();
        //                    transactionDetail.Amount = account.Amount;
        //                    transactionDetail.AccountId = account.Id;
        //                    scheduledTransaction.ScheduledTransactionDetails.Add( transactionDetail );
        //                    changeSummary.AppendFormat( "{0}: {1:C2}", account.Name, account.Amount );
        //                    changeSummary.AppendLine();
        //                }

        //                var transactionService = new FinancialScheduledTransactionService( rockContext );
        //                transactionService.Add( scheduledTransaction );
        //                rockContext.SaveChanges();

        //                // Add a note about the change
        //                var noteType = NoteTypeCache.Read( Rock.SystemGuid.NoteType.SCHEDULED_TRANSACTION_NOTE.AsGuid() );
        //                if ( noteType != null )
        //                {
        //                    var noteService = new NoteService( rockContext );
        //                    var note = new Note();
        //                    note.NoteTypeId = noteType.Id;
        //                    note.EntityId = scheduledTransaction.Id;
        //                    note.Caption = "Created Transaction";
        //                    note.Text = changeSummary.ToString();
        //                    noteService.Add( note );
        //                }
        //                rockContext.SaveChanges();

        //                ScheduleId = scheduledTransaction.GatewayScheduleId;
        //                TransactionCode = scheduledTransaction.TransactionCode;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }
        //        else
        //        {
        //            var transaction = gateway.Charge( financialGateway, paymentInfo, out errorMessage );
        //            if ( transaction != null )
        //            {
        //                var txnChanges = new List<string>();
        //                txnChanges.Add( "Created Transaction" );

        //                History.EvaluateChange( txnChanges, "Transaction Code", string.Empty, transaction.TransactionCode );

        //                transaction.AuthorizedPersonAliasId = person.PrimaryAliasId;
        //                History.EvaluateChange( txnChanges, "Person", string.Empty, person.FullName );

        //                transaction.TransactionDateTime = RockDateTime.Now;
        //                History.EvaluateChange( txnChanges, "Date/Time", null, transaction.TransactionDateTime );

        //                transaction.FinancialGatewayId = financialGateway.Id;
        //                History.EvaluateChange( txnChanges, "Gateway", string.Empty, financialGateway.Name );

        //                var txnType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION ) );
        //                transaction.TransactionTypeValueId = txnType.Id;
        //                History.EvaluateChange( txnChanges, "Type", string.Empty, txnType.Value );

        //                transaction.CurrencyTypeValueId = paymentInfo.CurrencyTypeValue.Id;
        //                History.EvaluateChange( txnChanges, "Currency Type", string.Empty, paymentInfo.CurrencyTypeValue.Value );

        //                transaction.CreditCardTypeValueId = CreditCardTypeValueId;
        //                if ( CreditCardTypeValueId.HasValue )
        //                {
        //                    var ccType = DefinedValueCache.Read( CreditCardTypeValueId.Value );
        //                    History.EvaluateChange( txnChanges, "Credit Card Type", string.Empty, ccType.Value );
        //                }

        //                Guid sourceGuid = Guid.Empty;
        //                if ( Guid.TryParse( GetAttributeValue( "Source" ), out sourceGuid ) )
        //                {
        //                    var source = DefinedValueCache.Read( sourceGuid );
        //                    if ( source != null )
        //                    {
        //                        transaction.SourceTypeValueId = source.Id;
        //                        History.EvaluateChange( txnChanges, "Source", string.Empty, source.Value );
        //                    }
        //                }

        //                foreach ( var account in SelectedAccounts.Where( a => a.Amount > 0 ) )
        //                {
        //                    var transactionDetail = new FinancialTransactionDetail();
        //                    transactionDetail.Amount = account.Amount;
        //                    transactionDetail.AccountId = account.Id;
        //                    transaction.TransactionDetails.Add( transactionDetail );
        //                    History.EvaluateChange( txnChanges, account.Name, 0.0M.ToString( "C2" ), transactionDetail.Amount.ToString( "C2" ) );
        //                }

        //                var batchService = new FinancialBatchService( rockContext );

        //                // Get the batch
        //                var batch = batchService.Get(
        //                    GetAttributeValue( "BatchNamePrefix" ),
        //                    paymentInfo.CurrencyTypeValue,
        //                    paymentInfo.CreditCardTypeValue,
        //                    transaction.TransactionDateTime.Value,
        //                    financialGateway.GetBatchTimeOffset() );

        //                var batchChanges = new List<string>();

        //                if ( batch.Id == 0 )
        //                {
        //                    batchChanges.Add( "Generated the batch" );
        //                    History.EvaluateChange( batchChanges, "Batch Name", string.Empty, batch.Name );
        //                    History.EvaluateChange( batchChanges, "Status", null, batch.Status );
        //                    History.EvaluateChange( batchChanges, "Start Date/Time", null, batch.BatchStartDateTime );
        //                    History.EvaluateChange( batchChanges, "End Date/Time", null, batch.BatchEndDateTime );
        //                }

        //                decimal newControlAmount = batch.ControlAmount + transaction.TotalAmount;
        //                History.EvaluateChange( batchChanges, "Control Amount", batch.ControlAmount.ToString( "C2" ), newControlAmount.ToString( "C2" ) );
        //                batch.ControlAmount = newControlAmount;

        //                transaction.BatchId = batch.Id;
        //                batch.Transactions.Add( transaction );

        //                rockContext.WrapTransaction( () =>
        //                {
        //                    rockContext.SaveChanges();

        //                    HistoryService.SaveChanges(
        //                        rockContext,
        //                        typeof( FinancialBatch ),
        //                        Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
        //                        batch.Id,
        //                        batchChanges
        //                    );

        //                    HistoryService.SaveChanges(
        //                        rockContext,
        //                        typeof( FinancialBatch ),
        //                        Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
        //                        batch.Id,
        //                        txnChanges,
        //                        person.FullName,
        //                        typeof( FinancialTransaction ),
        //                        transaction.Id
        //                    );
        //                } );

        //                TransactionCode = transaction.TransactionCode;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }

        //        tdTransactionCodeReceipt.Description = TransactionCode;
        //        tdTransactionCodeReceipt.Visible = !string.IsNullOrWhiteSpace( TransactionCode );

        //        tdScheduleId.Description = ScheduleId;
        //        tdScheduleId.Visible = !string.IsNullOrWhiteSpace( ScheduleId );

        //        tdNameReceipt.Description = paymentInfo.FullName;
        //        tdPhoneReceipt.Description = paymentInfo.Phone;
        //        tdEmailReceipt.Description = paymentInfo.Email;
        //        tdAddressReceipt.Description = string.Format( "{0} {1}, {2} {3}", paymentInfo.Street1, paymentInfo.City, paymentInfo.State, paymentInfo.PostalCode );

        //        rptAccountListReceipt.DataSource = SelectedAccounts.Where( a => a.Amount != 0 );
        //        rptAccountListReceipt.DataBind();

        //        tdTotalReceipt.Description = paymentInfo.Amount.ToString( "C" );

        //        tdPaymentMethodReceipt.Description = paymentInfo.CurrencyTypeValue.Description;
        //        tdAccountNumberReceipt.Description = paymentInfo.MaskedNumber;
        //        tdWhenReceipt.Description = schedule != null ? schedule.ToString() : "Today";

        //        // If there was a transaction code returned and this was not already created from a previous saved account,
        //        // show the option to save the account.
        //        if ( !( paymentInfo is ReferencePaymentInfo ) && !string.IsNullOrWhiteSpace( TransactionCode ) && gateway.SupportsSavedAccount( paymentInfo.CurrencyTypeValue ) )
        //        {
        //            cbSaveAccount.Visible = true;
        //            pnlSaveAccount.Visible = true;
        //            txtSaveAccount.Visible = true;

        //            // If current person does not have a login, have them create a username and password
        //            phCreateLogin.Visible = !new UserLoginService( rockContext ).GetByPersonId( person.Id ).Any();
        //        }
        //        else
        //        {
        //            pnlSaveAccount.Visible = false;
        //        }

        //        return true;
        //    }
        //    else
        //    {
        //        pnlDupWarning.Visible = true;
        //        divActions.Visible = false;
        //        errorMessage = string.Empty;
        //        return false;
        //    }
        //}

        //private PaymentInfo GetPaymentInfo( GatewayComponent gateway )
        //{
        //    var ccPaymentInfo = new CreditCardPaymentInfo( txtCreditCard.Text, txtCVV.Text, mypExpiration.SelectedDate.Value );
        //    ccPaymentInfo.NameOnCard = gateway != null && gateway.SplitNameOnCard ? txtCardFirstName.Text : txtCardName.Text;
        //    ccPaymentInfo.LastNameOnCard = txtCardLastName.Text;

        //    ccPaymentInfo.BillingStreet1 = acBillingAddress.Street1;
        //    ccPaymentInfo.BillingStreet2 = acBillingAddress.Street2;
        //    ccPaymentInfo.BillingCity = acBillingAddress.City;
        //    ccPaymentInfo.BillingState = acBillingAddress.State;
        //    ccPaymentInfo.BillingPostalCode = acBillingAddress.PostalCode;
        //    ccPaymentInfo.BillingCountry = acBillingAddress.Country;

        //    ccPaymentInfo.Amount = RegistrationState.PaymentAmount;
        //    ccPaymentInfo.Email = RegistrationState.ConfirmationEmail;

        //    if ( RegistrationState.personId.HasValue )
        //    ccPaymentInfo.Phone = PhoneNumber.FormattedNumber( pnbPhone.CountryCode, pnbPhone.Number, true );
        //    ccPaymentInfo.Street1 = acAddress.Street1;
        //    ccPaymentInfo.Street2 = acAddress.Street2;
        //    ccPaymentInfo.City = acAddress.City;
        //    ccPaymentInfo.State = acAddress.State;
        //    ccPaymentInfo.PostalCode = acAddress.PostalCode;
        //    ccPaymentInfo.Country = acAddress.Country;

        //    return ccPaymentInfo;
        //}

        #region Display Methods

        /// <summary>
        /// Shows the how many panel
        /// </summary>
        private void ShowHowMany()
        {
            if ( MaxRegistrants > MinRegistrants )
            {
                numHowMany.Maximum = MaxRegistrants;
                numHowMany.Minimum = MinRegistrants;
                numHowMany.Value = RegistrantCount;

                SetPanel( 0 );
            }
            else
            {
                CurrentRegistrantIndex = 0;
                CurrentFormIndex = 0;

                SetRegistrantState( MinRegistrants );

                ShowRegistrant();
            }
        }

        /// <summary>
        /// Shows the registrant panel
        /// </summary>
        private void ShowRegistrant()
        {
            if ( RegistrantCount > 0 )
            {
                if ( CurrentRegistrantIndex < 0 )
                {
                    ShowHowMany();
                }
                else if ( CurrentRegistrantIndex >= RegistrantCount )
                {
                    ShowSummary();
                }
                else
                {
                    string title = RegistrantCount <= 1 ? "Individual" : ( CurrentRegistrantIndex + 1 ).ToOrdinalWords().Humanize( LetterCasing.Title ) + " Individual";
                    if ( CurrentFormIndex > 0 )
                    {
                        title += " (cont)";
                    }
                    lRegistrantTitle.Text = title;

                    rblFamilyOptions.Visible = CurrentRegistrantIndex > 0 && RegistrationTemplate != null && RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask;

                    SetPanel( 1 );
                }
            }
            else
            {
                // If for some reason there are not any registrants ( i.e. viewstate expired ), return to first screen
                ShowHowMany();
            }
        }

        /// <summary>
        /// Shows the summary panel
        /// </summary>
        private void ShowSummary()
        {
            SetPanel( 2 );
        }

        /// <summary>
        /// Shows the success panel
        /// </summary>
        private void ShowSuccess()
        {
            SetPanel( 3 );
        }

        /// <summary>
        /// Creates the dynamic controls, and shows correct panel
        /// </summary>
        /// <param name="currentPanel">The current panel.</param>
        private void SetPanel( int currentPanel )
        {
            CurrentPanel = currentPanel;

            CreateDynamicControls( true );

            pnlHowMany.Visible = CurrentPanel <= 0;
            pnlRegistrant.Visible = CurrentPanel == 1;
            pnlSummaryAndPayment.Visible = CurrentPanel == 2;
            pnlSuccess.Visible = CurrentPanel == 3;
        }

        /// <summary>
        /// Registers the client script.
        /// </summary>
        private void RegisterClientScript()
        {
            RockPage.AddScriptLink( ResolveUrl( "~/Scripts/jquery.creditCardTypeDetector.js" ) );

            string script = string.Format( @"
    // Adjust the label of 'is in the same family' based on value of first name entered
    $('input.js-first-name').change( function() {{
        var name = $(this).val();
        if ( name == null || name == '') {{
            name = 'Individual';
        }}
        var $lbl = $('div.js-registration-same-family').find('label.control-label')
        $lbl.text( name + ' is in the same family as');
    }} );

    $('#{0}').on('change', function() {{

        var totalCost = Number($('#{1}').val());
        var minDue = Number($('#{2}').val());
        var previouslyPaid = Number($('#{3}').val());
        var balanceDue = totalCost - previouslyPaid;

        // Format and validate the amount entered
        var amountPaid = minDue;
        var amountValue = $(this).val();
        if ( amountValue != null && amountValue != '' && !isNaN( amountValue ) ) {{
            amountPaid = Number( amountValue );
            if ( amountPaid < minDue ) {{
                amountPaid = minDue;
            }}
            if ( amountPaid > balanceDue ) {{
                amountPaid = balanceDue
            }}
        }}
        $(this).val(amountPaid.toFixed(2));

        var amountRemaining = totalCost - ( previouslyPaid + amountPaid );
        $('#{4}').text( '$' + amountRemaining.toFixed(2) );
        
    }});

    // Detect credit card type
    $('.credit-card').creditCardTypeDetector({{ 'credit_card_logos': '.card-logos' }});

    if ( $('#{5}').val() == 'true' ) {{
        setTimeout('window.scrollTo(0,0)',0);
        $('#{5}').val('')
    }}
",
            nbAmountPaid.ClientID, hfTotalCost.ClientID, hfMinimumDue.ClientID, hfPreviouslyPaid.ClientID, lRemainingDue.ClientID, hfTriggerScroll.ClientID);

            ScriptManager.RegisterStartupScript( Page, Page.GetType(), "registrationEntry", script, true );
        }

        #endregion

        #region Dynamic Control Methods

        /// <summary>
        /// Creates the dynamic controls fore each panel
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void CreateDynamicControls( bool setValues )
        {
            switch( CurrentPanel )
            {
                case 1:
                    CreateRegistrantControls( setValues );
                    break;
                case 2:
                    CreateSummaryControls( setValues );
                    break;
                case 3:
                    CreateSuccessControls( setValues );
                    break;
            }
        }

        /// <summary>
        /// Parses the dynamic controls.
        /// </summary>
        private void ParseDynamicControls()
        {
            switch ( CurrentPanel )
            {
                case 1:
                    ParseRegistrantControls();
                    break;
                case 2:
                    ParseSummaryControls();
                    break;
            }
        }

        #region Registrant Controls

        /// <summary>
        /// Creates the registrant controls.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void CreateRegistrantControls( bool setValues )
        {
            phRegistrantControls.Controls.Clear();
            phFees.Controls.Clear();

            if ( FormCount > CurrentFormIndex )
            {
                // Get the current and previous registrant ( previous is used when a field has the 'IsSharedValue' property )
                // so that current registrant can use the previous registrants value
                RegistrantInfo registrant = null;
                RegistrantInfo previousRegistrant = null;

                if ( RegistrationState != null && RegistrationState.RegistrantCount > CurrentRegistrantIndex )
                {
                    registrant = RegistrationState.Registrants[CurrentRegistrantIndex];

                    // If this is not the first person, then check to see if option for asking about family should be displayed
                    if ( CurrentFormIndex == 0 && CurrentRegistrantIndex > 0 &&
                        RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask )
                    {
                        var familyOptions = RegistrationState.GetFamilyOptions( RegistrationTemplate, CurrentRegistrantIndex );
                        if ( familyOptions.Any() )
                        {
                            Guid noneGuid = familyOptions.Keys.Contains( registrant.Guid ) ? Guid.NewGuid() : registrant.Guid;
                            familyOptions.Add( noneGuid, "None of the above" );
                            rblFamilyOptions.DataSource = familyOptions;
                            rblFamilyOptions.DataBind();
                            rblFamilyOptions.Visible = true;
                        }
                        else
                        {
                            rblFamilyOptions.Visible = false;
                        }
                    }
                    else
                    {
                        rblFamilyOptions.Visible = false;
                    }

                    if ( setValues )
                    {
                        if ( CurrentRegistrantIndex > 0 )
                        {
                            previousRegistrant = RegistrationState.Registrants[CurrentRegistrantIndex - 1];
                        }

                        rblFamilyOptions.SetValue( registrant.FamilyGuid.ToString() );
                    }
                }

                var form = RegistrationTemplate.Forms.OrderBy( f => f.Order ).ToList()[CurrentFormIndex];
                foreach ( var field in form.Fields.OrderBy( f => f.Order ) )
                {
                    object value = null;
                    if ( registrant != null && registrant.FieldValues.ContainsKey( field.Id ) )
                    {
                        value = registrant.FieldValues[field.Id];
                        if ( value == null && field.IsSharedValue && previousRegistrant != null && previousRegistrant.FieldValues.ContainsKey( field.Id ) )
                        {
                            value = previousRegistrant.FieldValues[field.Id];
                        }
                    }

                    if ( !string.IsNullOrWhiteSpace( field.PreText ) )
                    {
                        phRegistrantControls.Controls.Add( new LiteralControl( field.PreText ) );
                    }

                    if ( field.FieldSource == RegistrationFieldSource.PersonField )
                    {
                        CreatePersonField( field, setValues, value);
                    }
                    else
                    {
                        CreateAttributeField( field, setValues, value );
                    }

                    if ( !string.IsNullOrWhiteSpace( field.PostText ) )
                    {
                        phRegistrantControls.Controls.Add( new LiteralControl( field.PostText ) );
                    }

                }

                // If the current form, is the last one, add any fee controls
                if ( FormCount - 1 == CurrentFormIndex )
                {
                    foreach ( var fee in RegistrationTemplate.Fees )
                    {
                        var feeValues = new List<FeeInfo>();
                        if ( registrant != null && registrant.FeeValues.ContainsKey( fee.Id ) )
                        {
                            feeValues = registrant.FeeValues[fee.Id];
                        }
                        CreateFeeField( fee, setValues, feeValues );
                    }
                }
            }

            divFees.Visible = phFees.Controls.Count > 0;
        }

        /// <summary>
        /// Creates the person field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <param name="fieldValue">The field value.</param>
        private void CreatePersonField( RegistrationTemplateFormField field, bool setValue, object fieldValue )
        {

            switch ( field.PersonFieldType )
            {
                case RegistrationPersonFieldType.Birthdate:
                    {
                        var bpBirthday = new BirthdayPicker();
                        bpBirthday.ID = "bpBirthday";
                        bpBirthday.Label = "Birthday";
                        bpBirthday.Required = field.IsRequired;
                        phRegistrantControls.Controls.Add( bpBirthday );

                        if ( setValue && fieldValue != null )
                        {
                            var value = fieldValue as DateTime?;
                            bpBirthday.SelectedDate = value;
                        }

                        break;
                    }

                case RegistrationPersonFieldType.Email:
                    {
                        var tbEmail = new EmailBox();
                        tbEmail.ID = "tbEmail";
                        tbEmail.Label = "Email";
                        tbEmail.Required = field.IsRequired;
                        tbEmail.ValidationGroup = BlockValidationGroup;
                        phRegistrantControls.Controls.Add( tbEmail );

                        if ( setValue && fieldValue != null )
                        {
                            tbEmail.Text = fieldValue.ToString();
                        }

                        break;
                    }

                case RegistrationPersonFieldType.FirstName:
                    {
                        var tbFirstName = new RockTextBox();
                        tbFirstName.ID = "tbFirstName";
                        tbFirstName.Label = "First Name";
                        tbFirstName.Required = field.IsRequired;
                        tbFirstName.ValidationGroup = BlockValidationGroup;
                        tbFirstName.AddCssClass( "js-first-name" );
                        phRegistrantControls.Controls.Add( tbFirstName );

                        if ( setValue && fieldValue != null )
                        {
                            tbFirstName.Text = fieldValue.ToString();
                        }

                        break;
                    }

                case RegistrationPersonFieldType.Gender:
                    {
                        var ddlGender = new RockDropDownList();
                        ddlGender.ID = "ddlGender";
                        ddlGender.Label = "Gender";
                        ddlGender.Required = field.IsRequired;
                        ddlGender.ValidationGroup = BlockValidationGroup;
                        ddlGender.BindToEnum<Gender>( true );
                        phRegistrantControls.Controls.Add( ddlGender );

                        if ( setValue && fieldValue != null )
                        {
                            var value = fieldValue.ToString().ConvertToEnumOrNull<Gender>() ?? Gender.Unknown;
                            ddlGender.SetValue( value.ConvertToInt() );
                        }

                        break;
                    }

                case RegistrationPersonFieldType.HomeCampus:
                    {
                        // TODO: Create campus picker
                        break;
                    }

                case RegistrationPersonFieldType.LastName:
                    {
                        var tbLastName = new RockTextBox();
                        tbLastName.ID = "tbLastName";
                        tbLastName.Label = "Last Name";
                        tbLastName.Required = field.IsRequired;
                        tbLastName.ValidationGroup = BlockValidationGroup;
                        phRegistrantControls.Controls.Add( tbLastName );

                        if ( setValue && fieldValue != null )
                        {
                            tbLastName.Text = fieldValue.ToString();
                        }

                        break;
                    }
                case RegistrationPersonFieldType.MaritalStatus:
                    {
                        var ddlMaritalStatus = new RockDropDownList();
                        ddlMaritalStatus.ID = "ddlGender";
                        ddlMaritalStatus.Label = "Marital Status";
                        ddlMaritalStatus.Required = field.IsRequired;
                        ddlMaritalStatus.ValidationGroup = BlockValidationGroup;
                        ddlMaritalStatus.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() ) );
                        phRegistrantControls.Controls.Add( ddlMaritalStatus );

                        if ( setValue && fieldValue != null )
                        {
                            var value = fieldValue as int? ?? 0;
                            ddlMaritalStatus.SetValue( value );
                        }

                        break;
                    }
                case RegistrationPersonFieldType.Phone:
                    {
                        break;
                    }
            }
        }

        /// <summary>
        /// Creates the attribute field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <param name="fieldValue">The field value.</param>
        private void CreateAttributeField( RegistrationTemplateFormField field, bool setValue, object fieldValue )
        {
            if ( field.AttributeId.HasValue )
            {
                var attribute = AttributeCache.Read( field.AttributeId.Value );

                string value = string.Empty;
                if ( setValue && fieldValue != null )
                {
                    value = fieldValue.ToString();
                }

                attribute.AddControl( phRegistrantControls.Controls, value, BlockValidationGroup, setValue, true, field.IsRequired, null, string.Empty );
            }
        }

        /// <summary>
        /// Creates the fee field.
        /// </summary>
        /// <param name="fee">The fee.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        /// <param name="feeValues">The fee values.</param>
        private void CreateFeeField( RegistrationTemplateFee fee, bool setValues, List<FeeInfo> feeValues )
        {
            if ( fee.FeeType == RegistrationFeeType.Single )
            {
                string label = fee.Name;
                var cost = fee.CostValue.AsDecimalOrNull();
                if ( cost.HasValue && cost.Value != 0.0M )
                {
                    label = string.Format( "{0} ({1})", fee.Name, cost.Value.ToString("C2"));
                }

                if ( fee.AllowMultiple )
                {
                    // Single Option, Multi Quantity
                    var numUpDown = new NumberUpDown();
                    numUpDown.ID = "fee_" + fee.Id.ToString();
                    numUpDown.Label = label;
                    numUpDown.Minimum = 0;
                    phFees.Controls.Add( numUpDown );

                    if ( setValues && feeValues != null && feeValues.Any() )
                    {
                        numUpDown.Value = feeValues.First().Quantity;
                    }
                }
                else
                {
                    // Single Option, Single Quantity
                    var cb = new RockCheckBox();
                    cb.ID = "fee_" + fee.Id.ToString();
                    cb.Label = label;
                    cb.SelectedIconCssClass = "fa fa-check-square-o fa-lg";
                    cb.UnSelectedIconCssClass = "fa fa-square-o fa-lg";
                    phFees.Controls.Add( cb );

                    if ( setValues && feeValues != null && feeValues.Any() )
                    {
                        cb.Checked = feeValues.First().Quantity > 0;
                    }
                }
            }
            else
            {
                // Parse the options to get name and cost for each
                var options = new Dictionary<string, string>();
                string[] nameValues = fee.CostValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                foreach ( string nameValue in nameValues )
                {
                    string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                    if ( nameAndValue.Length == 1)
                    {
                        options.AddOrIgnore( nameAndValue[0], nameAndValue[0] );
                    }
                    if ( nameAndValue.Length == 2 )
                    {
                        options.AddOrIgnore( nameAndValue[0], string.Format( "{0} ({1:C2})", nameAndValue[0], nameAndValue[1].AsDecimal() ) );
                    }
                }

                if ( fee.AllowMultiple )
                {
                    foreach( var optionKeyVal in options )
                    {
                        var numUpDown = new NumberUpDown();
                        numUpDown.ID = string.Format( "fee_{0}_{1}", fee.Id, optionKeyVal.Key );
                        numUpDown.Label = string.Format( "{0} - {1}", fee.Name, optionKeyVal.Value );
                        numUpDown.Minimum = 0;
                        phFees.Controls.Add( numUpDown );

                        if ( setValues && feeValues != null && feeValues.Any() )
                        {
                            numUpDown.Value = feeValues
                                .Where( f => f.Option == optionKeyVal.Key )
                                .Select( f => f.Quantity )
                                .FirstOrDefault();
                        }
                    }
                }
                else
                {
                    // Multi Option, Single Quantity
                    var ddl = new RockDropDownList();
                    ddl.ID = "fee_" + fee.Id.ToString();
                    ddl.AddCssClass( "input-width-md" );
                    ddl.Label = fee.Name;
                    ddl.DataValueField = "Key";
                    ddl.DataTextField = "Value";
                    ddl.DataSource = options;
                    ddl.DataBind();
                    ddl.Items.Insert( 0, "");
                    phFees.Controls.Add( ddl );

                    if ( setValues && feeValues != null && feeValues.Any() )
                    {
                        ddl.SetValue( feeValues
                            .Where( f => f.Quantity > 0 )
                            .Select( f => f.Option )
                            .FirstOrDefault() );
                    }
                }
            }
        }

        /// <summary>
        /// Parses the registrant controls.
        /// </summary>
        private void ParseRegistrantControls()
        {
            if ( RegistrationState != null && RegistrationState.Registrants.Count > CurrentRegistrantIndex )
            {
                var registrant = RegistrationState.Registrants[CurrentRegistrantIndex];

                if ( rblFamilyOptions.Visible )
                {
                    Guid? familyGuid = rblFamilyOptions.SelectedValueAsGuid();
                    if ( !familyGuid.HasValue || familyGuid.Value.Equals( Guid.Empty ) )
                    {
                        familyGuid = Guid.NewGuid();
                    }
                    registrant.FamilyGuid = familyGuid.Value;
                }

                var form = RegistrationTemplate.Forms.OrderBy( f => f.Order ).ToList()[CurrentFormIndex];
                foreach ( var field in form.Fields.OrderBy( f => f.Order ) )
                {
                    object value = null;

                    if ( field.FieldSource == RegistrationFieldSource.PersonField )
                    {
                        value = ParsePersonField( field );
                    }
                    else
                    {
                        value = ParseAttributeField( field );
                    }

                    if ( value != null )
                    {
                        registrant.FieldValues.AddOrReplace( field.Id, value );
                    }
                    else
                    {
                        registrant.FieldValues.Remove( field.Id );
                    }
                }

                if ( FormCount - 1 == CurrentFormIndex )
                {
                    foreach ( var fee in RegistrationTemplate.Fees )
                    {
                        List<FeeInfo> feeValues = ParseFee( fee );
                        if ( fee != null )
                        {
                            registrant.FeeValues.AddOrReplace( fee.Id, feeValues );
                        }
                        else
                        {
                            registrant.FeeValues.Remove( fee.Id );
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Parses the person field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        private object ParsePersonField( RegistrationTemplateFormField field )
        {
            switch ( field.PersonFieldType )
            {
                case RegistrationPersonFieldType.Birthdate:
                    {
                        var bpBirthday = phRegistrantControls.FindControl( "bpBirthday" ) as BirthdayPicker;
                        return bpBirthday != null ? bpBirthday.SelectedDate : null;
                    }

                case RegistrationPersonFieldType.Email:
                    {
                        var tbEmail = phRegistrantControls.FindControl( "tbEmail" ) as EmailBox;
                        return tbEmail != null ? tbEmail.Text : null;
                    }

                case RegistrationPersonFieldType.FirstName:
                    {
                        var tbFirstName = phRegistrantControls.FindControl( "tbFirstName" ) as RockTextBox;
                        return tbFirstName != null ? tbFirstName.Text : null;
                    }

                case RegistrationPersonFieldType.Gender:
                    {
                        var ddlGender = phRegistrantControls.FindControl( "ddlGender" ) as RockDropDownList;
                        return ddlGender != null ? ddlGender.SelectedValueAsInt() : null;
                    }

                case RegistrationPersonFieldType.HomeCampus:
                    {
                        // TODO: Create campus picker
                        break;
                    }

                case RegistrationPersonFieldType.LastName:
                    {
                        var tbLastName = phRegistrantControls.FindControl( "tbLastName" ) as RockTextBox;
                        return tbLastName != null ? tbLastName.Text : null;
                    }

                case RegistrationPersonFieldType.MaritalStatus:
                    {
                        var ddlMaritalStatus = phRegistrantControls.FindControl( "ddlMaritalStatus" ) as RockDropDownList;
                        return ddlMaritalStatus != null ? ddlMaritalStatus.SelectedValueAsInt() : null;
                    }

                case RegistrationPersonFieldType.Phone:
                    {
                        break;
                    }
            }

            return null;

        }

        /// <summary>
        /// Parses the attribute field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        private object ParseAttributeField( RegistrationTemplateFormField field )
        {
            if ( field.AttributeId.HasValue )
            {
                var attribute = AttributeCache.Read( field.AttributeId.Value );
                string fieldId = "attribute_field_" + attribute.Id.ToString();

                Control control = phRegistrantControls.FindControl( fieldId );
                if ( control != null )
                {
                    return attribute.FieldType.Field.GetEditValue( control, attribute.QualifierValues );
                }
            }

            return null;
        }

        /// <summary>
        /// Parses the fee.
        /// </summary>
        /// <param name="fee">The fee.</param>
        /// <returns></returns>
        private List<FeeInfo> ParseFee( RegistrationTemplateFee fee )
        {
            string fieldId = string.Format( "fee_{0}", fee.Id );

            if ( fee.FeeType == RegistrationFeeType.Single )
            {
                if ( fee.AllowMultiple )
                {
                    // Single Option, Multi Quantity
                    var numUpDown = phFees.FindControl( fieldId ) as NumberUpDown;
                    if ( numUpDown != null && numUpDown.Value > 0 )
                    {
                        return new List<FeeInfo> { new FeeInfo( string.Empty, numUpDown.Value, fee.CostValue.AsDecimal() ) };
                    }
                }
                else
                {
                    // Single Option, Single Quantity
                    var cb = phFees.FindControl( fieldId ) as RockCheckBox;
                    if ( cb != null && cb.Checked )
                    {
                        return new List<FeeInfo> { new FeeInfo( string.Empty, 1, fee.CostValue.AsDecimal() ) };
                    }
                }
            }
            else
            {
                // Parse the options to get name and cost for each
                var options = new Dictionary<string, string>();
                var optionCosts = new Dictionary<string, decimal>();

                string[] nameValues = fee.CostValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                foreach ( string nameValue in nameValues )
                {
                    string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                    if ( nameAndValue.Length == 1 )
                    {
                        options.AddOrIgnore( nameAndValue[0], nameAndValue[0] );
                        optionCosts.AddOrIgnore( nameAndValue[0], 0.0m );
                    }
                    if ( nameAndValue.Length == 2 )
                    {
                        options.AddOrIgnore( nameAndValue[0], string.Format( "{0} ({1:C2})", nameAndValue[0], nameAndValue[1].AsDecimal() ) );
                        optionCosts.AddOrIgnore( nameAndValue[0], nameAndValue[1].AsDecimal() );
                    }
                }

                if ( fee.AllowMultiple )
                {
                    // Multi Option, Multi Quantity
                    var result = new List<FeeInfo>();

                    foreach ( var optionKeyVal in options )
                    {
                        string optionFieldId = string.Format( "{0}_{1}", fieldId, optionKeyVal.Key );
                        var numUpDown = phFees.FindControl( optionFieldId ) as NumberUpDown;
                        if ( numUpDown != null && numUpDown.Value > 0 )
                        {
                            result.Add( new FeeInfo( optionKeyVal.Key, numUpDown.Value, optionCosts[optionKeyVal.Key] ) );
                        }
                    }

                    if ( result.Any() )
                    {
                        return result;
                    }
                }
                else
                {
                    // Multi Option, Single Quantity
                    var ddl = phFees.FindControl( fieldId ) as RockDropDownList;
                    if ( ddl != null && ddl.SelectedValue != "" )
                    {
                        return new List<FeeInfo> { new FeeInfo( ddl.SelectedValue, 1, optionCosts[ddl.SelectedValue] ) };
                    }
                }
            }

            return null;
        }

        #endregion

        #region Summary/Payment Controls

        private void CreateSummaryControls( bool setValues )
        {
            phSuccessControls.Controls.Clear();

            if ( setValues )
            {
                // Check to see if any information has already been entered
                if ( !string.IsNullOrWhiteSpace( RegistrationState.YourFirstName) ||
                    !string.IsNullOrWhiteSpace( RegistrationState.YourLastName ) ||
                    !string.IsNullOrWhiteSpace( RegistrationState.ConfirmationEmail ) )
                {
                    // If so, use it
                    tbYourFirstName.Text = RegistrationState.YourFirstName;
                    tbYourLastName.Text = RegistrationState.YourLastName;
                    tbConfirmationEmail.Text = RegistrationState.ConfirmationEmail;
                }
                else
                {
                    // If not, find the field information from first registrant
                    if ( RegistrationState.Registrants.Any() )
                    {
                        var firstRegistrant = RegistrationState.Registrants.First();
                        tbYourFirstName.Text = firstRegistrant.GetFirstName( RegistrationTemplate );
                        tbYourLastName.Text = firstRegistrant.GetLastName( RegistrationTemplate );
                        tbConfirmationEmail.Text = firstRegistrant.GetEmail( RegistrationTemplate );
                    }
                    else
                    {
                        tbYourFirstName.Text = string.Empty;
                        tbYourLastName.Text = string.Empty;
                        tbConfirmationEmail.Text = string.Empty;
                    }
                }

                // Build Discount info
                nbDiscountCode.Visible = false;
                decimal discountPercent = 0.0m;
                decimal discountAmount = 0.0m;
                if ( RegistrationTemplate != null && RegistrationTemplate.Discounts.Any() )
                {
                    divDiscountCode.Visible = true;

                    string discountCode = RegistrationState.DiscountCode;
                    tbDiscountCode.Text = discountCode;
                    if ( !string.IsNullOrWhiteSpace( discountCode ))
                    {
                        var discount = RegistrationTemplate.Discounts
                            .Where( d => d.Code.Equals( discountCode, StringComparison.OrdinalIgnoreCase ) )
                            .FirstOrDefault();
                        if ( discount != null )
                        {
                            discountPercent = discount.DiscountPercentage;
                            discountAmount = discount.DiscountAmount;
                        }
                        else
                        {
                            nbDiscountCode.Text = string.Format( "Discount Code '{0}' is not a valid discount code.", discountCode );
                            nbDiscountCode.Visible = true;
                        }

                    }
                }
                else
                {
                    divDiscountCode.Visible = false;
                }

                // Get the cost/fee summary
                gFeeSummary.Columns[2].Visible = discountPercent > 0.0m;
                var costs = new List<CostSummaryInfo>();
                foreach( var registrant in RegistrationState.Registrants )
                {
                    if ( registrant.Cost > 0 )
                    {
                        var costSummary = new CostSummaryInfo();
                        costSummary.Type = CostSummaryType.Cost;
                        costSummary.Description = string.Format( "{0} {1}",
                            registrant.GetFirstName( RegistrationTemplate ),
                            registrant.GetLastName( RegistrationTemplate ) );
                        costSummary.Cost = registrant.Cost;
                        if ( !registrant.Existing && discountPercent > 0.0m )
                        {
                            costSummary.DiscountedCost = costSummary.Cost - ( costSummary.Cost * (decimal)discountPercent );
                        }
                        else
                        {
                            costSummary.DiscountedCost = costSummary.Cost;
                        }

                        // If registration allows a minimum payment calculate that amount, otherwise use the discounted amount as minimum
                        costSummary.MinPayment = RegistrationTemplate.MinimumInitialPayment != 0 ? 
                            RegistrationTemplate.MinimumInitialPayment : costSummary.DiscountedCost;

                        costs.Add( costSummary );
                    }

                    foreach( var fee in registrant.FeeValues )
                    {
                        var templateFee = RegistrationTemplate.Fees.Where( f => f.Id == fee.Key ).FirstOrDefault();
                        if ( fee.Value != null )
                        {
                            foreach ( var feeInfo in fee.Value )
                            {
                                decimal cost = feeInfo.PreviousCost > 0.0m ? feeInfo.PreviousCost : feeInfo.Cost;
                                string desc = string.Format( "{0}{1} ({2:N0} @ {3:C2})",
                                    templateFee != null ? templateFee.Name : "Previous Fee",
                                    string.IsNullOrWhiteSpace( feeInfo.Option ) ? "" : "-" + feeInfo.Option,
                                    feeInfo.Quantity,
                                    cost );

                                var costSummary = new CostSummaryInfo();
                                costSummary.Type = CostSummaryType.Fee;
                                costSummary.Description = desc;
                                costSummary.Cost = feeInfo.Quantity * cost;

                                if ( !registrant.Existing && discountPercent > 0.0m && templateFee != null && templateFee.DiscountApplies )
                                {
                                    costSummary.DiscountedCost = costSummary.Cost - ( costSummary.Cost * (decimal)discountPercent );
                                }
                                else
                                {
                                    costSummary.DiscountedCost = costSummary.Cost;
                                }

                                // Optional Fees are always included in minimum payment
                                costSummary.MinPayment = costSummary.DiscountedCost;

                                costs.Add( costSummary );
                            }
                        }
                    }
                }

                // If there were any costs
                if ( costs.Any() )
                {
                    pnlMoney.Visible = true;

                    // Get the total min payment for all costs and fees
                    decimal minPayment = costs.Sum( c => c.MinPayment );

                    // Add row for amount discount
                    if ( discountAmount > 0.0m )
                    {
                        costs.Add( new CostSummaryInfo
                        {
                            Type = CostSummaryType.Discount,
                            Description = "Discount",
                            Cost = 0.0m - discountAmount,
                            DiscountedCost = 0.0m - discountAmount
                        } );
                    }

                    // Get the total cost after discounts
                    RegistrationState.TotalCost = costs.Sum( c => c.DiscountedCost );

                    // If minimum payment is greater than total cost ( which is possible with discounts ), adjust the minimum payment
                    minPayment = minPayment > RegistrationState.TotalCost ? RegistrationState.TotalCost : minPayment;

                    // Add row for totals
                    costs.Add( new CostSummaryInfo
                    {
                        Type = CostSummaryType.Total,
                        Description = "Total",
                        Cost = costs.Sum( c => c.Cost ),
                        DiscountedCost = RegistrationState.TotalCost,
                    } );

                    // Bind the cost/fee summary grid
                    gFeeSummary.DataSource = costs;
                    gFeeSummary.DataBind();

                    // Set the total cost
                    hfTotalCost.Value = RegistrationState.TotalCost.ToString( "N2" );
                    lTotalCost.Text = RegistrationState.TotalCost.ToString( "C2" );

                    // Check for previous payments
                    lPreviouslyPaid.Visible = RegistrationState.PreviousPaymentTotal != 0.0m;
                    hfPreviouslyPaid.Value = RegistrationState.PreviousPaymentTotal.ToString( "N2" );
                    lPreviouslyPaid.Text = RegistrationState.PreviousPaymentTotal.ToString( "C2" );
                    minPayment = minPayment - RegistrationState.PreviousPaymentTotal;

                    // Calculate balance due, and if a partial payment is still allowed
                    decimal balanceDue = RegistrationState.TotalCost - RegistrationState.PreviousPaymentTotal;
                    bool allowPartialPayment = minPayment < balanceDue;

                    // If partial payment is allowed, show the minimum payment due
                    lMinimumDue.Visible = allowPartialPayment;
                    hfMinimumDue.Value = minPayment.ToString( "N2" );
                    lMinimumDue.Text = minPayment.ToString( "C2" );

                    // Make sure payment amount is at least as high as the minimum payment due
                    RegistrationState.PaymentAmount = RegistrationState.PaymentAmount < minPayment ? minPayment : RegistrationState.PaymentAmount;
                    nbAmountPaid.Visible = allowPartialPayment;
                    nbAmountPaid.Text = RegistrationState.PaymentAmount.ToString( "N2" );

                    // If a previous payment was made, or partial payment is allowed, show the amount remaining after selected payment amount
                    lRemainingDue.Visible = allowPartialPayment || RegistrationState.PreviousPaymentTotal != 0.0m;
                    lRemainingDue.Text = ( RegistrationState.TotalCost - ( RegistrationState.PreviousPaymentTotal + RegistrationState.PaymentAmount ) ).ToString( "C2" );

                    // Set payment options based on gateway settings
                    if ( RegistrationTemplate.FinancialGateway != null )
                    {
                        if ( RegistrationTemplate.FinancialGateway.Attributes == null )
                        {
                            RegistrationTemplate.LoadAttributes();
                        }

                        var component = RegistrationTemplate.FinancialGateway.GetGatewayComponent();
                        if ( component != null )
                        {
                            txtCardFirstName.Visible = component.SplitNameOnCard;
                            txtCardLastName.Visible = component.SplitNameOnCard;
                            txtCardName.Visible = !component.SplitNameOnCard;
                            mypExpiration.MinimumYear = RockDateTime.Now.Year;
                        }
                    }
                }
                else
                {
                    RegistrationState.TotalCost = 0.0m;
                    pnlMoney.Visible = false;
                }
            }
        }

        private void ParseSummaryControls()
        {
            if ( RegistrationState != null )
            {
                RegistrationState.YourFirstName = tbYourFirstName.Text;
                RegistrationState.YourLastName = tbYourLastName.Text;
                RegistrationState.ConfirmationEmail = tbConfirmationEmail.Text;
                RegistrationState.DiscountCode = tbDiscountCode.Text.Trim();
                RegistrationState.PaymentAmount = nbAmountPaid.Text.AsDecimal();
            }
        }

        #endregion

        #region Success Controls 

        private void CreateSuccessControls( bool setValues )
        {
            phSuccessControls.Controls.Clear();
        }

        #endregion

        #endregion

        #endregion

        #region Helper Classes

        /// <summary>
        /// Registration Helper Class
        /// </summary>
        [Serializable]
        public class RegistrationInfo
        {
            /// <summary>
            /// Gets or sets the registration identifier.
            /// </summary>
            /// <value>
            /// The registration identifier.
            /// </value>
            public int? RegistrationId { get; set; }

            /// <summary>
            /// Gets or sets the person alias identifier.
            /// </summary>
            /// <value>
            /// The person alias identifier.
            /// </value>
            public int? PersonAliasId { get; set; }

            /// <summary>
            /// Gets or sets your first name.
            /// </summary>
            /// <value>
            /// Your first name.
            /// </value>
            public string YourFirstName { get; set; }

            /// <summary>
            /// Gets or sets your last name.
            /// </summary>
            /// <value>
            /// Your last name.
            /// </value>
            public string YourLastName { get; set; }

            /// <summary>
            /// Gets or sets the confirmation email.
            /// </summary>
            /// <value>
            /// The confirmation email.
            /// </value>
            public string ConfirmationEmail { get; set; }

            /// <summary>
            /// Gets or sets the discount code.
            /// </summary>
            /// <value>
            /// The discount code.
            /// </value>
            public string DiscountCode { get; set; }

            public decimal DiscountAmount { get; set; }

            /// <summary>
            /// Gets or sets the total cost.
            /// </summary>
            /// <value>
            /// The total cost.
            /// </value>
            public decimal TotalCost { get; set; }

            /// <summary>
            /// Gets or sets the previous payment total.
            /// </summary>
            /// <value>
            /// The previous payment total.
            /// </value>
            public decimal PreviousPaymentTotal { get; set; }

            /// <summary>
            /// Gets or sets the payment amount.
            /// </summary>
            /// <value>
            /// The payment amount.
            /// </value>
            public decimal PaymentAmount { get; set; }

            /// <summary>
            /// Gets or sets the registrants.
            /// </summary>
            /// <value>
            /// The registrants.
            /// </value>
            public List<RegistrantInfo> Registrants { get; set; }

            /// <summary>
            /// Gets the registrant count.
            /// </summary>
            /// <value>
            /// The registrant count.
            /// </value>
            public int RegistrantCount
            {
                get { return Registrants.Count; }
            }

            /// <summary>
            /// Gets the existing registrants count.
            /// </summary>
            /// <value>
            /// The existing registrants count.
            /// </value>
            public int ExistingRegistrantsCount
            {
                get { return Registrants.Where( r => r.Existing ).Count(); }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="RegistrationInfo"/> class.
            /// </summary>
            public RegistrationInfo()
            {
                Registrants = new List<RegistrantInfo>();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="RegistrationInfo"/> class.
            /// </summary>
            /// <param name="person">The person.</param>
            public RegistrationInfo ( Person person ) : this()
            {
                PersonAliasId = person.PrimaryAliasId;
                YourFirstName = person.NickName;
                YourLastName = person.LastName;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="RegistrationInfo"/> class.
            /// </summary>
            /// <param name="registration">The registration.</param>
            public RegistrationInfo( Registration registration ) : this()
            {
                if ( registration != null )
                {
                    RegistrationId = registration.Id;
                    PersonAliasId = registration.PersonAliasId;
                    if ( registration.PersonAlias != null && registration.PersonAlias.Person != null )
                    {
                        YourFirstName = registration.PersonAlias.Person.NickName;
                        YourLastName = registration.PersonAlias.Person.LastName;
                    }

                    DiscountCode = registration.DiscountCode.Trim();

                    foreach ( var registrant in registration.Registrants )
                    {
                        Registrants.Add( new RegistrantInfo( registrant ) );
                    }
                }
            }

            /// <summary>
            /// Gets the options that should be available for additional registrants to specify the family they belong to
            /// </summary>
            /// <param name="template">The template.</param>
            /// <param name="currentRegistrantIndex">Index of the current registrant.</param>
            /// <returns></returns>
            public Dictionary<Guid, string> GetFamilyOptions( RegistrationTemplate template, int currentRegistrantIndex )
            {
                // Return a dictionary of family group guid, and the formated name (i.e. "Ted & Cindy Decker" )
                var result = new Dictionary<Guid, string>();

                // Get all the registrants prior to the current registrant
                var familyRegistrants = new Dictionary<Guid, List<RegistrantInfo>>();
                for ( int i = 0; i < currentRegistrantIndex; i++ )
                {
                    if ( Registrants != null && Registrants.Count > i )
                    {
                        var registrant = Registrants[i];
                        familyRegistrants.AddOrIgnore( registrant.FamilyGuid, new List<RegistrantInfo>() );
                        familyRegistrants[registrant.FamilyGuid].Add(registrant);
                    }
                    else
                    {
                        break;
                    }
                }

                // Loop through those registrants
                foreach( var keyVal in familyRegistrants )
                {
                    // Find all the people and group them by same last name
                    var lastNames = new Dictionary<string, List<string>>();
                    foreach( var registrant in keyVal.Value )
                    {
                        string firstName = registrant.GetFirstName( template );
                        string lastName = registrant.GetLastName( template );
                        lastNames.AddOrIgnore( lastName, new List<string>() );
                        lastNames[lastName].Add( firstName );
                    }

                    // Build a formated output for each unique last name
                    var familyNames = new List<string>();
                    foreach( var lastName in lastNames )
                    {
                        familyNames.Add( string.Format( "{0} {1}", lastName.Value.AsDelimited( " & "), lastName.Key ) );
                    }

                    // Join each of the formated values for each unique last name for the current family
                    result.Add( keyVal.Key, familyNames.AsDelimited( " and ") );
                }

                return result;
            }

        }

        /// <summary>
        /// Registrant Helper Class
        /// </summary>
        [Serializable]
        public class RegistrantInfo
        {
            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            /// <value>
            /// The unique identifier.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the person alias unique identifier.
            /// </summary>
            /// <value>
            /// The person alias unique identifier.
            /// </value>
            public Guid PersonAliasGuid { get; set; }

            /// <summary>
            /// Gets or sets the family unique identifier.
            /// </summary>
            /// <value>
            /// The family unique identifier.
            /// </value>
            public Guid FamilyGuid { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="RegistrantInfo"/> is existing.
            /// </summary>
            /// <value>
            ///   <c>true</c> if existing; otherwise, <c>false</c>.
            /// </value>
            public bool Existing { get; set; }

            /// <summary>
            /// Gets or sets the cost.
            /// </summary>
            /// <value>
            /// The cost.
            /// </value>
            public decimal Cost { get; set; }

            /// <summary>
            /// Gets or sets the field values.
            /// </summary>
            /// <value>
            /// The field values.
            /// </value>
            public Dictionary<int, object> FieldValues { get; set; }

            /// <summary>
            /// Gets or sets the fee values.
            /// </summary>
            /// <value>
            /// The fee values.
            /// </value>
            public Dictionary<int, List<FeeInfo>> FeeValues { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="RegistrantInfo"/> class.
            /// </summary>
            public RegistrantInfo()
            {
                Guid = Guid.NewGuid();
                PersonAliasGuid = Guid.Empty;
                FamilyGuid = Guid.NewGuid();
                Existing = false;
                FieldValues = new Dictionary<int, object>();
                FeeValues = new Dictionary<int, List<FeeInfo>>();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="RegistrantInfo"/> class.
            /// </summary>
            /// <param name="registrant">The registrant.</param>
            public RegistrantInfo( RegistrationRegistrant registrant ) : this()
            {
                if ( registrant != null )
                {
                    Guid = registrant.Guid;
                    Existing = registrant.Id > 0;
                    Cost = registrant.Cost;

                    if ( registrant.PersonAlias != null )
                    {
                        PersonAliasGuid = registrant.PersonAlias.Guid;
                    }

                    if ( registrant.Registration != null &&
                        registrant.Registration.RegistrationInstance != null &&
                        registrant.Registration.RegistrationInstance.RegistrationTemplate != null )
                    {
                        foreach ( var field in registrant.Registration.RegistrationInstance.RegistrationTemplate.Forms
                            .SelectMany( f => f.Fields ) )
                        {
                            if ( field.ShowCurrentValue )
                            {
                                object dbValue = GetRegistrantValue( registrant, field );
                                if ( dbValue != null )
                                {
                                    FieldValues.Add( field.Id, dbValue );
                                }
                            }
                        }

                        foreach( var fee in registrant.Fees )
                        {
                            FeeValues.AddOrIgnore( fee.RegistrationTemplateFeeId, new List<FeeInfo>() );
                            FeeValues[fee.RegistrationTemplateFeeId].Add( new FeeInfo( fee ) );
                        }
                    }
                }
            }

            /// <summary>
            /// Gets the existing value for a specific field for the given registrant.
            /// </summary>
            /// <param name="registrant">The registrant.</param>
            /// <param name="Field">The field.</param>
            /// <returns></returns>
            public object GetRegistrantValue( RegistrationRegistrant registrant, RegistrationTemplateFormField Field)
            {
                if ( Field.FieldSource == RegistrationFieldSource.PersonField )
                {
                    if ( registrant.PersonAlias != null && registrant.PersonAlias.Person != null )
                    {
                        var person = registrant.PersonAlias.Person;
                        switch( Field.PersonFieldType )
                        {
                            case RegistrationPersonFieldType.Birthdate: return person.BirthDate;
                            case RegistrationPersonFieldType.Email: return person.Email;
                            case RegistrationPersonFieldType.FirstName: return person.NickName;
                            case RegistrationPersonFieldType.Gender: return person.Gender;
                            case RegistrationPersonFieldType.HomeCampus: return null;
                            case RegistrationPersonFieldType.LastName: return person.LastName;
                            case RegistrationPersonFieldType.MaritalStatus: return person.MaritalStatusValueId;
                            case RegistrationPersonFieldType.Phone: return null;
                        }
                    }
                }
                else
                {
                    var attribute = AttributeCache.Read( Field.AttributeId ?? 0 );
                    if ( attribute != null )
                    {
                        switch ( Field.FieldSource )
                        {
                            case RegistrationFieldSource.PersonAttribute:
                                {
                                    if ( registrant.PersonAlias != null && registrant.PersonAlias.Person != null )
                                    {
                                        var person = registrant.PersonAlias.Person;
                                        if ( person.Attributes == null )
                                        {
                                            person.LoadAttributes();
                                        }
                                        return person.GetAttributeValue( attribute.Key );
                                    }
                                    break;
                                }

                            case RegistrationFieldSource.GroupMemberAttribute:
                                {
                                    if ( registrant.GroupMember != null )
                                    {
                                        if ( registrant.GroupMember.Attributes == null )
                                        {
                                            registrant.GroupMember.LoadAttributes();
                                        }
                                        return registrant.GroupMember.GetAttributeValue( attribute.Key );
                                    }
                                    break;
                                }

                            case RegistrationFieldSource.RegistrationAttribute:
                                {
                                    if ( registrant.Attributes == null )
                                    {
                                        registrant.LoadAttributes();
                                    }
                                    return registrant.GetAttributeValue( attribute.Key );
                                }
                        }
                    }
                }

                return null;
            }

            /// <summary>
            /// Gets the first name.
            /// </summary>
            /// <param name="template">The template.</param>
            /// <returns></returns>
            public string GetFirstName( RegistrationTemplate template )
            {
                object value = GetPersonFieldValue( template, RegistrationPersonFieldType.FirstName );
                return value != null ? value.ToString() : string.Empty;
            }

            /// <summary>
            /// Gets the last name.
            /// </summary>
            /// <param name="template">The template.</param>
            /// <returns></returns>
            public string GetLastName( RegistrationTemplate template )
            {
                object value = GetPersonFieldValue( template, RegistrationPersonFieldType.LastName );
                return value != null ? value.ToString() : string.Empty;
            }

            /// <summary>
            /// Gets the email.
            /// </summary>
            /// <param name="template">The template.</param>
            /// <returns></returns>
            public string GetEmail( RegistrationTemplate template )
            {
                object value = GetPersonFieldValue( template, RegistrationPersonFieldType.Email );
                return value != null ? value.ToString() : string.Empty;
            }

            /// <summary>
            /// Gets a person field value.
            /// </summary>
            /// <param name="template">The template.</param>
            /// <param name="personFieldType">Type of the person field.</param>
            /// <returns></returns>
            public object GetPersonFieldValue( RegistrationTemplate template, RegistrationPersonFieldType personFieldType )
            {
                if ( template != null && template.Forms != null )
                {
                    var fieldId = template.Forms
                        .SelectMany( t => t.Fields
                            .Where( f =>
                                f.FieldSource == RegistrationFieldSource.PersonField &&
                                f.PersonFieldType == personFieldType )
                            .Select( f => f.Id ) )
                        .FirstOrDefault();

                    return FieldValues.ContainsKey( fieldId ) ? FieldValues[fieldId] : null;
                }

                return null;
            }
        }

        /// <summary>
        /// Registrant  Fee Helper Class
        /// </summary>
        [Serializable]
        public class FeeInfo
        {
            /// <summary>
            /// Gets or sets the option.
            /// </summary>
            /// <value>
            /// The option.
            /// </value>
            public string Option { get; set; }

            /// <summary>
            /// Gets or sets the quantity.
            /// </summary>
            /// <value>
            /// The quantity.
            /// </value>
            public int Quantity { get; set; }

            /// <summary>
            /// Gets or sets the cost.
            /// </summary>
            /// <value>
            /// The cost.
            /// </value>
            public decimal Cost { get; set; }

            /// <summary>
            /// Gets or sets the previous cost.
            /// </summary>
            /// <value>
            /// The previous cost.
            /// </value>
            public decimal PreviousCost { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="FeeInfo"/> class.
            /// </summary>
            public FeeInfo()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="FeeInfo"/> class.
            /// </summary>
            /// <param name="option">The option.</param>
            /// <param name="quantity">The quantity.</param>
            /// <param name="cost">The cost.</param>
            public FeeInfo( string option, int quantity, decimal cost )
                : this()
            {
                Option = option;
                Quantity = quantity;
                Cost = cost;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="FeeInfo"/> class.
            /// </summary>
            /// <param name="fee">The fee.</param>
            public FeeInfo( RegistrationRegistrantFee fee )
                : this()
            {
                Option = fee.Option;
                Quantity = fee.Quantity;
                Cost = fee.Cost;
                PreviousCost = fee.Cost;
            }
        }

        /// <summary>
        /// Helper class for creating summary of cost/fee information to bind to summary grid
        /// </summary>
        public class CostSummaryInfo
        {
            /// <summary>
            /// Gets or sets the type.
            /// </summary>
            /// <value>
            /// The type.
            /// </value>
            public CostSummaryType Type { get; set; }

            /// <summary>
            /// Gets or sets the description.
            /// </summary>
            /// <value>
            /// The description.
            /// </value>
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets the cost.
            /// </summary>
            /// <value>
            /// The cost.
            /// </value>
            public decimal Cost { get; set; }

            /// <summary>
            /// Gets or sets the discounted cost.
            /// </summary>
            /// <value>
            /// The discounted cost.
            /// </value>
            public decimal DiscountedCost { get; set; }

            /// <summary>
            /// Gets or sets the minimum payment.
            /// </summary>
            /// <value>
            /// The minimum payment.
            /// </value>
            public decimal MinPayment { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public enum CostSummaryType
        {
            /// <summary>
            /// a cost
            /// 
            /// </summary>
            Cost,

            /// <summary>
            /// a fee
            /// </summary>
            Fee,

            /// <summary>
            /// The discount
            /// </summary>
            Discount,

            /// <summary>
            /// The total
            /// </summary>
            Total
        }

        #endregion
}
}