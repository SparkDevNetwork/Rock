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
    /// Represents a container for file-related information, including the file's path, name, and contents.
    /// </summary>
    /// <remarks>This class is designed to encapsulate the details of a file, such as its relative path, name,
    /// and content. It can be used in scenarios where file metadata and content need to be passed or manipulated as a
    /// single unit.</remarks>
    public class FileEditorBag
    {
        /// <summary>
        /// Gets or sets the file path associated with the operation.
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// Gets or sets the contents of the file as a string.
        /// </summary>
        public string FileContents { get; set; }
    }
}
