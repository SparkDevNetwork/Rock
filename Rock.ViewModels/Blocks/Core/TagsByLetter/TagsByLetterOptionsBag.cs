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

namespace Rock.ViewModels.Blocks.Core.TagsByLetter
{
    /// <summary>
    /// Configuration options for the Tags By Letter block.
    /// </summary>
    public class TagsByLetterOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the entity type picker should be visible.
        /// When <c>true</c>, the user can select which entity type to filter tags by.
        /// </summary>
        public bool IsEntityTypePickerVisible { get; set; }

        /// <summary>
        /// Gets or sets the default entity type unique identifier to use when
        /// the block first loads. This comes from the block's Entity Type attribute setting.
        /// </summary>
        public Guid? DefaultEntityTypeGuid { get; set; }
    }
}
