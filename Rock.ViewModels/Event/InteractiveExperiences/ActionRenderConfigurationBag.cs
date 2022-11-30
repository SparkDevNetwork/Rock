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

using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Event.InteractiveExperiences
{
    /// <summary>
    /// The configuration required to display an action.
    /// </summary>
    public class ActionRenderConfigurationBag
    {
        /// <summary>
        /// Gets or sets the action type unique identifier.
        /// </summary>
        /// <value>The action type unique identifier.</value>
        public Guid ActionTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the configuration values for the action type.
        /// </summary>
        /// <value>The configuration values for the action type.</value>
        public Dictionary<string, string> ConfigurationValues { get; set; }
    }
}
