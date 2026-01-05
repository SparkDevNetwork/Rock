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

namespace Rock.ViewModels.Blocks.Communication.CommunicationDetail
{
    /// <summary>
    /// A bag that contains information about the recipient grid settings for the communication detail block.
    /// </summary>
    public class CommunicationRecipientGridSettingsBag
    {
        /// <summary>
        /// Gets or sets the names of the person properties to display in the recipient grid.
        /// </summary>
        public List<string> SelectedProperties { get; set; }

        /// <summary>
        /// Gets or sets the unique identifiers of the person attributes to display in the recipient grid.
        /// </summary>
        public List<Guid> SelectedAttributes { get; set; }
    }
}
