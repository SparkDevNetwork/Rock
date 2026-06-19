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

namespace Rock.ViewModels.Blocks.CheckIn.RapidAttendanceEntry
{
    /// <summary>
    /// A note entered for an individual on the entry screen.
    /// </summary>
    public class RapidAttendanceEntryNoteBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the selected note type.
        /// </summary>
        public Guid? NoteTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the text of the note.
        /// </summary>
        public string Text { get; set; }
    }
}
