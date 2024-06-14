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

namespace Rock.ViewModels.CheckIn.Labels
{
    /// <summary>
    /// The configuration options for a rectangle field.
    /// </summary>
    public class RectangleFieldConfigurationBag
    {
        /// <summary>
        /// Determines if the rectangle should be drawn as a black shape or
        /// as a white shape.
        /// </summary>
        /// <value>
        /// This should be the string "False" or "True".
        /// </value>
        public string IsBlack { get; set; }

        /// <summary>
        /// Determines if the rectangle should be filled in or only an outline.
        /// </summary>
        /// <value>
        /// This should be the string "False" or "True".
        /// </value>
        public string IsFilled { get; set; }

        /// <summary>
        /// When the rectangle is not filled, this determines the thickness of
        /// the outline border around the rectangle in printer dots.
        /// </summary>
        /// <value>
        /// This is a string value of an integer.
        /// </value>
        public string BorderThickness { get; set; }

        /// <summary>
        /// Determines the corner radius of the rectangle. The value should be
        /// between 0 and 8 with 0 being no corner radius and 8 being maximum
        /// corner radius (usually creating a circle).
        /// </summary>
        /// <value>
        /// This is a string value of an integer.
        /// </value>
        public string CornerRadius { get; set; }
    }
}
