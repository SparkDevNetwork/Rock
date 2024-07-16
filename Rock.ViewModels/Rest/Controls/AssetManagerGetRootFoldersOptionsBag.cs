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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the GetRootFolders API action of
    /// the AssetManager control.
    /// </summary>
    public class AssetManagerGetRootFoldersOptionsBag
    {
        /// <summary>
        /// A list of keys of the folders that should be expanded in the tree
        /// (include their subfolders in the results)
        /// </summary>
        public List<string> ExpandedFolders { get; set; }

        /// <summary>
        /// The key of the currently selected folder. In order for the client to
        /// be able to use this key, we need to have the encrypted root of this
        /// asset provider / folder match this key's encrypted root.
        /// </summary>
        public string SelectedFolder { get; set; }

        /// <summary>
        /// Root folder for the file manager (encrypted).
        /// </summary>
        public string RootFolder { get; set; } = "";

        /// <summary>
        /// Whether Asset providers should be included in the result.
        /// </summary>
        public bool EnableAssetManager { get; set; } = false;

        /// <summary>
        /// Whether local file system folders should be included in the result.
        /// </summary>
        public bool EnableFileManager { get; set; } = false;
    }
}
