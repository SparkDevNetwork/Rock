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

using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Lms;
using Rock.Extension;
using Rock.Model;
using Rock.Net;

namespace Rock.Lms
{
    /// <summary>
    /// Base class for learning activity components
    /// </summary>
    [RockInternal( "17.0" )]
    public abstract class LearningActivityComponent : Component
    {
        #region Properties

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults { get; } = new Dictionary<string, string>
        {
            { "Active", "True" },
            { "Order", "0" }
        };

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public override bool IsActive => true;

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public override int Order => 0;

        /// <summary>
        /// Gets or sets the path to the Obsidian component's .obs file.
        /// </summary>
        public abstract string ComponentUrl { get; }

        /// <summary>
        /// Gets or sets the Icon CSS Class for this component.
        /// </summary>
        /// <value>
        /// The <see cref="System.String"/> CSS class for the icon.
        /// </value>
        public virtual string IconCssClass => null;

        /// <summary>
        /// Gets or sets the highlight color of the icon for this component.
        /// </summary>
        /// <value>
        /// The <see cref="System.String"/> hex highlight color of the icon.
        /// </value>
        public virtual string HighlightColor => null;

        /// <summary>
        /// Gets or sets the name of this component.
        /// </summary>
        /// <value>
        /// The <see cref="System.String"/> name of the component.
        /// </value>
        public virtual string Name => null;

        #endregion

        #region Methods

