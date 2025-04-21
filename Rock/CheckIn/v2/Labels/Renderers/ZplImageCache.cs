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

namespace Rock.CheckIn.v2.Labels.Renderers
{
    /// <summary>
    /// Contains the cached data for an image cached by the ZPL renderer.
    /// </summary>
    internal class ZplImageCache : Rock.Web.Cache.ItemCache<ZplImageCache>
    {
        /// <summary>
        /// The raw image data in a native format known by the renderer.
        /// </summary>
        public byte[] ImageData { get; }

        /// <summary>
        /// The string Hex encoded string that represents <see cref="ImageData"/>.
        /// </summary>
        public string ZplContent { get; }

        /// <summary>
        /// The width of the image data in pixels.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// The height of the image data in pixels.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Creates a new instance of <see cref="ZplImageCache"/>.
        /// </summary>
        /// <param name="imageData">The raw image data.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        public ZplImageCache( byte[] imageData, int width, int height )
        {
            ImageData = imageData;
            Width = width;
            Height = height;

            ZplContent = ZplImageHelper.ByteArrayToHexViaLookup32( ImageData );
        }
    }
}
