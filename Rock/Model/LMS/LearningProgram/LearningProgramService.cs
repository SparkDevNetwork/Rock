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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Enums.Lms;

namespace Rock.Model
{
    public partial class LearningProgramService
    {

        /// <summary>
        /// Get a list of all active <see cref="LearningProgram"/>s.
        /// </summary>
        /// <returns>A list of LearningProgram where the IsActive property is <c>true</c>.</returns>
        public IQueryable<LearningProgram> GetActive()
        {
            return Queryable().Where( p => p.IsActive );
        }

        /// <summary>
        /// Gets the configuration mode of the specified learning program.
        /// </summary>
        /// <param name="learningProgramId">The identifier of the learning program for which to get the configuration mode.</param>
        /// <returns>The ConfigurationMode of the <see cref="LearningProgram"/>.</returns>
        public ConfigurationMode GetConfigurationMode( int learningProgramId )
        {
            return Queryable()
                .Where( p => p.Id == learningProgramId)
                .Select( p => p.ConfigurationMode )
                .FirstOrDefault();
        }

        /// <summary>
        /// Get a list of <see cref="LearningSemester"/> for the specified Learning Program.
        /// </summary>
        /// <param name="learningProgramId">The </param>
        /// <returns></returns>
        public IQueryable<LearningSemester> Semesters ( int learningProgramId )
        {
            return Queryable()
                .Where( p => p.Id == learningProgramId )
                .Include( p => p.LearningSemesters )
                .SelectMany( p => p.LearningSemesters );
        }

        /// <summary>
        /// Gets a list of several Key Performance Indicators for the specified Learning Program.
        /// </summary>
        /// <param name="learningProgramId">The identifier of the learning program for which to get the KPIs.</param>
        /// <returns>An object containing the KPI values.</returns>
        public LearningProgramKpis GetProgramKpis ( int learningProgramId )
        {
            var context = ( RockContext ) Context;

            var classAndStudentData = new LearningClassService( context )
                .Queryable()
                .AsNoTracking()
                .Where( c => c.IsActive )
                .Where( c => c.LearningCourse.LearningProgramId == learningProgramId )
                .Select( c => new
                {
                    ClassId = c.Id,
                    StudentCount = c.LearningParticipants.Count( p => !p.GroupRole.IsLeader )
                } ).ToList();

            var completions = new LearningProgramCompletionService( context )
                .Queryable()
                .AsNoTracking()
                .Count( p => p.LearningProgramId == learningProgramId && p.CompletionStatus == CompletionStatus.Completed );

            return new LearningProgramKpis
            {
                ActiveClasses = classAndStudentData.Select( c => c.ClassId ).Distinct().Count(),
                ActiveStudents = classAndStudentData.Sum( c => c.StudentCount ),
                Completions = completions
            };
        }
    }
}