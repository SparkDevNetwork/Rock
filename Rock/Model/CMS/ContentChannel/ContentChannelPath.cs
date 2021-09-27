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
    /// Represents a ContentChannelPath object in Rock.
    /// </summary>
    public class ContentChannelPath
    {
        /// <summary>
        /// Gets or sets the ID of the ContentChannel.
        /// </summary>
        /// <value>
        /// ID of the ContentChannel.
        /// </value>
        public int ContentChannelId { get; set; }

        /// <summary>
        /// Gets or sets the full associated ancestor path (of content channel type associations). 
        /// </summary>
        /// <value>
        /// Full path of the ancestor content channel type associations. 
        /// </value>
        public string Path { get; set; }

        /// <summary>
        /// Returns the Path of the ContentChannelPath
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Path;
        }
    }
}
