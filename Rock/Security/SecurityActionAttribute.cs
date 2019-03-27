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

namespace Rock.Security
{
    /// <summary>
    /// A class Attribute that can be used by objects that implement ISecured to add an additional security action or change the description of an action
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
    public class SecurityActionAttribute : System.Attribute
    {
        /// <summary>
        /// Gets or sets the action to add or change description for
        /// </summary>
        /// <value>
        /// The Action.
        /// </value>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the description of the action
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityActionAttribute" /> class.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="description">The description.</param>
        public SecurityActionAttribute( string action, string description )
        {
            this.Action = action;
            this.Description = description;
        }

    }
}