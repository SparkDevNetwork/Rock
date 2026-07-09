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

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.AttributeValues
{
    /// <summary>
    /// The read-only view of the person's displayed attributes. This is the
    /// payload returned by each save action so the panel can refresh in place.
    /// </summary>
    public class AttributeValuesViewBag : IViewModelWithAttributes
    {
        /// <summary>
        /// Gets or sets the public attribute metadata, keyed by attribute key.
        /// The Rock.ViewModels.Utility.PublicAttributeBag.Order reflects the
        /// effective display order. When the block is not grouping by category
        /// the categories are cleared so the attributes render as a single
        /// flat, custom-ordered list.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the formatted attribute values, keyed by attribute key.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}
