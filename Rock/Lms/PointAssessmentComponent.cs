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
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Model;

namespace Rock.Lms
{
    /// <summary>
    /// The Point Assessment activity is a learning activity that
    /// has the facilitator evaluating a presentation or other material for completion.
    /// </summary>
    [Description( "A Learning Activity that has the facilitator evaluating a presentation or other material." )]
    [Export( typeof( LearningActivityComponent ) )]
    [ExportMetadata( "ComponentName", "Point Assessment" )]

    [Rock.SystemGuid.EntityTypeGuid( "a6e91c3c-4a4c-4fc2-816a-a6b1e6422381" )]
    public class PointAssessmentComponent : LearningActivityComponent
    {
        /// <summary>
        /// Gets the Highlight color for the component.
        /// </summary>
        public override string HighlightColor => "#9d174d";

        /// <summary>
        /// Gets the icon CSS class for the component.
        /// </summary>
        public override string IconCssClass => "fa fa-photo-video";

        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        public override string Name => "Point Assessment";

        /// <summary>
        /// Initializes a new instance of the CheckOffComponent.
        /// </summary>
        public PointAssessmentComponent(): base( @"/Obsidian/Controls/Internal/LearningActivity/pointAssessmentLearningActivity.obs" ) { }

        /// <summary>
        /// Point Assessments require grading when the due date is in the past and the activity hasn't yet been graded.
        /// </summary>
        /// <remarks>
        /// Because there's no way to know when a student has completed the activity
        /// we are using the due date as a qualifier for when to consider the activity
        /// "completed" by a student.
        /// </remarks>
        /// <param name="completion">The learning activity completion.</param>
        /// <returns><c>true</c> if the facilitator needs to grade the activity; otherwise <c>false</c>.</returns>
        public override bool RequiresGrading( LearningActivityCompletion completion )
        {
            return
                completion.DueDate.HasValue &&
                completion.DueDate.Value.IsPast() &&
                !completion.GradedByPersonAliasId.HasValue;
        }
    }
}
