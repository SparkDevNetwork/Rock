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
using System.Linq;
using System.Reflection;
using Rock.Utility;

namespace Rock
{
    /// <summary>
    /// System.Enum extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Gets the enum fields sorted by <see cref="EnumOrderAttribute"/>
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="enumType">Type of the enum.</param>
        /// <returns></returns>
        public static IEnumerable<TEnum> GetOrderedValues<TEnum>( this Type enumType ) where TEnum : struct
        {

            var enumFields = enumType.GetFields( BindingFlags.Public | BindingFlags.Static );
            var enumFieldsOrder = new Dictionary<TEnum, int>();
            foreach ( var enumField in enumFields )
            {
                var enumFieldOrder = enumField.GetCustomAttribute<EnumOrderAttribute>()?.Order ?? int.MaxValue;
                enumFieldsOrder.Add( ( TEnum ) enumField.GetValue( null ), enumFieldOrder );
            }

            return enumFieldsOrder.OrderBy( f => f.Value ).ThenBy( f => f.Key ).Select( a => a.Key ).ToList();
        }
    }
}
