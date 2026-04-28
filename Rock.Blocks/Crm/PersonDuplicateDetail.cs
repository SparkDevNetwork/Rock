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
        #region Keys

        private static class AttributeKey
        {
            public const string ConfidenceScoreHigh = "ConfidenceScoreHigh";
            public const string ConfidenceScoreLow = "ConfidenceScoreLow";
            public const string IncludeInactive = "IncludeInactive";
            public const string IncludeBusinesses = "IncludeBusinesses";
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

            options.ConfidenceScoreHigh = GetAttributeValue( AttributeKey.ConfidenceScoreHigh ).AsDouble();
            options.ConfidenceScoreLow = GetAttributeValue( AttributeKey.ConfidenceScoreLow ).AsDouble();
            options.IncludeInactive = GetAttributeValue( AttributeKey.IncludeInactive ).AsBoolean();
            options.IncludeBusinesses = GetAttributeValue( AttributeKey.IncludeBusinesses ).AsBoolean();
            options.HasMultipleCampuses = CampusCache.All().Count( c => c.IsActive ?? true ) > 1;

            return options;
        }

        #endregion Initialization Methods

        #region Grid Data Methods

        /// <inheritdoc/>
        protected override IQueryable<PersonDuplicateWrapper> GetListQueryable( RockContext rockContext )
        {
            var recordStatusInactiveId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;
            var recordTypeBusinessId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;

            // The WebForms block pulled addresses from all families (Person.GetFamilies()),
            // not just the primary one, so that duplicate candidates who share a secondary
            // family still show their matching addresses.
            var familyGroupTypeId = GroupTypeCache.GetFamilyGroupType().Id;

            var personDuplicateService = new PersonDuplicateService( rockContext );

            var personId = RequestContext.PageParameterAsId( PageParameterKey.PersonId );
            if ( personId == 0 )
            {
                return Enumerable.Empty<PersonDuplicateWrapper>().AsQueryable();
            }

            //// Take duplicates that aren't confirmed as NotDuplicate and aren't IgnoreUntilScoreChanges.
            //// Exclude rows where both aliases resolve to the same Person.
            var query = personDuplicateService.Queryable()
                .Where( pd => pd.PersonAlias.PersonId == personId
                    && pd.PersonAlias.PersonId != pd.DuplicatePersonAlias.PersonId
                    && !pd.IsConfirmedAsNotDuplicate
                    && !pd.IgnoreUntilScoreChanges );

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
                        DuplicatePersonGroupLocations = pd.DuplicatePersonAlias.Person.Members
                            .Where( gm => gm.Group.GroupTypeId == familyGroupTypeId )
                            .SelectMany( gm => gm.Group.GroupLocations )
                            .OrderByDescending( gl => gl.IsMappedLocation )
                            .ThenBy( gl => gl.Id )
                            .Select( gl => new GroupLocationProjection
                            {
                                GroupLocationTypeValue = gl.GroupLocationTypeValue != null ? gl.GroupLocationTypeValue.Value : string.Empty,
                                Location = gl.Location
                            } )
                            .ToList(),
                        DuplicatePersonPhoneNumbers = pd.DuplicatePersonAlias.Person.PhoneNumbers
                            .Select( pn => new PhoneNumberDto
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
            return query
                .OrderByDescending( x => x.Projection.ConfidenceScore )
                .ThenBy( x => x.Projection.DuplicatePerson.PersonLastName )
                .ThenBy( x => x.Projection.DuplicatePerson.PersonNickName );
        }

        /// <inheritdoc/>
        protected override List<PersonDuplicateWrapper> GetListItems( IQueryable<PersonDuplicateWrapper> queryable, RockContext rockContext )
        {
            var wrappers = queryable.ToList();

            foreach ( var wrapper in wrappers )
            {
                var projection = wrapper.Projection;
                wrapper.Dto = new PersonDuplicateDetailDto
                {
                    DuplicateRecordIdKey = projection.DuplicateRecordIdKey,
                    PersonIdKey = projection.DuplicatePerson.PersonIdKey,
                    IsDuplicateRow = true,
                    ConfidenceScore = projection.ConfidenceScore ?? 0.0,
                    Campus = projection.DuplicatePerson.PersonCampus,
                    AccountProtectionProfile = projection.DuplicatePerson.PersonAccountProtectionProfile,
                    RecordSource = GetRecordSourceValue( projection.DuplicatePerson.PersonRecordSourceValueId ),
                    FullName = GetPersonFullName( projection.DuplicatePerson ),
                    Email = projection.DuplicatePerson.PersonEmail,
                    Gender = projection.DuplicatePerson.PersonGender == 0 ? "" : ( ( Gender ) projection.DuplicatePerson.PersonGender ).ToStringSafe(),
                    Age = projection.DuplicatePerson.PersonAge,
                    Addresses = projection.DuplicatePersonGroupLocations.Select( gl => new AddressDto
                    {
                        GroupLocationTypeValue = gl.GroupLocationTypeValue ?? string.Empty,
                        FormattedHtmlAddress = gl.Location?.FormattedHtmlAddress ?? string.Empty
                    } ).ToList(),
                    PhoneNumbers = projection.DuplicatePersonPhoneNumbers.ToList()
                };
            }

            // Build the primary person row independently so that it always renders —
            // even when the target person has zero potential duplicates.
            var assembledList = new List<PersonDuplicateWrapper>();
            var primaryRow = BuildPrimaryPersonRow( rockContext );
            if ( primaryRow != null )
            {
                assembledList.Add( primaryRow );
            }
            assembledList.AddRange( wrappers );

            return assembledList;
        }

        /// <summary>
        /// Builds a synthesized wrapper representing the target person (the person
        /// whose duplicates are being viewed). This row is pinned at the top of the
        /// grid and is always included in selection so merge/communication actions
        /// operate against the target person.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <returns>The wrapper for the primary person, or <c>null</c> if the person cannot be resolved.</returns>
        private PersonDuplicateWrapper BuildPrimaryPersonRow( RockContext rockContext )
        {
            var personId = RequestContext.PageParameterAsId( PageParameterKey.PersonId );
            if ( personId == 0 )
            {
                return null;
            }

            // Mirror the duplicate-query behavior — addresses come from every family
            // the person belongs to, ordered so the mapped location surfaces first.
            var familyGroupTypeId = GroupTypeCache.GetFamilyGroupType().Id;

            var primaryProjection = new PersonService( rockContext ).Queryable()
                .Where( p => p.Id == personId )
                .Select( p => new
                {
                    Person = new PersonProjection
                    {
                        PersonId = p.Id,
                        PersonCampus = p.PrimaryCampus.Name,
                        PersonAccountProtectionProfile = ( int ) p.AccountProtectionProfile,
                        PersonRecordSourceValueId = p.RecordSourceValueId,
                        PersonNickName = p.NickName,
                        PersonLastName = p.LastName,
                        PersonSuffixValueId = p.SuffixValueId,
                        PersonRecordTypeValueId = p.RecordTypeValueId,
                        PersonEmail = p.Email,
                        PersonGender = ( int ) p.Gender,
                        PersonAge = p.Age,
                    },
                    GroupLocations = p.Members
                        .Where( gm => gm.Group.GroupTypeId == familyGroupTypeId )
                        .SelectMany( gm => gm.Group.GroupLocations )
                        .OrderByDescending( gl => gl.IsMappedLocation )
                        .ThenBy( gl => gl.Id )
                        .Select( gl => new GroupLocationProjection
                        {
                            GroupLocationTypeValue = gl.GroupLocationTypeValue != null ? gl.GroupLocationTypeValue.Value : string.Empty,
                            Location = gl.Location
                        } ).ToList(),
                    PhoneNumbers = p.PhoneNumbers.Select( pn => new PhoneNumberDto
                    {
                        PhoneNumberTypeValue = pn.NumberTypeValue != null ? pn.NumberTypeValue.Value : string.Empty,
                        PhoneNumber = pn.NumberFormatted != null ? pn.NumberFormatted : string.Empty
                    } ).ToList()
                } )
                .FirstOrDefault();

            if ( primaryProjection == null )
            {
                return null;
            }

            var person = primaryProjection.Person;
            return new PersonDuplicateWrapper
            {
                Dto = new PersonDuplicateDetailDto
                {
                    DuplicateRecordIdKey = "PRIMARY_PERSON",
                    PersonIdKey = person.PersonIdKey,
                    IsDuplicateRow = false,
                    ConfidenceScore = 0d,
                    Campus = person.PersonCampus,
                    AccountProtectionProfile = person.PersonAccountProtectionProfile,
                    RecordSource = GetRecordSourceValue( person.PersonRecordSourceValueId ),
                    FullName = GetPersonFullName( person ),
                    Email = person.PersonEmail,
                    Gender = person.PersonGender == 0 ? "" : ( ( Gender ) person.PersonGender ).ToStringSafe(),
                    Age = person.PersonAge,
                    Addresses = primaryProjection.GroupLocations.Select( gl => new AddressDto
                    {
                        GroupLocationTypeValue = gl.GroupLocationTypeValue ?? string.Empty,
                        FormattedHtmlAddress = gl.Location?.FormattedHtmlAddress ?? string.Empty
                    } ).ToList(),
                    PhoneNumbers = primaryProjection.PhoneNumbers.ToList()
                }
            };
        }

        /// <inheritdoc/>
        protected override GridBuilder<PersonDuplicateWrapper> GetGridBuilder()
        {
            return new GridBuilder<PersonDuplicateWrapper>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.Dto.DuplicateRecordIdKey )
                .AddTextField( "personIdKey", a => a.Dto.PersonIdKey )
                .AddField( "isDuplicateRow", a => a.Dto.IsDuplicateRow )
                .AddField( "confidenceScore", a => a.Dto.ConfidenceScore )
                .AddTextField( "campus", a => a.Dto.Campus )
                .AddField( "accountProtectionProfile", a => a.Dto.AccountProtectionProfile )
                .AddTextField( "recordSource", a => a.Dto.RecordSource )
                .AddTextField( "fullName", a => a.Dto.FullName )
                .AddTextField( "email", a => a.Dto.Email )
                .AddTextField( "gender", a => a.Dto.Gender )
                .AddField( "age", a => a.Dto.Age )
                .AddField( "addresses", a => a.Dto.Addresses )
                .AddField( "phoneNumbers", a => a.Dto.PhoneNumbers );
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
            var personDuplicateService = new PersonDuplicateService( RockContext );
            var personDuplicate = personDuplicateService.Get( personDuplicateIdKey, !PageCache.Layout.Site.DisablePredictableIds );
            if ( personDuplicate == null )
            {
                return ActionNotFound();
            }

            personDuplicate.IsConfirmedAsNotDuplicate = true;
            RockContext.SaveChanges();

            return ActionOk();
        }

        [BlockAction]
        public virtual BlockActionResult MarkIgnoreDuplicate( string personDuplicateIdKey )
        {
            var personDuplicateService = new PersonDuplicateService( RockContext );
            var personDuplicate = personDuplicateService.Get( personDuplicateIdKey, !PageCache.Layout.Site.DisablePredictableIds );
            if ( personDuplicate == null )
            {
                return ActionNotFound();
            }

            personDuplicate.IgnoreUntilScoreChanges = true;
            RockContext.SaveChanges();

            return ActionOk();
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
        public PersonDuplicateProjection Projection { get; set; } = new PersonDuplicateProjection();

        /// <summary>
        /// Gets or sets the data transfer object (DTO) containing formatted details for the person duplicate.
        /// </summary>
        public PersonDuplicateDetailDto Dto { get; set; } = new PersonDuplicateDetailDto();
    }

    /// <summary>
    /// Represents a projection of a potential duplicate person record, carrying the
    /// confidence score and the detail fields needed to render the duplicate side of
    /// a row in the grid.
    /// </summary>
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

        /// <summary>
        /// Gets or sets the duplicate person details.
        /// </summary>
        public PersonProjection DuplicatePerson { get; set; }

        /// <summary>
        /// Gets or sets the collection of group locations associated with the duplicate person.
        /// </summary>
        public ICollection<GroupLocationProjection> DuplicatePersonGroupLocations { get; set; }

        /// <summary>
        /// Gets or sets the collection of phone numbers associated with the duplicate person.
        /// </summary>
        public ICollection<PhoneNumberDto> DuplicatePersonPhoneNumbers { get; set; }
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
    /// Represents a projection of a group location, carrying the full
    /// <see cref="Rock.Model.Location"/> entity so callers can access the
    /// locale-aware <see cref="Rock.Model.Location.FormattedHtmlAddress"/>
    /// (which honors the country-specific AddressFormat Lava template and any
    /// Location attribute values the template references).
    /// </summary>
    public class GroupLocationProjection
    {
        /// <summary>
        /// Gets or sets the defined value representing the type of group location.
        /// </summary>
        public string GroupLocationTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the materialized Location entity for this group location.
        /// </summary>
        public Rock.Model.Location Location { get; set; }
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
        /// Gets or sets the age of the person.
        /// </summary>
        public int? Age { get; set; }
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
        /// Gets or sets the defined value representing the type of group location (e.g. Home, Work).
        /// </summary>
        public string GroupLocationTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the locale-aware HTML-formatted address. Rendered to the
        /// client as-is via <c>v-html</c>; sourced server-side from
        /// <see cref="Rock.Model.Location.FormattedHtmlAddress"/>.
        /// </summary>
        public string FormattedHtmlAddress { get; set; }
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
