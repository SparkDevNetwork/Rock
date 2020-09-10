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
using System.Collections.Generic;
using System.Dynamic;

namespace Rock
{
    /// <summary>
    /// ExpandoObject extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region ExpandoObject extension methods

        /// <summary>
        /// Creates a shallow clone of the original object.
        /// From https://stackoverflow.com/a/40470282/1755417 and https://stackoverflow.com/a/22828054/1755417
        /// </summary>
        /// <param name="original">The original.</param>
        /// <returns>A shallow clone of the original.</returns>
        public static ExpandoObject ShallowCopy( this ExpandoObject original )
        {
            var clone = new ExpandoObject();

            foreach ( var kvp in original )
            {
                ( ( IDictionary<string, object> ) clone ).Add( kvp );
            }

            return clone;
        }

        #endregion ExpandoObject extension methods
    }
}
