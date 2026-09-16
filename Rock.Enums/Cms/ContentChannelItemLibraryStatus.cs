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
    /// Identifies which Content Library status panel the Content Channel Item
    /// Detail block renders for an item.
    /// </summary>
    [Enums.EnumDomain( "Cms" )]
    public enum ContentChannelItemLibraryStatus
    {
        /// <summary>
        /// No status panel (the item was never touched by the Content Library).
        /// </summary>
        None = 0,

        /// <summary>
        /// The uploaded-to-library status panel.
        /// </summary>
        Uploaded = 1,

        /// <summary>
        /// The downloaded-from-library status panel.
        /// </summary>
        Downloaded = 2
    }
}
