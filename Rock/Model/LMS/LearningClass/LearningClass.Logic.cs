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
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class LearningClass
    {
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

            // We need to explicitly call to the ParentAuthority's IsAuthorized
            // method so that the logic for VIEW_GRADES actions can be evaluated;
            // the base security logic will just recursively call
            // Rock.Security.Authorization.ItemAuthorized which doesn't
            // know anything about the special handling of "VIEW_GRADES" and "EDIT_GRADES".
            return ParentAuthority.IsAuthorized( action, person );
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
                return this.LearningCourse != null ? this.LearningCourse : base.ParentAuthority;
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
    }
}
