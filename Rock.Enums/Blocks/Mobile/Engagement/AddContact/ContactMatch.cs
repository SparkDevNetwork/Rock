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

namespace Rock.Enums.Blocks.Mobile.Engagement.AddContact
{
    /// <summary>
    /// The type of match that was found for an existing contact.
    /// </summary>
    public enum ContactMatch
    {
        /// <summary>
        /// No contact was found that matches the provided information.
        /// </summary>
        NoMatch = 0,

        /// <summary>
        /// A contact was found that could potentially match the provided
        /// information.
        /// </summary>
        SoftMatch = 1,

        /// <summary>
        /// A contact was found taht definitely matches the provided
        /// information.
        /// </summary>
        HardMatch = 2
    }
}
