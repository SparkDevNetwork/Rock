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

using System;

namespace Rock.Model
{
    /// <summary>
    /// Flag indicating who should be notified on a new registration
    /// </summary>
    [Flags]
    public enum RegistrationNotify
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,

        /// <summary>
        /// The registration contact
        /// </summary>
        RegistrationContact = 1,

        /// <summary>
        /// The group followers
        /// </summary>
        GroupFollowers = 2,

        /// <summary>
        /// The group leaders
        /// </summary>
        GroupLeaders = 4,

        /// <summary>
        /// All
        /// </summary>
        All = RegistrationContact | GroupFollowers | GroupLeaders
    }
}
