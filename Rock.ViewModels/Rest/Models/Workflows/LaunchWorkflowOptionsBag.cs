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

namespace Rock.ViewModels.Rest.Models.Workflows
{
    /// <summary>
    /// Details about how the workflow should be launched.
    /// </summary>
    public class LaunchWorkflowOptionsBag
    {
        /// <summary>
        /// The entity type identifier as either an Id, Guid, IdKey, or name
        /// value. This will be used when loading the entity to launch the
        /// workflow with.
        /// </summary>
        public string EntityTypeId { get; set; }

        /// <summary>
        /// The entity identifier as either an Id, Guid or IdKey value to
        /// launch the workflow with.
        /// </summary>
        public string EntityId { get; set; }

        /// <summary>
        /// If <c>true</c> then an error should be returned if the entity
        /// was specified but could not be found.
        /// </summary>
        public bool IsEntityRequired { get; set; }

        /// <summary>
        /// The name to set on the workflow. If not provided then a default
        /// name will be used based on the workflow type name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The attribute values that will be provided to the workflow. The
        /// key should match the key of a workflow attribute. The value is the
        /// raw database value that it will be set to.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }

        /// <summary>
        /// If <c>true</c> then the workflow will be launched immediately.
        /// Otherwise the workflow is launched in the background and may take
        /// up to one minute to be launched.
        /// </summary>
        public bool Immediate { get; set; }

        /// <summary>
        /// If <c>true</c> then the request will not return until the workflow
        /// has launched and completed processing. This will also force
        /// <see cref="Immediate"/> to be <c>true</c>.
        /// </summary>
        public bool Wait { get; set; }
    }
}
