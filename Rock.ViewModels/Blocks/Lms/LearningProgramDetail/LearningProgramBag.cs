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

using Rock.Enums.Lms;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Lms.LearningProgramDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class LearningProgramBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the number of absences at which a critical alert should be triggered.
        /// </summary>
        public int? AbsencesCriticalCount { get; set; }

        /// <summary>
        /// Gets or sets the number of absences at which a warning should be triggered.
        /// </summary>
        public int? AbsencesWarningCount { get; set; }

        /// <summary>
        /// Gets or sets the number of active classes in the Program.
        /// </summary>
        public int ActiveClasses { get; set; }

        /// <summary>
        /// Gets or sets the number of active students in the Program.
        /// </summary>
        public int ActiveStudents { get; set; }

        /// <summary>
        /// Gets or sets the additional settings json.
        /// </summary>
        public string AdditionalSettingsJson { get; set; }

        /// <summary>
        /// Gets or sets the related Rock.Model.Category for the program.
        /// </summary>
        public ListItemBag Category { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.Category.HighlightColor for the program.
        /// </summary>
        public string CategoryColor { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.Category identifier.
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the related completion WorkflowType for the program.
        /// </summary>
        public ListItemBag CompletionWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Rock.Model.WorkflowType that is triggered when the program is completed by a student.
        /// </summary>
        public int? CompletionWorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets the number of students who have completed this Program.
        /// </summary>
        public int Completions { get; set; }

        /// <summary>
        /// Gets or sets the configuration mode of the LearningProgram.
        /// </summary>
        public ConfigurationMode? ConfigurationMode { get; set; }

        /// <summary>
        /// Gets or sets the Description of the LearningProgram.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the color of the highlight.
        /// </summary>
        public string HighlightColor { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the related Rock.Model.BinaryFile for the program.
        /// </summary>
        public ListItemBag ImageBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the binary file id of the image for the LearningProgram.
        /// </summary>
        public int? ImageBinaryFileId { get; set; }

        /// <summary>
        /// Gets a value indicating whether this LearningProgram is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets a value indicating whether this LearningProgram tracks student's completion of the program.
        /// Once a LearningProgram begins this value cannot be changed.
        /// </summary>
        public bool IsCompletionStatusTracked { get; set; }

        /// <summary>
        /// Indicates whether or not this LearningProgram should
        /// be displayed in public contexts (e.g. on a public site).
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or sets the name of the LearningProgram.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the public name of the LearningProgram.
        /// </summary>
        public string PublicName { get; set; }

        /// <summary>
        /// Gets or sets whether the KPIs are visible.
        /// </summary>
        public bool ShowKpis { get; set; }

        /// <summary>
        /// Gets or sets the summary text of the LearningProgram.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the related Rock.Model.SystemCommunication for the program.
        /// </summary>
        public ListItemBag SystemCommunication { get; set; }
    }
}
