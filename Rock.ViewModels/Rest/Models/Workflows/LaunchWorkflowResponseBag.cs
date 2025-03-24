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
using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Rest.Models.Workflows
{
    /// <summary>
    /// Contains the response data from launching a workflow.
    /// </summary>
    public class LaunchWorkflowResponseBag
    {
        /// <summary>
        /// The integer identifier of the launched workflow. Will be 0 if the
        /// workflow was not waited or not set to persist.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The unique identifier of the launched workflow. Will be an empty
        /// Guid value if the workflow was not waited or not set to persist.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// The identifier key of the launched workflow. Will be null if the
        /// workflow was not waited or not set to persist.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// If the workflow was waited and did not fully complete while
        /// processing then this value will be <c>true</c>. For example, if the
        /// workflow hit an Entry form it will stop processing but remain
        /// active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// The value of the status property for the workflow that was launched.
        /// This will be null if the workflow was not waited.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Will contain any errors that occurred while processing the workflow.
        /// </summary>
        public List<string> Errors { get; set; }
    }
}
