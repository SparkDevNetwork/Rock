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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.NamelessPersonList;
using Rock.ViewModels.Rest.Controls;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Nameless Person List" )]
    [Category( "CRM" )]
    [Description( "List unmatched phone numbers with an option to link to a person that has the same phone number." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]


    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "911EA779-AC00-4A93-B706-B6A642C727CB" )]
    [Rock.SystemGuid.BlockTypeGuid( "6e9672e6-ee42-4aac-b0a9-b041c3b8368c" )]
    [CustomizedGrid]
    public class NamelessPersonList : RockListBlockType<NamelessPersonList.NamelessPersonRow>
    {
        #region Keys

        #endregion Keys

        #region Block Actions
        [BlockAction( "LinkToExistingPerson" )]
        public BlockActionResult LinkToExistingPerson( string existingPersonAliasGuid, int namelessPersonId )
        {
            using ( var rockContext = new RockContext() )
            {
                var personAliasService = new PersonAliasService( rockContext );
                var personService = new PersonService( rockContext );

                // Get the Person Alias and then the associated Person using the Person Alias GUID
                var personAlias = personAliasService.Get( new Guid( existingPersonAliasGuid ) );
                if ( personAlias == null )
                {
                    return ActionBadRequest( "Person Alias not found" );
                }

                // Get the existing person using the associated Person's ID
                var existingPerson = personService.Get( personAlias.PersonId );
                if ( existingPerson == null )
                {
                    return ActionBadRequest( "Existing person not found" );
                }

                // Get the nameless person by their ID
                var namelessPerson = personService.Get( namelessPersonId );
                if ( namelessPerson == null )
                {
                    return ActionBadRequest( "Nameless person not found" );
                }

                // Create and save the merge request
                var mergeRequest = existingPerson.CreateMergeRequest( namelessPerson );
                var entitySetService = new EntitySetService( rockContext );
                entitySetService.Add( mergeRequest );
                rockContext.SaveChanges();

                // Redirect to merge page
                var mergePageUrl = string.Format( "/PersonMerge/{0}", mergeRequest.Id );
                return new BlockActionResult( System.Net.HttpStatusCode.OK, mergePageUrl );
            }
        }

        [BlockAction( "Save" )]
        public BlockActionResult Save( PersonBasicEditorBag personBag, int namelessPersonId )
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var cleanMobilePhone = PhoneNumber.CleanNumber( personBag.MobilePhoneNumber );

                // Attempt to find an existing person by phone number
                var existingPerson = personService.Queryable( "PhoneNumbers" )
                    .FirstOrDefault( p => p.PhoneNumbers.Any( n => n.Number == cleanMobilePhone ) );

                var namelessPerson = personService.Get( namelessPersonId );
                if ( namelessPerson == null )
                {
                    return ActionNotFound( "Nameless person not found." );
                }

                Person personToUpdate = existingPerson ?? new Person();

                if ( existingPerson == null )
                {
                    personService.Add( personToUpdate );
                }

                UpdatePersonFromEditorBag( personToUpdate, personBag, rockContext );

                rockContext.SaveChanges();

                // Create a merge request
                var mergeRequest = namelessPerson.CreateMergeRequest( personToUpdate );
                var entitySetService = new EntitySetService( rockContext );
                entitySetService.Add( mergeRequest );
                rockContext.SaveChanges();

                var mergePageUrl = string.Format( "/PersonMerge/{0}", mergeRequest.Id );
                return new BlockActionResult( System.Net.HttpStatusCode.OK, mergePageUrl );
            }
        }

        private void UpdatePersonFromEditorBag( Person person, PersonBasicEditorBag personBag, RockContext rockContext )
        {
            // Update person properties from PersonBasicEditorBag
            person.FirstName = personBag.FirstName;
            person.NickName = personBag.NickName ?? person.FirstName;
            person.LastName = personBag.LastName;
            person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
            person.RecordStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
            person.ConnectionStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT.AsGuid() ).Id;

            if ( personBag.PersonConnectionStatus != null )
            {
                person.ConnectionStatusValueId = DefinedValueCache.Get( personBag.PersonConnectionStatus.Value )?.Id;
            }
            if ( personBag.PersonTitle != null )
            {
                person.TitleValueId = DefinedValueCache.Get( personBag.PersonTitle.Value )?.Id;
            }
            if ( personBag.PersonSuffix != null )
            {
                person.SuffixValueId = DefinedValueCache.Get( personBag.PersonSuffix.Value )?.Id;
            }
            if ( personBag.PersonMaritalStatus != null )
            {
                person.MaritalStatusValueId = DefinedValueCache.Get( personBag.PersonMaritalStatus.Value )?.Id;
            }
            if ( personBag.PersonRace != null )
            {
                person.RaceValueId = DefinedValueCache.Get( personBag.PersonRace.Value )?.Id;
            }
            if ( personBag.PersonEthnicity != null )
            {
                person.EthnicityValueId = DefinedValueCache.Get( personBag.PersonEthnicity.Value )?.Id;
            }
            if ( personBag.PersonGender.HasValue )
            {
                person.Gender = personBag.PersonGender.Value;
            }
            if ( personBag.PersonBirthDate != null && personBag.PersonBirthDate.Day != default( int ) && personBag.PersonBirthDate.Month != default( int ) && personBag.PersonBirthDate.Year != default( int ) )
            {
                person.SetBirthDate( new DateTime( personBag.PersonBirthDate.Year, personBag.PersonBirthDate.Month, personBag.PersonBirthDate.Day ) );
            }
            if ( personBag.PersonGradeOffset != null )
            {
                int offset = Int32.Parse( personBag.PersonGradeOffset.Value );

                if ( offset >= 0 )
                {
                    person.GradeOffset = offset;
                }
            }

            UpdatePhoneNumber( person, personBag, rockContext );
        }

        private void UpdatePhoneNumber( Person person, PersonBasicEditorBag personBag, RockContext rockContext )
        {
            if ( !string.IsNullOrWhiteSpace( personBag.MobilePhoneNumber ) )
            {
                var cleanNumber = PhoneNumber.CleanNumber( personBag.MobilePhoneNumber );
                var phone = person.PhoneNumbers.FirstOrDefault( pn => pn.Number == cleanNumber );
                if ( phone == null )
                {
                    phone = new PhoneNumber
                    {
                        Number = cleanNumber,
                        CountryCode = personBag.MobilePhoneCountryCode,
                        IsMessagingEnabled = personBag.IsMessagingEnabled ?? false,
                        NumberTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).Id
                    };
                    person.PhoneNumbers.Add( phone );
                }
                else
                {
                    phone.CountryCode = personBag.MobilePhoneCountryCode;
                    phone.IsMessagingEnabled = personBag.IsMessagingEnabled ?? false;
                    phone.Number = cleanNumber;
                }
            }
        }

        #endregion Block Actions

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<NamelessPersonListOptionsBag>();
            var builder = GetGridBuilder();

            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private NamelessPersonListOptionsBag GetBoxOptions()
        {
            var options = new NamelessPersonListOptionsBag();

            return options;
        }

        /// <inheritdoc/>
        protected override IQueryable<NamelessPersonRow> GetListQueryable( RockContext rockContext )
        {
            var namelessPersonRecordTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() )?.Id;
            var currentMergeRequestQry = PersonService.GetMergeRequestQuery( rockContext );
            var personService = new PersonService( rockContext );

            var qry = personService
                .Queryable( new PersonService.PersonQueryOptions() { IncludeNameless = true } )
                .Where( p => p.RecordTypeValueId == namelessPersonRecordTypeId )
                .Where( p => !currentMergeRequestQry.Any( mr => mr.Items.Any( i => i.EntityId == p.Id ) ) )
                .AsNoTracking()
                .Select( p => new NamelessPersonRow
                {
                    Person = p,
                    RecordTypeValue = p.RecordTypeValue != null ? p.RecordTypeValue.Value : string.Empty,
                    RecordStatusValue = p.RecordStatusValue != null ? p.RecordStatusValue.Value : string.Empty,
                    RecordStatusReasonValue = p.RecordStatusReasonValue != null ? p.RecordStatusReasonValue.Value : string.Empty,
                    ConnectionStatusValue = p.ConnectionStatusValue != null ? p.ConnectionStatusValue.Value : string.Empty,
                    ReviewReasonValue = p.ReviewReasonValue != null ? p.ReviewReasonValue.Value : string.Empty,
                    TitleValue = p.TitleValue != null ? p.TitleValue.Value : string.Empty,
                    SuffixValue = p.SuffixValue != null ? p.SuffixValue.Value : string.Empty,
                    MaritalStatusValue = p.MaritalStatusValue != null ? p.MaritalStatusValue.Value : string.Empty,
                    PreferredLanguageValue = p.PreferredLanguageValue != null ? p.PreferredLanguageValue.Value : string.Empty,
                    RaceValue = p.RaceValue != null ? p.RaceValue.Value : string.Empty,
                    EthnicityValue = p.EthnicityValue != null ? p.EthnicityValue.Value : string.Empty,
                    PrimaryAliasId = p.PrimaryAliasId,

                    PhoneNumbersFormatted = p.PhoneNumbers
                        .Where( pn => pn.NumberFormatted != null && pn.NumberFormatted != "" )
                        .Select( pn => pn.NumberFormatted )
                } );

            return qry;
        }

        /// <inheritdoc/>
        protected override IQueryable<NamelessPersonRow> GetOrderedListQueryable( IQueryable<NamelessPersonRow> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( a => a.Person.CreatedDateTime );
        }

        /// <inheritdoc/>
        protected override GridBuilder<NamelessPersonRow> GetGridBuilder()
        {
            return new GridBuilder<NamelessPersonRow>()
                .WithBlock( this )
                .AddPersonField( "Person", a => a.Person )
                .AddTextField( "idKey", a => a.Person.IdKey )
                .AddTextField( "guid", a => a.Person.Guid.ToString() )
                .AddField( "id", a => a.Person.Id )
                .AddField( "isSystem", a => a.Person.IsSystem.ToTrueFalse() )
                .AddField( "recordType", a => a.RecordTypeValue )
                .AddField( "recordStatus", a => a.RecordStatusValue )
                .AddField( "recordStatusLastModifiedDateTime", a => a.Person.RecordStatusLastModifiedDateTime )
                .AddField( "recordStatusReasonValue", a => a.RecordStatusReasonValue )
                .AddField( "connectionStatusValue", a => a.ConnectionStatusValue )
                .AddField( "reviewReasonValue", a => a.ReviewReasonValue )
                .AddField( "isDeceased", a => a.Person.IsDeceased )
                .AddField( "titleValue", a => a.TitleValue )
                .AddField( "firstName", a => a.Person.FirstName )
                .AddField( "nickName", a => a.Person.NickName )
                .AddField( "middleName", a => a.Person.MiddleName )
                .AddField( "lastName", a => a.Person.LastName )
                .AddField( "suffixValue", a => a.SuffixValue )
                .AddField( "photoId", a => a.Person.PhotoId )
                .AddField( "birthDay", a => a.Person.BirthDay )
                .AddField( "birthMonth", a => a.Person.BirthMonth )
                .AddField( "birthYear", a => a.Person.BirthYear )
                .AddField( "age", a => a.Person.Age )
                .AddField( "gender", a => a.Person.Gender.ToString() )
                .AddField( "martialStatusValue", a => a.MaritalStatusValue )
                .AddField( "anniversaryDate", a => a.Person.AnniversaryDate )
                .AddField( "graduationYear", a => a.Person.GraduationYear )
                .AddField( "givingId", a => a.Person.GivingId )
                .AddField( "givingLeaderId", a => a.Person.GivingLeaderId )
                .AddField( "email", a => a.Person.Email )
                .AddField( "isEmailActive", a => a.Person.IsEmailActive )
                .AddField( "emailNote", a => a.Person.EmailNote )
                .AddField( "emailPreference", a => a.Person.EmailPreference.ToString() )
                .AddField( "communicationPreference", a => a.Person.CommunicationPreference.ToString() )
                .AddField( "reviewReasonNote", a => a.Person.ReviewReasonNote )
                .AddField( "inactiveReasonNote", a => a.Person.InactiveReasonNote )
                .AddField( "systemNote", a => a.Person.SystemNote )
                .AddField( "viewedCount", a => a.Person.ViewedCount )
                .AddField( "topSignalColor", a => a.Person.TopSignalColor )
                .AddField( "topSignalIconCssClass", a => a .Person.TopSignalIconCssClass )
                .AddField( "topSignalId", a => a.Person.TopSignalId )
                .AddField( "ageClassification", a => a.Person.AgeClassification.ToString() )
                .AddField( "primaryFamilyId", a => a.Person.PrimaryFamilyId )
                .AddField( "primaryCampusId", a => a.Person.PrimaryCampusId )
                .AddField( "isLockedAsChild", a => a.Person.IsLockedAsChild )
                .AddField( "deceasedDate", a => a.Person.DeceasedDate )
                .AddField( "contributionFinancialAccountId", a => a.Person.ContributionFinancialAccountId )
                .AddField( "accountProtectionProfile", a => a.Person.AccountProtectionProfile.ToString() )
                .AddField( "preferredLanguageValueId", a => a.PreferredLanguageValue )
                .AddField( "reminderCount", a => a.Person.ReminderCount )
                .AddField( "raceValueId", a => a.RaceValue )
                .AddField( "ethnicityValueId", a => a.EthnicityValue )
                .AddField( "birthDateKey", a => a.Person.BirthDateKey )
                .AddField( "ageBracket", a => a.Person.AgeBracket.ToString() )
                .AddField( "firstNamePronounciationOverride", a => a.Person.FirstNamePronunciationOverride )
                .AddField( "nickNamePronounciationOverride", a => a.Person.NickNamePronunciationOverride )
                .AddField( "lastNamePronounciationOverride", a => a.Person.LastNamePronunciationOverride )
                .AddField( "pronounciationNote", a => a.Person.PronunciationNote )
                .AddField( "primaryAliasId", a => a.PrimaryAliasId )
                .AddField( "daysUntilBirthday", a => a.Person.DaysUntilBirthday )
                .AddField( "givingGroupId", a => a.Person.GivingGroupId )
                .AddDateTimeField( "birthDate", a => a.Person.BirthDate )
                .AddField( "daysUntilAnniversary", a => a.Person.DaysUntilBirthday )
                .AddField( "allowsInteractiveBulkIndexing", a => a.Person.AllowsInteractiveBulkIndexing.ToTrueFalse() )
                .AddDateTimeField( "createdDateTime", a => a.Person.CreatedDateTime )
                .AddDateTimeField( "modifiedDateTime", a => a.Person.ModifiedDateTime )
                .AddField( "createdByPersonAliasId", a => a.Person.CreatedByPersonAliasId )
                .AddField( "modifiedByPersonAliasId", a => a.Person.ModifiedByPersonAliasId )
                .AddField( "foreignId", a => a.Person.ForeignId )
                .AddField( "foreignGuid", a => a.Person.ForeignGuid )
                .AddField( "foreignKey", a => a.Person.ForeignKey )
                .AddTextField( "phoneNumber", a => a.PhoneNumbersFormatted.FirstOrDefault() )
                .AddTextField( "personLabel", a => a.PhoneNumbersFormatted.Any() ? $"{a.PhoneNumbersFormatted.FirstOrDefault()} (Unknown Person)" : "Unknown Person" );
        }

        #endregion

        #region Helper Classes

        public class NamelessPersonRow
        {
            public Person Person { get; set; }

            public string RecordTypeValue { get; set; }

            public string RecordStatusValue { get; set; }

            public string RecordStatusReasonValue { get; set; }

            public string ConnectionStatusValue { get; set; }

            public string ReviewReasonValue { get; set; }

            public string TitleValue { get; set; }

            public string SuffixValue { get; set; }

            public string MaritalStatusValue { get; set; }

            public string PreferredLanguageValue { get; set; }

            public string RaceValue { get; set; }

            public string EthnicityValue { get; set; }

            public int? PrimaryAliasId { get; set; }

            public IEnumerable<string> PhoneNumbersFormatted { get; set; }
        }

        #endregion
    }
}
