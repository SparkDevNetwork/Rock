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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using Rock.Attribute;
using Rock.Badge;
using Rock.ClientService.Core.Category;
using Rock.ClientService.Core.Category.Options;
using Rock.Communication;
using Rock.Data;
using Rock.Enums.Controls;
using Rock.Extension;
using Rock.Field.Types;
using Rock.Financial;
using Rock.Lava;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Utility;
using Rock.Utility.CaptchaApi;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Crm;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;
using Rock.Web.UI.Controls;
using Rock.Workflow;

namespace Rock.Rest.v2
{
    /// <summary>
    /// Provides API endpoints for the Controls controller.
    /// </summary>
    [RoutePrefix( "api/v2/Controls" )]
    [Rock.SystemGuid.RestControllerGuid( "815B51F0-B552-47FD-8915-C653EEDD5B67" )]
    public class ControlsController : ApiControllerBase
    {
        #region Account Picker

        /// <summary>
        /// Gets the accounts that can be displayed in the account picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent the accounts.</returns>
        [HttpPost]
        [System.Web.Http.Route( "AccountPickerGetChildren" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "5052e4a9-8cc3-4937-a2d3-9cfec07ed070" )]
        public IHttpActionResult AccountPickerGetChildren( [FromBody] AccountPickerGetChildrenOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                return Ok( AccountPickerGetChildrenData( options, rockContext ) );
            }
        }

        /// <summary>
        /// Gets the accounts that can be displayed in the account picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <param name="rockContext">DB context.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent the accounts.</returns>
        private List<TreeItemBag> AccountPickerGetChildrenData( AccountPickerGetChildrenOptionsBag options, RockContext rockContext )
        {
            var financialAccountService = new FinancialAccountService( rockContext );

            IQueryable<FinancialAccount> qry;

            if ( options.ParentGuid == Guid.Empty )
            {
                qry = financialAccountService.Queryable().AsNoTracking()
                    .Where( f => f.ParentAccountId.HasValue == false );
            }
            else
            {
                qry = financialAccountService.Queryable().AsNoTracking()
                    .Where( f => f.ParentAccount != null && f.ParentAccount.Guid == options.ParentGuid );
            }

            if ( !options.IncludeInactive )
            {
                qry = qry
                    .Where( f => f.IsActive == true );
            }

            var accountList = qry
                .OrderBy( f => f.Order )
                .ThenBy( f => f.Name )
                .ToList();

            var accountTreeViewItems = accountList
                .Select( a => new TreeItemBag
                {
                    Value = a.Guid.ToString(),
                    Text = HttpUtility.HtmlEncode( options.DisplayPublicName ? a.PublicName : a.Name ),
                    IsActive = a.IsActive,
                    IconCssClass = "fa fa-file-o"
                } ).ToList();

            var resultIds = accountList.Select( f => f.Id ).ToList();

            if ( options.LoadFullTree )
            {
                foreach ( var accountTreeViewItem in accountTreeViewItems )
                {
                    var newOptions = new AccountPickerGetChildrenOptionsBag
                    {
                        DisplayPublicName = options.DisplayPublicName,
                        IncludeInactive = options.IncludeInactive,
                        LoadFullTree = options.LoadFullTree,
                        ParentGuid = new Guid( accountTreeViewItem.Value ),
                        SecurityGrantToken = options.SecurityGrantToken
                    };
                    accountTreeViewItem.Children = AccountPickerGetChildrenData( newOptions, rockContext );
                    int childrenCount = accountTreeViewItem.Children.Count;

                    accountTreeViewItem.HasChildren = childrenCount > 0;
                    accountTreeViewItem.IsFolder = accountTreeViewItem.HasChildren;
                    accountTreeViewItem.ChildCount = childrenCount;

                    if ( !accountTreeViewItem.HasChildren )
                    {
                        accountTreeViewItem.Children = null;
                    }
                }
            }
            else
            {
                var childQry = financialAccountService.Queryable().AsNoTracking()
                    .Where( f =>
                    f.ParentAccountId.HasValue && resultIds.Contains( f.ParentAccountId.Value )
                    );

                if ( !options.IncludeInactive )
                {
                    childQry = childQry.Where( f => f.IsActive == true );
                }

                var childrenList = childQry.Select( f => f.ParentAccount.Guid.ToString() )
                    .ToList();

                foreach ( var accountTreeViewItem in accountTreeViewItems )
                {
                    int childrenCount = 0;
                    childrenCount = ( childrenList?.Count( v => v == accountTreeViewItem.Value ) ).GetValueOrDefault( 0 );

                    accountTreeViewItem.HasChildren = childrenCount > 0;
                    accountTreeViewItem.IsFolder = childrenCount > 0;

                    if ( accountTreeViewItem.HasChildren )
                    {
                        accountTreeViewItem.ChildCount = childrenCount;
                    }
                }
            }

            return accountTreeViewItems;
        }

        /// <summary>
        /// Gets the accounts that can be displayed in the account picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent the accounts.</returns>
        [HttpPost]
        [System.Web.Http.Route( "AccountPickerGetParentGuids" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "007512c6-0147-4683-a3fe-3fdd1da275c2" )]
        public IHttpActionResult AccountPickerGetParentGuids( [FromBody] AccountPickerGetParentGuidsOptionsBag options )
        {
            var results = new HashSet<Guid>();

            foreach ( var guid in options.Guids )
            {
                var result = FinancialAccountCache.Get( guid )?
                    .GetAncestorFinancialAccounts()?
                    .OrderBy( a => 0 )?
                    .Reverse()?
                    .Select( a => a.Guid );

                foreach ( var resultGuid in result )
                {
                    results.Add( resultGuid );
                }
            }

            return Ok( results );
        }

        /// <summary>
        /// Gets the accounts that match the given search terms.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the accounts that match the search.</returns>
        [HttpPost]
        [System.Web.Http.Route( "AccountPickerGetSearchedAccounts" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "69fd94cc-f049-4cee-85d1-13e573e30586" )]
        public IHttpActionResult AccountPickerGetSearchedAccounts( [FromBody] AccountPickerGetSearchedAccountsOptionsBag options )
        {
            IQueryable<FinancialAccount> qry;

            if ( options.SearchTerm.IsNullOrWhiteSpace() )
            {
                return BadRequest( "Search Term is required" );
            }

            using ( var rockContext = new RockContext() )
            {
                var financialAccountService = new FinancialAccountService( rockContext );
                qry = financialAccountService.GetAccountsBySearchTerm( options.SearchTerm );

                if ( !options.IncludeInactive )
                {
                    qry = qry.Where( f => f.IsActive == true );
                }

                var accountList = qry
                    .OrderBy( f => f.Order )
                    .ThenBy( f => f.Name )
                    .ToList()
                    .Select( a => new ListItemBag
                    {
                        Value = a.Guid.ToString(),
                        Text = HttpUtility.HtmlEncode( ( options.DisplayPublicName ? a.PublicName : a.Name ) + ( a.GlCode.IsNotNullOrWhiteSpace() ? $" ({a.GlCode})" : "" ) ),
                        Category = financialAccountService.GetDelimitedAccountHierarchy( a, FinancialAccountService.AccountHierarchyDirection.CurrentAccountToParent )
                    } )
                    .ToList();

                return Ok( accountList );
            }
        }

        /// <summary>
        /// Gets the full account information of the selected accounts for the "preview" view
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the selected accounts.</returns>
        [HttpPost]
        [System.Web.Http.Route( "AccountPickerGetPreviewItems" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "b080e9d6-207a-412d-acf5-d811fdec30a3" )]
        public IHttpActionResult AccountPickerGetPreviewItems( [FromBody] AccountPickerGetPreviewItemsOptionsBag options )
        {
            IQueryable<FinancialAccount> qry;

            if ( options.SelectedGuids.Count == 0 )
            {
                return Ok( new List<ListItemBag>() );
            }

            using ( var rockContext = new RockContext() )
            {
                var financialAccountService = new FinancialAccountService( rockContext );
                qry = financialAccountService.Queryable().AsNoTracking()
                    .Where( f => options.SelectedGuids.Contains( f.Guid ) );

                var accountList = qry
                    .OrderBy( f => f.Order )
                    .ThenBy( f => f.Name )
                    .ToList()
                    .Select( a => new ListItemBag
                    {
                        Value = a.Guid.ToString(),
                        Text = HttpUtility.HtmlEncode( options.DisplayPublicName ? a.PublicName : a.Name ),
                        Category = financialAccountService.GetDelimitedAccountHierarchy( a, FinancialAccountService.AccountHierarchyDirection.CurrentAccountToParent )
                    } )
                    .ToList();

                return Ok( accountList );
            }
        }

        /// <summary>
        /// Gets whether or not to allow account picker to Select All based on how many accounts exist
        /// </summary>
        /// <returns>True if there are few enough accounts</returns>
        [HttpPost]
        [System.Web.Http.Route( "AccountPickerGetAllowSelectAll" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "4a13b6ea-3031-48c2-9cdb-be183ccad9a2" )]
        public IHttpActionResult AccountPickerGetAllowSelectAll()
        {
            using ( var rockContext = new RockContext() )
            {
                var financialAccountService = new FinancialAccountService( rockContext );
                var count = financialAccountService.Queryable().Count();

                return Ok( count < 1500 );
            }
        }

        #endregion

        #region Achievement Type Picker

