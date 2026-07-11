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

namespace Rock.ViewModels.Blocks.Communication.CommunicationSettings
{
    /// <summary>
    /// Contains the settings that can be edited for the Communication Settings block.
    /// </summary>
    public class CommunicationSettingsBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the system communication used
        /// for approval notification emails.
        /// </summary>
        public string ApprovalEmailTemplate { get; set; }

        /// <summary>
        /// Gets or sets the system communications available for selection as the
        /// approval notification template.
        /// </summary>
        public List<ListItemBag> ApprovalEmailTemplateOptions { get; set; }
    }
}
