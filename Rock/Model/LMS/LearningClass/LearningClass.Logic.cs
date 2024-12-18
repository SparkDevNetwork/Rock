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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using Rock.Data;
using Rock.Lava;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class LearningClass
    {
        /// <summary>
        /// Backing field for <see cref="TypeId"/>.
        /// </summary>
        private int? _typeId;

        /// <summary>
        /// Backing field for <see cref="TypeName"/>.
        /// </summary>
        private string _typeName = null;

        /// <inheritdoc/>
        public override bool IsAuthorized( string action, Rock.Model.Person person )
        {
            // Check to see if user is authorized using normal authorization rules
            bool authorized = base.IsAuthorized( action, person );

            if ( authorized )
            {
                return true;
            }

            // Authorize "ViewGrades" when the person has "EditGrades".
            if ( action == Authorization.VIEW_GRADES )
            {
                // We only need to check for the additional action, "EditGrades" because
                // the call to base.IsAuthorized would already have checked for "ViewGrades".
                var isAuthorizedToEditGrades = Authorization.Authorized( this, Authorization.EDIT_GRADES, person );

                if ( isAuthorizedToEditGrades )
                {
                    return true;
                }
            }

            // We need the person to check for the facilitator-level role access.
            // If the person wasn't provided then we can skip the database call.
            if ( person == null )
            {
                return ParentAuthority.IsAuthorized( action, person );
            }

            // If the person isn't authorized through normal security roles,
            // check if the person has a role that authorizes them ( a Facilitator role ).
            // Facilitators should have authorization for all actions.
            var groupType = GroupTypeCache.Get( this.GroupTypeId );
            var facilitatorRoleIds = groupType?.Roles.Where( r => r.IsLeader ).Select( r => r.Id ) ?? new List<int>();

            var hasFaciltiatorRole = Members.Any( p => p.PersonId == person.Id && facilitatorRoleIds.Contains( p.GroupRoleId ) );

            if ( hasFaciltiatorRole )
            {
                return true;
            }

            using ( var rockContext = new RockContext() )
            {
                hasFaciltiatorRole = new LearningParticipantService( rockContext )
                    .Queryable()
                    .Any( p =>
                        p.LearningClassId == this.Id
                        && p.PersonId == person.Id
                        && p.GroupRole.IsLeader );

                if ( hasFaciltiatorRole )
                {
                    return true;
                }
            }

            return false;
        }

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
                if ( this.LearningCourse?.Id > 0 )
                {
                    return this.LearningCourse;
                }
                else
                {
                    return this.LearningCourseId > 0 ?
                        new LearningCourseService( new Data.RockContext() ).Get( this.LearningCourseId ) :
                        base.ParentAuthority;
                }
            }
        }

        /// <summary>
        /// Provides a <see cref="Dictionary{TKey, TValue}"/> of actions that this model supports, and the description of each.
        /// </summary>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                if ( _supportedActions == null )
                {
                    _supportedActions = new Dictionary<string, string>
                    {
                        { Authorization.VIEW, "The roles and/or users that have access to view." },
                        { Authorization.EDIT, "The roles and/or users that have access to edit." },
                        { Authorization.ADMINISTRATE, "The roles and/or users that have access to administrate." },
                        { Authorization.VIEW_GRADES, "The roles and/or users that have access to view grades." },
                        { Authorization.EDIT_GRADES, "The roles and/or users that have access to edit grades."}
                    };
                }

                return _supportedActions;
            }
        }

        private Dictionary<string, string> _supportedActions;

        /// <summary>
        /// Returns the label style for the class and the number of absences given.
        /// </summary>
        /// <param name="count">The number of absences for the class.</param>
        /// <param name="program">The <see cref="LearningProgram">Program</see> containing the counts for warning and critical absences.</param>
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

        /// <inheritdoc/>
        [LavaVisible]
        public override int TypeId
        {
            get
            {
                if ( _typeId == null )
                {
                    // Once this instance is created, there is no need to set the _typeId more than once.
                    // Also, read should never return null since it will create entity type if it doesn't exist.
                    _typeId = EntityTypeCache.GetId<LearningClass>();
                }

                return _typeId.Value;

            }
        }

        /// <inheritdoc/>
        [NotMapped]
        [LavaVisible]
        public override string TypeName
        {
            get
            {
                if ( _typeName.IsNullOrWhiteSpace() )
                {
                    // Once this instance is created, there is no need to set the _typeName more than once.
                    // Also, read should never return null since it will create entity type if it doesn't exist.
                    _typeName = typeof( LearningClass ).FullName;
                }

                return _typeName;
            }
        }
    }
}
