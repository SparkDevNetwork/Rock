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
using Rock.Transactions;

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
            private int? preSavePersonAliasId { get; set; }

            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                var registrationRegistrant = this.Entity as RegistrationRegistrant;

                if ( this.State == EntityContextState.Added )
                {
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
                else if ( State == EntityContextState.Modified )
                {
                    preSavePersonAliasId = registrationRegistrant.PersonAliasId;
                }
                else if ( State == EntityContextState.Deleted )
                {
                    preSavePersonAliasId = ( int? ) OriginalValues[nameof( registrationRegistrant.PersonAliasId )];
                }

                base.PreSave();
            }

            protected override void PostSave()
            {
                // If we need to send a real-time notification then do so after
                // this change has been committed to the database.
                if ( ShouldSendRealTimeMessage() )
                {
                    // EF will null out this field when deleting, so we need to
                    // snag it from the original values in that case.
                    var personAliasId = State == EntityContextState.Deleted
                        ? preSavePersonAliasId
                        : Entity.PersonAliasId;

                    var registrantState = new RegistrationRegistrantService.RegistrationRegistrantUpdatedState( Entity, personAliasId.Value, State );

                    new SendRegistrantRealTimeNotificationsTransaction( registrantState ).Enqueue( true );
                }

                base.PostSave();
            }

            /// <summary>
            /// Determines if we need to send any real-time messages for the
            /// changes made to this entity.
            /// </summary>
            /// <returns><c>true</c> if a message should be sent, <c>false</c> otherwise.</returns>
            private bool ShouldSendRealTimeMessage()
            {
                if ( !RockContext.IsRealTimeEnabled )
                {
                    return false;
                }

                if ( PreSaveState == EntityContextState.Added )
                {
                    return true;
                }
                //else if ( PreSaveState == EntityContextState.Modified )
                //{
                //    if ( ( Entity.DidAttend ?? false ) != ( ( ( bool? ) OriginalValues[nameof( Entity.DidAttend )] ) ?? false ) )
                //    {
                //        return true;
                //    }
                //    else if ( Entity.RSVP != ( RSVP ) OriginalValues[nameof( Entity.RSVP )] )
                //    {
                //        return true;
                //    }
                //    else if ( Entity.PresentDateTime != ( DateTime? ) OriginalValues[nameof( Entity.PresentDateTime )] )
                //    {
                //        return true;
                //    }
                //    else if ( Entity.EndDateTime != ( DateTime? ) OriginalValues[nameof( Entity.EndDateTime )] )
                //    {
                //        return true;
                //    }
                //    else if ( Entity.CheckInStatus != ( CheckInStatus ) OriginalValues[nameof( Entity.CheckInStatus )] )
                //    {
                //        return true;
                //    }
                //}
                else if ( PreSaveState == EntityContextState.Deleted )
                {
                    return true;
                }

                return false;
            }
        }
    }
}
