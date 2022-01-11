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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class PhoneNumber
    {
        #region Properties

        /// <summary>
        /// Gets the full phone number (country code and number). This should really only be used for queries and comparisons. 
        /// NOTE: If <seealso cref="Number"/> is a partial number (for example, no area code), then FullNumber isn't really the full number, and neither is NumberFormatted.
        /// </summary>
        /// <value>
        /// The full number.
        /// </value>
        [Required]
        [MaxLength( 23 )]
        [DataMember( IsRequired = true )]
        [Index( "IX_FullNumber" )]
        public string FullNumber
        {
            get
            {
                _fullNumber = this.CountryCode + this.Number;
                return _fullNumber;
            }

            private set
            {
                _fullNumber = value;
            }
        }

        private string _fullNumber;

        /// <summary>
        /// Gets the number formatted with country code.
        /// </summary>
        /// <value>
        /// The number formatted with country code.
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual string NumberFormattedWithCountryCode
        {
            get { return PhoneNumber.FormattedNumber( CountryCode, Number, true ); }
            private set { }
        }

        /// <summary>
        /// Gets or sets the history changes.
        /// </summary>
        /// <value>
        /// The history changes.
        /// </value>
        [NotMapped]
        private Dictionary<int, History.HistoryChangeList> PersonHistoryChanges { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="entry"></param>
        public override void PreSaveChanges( Data.DbContext dbContext, DbEntityEntry entry )
        {
            if ( entry.State == EntityState.Added || entry.State == EntityState.Modified )
            {
                if ( string.IsNullOrEmpty( CountryCode ) )
                {
                    CountryCode = PhoneNumber.DefaultCountryCode();
                }

                // Clean up the number so that the Formatted number looks like (555) 123-4567 (without country code prefix)
                NumberFormatted = PhoneNumber.FormattedNumber( CountryCode, Number );

                // then use the NumberFormatted to set the cleaned up 'Number' value, so it would be 5551234567
                Number = PhoneNumber.CleanNumber( NumberFormatted );
            }

            // Check for duplicate
            if ( entry.State == EntityState.Added || entry.State == EntityState.Modified )
            {
                var rockContext = ( RockContext ) dbContext;
                var phoneNumberService = new PhoneNumberService( rockContext );
                var duplicates = phoneNumberService.Queryable().Where( pn => pn.PersonId == PersonId && pn.Number == Number && pn.CountryCode == CountryCode );

                // Make sure this number isn't considered a duplicate
                if ( entry.State == EntityState.Modified )
                {
                    duplicates = duplicates.Where( d => d.Id != Id );
                }

                if ( duplicates.Any() )
                {
                    var highestOrderedDuplicate = duplicates.Where( p => p.NumberTypeValue != null ).OrderBy( p => p.NumberTypeValue.Order ).FirstOrDefault();
                    if ( NumberTypeValueId.HasValue && highestOrderedDuplicate != null && highestOrderedDuplicate.NumberTypeValue != null )
                    {
                        // Ensure that we preserve the PhoneNumber with the highest preference phone type
                        var numberType = DefinedValueCache.Get( NumberTypeValueId.Value, rockContext );
                        if ( highestOrderedDuplicate.NumberTypeValue.Order < numberType.Order )
                        {
                            entry.State = entry.State == EntityState.Added ? EntityState.Detached : EntityState.Deleted;
                        }
                        else
                        {
                            phoneNumberService.DeleteRange( duplicates );
                        }
                    }
                }
            }

            int personId = PersonId;
            PersonHistoryChanges = new Dictionary<int, History.HistoryChangeList> { { personId, new History.HistoryChangeList() } };

            switch ( entry.State )
            {
                case EntityState.Added:
                    {

                        History.EvaluateChange( PersonHistoryChanges[personId], string.Format( "{0} Phone", DefinedValueCache.GetName( NumberTypeValueId ) ), string.Empty, NumberFormatted );
                        History.EvaluateChange( PersonHistoryChanges[personId], string.Format( "{0} Phone Unlisted", DefinedValueCache.GetName( NumberTypeValueId ) ), ( bool? ) null, IsUnlisted );
                        History.EvaluateChange( PersonHistoryChanges[personId], string.Format( "{0} Phone Messaging Enabled", DefinedValueCache.GetName( NumberTypeValueId ) ), ( bool? ) null, IsMessagingEnabled );
                        break;
                    }

                case EntityState.Modified:
                    {
                        string numberTypeName = DefinedValueCache.GetName( NumberTypeValueId );
                        int? oldPhoneNumberTypeId = entry.OriginalValues[nameof( this.NumberTypeValueId )].ToStringSafe().AsIntegerOrNull();
                        if ( ( oldPhoneNumberTypeId ?? 0 ) == ( NumberTypeValueId ?? 0 ) )
                        {
                            History.EvaluateChange( PersonHistoryChanges[personId], string.Format( "{0} Phone", numberTypeName ), entry.OriginalValues[nameof( this.NumberFormatted )].ToStringSafe(), NumberFormatted );
                        }
                        else
                        {
                            History.EvaluateChange( PersonHistoryChanges[personId], string.Format( "{0} Phone", DefinedValueCache.GetName( oldPhoneNumberTypeId ) ), entry.OriginalValues[nameof( this.NumberFormatted )].ToStringSafe(), string.Empty );
                            History.EvaluateChange( PersonHistoryChanges[personId], string.Format( "{0} Phone", numberTypeName ), string.Empty, NumberFormatted );
                        }

                        History.EvaluateChange( PersonHistoryChanges[personId], string.Format( "{0} Phone Unlisted", numberTypeName ), entry.OriginalValues[nameof( this.IsUnlisted )].ToStringSafe().AsBooleanOrNull(), IsUnlisted );
                        History.EvaluateChange( PersonHistoryChanges[personId], string.Format( "{0} Phone Messaging Enabled", numberTypeName ), entry.OriginalValues[nameof( this.IsMessagingEnabled )].ToStringSafe().AsBooleanOrNull(), IsMessagingEnabled );

                        break;
                    }

                case EntityState.Deleted:
                    {
                        personId = entry.OriginalValues[nameof( this.PersonId )].ToStringSafe().AsInteger();
                        PersonHistoryChanges.AddOrIgnore( personId, new History.HistoryChangeList() );
                        int? oldPhoneNumberTypeId = entry.OriginalValues[nameof( this.NumberTypeValueId )].ToStringSafe().AsIntegerOrNull();
                        History.EvaluateChange( PersonHistoryChanges[personId], string.Format( "{0} Phone", DefinedValueCache.GetName( oldPhoneNumberTypeId ) ), entry.OriginalValues[nameof( this.NumberFormatted )].ToStringSafe(), string.Empty );

                        return;
                    }
            }

            base.PreSaveChanges( dbContext, entry );
        }

        /// <summary>
        /// Posts the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( Data.DbContext dbContext )
        {
            var rockContext = dbContext as RockContext;
            if ( PersonHistoryChanges != null )
            {
                foreach ( var keyVal in PersonHistoryChanges )
                {
                    int personId = keyVal.Key > 0 ? keyVal.Key : PersonId;
                    HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), personId, keyVal.Value, true, this.ModifiedByPersonAliasId );
                }
            }

            // update the ModifiedDateTime on the Person that this phone number is associated with
            var currentDateTime = RockDateTime.Now;
            var qryPersonsToUpdate = new PersonService( rockContext ).Queryable( true, true ).Where( a => a.Id == this.PersonId );
            rockContext.BulkUpdate( qryPersonsToUpdate, p => new Person { ModifiedDateTime = currentDateTime, ModifiedByPersonAliasId = this.ModifiedByPersonAliasId } );

            base.PostSaveChanges( dbContext );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the Number and represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the Number and represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( !IsUnlisted )
            {
                return FormattedNumber( CountryCode, Number );
            }
            else
            {
                return "Unlisted";
            }
        }

        /// <summary>
        /// Converts the phone number to a string that is correctly formatted to be used as an SMS phone number.
        /// </summary>
        /// <returns></returns>
        public string ToSmsNumber()
        {
            string smsNumber = this.Number;
            if ( !string.IsNullOrWhiteSpace( this.CountryCode ) )
            {
                smsNumber = "+" + this.CountryCode + this.Number;
            }
            return smsNumber;
        }

        /// <summary>
        /// Gets the defaults country code.
        /// </summary>
        /// <returns></returns>
        public static string DefaultCountryCode()
        {
            var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.COMMUNICATION_PHONE_COUNTRY_CODE.AsGuid() );
            if ( definedType != null )
            {
                string countryCode = definedType.DefinedValues.OrderBy( v => v.Order ).Select( v => v.Value ).FirstOrDefault();
                if ( !string.IsNullOrWhiteSpace( countryCode ) )
                {
                    return countryCode;
                }
            }

            return "1";
        }

        /// <summary>
        /// Formats the PhoneNumber in the format defined for the COMMUNICATION_PHONE_COUNTRY_CODE defined value(s).
        /// For example, for formatted number would look something like '(555) 555-1212'.
        /// </summary>
        /// <param name="countryCode">The country code.</param>
        /// <param name="number">A <see cref="System.String" /> containing the number to format.</param>
        /// <param name="includeCountryCode">if set to <c>true</c> [include country code].</param>
        /// <returns>
        /// A <see cref="System.String" /> containing the formatted number.
        /// </returns>
        public static string FormattedNumber( string countryCode, string number, bool includeCountryCode = false )
        {
            if ( string.IsNullOrWhiteSpace( number ) )
            {
                return string.Empty;
            }

            number = CleanNumber( number );

            var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.COMMUNICATION_PHONE_COUNTRY_CODE.AsGuid() );
            if ( definedType != null )
            {
                var definedValues = definedType.DefinedValues.OrderBy( v => v.Order );
                if ( definedValues != null && definedValues.Any() )
                {
                    if ( string.IsNullOrWhiteSpace( countryCode ) )
                    {
                        countryCode = definedValues.Select( v => v.Value ).FirstOrDefault();
                    }

                    foreach ( var phoneFormat in definedValues.Where( v => v.Value.Equals( countryCode ) ).OrderBy( v => v.Order ) )
                    {
                        string match = phoneFormat.GetAttributeValue( "MatchRegEx" );
                        string replace = phoneFormat.GetAttributeValue( "FormatRegEx" );
                        if ( !string.IsNullOrWhiteSpace( match ) && !string.IsNullOrWhiteSpace( replace ) )
                        {
                            number = Regex.Replace( number, match, replace, RegexOptions.None );
                        }
                    }
                }
            }

            if ( includeCountryCode )
            {
                if ( string.IsNullOrWhiteSpace( countryCode ) )
                {
                    countryCode = "1";
                }

                number = string.Format( "+{0} {1}", countryCode, number );
            }

            return number;
        }

        /// <summary>
        /// Removes non-numeric characters from a provided number
        /// </summary>
        /// <param name="number">A <see cref="System.String"/> containing the phone number to clean.</param>
        /// <returns>A <see cref="System.String"/> containing the phone number with all non numeric characters removed. </returns>
        public static string CleanNumber( string number )
        {
            if ( !string.IsNullOrEmpty( number ) )
            {
                return digitsOnly.Replace( number, string.Empty );
            }
            else
            {
                return string.Empty;
            }
        }

        private static Regex digitsOnly = new Regex( @"[^\d]" );

        #endregion
    }
}
