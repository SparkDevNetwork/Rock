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

namespace Rock.ViewModels.Blocks.Administration.SparkConnectedServices
{
    /// <summary>
    /// Summary information about a credit card for the organization.
    /// </summary>
    public class CreditCardSummaryBag
    {
        /// <summary>
        /// The name of the type of credit card, such as "Visa" or "MasterCard".
        /// </summary>
        public string CardType { get; set; }

        /// <summary>
        /// The month of the credit card's expiration date, represented as an
        /// integer (1-12).
        /// </summary>
        public int? ExpirationMonth { get; set; }

        /// <summary>
        /// The year of the credit card's expiration date, represented as a
        /// four-digit integer (e.g., 2025).
        /// </summary>
        public int? ExpirationYear { get; set; }

        /// <summary>
        /// The last four digits of the credit card number, used for display
        /// purposes to help identify the card without exposing the full number.
        /// </summary>
        public string LastFourDigits { get; set; }

        /// <summary>
        /// Indicates whether a credit card is on file for the organization.
        /// </summary>
        public bool IsCardOnFile { get; set; }

        /// <summary>
        /// Indicates whether the credit card on file is expired based on the
        /// current date.
        /// </summary>
        public bool IsCardExpired { get; set; }

        /// <summary>
        /// Indicates whether the credit card on file is expiring soon, defined as
        /// within the next two months.
        /// </summary>
        public bool IsCardExpiringSoon { get; set; }
    }
}
