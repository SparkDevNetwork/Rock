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

using System;

namespace Rock.ViewModels.CheckIn.Labels
{
    /// <summary>
    /// The configuration options for an image field.
    /// </summary>
    public class ImageFieldConfigurationBag
    {
        /// <summary>
        /// A unique value to identify this image data. Any time the image data
        /// is changed, this value must also be changed.
        /// </summary>
        public Guid? ImageId { get; set; }

        /// <summary>
        /// The PNG image data encoded in base 64.
        /// </summary>
        public string ImageData { get; set; }

        /// <summary>
        /// After the image has been converted to black and white, this value
        /// will be used to determine if the black and white colors should be
        /// inverted.
        /// </summary>
        /// <value>
        /// This should be the string "False" or "True".
        /// </value>
        public string IsColorInverted { get; set; }

        /// <summary>
        /// The brightness adjustment to perform on the image. A value of 1
        /// means no adjustment is made.
        /// </summary>
        /// <value>
        /// This should be a string representing a floating point value.
        /// </value>
        public string Brightness { get; set; }
    }
}
