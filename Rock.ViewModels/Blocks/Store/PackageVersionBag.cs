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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Store.PackageDetail
{
    /// <summary>
    /// Represents a single package version.
    /// </summary>
    public class PackageVersionBag
    {
        /// <summary>
        /// Unique identifier for the version.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Human-readable version label (e.g., "1.2.0").
        /// </summary>
        public string VersionLabel { get; set; }

        /// <summary>
        /// Date the version was added to the store.
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Display version of the date
        /// </summary>
        public string DisplayDate { get; set; }

        /// <summary>
        /// Release notes or description for the version.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// URL to documentation specific to this version.
        /// </summary>
        public string DocumentationUrl { get; set; }

        /// <summary>
        /// Pre-formatted Rock version requirement string.
        /// </summary>
        public string RequiredRockVersionDisplay { get; set; }

        /// <summary>
        /// Screenshots associated with this version.
        /// </summary>
        public List<string> ScreenshotURLs { get; set; }

        /// <summary>
        /// Rating of the specific version
        /// </summary>
        public int? Rating { get; set; }
    }
}