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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.PersonDuplicateDetail;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Displays a list of person duplicates.
    /// </summary>

    [DisplayName( "Person Duplicate Detail" )]
    [Category( "CRM" )]
    [Description( "Shows records that are possible duplicates of the selected person." )]
    [IconCssClass( "ti ti-users" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [DecimalField(
        "Confidence Score High",
        Key = AttributeKey.ConfidenceScoreHigh,
        Description = "The minimum confidence score required to be considered a likely match.",
        IsRequired = true,
        DefaultDecimalValue = 80.00,
        Order = 0 )]

    [DecimalField(
        "Confidence Score Low",
        Key = AttributeKey.ConfidenceScoreLow,
        Description = "The maximum confidence score required to be considered an unlikely match. Values lower than this will not be shown in the grid.",
        IsRequired = true,
        DefaultDecimalValue = 40.00,
        Order = 1 )]

    [BooleanField(
        "Include Inactive",
        Key = AttributeKey.IncludeInactive,
        Description = "Set to true to also include potential matches when both records are inactive.",
        DefaultBooleanValue = false,
        Order = 2 )]

    [BooleanField(
        "Include Businesses",
        Key = AttributeKey.IncludeBusinesses,
        Description = "Set to true to also include potential matches when either record is a Business.",
        DefaultBooleanValue = false,
        Order = 3 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "B96C02DC-F624-4953-BED3-F7BA52CE854D" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "AAA53F35-1891-4236-B9CB-37805B9134DF" )]
    [Rock.SystemGuid.BlockTypeGuid( "A65CF2F8-93A4-4AC6-9018-D7C6996D9017" )]
    [CustomizedGrid]
    public class PersonDuplicateDetail : RockListBlockType<PersonDuplicateWrapper>
    {
        #region Fields

        // PersonIds, MatchCount
        Dictionary<int, int> _matchCounts = new Dictionary<int, int>();

        #endregion Fields

        #region Keys

        private static class AttributeKey
        {
            public const string ConfidenceScoreHigh = "ConfidenceScoreHigh";
            public const string ConfidenceScoreLow = "ConfidenceScoreLow";
            public const string IncludeInactive = "IncludeInactive";
            public const string IncludeBusinesses = "IncludeBusinesses";
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
        }

        #endregion Keys

        #region Methods

        #region Initialization Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PersonDuplicateDetailOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private PersonDuplicateDetailOptionsBag GetBoxOptions()
        {
            var options = new PersonDuplicateDetailOptionsBag();

            options.ConfidenceScoreHigh = GetAttributeValue( AttributeKey.ConfidenceScoreHigh ).AsInteger();
            options.ConfidenceScoreLow = GetAttributeValue( AttributeKey.ConfidenceScoreLow ).AsInteger();
            options.IncludeInactive = GetAttributeValue( AttributeKey.IncludeInactive ).AsBoolean();
            options.IncludeBusinesses = GetAttributeValue( AttributeKey.IncludeBusinesses ).AsBoolean();
            options.HasMultipleCampuses = CampusCache.All().Count( c => c.IsActive ?? true ) > 1;

            return options;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "PersonId", "((Key))" )
            };
        }

        #endregion Initialization Methods

        #region Grid Data Methods

        /// <inheritdoc/>
        protected override IQueryable<PersonDuplicateWrapper> GetListQueryable( RockContext rockContext )
        {
            var recordStatusInactiveId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;
            var recordTypeBusinessId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;

            var personDuplicateService = new PersonDuplicateService( RockContext );

            var pageParameterPersonId = RequestContext.GetPageParameter( PageParameterKey.PersonId );
            var isPersonIdHashed = !int.TryParse( pageParameterPersonId, out int personId );
            if ( isPersonIdHashed )
            {
                var unhashedKey = Rock.Utility.IdHasher.Instance.GetId( pageParameterPersonId );

                if ( !unhashedKey.HasValue )
                {
                    return Enumerable.Empty<PersonDuplicateWrapper>().AsQueryable();
                }
                personId = unhashedKey.Value;
            }

            //// Take duplicates that aren't confirmed as NotDuplicate and aren't IgnoreUntilScoreChanges.
            var query = personDuplicateService.Queryable()
                .Where( pd => pd.PersonAlias.PersonId == personId && !pd.IsConfirmedAsNotDuplicate && !pd.IgnoreUntilScoreChanges );

            // Don't include records where both the Person and Duplicate are inactive
            if ( this.GetAttributeValue( AttributeKey.IncludeInactive ).AsBoolean() == false )
            {
                query = query.Where( pd => !(
                    pd.PersonAlias.Person.RecordStatusValueId == recordStatusInactiveId
                    && pd.DuplicatePersonAlias.Person.RecordStatusValueId == recordStatusInactiveId
                ) );
            }

            // Don't include records where either the Person or Duplicate is a Business
            if ( this.GetAttributeValue( AttributeKey.IncludeBusinesses ).AsBoolean() == false )
            {
                query = query.Where( pd => !(
                    pd.PersonAlias.Person.RecordTypeValueId == recordTypeBusinessId
                    || pd.DuplicatePersonAlias.Person.RecordTypeValueId == recordTypeBusinessId
                ) );
            }

            // Don't include records that don't meet the minimum confidence score
            double? confidenceScoreLow = GetAttributeValue( AttributeKey.ConfidenceScoreLow ).AsDoubleOrNull();
            if ( confidenceScoreLow.HasValue )
            {
                query = query.Where( pd => pd.ConfidenceScore >= confidenceScoreLow );
            }

            var containeredQuery = query.Select( pd =>
                new PersonDuplicateWrapper
                {
                    Projection = new PersonDuplicateProjection
                    {
                        DuplicateRecordId = pd.Id,
                        ConfidenceScore = pd.ConfidenceScore,
                        Person = new PersonProjection
                        {
                            PersonId = pd.PersonAlias.Person.Id,
                            PersonCampus = pd.PersonAlias.Person.PrimaryCampus.Name,
                            PersonAccountProtectionProfile = ( int ) pd.PersonAlias.Person.AccountProtectionProfile,
                            PersonRecordSourceValueId = pd.PersonAlias.Person.RecordSourceValueId,
                            PersonNickName = pd.PersonAlias.Person.NickName,
                            PersonLastName = pd.PersonAlias.Person.LastName,
                            PersonSuffixValueId = pd.PersonAlias.Person.SuffixValueId,
                            PersonRecordTypeValueId = pd.PersonAlias.Person.RecordTypeValueId,
                            PersonEmail = pd.PersonAlias.Person.Email,
                            PersonGender = ( int ) pd.PersonAlias.Person.Gender,
                            PersonAge = pd.PersonAlias.Person.Age,
                        },
                        DuplicatePerson = new PersonProjection
                        {
                            PersonId = pd.DuplicatePersonAlias.Person.Id,
                            PersonCampus = pd.DuplicatePersonAlias.Person.PrimaryCampus.Name,
                            PersonAccountProtectionProfile = ( int ) pd.DuplicatePersonAlias.Person.AccountProtectionProfile,
                            PersonRecordSourceValueId = pd.DuplicatePersonAlias.Person.RecordSourceValueId,
                            PersonNickName = pd.DuplicatePersonAlias.Person.NickName,
                            PersonLastName = pd.DuplicatePersonAlias.Person.LastName,
                            PersonSuffixValueId = pd.DuplicatePersonAlias.Person.SuffixValueId,
                            PersonRecordTypeValueId = pd.DuplicatePersonAlias.Person.RecordTypeValueId,
                            PersonEmail = pd.DuplicatePersonAlias.Person.Email,
                            PersonGender = ( int ) pd.DuplicatePersonAlias.Person.Gender,
                            PersonAge = pd.DuplicatePersonAlias.Person.Age,
                        },
                        PersonGroupLocations = pd.PersonAlias.Person.PrimaryFamily.GroupLocations
                            .Select( gl => new GroupLocationProjection
                            {
                                GroupLocationTypeValue = gl.GroupLocationTypeValue != null ? gl.GroupLocationTypeValue.Value : string.Empty,
                                Street1 = gl.Location != null ? gl.Location.Street1 : string.Empty,
                                Street2 = gl.Location != null ? gl.Location.Street2 : string.Empty,
                                City = gl.Location != null ? gl.Location.City : string.Empty,
                                State = gl.Location != null ? gl.Location.State : string.Empty,
                                PostalCode = gl.Location != null ? gl.Location.PostalCode : string.Empty
                            } )
                            .ToList(),
                        DuplicatePersonGroupLocations = pd.DuplicatePersonAlias.Person.PrimaryFamily.GroupLocations
                            .Select( gl => new GroupLocationProjection
                            {
                                GroupLocationTypeValue = gl.GroupLocationTypeValue != null ? gl.GroupLocationTypeValue.Value : string.Empty,
                                Street1 = gl.Location != null ? gl.Location.Street1 : string.Empty,
                                Street2 = gl.Location != null ? gl.Location.Street2 : string.Empty,
                                City = gl.Location != null ? gl.Location.City : string.Empty,
                                State = gl.Location != null ? gl.Location.State : string.Empty,
                                PostalCode = gl.Location != null ? gl.Location.PostalCode : string.Empty
                            } )
                            .ToList(),
                        PersonPhoneNumbers = pd.PersonAlias.Person.PhoneNumbers
                            .Select( pn => new PhoneNumberProjection
                            {
                                PhoneNumberTypeValue = pn.NumberTypeValue != null ? pn.NumberTypeValue.Value : string.Empty,
                                PhoneNumber = pn.NumberFormatted != null ? pn.NumberFormatted : string.Empty
                            } )
                            .ToList(),
                        DuplicatePersonPhoneNumbers = pd.DuplicatePersonAlias.Person.PhoneNumbers
                            .Select( pn => new PhoneNumberProjection
                            {
                                PhoneNumberTypeValue = pn.NumberTypeValue != null ? pn.NumberTypeValue.Value : string.Empty,
                                PhoneNumber = pn.NumberFormatted != null ? pn.NumberFormatted : string.Empty
                            } )
                            .ToList(),
                    }
                }
            );

            return containeredQuery;
        }

        /// <inheritdoc/>
        protected override IQueryable<PersonDuplicateWrapper> GetOrderedListQueryable( IQueryable<PersonDuplicateWrapper> query, RockContext rockContext )
        {
            if ( !query.Any() )
            {
                return query;
            }

            return query
                   .OrderBy( x => x.Projection.ConfidenceScore );
        }

        /// <inheritdoc/>
        protected override List<PersonDuplicateWrapper> GetListItems( IQueryable<PersonDuplicateWrapper> queryable, RockContext rockContext )
        {
            var wrappers = queryable.ToList();

            if ( !wrappers.Any() )
            {
                return wrappers;
            }

            PersonDuplicateProjection projection;
            foreach ( var wrapper in wrappers )
            {
                var dto = new PersonDuplicateDetailDto();
                projection = wrapper.Projection;

                // Replace lines 320-330 in GetListItems with the following:

                dto.DuplicateRecordIdKey = projection.DuplicateRecordIdKey;
                dto.PersonIdKey = projection.DuplicatePerson.PersonIdKey;
                dto.IsDuplicateRow = true;
                dto.ConfidenceScore = projection.ConfidenceScore ?? 0.0;
                dto.Campus = projection.DuplicatePerson.PersonCampus;
                dto.AccountProtectionProfile = projection.DuplicatePerson.PersonAccountProtectionProfile;
                dto.RecordSource = GetRecordSourceValue( projection.DuplicatePerson.PersonRecordSourceValueId );
                dto.FullName = GetPersonFullName( projection.DuplicatePerson );
                dto.Email = projection.DuplicatePerson.PersonEmail;
                dto.Gender = projection.DuplicatePerson.PersonGender == 0 ? "" : ( ( Gender ) projection.DuplicatePerson.PersonGender ).ToStringSafe();
                dto.Age = projection.DuplicatePerson.PersonAge.ToStringSafe();

                foreach ( var groupLocation in projection.DuplicatePersonGroupLocations )
                {
                    var addressDto = new AddressDto
                    {
                        GroupLocationTypeValue = groupLocation.GroupLocationTypeValue != null ? groupLocation.GroupLocationTypeValue : string.Empty,
                        Street1 = groupLocation.Street1,
                        Street2 = groupLocation.Street2,
                        City = groupLocation.City,
                        State = groupLocation.State,
                        PostalCode = groupLocation.PostalCode
                    };
                    if ( dto.Addresses == null )
                    {
                        dto.Addresses = new List<AddressDto>();
                    }
                    dto.Addresses.Add( addressDto );
                }

                foreach ( var phoneNumber in projection.DuplicatePersonPhoneNumbers )
                {
                    var phoneDto = new PhoneNumberDto
                    {
                        PhoneNumberTypeValue = phoneNumber.PhoneNumberTypeValue != null ? phoneNumber.PhoneNumberTypeValue : string.Empty,
                        PhoneNumber = phoneNumber.PhoneNumber
                    };
                    if ( dto.PhoneNumbers == null )
                    {
                        dto.PhoneNumbers = new List<PhoneNumberDto>();
                    }
                    dto.PhoneNumbers.Add( phoneDto );
                }

                wrapper.DTO = dto;
            }

            var personRecord = new PersonDuplicateWrapper();
            projection = wrappers.First().Projection;
            personRecord.DTO = new PersonDuplicateDetailDto
            {
                DuplicateRecordIdKey = projection.DuplicateRecordIdKey,
                PersonIdKey = projection.Person.PersonIdKey,
                IsDuplicateRow = false,
                ConfidenceScore = 0d,
                Campus = projection.Person.PersonCampus,
                AccountProtectionProfile = projection.Person.PersonAccountProtectionProfile,
                RecordSource = GetRecordSourceValue( projection.Person.PersonRecordSourceValueId ),
                FullName = GetPersonFullName( projection.Person ),
                Email = projection.Person.PersonEmail,
                Gender = projection.Person.PersonGender == 0 ? "" : ( ( Gender ) projection.Person.PersonGender ).ToStringSafe(),
                Age = projection.Person.PersonAge.ToStringSafe(),
                Addresses = projection.PersonGroupLocations.Select( gl => new AddressDto
                {
                    GroupLocationTypeValue = gl.GroupLocationTypeValue != null ? gl.GroupLocationTypeValue : string.Empty,
                    Street1 = gl.Street1,
                    Street2 = gl.Street2,
                    City = gl.City,
                    State = gl.State,
                    PostalCode = gl.PostalCode
                } ).ToList(),
                PhoneNumbers = projection.PersonPhoneNumbers.Select( pn => new PhoneNumberDto
                {
                    PhoneNumberTypeValue = pn.PhoneNumberTypeValue != null ? pn.PhoneNumberTypeValue : string.Empty,
                    PhoneNumber = pn.PhoneNumber
                } ).ToList()
            };

            var assembledList = new List<PersonDuplicateWrapper> { personRecord };
            assembledList.AddRange( wrappers.OrderByDescending( w => w.DTO.ConfidenceScore ) );

            return assembledList;
        }

        /// <inheritdoc/>
        protected override GridBuilder<PersonDuplicateWrapper> GetGridBuilder()
        {
            return new GridBuilder<PersonDuplicateWrapper>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.DTO.DuplicateRecordIdKey )
                .AddTextField( "personIdKey", a => a.DTO.PersonIdKey )
                .AddField( "isDuplicateRow", a => a.DTO.IsDuplicateRow )
                .AddField( "confidenceScore", a => a.DTO.ConfidenceScore )
                .AddTextField( "campus", a => a.DTO.Campus )
                .AddField( "accountProtectionProfile", a => a.DTO.AccountProtectionProfile )
                .AddTextField( "recordSource", a => a.DTO.RecordSource )
                .AddTextField( "fullName", a => a.DTO.FullName )
                .AddTextField( "email", a => a.DTO.Email )
                .AddTextField( "gender", a => a.DTO.Gender )
                .AddTextField( "age", a => a.DTO.Age )
                .AddField( "addresses", a => a.DTO.Addresses )
                .AddField( "phoneNumbers", a => a.DTO.PhoneNumbers );
        }

        /// <summary>
        /// Gets the record source <see cref="DefinedValueCache.Value" /> for the provided identifier.
        /// </summary>
        /// <param name="recordSourceValueId">The identifier of the record source value to get.</param>
        /// <returns>The record source value or an empty string if no matching <see cref="DefinedValueCache"/> was found.</returns>
        private string GetRecordSourceValue( int? recordSourceValueId )
        {
            return recordSourceValueId.HasValue
                ? DefinedValueCache.Get( recordSourceValueId.Value )?.Value
                : string.Empty;
        }

        /// <summary>
        /// Gets the full name of the person from the provided projection.
        /// </summary>
        /// <param name="projection">The person projection.</param>
        /// <returns>The full name of the person.</returns>
        private string GetPersonFullName( PersonProjection projection )
        {
            return Person.FormatFullName(
                projection.PersonNickName,
                projection.PersonLastName,
                projection.PersonSuffixValueId,
                projection.PersonRecordTypeValueId
            );
        }

        #endregion Grid Data Methods

        #endregion Methods

        #region Block Actions

        [BlockAction]
        public virtual BlockActionResult MarkNotDuplicate( string personDuplicateIdKey )
        {
            if ( BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
              || BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                var personDuplicateService = new PersonDuplicateService( RockContext );
                var personDuplicate = personDuplicateService.Get( personDuplicateIdKey, true );
                personDuplicate.IsConfirmedAsNotDuplicate = true;
                if ( RockContext.SaveChanges() > 0 )
                {
                    return ActionOk();
                }
                else
                {
                    return ActionBadRequest( $"Failed to mark IsConfirmedAsNotDuplicate for PersonDuplicate with idKey of '{personDuplicateIdKey}'" );
                }
            }

            var requesterName = $"{RequestContext.CurrentPerson.NickName} {RequestContext.CurrentPerson.LastName}";
            return ActionForbidden( $"{requesterName} does not have block edit permission, " +
                                     "which is needed to mark duplicate as not a duplicate." );
        }

        [BlockAction]
        public virtual BlockActionResult MarkIgnoreDuplicate( string personDuplicateIdKey )
        {
            if ( BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
              || BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                var personDuplicateService = new PersonDuplicateService( RockContext );
                var personDuplicate = personDuplicateService.Get( personDuplicateIdKey, true );
                personDuplicate.IgnoreUntilScoreChanges = true;
                if ( RockContext.SaveChanges() > 0 )
                {
                    return ActionOk();
                }
                else
                {
                    return ActionBadRequest( $"Failed to mark IgnoreUntilScoreChanges for PersonDuplicate with idKey of '{personDuplicateIdKey}'" );
                }
            }

            var requesterName = $"{RequestContext.CurrentPerson.NickName} {RequestContext.CurrentPerson.LastName}";
            return ActionForbidden( $"{requesterName} does not have block edit permission, which is needed to ignore duplicates." );
        }

        #endregion Block Actions
    }

    #region Helper Classes

    /// <summary>
    /// Encapsulates a projection and a data transfer object (DTO) for handling person duplicate information.
    /// </summary>
    /// <remarks>
    /// This class provides a convenient wrapper for managing both the projection and the DTO related
    /// to person duplicates. It is intended to be used in scenarios where both representations are needed
    /// together, such as when overriding several methods with a unified expected type.
    /// </remarks>
    public class PersonDuplicateWrapper
    {
        /// <summary>
        /// Gets or sets the projection containing detailed information about the person duplicate.
        /// </summary>
        public PersonDuplicateProjection Projection = new PersonDuplicateProjection();

        /// <summary>
        /// Gets or sets the data transfer object (DTO) containing formatted details for the person duplicate.
        /// </summary>
        public PersonDuplicateDetailDto DTO = new PersonDuplicateDetailDto();
    }

    /// <summary>
    /// Represents a projection of a potential duplicate person record, including details about the original and
    /// duplicate person.
    /// </summary>
    /// <remarks>This class is used to store and manage information about a person and their potential
    /// duplicate, including identifiers, personal details, and contact information. It provides properties for both the
    /// original person and the duplicate, allowing for comparison and analysis of potential duplicates.</remarks>
    public class PersonDuplicateProjection
    {
        /// <summary>
        /// Gets or sets the unique identifier for the duplicate record.
        /// </summary>
        public int DuplicateRecordId { get; set; }

        /// <summary>
        /// Gets the hashed key for the duplicate record identifier.
        /// </summary>
        public string DuplicateRecordIdKey => Rock.Utility.IdHasher.Instance.GetHash( DuplicateRecordId );

        /// <summary>
        /// Gets or sets the confidence score indicating the likelihood of a duplicate match.
        /// </summary>
        public double? ConfidenceScore { get; set; }

        public PersonProjection Person { get; set; }

        public PersonProjection DuplicatePerson { get; set; }

        /// <summary>
        /// Gets or sets the collection of group locations associated with the original person.
        /// </summary>
        public ICollection<GroupLocationProjection> PersonGroupLocations { get; set; }

        /// <summary>
        /// Gets or sets the collection of group locations associated with the duplicate person.
        /// </summary>
        public ICollection<GroupLocationProjection> DuplicatePersonGroupLocations { get; set; }

        /// <summary>
        /// Gets or sets the collection of phone numbers associated with the original person.
        /// </summary>
        public ICollection<PhoneNumberProjection> PersonPhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the collection of phone numbers associated with the duplicate person.
        /// </summary>
        public ICollection<PhoneNumberProjection> DuplicatePersonPhoneNumbers { get; set; }
    }

    /// <summary>
    /// Represents a projection of a person with various attributes such as identifier, name, and contact information.
    /// </summary>
    /// <remarks>This class provides a way to access and manipulate key information about a person, including
    /// their unique identifier, campus affiliation, and personal details like name and email. It also includes a hashed
    /// key for secure identification.</remarks>
    public class PersonProjection
    {

        /// <summary>
        /// Gets or sets the unique identifier for the person.
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets the hashed key for the person identifier.
        /// </summary>
        public string PersonIdKey => Rock.Utility.IdHasher.Instance.GetHash( PersonId );

        /// <summary>
        /// Gets or sets the campus name for the person.
        /// </summary>
        public string PersonCampus { get; set; }

        /// <summary>
        /// Gets or sets the account protection profile for the person.
        /// </summary>
        public int PersonAccountProtectionProfile { get; set; }

        /// <summary>
        /// Gets or sets the record source defined value identifier for the person.
        /// </summary>
        public int? PersonRecordSourceValueId { get; set; }

        /// <summary>
        /// Gets or sets the nick name of the person.
        /// </summary>
        public string PersonNickName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the person.
        /// </summary>
        public string PersonLastName { get; set; }

        /// <summary>
        /// Gets or sets the suffix defined value identifier for the person.
        /// </summary>
        public int? PersonSuffixValueId { get; set; }

        /// <summary>
        /// Gets or sets the record type defined value identifier for the person.
        /// </summary>
        public int? PersonRecordTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the email address of the person.
        /// </summary>
        public string PersonEmail { get; set; }

        /// <summary>
        /// Gets or sets the gender of the person.
        /// </summary>
        public int PersonGender { get; set; }

        /// <summary>
        /// Gets or sets the age of the person.
        /// </summary>
        public int? PersonAge { get; set; }
    }

    /// <summary>
    /// Represents a projection of a group location, including address details and location type.
    /// </summary>
    /// <remarks>This class provides properties to store and retrieve information about a group's location,
    /// such as the type of location, street address, city, state, and postal code. It is useful for applications that
    /// need to manage or display group location data.</remarks>
    public class GroupLocationProjection
    {
        /// <summary>
        /// Gets or sets the defined value representing the type of group location.
        /// </summary>
        public string GroupLocationTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the first line of the street address.
        /// </summary>
        public string Street1 { get; set; }

        /// <summary>
        /// Gets or sets the second line of the street address.
        /// </summary>
        public string Street2 { get; set; }

        /// <summary>
        /// Gets or sets the city of the address.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state or province of the address.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the postal code of the address.
        /// </summary>
        public string PostalCode { get; set; }
    }

    /// <summary>
    /// Represents a projection of a phone number with its associated type.
    /// </summary>
    /// <remarks>This class is used to encapsulate a phone number and its type, allowing for easy manipulation
    /// and storage of phone number data in applications.</remarks>
    public class PhoneNumberProjection
    {
        /// <summary>
        /// Gets or sets the defined value representing the type of phone number.
        /// </summary>
        public string PhoneNumberTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the phone number in string format.
        /// </summary>
        public string PhoneNumber { get; set; }
    }

    /// <summary>
    /// Represents detailed information about a potential duplicate person record.
    /// </summary>
    /// <remarks>This data transfer object is used to convey information about a person that may be a
    /// duplicate in a system, including identifiers, personal details, and associated contact information.</remarks>
    public class PersonDuplicateDetailDto
    {
        /// <summary>
        /// Gets or sets the hashed key for the duplicate record identifier.
        /// </summary>
        public string DuplicateRecordIdKey { get; set; }
        /// <summary>
        /// Gets or sets the hashed key for the person identifier.
        /// </summary>
        public string PersonIdKey { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this row represents a duplicate person.
        /// </summary>
        public bool IsDuplicateRow { get; set; }
        /// <summary>
        /// Gets or sets the confidence score indicating the likelihood of a duplicate match.
        /// </summary>
        public double ConfidenceScore { get; set; }
        /// <summary>
        /// Gets or sets the campus name associated with the person.
        /// </summary>
        public string Campus { get; set; }
        /// <summary>
        /// Gets or sets the account protection profile for the person.
        /// </summary>
        public int AccountProtectionProfile { get; set; }
        /// <summary>
        /// Gets or sets the record source for the person.
        /// </summary>
        public string RecordSource { get; set; }
        /// <summary>
        /// Gets or sets the full name of the person.
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// Gets or sets the email address of the person.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Gets or sets the gender of the person.
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// Gets or sets the age of the person as a string.
        /// </summary>
        public string Age { get; set; }
        /// <summary>
        /// Gets or sets the list of addresses associated with the person.
        /// </summary>
        public List<AddressDto> Addresses { get; set; }
        /// <summary>
        /// Gets or sets the list of phone numbers associated with the person.
        /// </summary>
        public List<PhoneNumberDto> PhoneNumbers { get; set; }
    }

    /// <summary>
    /// Represents a data transfer object for an address, containing details such as street, city, state, and postal
    /// code.
    /// </summary>
    /// <remarks>This class is used to encapsulate address information in a structured format, suitable for
    /// data transfer operations.</remarks>
    public class AddressDto
    {
        /// <summary>
        /// Gets or sets the defined value representing the type of number.
        /// </summary>
        public string GroupLocationTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the phone number in string format.
        /// </summary>
        public string Street1 { get; set; }

        /// <summary>
        /// Gets or sets the phone number in string format.
        /// </summary>
        public string Street2 { get; set; }

        /// <summary>
        /// Gets or sets the phone number in string format.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the phone number in string format.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the phone number in string format.
        /// </summary>
        public string PostalCode { get; set; }
    }

    /// <summary>
    /// Represents a data transfer object for a phone number, including its type.
    /// </summary>
    /// <remarks>This class is used to encapsulate phone number information in a structured format, suitable for
    /// data transfer operations.</remarks>
    public class PhoneNumberDto
    {
        /// <summary>
        /// Gets or sets the defined value representing the type of number.
        /// </summary>
        public string PhoneNumberTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the phone number in string format.
        /// </summary>
        public string PhoneNumber { get; set; }
    }

    #endregion Helper Classes
}
