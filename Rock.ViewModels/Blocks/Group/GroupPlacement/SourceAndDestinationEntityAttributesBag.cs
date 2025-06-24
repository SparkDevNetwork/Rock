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

namespace Rock.ViewModels.Blocks.Group.GroupPlacement
{
    /// <summary>
    /// A bag containing attribute data for both source and destination entities in the placement process.
    /// </summary>
    public class SourceAndDestinationEntityAttributesBag
    {
        /// <summary>
        /// The source entity attributes.
        /// </summary>
        public Dictionary<string, AttributeDataBag> SourceEntityAttributes { get; set; }

        /// <summary>
        /// The destination entity attributes.
        /// </summary>
        public Dictionary<string, AttributeDataBag> DestinationEntityAttributes { get; set; }
    }

}
