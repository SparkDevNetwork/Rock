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

using Rock.Field;

namespace Rock.Attribute
{
    /// <summary>
    /// Identifies the way a <see cref="FieldType"/> is going to be used and presented in
    /// the system.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
    public class FieldTypeUsageAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the ways this field type can be used in the system.
        /// </summary>
        public FieldTypeUsage Usage { get; }

        /// <summary>
        /// Creates instance of the <see cref="FieldTypeUsageAttribute"/> class that
        /// specifies how the <see cref="FieldType"/> can be used.
        /// </summary>
        /// <param name="usage">The ways the field type can be used or presented.</param>
        public FieldTypeUsageAttribute( FieldTypeUsage usage )
        {
            Usage = usage;
        }
    }
}
