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
    /// Represents an object that can be accessed as a dictionary of values by the Lava Engine.
    /// </summary>
    public interface ILavaDataDictionary
    {
        /// <summary>
        /// Returns the data value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object GetValue( string key );

        /// <summary>
        /// Returns a flag indicating if this data object contains a value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool ContainsKey( string key );

        /// <summary>
        /// Gets a list of the keys defined by this data object.
        /// </summary>
        List<string> AvailableKeys { get; }

        /// <summary>
        /// Gets a string representation of the object.
        /// </summary>
        /// <returns></returns>
        string ToString();
    }
}
