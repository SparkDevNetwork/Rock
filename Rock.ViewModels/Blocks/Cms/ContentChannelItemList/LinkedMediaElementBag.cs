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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.ContentChannelItemList
{
    /// <summary>
    /// A bag that contains information about a linked media element for the Content Channel Item List block.
    /// </summary>
    public class LinkedMediaElementBag : ITranslateIdKey
    {
        /// <inheritdoc />
        public int? Id { get; set; }

        /// <inheritdoc />
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of this linked media element.
        /// </summary>
        public string Name { get; set; }
    }
}
