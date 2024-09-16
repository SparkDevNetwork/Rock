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
namespace Rock.Model
{
    public partial class LearningClass
    {
        /// <summary>
        /// Returns the label style for the class and the number of absences given.
        /// </summary>
        /// <param name="count">The number of absences for the class.</param>
        /// <param name="program">The <see cref="LearningProgram">Program</see> containing the counts for warning and critical absences. </param>
        /// <returns>The type of label to use based on the number of absences ("success", "warning", "danger" or "").</returns>
        public string AbsencesLabelStyle( int count, LearningProgram program = null )
        {
            if ( program == null )
            {
                program = LearningCourse?.LearningProgram;
            }

            if ( program == null )
            {
                return "";
            }
            else if ( count > program.AbsencesCriticalCount )
            {
                return "danger";
            }
            else if ( count > program.AbsencesWarningCount )
            {
                return "warning";
            }
            else
            {
                return "success";
            }
        }
    }
}