        /// <summary>
        /// Gets the achievement types that can be displayed in the achievement type picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the achievement types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "AchievementTypePickerGetAchievementTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "F98E3033-C652-4031-94B3-E7C44ECA51AA" )]
        public IHttpActionResult AchievementTypePickerGetAchievementTypes( [FromBody] AchievementTypePickerGetAchievementTypesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = AchievementTypeCache.All( rockContext )
                    .Select( t => new ListItemBag
                    {
                        Value = t.Guid.ToString(),
                        Text = t.Name,
                        Category = t.Category?.Name
                    } )
                    .ToList();

                return Ok( items );
            }
        }

        #endregion

        #region Address Control

        /// <summary>
        /// Validates the given address and returns the string representation of the address
        /// </summary>
        /// <param name="options">Address details to validate</param>
        /// <returns>Validation information and a single string representation of the address</returns>
        [HttpPost]
        [System.Web.Http.Route( "AddressControlGetConfiguration" )]
        [Rock.SystemGuid.RestActionGuid( "b477fb6d-4a35-45ec-ac98-b6b5c3727375" )]
        public IHttpActionResult AddressControlGetConfiguration( [FromBody] AddressControlGetConfigurationOptionsBag options )
        {
            var globalAttributesCache = GlobalAttributesCache.Get();
            var showCountrySelection = globalAttributesCache.GetValue( "SupportInternationalAddresses" ).AsBooleanOrNull() ?? false;

            var orgCountryCode = globalAttributesCache.OrganizationCountry;
            var defaultCountryCode = string.IsNullOrWhiteSpace( orgCountryCode ) ? "US" : orgCountryCode;
            var countryCode = options.CountryCode.IsNullOrWhiteSpace() ? defaultCountryCode : options.CountryCode;

            var orgStateCode = globalAttributesCache.OrganizationState;
            var defaultStateCode = countryCode == orgCountryCode ? orgStateCode : string.Empty;

            // Generate List of Countries
            var countries = new List<ListItemBag>();
            var countryValues = DefinedTypeCache.Get( SystemGuid.DefinedType.LOCATION_COUNTRIES.AsGuid() )
                .DefinedValues
                .OrderBy( v => v.Order )
                .ThenBy( v => v.Value )
                .ToList();

            // Move default country to the top of the list
            if ( !string.IsNullOrWhiteSpace( defaultCountryCode ) )
            {
                var defaultCountry = countryValues
                    .Where( v => v.Value.Equals( defaultCountryCode, StringComparison.OrdinalIgnoreCase ) )
                    .FirstOrDefault();
                if ( defaultCountry != null )
                {
                    countries.Add( new ListItemBag { Text = "Countries", Value = string.Empty } );
                    countries.Add( new ListItemBag { Text = options.UseCountryAbbreviation ? defaultCountry.Value : defaultCountry.Description, Value = defaultCountry.Value } );
                    countries.Add( new ListItemBag { Text = "------------------------", Value = "------------------------" } );
                }
            }

            foreach ( var country in countryValues )
            {
                countries.Add( new ListItemBag { Text = options.UseCountryAbbreviation ? country.Value : country.Description, Value = country.Value } );
            }

            // Generate List of States
            string countryGuid = DefinedTypeCache.Get( new Guid( SystemGuid.DefinedType.LOCATION_COUNTRIES ) )
                .DefinedValues
                .Where( v => v.Value.Equals( countryCode, StringComparison.OrdinalIgnoreCase ) )
                .Select( v => v.Guid )
                .FirstOrDefault()
                .ToString();

            List<ListItemBag> states = null;
            var hasStateList = false;

            if ( countryGuid.IsNotNullOrWhiteSpace() )
            {
                var definedType = DefinedTypeCache.Get( new Guid( SystemGuid.DefinedType.LOCATION_ADDRESS_STATE ) );

                states = definedType
                    .DefinedValues
                    .Where( v =>
                        (
                            v.AttributeValues.ContainsKey( "Country" ) &&
                            v.AttributeValues["Country"] != null &&
                            v.AttributeValues["Country"].Value.Equals( countryGuid, StringComparison.OrdinalIgnoreCase )
                        ) ||
                        (
                            ( !v.AttributeValues.ContainsKey( "Country" ) || v.AttributeValues["Country"] == null ) &&
                            v.Attributes.ContainsKey( "Country" ) &&
                            v.Attributes["Country"].DefaultValue.Equals( countryGuid, StringComparison.OrdinalIgnoreCase )
                        ) )
                    .OrderBy( v => v.Order )
                    .ThenBy( v => v.Value )
                    .Select( v => new ListItemBag { Value = v.Value, Text = v.Value } )
                    .ToList();

                hasStateList = states.Any();
            }

            // Get Labels and Validation Rules
            string cityLabel = null;
            string localityLabel = null;
            string stateLabel = null;
            string postalCodeLabel = null;
            DataEntryRequirementLevelSpecifier addressLine1Requirement = DataEntryRequirementLevelSpecifier.Optional;
            DataEntryRequirementLevelSpecifier addressLine2Requirement = DataEntryRequirementLevelSpecifier.Optional;
            DataEntryRequirementLevelSpecifier cityRequirement = DataEntryRequirementLevelSpecifier.Optional;
            DataEntryRequirementLevelSpecifier localityRequirement = DataEntryRequirementLevelSpecifier.Optional;
            DataEntryRequirementLevelSpecifier stateRequirement = DataEntryRequirementLevelSpecifier.Optional;
            DataEntryRequirementLevelSpecifier postalCodeRequirement = DataEntryRequirementLevelSpecifier.Optional;

            var countryValue = DefinedTypeCache.Get( new Guid( SystemGuid.DefinedType.LOCATION_COUNTRIES ) )
                .DefinedValues
                .Where( v => v.Value.Equals( countryCode, StringComparison.OrdinalIgnoreCase ) )
                .FirstOrDefault();

            if ( countryValue != null )
            {
                cityLabel = countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressCityLabel ).ToStringOrDefault( "City" );
                localityLabel = countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressLocalityLabel ).ToStringOrDefault( "Locality" );
                stateLabel = countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressStateLabel ).ToStringOrDefault( "State" );
                postalCodeLabel = countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressPostalCodeLabel ).ToStringOrDefault( "Postal Code" );

                var requirementField = new DataEntryRequirementLevelFieldType();

                addressLine1Requirement = requirementField.GetDeserializedValue( countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressLine1Requirement ), DataEntryRequirementLevelSpecifier.Optional );
                addressLine2Requirement = requirementField.GetDeserializedValue( countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressLine2Requirement ), DataEntryRequirementLevelSpecifier.Optional );
                cityRequirement = requirementField.GetDeserializedValue( countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressCityRequirement ), DataEntryRequirementLevelSpecifier.Optional );
                localityRequirement = requirementField.GetDeserializedValue( countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressLocalityRequirement ), DataEntryRequirementLevelSpecifier.Optional );
                stateRequirement = requirementField.GetDeserializedValue( countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressStateRequirement ), DataEntryRequirementLevelSpecifier.Optional );
                postalCodeRequirement = requirementField.GetDeserializedValue( countryValue.GetAttributeValue( SystemKey.CountryAttributeKey.AddressPostalCodeRequirement ), DataEntryRequirementLevelSpecifier.Optional );
            }

            return Ok( new AddressControlConfigurationBag
            {
                ShowCountrySelection = showCountrySelection,
                DefaultCountry = defaultCountryCode,
                DefaultState = defaultStateCode,
                Countries = countries,
                States = states,

                HasStateList = hasStateList,
                SelectedCountry = countryCode,

                CityLabel = cityLabel,
                LocalityLabel = localityLabel,
                StateLabel = stateLabel,
                PostalCodeLabel = postalCodeLabel,

                AddressLine1Requirement = ( RequirementLevel ) addressLine1Requirement,
                AddressLine2Requirement = ( RequirementLevel ) addressLine2Requirement,
                CityRequirement = ( RequirementLevel ) cityRequirement,
                LocalityRequirement = ( RequirementLevel ) localityRequirement,
                StateRequirement = ( RequirementLevel ) stateRequirement,
                PostalCodeRequirement = ( RequirementLevel ) postalCodeRequirement,
            } );
        }

        /// <summary>
        /// Validates the given address and returns the string representation of the address
        /// </summary>
        /// <param name="options">Address details to validate</param>
        /// <returns>Validation information and a single string representation of the address</returns>
        [HttpPost]
        [System.Web.Http.Route( "AddressControlValidateAddress" )]
        [Rock.SystemGuid.RestActionGuid( "ff879ea7-07dd-43ec-a5de-26f55e9f073a" )]
        public IHttpActionResult AddressControlValidateAddress( [FromBody] AddressControlValidateAddressOptionsBag options )
        {
            var editedLocation = new Location();
            string errorMessage = null;
            string addressString = null;

            editedLocation.Street1 = options.Street1;
            editedLocation.Street2 = options.Street2;
            editedLocation.City = options.City;
            editedLocation.State = options.State;
            editedLocation.PostalCode = options.PostalCode;
            editedLocation.Country = options.Country.IsNotNullOrWhiteSpace() ? options.Country : "US";

            var locationService = new LocationService( new RockContext() );

            string validationMessage;

            var isValid = LocationService.ValidateLocationAddressRequirements( editedLocation, out validationMessage );

            if ( !isValid )
            {
                errorMessage = validationMessage;
            }
            else
            {
                editedLocation = locationService.Get( editedLocation.Street1, editedLocation.Street2, editedLocation.City, editedLocation.State, editedLocation.County, editedLocation.PostalCode, editedLocation.Country, null );
                addressString = editedLocation.GetFullStreetAddress().ConvertCrLfToHtmlBr();
            }

            return Ok( new AddressControlValidateAddressResultsBag
            {
                ErrorMessage = errorMessage,
                IsValid = isValid,
                AddressString = addressString,
                Address = new AddressControlBag
                {
                    Street1 = editedLocation.Street1,
                    Street2 = editedLocation.Street2,
                    City = editedLocation.City,
                    State = editedLocation.State,
                    PostalCode = editedLocation.PostalCode,
                    Country = editedLocation.Country
                }
            } );
        }

        #endregion

        #region Assessment Type Picker

        /// <summary>
        /// Gets the assessment types that can be displayed in the assessment type picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the assessment types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "AssessmentTypePickerGetAssessmentTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "B47DCE1B-89D7-4DD5-88A7-B3C393D49A7C" )]
        public IHttpActionResult AssessmentTypePickerGetEntityTypes( [FromBody] AssessmentTypePickerGetAssessmentTypesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = AssessmentTypeCache.All( rockContext )
                    .Where( at => options.isInactiveIncluded == true || at.IsActive )
                    .OrderBy( at => at.Title )
                    .ThenBy( at => at.Id )
                    .Select( at => new ListItemBag
                    {
                        Value = at.Guid.ToString(),
                        Text = at.Title
                    } )
                    .ToList();

                return Ok( items );
            }
        }

        #endregion

        #region Asset Storage Provider Picker

        /// <summary>
        /// Gets the asset storage providers that can be displayed in the asset storage provider picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the asset storage providers.</returns>
        [HttpPost]
        [System.Web.Http.Route( "AssetStorageProviderPickerGetAssetStorageProviders" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "665EDE0C-1FEA-4421-B355-4D4F72B7E26E" )]
        public IHttpActionResult AssetStorageProviderPickerGetAssetStorageProviders( [FromBody] AssetStorageProviderPickerGetAssetStorageProvidersOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = AssetStorageProviderCache.All()
                    .Where( a => a.EntityTypeId.HasValue && a.IsActive )
                    .OrderBy( a => a.Name )
                    .ToListItemBagList();

                return Ok( items );
            }
        }

        #endregion

        #region Audit Detail

        /// <summary>
        /// Gets the audit details about the entity.
        /// </summary>
        /// <param name="options">The options that describe which entity to be audited.</param>
        /// <returns>A <see cref="EntityAuditBag"/> that contains the requested information.</returns>
        [HttpPost]
        [Authenticate]
        [System.Web.Http.Route( "AuditDetailGetAuditDetails" )]
        [Rock.SystemGuid.RestActionGuid( "714D83C9-96E4-49D7-81AF-2EED7D5CCD56" )]
        public IHttpActionResult AuditDetailGetAuditDetails( [FromBody] AuditDetailGetAuditDetailsOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                // Get the entity type identifier to use to lookup the entity.
                var entityType = EntityTypeCache.Get( options.EntityTypeGuid )?.GetEntityType();

                if ( entityType == null )
                {
                    return NotFound();
                }

                var entity = Reflection.GetIEntityForEntityType( entityType, options.EntityKey, rockContext ) as IModel;

                if ( entity == null )
                {
                    return NotFound();
                }

                // If the entity can be secured, ensure the person has access to it.
                if ( entity is ISecured securedEntity )
                {
                    var isAuthorized = securedEntity.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson )
                        || grant?.IsAccessGranted( entity, Authorization.VIEW ) == true;

                    if ( !isAuthorized )
                    {
                        return Unauthorized();
                    }
                }

                return Ok( entity.GetEntityAuditBag() );
            }
        }

        #endregion

        #region Badge Component Picker

        /// <summary>
        /// Gets the badge components that can be displayed in the badge component picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the badge components.</returns>
        [HttpPost]
        [System.Web.Http.Route( "BadgeComponentPickerGetBadgeComponents" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "ABDFC10F-BCCC-4AF1-8DB3-88A26862485D" )]
        public IHttpActionResult BadgeComponentPickerGetEntityTypes( [FromBody] BadgeComponentPickerGetBadgeComponentsOptionsBag options )
        {
            var componentsList = GetComponentListItems( "Rock.Badge.BadgeContainer, Rock", ( Component component ) =>
            {
                var badgeComponent = component as BadgeComponent;
                var entityType = EntityTypeCache.Get( options.EntityTypeGuid.GetValueOrDefault() )?.Name;

                return badgeComponent != null && badgeComponent.DoesApplyToEntityType( entityType );
            } );

            return Ok( componentsList );
        }

        #endregion

        #region Badge List

        /// <summary>
        /// Get the rendered badge information for a specific entity.
        /// </summary>
        /// <param name="options">The options that describe which badges to render.</param>
        /// <returns>A collection of <see cref="RenderedBadgeBag"/> objects.</returns>
        [HttpPost]
        [System.Web.Http.Route( "BadgeListGetBadges" )]
        [Rock.SystemGuid.RestActionGuid( "34387B98-BF7E-4000-A28A-24EA08605285" )]
        public IHttpActionResult BadgeListGetBadges( [FromBody] BadgeListGetBadgesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityTypeCache = EntityTypeCache.Get( options.EntityTypeGuid, rockContext );
                var entityType = entityTypeCache?.GetEntityType();
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                // Verify that we found the entity type.
                if ( entityType == null )
                {
                    return BadRequest( "Unknown entity type." );
                }

                // Load the entity and verify we got one.
                var entity = Rock.Reflection.GetIEntityForEntityType( entityType, options.EntityKey );

                if ( entity == null )
                {
                    return NotFound();
                }

                // If the entity can be secured, ensure the person has access to it.
                if ( entity is ISecured securedEntity )
                {
                    var isAuthorized = securedEntity.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson )
                        || grant?.IsAccessGranted( entity, Authorization.VIEW ) == true;

                    if ( !isAuthorized )
                    {
                        return Unauthorized();
                    }
                }

                List<BadgeCache> badges;

                // Load the list of badges that were requested or all badges
                // if no specific badges were requested.
                if ( options.BadgeTypeGuids != null && options.BadgeTypeGuids.Any() )
                {
                    badges = options.BadgeTypeGuids
                        .Select( g => BadgeCache.Get( g ) )
                        .Where( b => b != null )
                        .ToList();
                }
                else
                {
                    badges = BadgeCache.All()
                        .Where( b => !b.EntityTypeId.HasValue || b.EntityTypeId.Value == entityTypeCache.Id )
                        .ToList();
                }

                // Filter out any badges that don't apply to the entity or are not
                // authorized by the person to be viewed.
                badges = badges.Where( b => b.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson )
                        || grant?.IsAccessGranted( b, Authorization.VIEW ) == true )
                    .ToList();

                // Render all the badges and then filter out any that are empty.
                var badgeResults = badges.Select( b => b.RenderBadge( entity ) )
                    .Where( b => b.Html.IsNotNullOrWhiteSpace() || b.JavaScript.IsNotNullOrWhiteSpace() )
                    .ToList();

                return Ok( badgeResults );
            }
        }

        #endregion

        #region Badge Picker

        /// <summary>
        /// Get the list of Badge types for use in a Badge Picker.
        /// </summary>
        /// <returns>A list of badge types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "BadgePickerGetBadges" )]
        [Rock.SystemGuid.RestActionGuid( "34387B98-BF7E-4000-A28A-24EA08605285" )]
        public IHttpActionResult BadgePickerGetBadges( [FromBody] BadgePickerGetBadgesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
                var badges = BadgeCache.All().ToList();

                // Filter out any badges that don't apply to the entity or are not
                // authorized by the person to be viewed.
                var badgeList = badges.Where( b => b.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson )
                        || grant?.IsAccessGranted( b, Authorization.VIEW ) == true )
                    .Select( b => new ListItemBag { Text = b.Name, Value = b.Guid.ToString() } )
                    .ToList();

                return Ok( badgeList );
            }
        }

        #endregion

        #region Binary File Picker

        /// <summary>
        /// Gets the binary files that can be displayed in the binary file picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the binary files.</returns>
        [HttpPost]
        [System.Web.Http.Route( "BinaryFilePickerGetBinaryFiles" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "9E5F190E-91FD-4E50-9F00-8B4F9DBD874C" )]
        public IHttpActionResult BinaryFilePickerGetBinaryFiles( [FromBody] BinaryFilePickerGetBinaryFilesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = new BinaryFileService( new RockContext() )
                    .Queryable()
                    .Where( f => f.BinaryFileType.Guid == options.BinaryFileTypeGuid && !f.IsTemporary )
                    .OrderBy( f => f.FileName )
                    .Select( t => new ListItemBag
                    {
                        Value = t.Guid.ToString(),
                        Text = t.FileName
                    } )
                    .ToList();

                return Ok( items );
            }
        }

        #endregion

        #region Binary File Type Picker

        /// <summary>
        /// Gets the binary file types that can be displayed in the binary file type picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the binary file types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "BinaryFileTypePickerGetBinaryFileTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "C93E5A06-82DE-4475-88B8-B173C03BFB50" )]
        public IHttpActionResult BinaryFileTypePickerGetBinaryFileTypes( [FromBody] BinaryFileTypePickerGetBinaryFileTypesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = new BinaryFileTypeService( rockContext )
                    .Queryable()
                    .OrderBy( f => f.Name )
                    .Select( t => new ListItemBag
                    {
                        Value = t.Guid.ToString(),
                        Text = t.Name
                    } )
                    .ToList();

                return Ok( items );
            }
        }

        #endregion

        #region Block Template Picker

        /// <summary>
        /// Gets the templates that can be displayed in the block template picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="BlockTemplatePickerGetBlockTemplatesResultsBag"/> objects that represent the binary file types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "BlockTemplatePickerGetBlockTemplates" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "f52a9356-9f05-42f4-a568-a2fc4baef2de" )]
        public IHttpActionResult BlockTemplatePickerGetBlockTemplates( [FromBody] BlockTemplatePickerGetBlockTemplatesOptionsBag options )
        {
            if ( !options.TemplateBlockValueGuid.HasValue )
            {
                return BadRequest( "Provide a Template Block Guid" );
            }

            var items = new List<BlockTemplatePickerGetBlockTemplatesResultsBag>();
            var blockTemplateDefinedValue = DefinedValueCache.Get( options.TemplateBlockValueGuid.Value );

            if ( blockTemplateDefinedValue != null )
            {
                var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.TEMPLATE );
                definedType.DefinedValues.LoadAttributes();

                foreach ( var item in definedType.DefinedValues )
                {
                    if ( item.GetAttributeValue( "TemplateBlock" ).AsGuid() == blockTemplateDefinedValue.Guid )
                    {
                        var imageUrl = string.Format( "~/GetImage.ashx?guid={0}", item.GetAttributeValue( "Icon" ).AsGuid() );

                        items.Add( new BlockTemplatePickerGetBlockTemplatesResultsBag { Guid = item.Guid, Name = item.Value, IconUrl = RockRequestContext.ResolveRockUrl( imageUrl ), Template = item.Description } );
                    }
                }

                return Ok( items );
            }

            return BadRequest( "Provided GUID does not match a Template Block" );
        }

        #endregion

        #region Campus Picker

        /// <summary>
        /// Gets the campuses that can be displayed in the campus picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="CampusPickerItemBag"/> objects that represent the binary file types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "CampusPickerGetCampuses" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "3D2E0AF9-9E1A-47BD-A1C5-008B6D2A5B22" )]
        public IHttpActionResult CampusPickerGetCampuses( [FromBody] CampusPickerGetCampusesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                return Ok( GetCampuses( options, rockContext ) );
            }
        }

        private List<CampusPickerItemBag> GetCampuses( CampusPickerGetCampusesOptionsBag options, RockContext rockContext )
        {
            var items = new CampusService( rockContext )
                .Queryable()
                .OrderBy( f => f.Order )
                .ThenBy( f => f.Name )
                .Select( c => new CampusPickerItemBag
                {
                    Value = c.Guid.ToString(),
                    Text = c.Name,
                    IsActive = c.IsActive ?? true,
                    CampusStatus = c.CampusStatusValue.Guid,
                    CampusType = c.CampusTypeValue.Guid
                } )
                .ToList();

            return items;
        }

        #endregion

        #region Campus Account Amount Picker

        /// <summary>
        /// Gets the accounts that can be displayed in the campus account amount picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="CampusPickerItemBag"/> objects that represent the binary file types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "CampusAccountAmountPickerGetAccounts" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "9833fcd3-30cf-4bab-840a-27ee497ebfb8" )]
        public IHttpActionResult CampusAccountAmountPickerGetAccounts( [FromBody] CampusAccountAmountPickerGetAccountsOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                IQueryable<FinancialAccount> accountsQry;
                var financialAccountService = new FinancialAccountService( rockContext );

                if ( options.SelectableAccountGuids.Any() )
                {
                    accountsQry = financialAccountService.GetByGuids( options.SelectableAccountGuids );
                }
                else
                {
                    accountsQry = financialAccountService.Queryable();
                }

                accountsQry = accountsQry.Where( f =>
                    f.IsActive &&
                    f.IsPublic.HasValue &&
                    f.IsPublic.Value &&
                    ( f.StartDate == null || f.StartDate <= RockDateTime.Today ) &&
                    ( f.EndDate == null || f.EndDate >= RockDateTime.Today ) )
                .OrderBy( f => f.Order );

                var accountsList = accountsQry.AsNoTracking().ToList();

                string accountHeaderTemplate = options.AccountHeaderTemplate;
                if ( accountHeaderTemplate.IsNullOrWhiteSpace() )
                {
                    accountHeaderTemplate = "{{ Account.PublicName }}";
                }

                if ( options.OrderBySelectableAccountsIndex )
                {
                    accountsList = accountsList.OrderBy( x => options.SelectableAccountGuids.IndexOf( x.Guid ) ).ToList();
                }

                var items = new List<CampusAccountAmountPickerGetAccountsResultItemBag>();
                var campuses = CampusCache.All();

                foreach ( var account in accountsList )
                {
                    var mergeFields = LavaHelper.GetCommonMergeFields( null, null, new CommonMergeFieldsOptions() );
                    mergeFields.Add( "Account", account );
                    var accountAmountLabel = accountHeaderTemplate.ResolveMergeFields( mergeFields );
                    items.Add( new CampusAccountAmountPickerGetAccountsResultItemBag
                    {
                        Name = accountAmountLabel,
                        Value = account.Guid,
                        CampusAccounts = getCampusAccounts( account, campuses )
                    } );
                }

                return Ok( items );
            }
        }

        private Dictionary<Guid, ListItemBag> getCampusAccounts( FinancialAccount baseAccount, List<CampusCache> campuses )
        {
            var results = new Dictionary<Guid, ListItemBag>();

            foreach ( var campus in campuses )
            {
                results.Add( campus.Guid, GetBestMatchingAccountForCampusFromDisplayedAccount( campus.Id, baseAccount ) );
            }

            return results;
        }

        private ListItemBag GetBestMatchingAccountForCampusFromDisplayedAccount( int campusId, FinancialAccount baseAccount )
        {
            if ( baseAccount.CampusId.HasValue && baseAccount.CampusId == campusId )
            {
                // displayed account is directly associated with selected campusId, so return it
                return GetAccountListItemBag( baseAccount );
            }
            else
            {
                // displayed account doesn't have a campus (or belongs to another campus). Find first active matching child account
                var firstMatchingChildAccount = baseAccount.ChildAccounts.Where( a => a.IsActive ).FirstOrDefault( a => a.CampusId.HasValue && a.CampusId == campusId );
                if ( firstMatchingChildAccount != null )
                {
                    // one of the child accounts is associated with the campus so, return the child account
                    return GetAccountListItemBag( firstMatchingChildAccount );
                }
                else
                {
                    // none of the child accounts is associated with the campus so, return the displayed account
                    return GetAccountListItemBag( baseAccount );
                }
            }
        }

        private ListItemBag GetAccountListItemBag( FinancialAccount account )
        {
            return new ListItemBag
            {
                Text = account.Name,
                Value = account.Guid.ToString()
            };
        }

        #endregion

        #region Captcha

        /// <summary>
        /// Gets saved captcha Site Key
        /// </summary>
        [HttpPost]
        [System.Web.Http.Route("CaptchaControlGetConfiguration")]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid("9e066058-13d9-4b4d-8457-07ba8e2cacd3")]
        public IHttpActionResult CaptchaControlGetConfiguration()
        {
            var bag = new CaptchaControlConfigurationBag()
            {
                SiteKey = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.CAPTCHA_SITE_KEY )
            };

            return Ok( bag );
        }

        /// <summary>
        /// Gets saved captcha Site Key
        /// </summary>
        [HttpPost]
        [System.Web.Http.Route( "CaptchaControlValidateToken" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "8f373592-d745-4d69-944a-729e15c3f941" )]
        public IHttpActionResult CaptchaControlValidateToken( [FromBody] CaptchaControlValidateTokenOptionsBag options )
        {
            var api = new CloudflareApi();

            var isTokenValid = api.IsTurnstileTokenValid( options.Token );

            var result = new CaptchaControlTokenValidateTokenResultBag()
            {
                IsTokenValid = isTokenValid
            };

            return Ok( result );
        }

        #endregion

        #region Categorized Value Picker

        /// <summary>
        /// Gets the child items that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A collection of view models that represent the tree items.</returns>
        [HttpPost]
        [System.Web.Http.Route( "CategorizedValuePickerGetTree" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "9294f070-e8c8-48da-bd50-076f26200d75" )]
        public IHttpActionResult CategorizedValuePickerGetTree( [FromBody] CategorizedValuePickerGetTreeOptionsBag options )
        {
            // NO Parent -> get roots using DefinedTypeGuid
            // Parent -> get children of ParentGuid
            // Eliminate values not in the LimitTo list

            if ( options.DefinedTypeGuid == null )
            {
                return BadRequest( "Please provide a Defined Type GUID" );
            }

            // Get the Defined Type and associated values.
            var definedType = DefinedTypeCache.Get( options.DefinedTypeGuid );

            if ( definedType == null || !definedType.IsActive )
            {
                return BadRequest( "Please provide a valid Defined Type GUID" );
            }

            using ( var rockContext = new RockContext() )
            {
                var definedValueService = new DefinedValueService( rockContext );
                var definedValues = definedValueService.GetByDefinedTypeGuid( options.DefinedTypeGuid )
                    .Where( x => x.IsActive )
                    .OrderBy( x => x.Order )
                    .ToList();

                // Filter the selectable values.
                if ( options.OnlyIncludeGuids != null && options.OnlyIncludeGuids.Any() )
                {
                    definedValues = definedValues.Where( x => options.OnlyIncludeGuids.Contains( x.Guid ) ).ToList();
                }

                if ( !definedValues.Any() )
                {
                    return NotFound();
                }

                // Get a list of the Categories associated with the Defined Values.
                var categories = new Dictionary<int, Category>();
                var definedValueCategoryIdList = new List<int>();

                foreach ( var definedValue in definedValues )
                {
                    if ( definedValue.CategoryId != null )
                    {
                        if ( !definedValueCategoryIdList.Contains( definedValue.CategoryId.Value ) )
                        {
                            definedValueCategoryIdList.Add( definedValue.CategoryId.Value );
                        }
                    }
                }

                // Retrieve the Category details, including any parent categories required to build the selection tree.
                var categoryService = new CategoryService( rockContext );

                foreach ( var categoryId in definedValueCategoryIdList )
                {
                    // If this category already exists in the categories list, ignore it as an ancestor of a previous category.
                    if ( categories.ContainsKey( categoryId ) )
                    {
                        continue;
                    }

                    var ancestors = categoryService.GetAllAncestors( categoryId ).ToList();
                    foreach ( var ancestor in ancestors )
                    {
                        if ( !categories.ContainsKey( ancestor.Id ) )
                        {
                            categories.Add( ancestor.Id, ancestor );
                        }
                    }
                }

                var categoryItems = new List<CategorizedValuePickerNodeBag>();

                // Create a selection tree structure from the Categories.
                // Categories are created with a placeholder label which will be replaced by applying the naming rules.
                foreach ( var category in categories.Values )
                {
                    var listItem = new CategorizedValuePickerNodeBag
                    {
                        Value = category.Guid.ToString(),
                        Text = category.Name,
                        ChildCategories = new List<CategorizedValuePickerNodeBag>(),
                        ChildValues = new List<CategorizedValuePickerNodeBag>()
                    };

                    categoryItems.Add( listItem );
                }

                var root = new CategorizedValuePickerNodeBag
                {
                    Value = null,
                    Text = definedType.Name,
                    ChildCategories = new List<CategorizedValuePickerNodeBag>(),
                    ChildValues = new List<CategorizedValuePickerNodeBag>()
                };

                // Go through the categories and add child categories as children of their parents
                foreach ( var category in categories.Values )
                {
                    var listItem = categoryItems.Find( c => c.Value == category.Guid.ToString() );

                    // No parent? Throw it at the root of the list
                    if ( category.ParentCategory == null )
                    {
                        root.ChildCategories.Add( listItem );
                    }
                    // Has a parent. Add it as a child of its parent
                    else
                    {
                        var parent = categoryItems.Find( c => c.Value == category.ParentCategory.Guid.ToString() );

                        parent.ChildCategories.Add( listItem );
                    }
                }

                // Go through the defined values and add them as children of their categories
                foreach ( var definedValue in definedValues )
                {
                    var listItem = new CategorizedValuePickerNodeBag
                    {
                        Value = definedValue.Guid.ToString(),
                        Text = definedValue.Value
                    };

                    // No category? Throw it at the root of the list
                    if ( definedValue.Category == null )
                    {
                        AddDefinedValueToCategoryAndChildCategories( listItem, root );
                    }
                    // Has a category. Add it as a child of its category
                    else
                    {
                        var category = categoryItems.Find( c => c.Value == definedValue.Category.Guid.ToString() );
                        AddDefinedValueToCategoryAndChildCategories( listItem, category );
                    }
                }

                return Ok( new CategorizedValuePickerGetTreeResultsBag
                {
                    Tree = root,
                    DefinedType = definedType.Name
                } );
            }
        }

        /// <summary>
        /// Adds the defined value to its category and all child categories of that category. It's added to the children
        /// to facilitate the picker showing values from ancestors.
        /// </summary>
        /// <param name="definedValue">The defined value.</param>
        /// <param name="category">The category node.</param>
        private void AddDefinedValueToCategoryAndChildCategories( CategorizedValuePickerNodeBag definedValue, CategorizedValuePickerNodeBag category )
        {
            category.ChildValues.Add( definedValue );
            foreach ( var childCat in category.ChildCategories )
            {
                AddDefinedValueToCategoryAndChildCategories( definedValue, childCat );
            }
        }

        #endregion

        #region Category Picker

        private static readonly Regex QualifierValueLookupRegex = new Regex( "^{EL:((?:[a-f\\d]{8})-(?:[a-f\\d]{4})-(?:[a-f\\d]{4})-(?:[a-f\\d]{4})-(?:[a-f\\d]{12})):((?:[a-f\\d]{8})-(?:[a-f\\d]{4})-(?:[a-f\\d]{4})-(?:[a-f\\d]{4})-(?:[a-f\\d]{12}))}$", RegexOptions.IgnoreCase );

        /// <summary>
        /// Gets the child items that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A collection of view models that represent the tree items.</returns>
        [HttpPost]
        [System.Web.Http.Route( "CategoryPickerChildTreeItems" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "A1D07211-6C50-463B-98ED-1622DC4D73DD" )]
        public IHttpActionResult CategoryPickerChildTreeItems( [FromBody] CategoryPickerChildTreeItemsOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = new CategoryClientService( rockContext, GetPerson( rockContext ) );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                var items = clientService.GetCategorizedTreeItems( new CategoryItemTreeOptions
                {
                    ParentGuid = options.ParentGuid,
                    GetCategorizedItems = options.GetCategorizedItems,
                    EntityTypeGuid = options.EntityTypeGuid,
                    EntityTypeQualifierColumn = options.EntityTypeQualifierColumn,
                    EntityTypeQualifierValue = GetQualifierValueLookupResult( options.EntityTypeQualifierValue, rockContext ),
                    IncludeUnnamedEntityItems = options.IncludeUnnamedEntityItems,
                    IncludeCategoriesWithoutChildren = options.IncludeCategoriesWithoutChildren,
                    DefaultIconCssClass = options.DefaultIconCssClass,
                    IncludeInactiveItems = options.IncludeInactiveItems,
                    ItemFilterPropertyName = options.ItemFilterPropertyName,
                    ItemFilterPropertyValue = options.ItemFilterPropertyValue,
                    LazyLoad = options.LazyLoad,
                    SecurityGrant = grant
                } );

                return Ok( items );
            }
        }

        /// <summary>
        /// Checks if the qualifier value is a lookup and if so translate it to the
        /// identifier from the unique identifier. Otherwise returns the original
        /// value.
        /// </summary>
        /// <remarks>
        /// At some point this needs to be moved into a ClientService layer, but
        /// I'm not sure where since it isn't related to any one service.
        /// </remarks>
        /// <param name="value">The value to be translated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The qualifier value to use.</returns>
        private static string GetQualifierValueLookupResult( string value, RockContext rockContext )
        {
            if ( value == null )
            {
                return null;
            }

            var m = QualifierValueLookupRegex.Match( value );

            if ( m.Success )
            {
                int? id = null;

                if ( Guid.TryParse( m.Groups[1].Value, out var g1 ) && Guid.TryParse( m.Groups[2].Value, out var g2 ) )
                {
                    id = Rock.Reflection.GetEntityIdForEntityType( g1, g2, rockContext );
                }

                return id?.ToString() ?? "0";
            }
            else
            {
                return value;
            }
        }

        #endregion

        #region Component Picker

        /// <summary>
        /// Gets the components that can be displayed in the component picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the components.</returns>
        [HttpPost]
        [System.Web.Http.Route( "ComponentPickerGetComponents" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "75DA0671-38E2-4FF9-B334-CC0C88B559D0" )]
        public IHttpActionResult ComponentPickerGetEntityTypes( [FromBody] ComponentPickerGetComponentsOptionsBag options )
        {
            var componentsList = GetComponentListItems( options.ContainerType );

            return Ok( componentsList );
        }

        #endregion

        #region Connection Request Picker

        /// <summary>
        /// Gets the data views and their categories that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which data views to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent a tree of data views.</returns>
        [HttpPost]
        [System.Web.Http.Route( "ConnectionRequestPickerGetChildren" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "5316914b-cf47-4dac-9e10-71767fdf1eb9" )]
        public IHttpActionResult ConnectionRequestPickerGetChildren( [FromBody] ConnectionRequestPickerGetChildrenOptionsBag options )
        {
            var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

            using ( var rockContext = new RockContext() )
            {
                string service = null;

                /*
                 * Determine what type of resource the GUID we received is so we know what types of
                 * children to query for.
                 */
                if ( options.ParentGuid == null )
                {
                    // Get the root Connection Types
                    service = "type";
                }
                else
                {
                    var conOpp = new ConnectionOpportunityService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( op => op.Guid == options.ParentGuid )
                        .ToList()
                        .Where( op => op.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( op, Authorization.VIEW ) == true );

                    if ( conOpp.Any() )
                    {
                        // Get the Connection Requests
                        service = "request";
                    }
                    else
                    {
                        var conType = new ConnectionTypeService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( t => t.Guid == options.ParentGuid )
                            .ToList()
                            .Where( t => t.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( t, Authorization.VIEW ) == true );

                        if ( conType.Any() )
                        {
                            // Get the Connection Opportunities
                            service = "opportunity";
                        }
                    }
                }

                /*
                 * Fetch the children
                 */
                var list = new List<TreeItemBag>();

                if ( service == "type" )
                {
                    // Get the Connection Types
                    var connectionTypes = new ConnectionTypeService( rockContext )
                        .Queryable().AsNoTracking()
                        .OrderBy( ct => ct.Name )
                        .ToList()
                        .Where( ct => ct.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( ct, Authorization.VIEW ) == true );

                    foreach ( var connectionType in connectionTypes )
                    {
                        var item = new TreeItemBag();
                        item.Value = connectionType.Guid.ToString();
                        item.Text = connectionType.Name;
                        item.HasChildren = connectionType.ConnectionOpportunities.Any();
                        item.IconCssClass = connectionType.IconCssClass;
                        list.Add( item );
                    }
                }
                else if ( service == "opportunity" )
                {
                    // Get the Connection Opportunities
                    var opportunities = new ConnectionOpportunityService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( op => op.ConnectionType.Guid == options.ParentGuid )
                        .OrderBy( op => op.Name )
                        .ToList()
                        .Where( op => op.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( op, Authorization.VIEW ) == true );

                    foreach ( var opportunity in opportunities )
                    {
                        var item = new TreeItemBag();
                        item.Value = opportunity.Guid.ToString();
                        item.Text = opportunity.Name;
                        item.HasChildren = opportunity.ConnectionRequests
                            .Any( r =>
                                r.ConnectionState == ConnectionState.Active ||
                                r.ConnectionState == ConnectionState.FutureFollowUp );
                        item.IconCssClass = opportunity.IconCssClass;
                        list.Add( item );
                    }
                }
                else if ( service == "request" )
                {
                    var requests = new ConnectionRequestService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( r =>
                            r.ConnectionOpportunity.Guid == options.ParentGuid &&
                            r.PersonAlias != null &&
                            r.PersonAlias.Person != null )
                        .OrderBy( r => r.PersonAlias.Person.LastName )
                        .ThenBy( r => r.PersonAlias.Person.NickName )
                        .ToList()
                        .Where( op => op.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( op, Authorization.VIEW ) == true );

                    foreach ( var request in requests )
                    {
                        var item = new TreeItemBag();
                        item.Value = request.Guid.ToString();
                        item.Text = request.PersonAlias.Person.FullName;
                        item.HasChildren = false;
                        item.IconCssClass = "fa fa-user";
                        list.Add( item );
                    }
                }
                else
                {
                    // service type wasn't set, so we don't know where to look
                    return NotFound();
                }

                return Ok( list );
            }
        }

        #endregion

        #region Content Channel Item Picker

        /// <summary>
        /// Gets the content channel items that can be displayed in the content channel item picker.
        /// </summary>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent the content channel items.</returns>
        [HttpPost]
        [System.Web.Http.Route( "ContentChannelItemPickerGetContentChannels" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "2182388d-ccae-44df-a0de-597b8d123666" )]
        public IHttpActionResult ContentChannelItemPickerGetContentChannels()
        {
            var contentChannels = ContentChannelCache.All()
                .OrderBy( cc => cc.Name )
                .Select( cc => new ListItemBag { Text = cc.Name, Value = cc.Guid.ToString() } )
                .ToList();

            return Ok( contentChannels );
        }

        /// <summary>
        /// Gets the content channel items that can be displayed in the content channel item picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent the content channel items.</returns>
        [HttpPost]
        [System.Web.Http.Route( "ContentChannelItemPickerGetContentChannelItems" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "e1f6ad6b-c3f5-4a1a-abc2-46726732daee" )]
        public IHttpActionResult ContentChannelItemPickerGetContentChannelItems( [FromBody] ContentChannelItemPickerGetContentChannelItemsOptionsBag options )
        {
            return Ok( ContentChannelItemPickerGetContentChannelItemsForContentChannel( options.ContentChannelGuid, options.ExcludeContentChannelItems ) );
        }

        /// <summary>
        /// Gets the content channel items and content channel information based on a selected content channel item.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>All the data for the selected role, selected type, and all of the content channel items</returns>
        [HttpPost]
        [System.Web.Http.Route( "ContentChannelItemPickerGetAllForContentChannelItem" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "ef6d055f-38b1-4225-b95f-cfe703f4d425" )]
        public IHttpActionResult ContentChannelItemPickerGetAllForContentChannelItem( [FromBody] ContentChannelItemPickerGetAllForContentChannelItemOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                List<Guid> excludeContentChannelItems = options.ExcludeContentChannelItems;

                var contentChannelItemService = new Rock.Model.ContentChannelItemService( rockContext );
                var contentChannelItem = contentChannelItemService.Queryable()
                    .Where( cc => cc.Guid == options.ContentChannelItemGuid )
                    .First();

                var contentChannel = contentChannelItem.ContentChannel;

                var contentChannelItems = ContentChannelItemPickerGetContentChannelItemsForContentChannel( contentChannel.Guid, excludeContentChannelItems, rockContext );

                return Ok( new ContentChannelItemPickerGetAllForContentChannelItemResultsBag
                {
                    SelectedContentChannelItem = new ListItemBag { Text = contentChannelItem.Title, Value = contentChannelItem.Guid.ToString() },
                    SelectedContentChannel = new ListItemBag { Text = contentChannel.Name, Value = contentChannel.Guid.ToString() },
                    ContentChannelItems = contentChannelItems
                } );
            }
        }

        /// <summary>
        /// Gets the content channel items that can be displayed in the content channel item picker.
        /// </summary>
        /// <param name="contentChannelGuid">Load content channel items of this type</param>
        /// <param name="excludeContentChannelItems">Do not include these items in the result</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent the content channel items.</returns>
        private List<ListItemBag> ContentChannelItemPickerGetContentChannelItemsForContentChannel( Guid contentChannelGuid, List<Guid> excludeContentChannelItems )
        {
            using ( var rockContext = new RockContext() )
            {
                return ContentChannelItemPickerGetContentChannelItemsForContentChannel( contentChannelGuid, excludeContentChannelItems, rockContext );
            }
        }

        /// <summary>
        /// Gets the content channel items that can be displayed in the content channel item picker.
        /// </summary>
        /// <param name="contentChannelGuid">Load content channel items of this type</param>
        /// <param name="excludeContentChannelItems">Do not include these items in the result</param>
        /// <param name="rockContext">DB context</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent the content channel items.</returns>
        private List<ListItemBag> ContentChannelItemPickerGetContentChannelItemsForContentChannel( Guid contentChannelGuid, List<Guid> excludeContentChannelItems, RockContext rockContext )
        {
            var contentChannelItemService = new Rock.Model.ContentChannelItemService( rockContext );

            var contentChannelitems = contentChannelItemService.Queryable()
                .Where( r =>
                    r.ContentChannel.Guid == contentChannelGuid &&
                    !excludeContentChannelItems.Contains( r.Guid ) )
                .OrderBy( a => a.Title )
                .Select( r => new ListItemBag { Text = r.Title, Value = r.Guid.ToString() } )
                .ToList();

            return contentChannelitems;
        }

        #endregion

        #region Currency Box

        /// <summary>
        /// Gets the currency info for the currency box matching the given currency code defined value Guid.
        /// </summary>
        /// <returns>The currency symbol and decimal places</returns>
        [HttpPost]
        [System.Web.Http.Route( "CurrencyBoxGetCurrencyInfo" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "6E8D0B48-EB88-4028-B03F-064A690902D4" )]
        public IHttpActionResult CurrencyBoxGetCurrencyInfo( [FromBody] CurrencyBoxGetCurrencyInfoOptionsBag options )
        {
            Guid currencyCodeGuid = options.CurrencyCodeGuid;
            RockCurrencyCodeInfo currencyInfo = null;

            if ( !currencyCodeGuid.IsEmpty() )
            {
                var currencyCodeDefinedValueCache = DefinedValueCache.Get( currencyCodeGuid );
                if ( currencyCodeDefinedValueCache != null )
                {
                    currencyInfo = new RockCurrencyCodeInfo( currencyCodeDefinedValueCache.Id );
                }
            }

            if ( currencyInfo == null )
            {
                currencyInfo = new RockCurrencyCodeInfo();
            }

            return Ok( new CurrencyBoxGetCurrencyInfoResultsBag
            {
                Symbol = currencyInfo.Symbol,
                DecimalPlaces = currencyInfo.DecimalPlaces
            } );
        }

        #endregion

        #region Data View Picker

        /// <summary>
        /// Gets the data views and their categories that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which data views to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent a tree of data views.</returns>
        [HttpPost]
        [System.Web.Http.Route( "DataViewPickerGetDataViews" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "1E079A57-9B44-4365-9C9C-2383A9A3F45B" )]
        public IHttpActionResult DataViewPickerGetDataViews( [FromBody] DataViewPickerGetDataViewsOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = new CategoryClientService( rockContext, GetPerson( rockContext ) );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
                Func<DataView, bool> filterMethod = null;
                if ( options.DisplayPersistedOnly )
                {
                    filterMethod = d => d.IsPersisted();
                }

                var items = clientService.GetCategorizedTreeItems( new CategoryItemTreeOptions
                {
                    ParentGuid = options.ParentGuid,
                    GetCategorizedItems = options.GetCategorizedItems,
                    EntityTypeGuid = EntityTypeCache.Get<Rock.Model.DataView>().Guid,
                    IncludeUnnamedEntityItems = options.IncludeUnnamedEntityItems,
                    IncludeCategoriesWithoutChildren = options.IncludeCategoriesWithoutChildren,
                    DefaultIconCssClass = options.DefaultIconCssClass,
                    ItemFilterPropertyName = options.EntityTypeGuidFilter.HasValue ? "EntityTypeId" : null,
                    ItemFilterPropertyValue = options.EntityTypeGuidFilter.HasValue ? EntityTypeCache.GetId( options.EntityTypeGuidFilter.Value ).ToString() : "",
                    LazyLoad = options.LazyLoad,
                    SecurityGrant = grant
                }, filterMethod );

                return Ok( items );
            }
        }

        #endregion

        #region Defined Value Picker

        /// <summary>
        /// Gets the defined values and their categories that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which defined values to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent a tree of defined values.</returns>
        [HttpPost]
        [System.Web.Http.Route( "DefinedValuePickerGetDefinedValues" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "1E4A1812-8A2C-4266-8F39-3004C1DEBC9F" )]
        public IHttpActionResult DefinedValuePickerGetDefinedValues( DefinedValuePickerGetDefinedValuesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var definedType = DefinedTypeCache.Get( options.DefinedTypeGuid );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                if ( definedType == null || !definedType.IsAuthorized( Rock.Security.Authorization.VIEW, RockRequestContext.CurrentPerson ) )
                {
                    return NotFound();
                }

                var definedValues = definedType.DefinedValues
                    .Where( v => ( v.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( v, Authorization.VIEW ) == true )
                        && ( options.IncludeInactive || v.IsActive ) )
                    .OrderBy( v => v.Order )
                    .ThenBy( v => v.Value )
                    .Select( v => new ListItemBag
                    {
                        Value = v.Guid.ToString(),
                        Text = v.Value
                    } )
                    .ToList();

                return Ok( definedValues );
            }
        }

        /// <summary>
        /// Get the attributes for the given Defined Type
        /// </summary>
        /// <param name="options">The options needed to find the attributes for the defined type</param>
        /// <returns>A list of attributes in a form the Attribute Values Container can use</returns>
        [HttpPost]
        [System.Web.Http.Route( "DefinedValuePickerGetAttributes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "10b3fa87-756e-4dde-bf67-fb102037ddc3" )]
        public IHttpActionResult DefinedValuePickerGetAttributes( DefinedValuePickerGetAttributesOptionsBag options )
        {
            if ( RockRequestContext.CurrentPerson == null )
            {
                return Unauthorized();
            }

            var definedType = DefinedTypeCache.Get( options.DefinedTypeGuid );
            var definedValue = new DefinedValue
            {
                Id = 0,
                DefinedTypeId = definedType.Id
            };

            return Ok( GetAttributes( definedValue ) );
        }

        /// <summary>
        /// Save a new Defined Value
        /// </summary>
        /// <param name="options">The options the new defined value</param>
        /// <returns>A <see cref="ListItemBag"/> representing the new defined value.</returns>
        [HttpPost]
        [System.Web.Http.Route( "DefinedValuePickerSaveNewValue" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "2a10eb70-cc9a-48be-8ed7-d9104fd9fdca" )]
        public IHttpActionResult DefinedValuePickerSaveNewValue( DefinedValuePickerSaveNewValueOptionsBag options )
        {
            if ( RockRequestContext.CurrentPerson == null )
            {
                return Unauthorized();
            }

            using ( var rockContext = new RockContext() )
            {
                var definedType = DefinedTypeCache.Get( options.DefinedTypeGuid );
                var definedValue = new DefinedValue
                {
                    Id = 0,
                    DefinedTypeId = definedType.Id,
                    IsSystem = false,
                    Value = options.Value,
                    Description = options.Description
                };

                DefinedValueService definedValueService = new DefinedValueService( rockContext );
                var orders = definedValueService.Queryable()
                    .Where( d => d.DefinedTypeId == definedType.Id )
                    .Select( d => d.Order )
                    .ToList();

                definedValue.Order = orders.Any() ? orders.Max() + 1 : 0;

                // Assign Attributes
                Attribute.Helper.LoadAttributes( definedValue );

                foreach ( KeyValuePair<string, AttributeValueCache> attr in definedValue.AttributeValues )
                {
                    definedValue.AttributeValues[attr.Key].Value = options.AttributeValues.GetValueOrNull( attr.Key );
                }

                if ( !definedValue.IsValid )
                {
                    return InternalServerError();
                }

                // Save the new value
                rockContext.WrapTransaction( () =>
                {
                    if ( definedValue.Id.Equals( 0 ) )
                    {
                        definedValueService.Add( definedValue );
                    }

                    rockContext.SaveChanges();

                    definedValue.SaveAttributeValues( rockContext );
                } );

                return Ok( new ListItemBag { Text = definedValue.Value, Value = definedValue.Guid.ToString() } );
            }
        }

        #endregion

        #region Entity Tag List

        /// <summary>
        /// Gets the tags that are currently specified for the given entity.
        /// </summary>
        /// <param name="options">The options that describe which tags to load.</param>
        /// <returns>A collection of <see cref="EntityTagListTagBag"/> that represent the tags.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EntityTagListGetEntityTags" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "7542D4B3-17DC-4640-ACBD-F02784130401" )]
        public IHttpActionResult EntityTagListGetEntityTags( [FromBody] EntityTagListGetEntityTagsOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );
                var entityGuid = Reflection.GetEntityGuidForEntityType( options.EntityTypeGuid, options.EntityKey, false, rockContext );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                if ( !entityTypeId.HasValue || !entityGuid.HasValue )
                {
                    return NotFound();
                }

                var taggedItemService = new TaggedItemService( rockContext );
                var items = taggedItemService.Get( entityTypeId.Value, options.EntityQualifierColumn, options.EntityQualifierValue, RockRequestContext.CurrentPerson?.Id, entityGuid.Value, options.CategoryGuid, options.ShowInactiveTags )
                    .Include( ti => ti.Tag.Category )
                    .Select( ti => ti.Tag )
                    .ToList()
                    .Where( t => t.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( t, Authorization.VIEW ) == true )
                    .Select( t => GetTagBagFromTag( t ) )
                    .ToList();

                return Ok( items );
            }
        }

        /// <summary>
        /// Gets the tags that are available for the given entity.
        /// </summary>
        /// <param name="options">The options that describe which tags to load.</param>
        /// <returns>A collection of list item bags that represent the tags.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EntityTagListGetAvailableTags" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "91890D39-6E3E-4623-AAD7-F32E686C784E" )]
        public IHttpActionResult EntityTagListGetAvailableTags( [FromBody] EntityTagListGetAvailableTagsOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );
                var entityGuid = Reflection.GetEntityGuidForEntityType( options.EntityTypeGuid, options.EntityKey, false, rockContext );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                if ( !entityTypeId.HasValue || !entityGuid.HasValue )
                {
                    return NotFound();
                }

                var tagService = new TagService( rockContext );
                var items = tagService.Get( entityTypeId.Value, options.EntityQualifierColumn, options.EntityQualifierValue, RockRequestContext.CurrentPerson?.Id, options.CategoryGuid, options.ShowInactiveTags )
                    .Where( t => t.Name.StartsWith( options.Name )
                        && !t.TaggedItems.Any( i => i.EntityGuid == entityGuid ) )
                    .ToList()
                    .Where( t => t.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( t, Authorization.VIEW ) == true )
                    .Select( t => GetTagBagFromTag( t ) )
                    .ToList();

                return Ok( items );
            }
        }

        /// <summary>
        /// Create a new personal tag for the EntityTagList control.
        /// </summary>
        /// <param name="options">The options that describe the tag to be created.</param>
        /// <returns>An instance of <see cref="EntityTagListTagBag"/> that represents the tag.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EntityTagListCreatePersonalTag" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "8CCB7B8D-5D5C-4AA6-A12C-ED062C7AFA05" )]
        public IHttpActionResult EntityTagListCreatePersonalTag( [FromBody] EntityTagListCreatePersonalTagOptionsBag options )
        {
            if ( RockRequestContext.CurrentPerson == null )
            {
                return Unauthorized();
            }

            using ( var rockContext = new RockContext() )
            {
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
                var entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );

                if ( !entityTypeId.HasValue )
                {
                    return NotFound();
                }

                var tagService = new TagService( rockContext );
                var tag = tagService.Get( entityTypeId.Value, options.EntityQualifierColumn, options.EntityQualifierValue, RockRequestContext.CurrentPerson?.Id, options.Name, options.CategoryGuid, true );

                // If the personal tag already exists, use a 409 to indicate
                // it already exists and return the existing tag.
                if ( tag != null && tag.OwnerPersonAliasId.HasValue )
                {
                    // If the personal tag isn't active, make it active.
                    if ( !tag.IsActive )
                    {
                        tag.IsActive = true;
                        System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", RockRequestContext.CurrentPerson );
                        rockContext.SaveChanges();
                    }

                    return Content( System.Net.HttpStatusCode.Conflict, GetTagBagFromTag( tag ) );
                }

                // At this point tag either doesn't exist or was an organization
                // tag so we need to create a new personal tag.
                tag = new Tag
                {
                    EntityTypeId = entityTypeId,
                    OwnerPersonAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( RockRequestContext.CurrentPerson.Id ),
                    Name = options.Name
                };

                if ( options.CategoryGuid.HasValue )
                {
                    var category = new CategoryService( rockContext ).Get( options.CategoryGuid.Value );

                    if ( category == null || ( !category.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) && !grant?.IsAccessGranted( category, Authorization.VIEW ) != true ) )
                    {
                        return NotFound();
                    }

                    // Set the category as well so we can properly convert to a bag.
                    tag.Category = category;
                    tag.CategoryId = category.Id;
                }

                tagService.Add( tag );

                System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", RockRequestContext.CurrentPerson );
                rockContext.SaveChanges();

                return Content( System.Net.HttpStatusCode.Created, GetTagBagFromTag( tag ) );
            }
        }

        /// <summary>
        /// Adds a tag on the given entity.
        /// </summary>
        /// <param name="options">The options that describe the tag and the entity to be tagged.</param>
        /// <returns>An instance of <see cref="EntityTagListTagBag"/> that represents the tag applied to the entity.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EntityTagListAddEntityTag" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "C9CACC7F-68DE-4765-8967-B50EE2949062" )]
        public IHttpActionResult EntityTagListAddEntityTag( [FromBody] EntityTagListAddEntityTagOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );
                var entityGuid = Reflection.GetEntityGuidForEntityType( options.EntityTypeGuid, options.EntityKey, false, rockContext );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                if ( !entityTypeId.HasValue || !entityGuid.HasValue )
                {
                    return NotFound();
                }

                var tagService = new TagService( rockContext );
                var tag = tagService.Get( options.TagKey );

                if ( tag == null || ( !tag.IsAuthorized( Authorization.TAG, RockRequestContext.CurrentPerson ) && grant?.IsAccessGranted( tag, Authorization.VIEW ) != true ) )
                {
                    return NotFound();
                }

                // If the entity is not already tagged, then tag it.
                var taggedItem = tag.TaggedItems.FirstOrDefault( i => i.EntityGuid.Equals( entityGuid ) );

                if ( taggedItem == null )
                {
                    taggedItem = new TaggedItem
                    {
                        Tag = tag,
                        EntityTypeId = entityTypeId.Value,
                        EntityGuid = entityGuid.Value
                    };

                    tag.TaggedItems.Add( taggedItem );

                    System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", RockRequestContext.CurrentPerson );
                    rockContext.SaveChanges();
                }

                return Ok( GetTagBagFromTag( tag ) );
            }
        }

        /// <summary>
        /// Removes a tag from the given entity.
        /// </summary>
        /// <param name="options">The options that describe the tag and the entity to be untagged.</param>
        /// <returns>A response code that indicates success or failure.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EntityTagListRemoveEntityTag" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "6A78D538-87DB-43FE-9150-4E9A3F276AFE" )]
        public IHttpActionResult EntityTagListRemoveEntityTag( [FromBody] EntityTagListRemoveEntityTagOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );
                var entityGuid = Reflection.GetEntityGuidForEntityType( options.EntityTypeGuid, options.EntityKey, false, rockContext );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
                var tagService = new TagService( rockContext );
                var taggedItemService = new TaggedItemService( rockContext );

                if ( !entityTypeId.HasValue || !entityGuid.HasValue )
                {
                    return NotFound();
                }

                var tag = tagService.Get( options.TagKey );

                if ( tag == null || ( !tag.IsAuthorized( Authorization.TAG, RockRequestContext.CurrentPerson ) && grant?.IsAccessGranted( tag, Authorization.VIEW ) != true ) )
                {
                    return NotFound();
                }

                // If the entity is tagged, then untag it.
                var taggedItem = taggedItemService.Queryable()
                    .FirstOrDefault( ti => ti.TagId == tag.Id && ti.EntityGuid == entityGuid.Value );

                if ( taggedItem != null )
                {
                    taggedItemService.Delete( taggedItem );

                    System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", RockRequestContext.CurrentPerson );
                    rockContext.SaveChanges();
                }

                return Ok();
            }
        }

        /// <summary>
        /// Removes a tag from the given entity.
        /// </summary>
        /// <param name="options">The options that describe the tag and the entity to be untagged.</param>
        /// <returns>A response code that indicates success or failure.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EntityTagListSaveTagValues" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "02886e54-6088-40ea-98be-9157ec2a3369" )]
        public IHttpActionResult EntityTagListSaveTagValues( [FromBody] EntityTagListSaveTagValuesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                Person currentPerson = RockRequestContext.CurrentPerson;
                int? currentPersonId = currentPerson.Id;
                var entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );
                var entityGuid = Reflection.GetEntityGuidForEntityType( options.EntityTypeGuid, options.EntityKey, false, rockContext );

                if ( entityGuid != Guid.Empty && entityGuid != null )
                {
                    var tagService = new TagService( rockContext );
                    var taggedItemService = new TaggedItemService( rockContext );
                    var person = currentPersonId.HasValue ? new PersonService( rockContext ).Get( currentPersonId.Value ) : null;

                    // Get the existing tagged items for this entity
                    var existingTaggedItems = new List<TaggedItem>();
                    foreach ( var taggedItem in taggedItemService.Get( entityTypeId ?? 0, options.EntityQualifierColumn, options.EntityQualifierValue, currentPersonId, entityGuid.Value, options.CategoryGuid, options.ShowInactiveTags ) )
                    {
                        if ( taggedItem.IsAuthorized( Authorization.VIEW, person ) )
                        {
                            existingTaggedItems.Add( taggedItem );
                        }
                    }

                    // Get tag values after user edit
                    var currentTags = new List<Tag>();
                    foreach ( var tagBag in options.Tags )
                    {
                        var tagName = tagBag.Name;

                        if ( tagName.IsNullOrWhiteSpace() )
                        {
                            continue;
                        }

                        // Only if this is a new tag, create it
                        var tag = tagService.Get( entityTypeId ?? 0, options.EntityQualifierColumn, options.EntityQualifierValue, currentPersonId, tagName, options.CategoryGuid, options.ShowInactiveTags );

                        if ( currentPerson.PrimaryAlias != null && tag == null )
                        {
                            var cat = CategoryCache.Get( options.CategoryGuid ?? Guid.Empty );
                            var categoryId = cat != null ? cat.Id : ( int? ) null;

                            tag = new Tag();
                            tag.EntityTypeId = entityTypeId;
                            tag.CategoryId = categoryId;
                            tag.EntityTypeQualifierColumn = options.EntityQualifierColumn;
                            tag.EntityTypeQualifierValue = options.EntityQualifierValue;
                            tag.OwnerPersonAliasId = currentPerson.PrimaryAlias.Id;
                            tag.Name = tagName;
                            tagService.Add( tag );
                        }

                        if ( tag != null )
                        {
                            currentTags.Add( tag );
                        }
                    }

                    rockContext.SaveChanges();

                    var currentNames = currentTags.Select( t => t.Name ).ToList();
                    var existingNames = existingTaggedItems.Select( t => t.Tag.Name ).ToList();

                    // Delete any tagged items that user removed
                    foreach ( var taggedItem in existingTaggedItems )
                    {
                        if ( !currentNames.Contains( taggedItem.Tag.Name, StringComparer.OrdinalIgnoreCase ) && taggedItem.IsAuthorized( Rock.Security.Authorization.TAG, person ) )
                        {
                            existingNames.Remove( taggedItem.Tag.Name );
                            taggedItemService.Delete( taggedItem );
                        }
                    }
                    rockContext.SaveChanges();

                    // Add any tagged items that user added
                    foreach ( var tag in currentTags )
                    {
                        // If the tagged item was not already there, and (it's their personal tag OR they are authorized to use it) then add it.
                        if ( !existingNames.Contains( tag.Name, StringComparer.OrdinalIgnoreCase ) &&
                             (
                                ( tag.OwnerPersonAliasId != null && tag.OwnerPersonAliasId == currentPerson.PrimaryAlias.Id ) ||
                                tag.IsAuthorized( Rock.Security.Authorization.TAG, person )
                             )
                           )
                        {
                            var taggedItem = new TaggedItem();
                            taggedItem.TagId = tag.Id;
                            taggedItem.EntityTypeId = entityTypeId ?? 0;
                            taggedItem.EntityGuid = entityGuid.Value;
                            taggedItemService.Add( taggedItem );
                        }
                    }
                    rockContext.SaveChanges();

                    var currentTagBags = currentTags.Select( t => GetTagBagFromTag( t ) ).ToList();
                    return Ok( currentTagBags );
                }

                return BadRequest( "Cannot get entity guid from given entity key and entity type GUID." );
            }
        }

        private static EntityTagListTagBag GetTagBagFromTag( Tag tag )
        {
            return new EntityTagListTagBag
            {
                IdKey = tag.IdKey,
                BackgroundColor = tag.BackgroundColor,
                Category = tag.Category != null
                    ? new ListItemBag
                    {
                        Value = tag.Category.Guid.ToString(),
                        Text = tag.Category.ToString()
                    }
                    : null,
                EntityTypeGuid = tag.EntityTypeId.HasValue ? EntityTypeCache.Get( tag.EntityTypeId.Value ).Guid : Guid.Empty,
                IconCssClass = tag.IconCssClass,
                IsPersonal = tag.OwnerPersonAliasId.HasValue,
                Name = tag.Name
            };
        }

        #endregion

        #region Entity Type Picker

        /// <summary>
        /// Gets the entity types that can be displayed in the entity type picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the entity types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EntityTypePickerGetEntityTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "AFDD3D40-5856-478B-A41A-0539127F0631" )]
        public IHttpActionResult EntityTypePickerGetEntityTypes( [FromBody] EntityTypePickerGetEntityTypesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = EntityTypeCache.All( rockContext )
                    .Where( t => t.IsEntity )
                    .OrderByDescending( t => t.IsCommon )
                    .ThenBy( t => t.FriendlyName )
                    .Select( t => new ListItemBag
                    {
                        Value = t.Guid.ToString(),
                        Text = t.FriendlyName,
                        Category = t.IsCommon ? "Common" : "All Entities"
                    } )
                    .ToList();

                return Ok( items );
            }
        }

        #endregion

        #region Ethnicity Picker

        /// <summary>
        /// Gets the ethnicities that can be displayed in the ethnicity picker.
        /// </summary>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the ethnicities and the label for the control.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EthnicityPickerGetEthnicities" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "a04bddf8-4169-47f8-8b03-ee8e2f110b35" )]
        public IHttpActionResult EthnicityPickerGetEthnicities()
        {
            var ethnicities = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_ETHNICITY ).DefinedValues
                .Select( e => new ListItemBag { Text = e.Value, Value = e.Guid.ToString() } )
                .ToList();

            return Ok( new EthnicityPickerGetEthnicitiesResultsBag
            {
                Ethnicities = ethnicities,
                Label = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.PERSON_ETHNICITY_LABEL )
            } );
        }

        #endregion

        #region Event Calendar Picker

        /// <summary>
        /// Gets the event calendars that can be displayed in the event calendar picker.
        /// </summary>
        /// <returns>A List of <see cref="ListItemBag" /> objects that represent the event calendars.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EventCalendarPickerGetEventCalendars" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "92d88be0-2971-441a-b582-eec304ce4bc9" )]
        public IHttpActionResult EventCalendarPickerGetEventCalendars()
        {
            using ( var rockContext = new RockContext() )
            {
                var calendars = EventCalendarCache.All();
                var calendarList = new List<ListItemBag>();

                foreach ( EventCalendarCache eventCalendar in calendars )
                {
                    calendarList.Add( new ListItemBag { Text = eventCalendar.Name, Value = eventCalendar.Guid.ToString() } );
                }

                return Ok( calendarList );
            }
        }

        #endregion

        #region Event Item Picker

        /// <summary>
        /// Gets the event items that can be displayed in the event item picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the event items.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EventItemPickerGetEventItems" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "1D558F8A-08C9-4B62-A3A9-853C9F66B748" )]
        public IHttpActionResult EventItemPickerGetEventItems( [FromBody] EventItemPickerGetEventItemsOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var eventItems = new EventCalendarItemService( rockContext ).Queryable()
                    .Where( i => options.IncludeInactive ? true : i.EventItem.IsActive )
                    .Select( i => new ListItemBag
                    {
                        Category = i.EventCalendar.Name,
                        Value = i.EventItem.Guid.ToString(),
                        Text = i.EventItem.Name
                    } )
                    .OrderBy( i => i.Category )
                    .ThenBy( i => i.Text )
                    .ToList();

                return Ok( eventItems );
            }
        }

        #endregion

        #region Field Type Editor

        /// <summary>
        /// Gets the available field types for the current person.
        /// </summary>
        /// <param name="options">The options that provide details about the request.</param>
        /// <returns>A collection <see cref="ListItemBag"/> that represents the field types that are available.</returns>
        [HttpPost]
        [System.Web.Http.Route( "FieldTypeEditorGetAvailableFieldTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "FEDEF3F7-FCB0-4538-9629-177C7D2AE06F" )]
        public IHttpActionResult FieldTypeEditorGetAvailableFieldTypes( [FromBody] FieldTypeEditorGetAvailableFieldTypesOptionsBag options )
        {
            var fieldTypes = FieldTypeCache.All()
                .Where( f => f.Platform.HasFlag( Rock.Utility.RockPlatform.Obsidian ) )
                .ToList();

            var fieldTypeItems = fieldTypes
                .Select( f => new ListItemBag
                {
                    Text = f.Name,
                    Value = f.Guid.ToString()
                } )
                .ToList();

            return Ok( fieldTypeItems );
        }

        /// <summary>
        /// Gets the attribute configuration information provided and returns a new
        /// set of configuration data. This is used by the attribute editor control
        /// when a field type makes a change that requires new data to be retrieved
        /// in order for it to continue editing the attribute.
        /// </summary>
        /// <param name="options">The view model that contains the update request.</param>
        /// <returns>An instance of <see cref="FieldTypeEditorUpdateAttributeConfigurationResultBag"/> that represents the state of the attribute configuration.</returns>
        [HttpPost]
        [System.Web.Http.Route( "FieldTypeEditorUpdateAttributeConfiguration" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "AFDF0EC4-5D17-4278-9FA6-3F859F38E3B5" )]
        public IHttpActionResult FieldTypeEditorUpdateAttributeConfiguration( [FromBody] FieldTypeEditorUpdateAttributeConfigurationOptionsBag options )
        {
            var fieldType = Rock.Web.Cache.FieldTypeCache.Get( options.FieldTypeGuid )?.Field;

            if ( fieldType == null )
            {
                return BadRequest( "Unknown field type." );
            }

            // Convert the public configuration options into our private
            // configuration options (values).
            var configurationValues = fieldType.GetPrivateConfigurationValues( options.ConfigurationValues );

            // Convert the default value from the public value into our
            // private internal value.
            var privateDefaultValue = fieldType.GetPrivateEditValue( options.DefaultValue, configurationValues );

            // Get the new configuration properties from the currently selected
            // options.
            var configurationProperties = fieldType.GetPublicEditConfigurationProperties( configurationValues );

            // Get the public configuration options from the internal options (values).
            var publicConfigurationValues = fieldType.GetPublicConfigurationValues( configurationValues, Field.ConfigurationValueUsage.Configure, null );

            return Ok( new FieldTypeEditorUpdateAttributeConfigurationResultBag
            {
                ConfigurationProperties = configurationProperties,
                ConfigurationValues = publicConfigurationValues,
                DefaultValue = fieldType.GetPublicEditValue( privateDefaultValue, configurationValues )
            } );
        }

        #endregion

        #region Field Type Picker

        /// <summary>
        /// Gets the field types that can be displayed in the field type picker.
        /// </summary>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the field types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "FieldTypePickerGetFieldTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "AB53509A-C8A9-481B-839F-DA53232A698A" )]
        public IHttpActionResult FieldTypePickerGetFieldTypes()
        {
            List<ListItemBag> items = new List<ListItemBag> { };

            foreach ( var item in FieldTypeCache.All() )
            {
                items.Add( new ListItemBag { Text = item.Name, Value = item.Guid.ToString() } );
            }

            return Ok( items );
        }

        #endregion

        #region Financial Gateway Picker

        /// <summary>
        /// Gets the financial gateways that can be displayed in the financial gateway picker.
        /// </summary>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the financial gateways.</returns>
        [HttpPost]
        [System.Web.Http.Route( "FinancialGatewayPickerGetFinancialGateways" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "DBF12D3D-09BF-419F-A315-E3B6C0206344" )]
        public IHttpActionResult FinancialGatewayPickerGetFinancialGateways( [FromBody] FinancialGatewayPickerGetFinancialGatewaysOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                List<ListItemBag> items = new List<ListItemBag> { };

                var gateways = new FinancialGatewayService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( g => g.EntityTypeId.HasValue )
                    .OrderBy( g => g.Name )
                    .ToList();

                foreach ( var gateway in gateways )
                {
                    var entityType = EntityTypeCache.Get( gateway.EntityTypeId.Value );
                    GatewayComponent component = GatewayContainer.GetComponent( entityType.Name );

                    // TODO: Need to see if the gateway is selected e.g. gateway.Guid == options.selectedGuid
                    // Add the gateway if the control is configured to show all of the gateways.
                    if ( options.IncludeInactive && options.ShowAllGatewayComponents )
                    {
                        items.Add( new ListItemBag { Text = gateway.Name, Value = gateway.Guid.ToString() } );
                        continue;
                    }

                    // Do not add if the component or gateway is not active and the controls has ShowInactive set to false.
                    if ( options.IncludeInactive == false && ( gateway.IsActive == false || component == null || component.IsActive == false ) )
                    {
                        continue;
                    }

                    if ( options.ShowAllGatewayComponents == false && ( component == null || component.SupportsRockInitiatedTransactions == false ) )
                    {
                        continue;
                    }

                    items.Add( new ListItemBag { Text = gateway.Name, Value = gateway.Guid.ToString() } );
                }

                return Ok( items );
            }
        }

        #endregion

        #region Financial Statement Template Picker

        /// <summary>
        /// Gets the financial statement templates that can be displayed in the financial statement template picker.
        /// </summary>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the financial statement templates.</returns>
        [HttpPost]
        [System.Web.Http.Route( "FinancialStatementTemplatePickerGetFinancialStatementTemplates" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "4E10F2DC-BD7C-4F75-919C-B3F71868ED24" )]
        public IHttpActionResult FinancialStatementTemplatePickerGetFinancialStatementTemplates()
        {
            using ( var rockContext = new RockContext() )
            {
                List<ListItemBag> items = new FinancialStatementTemplateService( rockContext )
                    .Queryable()
                    .Where( s => s.IsActive == true )
                    .Select( i => new ListItemBag
                    {
                        Value = i.Guid.ToString(),
                        Text = i.Name
                    } )
                    .OrderBy( a => a.Text )
                    .ToList();

                return Ok( items );
            }
        }

        #endregion

        #region Following

        /// <summary>
        /// Determines if the entity is currently being followed by the logged in person.
        /// </summary>
        /// <param name="options">The options that describe which entity to be checked.</param>
        /// <returns>A <see cref="FollowingGetFollowingResponseBag"/> that contains the followed state of the entity.</returns>
        [HttpPost]
        [Authenticate]
        [System.Web.Http.Route( "FollowingGetFollowing" )]
        [Rock.SystemGuid.RestActionGuid( "FA1CC136-A994-4870-9507-818EA7A70F01" )]
        public IHttpActionResult FollowingGetFollowing( [FromBody] FollowingGetFollowingOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( RockRequestContext.CurrentPerson == null )
                {
                    return Unauthorized();
                }

                // Get the entity type identifier to use to lookup the entity.
                int? entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );

                if ( !entityTypeId.HasValue )
                {
                    return NotFound();
                }

                int? entityId = null;

                // Special handling for a person record, need to translate it to
                // a person alias record.
                if ( entityTypeId.Value == EntityTypeCache.GetId<Person>() )
                {
                    entityTypeId = EntityTypeCache.GetId<PersonAlias>();
                    entityId = new PersonService( rockContext ).Get( options.EntityKey, true )?.PrimaryAliasId;
                }
                else
                {
                    // Get the entity identifier to use for the following query.
                    entityId = Reflection.GetEntityIdForEntityType( entityTypeId.Value, options.EntityKey, true, rockContext );
                }

                if ( !entityId.HasValue )
                {
                    return NotFound();
                }

                var purposeKey = options.PurposeKey ?? string.Empty;

                // Look for any following objects that match the criteria.
                var followings = new FollowingService( rockContext ).Queryable()
                    .Where( f =>
                        f.EntityTypeId == entityTypeId.Value &&
                        f.EntityId == entityId.Value &&
                        f.PersonAlias.PersonId == RockRequestContext.CurrentPerson.Id &&
                        ( ( f.PurposeKey == null && purposeKey == "" ) || f.PurposeKey == purposeKey ) );

                return Ok( new FollowingGetFollowingResponseBag
                {
                    IsFollowing = followings.Any()
                } );
            }
        }

        /// <summary>
        /// Sets the following state of the entity for the logged in person.
        /// </summary>
        /// <param name="options">The options that describe which entity to be followed or unfollowed.</param>
        /// <returns>An HTTP status code that indicates if the request was successful.</returns>
        [HttpPost]
        [Authenticate]
        [System.Web.Http.Route( "FollowingSetFollowing" )]
        [Rock.SystemGuid.RestActionGuid( "8CA2EAFB-E577-4F65-8D96-F42D8D5AAE7A" )]
        public IHttpActionResult FollowingSetFollowing( [FromBody] FollowingSetFollowingOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var followingService = new FollowingService( rockContext );

                if ( RockRequestContext.CurrentPerson == null )
                {
                    return Unauthorized();
                }

                // Get the entity type identifier to use to lookup the entity.
                int? entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );

                if ( !entityTypeId.HasValue )
                {
                    return NotFound();
                }

                int? entityId = null;

                // Special handling for a person record, need to translate it to
                // a person alias record.
                if ( entityTypeId.Value == EntityTypeCache.GetId<Person>() )
                {
                    entityTypeId = EntityTypeCache.GetId<PersonAlias>();
                    entityId = new PersonService( rockContext ).Get( options.EntityKey, true )?.PrimaryAliasId;
                }
                else
                {
                    // Get the entity identifier to use for the following query.
                    entityId = Reflection.GetEntityIdForEntityType( entityTypeId.Value, options.EntityKey, true, rockContext );
                }

                if ( !entityId.HasValue )
                {
                    return NotFound();
                }

                var purposeKey = options.PurposeKey ?? string.Empty;

                // Look for any following objects that match the criteria.
                var followings = followingService.Queryable()
                    .Where( f =>
                        f.EntityTypeId == entityTypeId.Value &&
                        f.EntityId == entityId.Value &&
                        f.PersonAlias.PersonId == RockRequestContext.CurrentPerson.Id &&
                        ( ( f.PurposeKey == null && purposeKey == "" ) || f.PurposeKey == purposeKey ) );

                if ( options.IsFollowing )
                {
                    // Already following, don't need to add a new record.
                    if ( followings.Any() )
                    {
                        return Ok();
                    }

                    var following = new Following
                    {
                        EntityTypeId = entityTypeId.Value,
                        EntityId = entityId.Value,
                        PersonAliasId = RockRequestContext.CurrentPerson.PrimaryAliasId.Value,
                        PurposeKey = purposeKey
                    };

                    followingService.Add( following );

                    if ( !following.IsValid )
                    {
                        return BadRequest( string.Join( ", ", following.ValidationResults.Select( r => r.ErrorMessage ) ) );
                    }
                }
                else
                {
                    foreach ( var following in followings )
                    {
                        // Don't check security here because a person is allowed
                        // to un-follow/delete something they previously followed.
                        followingService.Delete( following );
                    }
                }

                System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", RockRequestContext.CurrentPerson );

                rockContext.SaveChanges();

                return Ok();
            }
        }

        #endregion

        #region Geo Picker

        /// <summary>
        /// Retrieve the Google API key for Google Maps.
        /// </summary>
        /// <returns>The Google API key as a string</returns>
        [HttpPost]
        [Authenticate]
        [System.Web.Http.Route( "GeoPickerGetGoogleMapSettings" )]
        [Rock.SystemGuid.RestActionGuid( "a3e0af9b-36d3-4ec8-a983-0087488c553d" )]
        public IHttpActionResult GeoPickerGetGoogleMapSettings( [FromBody] GeoPickerGetGoogleMapSettingsOptionsBag options )
        {
            // Map Styles
            Guid MapStyleValueGuid = options.MapStyleValueGuid == null || options.MapStyleValueGuid.IsEmpty() ? Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK.AsGuid() : options.MapStyleValueGuid;
            string mapStyle = "null";
            string markerColor = "";

            try
            {
                DefinedValueCache dvcMapStyle = DefinedValueCache.Get( MapStyleValueGuid );
                if ( dvcMapStyle != null )
                {
                    mapStyle = dvcMapStyle.GetAttributeValue( "DynamicMapStyle" );
                    var colors = dvcMapStyle.GetAttributeValue( "Colors" ).Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                    if ( colors.Any() )
                    {
                        markerColor = colors.First().Replace( "#", "" );
                    }
                }
            }
            catch { } // oh well...

            // Google API Key
            string googleApiKey = GlobalAttributesCache.Get().GetValue( "GoogleAPIKey" );

            // Default map location
            double? centerLatitude = null;
            double? centerLongitude = null;
            Guid guid = GlobalAttributesCache.Get().GetValue( "OrganizationAddress" ).AsGuid();

            if ( !guid.Equals( Guid.Empty ) )
            {
                var location = new Rock.Model.LocationService( new Rock.Data.RockContext() ).Get( guid );
                if ( location != null && location.GeoPoint != null && location.GeoPoint.Latitude != null && location.GeoPoint.Longitude != null )
                {
                    centerLatitude = location.GeoPoint.Latitude;
                    centerLongitude = location.GeoPoint.Longitude;
                }
            }

            return Ok( new GeoPickerGoogleMapSettingsBag
            {
                MapStyle = mapStyle,
                MarkerColor = markerColor,
                GoogleApiKey = googleApiKey,
                CenterLatitude = centerLatitude,
                CenterLongitude = centerLongitude
            } );
        }

        #endregion

        #region Grade Picker

        /// <summary>
        /// Gets the school grades that can be displayed in the grade picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the grades.</returns>
        [HttpPost]
        [System.Web.Http.Route( "GradePickerGetGrades" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "2C8F0B8E-F54D-460D-91DB-97B34A9AA174" )]
        public IHttpActionResult GradePickerGetGrades( GradePickerGetGradesOptionsBag options )
        {
            var schoolGrades = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() );

            if ( schoolGrades == null )
            {
                return NotFound();
            }

            var list = new List<ListItemBag>();

            foreach ( var schoolGrade in schoolGrades.DefinedValues.OrderByDescending( a => a.Value.AsInteger() ) )
            {
                ListItemBag listItem = new ListItemBag();
                if ( options.UseAbbreviation )
                {
                    string abbreviation = schoolGrade.GetAttributeValue( "Abbreviation" );
                    listItem.Text = string.IsNullOrWhiteSpace( abbreviation ) ? schoolGrade.Description : abbreviation;
                }
                else
                {
                    listItem.Text = schoolGrade.Description;
                }

                listItem.Value = options.UseGuidAsValue ? schoolGrade.Guid.ToString() : schoolGrade.Value;

                list.Add( listItem );
            }

            return Ok( list );
        }

        #endregion

        #region Group and Role Picker

        /// <summary>
        /// Gets the roles that can be displayed in the group and role picker for the specified group.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent the groups.</returns>
        [HttpPost]
        [System.Web.Http.Route( "GroupAndRolePickerGetRoles" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "285de6f4-0bf0-47e4-bda5-bcaa5a18b990" )]
        public IHttpActionResult GroupAndRolePickerGetRoles( [FromBody] GroupAndRolePickerGetRolesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupRoles = new List<ListItemBag>();
                if ( options.GroupTypeGuid != Guid.Empty )
                {
                    var groupTypeRoleService = new Rock.Model.GroupTypeRoleService( rockContext );
                    groupRoles = groupTypeRoleService.Queryable()
                        .Where( r => r.GroupType.Guid == options.GroupTypeGuid )
                        .OrderBy( r => r.Order )
                        .ThenBy( r => r.Name )
                        .Select( r => new ListItemBag { Text = r.Name, Value = r.Guid.ToString() } )
                        .ToList();
                }

                return Ok( groupRoles );
            }
        }

        #endregion

        #region Group Member Picker

        /// <summary>
        /// Gets the group members that can be displayed in the group member picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the group members.</returns>
        [HttpPost]
        [System.Web.Http.Route( "GroupMemberPickerGetGroupMembers" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "E0A893FD-0275-4251-BA6E-F669F110D179" )]
        public IHttpActionResult GroupMemberPickerGetGroupMembers( [FromBody] GroupMemberPickerGetGroupMembersOptionsBag options )
        {
            Rock.Model.Group group;

            if ( !options.GroupGuid.HasValue )
            {
                return NotFound();
            }

            group = new GroupService( new RockContext() ).Get( options.GroupGuid.Value );

            if ( group == null || !group.Members.Any() )
            {
                return NotFound();
            }

            var list = new List<ListItemBag>();

            foreach ( var groupMember in group.Members.OrderBy( m => m.Person.FullName ) )
            {
                var li = new ListItemBag
                {
                    Text = groupMember.Person.FullName,
                    Value = groupMember.Guid.ToString()
                };

                list.Add( li );
            }

            return Ok( list );
        }

        #endregion

        #region Group Type Group Picker

        /// <summary>
        /// Gets the groups that can be displayed in the group type group picker for the specified group type.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent the groups.</returns>
        [HttpPost]
        [System.Web.Http.Route( "GroupTypeGroupPickerGetGroups" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "f07ac6f8-128c-4881-a4ec-c245b8f10f9e" )]
        public IHttpActionResult GroupTypeGroupPickerGetGroups( [FromBody] GroupTypeGroupPickerGetGroupsOptionsBag options )
        {
            var groups = new List<ListItemBag>();
            if ( options.GroupTypeGuid != Guid.Empty )
            {
                var groupService = new Rock.Model.GroupService( new RockContext() );
                groups = groupService.Queryable()
                    .Where( g => g.GroupType.Guid == options.GroupTypeGuid )
                    .OrderBy( g => g.Name )
                    .Select( g => new ListItemBag { Text = g.Name, Value = g.Guid.ToString() } )
                    .ToList();
            }

            return Ok( groups );
        }

        /// <summary>
        /// Gets the groups that can be displayed in the group type group picker for the specified group type.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent the groups.</returns>
        [HttpPost]
        [System.Web.Http.Route( "GroupTypeGroupPickerGetGroupTypeOfGroup" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "984ce064-6073-4b8d-b670-338a3049e13b" )]
        public IHttpActionResult GroupTypeGroupPickerGetGroupTypeOfGroup( [FromBody] GroupTypeGroupPickerGetGroupTypeOfGroupOptionsBag options )
        {
            if ( options.GroupGuid != Guid.Empty )
            {
                var groupService = new Rock.Model.GroupService( new RockContext() );
                var group = groupService.Get( options.GroupGuid );

                if ( group == null )
                {
                    return NotFound();
                }

                return Ok( new ListItemBag { Text = group.GroupType.Name, Value = group.GroupType.Guid.ToString() } );
            }

            return NotFound();
        }

        #endregion

        #region Group Type Picker

        /// <summary>
        /// Gets the group types that can be displayed in the group type picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent the group types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "GroupTypePickerGetGroupTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "b0e07419-0e3c-4235-b5d4-4262fd63e050" )]
        public IHttpActionResult GroupTypePickerGetGroupTypes( [FromBody] GroupTypePickerGetGroupTypesOptionsBag options )
        {
            var groupTypes = new List<GroupTypeCache>();
            var results = new List<ListItemBag>();

            if ( options.GroupTypes == null || options.GroupTypes.Count < 1 )
            {
                groupTypes = GroupTypeCache.All();
            }
            else
            {
                foreach ( var groupTypeGuid in options.GroupTypes )
                {
                    var groupType = GroupTypeCache.Get( groupTypeGuid );
                    groupTypes.Add( groupType );
                }
            }

            if ( options.OnlyGroupListItems )
            {
                // get all group types that have the ShowInGroupList flag set
                groupTypes = groupTypes.Where( a => a.ShowInGroupList ).ToList();
            }

            if ( options.IsSortedByName )
            {
                groupTypes = groupTypes.OrderBy( gt => gt.Name ).ToList();
            }
            else
            {
                groupTypes = groupTypes.OrderBy( gt => gt.Order ).ThenBy( gt => gt.Name ).ToList();
            }

            foreach ( var gt in groupTypes )
            {
                results.Add( new ListItemBag { Text = gt.Name, Value = gt.Guid.ToString() } );
            }

            return Ok( results );
        }

        #endregion

        #region Group Picker

        /// <summary>
        /// Gets the groups that can be displayed in the group picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent the groups.</returns>
        [HttpPost]
        [System.Web.Http.Route( "GroupPickerGetChildren" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "c4f5432a-eb1e-4235-a5cd-bde37cc324f7" )]
        public IHttpActionResult GroupPickerGetChildren( GroupPickerGetChildrenOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupService = new GroupService( rockContext );

                List<int> includedGroupTypeIds = options.IncludedGroupTypeGuids
                    .Select( ( guid ) =>
                    {
                        var gt = GroupTypeCache.Get( guid );

                        if ( gt != null )
                        {
                            return gt.Id;
                        }

                        return 0;
                    } )
                    .ToList();

                // if specific group types are specified, show the groups regardless of ShowInNavigation
                bool limitToShowInNavigation = !includedGroupTypeIds.Any();

                Rock.Model.Group parentGroup = groupService.GetByGuid( options.Guid ?? Guid.Empty );
                int id = parentGroup == null ? 0 : parentGroup.Id;

                Rock.Model.Group rootGroup = groupService.GetByGuid( options.RootGroupGuid ?? Guid.Empty );
                int rootGroupId = rootGroup == null ? 0 : rootGroup.Id;

                var qry = groupService
                    .GetChildren( id, rootGroupId, false, includedGroupTypeIds, new List<int>(), options.IncludeInactiveGroups, limitToShowInNavigation, 0, false, false )
                    .AsNoTracking();

                List<Rock.Model.Group> groupList = new List<Rock.Model.Group>();
                List<TreeItemBag> groupNameList = new List<TreeItemBag>();

                var person = GetPerson();

                if ( parentGroup == null )
                {
                    parentGroup = rootGroup;
                }

                List<int> groupIdsWithSchedulingEnabledWithAncestors = null;
                List<int> groupIdsWithRSVPEnabledWithAncestors = null;

                var listOfChildGroups = qry.ToList().OrderBy( g => g.Order ).ThenBy( g => g.Name ).ToList();
                if ( listOfChildGroups.Any() )
                {
                    if ( options.LimitToSchedulingEnabled )
                    {
                        groupIdsWithSchedulingEnabledWithAncestors = groupService.GetGroupIdsWithSchedulingEnabledWithAncestors();
                    }

                    if ( options.LimitToRSVPEnabled )
                    {
                        groupIdsWithRSVPEnabledWithAncestors = groupService.GetGroupIdsWithRSVPEnabledWithAncestors();
                    }
                }

                foreach ( var group in listOfChildGroups )
                {
                    // we already have the ParentGroup record, so lets set it for each group to avoid a database round-trip during Auth
                    group.ParentGroup = parentGroup;

                    var groupType = GroupTypeCache.Get( group.GroupTypeId );

                    //// Before checking Auth, filter based on the limitToSchedulingEnabled and limitToRSVPEnabled option.
                    //// Auth takes longer to check, so if we can rule the group out sooner, that will save a bunch of time

                    if ( options.LimitToSchedulingEnabled )
                    {
                        var includeGroup = false;
                        if ( groupType?.IsSchedulingEnabled == true )
                        {
                            // if this group's group type has scheduling enabled, we will include this group
                            includeGroup = true;
                        }
                        else
                        {
                            // if this group's group type does not have scheduling enabled, we will need to include it if any of its children
                            // have scheduling enabled

                            if ( groupIdsWithSchedulingEnabledWithAncestors != null )
                            {
                                bool hasChildScheduledEnabledGroups = groupIdsWithSchedulingEnabledWithAncestors.Contains( group.Id );
                                if ( hasChildScheduledEnabledGroups )
                                {
                                    includeGroup = true;
                                }
                            }
                        }

                        if ( !includeGroup )
                        {
                            continue;
                        }
                    }

                    if ( options.LimitToRSVPEnabled )
                    {
                        var includeGroup = false;
                        if ( groupType?.EnableRSVP == true )
                        {
                            // if this group's group type has RSVP enabled, we will include this group
                            includeGroup = true;
                        }
                        else
                        {
                            if ( groupIdsWithRSVPEnabledWithAncestors != null )
                            {
                                bool hasChildRSVPEnabledGroups = groupIdsWithRSVPEnabledWithAncestors.Contains( group.Id );
                                if ( hasChildRSVPEnabledGroups )
                                {
                                    includeGroup = true;
                                }
                            }
                        }

                        if ( !includeGroup )
                        {
                            continue;
                        }
                    }

                    bool groupIsAuthorized = group.IsAuthorized( Rock.Security.Authorization.VIEW, person );
                    if ( !groupIsAuthorized )
                    {
                        continue;
                    }

                    groupList.Add( group );
                    var treeViewItem = new TreeItemBag();
                    treeViewItem.Value = group.Guid.ToString();
                    treeViewItem.Text = group.Name;
                    treeViewItem.IsActive = group.IsActive;

                    // if there a IconCssClass is assigned, use that as the Icon.
                    treeViewItem.IconCssClass = groupType?.IconCssClass;

                    groupNameList.Add( treeViewItem );
                }

                // try to quickly figure out which items have Children
                List<int> resultIds = groupList.Select( a => a.Id ).ToList();
                var qryHasChildren = groupService.Queryable().AsNoTracking()
                    .Where( g =>
                        g.ParentGroupId.HasValue &&
                        resultIds.Contains( g.ParentGroupId.Value ) );

                if ( includedGroupTypeIds.Any() )
                {
                    qryHasChildren = qryHasChildren.Where( a => includedGroupTypeIds.Contains( a.GroupTypeId ) );
                }

                var qryHasChildrenList = qryHasChildren
                    .Select( g => g.ParentGroup.Guid )
                    .Distinct()
                    .ToList();

                foreach ( var g in groupNameList )
                {
                    Guid groupGuid = g.Value.AsGuid();
                    g.HasChildren = qryHasChildrenList.Any( a => a == groupGuid );
                }

                return Ok( groupNameList );
            }
        }

        #endregion

        #region Group Role Picker

        /// <summary>
        /// Gets the group types that can be displayed in the group role picker.
        /// </summary>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent the group types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "GroupRolePickerGetGroupTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "56891c9b-f714-4083-8252-4c73b358aa02" )]
        public IHttpActionResult GroupRolePickerGetGroupTypes()
        {
            using ( var rockContext = new RockContext() )
            {
                var groupTypeService = new Rock.Model.GroupTypeService( rockContext );

                // get all group types that have at least one role
                var groupTypes = groupTypeService.Queryable()
                    .Where( a => a.Roles.Any() )
                    .OrderBy( a => a.Name )
                    .Select( g => new ListItemBag { Text = g.Name, Value = g.Guid.ToString() } )
                    .ToList();

                return Ok( groupTypes );
            }
        }

        /// <summary>
        /// Gets the group roles that can be displayed in the group role picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent the group roles.</returns>
        [HttpPost]
        [System.Web.Http.Route( "GroupRolePickerGetGroupRoles" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "968033ab-2596-4b0c-b06e-2c9cf59949c5" )]
        public IHttpActionResult GroupRolePickerGetGroupRoles( [FromBody] GroupRolePickerGetGroupRolesOptionsBag options )
        {
            return Ok( GroupRolePickerGetGroupRolesForGroupType( options.GroupTypeGuid, options.ExcludeGroupRoles ) );
        }

        /// <summary>
        /// Gets the group roles and group type information based on a selected group role.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>All the data for the selected role, selected type, and all of the group roles</returns>
        [HttpPost]
        [System.Web.Http.Route( "GroupRolePickerGetAllForGroupRole" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "e55374dd-7715-4392-a162-c40f09d25fc9" )]
        public IHttpActionResult GroupRolePickerGetAllForGroupRole( [FromBody] GroupRolePickerGetAllForGroupRoleOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                List<Guid> excludeGroupRoles = options.ExcludeGroupRoles;

                var groupRoleService = new Rock.Model.GroupTypeRoleService( rockContext );
                var groupRole = groupRoleService.Queryable()
                    .Where( r => r.Guid == options.GroupRoleGuid )
                    .First();

                var groupType = groupRole.GroupType;

                var groupRoles = GroupRolePickerGetGroupRolesForGroupType( groupType.Guid, excludeGroupRoles, rockContext );

                return Ok( new GroupRolePickerGetAllForGroupRoleResultsBag
                {
                    SelectedGroupRole = new ListItemBag { Text = groupRole.Name, Value = groupRole.Guid.ToString() },
                    SelectedGroupType = new ListItemBag { Text = groupType.Name, Value = groupType.Guid.ToString() },
                    GroupRoles = groupRoles
                } );
            }
        }

        /// <summary>
        /// Gets the group roles that can be displayed in the group role picker.
        /// </summary>
        /// <param name="groupTypeGuid">Load group roles of this type</param>
        /// <param name="excludeGroupRoles">Do not include these roles in the result</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent the group roles.</returns>
        private List<ListItemBag> GroupRolePickerGetGroupRolesForGroupType( Guid groupTypeGuid, List<Guid> excludeGroupRoles )
        {
            using ( var rockContext = new RockContext() )
            {
                return GroupRolePickerGetGroupRolesForGroupType( groupTypeGuid, excludeGroupRoles, rockContext );
            }
        }

        /// <summary>
        /// Gets the group roles that can be displayed in the group role picker.
        /// </summary>
        /// <param name="groupTypeGuid">Load group roles of this type</param>
        /// <param name="excludeGroupRoles">Do not include these roles in the result</param>
        /// <param name="rockContext">DB context</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent the group roles.</returns>
        private List<ListItemBag> GroupRolePickerGetGroupRolesForGroupType( Guid groupTypeGuid, List<Guid> excludeGroupRoles, RockContext rockContext )
        {
            var groupRoleService = new Rock.Model.GroupTypeRoleService( rockContext );

            var groupRoles = groupRoleService.Queryable()
                .Where( r =>
                    r.GroupType.Guid == groupTypeGuid &&
                    !excludeGroupRoles.Contains( r.Guid ) )
                .OrderBy( r => r.Name )
                .Select( r => new ListItemBag { Text = r.Name, Value = r.Guid.ToString() } )
                .ToList();

            return groupRoles;
        }

        #endregion

        #region Interaction Channel Interaction Component Picker

        /// <summary>
        /// Gets the interaction channel that the given interaction component is a part of.
        /// </summary>
        /// <returns>A <see cref="ListItemBag"/> object that represents the interaction channel.</returns>
        [HttpPost]
        [System.Web.Http.Route( "InteractionChannelInteractionComponentPickerGetChannelFromComponent" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "ebef7cb7-f20d-40d9-9f70-1f30aff1cd8f" )]
        public IHttpActionResult InteractionChannelInteractionComponentPickerGetChannelFromComponent( [FromBody] InteractionChannelInteractionComponentPickerGetChannelFromComponentOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var interactionComponentService = new InteractionComponentService( rockContext );
                var component = interactionComponentService.Get( options.InteractionComponentGuid );

                if ( component == null )
                {
                    return NotFound();
                }

                var channel = component.InteractionChannel;

                return Ok( new ListItemBag { Text = $"{channel.Name} ({channel.ChannelTypeMediumValue.Value ?? string.Empty})", Value = channel.Guid.ToString() } );
            }
        }

        #endregion

        #region Interaction Channel Picker

        /// <summary>
        /// Gets the interaction channels that can be displayed in the interaction channel picker.
        /// </summary>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the interaction channels.</returns>
        [HttpPost]
        [System.Web.Http.Route( "InteractionChannelPickerGetInteractionChannels" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "2F855DC7-7C20-4C09-9CB1-FFC1E022385B" )]
        public IHttpActionResult InteractionChannelPickerGetInteractionChannels()
        {
            var items = new List<ListItemBag>();
            var rockContext = new RockContext();
            var interactionChannelService = new InteractionChannelService( rockContext );
            var channels = interactionChannelService.Queryable().AsNoTracking()
                .Include( "ChannelTypeMediumValue" )
                .Where( ic => ic.IsActive )
                .OrderBy( ic => ic.Name )
                .Select( ic => new
                {
                    ic.Name,
                    ic.Guid,
                    Medium = ic.ChannelTypeMediumValue.Value
                } )
                .ToList();

            foreach ( var channel in channels )
            {
                ListItemBag li;

                if ( channel.Medium.IsNullOrWhiteSpace() )
                {
                    li = new ListItemBag { Text = channel.Name, Value = channel.Guid.ToString() };
                }
                else
                {
                    li = new ListItemBag { Text = $"{channel.Name} ({channel.Medium ?? string.Empty})", Value = channel.Guid.ToString() };
                }

                items.Add( li );
            }

            return Ok( items );
        }

        #endregion

        #region Interaction Component Picker

        /// <summary>
        /// Gets the interection components that can be displayed in the interection component picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the interection components.</returns>
        [HttpPost]
        [System.Web.Http.Route( "InteractionComponentPickerGetInteractionComponents" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "BD61A390-39F9-4FDE-B9AD-02E53B5F2073" )]
        public IHttpActionResult InteractionComponentPickerGetInteractionComponents( [FromBody] InteractionComponentPickerGetInteractionComponentsOptionsBag options )
        {
            if ( !options.InteractionChannelGuid.HasValue )
            {
                return NotFound();
            }

            int interactionChannelId = InteractionChannelCache.GetId( options.InteractionChannelGuid.Value ) ?? 0;
            var rockContext = new RockContext();
            var interactionComponentService = new InteractionComponentService( rockContext );

            var components = interactionComponentService.Queryable().AsNoTracking()
                .Where( ic => ic.InteractionChannelId == interactionChannelId )
                .OrderBy( ic => ic.Name )
                .Select( ic => new ListItemBag
                {
                    Text = ic.Name,
                    Value = ic.Guid.ToString()
                } )
                .ToList();

            return Ok( components );
        }

        #endregion

        #region Lava Command Picker

        /// <summary>
        /// Gets the lava commands that can be displayed in the lava command picker.
        /// </summary>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the lava commands.</returns>
        [HttpPost]
        [System.Web.Http.Route( "LavaCommandPickerGetLavaCommands" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "9FD03EE7-49E8-4C64-AC25-648422579F28" )]
        public IHttpActionResult LavaCommandPickerGetLavaCommands()
        {
            var items = new List<ListItemBag>();

            items.Add( new ListItemBag { Text = "All", Value = "All" } );

            foreach ( var command in Rock.Lava.LavaHelper.GetLavaCommands() )
            {
                items.Add( new ListItemBag { Text = command.SplitCase(), Value = command } );
            }

            return Ok( items );
        }

        #endregion

        #region Location Item Picker

        /// <summary>
        /// Gets the child locations, excluding inactive items.
        /// </summary>
        /// <param name="options">The options that describe which child locations to retrieve.</param>
        /// <returns>A collection of <see cref="TreeItemBag"/> objects that represent the child locations.</returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "LocationItemPickerGetActiveChildren" )]
        [Rock.SystemGuid.RestActionGuid( "E57312EC-92A7-464C-AA7E-5320DDFAEF3D" )]
        public IHttpActionResult LocationItemPickerGetActiveChildren( [FromBody] LocationItemPickerGetActiveChildrenOptionsBag options )
        {
            IQueryable<Location> qry;

            using ( var rockContext = new RockContext() )
            {
                var locationService = new LocationService( rockContext );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                if ( options.Guid == Guid.Empty )
                {
                    qry = locationService.Queryable().AsNoTracking().Where( a => a.ParentLocationId == null );
                    if ( options.RootLocationGuid != Guid.Empty )
                    {
                        qry = qry.Where( a => a.Guid == options.RootLocationGuid );
                    }
                }
                else
                {
                    qry = locationService.Queryable().AsNoTracking().Where( a => a.ParentLocation.Guid == options.Guid );
                }

                // limit to only active locations.
                qry = qry.Where( a => a.IsActive );

                // limit to only Named Locations (don't show home addresses, etc)
                qry = qry.Where( a => a.Name != null && a.Name != string.Empty );

                List<Location> locationList = new List<Location>();
                List<TreeItemBag> locationNameList = new List<TreeItemBag>();

                var person = GetPerson();

                foreach ( var location in qry.OrderBy( l => l.Name ) )
                {
                    if ( location.IsAuthorized( Authorization.VIEW, person ) || grant?.IsAccessGranted( location, Authorization.VIEW ) == true )
                    {
                        locationList.Add( location );
                        var treeViewItem = new TreeItemBag();
                        treeViewItem.Value = location.Guid.ToString();
                        treeViewItem.Text = location.Name;
                        locationNameList.Add( treeViewItem );
                    }
                }

                // try to quickly figure out which items have Children
                List<int> resultIds = locationList.Select( a => a.Id ).ToList();

                var qryHasChildren = locationService.Queryable().AsNoTracking()
                    .Where( l =>
                        l.ParentLocationId.HasValue &&
                        resultIds.Contains( l.ParentLocationId.Value ) &&
                        l.IsActive
                    )
                    .Select( l => l.ParentLocation.Guid )
                    .Distinct()
                    .ToList();

                var qryHasChildrenList = qryHasChildren.ToList();

                foreach ( var item in locationNameList )
                {
                    var locationGuid = item.Value.AsGuid();
                    item.IsFolder = qryHasChildrenList.Any( a => a == locationGuid );
                    item.HasChildren = item.IsFolder;
                }

                return Ok( locationNameList );
            }
        }

        #endregion

        #region Location List

        /// <summary>
        /// Gets the child locations, excluding inactive items.
        /// </summary>
        /// <param name="options">The options that describe which child locations to retrieve.</param>
        /// <returns>A collection of <see cref="TreeItemBag"/> objects that represent the child locations.</returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "LocationListGetLocations" )]
        [Rock.SystemGuid.RestActionGuid( "E57312EC-92A7-464C-AA7E-5320DDFAEF3D" )]
        public IHttpActionResult LocationListGetLocations( [FromBody] LocationListGetLocationsOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                List<ListItemBag> locations = null;
                var locationService = new LocationService( rockContext );
                int parentLocationId = 0;
                int locationTypeValueId = 0;

                if ( options.ParentLocationGuid != null )
                {
                    var parentLocation = locationService.Get( options.ParentLocationGuid );
                    parentLocationId = parentLocation == null ? 0 : parentLocation.Id;
                }

                if ( options.LocationTypeValueGuid != null )
                {
                    var locationTypeDefinedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.LOCATION_TYPE.AsGuid() );
                    var locationTypeValue = DefinedValueCache.Get( options.LocationTypeValueGuid );

                    // Verify the given GUID is a LocationType GUID
                    if ( locationTypeValue != null && locationTypeDefinedType.Equals( locationTypeValue.DefinedType ) )
                    {
                        locationTypeValueId = locationTypeValue.Id;
                    }
                }

                var locationQuery = locationService
                    .Queryable()
                    .AsNoTracking()
                    .Where( l => l.IsActive )
                    .Where( l => locationTypeValueId == 0 || l.LocationTypeValueId == locationTypeValueId )
                    .Where( l => parentLocationId == 0 || l.ParentLocationId == parentLocationId )
                    .Select( l => new { l.Name, l.City, l.State, l.Guid } )
                    .ToList()
                    .OrderBy( l => l.Name );

                if ( options.ShowCityState )
                {
                    locations = locationQuery
                        .Select( l => new ListItemBag { Text = $"{l.Name} ({l.City}, {l.State})", Value = l.Guid.ToString() } )
                        .ToList();
                }
                else
                {
                    locations = locationQuery
                        .Where( l => l.Name.IsNotNullOrWhiteSpace() )
                        .Select( l => new ListItemBag { Text = $"{l.Name}", Value = l.Guid.ToString() } )
                        .ToList();
                }

                return Ok( locations );
            }
        }

        /// <summary>
        /// Get the attributes for Locations
        /// </summary>
        /// <returns>A list of attributes in a form the Attribute Values Container can use</returns>
        [HttpPost]
        [System.Web.Http.Route( "LocationListGetAttributes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "e2b28b2f-a46d-40cd-a48d-7e5351383de5" )]
        public IHttpActionResult LocationListGetAttributes( /*LocationListGetAttributesOptionsBag options*/ )
        {
            if ( RockRequestContext.CurrentPerson == null )
            {
                return Unauthorized();
            }

            return Ok( GetAttributes( new Location { Id = 0 } ) );
        }

        /// <summary>
        /// Save a new Location
        /// </summary>
        /// <param name="options">The data for the new Location</param>
        /// <returns>A <see cref="ListItemBag"/> representing the new Location.</returns>
        [HttpPost]
        [System.Web.Http.Route( "LocationListSaveNewLocation" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "f8342fdb-3e19-4f17-804c-c14fdee87a2b" )]
        public IHttpActionResult LocationListSaveNewLocation( LocationListSaveNewLocationOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var locationService = new LocationService( rockContext );

                // Create and save new location with data from client
                var location = new Location
                {
                    Name = options.Name,
                    IsActive = true,
                };

                if ( options.Address != null )
                {
                    location.Street1 = options.Address.Street1;
                    location.Street2 = options.Address.Street2;
                    location.City = options.Address.City;
                    location.County = options.Address.Locality;
                    location.State = options.Address.State;
                    location.Country = options.Address.Country;
                    location.PostalCode = options.Address.PostalCode;
                }

                if ( options.ParentLocationGuid != null )
                {
                    Location parentLocation = locationService.Get( options.ParentLocationGuid );
                    location.ParentLocation = parentLocation;
                }

                if ( options.LocationTypeValueGuid != null )
                {
                    var locationTypeDefinedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.LOCATION_TYPE.AsGuid() );
                    var locationTypeValue = DefinedValueCache.Get( options.LocationTypeValueGuid );

                    // Verify the given GUID is a LocationType GUID
                    if ( locationTypeValue != null && locationTypeDefinedType.Equals( locationTypeValue.DefinedType ) )
                    {
                        location.LocationTypeValueId = locationTypeValue.Id;
                    }
                }

                locationService.Add( location );

                rockContext.SaveChanges();

                // Load up the new location's attributes and save those
                location.LoadAttributes();

                foreach ( KeyValuePair<string, AttributeValueCache> attr in location.AttributeValues )
                {
                    location.AttributeValues[attr.Key].Value = options.AttributeValues.GetValueOrNull( attr.Key );
                }

                if ( !location.IsValid )
                {
                    return InternalServerError();
                }

                location.SaveAttributeValues( rockContext );

                // Return a representation of the location so it can be used right away on the client
                return Ok( new ListItemBag
                {
                    Text = options.ShowCityState ? $"{location.Name} ({location.City}, {location.State})" : location.Name,
                    Value = location.Guid.ToString()
                } );
            }
        }

        #endregion

        #region Media Element Picker

        /// <summary>
        /// Gets the media accounts that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <returns>A List of <see cref="TreeItemBag" /> objects that represent media accounts.</returns>
        [HttpPost]
        [System.Web.Http.Route( "MediaElementPickerGetMediaAccounts" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "849e3ac3-f1e1-4efa-b0c8-1a79c4a666c7" )]
        public IHttpActionResult MediaElementPickerGetMediaAccounts()
        {
            using ( var rockContext = new RockContext() )
            {
                return Ok( GetMediaAccounts( rockContext ) );
            }
        }

        /// <summary>
        /// Gets the media folders that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which media folders to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent media folders.</returns>
        [HttpPost]
        [System.Web.Http.Route( "MediaElementPickerGetMediaFolders" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "a68493aa-8f41-404f-90dd-fbb2df0309a0" )]
        public IHttpActionResult MediaElementPickerGetMediaFolders( [FromBody] MediaElementPickerGetMediaFoldersOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var mediaAccount = GetMediaAccountByGuid( options.MediaAccountGuid, rockContext );

                if ( mediaAccount == null )
                {
                    return NotFound();
                }

                return Ok( GetMediaFoldersForAccount( mediaAccount, rockContext ) );
            }
        }

        /// <summary>
        /// Gets the media elements that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which media elements to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent media elements.</returns>
        [HttpPost]
        [System.Web.Http.Route( "MediaElementPickerGetMediaElements" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "9b922b7e-95b4-4ecf-a6ec-f61b45f5e210" )]
        public IHttpActionResult MediaElementPickerGetMediaElements( [FromBody] MediaElementPickerGetMediaElementsOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var mediaFolder = GetMediaFolderByGuid( options.MediaFolderGuid, rockContext );

                if ( mediaFolder == null )
                {
                    return NotFound();
                }

                return Ok( GetMediaElementsForFolder( mediaFolder, rockContext ) );
            }
        }

        /// <summary>
        /// Get all of the list items and the account/folder/element, depending on what the deepest given item is.
        /// </summary>
        /// <param name="options">The options that describe which media element picker data to load.</param>
        /// <returns>All of the picker lists (as List&lt;ListItemBag&gt;), and individual picker selections that could be derived from the given options</returns>
        [HttpPost]
        [System.Web.Http.Route( "MediaElementPickerGetMediaTree" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "2cc15018-201e-4f22-b116-06846c70ad0b" )]
        public IHttpActionResult MediaElementPickerGetMediaTree( [FromBody] MediaElementPickerGetMediaTreeOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var accounts = new List<ListItemBag>();
                var folders = new List<ListItemBag>();
                var elements = new List<ListItemBag>();

                MediaAccount mediaAccount = null;
                MediaFolder mediaFolder = null;
                MediaElement mediaElement = null;

                ListItemBag mediaAccountItem = null;
                ListItemBag mediaFolderItem = null;
                ListItemBag mediaElementItem = null;

                // If a media element is specified, get everything based on that
                if ( options.MediaElementGuid.HasValue )
                {
                    mediaElement = GetMediaElementByGuid( ( Guid ) options.MediaElementGuid, rockContext );
                    mediaFolder = mediaElement.MediaFolder;
                    mediaAccount = mediaFolder.MediaAccount;

                    mediaAccountItem = new ListItemBag { Text = mediaAccount.Name, Value = mediaAccount.Guid.ToString() };
                    mediaFolderItem = new ListItemBag { Text = mediaFolder.Name, Value = mediaFolder.Guid.ToString() };
                    mediaElementItem = new ListItemBag { Text = mediaElement.Name, Value = mediaElement.Guid.ToString() };

                    accounts = GetMediaAccounts( rockContext );
                    folders = GetMediaFoldersForAccount( mediaAccount, rockContext );
                    elements = GetMediaElementsForFolder( mediaFolder, rockContext );
                }
                // Otherwise, if a media folder is specified, get everything based on that, not getting a media element
                else if ( options.MediaFolderGuid.HasValue )
                {
                    mediaFolder = GetMediaFolderByGuid( ( Guid ) options.MediaFolderGuid, rockContext );
                    mediaAccount = mediaFolder.MediaAccount;

                    mediaAccountItem = new ListItemBag { Text = mediaAccount.Name, Value = mediaAccount.Guid.ToString() };
                    mediaFolderItem = new ListItemBag { Text = mediaFolder.Name, Value = mediaFolder.Guid.ToString() };

                    accounts = GetMediaAccounts( rockContext );
                    folders = GetMediaFoldersForAccount( mediaAccount, rockContext );
                    elements = GetMediaElementsForFolder( mediaFolder, rockContext );
                }
                // Otherwise, if a media account is specified, get the account and the lists of accounts and folders
                else if ( options.MediaAccountGuid.HasValue )
                {
                    mediaAccount = GetMediaAccountByGuid( ( Guid ) options.MediaAccountGuid, rockContext );

                    mediaAccountItem = new ListItemBag { Text = mediaAccount.Name, Value = mediaAccount.Guid.ToString() };

                    accounts = GetMediaAccounts( rockContext );
                    folders = GetMediaFoldersForAccount( mediaAccount, rockContext );
                }

                // Some things might be null, but we pass back everything we have
                return Ok( new MediaElementPickerGetMediaTreeResultsBag
                {
                    MediaAccount = mediaAccountItem,
                    MediaFolder = mediaFolderItem,
                    MediaElement = mediaElementItem,

                    MediaAccounts = accounts,
                    MediaFolders = folders,
                    MediaElements = elements
                } );
            }
        }

        /// <summary>
        /// Retrieve a MediaAccount object based on its Guid
        /// </summary>
        /// <param name="guid">The Media Account's Guid</param>
        /// <param name="rockContext">DB context</param>
        /// <returns>The MediaAccount with that Guid</returns>
        private MediaAccount GetMediaAccountByGuid( Guid guid, RockContext rockContext )
        {
            // Get the media folder from the given GUID so we can filter elements by folder
            var mediaAccountService = new Rock.Model.MediaAccountService( rockContext );
            var mediaAccount = mediaAccountService.Queryable()
                .Where( a => a.Guid == guid )
                .First();

            return mediaAccount;
        }

        /// <summary>
        /// Retrieve a MediaFolder object based on its Guid
        /// </summary>
        /// <param name="guid">The Media Folder's Guid</param>
        /// <param name="rockContext">DB context</param>
        /// <returns>The MediaFolder with that Guid</returns>
        private MediaFolder GetMediaFolderByGuid( Guid guid, RockContext rockContext )
        {
            // Get the media folder from the given GUID so we can filter elements by folder
            var mediaFolderService = new Rock.Model.MediaFolderService( rockContext );
            var mediaFolder = mediaFolderService.Queryable()
                .Where( a => a.Guid == guid )
                .First();

            return mediaFolder;
        }

        /// <summary>
        /// Retrieve a MediaElement object based on its Guid
        /// </summary>
        /// <param name="guid">The Media Element's Guid</param>
        /// <param name="rockContext">DB context</param>
        /// <returns>The MediaElement with that Guid</returns>
        private MediaElement GetMediaElementByGuid( Guid guid, RockContext rockContext )
        {
            // Get the media folder from the given GUID so we can filter elements by folder
            var mediaElementService = new Rock.Model.MediaElementService( rockContext );
            var mediaElement = mediaElementService.Queryable()
                .Where( a => a.Guid == guid )
                .First();

            return mediaElement;
        }

        /// <summary>
        /// Get a list of all the Media Accounts
        /// </summary>
        /// <param name="rockContext">DB context</param>
        /// <returns>List of ListItemBags representing all of the Media Accounts</returns>
        private List<ListItemBag> GetMediaAccounts( RockContext rockContext )
        {
            var mediaAccountService = new Rock.Model.MediaAccountService( rockContext );

            // Get all media accounts that are active.
            var mediaAccounts = mediaAccountService.Queryable()
                .Where( ma => ma.IsActive )
                .OrderBy( ma => ma.Name )
                .Select( ma => new ListItemBag { Text = ma.Name, Value = ma.Guid.ToString() } )
                .ToList();

            return mediaAccounts;
        }

        /// <summary>
        /// Get a list of all the Media Folders for the given Media Account
        /// </summary>
        /// <param name="mediaAccount">MediaAccount object we want to get the child Media Folders of</param>
        /// <param name="rockContext">DB context</param>
        /// <returns>List of ListItemBags representing all of the Media Folders for the given Media Account</returns>
        private List<ListItemBag> GetMediaFoldersForAccount( MediaAccount mediaAccount, RockContext rockContext )
        {
            // Get all media folders
            var mediaFolderService = new Rock.Model.MediaFolderService( rockContext );
            var mediaFolders = mediaFolderService.Queryable()
                .Where( mf => mf.MediaAccountId == mediaAccount.Id )
                .OrderBy( mf => mf.Name )
                .Select( mf => new ListItemBag
                {
                    Text = mf.Name,
                    Value = mf.Guid.ToString()
                } )
                .ToList();

            return mediaFolders;
        }

        /// <summary>
        /// Get a list of all the Media Elements for the given Media Account
        /// </summary>
        /// <param name="mediaFolder">The media folder.</param>
        /// <param name="rockContext">DB context</param>
        /// <returns>List of ListItemBags representing all of the Media Elements for the given Media Folder</returns>
        private List<ListItemBag> GetMediaElementsForFolder( MediaFolder mediaFolder, RockContext rockContext )
        {
            var mediaElementService = new Rock.Model.MediaElementService( rockContext );
            var mediaElements = mediaElementService.Queryable()
                .Where( me => me.MediaFolderId == mediaFolder.Id )
                .OrderBy( me => me.Name )
                .Select( me => new ListItemBag
                {
                    Text = me.Name,
                    Value = me.Guid.ToString()
                } )
                .ToList();

            return mediaElements;
        }

        #endregion

        #region Merge Field Picker

        /// <summary>
        /// Gets the merge fields and their categories that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which merge fields to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent a tree of merge fields.</returns>
        [HttpPost]
        [System.Web.Http.Route( "MergeFieldPickerGetChildren" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "f6722f7a-64ed-401a-9dea-c64fa9738b75" )]
        public IHttpActionResult MergeFieldPickerGetChildren( [FromBody] MergeFieldPickerGetChildrenOptionsBag options )
        {
            var children = MergeFieldPickerGetChildren( options.Id, options.AdditionalFields, RockRequestContext.CurrentPerson );

            var treeItemChildren = children?.Select( convertTreeViewItemToTreeItemBag ).ToList();

            return Ok( treeItemChildren );
        }

        /// <summary>
        /// Formats a selected Merge Field value as Lava
        /// This endpoint returns items formatted for use in a tree view control.
        /// ***NOTE***: Also implemented in Rock.Web.UI.Controls.MergeFieldPicker's FormatSelectedValue method.
        /// Any changes here should also be made there
        /// </summary>
        /// <param name="options">The options that contain the selected value</param>
        /// <returns>A string of Lava</returns>
        [HttpPost]
        [System.Web.Http.Route( "MergeFieldPickerFormatSelectedValue" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "ffe018c4-c088-4057-b28b-4980541f16d5" )]
        public IHttpActionResult MergeFieldPickerFormatSelectedValue( [FromBody] MergeFieldPickerFormatSelectedValueOptionsBag options )
        {
            if ( options.SelectedValue == null )
            {
                return BadRequest();
            }

            var idParts = options.SelectedValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            if ( idParts.Count > 0 )
            {
                if ( idParts.Count == 2 && idParts[0] == "GlobalAttribute" )
                {
                    return Ok( string.Format( "{{{{ 'Global' | Attribute:'{0}' }}}}", idParts[1] ) );
                }

                if ( idParts.Count == 1 && idParts[0].StartsWith( "AdditionalMergeField" ) )
                {
                    string mFields = idParts[0].Replace( "AdditionalMergeField_", "" ).Replace( "AdditionalMergeFields_", "" );
                    if ( mFields.IsNotNullOrWhiteSpace() )
                    {
                        string beginFor = "{% for field in AdditionalFields %}";
                        string endFor = "{% endfor %}";
                        var mergeFields = String.Join( "", mFields.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries )
                            .Select( f => "{{ field." + f + "}}" ) );

                        return Ok( $"{beginFor}{mergeFields}{endFor}" );
                    }
                }

                if ( idParts.Count == 1 )
                {
                    if ( idParts[0] == "Campuses" )
                    {
                        return Ok( @"
{% for campus in Campuses %}
<p>
    Name: {{ campus.Name }}<br/>
    Description: {{ campus.Description }}<br/>
    Is Active: {{ campus.IsActive }}<br/>
    Short Code: {{ campus.ShortCode }}<br/>
    Url: {{ campus.Url }}<br/>
    Phone Number: {{ campus.PhoneNumber }}<br/>
    Service Times:
    {% for serviceTime in campus.ServiceTimes %}
        {{ serviceTime.Day }} {{ serviceTime.Time }},
    {% endfor %}
    <br/>
{% endfor %}
" );
                    }

                    if ( idParts[0] == "Date" )
                    {
                        return Ok( "{{ 'Now' | Date:'MM/dd/yyyy' }}" );
                    }

                    if ( idParts[0] == "Time" )
                    {
                        return Ok( "{{ 'Now' | Date:'hh:mm:ss tt' }}" );
                    }

                    if ( idParts[0] == "DayOfWeek" )
                    {
                        return Ok( "{{ 'Now' | Date:'dddd' }}" );
                    }

                    if ( idParts[0] == "PageParameter" )
                    {
                        return Ok( "{{ PageParameter.[Enter Page Parameter Name Here] }}" );
                    }
                }

                var workingParts = new List<string>();

                // Get the root type
                int pathPointer = 0;
                EntityTypeCache entityType = null;
                while ( entityType == null && pathPointer < idParts.Count() )
                {
                    string item = idParts[pathPointer];
                    string[] itemParts = item.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );

                    string itemName = itemParts.Length > 1 ? itemParts[0] : string.Empty;
                    string mergeFieldId = itemParts.Length > 1 ? itemParts[1] : item;

                    var entityTypeInfo = MergeFieldPicker.GetEntityTypeInfoFromMergeFieldId( mergeFieldId );
                    entityType = entityTypeInfo?.EntityType;

                    workingParts.Add( entityType != null ?
                        ( itemName != string.Empty ? itemName : entityType.FriendlyName.Replace( " ", string.Empty ) ) :
                        idParts[pathPointer] );
                    pathPointer++;
                }

                if ( entityType != null )
                {
                    Type type = entityType.GetEntityType();

                    var formatString = "{0}";

                    // Traverse the Property path
                    bool itemIsCollection = false;
                    bool lastItemIsProperty = true;

                    while ( idParts.Count > pathPointer )
                    {
                        string propertyName = idParts[pathPointer];
                        workingParts.Add( propertyName );

                        var childProperty = type.GetProperty( propertyName );
                        if ( childProperty != null )
                        {
                            lastItemIsProperty = true;
                            type = childProperty.PropertyType;

                            if ( type.IsGenericType &&
                                type.GetGenericTypeDefinition() == typeof( ICollection<> ) &&
                                type.GetGenericArguments().Length == 1 )
                            {
                                string propertyNameSingularized = propertyName.Singularize();
                                string forString = string.Format( "<% for {0} in {1} %> {{0}} <% endfor %>", propertyNameSingularized, workingParts.AsDelimited( "." ) );
                                workingParts.Clear();
                                workingParts.Add( propertyNameSingularized );
                                formatString = string.Format( formatString, forString );

                                type = type.GetGenericArguments()[0];

                                itemIsCollection = true;
                            }
                            else
                            {
                                itemIsCollection = false;
                            }
                        }
                        else
                        {
                            lastItemIsProperty = false;
                        }

                        pathPointer++;
                    }

                    string itemString = string.Empty;
                    if ( !itemIsCollection )
                    {
                        if ( lastItemIsProperty )
                        {
                            itemString = string.Format( "<< {0} >>", workingParts.AsDelimited( "." ) );
                        }
                        else
                        {
                            string partPath = workingParts.Take( workingParts.Count - 1 ).ToList().AsDelimited( "." );
                            var partItem = workingParts.Last();
                            if ( type == typeof( Rock.Model.Person ) && partItem == "Campus" )
                            {
                                itemString = string.Format( "{{{{ {0} | Campus | Property:'Name' }}}}", partPath );
                            }
                            else
                            {
                                itemString = string.Format( "{{{{ {0} | Attribute:'{1}' }}}}", partPath, partItem );
                            }
                        }
                    }

                    return Ok( string.Format( formatString, itemString ).Replace( "<", "{" ).Replace( ">", "}" ) );
                }

                return Ok( string.Format( "{{{{ {0} }}}}", idParts.AsDelimited( "." ) ) );
            }

            return Ok( string.Empty );
        }

        /// <summary>
        /// Gets the child merge fields available to the given user.
        /// NOTE: This is used by the legacy MergeFieldsController and was copied from there
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="additionalFields">The additional fields.</param>
        /// <param name="person">The current user</param>
        /// <returns></returns>
        internal static IQueryable<TreeViewItem> MergeFieldPickerGetChildren( string id, string additionalFields, Person person )
        {
            List<TreeViewItem> items = new List<TreeViewItem>();

            switch ( id )
            {
                case "0":
                    {
                        if ( !string.IsNullOrWhiteSpace( additionalFields ) )
                        {
                            foreach ( string fieldInfo in additionalFields.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                            {
                                string[] parts = fieldInfo.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );

                                string fieldId = parts.Length > 0 ? parts[0] : string.Empty;

                                if ( fieldId == "AdditionalMergeFields" )
                                {
                                    if ( parts.Length > 1 )
                                    {
                                        var fieldsTv = new TreeViewItem
                                        {
                                            Id = $"AdditionalMergeFields_{parts[1]}",
                                            Name = "Additional Fields",
                                            HasChildren = true,
                                            Children = new List<TreeViewItem>()
                                        };

                                        foreach ( string fieldName in parts[1].Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries ) )
                                        {
                                            fieldsTv.Children.Add( new TreeViewItem
                                            {
                                                Id = $"AdditionalMergeField_{fieldName}",
                                                Name = fieldName.SplitCase(),
                                                HasChildren = false
                                            } );
                                        }
                                        items.Add( fieldsTv );
                                    }
                                }
                                else
                                {
                                    string[] idParts = fieldId.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );

                                    string mergeFieldId = idParts.Length > 1 ? idParts[1] : fieldId;

                                    var entityTypeInfo = MergeFieldPicker.GetEntityTypeInfoFromMergeFieldId( mergeFieldId );
                                    if ( entityTypeInfo?.EntityType != null )
                                    {
                                        items.Add( new TreeViewItem
                                        {
                                            Id = fieldId.UrlEncode(),
                                            Name = parts.Length > 1 ? parts[1] : entityTypeInfo.EntityType.FriendlyName,
                                            HasChildren = true
                                        } );
                                    }
                                    else
                                    {
                                        items.Add( new TreeViewItem
                                        {
                                            Id = fieldId,
                                            Name = parts.Length > 1 ? parts[1] : mergeFieldId.SplitCase(),
                                            HasChildren = mergeFieldId == "GlobalAttribute"
                                        } );
                                    }
                                }
                            }
                        }

                        break;
                    }

                case "GlobalAttribute":
                    {
                        var globalAttributes = GlobalAttributesCache.Get();

                        foreach ( var attributeCache in globalAttributes.Attributes.OrderBy( a => a.Key ) )
                        {
                            if ( attributeCache.IsAuthorized( Authorization.VIEW, person ) )
                            {
                                items.Add( new TreeViewItem
                                {
                                    Id = "GlobalAttribute|" + attributeCache.Key,
                                    Name = attributeCache.Name,
                                    HasChildren = false
                                } );
                            }
                        }

                        break;
                    }

                default:
                    {
                        // In this scenario, the id should be a concatenation of a root qualified entity name and then the property path
                        var idParts = id.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                        if ( idParts.Count > 0 )
                        {
                            // Get the root type
                            int pathPointer = 0;
                            EntityTypeCache entityType = null;
                            MergeFieldPicker.EntityTypeInfo.EntityTypeQualifier[] entityTypeQualifiers = null;
                            while ( entityType == null && pathPointer < idParts.Count() )
                            {
                                string item = idParts[pathPointer];
                                string[] itemParts = item.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                                string entityTypeMergeFieldId = itemParts.Length > 1 ? itemParts[1] : item;
                                MergeFieldPicker.EntityTypeInfo entityTypeInfo = MergeFieldPicker.GetEntityTypeInfoFromMergeFieldId( entityTypeMergeFieldId );
                                entityType = entityTypeInfo?.EntityType;
                                entityTypeQualifiers = entityTypeInfo?.EntityTypeQualifiers;
                                pathPointer++;
                            }

                            if ( entityType != null )
                            {
                                Type type = entityType.GetEntityType();

                                // Traverse the Property path
                                while ( idParts.Count > pathPointer )
                                {
                                    var childProperty = type.GetProperty( idParts[pathPointer] );
                                    if ( childProperty != null )
                                    {
                                        type = childProperty.PropertyType;

                                        if ( type.IsGenericType &&
                                            type.GetGenericTypeDefinition() == typeof( ICollection<> ) &&
                                            type.GetGenericArguments().Length == 1 )
                                        {
                                            type = type.GetGenericArguments()[0];
                                        }
                                    }

                                    pathPointer++;
                                }

                                entityType = EntityTypeCache.Get( type );

                                // Add the tree view items
                                foreach ( var propInfo in Rock.Lava.LavaHelper.GetLavaProperties( type ) )
                                {
                                    var treeViewItem = new TreeViewItem
                                    {
                                        Id = id + "|" + propInfo.Name,
                                        Name = propInfo.Name.SplitCase()
                                    };

                                    Type propertyType = propInfo.PropertyType;

                                    if ( propertyType.IsGenericType &&
                                        propertyType.GetGenericTypeDefinition() == typeof( ICollection<> ) &&
                                        propertyType.GetGenericArguments().Length == 1 )
                                    {
                                        treeViewItem.Name += " (Collection)";
                                        propertyType = propertyType.GetGenericArguments()[0];
                                    }

                                    bool hasChildren = false;
                                    if ( EntityTypeCache.Get( propertyType.FullName, false ) != null )
                                    {
                                        hasChildren = Rock.Lava.LavaHelper.GetLavaProperties( propertyType ).Any();
                                    }

                                    treeViewItem.HasChildren = hasChildren;

                                    items.Add( treeViewItem );
                                }

                                if ( type == typeof( Rock.Model.Person ) )
                                {
                                    items.Add( new TreeViewItem
                                    {
                                        Id = id + "|" + "Campus",
                                        Name = "Campus"
                                    } );
                                }

                                if ( entityType.IsEntity )
                                {
                                    var attributeList = new AttributeService( new Rock.Data.RockContext() ).GetByEntityTypeId( entityType.Id, false ).ToAttributeCacheList();
                                    if ( entityTypeQualifiers?.Any() == true )
                                    {
                                        var qualifiedAttributeList = new List<AttributeCache>();
                                        foreach ( var entityTypeQualifier in entityTypeQualifiers )
                                        {
                                            var qualifierAttributes = attributeList.Where( a =>
                                                 a.EntityTypeQualifierColumn.Equals( entityTypeQualifier.Column, StringComparison.OrdinalIgnoreCase )
                                                 && a.EntityTypeQualifierValue.Equals( entityTypeQualifier.Value, StringComparison.OrdinalIgnoreCase ) ).ToList();

                                            qualifiedAttributeList.AddRange( qualifierAttributes );
                                        }

                                        attributeList = qualifiedAttributeList;
                                    }
                                    else
                                    {
                                        // Only include attributes without a qualifier since we weren't specified a qualifiercolumn/value
                                        attributeList = attributeList.Where( a => a.EntityTypeQualifierColumn.IsNullOrWhiteSpace() && a.EntityTypeQualifierValue.IsNullOrWhiteSpace() ).ToList();
                                    }

                                    foreach ( var attribute in attributeList )
                                    {
                                        if ( attribute.IsAuthorized( Authorization.VIEW, person ) )
                                        {
                                            items.Add( new TreeViewItem
                                            {
                                                Id = id + "|" + attribute.Key,
                                                Name = attribute.Name
                                            } );
                                        }
                                    }
                                }
                            }
                        }

                        break;
                    }
            }

            return items.OrderBy( i => i.Name ).AsQueryable();
        }

        #endregion

        #region Merge Template Picker

        /// <summary>
        /// Gets the merge templates and their categories that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which merge templates to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent a tree of merge templates.</returns>
        [HttpPost]
        [System.Web.Http.Route( "MergeTemplatePickerGetMergeTemplates" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "2e486da8-927f-4474-8ba8-00a68d261403" )]
        public IHttpActionResult MergeTemplatePickerGetMergeTemplates( [FromBody] MergeTemplatePickerGetMergeTemplatesOptionsBag options )
        {
            List<Guid> include = null;
            List<Guid> exclude = null;

            if ( options.MergeTemplateOwnership == Rock.Enums.Controls.MergeTemplateOwnership.Global )
            {
                exclude = new List<Guid>();
                exclude.Add( Rock.SystemGuid.Category.PERSONAL_MERGE_TEMPLATE.AsGuid() );
            }
            else if ( options.MergeTemplateOwnership == Rock.Enums.Controls.MergeTemplateOwnership.Personal )
            {
                include = new List<Guid>();
                include.Add( Rock.SystemGuid.Category.PERSONAL_MERGE_TEMPLATE.AsGuid() );
            }

            using ( var rockContext = new RockContext() )
            {
                var clientService = new CategoryClientService( rockContext, GetPerson( rockContext ) );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
                var queryOptions = new CategoryItemTreeOptions
                {
                    ParentGuid = options.ParentGuid,
                    GetCategorizedItems = options.ParentGuid.HasValue,
                    EntityTypeGuid = EntityTypeCache.Get<MergeTemplate>().Guid,
                    IncludeUnnamedEntityItems = false,
                    IncludeCategoriesWithoutChildren = false,
                    IncludeCategoryGuids = include,
                    ExcludeCategoryGuids = exclude,
                    DefaultIconCssClass = options.DefaultIconCssClass,
                    ItemFilterPropertyName = null,
                    ItemFilterPropertyValue = "",
                    LazyLoad = true,
                    SecurityGrant = grant
                };

                var items = clientService.GetCategorizedTreeItems( queryOptions );

                return Ok( items );
            }
        }

        #endregion

        #region Metric Category Picker

        /// <summary>
        /// Gets the metric categories and their categories that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which metric categories to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent a tree of metric categories.</returns>
        [HttpPost]
        [System.Web.Http.Route( "MetricCategoryPickerGetChildren" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "92a11376-6bcd-4299-a54d-946cbde7566b" )]
        public IHttpActionResult MetricCategoryPickerGetChildren( [FromBody] MetricCategoryPickerGetChildrenOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = new CategoryClientService( rockContext, GetPerson( rockContext ) );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
                var queryOptions = new CategoryItemTreeOptions
                {
                    ParentGuid = options.ParentGuid,
                    GetCategorizedItems = options.ParentGuid.HasValue,
                    EntityTypeGuid = EntityTypeCache.Get<MetricCategory>().Guid,
                    IncludeUnnamedEntityItems = true,
                    IncludeCategoriesWithoutChildren = false,
                    DefaultIconCssClass = options.DefaultIconCssClass,
                    ItemFilterPropertyName = null,
                    ItemFilterPropertyValue = "",
                    LazyLoad = true,
                    SecurityGrant = grant
                };

                var items = clientService.GetCategorizedTreeItems( queryOptions );

                return Ok( items );
            }
        }

        #endregion

        #region Metric Item Picker

        /// <summary>
        /// Gets the metric items and their categories that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which metric items to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent a tree of metric items.</returns>
        [HttpPost]
        [System.Web.Http.Route( "MetricItemPickerGetChildren" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "c8e8f26e-a7cd-445a-8d72-6d4484a8ee59" )]
        public IHttpActionResult MetricItemPickerGetChildren( [FromBody] MetricItemPickerGetChildrenOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = GetMetricItemPickerChildren( options, rockContext );

                if ( items == null || items.Count == 0 )
                {
                    return NotFound();
                }

                return Ok( items );
            }
        }

        /// <summary>
        /// Gets the metric items and their categories that match the options given.
        /// </summary>
        /// <param name="options">The options that describe which metric items to load.</param>
        /// <param name="rockContext">Context for performing DB queries.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent a tree of metric items.</returns>
        private List<TreeItemBag> GetMetricItemPickerChildren( [FromBody] MetricItemPickerGetChildrenOptionsBag options, RockContext rockContext )
        {
            var clientService = new CategoryClientService( rockContext, GetPerson( rockContext ) );
            var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
            var queryOptions = new CategoryItemTreeOptions
            {
                ParentGuid = options.ParentGuid,
                GetCategorizedItems = options.ParentGuid.HasValue,
                EntityTypeGuid = EntityTypeCache.Get<MetricCategory>().Guid,
                IncludeUnnamedEntityItems = true,
                IncludeCategoriesWithoutChildren = false,
                DefaultIconCssClass = options.DefaultIconCssClass,
                LazyLoad = true,
                SecurityGrant = grant,
                IncludeCategoryGuids = options.IncludeCategoryGuids
            };

            var metricCategories = clientService.GetCategorizedTreeItems( queryOptions );
            var metricCategoryService = new MetricCategoryService( new RockContext() );
            var convertedMetrics = new List<TreeItemBag>();

            // Translate from MetricCategory to Metric.
            foreach ( var categoryItem in metricCategories )
            {
                if ( !categoryItem.IsFolder )
                {
                    // Load the MetricCategory.
                    var metricCategory = metricCategoryService.Get( categoryItem.Value.AsGuid() );
                    if ( metricCategory != null )
                    {
                        // Swap the Id to the Metric Guid (instead of MetricCategory.Guid).
                        categoryItem.Value = metricCategory.Guid.ToString();
                    }
                }

                if ( categoryItem.HasChildren )
                {
                    categoryItem.Children = new List<TreeItemBag>();
                    categoryItem.Children.AddRange( GetMetricItemPickerChildren( new MetricItemPickerGetChildrenOptionsBag
                    {
                        ParentGuid = categoryItem.Value.AsGuid(),
                        DefaultIconCssClass = options.DefaultIconCssClass,
                        SecurityGrantToken = options.SecurityGrantToken,
                        IncludeCategoryGuids = options.IncludeCategoryGuids
                    }, rockContext ) );
                }

                convertedMetrics.Add( categoryItem );
            }

            return convertedMetrics;
        }

        #endregion

        #region Note Editor

        /// <summary>
        /// Searches for possible mention candidates to display that match the request.
        /// </summary>
        /// <param name="options">The options that describe the mention sources to search for.</param>
        /// <returns>An instance of <see cref="NoteEditorMentionSearchResultsBag"/> that contains the possible matches.</returns>
        [HttpPost]
        [System.Web.Http.Route( "NoteEditorMentionSearch" )]
        [Authenticate]
        [SecurityAction( "FullSearch", "Allows individuals to perform a full search of all individuals in the database." )]
        [Rock.SystemGuid.RestActionGuid( "dca338b6-9749-427e-8238-1686c9587d16" )]
        public IHttpActionResult NoteEditorMentionSearch( [FromBody] NoteEditorMentionSearchOptionsBag options )
        {
            var restAction = RestActionCache.Get( new Guid( "dca338b6-9749-427e-8238-1686c9587d16" ) );
            var isFullSearchAllowed = restAction.IsAuthorized( "FullSearch", RockRequestContext.CurrentPerson );

            using ( var rockContext = new RockContext() )
            {
                var searchComponent = Rock.Search.SearchContainer.GetComponent( typeof( Rock.Search.Person.Name ).FullName );
                var allowFirstNameOnly = searchComponent?.GetAttributeValue( "FirstNameSearch" ).AsBoolean() ?? false;
                var personService = new PersonService( rockContext );

                var personSearchOptions = new PersonService.PersonSearchOptions
                {
                    Name = options.Name,
                    AllowFirstNameOnly = allowFirstNameOnly,
                    IncludeBusinesses = false,
                    IncludeDeceased = false
                };

                // Prepare the basic person search filter that wil be used
                // for both the "full database" search as well as the priority
                // list search.
                var basicPersonSearchQry = personService.Search( personSearchOptions ).AsNoTracking();

                // Get the query to run for a full-database search. The where
                // clause will make it so we get no results unless full search
                // is allowed.
                var searchQry = basicPersonSearchQry
                    .Where( p => isFullSearchAllowed || p.Id == 0 )
                    .Select( p => new
                    {
                        Person = p,
                        Priority = false
                    } );

                // This is intentionally commented out since we don't support
                // this just yet. But it is here to see the pattern of how to
                // provide priority search results based on values in the token.
                //if ( DecryptedToken.GroupId.HasValue )
                //{
                //    var groupPersonIdQry = new GroupMemberService( rockContext ).Queryable()
                //        .Where( gm => gm.GroupId == DecryptedToken.GroupId.Value )
                //        .Select( gm => gm.PersonId );

                //    var prioritySearchQry = basicPersonSearchQry
                //        .Where( p => groupPersonIdQry.Contains( p.Id ) )
                //        .Select( p => new
                //        {
                //            Person = p,
                //            Priority = true
                //        } );

                //    searchQry = searchQry.Union( prioritySearchQry );
                //}

                // We want the priority people first and then after that sort by
                // view count in descending order.
                //
                // Then take 50 total items, put it in C# memory and then get the
                // distinct ones and finally limit to our final 25 people. This
                // is done because if we do a Distinct() in SQL it will lose the
                // sorting, but we can't do the sorting after a SQL .Distinct().
                var people = searchQry
                    .OrderByDescending( p => p.Priority )
                    .ThenByDescending( p => p.Person.ViewedCount )
                    .ThenBy( p => p.Person.Id )
                    .Select( p => p.Person )
                    .Take( 50 )
                    .ToList()
                    .DistinctBy( p => p.Id )
                    .Take( 25 )
                    .ToList();

                var hasMultipleCampuses = CampusCache.All().Count( c => c.IsActive == true ) > 1;

                // Convert the list of people into a collection of mention items.
                var items = people
                    .Select( p => new NoteMentionItemBag
                    {
                        CampusName = p.PrimaryCampusId.HasValue && hasMultipleCampuses
                            ? CampusCache.Get( p.PrimaryCampusId.Value )?.CondensedName
                            : string.Empty,
                        DisplayName = p.FullName,
                        Email = p.Email,
                        Identifier = IdHasher.Instance.GetHash( p.PrimaryAliasId ?? 0 ),
                        ImageUrl = p.PhotoUrl
                    } )
                    .ToList();

                return Ok( new NoteEditorMentionSearchResultsBag
                {
                    Items = items
                } );
            }
        }

        #endregion

        #region Page Picker

        /// <summary>
        /// Gets the tree list of pages
        /// </summary>
        /// <param name="options">The options that describe which pages to retrieve.</param>
        /// <returns>A collection of <see cref="TreeItemBag"/> objects that represent the pages.</returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "PagePickerGetChildren" )]
        [Rock.SystemGuid.RestActionGuid( "EE9AB2EA-EE01-4D0F-B626-02D1C8D1ABF4" )]
        public IHttpActionResult PagePickerGetChildren( [FromBody] PagePickerGetChildrenOptionsBag options )
        {
            var service = new Service<Page>( new RockContext() ).Queryable().AsNoTracking();
            var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
            IQueryable<Page> qry;

            if ( options.Guid.IsEmpty() )
            {
                qry = service.Where( a => a.ParentPage.Guid == options.RootPageGuid );
            }
            else
            {
                qry = service.Where( a => a.ParentPage.Guid == options.Guid );
            }

            if ( options.SiteType != null )
            {
                qry = qry.Where( p => ( int ) p.Layout.Site.SiteType == options.SiteType.Value );
            }

            var hidePageGuids = options.HidePageGuids ?? new List<Guid>();

            List<Page> pageList = qry
                .Where( p => !hidePageGuids.Contains( p.Guid ) )
                .OrderBy( p => p.Order )
                .ThenBy( p => p.InternalName )
                .ToList()
                .Where( p => p.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( p, Authorization.VIEW ) == true )
                .ToList();
            List<TreeItemBag> pageItemList = new List<TreeItemBag>();
            foreach ( var page in pageList )
            {
                var pageItem = new TreeItemBag();
                pageItem.Value = page.Guid.ToString();
                pageItem.Text = page.InternalName;

                pageItemList.Add( pageItem );
            }

            // try to quickly figure out which items have Children
            List<int> resultIds = pageList.Select( a => a.Id ).ToList();

            var qryHasChildren = service
                .Where( p =>
                    p.ParentPageId.HasValue &&
                    resultIds.Contains( p.ParentPageId.Value ) )
                .Select( p => p.ParentPage.Guid )
                .Distinct()
                .ToList();

            foreach ( var g in pageItemList )
            {
                var hasChildren = qryHasChildren.Any( a => a.ToString() == g.Value );
                g.HasChildren = hasChildren;
                g.IsFolder = hasChildren;
                g.IconCssClass = "fa fa-file-o";
            }

            return Ok( pageItemList.AsQueryable() );
        }

        /// <summary>
        /// Gets the list of pages in the hierarchy going from the root to the given page
        /// </summary>
        /// <param name="options">The options that describe which pages to retrieve.</param>
        /// <returns>A collection of <see cref="Guid"/> that represent the pages.</returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "PagePickerGetSelectedPageHierarchy" )]
        [Rock.SystemGuid.RestActionGuid( "e74611a0-1711-4a0b-b3bd-df242d344679" )]
        public IHttpActionResult PagePickerGetSelectedPageHierarchy( [FromBody] PagePickerGetSelectedPageHierarchyOptionsBag options )
        {
            var parentPageGuids = new List<string>();
            var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

            foreach ( Guid pageGuid in options.SelectedPageGuids )
            {
                var page = PageCache.Get( pageGuid );

                if ( page == null )
                {
                    continue;
                }

                var parentPage = page.ParentPage;

                while ( parentPage != null )
                {
                    if ( !parentPageGuids.Contains( parentPage.Guid.ToString() ) && ( parentPage.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || ( grant?.IsAccessGranted( parentPage, Authorization.VIEW ) == true ) ) )
                    {
                        parentPageGuids.Insert( 0, parentPage.Guid.ToString() );
                    }
                    else
                    {
                        // infinite recursion
                        break;
                    }

                    parentPage = parentPage.ParentPage;
                }
            }

            return Ok( parentPageGuids );
        }

        /// <summary>
        /// Gets the internal name of the page with the given Guid
        /// </summary>
        /// <param name="options">The options that contains the Guid of the page</param>
        /// <returns>A string internal name of the page with the given Guid.</returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "PagePickerGetPageName" )]
        [Rock.SystemGuid.RestActionGuid( "20d219bd-3635-4cbc-b79f-250972ae6b97" )]
        public IHttpActionResult PagePickerGetPageName( [FromBody] PagePickerGetPageNameOptionsBag options )
        {
            var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
            var page = PageCache.Get( options.PageGuid );

            if ( page == null )
            {
                return NotFound();
            }

            var isAuthorized = page.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( page, Authorization.VIEW ) == true;

            if ( !isAuthorized )
            {
                return Unauthorized();
            }

            return Ok( page.InternalName );
        }

        /// <summary>
        /// Gets the list of routes to the given page
        /// </summary>
        /// <param name="options">The options that describe which routes to retrieve.</param>
        /// <returns>A collection of <see cref="ListItemBag"/> that represent the routes.</returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "PagePickerGetPageRoutes" )]
        [Rock.SystemGuid.RestActionGuid( "858209a4-7715-43e6-aff5-00b82773f241" )]
        public IHttpActionResult PagePickerGetPageRoutes( [FromBody] PagePickerGetPageRoutesOptionsBag options )
        {
            var page = PageCache.Get( options.PageGuid );
            var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

            if ( page == null )
            {
                return NotFound();
            }

            var isAuthorized = page.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( page, Authorization.VIEW ) == true;

            if ( !isAuthorized )
            {
                return Unauthorized();
            }

            var routes = page.PageRoutes
                .Select( r => new ListItemBag
                {
                    Text = r.Route,
                    Value = r.Guid.ToString()
                } )
                .ToList();

            return Ok( routes );
        }

        #endregion

        #region Person Link

        /// <summary>
        /// Gets the popup HTML for the selected person
        /// </summary>
        /// <param name="options">The data needed to get the person's popup HTML</param>
        /// <returns>A string containing the popup markups</returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "PersonLinkGetPopupHtml" )]
        [Rock.SystemGuid.RestActionGuid( "39f44203-9944-4dbd-87ca-d23657e0daa5" )]
        public IHttpActionResult PersonLinkGetPopupHtml( [FromBody] PersonLinkGetPopupHtmlOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var result = "No Details Available";
                var html = new StringBuilder();
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                // Create new service (need ProxyServiceEnabled)
                var person = new PersonService( rockContext ).Queryable( "ConnectionStatusValue, PhoneNumbers" )
                    .Where( p => p.Id == options.PersonId )
                    .FirstOrDefault();

                if ( person != null )
                {
                    // If the entity can be secured, ensure the person has access to it.
                    if ( person is ISecured securedEntity )
                    {
                        var isAuthorized = securedEntity.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson )
                            || grant?.IsAccessGranted( person, Authorization.VIEW ) == true;

                        if ( !isAuthorized )
                        {
                            return Unauthorized();
                        }
                    }

                    var appPath = System.Web.VirtualPathUtility.ToAbsolute( "~" );
                    html.AppendFormat(
                        "<header>{0} <h3>{1}<small>{2}</small></h3></header>",
                        Person.GetPersonPhotoImageTag( person, 65, 65 ),
                        person.FullName,
                        person.ConnectionStatusValue != null ? person.ConnectionStatusValue.Value : string.Empty );

                    html.Append( "<div class='body'>" );

                    var spouse = person.GetSpouse( rockContext );
                    if ( spouse != null )
                    {
                        html.AppendFormat(
                            "<div><strong>Spouse</strong> {0}</div>",
                            spouse.LastName == person.LastName ? spouse.FirstName : spouse.FullName );
                    }

                    int? age = person.Age;
                    if ( age.HasValue )
                    {
                        html.AppendFormat( "<div><strong>Age</strong> {0}</div>", age );
                    }

                    if ( !string.IsNullOrWhiteSpace( person.Email ) )
                    {
                        html.AppendFormat( "<div style='text-overflow: ellipsis; white-space: nowrap; overflow:hidden; width: 245px;'><strong>Email</strong> {0}</div>", person.GetEmailTag( VirtualPathUtility.ToAbsolute( "~/" ) ) );
                    }

                    foreach ( var phoneNumber in person.PhoneNumbers.Where( n => n.IsUnlisted == false && n.NumberTypeValueId.HasValue ).OrderBy( n => n.NumberTypeValue.Order ) )
                    {
                        html.AppendFormat( "<div><strong>{0}</strong> {1}</div>", phoneNumber.NumberTypeValue.Value, phoneNumber.ToString() );
                    }

                    html.Append( "</div>" );

                    result = html.ToString();
                }

                return Ok( result );
            }
        }

        #endregion

        #region Person Picker

        /// <summary>
        /// Searches for people that match the given search options and returns
        /// those matches.
        /// </summary>
        /// <param name="options">The options that describe how the search should be performed.</param>
        /// <returns>A collection of <see cref="Rock.Rest.Controllers.PersonSearchResult"/> objects.</returns>
        [Authenticate]
        [Secured]
        [HttpPost]
        [System.Web.Http.Route( "PersonPickerSearch" )]
        [Rock.SystemGuid.RestActionGuid( "1947578D-B28F-4956-8666-DCC8C0F2B945" )]
        public IQueryable<Rock.Rest.Controllers.PersonSearchResult> PersonPickerSearch( [FromBody] PersonPickerSearchOptionsBag options )
        {
            var rockContext = new RockContext();

            // Chain to the v1 controller.
            return Rock.Rest.Controllers.PeopleController.SearchForPeople( rockContext, options.Name, options.Address, options.Phone, options.Email, options.IncludeDetails, options.IncludeBusinesses, options.IncludeDeceased, false );
        }

        #endregion

        #region Phone Number Box

        /// <summary>
        /// Get the phone number configuration related to country codes and number formats
        /// </summary>
        /// <returns>The configurations in the form of <see cref="ViewModels.Rest.Controls.PhoneNumberBoxGetConfigurationResultsBag"/>.</returns>
        [Authenticate]
        [HttpPost]
        [System.Web.Http.Route( "PhoneNumberBoxGetConfiguration" )]
        [Rock.SystemGuid.RestActionGuid( "2f15c4a2-92c7-4bd3-bf48-7eb11a644142" )]
        public IHttpActionResult PhoneNumberBoxGetConfiguration( [FromBody] PhoneNumberBoxGetConfigurationOptionsBag options )
        {
            var countryCodeRules = new Dictionary<string, List<PhoneNumberCountryCodeRulesConfigurationBag>>();
            var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.COMMUNICATION_PHONE_COUNTRY_CODE.AsGuid() );
            string defaultCountryCode = null;

            if ( definedType != null )
            {
                var definedValues = definedType.DefinedValues;

                foreach ( var countryCode in definedValues.OrderBy( v => v.Order ).Select( v => v.Value ).Distinct() )
                {
                    var rules = new List<PhoneNumberCountryCodeRulesConfigurationBag>();

                    if ( defaultCountryCode == null )
                    {
                        defaultCountryCode = countryCode;
                    }

                    foreach ( var definedValue in definedValues.Where( v => v.Value == countryCode ).OrderBy( v => v.Order ) )
                    {
                        string match = definedValue.GetAttributeValue( "MatchRegEx" );
                        string replace = definedValue.GetAttributeValue( "FormatRegEx" );
                        if ( !string.IsNullOrWhiteSpace( match ) && !string.IsNullOrWhiteSpace( replace ) )
                        {
                            rules.Add( new PhoneNumberCountryCodeRulesConfigurationBag { Match = match, Format = replace } );
                        }
                    }

                    countryCodeRules.Add( countryCode, rules );
                }
            }

            if ( options?.ShowSmsOptIn ?? false )
            {
                return Ok( new PhoneNumberBoxGetConfigurationResultsBag
                {
                    Rules = countryCodeRules,
                    DefaultCountryCode = defaultCountryCode,
                    SmsOptInText = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.SMS_OPT_IN_MESSAGE_LABEL )
                } );
            }

            return Ok( new PhoneNumberBoxGetConfigurationResultsBag
            {
                Rules = countryCodeRules,
                DefaultCountryCode = defaultCountryCode
            } );
        }

        #endregion

        #region Race Picker

        /// <summary>
        /// Gets the races that can be displayed in the race picker.
        /// </summary>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the races and the label for the control.</returns>
        [HttpPost]
        [System.Web.Http.Route( "RacePickerGetRaces" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "126eec10-7a19-49af-9646-909bd92ea516" )]
        public IHttpActionResult RacePickerGetRaces()
        {
            var races = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_RACE ).DefinedValues
                .Select( e => new ListItemBag { Text = e.Value, Value = e.Guid.ToString() } )
                .ToList();

            return Ok( new RacePickerGetRacesResultsBag
            {
                Races = races,
                Label = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.PERSON_RACE_LABEL )
            } );
        }

        #endregion

        #region Registration Instance Picker

        /// <summary>
        /// Gets the instances that can be displayed in the registration instance picker.
        /// </summary>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the registration instances for the control.</returns>
        [HttpPost]
        [System.Web.Http.Route( "RegistrationInstancePickerGetRegistrationInstances" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "26ecd3a7-9c55-4052-afc9-b59e84ab890b" )]
        public IHttpActionResult RegistrationInstancePickerGetRegistrationInstances( [FromBody] RegistrationInstancePickerGetRegistrationInstancesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var registrationInstanceService = new Rock.Model.RegistrationInstanceService( new RockContext() );
                var registrationInstances = registrationInstanceService.Queryable()
                    .Where( ri => ri.RegistrationTemplate.Guid == options.RegistrationTemplateGuid && ri.IsActive )
                    .OrderBy( ri => ri.Name )
                    .Select( ri => new ListItemBag { Text = ri.Name, Value = ri.Guid.ToString() } )
                    .ToList();

                return Ok( registrationInstances );
            }
        }

        /// <summary>
        /// Gets the registration template that the given instance uses.
        /// </summary>
        /// <returns>A <see cref="ListItemBag"/> object that represents the registration template.</returns>
        [HttpPost]
        [System.Web.Http.Route( "RegistrationInstancePickerGetRegistrationTemplateForInstance" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "acbccf4f-54d6-4c7c-8201-07fdefe87352" )]
        public IHttpActionResult RegistrationInstancePickerGetRegistrationTemplateForInstance( [FromBody] RegistrationInstancePickerGetRegistrationTemplateForInstanceOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var registrationInstance = new Rock.Model.RegistrationInstanceService( rockContext ).Get( options.RegistrationInstanceGuid );
                if ( registrationInstance == null )
                {
                    return NotFound();
                }

                return Ok( new ListItemBag { Text = registrationInstance.RegistrationTemplate.Name, Value = registrationInstance.RegistrationTemplate.Guid.ToString() } );
            }
        }

        #endregion

        #region Registration Template Picker

        /// <summary>
        /// Gets the registration templates and their categories that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which registration templates to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent a tree of registration templates.</returns>
        [HttpPost]
        [System.Web.Http.Route( "RegistrationTemplatePickerGetChildren" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "41eac873-20f3-4456-9fb4-746a1363807e" )]
        public IHttpActionResult RegistrationTemplatePickerGetChildren( [FromBody] RegistrationTemplatePickerGetChildrenOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = new CategoryClientService( rockContext, GetPerson( rockContext ) );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
                var queryOptions = new CategoryItemTreeOptions
                {
                    ParentGuid = options.ParentGuid,
                    GetCategorizedItems = options.ParentGuid.HasValue,
                    EntityTypeGuid = EntityTypeCache.Get<RegistrationTemplate>().Guid,
                    IncludeUnnamedEntityItems = false,
                    IncludeCategoriesWithoutChildren = false,
                    DefaultIconCssClass = "fa fa-list-ol",
                    LazyLoad = true,
                    SecurityGrant = grant
                };

                var items = clientService.GetCategorizedTreeItems( queryOptions );

                return Ok( items );
            }
        }

        #endregion

        #region Reminder Type Picker

        /// <summary>
        /// Gets the reminder types that can be displayed in the reminder type picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the reminder types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "ReminderTypePickerGetReminderTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "c1c338d2-6364-4217-81ec-7fc34e9218b6" )]
        public IHttpActionResult ReminderTypePickerGetReminderTypes( [FromBody] ReminderTypePickerGetReminderTypesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var reminderTypesQuery = new ReminderTypeService( rockContext ).Queryable();

                if ( options.EntityTypeGuid != null )
                {
                    reminderTypesQuery = reminderTypesQuery.Where( t => t.EntityType.Guid == options.EntityTypeGuid );
                }

                var orderedReminderTypes = reminderTypesQuery
                    .OrderBy( t => t.EntityType.FriendlyName )
                    .ThenBy( t => t.Name )
                    .Select( t => new ListItemBag
                    {
                        Value = t.Guid.ToString(),
                        Text = t.EntityType.FriendlyName + " - " + t.Name
                    } )
                    .ToList();

                return Ok( orderedReminderTypes );
            }
        }

        #endregion

        #region Remote Auths Picker

        /// <summary>
        /// Gets the remote auths that can be displayed in the remote auths picker.
        /// </summary>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the remote auths.</returns>
        [HttpPost]
        [System.Web.Http.Route( "RemoteAuthsPickerGetRemoteAuths" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "844D17E3-45FF-4A63-8BC7-32956A11CC94" )]
        public IHttpActionResult RemoteAuthsPickerGetRemoteAuths()
        {
            var items = new List<ListItemBag>();

            foreach ( var serviceEntry in AuthenticationContainer.Instance.Components )
            {
                var component = serviceEntry.Value.Value;

                if ( component.IsActive && component.RequiresRemoteAuthentication )
                {
                    var entityType = EntityTypeCache.Get( component.GetType() );
                    if ( entityType != null )
                    {
                        items.Add( new ListItemBag { Text = entityType.FriendlyName, Value = entityType.Guid.ToString() } );
                    }
                }
            }

            return Ok( items );
        }

        #endregion

        #region Report Picker

        /// <summary>
        /// Gets the reports and their categories that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which reports to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent a tree of reports.</returns>
        [HttpPost]
        [System.Web.Http.Route( "ReportPickerGetChildren" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "59545f7f-a27b-497c-8376-c85dfc360c11" )]
        public IHttpActionResult ReportPickerGetChildren( [FromBody] ReportPickerGetChildrenOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = new CategoryClientService( rockContext, GetPerson( rockContext ) );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
                var queryOptions = new CategoryItemTreeOptions
                {
                    ParentGuid = options.ParentGuid,
                    GetCategorizedItems = options.ParentGuid.HasValue,
                    EntityTypeGuid = EntityTypeCache.Get<Report>().Guid,
                    IncludeUnnamedEntityItems = false,
                    IncludeCategoriesWithoutChildren = false,
                    IncludeCategoryGuids = options.IncludeCategoryGuids == null || options.IncludeCategoryGuids.Count == 0 ? null : options.IncludeCategoryGuids,
                    ItemFilterPropertyName = options.EntityTypeGuid.HasValue ? "EntityTypeId" : null,
                    ItemFilterPropertyValue = options.EntityTypeGuid.HasValue ? EntityTypeCache.Get( options.EntityTypeGuid.Value ).Id.ToString() : "",
                    DefaultIconCssClass = "fa fa-list-ol",
                    LazyLoad = true,
                    SecurityGrant = grant
                };

                var items = clientService.GetCategorizedTreeItems( queryOptions );

                return Ok( items );
            }
        }

        #endregion

        #region Save Financial Account Form

        /// <summary>
        /// Saves the financial account.
        /// </summary>
        /// <param name="options">The options that describe what account should be saved.</param>
        /// <returns></returns>
        [Authenticate]
        [HttpPost]
        [System.Web.Http.Route( "SaveFinancialAccountFormSaveAccount" )]
        [Rock.SystemGuid.RestActionGuid( "544B6302-A9E0-430E-A1C1-7BCBC4A6230C" )]
        public SaveFinancialAccountFormSaveAccountResultBag SaveFinancialAccountFormSaveAccount( [FromBody] SaveFinancialAccountFormSaveAccountOptionsBag options )
        {
            // Validate the arguments
            if ( options?.TransactionCode.IsNullOrWhiteSpace() != false )
            {
                return new SaveFinancialAccountFormSaveAccountResultBag
                {
                    Title = "Sorry",
                    Detail = "The account information cannot be saved as there's not a valid transaction code to reference",
                    IsSuccess = false
                };
            }

            if ( options.SavedAccountName.IsNullOrWhiteSpace() )
            {
                return new SaveFinancialAccountFormSaveAccountResultBag
                {
                    Title = "Missing Account Name",
                    Detail = "Please enter a name to use for this account",
                    IsSuccess = false
                };
            }

            var currentPerson = GetPerson();
            var isAnonymous = currentPerson == null;

            using ( var rockContext = new RockContext() )
            {
                if ( isAnonymous )
                {
                    if ( options.Username.IsNullOrWhiteSpace() || options.Password.IsNullOrWhiteSpace() )
                    {
                        return new SaveFinancialAccountFormSaveAccountResultBag
                        {
                            Title = "Missing Information",
                            Detail = "A username and password are required when saving an account",
                            IsSuccess = false
                        };
                    }

                    var userLoginService = new UserLoginService( rockContext );

                    if ( userLoginService.GetByUserName( options.Username ) != null )
                    {
                        return new SaveFinancialAccountFormSaveAccountResultBag
                        {
                            Title = "Invalid Username",
                            Detail = "The selected Username is already being used. Please select a different Username",
                            IsSuccess = false
                        };
                    }

                    if ( !UserLoginService.IsPasswordValid( options.Password ) )
                    {
                        return new SaveFinancialAccountFormSaveAccountResultBag
                        {
                            Title = "Invalid Password",
                            Detail = UserLoginService.FriendlyPasswordRules(),
                            IsSuccess = false
                        };
                    }
                }

                // Load the gateway from the database
                var financialGatewayService = new FinancialGatewayService( rockContext );
                var financialGateway = financialGatewayService.Get( options.GatewayGuid );
                var gateway = financialGateway?.GetGatewayComponent();

                if ( gateway is null )
                {
                    return new SaveFinancialAccountFormSaveAccountResultBag
                    {
                        Title = "Invalid Gateway",
                        Detail = "Sorry, the financial gateway information is not valid.",
                        IsSuccess = false
                    };
                }

                // Load the transaction from the database
                var financialTransactionService = new FinancialTransactionService( rockContext );
                var transaction = financialTransactionService.GetByTransactionCode( financialGateway.Id, options.TransactionCode );
                var transactionPersonAlias = transaction?.AuthorizedPersonAlias;
                var transactionPerson = transactionPersonAlias?.Person;
                var paymentDetail = transaction?.FinancialPaymentDetail;

                if ( transactionPerson is null || paymentDetail is null )
                {
                    return new SaveFinancialAccountFormSaveAccountResultBag
                    {
                        Title = "Invalid Transaction",
                        Detail = "Sorry, the account information cannot be saved as there's not a valid transaction to reference",
                        IsSuccess = false
                    };
                }

                // Create the login if needed
                if ( isAnonymous )
                {
                    var user = UserLoginService.Create(
                        rockContext,
                        transactionPerson,
                        AuthenticationServiceType.Internal,
                        EntityTypeCache.Get( SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                        options.Username,
                        options.Password,
                        false );

                    var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, currentPerson );
                    // TODO mergeFields.Add( "ConfirmAccountUrl", RootPath + "ConfirmAccount" );
                    mergeFields.Add( "Person", transactionPerson );
                    mergeFields.Add( "User", user );

                    var emailMessage = new RockEmailMessage( SystemGuid.SystemCommunication.SECURITY_CONFIRM_ACCOUNT.AsGuid() );
                    emailMessage.AddRecipient( new RockEmailMessageRecipient( transactionPerson, mergeFields ) );
                    // TODO emailMessage.AppRoot = ResolveRockUrl( "~/" );
                    // TODO emailMessage.ThemeRoot = ResolveRockUrl( "~~/" );
                    emailMessage.CreateCommunicationRecord = false;
                    emailMessage.Send();
                }

                var savedAccount = new FinancialPersonSavedAccount
                {
                    PersonAliasId = transactionPersonAlias.Id,
                    ReferenceNumber = options.TransactionCode,
                    GatewayPersonIdentifier = options.GatewayPersonIdentifier,
                    Name = options.SavedAccountName,
                    TransactionCode = options.TransactionCode,
                    FinancialGatewayId = financialGateway.Id,
                    FinancialPaymentDetail = new FinancialPaymentDetail
                    {
                        AccountNumberMasked = paymentDetail.AccountNumberMasked,
                        CurrencyTypeValueId = paymentDetail.CurrencyTypeValueId,
                        CreditCardTypeValueId = paymentDetail.CreditCardTypeValueId,
                        NameOnCard = paymentDetail.NameOnCard,
                        ExpirationMonth = paymentDetail.ExpirationMonth,
                        ExpirationYear = paymentDetail.ExpirationYear,
                        BillingLocationId = paymentDetail.BillingLocationId
                    }
                };

                var financialPersonSavedAccountService = new FinancialPersonSavedAccountService( rockContext );
                financialPersonSavedAccountService.Add( savedAccount );

                System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", RockRequestContext.CurrentPerson );
                rockContext.SaveChanges();

                return new SaveFinancialAccountFormSaveAccountResultBag
                {
                    Title = "Success",
                    Detail = "The account has been saved for future use",
                    IsSuccess = true
                };
            }
        }

        #endregion

        #region Schedule Picker

        /// <summary>
        /// Gets the schedules and their categories that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which schedules to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent a tree of schedules.</returns>
        [HttpPost]
        [System.Web.Http.Route( "SchedulePickerGetChildren" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "60447abf-18f5-4ad1-a191-3a614408653b" )]
        public IHttpActionResult SchedulePickerGetChildren( [FromBody] SchedulePickerGetChildrenOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = new CategoryClientService( rockContext, GetPerson( rockContext ) );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
                var queryOptions = new CategoryItemTreeOptions
                {
                    ParentGuid = options.ParentGuid,
                    GetCategorizedItems = options.ParentGuid.HasValue,
                    EntityTypeGuid = EntityTypeCache.Get<Schedule>().Guid,
                    IncludeUnnamedEntityItems = false,
                    IncludeCategoriesWithoutChildren = false,
                    IncludeInactiveItems = options.IncludeInactiveItems,
                    DefaultIconCssClass = "fa fa-list-ol",
                    LazyLoad = true,
                    SecurityGrant = grant
                };

                var items = clientService.GetCategorizedTreeItems( queryOptions );

                return Ok( items );
            }
        }

        #endregion

        #region Step Program Picker

        /// <summary>
        /// Gets the step programs that can be displayed in the step program picker.
        /// </summary>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the step programs.</returns>
        [HttpPost]
        [System.Web.Http.Route( "StepProgramPickerGetStepPrograms" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "6C7816B0-D41D-4081-B998-0B42B542111F" )]
        public IHttpActionResult StepProgramPickerGetStepPrograms()
        {
            var items = new List<ListItemBag>();

            var stepProgramService = new StepProgramService( new RockContext() );
            var stepPrograms = stepProgramService.Queryable().AsNoTracking()
                .Where( sp => sp.IsActive )
                .OrderBy( sp => sp.Order )
                .ThenBy( sp => sp.Name )
                .ToList();

            foreach ( var stepProgram in stepPrograms )
            {
                var li = new ListItemBag { Text = stepProgram.Name, Value = stepProgram.Guid.ToString() };
                items.Add( li );
            }

            return Ok( items );
        }

        #endregion

        #region Step Status Picker

        /// <summary>
        /// Gets the step statuses that can be displayed in the step status picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the step statuses.</returns>
        [HttpPost]
        [System.Web.Http.Route( "StepStatusPickerGetStepStatuses" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "5B4E7419-266C-4235-93B7-8D0DE0E80D2B" )]
        public IHttpActionResult StepStatusPickerGetStepStatuses( [FromBody] StepStatusPickerGetStepStatusesOptionsBag options )
        {
            if ( !options.StepProgramGuid.HasValue )
            {
                return NotFound();
            }

            var items = new List<ListItemBag>();
            int stepProgramId = StepProgramCache.GetId( options.StepProgramGuid.Value ) ?? 0;

            var stepStatusService = new StepStatusService( new RockContext() );
            var statuses = stepStatusService.Queryable().AsNoTracking()
                .Where( ss =>
                    ss.StepProgramId == stepProgramId &&
                    ss.IsActive )
                .OrderBy( ss => ss.Order )
                .ThenBy( ss => ss.Name )
                .ToList();

            foreach ( var status in statuses )
            {
                var li = new ListItemBag { Text = status.Name, Value = status.Guid.ToString() };
                items.Add( li );
            }

            return Ok( items );
        }

        #endregion

        #region Step Type Picker

        /// <summary>
        /// Gets the step types that can be displayed in the step type picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the step types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "StepTypePickerGetStepTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "9BC4C3BA-573E-4FB4-A4FC-938D40BED2BE" )]
        public IHttpActionResult StepTypePickerGetStepTypes( [FromBody] StepTypePickerGetStepTypesOptionsBag options )
        {
            if ( !options.StepProgramGuid.HasValue )
            {
                return NotFound();
            }

            var items = new List<ListItemBag>();
            int stepProgramId = StepProgramCache.GetId( options.StepProgramGuid.Value ) ?? 0;

            var stepTypeService = new StepTypeService( new RockContext() );
            var stepTypes = stepTypeService.Queryable().AsNoTracking()
                .Where( st =>
                    st.StepProgramId == stepProgramId &&
                    st.IsActive )
                .OrderBy( st => st.Order )
                .ThenBy( st => st.Name )
                .ToList();

            foreach ( var stepType in stepTypes )
            {
                var li = new ListItemBag { Text = stepType.Name, Value = stepType.Guid.ToString() };
                items.Add( li );
            }

            return Ok( items );
        }

        #endregion

        #region Streak Type Picker

        /// <summary>
        /// Gets the streak types that can be displayed in the streak type picker.
        /// </summary>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent the streak types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "StreakTypePickerGetStreakTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "78D0A6D1-317E-4CB7-98BB-AF9194AD3C94" )]
        public IHttpActionResult StreakTypePickerGetStreakTypes()
        {
            var items = new List<ListItemBag>();

            var streakTypes = StreakTypeCache.All()
                .Where( st => st.IsActive )
                .OrderBy( st => st.Name )
                .ThenBy( st => st.Id )
                .ToList();

            foreach ( var streakType in streakTypes )
            {
                var li = new ListItemBag { Text = streakType.Name, Value = streakType.Guid.ToString() };
                items.Add( li );
            }

            return Ok( items );
        }

        #endregion

        #region Structured Content Editor

        /// <summary>
        /// Gets the structured content editor configuration.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>The structured content editor configuration.</returns>
        [HttpPost]
        [System.Web.Http.Route( "StructuredContentEditorGetConfiguration" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "71AD8E7A-3B38-4FC0-A4C7-95DB77F070F6" )]
        public IHttpActionResult StructuredContentEditorGetConfiguration( [FromBody] StructuredContentEditorGetConfigurationOptionsBag options )
        {
            var structuredContentToolsConfiguration = string.Empty;
            if ( options.StructuredContentToolsValueGuid.HasValue )
            {
                var structuredContentToolsValue = DefinedValueCache.Get( options.StructuredContentToolsValueGuid.Value );
                if ( structuredContentToolsValue != null )
                {
                    structuredContentToolsConfiguration = structuredContentToolsValue.Description;
                }
            }

            if ( structuredContentToolsConfiguration.IsNullOrWhiteSpace() )
            {
                var structuredContentToolsValue = DefinedValueCache.Get( SystemGuid.DefinedValue.STRUCTURE_CONTENT_EDITOR_DEFAULT );
                if ( structuredContentToolsValue != null )
                {
                    structuredContentToolsConfiguration = structuredContentToolsValue.Description;
                }
            }

            return Ok( new StructuredContentEditorConfigurationBag
            {
                ToolsScript = structuredContentToolsConfiguration
            } );
        }

        #endregion

        #region Workflow Action Type Picker

        /// <summary>
        /// Gets the workflow action types and their categories that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which workflow action types to load.</param>
        /// <returns>A List of <see cref="TreeItemBag"/> objects that represent a tree of workflow action types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "WorkflowActionTypePickerGetChildren" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "4275ae7f-16ab-4720-a79f-bf7b5ca979e8" )]
        public IHttpActionResult WorkflowActionTypePickerGetChildren( [FromBody] WorkflowActionTypePickerGetChildrenOptionsBag options )
        {
            var list = new List<TreeItemBag>();

            // Folders
            if ( options.ParentId == 0 )
            {
                // Root
                foreach ( var category in ActionContainer.Instance.Categories )
                {
                    var item = new TreeItemBag();
                    item.Value = category.Key.ToString();
                    item.Text = category.Value;
                    item.HasChildren = true;
                    item.IconCssClass = "fa fa-folder";
                    list.Add( item );
                }
            }
            // Action Types
            else if ( options.ParentId < 0 && ActionContainer.Instance.Categories.ContainsKey( options.ParentId ) )
            {
                string categoryName = ActionContainer.Instance.Categories[options.ParentId];
                var categorizedActions = GetCategorizedWorkflowActions();
                if ( categorizedActions.ContainsKey( categoryName ) )
                {
                    foreach ( var entityType in categorizedActions[categoryName].OrderBy( e => e.FriendlyName ) )
                    {
                        var item = new TreeItemBag();
                        item.Value = entityType.Guid.ToString();
                        item.Text = ActionContainer.GetComponentName( entityType.Name );
                        item.HasChildren = false;
                        item.IconCssClass = "fa fa-cube";
                        list.Add( item );
                    }
                }
            }

            return Ok( list.OrderBy( i => i.Text ) );
        }

        /// <summary>
        /// Gets the categorized workflow actions.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, List<EntityTypeCache>> GetCategorizedWorkflowActions()
        {
            var categorizedActions = new Dictionary<string, List<EntityTypeCache>>();

            foreach ( var action in ActionContainer.Instance.Dictionary.Select( d => d.Value.Value ) )
            {
                string categoryName = "Uncategorized";

                var actionType = action.GetType();
                var obj = actionType.GetCustomAttributes( typeof( ActionCategoryAttribute ), true ).FirstOrDefault();
                if ( obj != null )
                {
                    var actionCategory = obj as ActionCategoryAttribute;
                    if ( actionCategory != null )
                    {
                        categoryName = actionCategory.CategoryName;
                    }
                }

                // "HideFromUser" is a special category name that is used to hide
                // workflow actions from showing up to the user. System user only.
                if ( !categoryName.Equals( "HideFromUser", System.StringComparison.OrdinalIgnoreCase ) )
                {
                    categorizedActions.AddOrIgnore( categoryName, new List<EntityTypeCache>() );
                    categorizedActions[categoryName].Add( action.EntityType );
                }
            }

            return categorizedActions;
        }

        #endregion

        #region Workflow Picker

        /// <summary>
        /// Gets the workflows and their categories that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which workflows to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent a tree of workflows.</returns>
        [HttpPost]
        [System.Web.Http.Route( "WorkflowPickerGetWorkflows" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "93024bbe-4941-4f84-a5e7-754cf30c03d3" )]
        public IHttpActionResult WorkflowPickerGetWorkflows( [FromBody] WorkflowPickerGetWorkflowsOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( options.WorkflowTypeGuid == null )
                {
                    return NotFound();
                }

                var workflowService = new Rock.Model.WorkflowService( rockContext );
                var workflows = workflowService.Queryable()
                    .Where( w =>
                        w.WorkflowType.Guid == options.WorkflowTypeGuid &&
                        w.ActivatedDateTime.HasValue &&
                        !w.CompletedDateTime.HasValue )
                    .OrderBy( w => w.Name )
                    .Select( w => new ListItemBag { Value = w.Guid.ToString(), Text = w.Name } )
                    .ToList();

                return Ok( workflows );
            }
        }

        /// <summary>
        /// Gets the workflow type that the given instance uses.
        /// </summary>
        /// <returns>A <see cref="ListItemBag"/> object that represents the workflow type.</returns>
        [HttpPost]
        [System.Web.Http.Route( "WorkflowPickerGetWorkflowTypeForWorkflow" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "a41c755c-ffcb-459c-a67a-f0311158976a" )]
        public IHttpActionResult WorkflowPickerGetWorkflowTypeForWorkflow( [FromBody] WorkflowPickerGetWorkflowTypeForWorkflowOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var workflow = new Rock.Model.WorkflowService( rockContext ).Get( options.WorkflowGuid );
                if ( workflow == null )
                {
                    return NotFound();
                }

                return Ok( new ListItemBag { Text = workflow.WorkflowType.Name, Value = workflow.WorkflowType.Guid.ToString() } );
            }
        }

        #endregion

        #region Workflow Type Picker

        /// <summary>
        /// Gets the workflow types and their categories that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which workflow types to load.</param>
        /// <returns>A List of <see cref="ListItemBag"/> objects that represent a tree of workflow types.</returns>
        [HttpPost]
        [System.Web.Http.Route( "WorkflowTypePickerGetWorkflowTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "622EE929-7A18-46BE-9AEA-9E0725293612" )]
        public IHttpActionResult WorkflowTypePickerGetWorkflowTypes( [FromBody] WorkflowTypePickerGetWorkflowTypesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = new CategoryClientService( rockContext, GetPerson( rockContext ) );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                var items = clientService.GetCategorizedTreeItems( new CategoryItemTreeOptions
                {
                    ParentGuid = options.ParentGuid,
                    GetCategorizedItems = true,
                    EntityTypeGuid = Rock.SystemGuid.EntityType.WORKFLOW_TYPE.AsGuid(),
                    IncludeUnnamedEntityItems = true,
                    IncludeCategoriesWithoutChildren = false,
                    IncludeInactiveItems = options.IncludeInactiveItems,
                    LazyLoad = false,
                    SecurityGrant = grant
                } );

                return Ok( items );
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Retrieve a list of ListItems representing components for the given container type
        /// </summary>
        /// <param name="containerType"></param>
        /// <returns>A list of ListItems representing components</returns>
        private List<ListItemBag> GetComponentListItems( string containerType )
        {
            return GetComponentListItems( containerType, ( x ) => true );
        }

        /// <summary>
        /// Retrieve a list of ListItemBags representing components for the given container type. Filters any components
        /// out that don't pass the given validator
        /// </summary>
        /// <param name="containerType"></param>
        /// <param name="isValidComponentChecker"></param>
        /// <returns>A list of ListItemBags representing components</returns>
        private List<ListItemBag> GetComponentListItems( string containerType, Func<Component, bool> isValidComponentChecker )
        {
            if ( containerType.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var resolvedContainerType = Container.ResolveContainer( containerType );

            if ( resolvedContainerType == null )
            {
                return null;
            }

            var instanceProperty = resolvedContainerType.GetProperty( "Instance" );

            if ( instanceProperty == null )
            {
                return null;
            }

            var container = instanceProperty.GetValue( null, null ) as IContainer;
            var componentDictionary = container?.Dictionary;

            var items = new List<ListItemBag>();

            foreach ( var component in componentDictionary )
            {
                var componentValue = component.Value.Value;
                var entityType = EntityTypeCache.Get( componentValue.GetType() );

                if ( !componentValue.IsActive || entityType == null || !isValidComponentChecker( componentValue ) )
                {
                    continue;
                }

                var componentName = component.Value.Key;

                // If the component name already has a space then trust
                // that they are using the exact name formatting they want.
                if ( !componentName.Contains( ' ' ) )
                {
                    componentName = componentName.SplitCase();
                }

                items.Add( new ListItemBag
                {
                    Text = componentName,
                    Value = entityType.Guid.ToString().ToUpper()
                } );
            }

            return items;
        }

        /// <summary>
        /// Get the attributes for the given object
        /// </summary>
        /// <param name="model">The object to find the attributes of</param>
        /// <returns>A list of attributes in a form the Attribute Values Container can use</returns>
        private List<PublicAttributeBag> GetAttributes( IHasInheritedAttributes model )
        {
            using ( var rockContext = new RockContext() )
            {
                Type entityType = model.GetType();
                if ( entityType.IsDynamicProxyType() )
                {
                    entityType = entityType.BaseType;
                }

                var attributes = new List<Rock.Web.Cache.AttributeCache>();

                var entityTypeCache = EntityTypeCache.Get( entityType );

                List<Rock.Web.Cache.AttributeCache> allAttributes = null;
                Dictionary<int, List<int>> inheritedAttributes = null;

                //
                // If this entity can provide inherited attribute information then
                // load that data now. If they don't provide any then generate empty lists.
                //
                if ( model is Rock.Attribute.IHasInheritedAttributes entityWithInheritedAttributes )
                {
                    allAttributes = entityWithInheritedAttributes.GetInheritedAttributes( rockContext );
                    inheritedAttributes = entityWithInheritedAttributes.GetAlternateEntityIdsByType( rockContext );
                }

                allAttributes = allAttributes ?? new List<AttributeCache>();
                inheritedAttributes = inheritedAttributes ?? new Dictionary<int, List<int>>();

                //
                // Get all the attributes that apply to this entity type and this entity's
                // properties match any attribute qualifiers.
                //
                var entityTypeId = entityTypeCache?.Id;

                if ( entityTypeCache != null )
                {
                    var entityTypeAttributesList = AttributeCache.GetByEntityType( entityTypeCache.Id );
                    if ( entityTypeAttributesList.Any() )
                    {
                        var entityTypeQualifierColumnPropertyNames = entityTypeAttributesList.Select( a => a.EntityTypeQualifierColumn ).Distinct().Where( a => !string.IsNullOrWhiteSpace( a ) ).ToList();
                        Dictionary<string, object> propertyValues = new Dictionary<string, object>( StringComparer.OrdinalIgnoreCase );
                        foreach ( var propertyName in entityTypeQualifierColumnPropertyNames )
                        {
                            System.Reflection.PropertyInfo propertyInfo = entityType.GetProperty( propertyName ) ?? entityType.GetProperties().Where( a => a.Name.Equals( propertyName, StringComparison.OrdinalIgnoreCase ) ).FirstOrDefault();
                            if ( propertyInfo != null )
                            {
                                propertyValues.AddOrIgnore( propertyName, propertyInfo.GetValue( model, null ) );
                            }
                        }

                        var entityTypeAttributesForQualifier = entityTypeAttributesList.Where( x =>
                          string.IsNullOrEmpty( x.EntityTypeQualifierColumn ) ||
                                 ( propertyValues.ContainsKey( x.EntityTypeQualifierColumn ) &&
                                 ( string.IsNullOrEmpty( x.EntityTypeQualifierValue ) ||
                                 ( propertyValues[x.EntityTypeQualifierColumn] ?? "" ).ToString() == x.EntityTypeQualifierValue ) ) );

                        attributes.AddRange( entityTypeAttributesForQualifier );
                    }
                }

                //
                // Append these attributes to our inherited attributes, in order.
                //
                foreach ( var attribute in attributes.OrderBy( a => a.Order ) )
                {
                    allAttributes.Add( attribute );
                }
                var attributeList = allAttributes
                    .Where( a => a.IsActive )
                    .Select( a => new PublicAttributeBag
                    {
                        AttributeGuid = a.Guid,
                        FieldTypeGuid = FieldTypeCache.Get( a.FieldTypeId ).Guid,
                        Name = a.Name,
                        Key = a.Key,
                        Description = a.Description,
                        IsRequired = a.IsRequired,
                        Order = a.Order,
                        ConfigurationValues = a.ConfigurationValues
                    } )
                    .ToList();

                return attributeList;
            }
        }

        /// <summary>
        /// Converts the TreeViewItem to TreeItemBag.
        /// </summary>
        /// <param name="item">The TreeViewItem to be converted.</param>
        /// <returns>The item as a TreeItemBag</returns>
        private TreeItemBag convertTreeViewItemToTreeItemBag( TreeViewItem item )
        {
            return new TreeItemBag
            {
                Value = item.Id,
                Text = item.Name,
                IsFolder = item.HasChildren,
                HasChildren = item.HasChildren,
                IconCssClass = item.IconCssClass,
                IsActive = item.IsActive,
                Children = item.Children?.Select( convertTreeViewItemToTreeItemBag ).ToList()
            };
        }

        #endregion
    }
}