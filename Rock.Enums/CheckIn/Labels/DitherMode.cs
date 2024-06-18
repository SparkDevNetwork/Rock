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
namespace Rock.Enums.CheckIn.Labels
{
    /// <summary>
    /// The types of dithering that can be applied to an image.
    /// </summary>
    public enum DitherMode
    {
        /// <summary>
        /// No dithering will be performed on the image.
        /// </summary>
        None = 0,

        /// <summary>
        /// A fast dithering will be performed on the image.
        /// </summary>
        Fast = 1,

        /// <summary>
        /// A higher quality dithering will be performed on the image. This is
        /// slower than the Fast option.
        /// </summary>
        Quality = 2
    }
}
