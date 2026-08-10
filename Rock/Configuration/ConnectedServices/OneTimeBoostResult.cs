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
    /// Represents the outcome of applying a one-time boost to a Rock
    /// Intelligence service.
    /// </summary>
    internal class OneTimeBoostResult
    {
        /// <summary>
        /// The status of the one-time boost.
        /// </summary>
        public OneTimeBoostStatus Status { get; set; }

        /// <summary>
        /// A human-readable message describing the outcome, suitable for
        /// display to an administrator. May be <c>null</c> when the outcome
        /// does not warrant a message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The amount that was applied (or attempted to be applied) as the
        /// one-time boost. May be <c>null</c> when the server did not
        /// return a body confirming the amount.
        /// </summary>
        public decimal? Amount { get; set; }
    }
}
