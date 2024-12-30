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
    /// <para>
    /// An Attribute that can be used by objects that implement ISecured
    /// to exclude inherited security actions.
    /// </para>
    /// <para>
    /// This is only supported by a few select object types in Rock.
    /// </para>
    /// </summary>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true )]
    public class ExcludeSecurityActionsAttribute : System.Attribute
    {
        /// <summary>
        /// Gets or sets the actions to remove.
        /// </summary>
        /// <value>
        /// The Action.
        /// </value>
        public string[] Actions { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcludeSecurityActionsAttribute" /> class.
        /// </summary>
        /// <param name="actions">The action.</param>
        public ExcludeSecurityActionsAttribute( params string[] actions )
        {
            this.Actions = actions;
        }
    }
}
