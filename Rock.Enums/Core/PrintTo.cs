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

namespace Rock.Model
{
    /// <summary>
    /// Where a label should be printed
    /// </summary>
    [Enums.EnumDomain( "Core" )]
    public enum PrintTo
    {
        /// <summary>
        /// Print to the default printer
        /// </summary>
        Default = 0,

        /// <summary>
        /// Print to the printer associated with the selected kiosk
        /// </summary>
        Kiosk = 1,

        /// <summary>
        /// Print to the printer associated with the selected location
        /// </summary>
        Location = 2
    }
}