        /// <summary>
        /// Gets the configuration that can be used when rendering the component.
        /// This is only ever passed to the component, it will never be sent
        /// back to the server. This can provide additional context or
        /// UI options that would not make sense to be in the settings or
        /// completion data.
        /// </summary>
        /// <param name="activity">The <see cref="LearningClassActivity"/> that will be displayed or <c>null</c> if this is a brand new activity.</param>
        /// <param name="componentData">If <paramref name="presentation"/> is <see cref="PresentedFor.Configuration"/> then this will be <c>null</c>; otherwise it will contain the component data previously returned by <c>GetComponentData</c>.</param>
        /// <param name="presentation">The target to which the component will be displayed.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <param name="requestContext">The context that describes the current network request.</param>
        /// <returns>A dictionary of additional configuraiton data to provide to the component.</returns>
        public virtual Dictionary<string, string> GetActivityConfiguration( LearningClassActivity activity, Dictionary<string, string> componentData, PresentedFor presentation, RockContext rockContext, RockRequestContext requestContext )
        {
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the settings that will be provided to the UI component when
        /// rendering for <see cref="PresentedFor.Configuration"/>. These
        /// are the values that can be edited and then sent back to be saved.
        /// </summary>
        /// <param name="activity">The <see cref="LearningClassActivity"/> that will be displayed.</param>
        /// <param name="componentData">The component data previously returned by <c>GetComponentData</c>.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <param name="requestContext">The context that describes the current network request.</param>
        /// <returns>A dictionary of settings that can be edited in the component.</returns>
        public virtual Dictionary<string, string> GetComponentSettings( LearningClassActivity activity, Dictionary<string, string> componentData, RockContext rockContext, RockRequestContext requestContext )
        {
            return componentData;
        }

        /// <summary>
        /// Gets the component data from the component settings that were edited
        /// in the UI component. This will always be called in the context of
        /// a <see cref="PresentedFor.Configuration"/> save operation.
        /// </summary>
        /// <param name="activity">The <see cref="LearningClassActivity"/> that is being updated.</param>
        /// <param name="componentSettings">The settings that were provided from the UI component.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <param name="requestContext">The context that describes the current network request.</param>
        /// <returns>A dictionary of raw component data values that will be stored in the database.</returns>
        public virtual Dictionary<string, string> GetComponentData( LearningClassActivity activity, Dictionary<string, string> componentSettings, RockContext rockContext, RockRequestContext requestContext )
        {
            return componentSettings;
        }

        /// <summary>
        /// Gets the completion values that will be provided to the UI
        /// component when displaying the completion for either the
        /// student or facilitator.
        /// </summary>
        /// <param name="completion">The <see cref="LearningClassActivityCompletion"/> that will be displayed. This will never be <c>null</c> but may not be fully populated if it is a new completion.</param>
        /// <param name="completionData">The completion data previously returned by <c>GetCompletionData</c>.</param>
        /// <param name="componentData">The component data that was previously returned by <c>GetComponentData</c>.</param>
        /// <param name="presentation">The component will be displayed to either <see cref="PresentedFor.Facilitator"/> or <see cref="PresentedFor.Student"/>.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <param name="requestContext">The context that describes the current network request.</param>
        /// <returns>A dictionary of values that can be edited in the component.</returns>
        public virtual Dictionary<string, string> GetCompletionValues( LearningClassActivityCompletion completion, Dictionary<string, string> completionData, Dictionary<string, string> componentData, PresentedFor presentation, RockContext rockContext, RockRequestContext requestContext )
        {
            return completionData;
        }

        /// <summary>
        /// Gets the completion data from the completion values that were edited
        /// in the UI component.
        /// </summary>
        /// <param name="completion">The <see cref="LearningClassActivityCompletion"/> that will be displayed. This will never be <c>null</c> but may not be fully populated if it is a new completion.</param>
        /// <param name="completionValues">The values that were provided from the UI component.</param>
        /// <param name="componentData">The component data that was previously returned by <c>GetComponentData</c>.</param>
        /// <param name="presentation">The component will be displayed to either <see cref="PresentedFor.Facilitator"/> or <see cref="PresentedFor.Student"/>.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <param name="requestContext">The context that describes the current network request.</param>
        /// <returns>A dictionary of raw completion data values that will be stored in the database.</returns>
        public virtual Dictionary<string, string> GetCompletionData( LearningClassActivityCompletion completion, Dictionary<string, string> completionValues, Dictionary<string, string> componentData, PresentedFor presentation, RockContext rockContext, RockRequestContext requestContext )
        {
            return completionValues;
        }

        /// <summary>
        /// Calculates the points earned based on the component data and
        /// completion data from the maximum points possible. This is used for
        /// automatic point assignment when completed by the student.
        /// </summary>
        /// <param name="completion">The <see cref="LearningClassActivityCompletion"/> that points are being calculated for.</param>
        /// <param name="completionData">The completion data previously returned by <c>GetCompletionData</c>.</param>
        /// <param name="componentData">The component data that was previously returned by <c>GetComponentData</c>.</param>
        /// <param name="pointsPossible">The maximum points possible for the activity./></param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <param name="requestContext">The context that describes the current network request, this may be <c>null</c>.</param>
        /// <returns>The actual points earned or <c>null</c> if no points should be assigned automatically.</returns>
        public virtual int? CalculatePointsEarned( LearningClassActivityCompletion completion, Dictionary<string, string> completionData, Dictionary<string, string> componentData, int pointsPossible, RockContext rockContext, RockRequestContext requestContext )
        {
            return pointsPossible;
        }

        /// <summary>
        /// Determines if the activity requires grading by a facilitator.
        /// Defaults to false allowing LearningActivityComponents to opt-in.
        /// </summary>
        /// <param name="completion">The <see cref="LearningClassActivityCompletion"/> that points are being calculated for.</param>
        /// <param name="completionData">The completion data previously returned by <c>GetCompletionData</c>.</param>
        /// <param name="componentData">The component data that was previously returned by <c>GetComponentData</c>.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <param name="requestContext">The context that describes the current network request, this may be <c>null</c>.</param>
        /// <returns><c>true</c> if the completion requires grading/scoring by a facilitator; otherwise <c>false</c>.</returns>
        public virtual bool RequiresGrading( LearningClassActivityCompletion completion, Dictionary<string, string> completionData, Dictionary<string, string> componentData, RockContext rockContext, RockRequestContext requestContext )
        {
            return false;
        }

        #endregion
    }
}
