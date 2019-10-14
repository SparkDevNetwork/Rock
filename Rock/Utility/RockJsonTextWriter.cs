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
using System.Globalization;
using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace Rock.Utility
{
    /// <summary>
    /// A JsonTextWriter that is aware of SerializeInSimpleMode
    /// AttributeCacheJsonConverter and AttributeValueJsonConverter use this to figure out how they should serialize
    /// </summary>
    public class RockJsonTextWriter : Newtonsoft.Json.JsonTextWriter
    {
        /// <summary>
        /// Gets or sets a value indicating whether [serialize in simple mode].
        /// </summary>
        /// <value>
        /// <c>true</c> if [serialize in simple mode]; otherwise, <c>false</c>.
        /// </value>
        public bool SerializeInSimpleMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockJsonTextWriter"/> class.
        /// </summary>
        /// <param name="textWriter">The text writer.</param>
        /// <param name="serializeInSimpleMode">if set to <c>true</c> [serialize in simple mode].</param>
        public RockJsonTextWriter( System.IO.TextWriter textWriter, bool serializeInSimpleMode )
            : base( textWriter )
        {
            SerializeInSimpleMode = serializeInSimpleMode;
        }

        /// <summary>
        /// Serializes the object in simple mode.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="formatting">The formatting.</param>
        /// <param name="jsonSettings">The json settings.</param>
        /// <returns></returns>
        public static string SerializeObjectInSimpleMode( object value, Formatting formatting, JsonSerializerSettings jsonSettings )
        {
            JsonSerializer jsonSerializer = JsonSerializer.CreateDefault( jsonSettings );
            jsonSerializer.Formatting = formatting;

            var stringWriter = new StringWriter( new StringBuilder( 256 ), CultureInfo.InvariantCulture );
            using ( var jsonTextWriter = new Rock.Utility.RockJsonTextWriter( stringWriter, true ) )
            {
                jsonTextWriter.Formatting = jsonSerializer.Formatting;
                jsonSerializer.Serialize( jsonTextWriter, value, null );
            }
            return stringWriter.ToString();
        }
    }
}
