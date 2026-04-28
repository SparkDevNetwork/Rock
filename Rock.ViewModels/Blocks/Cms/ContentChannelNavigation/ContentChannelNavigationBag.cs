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

namespace Rock.ViewModels.Blocks.Cms.ContentChannelNavigation
{
    /// <summary>
    /// The initialization data for the Content Channel Navigation block.
    /// </summary>
    public class ContentChannelNavigationBag
    {
        /// <summary>
        /// Gets or sets the Guid of the initially selected channel,
        /// determined from page parameters or person preferences.
        /// </summary>
        public Guid? SelectedChannelGuid { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the initially selected category,
        /// determined from the CategoryGuid page parameter.
        /// Null if no category was specified via page parameter.
        /// </summary>
        public Guid? SelectedCategoryGuid { get; set; }
    }
}
