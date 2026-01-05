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

using System.ComponentModel;

namespace Rock.Enums.CheckIn
{
    /// <summary>
    /// The different ways to select adults or children in check-in. This provides
    /// a standard set of options to select all possible combinations of adults
    /// and children.
    /// </summary>
    public enum AdultsOrChildrenSelectionMode
    {
        /// <summary>
        /// Neither adults nor children are valid.
        /// </summary>
        None = 0,

        /// <summary>
        /// Both adults and children are valid.
        /// </summary>
        [Description( "Adults & Children" )]
        AdultsAndChildren = 1,

        /// <summary>
        /// Only adults are valid.
        /// </summary>
        AdultsOnly = 2,

        /// <summary>
        /// Only children are valid.
        /// </summary>
        ChildrenOnly = 3
    }
}
