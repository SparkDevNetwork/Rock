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
using System.Data.SqlClient;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Event.RegistrationInstanceRegistrantList;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Core.Grid;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays the list of Registrants related to a Registration Instance.
    /// </summary>
    [DisplayName( "Registration Instance - Registrant List" )]
    [Category( "Event" )]
    [Description( "Displays the list of Registrants related to a Registration Instance." )]
    [IconCssClass( "ti ti-users" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Registration Page",
        Description = "The page for editing registration and registrant information.",
        Key = AttributeKey.RegistrationPage,
        DefaultValue = Rock.SystemGuid.Page.REGISTRATION_DETAIL,
        IsRequired = false,
        Order = 1 )]

    [LinkedPage(
        "Group Placement Page",
        Description = "The page for managing the registrant's group placements.",
        Key = AttributeKey.GroupPlacementPage,
        DefaultValue = Rock.SystemGuid.Page.GROUP_PLACEMENT + "," + Rock.SystemGuid.PageRoute.GROUP_PLACEMENT,
        IsRequired = false,
        Order = 2 )]

    [LinkedPage(
        "Group Detail Page",
        Description = "The page for viewing details about a group.",
        Key = AttributeKey.GroupDetailPage,
        DefaultValue = Rock.SystemGuid.Page.GROUP_VIEWER,
        IsRequired = true,
        Order = 3 )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]

    [Rock.SystemGuid.EntityTypeGuid( "3B4682FB-30D2-416E-B895-E0B5765D98BD" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "2270724C-64CB-4779-98B4-96DFBC4AA285" )]
    [Rock.SystemGuid.BlockTypeGuid( "4D4FBC7B-068C-499A-8BA4-C9209CA9BB6E" )]
    [CustomizedGrid]
    public class RegistrationInstanceRegistrantList : RockEntityListBlockType<RegistrationRegistrant>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string RegistrationPage = "RegistrationPage";
            public const string GroupPlacementPage = "GroupPlacementPage";
            public const string GroupDetailPage = "GroupDetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string RegistrationPage = "RegistrationPage";
            public const string GroupPlacementPage = "GroupPlacementPage";
            public const string GroupDetailPage = "GroupDetailPage";
        }

        private static class PageParameterKey
        {
            public const string RegistrationInstanceId = "RegistrationInstanceId";
            public const string RegistrationId = "RegistrationId";
            public const string RegistrationTemplatePlacementId = "RegistrationTemplatePlacementId";
            public const string SourcePerson = "SourcePerson";
            public const string GroupId = "GroupId";
            public const string CommunicationId = "CommunicationId";
        }

        private static class PreferenceKey
        {
            public const string FilterDateRange = "filter-date-range";
            public const string FilterInGroup = "filter-in-group";
            public const string FilterSignedDocument = "filter-signed-document";
        }

        #endregion Keys

        #region Fields

        private RegistrationInstance _registrationInstance;
        private bool _hasAttemptedRegistrationInstanceLoad;
        private List<RegistrantFormFieldInfo> _registrantFormFields;
        private List<RegistrationPersonFieldType> _visiblePersonFieldTypes;
        private List<RegistrationTemplatePlacement> _registrationTemplatePlacements;
        private Dictionary<int, List<PlacementGroupInfo>> _placementGroupsByPlacementId = new Dictionary<int, List<PlacementGroupInfo>>();
        private Dictionary<int, Location> _homeAddresses = new Dictionary<int, Location>();
        private Dictionary<int, PhoneNumberLookupResult> _mobilePhoneNumbers = new Dictionary<int, PhoneNumberLookupResult>();
        private Dictionary<int, PhoneNumberLookupResult> _homePhoneNumbers = new Dictionary<int, PhoneNumberLookupResult>();
        private Dictionary<int, PhoneNumberLookupResult> _workPhoneNumbers = new Dictionary<int, PhoneNumberLookupResult>();
        private Dictionary<int, List<string>> _personCampusNames = new Dictionary<int, List<string>>();

        #endregion

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

        /// <summary>
        /// Gets the "In Group" filter from person preferences. True limits the
        /// list to registrants placed in a group, false to registrants not in
        /// a group, and null applies no filter.
        /// </summary>
        private bool? FilterInGroup => BlockPersonPreferences
            .GetValue( MakeKeyUniqueToRegistrationTemplate( PreferenceKey.FilterInGroup ) )
            .AsBooleanOrNull();

        /// <summary>
        /// Gets the "Signed Document" filter from person preferences. True
        /// limits the list to registrants whose required document is signed,
        /// false to registrants without a signed document, and null applies
        /// no filter.
        /// </summary>
        private bool? FilterSignedDocument => BlockPersonPreferences
            .GetValue( MakeKeyUniqueToRegistrationTemplate( PreferenceKey.FilterSignedDocument ) )
            .AsBooleanOrNull();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<RegistrationInstanceRegistrantListOptionsBag>();
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
        private RegistrationInstanceRegistrantListOptionsBag GetBoxOptions()
        {
            var registrationInstance = GetRegistrationInstance();

            var options = new RegistrationInstanceRegistrantListOptionsBag
            {
                IsSignedDocumentColumnVisible = registrationInstance?.RegistrationTemplate?.RequiredSignatureDocumentTemplateId != null,
                IsRegistrarCommunicationVisible = GetIsRegistrarCommunicationAuthorized(),
                VisiblePersonFieldTypes = GetVisiblePersonFieldTypes(),
                Placements = GetRegistrationTemplatePlacements()
                    .Select( placement =>
                    {
                        var iconCssClass = placement.GetIconCssClass();

                        return new RegistrantPlacementConfigBag
                        {
                            Id = placement.Id,
                            Name = placement.Name,
                            IconCssClass = iconCssClass.IsNotNullOrWhiteSpace()
                                ? iconCssClass
                                : "ti ti-users",
                            IsMultiplePlacementAllowed = placement.AllowMultiplePlacements
                        };
                    } )
                    .ToList(),
                MobilePhoneLabel = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() )?.Value,
                HomePhoneLabel = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() )?.Value,
                WorkPhoneLabel = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() )?.Value
            };

            if ( registrationInstance != null )
            {
                options.ExportTitle = $"{registrationInstance.Name} - Registrants";
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
        /// Determines whether the current person may use the "Communicate to
        /// Registrars" action. The action is denied when the site has no
        /// communication page configured.
        /// </summary>
        /// <returns><c>true</c> if the current person may communicate to registrars; otherwise, <c>false</c>.</returns>
        private bool GetIsRegistrarCommunicationAuthorized()
        {
            /*
                6/5/26 - MSE

                The WebForms block pointed the custom action button's route at
                the communication page so the button was only shown to people
                authorized for that page

                https://github.com/SparkDevNetwork/Rock/issues/6455

                The Obsidian grid defines custom actions client side, so the same page security
                is checked here, sent to the component to show or hide the
                action, and enforced again in CreateRegistrarCommunication().

                When no communication page is configured (or the page cannot
                be found), the WebForms grid's CanViewTargetPage() returned
                false and hid the button, so this returns false in those
                cases as well.
            */
            var pageReference = PageCache.Layout.Site.CommunicationPageReference;

            if ( pageReference.PageId <= 0 )
            {
                return false;
            }

            var communicationPage = PageCache.Get( pageReference.PageId );

            return communicationPage != null
                && communicationPage.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
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

            var groupPlacementPageParams = new Dictionary<string, string>
            {
                { PageParameterKey.RegistrationTemplatePlacementId, "((PlacementId))" },
                { PageParameterKey.RegistrationInstanceId, registrationInstance?.IdKey },
                { PageParameterKey.SourcePerson, "((PersonId))" }
            };

            var groupDetailPageParams = new Dictionary<string, string>
            {
                { PageParameterKey.GroupId, "((GroupId))" }
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.RegistrationPage] = this.GetLinkedPageUrl( AttributeKey.RegistrationPage, registrationPageParams ),
                [NavigationUrlKey.GroupPlacementPage] = this.GetLinkedPageUrl( AttributeKey.GroupPlacementPage, groupPlacementPageParams ),
                [NavigationUrlKey.GroupDetailPage] = this.GetLinkedPageUrl( AttributeKey.GroupDetailPage, groupDetailPageParams )
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

            var requiredSignatureDocumentTemplateId = registrationInstance.RegistrationTemplate?.RequiredSignatureDocumentTemplateId;

            var registrantQry = new RegistrationRegistrantService( rockContext ).Queryable()
                .Include( r => r.PersonAlias.Person )
                .Include( r => r.Fees.Select( f => f.RegistrationTemplateFee ) )
                .Include( r => r.GroupMember.Group );

            // The signature document is only read by the signature grid
            // fields, which only exist when the template requires a signature
            // document, so skip the join when it would never be consumed.
            if ( requiredSignatureDocumentTemplateId.HasValue )
            {
                registrantQry = registrantQry.Include( r => r.SignatureDocument );
            }

            var qry = registrantQry
                .AsNoTracking()
                .Where( r =>
                    r.Registration.RegistrationInstanceId == registrationInstance.Id
                    && r.PersonAlias != null
                    && r.PersonAlias.Person != null
                    && !r.OnWaitList );

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

            // Apply the "In Group" filter.
            var inGroup = FilterInGroup;

            if ( inGroup == true )
            {
                qry = qry.Where( r => r.GroupMemberId.HasValue );
            }
            else if ( inGroup == false )
            {
                qry = qry.Where( r => !r.GroupMemberId.HasValue );
            }

            // Apply the "Signed Document" filter. An unexecuted subquery is
            // used so EF generates a single SQL statement instead of a
            // potentially huge WHERE IN list of person identifiers.
            var signedDocumentFilter = FilterSignedDocument;

            if ( requiredSignatureDocumentTemplateId.HasValue && signedDocumentFilter.HasValue )
            {
                var signersPersonIdQry = new SignatureDocumentService( rockContext )
                    .Queryable()
                    .Where( d =>
                        d.SignatureDocumentTemplateId == requiredSignatureDocumentTemplateId.Value
                        && d.Status == SignatureDocumentStatus.Signed
                        && d.BinaryFileId.HasValue
                        && d.AppliesToPersonAlias != null )
                    .Select( d => d.AppliesToPersonAlias.PersonId );

                qry = signedDocumentFilter.Value
                    ? qry.Where( r => signersPersonIdQry.Contains( r.PersonAlias.PersonId ) )
                    : qry.Where( r => !signersPersonIdQry.Contains( r.PersonAlias.PersonId ) );
            }

            return qry;
        }

        /// <inheritdoc/>
        protected override IQueryable<RegistrationRegistrant> GetOrderedListQueryable( IQueryable<RegistrationRegistrant> queryable, RockContext rockContext )
        {
            return queryable
                .OrderBy( r => r.PersonAlias.Person.LastName )
                .ThenBy( r => r.PersonAlias.Person.NickName );
        }

        /// <inheritdoc/>
        protected override List<RegistrationRegistrant> GetListItems( IQueryable<RegistrationRegistrant> queryable, RockContext rockContext )
        {
            rockContext.Database.CommandTimeout = 180;

            var items = queryable.ToList();

            if ( !items.Any() )
            {
                return items;
            }

            var personIds = items
                .Where( r => r.PersonAlias != null )
                .Select( r => r.PersonAlias.PersonId )
                .Distinct()
                .ToList();

            /*
                6/5/26 - MSE

                The lookups below pass the person ids as a dbo.IdList
                table-valued parameter so each runs as one round trip with one
                cached plan at any registrant count, unlike Contains() lists
                or subqueries that re-run this method's filtered query.
                Addresses load on every request because exports happen client
                side and the export-only street columns need them present.

                Reason: TVP lookups keep the per-person queries cheap; exports need addresses.
            */
            _homeAddresses = GetHomeAddressLookup( rockContext, personIds );

            var visiblePersonFieldTypes = GetVisiblePersonFieldTypes();

            if ( visiblePersonFieldTypes.Contains( RegistrationPersonFieldType.MobilePhone ) )
            {
                _mobilePhoneNumbers = GetPhoneNumberLookup( rockContext, personIds, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
            }

            if ( visiblePersonFieldTypes.Contains( RegistrationPersonFieldType.HomePhone ) )
            {
                _homePhoneNumbers = GetPhoneNumberLookup( rockContext, personIds, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
            }

            if ( visiblePersonFieldTypes.Contains( RegistrationPersonFieldType.WorkPhone ) )
            {
                _workPhoneNumbers = GetPhoneNumberLookup( rockContext, personIds, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK );
            }

            if ( visiblePersonFieldTypes.Contains( RegistrationPersonFieldType.Campus ) )
            {
                LoadPersonCampusNames( rockContext, personIds );
            }

            LoadPlacementGroupInfo( rockContext );
            LoadAdditionalEntityAttributes( rockContext, items );

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
            var registrationInstance = GetRegistrationInstance();
            var registrationTemplateName = registrationInstance?.RegistrationTemplate?.Name;

            var builder = new GridBuilder<RegistrationRegistrant>()
                .WithBlock( this )
                .AddTextField( "idKey", r => r.IdKey )
                .AddTextField( "registrantName", r => r.PersonAlias?.Person?.FullNameReversed )
                .AddTextField( "legalFirstName", r => GetSearchableLegalFirstName( r.PersonAlias?.Person ) )
                .AddTextField( "firstName", r => r.PersonAlias?.Person?.NickName )
                .AddTextField( "lastName", r => r.PersonAlias?.Person?.LastName )
                .AddTextField( "personIdKey", r => r.PersonAlias?.Person?.IdKey )
                .AddTextField( "registrationIdKey", r => IdHasher.Instance.GetHash( r.RegistrationId ) )
                .AddField( "registrantId", r => r.Id )
                .AddTextField( "groupName", r => r.GroupMember?.Group?.Name )
                .AddField( "groupId", r => r.GroupMember?.GroupId )
                .AddField( "fees", GetFeeSummaries )
                .AddDateTimeField( "createdDateTime", r => r.CreatedDateTime )
                .AddTextField( "street1", r => GetHomeLocation( r )?.Street1 )
                .AddTextField( "street2", r => GetHomeLocation( r )?.Street2 )
                .AddTextField( "city", r => GetHomeLocation( r )?.City )
                .AddTextField( "state", r => GetHomeLocation( r )?.State )
                .AddTextField( "postalCode", r => GetHomeLocation( r )?.PostalCode )
                .AddTextField( "country", r => GetHomeLocation( r )?.Country );

            if ( GetRegistrationTemplatePlacements().Any() )
            {
                // The person identifier is only consumed when building the
                // placement page links, so only emit it alongside them.
                builder
                    .AddField( "personId", r => r.PersonAlias?.PersonId )
                    .AddField( "placements", GetRegistrantPlacements );
            }

            if ( registrationInstance?.RegistrationTemplate?.RequiredSignatureDocumentTemplateId != null )
            {
                builder
                    .AddTextField( "signedDocumentUrl", GetSignedDocumentUrl )
                    .AddTextField( "signatureNotSignedMessage", r => GetSignatureNotSignedMessage( r, registrationTemplateName ) );
            }

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
                        builder
                            .AddDateTimeField( "anniversaryDate", r => r.PersonAlias?.Person?.AnniversaryDate )
                            .AddTextField( "anniversaryDateAge", r => GetFormattedAgeText( r.PersonAlias?.Person?.AnniversaryDate ) );
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
        /// template's forms.
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

                    case RegistrationFieldSource.GroupMemberAttribute:
                        AddEntityAttributeField( builder, attribute, r => r.GroupMember );
                        break;
                }
            }
        }

        /// <summary>
        /// Adds an attribute value field to the grid, reading values from the
        /// entity returned by <paramref name="selector"/>.
        /// </summary>
        /// <remarks>
        /// This grid mixes attributes from three entity types (registrant,
        /// person and group member), so field names include the attribute
        /// identifier to stay unique when the same attribute key exists on
        /// more than one source. The selector result may be null (e.g. a
        /// registrant without a group member), which produces an empty cell.
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
        /// Gets the formatted fee summary lines for a registrant, one line
        /// per fee in the form "2 Shirts ($30.00)".
        /// </summary>
        /// <param name="registrant">The registrant.</param>
        /// <returns>The list of formatted fee descriptions.</returns>
        private static List<string> GetFeeSummaries( RegistrationRegistrant registrant )
        {
            if ( registrant.Fees == null )
            {
                return new List<string>();
            }

            return registrant.Fees
                .Select( fee =>
                {
                    var quantityPrefix = fee.Quantity > 1 ? $"{fee.Quantity:N0} " : string.Empty;
                    var feeName = fee.Quantity > 1
                        ? fee.RegistrationTemplateFee?.Name.Pluralize()
                        : fee.RegistrationTemplateFee?.Name;

                    return $"{quantityPrefix}{feeName} ({fee.Cost.FormatAsCurrency()})";
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the placement state of the registrant for each placement
        /// configured on the registration template.
        /// </summary>
        /// <param name="registrant">The registrant.</param>
        /// <returns>One entry per configured placement.</returns>
        private List<RegistrantPlacementBag> GetRegistrantPlacements( RegistrationRegistrant registrant )
        {
            var personId = registrant.PersonAlias?.PersonId;

            return GetRegistrationTemplatePlacements()
                .Select( placement =>
                {
                    var placedGroupNames = personId.HasValue && _placementGroupsByPlacementId.TryGetValue( placement.Id, out var placementGroups )
                        ? placementGroups
                            .Where( g => g.PersonIds.Contains( personId.Value ) )
                            .Select( g => g.GroupName )
                            .ToList()
                        : new List<string>();

                    return new RegistrantPlacementBag
                    {
                        PlacementId = placement.Id,
                        GroupCount = placedGroupNames.Count,
                        GroupNames = placedGroupNames
                    };
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the URL of the registrant's signed document file, or null if
        /// the required document has not been signed.
        /// </summary>
        /// <param name="registrant">The registrant.</param>
        /// <returns>The file URL or null.</returns>
        private static string GetSignedDocumentUrl( RegistrationRegistrant registrant )
        {
            var document = registrant.SignatureDocument;

            if ( document?.Status == SignatureDocumentStatus.Signed && document.BinaryFileId.HasValue )
            {
                return FileUrlHelper.GetFileUrl( document.BinaryFileId.Value );
            }

            return null;
        }

        /// <summary>
        /// Gets the message describing why the registrant's required document
        /// is not yet signed, or null when the document has been signed.
        /// </summary>
        /// <param name="registrant">The registrant.</param>
        /// <param name="registrationTemplateName">The name of the registration template, used in the message text.</param>
        /// <returns>The not-signed message or null.</returns>
        private static string GetSignatureNotSignedMessage( RegistrationRegistrant registrant, string registrationTemplateName )
        {
            var document = registrant.SignatureDocument;

            if ( document == null )
            {
                return "Document not signed";
            }

            if ( document.Status == SignatureDocumentStatus.Signed )
            {
                /*
                    6/10/26 - MSE

                    A signed document with no BinaryFileId cannot be linked, so
                    GetSignedDocumentUrl returns null for it. Without a message
                    here the cell would fall through to the unsigned icon with an
                    empty tooltip, so explain the anomaly instead. Keep this
                    predicate in sync with GetSignedDocumentUrl.

                    Reason: Avoid a blank tooltip on a signed-but-fileless document.
                */
                if ( !document.BinaryFileId.HasValue )
                {
                    return $"A signed {registrationTemplateName} document was received for {registrant.NickName}, but the document file is missing.";
                }

                return null;
            }

            if ( document.LastInviteDate.HasValue )
            {
                return $"A signed {registrationTemplateName} document has not yet been received for {registrant.NickName}. The last request was sent {document.LastInviteDate.Value.ToElapsedString()}.";
            }

            return $"The required {registrationTemplateName} document has not yet been sent to {registrant.NickName} for signing.";
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
        private static string GetPhoneNumberDisplay( Dictionary<int, PhoneNumberLookupResult> phoneNumbers, RegistrationRegistrant registrant )
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
        /// Gets the person's legal first name when it differs from the nick
        /// name shown in the registrant column, so name searches can match
        /// the legal first name.
        /// </summary>
        /// <param name="person">The registrant's person.</param>
        /// <returns>The legal first name, or null when it would duplicate the displayed name.</returns>
        private static string GetSearchableLegalFirstName( Person person )
        {
            if ( person == null || person.FirstName.IsNullOrWhiteSpace() )
            {
                return null;
            }

            /*
                6/5/26 - MSE

                The WebForms First Name filter matched the legal first name in
                addition to the nick name, but the registrant column only
                displays the nick name. Emitting the legal first name, and
                only when it differs so the grid payload stays near zero, lets
                the component include it in the column's filter values.

                Reason: Name searches must match the legal first name like WebForms did.
            */
            return person.FirstName.Equals( person.NickName, StringComparison.OrdinalIgnoreCase )
                ? null
                : person.FirstName;
        }

        /// <summary>
        /// Gets the formatted age text (e.g. "35 yrs") for a past date, used
        /// as the suffix of the Birthdate and Anniversary Date columns.
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
        /// Gets a lookup of one phone number of the given type per person.
        /// </summary>
        /// <param name="rockContext">The database context.</param>
        /// <param name="personIds">The person identifiers to include.</param>
        /// <param name="phoneTypeValueGuid">The defined value unique identifier of the phone type.</param>
        /// <returns>A dictionary of phone numbers keyed by person identifier.</returns>
        private static Dictionary<int, PhoneNumberLookupResult> GetPhoneNumberLookup( RockContext rockContext, List<int> personIds, string phoneTypeValueGuid )
        {
            var phoneTypeValueId = DefinedValueCache.Get( phoneTypeValueGuid.AsGuid() )?.Id;

            if ( !phoneTypeValueId.HasValue )
            {
                return new Dictionary<int, PhoneNumberLookupResult>();
            }

            var phoneNumbers = rockContext.Database.SqlQuery<PhoneNumberLookupResult>( @"
SELECT
    [pn].[Id],
    [pn].[PersonId],
    [pn].[IsUnlisted],
    [pn].[NumberFormatted]
FROM [PhoneNumber] AS [pn]
INNER JOIN @PersonIds AS [personId] ON [personId].[Id] = [pn].[PersonId]
WHERE [pn].[NumberTypeValueId] = @PhoneTypeValueId",
                personIds.ConvertToIdListParameter( "@PersonIds" ),
                new SqlParameter( "@PhoneTypeValueId", phoneTypeValueId.Value ) )
                .ToList();

            return phoneNumbers
                .GroupBy( pn => pn.PersonId )
                .ToDictionary( g => g.Key, g => g.OrderBy( pn => pn.Id ).First() );
        }

        /// <summary>
        /// Gets a lookup of one home address per person. This intentionally
        /// mirrors the query in <see cref="Person.GetHomeLocations(List{int}, RockContext)"/>
        /// but projects only the address fields the grid reads.
        /// </summary>
        /// <param name="rockContext">The database context.</param>
        /// <param name="personIds">The person identifiers to include.</param>
        /// <returns>A dictionary of home address <see cref="Location"/> values keyed by person identifier.</returns>
        private static Dictionary<int, Location> GetHomeAddressLookup( RockContext rockContext, List<int> personIds )
        {
            /*
                6/5/26 - MSE

                This mirrors Person.GetHomeLocations() but selects only the
                address columns the grid reads, skipping the expensive
                GeoPoint and GeoFence geography columns, into lightweight
                in-memory Location instances. County is included because
                FormattedAddress can resolve a country AddressFormat template
                that references it.

                Reason: Select only the address columns the grid uses.
            */
            var homeAddresses = new Dictionary<int, Location>();
            var homeLocationTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() )?.Id;
            var familyGroupTypeId = GroupTypeCache.GetFamilyGroupType()?.Id;

            if ( !homeLocationTypeValueId.HasValue || !familyGroupTypeId.HasValue )
            {
                return homeAddresses;
            }

            // The IsArchived and IsDeceased predicates mirror the implicit
            // filters that GroupMemberService.Queryable() applies, which the
            // previous LINQ implementation relied on.
            var personLocations = rockContext.Database.SqlQuery<HomeAddressLookupResult>( @"
SELECT
    [gm].[PersonId],
    [address].[LocationId],
    [address].[Street1],
    [address].[Street2],
    [address].[City],
    [address].[County],
    [address].[State],
    [address].[PostalCode],
    [address].[Country]
FROM [GroupMember] AS [gm]
INNER JOIN @PersonIds AS [personId] ON [personId].[Id] = [gm].[PersonId]
INNER JOIN [Group] AS [g] ON [g].[Id] = [gm].[GroupId]
INNER JOIN [Person] AS [p] ON [p].[Id] = [gm].[PersonId]
OUTER APPLY
(
    SELECT TOP 1
        [l].[Id] AS [LocationId],
        [l].[Street1],
        [l].[Street2],
        [l].[City],
        [l].[County],
        [l].[State],
        [l].[PostalCode],
        [l].[Country]
    FROM [GroupLocation] AS [gl]
    INNER JOIN [Location] AS [l] ON [l].[Id] = [gl].[LocationId]
    WHERE [gl].[GroupId] = [g].[Id]
        AND [gl].[GroupLocationTypeValueId] = @HomeLocationTypeValueId
    ORDER BY [gl].[Id]
) AS [address]
WHERE [g].[GroupTypeId] = @FamilyGroupTypeId
    AND [gm].[IsArchived] = 0
    AND [p].[IsDeceased] = 0
ORDER BY [gm].[PersonId], [gm].[Id]",
                personIds.ConvertToIdListParameter( "@PersonIds" ),
                new SqlParameter( "@HomeLocationTypeValueId", homeLocationTypeValueId.Value ),
                new SqlParameter( "@FamilyGroupTypeId", familyGroupTypeId.Value ) )
                .ToList();

            // Mirror Person.GetHomeLocations(): the first row returned for a
            // person wins, and persons whose family has no home address still
            // get a (null) entry.
            foreach ( var personLocation in personLocations )
            {
                if ( homeAddresses.ContainsKey( personLocation.PersonId ) )
                {
                    continue;
                }

                var location = personLocation.LocationId == null
                    ? null
                    : new Location
                    {
                        Street1 = personLocation.Street1,
                        Street2 = personLocation.Street2,
                        City = personLocation.City,
                        County = personLocation.County,
                        State = personLocation.State,
                        PostalCode = personLocation.PostalCode,
                        Country = personLocation.Country
                    };

                homeAddresses.Add( personLocation.PersonId, location );
            }

            return homeAddresses;
        }

        /// <summary>
        /// Loads the lookup of family campus names per person for the campus
        /// column.
        /// </summary>
        /// <param name="rockContext">The database context.</param>
        /// <param name="personIds">The person identifiers to include.</param>
        private void LoadPersonCampusNames( RockContext rockContext, List<int> personIds )
        {
            var familyGroupTypeId = GroupTypeCache.GetFamilyGroupType()?.Id;

            if ( !familyGroupTypeId.HasValue )
            {
                return;
            }

            // The IsArchived and IsDeceased predicates mirror the implicit
            // filters that GroupMemberService.Queryable() applies, which the
            // previous LINQ implementation relied on.
            var personCampusIds = rockContext.Database.SqlQuery<PersonCampusLookupResult>( @"
SELECT DISTINCT
    [gm].[PersonId],
    [g].[CampusId]
FROM [GroupMember] AS [gm]
INNER JOIN @PersonIds AS [personId] ON [personId].[Id] = [gm].[PersonId]
INNER JOIN [Group] AS [g] ON [g].[Id] = [gm].[GroupId]
INNER JOIN [Person] AS [p] ON [p].[Id] = [gm].[PersonId]
WHERE [g].[GroupTypeId] = @FamilyGroupTypeId
    AND [g].[CampusId] IS NOT NULL
    AND [gm].[IsArchived] = 0
    AND [p].[IsDeceased] = 0",
                personIds.ConvertToIdListParameter( "@PersonIds" ),
                new SqlParameter( "@FamilyGroupTypeId", familyGroupTypeId.Value ) )
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
        /// Loads the placement group information (group names and member
        /// person identifiers) for every placement configured on the
        /// registration template. Includes both instance-specific placement
        /// groups and shared template placement groups.
        /// </summary>
        /// <param name="rockContext">The database context.</param>
        private void LoadPlacementGroupInfo( RockContext rockContext )
        {
            var placements = GetRegistrationTemplatePlacements();
            var registrationInstance = GetRegistrationInstance();

            _placementGroupsByPlacementId = new Dictionary<int, List<PlacementGroupInfo>>();

            if ( !placements.Any() || registrationInstance == null )
            {
                return;
            }

            var registrationInstanceService = new RegistrationInstanceService( rockContext );
            var registrationTemplatePlacementService = new RegistrationTemplatePlacementService( rockContext );

            foreach ( var placement in placements )
            {
                var instanceGroupInfo = registrationInstanceService
                    .GetRegistrationInstancePlacementGroupsByPlacement( registrationInstance.Id, placement.Id )
                    .AsNoTracking()
                    .Select( g => new
                    {
                        g.Id,
                        g.Name,
                        PersonIds = g.Members.Select( m => m.PersonId )
                    } )
                    .ToList();

                var templateGroupInfo = registrationTemplatePlacementService
                    .GetRegistrationTemplatePlacementPlacementGroups( placement )
                    .AsNoTracking()
                    .Select( g => new
                    {
                        g.Id,
                        g.Name,
                        PersonIds = g.Members.Select( m => m.PersonId )
                    } )
                    .ToList();

                /*
                    6/5/26 - MSE

                    A group can be attached to a placement both as an instance
                    placement group and as a shared template placement group.
                    The WebForms block counted such a group twice, which
                    inflated the placement button's group count, so the two
                    lists are deduped by group identifier here.

                    Reason: Count a group attached both ways only once.
                */
                var seenGroupIds = new HashSet<int>();
                var placementGroups = new List<PlacementGroupInfo>();

                foreach ( var groupInfo in instanceGroupInfo.Concat( templateGroupInfo ) )
                {
                    if ( !seenGroupIds.Add( groupInfo.Id ) )
                    {
                        continue;
                    }

                    placementGroups.Add( new PlacementGroupInfo
                    {
                        GroupName = groupInfo.Name,
                        PersonIds = new HashSet<int>( groupInfo.PersonIds )
                    } );
                }

                _placementGroupsByPlacementId[placement.Id] = placementGroups;
            }
        }

        /// <summary>
        /// Bulk loads the attribute values for the person and group member
        /// attributes that are shown on the grid. Registrant attribute values
        /// are loaded automatically by the base class.
        /// </summary>
        /// <param name="rockContext">The database context.</param>
        /// <param name="items">The registrants being displayed.</param>
        private void LoadAdditionalEntityAttributes( RockContext rockContext, List<RegistrationRegistrant> items )
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

                Helper.LoadFilteredAttributes( typeof( Person ), persons, rockContext, a => personAttributeIds.Contains( a.Id ) );
            }

            var groupMemberAttributes = GetGridAttributesBySource( RegistrationFieldSource.GroupMemberAttribute );

            if ( groupMemberAttributes.Any() )
            {
                var groupMemberAttributeIds = groupMemberAttributes.Select( a => a.Id ).ToList();
                var groupMembers = items
                    .Select( r => r.GroupMember )
                    .Where( gm => gm != null )
                    .Cast<IHasAttributes>()
                    .ToList();

                Helper.LoadFilteredAttributes( typeof( GroupMember ), groupMembers, rockContext, a => groupMemberAttributeIds.Contains( a.Id ) );
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
        /// excluded because the registrant column already displays the name.
        /// The result is cached for the lifetime of the request.
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
        /// Gets the placements configured on the registration template,
        /// ordered for display. The result is cached for the lifetime of the
        /// request.
        /// </summary>
        /// <returns>The list of registration template placements.</returns>
        private List<RegistrationTemplatePlacement> GetRegistrationTemplatePlacements()
        {
            if ( _registrationTemplatePlacements != null )
            {
                return _registrationTemplatePlacements;
            }

            var registrationTemplateId = GetRegistrationInstance()?.RegistrationTemplateId;

            if ( !registrationTemplateId.HasValue )
            {
                _registrationTemplatePlacements = new List<RegistrationTemplatePlacement>();
                return _registrationTemplatePlacements;
            }

            _registrationTemplatePlacements = new RegistrationTemplatePlacementService( RockContext )
                .Queryable().AsNoTracking()
                .Where( p => p.RegistrationTemplateId == registrationTemplateId.Value )
                .ToList()
                .OrderBy( p => p.Order )
                .ThenBy( p => p.Name )
                .ToList();

            return _registrationTemplatePlacements;
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
            // half-dozen callers do not each re-query when the parameter is
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
        /// Determines if the specified registrant may be deleted by the
        /// current person. True when the user has block EDIT or REGISTER /
        /// EDIT / ADMINISTRATE on the registration the registrant belongs to.
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

        /// <summary>
        /// Validates a client-provided page URL before it is stored as a
        /// communication's UrlReferrer. Only a well-formed http(s) URL on the
        /// request's host or a registered Rock site domain is accepted, rebuilt
        /// without user info or fragment. Returns null when the URL should not
        /// be trusted.
        /// </summary>
        /// <param name="pageUrl">The client-provided page URL.</param>
        /// <returns>The validated and normalized URL, or null when it should not be trusted.</returns>
        private string GetValidatedReferrerUrl( string pageUrl )
        {
            if ( pageUrl.IsNullOrWhiteSpace() )
            {
                return null;
            }

            if ( !Uri.TryCreate( pageUrl, UriKind.Absolute, out var pageUri ) )
            {
                return null;
            }

            var isHttp = pageUri.Scheme == Uri.UriSchemeHttp || pageUri.Scheme == Uri.UriSchemeHttps;

            if ( !isHttp )
            {
                return null;
            }

            var isRequestHost = RequestContext.RequestUri != null
                && pageUri.Host.Equals( RequestContext.RequestUri.Host, StringComparison.OrdinalIgnoreCase );

            var isKnownSiteDomain = SiteCache.GetSiteByDomain( pageUri.Host ) != null;

            if ( !isRequestHost && !isKnownSiteDomain )
            {
                return null;
            }

            return pageUri.GetComponents( UriComponents.HttpRequestUrl, UriFormat.UriEscaped );
        }

        #endregion

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
        /// Creates a new bulk communication addressed to the registrars (the
        /// people who registered the selected registrants) and returns the URL
        /// of the communication page where it can be completed.
        /// </summary>
        /// <param name="keys">The identifiers of the selected registrants. An empty list means every registrant currently in the list.</param>
        /// <param name="pageUrl">The URL of the page the request was made from, recorded on the communication.</param>
        /// <returns>The URL to navigate to in order to complete the communication.</returns>
        [BlockAction]
        public BlockActionResult CreateRegistrarCommunication( List<string> keys, string pageUrl )
        {
            if ( !GetIsRegistrarCommunicationAuthorized() )
            {
                return ActionBadRequest( "You are not authorized to create a communication to registrars." );
            }

            var registrationInstance = GetRegistrationInstance();

            if ( registrationInstance == null )
            {
                return ActionBadRequest( "Registration instance not found." );
            }

            List<int> personIds;

            if ( keys != null && keys.Any() )
            {
                var registrantIds = Reflection.GetEntityIdsForEntityType( EntityTypeCache.Get<RegistrationRegistrant>(), keys, !PageCache.Layout.Site.DisablePredictableIds, RockContext )
                    .Values
                    .ToList();

                var registrantService = new RegistrationRegistrantService( RockContext );

                var registrarPersonIds = new HashSet<int>();
                const int chunkSize = 1000;

                for ( var skip = 0; skip < registrantIds.Count; skip += chunkSize )
                {
                    var chunkRegistrantIds = registrantIds.Skip( skip ).Take( chunkSize ).ToList();

                    var chunkRegistrantQry = registrantService
                        .Queryable()
                        .Where( r =>
                            chunkRegistrantIds.Contains( r.Id )
                            && r.Registration.RegistrationInstanceId == registrationInstance.Id
                            && r.PersonAliasId.HasValue );

                    registrarPersonIds.UnionWith( GetRegistrarPersonIds( chunkRegistrantQry, registrationInstance.Id ) );
                }

                personIds = registrarPersonIds.ToList();
            }
            else
            {
                /*
                    6/5/26 - MSE

                    An empty selection means everyone currently in the list.
                    Client-side column and quick filters cannot be applied
                    here, so the component sends the filtered row keys when
                    any of those filters are active and only sends an empty
                    list when the grid is unfiltered. This path therefore only
                    needs to honor the server-side grid-settings filters.

                    The grid-settings queryable is composable, so the registrar
                    expansion runs as a single subquery with no large "IN" list.
                */
                personIds = GetRegistrarPersonIds( GetListQueryable( RockContext ), registrationInstance.Id );
            }

            if ( !personIds.Any() )
            {
                var errorMessage = keys != null && keys.Any()
                    ? "No registrars were found for the selected registrants."
                    : "No registrars were found for the registrants in this list.";

                return ActionBadRequest( errorMessage );
            }

            var communicationService = new CommunicationService( RockContext );
            var communication = new Rock.Model.Communication
            {
                IsBulkCommunication = true,
                Status = CommunicationStatus.Transient,
                SenderPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId
            };

            // Prefer the page URL the client reports, but only when it points
            // at this server, so an arbitrary client value is never stored.
            var urlReferrer = GetValidatedReferrerUrl( pageUrl ) ?? RequestContext.RequestUri?.AbsoluteUri;

            if ( urlReferrer.IsNotNullOrWhiteSpace() )
            {
                communication.UrlReferrer = urlReferrer.TrimForMaxLength( communication, nameof( Rock.Model.Communication.UrlReferrer ) );
            }

            communicationService.Add( communication );

            // Save now so the communication gets an identifier the recipient
            // records and the page URL below can reference.
            RockContext.SaveChanges();

            // Get the primary alias identifiers in chunks to avoid hitting the
            // SQL expression limit when an instance has a very large number of
            // registrations.
            var personAliasService = new PersonAliasService( RockContext );
            var primaryAliasIds = new List<int>( personIds.Count );
            var chunkedPersonIds = personIds.Take( 1000 ).ToList();
            var skipCount = 0;

            while ( chunkedPersonIds.Any() )
            {
                var chunkPersonIds = chunkedPersonIds;
                var chunkAliasIds = personAliasService.Queryable()
                    .Where( a => a.PersonId == a.AliasPersonId && chunkPersonIds.Contains( a.PersonId ) )
                    .Select( a => a.Id )
                    .ToList();

                primaryAliasIds.AddRange( chunkAliasIds );
                skipCount += 1000;
                chunkedPersonIds = personIds.Skip( skipCount ).Take( 1000 ).ToList();
            }

            // BulkInsert bypasses EF change tracking for speed, so the audit
            // values must be set manually.
            var currentDateTime = RockDateTime.Now;
            var currentPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;

            var communicationRecipients = primaryAliasIds
                .Select( aliasId => new CommunicationRecipient
                {
                    CommunicationId = communication.Id,
                    PersonAliasId = aliasId,
                    CreatedByPersonAliasId = currentPersonAliasId,
                    ModifiedByPersonAliasId = currentPersonAliasId,
                    CreatedDateTime = currentDateTime,
                    ModifiedDateTime = currentDateTime
                } )
                .ToList();

            RockContext.BulkInsert( communicationRecipients );

            var pageReference = PageCache.Layout.Site.CommunicationPageReference;
            string communicationUrl;

            if ( pageReference.PageId > 0 )
            {
                pageReference.Parameters.AddOrReplace( PageParameterKey.CommunicationId, communication.Id.ToString() );
                communicationUrl = pageReference.BuildUrl();
            }
            else
            {
                communicationUrl = RequestContext.ResolveRockUrl( $"~/Communication/{communication.Id}" );
            }

            return ActionOk( communicationUrl );
        }

        /// <summary>
        /// Expands a set of registrants to the distinct person identifiers of
        /// the registrars who registered them within the registration instance.
        /// When the same person is a registrant on more than one registration in
        /// the instance, every one of those registrations' registrars is
        /// included.
        /// </summary>
        /// <param name="registrantQry">The registrants to expand. The caller is responsible for keeping any literal "IN" list within this queryable bounded.</param>
        /// <param name="registrationInstanceId">The registration instance the registrars must belong to.</param>
        /// <returns>The distinct registrar person identifiers.</returns>
        private List<int> GetRegistrarPersonIds( IQueryable<RegistrationRegistrant> registrantQry, int registrationInstanceId )
        {
            var personAliasIdQry = registrantQry.Select( r => r.PersonAliasId );

            return new RegistrationRegistrantService( RockContext ).Queryable()
                .Where( r =>
                    r.PersonAliasId.HasValue
                    && personAliasIdQry.Contains( r.PersonAliasId.Value )
                    && r.Registration.RegistrationInstanceId == registrationInstanceId
                    && r.Registration.PersonAlias != null )
                .Select( r => r.Registration.PersonAlias.PersonId )
                .Distinct()
                .ToList();
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// A registration template form field that has been configured to
        /// show on the grid.
        /// </summary>
        private class RegistrantFormFieldInfo
        {
            /// <summary>
            /// Gets or sets the source of the field (person field, registrant
            /// attribute, person attribute or group member attribute).
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

        /// <summary>
        /// The members of a single placement group, used to determine which
        /// groups a registrant has been placed in.
        /// </summary>
        private class PlacementGroupInfo
        {
            /// <summary>
            /// Gets or sets the name of the placement group.
            /// </summary>
            public string GroupName { get; set; }

            /// <summary>
            /// Gets or sets the person identifiers of the group's members.
            /// </summary>
            public HashSet<int> PersonIds { get; set; }
        }

        /// <summary>
        /// The shape of a row returned by the phone number lookup query in
        /// <see cref="GetPhoneNumberLookup(RockContext, List{int}, string)"/>.
        /// </summary>
        private class PhoneNumberLookupResult
        {
            /// <summary>
            /// Gets or sets the phone number identifier, used to pick the
            /// lowest-identifier number when a person has more than one of
            /// the same type.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the person the number belongs to.
            /// </summary>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the number is unlisted.
            /// </summary>
            public bool IsUnlisted { get; set; }

            /// <summary>
            /// Gets or sets the formatted phone number.
            /// </summary>
            public string NumberFormatted { get; set; }
        }

        /// <summary>
        /// The shape of a row returned by the home address lookup query in
        /// <see cref="GetHomeAddressLookup(RockContext, List{int})"/>.
        /// </summary>
        private class HomeAddressLookupResult
        {
            /// <summary>
            /// Gets or sets the identifier of the person the address belongs to.
            /// </summary>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the location identifier. Null when the person's
            /// family has no home address, which mirrors the null Location
            /// entries Person.GetHomeLocations() produces.
            /// </summary>
            public int? LocationId { get; set; }

            /// <summary>
            /// Gets or sets the first street line.
            /// </summary>
            public string Street1 { get; set; }

            /// <summary>
            /// Gets or sets the second street line.
            /// </summary>
            public string Street2 { get; set; }

            /// <summary>
            /// Gets or sets the city.
            /// </summary>
            public string City { get; set; }

            /// <summary>
            /// Gets or sets the county.
            /// </summary>
            public string County { get; set; }

            /// <summary>
            /// Gets or sets the state.
            /// </summary>
            public string State { get; set; }

            /// <summary>
            /// Gets or sets the postal code.
            /// </summary>
            public string PostalCode { get; set; }

            /// <summary>
            /// Gets or sets the country.
            /// </summary>
            public string Country { get; set; }
        }

        /// <summary>
        /// The shape of a row returned by the family campus lookup query in
        /// <see cref="LoadPersonCampusNames(RockContext, List{int})"/>.
        /// </summary>
        private class PersonCampusLookupResult
        {
            /// <summary>
            /// Gets or sets the identifier of the person in the family.
            /// </summary>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the campus identifier of the person's family group.
            /// </summary>
            public int CampusId { get; set; }
        }

        #endregion
    }
}
