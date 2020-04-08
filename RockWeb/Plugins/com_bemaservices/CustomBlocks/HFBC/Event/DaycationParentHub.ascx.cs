// <copyright>
// Copyright by BEMA Information Technologies
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
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_bemaservices.Event
{
    /// <summary>
    /// Generates a list of scheduled transactions for the current person with edit/transfer and delete buttons.
    /// </summary>
    [DisplayName( "Daycation Parent Hub" )]
    [Category( "BEMA Services > Finance" )]
    [Description( "Block that shows a list of scheduled transactions for the currently logged in user with the ability to modify the formatting using a lava template." )]


    [FinancialGatewayField(
        "Gateway Filter",
        Key = AttributeKey.GatewayFilter,
        Description = "When set, causes only scheduled transaction's of a particular gateway to be shown.",
        IsRequired = false,
        Order = 1 )]

    [BooleanField( "Show Event Registration Payments", order: 2 )]
    [IntegerField( "Category Id", order: 3 )]
    [SystemEmailField( "Registration Deleted Notification", "", false, "", "", 4, "RegistrationDeletedNotification" )]

    [CodeEditorField( "Scheduled Transaction Template",
        Key = AttributeKey.Template,
        Description = "Lava template for the display of the scheduled transactions.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = @"{% include '~/Plugins/com_bemaservices/Event/Assets/Lava/DaycationScheduledTransactions.lava'  %}",
        Order = 5 )]

    [CodeEditorField( "Registrant Template",
        Key = AttributeKey.RegistrantTemplate,
        Description = "Lava template for the display of the scheduled transactions.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = @"{% include '~/Plugins/com_bemaservices/Event/Assets/Lava/DaycationRegistrants.lava'  %}",
        Order = 6 )]

    [CodeEditorField( "Transaction Template",
        Key = AttributeKey.TransactionTemplate,
        Description = "Lava template for the display of the scheduled transactions.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = @"{% include '~/Plugins/com_bemaservices/Event/Assets/Lava/DaycationTransactions.lava'  %}",
        Order = 7 )]


    public partial class DaycationParentHub : RockBlock
    {
        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        protected static class AttributeKey
        {
            public const string Template = "Template";
            public const string TransactionTemplate = "TransactionTemplate";
            public const string RegistrantTemplate = "RegistrantTemplate";
            public const string GatewayFilter = "GatewayFilter";
        }

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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // set initial info
            if ( !IsPostBack )
            {
                ShowContent();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowContent();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptScheduledTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptScheduledTransactions_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var transactionSchedule = e.Item.DataItem as FinancialScheduledTransaction;

                BootstrapButton bbtnDelete = ( BootstrapButton ) e.Item.FindControl( "bbtnDelete" );
                HiddenField hfScheduledTransactionId = ( HiddenField ) e.Item.FindControl( "hfScheduledTransactionId" );
                hfScheduledTransactionId.Value = transactionSchedule.Id.ToString();

                // create dictionary for liquid
                Dictionary<string, object> scheduleSummary = new Dictionary<string, object>();
                scheduleSummary.Add( "Id", transactionSchedule.Id );
                scheduleSummary.Add( "Guid", transactionSchedule.Guid );
                scheduleSummary.Add( "StartDate", transactionSchedule.StartDate );
                scheduleSummary.Add( "EndDate", transactionSchedule.EndDate );
                scheduleSummary.Add( "NextPaymentDate", transactionSchedule.NextPaymentDate );

                if ( transactionSchedule.NextPaymentDate.HasValue )
                {
                    scheduleSummary.Add( "DaysTillNextPayment", ( transactionSchedule.NextPaymentDate.Value - DateTime.Now ).Days );
                }
                else
                {
                    scheduleSummary.Add( "DaysTillNextPayment", null );
                }

                DateTime? lastPaymentDate = transactionSchedule.Transactions.Max( t => t.TransactionDateTime );
                scheduleSummary.Add( "LastPaymentDate", lastPaymentDate );

                if ( lastPaymentDate.HasValue )
                {
                    scheduleSummary.Add( "DaysSinceLastPayment", ( DateTime.Now - lastPaymentDate.Value ).Days );
                }
                else
                {
                    scheduleSummary.Add( "DaysSinceLastPayment", null );
                }

                scheduleSummary.Add( "PersonName", transactionSchedule.AuthorizedPersonAlias != null && transactionSchedule.AuthorizedPersonAlias.Person != null ? transactionSchedule.AuthorizedPersonAlias.Person.FullName : string.Empty );
                scheduleSummary.Add( "CurrencyType", ( transactionSchedule.FinancialPaymentDetail != null && transactionSchedule.FinancialPaymentDetail.CurrencyTypeValue != null ) ? transactionSchedule.FinancialPaymentDetail.CurrencyTypeValue.Value : string.Empty );
                scheduleSummary.Add( "CreditCardType", ( transactionSchedule.FinancialPaymentDetail != null && transactionSchedule.FinancialPaymentDetail.CreditCardTypeValue != null ) ? transactionSchedule.FinancialPaymentDetail.CreditCardTypeValue.Value : string.Empty );
                scheduleSummary.Add( "AccountNumberMasked", ( transactionSchedule.FinancialPaymentDetail != null ) ? transactionSchedule.FinancialPaymentDetail.AccountNumberMasked ?? string.Empty : string.Empty );
                scheduleSummary.Add( "UrlEncryptedKey", transactionSchedule.UrlEncodedKey );
                scheduleSummary.Add( "Frequency", transactionSchedule.TransactionFrequencyValue.Value );
                scheduleSummary.Add( "FrequencyDescription", transactionSchedule.TransactionFrequencyValue.Description );

                var entityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Registration ) ).Id;
                var registrationId = transactionSchedule.ScheduledTransactionDetails.Where( std => std.EntityTypeId == entityTypeId ).Max( std => std.EntityId );
                if ( registrationId != null )
                {
                    var registration = new RegistrationService( new RockContext() ).Get( registrationId.Value );
                    if ( registration != null )
                    {
                        scheduleSummary.Add( "Registration", registration );
                        bbtnDelete.Text = "Cancel Registration";
                    }

                }

                List<Dictionary<string, object>> summaryDetails = new List<Dictionary<string, object>>();
                decimal totalAmount = 0;

                foreach ( FinancialScheduledTransactionDetail detail in transactionSchedule.ScheduledTransactionDetails )
                {
                    Dictionary<string, object> detailSummary = new Dictionary<string, object>();
                    detailSummary.Add( "AccountId", detail.Id );
                    detailSummary.Add( "AccountName", detail.Account.Name );
                    detailSummary.Add( "Amount", detail.Amount );
                    detailSummary.Add( "Summary", detail.Summary );

                    summaryDetails.Add( detailSummary );

                    totalAmount += detail.Amount;
                }

                scheduleSummary.Add( "ScheduledAmount", totalAmount );
                scheduleSummary.Add( "TransactionDetails", summaryDetails );

                Dictionary<string, object> schedule = new Dictionary<string, object>();
                schedule.Add( "ScheduledTransaction", scheduleSummary );

                // merge into content
                Literal lScheduledContent = ( Literal ) e.Item.FindControl( "lScheduledContent" );
                lScheduledContent.Text = GetAttributeValue( AttributeKey.Template ).ResolveMergeFields( schedule );


            }
        }

        /// <summary>
        /// Handles the Click event of the bbtnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bbtnDelete_Click( object sender, EventArgs e )
        {
            BootstrapButton bbtnDelete = ( BootstrapButton ) sender;
            RepeaterItem riItem = ( RepeaterItem ) bbtnDelete.NamingContainer;

            HiddenField hfScheduledTransactionId = ( HiddenField ) riItem.FindControl( "hfScheduledTransactionId" );
            Literal content = ( Literal ) riItem.FindControl( "lScheduledContent" );

            using ( var rockContext = new Rock.Data.RockContext() )
            {
                FinancialScheduledTransactionService fstService = new FinancialScheduledTransactionService( rockContext );
                RegistrationService registrationService = new RegistrationService( rockContext );
                var currentTransaction = fstService.Get( hfScheduledTransactionId.Value.AsInteger() );
                if ( currentTransaction != null && currentTransaction.FinancialGateway != null )
                {
                    currentTransaction.FinancialGateway.LoadAttributes( rockContext );
                }

                string errorMessage = string.Empty;
                if ( fstService.Cancel( currentTransaction, out errorMessage ) )
                {
                    try
                    {
                        fstService.GetStatus( currentTransaction, out errorMessage );
                    }
                    catch
                    {
                        // Ignore
                    }

                    rockContext.SaveChanges();
                    content.Text = "<div class='alert alert-success'>Your upcoming payment has been deleted.</div>";

                    var entityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Registration ) ).Id;
                    var registrationId = currentTransaction.ScheduledTransactionDetails.Where( std => std.EntityTypeId == entityTypeId ).Max( std => std.EntityId );
                    if ( registrationId != null )
                    {
                        var registration = registrationService.Get( registrationId.Value );
                        if ( registration != null )
                        {
                            if ( registration != null )
                            {
                                int registrationInstanceId = registration.RegistrationInstanceId;


                                var changes = new History.HistoryChangeList();
                                changes.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Registration" );

                                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                                mergeFields.Add( "RegistrationInstance", registration.RegistrationInstance.Name );
                                mergeFields.Add( "RegistrationTemplate", registration.RegistrationInstance.RegistrationTemplate.Name );
                                mergeFields.Add( "Registrants", registration.Registrants.Select( r => r.PersonAlias.Person.FullName ).ToList() );

                                var emailMessage = new RockEmailMessage( GetAttributeValue( "RegistrationDeletedNotification" ).AsGuid() );
                                emailMessage.AddRecipient( new RecipientData( "Daycation@houstonsfirst.org", mergeFields ) );
                                emailMessage.AppRoot = ResolveRockUrl( "~/" );
                                emailMessage.ThemeRoot = ResolveRockUrl( "~~/" );
                                emailMessage.CreateCommunicationRecord = false;
                                emailMessage.Send();

                                rockContext.WrapTransaction( () =>
                                {
                                    HistoryService.SaveChanges(
                                        rockContext,
                                        typeof( Registration ),
                                        Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                                        registration.Id,
                                        changes );

                                    registrationService.Delete( registration );
                                    rockContext.SaveChanges();
                                    content.Text = "<div class='alert alert-success'>Your registration and related scheduled payment has been deleted.</div>";

                                } );
                            }
                        }

                    }
                }
                else
                {
                    content.Text = string.Format( "<div class='alert alert-danger'>An error occurred while deleting your scheduled transaction. Message: {0}</div>", errorMessage );
                }
            }

            bbtnDelete.Visible = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the content.
        /// </summary>
        private void ShowContent()
        {
            // get scheduled contributions for current user
            if ( CurrentPerson != null )
            {
                var rockContext = new RockContext();
                var personService = new PersonService( rockContext );

                // get giving id
                var givingIds = personService.GetBusinesses( CurrentPerson.Id ).Select( g => g.GivingId ).ToList();
                givingIds.Add( CurrentPerson.GivingId );

                BuildUpcomingPayments( rockContext, givingIds );
                BuildPastPayments( rockContext, givingIds );
                BuildRegistrants( rockContext, personService );

            }
        }

        private void BuildRegistrants( RockContext rockContext, PersonService personService )
        {
            var registrationRegistrantService = new RegistrationRegistrantService( rockContext );
            var categoryId = GetAttributeValue( "CategoryId" ).AsInteger();
            var familyMemberPersonIds = CurrentPerson.GetFamilyMembers().Select( fm => fm.PersonId ).ToList();
            var registrationRegistrantPersonIds = registrationRegistrantService.Queryable().AsNoTracking()
                    .Where( rr => familyMemberPersonIds.Contains( rr.PersonAlias.PersonId ) )
                    .Where( rr => rr.Registration.RegistrationInstance.RegistrationTemplate.CategoryId == categoryId )
                    .Where( rr => rr.Registration.RegistrationInstance.RegistrationTemplate.Name.Contains( "Admin" ) )
                    .Select( rr => rr.PersonAlias.PersonId )
                    .ToList();

            var registeredPeople = personService.GetByIds( registrationRegistrantPersonIds );
            rptRegistrants.DataSource = registeredPeople.ToList();
            rptRegistrants.DataBind();


            if ( registeredPeople.Count() == 0 )
            {
                pnlNoRegistrants.Visible = true;
                lNoRegistrantsMessage.Text = "No registrants currently exist.";
            }
        }

        private void BuildPastPayments( RockContext rockContext, List<string> givingIds )
        {
            var transactionService = new FinancialTransactionService( rockContext );
            var registrationService = new RegistrationService( rockContext );
            var txnType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION ) );
            var showEventRegistrationPayments = GetAttributeValue( "ShowEventRegistrationPayments" ).AsBoolean();

            var transactions = transactionService.Queryable()
                .Include( a => a.TransactionDetails.Select( s => s.Account ) )
                .Where( s => givingIds.Contains( s.AuthorizedPersonAlias.Person.GivingId ) )
                .Where( s => ( s.TransactionTypeValue.Guid == txnType.Guid ) == showEventRegistrationPayments );

            var entityIds = transactions.SelectMany( t => t.TransactionDetails.Select( td => td.EntityId ) ).ToList();
            var registrationIds = registrationService.Queryable().AsNoTracking().Where( r => entityIds.Contains( r.Id ) ).Select( r => r.Id ).ToList();

            transactions = transactions.Where( t => t.TransactionDetails.Any( td => td.EntityId.HasValue && registrationIds.Contains( td.EntityId.Value ) ) );

            // filter the list if necessary
            var gatewayFilterGuid = GetAttributeValue( AttributeKey.GatewayFilter ).AsGuidOrNull();
            if ( gatewayFilterGuid != null )
            {
                transactions = transactions.Where( s => s.FinancialGateway.Guid == gatewayFilterGuid );
            }

            rptTransactions.DataSource = transactions.OrderByDescending( t => t.TransactionDateTime ).ToList();
            rptTransactions.DataBind();

            if ( transactions.Count() == 0 )
            {
                pnlNoTransactions.Visible = true;
                lNoTransactionsMessage.Text = "No past payments currently exist.";
            }
        }

        private void BuildUpcomingPayments( RockContext rockContext, List<string> givingIds )
        {
            var scheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
            var txnType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION ) );
            var showEventRegistrationPayments = GetAttributeValue( "ShowEventRegistrationPayments" ).AsBoolean();

            var schedules = scheduledTransactionService.Queryable()
                .Include( a => a.ScheduledTransactionDetails.Select( s => s.Account ) )
                .Where( s => givingIds.Contains( s.AuthorizedPersonAlias.Person.GivingId ) && s.IsActive == true )
                .Where( s => ( s.TransactionTypeValue.Guid == txnType.Guid ) == showEventRegistrationPayments );

            // filter the list if necessary
            var gatewayFilterGuid = GetAttributeValue( AttributeKey.GatewayFilter ).AsGuidOrNull();
            if ( gatewayFilterGuid != null )
            {
                schedules = schedules.Where( s => s.FinancialGateway.Guid == gatewayFilterGuid );
            }

            rptScheduledTransactions.DataSource = schedules.ToList();
            rptScheduledTransactions.DataBind();

            if ( schedules.Count() == 0 )
            {
                pnlNoScheduledTransactions.Visible = true;
                lNoScheduledTransactionsMessage.Text = "No upcoming payments currently exist.";
            }
        }

        #endregion

        protected void rptTransactions_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var transaction = e.Item.DataItem as FinancialTransaction;

                HiddenField hfTransactionId = ( HiddenField ) e.Item.FindControl( "hfTransactionId" );
                hfTransactionId.Value = transaction.Id.ToString();

                // create dictionary for liquid
                Dictionary<string, object> transactionSummary = new Dictionary<string, object>();
                transactionSummary.Add( "Id", transaction.Id );
                transactionSummary.Add( "Guid", transaction.Guid );
                transactionSummary.Add( "PaymentDate", transaction.TransactionDateTime );
                transactionSummary.Add( "PersonName", transaction.AuthorizedPersonAlias != null && transaction.AuthorizedPersonAlias.Person != null ? transaction.AuthorizedPersonAlias.Person.FullName : string.Empty );
                transactionSummary.Add( "CurrencyType", ( transaction.FinancialPaymentDetail != null && transaction.FinancialPaymentDetail.CurrencyTypeValue != null ) ? transaction.FinancialPaymentDetail.CurrencyTypeValue.Value : string.Empty );
                transactionSummary.Add( "CreditCardType", ( transaction.FinancialPaymentDetail != null && transaction.FinancialPaymentDetail.CreditCardTypeValue != null ) ? transaction.FinancialPaymentDetail.CreditCardTypeValue.Value : string.Empty );
                transactionSummary.Add( "AccountNumberMasked", ( transaction.FinancialPaymentDetail != null ) ? transaction.FinancialPaymentDetail.AccountNumberMasked ?? string.Empty : string.Empty );
                transactionSummary.Add( "UrlEncryptedKey", transaction.UrlEncodedKey );

                List<Dictionary<string, object>> summaryDetails = new List<Dictionary<string, object>>();
                decimal totalAmount = 0;
                var entityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Registration ) ).Id;

                foreach ( FinancialTransactionDetail detail in transaction.TransactionDetails )
                {
                    Dictionary<string, object> detailSummary = new Dictionary<string, object>();
                    detailSummary.Add( "AccountId", detail.Id );
                    detailSummary.Add( "AccountName", detail.Account.Name );
                    detailSummary.Add( "Amount", detail.Amount );
                    detailSummary.Add( "Summary", detail.Summary );
                    if ( detail.EntityId != null )
                    {
                        var registration = new RegistrationService( new RockContext() ).Get( detail.EntityId.Value );
                        if ( registration != null )
                        {
                            detailSummary.Add( "Registration", registration );
                            summaryDetails.Add( detailSummary );

                            totalAmount += detail.Amount;
                        }
                    }
                }

                if ( totalAmount > 0 )
                {
                    transactionSummary.Add( "TotalAmount", totalAmount );
                    transactionSummary.Add( "TransactionDetails", summaryDetails );

                    Dictionary<string, object> transactionLava = new Dictionary<string, object>();
                    transactionLava.Add( "Transaction", transactionSummary );

                    // merge into content
                    Literal lTransactionContent = ( Literal ) e.Item.FindControl( "lTransactionContent" );
                    lTransactionContent.Text = GetAttributeValue( AttributeKey.TransactionTemplate ).ResolveMergeFields( transactionLava );
                }
            }
        }

        protected void rptRegistrants_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var person = e.Item.DataItem as Person;
                var registrationRegistrantService = new RegistrationRegistrantService( new RockContext() );
                var categoryId = GetAttributeValue( "CategoryId" ).AsInteger();

                HiddenField hfPersonId = ( HiddenField ) e.Item.FindControl( "hfPersonId" );
                hfPersonId.Value = person.Id.ToString();

                // create dictionary for liquid
                Dictionary<string, object> personSummary = new Dictionary<string, object>();
                personSummary.Add( "Id", person.Id );
                personSummary.Add( "Guid", person.Guid );
                personSummary.Add( "PersonName", person.FullName );

                var registrationRegistrants = registrationRegistrantService.Queryable().AsNoTracking()
                        .Where( rr => rr.PersonAlias.PersonId == person.Id )
                        .Where( rr => rr.Registration.RegistrationInstance.RegistrationTemplate.CategoryId == categoryId )
                        .ToList();

                personSummary.Add( "AdminFeeRegistration", registrationRegistrants.Where( rr => rr.Registration.RegistrationInstance.RegistrationTemplate.Name.Contains( "Admin" ) ).FirstOrDefault() );
                personSummary.Add( "Registrants", registrationRegistrants
                                                        .Where( rr => !rr.Registration.RegistrationInstance.RegistrationTemplate.Name.Contains( "Admin" ) )
                                                        .OrderBy( rr => rr.Registration.RegistrationInstance.EndDateTime )
                                                        .ToList() );

                Dictionary<string, object> personLava = new Dictionary<string, object>();

                personLava.Add( "Person", personSummary );

                // merge into content
                Literal lRegistrantContent = ( Literal ) e.Item.FindControl( "lRegistrantContent" );
                lRegistrantContent.Text = GetAttributeValue( AttributeKey.RegistrantTemplate ).ResolveMergeFields( personLava );

            }
        }
    }
}