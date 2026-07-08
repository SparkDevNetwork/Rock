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

using Rock.Model;

namespace Rock.ViewModels.Blocks.Event.RegistrantDetail
{
    /// <summary>
    /// The set of field visibility conditions that control whether a single registrant
    /// attribute is shown. Mirrors the per-field visibility data the RegistrationEntry
    /// block sends so the conditional show/hide behavior of the legacy WebForms block is
    /// preserved.
    /// </summary>
    public class RegistrantAttributeVisibilityBag
    {
        /// <summary>
        /// Gets or sets how the individual <see cref="Rules"/> are combined (all, any, all-false, any-false).
        /// </summary>
        public FilterExpressionType FilterExpressionType { get; set; }

        /// <summary>
        /// Gets or sets the conditions evaluated to determine whether the governed attribute is shown.
        /// </summary>
        public List<RegistrantAttributeVisibilityRuleBag> Rules { get; set; }
    }
}
