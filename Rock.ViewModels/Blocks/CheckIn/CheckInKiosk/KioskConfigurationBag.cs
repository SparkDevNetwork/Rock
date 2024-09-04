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
using System.Collections.Generic;

using Rock.ViewModels.CheckIn;

namespace Rock.ViewModels.Blocks.CheckIn.CheckInKiosk
{
    /// <summary>
    /// Details about the configuration of a kiosk that will be used for check-in.
    /// This is intended to contain everything required for the kiosk to start.
    /// </summary>
    public class KioskConfigurationBag
    {
        /// <summary>
        /// Gets or sets the kiosk details.
        /// </summary>
        /// <value>The kiosk details.</value>
        public WebKioskBag Kiosk { get; set; }

        /// <summary>
        /// Gets or sets the check-in template.
        /// </summary>
        /// <value>The check-in template.</value>
        public ConfigurationTemplateBag Template { get; set; }

        /// <summary>
        /// Gets or sets the enabled areas.
        /// </summary>
        /// <value>The enabled areas.</value>
        public List<CheckInItemBag> Areas { get; set; }
    }
}
