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

namespace Rock.Model
{
    /// <summary>
    /// Alert Type
    /// </summary>
    [Enums.EnumDomain( "Finance" )]
    public enum AlertType
    {
        /// <summary>
        /// Gratitude looks for amounts larger than normal, or transactions that are earlier than usual.
        /// </summary>
        [Description( "Gratitude" )]
        Gratitude = 0,

        /// <summary>
        /// Follow Up looks for amounts smaller than normal, or transactions later than usual (or stopped occurring)
        /// </summary>
        [Description( "Follow-up" )]
        FollowUp = 1,
    }
}