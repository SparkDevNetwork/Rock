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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the GetListOfAllFolders API action of
    /// the AssetManager control.
    /// </summary>
    public class AssetManagerGetListOfAllFoldersOptionsBag
    {
        /// <summary>
        /// The file manager's root folder, encrypted
        /// </summary>
        public string EncryptedRoot { get; set; }

        /// <summary>
        /// The folder that we want to move. Don't show this folder or its children in the list.
        /// </summary>
        public string SelectedFolder { get; set; }
    }
}
