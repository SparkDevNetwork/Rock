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
using Rock.ViewModels.CheckIn;

namespace Rock.ViewModels.Blocks.CheckIn.CheckInKiosk
{
    /// <summary>
    /// Contains the information about a location's status for the admin screens.
    /// </summary>
    public class LocationStatusItemBag : CheckInItemBag
    {
        /// <summary>
        /// Determines if the location is currently open or closed.
        /// </summary>
        public bool IsOpen { get; set; }
    }
}
