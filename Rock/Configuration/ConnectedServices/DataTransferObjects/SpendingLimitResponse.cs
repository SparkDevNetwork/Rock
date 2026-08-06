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

namespace Rock.Configuration.ConnectedServices.DataTransferObjects
{
    /// <summary>
    /// Represents the response from the Spark connected services API about
    /// the spending limit for the service.
    /// </summary>
    internal class SpendingLimitResponse
    {
        /// <summary>
        /// The spending limit for the service. This will be <c>null</c> if
        /// not available.
        /// </summary>
        public decimal? SpendingLimit { get; set; }
    }
}
