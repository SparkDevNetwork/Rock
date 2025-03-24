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
    /// The Video Watch activity is a learning activity that requires a participant to watch a video for completion.
    /// </summary>
    [Description( "A Learning Activity that requires a participant to watch a video." )]
    [Export( typeof( LearningActivityComponent ) )]
    [ExportMetadata( "ComponentName", "Video Watch" )]

    [RockInternal( "17.0" )]
    [Rock.SystemGuid.EntityTypeGuid( "70f13b8f-ab1e-4ea4-847e-a448501dab1c" )]
    public class VideoWatchComponent : LearningActivityComponent
    {
        #region Keys

        private static class SettingKey
        {
            public const string CompletionThreshold = "completionThreshold";

            public const string FooterContent = "footerContent";

            public const string HeaderContent = "headerContent";

            public const string Video = "video";
        }

        private static class CompletionKey
        {
            public const string WatchedPercentage = "watchedPercentage";

            public const string PointsAvailableAtCompletion = "pointsAvailableAtCompletion";
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public override string HighlightColor => "#2f699f";

        /// <inheritdoc/>
        public override string IconCssClass => "fa fa-video";

        /// <inheritdoc/>
        public override string Name => "Video Watch";

        /// <inheritdoc/>
        public override string ComponentUrl => @"/Obsidian/Controls/Internal/LearningActivity/videoWatchLearningActivity.obs";

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
                var headerContent = componentData.GetValueOrNull( SettingKey.HeaderContent );
                var footerContent = componentData.GetValueOrNull( SettingKey.FooterContent );
                var mergeFields = requestContext.GetCommonMergeFields();

                var headerContentHtml = headerContent.IsNotNullOrWhiteSpace()
                    ? new StructuredContentHelper( headerContent ).Render().ResolveMergeFields( mergeFields )
                    : string.Empty;

                var footerContentHtml = footerContent.IsNotNullOrWhiteSpace()
                    ? new StructuredContentHelper( footerContent ).Render().ResolveMergeFields( mergeFields )
                    : string.Empty;

                return new Dictionary<string, string>
                {
                    [SettingKey.CompletionThreshold] = componentData.GetValueOrNull( SettingKey.CompletionThreshold ),
                    [SettingKey.FooterContent] = footerContentHtml,
                    [SettingKey.HeaderContent] = headerContentHtml,
                    [SettingKey.Video] = componentData.GetValueOrNull( SettingKey.Video )
                };
            }
        }

        #endregion
    }
}
