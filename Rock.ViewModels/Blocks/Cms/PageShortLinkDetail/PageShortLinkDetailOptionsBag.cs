﻿// <copyright>
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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.PageShortLinkDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class PageShortLinkDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the site options.
        /// </summary>
        /// <value>
        /// The site options.
        /// </value>
        public List<ListItemBag> SiteOptions { get; set; }

        /// <summary>
        /// Get the Default Domain URLs of the sites to be passed to the front end.
        /// </summary>
        public List<ListItemBag> DefaultDomainUrls { get; set; }
    }
}
