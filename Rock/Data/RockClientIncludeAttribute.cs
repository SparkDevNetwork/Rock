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
using System.ComponentModel.DataAnnotations;

namespace Rock.Data
{
    /// <summary>
    /// Attribute so that the code generator knows to include this class even though it isn't a mapped entity
    /// </summary>
    public class RockClientIncludeAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the documentation message.
        /// </summary>
        /// <value>
        /// The documentation message.
        /// </value>
        public string DocumentationMessage { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockClientIncludeAttribute"/> class.
        /// </summary>
        /// <param name="documentationMessage">The documentation message.</param>
        public RockClientIncludeAttribute( string documentationMessage )
        {
            DocumentationMessage = documentationMessage;
        }
    }
}