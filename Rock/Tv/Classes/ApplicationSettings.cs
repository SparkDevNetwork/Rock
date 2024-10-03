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

namespace Rock.Tv.Classes
{
    /// <summary>
    /// POCO for Application Settings
    /// </summary>
    public class ApplicationSettings
    {
        /// <summary>
        /// Gets or sets the type of the TV application.
        /// </summary>
        /// <value>
        /// The type of the TV application.
        /// </value>
        public TvApplicationType TvApplicationType { get; set; }
    }

    /// <summary>
    /// Enum for determining the type of TV application.
    /// </summary>
    public enum TvApplicationType
    {
        /// <summary>
        /// Apple TV
        /// </summary>
        AppleTv = 0,

        /// <summary>
        /// Roku
        /// </summary>
        Roku = 1
    }
}
