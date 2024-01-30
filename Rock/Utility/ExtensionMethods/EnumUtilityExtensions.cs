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
using Rock.Utility;
using Rock.ViewModels.Utility;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Reflection;

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


        /// <summary>
        /// Only check for the value if the source enumerable is not null or empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if the enumerable is null, empty, or if the value is found in the list.</returns>
        public static bool ContainsOrEmpty<T>( this IEnumerable<T> enumerable, T value )
        {
            return enumerable == null || enumerable.Any() == false || enumerable.Contains( value );
        }

        /// <summary>
        /// An extension function to get a ListItemBag array from an enum.
        /// </summary>
        /// <param name="enumType">The type of the enum to be converted to ListItemBag</param>
        /// <returns>An array of ListItemBag</returns>
        public static List<ListItemBag> ToEnumListItemBag( this Type enumType )
        {
            var listItemBag = new List<ListItemBag>();
            foreach ( Enum enumValue in Enum.GetValues(enumType))
            {
                var text = enumValue.GetDescription() ?? enumValue.ToString().SplitCase();
                var value = enumValue.ConvertToInt().ToString();
                listItemBag.Add( new ListItemBag { Text = text, Value = value } );
            }

            return listItemBag.ToList();
        }

        /// <summary>
        /// Converts the IEnumerable <see cref="int"/> to a dbo.EntityIdList <see cref="SqlParameter"/> populated with values.
        /// </summary>
        /// <param name="entityIds">An The enumerable of int to convert.</param>
        /// <param name="parameterName">The name of the Sql Parameter to be set.</param>
        /// <returns>A SqlParameter of Type dbo.EntityIdList whose values are those of this enumerable.</returns>
        public static SqlParameter ConvertToEntityIdListParameter( this IEnumerable<int> entityIds, string parameterName )
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add( "EntityId", typeof( int ) );

            foreach ( var value in entityIds )
            {
                dataTable.Rows.Add( value );
            }

            return new SqlParameter( parameterName, SqlDbType.Structured )
            {
                TypeName = "dbo.EntityIdList",
                Value = dataTable
            };
        }

    }
}
