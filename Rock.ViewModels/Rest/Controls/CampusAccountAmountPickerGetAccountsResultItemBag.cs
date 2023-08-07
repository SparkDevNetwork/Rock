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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the GetAccounts API action of
    /// the ampusAccountAmountPicker control.
    /// </summary>
    public class CampusAccountAmountPickerGetAccountsResultItemBag
    {
        /// <summary>
        /// Name of the account
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// GUID of the account
        /// </summary>
        public Guid Value { get; set; }

        /// <summary>
        /// List of the ACTUAL accounts to use (if using account-campus mapping logic) for each of the campuses.
        /// The Campus GUID is the key and a ListItemBag of the account details are the value.
        /// </summary>
        public Dictionary<Guid, ListItemBag> CampusAccounts { get; set; }
    }
}
