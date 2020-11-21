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
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// Manually created Service methods for ContentChannelItem
    /// </summary>
    public partial class ContentChannelItemService
    {

        /// <summary>
        /// Gets the maximum item order value for content channel.
        /// </summary>
        /// <param name="contentChannelId">The content channel identifier.</param>
        /// <returns>The highest value in the Order field for the specified content channel</returns>
        public int? GetMaxItemOrderValueForContentChannel( int contentChannelId )
        {
            return Queryable().Where( i => i.ContentChannelId == contentChannelId ).Max( i => ( int? ) i.Order );
        }

        /// <summary>
        /// Gets the next Order value for the content channel.
        /// </summary>
        /// <param name="contentChannelId">The content channel identifier.</param>
        /// <returns></returns>
        public int GetNextItemOrderValueForContentChannel( int contentChannelId )
        {
            int? i = GetMaxItemOrderValueForContentChannel( contentChannelId );
            return i == null ? 0 : (int)i + 1;
        }
    }
}
