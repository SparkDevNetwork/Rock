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

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.AttributeValues
{
    /// <summary>
    /// Represents a single field rendered in edit mode. The fields are ordered
    /// to match the display order and indicate whether the current person may
    /// edit the value or only view it.
    /// </summary>
    public class AttributeValuesFieldBag
    {
        /// <summary>
        /// Gets or sets the attribute key used to look up the metadata and
        /// value in the edit bag's dictionaries.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person may edit
        /// this attribute's value. When <c>false</c> the field is rendered as a
        /// read-only formatted value.
        /// </summary>
        public bool CanEdit { get; set; }
    }
}
