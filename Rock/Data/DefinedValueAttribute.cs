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
    /// Custom attribute used to decorate model properties that are defined values from a system defined type
    /// </summary>
    [AttributeUsage(AttributeTargets.Property )]
    public class DefinedValueAttribute : System.Attribute
    {
        /// <summary>
        /// Gets or sets the defined type GUID.
        /// </summary>
        /// <value>
        /// The defined type GUID.
        /// </value>
        public Guid? DefinedTypeGuid { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedValueAttribute"/> class.
        /// </summary>
        public DefinedValueAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedValueAttribute" /> class.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type GUID.</param>
        public DefinedValueAttribute( string definedTypeGuid )
        {
            DefinedTypeGuid = new Guid( definedTypeGuid );
        }

    }
}