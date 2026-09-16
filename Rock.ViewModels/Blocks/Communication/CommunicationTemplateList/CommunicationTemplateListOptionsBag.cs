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

namespace Rock.ViewModels.Blocks.Communication.CommunicationTemplateList
{
    /// <summary>
    /// The additional configuration options for the Communication Template List block.
    /// </summary>
    public class CommunicationTemplateListOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the current user is authorized
        /// to edit the block. Controls visibility of the "Created By" column.
        /// </summary>
        public bool IsBlockAuthorizedToEdit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the security column should
        /// be visible in the grid.
        /// </summary>
        public bool IsSecurityColumnVisible { get; set; }
    }
}
