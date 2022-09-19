﻿// <copyright>
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

namespace Rock.ViewModels.Blocks.WorkFlow.FormBuilder.FormTemplateDetail
{
    /// <summary>
    /// Represents the sources of truth for various pickers and lists of entities
    /// that will be used by the JavaScript code.
    /// </summary>
    public class ValueSourcesViewModel
    {
        /// <summary>
        /// The list of campus type options that are available to pick from.
        /// </summary>
        public List<ListItemBag> CampusTypeOptions { get; set; }

        /// <summary>
        /// The list of campus status options that are available to pick from.
        /// </summary>
        public List<ListItemBag> CampusStatusOptions { get; set; }

        /// <summary>
        /// The list of record status options that are available to pick from.
        /// </summary>
        public List<ListItemBag> RecordStatusOptions { get; set; }

        /// <summary>
        /// The list of connection status options that are available to pick from.
        /// </summary>
        public List<ListItemBag> ConnectionStatusOptions { get; set; }

        /// <summary>
        /// The list of address type options that are available to pick from.
        /// </summary>
        public List<ListItemBag> AddressTypeOptions { get; set; }

        /// <summary>
        /// The list of e-mail template options that are available to pick from.
        /// </summary>
        public List<ListItemBag> EmailTemplateOptions { get; set; }
    }
}
