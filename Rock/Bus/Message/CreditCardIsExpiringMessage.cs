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

using Rock.Bus.Queue;
using Rock.Configuration;
using Rock.Logging;
using Rock.Model;

using static Rock.Bus.Message.CreditCardIsExpiringMessage;

namespace Rock.Bus.Message
{
    /// <summary>
    /// Credit Card Is Expiring Message.
    /// Sends queues for member credit cards that are close to expiration when running the <see cref="Rock.Jobs.SendCreditCardExpirationNotices"/> job
    /// </summary>
    public interface ICreditCardIsExpiringMessage : IEventMessage<ExpiringCardEventQueue>
    {
        /// <summary>
        /// Gets or sets the account number masked.
        /// </summary>
        /// <value>
        /// The account number masked.
        /// </value>
        string AccountNumberMasked { get; set; }

        /// <summary>
        /// Gets or sets the card expiration date.
        /// </summary>
        /// <value>
        /// The card expiration date.
        /// </value>
        DateTime? CardExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        /// <value>
        /// The expiration date.
        /// </value>
        string ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the expiration month.
        /// </summary>
        /// <value>
        /// The expiration month.
        /// </value>
        int? ExpirationMonth { get; set; }

        /// <summary>
        /// Gets or sets the expiration year.
        /// </summary>
        /// <value>
        /// The expiration year.
        /// </value>
        int? ExpirationYear { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        long Id { get; set; }

        /// <summary>
        /// Gets or sets the name on card.
        /// </summary>
        /// <value>
        /// The name on card.
        /// </value>
        string NameOnCard { get; set; }

        /// <summary>
        /// Gets or sets the credit card type value.
        /// </summary>
        /// <value>
        /// The credit card type value.
        /// </value>
        string CreditCardTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the name of the financial person saved account.
        /// </summary>
        /// <value>
        /// The name of the financial person saved account.
        /// </value>
        string FinancialPersonSavedAccountName { get; set; }

        /// <summary>
        /// Gets or sets the financial scheduled transactions.
        /// </summary>
        /// <value>
        /// The financial scheduled transactions.
        /// </value>
        List<int> FinancialScheduledTransactions { get; set; }

        /// <summary>
        /// Gets or sets the billing location.
        /// </summary>
        /// <value>
        /// The billing location.
        /// </value>
        string BillingLocation { get; set; }

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        CardHolder Person { get; set; }
    }

    /// <summary>
    /// Credit Card Is Expiring Message.
    /// Sends queues for member credit cards that are close to expiration when running the <see cref="Rock.Jobs.SendCreditCardExpirationNotices"/> job
    /// </summary>
    public class CreditCardIsExpiringMessage : ICreditCardIsExpiringMessage
    {
        /// <summary>
        /// Gets or sets the sender rock instance unique identifier.
        /// </summary>
        /// <value>
        /// The sender rock instance unique identifier.
        /// </value>
        public string SenderNodeName { get; set; }

        /// <summary>
        /// Gets or sets the account number masked.
        /// </summary>
        /// <value>
        /// The account number masked.
        /// </value>
        public string AccountNumberMasked { get; set; }

        /// <summary>
        /// Gets or sets the card expiration date.
        /// </summary>
        /// <value>
        /// The card expiration date.
        /// </value>
        public DateTime? CardExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        /// <value>
        /// The expiration date.
        /// </value>
        public string ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the expiration month.
        /// </summary>
        /// <value>
        /// The expiration month.
        /// </value>
        public int? ExpirationMonth { get; set; }

