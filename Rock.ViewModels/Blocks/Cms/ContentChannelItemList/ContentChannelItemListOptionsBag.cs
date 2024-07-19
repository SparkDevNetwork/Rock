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

using Rock.Model;

namespace Rock.ViewModels.Blocks.Cms.ContentChannelItemList
{
    /// <summary>
    /// The additional configuration options for the Content Channel Item List block.
    /// </summary>
    public class ContentChannelItemListOptionsBag
    {
        /// <summary>
        ///
        /// </summary>
        public string ContentItemName { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool IncludeTime { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool IsManuallyOrdered { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool IsContentLibraryEnabled { get; set; }

        /// <summary>
        ///
        /// </summary>
        public ContentChannelDateType DateType { get; set; }
        
        /// <summary>
        ///
        /// </summary>
        public int ContentChannelId { get; set; }
        
        /// <summary>
        ///
        /// </summary>
        public Guid LibraryLicenseGuid { get; set; }
        
        /// <summary>
        ///
        /// </summary>
        public string LibraryLicenseName { get; set; }
        
        /// <summary>
        ///
        /// </summary>
        public bool ShowFilters { get; set; }
        
        /// <summary>
        ///
        /// </summary>
        public bool ShowReorderColumn { get; set; }
        
        /// <summary>
        ///
        /// </summary>
        public bool ShowPriorityColumn { get; set; }
        
        /// <summary>
        ///
        /// </summary>
        public bool ShowStartDateTimeColumn { get; set; }
        
        /// <summary>
        ///
        /// </summary>
        public bool ShowExpireDateTimeColumn { get; set; }
        
        /// <summary>
        ///
        /// </summary>
        public bool ShowStatusColumn { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool ShowSecurityColumn { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool ShowOccurrencesColumn { get; set; }
    }
}
