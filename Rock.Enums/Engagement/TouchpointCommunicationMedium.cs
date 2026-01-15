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

namespace Rock.Enums.Engagement
{
    /// <summary>
    /// The communication medium used for a touchpoint.
    /// </summary>
    public enum TouchpointCommunicationMedium
    {
        /// <summary>
        /// Call (phone call, video call, etc.)
        /// </summary>
        Call = 0,

        /// <summary>
        /// Text (SMS, instant message, etc.)
        /// </summary>
        Text = 1,

        /// <summary>
        /// Email
        /// </summary>
        Email = 2,

        /// <summary>
        /// In Person
        /// </summary>
        InPerson = 3,
    }
}
