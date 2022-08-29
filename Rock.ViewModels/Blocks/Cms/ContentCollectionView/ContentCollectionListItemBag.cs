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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.ContentCollectionView
{
    /// <summary>
    /// Contains the details about a single content collection item that can
    /// be picked by the individual in the custom settings for the Content
    /// Collection View block. This extends the standard ListItemBag to include
    /// the filters that are available to be used with this content collection.
    /// </summary>
    /// <seealso cref="ListItemBag" />
    public class ContentCollectionListItemBag : ListItemBag
    {
        /// <summary>collection
        /// Gets or sets the filters that are available to this content collection.
        /// </summary>
        /// <value>The filters that are available to this content collection.</value>
        public List<ListItemBag> Filters { get; set; }
    }
}
