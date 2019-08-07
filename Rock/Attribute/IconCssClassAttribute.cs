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

namespace Rock.Attribute
{
    /// <summary>
    /// Defines a CSS icon that will be used in conjunction with this item.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class)]
    public class IconCssClassAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        public string IconCssClass { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IconCssClassAttribute"/> class.
        /// </summary>
        /// <param name="iconCssClass">The icon CSS class.</param>
        public IconCssClassAttribute( string iconCssClass )
        {
            IconCssClass = iconCssClass;
        }
    }
}
