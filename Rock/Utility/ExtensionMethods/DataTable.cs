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
using System.Collections.Generic;
using System.Data;
using System.Dynamic;

namespace Rock
{
    /// <summary>
    ///
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
        
    }
}