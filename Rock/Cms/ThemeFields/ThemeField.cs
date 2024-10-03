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

using System.Diagnostics.CodeAnalysis;

using Rock.Enums.Cms;

namespace Rock.Cms.ThemeFields
{
    /// <summary>
    /// A generic field that will appear in the theme editor UI.
    /// </summary>
    internal abstract class ThemeField
    {
        /// <summary>
        /// The type of field of this instance.
        /// </summary>
        public ThemeFieldType Type { get; }

        /// <summary>
        /// Creates a new instance of <see cref="ThemeField"/>.
        /// </summary>
        /// <param name="type">The type of field of this instance.</param>
        public ThemeField( ThemeFieldType type )
        {
            Type = type;
        }

        /// <summary>
        /// Add any CSS overrides to the builder. This can be either variables
        /// or custom content.
        /// </summary>
        /// <param name="builder">The builder that will generate the CSS overrides file.</param>
        [ExcludeFromCodeCoverage]
        public virtual void AddCssOverrides( IThemeOverrideBuilder builder )
        {
        }
    }
}
