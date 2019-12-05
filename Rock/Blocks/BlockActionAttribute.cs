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
using System.Runtime.CompilerServices;

namespace Rock.Blocks
{
    /// <summary>
    /// Identifies a method on an IRockBlockType as being allowed to be called via an API.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage( AttributeTargets.Method )]
    public class BlockActionAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the name of the action.
        /// </summary>
        /// <value>
        /// The name of the action.
        /// </value>
        public string ActionName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockActionAttribute"/> class.
        /// </summary>
        /// <param name="actionName">Name of the action.</param>
        public BlockActionAttribute( [CallerMemberName] string actionName = "" )
        {
            ActionName = actionName;
        }
    }
}
