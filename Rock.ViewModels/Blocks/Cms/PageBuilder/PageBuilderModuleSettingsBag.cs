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

namespace Rock.ViewModels.Blocks.Cms.PageBuilder
{
    /// <summary>
    /// The settings of the module a Canvas block displays, as edited in the Page Builder.
    /// </summary>
    public class PageBuilderModuleSettingsBag
    {
        /// <summary>
        /// Gets or sets the identifier of the Canvas block that displays the module.
        /// </summary>
        public int BlockId { get; set; }

        /// <summary>
        /// Gets or sets the module's settings, which are the attributes of its module instance.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the values of the module's settings, keyed by attribute key.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}
