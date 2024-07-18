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

namespace Rock.ViewModels.Blocks.Cms.AssetManager
{
    /// <summary>
    /// Contains extra configuration details for the Asset Manager block.
    /// </summary>
    public class AssetManagerOptionsBag
    {
        /// <summary>
        /// Title shown at the top of the block
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Set this to true to enable showing folders and files from your configured asset storage providers.
        /// </summary>
        public bool EnableAssetProviders { get; set; }

        /// <summary>
        /// Set this to true to enable showing folders and files your server's local file system.
        /// </summary>
        public bool EnableFileManager { get; set; }

        /// <summary>
        /// Set this to true to be able to set a CSS height value dictating how tall the block will be. Otherwise, it will grow with the content.
        /// </summary>
        public bool IsStaticHeight { get; set; }

        /// <summary>
        /// If `IsStaticHeight` is true, this will be the CSS length value that dictates how tall the block will be.
        /// </summary>
        public string Height { get; set; }

        /// <summary>
        /// The root file manager folder to browse; encrypted
        /// </summary>
        public string RootFolder { get; set; }

        /// <summary>
        /// Select 'image' to show only image files. Select 'doc' to show all files.
        /// </summary>
        public string BrowseMode { get; set; }

        /// <summary>
        /// Page used to edit the contents of a file.
        /// </summary>
        public string FileEditorPage { get; set; }

        /// <summary>
        /// Set this to true to enable the Zip File uploader.
        /// </summary>
        public bool EnableZipUploader { get; set; }

    }
}
