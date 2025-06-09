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

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Rock.Rest.Utility
{
    /// <summary>
    /// Internal methods to help blocks that need some special access to the
    /// REST system.
    /// </summary>
    internal class BlockUtilities
    {
        /// <summary>
        /// Special internal helper method to allow certain block actions to
        /// encode an object as it would appear if sent over the API.
        /// </summary>
        /// <param name="value">The value to be serialized.</param>
        /// <param name="defaultPropertyNames"><c>true</c> if property names should be written as-is instead of converted to camelCase.</param>
        /// <returns>A string that contains the serialzied data.</returns>
        internal static string ToV2ResponseJson( object value, bool defaultPropertyNames = false )
        {
            var formatter = ApiPickerJsonMediaTypeFormatter.CreateV2Formatter();

            if ( defaultPropertyNames )
            {
                if ( formatter.SerializerSettings.ContractResolver is DefaultContractResolver defaultContractResolver )
                {
                    defaultContractResolver.NamingStrategy = new DefaultNamingStrategy();
                }
            }

            return JsonConvert.SerializeObject( value, formatter.SerializerSettings );
        }
    }
}
