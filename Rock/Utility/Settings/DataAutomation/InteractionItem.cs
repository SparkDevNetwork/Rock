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

namespace Rock.Utility.Settings.DataAutomation
{
    /// <summary>
    /// Helper class used by the inactivate/reactivate settings 
    /// </summary>
    public class InteractionItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionItem"/> class.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="name">The name.</param>
        public InteractionItem( Guid guid, string name )
        {
            Guid = guid;
            Name = name;
            LastInteractionDays = 90;
            IsInteractionTypeEnabled = true;
        }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the last interaction days.
        /// </summary>
        /// <value>
        /// The last interaction days.
        /// </value>
        public int LastInteractionDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is interaction type enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is interaction type enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsInteractionTypeEnabled { get; set; }
    }

}