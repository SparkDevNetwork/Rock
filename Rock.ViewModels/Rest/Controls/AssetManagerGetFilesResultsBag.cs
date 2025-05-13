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
    /// The result from the GetFiles API action of the AssetManager control.
    /// </summary>
    public class AssetManagerGetFilesResultsBag<T>
    {
        /// <summary>
        /// List of files to display
        /// </summary>
        public List<T> Files { get; set; } = null;

        /// <summary>
        /// Whether or not this folder is restricted from having certain actions performed on it.
        /// </summary>
        public bool IsFolderRestricted { get; set; } = false;

        /// <summary>
        /// Whetehr or not this folder is restricted from having certain types of files uploaded to it.
        /// </summary>
        public bool IsFolderUploadRestricted { get; set; } = false;
    }
}
