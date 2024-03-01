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

namespace Rock.Field
{
    /*
     * 3/1/2024 - DSH
     * 
     * We intentionally do not inherit from RockGuidAttribute because that will
     * trigger false positives for duplicate guids.
     */

    /// <summary>
    /// Rock attribute to be used to specify the <see cref="Rock.Model.FieldType" />
    /// unique identifier that would be sent down to the client. This is used to allow
    /// subclasses to use a standard, common, UI for the field type.
    /// </summary>
    [System.AttributeUsage( System.AttributeTargets.Class, Inherited = true, AllowMultiple = false )]
    internal class UniversalFieldTypeGuidAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the unique identifier.
        /// </summary>
        /// <value>The unique identifier.</value>
        public Guid Guid { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalFieldTypeGuidAttribute"/> class.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        public UniversalFieldTypeGuidAttribute( string guid )
        {
            Guid = new Guid( guid );
        }
    }
}
