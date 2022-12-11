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
    /// An enum that represents when a Job notification status should be sent.
    /// </summary>
    [Enums.EnumDomain( "Core" )]
    public enum JobNotificationStatus
    {
        /// <summary>
        /// Notifications should be sent when a job completes with any notification status.
        /// </summary>
        All = 1,

        /// <summary>
        /// Notification should be sent when the job has completed successfully.
        /// </summary>
        /// 
        Success = 2,

        /// <summary>
        /// Notification should be sent when the job has completed with an error status.
        /// </summary>
        Error = 3,

        /// <summary>
        /// Notifications should not be sent when this job completes with any status.
        /// </summary>
        None = 4
    }
}
