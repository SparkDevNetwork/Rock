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

namespace Rock.Lms
{
    /// <summary>
    /// The Video Watch activity is a learning activity that requires a participant to watch a video for completion.
    /// </summary>
    [Description( "A Learning Activity that requires a participant to watch a video." )]
    [Export( typeof( LearningActivityComponent ) )]
    [ExportMetadata( "ComponentName", "Video Watch" )]

    [Rock.SystemGuid.EntityTypeGuid( "70f13b8f-ab1e-4ea4-847e-a448501dab1c" )]
    public class VideoWatchComponent : LearningActivityComponent
    {
        /// <summary>
        /// Gets the Highlight color for the component.
        /// </summary>
        public override string HighlightColor => "#2f699f";

        /// <summary>
        /// Gets the icon CSS class for the component.
        /// </summary>
        public override string IconCssClass => "fa fa-video";

        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        public override string Name => "Video Watch";

        /// <summary>
        /// Initializes a new instance of the CheckOffComponent.
        /// </summary>
        public VideoWatchComponent() : base( @"/Obsidian/Controls/Internal/LearningActivity/videoWatchLearningActivity.obs" ) { }
    }
}
