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

namespace Rock.ViewModels.Blocks.Mobile.MobilePageDetail
{
    /// <summary>
    /// The page builder data (the "Builder" panel): the block types available in
    /// the palette and the layout zones with their placed blocks.
    /// </summary>
    public class MobilePageBuilderBag
    {
        /// <summary>
        /// Gets or sets the block types shown in the palette (left column).
        /// </summary>
        public List<MobilePageBlockTypeBag> BlockTypes { get; set; }

        /// <summary>
        /// Gets or sets the layout zones and their placed blocks (right column).
        /// </summary>
        public List<MobilePageZoneBag> Zones { get; set; }

        /// <summary>
        /// Gets or sets the error message produced when the layout's phone or
        /// tablet XAML could not be parsed. When set, <see cref="Zones"/> will be
        /// empty since zone parsing stops at the first layout that fails to parse.
        /// </summary>
        public string ZoneErrorMessage { get; set; }
    }
}
