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
    /// The configuration options for a line field.
    /// </summary>
    public class LineFieldConfigurationBag
    {
        /// <summary>
        /// Determines if the line should be drawn as a black shape or
        /// as a white shape.
        /// </summary>
        /// <value>
        /// This should be the string "False" or "True".
        /// </value>
        public string IsBlack { get; set; }

        /// <summary>
        /// Determines the thickness of the line in printer dots.
        /// </summary>
        /// <value>
        /// This is a string value of an integer.
        /// </value>
        public string Thickness { get; set; }
    }
}
