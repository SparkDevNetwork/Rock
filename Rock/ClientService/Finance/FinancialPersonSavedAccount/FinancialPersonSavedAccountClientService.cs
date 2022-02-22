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

using System.Collections.Generic;
using System.Linq;

using Rock.ClientService.Finance.FinancialPersonSavedAccount.Options;
using Rock.Data;
using Rock.Model;
using Rock.ViewModel.NonEntities;
using Rock.Web.Cache;

namespace Rock.ClientService.Finance.FinancialPersonSavedAccount
{
    /// <summary>
    /// Provides methods to work with <see cref="FinancialPersonSavedAccount"/> and translate
    /// information into data that can be consumed by the clients.
    /// </summary>
    /// <seealso cref="Rock.ClientService.ClientServiceBase" />
    public class FinancialPersonSavedAccountClientService : ClientServiceBase
    {
        #region Default Options

        /// <summary>
        /// The default saved financial account options.
        /// </summary>
        private static readonly SavedFinancialAccountOptions DefaultSavedFinancialAccountOptions = new SavedFinancialAccountOptions();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialPersonSavedAccountClientService"/> class.
        /// </summary>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <param name="person">The person to use for security checks.</param>
        public FinancialPersonSavedAccountClientService( RockContext rockContext, Person person )
            : base( rockContext, person )
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the saved financial accounts associated with the specified
        /// person and returns the view models that represent those accounts.
        /// </summary>
        /// <param name="personId">The person identifier whose accounts should be retrieved.</param>
        /// <param name="options">The options for filtering and limiting the results.</param>
        /// <returns>A list of <see cref="SavedFinancialAccountListItemViewModel"/> objects that represent the accounts.</returns>
        public List<SavedFinancialAccountListItemViewModel> GetSavedFinancialAccountsForPersonAsAccountListItems( int personId, SavedFinancialAccountOptions options = null )
        {
            var financialPersonSavedAccountService = new FinancialPersonSavedAccountService( RockContext );

            options = options ?? DefaultSavedFinancialAccountOptions;

            var queryOptions = options.ToQueryOptions();

            // Limit the query to just the one person we are interested in.
            queryOptions.PersonIds = new List<int> { personId };

            // Build the query to get all the matching saved accounts for
            // the specified person.
            var savedAccountsQuery = financialPersonSavedAccountService
                .GetFinancialPersonSavedAccountsQuery( queryOptions );

            // Get the data from the database, pulling in only the bits we need.
            var savedAccounts = savedAccountsQuery
                .Select( a => new
                {
                    a.Guid,
                    a.Name,
                    a.FinancialPaymentDetail.ExpirationMonth,
                    a.FinancialPaymentDetail.ExpirationYear,
                    a.FinancialPaymentDetail.AccountNumberMasked,
                    a.FinancialPaymentDetail.CurrencyTypeValueId,
                    a.FinancialPaymentDetail.CreditCardTypeValueId
                } )
                .ToList();

            // Note: We don't perform a security check because saved accounts
            // don't have explicit security. It is implied that the matching
            // person identifier is enough security check.

            // Translate the saved accounts into something that will be
            // recognized by the client.
            return savedAccounts
                .Select( a =>
                {
                    string image = null;
                    string expirationDate = null;

                    if ( a.ExpirationMonth.HasValue && a.ExpirationYear.HasValue )
                    {
                        // ExpirationYear returns 4 digits, but just in case,
                        // check if it is 4 digits before just getting the last 2.
                        string expireYY = a.ExpirationYear.Value.ToString();
                        if ( expireYY.Length == 4 )
                        {
                            expireYY = expireYY.Substring( 2 );
                        }

                        expirationDate = $"{a.ExpirationMonth.Value:00}/{expireYY:00}";
                    }

                    // Determine the descriptive text to associate with this account.
                    string description = a.AccountNumberMasked != null && a.AccountNumberMasked.Length >= 4
                        ? $"Ending in {a.AccountNumberMasked.Right( 4 )} and Expires {expirationDate}"
                        : $"Expires {expirationDate}";

                    // Determine the image to use for this account.
                    if ( a.CreditCardTypeValueId.HasValue )
                    {
                        var creditCardTypeValueCache = DefinedValueCache.Get( a.CreditCardTypeValueId.Value );

                        image = creditCardTypeValueCache?.GetAttributeValue( SystemKey.CreditCardTypeAttributeKey.IconImage );
                    }

                    return new SavedFinancialAccountListItemViewModel
                    {
                        Value = a.Guid.ToString(),
                        Text = a.Name,
                        Description = description,
                        Image = image
                    };
                } )
                .OrderBy( a => a.Text )
                .ToList();
        }

        #endregion
    }
}
