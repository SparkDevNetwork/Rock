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

namespace Rock.ViewModel
{
    /// <summary>
    /// TypeScript Type
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage( AttributeTargets.Property, AllowMultiple = false, Inherited = true )]
    public class TypeScriptTypeAttribute : System.Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeScriptTypeAttribute" /> class.
        /// </summary>
        /// <param name="tsType">Type of the ts.</param>
        /// <param name="importStatement">The import statement.</param>
        public TypeScriptTypeAttribute( string tsType, string importStatement = null )
        {
            TsType = tsType;
            ImportStatement = importStatement;
        }

        /// <summary>
        /// Gets the type for TypeScript.
        /// </summary>
        /// <value>
        /// The type of the related entity.
        /// </value>
        public string TsType { get; }

        /// <summary>
        /// Gets the import statement.
        /// </summary>
        /// <value>
        /// The import statement.
        /// </value>
        public string ImportStatement { get; }
    }
}
