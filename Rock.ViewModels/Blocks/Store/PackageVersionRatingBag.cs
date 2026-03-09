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

namespace Rock.ViewModels.Blocks.Store.PackageDetail
{
    /// <summary>
    /// Represents a user-submitted rating for a package version.
    /// </summary>
    public class PackageVersionRatingBag
    {
        /// <summary>
        /// Rating identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Star rating value.
        /// </summary>
        public int? Rating { get; set; }

        /// <summary>
        /// Name of the reviewer.
        /// </summary>
        public string ReviewerName { get; set; }

        /// <summary>
        /// Review text.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Date the rating was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Photo of the reviewer
        /// </summary>
        public string ReviewerPhoto { get; set; }
    }
}