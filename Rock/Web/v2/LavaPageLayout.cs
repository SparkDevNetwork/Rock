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
using System.Collections.Generic;

using Rock.Lava;

namespace Rock.Web.v2
{
    /// <summary>
    /// Represents a page layout that has been parsed and is ready to render.
    /// </summary>
    internal class LavaPageLayout
    {
        #region Properties

        /// <summary>
        /// The lava template that can be used to render the page.
        /// </summary>
        public ILavaTemplate Template { get; }

        /// <summary>
        /// The original source content of the Lava that was parsed
        /// into <see cref="Template"/>.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// The zones that exist in the layout and can be used when rendering
        /// blocks into the layout.
        /// </summary>
        public IReadOnlyCollection<LavaPageZone> Zones { get; }

        /// <summary>
        /// The list of file paths that were used when creating this layout.
        /// This should be used to monitor for file changes that would require
        /// this layout to be re-generated.
        /// </summary>
        public IReadOnlyList<string> Dependencies { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LavaPageLayout"/> class.
        /// </summary>
        /// <param name="template">The Lava template to be used for rendering the page layout.</param>
        /// <param name="source">The original source string for the page layout.</param>
        /// <param name="zones">A read-only collection of zones are defined in the layout.</param>
        /// <param name="dependencies">A read-only list of dependency filenames that should be watched.</param>
        public LavaPageLayout( ILavaTemplate template, string source, IReadOnlyCollection<LavaPageZone> zones, IReadOnlyList<string> dependencies )
        {
            Template = template;
            Source = source;
            Zones = zones;
            Dependencies = dependencies;
        }

        #endregion
    }
}
