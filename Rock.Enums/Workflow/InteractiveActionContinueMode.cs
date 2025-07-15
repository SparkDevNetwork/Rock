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

namespace Rock.Enums.Workflow
{
    /// <summary>
    /// Defines how processing will proceed after an interactive action has been
    /// processed.
    /// </summary>
    public enum InteractiveActionContinueMode
    {
        /// <summary>
        /// Workflow processing will stop after the current interactive action.
        /// The action response details will be returned to the client.
        /// </summary>
        Stop = 0,

        /// <summary>
        /// Workflow processing will continue. If another interactive action
        /// needs to be processed then it will be processed and replace the
        /// current response details.
        /// </summary>
        Continue = 1,

        /// <summary>
        /// Workflow processing will continue but stop before another interactive
        /// action is processed. The action response details will be returned to
        /// the client.
        /// </summary>
        ContinueWhileUnattended = 2,
    }
}
