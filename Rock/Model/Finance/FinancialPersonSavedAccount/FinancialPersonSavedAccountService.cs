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
using System.Linq;

using Rock.Data;
using Rock.Model.Finance.FinancialPersonSavedAccountService.Options;

namespace Rock.Model
{
    /// <summary>
    /// Service and data access class for <see cref="Rock.Model.FinancialPersonSavedAccount"/> objects.
    /// </summary>
    public partial class FinancialPersonSavedAccountService
    {
        #region Default Options

        /// <summary>
        /// The default financial person saved account query options.
        /// </summary>
        private static readonly FinancialPersonSavedAccountQueryOptions DefaultFinancialPersonSavedAccountQueryOptions = new FinancialPersonSavedAccountQueryOptions();

        #endregion

        /// <summary>
        /// Returns an queryable collection of saved accounts (<see cref="Rock.Model.FinancialPersonSavedAccount"/> by PersonId
        /// </summary>
        /// <param name="personId">A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> to retrieve saved accounts for.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.FinancialPersonSavedAccount">Saved Accounts</see> belonging to the specified <see cref="Rock.Model.Person"/>.</returns>
        public IQueryable<FinancialPersonSavedAccount> GetByPersonId( int personId )
        {
            return this.Queryable().Where( a => a.PersonAlias.PersonId == personId );
        }

        /// <summary>
        /// Gets a standard query that matches the specified query options.
        /// </summary>
        /// <param name="options">The options to use when filtering results.</param>
        /// <returns>A queryable of <see cref="FinancialPersonSavedAccount"/> objects that match the options.</returns>
        public IQueryable<FinancialPersonSavedAccount> GetFinancialPersonSavedAccountsQuery( FinancialPersonSavedAccountQueryOptions options )
        {
            options = options ?? DefaultFinancialPersonSavedAccountQueryOptions;

            // Build the query to get all the matching saved accounts for
            // the specified person.
            var savedAccountsQuery = Queryable()
                .Where( a => !a.IsSystem );

            // If the options includes any person identifiers then we limit our
            // results to only records that match one of those people.
            if ( options.PersonIds?.Any() ?? false )
            {
                savedAccountsQuery = savedAccountsQuery
                    .Where( a => options.PersonIds.Contains( a.PersonAlias.PersonId ) );
            }

            // If the options includes any financial gateways then we limit our
            // results to only records that match one of those gateways.
            if ( options.FinancialGatewayGuids?.Any() ?? false )
            {
                savedAccountsQuery = savedAccountsQuery
                    .Where( a => a.FinancialGatewayId.HasValue
                        && options.FinancialGatewayGuids.Contains( a.FinancialGateway.Guid ) );
            }

            // If the options includes any currency types then we limit our
            // results to only records that match one of those currency types.
            if ( options.CurrencyTypeGuids?.Any() ?? false )
            {
                savedAccountsQuery = savedAccountsQuery
                    .Where( a => a.FinancialPaymentDetailId.HasValue
                        && a.FinancialPaymentDetail.CurrencyTypeValueId.HasValue
                        && options.CurrencyTypeGuids.Contains( a.FinancialPaymentDetail.CurrencyTypeValue.Guid ) );
            }

            return savedAccountsQuery;
        }

        /// <summary>
        /// 
        /// </summary>
        internal class RemoveExpiredSavedAccountsResult
        {
            /// <summary>
            /// Gets or sets the delete if expired before date.
            /// </summary>
            /// <value>
            /// The delete if expired before date.
            /// </value>
            public DateTime DeleteIfExpiredBeforeDate { get; internal set; }

            /// <summary>
            /// Gets or sets the accounts deleted count.
            /// </summary>
            /// <value>
            /// The accounts deleted count.
            /// </value>
            public int AccountsDeletedCount { get; internal set; }

            /// <summary>
            /// Gets or sets the account removal exceptions.
            /// </summary>
            /// <value>
            /// The account removal exceptions.
            /// </value>
            public IList<Exception> AccountRemovalExceptions { get; set; } = new List<Exception>();
        }

