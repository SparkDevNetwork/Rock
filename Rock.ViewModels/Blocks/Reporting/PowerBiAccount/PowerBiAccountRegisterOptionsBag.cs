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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.ViewModels.Blocks.Reporting.PowerBiAccount
{
    /// <summary>
    /// The configuration options for the Power BI Account Register block.
    /// </summary>
    public class PowerBiAccountRegisterOptionsBag
    {
        /// <summary>
        /// Gets or sets the homepage URL.
        /// </summary>
        public string HomepageUrl { get; set; }

        /// <summary>
        /// Gets or sets the redirect URL.
        /// </summary>
        public string RedirectUrl { get; set; }
    }
}