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

namespace Rock.ViewModels.Blocks.Group.GroupPlacement
{
    /// <summary>
    /// A bag containing registrant details for use in placement.
    /// </summary>
    public class RegistrantBag
    {
        /// <summary>
        /// The registrant ID key.
        /// </summary>
        public string RegistrantIdKey { get; set; }

        /// <summary>
        /// The registration instance name.
        /// </summary>
        public string RegistrationInstanceName { get; set; }

        /// <summary>
        /// The registration instance ID key.
        /// </summary>
        public string RegistrationInstanceIdKey { get; set; }

        /// <summary>
        /// The created date and time.
        /// </summary>
        public DateTimeOffset? CreatedDateTime { get; set; }

        /// <summary>
        /// The fees.
        /// </summary>
        public Dictionary<string, ListItemBag> Fees { get; set; }

        /// <summary>
        /// The attributes.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

        /// <summary>
        /// The attribute values.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }
    }

}
