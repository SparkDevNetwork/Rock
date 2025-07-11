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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.LavaApplicationDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class LavaApplicationBag : EntityBagBase
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
        /// Gets or sets the configuration rigging.
        /// </summary>
        /// <value>
        /// The configuration rigging.
        /// </value>
        public string ConfigurationRigging { get; set; }
    }
}
