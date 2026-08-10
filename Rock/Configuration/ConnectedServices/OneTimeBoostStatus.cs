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

namespace Rock.Configuration.ConnectedServices
{
    /// <summary>
    /// Represents the outcome of a one-time boost purchase attempt for a
    /// Rock Intelligence service.
    /// </summary>
    internal enum OneTimeBoostStatus
    {
        /// <summary>
        /// The one-time boost was purchased and applied successfully.
        /// </summary>
        Complete = 0,

        /// <summary>
        /// The payment for the one-time boost was accepted but the credit
        /// has not yet been applied. This typically resolves within 24 hours.
        /// </summary>
        Pending = 1,

        /// <summary>
        /// The payment for the one-time boost was declined by the payment
        /// processor or the connected services API.
        /// </summary>
        Declined = 2,

        /// <summary>
        /// An error prevented us from determining a definitive outcome, such
        /// as a network failure, a malformed server response, or an
        /// unexpected HTTP status code.
        /// </summary>
        Error = 3,
    }
}
