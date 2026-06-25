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
using System.Linq;
using System.Text;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.PersonSearch;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Displays a list of people that match a given search type and term.
    /// </summary>
    [DisplayName( "Person Search" )]
    [Category( "CRM" )]
    [Description( "Displays list of people that match a given search type and term." )]
    [IconCssClass( "ti ti-users" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Person Detail Page",
        Key = AttributeKey.PersonDetailPage,
        Order = 0 )]

    [DefinedValueField(
        "Phone Number Types",
        Key = AttributeKey.PhoneNumberTypes,
        Description = "Types of phone numbers to include with person detail.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE,
        IsRequired = false,
        AllowMultiple = true,
        Order = 1 )]

    [BooleanField(
        "Show Birthdate",
        Key = AttributeKey.ShowBirthdate,
        Description = "Should a birthdate column be displayed?",
        DefaultBooleanValue = false,
        Order = 2 )]

    [BooleanField(
        "Show Age",
        Key = AttributeKey.ShowAge,
        Description = "Should an age column be displayed?",
        DefaultBooleanValue = true,
        Order = 3 )]

    [BooleanField(
        "Show Gender",
        Key = AttributeKey.ShowGender,
        Description = "Should a gender column be displayed?",
        DefaultBooleanValue = false,
        Order = 4 )]

    [BooleanField(
        "Show Spouse",
        Key = AttributeKey.ShowSpouse,
        Description = "Should a spouse column be displayed?",
        DefaultBooleanValue = false,
        Order = 5 )]

    [BooleanField(
        "Show Envelope Number",
        Key = AttributeKey.ShowEnvelopeNumber,
        Description = "Should an envelope # column be displayed?",
        DefaultBooleanValue = false,
        Order = 6 )]

    [BooleanField(
        "Show Performance",
        Key = AttributeKey.ShowPerformance,
        Description = "Displays how long the search took.",
        DefaultBooleanValue = false,
        Order = 7 )]

    [DataViewsField(
        "Highlight Indicators",
        Key = AttributeKey.DataViewIcons,
        Description = "Select one or more Data Views for Person search result icons. Note: More selections increase processing time.",
        EntityTypeName = "Rock.Model.Person",
        DisplayPersistedOnly = true,
        IsRequired = false,
        Order = 8 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "205B98E9-5FDF-41CC-9A36-94B1E251271D" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "1FB17B14-3398-4D1A-8687-FBA0B2C9A738" )]
    [Rock.SystemGuid.BlockTypeGuid( "764D3E67-2D01-437A-9F45-9F8C97878434" )]
    [CustomizedGrid]
    public class PersonSearch : RockListBlockType<PersonSearch.PersonSearchResult>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string PersonDetailPage = "PersonDetailPage";
            public const string PhoneNumberTypes = "PhoneNumberTypes";
            public const string ShowBirthdate = "ShowBirthdate";
            public const string ShowAge = "ShowAge";
            public const string ShowGender = "ShowGender";
            public const string ShowSpouse = "ShowSpouse";
            public const string ShowEnvelopeNumber = "ShowEnvelopeNumber";
            public const string ShowPerformance = "ShowPerformance";
            public const string DataViewIcons = "DataViewIcons";
        }

        private static class NavigationUrlKey
        {
            public const string PersonDetailPage = "PersonDetailPage";
        }

        private static class PageParameterKey
        {
            public const string SearchType = "SearchType";
            public const string SearchTerm = "SearchTerm";
            public const string AllowFirstNameOnly = "AllowFirstNameOnly";

            /*
                The birthdate search is reached from external links (e.g. the birthday calendar) that
                supply these lowercase/hyphenated query-string keys. They are an existing contract, so
                they are read verbatim rather than renamed to the usual PascalCase convention.
            */
            public const string Birthdate = "birthdate";
            public const string BirthdatePersonId = "person-id";
        }

        private static class SearchTypeValue
        {
            public const string Name = "name";
            public const string Phone = "phone";
            public const string Address = "address";
            public const string Email = "email";
            public const string Birthdate = "birthdate";
        }

        #endregion Keys

        #region Fields

        private List<Guid> _phoneTypeGuids;
        private List<Guid> _dataViewGuids;
        private int? _inactiveRecordStatusValueId;
        private bool _isInactiveStatusResolved;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the configured phone number type Guids whose numbers are shown in the person cell.
        /// </summary>
        private List<Guid> PhoneTypeGuids
        {
            get
            {
                if ( _phoneTypeGuids == null )
                {
                    _phoneTypeGuids = GetAttributeValue( AttributeKey.PhoneNumberTypes ).SplitDelimitedValues().AsGuidList();
                }

                return _phoneTypeGuids;
            }
        }

        /// <summary>
        /// Gets the configured data view Guids used to render highlight indicator icons.
        /// </summary>
        private List<Guid> DataViewGuids
        {
            get
            {
                if ( _dataViewGuids == null )
                {
                    _dataViewGuids = GetAttributeValue( AttributeKey.DataViewIcons ).SplitDelimitedValues().AsGuidList();
                }

                return _dataViewGuids;
            }
        }

        /// <summary>
        /// Gets the Id of the inactive person record status, or <c>null</c> if it cannot be resolved.
        /// </summary>
        private int? InactiveRecordStatusValueId
        {
            get
            {
                if ( !_isInactiveStatusResolved )
                {
                    _inactiveRecordStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() )?.Id;
                    _isInactiveStatusResolved = true;
                }

                return _inactiveRecordStatusValueId;
            }
        }

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PersonSearchOptionsBag>();
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
        private PersonSearchOptionsBag GetBoxOptions()
        {
            var options = new PersonSearchOptionsBag
            {
                IsBirthdateColumnVisible = GetAttributeValue( AttributeKey.ShowBirthdate ).AsBoolean(),
                IsAgeColumnVisible = GetAttributeValue( AttributeKey.ShowAge ).AsBoolean(),
                IsGenderColumnVisible = GetAttributeValue( AttributeKey.ShowGender ).AsBoolean(),
                IsSpouseColumnVisible = GetAttributeValue( AttributeKey.ShowSpouse ).AsBoolean(),
                IsEnvelopeNumberColumnVisible = GetIsEnvelopeNumberColumnVisible(),
                IsPerformanceShown = GetAttributeValue( AttributeKey.ShowPerformance ).AsBoolean()
            };

            var personIdQuery = GetMatchingPersonIdQueryable( RockContext );
            if ( personIdQuery == null )
            {
                return options;
            }

            var isNameSearch = string.Equals( PageParameter( PageParameterKey.SearchType )?.Trim(), SearchTypeValue.Name, StringComparison.OrdinalIgnoreCase );

            if ( isNameSearch )
            {
                // The full set of result Ids is needed to exclude them from the "Other Possible Matches" suggestions.
                var resultPersonIds = personIdQuery.Distinct().ToList();

                if ( resultPersonIds.Count == 1 )
                {
                    // A single match redirects straight to the person profile, so the "Other Possible
                    // Matches" suggestions would never be seen. Skip building them to avoid the extra query.
                    options.RedirectUrl = GetPersonProfileUrl( resultPersonIds[0] );
                }
                else
                {
                    options.AlternateMatches = GetAlternateMatches( resultPersonIds );
                }
            }
            else
            {
                // Only the single-result check is needed, so avoid materializing the entire id set.
                var topResultIds = personIdQuery.Distinct().Take( 2 ).ToList();

                if ( topResultIds.Count == 1 )
                {
                    options.RedirectUrl = GetPersonProfileUrl( topResultIds[0] );
                }
            }

            // Redirect server-side so a single-result search never paints the (empty) grid first.
            // RedirectUrl is still returned on the bag as a client-side fallback.
            if ( options.RedirectUrl.IsNotNullOrWhiteSpace() )
            {
                RequestContext.Response.RedirectToUrl( options.RedirectUrl );
            }

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
                [NavigationUrlKey.PersonDetailPage] = this.GetLinkedPageUrl( AttributeKey.PersonDetailPage, "PersonId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<PersonSearchResult> GetListQueryable( RockContext rockContext )
        {
            var personIdQuery = GetMatchingPersonIdQueryable( rockContext );
            if ( personIdQuery == null )
            {
                return Enumerable.Empty<PersonSearchResult>().AsQueryable();
            }

            var familyGroupTypeId = GroupTypeCache.GetFamilyGroupType()?.Id ?? 0;
            var homeAddressTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() ) ?? 0;

            var people = new PersonService( rockContext ).Queryable( true )
                .Where( p => personIdQuery.Contains( p.Id ) );

            return people.Select( p => new PersonSearchResult
            {
                Id = p.Id,
                FirstName = p.FirstName,
                NickName = p.NickName,
                LastName = p.LastName,
                BirthDate = p.BirthDate,
                DeceasedDate = p.DeceasedDate,
                BirthYear = p.BirthYear,
                BirthMonth = p.BirthMonth,
                BirthDay = p.BirthDay,
                ConnectionStatusValueId = p.ConnectionStatusValueId,
                RecordStatusValueId = p.RecordStatusValueId,
                RecordTypeValueId = p.RecordTypeValueId,
                AgeClassification = p.AgeClassification,
                SuffixValueId = p.SuffixValueId,
                IsDeceased = p.IsDeceased,
                Email = p.Email,
                Gender = p.Gender,
                PhotoId = p.PhotoId,
                CampusIds = p.Members
                    .Where( m =>
                        m.Group.GroupTypeId == familyGroupTypeId &&
                        m.Group.CampusId.HasValue )
                    .Select( m => m.Group.CampusId.Value )
                    .ToList(),
                HomeAddresses = p.Members
                    .Where( m => m.Group.GroupTypeId == familyGroupTypeId )
                    .SelectMany( m => m.Group.GroupLocations )
                    .Where( gl => gl.GroupLocationTypeValueId == homeAddressTypeId )
                    .Select( gl => gl.Location ),
                PhoneNumbers = p.PhoneNumbers
                    .Where( n => n.NumberTypeValueId.HasValue )
                    .Select( n => new PersonSearchResultPhone
                    {
                        NumberTypeValueId = n.NumberTypeValueId.Value,
                        Number = n.NumberFormatted
                    } )
                    .ToList(),
                TopSignalColor = p.TopSignalColor,
                TopSignalIconCssClass = p.TopSignalIconCssClass
            } );
        }

        /// <inheritdoc/>
        protected override IQueryable<PersonSearchResult> GetOrderedListQueryable( IQueryable<PersonSearchResult> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( p => p.LastName ).ThenBy( p => p.FirstName );
        }

        /// <inheritdoc/>
        protected override List<PersonSearchResult> GetListItems( IQueryable<PersonSearchResult> queryable, RockContext rockContext )
        {
            var items = queryable.ToList();
            if ( !items.Any() )
            {
                return items;
            }

            var personIds = items.Select( i => i.Id ).ToList();

            AddDataViewIcons( items, personIds, rockContext );
            AddSpouseNames( items, personIds, rockContext );
            AddEnvelopeNumbers( items, personIds, rockContext );

            return items;
        }

        /// <inheritdoc/>
        protected override GridBuilder<PersonSearchResult> GetGridBuilder()
        {
            return new GridBuilder<PersonSearchResult>()
                .WithBlock( this )
                .AddField( "id", p => p.Id )
                .AddTextField( "idKey", p => p.IdKey )
                .AddTextField( "personHtml", p => BuildPersonHtml( p ) )
                .AddTextField( "indicatorsHtml", p => BuildIndicatorsHtml( p ) )
                .AddTextField( "fullNameReversed", p => p.FullNameReversed )
                .AddDateTimeField( "birthDate", p => p.BirthDate )
                .AddField( "age", p => p.Age )
                .AddTextField( "gender", p => p.Gender.ConvertToString() )
                .AddTextField( "spouseName", p => p.SpouseName )
                .AddTextField( "connectionStatus", p => DefinedValueCache.GetName( p.ConnectionStatusValueId ) )
                .AddTextField( "recordStatus", p => DefinedValueCache.GetName( p.RecordStatusValueId ) )
                .AddTextField( "campus", p => p.CampusNames )
                .AddTextField( "envelopeNumber", p => p.EnvelopeNumber )
                .AddField( "isDeceased", p => p.IsDeceased )
                .AddField( "isInactive", p => GetIsInactive( p ) );
        }

        #endregion Methods

        #region Helper Methods

        /// <summary>
        /// Builds the queryable of person Ids that match the current search type and term.
        /// </summary>
        /// <param name="rockContext">The database context to query against.</param>
        /// <returns>An unexecuted queryable of matching person Ids, or <c>null</c> when there is no valid search.</returns>
        private IQueryable<int> GetMatchingPersonIdQueryable( RockContext rockContext )
        {
            var searchType = PageParameter( PageParameterKey.SearchType );
            var searchTerm = PageParameter( PageParameterKey.SearchTerm );

            if ( searchType.IsNullOrWhiteSpace() || searchTerm.IsNullOrWhiteSpace() )
            {
                return null;
            }

            searchType = searchType.Trim();
            searchTerm = searchTerm.Trim();

            if ( searchTerm.IsSingleSpecialCharacter() )
            {
                return null;
            }

            var personService = new PersonService( rockContext );

            switch ( searchType.ToLower() )
            {
                case SearchTypeValue.Name:
                {
                    var allowFirstNameOnly = PageParameter( PageParameterKey.AllowFirstNameOnly ).AsBoolean();
                    return personService.GetByFullName( searchTerm, allowFirstNameOnly, includeDeceased: true ).Select( p => p.Id );
                }

                case SearchTypeValue.Phone:
                {
                    var phoneNumberPersonIds = new PhoneNumberService( rockContext ).GetPersonIdsByNumber( searchTerm );
                    return personService.Queryable( new PersonService.PersonQueryOptions { IncludeNameless = true } )
                        .Where( p => phoneNumberPersonIds.Contains( p.Id ) )
                        .Select( p => p.Id );
                }

                case SearchTypeValue.Address:
                {
                    var addressPersonIds = new GroupMemberService( rockContext ).GetPersonIdsByHomeAddress( searchTerm );
                    return personService.Queryable()
                        .Where( p => addressPersonIds.Contains( p.Id ) )
                        .Select( p => p.Id );
                }

                case SearchTypeValue.Email:
                {
                    /*
                        6/17/26 - MSE

                        The email search returns the union of two separate id queries (Person.Email matches and
                        PersonSearchKey matches) instead of joining them. Keeping them as a union avoids a SQL
                        deadlock that was observed in production when the two sources were combined in one query.

                        Reason: Email matches and search-key matches are unioned, not joined, to avoid a deadlock.
                    */
                    var emailSearchTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_EMAIL.AsGuid() );

                    var peopleByEmailIds = personService.Queryable()
                        .Where( p => p.Email.Contains( searchTerm ) )
                        .Select( p => p.Id );

                    var searchKeyPersonIds = new PersonSearchKeyService( rockContext ).Queryable()
                        .Where( a => emailSearchTypeValueId.HasValue
                            && a.PersonAliasId.HasValue
                            && a.SearchTypeValueId == emailSearchTypeValueId.Value
                            && a.SearchValue.Contains( searchTerm ) )
                        .Select( a => a.PersonAlias.PersonId );

                    return peopleByEmailIds.Union( searchKeyPersonIds );
                }

                case SearchTypeValue.Birthdate:
                {
                    var birthDatePersonId = PageParameter( PageParameterKey.BirthdatePersonId ).AsIntegerOrNull();
                    if ( birthDatePersonId.HasValue )
                    {
                        return personService.Queryable()
                            .Where( p => p.Id == birthDatePersonId.Value )
                            .Select( p => p.Id );
                    }

                    var birthDate = PageParameter( PageParameterKey.Birthdate ).AsDateTime() ?? searchTerm.AsDateTime();
                    if ( !birthDate.HasValue )
                    {
                        return null;
                    }

                    return personService.Queryable()
                        .Where( p => p.BirthDate.HasValue && p.BirthDate == birthDate.Value )
                        .Select( p => p.Id );
                }

                default:
                    return null;
            }
        }

        /// <summary>
        /// Builds the "Other Possible Matches" suggestions for a name search.
        /// </summary>
        /// <param name="resultPersonIds">The Ids already present in the result set, which are excluded from the suggestions.</param>
        /// <returns>A list of name suggestions with re-search URLs, or <c>null</c> when there are none.</returns>
        private List<ListItemBag> GetAlternateMatches( List<int> resultPersonIds )
        {
            var searchTerm = PageParameter( PageParameterKey.SearchTerm )?.Trim();
            if ( searchTerm.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var similarNames = new PersonService( RockContext )
                .GetSimilarNames( searchTerm, resultPersonIds, includeDeceased: true )
                .Distinct()
                .ToList();

            if ( !similarNames.Any() )
            {
                return null;
            }

            return similarNames
                .Select( name => new ListItemBag
                {
                    Text = name,
                    Value = GetSearchUrl( name )
                } )
                .ToList();
        }

        /// <summary>
        /// Builds the rich HTML shown in the "Person" cell (photo, name, signal, responsive detail lines, phones, email, and addresses).
        /// </summary>
        /// <param name="person">The person row.</param>
        /// <returns>The HTML markup for the cell.</returns>
        private string BuildPersonHtml( PersonSearchResult person )
        {
            if ( person.IsBusiness )
            {
                return person.LastName.EncodeHtml();
            }

            var sb = new StringBuilder();

            var photoUrl = Person.GetPersonPhotoUrl( person.Initials, person.PhotoId, person.Age, person.Gender, person.RecordTypeValueId, person.AgeClassification, 100 );
            var campusNames = person.CampusNames;

            sb.Append( $"<div class=\"photo-round photo-round-sm pull-left\" style=\"background-image: url('{photoUrl}');\"></div>" );
            sb.Append( "<div class=\"pull-left margin-l-sm\">" );
            sb.Append( $"<strong>{person.FullNameReversed.EncodeHtml()}</strong> " );
            sb.Append( $"{Person.GetSignalMarkup( person.TopSignalColor, person.TopSignalIconCssClass )} " );
            sb.Append( $"<small class=\"hidden-sm hidden-md hidden-lg\"><br>{campusNames.EncodeHtml()}<br></small>" );
            sb.Append( $"<small class=\"hidden-sm hidden-md hidden-lg\">{DefinedValueCache.GetName( person.ConnectionStatusValueId ).EncodeHtml()}</small>" );
            sb.Append( $" <small class=\"hidden-md hidden-lg\">{person.AgeFormatted.EncodeHtml()}</small>" );

            foreach ( var phoneTypeGuid in PhoneTypeGuids )
            {
                var phoneType = DefinedValueCache.Get( phoneTypeGuid );
                if ( phoneType == null )
                {
                    continue;
                }

                var phone = person.PhoneNumbers?.FirstOrDefault( n => n.NumberTypeValueId == phoneType.Id );
                if ( phone != null )
                {
                    var typeInitial = phoneType.Value.Left( 1 ).ToUpper();
                    sb.Append( $"<br/><small>{typeInitial.EncodeHtml()}: {phone.Number.EncodeHtml()}</small>" );
                }
            }

            if ( person.Email.IsNotNullOrWhiteSpace() )
            {
                sb.Append( $"<br/><small>{person.Email.EncodeHtml()}</small>" );
            }

            if ( person.HomeAddresses != null )
            {
                foreach ( var location in person.HomeAddresses )
                {
                    var formattedAddress = location?.GetFullStreetAddress();
                    if ( formattedAddress.IsNullOrWhiteSpace() )
                    {
                        continue;
                    }

                    var addressHtml = formattedAddress.EncodeHtml().ConvertCrLfToHtmlBr().Replace( "<br><br>", "<br>" );
                    sb.Append( $"<small><br>{addressHtml}</small>" );
                }
            }

            sb.Append( "</div>" );

            return sb.ToString();
        }

        /// <summary>
        /// Builds the highlight indicator icon HTML for the configured data views.
        /// </summary>
        /// <param name="person">The person row.</param>
        /// <returns>The HTML markup for the indicators cell, or an empty string when there are none.</returns>
        private string BuildIndicatorsHtml( PersonSearchResult person )
        {
            if ( person.DataViewIcons == null || !person.DataViewIcons.Any() )
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach ( var icon in person.DataViewIcons )
            {
                if ( icon.IconCssClass.IsNotNullOrWhiteSpace() )
                {
                    var tooltip = $"{person.NickName} meets the conditions of the {icon.DataViewName} data view.";
                    sb.AppendLine( $"<i style=\"color:{icon.HighlightColor}\" class=\"ti-3x ti-fw {icon.IconCssClass}\" data-toggle=\"tooltip\" title=\"{tooltip.EncodeHtml()}\"></i>" );
                }
                else
                {
                    // Render a blank placeholder so every person's icons stay aligned in the same column.
                    sb.AppendLine( "<span style=\"display:block;\" class=\"ti-3x ti-fw\">&nbsp;</span>" );
                }
            }

            return $"<div class=\"d-flex align-items-end\">{sb}</div>";
        }

        /// <summary>
        /// Determines whether a person row should be styled as inactive.
        /// </summary>
        /// <param name="person">The person row.</param>
        /// <returns><c>true</c> if the person's record status is inactive; otherwise <c>false</c>.</returns>
        private bool GetIsInactive( PersonSearchResult person )
        {
            var inactiveStatusId = InactiveRecordStatusValueId;
            return inactiveStatusId.HasValue && person.RecordStatusValueId == inactiveStatusId.Value;
        }

        /// <summary>
        /// Populates the highlight indicator icons for each person row.
        /// </summary>
        /// <param name="items">The person rows.</param>
        /// <param name="personIds">The Ids of the person rows.</param>
        /// <param name="rockContext">The database context to query against.</param>
        private void AddDataViewIcons( List<PersonSearchResult> items, List<int> personIds, RockContext rockContext )
        {
            if ( !DataViewGuids.Any() )
            {
                return;
            }

            var dataViewService = new DataViewService( rockContext );

            // Materialize the data views (.ToList) so authorization can be checked, then keep only the icon metadata.
            var dataViews = dataViewService.Queryable()
                .Where( d => DataViewGuids.Contains( d.Guid ) )
                .ToList()
                .Where( d => d.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .Select( d => new DataViewIconResult
                {
                    DataViewId = d.Id,
                    DataViewName = d.Name,
                    IconCssClass = d.IconCssClass,
                    HighlightColor = d.HighlightColor
                } )
                .ToList();

            if ( !dataViews.Any() )
            {
                return;
            }

            var dataViewIds = dataViews.Select( d => d.DataViewId ).ToList();
            var persistedValues = dataViewService.GetDataViewPersistedValuesForIds( personIds, dataViewIds );

            // Group the persisted (EntityId, DataViewId) pairs once so each person's lookup is O(1).
            var persistedDataViewIdsByPersonId = persistedValues
                .ToLookup( pv => pv.EntityId, pv => pv.DataViewId );

            // Order the data views by how many of the results they match (descending) so the most populated
            // indicators appear first and the icon columns line up consistently across every row.
            var matchCountByDataViewId = persistedValues
                .GroupBy( pv => pv.DataViewId )
                .ToDictionary( g => g.Key, g => g.Count() );

            var orderedDataViews = dataViews
                .OrderByDescending( dv => matchCountByDataViewId.TryGetValue( dv.DataViewId, out var count ) ? count : 0 )
                .ToList();

            foreach ( var person in items )
            {
                var personDataViewIds = new HashSet<int>( persistedDataViewIdsByPersonId[person.Id] );

                // Emit an icon for every data view; one the person isn't in becomes a blank placeholder so columns align.
                person.DataViewIcons = orderedDataViews
                    .Select( dv => new DataViewIconResult
                    {
                        DataViewId = dv.DataViewId,
                        DataViewName = dv.DataViewName,
                        IconCssClass = personDataViewIds.Contains( dv.DataViewId ) ? dv.IconCssClass : string.Empty,
                        HighlightColor = personDataViewIds.Contains( dv.DataViewId ) ? dv.HighlightColor : string.Empty
                    } )
                    .ToList();
            }
        }

        /// <summary>
        /// Populates the spouse name for each person row when the spouse column is enabled.
        /// </summary>
        /// <param name="items">The person rows.</param>
        /// <param name="personIds">The Ids of the person rows.</param>
        /// <param name="rockContext">The database context to query against.</param>
        private void AddSpouseNames( List<PersonSearchResult> items, List<int> personIds, RockContext rockContext )
        {
            if ( !GetAttributeValue( AttributeKey.ShowSpouse ).AsBoolean() )
            {
                return;
            }

            var personService = new PersonService( rockContext );
            var personQuery = personService.Queryable().Where( p => personIds.Contains( p.Id ) );
            var spouseNamesByPersonId = personService.GetSpousesFullName( personQuery );

            foreach ( var person in items )
            {
                if ( spouseNamesByPersonId.TryGetValue( person.Id, out var spouseName ) )
                {
                    person.SpouseName = spouseName;
                }
            }
        }

        /// <summary>
        /// Populates the giving envelope number for each person row when the envelope column is visible.
        /// </summary>
        /// <param name="items">The person rows.</param>
        /// <param name="personIds">The Ids of the person rows.</param>
        /// <param name="rockContext">The database context to query against.</param>
        private void AddEnvelopeNumbers( List<PersonSearchResult> items, List<int> personIds, RockContext rockContext )
        {
            var envelopeAttribute = GetVisibleEnvelopeNumberAttribute();
            if ( envelopeAttribute == null )
            {
                return;
            }

            var envelopeNumbersByPersonId = new AttributeValueService( rockContext ).Queryable()
                .Where( a => a.AttributeId == envelopeAttribute.Id )
                .Where( a => a.EntityId.HasValue && personIds.Contains( a.EntityId.Value ) )
                .Select( a => new
                {
                    PersonId = a.EntityId.Value,
                    a.Value
                } )
                .ToList()
                .ToDictionary( k => k.PersonId, v => v.Value );

            foreach ( var person in items )
            {
                if ( envelopeNumbersByPersonId.TryGetValue( person.Id, out var envelopeNumber ) )
                {
                    person.EnvelopeNumber = envelopeNumber;
                }
            }
        }

        /// <summary>
        /// Gets the giving envelope number attribute when the envelope column should be shown; otherwise <c>null</c>.
        /// The column requires the giving envelope feature to be globally enabled, the block setting to be on, and
        /// the envelope attribute to exist.
        /// </summary>
        /// <returns>The envelope number <see cref="AttributeCache"/> when the column is visible; otherwise <c>null</c>.</returns>
        private AttributeCache GetVisibleEnvelopeNumberAttribute()
        {
            if ( !GetAttributeValue( AttributeKey.ShowEnvelopeNumber ).AsBoolean() )
            {
                return null;
            }

            if ( !GlobalAttributesCache.Get().EnableGivingEnvelopeNumber )
            {
                return null;
            }

            return AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_ENVELOPE_NUMBER.AsGuid() );
        }

        /// <summary>
        /// Determines whether the envelope number column should be visible.
        /// </summary>
        /// <returns><c>true</c> if the envelope number column should be visible; otherwise <c>false</c>.</returns>
        private bool GetIsEnvelopeNumberColumnVisible()
        {
            return GetVisibleEnvelopeNumberAttribute() != null;
        }

        /// <summary>
        /// Builds the absolute person profile URL for a single-result redirect.
        /// </summary>
        /// <param name="personId">The Id of the matched person.</param>
        /// <returns>The person profile URL keyed by IdKey.</returns>
        private string GetPersonProfileUrl( int personId )
        {
            var idKey = Rock.Utility.IdHasher.Instance.GetHash( personId );
            return $"/Person/{idKey}";
        }

        /// <summary>
        /// Builds the relative URL that re-runs the current name search for a suggested name.
        /// </summary>
        /// <param name="name">The suggested name to search for.</param>
        /// <returns>A relative query-string URL for the same page.</returns>
        private string GetSearchUrl( string name )
        {
            var queryParams = new List<string>
            {
                $"{PageParameterKey.SearchType}={Uri.EscapeDataString( PageParameter( PageParameterKey.SearchType ) ?? string.Empty )}",
                $"{PageParameterKey.SearchTerm}={Uri.EscapeDataString( name ?? string.Empty )}"
            };

            var allowFirstNameOnly = PageParameter( PageParameterKey.AllowFirstNameOnly );
            if ( allowFirstNameOnly.IsNotNullOrWhiteSpace() )
            {
                queryParams.Add( $"{PageParameterKey.AllowFirstNameOnly}={Uri.EscapeDataString( allowFirstNameOnly )}" );
            }

            return $"?{string.Join( "&", queryParams )}";
        }

        #endregion Helper Methods

        #region Helper Classes

        /// <summary>
        /// A row in the person search results grid.
        /// </summary>
        public class PersonSearchResult
        {
            /// <summary>
            /// Gets or sets the person Id.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets the IdKey used for navigation and entity-set operations.
            /// </summary>
            public string IdKey => Rock.Utility.IdHasher.Instance.GetHash( Id );

            /// <summary>
            /// Gets or sets the first name.
            /// </summary>
            public string FirstName { get; set; }

            /// <summary>
            /// Gets or sets the nick name.
            /// </summary>
            public string NickName { get; set; }

            /// <summary>
            /// Gets or sets the last name (or the business name for business records).
            /// </summary>
            public string LastName { get; set; }

            /// <summary>
            /// Gets the person's initials, used to render the avatar when no photo exists.
            /// </summary>
            public string Initials => $"{( NickName ?? string.Empty ).Truncate( 1, false )}{( LastName ?? string.Empty ).Truncate( 1, false )}";

            private bool? _isBusiness;

            /// <summary>
            /// Gets a value indicating whether this record is a business. The result is resolved once per
            /// row because <see cref="IsBusiness"/> is read several times while rendering the person cell.
            /// </summary>
            public bool IsBusiness
            {
                get
                {
                    if ( !_isBusiness.HasValue )
                    {
                        var businessRecordTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() )?.Id;
                        _isBusiness = RecordTypeValueId.HasValue && businessRecordTypeId.HasValue && RecordTypeValueId.Value == businessRecordTypeId.Value;
                    }

                    return _isBusiness.Value;
                }
            }

            private string _fullNameReversed;

            /// <summary>
            /// Gets the full name in "Last Suffix, Nick" form. Suffix is resolved from the cache to avoid lazy loading.
            /// The result is built once per row because it is read both for the person cell and the sortable column.
            /// </summary>
            public string FullNameReversed
            {
                get
                {
                    if ( _fullNameReversed != null )
                    {
                        return _fullNameReversed;
                    }

                    if ( IsBusiness )
                    {
                        _fullNameReversed = LastName;
                        return _fullNameReversed;
                    }

                    var fullName = new StringBuilder();
                    fullName.Append( LastName );

                    if ( SuffixValueId.HasValue )
                    {
                        var suffix = DefinedValueCache.GetName( SuffixValueId.Value );
                        if ( suffix != null )
                        {
                            fullName.AppendFormat( " {0}", suffix );
                        }
                    }

                    fullName.AppendFormat( ", {0}", NickName );
                    _fullNameReversed = fullName.ToString();
                    return _fullNameReversed;
                }
            }

            /// <summary>
            /// Gets or sets the home addresses for the person's family.
            /// </summary>
            public IEnumerable<Location> HomeAddresses { get; set; }

            /// <summary>
            /// Gets or sets the birth date.
            /// </summary>
            public DateTime? BirthDate { get; set; }

            /// <summary>
            /// Gets or sets the deceased date.
            /// </summary>
            public DateTime? DeceasedDate { get; set; }

            /// <summary>
            /// Gets or sets the birth year.
            /// </summary>
            public int? BirthYear { get; set; }

            /// <summary>
            /// Gets or sets the birth month.
            /// </summary>
            public int? BirthMonth { get; set; }

            /// <summary>
            /// Gets or sets the birth day.
            /// </summary>
            public int? BirthDay { get; set; }

            /// <summary>
            /// Gets or sets the email address.
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// Gets or sets the photo identifier.
            /// </summary>
            public int? PhotoId { get; set; }

            /// <summary>
            /// Gets or sets the Ids of the campuses associated with the person's families.
            /// </summary>
            public List<int> CampusIds { get; set; }

            private string _campusNames;

            /// <summary>
            /// Gets the comma-delimited campus names. Resolved once per row because it is read both for the
            /// person cell and the campus column.
            /// </summary>
            public string CampusNames
            {
                get
                {
                    if ( _campusNames != null )
                    {
                        return _campusNames;
                    }

                    if ( CampusIds == null || !CampusIds.Any() )
                    {
                        _campusNames = string.Empty;
                        return _campusNames;
                    }

                    _campusNames = CampusIds
                        .Select( id => CampusCache.Get( id )?.Name )
                        .Where( name => name.IsNotNullOrWhiteSpace() )
                        .ToList()
                        .AsDelimited( ", " );
                    return _campusNames;
                }
            }

            /// <summary>
            /// Gets or sets the gender.
            /// </summary>
            public Gender Gender { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the person is deceased.
            /// </summary>
            public bool IsDeceased { get; set; }

            /// <summary>
            /// Gets the person's age, calculated from the birth and deceased dates.
            /// </summary>
            public int? Age => Person.GetAge( BirthDate, DeceasedDate );

            /// <summary>
            /// Gets the age formatted in parentheses (e.g. "(34)"), or an empty string when the age is unknown.
            /// </summary>
            public string AgeFormatted => Age.HasValue ? $"({Age.Value})" : string.Empty;

            /// <summary>
            /// Gets or sets the connection status defined value Id.
            /// </summary>
            public int? ConnectionStatusValueId { get; set; }

            /// <summary>
            /// Gets or sets the record type defined value Id.
            /// </summary>
            public int? RecordTypeValueId { get; set; }

            /// <summary>
            /// Gets or sets the suffix defined value Id.
            /// </summary>
            public int? SuffixValueId { get; set; }

            /// <summary>
            /// Gets or sets the record status defined value Id.
            /// </summary>
            public int? RecordStatusValueId { get; set; }

            /// <summary>
            /// Gets or sets the age classification.
            /// </summary>
            public AgeClassification AgeClassification { get; set; }

            /// <summary>
            /// Gets or sets the spouse's full name, populated only when the spouse column is enabled.
            /// </summary>
            public string SpouseName { get; set; }

            /// <summary>
            /// Gets or sets the giving envelope number, populated only when the envelope column is visible.
            /// </summary>
            public string EnvelopeNumber { get; set; }

            /// <summary>
            /// Gets or sets the phone numbers for the person.
            /// </summary>
            public List<PersonSearchResultPhone> PhoneNumbers { get; set; }

            /// <summary>
            /// Gets or sets the top signal color, indicating whether the person has a signal attached.
            /// </summary>
            public string TopSignalColor { get; set; }

            /// <summary>
            /// Gets or sets the top signal icon CSS class.
            /// </summary>
            public string TopSignalIconCssClass { get; set; }

            /// <summary>
            /// Gets or sets the highlight indicator icons for the configured data views.
            /// </summary>
            public List<DataViewIconResult> DataViewIcons { get; set; }
        }

        /// <summary>
        /// A phone number shown in the person cell.
        /// </summary>
        public class PersonSearchResultPhone
        {
            /// <summary>
            /// Gets or sets the number type defined value Id.
            /// </summary>
            public int NumberTypeValueId { get; set; }

            /// <summary>
            /// Gets or sets the formatted number.
            /// </summary>
            public string Number { get; set; }
        }

        /// <summary>
        /// Minimal icon information for a persisted data view that defines an icon CSS class and optional highlight color.
        /// </summary>
        public class DataViewIconResult
        {
            /// <summary>
            /// Gets or sets the icon CSS class defined by the data view.
            /// </summary>
            public string IconCssClass { get; set; }

            /// <summary>
            /// Gets or sets the highlight color defined by the data view.
            /// </summary>
            public string HighlightColor { get; set; }

            /// <summary>
            /// Gets or sets the Id of the data view the icon comes from.
            /// </summary>
            public int DataViewId { get; set; }

            /// <summary>
            /// Gets or sets the name of the data view the icon comes from.
            /// </summary>
            public string DataViewName { get; set; }
        }

        #endregion Helper Classes
    }
}
