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

namespace Rock.ViewModels.Blocks.CheckIn.Configuration
{
    /// <summary>
    /// The Custom Settings Bag
    /// </summary>
    public class CustomSettingsBag
    {
        /// <summary>
        /// List of check-in areas.
        /// </summary>
        public List<string> CheckInAreas { get; set; }

        /// <summary>
        /// Configuration settings for check-in.
        /// </summary>
        public string CheckInConfiguration { get; set; }

        /// <summary>
        /// Kiosk identifier.
        /// </summary>
        public string Kiosk { get; set; }
    }
}
