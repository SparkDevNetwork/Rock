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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Cms.SiteDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class SiteDetailOptionsBag
    {
        /// <summary>
        /// The themes
        /// </summary>
        public List<ListItemBag> Themes { get; set; }

        /// <summary>
        /// Gets or sets the reserved key names.
        /// </summary>
        /// <value>
        /// The reserved key names.
        /// </value>
        public List<string> ReservedKeyNames { get; set; }
    }
}
