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

namespace Rock.AI.Agent
{
    /// <summary>
    /// Specifies the user-friendly description of an AI agent skill or
    /// function. This value is displayed in the UI to help users
    /// understand what the skill does.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
    public class UserDescriptionAttribute : System.Attribute
    {
        /// <summary>
        /// The description to display in the UI for the AI agent skill or function.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDescriptionAttribute"/> class with the specified
        /// description.
        /// </summary>
        /// <param name="description">The description to display in the UI.</param>
        public UserDescriptionAttribute( string description )
        {
            Description = description;
        }
    }
}
