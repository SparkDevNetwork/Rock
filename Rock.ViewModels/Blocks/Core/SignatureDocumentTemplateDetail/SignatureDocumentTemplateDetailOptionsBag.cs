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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Core.SignatureDocumentTemplateDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class SignatureDocumentTemplateDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the communication templates.
        /// </summary>
        /// <value>
        /// The communication templates.
        /// </value>
        public List<ListItemBag> CommunicationTemplates { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show legacy external providers].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show legacy external providers]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowLegacyExternalProviders { get; set; }
    }
}
