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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rock.Utility
{
    /// <summary>
    /// Serializes only the specified fields depending on the LoadAttributes parameter of a REST call
    /// if the parameter value is 'simple' or True, only the specified fields will be specified
    /// if the paremter value is 'expanded', the object will be serialized normally
    /// </summary>
    public abstract class SimpleModeJsonConverter<T> : JsonConverter
    {
        /// <summary>
        /// The properties to serialize in simple mode
        /// </summary>
        public abstract string[] PropertiesToSerializeInSimpleMode { get; }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
        {
            var rockJsonWriter = writer as RockJsonTextWriter;
            var serializeInSimpleMode = rockJsonWriter != null && rockJsonWriter.SerializeInSimpleMode;
            IEnumerable<PropertyInfo> properties;

            if ( serializeInSimpleMode )
            {
                // only include the properties that are specified in PropertiesToSerializeInSimpleMode
                properties = value.GetType().GetProperties().Where( a => PropertiesToSerializeInSimpleMode.Contains( a.Name ) );
            }
            else
            {
                // include all properties that are DataMembers
                properties = value.GetType().GetProperties().Where( a => a.GetCustomAttribute<DataMemberAttribute>() != null );
            }

            JObject jo = new JObject();
            foreach ( PropertyInfo prop in properties )
            {
                if ( prop.CanRead )
                {
                    object propValue = prop.GetValue( value );
                    if ( propValue != null )
                    {
                        jo.Add( prop.Name, JToken.FromObject( propValue, serializer ) );
                    }
                }
            }

            jo.WriteTo( writer );
        }

        /// <summary>
        /// Gets a value indicating whether this 
        /// <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON; otherwise, <c>false</c>.
        /// </value>
        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this 
        /// <see cref="T:Newtonsoft.Json.JsonConverter" /> can write JSON.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="T:Newtonsoft.Json.JsonConverter" /> can write JSON; otherwise, <c>false</c>.
        /// </value>
        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
        {
            // CanRead returns false, so this shouldn't happen
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert( Type objectType )
        {
            return typeof( T ).IsAssignableFrom( objectType );
        }
    }
}
