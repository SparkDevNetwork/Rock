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

                // Check for duplicate
                if ( State == EntityContextState.Added || State == EntityContextState.Modified )
                {
                    var rockContext = ( RockContext ) this.RockContext;
                    var phoneNumberService = new PhoneNumberService( rockContext );
                    var duplicates = phoneNumberService.Queryable().Where( pn => pn.PersonId == Entity.PersonId && pn.Number == Entity.Number && pn.CountryCode == Entity.CountryCode );

                    // Make sure this number isn't considered a duplicate
                    if ( State == EntityContextState.Modified )
                    {
                        duplicates = duplicates.Where( d => d.Id != Entity.Id );
                    }

                    if ( duplicates.Any() )
                    {
                        var highestOrderedDuplicate = duplicates.Where( p => p.NumberTypeValue != null ).OrderBy( p => p.NumberTypeValue.Order ).FirstOrDefault();
                        if ( Entity.NumberTypeValueId.HasValue && highestOrderedDuplicate != null && highestOrderedDuplicate.NumberTypeValue != null )
                        {
                            // Ensure that we preserve the PhoneNumber with the highest preference phone type
                            var numberType = DefinedValueCache.Get( Entity.NumberTypeValueId.Value, rockContext );
                            if ( highestOrderedDuplicate.NumberTypeValue.Order < numberType.Order )
                            {
                                Entity.NumberTypeValueId = highestOrderedDuplicate.NumberTypeValueId;
                            }

                            phoneNumberService.DeleteRange( duplicates );
                        }
                    }
                }

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
                            PersonHistoryChanges.AddOrIgnore( personId, new History.HistoryChangeList() );
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
