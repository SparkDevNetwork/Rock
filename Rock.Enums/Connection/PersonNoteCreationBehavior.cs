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

namespace Rock.Enums.Connection
{
    /// <summary>
    /// Person Note Creation Behavior
    /// </summary>
    public enum PersonNoteCreationBehavior
    {
        /// <summary>
        /// Does not create a person note.
        /// </summary>
        DoNotCreatePersonNote = 0,

        /// <summary>
        /// Asks whether to create a person note at activity creation.
        /// </summary>
        AskAtActivityCreation = 1,

        /// <summary>
        /// Always creates a person note.
        /// </summary>
        AlwaysCreateAPersonNote = 2,
    }
}
