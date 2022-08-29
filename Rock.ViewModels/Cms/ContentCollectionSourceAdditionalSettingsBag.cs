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

namespace Rock.ViewModels.Cms
{
    /// <summary>
    /// The settings that describe additional details about a content collection
    /// source that will be used during indexing.
    /// </summary>
    public class ContentCollectionSourceAdditionalSettingsBag
    {
        /// <summary>
        /// Gets or sets the attribute unique identifiers that are included
        /// for indexing from this source.
        /// </summary>
        /// <value>
        /// The attribute unique identifiers that are included for indexing.
        /// </value>
        public List<Guid> AttributeGuids { get; set; }
    }
}
