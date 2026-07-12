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

namespace Rock.ViewModels.Blocks.Utility.DefinedTypeCheckList
{
    /// <summary>
    /// A single checklist item representing one value of the configured Defined Type.
    /// </summary>
    public class DefinedTypeCheckListItemBag
    {
        /// <summary>
        /// Gets or sets the IdKey identifying the underlying Defined Value.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the text displayed next to the checkbox (the Defined Value's value).
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the HTML description shown when the item is expanded.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item has been checked off as complete.
        /// </summary>
        public bool IsChecked { get; set; }
    }
}
