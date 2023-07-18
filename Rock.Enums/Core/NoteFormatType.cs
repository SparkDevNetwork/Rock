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

namespace Rock.Enums.Core
{
    /// <summary>
    /// Specifies the format of notes that belong to the NoteType.
    /// </summary>
    public enum NoteFormatType
    {
        /// <summary>
        /// The format of the notes is unknown, but assumed to be structured.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The format of the notes is unstructured data and not compatible
        /// with some of the more advanced features of notes.
        /// </summary>
        Unstructured = 1,

        /// <summary>
        /// The format of the notes is a structured format. Only Rock components
        /// should set the note text value.
        /// </summary>
        Structured = 2
    }
}
