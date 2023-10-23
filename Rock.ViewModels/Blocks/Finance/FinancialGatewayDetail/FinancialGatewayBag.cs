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

namespace Rock.ViewModels.Blocks.Finance.FinancialGatewayDetail
{
    /// <summary>
    /// Used to store options for the FinancialGateway
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class FinancialGatewayBag : EntityBagBase
    {
        /// <summary>
        /// Gets the batch time offset (in ticks). By default online payments will be grouped into batches with a start time
        /// of 12:00:00 AM.  However if the payment gateway groups transactions into batches based on a different
        /// time, this offset can specified so that Rock will use the same time when creating batches for online
        /// transactions
        /// </summary>
        public string BatchTimeOffsetTicks { get; set; }

        /// <summary>
        /// Gets or sets the user defined description of the FinancialGateway.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type of the gateway entity.
        /// </summary>
        public ListItemBag EntityType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the (internal) Name of the FinancialGateway. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the batch schedule whether Weekly or Daily.
        /// </summary>
        /// <value>
        /// The batch schedule.
        /// </value>
        public string BatchSchedule { get; set; }

        /// <summary>
        /// Gets or sets the batch start day if BtachSchedule is set to Weekly.
        /// </summary>
        /// <value>
        /// The batch start day.
        /// </value>
        public string BatchStartDay { get; set; }
    }
}
