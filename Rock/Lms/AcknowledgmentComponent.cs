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
    /// The acknowledgment activity is a learning activity that requires a participant to check a box for completion.
    /// </summary>
    [Description( "A Learning Activity that requires a participant to check a box of acknowledgment." )]
    [Export( typeof( LearningActivityComponent ) )]
    [ExportMetadata( "ComponentName", "Acknowledgment" )]

    [RockInternal( "17.0" )]
    [Rock.SystemGuid.EntityTypeGuid( "7fae61a2-5f08-4fd9-8bb7-ff7fab410ac5" )]
    public class AcknowledgmentComponent : LearningActivityComponent
    {
        #region Keys

        private class SettingKey
        {
            public const string ConfirmationText = "confirmationText";

            public const string Content = "content";

            public const string IsConfirmationRequired = "isConfirmationRequired";
        }

        private class CompletionKey
        {
            public const string IsConfirmed = "isConfirmed";

            public const string PointsPossibleAtCompletion = "pointsPossibleAtCompletion";
        }

        #endregion

        /// <inheritdoc/>
        public override string HighlightColor => "#644a88";

        /// <inheritdoc/>
        public override string IconCssClass => "far fa-check-square";

        /// <inheritdoc/>
        public override string Name => "Acknowledgment";

        /// <inheritdoc/>
        public override string ComponentUrl => @"/Obsidian/Controls/Internal/LearningActivity/acknowledgmentLearningActivity.obs";

        /// <inheritdoc/>
        public override Dictionary<string, string> GetActivityConfiguration( LearningActivity activity, Dictionary<string, string> componentData, PresentedFor presentation, RockContext rockContext, RockRequestContext requestContext )
        {
            if ( presentation == PresentedFor.Configuration )
            {
                return new Dictionary<string, string>();
            }
            else
            {
                var content = componentData.GetValueOrNull( SettingKey.Content );

                var contentHtml = content.IsNotNullOrWhiteSpace()
                    ? new StructuredContentHelper( content ).Render()
                    : string.Empty;

                if ( contentHtml.IsNotNullOrWhiteSpace() )
                {
                    var mergeFields = requestContext.GetCommonMergeFields();

                    contentHtml = contentHtml.ResolveMergeFields( mergeFields );
                }

                return new Dictionary<string, string>
                {
                    [SettingKey.ConfirmationText] = componentData.GetValueOrNull( SettingKey.ConfirmationText ),
                    [SettingKey.Content] = contentHtml,
                    [SettingKey.IsConfirmationRequired] = componentData.GetValueOrNull( SettingKey.IsConfirmationRequired )
                };
            }
        }
    }
}
