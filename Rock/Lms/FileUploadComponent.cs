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
    /// The File Upload activity is a learning activity that requires a participant to upload a file for completion.
    /// </summary>
    [Description( "A Learning Activity that requires a participant to upload a file." )]
    [Export( typeof( LearningActivityComponent ) )]
    [ExportMetadata( "ComponentName", "File Upload" )]

    [Rock.SystemGuid.EntityTypeGuid( "deb298fe-383b-46bd-a974-afd92c09843a" )]
    public class FileUploadComponent : LearningActivityComponent
    {
        /// <summary>
        /// Gets the Highlight color for the component.
        /// </summary>
        public override string HighlightColor => "#d8f9e5";

        /// <summary>
        /// Gets the icon CSS class for the component.
        /// </summary>
        public override string IconCssClass => "fa fa-file-alt";

        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        public override string Name => "File Upload";

        /// <summary>
        /// Initializes a new instance of the CheckOffComponent.
        /// </summary>
        public FileUploadComponent() : base( @"/Obsidian/Controls/Internal/LearningActivity/fileUploadLearningActivity.obs" ) { }
    }
}
