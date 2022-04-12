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
    /// Defines the SVG content that will be used in conjunction with this item.
    /// This is not a path or URL to an SVG but the actual SVG content itself that
    /// can be injected directly into the DOM.
    /// </summary>
    /// <example>
    ///     <code>[IconSvg( "&lt;svg&gt;...&lt;/svg&gt;" )]</code>
    /// </example>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage( AttributeTargets.Class )]
    public class IconSvgAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the icon SVG data.
        /// </summary>
        /// <value>
        /// The icon SVG data.
        /// </value>
        public string IconSvg { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IconSvgAttribute"/> class.
        /// </summary>
        /// <param name="iconSvg">The icon SVG data.</param>
        public IconSvgAttribute( string iconSvg )
        {
            IconSvg = iconSvg;
        }
    }
}
