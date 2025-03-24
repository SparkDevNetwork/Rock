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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

using Rock.Data;

namespace Rock.Model
{
    public partial class LearningCourse
    {
        /// <summary>
        /// Save hook implementation for <see cref="LearningCourse"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<LearningCourse>
        {
            /// <summary>
            /// Called after the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                base.PreSave();
                if ( State == EntityContextState.Added )
                {
                    var programService = new LearningProgramService( RockContext );
                    
                    if ( !Entity.LearningClasses.Any() )
                    {
                        var lmsClassGroupTypeId = new GroupTypeService( RockContext ).GetId( SystemGuid.GroupType.GROUPTYPE_LMS_CLASS.AsGuid() ).ToIntSafe();
                        var defaultSemesterId = programService.GetDefaultSemester( Entity.LearningProgramId )?.Id;

                        // Use the default grading system specified by the LearningProgram (if any).
                        var defaultGradingSystemId = programService.GetSelect( Entity.LearningProgramId, p => p.DefaultLearningGradingSystemId ).ToIntSafe();
                        if ( defaultGradingSystemId == 0 )
                        {
                            // If no default grading system was specified by the program
                            // use the first active system (sorted by name).
                            defaultGradingSystemId = new LearningGradingSystemService( RockContext )
                                .Queryable()
                                .Where( g => g.IsActive )
                                .OrderBy( g => g.Name )
                                .Select( g => g.Id )
                                .FirstOrDefault();
                        }

                        Entity.LearningClasses = new List<LearningClass>
                        {
                            new LearningClass
                            {
                                Name = "Initial Class",
                                Guid = Guid.NewGuid(),
                                LearningCourseId = Entity.Id,
                                LearningSemesterId = defaultSemesterId,
                                LearningGradingSystemId = defaultGradingSystemId,
                                GroupTypeId = lmsClassGroupTypeId,
                                IsActive = true,
                            }
                        };
                    }
                }
            }
        }
    }
}