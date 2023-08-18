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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// Used for configuration options for the PhoneNumberBox
    /// </summary>
    public class PhoneNumberBoxGetConfigurationOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether [show SMS opt in].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show SMS opt in]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSmsOptIn { get; set; }
    }
}
