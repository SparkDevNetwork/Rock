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

namespace Rock.ViewModels.Blocks.Engagement.StepProgramList
{
    /// <summary>
    ///  Represents an entry in the list of Step Programs.
    /// </summary>
    public class StepProgramListBag
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        /// <value>
        /// Displays ID
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        /// <value>
        /// Display name of the program
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the icon
        /// </summary>
        /// <value>
        /// Display the icon
        /// </value>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the category name
        /// </summary>
        /// <value>
        /// Display category name
        /// </value>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the order
        /// </summary>
        /// <value>
        /// Display the order
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the step types count
        /// </summary>
        /// <value>
        /// Display count of step types
        /// </value>
        public int StepTypeCount { get; set; }

        /// <summary>
        /// Gets or sets the steps taken count
        /// </summary>
        /// <value>
        /// Display count of steps taken
        /// </value>
        public int StepCompletedCount { get; set; }

    }

}