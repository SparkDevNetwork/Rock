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

namespace Rock.StatementGenerator
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.StatementGenerator.StatementGeneratorRecipient" />
    [Obsolete( "Use ~/api/FinancialGivingStatement/ endpoints instead " )]
    [RockObsolete( "1.12.4" )]
    public class StatementGeneratorRecipientResult : StatementGeneratorRecipient
    {
        /// <summary>
        /// Gets or sets the HTML.
        /// NOTE: If this is NULL/EmptyString or OptedOut == True, don't show the statement
        /// </summary>
        /// <value>
        /// The HTML.
        /// </value>
        public string Html { get; set; }

        /// <summary>
        /// Gets or sets the footer HTML.
        /// </summary>
        /// <value>
        /// The footer HTML.
        /// </value>
        public string FooterHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this statement should not be included due to the 'Do Not Send Giving Statement' option
        /// </summary>
        /// <value>
        ///   <c>true</c> if [opted out]; otherwise, <c>false</c>.
        /// </value>
        public bool OptedOut { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [Obsolete( "Use ~/api/FinancialGivingStatement/ endpoints instead " )]
    [RockObsolete( "1.12.4" )]
    public class StatementGeneratorRecipient
    {
        /// <summary>
        /// Gets or sets the GroupId of the Family to use as the Address.
        /// if PersonId is null, this is also the GivingGroupId to use when fetching transactions
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the person identifier for people that give as Individuals. If this is null, get the Transactions based on the GivingGroupId
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the location identifier.
        /// This is the Mailing Address that the statement should be sent to
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        public Guid? LocationGuid { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"GroupId:{GroupId}, PersonId:{PersonId}, LocationGuid:{LocationGuid}";
        }
    }
}
