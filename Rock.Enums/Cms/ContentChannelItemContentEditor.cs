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
namespace Rock.Model
{
    /// <summary>
    /// Identifies which content editor the Content Channel Item Detail block
    /// renders for an item.
    /// </summary>
    [Enums.EnumDomain( "Cms" )]
    public enum ContentChannelItemContentEditor
    {
        /// <summary>
        /// No editor renders (the content channel type disables the content field).
        /// </summary>
        None = 0,

        /// <summary>
        /// The HTML (rich text / code) editor renders.
        /// </summary>
        Html = 1,

        /// <summary>
        /// The structured (EditorJS block) editor renders.
        /// </summary>
        Structured = 2
    }
}
