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

namespace Rock.Enums.Cms
{
    /// <summary>
    /// The way to perform an automatic reload for a block when the standard
    /// block settings have been changed.
    /// </summary>
    public enum BlockReloadMode
    {
        /// <summary>
        /// No reload is performed.
        /// </summary>
        None = 0,

        /// <summary>
        /// The block is reloaded in place on the page.
        /// </summary>
        Block = 1,

        /// <summary>
        /// The entire page is reloaded.
        /// </summary>
        Page = 2
    }
}
