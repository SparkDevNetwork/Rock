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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.AchievementTypeDetail
{
    public class AchievementTypeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the Rock.Model.EntityType of the achievement.
        /// </summary>
        public ListItemBag AchievementEntityType { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.WorkflowType to be launched when the achievement is failed (closed and not successful).
        /// </summary>
        public ListItemBag AchievementFailureWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        public string AchievementIconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.WorkflowType to be launched when the achievement starts.
        /// </summary>
        public ListItemBag AchievementStartWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.StepStatus to be used for the Rock.Model.StepType created when the achievement is completed.
        /// </summary>
        public ListItemBag AchievementStepStatus { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.StepType to be created when the achievement is completed.
        /// </summary>
        public ListItemBag AchievementStepType { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.WorkflowType to be launched when the achievement is successful.
        /// </summary>
        public ListItemBag AchievementSuccessWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets whether over achievement is allowed. This cannot be true if Rock.Model.AchievementType.MaxAccomplishmentsAllowed is greater than 1.
        /// </summary>
        public bool AllowOverAchievement { get; set; }

        /// <summary>
        /// Gets or sets the alternate image binary file.
        /// </summary>
        public ListItemBag AlternateImageBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the attempts.
        /// </summary>
        public List<ListItemBag> Attempts { get; set; }

        /// <summary>
        /// Gets or sets the available prerequisites.
        /// </summary>
        /// <value>
        /// The available prerequisites.
        /// </value>
        public List<ListItemBag> AvailablePrerequisites { get; set; }

        /// <summary>
        /// Gets or sets the lava template used to render a badge.
        /// </summary>
        public string BadgeLavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.Category.
        /// </summary>
        public ListItemBag Category { get; set; }

        /// <summary>
        /// Gets or sets the lava template used to render the status summary of the achievement.
        /// </summary>
        public string CustomSummaryLavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets a description of the achievement type.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the color of the highlight.
        /// </summary>
        public string HighlightColor { get; set; }

        /// <summary>
        /// Gets or sets the image binary file.
        /// </summary>
        public ListItemBag ImageBinaryFile { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is public.
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or sets the maximum accomplishments allowed.
        /// </summary>
        public int? MaxAccomplishmentsAllowed { get; set; }

        /// <summary>
        /// Gets or sets the name of the achievement type. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the prerequisites.
        /// </summary>
        /// <value>
        /// The prerequisites.
        /// </value>
        public List<string> Prerequisites { get; set; }

        /// <summary>
        /// Gets or sets the lava template used to render results.
        /// </summary>
        public string ResultsLavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets the source entity type. The source supplies the data framework from which achievements are computed.
        /// The original achievement sources were Streaks.
        /// </summary>
        public int? SourceEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [add step on success].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [add step on success]; otherwise, <c>false</c>.
        /// </value>
        public bool AddStepOnSuccess { get; set; }

        /// <summary>
        /// Gets or sets the step program.
        /// </summary>
        /// <value>
        /// The step program.
        /// </value>
        public ListItemBag StepProgram { get; set; }

        /// <summary>
        /// Gets or sets the achievement event description.
        /// </summary>
        /// <value>
        /// The achievement event description.
        /// </value>
        public string AchievementEventDescription { get; set; }

        /// <summary>
        /// Gets or sets the chart data json.
        /// </summary>
        /// <value>
        /// The chart data json.
        /// </value>
        public string ChartDataJSON { get; set; }
    }
}
