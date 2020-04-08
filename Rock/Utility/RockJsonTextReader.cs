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
using System.IO;

using Newtonsoft.Json;

namespace Rock.Utility
{
    /// <summary>
    /// A JsonTextReader that is aware of SerializeInSimpleMode
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.JsonTextReader" />
    public class RockJsonTextReader : JsonTextReader
    {
        /// <summary>
        /// Gets or sets a value indicating whether [serialize in simple mode].
        /// </summary>
        /// <value>
        /// <c>true</c> if [serialize in simple mode]; otherwise, <c>false</c>.
        /// </value>
        public bool DeserializeInSimpleMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockJsonTextReader"/> class.
        /// </summary>
        /// <param name="textReader">The text reader.</param>
        /// <param name="deserializeInSimpleMode">if set to <c>true</c> [deserialize in simple mode].</param>
        public RockJsonTextReader( TextReader textReader, bool deserializeInSimpleMode ) 
            : base( textReader )
        {
            DeserializeInSimpleMode = deserializeInSimpleMode;
        }

        /// <summary>
        /// Deserializes the object in simple mode.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">value</exception>
        public static T DeserializeObjectInSimpleMode<T>( string value )
        {
            T obj;

            if ( value == null )
            {
                throw new ArgumentNullException( "value" );
            }

            JsonSerializer jsonSerializer = JsonSerializer.CreateDefault( (JsonSerializerSettings)null );
            jsonSerializer.CheckAdditionalContent = true;
            using ( var jsonTextReader = new RockJsonTextReader( new StringReader( value ), true ) )
            {
                obj = jsonSerializer.Deserialize<T>( jsonTextReader );
            }

            return obj;
        }
    }
}
