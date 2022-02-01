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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq.Dynamic.Core;

namespace Rock
{
    /// <summary>
    /// Extension methods for DataTable objects.
    /// </summary>
    public static partial class ExtensionMethods
    {

        /// <summary>
        /// Returns DataTable as a Dynamic.
        /// </summary>
        /// <param name="dt">The DataTable.</param>
        /// <returns></returns>
        public static List<dynamic> ToDynamic( this DataTable dt )
        {
            var dynamicDt = new List<dynamic>();
            foreach ( DataRow row in dt.Rows )
            {
                dynamic dyn = new ExpandoObject();
                dynamicDt.Add( dyn );
                foreach ( DataColumn column in dt.Columns )
                {
                    var dic = ( IDictionary<string, object> ) dyn;
                    dic[column.ColumnName] = row[column];
                }
            }
            return dynamicDt;
        }

        /// <summary>
        /// Returns the content of a DataTable as a collection of strongly-typed objects having a System.Type created at runtime.
        /// </summary>
        /// <param name="dt">The DataTable.</param>
        /// <returns>A collection of objects having a runtime System.Type matching the types specified by the table columns.</returns>
        public static IList ToDynamicTypeCollection( this DataTable dt )
        {
            // Create a new System.Type representing the DataRow, and a collection of items of that type.
            var properties = new List<DynamicProperty>();
            var columnNames = new List<string>();

            foreach ( DataColumn col in dt.Columns )
            {
                columnNames.Add( col.ColumnName );
                properties.Add( new DynamicProperty( col.ColumnName, col.DataType ) );
            }

            var rowType = DynamicClassFactory.CreateType( properties );
            var listType = typeof( List<> ).MakeGenericType( rowType );

            var list = ( IList ) Activator.CreateInstance( listType );

            // Create a collection of objects representing the rows in the table.
            foreach ( DataRow row in dt.Rows )
            {
                var listObject = Activator.CreateInstance( rowType ) as DynamicClass;
                foreach ( var columnName in columnNames )
                {
                    listObject.SetDynamicPropertyValue( columnName, row[columnName] );
                }

                list.Add( listObject );
            }

            return list;
        }
    }
}