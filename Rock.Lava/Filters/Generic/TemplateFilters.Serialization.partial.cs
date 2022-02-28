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

namespace Rock.Lava.Filters
{
    public static partial class TemplateFilters
    {
        /// <summary>
        /// Returns a JSON representation of the object.
        /// See https://www.rockrms.com/page/565#tojson
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToJSON( object input )
        {
            return input.ToJson( indentOutput: true );
        }

        /// <summary>
        /// Returns a dynamic object from a JSON string.
        /// See https://www.rockrms.com/page/565#fromjson
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static object FromJSON( object input )
        {
            return input.ToStringSafe().FromJsonDynamicOrNull();
        }

    }
}
