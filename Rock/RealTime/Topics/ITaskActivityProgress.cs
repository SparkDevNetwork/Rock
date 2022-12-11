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

using System.Threading;
using System.Threading.Tasks;

using Rock.ViewModels.Utility;

namespace Rock.RealTime.Topics
{
    /// <summary>
    /// A topic interface for sending task activity updates to clients.
    /// </summary>
    public interface ITaskActivityProgress
    {
        /// <summary>
        /// Send a notification to add a message to the task log.
        /// </summary>
        /// <param name="message">Details about the task progress message.</param>
        Task UpdateTaskLog( TaskActivityProgressLogBag message );

        /// <summary>
        /// Send an update to the progress of the task.
        /// </summary>
        /// <param name="progress">The details about the most current progress update.</param>
        Task UpdateTaskProgress( TaskActivityProgressUpdateBag progress );

        /// <summary>
        /// Send a message that the task has started.
        /// </summary>
        /// <param name="status">The details about the task that has started.</param>
        Task TaskStarted( TaskActivityProgressStatusBag status );

        /// <summary>
        /// Send a message that the task is completed.
        /// </summary>
        /// <param name="status">The details about the task that has completed.</param>
        Task TaskCompleted( TaskActivityProgressStatusBag status );
    }
}
