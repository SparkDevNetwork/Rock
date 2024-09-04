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

namespace Rock.Enums.Cms
{
    /// <summary>
    /// Flags for the <see cref="Rock.Model.SiteType"/>. This enum was introduced with the motivation to improve performance.
    /// Prior to this, reflection was the only way to get the SiteTypes on a BlockType which turned out to be expensive.
    /// With this enum, the computed SiteTypes could be stored in the database to be read later.
    /// The developer should ensure that the values in this enum is in sync with the ones in <see cref="Rock.Model.SiteType"/> enum.
    /// </summary>
    [Flags]
    public enum SiteTypeFlags
    {
        /// <summary>
        /// The default Site Type flag.
        /// </summary>
        None = 0,

        /// <summary>
        /// Websites
        /// </summary>
        Web = 0x0001,

        /// <summary>
        /// Mobile applications
        /// </summary>
        Mobile = 0x0002,

        /// <summary>
        /// TV Apps
        /// </summary>
        Tv = 0x0004
    }
}
