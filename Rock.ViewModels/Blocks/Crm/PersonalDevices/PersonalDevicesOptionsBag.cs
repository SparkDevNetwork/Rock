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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.PersonalDevices
{
	/// <summary>
	/// Options for the Personal Devices block.
	/// </summary>
	public class PersonalDevicesOptionsBag
	{
        /// <summary>
        /// Gets or sets a list of the personal device type options used for filtering.
        /// </summary>
        public List<ListItemBag> DeviceTypeOptions { get; set; }
    }
}


