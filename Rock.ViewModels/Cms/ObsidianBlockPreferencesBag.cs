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

using Rock.ViewModels.Core;

namespace Rock.ViewModels.Cms
{
    /// <summary>
    /// Contains details about the person preferences associated with a block.
    /// </summary>
    public class ObsidianBlockPreferencesBag
    {
        /// <summary>
        /// Gets or sets the entity type key that these preferences are scoped to.
        /// </summary>
        /// <value>The entity type key that these preferences are scoped to.</value>
        public string EntityTypeKey { get; set; }

        /// <summary>
        /// Gets or sets the entity key that these preferences are scoped to.
        /// </summary>
        /// <value>The entity key that these preferences are scoped to.</value>
        public string EntityKey { get; set; }

        /// <summary>
        /// Gets or sets the person preference values.
        /// </summary>
        /// <value>The person preference values.</value>
        public List<PersonPreferenceValueBag> Values { get; set; }

        /// <summary>
        /// Gets or sets the time stamp at which the preferences were fetched from the database.
        /// The remote device may use this field to aid caching of the preference.
        /// </summary>
        public long TimeStamp { get; set; }
    }
}
