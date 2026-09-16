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

namespace Rock.ViewModels.Controls
{
    /// <summary>
    /// A single data filter type that can be selected for a filtered entity type.
    /// </summary>
    public class DataFilterTypeItemBag
    {
        /// <summary>
        /// The unique identifier of the filter type entity.
        /// </summary>
        public Guid FilterTypeGuid { get; set; }

        /// <summary>
        /// The display title of the filter.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The section/group the filter belongs to in the picker.
        /// </summary>
        public string Section { get; set; }

        /// <summary>
        /// The optional descriptive text for the filter.
        /// </summary>
        public string Description { get; set; }
    }
}
