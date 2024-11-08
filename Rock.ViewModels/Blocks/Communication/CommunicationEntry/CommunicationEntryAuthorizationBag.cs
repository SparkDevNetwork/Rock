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
namespace Rock.ViewModels.Blocks.Communication.CommunicationEntry
{
    /// <summary>
    /// Bag containing the authorization information for the Communication Entry block.
    /// </summary>
    public class CommunicationEntryAuthorizationBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the approve action is authorized on the block for the logged in person.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the approve action is authorized on the block; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsBlockApproveActionAuthorized { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the edit action is authorized on the block for the logged in person.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the edit action is authorized on the block; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsBlockEditActionAuthorized { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the edit action is authorized on the communication for the logged in person.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the edit action is authorized on the communication; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsCommunicationEditActionAuthorized { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the logged in person can view the communication entry block.
        /// </summary>
        /// <remarks>
        /// This is an internal property used by Communication Entry block.
        /// </remarks>
        /// <value>
        ///   <see langword="true"/> if the logged in person can view the communication entry block; otherwise, <see langword="false"/>.
        /// </value>
        internal bool CanViewBlock { get; set; }
    }
}
