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

#if NET
namespace Rock.Enums.Core
#else
namespace Rock.Model
#endif
{
    /// <summary>
    /// The Camera barcode configuration values.
    /// </summary>
#if !NET
    [Enums.EnumDomain( "Core" )]
#endif
    public enum CameraBarcodeConfiguration
    {
        /// <summary>
        /// Off
        /// </summary>
        Off = 0,

        /// <summary>
        /// Available
        /// </summary>
        Available = 1,

        /// <summary>
        /// Always on
        /// </summary>
        AlwaysOn = 2,

        /// <summary>
        /// Passive
        /// </summary>
        Passive = 3
    }
}
