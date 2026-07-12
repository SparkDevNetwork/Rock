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

namespace Rock.ViewModels.Blocks.Crm.PhotoSendRequest
{
    /// <summary>
    /// The initial criteria values shown when the Send Photo Request block first loads.
    /// </summary>
    public class PhotoSendRequestBag
    {
        /// <summary>
        /// Gets or sets the initial value for the "Age is more than" criteria, in years.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the initial value for the "Exclude people with a photo updated in the
        /// last (years)" criteria, in years.
        /// </summary>
        public int PhotoUpdatedYears { get; set; }
    }
}