        /// <summary>
        /// Removes the expired saved accounts.
        /// </summary>
        /// <param name="removedExpiredSavedAccountDays">The removed expired saved account days.</param>
        /// <returns></returns>
        internal RemoveExpiredSavedAccountsResult RemoveExpiredSavedAccounts( int removedExpiredSavedAccountDays )
        {
            var financialPersonSavedAccountQry = new FinancialPersonSavedAccountService( new RockContext() ).Queryable()
                .Where( a =>
                    a.FinancialPaymentDetail.CardExpirationDate != null
                    && ( a.PersonAliasId.HasValue || a.GroupId.HasValue )
                    && a.FinancialPaymentDetailId.HasValue
                    && a.IsSystem == false )
                .OrderBy( a => a.Id );

            var savedAccountInfoList = financialPersonSavedAccountQry.Select( a => new
            {
                Id = a.Id,
                FinancialPaymentDetail = a.FinancialPaymentDetail
            } ).ToList();

            DateTime now = RockDateTime.Now;
            int currentMonth = now.Month;
            int currentYear = now.Year;

            var result = new RemoveExpiredSavedAccountsResult()
            {
                // if today is 3/16/2020 and removedExpiredSavedAccountDays is 90, only delete card if it expired before 12/17/2019
                DeleteIfExpiredBeforeDate = RockDateTime.Today.AddDays( -removedExpiredSavedAccountDays )
            };

            foreach ( var savedAccountInfo in savedAccountInfoList )
            {
                int? expirationMonth = savedAccountInfo.FinancialPaymentDetail.ExpirationMonth;
                int? expirationYear = savedAccountInfo.FinancialPaymentDetail.ExpirationYear;
                if ( !expirationMonth.HasValue || !expirationYear.HasValue )
                {
                    continue;
                }

                if ( expirationMonth.Value < 1 || expirationMonth.Value > 12 || expirationYear <= DateTime.MinValue.Year || expirationYear >= DateTime.MaxValue.Year )
                {
                    // invalid month (or year)
                    continue;
                }

                // a credit card with an expiration of April 2020 would be expired on May 1st, 2020
                var cardExpirationDate = new DateTime( expirationYear.Value, expirationMonth.Value, 1 ).AddMonths( 1 );

                /* Example:
                 Today's Date: 2020-3-16
                 removedExpiredSavedAccountDays: 90
                 Expired Before Date: 2019-12-17 (Today (2020-3-16) - removedExpiredSavedAccountDays)
                 Cards that expired before 2019-12-17 should be deleted
                 Delete 04/20 (Expires 05/01/2020) card? No
                 Delete 05/20 (Expires 06/01/2020) card? No
                 Delete 01/20 (Expires 03/01/2020) card? No
                 Delete 12/19 (Expires 01/01/2020) card? No
                 Delete 11/19 (Expires 12/01/2019) card? Yes
                 Delete 10/19 (Expires 11/01/2019) card? Yes

                 Today's Date: 2020-3-16
                 removedExpiredSavedAccountDays: 0
                 Expired Before Date: 2019-03-16 (Today (2020-3-16) - 0)
                 Cards that expired before 2019-03-16 should be deleted
                 Delete 04/20 (Expires 05/01/2020) card? No
                 Delete 05/20 (Expires 06/01/2020) card? No
                 Delete 01/20 (Expires 03/01/2020) card? Yes
                 Delete 12/19 (Expires 01/01/2020) card? Yes
                 Delete 11/19 (Expires 12/01/2019) card? Yes
                 Delete 10/19 (Expires 11/01/2019) card? Yes
                 */

                if ( cardExpirationDate >= result.DeleteIfExpiredBeforeDate )
                {
                    // We want to only delete cards that expired more than X days ago, so if this card expiration day is after that, skip
                    continue;
                }

                // Card expiration date is older than X day ago, so delete it.
                // Wrapping the following in a try/catch so a single deletion failure doesn't end the process for all deletion candidates.
                try
                {
                    using ( var savedAccountRockContext = new RockContext() )
                    {
                        var financialPersonSavedAccountService = new FinancialPersonSavedAccountService( savedAccountRockContext );
                        var financialPersonSavedAccount = financialPersonSavedAccountService.Get( savedAccountInfo.Id );
                        if ( financialPersonSavedAccount != null )
                        {
                            if ( financialPersonSavedAccountService.CanDelete( financialPersonSavedAccount, out _ ) )
                            {
                                financialPersonSavedAccountService.Delete( financialPersonSavedAccount );
                                savedAccountRockContext.SaveChanges();
                                result.AccountsDeletedCount++;
                            }
                        }
                    }
                }
                catch ( Exception ex )
                {
                    // Provide better identifying context in case the caught exception is too vague.
                    var exception = new Exception( $"Unable to delete FinancialPersonSavedAccount (ID = {savedAccountInfo.Id}).", ex );

                    result.AccountRemovalExceptions.Add( exception );
                }
            }

            return result;
        }
    }
}