        /// <summary>
        /// Gets or sets the expiration year.
        /// </summary>
        /// <value>
        /// The expiration year.
        /// </value>
        public int? ExpirationYear { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name on card.
        /// </summary>
        /// <value>
        /// The name on card.
        /// </value>
        public string NameOnCard { get; set; }

        /// <summary>
        /// Gets or sets the credit card type value.
        /// </summary>
        /// <value>
        /// The credit card type value.
        /// </value>
        public string CreditCardTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the name of the financial person saved account.
        /// </summary>
        /// <value>
        /// The name of the financial person saved account.
        /// </value>
        public string FinancialPersonSavedAccountName { get; set; }

        /// <summary>
        /// Gets or sets the financial scheduled transactions.
        /// </summary>
        /// <value>
        /// The financial scheduled transactions.
        /// </value>
        public List<int> FinancialScheduledTransactions { get; set; }

        /// <summary>
        /// Gets or sets the billing location.
        /// </summary>
        /// <value>
        /// The billing location.
        /// </value>
        public string BillingLocation { get; set; }

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        public CardHolder Person { get; set; }

        /// <summary>
        /// Publishes the expiring card and cardholder details
        /// </summary>
        /// <param name="person"></param>
        /// <param name="financialPaymentDetail"></param>
        /// <param name="financialScheduledTransactions"></param>
        public static void Publish( Person person, FinancialPaymentDetail financialPaymentDetail, List<int> financialScheduledTransactions)
        {
            if ( !RockMessageBus.IsRockStarted )
            {
                // Don't publish events until Rock is all the way started
                const string logMessage = "'Credit Card Is Expiring Message' message was not published because Rock is not fully started yet.";

                var elapsedSinceProcessStarted = RockDateTime.Now - RockApp.Current.HostingSettings.ApplicationStartDateTime;

                if ( elapsedSinceProcessStarted.TotalSeconds > RockMessageBus.MAX_SECONDS_SINCE_STARTTIME_LOG_ERROR )
                {
                    RockLogger.Log.Error( RockLogDomains.Bus, logMessage );
                    ExceptionLogService.LogException( new BusException(logMessage ) );
                }
                else
                {
                    RockLogger.Log.Debug( RockLogDomains.Bus, logMessage );
                }

                return;
            }

            var cardHolder = new CardHolder()
            {
                CommunicationPreference = (int)person.CommunicationPreference,
                Email = person.Email,
                FirstName = person.FirstName,
                Id = person.Id,
                LastName = person.LastName,
                MiddleName = person.MiddleName,
                NickName = person.NickName
            };

            var message = new CreditCardIsExpiringMessage
            {
                AccountNumberMasked = financialPaymentDetail.AccountNumberMasked,
                BillingLocation = financialPaymentDetail.BillingLocation?.GetFullStreetAddress(),
                CardExpirationDate = financialPaymentDetail.CardExpirationDate,
                CreatedDateTime = financialPaymentDetail.CreatedDateTime,
                CreditCardTypeValue = financialPaymentDetail.CreditCardTypeValue?.Value,
                ExpirationDate = financialPaymentDetail.ExpirationDate,
                ExpirationMonth = financialPaymentDetail.ExpirationMonth,
                ExpirationYear = financialPaymentDetail.ExpirationYear,
                FinancialPersonSavedAccountName = financialPaymentDetail.FinancialPersonSavedAccount?.Name,
                Id = financialPaymentDetail.Id,
                NameOnCard = financialPaymentDetail.NameOnCard,
                Person = cardHolder,
                FinancialScheduledTransactions = financialScheduledTransactions
            };

            _ = RockMessageBus.PublishAsync<ExpiringCardEventQueue, CreditCardIsExpiringMessage>( message );

            RockLogger.Log.Debug( RockLogDomains.Bus, "Published 'Credit Card Is Expiring Message' message." );
        }

        /// <summary>
        /// Details of the owner of the expiring card
        /// </summary>
        public class CardHolder
        {
            /// <summary>
            /// Gets or sets the communication preference.
            /// </summary>
            /// <value>
            /// The communication preference.
            /// </value>
            public int CommunicationPreference { get; set; }

            /// <summary>
            /// Gets or sets the email.
            /// </summary>
            /// <value>
            /// The email.
            /// </value>
            public string Email { get; set; }

            /// <summary>
            /// Gets or sets the first name.
            /// </summary>
            /// <value>
            /// The first name.
            /// </value>
            public string FirstName { get; set; }

            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public long Id { get; set; }

            /// <summary>
            /// Gets or sets the last name.
            /// </summary>
            /// <value>
            /// The last name.
            /// </value>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets the name of the middle.
            /// </summary>
            /// <value>
            /// The name of the middle.
            /// </value>
            public string MiddleName { get; set; }

            /// <summary>
            /// Gets or sets the name of the nick.
            /// </summary>
            /// <value>
            /// The name of the nick.
            /// </value>
            public string NickName { get; set; }
        }
    }
}
