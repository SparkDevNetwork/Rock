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

using System.Linq;
using Rock.Data;

namespace Rock.Model
{
    public partial class RegistrationRegistrant
    {
        /// <summary>
        /// Save hook implementation for <see cref="RegistrationRegistrant"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<RegistrationRegistrant>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                if ( this.State == EntityContextState.Added )
                {
                    var registrationRegistrant = this.Entity as RegistrationRegistrant;
                    int? registrationTemplateId = registrationRegistrant.RegistrationTemplateId;

                    // If the Registration Template foreign key is not assigned, populate it now.
                    if ( registrationTemplateId == null || registrationTemplateId == 0 )
                    {
                        if ( registrationRegistrant.Registration != null && registrationRegistrant.Registration.RegistrationInstance != null )
                        {
                            registrationTemplateId = registrationRegistrant.Registration.RegistrationInstance.RegistrationTemplateId;
                        }
                        else
                        {
                            var rockContext = ( RockContext ) this.RockContext;
                            registrationTemplateId = new RegistrationService( rockContext )
                                .Queryable()
                                .Where( a => a.Id == registrationRegistrant.RegistrationId && a.RegistrationInstance != null )
                                .Select( a => a.RegistrationInstance.RegistrationTemplateId )
                                .FirstOrDefault();
                        }
                    }

                    if ( registrationTemplateId.HasValue )
                    {
                        this.Entity.RegistrationTemplateId = registrationTemplateId.Value;
                    }
                }

                base.PreSave();
            }
        }
    }
}
