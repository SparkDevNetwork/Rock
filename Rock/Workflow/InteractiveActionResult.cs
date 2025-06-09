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
using Rock.ViewModels.Workflow;

namespace Rock.Workflow
{
    /// <summary>
    /// Contains the result of processing an action interactively with an
    /// individual.
    /// </summary>
    internal class InteractiveActionResult
    {
        /// <summary>
        /// Indicates if the action was completed successfully. This is used to
        /// determine if the action should be marked as completed in the database
        /// per the configured "on completion" settings.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Determines if the block should display existing notes and allow
        /// entry of new notes on the page.
        /// </summary>
        public bool IsNotesVisible { get; set; }

        /// <summary>
        /// How processing of the workflow should continue.
        /// </summary>
        public InteractiveActionContinueMode ProcessingType { get; set; }

        /// <summary>
        /// The data that will be used to display the interactive action or
        /// the result of the action's execution.
        /// </summary>
        public InteractiveActionDataBag ActionData { get; set; }
    }
}
