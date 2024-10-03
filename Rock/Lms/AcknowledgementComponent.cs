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
    /// The acknowledgement activity is a learning activity that requires a participant to check a box for completion.
    /// </summary>
    [Description( "A Learning Activity that requires a participant to check a box of acknowledgement." )]
    [Export( typeof( LearningActivityComponent ) )]
    [ExportMetadata( "ComponentName", "Acknowledgement" )]

    [Rock.SystemGuid.EntityTypeGuid( "7fae61a2-5f08-4fd9-8bb7-ff7fab410ac5" )]
    public class AcknowledgementComponent : LearningActivityComponent
    {
        /// <summary>
        /// Gets the Highlight color for the component.
        /// </summary>
        public override string HighlightColor => "#644a88";

        /// <summary>
        /// Gets the icon CSS class for the component.
        /// </summary>
        public override string IconCssClass => "far fa-check-square";

        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        public override string Name => "Acknowledgement";

        /// <summary>
        /// Initializes a new instance of the CheckOffComponent.
        /// </summary>
        public AcknowledgementComponent() : base( @"/Obsidian/Controls/Internal/LearningActivity/acknowledgementLearningActivity.obs" ) { }
    }
}
