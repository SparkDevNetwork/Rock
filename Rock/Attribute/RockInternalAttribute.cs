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

namespace Rock.Attribute
{
    /// <summary>
    /// <para>
    /// Marks an API as internal to Rock. These APIs are not subject to the same
    /// compatibility standards as public APIs. It may be changed or removed
    /// without notice in any release. You should not use such APIs directly in
    /// any plug-ins. Doing so can result in application failures when updating
    /// to a new Rock release.
    /// </para>
    /// <para>
    /// When marking items as RockInternal, provide the version number it was
    /// introduced in. This allows the team to review these later and decide if
    /// it is safe to make public now.
    /// </para>
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Enum
        | AttributeTargets.Class
        | AttributeTargets.Struct
        | AttributeTargets.Interface
        | AttributeTargets.Event
        | AttributeTargets.Field
        | AttributeTargets.Method
        | AttributeTargets.Delegate
        | AttributeTargets.Property
        | AttributeTargets.Constructor )]
    public sealed class RockInternalAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the Rock version this item was introduced in.
        /// </summary>
        /// <value>The Rock version this item was introduced in.</value>
        public string IntroducedInVersion { get; }

        /// <summary>
        /// Gets a value indicating whether the item should stay internal forever.
        /// </summary>
        /// <value><c>true</c> if the item should stay internal forever; otherwise, <c>false</c>.</value>
        public bool KeepInternalForever { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockInternalAttribute"/> class.
        /// </summary>
        /// <param name="introducedInVersion">The version this item was introduced in.</param>
        public RockInternalAttribute( string introducedInVersion )
        {
            IntroducedInVersion = introducedInVersion;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockInternalAttribute"/> class.
        /// </summary>
        /// <param name="introducedInVersion">The version this item was introduced in.</param>
        /// <param name="keepInternalForever"><c>true</c> if the item is intended to stay internal forever.</param>
        public RockInternalAttribute( string introducedInVersion, bool keepInternalForever )
        {
            IntroducedInVersion = introducedInVersion;
            KeepInternalForever = keepInternalForever;
        }
    }
}
