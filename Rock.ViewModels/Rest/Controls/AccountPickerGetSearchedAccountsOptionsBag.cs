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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the GetSearchedAccounts API action of the AccountPicker control.
    /// </summary>
    public class AccountPickerGetSearchedAccountsOptionsBag
    {
        /// <summary>
        /// The query we're using to find accounts
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// Whether or not to include inactive accounts
        /// </summary>
        public bool IncludeInactive { get; set; } = false;

        /// <summary>
        /// Whether or not to display the public name (vs the normal name)
        /// </summary>
        public bool DisplayPublicName { get; set; } = false;

        /// <summary>
        /// The security grant token to use when performing authorization checks.
        /// </summary>
        public string SecurityGrantToken { get; set; }
    }
}
