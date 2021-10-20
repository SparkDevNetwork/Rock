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

namespace Rock.Utility
{
    /// <summary>
    /// Defines a set of ordering operations that can be performed on
    /// a collection of <see cref="Rock.Model.PrayerRequest"/> objects.
    /// </summary>
    public enum PrayerRequestOrder
    {
        /// <summary>
        /// Ordered from least prayed for to most prayed for.
        /// </summary>
        LeastPrayedFor = 0,

        /// <summary>
        /// Ordered from the newest to the oldest.
        /// </summary>
        Newest = 1,

        /// <summary>
        /// Ordered from the oldest to the newest.
        /// </summary>
        Oldest = 2,

        /// <summary>
        /// Ordered randomly.
        /// </summary>
        Random = 3
    }
}
