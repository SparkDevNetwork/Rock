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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.PersonDirectory;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// A directory of people in the database.
    /// </summary>
    [DisplayName( "Person Directory" )]
    [Category( "CRM" )]
    [Description( "A directory of people in database." )]
    [IconCssClass( "ti ti-users" )]

    #region Block Attributes

    [DataViewField(
        "Data View",
        Key = AttributeKey.DataView,
        Description = "The data view to use as the source for the directory. Only those people returned by the data view filter will be displayed on this directory.",
        IsRequired = true,
        DefaultValue = "cb4bb264-a1f4-4edb-908f-2ccf3a534bc7",
        EntityTypeName = "Rock.Model.Person",
        Order = 0 )]

    [GroupField(
        "Opt-out Group",
        Key = AttributeKey.OptOut,
        Description = "A group that contains people that should be excluded from this list.",
        IsRequired = false,
        Order = 1 )]

    [CustomRadioListField(
        "Show By",
        Key = AttributeKey.ShowBy,
        Description = "People can be displayed individually, or grouped by family.",
        ListSource = "Individual,Family",
        IsRequired = true,
        DefaultValue = "Individual",
        Order = 2 )]

    [BooleanField(
        "Show All People",
        Key = AttributeKey.ShowAllPeople,
        Description = "Display all people by default? If false, a search is required first, and only those matching search criteria will be displayed.",
        DefaultBooleanValue = false,
        Order = 3 )]

    [LinkedPage(
        "Person Profile Page",
        Key = AttributeKey.PersonProfilePage,
        Description = "Page to navigate to when clicking a person's name (leave blank if link should not be enabled).",
        IsRequired = false,
        Order = 4 )]

    [IntegerField(
        "First Name Characters Required",
        Key = AttributeKey.FirstNameCharactersRequired,
        Description = "The number of characters that need to be entered before allowing a search.",
        IsRequired = false,
        DefaultIntegerValue = 1,
        Order = 5 )]

    [IntegerField(
        "Last Name Characters Required",
        Key = AttributeKey.LastNameCharactersRequired,
        Description = "The number of characters that need to be entered before allowing a search.",
        IsRequired = false,
        DefaultIntegerValue = 3,
        Order = 6 )]

    [BooleanField(
        "Show Email",
        Key = AttributeKey.ShowEmail,
        Description = "Should the email address be included in the directory?",
        DefaultBooleanValue = true,
        Order = 7 )]

    [BooleanField(
        "Show Address",
        Key = AttributeKey.ShowAddress,
        Description = "Should the address be included in the directory?",
        DefaultBooleanValue = true,
        Order = 8 )]

    [DefinedValueField(
        "Show Phones",
        Key = AttributeKey.ShowPhones,
        Description = "The phone numbers to be included in the directory.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE,
        IsRequired = false,
        AllowMultiple = true,
        Order = 9 )]

    [BooleanField(
        "Show Birthday",
        Key = AttributeKey.ShowBirthday,
        Description = "Should the birthday be included in the directory?",
        DefaultBooleanValue = true,
        Order = 10 )]

    [BooleanField(
        "Show Gender",
        Key = AttributeKey.ShowGender,
        Description = "Should the gender be included in the directory?",
        DefaultBooleanValue = true,
        Order = 11 )]

    [BooleanField(
        "Show Grade",
        Key = AttributeKey.ShowGrade,
        Description = "Should grade be included in the directory?",
        DefaultBooleanValue = false,
        Order = 12 )]

    [BooleanField(
        "Show Envelope Number",
        Key = AttributeKey.ShowEnvelopeNumber,
        Description = "Should envelope # be included in the directory?",
        DefaultBooleanValue = false,
        Order = 13 )]

    [IntegerField(
        "Max Results",
        Key = AttributeKey.MaxResults,
        Description = "The maximum number of results to show on the page.",
        IsRequired = true,
        DefaultIntegerValue = 1500,
        Order = 14 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "D3F7210D-BF89-4C30-A3C4-8F29793EC296" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "BE4112FF-C1F9-4B9D-99F2-469EFB84A4A6" )]
    [Rock.SystemGuid.BlockTypeGuid( "FAA234E0-9B34-4539-9987-F15E3318B4FF" )]
    public class PersonDirectory : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DataView = "DataView";
            public const string OptOut = "OptOut";
            public const string ShowBy = "ShowBy";
            public const string ShowAllPeople = "ShowAllPeople";
            public const string FirstNameCharactersRequired = "FirstNameCharactersRequired";
            public const string LastNameCharactersRequired = "LastNameCharactersRequired";
            public const string ShowEmail = "ShowEmail";
            public const string ShowAddress = "ShowAddress";
            public const string ShowBirthday = "ShowBirthday";
            public const string ShowGender = "ShowGender";
            public const string ShowGrade = "ShowGrade";
            public const string ShowEnvelopeNumber = "ShowEnvelopeNumber";
            public const string PersonProfilePage = "PersonProfilePage";
            public const string ShowPhones = "ShowPhones";
            public const string MaxResults = "MaxResults";
        }

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
        }

        private static class NavigationUrlKey
        {
            public const string PersonProfilePage = "PersonProfilePage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<PersonDirectoryBag, PersonDirectoryOptionsBag>();

            var dataView = GetDataView();
            var isOptOutEnabled = RequestContext.CurrentPerson != null && GetOptOutGroupGuid().HasValue;
            var isShowAllPeopleEnabled = GetAttributeValue( AttributeKey.ShowAllPeople ).AsBoolean();

            box.Options = new PersonDirectoryOptionsBag
            {
                IsShowByFamily = IsShowByFamily(),
                IsEmailShown = GetAttributeValue( AttributeKey.ShowEmail ).AsBoolean(),
                IsAddressShown = GetAttributeValue( AttributeKey.ShowAddress ).AsBoolean(),
                ArePhonesShown = GetPhoneCaptions().Any(),
                IsBirthdayShown = GetAttributeValue( AttributeKey.ShowBirthday ).AsBoolean(),
                IsGenderShown = GetAttributeValue( AttributeKey.ShowGender ).AsBoolean(),
                IsGradeShown = GetAttributeValue( AttributeKey.ShowGrade ).AsBoolean(),
                IsEnvelopeNumberShown = GetAttributeValue( AttributeKey.ShowEnvelopeNumber ).AsBoolean(),
                IsShowAllPeopleEnabled = isShowAllPeopleEnabled,
                IsOptOutEnabled = isOptOutEnabled,
                IsDataViewConfigured = dataView != null,
                FirstNameCharactersRequired = GetAttributeValue( AttributeKey.FirstNameCharactersRequired ).AsIntegerOrNull(),
                LastNameCharactersRequired = GetAttributeValue( AttributeKey.LastNameCharactersRequired ).AsIntegerOrNull()
            };

            box.NavigationUrls = new Dictionary<string, string>
            {
                [NavigationUrlKey.PersonProfilePage] = this.GetLinkedPageUrl( AttributeKey.PersonProfilePage, PageParameterKey.PersonId, "((Key))" )
            };

            // Only pre-load results when the block shows everyone by default; otherwise a search is required first.
            var initialResults = new PersonDirectoryResultsBag();
            string resultsErrorMessage = null;
            if ( isShowAllPeopleEnabled )
            {
                initialResults = BuildResults( null, null, dataView, out resultsErrorMessage );
            }

            box.Bag = new PersonDirectoryBag
            {
                Results = initialResults,
                IsCurrentPersonOptedOut = GetCurrentPersonOptedOut()
            };

            // Surface a data view evaluation failure as a block-level error so the block does not render a broken state.
            if ( !string.IsNullOrWhiteSpace( resultsErrorMessage ) )
            {
                box.ErrorMessage = resultsErrorMessage;
            }

            return box;
        }

        /// <summary>
        /// Builds the directory results for the given search terms, honoring the data view,
        /// opt-out exclusions, and the individual/family display mode.
        /// </summary>
        /// <param name="firstName">The first name search term, or null/empty for no first-name filter.</param>
        /// <param name="lastName">The last name search term, or null/empty for no last-name filter.</param>
        /// <param name="errorMessage">When this method returns, contains a user-facing message if the data view could not be evaluated; otherwise null.</param>
        /// <returns>The results to display. Empty when no data view is configured, the data view fails to evaluate, or a search is required but absent.</returns>
        private PersonDirectoryResultsBag BuildResults( string firstName, string lastName, out string errorMessage )
        {
            return BuildResults( firstName, lastName, GetDataView(), out errorMessage );
        }

        /// <summary>
        /// Builds the directory results for the given search terms using an already-resolved data view.
        /// </summary>
        /// <param name="firstName">The first name search term, or null/empty for no first-name filter.</param>
        /// <param name="lastName">The last name search term, or null/empty for no last-name filter.</param>
        /// <param name="dataView">The directory's data view, or null when none is configured.</param>
        /// <param name="errorMessage">When this method returns, contains a user-facing message if the data view could not be evaluated; otherwise null.</param>
        /// <returns>The results to display. Empty when no data view is configured, the data view fails to evaluate, or a search is required but absent.</returns>
        private PersonDirectoryResultsBag BuildResults( string firstName, string lastName, DataView dataView, out string errorMessage )
        {
            errorMessage = null;
            var results = new PersonDirectoryResultsBag();

            if ( dataView == null )
            {
                return results;
            }

            var personService = new PersonService( RockContext );

            if ( !TryGetDataViewPersonQuery( personService, dataView, out var personQry ) )
            {
                errorMessage = "The configured Data View could not be loaded. Please verify the block's Data View setting.";
                return results;
            }

            var isShowByFamily = IsShowByFamily();

            // Capture the full set of data view people before the name filters so family
            // mode can show every data-view member of a matched family. Only needed in family mode.
            var dataViewPersonIdQry = isShowByFamily ? personQry.Select( p => p.Id ) : null;

            var hasSearchFilter = false;

            firstName = firstName?.Trim();
            if ( !string.IsNullOrWhiteSpace( firstName ) )
            {
                personQry = personQry.Where( p => p.FirstName.StartsWith( firstName ) || p.NickName.StartsWith( firstName ) );
                hasSearchFilter = true;
            }

            lastName = lastName?.Trim();
            if ( !string.IsNullOrWhiteSpace( lastName ) )
            {
                personQry = personQry.Where( p => p.LastName.StartsWith( lastName ) );
                hasSearchFilter = true;
            }

            // Without a search and without "show all", nothing is displayed yet.
            if ( !hasSearchFilter && !GetAttributeValue( AttributeKey.ShowAllPeople ).AsBoolean() )
            {
                return results;
            }

            var optOutGroupGuid = GetOptOutGroupGuid();
            if ( optOutGroupGuid.HasValue )
            {
                var optOutPersonIdQry = new GroupMemberService( RockContext )
                    .Queryable()
                    .Where( m => m.Group.Guid == optOutGroupGuid.Value )
                    .Select( m => m.PersonId );

                personQry = personQry.Where( p => !optOutPersonIdQry.Contains( p.Id ) );
            }

            var visibility = GetPersonFieldVisibility();

            if ( isShowByFamily )
            {
                results.Families = BuildFamilies( personQry, dataViewPersonIdQry, visibility );
            }
            else
            {
                results.People = BuildPeople( personQry, visibility );
            }

            return results;
        }

        /// <summary>
        /// Attempts to build the data-view-filtered person query. The data view's filter expression
        /// can throw when the data view is misconfigured, so failures are caught and reported rather
        /// than surfaced as an unhandled exception.
        /// </summary>
        /// <param name="personService">The person service used to build the query.</param>
        /// <param name="dataView">The configured data view.</param>
        /// <param name="personQry">When this method returns, contains the filtered query, or null on failure.</param>
        /// <returns><c>true</c> when the query was built successfully; otherwise <c>false</c>.</returns>
        private bool TryGetDataViewPersonQuery( PersonService personService, DataView dataView, out IQueryable<Person> personQry )
        {
            personQry = null;

            try
            {
                var paramExpression = personService.ParameterExpression;
                var whereExpression = dataView.GetExpression( personService, paramExpression );

                personQry = personService
                    .Queryable( false, false )
                    .Where( paramExpression, whereExpression, null );

                return true;
            }
            catch ( Exception ex )
            {
                // A misconfigured data view filter can throw when its expression is built; log and report instead of failing the block.
                ExceptionLogService.LogException( ex );
                return false;
            }
        }

        /// <summary>
        /// Builds the people to display when results are listed as individuals.
        /// </summary>
        /// <param name="personQry">The filtered person query.</param>
        /// <param name="visibility">The resolved per-person field visibility.</param>
        /// <returns>The list of person bags.</returns>
        private List<PersonDirectoryPersonBag> BuildPeople( IQueryable<Person> personQry, PersonFieldVisibility visibility )
        {
            var maxResults = GetAttributeValue( AttributeKey.MaxResults ).AsInteger();

            var people = personQry
                .OrderBy( p => p.LastName )
                .ThenBy( p => p.NickName )
                .Take( maxResults )
                .Select( p => new PersonInfo
                {
                    Id = p.Id,
                    RecordTypeValueId = p.RecordTypeValueId,
                    AgeClassification = p.AgeClassification,
                    NickName = p.NickName,
                    LastName = p.LastName,
                    Email = p.Email,
                    BirthMonth = p.BirthMonth,
                    BirthDay = p.BirthDay,
                    BirthDate = p.BirthDate,
                    DeceasedDate = p.DeceasedDate,
                    Gender = p.Gender,
                    PhotoId = p.PhotoId,
                    GraduationYear = p.GraduationYear
                } )
                .ToList();

            var personIds = people.Select( p => p.Id ).ToList();

            var addressesByPersonId = visibility.IsAddressShown ? GetPersonAddresses( personIds ) : null;
            var phonesByPersonId = GetPhones( personIds );
            var envelopeNumbersByPersonId = GetEnvelopeNumbers( personIds );

            return people
                .Select( p => ToPersonBag(
                    p,
                    visibility,
                    GetValueOrDefault( addressesByPersonId, p.Id ),
                    GetValueOrDefault( phonesByPersonId, p.Id ),
                    GetValueOrDefault( envelopeNumbersByPersonId, p.Id ) ) )
                .ToList();
        }

        /// <summary>
        /// Builds the families to display when results are grouped by family.
        /// </summary>
        /// <param name="personQry">The filtered person query used to locate matching families.</param>
        /// <param name="dataViewPersonIdQry">The full set of data view person ids used to populate each family's members.</param>
        /// <param name="visibility">The resolved per-person field visibility.</param>
        /// <returns>The list of family bags.</returns>
        private List<PersonDirectoryFamilyBag> BuildFamilies( IQueryable<Person> personQry, IQueryable<int> dataViewPersonIdQry, PersonFieldVisibility visibility )
        {
            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            if ( familyGroupType == null )
            {
                return new List<PersonDirectoryFamilyBag>();
            }

            var maxResults = GetAttributeValue( AttributeKey.MaxResults ).AsInteger();
            var personIdQry = personQry.Select( p => p.Id );

            var familyMemberQry = new GroupMemberService( RockContext )
                .Queryable()
                .Where( m => m.Group.GroupTypeId == familyGroupType.Id && personIdQry.Contains( m.PersonId ) );

            // The distinct families containing a matched person, ordered by name and capped.
            var families = familyMemberQry
                .Select( m => new { m.GroupId, m.Group.Name } )
                .Distinct()
                .OrderBy( f => f.Name )
                .Take( maxResults )
                .ToList();

            var familyIds = families.Select( f => f.GroupId ).ToList();

            var membersByFamilyId = new GroupService( RockContext )
                .Queryable()
                .Where( g => familyIds.Contains( g.Id ) )
                .Select( g => new
                {
                    GroupId = g.Id,
                    Members = g.Members
                        .Where( m => dataViewPersonIdQry.Contains( m.PersonId ) )
                        .OrderBy( m => m.GroupRole.Order )
                        .ThenBy( m => m.Person.BirthDate )
                        .Select( m => m.Person )
                        .Select( p => new PersonInfo
                        {
                            Id = p.Id,
                            RecordTypeValueId = p.RecordTypeValueId,
                            AgeClassification = p.AgeClassification,
                            NickName = p.NickName,
                            LastName = p.LastName,
                            Email = p.Email,
                            BirthMonth = p.BirthMonth,
                            BirthDay = p.BirthDay,
                            BirthDate = p.BirthDate,
                            DeceasedDate = p.DeceasedDate,
                            Gender = p.Gender,
                            PhotoId = p.PhotoId,
                            GraduationYear = p.GraduationYear
                        } )
                        .ToList()
                } )
                .ToList()
                .ToDictionary( x => x.GroupId, x => x.Members );

            var memberPersonIds = membersByFamilyId.Values
                .SelectMany( m => m )
                .Select( p => p.Id )
                .Distinct()
                .ToList();

            var addressesByFamilyId = visibility.IsAddressShown ? GetFamilyAddresses( familyIds ) : null;
            var phonesByPersonId = GetPhones( memberPersonIds );
            var envelopeNumbersByPersonId = GetEnvelopeNumbers( memberPersonIds );

            return families
                .Select( f => new PersonDirectoryFamilyBag
                {
                    Name = f.Name,
                    FormattedHtmlAddress = GetValueOrDefault( addressesByFamilyId, f.GroupId ),
                    Members = ( membersByFamilyId.TryGetValue( f.GroupId, out var members ) ? members : new List<PersonInfo>() )
                        .Select( p => ToPersonBag(
                            p,
                            visibility,
                            // The address is shown once on the family, not on each member.
                            null,
                            GetValueOrDefault( phonesByPersonId, p.Id ),
                            GetValueOrDefault( envelopeNumbersByPersonId, p.Id ) ) )
                        .ToList()
                } )
                .ToList();
        }

        /// <summary>
        /// Maps a projected person to its display bag.
        /// </summary>
        /// <param name="person">The projected person.</param>
        /// <param name="visibility">The resolved per-person field visibility.</param>
        /// <param name="formattedAddress">The person's formatted address, or null when not shown.</param>
        /// <param name="phones">The person's phone numbers, or null when none.</param>
        /// <param name="envelopeNumber">The person's envelope number, or null when not shown.</param>
        /// <returns>The populated person bag.</returns>
        private static PersonDirectoryPersonBag ToPersonBag( PersonInfo person, PersonFieldVisibility visibility, string formattedAddress, List<PersonDirectoryPhoneBag> phones, string envelopeNumber )
        {
            var bag = new PersonDirectoryPersonBag
            {
                IdKey = IdHasher.Instance.GetHash( person.Id ),
                FullName = $"{person.NickName} {person.LastName}",
                PhotoUrl = person.PhotoUrl,
                Email = visibility.IsEmailShown ? person.Email : null,
                FormattedHtmlAddress = formattedAddress,
                PhoneNumbers = phones,
                EnvelopeNumber = !string.IsNullOrWhiteSpace( envelopeNumber ) ? envelopeNumber : null
            };

            if ( visibility.IsBirthdayShown && person.BirthMonth.HasValue && person.BirthDay.HasValue )
            {
                // A fixed leap year is used so the month/day always format and Feb 29 is valid.
                var birthday = new DateTime( 2000, person.BirthMonth.Value, person.BirthDay.Value );
                bag.BirthdayText = birthday.ToString( "MMM d" );
            }

            if ( visibility.IsGenderShown && person.Gender != Gender.Unknown )
            {
                bag.GenderText = person.Gender == Gender.Male ? "M" : "F";
            }

            if ( visibility.IsGradeShown && !string.IsNullOrWhiteSpace( person.Grade ) )
            {
                bag.Grade = person.Grade;
            }

            return bag;
        }

        /// <summary>
        /// Gets the formatted home addresses for the given people, keyed by person id. Used in individual mode.
        /// </summary>
        /// <param name="personIds">The person ids to load addresses for.</param>
        /// <returns>A dictionary of person id to formatted HTML address.</returns>
        private Dictionary<int, string> GetPersonAddresses( List<int> personIds )
        {
            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            var homeLocationValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() )?.Id;

            if ( familyGroupType == null || !homeLocationValueId.HasValue )
            {
                return new Dictionary<int, string>();
            }

            var locations = new GroupMemberService( RockContext )
                .Queryable().AsNoTracking()
                .Where( m => personIds.Contains( m.PersonId ) && m.Group.GroupTypeId == familyGroupType.Id )
                .SelectMany( m => m.Group.GroupLocations
                    .Where( gl => gl.GroupLocationTypeValueId == homeLocationValueId.Value )
                    .Select( gl => new { m.PersonId, gl.Location } ) )
                .ToList();

            return locations
                .GroupBy( x => x.PersonId )
                .ToDictionary(
                    g => g.Key,
                    g => string.Join( "<br/><br/>", g.Select( x => x.Location.FormattedHtmlAddress ) ) );
        }

        /// <summary>
        /// Gets the formatted home addresses for the given families, keyed by group id. Used in family mode.
        /// </summary>
        /// <param name="familyIds">The family group ids to load addresses for.</param>
        /// <returns>A dictionary of group id to formatted HTML address.</returns>
        private Dictionary<int, string> GetFamilyAddresses( List<int> familyIds )
        {
            var homeLocationValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() )?.Id;
            if ( !homeLocationValueId.HasValue )
            {
                return new Dictionary<int, string>();
            }

            var locations = new GroupService( RockContext )
                .Queryable().AsNoTracking()
                .Where( g => familyIds.Contains( g.Id ) )
                .SelectMany( g => g.GroupLocations
                    .Where( gl => gl.GroupLocationTypeValueId == homeLocationValueId.Value )
                    .Select( gl => new { GroupId = g.Id, gl.Location } ) )
                .ToList();

            return locations
                .GroupBy( x => x.GroupId )
                .ToDictionary(
                    g => g.Key,
                    g => string.Join( "<br/><br/>", g.Select( x => x.Location.FormattedHtmlAddress ) ) );
        }

        /// <summary>
        /// Gets the displayable phone numbers for the given people, keyed by person id. Returns an empty
        /// dictionary when no phone types are configured for display.
        /// </summary>
        /// <param name="personIds">The person ids to load phone numbers for.</param>
        /// <returns>A dictionary of person id to phone bags.</returns>
        private Dictionary<int, List<PersonDirectoryPhoneBag>> GetPhones( List<int> personIds )
        {
            var phoneCaptions = GetPhoneCaptions();
            if ( !phoneCaptions.Any() )
            {
                return new Dictionary<int, List<PersonDirectoryPhoneBag>>();
            }

            var phoneTypeValueIds = phoneCaptions.Keys.ToList();

            var phoneNumbers = new PhoneNumberService( RockContext )
                .Queryable().AsNoTracking()
                .Where( p => personIds.Contains( p.PersonId )
                    && !p.IsUnlisted
                    && p.NumberTypeValueId.HasValue
                    && phoneTypeValueIds.Contains( p.NumberTypeValueId.Value ) )
                .OrderBy( p => p.PersonId )
                .ThenBy( p => p.NumberTypeValue.Order )
                .ToList();

            return phoneNumbers
                .GroupBy( p => p.PersonId )
                .ToDictionary(
                    g => g.Key,
                    g => g.Select( p => new PersonDirectoryPhoneBag
                    {
                        NumberFormatted = p.NumberFormatted,
                        TypeName = phoneCaptions[p.NumberTypeValueId.Value]
                    } ).ToList() );
        }

        /// <summary>
        /// Gets the giving envelope numbers for the given people, keyed by person id. Returns an empty
        /// dictionary when envelope numbers are disabled by block setting or global attribute.
        /// </summary>
        /// <param name="personIds">The person ids to load envelope numbers for.</param>
        /// <returns>A dictionary of person id to envelope number.</returns>
        private Dictionary<int, string> GetEnvelopeNumbers( List<int> personIds )
        {
            var isEnvelopeNumberShown = GetAttributeValue( AttributeKey.ShowEnvelopeNumber ).AsBoolean();
            if ( !isEnvelopeNumberShown || !GlobalAttributesCache.Get().EnableGivingEnvelopeNumber )
            {
                return new Dictionary<int, string>();
            }

            var envelopeAttributeId = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_ENVELOPE_NUMBER.AsGuid() )?.Id;
            if ( !envelopeAttributeId.HasValue )
            {
                return new Dictionary<int, string>();
            }

            return new AttributeValueService( RockContext )
                .Queryable()
                .Where( a => a.AttributeId == envelopeAttributeId.Value && a.EntityId.HasValue && personIds.Contains( a.EntityId.Value ) )
                .Select( a => new { PersonId = a.EntityId.Value, a.Value } )
                .ToList()
                .ToDictionary( k => k.PersonId, v => v.Value );
        }

        /// <summary>
        /// Gets the phone type captions to display, keyed by defined value id.
        /// </summary>
        /// <returns>A dictionary of phone type defined value id to caption.</returns>
        private Dictionary<int, string> GetPhoneCaptions()
        {
            var captions = new Dictionary<int, string>();

            foreach ( var guid in GetAttributeValue( AttributeKey.ShowPhones ).SplitDelimitedValues().AsGuidList() )
            {
                var phoneType = DefinedValueCache.Get( guid );
                if ( phoneType != null )
                {
                    captions[phoneType.Id] = phoneType.Value;
                }
            }

            return captions;
        }

        /// <summary>
        /// Gets whether the current person is currently a member of the configured opt-out group.
        /// </summary>
        /// <returns><c>true</c> if the current person is opted out; otherwise <c>false</c>.</returns>
        private bool GetCurrentPersonOptedOut()
        {
            var currentPersonId = RequestContext.CurrentPerson?.Id;
            var optOutGroupGuid = GetOptOutGroupGuid();

            if ( !currentPersonId.HasValue || !optOutGroupGuid.HasValue )
            {
                return false;
            }

            return new GroupMemberService( RockContext )
                .Queryable()
                .Any( m => m.PersonId == currentPersonId.Value && m.Group.Guid == optOutGroupGuid.Value );
        }

        /// <summary>
        /// Gets the data view configured as the directory source, or null when not configured.
        /// </summary>
        /// <returns>The data view, or null.</returns>
        private DataView GetDataView()
        {
            var dataViewGuid = GetAttributeValue( AttributeKey.DataView ).AsGuidOrNull();
            if ( !dataViewGuid.HasValue )
            {
                return null;
            }

            return new DataViewService( RockContext ).Get( dataViewGuid.Value );
        }

        /// <summary>
        /// Gets the configured opt-out group guid, or null when not configured.
        /// </summary>
        /// <returns>The opt-out group guid, or null.</returns>
        private Guid? GetOptOutGroupGuid()
        {
            return GetAttributeValue( AttributeKey.OptOut ).AsGuidOrNull();
        }

        /// <summary>
        /// Gets whether the directory is grouped by family rather than listed individually.
        /// </summary>
        /// <returns><c>true</c> when grouped by family.</returns>
        private bool IsShowByFamily()
        {
            return GetAttributeValue( AttributeKey.ShowBy ) == "Family";
        }

        /// <summary>
        /// Resolves the per-person field visibility from block settings.
        /// </summary>
        /// <returns>The field visibility flags.</returns>
        private PersonFieldVisibility GetPersonFieldVisibility()
        {
            return new PersonFieldVisibility
            {
                IsEmailShown = GetAttributeValue( AttributeKey.ShowEmail ).AsBoolean(),
                IsAddressShown = GetAttributeValue( AttributeKey.ShowAddress ).AsBoolean(),
                IsBirthdayShown = GetAttributeValue( AttributeKey.ShowBirthday ).AsBoolean(),
                IsGenderShown = GetAttributeValue( AttributeKey.ShowGender ).AsBoolean(),
                IsGradeShown = GetAttributeValue( AttributeKey.ShowGrade ).AsBoolean()
            };
        }

        /// <summary>
        /// Gets the person ids the opt-out change applies to: the whole family in family mode,
        /// otherwise just the current person.
        /// </summary>
        /// <param name="currentPersonId">The current person's id.</param>
        /// <returns>The distinct list of person ids to toggle.</returns>
        private List<int> GetOptOutTargetPersonIds( int currentPersonId )
        {
            if ( !IsShowByFamily() )
            {
                return new List<int> { currentPersonId };
            }

            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            if ( familyGroupType == null )
            {
                return new List<int> { currentPersonId };
            }

            var groupMemberService = new GroupMemberService( RockContext );

            var familyIdQry = groupMemberService
                .Queryable()
                .Where( m => m.Group.GroupTypeId == familyGroupType.Id && m.PersonId == currentPersonId )
                .Select( m => m.GroupId );

            return groupMemberService
                .Queryable()
                .Where( m => familyIdQry.Contains( m.GroupId ) )
                .Select( m => m.PersonId )
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Returns the value for the given key, or default when the dictionary is null or the key is absent.
        /// </summary>
        private static TValue GetValueOrDefault<TValue>( Dictionary<int, TValue> dictionary, int key )
        {
            if ( dictionary != null && dictionary.TryGetValue( key, out var value ) )
            {
                return value;
            }

            return default;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the directory results for the given search criteria.
        /// </summary>
        /// <param name="search">The search criteria.</param>
        /// <returns>The directory results.</returns>
        [BlockAction]
        public BlockActionResult GetResults( PersonDirectorySearchBag search )
        {
            var results = BuildResults( search?.FirstName, search?.LastName, out var errorMessage );

            if ( !string.IsNullOrWhiteSpace( errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk( results );
        }

        /// <summary>
        /// Toggles the current person's directory opt-out state. In family mode the change is
        /// applied to every member of the current person's family.
        /// </summary>
        /// <returns>The new opt-out state.</returns>
        [BlockAction]
        public BlockActionResult ToggleOptOut()
        {
            var currentPersonId = RequestContext.CurrentPerson?.Id;
            var optOutGroupGuid = GetOptOutGroupGuid();

            if ( !currentPersonId.HasValue || !optOutGroupGuid.HasValue )
            {
                return ActionBadRequest( "You must be signed in to change your directory preference." );
            }

            var optOutGroup = new GroupService( RockContext ).Get( optOutGroupGuid.Value );
            if ( optOutGroup == null )
            {
                return ActionBadRequest( "The opt-out group is not configured correctly." );
            }

            var groupMemberService = new GroupMemberService( RockContext );

            // The toggle direction follows the current person's current state.
            var isCurrentlyOptedOut = groupMemberService.Queryable()
                .Any( m => m.GroupId == optOutGroup.Id && m.PersonId == currentPersonId.Value );
            var shouldOptOut = !isCurrentlyOptedOut;

            var personIds = GetOptOutTargetPersonIds( currentPersonId.Value );

            if ( shouldOptOut )
            {
                var defaultGroupRoleId = GroupTypeCache.Get( optOutGroup.GroupTypeId )?.DefaultGroupRoleId ?? 0;

                var existingMemberPersonIds = groupMemberService.Queryable()
                    .Where( m => m.GroupId == optOutGroup.Id && personIds.Contains( m.PersonId ) )
                    .Select( m => m.PersonId )
                    .ToList();

                foreach ( var personId in personIds.Except( existingMemberPersonIds ) )
                {
                    groupMemberService.Add( new GroupMember
                    {
                        GroupId = optOutGroup.Id,
                        PersonId = personId,
                        GroupRoleId = defaultGroupRoleId
                    } );
                }
            }
            else
            {
                var membersToRemove = groupMemberService.Queryable()
                    .Where( m => m.GroupId == optOutGroup.Id && personIds.Contains( m.PersonId ) )
                    .ToList();

                groupMemberService.DeleteRange( membersToRemove );
            }

            RockContext.SaveChanges();

            return ActionOk( new { IsOptedOut = shouldOptOut } );
        }

        #endregion Block Actions

        #region Support Classes

        /// <summary>
        /// The per-person field visibility resolved from block settings.
        /// </summary>
        private sealed class PersonFieldVisibility
        {
            public bool IsEmailShown { get; set; }

            public bool IsAddressShown { get; set; }

            public bool IsBirthdayShown { get; set; }

            public bool IsGenderShown { get; set; }

            public bool IsGradeShown { get; set; }
        }

        /// <summary>
        /// The person fields projected from the database, with the computed display values
        /// (photo URL, grade, initials) evaluated in memory.
        /// </summary>
        public class PersonInfo
        {
            public int Id { get; set; }

            public int? RecordTypeValueId { get; set; }

            public AgeClassification AgeClassification { get; set; }

            public string NickName { get; set; }

            public string LastName { get; set; }

            public string Email { get; set; }

            public int? BirthMonth { get; set; }

            public int? BirthDay { get; set; }

            public DateTime? BirthDate { get; set; }

            public DateTime? DeceasedDate { get; set; }

            public Gender Gender { get; set; }

            public int? GraduationYear { get; set; }

            public int? PhotoId { get; set; }

            public int? Age => Person.GetAge( BirthDate, DeceasedDate );

            public string Grade => Person.GradeFormattedFromGraduationYear( GraduationYear );

            public string Initials => $"{( NickName ?? string.Empty ).Truncate( 1, false )}{( LastName ?? string.Empty ).Truncate( 1, false )}";

            public string PhotoUrl => Person.GetPersonPhotoUrl( Initials, PhotoId, Age, Gender, RecordTypeValueId, AgeClassification );
        }

        #endregion Support Classes
    }
}
