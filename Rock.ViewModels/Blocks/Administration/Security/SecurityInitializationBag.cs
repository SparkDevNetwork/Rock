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

namespace Rock.ViewModels.Blocks.Administration.Security
{
    /// <summary>
    /// The data used to initialize the Security block for a specific secured entity.
    /// </summary>
    public class SecurityInitializationBag
    {
        /// <summary>
        /// Gets or sets the security actions (tabs) supported by the secured
        /// entity, ordered for display.
        /// </summary>
        public List<SecurityActionBag> Actions { get; set; }

        /// <summary>
        /// Gets or sets the action that should be selected when the block first loads.
        /// </summary>
        public string CurrentAction { get; set; }

        /// <summary>
        /// Gets or sets the permission data for the initially selected action.
        /// </summary>
        public SecurityActionDataBag ActionData { get; set; }
    }
}
