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
using Rock.Enums.Cms;

namespace Rock.ViewModels.Blocks.Cms.LibraryViewer
{
    /// <summary>
    /// The bag containing information to filter library viewer items.
    /// </summary>
    public class LibraryViewerItemFiltersBag
    {
        /// <summary>
        /// Only items with these topics will be included. Filter is ignored when empty.
        /// </summary>
        public List<Guid> Topics { get; set; }

        /// <summary>
        /// Only items from these organizations will be included. Filter is ignored when empty.
        /// </summary>
        public List<string> Organizations { get; set; }

        /// <summary>
        /// Only items with these experience levels will be included. Filter is ignored when empty.
        /// </summary>
        public List<ContentLibraryItemExperienceLevel> ExperienceLevels { get; set; }

        /// <summary>
        /// When <c>true</c>, only trending items will be included. Filter is ignored when <c>false</c>.
        /// </summary>
        public bool MustBeTrending { get; set; }

        /// <summary>
        /// When <c>true</c>, only popular items will be included. Filter is ignored when <c>false</c>.
        /// </summary>
        public bool MustBePopular { get; set; }

        /// <summary>
        /// Only items with these license types will be included. Filter is ignored when empty.
        /// </summary>
        public List<Guid> LicenseTypes { get; set; }

        /// <summary>
        /// Only items published on this date will be included. Filter is ignored when <c>null</c>.
        /// </summary>
        public DateTime? PublishedDate { get; set; }
    }
}
