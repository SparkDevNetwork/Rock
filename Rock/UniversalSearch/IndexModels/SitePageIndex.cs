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

using Rock.Model;
using Rock.UniversalSearch.IndexModels.Attributes;

namespace Rock.UniversalSearch.IndexModels
{
    /// <summary>
    /// Site/Page Index
    /// </summary>
    /// <seealso cref="Rock.UniversalSearch.IndexModels.IndexModelBase" />
    public class SitePageIndex : IndexModelBase
    {
        /// <summary>
        /// Gets or sets the site identifier.
        /// </summary>
        /// <value>
        /// The site identifier.
        /// </value>
        [RockIndexField]
        public int SiteId { get; set; }

        /// <summary>
        /// Gets or sets the name of the site.
        /// </summary>
        /// <value>
        /// The name of the site.
        /// </value>
        [RockIndexField]
        public string SiteName { get; set; }

        /// <summary>
        /// Gets or sets the page title.
        /// </summary>
        /// <value>
        /// The page title.
        /// </value>
        [RockIndexField( Boost = 4 )]
        public string PageTitle { get; set; }

        /// <summary>
        /// Gets or sets the page summary.
        /// </summary>
        /// <value>
        /// The page summary.
        /// </value>
        [RockIndexField( Boost = 2 )]
        public string PageSummary { get; set; }

        /// <summary>
        /// Gets or sets the page keywords.
        /// </summary>
        /// <value>
        /// The page keywords.
        /// </value>
        [RockIndexField( Boost = 8 )]
        public string PageKeywords { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        [RockIndexField]
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the last indexed date time.
        /// </summary>
        /// <value>
        /// The last indexed date time.
        /// </value>
        [RockIndexField( Type = IndexFieldType.Date )]
        public DateTime LastIndexedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public override string IconCssClass
        {
            get
            {
                return "fa fa-desktop";
            }
        }

        /// <summary>
        /// Loads the by model.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns></returns>
        public static SitePageIndex LoadByModel( Site site )
        {
            // site page does not have a Rock model
            return null;
        }
    }
}
