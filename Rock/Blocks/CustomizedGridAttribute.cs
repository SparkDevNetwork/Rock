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

using Rock.Attribute;

namespace Rock.Blocks
{
    /// <summary>
    /// Identifies a block type as supporting custom grid features.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public class CustomizedGridAttribute : System.Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether to allow the administrator
        /// to force sticky header to be enabled.
        /// </summary>
        /// <value><c>true</c> if sticky header option is supported; otherwise, <c>false</c>.</value>
        public bool IsStickyHeaderSupported { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to allow the administrator
        /// to add custom actions.
        /// </summary>
        /// <value><c>true</c> if custom actions are supported; otherwise, <c>false</c>.</value>
        public bool IsCustomActionsSupported { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to allow the administrator
        /// to add custom columns.
        /// </summary>
        /// <value><c>true</c> if custom columns are supported; otherwise, <c>false</c>.</value>
        public bool IsCustomColumnsSupported { get; set; } = true;
    }
}
