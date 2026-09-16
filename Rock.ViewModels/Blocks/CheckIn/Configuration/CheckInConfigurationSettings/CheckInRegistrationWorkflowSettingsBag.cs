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

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInConfigurationSettings
{
    /// <summary>
    /// The registration workflow settings for a check-in configuration. Configures optional workflows to
    /// run when a new family or person is registered.
    /// </summary>
    public class CheckInRegistrationWorkflowSettingsBag
    {
        /// <summary>
        /// Gets or sets the workflow types that are launched when a new family is registered during
        /// check-in.
        /// </summary>
        public List<ListItemBag> NewFamilyWorkflowTypes { get; set; }

        /// <summary>
        /// Gets or sets the workflow types that are launched when a new person is registered during
        /// check-in.
        /// </summary>
        public List<ListItemBag> NewPersonWorkflowTypes { get; set; }
    }
}
