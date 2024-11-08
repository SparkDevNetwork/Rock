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

using Rock.Enums.CheckIn.Labels;

namespace Rock.CheckIn.v2.Labels.Renderers
{
    /// <summary>
    /// The options to use when generating a ZPL image.
    /// </summary>
    internal class ZplImageOptions
    {
        /// <summary>
        /// The target width of the image.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The target height of the image.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The brightness adjustment to apply to the image.
        /// </summary>
        public float Brightness { get; set; } = 1;

        /// <summary>
        /// The type of dithering to perform on the image.
        /// </summary>
        public DitherMode Dithering { get; set; }

        /// <summary>
        /// Gets the cache key associated with these settings.
        /// </summary>
        /// <returns>A string that represents the cache key.</returns>
        public string ToCacheKey()
        {
            return $"{Width}:{Height}:{Brightness}:{( int ) Dithering}";
        }
    }
}
