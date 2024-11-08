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
    /// The configuration options for an ellipse field.
    /// </summary>
    public class EllipseFieldConfigurationBag
    {
        /// <summary>
        /// Determines if the ellipse should be drawn as a black shape or
        /// as a white shape.
        /// </summary>
        /// <value>
        /// This should be the string "False" or "True".
        /// </value>
        public string IsBlack { get; set; }

        /// <summary>
        /// Determines if the ellipse should be filled in or only an outline.
        /// </summary>
        /// <value>
        /// This should be the string "False" or "True".
        /// </value>
        public string IsFilled { get; set; }

        /// <summary>
        /// When the ellipse is not filled, this determines the thickness of
        /// the outline border around the ellipse in printer dots.
        /// </summary>
        /// <value>
        /// This is a string value of an integer.
        /// </value>
        public string BorderThickness { get; set; }
    }
}
