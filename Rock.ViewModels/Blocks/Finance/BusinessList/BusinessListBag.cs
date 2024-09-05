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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Finance.BusinessList
{
    /// <summary>
    /// 
    /// </summary>
    public class BusinessListBag
    {
        /// <summary>
        /// Gets or sets the ID of the businesses in the Business List
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the ID Key of the businesses in the list 
        /// </summary>
        public string IdKey { get; set; }
        /// <summary>
        /// Gets or sets the name of the business in the Business List
        /// </summary>
        public string BusinessName { get; set; }
        /// <summary>
        /// Gets or sets the contacts of the business
        /// </summary>
        public string Contacts { get; set; }
        /// <summary>
        /// Gets or sets the street address of the business
        /// </summary>
        public string Street { get; set; }
        /// <summary>
        /// Gets or sets the city of the business
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// Gets or sets the state of the business
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// Gets or sets the postal code of the business
        /// </summary>
        public string Zip { get; set; }
        /// <summary>
        /// Gets or sets the email of the business
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Gets or sets the mobile number of the business
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// Gets or sets the Campus of the business
        /// </summary>
        public ListItemBag Campus { get; set; }
    }
}
