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
using Rock.Web.Cache;

namespace Rock.Lava
{
    /// <summary>
    /// An implementation of the Rock web cache for Lava Templates
    /// </summary>
    public class WebsiteLavaTemplateCache : ItemCache<WebsiteLavaTemplateCache>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WebsiteLavaTemplateCache"/> class.
        /// </summary>
        public WebsiteLavaTemplateCache()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebsiteLavaTemplateCache"/> class.
        /// </summary>
        /// <param name="template">The parsed template.</param>
        /// <param name="sourceText">The Lava source text the template was parsed from.</param>
        internal WebsiteLavaTemplateCache( ILavaTemplate template, string sourceText )
        {
            Template = template;
            SourceText = sourceText;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Template object.
        /// </summary>
        public ILavaTemplate Template { get; set; }

        /// <summary>
        /// Gets the Lava source text the <see cref="Template"/> was parsed from,
        /// or <c>null</c> if it is not known.
        /// </summary>
        public string SourceText { get; private set; }

        #endregion
    }
}