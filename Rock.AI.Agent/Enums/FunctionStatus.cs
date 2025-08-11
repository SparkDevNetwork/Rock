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

namespace Rock.AI.Agent.Enums
{
    /// <summary>
    /// Indicates the overall outcome of a lookup operation.
    /// </summary>
    /// <remarks>
    /// Use <see cref="FunctionStatus.Success"/> when items were found,
    /// <see cref="FunctionStatus.NoData"/> when the operation succeeded but returned no items,
    /// and <see cref="FunctionStatus.Error"/> when the operation failed.
    /// </remarks>
    public enum FunctionStatus
    {
        /// <summary>
        /// The lookup executed successfully and returned one or more items.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The lookup executed successfully but returned no items.
        /// </summary>
        NoData = 1,

        /// <summary>
        /// The lookup failed. See the error message on the result for details.
        /// </summary>
        Error = 2
    }
}
