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

namespace Rock.ViewModels.Blocks.Administration.DataAutomationSettings
{
    /// <summary>
    /// Settings that control automatically updating a family's status based on
    /// data view membership.
    /// </summary>
    public class UpdateFamilyStatusSettingsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether automatic family status updating is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the mapping of each family status to the data view that drives it.
        /// </summary>
        public List<StatusDataViewMappingBag> StatusDataViews { get; set; }
    }
}
