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
namespace Rock.ViewModels.Blocks.CheckIn.CheckInKiosk
{
    /// <summary>
    /// Details about a single promotion to display on the kiosk.
    /// </summary>
    public class PromotionBag
    {
        /// <summary>
        /// Gets or sets the URL of the image.
        /// </summary>
        /// <value>The URL of the image.</value>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the duration in seconds that this promotion will
        /// stay visible on screen for.
        /// </summary>
        /// <value>The duration in seconds of this promotion.</value>
        public int Duration { get; set; }
    }
}
