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
    public class LaunchWorkflowBag
    {
        /// <summary>
        /// Gets or sets the entity type identifier as either an Id, Guid,
        /// IdKey, or name value. This will be used when loading the entity to launch
        /// the workflow with.
        /// </summary>
        /// <value>The identifier identifying the type of entity to be loaded.</value>
        public string EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier as either an Id, Guid or IdKey
        /// value to launch the workflow with.
        /// </summary>
        /// <value>The entity identifier to launch the workflow with.</value>
        public string EntityId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an error should be returned
        /// if the entity could not be found.
        /// </summary>
        /// <value><c>true</c> if an entity is required to launch the workflow; otherwise, <c>false</c>.</value>
        public bool? IsEntityRequired { get; set; }

        /// <summary>
        /// Gets or sets the name of the workflow. If not provided then a
        /// default name will be used based on the workflow type name.
        /// </summary>
        /// <value>The name of the workflow.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the attribute values that will be provided to the
        /// workflow. The key should match the key of a workflow attribute.
        /// The value is the raw database value that it will be set to.
        /// </summary>
        /// <value>The attribute values to start the workflow with.</value>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}
