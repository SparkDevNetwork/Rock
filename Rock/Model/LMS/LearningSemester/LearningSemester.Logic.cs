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
using System.ComponentModel.DataAnnotations.Schema;

namespace Rock.Model
{
    public partial class LearningSemester
    {
        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        [NotMapped]
        public override Security.ISecured ParentAuthority
        {
            get
            {
                if ( this.LearningProgram?.Id > 0 )
                {
                    return this.LearningProgram;
                }
                else
                {
                    return this.LearningProgramId > 0 ?
                        new LearningProgramService( new Data.RockContext() ).Get( this.LearningProgramId ) :
                        base.ParentAuthority;
                }
            }
        }
    }
}
