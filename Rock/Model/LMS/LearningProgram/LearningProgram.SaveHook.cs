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
using System.Linq.Dynamic.Core;
using Rock.Data;

namespace Rock.Model
{
    public partial class LearningProgram
    {
        /// <summary>
        /// Save hook implementation for <see cref="LearningProgram"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<LearningProgram>
        {
            /// <summary>
            /// Called after the save operation is executed.
            /// </summary>
            protected override void PostSave()
            {
                base.PostSave();
                if ( State == EntityContextState.Added )
                {
                    var semesterService = new LearningSemesterService( RockContext );

                    // Ensure there's at least one semester available.
                    if ( !semesterService.Queryable().Any() )
                    {
                        semesterService.Add( new LearningSemester
                        {
                            Name = "Default Semester",
                            Guid = Guid.NewGuid()
                        } );
                    }
                }
            }
        }
    }
}