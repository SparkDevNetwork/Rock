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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Core.ServiceJobDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceJobDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the job type options.
        /// </summary>
        /// <value>
        /// The job type options.
        /// </value>
        public List<ListItemBag> JobTypeOptions { get; set; }

        /// <summary>
        /// Gets or sets the notification status options.
        /// </summary>
        /// <value>
        /// The notification status options.
        /// </value>
        public List<ListItemBag> NotificationStatusOptions { get; set; }
    }
}
