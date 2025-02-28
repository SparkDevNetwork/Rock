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
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Attribute;
using Rock.Cms.StructuredContent;
using Rock.Data;
using Rock.Enums.Lms;
using Rock.Model;
using Rock.Net;

namespace Rock.Lms
{
    /// <summary>
    /// The Point Assessment activity is a learning activity that
    /// has the facilitator evaluating a presentation or other material for completion.
    /// </summary>
    [Description( "A Learning Activity that has the facilitator evaluating a presentation or other material." )]
    [Export( typeof( LearningActivityComponent ) )]
    [ExportMetadata( "ComponentName", "Point Assessment" )]

    [RockInternal( "17.0" )]
    [Rock.SystemGuid.EntityTypeGuid( "a6e91c3c-4a4c-4fc2-816a-a6b1e6422381" )]
    public class PointAssessmentComponent : LearningActivityComponent
    {
        #region Keys

        private static class SettingKey
        {
            public const string Instructions = "instructions";

            public const string Rubric = "rubric";

            public const string AcknowledgedButtonText = "acknowledgedButtonText";
        }

        private static class CompletionKey
        {
            public const string PointsAvailableAtCompletion = "pointsAvailableAtCompletion";
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public override string HighlightColor => "#9d174d";

        /// <inheritdoc/>
        public override string IconCssClass => "fa fa-photo-video";

        /// <inheritdoc/>
        public override string Name => "Point Assessment";

        /// <inheritdoc/>
        public override string ComponentUrl => @"/Obsidian/Controls/Internal/LearningActivity/pointAssessmentLearningActivity.obs";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override Dictionary<string, string> GetActivityConfiguration( LearningActivity activity, Dictionary<string, string> componentData, PresentedFor presentation, RockContext rockContext, RockRequestContext requestContext )
        {
            if ( presentation == PresentedFor.Configuration )
            {
                return new Dictionary<string, string>();
            }
            else
            {
                var instructions = componentData.GetValueOrNull( SettingKey.Instructions );
                var rubric = componentData.GetValueOrNull( SettingKey.Rubric );

                var instructionsHtml = instructions.IsNotNullOrWhiteSpace()
                    ? new StructuredContentHelper( instructions ).Render()
                    : string.Empty;

                var rubricHtml = instructions.IsNotNullOrWhiteSpace()
                    ? new StructuredContentHelper( rubric ).Render()
                    : string.Empty;

                return new Dictionary<string, string>
                {
                    [SettingKey.AcknowledgedButtonText] = componentData.GetValueOrNull( SettingKey.AcknowledgedButtonText ),
                    [SettingKey.Instructions] = instructionsHtml,
                    [SettingKey.Rubric] = rubricHtml
                };
            }
        }

        /// <inheritdoc/>
        public override int? CalculatePointsEarned( LearningActivityCompletion completion, Dictionary<string, string> completionData, Dictionary<string, string> componentData, int pointsPossible, RockContext rockContext, RockRequestContext requestContext )
        {
            // We don't auto-assign points based on submission.
            return null;
        }

        /// <inheritdoc/>
        public override bool RequiresGrading( LearningActivityCompletion completion, Dictionary<string, string> completionData, Dictionary<string, string> componentData, RockContext rockContext, RockRequestContext requestContext )
        {
            // It has already been graded.
            if ( completion.PointsEarned.HasValue )
            {
                return false;
            }

            var isPastDue = completion.DueDate.HasValue
                && completion.DueDate.Value.IsPast();

            // If it is past due or completed then it needs to be graded.
            return isPastDue || completion.CompletedDateTime.HasValue;
        }

        #endregion
    }
}
