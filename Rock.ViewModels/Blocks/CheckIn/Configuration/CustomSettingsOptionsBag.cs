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

using Rock.ViewModels.CheckIn;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration
{
    /// <summary>
    /// The Custom Settings Options Bag
    /// </summary>
    public class CustomSettingsOptionsBag
    {
        /// <summary>
        /// Available check-in configuration options.
        /// </summary>
        public List<ConfigurationTemplateBag> CheckInConfigurationOptions { get; set; }

        /// <summary>
        /// List of available check-in areas.
        /// </summary>
        public List<ConfigurationAreaBag> CheckInAreas { get; set; }

        /// <summary>
        /// List of campuses and kiosks.
        /// </summary>
        public List<CampusBag> CampusesAndKiosks { get; set; }
    }
}
