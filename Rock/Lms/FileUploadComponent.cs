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
    /// The File Upload activity is a learning activity that requires a participant to upload a file for completion.
    /// </summary>
    [Description( "A Learning Activity that requires a participant to upload a file." )]
    [Export( typeof( LearningActivityComponent ) )]
    [ExportMetadata( "ComponentName", "File Upload" )]

    [RockInternal( "17.0" )]
    [Rock.SystemGuid.EntityTypeGuid( "deb298fe-383b-46bd-a974-afd92c09843a" )]
    public class FileUploadComponent : LearningActivityComponent
    {
        #region Keys

        private static class SettingKey
        {
            public const string Instructions = "instructions";

            public const string Rubric = "rubric";

            public const string ShowRubricOnScoring = "showRubricOnScoring";

            public const string ShowRubricOnUpload = "showRubricOnUpload";
        }

        private static class CompletionKey
        {
            public const string BinaryFile = "binaryFile";

            public const string PointsAvailableAtCompletion = "pointsAvailableAtCompletion";
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public override string HighlightColor => "#d8f9e5";

        /// <inheritdoc/>
        public override string IconCssClass => "fa fa-file-alt";

        /// <inheritdoc/>
        public override string Name => "File Upload";

        /// <inheritdoc/>
        public override string ComponentUrl => @"/Obsidian/Controls/Internal/LearningActivity/fileUploadLearningActivity.obs";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override Dictionary<string, string> GetActivityConfiguration( LearningClassActivity activity, Dictionary<string, string> componentData, PresentedFor presentation, RockContext rockContext, RockRequestContext requestContext )
        {
            if ( presentation == PresentedFor.Configuration )
            {
                return new Dictionary<string, string>();
            }
            else
            {
                var instructions = componentData.GetValueOrNull( SettingKey.Instructions );
                var rubric = componentData.GetValueOrNull( SettingKey.Rubric );
                var mergeFields = requestContext.GetCommonMergeFields();

                var instructionsHtml = instructions.IsNotNullOrWhiteSpace()
                    ? new StructuredContentHelper( instructions ).Render().ResolveMergeFields( mergeFields )
                    : string.Empty;

                var rubricHtml = instructions.IsNotNullOrWhiteSpace()
                    ? new StructuredContentHelper( rubric ).Render().ResolveMergeFields( mergeFields )
                    : string.Empty;

                return new Dictionary<string, string>
                {
                    [SettingKey.Instructions] = instructionsHtml,
                    [SettingKey.Rubric] = rubricHtml,
                    [SettingKey.ShowRubricOnScoring] = componentData.GetValueOrNull( SettingKey.ShowRubricOnScoring ),
                    [SettingKey.ShowRubricOnUpload] = componentData.GetValueOrNull( SettingKey.ShowRubricOnUpload )
                };
            }
        }

        /// <inheritdoc/>
        public override bool RequiresGrading( LearningClassActivityCompletion completion, Dictionary<string, string> completionData, Dictionary<string, string> componentData, RockContext rockContext, RockRequestContext requestContext )
        {
            return !completion.GradedByPersonAliasId.HasValue;
        }

        /// <inheritdoc/>
        public override int? CalculatePointsEarned( LearningClassActivityCompletion completion, Dictionary<string, string> completionData, Dictionary<string, string> componentData, int pointsPossible, RockContext rockContext, RockRequestContext requestContext )
        {
            // Points are not auto-awarded for file uploads.
            return null;
        }

        #endregion
    }
}
