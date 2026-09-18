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

namespace Rock.ViewModels.Blocks.Crm.BulkUpdate
{
    /// <summary>
    /// Represents a set of attributes available for bulk updating along with the starting values for
    /// any of them whose editor cannot render from a blank value.
    /// </summary>
    public class BulkUpdateAttributesBag
    {
        /// <summary>
        /// Gets or sets the attributes available for bulk updating, in display order.
        /// </summary>
        public List<PublicAttributeBag> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the starting value for each Matrix attribute, keyed by attribute key. The
        /// Matrix editor reads its column definitions out of the value, so it cannot start from an
        /// empty string. No other field type belongs here; every one of them starts blank.
        /// </summary>
        public Dictionary<string, string> MatrixAttributeValues { get; set; }
    }
}
