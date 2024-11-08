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

namespace Rock.Lava.Filters
{
    public static partial class TemplateFilters
    {
        /// <summary>
        /// Converts the input to a Boolean(true/false) value.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="resultIfNullOrEmpty"></param>
        /// <returns></returns>
        public static bool AsBoolean( object input, bool resultIfNullOrEmpty = false )
        {
            return input.ToStringSafe().AsBoolean( resultIfNullOrEmpty );
        }

        /// <summary>
        /// Converts the input to a decimal value, or 0 if the conversion is unsuccessful.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static decimal AsDecimal( object input )
        {
            return input.ToStringSafe().AsDecimal();
        }

        /// <summary>
        /// Converts the input to a double-precision value, or 0 if the conversion is unsuccessful.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static double AsDouble( object input )
        {
            return input.ToStringSafe().AsDouble();
        }

        /// <summary>
        /// Converts the input to an integer value, or 0 if the conversion is unsuccessful.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int AsInteger( object input )
        {
            return input.ToStringSafe().AsInteger();
        }

        /// <summary>
        /// Converts the input to a Guid value, or null if the conversion is unsuccessful.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Guid? AsGuid( object input )
        {
            return input.ToStringSafe().AsGuidOrNull();
        }
    }
}
