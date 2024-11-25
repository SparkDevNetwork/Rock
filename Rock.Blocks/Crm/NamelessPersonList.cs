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

using DotLiquid;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.NamelessPersonList;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Rest.Controls;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Crm
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Nameless Person List" )]
    [Category( "CRM" )]
    [Description( "List unmatched phone numbers with an option to link to a person that has the same phone number." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.BlockTypeGuid( "6e9672e6-ee42-4aac-b0a9-b041c3b8368c" )]
    [CustomizedGrid]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "911EA779-AC00-4A93-B706-B6A642C727CB" )]
    public class NamelessPersonList : RockListBlockType<Person>
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
            person.LastName = personBag.LastName;
            person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
            person.RecordStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
            person.ConnectionStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT.AsGuid() ).Id;

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
            if ( personBag.PersonBirthDate != null )
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
        protected override IQueryable<Person> GetListQueryable( RockContext rockContext )
        {
            var namelessPersonRecordTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() )?.Id;
            var currentMergeRequestQry = PersonService.GetMergeRequestQuery( rockContext );
            var personService = new PersonService( rockContext );

            var qry = personService
                .Queryable( new PersonService.PersonQueryOptions() { IncludeNameless = true } )
                .Where( p => p.RecordTypeValueId == namelessPersonRecordTypeId )
                .Where( p => !currentMergeRequestQry.Any( mr => mr.Items.Any( i => i.EntityId == p.Id ) ) )
                .AsNoTracking();

            return qry;
        }

        /// <inheritdoc/>
        protected override GridBuilder<Person> GetGridBuilder()
        {
            return new GridBuilder<Person>()
                .WithBlock( this )
                .AddPersonField( "Person", a => a )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "guid", a => a.Guid.ToString() )
                .AddField( "id", a => a.Id )
                .AddField( "isSystem", a => a.IsSystem.ToTrueFalse() )
                .AddField( "recordType", a => a.RecordTypeValue?.Value )
                .AddField( "recordStatus", a => a.RecordStatusValue?.Value )
                .AddField( "recordStatusLastModifiedDateTime", a => a.RecordStatusLastModifiedDateTime )
                .AddField( "recordStatusReasonValue", a => a.RecordStatusReasonValue?.Value )
                .AddField( "connectionStatusValue", a => a.ConnectionStatusValue?.Value )
                .AddField( "reviewReasonValue", a => a.ReviewReasonValue?.Value )
                .AddField( "isDeceased", a => a.IsDeceased )
                .AddField( "titleValue", a => a.TitleValue?.Value )
                .AddField( "firstName", a => a.FirstName )
                .AddField( "nickName", a => a.NickName )
                .AddField( "middleName", a => a.MiddleName )
                .AddField( "lastName", a => a.LastName )
                .AddField( "suffixValue", a => a.SuffixValue?.Value )
                .AddField( "photoId", a => a.PhotoId )
                .AddField( "birthDay", a => a.BirthDay )
                .AddField( "birthMonth", a => a.BirthMonth )
                .AddField( "birthYear", a => a.BirthYear )
                .AddField( "age", a => a.Age )
                .AddField( "gender", a => a.Gender.ToString() )
                .AddField( "martialStatusValue", a => a.MaritalStatusValue?.Value )
                .AddField( "anniversaryDate", a => a.AnniversaryDate )
                .AddField( "graduationYear", a => a.GraduationYear )
                .AddField( "givingId", a => a.GivingId )
                .AddField( "givingLeaderId", a => a.GivingLeaderId )
                .AddField( "email", a => a.Email )
                .AddField( "isEmailActive", a => a.IsEmailActive )
                .AddField( "emailNote", a => a.EmailNote )
                .AddField( "emailPreference", a => a.EmailPreference.ToString() )
                .AddField( "communicationPreference", a => a.CommunicationPreference.ToString() )
                .AddField( "reviewReasonNote", a => a.ReviewReasonNote )
                .AddField( "inactiveReasonNote", a => a.InactiveReasonNote )
                .AddField( "systemNote", a => a.SystemNote )
                .AddField( "viewedCount", a => a.ViewedCount )
                .AddField( "topSignalColor", a => a.TopSignalColor )
                .AddField( "topSignalIconCssClass", a => a.TopSignalIconCssClass )
                .AddField( "topSignalId", a => a.TopSignalId )
                .AddField( "ageClassification", a => a.AgeClassification.ToString() )
                .AddField( "primaryFamilyId", a => a.PrimaryFamilyId )
                .AddField( "primaryCampusId", a => a.PrimaryCampusId )
                .AddField( "isLockedAsChild", a => a.IsLockedAsChild )
                .AddField( "deceasedDate", a => a.DeceasedDate )
                .AddField( "contributionFinancialAccountId", a => a.ContributionFinancialAccountId )
                .AddField( "accountProtectionProfile", a => a.AccountProtectionProfile.ToString() )
                .AddField( "preferredLanguageValueId", a => a.PreferredLanguageValue?.Value )
                .AddField( "reminderCount", a => a.ReminderCount )
                .AddField( "raceValueId", a => a.RaceValue?.Value )
                .AddField( "ethnicityValueId", a => a.EthnicityValue?.Value )
                .AddField( "birthDateKey", a => a.BirthDateKey )
                .AddField( "ageBracket", a => a.AgeBracket.ToString() )
                .AddField( "firstNamePronounciationOverride", a => a.FirstNamePronunciationOverride )
                .AddField( "nickNamePronounciationOverride", a => a.NickNamePronunciationOverride )
                .AddField( "lastNamePronounciationOverride", a => a.LastNamePronunciationOverride )
                .AddField( "pronounciationNote", a => a.PronunciationNote )
                .AddField( "primaryAliasId", a => a.PrimaryAliasId )
                .AddField( "daysUntilBirthday", a => a.DaysUntilBirthday )
                .AddField( "givingGroupId", a => a.GivingGroupId )
                .AddDateTimeField( "birthDate", a => a.BirthDate )
                .AddField( "daysUntilAnniversary", a => a.DaysUntilBirthday )
                .AddField( "allowsInteractiveBulkIndexing", a => a.AllowsInteractiveBulkIndexing.ToTrueFalse() )
                .AddDateTimeField( "createdDateTime", a => a.CreatedDateTime )
                .AddDateTimeField( "modifiedDateTime", a => a.ModifiedDateTime )
                .AddField( "createdByPersonAliasId", a => a.CreatedByPersonAliasId )
                .AddField( "modifiedByPersonAliasId", a => a.ModifiedByPersonAliasId )
                .AddField( "foreignId", a => a.ForeignId )
                .AddField( "foreignGuid", a => a.ForeignGuid )
                .AddField( "foreignKey", a => a.ForeignKey )
                .AddTextField( "phoneNumber", a => a.PhoneNumbers.Select( pn => pn.NumberFormatted ).FirstOrDefault() )
                .AddTextField( "personLabel", a => a.PhoneNumbers.Any() ? $"{a.PhoneNumbers.Select( pn => pn.NumberFormatted ).FirstOrDefault()} (Unknown Person)" : "Unknown Person" );
        }

        #endregion
    }
}
