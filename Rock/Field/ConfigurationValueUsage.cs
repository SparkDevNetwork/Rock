﻿// <copyright>
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

using Rock.Attribute;

namespace Rock.Field
{
    /// <summary>
    /// Defines the way configuration values are intended to be used.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal]
    public enum ConfigurationValueUsage
    {
        /// <summary>
        /// The configuration values are intended to be used to view an existing value.
        /// </summary>
        View = 0,

        /// <summary>
        /// The configuration values are intended to be used to edit or create a new value.
        /// </summary>
        Edit = 1,

        /// <summary>
        /// The configuration values are intended to be used in editing the
        /// configuraiton values to change the behavior of the field.
        /// </summary>
        Configure = 2
    }
}
