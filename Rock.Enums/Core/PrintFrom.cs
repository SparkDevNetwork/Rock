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
    /// The application responsible for printing a label
    /// </summary>
#if !NET
    [Enums.EnumDomain( "Core" )]
#endif
    public enum PrintFrom
    {
        /// <summary>
        /// The label will be printed by the kiosk
        /// </summary>
        Client = 0,

        /// <summary>
        /// The label will be printed by the server.
        /// </summary>
        Server = 1
    }
}
