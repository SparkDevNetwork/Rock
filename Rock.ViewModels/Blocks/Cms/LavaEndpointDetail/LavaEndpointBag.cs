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

using Rock.Enums.Cms;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.LavaEndpointDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class LavaEndpointBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a description of the lava application.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the slug of lava application.
        /// </summary>
        public string Slug { get; set; }

        /// <summary>
        /// Gets or sets the http method.
        /// </summary>
        /// <value>
        /// The http method.
        /// </value>
        public LavaEndpointHttpMethod? HttpMethod { get; set; }

        /// <summary>
        /// Gets or sets the mode of the security.
        /// </summary>
        /// <value>
        /// The mode of the security.
        /// </value>
        public LavaEndpointSecurityMode? SecurityMode { get; set; }

        /// <summary>
        /// Gets or sets the code template.
        /// </summary>
        /// <value>
        /// The code template.
        /// </value>
        public string CodeTemplate { get; set; }

        /// <summary>
        /// Gets or sets the enabled Lava commands.
        /// </summary>
        /// <value>
        /// The enabled Lava commands.
        /// </value>
        public List<ListItemBag> EnabledLavaCommands { get; set; }

        /// <summary>
        /// Gets or sets a cache control settings.
        /// </summary>
        /// <value>
        /// The cache control settings.
        /// </value>
        public RockCacheabilityBag CacheControlHeaderSettings { get; set; }

        /// <summary>
        /// Gets or sets the rate limit period in seconds.
        /// </summary>
        /// <value>
        /// The rate limit period in seconds.
        /// </value>
        public int? RateLimitPeriodDurationSeconds { get; set; }

        /// <summary>
        /// Gets or sets the rate limit requests per period.
        /// </summary>
        /// <value>
        /// The rate limit requests per period.
        /// </value>
        public int? RateLimitRequestPerPeriod { get; set; }

        /// <summary>
        /// Gets or sets whether cross-site forgery protection is enabled or not.
        /// </summary>
        /// <value>
        /// If cross-site forgery protection is enabled or not.
        /// </value>
        public bool EnableCrossSiteForgeryProtection { get; set; }
    }
}
