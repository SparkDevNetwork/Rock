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
    /// The MICR Status
    /// </summary>
    [Enums.EnumDomain( "Finance" )]
    public enum MICRStatus
    {
        /// <summary>
        /// Success means the scanned MICR contains no invalid read chars ('!' for Canon and '?' for Magtek)
        /// </summary>
        Success = 0,

        /// <summary>
        /// Fail means the scanned MICR contains at least one invalid read char ('!' for Canon and '?' for Magtek)
        /// but the user chose to Upload it anyway
        /// </summary>
        Fail = 1
    }
}
