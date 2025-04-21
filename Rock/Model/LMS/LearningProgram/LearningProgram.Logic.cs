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

using Rock.Enums.Lms;
using Rock.Security;

namespace Rock.Model
{
    public partial class LearningProgram
    {
        /// <inheritdoc/>
        public override bool IsAuthorized( string action, Rock.Model.Person person )
        {
            // Check to see if user is authorized using normal authorization rules
            bool authorized = base.IsAuthorized( action, person );

            if ( authorized )
            {
                return authorized;
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

            return false;
        }

        /// <summary>
        /// Provides a <see cref="Dictionary{TKey, TValue}"/> of actions that this model supports, and the description of each.
        /// </summary>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var supportedActions = base.SupportedActions;
                supportedActions.AddOrReplace( Authorization.VIEW_GRADES, "The roles and/or users that have access to view grades." );
                supportedActions.AddOrReplace( Authorization.EDIT_GRADES, "The roles and/or users that have access to edit grades." );
                return supportedActions;
            }
        }
    }
}
