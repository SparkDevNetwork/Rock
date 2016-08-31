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
namespace Rock.Data
{
    /// <summary>
    /// Represents a model that supports generating a feed
    /// </summary>
    interface IFeed
    {
        /// <summary>
        /// Returns the feed.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="count">The count.</param>
        /// <param name="format">The format.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <returns></returns>
        string ReturnFeed( int key, int count, string format, out string errorMessage, out string contentType );
    }
}
