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

using System.Collections.Generic;

namespace Rock.ViewModels.Utility
{
    /// <summary>
    /// Contains information about the starting or stopping of a task.
    /// </summary>
    public class TaskActivityProgressStatusBag : ITaskActivityProgressMessage
    {
        /// <summary>
        /// A unique identifier for the task to which this status applies.
        /// </summary>
        public string TaskId { get; set; }

        /// <summary>
        /// The name of the task.
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// A flag indicating if this task has started.
        /// </summary>
        public bool IsStarted { get; set; }

        /// <summary>
        /// A flag indicating if this task is finished.
        /// </summary>
        public bool IsFinished { get; set; }

        /// <summary>
        /// A message indicating the current state of this task.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// A collection of errors the task encountered while running.
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        /// A collection of warnings the task encountered while running.
        /// </summary>
        public List<string> Warnings { get; set; }

        /// <summary>
        /// Additional custom data related to the status of this task.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// The total time elapsed for this task, measured in milliseconds.
        /// </summary>
        public long ElapsedMilliseconds { get; set; }
    }
}
