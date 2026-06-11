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

namespace Rock.ViewModels.Core.Grid
{
    /// <summary>
    /// Represents a grouping of data in a grid. This is used by a few special
    /// blocks that show grouped data in multiple grids.
    /// </summary>
    public class GroupingFieldBag
    {
        /// <summary>
        /// The unique key that identifies this data group.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Identifies the type of grouping operation that was used to create
        /// this group.
        /// </summary>
        public string Type { get; set; } // TODO - Consider enum

        /// <summary>
        /// The label that should be displayed for this data group.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The icon that should be displayed for this data group.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Additional CSS classes that will be applied to the text component.
        /// </summary>
        public string TextColorCssClass { get; set; }

        /// <summary>
        /// A URL to display a small photo icon that represents the grouped
        /// data.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// The order that this group should be displayed in, if no manual
        /// ordering has been applied.
        /// </summary>
        public int? Order { get; set; }
    }
}
