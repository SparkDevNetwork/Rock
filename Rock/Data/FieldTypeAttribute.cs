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

namespace Rock.Data
{
    /// <summary>
    /// Custom attribute used to decorate model properties that have an associated Rock.Field.FieldType
    /// </summary>
    [AttributeUsage(AttributeTargets.Property )]
    public class FieldTypeAttribute : System.Attribute
    {
        /// <summary>
        /// Gets or sets the field type unique identifier.
        /// </summary>
        /// <value>
        /// The field type unique identifier.
        /// </value>
        public Guid FieldTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the configuration key.
        /// </summary>
        /// <value>
        /// The configuration key.
        /// </value>
        public string ConfigurationKey { get; set; }

        /// <summary>
        /// Gets or sets the configuration value.
        /// </summary>
        /// <value>
        /// The configuration value.
        /// </value>
        public string ConfigurationValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldTypeAttribute" /> class.
        /// </summary>
        /// <param name="fieldTypeGuid">The field type unique identifier.</param>
        /// <param name="configKey">The configuration key.</param>
        /// <param name="configValue">The configuration value.</param>
        public FieldTypeAttribute( string fieldTypeGuid, string configKey = null, string configValue = null )
        {
            FieldTypeGuid = new Guid( fieldTypeGuid );
            ConfigurationKey = configKey;
            ConfigurationValue = configValue;
        }
    }
}