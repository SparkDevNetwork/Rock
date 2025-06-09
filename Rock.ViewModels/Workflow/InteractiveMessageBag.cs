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

using Rock.Enums.Workflow;

namespace Rock.ViewModels.Workflow
{
    /// <summary>
    /// A message that should be displayed to the person from an interactive
    /// workflow action.
    /// </summary>
    public class InteractiveMessageBag
    {
        /// <summary>
        /// The type of message to be displayed.
        /// </summary>
        public InteractiveMessageType Type { get; set; }

        /// <summary>
        /// The title of the message, this may be <c>null</c> or empty.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The content of the message or action.
        /// </summary>
        public string Content { get; set; }
    }
}
