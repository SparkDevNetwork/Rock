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

namespace Rock.ViewModels.Workflow
{
    /// <summary>
    /// Describes how to construct the UI to display a workflow action or
    /// display the result of its execution.
    /// </summary>
    public class InteractiveActionDataBag
    {
        /// <summary>
        /// The URL of a component to display on the workflow entry block.
        /// </summary>
        public string ComponentUrl { get; set; }

        /// <summary>
        /// The custom configuration options that will be sent to the component
        /// to help it render correctly. These will not be sent back to the
        /// server.
        /// </summary>
        public Dictionary<string, string> ComponentConfiguration { get; set; }

        /// <summary>
        /// The custom data to pass to the component as data values that will
        /// then be sent back to the server.
        /// </summary>
        public Dictionary<string, string> ComponentData { get; set; }

        /// <summary>
        /// Describes an error that occurred while processing the component
        /// action. This is only valid to be returned during the UpdateAction
        /// processing. If this is set, the component will be passed this
        /// error and allowed to handle it internally.
        /// </summary>
        public InteractiveActionExceptionBag Exception { get; set; }

        /// <summary>
        /// The message details to display to the individual. This may be <c>null</c>
        /// to cause a generic message to be displayed, but that should be uncommon.
        /// </summary>
        public InteractiveMessageBag Message { get; set; }
    }
}
