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

using System.ComponentModel;

#if NET
namespace Rock.Enums.Connection
#else
namespace Rock.Model
#endif
{
    /// <summary>
    /// Type of connection state
    /// </summary>
#if !NET
    [Enums.EnumDomain( "Connection" )]
#endif
    public enum ConnectionState
    {
        /// <summary>
        /// Active
        /// </summary>
        Active = 0,

        /// <summary>
        /// Inactive
        /// </summary>
        Inactive = 1,

        /// <summary>
        /// Future Follow-up
        /// </summary>
        FutureFollowUp = 2,

        /// <summary>
        /// Connected
        /// </summary>
        [Description( "Completed" )]
        Connected = 3,
    }
}
