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

using System.Collections.Generic;

namespace Rock.ViewModels.CheckIn.Labels
{
    /// <summary>
    /// Represents a single check-in label built with the designer UI and
    /// all the information required to edit or print the label.
    /// </summary>
    public class DesignedLabelBag
    {
        /// <summary>
        /// The width of the label in inches.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// The height of the label in inches.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// The list of objects that describe each field that exists on the
        /// label.
        /// </summary>
        public List<LabelFieldBag> Fields { get; set; }
    }
}
