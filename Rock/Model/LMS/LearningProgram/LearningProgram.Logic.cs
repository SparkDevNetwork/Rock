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

namespace Rock.Model
{
    public partial class LearningProgram
    {

    }

    /// <summary>
    /// POCO for encapsulating KPIs for a given <see cref="LearningProgram"/> .
    /// </summary>
    public class LearningProgramKpis
    {
        /// <summary>
        ///     Gets or sets the number of active classes in the Program.
        /// </summary>
        /// <remarks>
        ///     Currently defined as the number of unique learning class Ids for the program
        ///     where the semester has started, but not ended and the class "isActive".
        /// </remarks>
        public int ActiveClasses { get; set; }

        /// <summary>
        ///     Gets or sets the number of active students in the Program.
        /// </summary>
        /// <remarks>
        ///     Currently defined as the number of unique student person Ids enrolled in any class
        ///     within the program where the <see cref="LearningCompletionStatus"/> is incomplete.
        /// </remarks>
        public int ActiveStudents { get; set; }

        /// <summary>
        ///     Gets or sets the number of students who have completed this Program.
        /// </summary>
        /// <remarks>
        ///     Currently defined as the number of unique program completions for the program
        ///     where the <see cref="CompletionStatus"/> is "Completed".
        /// </remarks>
        public int Completions { get; set; }
    }
}
