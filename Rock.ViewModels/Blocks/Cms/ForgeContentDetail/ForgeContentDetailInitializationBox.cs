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
    /// The box that contains all the initialization information for the
    /// Forge Content block.
    /// </summary>
    public class ForgeContentDetailInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the clean Vue source the author wrote. Only populated for
        /// authorized editors; a plain visitor never receives the source.
        /// </summary>
        /// <value>The authored Vue source.</value>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the compiled SystemJS module string used to render the
        /// component in view mode.
        /// </summary>
        /// <value>The compiled component module.</value>
        public string CompiledContent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person may edit
        /// this component in place.
        /// </summary>
        /// <value><c>true</c> if the current person may edit; otherwise <c>false</c>.</value>
        public bool IsEditable { get; set; }
    }
}
