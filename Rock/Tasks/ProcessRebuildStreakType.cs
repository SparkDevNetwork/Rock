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

using Rock.Model;

namespace Rock.Tasks
{
    /// <summary>
    /// Rebuilds a streak type binary maps from source data (attendance or interactions).
    /// </summary>
    public sealed class ProcessRebuildStreakType : BusStartedTask<ProcessRebuildStreakType.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            StreakTypeService.RebuildStreakType( null, message.StreakTypeId, out var errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                ExceptionLogService.LogException( errorMessage );
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the streak type identifier.
            /// </summary>
            /// <value>
            /// The streak type identifier.
            /// </value>
            public int StreakTypeId { get; set; }
        }
    }
}