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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.GivingConfiguration
{
    /// <summary>
    /// Contains all the initial configuration data required to render the
    /// Giving Configuration block.
    /// </summary>
    public class GivingConfigurationInitializationBag : BlockBox
    {
        /// <summary>
        ///  Gets or sets the isVisible property to determine if the block is visible.
        /// </summary>
        /// <value>The isVisible property to determine if the block is visible.</value>
        public bool IsVisible { get; set; }
        /// <summary>
        /// Gets or sets the saved accounts for the person or business.
        /// </summary>
        /// <value>The list of saved accounts for the person or business.</value>
        public List<FinancialPersonSavedAccountBag> SavedAccounts { get; set; }
        /// <summary>
        /// Gets or sets the default financial account for the person or business.
        /// </summary>
        /// <value>The default financial account for the person or business.</value>
        public FinancialAccountBag DefaultFinancialAccount { get; set; }
        /// <summary>
        /// Gets or sets the default saved account for the person or business.
        /// </summary>
        /// <value>The default saved account for the person or business.</value>
        public FinancialPersonSavedAccountBag DefaultSavedAccount { get; set; }
        /// <summary>
        /// Gets or sets the default saved account name for the person or business.
        /// </summary>
        /// <value>The default saved account name for the person or business.</value>
        public string DefaultSavedAccountName { get; set; }
        /// <summary>
        /// Gets or sets the scheduled transactions for the person or business.
        /// </summary>
        /// <value>The list of scheduled transactions for the person or business.</value>
        public List<FinancialScheduledTransactionBag> ScheduledTransactions { get; set; }
        /// <summary>
        /// Gets or sets the pledges for the person or business.
        /// </summary>
        /// <value>The list of pledges for the person or business.</value>
        public List<FinancialPledgeBag> Pledges { get; set; }
        /// <summary>
        /// Gets or sets the contribution statements for the person or business.
        /// </summary>
        /// <value>The list of contribution statements for the person or business.</value>
        public List<ContributionStatementBag> ContributionStatements { get; set; }
        /// <summary>
        /// Gets or sets the person action identifiers for pledges.
        /// </summary>
        /// <value>The person action identifiers for pledges.</value>
        public string PersonActionIdentifierPledge { get; set; }
        /// <summary>
        /// Gets or sets the person action identifiers for transactions.
        /// </summary>
        /// <value>The person action identifiers for transactions.</value>
        public string PersonActionIdentifierTransaction { get; set; }
        /// <summary>
        /// Gets or sets the person action identifiers for contributions.
        /// </summary>
        /// <value>The person action identifiers for contributions.</value>
        public string PersonActionIdentifierContribution { get; set; }
    }

    /// <summary>
    /// Contains the Text-To-Give settings for the person or business.
    /// </summary>
    public class TextToGiveSettingsBag
    {
        /// <summary>
        /// Gets or sets the selected financial account identifier.
        /// </summary>
        /// <value>The selected financial account identifier.</value>
        public string SelectedFinancialAccountId { get; set; }
        /// <summary>
        /// Gets or sets the selected saved account identifier.
        /// </summary>
        /// <value>The selected saved account identifier.</value>
        public string SelectedSavedAccountId { get; set; }
    }

    /// <summary>
    /// Contains the Financial Scheduled Transaction data for the person or business.
    /// </summary>
    public class FinancialScheduledTransactionBag
    {
        /// <summary>
        /// Gets or sets the identifier of the financial scheduled transaction.
        /// </summary>
        /// <value>The identifier of the financial scheduled transaction.</value>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the GUID of the financial scheduled transaction.
        /// </summary>
        /// <value>The GUID of the financial scheduled transaction.</value>
        public Guid Guid { get; set; }
        /// <summary>
        /// Gets or sets the isActive status of the financial scheduled transaction.
        /// </summary>
        /// <value>The isActive status of the financial scheduled transaction.</value>
        public bool IsActive { get; set; }
        /// <summary>
        /// Gets or sets the start date of the financial scheduled transaction.
        /// </summary>
        /// <value>The start date of the financial scheduled transaction.</value>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// Gets or sets the authorized person alias for the financial scheduled transaction.
        /// </summary>
        /// <value>The authorized person alias for the financial scheduled transaction.</value>
        public PersonAliasBag AuthorizedPersonAlias { get; set; }
        /// <summary>
        /// Gets or sets the scheduled transaction details for the financial scheduled transaction.
        /// </summary>
        /// <value>The list of scheduled transaction details for the financial scheduled transaction.</value>
        public List<FinancialScheduledTransactionDetailBag> ScheduledTransactionDetails { get; set; }
        /// <summary>
        /// Gets or sets the account summary for the financial scheduled transaction.
        /// </summary>
        /// <value>The list of account summaries for the financial scheduled transaction.</value>
        public List<string> AccountSummary { get; set; }
        /// <summary>
        /// Gets or sets the next payment date for the financial scheduled transaction.
        /// </summary>
        /// <value>The next payment date for the financial scheduled transaction.</value>
        public DateTime? NextPaymentDate { get; set; }
        /// <summary>
        /// Gets or sets the total amount for the financial scheduled transaction.
        /// </summary>
        /// <value>The total amount for the financial scheduled transaction.</value>
        public decimal TotalAmount { get; set; }
        /// <summary>
        /// Gets or sets the foreign currency code identifier for the financial scheduled transaction.
        /// </summary>
        /// <value>The foreign currency code identifier for the financial scheduled transaction.</value>
        public int? ForeignCurrencyCodeValueId { get; set; }
        /// <summary>
        /// Gets or sets the frequency text for the financial scheduled transaction.
        /// </summary>
        /// <value>The frequency text for the financial scheduled transaction.</value>
        public string FrequencyText { get; set; }
        /// <summary>
        /// Gets or sets the payment detail for the financial scheduled transaction.
        /// </summary>
        /// <value>The payment detail for the financial scheduled transaction.</value>
        public FinancialPaymentDetailBag FinancialPaymentDetail { get; set; }
        /// <summary>
        /// Gets or sets the saved account name for the financial scheduled transaction.
        /// </summary>
        /// <value></value>
        public string SavedAccountName { get; set; }
    }

    /// <summary>
    /// Contains the summary data of the account for the person or business.
    /// </summary>
    public class AccountSummaryBag
    {
        /// <summary>
        /// Gets or sets the is other flag.
        /// </summary>
        /// <value>The is other flag.</value>
        public bool IsOther { get; set; }
        /// <summary>
        /// Gets or sets the account name.
        /// </summary>
        /// <value>The account name.</value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the order of the account.
        /// </summary>
        /// <value>The order of the account.</value>
        public int Order { get; set; }
    }

    /// <summary>
    /// Contains all the financial pledge data for the person or business.
    /// </summary>
    public class FinancialPledgeBag
    {
        /// <summary>
        /// Gets or sets the GUID of the financial pledge.
        /// </summary>
        /// <value>The GUID of the financial pledge.</value>
        public Guid Guid { get; set; }
        /// <summary>
        /// Gets or sets the identifier of the financial pledge.
        /// </summary>
        /// <value>The identifier of the financial pledge.</value>
        public int Id {  get; set; }
        /// <summary>
        /// Gets or sets the total amount of the financial pledge.
        /// </summary>
        /// <value>The total amount of the financial pledge.</value>
        public decimal TotalAmount { get; set; }
        /// <summary>
        /// Gets or sets the account for the financial pledge.
        /// </summary>
        /// <value>The account for the financial pledge.</value>
        public FinancialAccountBag Account { get; set; }
        /// <summary>
        /// Gets or sets the person alias for the financial pledge.
        /// </summary>
        /// <value>The person alias for the financial pledge.</value>
        public PersonAliasBag PersonAlias { get; set; }
        /// <summary>
        /// Gets or sets the start date of the financial pledge.
        /// </summary>
        /// <value>The start date of the financial pledge.</value>
        public DateTimeOffset? StartDate { get; set; }
        /// <summary>
        /// Gets or sets the end date of the financial pledge.
        /// </summary>
        /// <value>The end date of the financial pledge.</value>
        public DateTimeOffset? EndDate { get; set; }
        /// <summary>
        /// Gets or sets the frequency value identifier of the financial pledge.
        /// </summary>
        /// <value>The frequency value identifier of the financial pledge.</value>
        public int? PledgeFrequencyValueId { get; set; }
        /// <summary>
        /// Gets or sets the pledge frequency value of the financial pledge.
        /// </summary>
        /// <value>The pledge frequency value of the financial pledge.</value>
        public string PledgeFrequencyValue { get; set; }
    }

    /// <summary>
    /// Contains the configuration options for the Giving Configuration block.
    /// </summary>
    public class GivingConfigurationOptionsBag
    {

    }

    /// <summary>
    /// Contains the contribution statement data for the person or business.
    /// </summary>
    public class ContributionStatementBag
    {
        /// <summary>
        /// Gets or sets the statement year.
        /// </summary>
        /// <value>The statement year.</value>
        public int Year { get; set; }
        /// <summary>
        /// Gets or sets the is current year flag.
        /// </summary>
        /// <value>The is current year flag.</value>
        public bool IsCurrentYear { get; set; }
    }

    /// <summary>
    /// Contains the financial payment detail data for the person or business.
    /// </summary>
    public class FinancialPaymentDetailBag
    {
        /// <summary>
        /// Gets or sets the currency type.
        /// </summary>
        /// <value>The currency type.</value>
        public string CurrencyType { get; set; }
        /// <summary>
        /// Gets or sets the credit card type.
        /// </summary>
        /// <value>The credit card type.</value>
        public string CreditCardType { get; set; }
        /// <summary>
        /// Gets or sets the masked account number.
        /// </summary>
        /// <value>The masked account number.</value>
        public string AccountNumberMasked { get; set; }
        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        /// <value>The expiration date.</value>
        public string ExpirationDate { get; set; }
        /// <summary>
        /// Gets or sets the Expiration Month.
        /// </summary>
        /// <value>The Expiration Month.</value>
        public string ExpirationMonth { get; set; }
        /// <summary>
        /// Gets or sets the Expiration Year.
        /// </summary>
        /// <value>The Expiration Year.</value>
        public string ExpirationYear { get; set; }
        /// <summary>
        /// Gets or sets the card expiration date.
        /// </summary>
        /// <value>The card expiration date.</value>
        public DateTime? CardExpirationDate { get; set; }
    }

    /// <summary>
    /// Contains the financial person saved account data for the person or business.
    /// </summary>
    public class FinancialPersonSavedAccountBag
    {
        /// <summary>
        /// Gets or sets the identifier of the financial person saved account.
        /// </summary>
        /// <value>The identifier of the financial person saved account.</value>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the GUID of the financial person saved account.
        /// </summary>
        /// <value>The GUID of the financial person saved account.</value>
        public Guid Guid { get; set; }
        /// <summary>
        /// Gets or sets the name of the financial person saved account.
        /// </summary>
        /// <value>The name of the financial person saved account.</value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the isDefault status of the financial person saved account.
        /// </summary>
        /// <value>The isDefault status of the financial person saved account.</value>
        public bool IsDefault { get; set; }
        /// <summary>
        /// Gets or sets the last error code of the financial person saved account.
        /// </summary>
        /// <value>The last error code of the financial person saved account.</value>
        public string LastErrorCode { get; set; }
        /// <summary>
        /// Gets or sets the last error code date time of the financial person saved account.
        /// </summary>
        /// <value>The last error code date time of the financial person saved account.</value>
        public DateTime? LastErrorCodeDateTime { get; set; }
        /// <summary>
        /// Gets or sets the saved account financial payment detail.
        /// </summary>
        /// <value>The saved account financial payment detail.</value>
        public FinancialPaymentDetailBag FinancialPaymentDetail { get; set; }
        /// <summary>
        /// Gets or sets the is expired status of the financial person saved account.
        /// </summary>
        /// <value>The is expired status of the financial person saved account.</value>
        public bool IsExpired { get; set; }
    }

    /// <summary>
    /// Contains the financial account data for the person or business.
    /// </summary>
    public class FinancialAccountBag
    {
        /// <summary>
        /// Gets or sets the identifier of the financial account.
        /// </summary>
        /// <value>The identifier of the financial account.</value>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the public name of the financial account.
        /// </summary>
        /// <value>The public name of the financial account.</value>
        public string PublicName { get; set; }
    }

    /// <summary>
    /// Contains the person alias data for the person or business.
    /// </summary>
    public class PersonAliasBag
    {
        /// <summary>
        /// Gets or sets the identifier of the person alias.
        /// </summary>
        /// <value>The identifier of the person alias.</value>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the person identifier of the person alias.
        /// </summary>
        /// <value>The person identifier of the person alias.</value>
        public int PersonId { get; set; }
        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>The person.</value>
        public PersonBag Person { get; set; }
    }

    /// <summary>
    /// Contains the person data for the person or business.
    /// </summary>
    public class PersonBag
    {
        /// <summary>
        /// Gets or sets the identifier of the person.
        /// </summary>
        /// <value>The identifier of the person.</value>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the last name of the person.
        /// </summary>
        /// <value>The last name of the person.</value>
        public string LastName { get; set; }
        /// <summary>
        /// Gets or sets the nick name of the person.
        /// </summary>
        /// <value>The nick name of the person.</value>
        public string NickName { get; set; }
    }

    /// <summary>
    /// Contains the financial scheduled transaction detail data for the person or business.
    /// </summary>
    public class FinancialScheduledTransactionDetailBag
    {
        /// <summary>
        /// Gets or sets the identifier of the financial scheduled transaction detail.
        /// </summary>
        /// <value>The identifier of the financial scheduled transaction detail.</value>
        public FinancialAccountBag Account { get; set; }
    }
}

