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
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.ViewModels.Event.InteractiveExperiences;

namespace Rock.RealTime.Topics
{
    /// <summary>
    /// A topic interface for sending messages to participants in an Interactive
    /// Experience event.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "1.14.1", true )]
    internal interface IInteractiveExperienceParticipant
    {
        /// <summary>
        /// Shows the action on the experience occurrence.
        /// </summary>
        /// <param name="occurrenceIdKey">The identifier of the experience occurrence this action is for.</param>
        /// <param name="actionIdKey">The identifier of the action to be shown.</param>
        /// <param name="actionData">The action render data.</param>
        Task ShowAction( string occurrenceIdKey, string actionIdKey, ActionRenderConfigurationBag actionData );

        /// <summary>
        /// Clears all actions from the specified experience occurrence.
        /// </summary>
        /// <param name="occurrenceIdKey">The identifier of the experience occurrence actions should be cleared from.</param>
        Task ClearActions( string occurrenceIdKey );

        /// <summary>
        /// Notifies that a new answer has been submitted to the experience occurrence.
        /// </summary>
        /// <param name="occurrenceIdKey">The identifier of the experience occurrence this answer is for.</param>
        /// <param name="answer">The details of the answer that was submitted.</param>
        Task AnswerSubmitted( string occurrenceIdKey, ExperienceAnswerBag answer );

        /// <summary>
        /// Notifies that an existing answer has been updated on the experience occurrence.
        /// </summary>
        /// <param name="occurrenceIdKey">The identifier of the experience occurrence this answer is for.</param>
        /// <param name="answer">The details of the answer that was updated.</param>
        Task AnswerUpdated( string occurrenceIdKey, ExperienceAnswerBag answer );

        /// <summary>
        /// Notifies that an existing answer has been removed from the experience occurrence.
        /// </summary>
        /// <param name="occurrenceIdKey">The identifier of the experience occurrence this answer was for.</param>
        /// <param name="answerIdKey">The identifier key of the answer that was removed.</param>
        Task AnswerRemoved( string occurrenceIdKey, string answerIdKey );

        /// <summary>
        /// Shows the visualizer on the experience occurrence.
        /// </summary>
        /// <param name="occurrenceIdKey">The identifier of the experience occurrence this action is for.</param>
        /// <param name="actionIdKey">The identifier of the action whose visualizer is to be shown.</param>
        /// <param name="visualizerData">The action visualizer render data.</param>
        Task ShowVisualizer( string occurrenceIdKey, string actionIdKey, VisualizerRenderConfigurationBag visualizerData );

        /// <summary>
        /// Clears the visualizer from the specified experience occurrence.
        /// </summary>
        /// <param name="occurrenceIdKey">The identifier of the experience occurrence the visualizer should be cleared from.</param>
        Task ClearVisualizer( string occurrenceIdKey );
    }
}
