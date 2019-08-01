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
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Rock.Utility
{
    /// <summary>
    /// A Json.NET contract resolver that allows specified properties to be renamed or ignored at runtime.
    /// </summary>
    public class DynamicPropertyMapContractResolver : DefaultContractResolver
    {
        private readonly Dictionary<Type, HashSet<string>> _IgnoreTypePropertyNames;
        private readonly Dictionary<Type, Dictionary<string, string>> _RemapTypePropertyNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicPropertyMapContractResolver"/> class.
        /// </summary>
        public DynamicPropertyMapContractResolver()
        {
            _IgnoreTypePropertyNames = new Dictionary<Type, HashSet<string>>();
            _RemapTypePropertyNames = new Dictionary<Type, Dictionary<string, string>>();
        }

        /// <summary>
        /// Add one or more items to the list of properties to ignore during serialization of the specified Type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="jsonPropertyNames"></param>
        public void IgnoreProperty( Type type, params string[] jsonPropertyNames )
        {
            if ( !_IgnoreTypePropertyNames.ContainsKey( type ) )
            {
                _IgnoreTypePropertyNames[type] = new HashSet<string>();
            }

            foreach ( var prop in jsonPropertyNames )
            {
                _IgnoreTypePropertyNames[type].Add( prop );
            }
        }

        /// <summary>
        /// Add a mapping to change the name of a property during serialization of the specified Type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="newJsonPropertyName">New name of the json property.</param>
        public void RenameProperty( Type type, string propertyName, string newJsonPropertyName )
        {
            if ( !_RemapTypePropertyNames.ContainsKey( type ) )
            {
                _RemapTypePropertyNames[type] = new Dictionary<string, string>();
            }

            _RemapTypePropertyNames[type][propertyName] = newJsonPropertyName;
        }

        /// <summary>
        /// Creates a <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> for the given <see cref="T:System.Reflection.MemberInfo" />.
        /// </summary>
        /// <param name="member">The member to create a <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> for.</param>
        /// <param name="memberSerialization">The member's parent <see cref="T:Newtonsoft.Json.MemberSerialization" />.</param>
        /// <returns>
        /// A created <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> for the given <see cref="T:System.Reflection.MemberInfo" />.
        /// </returns>
        protected override JsonProperty CreateProperty( MemberInfo member, MemberSerialization memberSerialization )
        {
            var property = base.CreateProperty( member, memberSerialization );

            if ( this.IsIgnored( property.DeclaringType, property.PropertyName ) )
            {
                property.ShouldSerialize = (x => false);
                property.Ignored = true;
            }

            if ( this.IsRenamed( property.DeclaringType, property.PropertyName, out var newJsonPropertyName ) )
            {
                property.PropertyName = newJsonPropertyName;
            }

            return property;
        }

        private bool IsIgnored( Type type, string jsonPropertyName )
        {
            Type ignoredType = null;

            if ( _IgnoreTypePropertyNames.ContainsKey( type ) )
            {
                ignoredType = type;
            }
            else if ( _IgnoreTypePropertyNames.ContainsKey( type.BaseType ) )
            { 
                ignoredType = type.BaseType;
            }

            if ( ignoredType == null )
            {
                return false;
            }

            return _IgnoreTypePropertyNames[ignoredType].Contains( jsonPropertyName );
        }

        private bool IsRenamed( Type type, string jsonPropertyName, out string newJsonPropertyName )
        {
            Dictionary<string, string> renames;

            if ( !_RemapTypePropertyNames.TryGetValue( type, out renames ) || !renames.TryGetValue( jsonPropertyName, out newJsonPropertyName ) )
            {
                newJsonPropertyName = null;
                return false;
            }

            return true;
        }
    }
}
