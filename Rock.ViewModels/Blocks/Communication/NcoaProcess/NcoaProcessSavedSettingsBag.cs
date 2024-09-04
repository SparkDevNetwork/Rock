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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.NcoaProcess
{
    /// <summary>
    /// 
    /// </summary>
    public class NcoaProcessSavedSettingsBag
    {
        /// <summary>
        /// Gets or sets the person data view Guid.
        /// </summary>
        /// <value>
        /// The person data view Guid.
        /// </value>
        public ListItemBag PersonDataView { get; set; }

        /// <summary>
        /// Gets or sets the reference to the uploaded file.
        /// </summary>
        /// <value>
        /// The reference to the uploaded file.
        /// </value>
        public ListItemBag UploadFileReference { get; set; }

        /// <summary>
        /// Gets or sets the minimum move distance to use when determining inactivating people.
        /// </summary>
        /// <value>
        /// The minimum move distance (in miles) to use when determining inactivating people.
        /// </value>
        public decimal? MinimumMoveDistance { get; set; }

        /// <summary>
        /// Gets or sets the boolean determining if a move in the 19-48 month catagory should be marked as a previous address.
        /// </summary>
        /// <value>
        /// The boolean determining if a move in the 19-48 month catagory should be marked as a previous address.
        /// </value>
        public bool? Is48MonthMoveChecked { get; set; }

        /// <summary>
        /// Gets or sets the boolean determining invalid addresses should be marked as previous addresses.
        /// </summary>
        /// <value>
        /// The boolean determining if invalid addresses should be marked as previous addresses.
        /// </value>
        public bool? IsInvalidAddressesChecked { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Inactive Record Reason to use when inactivating people due to moving beyond the configured number of miles.
        /// </summary>
        /// <value>
        /// A defined value representing the Id of the Inactive Record Reason to use when inactivating people due to moving beyond the configured number of miles.
        /// </value>
        public ListItemBag InactiveRecordReason { get; set; }
    }
}
