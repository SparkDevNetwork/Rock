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

namespace Rock.ViewModels.Blocks.Communication.CommunicationDetail
{
    /// <summary>
    /// A bag that contains the configuration options for the Communication Detail block's recipient grid.
    /// </summary>
    public class CommunicationRecipientGridOptionsBag
    {
        /// <summary>
        /// Gets or sets the person properties the individual may select to display as columns in the recipient grid.
        /// </summary>
        public List<ListItemBag> PersonPropertyItems { get; set; }

        /// <summary>
        /// Gets or sets the person attributes the individual may select to display as columns in the recipient grid.
        /// </summary>
        public List<ListItemBag> PersonAttributeItems { get; set; }
    }
}
