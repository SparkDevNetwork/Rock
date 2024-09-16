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
using System.Linq;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class PhoneNumber
    {
        /// <summary>
        /// Save hook implementation for <see cref="Person"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<PhoneNumber>
        {
            /// <summary>
            /// Gets or sets the history changes.
            /// </summary>
            /// <value>
            /// The history changes.
            /// </value>
            private Dictionary<int, History.HistoryChangeList> PersonHistoryChanges { get; set; }

            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                if ( State == EntityContextState.Added || State == EntityContextState.Modified )
                {
                    if ( string.IsNullOrEmpty( Entity.CountryCode ) )
                    {
                        Entity.CountryCode = PhoneNumber.DefaultCountryCode();
                    }

                    // Clean up the number so that the Formatted number looks like (555) 123-4567 (without country code prefix)
                    Entity.NumberFormatted = PhoneNumber.FormattedNumber( Entity.CountryCode, Entity.Number );

                    // then use the NumberFormatted to set the cleaned up 'Number' value, so it would be 5551234567
                    Entity.Number = PhoneNumber.CleanNumber( Entity.NumberFormatted );
                }

                /*
                 * 2023-02-23 - MZS
                 * 
                 * This section used to check for duplicate numbers in this location.
                 * Now, a RemoveEmptyAndDuplicatePhoneNumbers method is within the PersonService to perform this task.
                 * Before the phone numbers reach this save hook, they will be filtered out to have duplicate removed and emptied. 
                 * This method is called within the block where the number is being changed.
                 * 
                 * Having the remove duplicate numbers in this location caused several issues:
                 * Duplicate numbers could still be added to a person initially when the person had no numbers
                 * Numbers were unable to be swapped (mobile and work phone number switched)
                 * 
                 */
                
                int personId = Entity.PersonId;
                PersonHistoryChanges = new Dictionary<int, History.HistoryChangeList> { { personId, new History.HistoryChangeList() } };

                switch ( State )
                {
                    case EntityContextState.Added:
                        {
                            History.EvaluateChange( PersonHistoryChanges[personId], string.Format( "{0} Phone", DefinedValueCache.GetName( Entity.NumberTypeValueId ) ), string.Empty, Entity.NumberFormatted );
                            History.EvaluateChange( PersonHistoryChanges[personId], string.Format( "{0} Phone Unlisted", DefinedValueCache.GetName( Entity.NumberTypeValueId ) ), ( bool? ) null, Entity.IsUnlisted );
                            History.EvaluateChange( PersonHistoryChanges[personId], string.Format( "{0} Phone Messaging Enabled", DefinedValueCache.GetName( Entity.NumberTypeValueId ) ), ( bool? ) null, Entity.IsMessagingEnabled );
                            break;
                        }

                    case EntityContextState.Modified:
                        {
                            string numberTypeName = DefinedValueCache.GetName( Entity.NumberTypeValueId );
                            int? oldPhoneNumberTypeId = Entry.OriginalValues[nameof( PhoneNumber.NumberTypeValueId )].ToStringSafe().AsIntegerOrNull();
                            if ( ( oldPhoneNumberTypeId ?? 0 ) == ( Entity.NumberTypeValueId ?? 0 ) )
                            {
                                History.EvaluateChange( PersonHistoryChanges[personId], string.Format( "{0} Phone", numberTypeName ), Entry.OriginalValues[nameof( PhoneNumber.NumberFormatted )].ToStringSafe(), Entity.NumberFormatted );
                            }
                            else
                            {
                                History.EvaluateChange( PersonHistoryChanges[personId], string.Format( "{0} Phone", DefinedValueCache.GetName( oldPhoneNumberTypeId ) ), Entry.OriginalValues[nameof( PhoneNumber.NumberFormatted )].ToStringSafe(), string.Empty );
                                History.EvaluateChange( PersonHistoryChanges[personId], string.Format( "{0} Phone", numberTypeName ), string.Empty, Entity.NumberFormatted );
                            }

                            History.EvaluateChange( PersonHistoryChanges[personId], string.Format( "{0} Phone Unlisted", numberTypeName ), Entry.OriginalValues[nameof( PhoneNumber.IsUnlisted )].ToStringSafe().AsBooleanOrNull(), Entity.IsUnlisted );
                            History.EvaluateChange( PersonHistoryChanges[personId], string.Format( "{0} Phone Messaging Enabled", numberTypeName ), Entry.OriginalValues[nameof( PhoneNumber.IsMessagingEnabled )].ToStringSafe().AsBooleanOrNull(), Entity.IsMessagingEnabled );

                            break;
                        }

                    case EntityContextState.Deleted:
                        {
                            personId = Entry.OriginalValues["PersonId"].ToStringSafe().AsInteger();
                            PersonHistoryChanges.TryAdd( personId, new History.HistoryChangeList() );
                            var oldPhoneNumberTypeId = Entity.NumberTypeValueId;
                            History.EvaluateChange( PersonHistoryChanges[personId], string.Format( "{0} Phone", DefinedValueCache.GetName( oldPhoneNumberTypeId ) ), Entity.NumberFormatted, string.Empty );

                            return;
                        }
                }

                base.PreSave();
            }

            /// <summary>
            /// Called after the save operation has been executed
            /// </summary>
            /// <remarks>
            /// This method is only called if <see cref="M:Rock.Data.EntitySaveHook`1.PreSave" /> returns
            /// without error.
            /// </remarks>
            protected override void PostSave()
            {
                if ( PersonHistoryChanges != null )
                {
                    foreach ( var keyVal in PersonHistoryChanges )
                    {
                        int personId = keyVal.Key > 0 ? keyVal.Key : Entity.PersonId;
                        HistoryService.SaveChanges( this.RockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), personId, keyVal.Value, true, Entity.ModifiedByPersonAliasId );
                    }
                }

                // update the ModifiedDateTime on the Person that this phone number is associated with
                var currentDateTime = RockDateTime.Now;
                var qryPersonsToUpdate = new PersonService( RockContext ).Queryable( true, true ).Where( a => a.Id == Entity.PersonId );
                RockContext.BulkUpdate( qryPersonsToUpdate, p => new Person { ModifiedDateTime = currentDateTime, ModifiedByPersonAliasId = Entity.ModifiedByPersonAliasId } );

                base.PostSave();
            }
        }
    }
}
