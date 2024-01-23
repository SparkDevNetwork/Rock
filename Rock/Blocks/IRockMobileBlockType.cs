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

namespace Rock.Blocks
{
    /// <summary>
    /// Defines the properties and methods that all mobile blocks must implement.
    /// </summary>
    /// <seealso cref="Rock.Blocks.IRockBlockType" />
    public interface IRockMobileBlockType : IRockBlockType
    {
        /// <summary>
        /// Gets the required mobile application binary interface version.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version.
        /// </value>
        /// <remarks>
        /// This can be removed with Mobile shell version 6 is the minimum supported version.
        /// </remarks>
        [Obsolete( "Use RequiredMobileVersion instead." )]
        [RockObsolete( "1.16" )]
        int RequiredMobileAbiVersion { get; }

        /// <summary>
        /// Gets the required mobile version for this block to operate properly.
        /// </summary>
        /// <value>
        /// The required mobile version for this block to operate properly.
        /// </value>
        Version RequiredMobileVersion { get; }

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        /// <remarks>
        /// This can be removed with Mobile shell version 6 is the minimum supported version.
        /// </remarks>
        [Obsolete( "Use MobileBlockTypeGuid instead." )]
        [RockObsolete( "1.16" )]
        string MobileBlockType { get; }

        /// <summary>
        /// Gets the mobile block type unique identifier used to identify this
        /// block to the mobile shell. If not null then the value from the
        /// <see cref="Rock.SystemGuid.BlockTypeGuidAttribute"/> will be used.
        /// </summary>
        /// <value>
        /// The mobile block type unique identifier.
        /// </value>
        Guid? MobileBlockTypeGuid { get; }
    }
}
