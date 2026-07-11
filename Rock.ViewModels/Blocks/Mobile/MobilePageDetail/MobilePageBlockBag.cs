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

namespace Rock.ViewModels.Blocks.Mobile.MobilePageDetail
{
    /// <summary>
    /// A single block instance shown within a zone (right column).
    /// </summary>
    public class MobilePageBlockBag
    {
        /// <summary>
        /// Gets or sets the numeric identifier of the block. Used to open the
        /// legacy Block Properties modal (which is keyed by the integer id).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier key of the block.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the block. Used for the block
        /// properties and security dialogs and when launching custom block actions.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the block's type. Used when
        /// launching a custom block action, which needs the block type context.
        /// </summary>
        public Guid BlockTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the order of the block within its zone. The client sorts
        /// the blocks in each zone by this value rather than by their position
        /// in the list.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the name of the block instance.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the block type (shown as the block's subtitle).
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class for the block.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block processes Lava on the server.
        /// </summary>
        public bool ProcessLavaOnServer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block processes Lava on the client.
        /// </summary>
        public bool ProcessLavaOnClient { get; set; }

        /// <summary>
        /// Gets or sets the cache duration, in seconds. A value of zero means caching
        /// is not enabled.
        /// </summary>
        public int CacheDuration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block is shown on phones.
        /// </summary>
        public bool ShowOnPhone { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block is shown on tablets.
        /// </summary>
        public bool ShowOnTablet { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block requires a network
        /// connection to display.
        /// </summary>
        public bool RequiresNetwork { get; set; }

        /// <summary>
        /// Gets or sets the custom actions provided by the block type (rendered as
        /// additional buttons on the block row).
        /// </summary>
        public List<MobilePageBlockActionBag> CustomActions { get; set; }
    }
}
