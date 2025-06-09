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

using Rock.Utility;

namespace Rock.Lava.Blocks.Internal
{
    /// <summary>
    /// Describes the result of a database transaction from the
    /// <see cref="DbTransaction"/> Lava block.
    /// </summary>
    internal class DbTransactionResult : RockDynamic
    {
        /// <summary>
        /// Was the transaction successful.
        /// </summary>
        /// <remarks>
        /// Note this is intentionally not IsSuccess as we want the name to be Success to match other parts of Lava.
        /// </remarks>
        public bool Success { get; set; } = true;

        /// <summary>
        /// The error message for the transaction.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// List of validation errors.
        /// </summary>
        public List<ValidationError> ValidationErrors { get; set; } = new List<ValidationError>();
    }
}
