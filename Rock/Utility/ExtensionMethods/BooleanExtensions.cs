// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;

namespace Rock
{
    /// <summary>
    /// Boolean Extensions
    /// </summary>
    public static class BooleanExtensions
    {
        #region Boolean Extensions

        /// <summary>
        /// Returns a numeric 1 (if true) or 0 (if false).
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static int Bit( this Boolean field )
        {
            return field ? 1 : 0;
        }

        /// <summary>
        /// Returns either "Yes" or "No".
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <returns></returns>
        public static string ToYesNo( this bool value )
        {
            return value ? "Yes" : "No";
        }

        /// <summary>
        /// Returns the string "True" or "False".
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <returns></returns>
        public static string ToTrueFalse( this bool value )
        {
            return value ? "True" : "False";
        }

        /// <summary>
        /// Use AsBoolean() instead.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [Obsolete( "Use AsBoolean() instead" )]
        public static bool FromTrueFalse( this string value )
        {
            return value.Equals( "True" );
        }

        #endregion Boolean Extensions
    }
}
