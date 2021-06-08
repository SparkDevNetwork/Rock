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
using System.Diagnostics;

using Rock.Data;

namespace Rock.Financial
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="FinancialStatementGeneratorRecipient" />
    [RockClientInclude( "Recipient Result Information for the Statement Generator" )]
    public class FinancialStatementGeneratorRecipientResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialStatementGeneratorRecipientResult"/> class.
        /// </summary>
        public FinancialStatementGeneratorRecipientResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialStatementGeneratorRecipientResult"/> class.
        /// </summary>
        /// <param name="recipient">The recipient.</param>
        public FinancialStatementGeneratorRecipientResult( FinancialStatementGeneratorRecipient recipient )
        {
            Recipient = recipient;
        }

        /// <summary>
        /// Gets or sets the recipient.
        /// </summary>
        /// <value>
        /// The recipient.
        /// </value>
        public FinancialStatementGeneratorRecipient Recipient { get; set; }

        /// <summary>
        /// Gets or sets the HTML.
        /// NOTE: If this is NULL/EmptyString or OptedOut == True, don't show the statement
        /// </summary>
        /// <value>
        /// The HTML.
        /// </value>
        public string Html { get; set; }

        /// <summary>
        /// Gets or sets the footer HTML fragment.
        /// </summary>
        /// <value>
        /// The footer HTML fragment.
        /// </value>
        public string FooterHtmlFragment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this statement should not be included due to the 'Do Not Send Giving Statement' option
        /// </summary>
        /// <value>
        ///   <c>true</c> if [opted out]; otherwise, <c>false</c>.
        /// </value>
        public bool OptedOut { get; set; }

        /// <summary>
        /// The total amount of contributions reported on the statement.
        /// </summary>
        /// <value>
        /// The contribution total.
        /// </value>
        public decimal ContributionTotal { get; set; }

        /// <summary>
        /// The total Pledged Amount of pledges reported on the statement. For example, if $100 was pledged, but only $25
        /// was given, use $100 as the Pledge Total
        /// </summary>
        /// <value>
        /// The pledge total.
        /// </value>
        public decimal? PledgeTotal { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [DebuggerDisplay( "GroupId:{GroupId}, PersonId:{PersonId}" )]
    [RockClientInclude( "Recipient Information for the Statement Generator" )]
    public class FinancialStatementGeneratorRecipient
    {
        /// <summary>
        /// Gets or sets the GroupId of the Family to use as the Address.
        /// if PersonId is null, this is also the GivingGroupId to use when fetching transactions.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the person identifier for people that give as Individuals. If this is null, get the Transactions based on the GivingGroupId.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the location identifier.
        /// This is the Mailing Address that the statement should be sent to.
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has valid mailing address.
        /// Either <see cref="LocationId"/> is null, or the address is missing <seealso cref="Rock.Model.Location.PostalCode"/> or <seealso cref="Rock.Model.Location.Street1"/> 
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has valid mailing address; otherwise, <c>false</c>.
        /// </value>
        public bool HasValidMailingAddress { get; set; }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        /// <value>
        /// The postal code.
        /// </value>
        public string PostalCode { get; set; }

        /// <inheritdoc cref="Rock.Model.Person.LastName"/>
        public string LastName { get; set; }

        /// <inheritdoc cref="Rock.Model.Person.NickName"/>
        public string NickName { get; set; }

        /// <summary>
        /// The country (if any) for the address on the statement.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is international address.
        /// Addresses with countries the same as the Organization’s address (Global Attribute) or 
        /// addresses with blank countries will be considered local.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is international address; otherwise, <c>false</c>.
        /// </value>
        public bool IsInternationalAddress { get; set; }

        /// <summary>
        /// The number of PDFs pages that resulted when generating the statement.
        /// If "Enable Page Count Pre-Determination" is enabled, the statement for this person will be run a 2nd time so that the RenderedPageCount.
        /// can be passed as a MergeField when generating the HTML
        /// </summary>
        /// <value>
        /// The rendered page count.
        /// </value>
        public int? RenderedPageCount { get; set; }

        /// <summary>
        /// Determines that this statement has been processed by the StatementGenerator. This is needed for the restart logic in the StatementGenerator application.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is complete; otherwise, <c>false</c>.
        /// </value>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Gets or sets the opted out.
        /// </summary>
        /// <value>
        /// The opted out.
        /// </value>
        public bool? OptedOut { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [paperless statement uploaded].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [paperless statement uploaded]; otherwise, <c>false</c>.
        /// </value>
        public bool? PaperlessStatementUploaded { get; set; }

        /// <summary>
        /// Gets or sets the paperless statements individual count.
        /// </summary>
        /// <value>
        /// The paperless statements individual count.
        /// </value>
        public int? PaperlessStatementsIndividualCount { get; set; }

        /// <summary>
        /// Gets or sets the contribution total.
        /// </summary>
        /// <value>
        /// The contribution total.
        /// </value>
        public decimal? ContributionTotal { get; set; }
    }
}
