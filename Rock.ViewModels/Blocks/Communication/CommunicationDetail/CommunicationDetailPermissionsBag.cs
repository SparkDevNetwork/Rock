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

namespace Rock.ViewModels.Blocks.Communication.CommunicationDetail
{
    /// <summary>
    /// A bag that contains information about which actions the individual is authorized to perform on a communication
    /// within the communication detail block.
    /// </summary>
    public class CommunicationDetailPermissionsBag
    {
        /// <summary>
        /// Gets or sets whether the individual is authorized to approve this communication.
        /// </summary>
        public bool CanApprove { get; set; }

        /// <summary>
        /// Gets or sets whether the individual is authorized to deny this communication.
        /// </summary>
        public bool CanDeny { get; set; }

        /// <summary>
        /// Gets or sets whether the individual is authorized to edit this communication.
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Gets or sets whether the individual is authorized to cancel this communication.
        /// </summary>
        public bool CanCancel { get; set; }

        /// <summary>
        /// Gets or sets whether the individual is authorized to duplicate this communication.
        /// </summary>
        public bool CanDuplicate { get; set; }

        /// <summary>
        /// Gets or sets whether the individual is authorized to create a personal template from this communication.
        /// </summary>
        public bool CanCreatePersonalTemplate { get; set; }
    }
}
