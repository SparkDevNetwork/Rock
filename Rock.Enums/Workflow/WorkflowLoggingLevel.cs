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
    /// The level of details to log
    /// </summary>
    [Enums.EnumDomain( "Workflow" )]
    public enum WorkflowLoggingLevel
    {
        /// <summary>
        /// Don't log any details
        /// </summary>
        None = 0,

        /// <summary>
        /// Log workflow events
        /// </summary>
        Workflow = 1,

        /// <summary>
        /// Log workflow and activity events
        /// </summary>
        Activity = 2,

        /// <summary>
        /// Log workflow, activity, and action events
        /// </summary>
        Action = 3
    }
}

