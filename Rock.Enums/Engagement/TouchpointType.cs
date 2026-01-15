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
    /// The type of touchpoint.
    /// </summary>
    public enum TouchpointType
    {
        /// <summary>
        /// The contact should be prayed for by the individual.
        /// </summary>
        Prayer = 0,

        /// <summary>
        /// The contact should be reached out to by the individual.
        /// </summary>
        Connection = 1,

        /// <summary>
        /// A personal reminder for the individual to do something with the
        /// contact. The details of what the something is should be stored
        /// in the touchpoint note.
        /// </summary>
        Reminder = 2,

        /// <summary>
        /// The individual should be asked how their relationship with the
        /// contact is going and if there have been any changes.
        /// </summary>
        Pulse = 3,

        /// <summary>
        /// The contact has a birthday that should be recognized by the
        /// individual.
        /// </summary>
        Birthday = 4,

        /// <summary>
        /// The contact has a wedding anniversary that should be recognized by
        /// the individual.
        /// </summary>
        WeddingAnniversary = 5,

        /// <summary>
        /// The contact has a baptism anniversary that should be recognized by
        /// the individual.
        /// </summary>
        BaptismAnniversary = 6,

        /// <summary>
        /// The contact has a salvation anniversary that should be recognized by
        /// the individual.
        /// </summary>
        SalvationAnniversary = 7,
    }
}
