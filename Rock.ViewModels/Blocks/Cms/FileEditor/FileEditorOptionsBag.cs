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

namespace Rock.ViewModels.Blocks.Cms.FileEditor
{
    /// <summary>
    /// Represents a container for configuration options used to customize the behavior and layout of a file editor.
    /// </summary>
    /// <remarks>This class provides properties to configure the operational mode, editability, and layout
    /// type of a file editor. It is intended to be used as a data transfer object for managing editor
    /// settings.</remarks>
    public class FileEditorOptionsBag
    {
        /// <summary>
        /// Gets or sets the editor mode, which determines the operational mode of the code editor,
        /// based on the type of file being edited.
        /// </summary>
        public string EditorMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is editable.
        /// </summary>
        /// <value><c>true</c> if this instance is editable; otherwise, <c>false</c>.</value>
        public bool IsEditable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current layout is a worksurface layout.
        /// </summary>
        public bool isWorksurfaceLayout { get; set; }
    }
}
