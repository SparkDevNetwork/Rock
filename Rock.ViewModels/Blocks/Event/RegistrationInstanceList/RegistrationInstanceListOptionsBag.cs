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

namespace Rock.ViewModels.Blocks.Event.RegistrationInstanceList
{
    /// <summary>
    /// The additional configuration options for the Registration Instance List block.
    /// </summary>
    public class RegistrationInstanceListOptionsBag
    {
        /// <summary>
        /// Whether to show the wait list column.
        /// </summary>
        public bool ShowWaitList { get; set; }

        /// <summary>
        /// The name of the registration instance.
        /// </summary>
        public string RegistrationInstanceName { get; set; }

        /// <summary>
        /// Whether to show the details list column.
        /// </summary>
        public bool ShowDetailsColumn { get; set; }
    }
}
