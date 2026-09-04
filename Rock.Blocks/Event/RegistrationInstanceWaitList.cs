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
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Event.RegistrationInstanceWaitList;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Core.Grid;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays the list of individuals on the wait list for a Registration Instance.
    /// </summary>
    [DisplayName( "Registration Instance - Wait List" )]
    [Category( "Event" )]
    [Description( "Block for editing the wait list associated with an event registration instance." )]
    [IconCssClass( "ti ti-clock" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Wait List Confirmation Page",
        Description = "The page that confirms moving selected people from the wait list into full registrations.",
        Key = AttributeKey.WaitListProcessingPage,
        DefaultValue = Rock.SystemGuid.Page.REGISTRATION_WAIT_LIST_CONFIRMATION,
        IsRequired = false,
        Order = 1 )]

    [LinkedPage(
        "Registration Page",
        Description = "The page for editing registration and registrant information.",
        Key = AttributeKey.RegistrationPage,
        DefaultValue = Rock.SystemGuid.Page.REGISTRATION_DETAIL,
        IsRequired = false,
        Order = 2 )]

    [LinkedPage(
        "Person Profile Page",
        Description = "Page used for viewing a person's profile. If set, a view profile button will show for each participant.",
        Key = AttributeKey.PersonProfilePage,
        DefaultValue = Rock.SystemGuid.Page.PERSON_PROFILE_PERSON_PAGES,
        IsRequired = false,
        Order = 3 )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]

    [Rock.SystemGuid.EntityTypeGuid( "68E87E9E-B412-44FF-801F-35AB9D0BCD51" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "D0D35429-5779-46CF-A15E-6203CCC62F48" )]
    [Rock.SystemGuid.BlockTypeGuid( "671244E1-747E-436D-B866-13469723B424" )]
    [CustomizedGrid]
    public class RegistrationInstanceWaitList : RockEntityListBlockType<RegistrationRegistrant>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string WaitListProcessingPage = "WaitListProcessingPage";
            public const string RegistrationPage = "RegistrationPage";
            public const string PersonProfilePage = "PersonProfilePage";
        }

        private static class NavigationUrlKey
        {
            public const string RegistrationPage = "RegistrationPage";
            public const string PersonProfilePage = "PersonProfilePage";
        }

        private static class PageParameterKey
        {
            public const string RegistrationInstanceId = "RegistrationInstanceId";
            public const string RegistrationId = "RegistrationId";
            public const string WaitListSetId = "WaitListSetId";
            public const string PersonId = "PersonId";
        }

        private static class PreferenceKey
        {
            public const string FilterDateRange = "filter-date-range";
        }

        #endregion Keys

        #region Fields

        private RegistrationInstance _registrationInstance;
        private bool _hasAttemptedRegistrationInstanceLoad;
        private List<RegistrantFormFieldInfo> _registrantFormFields;
        private List<RegistrationPersonFieldType> _visiblePersonFieldTypes;
        private Dictionary<int, Location> _homeAddresses = new Dictionary<int, Location>();
        private Dictionary<int, PhoneNumber> _mobilePhoneNumbers = new Dictionary<int, PhoneNumber>();
        private Dictionary<int, PhoneNumber> _homePhoneNumbers = new Dictionary<int, PhoneNumber>();
        private Dictionary<int, PhoneNumber> _workPhoneNumbers = new Dictionary<int, PhoneNumber>();
        private Dictionary<int, List<string>> _personCampusNames = new Dictionary<int, List<string>>();
        private Dictionary<int, int> _waitListOrderByRegistrantId = new Dictionary<int, int>();

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the current person's block-scoped preferences.
        /// </summary>
        private PersonPreferenceCollection BlockPersonPreferences => GetBlockPersonPreferences();

        /// <summary>
        /// Gets the registration date range filter from person preferences.
        /// </summary>
        private SlidingDateRangeBag FilterDateRange => BlockPersonPreferences
            .GetValue( MakeKeyUniqueToRegistrationTemplate( PreferenceKey.FilterDateRange ) )
            .ToSlidingDateRangeBagOrNull();

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<RegistrationInstanceWaitListOptionsBag>();
            var builder = GetGridBuilder();

            var isAddDeleteEnabled = GetIsAddDeleteEnabled();
            box.IsAddEnabled = isAddDeleteEnabled;
            box.IsDeleteEnabled = isAddDeleteEnabled;
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
        private RegistrationInstanceWaitListOptionsBag GetBoxOptions()
        {
            var registrationInstance = GetRegistrationInstance();

            var options = new RegistrationInstanceWaitListOptionsBag
            {
                VisiblePersonFieldTypes = GetVisiblePersonFieldTypes(),
                MobilePhoneLabel = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() )?.Value,
                HomePhoneLabel = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() )?.Value,
                WorkPhoneLabel = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() )?.Value
            };

            if ( registrationInstance != null )
            {
                options.ExportTitle = $"{registrationInstance.Name} - Wait List";
                options.RegistrationTemplateGuid = registrationInstance.RegistrationTemplate?.Guid;
            }

            return options;
        }

        /// <summary>
        /// Determines whether the add and delete actions should be enabled.
        /// True when the user has block EDIT, or REGISTER / EDIT /
        /// ADMINISTRATE on the registration instance.
        /// </summary>
        /// <returns>A boolean value indicating whether add/delete should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            var registrationInstance = GetRegistrationInstance();

            if ( registrationInstance == null )
            {
                return false;
            }

            var currentPerson = RequestContext.CurrentPerson;

            return BlockCache.IsAuthorized( Authorization.EDIT, currentPerson )
                || registrationInstance.IsAuthorized( Authorization.REGISTER, currentPerson )
                || registrationInstance.IsAuthorized( Authorization.EDIT, currentPerson )
                || registrationInstance.IsAuthorized( Authorization.ADMINISTRATE, currentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var registrationInstance = GetRegistrationInstance();

            var registrationPageParams = new Dictionary<string, string>
            {
                { PageParameterKey.RegistrationId, "((Key))" },
                { PageParameterKey.RegistrationInstanceId, registrationInstance?.IdKey }
            };

            var personProfilePageParams = new Dictionary<string, string>
            {
                { PageParameterKey.PersonId, "((Key))" }
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.RegistrationPage] = this.GetLinkedPageUrl( AttributeKey.RegistrationPage, registrationPageParams ),
                [NavigationUrlKey.PersonProfilePage] = this.GetLinkedPageUrl( AttributeKey.PersonProfilePage, personProfilePageParams )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<RegistrationRegistrant> GetListQueryable( RockContext rockContext )
        {
            var registrationInstance = GetRegistrationInstance();

            if ( registrationInstance == null )
            {
                return Enumerable.Empty<RegistrationRegistrant>().AsQueryable();
            }

            var qry = new RegistrationRegistrantService( RockContext ).Queryable()
                .Include( r => r.PersonAlias.Person )
                .Include( r => r.Registration.PersonAlias.Person )
                .AsNoTracking()
                .Where( r =>
                    r.Registration.RegistrationInstanceId == registrationInstance.Id
                    && r.PersonAlias != null
                    && r.PersonAlias.Person != null
                    && r.OnWaitList );

            // Apply the registration date range filter.
            var dateRange = FilterDateRange?.ToActualDateRange();

            if ( dateRange?.Start != null )
            {
                var start = dateRange.Start.Value;
                qry = qry.Where( r => r.CreatedDateTime.HasValue && r.CreatedDateTime.Value >= start );
            }

            if ( dateRange?.End != null )
            {
                var end = dateRange.End.Value;
                qry = qry.Where( r => r.CreatedDateTime.HasValue && r.CreatedDateTime.Value < end );
            }

            return qry;
        }

        /// <inheritdoc/>
        protected override IQueryable<RegistrationRegistrant> GetOrderedListQueryable( IQueryable<RegistrationRegistrant> queryable, RockContext rockContext )
        {
            return queryable
                .OrderBy( r => r.CreatedDateTime )
                .ThenBy( r => r.Id );
        }

        /// <inheritdoc/>
        protected override List<RegistrationRegistrant> GetListItems( IQueryable<RegistrationRegistrant> queryable, RockContext rockContext )
        {
            RockContext.Database.CommandTimeout = 180;

            var items = queryable.ToList();

            LoadWaitListOrder();

            if ( !items.Any() )
            {
                return items;
            }

            var personIds = items
                .Where( r => r.PersonAlias != null )
                .Select( r => r.PersonAlias.PersonId )
                .Distinct()
                .ToList();

            var visiblePersonFieldTypes = GetVisiblePersonFieldTypes();

            if ( visiblePersonFieldTypes.Contains( RegistrationPersonFieldType.Address ) )
            {
                _homeAddresses = Person.GetHomeLocations( personIds, RockContext );
            }

            if ( visiblePersonFieldTypes.Contains( RegistrationPersonFieldType.MobilePhone ) )
            {
                _mobilePhoneNumbers = GetPhoneNumberLookup( personIds, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
            }

            if ( visiblePersonFieldTypes.Contains( RegistrationPersonFieldType.HomePhone ) )
            {
                _homePhoneNumbers = GetPhoneNumberLookup( personIds, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
            }

            if ( visiblePersonFieldTypes.Contains( RegistrationPersonFieldType.WorkPhone ) )
            {
                _workPhoneNumbers = GetPhoneNumberLookup( personIds, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK );
            }

            if ( visiblePersonFieldTypes.Contains( RegistrationPersonFieldType.Campus ) )
            {
                LoadPersonCampusNames( personIds );
            }

            LoadAdditionalEntityAttributes( items );

            return items;
        }

        /// <inheritdoc/>
        protected override List<AttributeCache> BuildGridAttributes()
        {
            return GetGridAttributesBySource( RegistrationFieldSource.RegistrantAttribute );
        }

        /// <inheritdoc/>
        protected override GridBuilder<RegistrationRegistrant> GetGridBuilder()
        {
            var builder = new GridBuilder<RegistrationRegistrant>()
                .WithBlock( this )
                .AddTextField( "idKey", r => r.IdKey )
                .AddPersonField( "person", r => r.PersonAlias?.Person )
                .AddTextField( "personIdKey", r => r.PersonAlias?.Person?.IdKey )
                .AddTextField( "registrationIdKey", r => IdHasher.Instance.GetHash( r.RegistrationId ) )
                .AddField( "registrantId", r => r.Id )
                .AddField( "waitListOrder", r => GetWaitListOrder( r ) )
                .AddTextField( "registeredBy", r => r.Registration?.PersonAlias?.Person?.FullName )
                .AddDateTimeField( "createdDateTime", r => r.CreatedDateTime );

            AddPersonFieldColumns( builder );
            AddConfiguredAttributeFields( builder );

            return builder;
        }

        /// <summary>
        /// Adds a grid field for each person field type the registration
        /// template has configured to show on the grid.
        /// </summary>
        /// <param name="builder">The grid builder to add the fields to.</param>
        private void AddPersonFieldColumns( GridBuilder<RegistrationRegistrant> builder )
        {
            foreach ( var personFieldType in GetVisiblePersonFieldTypes() )
            {
                switch ( personFieldType )
                {
                    case RegistrationPersonFieldType.Campus:
                        builder.AddField( "campus", GetCampusNames );
                        break;

                    case RegistrationPersonFieldType.Email:
                        builder.AddTextField( "email", r => r.PersonAlias?.Person?.Email );
                        break;

                    case RegistrationPersonFieldType.Birthdate:
                        builder
                            .AddDateTimeField( "birthdate", r => r.PersonAlias?.Person?.BirthDate )
                            .AddTextField( "birthdateAge", r => GetFormattedAgeText( r.PersonAlias?.Person?.BirthDate ) );
                        break;

                    case RegistrationPersonFieldType.MiddleName:
                        builder.AddTextField( "middleName", r => r.PersonAlias?.Person?.MiddleName );
                        break;

                    case RegistrationPersonFieldType.AnniversaryDate:
                        builder.AddDateTimeField( "anniversaryDate", r => r.PersonAlias?.Person?.AnniversaryDate );
                        break;

                    case RegistrationPersonFieldType.Grade:
                        builder.AddTextField( "grade", r => r.PersonAlias?.Person?.GradeFormatted );
                        break;

                    case RegistrationPersonFieldType.Gender:
                        builder.AddTextField( "gender", r => r.PersonAlias?.Person == null ? null : r.PersonAlias.Person.Gender.ConvertToString() );
                        break;

                    case RegistrationPersonFieldType.MaritalStatus:
                        builder.AddTextField( "maritalStatus", r => GetDefinedValueName( r.PersonAlias?.Person?.MaritalStatusValueId ) );
                        break;

                    case RegistrationPersonFieldType.ConnectionStatus:
                        builder.AddTextField( "connectionStatus", r => GetDefinedValueName( r.PersonAlias?.Person?.ConnectionStatusValueId ) );
                        break;

                    case RegistrationPersonFieldType.MobilePhone:
                        builder.AddTextField( "mobilePhone", r => GetPhoneNumberDisplay( _mobilePhoneNumbers, r ) );
                        break;

                    case RegistrationPersonFieldType.HomePhone:
                        builder.AddTextField( "homePhone", r => GetPhoneNumberDisplay( _homePhoneNumbers, r ) );
                        break;

                    case RegistrationPersonFieldType.WorkPhone:
                        builder.AddTextField( "workPhone", r => GetPhoneNumberDisplay( _workPhoneNumbers, r ) );
                        break;

                    case RegistrationPersonFieldType.Address:
                        builder.AddTextField( "address", r => GetHomeLocation( r )?.FormattedAddress );
                        break;

                    case RegistrationPersonFieldType.Race:
                        builder.AddTextField( "race", r => GetDefinedValueName( r.PersonAlias?.Person?.RaceValueId ) );
                        break;

                    case RegistrationPersonFieldType.Ethnicity:
                        builder.AddTextField( "ethnicity", r => GetDefinedValueName( r.PersonAlias?.Person?.EthnicityValueId ) );
                        break;
                }
            }
        }

        /// <summary>
        /// Adds an attribute value field to the grid for every show-on-grid
        /// attribute, in the order the attributes appear on the registration
        /// template's forms. Group member attributes are excluded, matching the
        /// WebForms wait list which filtered them out.
        /// </summary>
        /// <param name="builder">The grid builder to add the fields to.</param>
        private void AddConfiguredAttributeFields( GridBuilder<RegistrationRegistrant> builder )
        {
            var currentPerson = RequestContext.CurrentPerson;
            var seenAttributeIds = new HashSet<int>();

            foreach ( var formField in GetRegistrantFormFields() )
            {
                var attribute = formField.Attribute;

                if ( attribute == null
                    || !attribute.IsAuthorized( Authorization.VIEW, currentPerson )
                    || !seenAttributeIds.Add( attribute.Id ) )
                {
                    continue;
                }

                switch ( formField.FieldSource )
                {
                    case RegistrationFieldSource.RegistrantAttribute:
                        AddEntityAttributeField( builder, attribute, r => r );
                        break;

                    case RegistrationFieldSource.PersonAttribute:
                        AddEntityAttributeField( builder, attribute, r => r.PersonAlias?.Person );
                        break;
                }
            }
        }

        /// <summary>
        /// Adds an attribute value field to the grid, reading values from the
        /// entity returned by <paramref name="selector"/>.
        /// </summary>
        /// <remarks>
        /// This grid mixes registrant and person attributes, so field names
        /// include the attribute identifier to stay unique when the same
        /// attribute key exists on more than one source. The selector result
        /// may be null, which produces an empty cell.
        /// </remarks>
        /// <param name="builder">The grid builder to add the field to.</param>
        /// <param name="attribute">The attribute to add a field for.</param>
        /// <param name="selector">The function that returns the entity that holds the attribute value.</param>
        private static void AddEntityAttributeField( GridBuilder<RegistrationRegistrant> builder, AttributeCache attribute, Func<RegistrationRegistrant, IHasAttributes> selector )
        {
            var key = attribute.Key;
            var fieldKey = $"attr_{attribute.Id}_{key}";
            var isBooleanFieldType = attribute.FieldType?.Guid == Rock.SystemGuid.FieldType.BOOLEAN.AsGuid();

            builder.AddField( fieldKey, item =>
            {
                var entity = selector( item );

                if ( entity == null )
                {
                    return null;
                }

                var htmlValue = entity.GetAttributeCondensedHtmlValue( key );

                if ( isBooleanFieldType )
                {
                    htmlValue = htmlValue == "Y" ? "<i class=\"ti ti-check\"></i>" : string.Empty;
                }

                return new
                {
                    Html = htmlValue,
                    Text = entity.GetAttributeCondensedTextValue( key )
                };
            } );

            builder.AddDefinitionAction( definition =>
            {
                definition.AttributeFields.Add( new AttributeFieldDefinitionBag
                {
                    Name = fieldKey,
                    Title = attribute.Name,
                    FieldTypeGuid = attribute.FieldType?.Guid ?? Rock.SystemGuid.FieldType.TEXT.AsGuid()
                } );
            } );
        }

        /// <summary>
        /// Gets the absolute wait list position of the registrant (1-based),
        /// computed across every wait list registrant regardless of the current
        /// filters, or null when the registrant is not in the lookup.
        /// </summary>
        /// <param name="registrant">The registrant.</param>
        /// <returns>The 1-based wait list order, or null.</returns>
        private int? GetWaitListOrder( RegistrationRegistrant registrant )
        {
            if ( _waitListOrderByRegistrantId.TryGetValue( registrant.Id, out var order ) )
            {
                return order;
            }

            return null;
        }

        /// <summary>
        /// Gets the family campus names for the registrant's person, one
        /// entry per campus so each renders on its own line, or null when no
        /// campus data was loaded.
        /// </summary>
        /// <param name="registrant">The registrant.</param>
        /// <returns>The list of campus names or null.</returns>
        private List<string> GetCampusNames( RegistrationRegistrant registrant )
        {
            var personId = registrant.PersonAlias?.PersonId;

            if ( personId.HasValue && _personCampusNames.TryGetValue( personId.Value, out var campusNames ) )
            {
                return campusNames;
            }

            return null;
        }

        /// <summary>
        /// Gets the home location of the registrant's person, or null when
        /// the person has no mapped home address.
        /// </summary>
        /// <param name="registrant">The registrant.</param>
        /// <returns>The home <see cref="Location"/> or null.</returns>
        private Location GetHomeLocation( RegistrationRegistrant registrant )
        {
            var personId = registrant.PersonAlias?.PersonId;

            if ( personId.HasValue && _homeAddresses.TryGetValue( personId.Value, out var location ) )
            {
                return location;
            }

            return null;
        }

        /// <summary>
        /// Gets the display text for a registrant's phone number from the
        /// given lookup. Unlisted numbers display as "Unlisted".
        /// </summary>
        /// <param name="phoneNumbers">The phone number lookup keyed by person identifier.</param>
        /// <param name="registrant">The registrant.</param>
        /// <returns>The phone number display text.</returns>
        private static string GetPhoneNumberDisplay( Dictionary<int, PhoneNumber> phoneNumbers, RegistrationRegistrant registrant )
        {
            var personId = registrant.PersonAlias?.PersonId;

            if ( !personId.HasValue || !phoneNumbers.TryGetValue( personId.Value, out var phoneNumber ) || phoneNumber == null )
            {
                return string.Empty;
            }

            if ( phoneNumber.NumberFormatted.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            return phoneNumber.IsUnlisted ? "Unlisted" : phoneNumber.NumberFormatted;
        }

        /// <summary>
        /// Gets the formatted age text (e.g. "35 yrs") for a past date, used
        /// as the suffix of the Birthdate column.
        /// </summary>
        /// <param name="dateTime">The date to compute the age of.</param>
        /// <returns>The formatted age, or null when the date is missing or not in the past.</returns>
        private static string GetFormattedAgeText( DateTime? dateTime )
        {
            if ( !dateTime.HasValue || dateTime.Value >= RockDateTime.Now )
            {
                return null;
            }

            return dateTime.Value.GetFormattedAge();
        }

        /// <summary>
        /// Gets the display value of a defined value by its identifier.
        /// </summary>
        /// <param name="definedValueId">The defined value identifier.</param>
        /// <returns>The defined value text or null.</returns>
        private static string GetDefinedValueName( int? definedValueId )
        {
            return definedValueId.HasValue
                ? DefinedValueCache.Get( definedValueId.Value )?.Value
                : null;
        }

        /// <summary>
        /// Loads the absolute wait list ordering (1-based position by
        /// CreatedDateTime) across every wait list registrant in the instance,
        /// independent of the current grid filters, so a registrant keeps the
        /// same order number even when the list is filtered by date range.
        /// </summary>
        private void LoadWaitListOrder()
        {
            _waitListOrderByRegistrantId = new Dictionary<int, int>();

            var registrationInstance = GetRegistrationInstance();

            if ( registrationInstance == null )
            {
                return;
            }

            var orderedRegistrantIds = new RegistrationRegistrantService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( r =>
                    r.Registration.RegistrationInstanceId == registrationInstance.Id
                    && r.PersonAlias != null
                    && r.PersonAlias.Person != null
                    && r.OnWaitList )
                .OrderBy( r => r.CreatedDateTime )
                .ThenBy( r => r.Id )
                .Select( r => r.Id )
                .ToList();

            for ( var index = 0; index < orderedRegistrantIds.Count; index++ )
            {
                _waitListOrderByRegistrantId[orderedRegistrantIds[index]] = index + 1;
            }
        }

        /// <summary>
        /// Gets a lookup of one phone number of the given type per person.
        /// </summary>
        /// <param name="personIds">The person identifiers to include.</param>
        /// <param name="phoneTypeValueGuid">The defined value unique identifier of the phone type.</param>
        /// <returns>A dictionary of phone numbers keyed by person identifier.</returns>
        private Dictionary<int, PhoneNumber> GetPhoneNumberLookup( List<int> personIds, string phoneTypeValueGuid )
        {
            var phoneTypeValueId = DefinedValueCache.Get( phoneTypeValueGuid.AsGuid() )?.Id;

            if ( !phoneTypeValueId.HasValue )
            {
                return new Dictionary<int, PhoneNumber>();
            }

            return new PhoneNumberService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( pn =>
                    pn.NumberTypeValueId == phoneTypeValueId.Value
                    && personIds.Contains( pn.PersonId ) )
                .ToList()
                .GroupBy( pn => pn.PersonId )
                .ToDictionary( g => g.Key, g => g.OrderBy( pn => pn.Id ).First() );
        }

        /// <summary>
        /// Loads the lookup of family campus names per person for the campus
        /// column.
        /// </summary>
        /// <param name="personIds">The person identifiers to include.</param>
        private void LoadPersonCampusNames( List<int> personIds )
        {
            var familyGroupTypeId = GroupTypeCache.GetFamilyGroupType()?.Id;

            if ( !familyGroupTypeId.HasValue )
            {
                return;
            }

            // GroupMemberService.Queryable() already excludes archived members
            // and deceased people, so those filters are not repeated here.
            var personCampusIds = new GroupMemberService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( gm =>
                    gm.Group.GroupTypeId == familyGroupTypeId.Value
                    && gm.Group.CampusId.HasValue
                    && personIds.Contains( gm.PersonId ) )
                .Select( gm => new { gm.PersonId, CampusId = gm.Group.CampusId.Value } )
                .Distinct()
                .ToList();

            // Resolve each distinct campus name once rather than per
            // person-campus pair; registrants overwhelmingly share the same
            // few campuses.
            var campusNameByCampusId = personCampusIds
                .Select( a => a.CampusId )
                .Distinct()
                .ToDictionary( campusId => campusId, campusId => CampusCache.Get( campusId )?.Name );

            _personCampusNames = personCampusIds
                .GroupBy( a => a.PersonId )
                .ToDictionary(
                    g => g.Key,
                    g => g.Select( a => campusNameByCampusId[a.CampusId] )
                        .Where( name => name.IsNotNullOrWhiteSpace() )
                        .ToList() );
        }

        /// <summary>
        /// Bulk loads the attribute values for the person attributes that are
        /// shown on the grid. Registrant attribute values are loaded
        /// automatically by the base class. Group member attributes are not
        /// shown on the wait list.
        /// </summary>
        /// <param name="items">The registrants being displayed.</param>
        private void LoadAdditionalEntityAttributes( List<RegistrationRegistrant> items )
        {
            var personAttributes = GetGridAttributesBySource( RegistrationFieldSource.PersonAttribute );

            if ( personAttributes.Any() )
            {
                var personAttributeIds = personAttributes.Select( a => a.Id ).ToList();
                var persons = items
                    .Select( r => r.PersonAlias?.Person )
                    .Where( p => p != null )
                    .Cast<IHasAttributes>()
                    .ToList();

                Helper.LoadFilteredAttributes( typeof( Person ), persons, RockContext, a => personAttributeIds.Contains( a.Id ) );
            }
        }

        /// <summary>
        /// Gets the person field types that the registration template has
        /// configured to show on the grid.
        /// </summary>
        /// <returns>The list of visible person field types.</returns>
        private List<RegistrationPersonFieldType> GetVisiblePersonFieldTypes()
        {
            if ( _visiblePersonFieldTypes != null )
            {
                return _visiblePersonFieldTypes;
            }

            _visiblePersonFieldTypes = GetRegistrantFormFields()
                .Where( f => f.FieldSource == RegistrationFieldSource.PersonField && f.PersonFieldType.HasValue )
                .Select( f => f.PersonFieldType.Value )
                .Distinct()
                .ToList();

            return _visiblePersonFieldTypes;
        }

        /// <summary>
        /// Gets the show-on-grid attributes for the given field source,
        /// limited to attributes the current person may view.
        /// </summary>
        /// <param name="fieldSource">The form field source to match.</param>
        /// <returns>The list of viewable grid attributes.</returns>
        private List<AttributeCache> GetGridAttributesBySource( RegistrationFieldSource fieldSource )
        {
            // The same attribute can be configured on more than one form, so
            // dedupe by identifier to keep the grid field names unique.
            return GetRegistrantFormFields()
                .Where( f => f.FieldSource == fieldSource && f.Attribute != null )
                .Select( f => f.Attribute )
                .Where( a => a.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .GroupBy( a => a.Id )
                .Select( g => g.First() )
                .ToList();
        }

        /// <summary>
        /// Gets the registration template form fields that are configured to
        /// show on the grid. First name and last name person fields are
        /// excluded because the person column already displays the name, and
        /// group member attributes are excluded to match the WebForms wait
        /// list. The result is cached for the lifetime of the request.
        /// </summary>
        /// <returns>The list of grid form fields.</returns>
        private List<RegistrantFormFieldInfo> GetRegistrantFormFields()
        {
            if ( _registrantFormFields != null )
            {
                return _registrantFormFields;
            }

            _registrantFormFields = new List<RegistrantFormFieldInfo>();

            var registrationTemplateId = GetRegistrationInstance()?.RegistrationTemplateId;

            if ( !registrationTemplateId.HasValue )
            {
                return _registrantFormFields;
            }

            var forms = new RegistrationTemplateFormService( RockContext )
                .Queryable().AsNoTracking()
                .Include( f => f.Fields )
                .Where( f => f.RegistrationTemplateId == registrationTemplateId.Value )
                .OrderBy( f => f.Order )
                .ToList();

            foreach ( var form in forms )
            {
                foreach ( var formField in form.Fields.Where( f => f.IsGridField ).OrderBy( f => f.Order ) )
                {
                    if ( formField.FieldSource == RegistrationFieldSource.PersonField )
                    {
                        if ( formField.PersonFieldType == RegistrationPersonFieldType.FirstName
                            || formField.PersonFieldType == RegistrationPersonFieldType.LastName )
                        {
                            continue;
                        }

                        _registrantFormFields.Add( new RegistrantFormFieldInfo
                        {
                            FieldSource = formField.FieldSource,
                            PersonFieldType = formField.PersonFieldType
                        } );
                    }
                    else if ( formField.FieldSource == RegistrationFieldSource.GroupMemberAttribute )
                    {
                        // Group member attributes are intentionally not shown on
                        // the wait list, matching the WebForms block.
                        continue;
                    }
                    else if ( formField.AttributeId.HasValue )
                    {
                        _registrantFormFields.Add( new RegistrantFormFieldInfo
                        {
                            FieldSource = formField.FieldSource,
                            Attribute = AttributeCache.Get( formField.AttributeId.Value )
                        } );
                    }
                }
            }

            return _registrantFormFields;
        }

        /// <summary>
        /// Gets the registration instance from the RegistrationInstanceId page
        /// parameter (Id, IdKey, or Guid) with its RegistrationTemplate eagerly
        /// loaded. The result is cached for the lifetime of the request.
        /// </summary>
        /// <returns>The registration instance, or null if the parameter is missing or does not resolve.</returns>
        private RegistrationInstance GetRegistrationInstance()
        {
            // Cache the result for the request, including a null result, so the
            // several callers do not each re-query when the parameter is
            // missing or does not resolve.
            if ( _hasAttemptedRegistrationInstanceLoad )
            {
                return _registrationInstance;
            }

            _hasAttemptedRegistrationInstanceLoad = true;

            var registrationInstanceKey = PageParameter( PageParameterKey.RegistrationInstanceId );

            if ( registrationInstanceKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            _registrationInstance = new RegistrationInstanceService( RockContext )
                .GetQueryableByKey( registrationInstanceKey, !PageCache.Layout.Site.DisablePredictableIds )
                .Include( a => a.RegistrationTemplate )
                .AsNoTracking()
                .FirstOrDefault();

            return _registrationInstance;
        }

        /// <summary>
        /// Returns the given preference key scoped to the current registration
        /// template's Guid so that switching between templates does not leak
        /// filter state. Falls back to the raw key if the template Guid is not
        /// available.
        /// </summary>
        /// <param name="key">The preference key.</param>
        /// <returns>The scoped preference key.</returns>
        private string MakeKeyUniqueToRegistrationTemplate( string key )
        {
            var templateGuid = GetRegistrationInstance()?.RegistrationTemplate?.Guid;

            if ( templateGuid.HasValue )
            {
                return $"{templateGuid.Value}-{key}";
            }

            return key;
        }

        /// <summary>
        /// Determines whether the current person is authorized to delete the
        /// given registrant. Allowed when the person has EDIT on the block, or
        /// REGISTER / EDIT / ADMINISTRATE on the registration the registrant
        /// belongs to.
        /// </summary>
        /// <param name="registrant">The registrant to check.</param>
        /// <returns><c>true</c> if the registrant may be deleted; otherwise, <c>false</c>.</returns>
        private bool CanDeleteRegistrant( RegistrationRegistrant registrant )
        {
            if ( registrant == null )
            {
                return false;
            }

            var currentPerson = RequestContext.CurrentPerson;

            return BlockCache.IsAuthorized( Authorization.EDIT, currentPerson )
                || registrant.Registration?.IsAuthorized( Authorization.REGISTER, currentPerson ) == true
                || registrant.Registration?.IsAuthorized( Authorization.EDIT, currentPerson ) == true
                || registrant.Registration?.IsAuthorized( Authorization.ADMINISTRATE, currentPerson ) == true;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Deletes the specified registrant.
        /// </summary>
        /// <param name="key">The identifier of the registrant to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var registrantService = new RegistrationRegistrantService( RockContext );
            var registrant = registrantService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( registrant == null )
            {
                return ActionBadRequest( $"{RegistrationRegistrant.FriendlyTypeName} not found." );
            }

            if ( !CanDeleteRegistrant( registrant ) )
            {
                return ActionBadRequest( "You are not authorized to delete this registrant." );
            }

            if ( !registrantService.CanDelete( registrant, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            registrantService.Delete( registrant );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Creates an entity set from the selected wait list registrants and
        /// returns the URL of the wait list confirmation page where they can be
        /// moved into full registrations.
        /// </summary>
        /// <param name="keys">The identifiers of the selected registrants.</param>
        /// <returns>The URL to navigate to in order to complete the move.</returns>
        [BlockAction]
        public BlockActionResult MoveToRegistrantList( List<string> keys )
        {
            // Only individuals who can add/edit registrants may move them off the wait list.
            if ( !GetIsAddDeleteEnabled() )
            {
                return ActionForbidden( "You are not authorized to move wait list individuals." );
            }

            var registrationInstance = GetRegistrationInstance();

            if ( registrationInstance == null )
            {
                return ActionBadRequest( "Registration instance not found." );
            }

            if ( keys == null || !keys.Any() )
            {
                return ActionBadRequest( "No wait list individuals were selected." );
            }

            // Resolve the selected keys to registrant identifiers.
            var selectedRegistrantIds = Reflection.GetEntityIdsForEntityType( EntityTypeCache.Get<RegistrationRegistrant>(), keys, !PageCache.Layout.Site.DisablePredictableIds, RockContext )
                .Values
                .ToList();

            // Re-scope to registrants that are actually on this instance's wait list
            // so a forged key cannot pull in registrants from another instance.
            var registrantIds = new RegistrationRegistrantService( RockContext ).Queryable()
                .Where( r =>
                    selectedRegistrantIds.Contains( r.Id )
                    && r.Registration.RegistrationInstanceId == registrationInstance.Id
                    && r.OnWaitList )
                .Select( r => r.Id )
                .ToList();

            if ( !registrantIds.Any() )
            {
                return ActionBadRequest( "No valid wait list individuals were selected." );
            }

            // Create an entity set of the selected registrants that expires after
            // 20 minutes, matching the WebForms behavior.
            var entitySet = new EntitySet
            {
                EntityTypeId = EntityTypeCache.Get<RegistrationRegistrant>().Id,
                ExpireDateTime = RockDateTime.Now.AddMinutes( 20 )
            };

            foreach ( var registrantId in registrantIds )
            {
                entitySet.Items.Add( new EntitySetItem { EntityId = registrantId } );
            }

            var entitySetService = new EntitySetService( RockContext );
            entitySetService.Add( entitySet );
            RockContext.SaveChanges();

            // Return the wait list processing page URL carrying the new entity set id.
            var queryParams = new Dictionary<string, string>
            {
                { PageParameterKey.WaitListSetId, entitySet.Id.ToString() }
            };

            return ActionOk( this.GetLinkedPageUrl( AttributeKey.WaitListProcessingPage, queryParams ) );
        }

        #endregion Block Actions

        #region Support Classes

        /// <summary>
        /// A registration template form field that has been configured to
        /// show on the grid.
        /// </summary>
        private class RegistrantFormFieldInfo
        {
            /// <summary>
            /// Gets or sets the source of the field (person field, registrant
            /// attribute or person attribute).
            /// </summary>
            public RegistrationFieldSource FieldSource { get; set; }

            /// <summary>
            /// Gets or sets the person field type. Only set when
            /// <see cref="FieldSource"/> is a person field.
            /// </summary>
            public RegistrationPersonFieldType? PersonFieldType { get; set; }

            /// <summary>
            /// Gets or sets the attribute. Only set when
            /// <see cref="FieldSource"/> is an attribute source.
            /// </summary>
            public AttributeCache Attribute { get; set; }
        }

        #endregion Support Classes
    }
}
