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

using Rock.Enums.Lms;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Lms.LearningClassDetail
{
    /// <summary>
    /// The additional configuration options for the Learning Class Detail block.
    /// </summary>
    public class LearningClassDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the number of active classes using the default grading system.
        /// </summary>
        public int ActiveClassesUsingDefaultGradingSystem { get; set; }

        /// <summary>
        /// Gets or sets the available grading systems.
        /// </summary>
        public List<ListItemBag> GradingSystems { get; set; }

        /// <summary>
        /// Gets or sets the related Rock.Model.LearningProgram's configuration mode.
        /// </summary>
        public ConfigurationMode? ProgramConfigurationMode { get; set; }

        /// <summary>
        /// Gets or sets the available semesters for this course.
        /// </summary>
        public List<ListItemBag> Semesters { get; set; }
    }
}
