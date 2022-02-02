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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Security;

namespace Rock.Model
{
    public partial class Step
    {

        /// <summary>
        /// Indicates if this step has been completed
        /// </summary>
        [DataMember]
        public virtual bool IsComplete
        {
            get => StepStatus != null && StepStatus.IsCompleteStatus;
        }

        /// <summary>
        /// Gets or sets the caption.
        /// </summary>
        /// <value>
        /// The caption.
        /// </value>
        [NotMapped]
        public virtual string Caption { get; set; }

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            RockContext rockContext = null;

            // Get PersonAlias.
            var person = this.PersonAlias?.Person;
            if ( person == null && this.PersonAliasId != 0 )
            {
                if ( rockContext == null )
                {
                    rockContext = new RockContext();
                }

                var personAlias = new PersonAliasService( rockContext ).Get( this.PersonAliasId );
                person = personAlias?.Person;
            }

            // Get StepType.
            var stepType = this.StepType;
            if ( stepType == null )
            {
                if ( rockContext == null )
                {
                    rockContext = new RockContext();
                }

                stepType = new StepTypeService( rockContext ).Get( this.StepTypeId );
            }

            if ( person != null && stepType != null )
            {
                if ( stepType.AllowMultiple )
                {
                    // If "AllowMultiple" is set, include the Order to distinguish this from duplicate steps.
                    return $"{stepType.Name} {this.Order} - {person.FullName}";
                }

                return $"{stepType.Name} - {person.FullName}";
            }

            return base.ToString();
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        public override bool IsValid
        {
            get
            {
                var isValid = base.IsValid;

                if ( StartDateTime.HasValue && EndDateTime.HasValue && StartDateTime.Value > EndDateTime.Value )
                {
                    ValidationResults.Add( new ValidationResult( "The StartDateTime must occur before the EndDateTime" ) );
                    isValid = false;
                }

                if ( StartDateTime.HasValue && CompletedDateTime.HasValue && StartDateTime.Value > CompletedDateTime.Value )
                {
                    ValidationResults.Add( new ValidationResult( "The StartDateTime must occur before the CompletedDateTime" ) );
                    isValid = false;
                }

                return isValid;
            }
        }

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check the default authorization on the current type, and
        /// then the authorization on the Rock.Security.GlobalDefault entity
        /// </summary>
        public override ISecured ParentAuthority
        {
            get
            {
                if ( this.StepType != null )
                {
                    return this.StepType;
                }
                else
                {
                    return base.ParentAuthority;
                }
            }
        }

        /// <summary>
        /// Return <c>true</c> if the user is authorized to perform the selected action on this object.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        /// <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsAuthorized( string action, Person person )
        {
            /* 2021-09-30 SK
             Step's record has a special MANAGE_STEPS action that grants access to managing steps.
             This is effectively the same as EDIT. So, if checking security on EDIT, also check Step Type's
             MANAGE_STEPS (in case they have MANAGE_STEPS but not EDIT)

             Possible 'action' parameters on Step are
             1) VIEW
             2) EDIT
             3) ADMINISTRATE

             NOTE: MANAGE_STEPS is NOT a possible action on Step. However, this can be confusing
             because MANAGE_STEPS is a possible action on Step Type (which would grant EDIT on its steps)

             This is how this has been implemented
             - If they can EDIT a Step Type, then can Manage Steps
             - If they can't EDIT a Step Type, but can Manage Steps, then can EDIT (which includes Add and Delete) steps.
                - Note that this is fairly complex, see StepType.IsAuthorized for how this should work

             */

            if ( action.Equals( Rock.Security.Authorization.EDIT, StringComparison.OrdinalIgnoreCase ) )
            {
                // first, see if they auth'd using normal AUTH rules
                var isAuthorized = base.IsAuthorized( action, person );
                if ( isAuthorized )
                {
                    return isAuthorized;
                }

                // now check if they are auth'd to EDIT or MANAGE_STEPS on this Step's Step Type
                var stepType = this.StepType ?? new StepTypeService( new RockContext() ).Get( this.StepTypeId );

                if ( stepType != null )
                {
                    // if they have EDIT on the step type, they can edit Steps records
                    var canEditMembers = stepType.IsAuthorized( Rock.Security.Authorization.EDIT, person );
                    if ( !canEditMembers )
                    {
                        // if they don't have EDIT on the step type, but do have MANAGE_STEPS, then they can 'edit' steps
                        canEditMembers = stepType.IsAuthorized( Rock.Security.Authorization.MANAGE_STEPS, person );
                    }

                    return canEditMembers;
                }
            }

            return base.IsAuthorized( action, person );
        }

        #endregion Overrides
    }
}
