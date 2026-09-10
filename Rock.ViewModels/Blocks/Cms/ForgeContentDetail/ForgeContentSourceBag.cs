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

namespace Rock.ViewModels.Blocks.Cms.ForgeContentDetail
{
    /// <summary>
    /// The response returned by the GetEditContent block action, carrying the
    /// authored source to the editor opened from the block's configuration bar.
    /// </summary>
    public class ForgeContentSourceBag
    {
        /// <summary>
        /// Gets or sets the clean Vue source the author wrote, or null when
        /// the block has no component yet.
        /// </summary>
        /// <value>The authored Vue source.</value>
        public string Source { get; set; }
    }
}
