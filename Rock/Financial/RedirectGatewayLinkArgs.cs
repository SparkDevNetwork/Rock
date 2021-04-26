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

namespace Rock.Financial
{
    /// <summary>
    /// RedirectGatewayLinkArgs
    /// </summary>
    public class RedirectGatewayLinkArgs
    {
        /// <summary>
        /// Recurring Options
        /// </summary>
        public enum RecurringOption
        {
            /// <summary>
            /// The weekly
            /// </summary>
            Weekly,

            /// <summary>
            /// The fortnightly
            /// </summary>
            Fortnightly,

            /// <summary>
            /// The first and fifteenth
            /// </summary>
            FirstAndFifteenth,

            /// <summary>
            /// The monthly
            /// </summary>
            Monthly,

            /// <summary>
            /// The no
            /// </summary>
            No
        }

        /// <summary>
        /// FundVisibility Options
        /// </summary>
        public enum FundVisibilityOption
        {
            /// <summary>
            /// The show
            /// </summary>
            Show,

            /// <summary>
            /// The hide
            /// </summary>
            Hide,

            /// <summary>
            /// The lock
            /// </summary>
            Lock
        }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Gets or sets the amount lock.
        /// </summary>
        /// <value>
        /// The amount lock.
        /// </value>
        public bool? AmountLock { get; set; }

        /// <summary>
        /// Gets or sets the recurring.
        /// </summary>
        /// <value>
        /// The recurring.
        /// </value>
        public RecurringOption? Recurring { get; set; }

        /// <summary>
        /// Gets or sets the recurring visibility.
        /// </summary>
        /// <value>
        /// The recurring visibility.
        /// </value>
        public bool? RecurringVisibility { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        /// <value>
        /// The email address.
        /// </value>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the source reference.
        /// </summary>
        /// <value>
        /// The source reference.
        /// </value>
        public string SourceReference { get; set; }

        /// <summary>
        /// Gets or sets the fund.
        /// </summary>
        /// <value>
        /// The fund.
        /// </value>
        public string Fund { get; set; }

        /// <summary>
        /// Gets or sets the fund visibility.
        /// </summary>
        /// <value>
        /// The fund visibility.
        /// </value>
        public FundVisibilityOption? FundVisibility { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the additional data.
        /// </summary>
        /// <value>
        /// The additional data.
        /// </value>
        public object AdditionalData { get; set; }

        /// <summary>
        /// Gets or sets the redirect URL.
        /// </summary>
        /// <value>
        /// The redirect URL.
        /// </value>
        public string RedirectUrl { get; set; }
    }
}
