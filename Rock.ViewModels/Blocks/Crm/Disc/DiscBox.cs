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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.Disc
{
    /// <summary>
    /// Contains all the initial configuration data required to render the Disc block.
    /// </summary>
    public class DiscInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the date the individual started the assessment (should be after clicking start).
        /// </summary>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date the assessment was last saved.
        /// </summary>
        public DateTime? LastSavedDate { get; set; }

        /// <summary>
        /// Gets or sets the instructions for the assessment.
        /// </summary>
        public string Instructions { get; set; }

        /// <summary>
        /// An optional informational message to display to the individual.
        /// </summary>
        public string InfoMessage { get; set; }

        /// <summary>
        /// Gets or sets the 1-2 character personality type for the individual based on the results of their DISC assessment.
        /// </summary>
        public string PersonalityType { get; set; }

        /// <summary>
        /// Gets or sets whether the individual can retake the test.
        /// </summary>
        public bool CanRetakeTest { get; set; }

        /// <summary>
        /// Gets or sets the overall personality description for the individual based on the results of their DISC assessment.
        /// </summary>
        public string DiscPersonalityDescription { get; set; }

        /// <summary>
        /// Gets or sets the "Strengths" description for the individual based on the results of their DISC assessment.
        /// </summary>
        public string DiscStrengths { get; set; }

        /// <summary>
        /// Gets or sets the "Challenges"" description for the individual based on the results of their DISC assessment.
        /// </summary>
        public string DiscChallenges { get; set; }

        /// <summary>
        /// Gets or sets the "Under Pressure" description for the individual based on the results of their DISC assessment.
        /// </summary>
        public string DiscUnderPressure { get; set; }

        /// <summary>
        /// Gets or sets the "Motivation" description for the individual based on the results of their DISC assessment.
        /// </summary>
        public string DiscMotivation { get; set; }

        /// <summary>
        /// Gets or sets the "Team Contribution" description for the individual based on the results of their DISC assessment.
        /// </summary>
        public string DiscTeamContribution { get; set; }

        /// <summary>
        /// Gets or sets the "Leadership Style" description for the individual based on the results of their DISC assessment.
        /// </summary>
        public string DiscLeadershipStyle { get; set; }

        /// <summary>
        /// Gets or sets the "Follower Style" description for the individual based on the results of their DISC assessment.
        /// </summary>
        public string DiscFollowerStyle { get; set; }

        /// <summary>
        /// Gets or sets the responses/questions for the DISC assessment.
        /// </summary>
        public List<AssessmentResponseBag> Responses { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the assessment is for the person currently viewing the assessment.
        /// </summary>
        public bool IsAsessmentForCurrentPerson { get; set; }

        /// <summary>
        /// Gets or sets the target person bag.
        /// </summary>
        public ListItemBag TargetPersonBag { get; set; }

        /// <summary>
        /// Gets or sets the CSS class to use for the panel icon.
        /// </summary>
        public string PanelIcon { get; set; }

        /// <summary>
        /// Gets or sets the title to use for the panel.
        /// </summary>
        public string PanelTitle { get; set; }

        /// <summary>
        /// Gets or sets the number of questions to show per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// The results of the most recent assessment.
        /// </summary>
        public AssessmentResultsBag Results { get; set; }
    }

    #region nested classes

    /// <summary>
    /// Contains the data representing the response to a question series for the DISC assessment.
    /// </summary>
    [Serializable]
    public class AssessmentResponseBag
    {
        /// <summary>
        /// Gets or sets the question number.
        /// </summary>
        public string QuestionNumber { get; set; }

        /// <summary>
        /// Gets or sets the questions.
        /// </summary>
        public Dictionary<string, string> Questions { get; set; }

        /// <summary>
        /// Gets or sets the most score.
        /// </summary>
        public string MostScore { get; set; }

        /// <summary>
        /// Gets or sets the least score.
        /// </summary>
        public string LeastScore { get; set; }
    }

    /// <summary>
    /// The bag containing the results of the assessment.
    /// </summary>
    public class AssessmentResultsBag
    {
        /// <summary>
        /// Gets or sets the adaptive behavior score for the 'D' trait.
        /// </summary>
        public int AdaptiveBehaviorD;

        /// <summary>
        /// Gets or sets the adaptive behavior score for the 'I' trait.
        /// </summary>
        public int AdaptiveBehaviorI;

        /// <summary>
        /// Gets or sets the adaptive behavior score for the 'S' trait.
        /// </summary>
        public int AdaptiveBehaviorC;

        /// <summary>
        /// Gets or sets the adaptive behavior score for the 'C' trait.
        /// </summary>
        public int AdaptiveBehaviorS;

        /// <summary>
        /// Gets or sets the natural behavior score for the 'D' trait.
        /// </summary>
        public int NaturalBehaviorD;

        /// <summary>
        /// Gets or sets the natural behavior score for the 'I' trait.
        /// </summary>
        public int NaturalBehaviorI;

        /// <summary>
        /// Gets or sets the natural behavior score for the 'S' trait.
        /// </summary>
        public int NaturalBehaviorS;

        /// <summary>
        /// Gets or sets the natural behavior score for the 'C' trait.
        /// </summary>
        public int NaturalBehaviorC;

        /// <summary>
        /// Gets or sets the personality type code.
        /// </summary>
        public string PersonalityType;
    }
    #endregion
}
