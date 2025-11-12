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

namespace Rock.ViewModels.Utility
{
    /// <summary>
    /// Contains information about the starting or stopping of a task.
    /// </summary>
    public class CsvImportActivityProgressStatusBag
    {
        /// <summary>
        /// The name of the task.
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// The percentage of completion for the task, from 0 to 100.
        /// </summary>
        public decimal CompletionPercentage { get; set; }

        /// <summary>
        /// A message indicating the current state of this task.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// An error the task encountered while running.
        /// </summary>
        public string Error { get; set; }
    }
}
