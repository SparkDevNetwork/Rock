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
using System.Runtime.Serialization;

namespace Rock.Field
{
    /// <summary>
    /// The Name, Description and Value of an field type's configuration items
    /// </summary>
    [Serializable]
    [DataContract]
    public class ConfigurationValue
    {
        /// <summary>
        /// Gets or sets the name. The name is used as the field label heading.
        /// </summary>
        /// <value>
        /// The name/label to use as the field label heading.
        /// </value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationValue" /> class.
        /// </summary>
        public ConfigurationValue()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationValue" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ConfigurationValue( string value )
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationValue"/> class.
        /// </summary>
        /// <param name="name">The name (used as the heading label when rendering markup).</param>
        /// <param name="description">The description.</param>
        /// <param name="value">The value.</param>
        public ConfigurationValue( string name, string description, string value )
        {
            Name = name;
            Description = description;
            Value = value;
        }
    }
}