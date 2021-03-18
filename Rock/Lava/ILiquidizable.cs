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
using System.Collections.Generic;

namespace Rock.Lava
{
    /// <summary>
    /// Represents an object model that can be used with Lava.
    /// This is a Rock-specific superset of the same-named DotLiquid interface.
    /// </summary>
    [RockObsolete( "13.0" )] // "Implement the Rock.Lava.ILavaDataDictionary interface instead."
    public interface ILiquidizable : global::DotLiquid.ILiquidizable, global::DotLiquid.IIndexable
    {
        /// <summary>
        /// Gets the available keys (for debugging info).
        /// </summary>
        /// <value>
        /// The available keys.
        /// </value>
        List<string> AvailableKeys { get; }
    }
}