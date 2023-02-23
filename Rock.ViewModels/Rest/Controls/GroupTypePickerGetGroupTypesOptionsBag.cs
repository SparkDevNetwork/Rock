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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the GetGroupTypes API action of
    /// the GroupTypePicker control.
    /// </summary>
    public class GroupTypePickerGetGroupTypesOptionsBag
    {
        /// <summary>
        /// List of GUIDs for the group types we want to retrieve
        /// </summary>
        public List<Guid> GroupTypes { get; set; }

        /// <summary>
        /// Whether the results are sorted by name or order
        /// </summary>
        public bool IsSortedByName { get; set; } = false;
    }
}
