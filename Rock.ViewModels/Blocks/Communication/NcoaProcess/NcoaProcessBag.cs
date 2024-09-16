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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.NcoaProcess
{
    /// <summary>
    /// 
    /// </summary>
    public class NcoaProcessBag
    {
        /// <summary>
        /// Gets or sets the Id of the Inactive Record Reason to use when inactivating people due to moving beyond the configured number of miles.
        /// </summary>
        /// <value>
        /// A defined value representing the Id of the Inactive Record Reason to use when inactivating people due to moving beyond the configured number of miles.
        /// </value>
        public ListItemBag InactiveReason {  get; set; }

        /// <summary>
        /// Gets or sets the boolean determining invalid addresses should be marked as previous addresses.
        /// </summary>
        /// <value>
        /// The boolean determining if invalid addresses should be marked as previous addresses.
        /// </value>
        public bool MarkInvalidAsPrevious {  get; set; }

        /// <summary>
        /// Gets or sets the boolean determining if a move in the 19-48 month catagory should be marked as a previous address.
        /// </summary>
        /// <value>
        /// The boolean determining if a move in the 19-48 month catagory should be marked as a previous address.
        /// </value>
        public bool Mark48MonthAsPrevious { get; set; }

        /// <summary>
        /// Gets or sets the minimum move distance to use when determining inactivating people.
        /// </summary>
        /// <value>
        /// The minimum move distance (in miles) to use when determining inactivating people.
        /// </value>
        public decimal MinMoveDistance { get; set; }

        /// <summary>
        /// Gets or sets the reference to the uploaded file.
        /// </summary>
        /// <value>
        /// The reference to the uploaded file.
        /// </value>
        public ListItemBag NcoaFileUploadReference { get; set; }

        /// <summary>
        /// Gets or sets the list of addresses that will be exported into the file used in the NCOA process.
        /// </summary>
        /// <value>
        /// A list that contains the specific addresses that will be used in the NCOA process.
        /// </value>
        public List<NcoaProcessPersonAddressBag> Addresses { get; set; }

        /// <summary>
        /// Gets or sets the success message.
        /// </summary>
        /// <value>
        /// The success message.
        /// </value>
        public string SuccessMessage { get; set; }
    }
}
