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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Finance.FinancialAccountDetail
{
    public class FinancialAccountParticipantBag
    {
        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The persobn alias.
        /// </value>
        public ListItemBag PersonAlias { get; set; }
        /// <summary>
        /// Gets or sets the full name of the person.
        /// </summary>
        /// <value>
        /// The full name of the person.
        /// </value>
        public string PersonFullName { get; set; }
        /// <summary>
        /// Gets or sets the purpose key.
        /// </summary>
        /// <value>
        /// The purpose key.
        /// </value>
        public string PurposeKey { get; set; }

        /// <summary>
        /// Gets or sets the purpose key description.
        /// </summary>
        /// <value>
        /// The purpose key description.
        /// </value>
        public string PurposeKeyDescription { get; set; }
    }
}
