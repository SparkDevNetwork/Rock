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

namespace Rock.Utility
{
    /// <summary>
    /// Excludes the property or class from one or more features of the CodeGen
    /// tool.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true )]
    internal class CodeGenExcludeAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the excluded features.
        /// </summary>
        /// <value>
        /// The excluded features.
        /// </value>
        public CodeGenFeature ExcludedFeatures { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenExcludeAttribute"/> class.
        /// </summary>
        /// <param name="excludedFeatures">The code generation features to be excluded.</param>
        public CodeGenExcludeAttribute( CodeGenFeature excludedFeatures )
        {
            ExcludedFeatures = excludedFeatures;
        }
    }
}
