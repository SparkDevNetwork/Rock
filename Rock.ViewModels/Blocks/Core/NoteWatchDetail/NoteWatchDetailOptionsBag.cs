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

namespace Rock.ViewModels.Blocks.Core.NoteWatchDetail
{
    /// <summary>
    /// Contains information required to configure the NoteWatchDetail edit view.
    /// </summary>
    public class NoteWatchDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the available note type options.
        /// </summary>
        /// <value>
        /// The note type options.
        /// </value>
        public List<ListItemBag> NoteTypeOptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [hide person picker] based on the current context Group.
        /// </summary>
        /// <value>
        ///   <c>true</c> if context Group for the block is not null; otherwise, <c>false</c>.
        /// </value>
        public bool HidePersonPicker { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [disable person picker] based on the current context Person.
        /// </summary>
        /// <value>
        ///   <c>true</c> if context Person for the block is not null; otherwise, <c>false</c>.
        /// </value>
        public bool DisablePersonPicker { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [hide group picker] based on the current context Person.
        /// </summary>
        /// <value>
        ///   <c>true</c> if context Person for the block is not null; otherwise, <c>false</c>.
        /// </value>
        public bool HideGroupPicker { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [disable group picker] based on the current context Group..
        /// </summary>
        /// <value>
        ///   <c>true</c> if context Group for the block is not null; otherwise, <c>false</c>.
        /// </value>
        public bool DisableGroupPicker { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [hide panel].
        /// </summary>
        /// <value>
        ///   <c>true</c> if Context Person does not match the Note Watcher; otherwise, <c>false</c>.
        /// </value>
        public bool HidePanel { get; set; }
    }
}
