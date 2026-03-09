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

namespace Rock.ViewModels.Blocks.Engagement.ConnectionTypeDetail
{
    /// <summary>
    /// Communication settings for a connection type.
    /// </summary>
    public class ConnectionTypeCommunicationSettingsBag
    {
        /// <summary>
        /// Gets or sets the communication template category guid.
        /// </summary>
        public Guid? CommunicationTemplateCategoryGuid { get; set; }

        /// <summary>
        /// Gets or sets the SMS snippet category guid.
        /// </summary>
        public Guid? SmsSnippetCategoryGuid { get; set; }
    }
}
