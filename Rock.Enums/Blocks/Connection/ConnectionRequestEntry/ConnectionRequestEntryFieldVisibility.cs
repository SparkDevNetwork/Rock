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

namespace Rock.Enums.Blocks.Connection.ConnectionRequestEntry
{
    /// <summary>
    /// Describes how a configurable field on the Connection Request Entry form is presented to the visitor.
    /// </summary>
    public enum ConnectionRequestEntryFieldVisibility
    {
        /// <summary>
        /// The field is not shown on the form.
        /// </summary>
        Hidden = 0,

        /// <summary>
        /// The field is shown and may be left blank.
        /// </summary>
        Optional = 1,

        /// <summary>
        /// The field is shown and must be provided to submit.
        /// </summary>
        Required = 2
    }
}
