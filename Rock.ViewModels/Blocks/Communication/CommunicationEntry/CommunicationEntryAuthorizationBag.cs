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
        ///   <c>true</c> if the approve action is authorized on the block; otherwise, <c>false</c>.
        /// </value>
        public bool IsBlockApproveActionAuthorized { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the edit action is authorized on the block for the logged in person.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the edit action is authorized on the block; otherwise, <c>false</c>.
        /// </value>
        public bool IsBlockEditActionAuthorized { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the edit action is authorized on the communication for the logged in person.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the edit action is authorized on the communication; otherwise, <c>false</c>.
        /// </value>
        public bool IsCommunicationEditActionAuthorized { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether the logged in person can approve the communication.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the logged in person can approve the communication; otherwise, <c>false</c>.
        /// </value>
        public bool CanApproveCommunication { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the logged in person can edit the communication.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the logged in person can edit the communication; otherwise, <c>false</c>.
        /// </value>
        public bool CanEditCommunication { get; internal set; }
    }
}
