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
    /// The type of field on a label.
    /// </summary>
    public enum LabelFieldType
    {
        /// <summary>
        /// A text field, the sub type specifies the exact text field type.
        /// </summary>
        Text = 0,

        /// <summary>
        /// A field that displays a line between two points. The left and top
        /// position of the field designate the first point. The relative offset
        /// of the field's width and height designate the second point. Meaning
        /// width and height can be negative.
        /// </summary>
        Line = 1,

        /// <summary>
        /// A field that displays a rectangle.
        /// </summary>
        Rectangle = 2,

        /// <summary>
        /// A field that displays an ellipse.
        /// </summary>
        Ellipse = 3,

        /// <summary>
        /// An icon field. This presents a pre-defined list of images to use
        /// on the label.
        /// </summary>
        Icon = 4,

        /// <summary>
        /// An image field. This allows for uploading custom images.
        /// </summary>
        Image = 5,

        /// <summary>
        /// A field that displays the attendee photo.
        /// </summary>
        AttendeePhoto = 6,

        /// <summary>
        /// A field that displays a barcode that can be scanned.
        /// </summary>
        Barcode = 7
    }
}
