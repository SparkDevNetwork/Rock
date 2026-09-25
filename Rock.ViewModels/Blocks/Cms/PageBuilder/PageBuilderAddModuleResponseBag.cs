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

namespace Rock.ViewModels.Blocks.Cms.PageBuilder
{
    /// <summary>
    /// Identifies the Canvas block the Page Builder added for a dropped module.
    /// </summary>
    public class PageBuilderAddModuleResponseBag
    {
        /// <summary>
        /// Gets or sets the identifier of the new Canvas block.
        /// </summary>
        public int BlockId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the new Canvas block, which its own block actions
        /// are addressed by.
        /// </summary>
        public Guid BlockGuid { get; set; }
    }
}
