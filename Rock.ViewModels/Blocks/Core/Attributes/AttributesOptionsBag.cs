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
using Rock.ViewModels.Utility;
namespace Rock.ViewModels.Blocks.Core.Attributes
{
    /// <summary>
    /// The additional configuration options for the Attributes block.
    /// </summary>
    public class AttributesOptionsBag
    {
        /// <summary>
        /// The names of the columns that should be hidden based on the configuration
        /// </summary>
        public List<string> HideColumns { get; set; }

        /// <summary>
        /// Identifier for the EntityType that the attributes are for
        /// </summary>
        public Guid? EntityTypeGuid { get; set; }

        /// <summary>
        /// Whether or not to show the "Show In Grid" checkbox in the attribute editor
        /// </summary>
        public bool EnableShowInGrid { get; set; }

        /// <summary>
        /// Whether or not to allow attribute values to be set
        /// </summary>
        public bool AllowSettingOfValues { get; set; }

        /// <summary>
        /// The list of EntityTypes that can be chosen from
        /// </summary>
        public List<ListItemBag> EntityTypes { get; set; }
    }
}