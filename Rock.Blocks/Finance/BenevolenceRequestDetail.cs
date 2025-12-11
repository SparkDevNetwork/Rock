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
using System.Text;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.BenevolenceRequestDetail;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Represents a block that displays and manages the details of a specific benevolence request.
    /// </summary>
    /// <remarks>This block is used to view, edit, and manage the details of a benevolence request, including
    /// associated case workers, request documents, and request results. It provides functionality for creating new
    /// requests, updating existing ones, and managing related data such as requesters, campuses, and workflows. <para>
    /// The block includes support for displaying government identifiers, managing workflows, and handling race and
    /// ethnicity options for individuals associated with the request. </para> <para> This block is primarily used in
    /// the context of financial assistance workflows and is designed to integrate with other components in the system,
    /// such as linked pages for workflow details and benevolence request statements. </para></remarks>

    [DisplayName( "Benevolence Request Detail" )]
    [Category( "Finance" )]
    [Description( "Displays the details of a particular benevolence request." )]
    [IconCssClass( "ti ti-question-mark" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [SecurityRoleField( "Case Worker Role",
        Description = "The security role to draw case workers from",
        IsRequired = false,
        Key = AttributeKey.CaseWorkerRole,
        Order = 1 )]

    [BooleanField( "Display Government Id",
        Key = AttributeKey.DisplayGovernmentId,
        Description = "Display the government identifier.",
        DefaultBooleanValue = true,
        Order = 2 )]

    [LinkedPage( "Benevolence Request Statement Page",
        Description = "The page which summarizes a benevolence request for printing",
        IsRequired = true,
        Key = AttributeKey.BenevolenceRequestStatementPage,
        Order = 3 )]

    [LinkedPage(
        "Workflow Detail Page",
        Description = "Page used to display details about a workflow.",
        Order = 4,
        Key = AttributeKey.WorkflowDetailPage,
        DefaultValue = Rock.SystemGuid.Page.WORKFLOW_DETAIL )]

    [LinkedPage(
        "Workflow Entry Page",
        Description = "Page used to launch a new workflow of the selected type.",
        Order = 5,
        Key = AttributeKey.WorkflowEntryPage,
        DefaultValue = Rock.SystemGuid.Page.WORKFLOW_ENTRY )]

    [CustomDropdownListField(
        "Race",
        Key = AttributeKey.RaceOption,
        Description = "Allow or require race to be selected. This field will not be saved unless a person record is created before saving the request.",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Hide",
        Category = "Individual",
        Order = 6 )]

    [CustomDropdownListField(
        "Ethnicity",
        Key = AttributeKey.EthnicityOption,
        Description = "Allow or require Ethnicity to be selected. This field will not be saved unless a person record is created before saving the request.",
        ListSource = ListSource.HIDE_OPTIONAL_REQUIRED,
        IsRequired = false,
        DefaultValue = "Hide",
        Category = "Individual",
        Order = 7 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "9B1BE948-F14A-4889-981D-75B86E6D458D" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "5CA8DF26-D85C-4D70-822A-15D4B1021FBC" )]
    [Rock.SystemGuid.BlockTypeGuid( "34275D0E-BC7E-4A9C-913E-623D086159A1" )]
    public class BenevolenceRequestDetail : RockEntityDetailBlockType<BenevolenceRequest, BenevolenceRequestBag>
    {
        #region Fields

        Guid _countryCodeTypeGuid = Rock.SystemGuid.DefinedType.COMMUNICATION_PHONE_COUNTRY_CODE.AsGuid();

        Guid _homePhoneGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid();
        Guid _cellPhoneGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid();
        Guid _workPhoneGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid();

        #endregion Fields

        #region Properties

        // Services
        private PersonAliasService PersonAliasService => new PersonAliasService( RockContext );
        private LocationService LocationService => new LocationService( RockContext );
        private CampusService CampusService => new CampusService( RockContext );
        private DefinedValueService DefinedValueService => new DefinedValueService( RockContext );
        private BenevolenceTypeService BenevolenceTypeService => new BenevolenceTypeService( RockContext );
        private BinaryFileService BinaryFileService => new BinaryFileService( RockContext );
        private BenevolenceRequestDocumentService BenevolenceRequestDocumentService => new BenevolenceRequestDocumentService( RockContext );

        #endregion Properties

        #region Keys

        /// <summary>
        /// Provides a collection of constant string keys used to identify specific attributes within the application.
        /// </summary>
        /// <remarks>The <see cref="AttributeKey"/> class contains predefined string constants that
        /// represent attribute keys. These keys are used to reference specific attributes in a consistent and
        /// centralized manner, reducing the likelihood of errors.</remarks>
        private static class AttributeKey
        {
            public const string CaseWorkerRole = "CaseWorkerRole";
            public const string DisplayGovernmentId = "DisplayGovernmentId";
            public const string BenevolenceRequestStatementPage = "BenevolenceRequestStatementPage";
            public const string WorkflowDetailPage = "WorkflowDetailPage";
            public const string WorkflowEntryPage = "WorkflowEntryPage";
            public const string RaceOption = "RaceOption";
            public const string EthnicityOption = "EthnicityOption";
        }

        /// <summary>
        /// Provides a source of predefined list values used for configuration or validation purposes.
        /// </summary>
        /// <remarks>This class contains constants that represent commonly used list values.  These values
        /// can be used to standardize input or enforce specific options in application logic.</remarks>
        private static class ListSource
        {
            public const string HIDE_OPTIONAL_REQUIRED = "Hide,Optional,Required";
        }

        /// <summary>
        /// Provides a collection of constant keys used for identifying page parameters.
        /// </summary>
        /// <remarks>This class contains string constants that represent specific parameter keys used in
        /// page navigation or data exchange. These keys are intended to be used as identifiers for passing or
        /// retrieving values in a consistent manner.</remarks>
        private static class PageParameterKey
        {
            public const string BenevolenceRequestId = "BenevolenceRequestId";
        }

        /// <summary>
        /// Provides constant keys used for navigation URLs within the application.
        /// </summary>
        /// <remarks>This class contains string constants that represent specific navigation targets.
        /// These keys are typically used to identify pages or routes in the application's navigation system.</remarks>
        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
            public const string BenevolenceRequestStatementPage = "BenevolenceRequestStatementPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<BenevolenceRequestBag, BenevolenceRequestDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );

            MinimizeDataBeforeSend( box );

            return box;
        }

        /// <summary>
        /// Minimizes the data sent to the client by removing unnecessary details from the box and it's contents.
        /// </summary>
        /// <param name="box">The box containing the details to minimize.</param>
        public void MinimizeDataBeforeSend( DetailBlockBox<BenevolenceRequestBag, BenevolenceRequestDetailOptionsBag> box )
        {
            // Minimize data sent to client for CaseWorkersByRole
            foreach ( var requestType in box.Options.BenevolenceRequestTypes )
            {
                requestType.Options.AdditionalSettingsJson = string.Empty;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private BenevolenceRequestDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new BenevolenceRequestDetailOptionsBag();

            var caseWorkerRoleGuid = GetAttributeValue( AttributeKey.CaseWorkerRole ).AsGuid();

            #region Attribute Options

            options.DisplayGovernmentIdAttribute = GetAttributeValue( AttributeKey.DisplayGovernmentId ).AsBoolean();
            options.BenevolenceRequestStatementPageAttribute = GetAttributeValue( AttributeKey.BenevolenceRequestStatementPage ).AsGuid();
            options.WorkflowDetailPageAttribute = GetAttributeValue( AttributeKey.WorkflowDetailPage ).AsGuid();
            options.WorkflowEntryPageAttribute = GetAttributeValue( AttributeKey.WorkflowEntryPage ).AsGuid();
            options.RaceOptionAttribute = GetAttributeValue( AttributeKey.RaceOption ).ToString();
            options.EthnicityOptionAttribute = GetAttributeValue( AttributeKey.EthnicityOption ).ToString();

            #endregion Attribute Options

            options.CaseWorkersByRole = new GroupMemberService( RockContext )
                .Queryable( "Person, Group" )
                .AsNoTracking()
                .Where( gm => gm.Group.Guid == caseWorkerRoleGuid )
                .Select( gm => new
                {
                    FirstOrNickName = gm.Person.NickName,
                    LastName = gm.Person.LastName,
                    PersonAliasGuid = gm.Person.PrimaryAliasGuid.ToString(),
                } )
                .ToList() // Materialize to memory then format to avoid SQL issues
                .Select( a => new ListItemBag
                {
                    Value = a.PersonAliasGuid,
                    Text = $"{a.FirstOrNickName} {a.LastName}",
                } )
                .ToList();

            options.CountryCodesEnabled = DefinedValueService
                .GetByDefinedTypeId( DefinedTypeCache.GetId( _countryCodeTypeGuid ).Value )
                .Where( dv => dv.IsActive )
                .Select( dv => dv.Value )
                .Distinct()
                .Count() > 1;

            options.BenevolenceRequestTypes = BenevolenceTypeService
                .Queryable()
                .OrderBy( type => type.Id )
                .Select( type => new BenevolenceTypeBag
                {
                    Id = type.Id,
                    Name = type.Name,
                    IsActive = type.IsActive,
                    LavaDetails = type.RequestLavaTemplate,
                    Options = new BenevolenceTypeOptionsBag
                    {
                        // Grab them in raw form and mutate below
                        AdditionalSettingsJson = type.AdditionalSettingsJson,
                        IsShowingFinancialResults = type.ShowFinancialResults,
                    }
                } )
                .ToList();

            // Handle BenevolenceRequestType configurable options including any staging transforms needed on AdditionalSettingsJson
            var commonMergeFields = new Lava.CommonMergeFieldsOptions();
            var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, null, commonMergeFields );
            mergeFields.Add( "BenevolenceRequest", new BenevolenceRequestService( RockContext ).Get( RequestContext.GetPageParameter( PageParameterKey.BenevolenceRequestId ) ) );
            foreach ( var requestType in options.BenevolenceRequestTypes )
            {
                // Replace the raw LavaDetails above with the merged, ready to display text
                requestType.LavaDetails = requestType.LavaDetails.ResolveMergeFields( mergeFields );

                // Set the MaximumDocuments per request for the type
                var maximumDocuments = requestType.Options.AdditionalSettingsJson.FromJsonOrNull<BenevolenceType.AdditionalSettings>()?.MaximumNumberOfDocuments;
                requestType.Options.MaximumDocuments = maximumDocuments.HasValue
                    ? maximumDocuments.Value
                    : 6;
            }

            options.RequestStatusValues = ( DefinedTypeCache.Get( SystemGuid.DefinedType.BENEVOLENCE_REQUEST_STATUS.AsGuid() ) != null )
                ? DefinedValueService
                    .GetByDefinedTypeId( DefinedTypeCache.GetId( SystemGuid.DefinedType.BENEVOLENCE_REQUEST_STATUS.AsGuid() ).Value )
                    .OrderBy( definedValue => definedValue.Id )
                    .Select( definedValue => new ListItemBag
                    {
                        Value = definedValue.Id.ToString(),
                        Text = definedValue.Value,
                    } )
                    .ToList()
                : new List<ListItemBag>();

            options.ConnectionStatusValues = ( DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ) != null )
                ? DefinedValueService
                    .GetByDefinedTypeId( DefinedTypeCache.GetId( SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ).Value )
                    .OrderBy( definedValue => definedValue.Id )
                    .Select( definedValue => new ListItemBag
                    {
                        Value = definedValue.Id.ToString(),
                        Text = definedValue.Value,
                    } )
                    .ToList()
                : new List<ListItemBag>();

            options.ResultTypeValues = ( DefinedTypeCache.Get( SystemGuid.DefinedType.BENEVOLENCE_RESULT_TYPE.AsGuid() ) != null )
                ? DefinedValueService
                    .GetByDefinedTypeId( DefinedTypeCache.GetId( SystemGuid.DefinedType.BENEVOLENCE_RESULT_TYPE.AsGuid() ).Value )
                    .OrderBy( definedValue => definedValue.Id )
                    .Select( definedValue => new ListItemBag
                    {
                        Value = definedValue.Id.ToString(),
                        Text = definedValue.Value,
                    } )
                    .ToList()
                : new List<ListItemBag>();

            options.BenevolenceDocumentBinaryFileTypeGuid = SystemGuid.BinaryFiletype.BENEVOLENCE_REQUEST_DOCUMENTS.AsGuid();

            return options;
        }

        /// <summary>
        /// Validates the BenevolenceRequest for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="benevolenceRequest">The BenevolenceRequest to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the BenevolenceRequest is valid, <c>false</c> otherwise.</returns>
        private bool ValidateBenevolenceRequest( BenevolenceRequest benevolenceRequest, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<BenevolenceRequestBag, BenevolenceRequestDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {BenevolenceRequest.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( BenevolenceRequest.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( BenevolenceRequest.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( BenevolenceRequest entity, ValidPropertiesBox<BenevolenceRequestBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            var activeCountryCodes = DefinedValueService
                .GetByDefinedTypeId( DefinedTypeCache.GetId( _countryCodeTypeGuid ).Value )
                .Where( dv => dv.IsActive )
                .Select( dv => dv.Value )
                .Distinct()
                .ToList();

            UpdateRequesterProperties( entity, box, activeCountryCodes );
            UpdateCaseWorkerProperties( entity, box );
            UpdateCampusProperties( entity, box );
            UpdateRequestDocuments( entity, box );
            UpdateAttributeValues( entity, box );
            UpdateDetailProperties( entity, box );

            return true;
        }

        /// <inheritdoc/>
        protected override BenevolenceRequest GetInitialEntity()
        {
            return GetInitialEntity<BenevolenceRequest, BenevolenceRequestService>( RockContext, PageParameterKey.BenevolenceRequestId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl(),
                [NavigationUrlKey.BenevolenceRequestStatementPage] = this.GetLinkedPageUrl(
                    AttributeKey.BenevolenceRequestStatementPage,
                    "BenevolenceRequestId",
                    "((Value))"
                ),

            };
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out BenevolenceRequest entity, out BlockActionResult error )
        {
            var entityService = new BenevolenceRequestService( RockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // Create a new entity.
                entity = new BenevolenceRequest();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{BenevolenceRequest.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${BenevolenceRequest.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates the properties of the requester in the specified <see cref="BenevolenceRequest"/> entity based on
        /// the provided <see cref="ValidPropertiesBox{T}"/> and a list of active country codes.
        /// </summary>
        /// <remarks>This method updates various properties of the <paramref name="entity"/> based on the
        /// values in the <paramref name="box"/>. If the requester information is valid, it updates fields such as the
        /// requester's name, email, phone numbers, government ID, connection status, and location. Phone numbers are
        /// formatted based on the presence of country codes and the number of active country codes. If location
        /// information is provided and valid, the location ID is resolved using the <see
        /// cref="LocationService"/>.</remarks>
        /// <param name="entity">The <see cref="BenevolenceRequest"/> entity to update with requester information.</param>
        /// <param name="box">A <see cref="ValidPropertiesBox{T}"/> containing the <see cref="BenevolenceRequestBag"/> with the requester
        /// details to be applied to the entity.</param>
        /// <param name="activeCountryCodes">A list of active country codes used to determine the formatting of phone numbers.</param>
        private void UpdateRequesterProperties( BenevolenceRequest entity, ValidPropertiesBox<BenevolenceRequestBag> box, List<string> activeCountryCodes )
        {
            box.IfValidProperty( nameof( box.Bag.Requester ), () =>
            {
                if ( box.Bag.Requester != null )
                {
                    entity.FirstName = box.Bag.Requester.FirstName;
                    entity.LastName = box.Bag.Requester.LastName;
                    entity.Email = box.Bag.Requester.Email;
                    entity.RequestedByPersonAliasId = box.Bag.Requester.PersonAliasId.HasValue && box.Bag.Requester.PersonAliasId != 0
                        ? box.Bag.Requester.PersonAliasId
                        : null;
                    entity.HomePhoneNumber = !string.IsNullOrWhiteSpace( box.Bag.Requester.HomePhoneNumber.NumberFormatted )
                        ? ( !string.IsNullOrWhiteSpace( box.Bag.Requester.HomePhoneNumber.CountryCode ) && activeCountryCodes.Count > 1
                            ? $"{box.Bag.Requester.HomePhoneNumber.CountryCode} {box.Bag.Requester.HomePhoneNumber.NumberFormatted}"
                            : box.Bag.Requester.HomePhoneNumber.NumberFormatted )
                        : string.Empty;
                    entity.CellPhoneNumber = !string.IsNullOrWhiteSpace( box.Bag.Requester.CellPhoneNumber.NumberFormatted )
                        ? ( !string.IsNullOrWhiteSpace( box.Bag.Requester.CellPhoneNumber.CountryCode ) && activeCountryCodes.Count > 1
                            ? $"{box.Bag.Requester.CellPhoneNumber.CountryCode} {box.Bag.Requester.CellPhoneNumber.NumberFormatted}"
                            : box.Bag.Requester.CellPhoneNumber.NumberFormatted )
                        : string.Empty;
                    entity.WorkPhoneNumber = !string.IsNullOrWhiteSpace( box.Bag.Requester.WorkPhoneNumber.NumberFormatted )
                        ? ( !string.IsNullOrWhiteSpace( box.Bag.Requester.WorkPhoneNumber.CountryCode ) && activeCountryCodes.Count > 1
                            ? $"{box.Bag.Requester.WorkPhoneNumber.CountryCode} {box.Bag.Requester.WorkPhoneNumber.NumberFormatted}"
                            : box.Bag.Requester.WorkPhoneNumber.NumberFormatted )
                        : string.Empty;
                    entity.GovernmentId = box.Bag.Requester.GovernmentId;
                    entity.ConnectionStatusValueId = box.Bag.Requester.ConnectionStatus.ConnectionStatusValueId;

                    entity.LocationId = null;
                    if ( box.Bag.Requester.Location != null && box.Bag.Requester.Location.AddressFields != null )
                    {
                        var addressFields = box.Bag.Requester.Location.AddressFields;
                        if ( addressFields.Street1 != null && addressFields.City != null && addressFields.State != null )
                        {
                            var location = LocationService.Get(
                                addressFields.Street1,
                                addressFields.Street2,
                                addressFields.City,
                                addressFields.State,
                                addressFields.PostalCode,
                                addressFields.Country,
                                new GetLocationArgs
                                {
                                    Group = null,
                                    ValidateLocation = false,
                                    VerifyLocation = false,
                                    CreateNewLocation = false,
                                }
                            );
                            entity.LocationId = location?.Id ?? ( int? ) null;
                        }
                    }
                }
            } );
        }

        /// <summary>
        /// Updates the case worker properties of the specified <see cref="BenevolenceRequest"/> entity  based on the
        /// valid properties provided in the <see cref="ValidPropertiesBox{T}"/>.
        /// </summary>
        /// <remarks>This method checks if the <c>CaseWorker</c> property in the provided <paramref
        /// name="box"/>  is valid. If valid, it updates the <c>CaseWorkerPersonAliasId</c> of the <paramref
        /// name="entity"/>  with the <c>PersonAliasId</c> of the case worker, or sets it to <c>null</c> if the ID is
        /// not valid.</remarks>
        /// <param name="entity">The <see cref="BenevolenceRequest"/> entity to update.</param>
        /// <param name="box">A <see cref="ValidPropertiesBox{T}"/> containing the properties to validate and apply.  The
        /// <c>CaseWorker</c> property of the box is used to update the <c>CaseWorkerPersonAliasId</c>  of the entity if
        /// it is valid.</param>
        private void UpdateCaseWorkerProperties( BenevolenceRequest entity, ValidPropertiesBox<BenevolenceRequestBag> box )
        {
            box.IfValidProperty( nameof( box.Bag.CaseWorker ), () =>
            {
                if ( box.Bag.CaseWorker != null )
                {
                    entity.CaseWorkerPersonAliasId = box.Bag.CaseWorker.PersonAliasId.HasValue && box.Bag.CaseWorker.PersonAliasId != 0
                        ? box.Bag.CaseWorker.PersonAliasId
                        : null;
                }
            } );
        }

        /// <summary>
        /// Updates the campus-related properties of the specified <see cref="BenevolenceRequest"/> entity  based on the
        /// valid properties provided in the <see cref="ValidPropertiesBox{T}"/>.
        /// </summary>
        /// <remarks>This method updates the <c>CampusId</c> of the <paramref name="entity"/> if the
        /// <c>Campus</c>  property in the <paramref name="box"/> is valid and contains a non-null, positive
        /// ID.</remarks>
        /// <param name="entity">The <see cref="BenevolenceRequest"/> entity to update.</param>
        /// <param name="box">A <see cref="ValidPropertiesBox{T}"/> containing the valid properties to apply to the entity.  The
        /// <c>Campus</c> property of the box is used to determine the campus ID to assign.</param>
        private void UpdateCampusProperties( BenevolenceRequest entity, ValidPropertiesBox<BenevolenceRequestBag> box )
        {
            box.IfValidProperty( nameof( box.Bag.Campus ), () =>
            {
                entity.CampusId = box.Bag.Campus != null && box.Bag.Campus.Id.HasValue && box.Bag.Campus.Id > 0
                    ? box.Bag.Campus.Id
                    : null;
            } );
        }

        /// <summary>
        /// Updates the collection of documents associated with the specified benevolence request based on the provided
        /// request document data.
        /// </summary>
        /// <remarks>This method ensures that the documents associated with the benevolence request are
        /// synchronized with the provided data. It performs the following operations: <list type="bullet">
        /// <item><description>Adds new documents to the request if they are not already
        /// associated.</description></item> <item><description>Removes documents from the request if they are marked
        /// for deletion.</description></item> </list> Documents are identified by their unique GUIDs, and only valid
        /// GUIDs are processed. Temporary files are marked as non-temporary when added, and reverted to temporary
        /// status when removed.</remarks>
        /// <param name="entity">The <see cref="BenevolenceRequest"/> entity whose documents are being updated.</param>
        /// <param name="box">A <see cref="ValidPropertiesBox{T}"/> containing the updated document data for the request. The <see
        /// cref="ValidPropertiesBox{T}.Bag"/> property must include the updated list of request documents.</param>
        private void UpdateRequestDocuments( BenevolenceRequest entity, ValidPropertiesBox<BenevolenceRequestBag> box )
        {
            box.IfValidProperty( nameof( box.Bag.RequestDocuments ), () =>
            {
                var existingDocumentIds = entity.Documents.Select( document => document.BinaryFileId ).ToList();
                foreach ( var documentBag in box.Bag.RequestDocuments )
                {
                    var isValidGuid = Guid.TryParse( documentBag.Guid.ToString(), out Guid fileGuid );
                    if ( isValidGuid && fileGuid != Guid.Empty )
                    {
                        var binaryFile = BinaryFileService.Get( fileGuid );
                        if ( binaryFile != null )
                        {
                            if ( !existingDocumentIds.Contains( binaryFile.Id ) && !documentBag.IsMarkedForDeletion )
                            {
                                var currentPersonAlias = RequestContext.CurrentPerson.PrimaryAlias;
                                var benevolenceDocument = new BenevolenceRequestDocument
                                {
                                    BenevolenceRequestId = entity.Id,
                                    BinaryFileId = binaryFile.Id,
                                    Guid = Guid.NewGuid(),
                                    CreatedByPersonAlias = currentPersonAlias,
                                    ModifiedByPersonAlias = currentPersonAlias,
                                };
                                binaryFile.IsTemporary = false;
                                entity.Documents.Add( benevolenceDocument );
                            }
                            if ( existingDocumentIds.Contains( binaryFile.Id ) && documentBag.IsMarkedForDeletion )
                            {
                                var benevolenceDocumentToRemove = entity.Documents.FirstOrDefault( d => d.BinaryFileId == binaryFile.Id );
                                if ( benevolenceDocumentToRemove != null )
                                {
                                    BenevolenceRequestDocumentService.Delete( benevolenceDocumentToRemove );
                                    binaryFile.IsTemporary = true;
                                }
                            }
                        }
                    }
                }
            } );
        }

        /// <summary>
        /// Updates the attribute values of the specified <see cref="BenevolenceRequest"/> entity  based on the provided
        /// valid properties.
        /// </summary>
        /// <remarks>This method ensures that the entity's attributes are loaded and updated securely,
        /// enforcing  attribute-level security rules based on the current user's permissions.</remarks>
        /// <param name="entity">The <see cref="BenevolenceRequest"/> entity whose attribute values will be updated.</param>
        /// <param name="box">A container specifying the valid properties and their corresponding values to be applied  to the entity. The
        /// <see cref="ValidPropertiesBox{T}"/> must include the attribute values  to update.</param>
        private void UpdateAttributeValues( BenevolenceRequest entity, ValidPropertiesBox<BenevolenceRequestBag> box )
        {
            box.IfValidProperty( nameof( box.Bag.AttributeValues ), () =>
            {
                entity.LoadAttributes( RockContext );
                entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
            } );
        }

        /// <summary>
        /// Updates the properties of the specified <see cref="BenevolenceRequest"/> entity  based on the valid
        /// properties provided in the <see cref="ValidPropertiesBox{T}"/>.
        /// </summary>
        /// <remarks>This method ensures that only properties explicitly marked as valid in the  <paramref
        /// name="box"/> are updated on the <paramref name="entity"/>. Properties not marked  as valid are ignored,
        /// leaving their existing values unchanged.</remarks>
        /// <param name="entity">The <see cref="BenevolenceRequest"/> entity to be updated. This object will have its properties  modified
        /// based on the valid values in the <paramref name="box"/>.</param>
        /// <param name="box">A <see cref="ValidPropertiesBox{T}"/> containing the <see cref="BenevolenceRequestBag"/>  with potential
        /// property values. Only properties marked as valid in the box will be applied  to the <paramref
        /// name="entity"/>.</param>
        private void UpdateDetailProperties( BenevolenceRequest entity, ValidPropertiesBox<BenevolenceRequestBag> box )
        {
            box.IfValidProperty( nameof( box.Bag.BenevolenceTypeId ), () => entity.BenevolenceTypeId = box.Bag.BenevolenceTypeId );
            box.IfValidProperty( nameof( box.Bag.RequestStatusValueId ), () => entity.RequestStatusValueId = box.Bag.RequestStatusValueId );
            box.IfValidProperty( nameof( box.Bag.RequestDateTime ), () => entity.RequestDateTime = box.Bag.RequestDateTime );
            box.IfValidProperty( nameof( box.Bag.RequestText ), () => entity.RequestText = box.Bag.RequestText );
            box.IfValidProperty( nameof( box.Bag.ResultSummary ), () => entity.ResultSummary = box.Bag.ResultSummary );
            box.IfValidProperty( nameof( box.Bag.ProvidedNextSteps ), () => entity.ProvidedNextSteps = box.Bag.ProvidedNextSteps );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="BenevolenceRequestBag"/> that represents the entity.</returns>
        private BenevolenceRequestBag GetCommonEntityBag( BenevolenceRequest entity )
        {
            if ( entity == null )
            {
                return null;
            }

            if ( entity.Id != 0 )
            {
                var requesterPersonBag = BuildPersonBag( entity, entity.RequestedByPersonAliasId ?? 0, entity.GovernmentId );
                var caseWorkerPersonBag = BuildPersonBag( entity, entity.CaseWorkerPersonAliasId ?? 0, "", isRequester: false );
                var campus = BuildCampusBag( entity.CampusId ?? 0 );

                // Add workflow info for frontend
                var workflows = GetAuthorizedManualWorkflows( entity )
                    .Select( w => new BenevolenceRequestWorkflowBag
                    {
                        TypeId = w.WorkflowTypeId,
                        TypeGuid = w.WorkflowType.Guid,
                        TypeName = w.WorkflowType.Name,
                        TriggerType = w.TriggerType,
                        LaunchUrl = this.GetLinkedPageUrl( AttributeKey.WorkflowEntryPage, new Dictionary<string, string>
                        {
                            { "WorkflowTypeId", w.WorkflowTypeId.ToString() },
                            { "BenevolenceRequestId", entity.IdKey }
                        }),
                        iconCssClass = w.WorkflowType.IconCssClass,
                    })
                    .ToList();

                return new BenevolenceRequestBag
                {
                    IdKey = entity.IdKey,
                    Requester = requesterPersonBag,
                    CaseWorker = caseWorkerPersonBag,
                    BenevolenceTypeId = entity.BenevolenceTypeId,
                    RequestStatusValueId = entity.RequestStatusValueId,
                    RequestDateTime = entity.RequestDateTime,
                    RequestText = entity.RequestText,
                    ResultSummary = entity.ResultSummary,
                    ProvidedNextSteps = entity.ProvidedNextSteps,
                    Campus = campus,
                    Results = entity.BenevolenceResults
                        .Select( result => new BenevolenceResultBag
                        {
                            IdKey = result.IdKey,
                            ResultTypeValueId = result.ResultTypeValueId,
                            Amount = result.Amount,
                            ResultSummary = result.ResultSummary,
                        } )
                        .ToList(),
                    RequestDocuments = entity.Documents
                        .OrderBy( document => document.Order )
                        .ThenBy( document => document.Id )
                        .Select( document => new BenevolenceDocumentBag
                        {
                            IdKey = document.IdKey,
                            Guid = BinaryFileService.Get( document.BinaryFileId ).Guid,
                            FileName = document.BinaryFile.FileName,
                            IsMarkedForDeletion = false
                        } )
                        .ToList(),
                    Workflows = workflows
                };
            }

            return new BenevolenceRequestBag();
        }

        /// <inheritdoc/>
        protected override BenevolenceRequestBag GetEntityBagForView( BenevolenceRequest entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( RockContext );
            }

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <inheritdoc/>
        protected override BenevolenceRequestBag GetEntityBagForEdit( BenevolenceRequest entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( RockContext );
            }

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <summary>
        /// Builds a <see cref="PersonBag"/> object containing detailed information about a person, including their
        /// contact details, location, and other attributes.
        /// </summary>
        /// <remarks>
        /// This method retrieves person-specific data if a valid <paramref name="personAliasId"/> is provided.
        /// If the person cannot be found, or if <paramref name="personAliasId"/> is not greater than 0,
        /// the method constructs the <see cref="PersonBag"/> using fallback data from the <paramref name="entity"/>.
        /// The <paramref name="isRequester"/> parameter determines whether requester-specific or case worker-specific
        /// data is used when building the object.
        /// </remarks>
        /// <param name="entity">The <see cref="BenevolenceRequest"/> entity containing fallback data for the person.</param>
        /// <param name="personAliasId">The identifier of the person's alias. Must be greater than 0 to retrieve person-specific data.</param>
        /// <param name="governmentId">The government-issued identifier for the person. Can be null or empty.</param>
        /// <param name="isRequester">A value indicating whether the person being built is the requester.</param>
        /// <returns>
        /// A <see cref="PersonBag"/> object populated with the person's details. If <paramref name="personAliasId"/> is
        /// greater than 0 and the person exists, the returned object contains data specific to that person. Otherwise,
        /// it is built using fallback data from the <paramref name="entity"/>.
        /// </returns>
        private PersonBag BuildPersonBag( BenevolenceRequest entity, int personAliasId, string governmentId, bool isRequester = true )
        {
            if ( entity == null )
            {
                throw new ArgumentNullException( nameof( entity ) );
            }

            var homePhoneTypeId = DefinedValueCache.Get( _homePhoneGuid ).Id;
            var cellPhoneTypeId = DefinedValueCache.Get( _cellPhoneGuid ).Id;
            var workPhoneTypeId = DefinedValueCache.Get( _workPhoneGuid ).Id;

            if ( personAliasId > 0 )
            {
                var person = PersonAliasService.GetPerson( personAliasId );
                var personAlias = PersonAliasService.Get( personAliasId );

                if ( person != null )
                {
                    var phoneNumbers = person.PhoneNumbers.ToList();
                    var personLocation = person.GetHomeLocation( RockContext );

                    var fallbackAddress = GetFallbackAddressControlBag( entity );

                    // Replace this block in BuildPersonBag method:

                    var personConnectionStatus = new ConnectionStatusBag
                    {
                        ConnectionStatusValueId = person.ConnectionStatusValueId ?? ( entity.ConnectionStatusValueId.HasValue && isRequester
                                                      ? entity.ConnectionStatusValueId.Value
                                                      : ( int? ) null ),
                        ConnectionStatusName = ( DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ) != null )
                                                ? DefinedValueCache.Get( person.ConnectionStatusValueId ?? 0 )?.Value ?? string.Empty
                                                : string.Empty,
                        ConnectionStatusGuid = DefinedValueCache.Get( person.ConnectionStatusValueId ?? 0 )?.Guid ?? Guid.Empty
                    };

                    return new PersonBag
                    {
                        PersonIdKey = person.IdKey,
                        PersonAliasId = personAlias.Id,
                        PersonAliasGuid = personAlias.Guid,
                        ConnectionStatus = personConnectionStatus,
                        PhotoUrl = person.PhotoUrl ?? "",
                        NickName = person.NickName ?? "",
                        FirstName = !string.IsNullOrEmpty( person.FirstName ) ? person.FirstName : ( isRequester ? entity.FirstName ?? "" : "" ),
                        LastName = !string.IsNullOrEmpty( person.LastName ) ? person.LastName : ( isRequester ? entity.LastName ?? "" : "" ),
                        Location = GetLocationBag( personLocation, entity.LocationId, entity.Location?.Guid, fallbackAddress ),
                        HomePhoneNumber = GetPhoneNumberBag( phoneNumbers, homePhoneTypeId, isRequester ? entity.HomePhoneNumber : "" ),
                        CellPhoneNumber = GetPhoneNumberBag( phoneNumbers, cellPhoneTypeId, isRequester ? entity.CellPhoneNumber : "" ),
                        WorkPhoneNumber = GetPhoneNumberBag( phoneNumbers, workPhoneTypeId, isRequester ? entity.WorkPhoneNumber : "" ),
                        Email = !string.IsNullOrEmpty( person.Email ) ? person.Email : ( isRequester ? entity.Email ?? "" : "" ),
                        GovernmentId = governmentId ?? "",
                        RaceGuid = person.RaceValue?.Guid ?? Guid.Empty,
                        EthnicityGuid = person.EthnicityValue?.Guid ?? Guid.Empty,
                    };
                }
            }

            // Fallback: Build from entity
            var fallbackLocationBag = GetFallbackLocationBag( entity, LocationService );

            var entityConnectionStatus = new ConnectionStatusBag
            {
                ConnectionStatusValueId = entity.ConnectionStatusValueId ?? ( entity.ConnectionStatusValueId.HasValue && isRequester
                                              ? entity.ConnectionStatusValueId.Value
                                              : ( int? ) null ),
                ConnectionStatusName = ( DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ) != null )
                                        ? DefinedValueCache.Get( entity.ConnectionStatusValueId ?? 0 )?.Value ?? string.Empty
                                        : string.Empty,
                ConnectionStatusGuid = DefinedValueCache.Get( entity.ConnectionStatusValueId ?? 0 )?.Guid ?? Guid.Empty
            };

            var requesterBagBuiltFromEntity = new PersonBag
            {
                PersonAliasId = entity.RequestedByPersonAliasId,
                ConnectionStatus = entityConnectionStatus,
                PhotoUrl = "",
                NickName = "",
                FirstName = entity.FirstName ?? "",
                LastName = entity.LastName ?? "",
                Location = fallbackLocationBag,
                HomePhoneNumber = GetPhoneNumberBag( new List<PhoneNumber>(), 0, entity.HomePhoneNumber ),
                CellPhoneNumber = GetPhoneNumberBag( new List<PhoneNumber>(), 0, entity.CellPhoneNumber ),
                WorkPhoneNumber = GetPhoneNumberBag( new List<PhoneNumber>(), 0, entity.WorkPhoneNumber ),
                Email = entity.Email ?? "",
                GovernmentId = entity.GovernmentId ?? "",
                RaceGuid = Guid.Empty,
                EthnicityGuid = Guid.Empty,
            };

            var caseWorkerBagBuiltFromEntity = new PersonBag
            {
                PersonAliasId = entity.CaseWorkerPersonAliasId
            };

            return isRequester ? requesterBagBuiltFromEntity : caseWorkerBagBuiltFromEntity;
        }

        /// <summary>
        /// Builds a <see cref="PersonBag"/> object containing detailed information about a person based on the
        /// specified person alias GUID.
        /// </summary>
        /// <remarks>This method attempts to retrieve a person's details using the provided person alias
        /// GUID. If the GUID is empty or does not correspond to a valid person, the method returns <see
        /// langword="false"/> and outputs an empty <see cref="PersonBag"/>. The generated <see cref="PersonBag"/>
        /// includes information such as the person's name, contact details, location, and other relevant
        /// attributes.</remarks>
        /// <param name="personAliasGuid">The unique identifier of the person alias used to retrieve the person's information.</param>
        /// <param name="generatedPersonBag">When this method returns, contains the generated <see cref="PersonBag"/> object with the person's details if
        /// the operation is successful; otherwise, contains an empty <see cref="PersonBag"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="PersonBag"/> was successfully generated; otherwise, <see
        /// langword="false"/>.</returns>
        private bool BuildPersonBag( Guid personAliasGuid, out PersonBag generatedPersonBag )
        {
            if ( personAliasGuid.IsEmpty() )
            {
                generatedPersonBag = new PersonBag();
                return false;
            }

            var person = PersonAliasService.GetPerson( personAliasGuid );
            var personAlias = PersonAliasService.Get( personAliasGuid );

            if ( person == null || personAlias == null )
            {
                generatedPersonBag = new PersonBag();
                return false;
            }

            var homePhoneTypeId = DefinedValueCache.Get( _homePhoneGuid ).Id;
            var cellPhoneTypeId = DefinedValueCache.Get( _cellPhoneGuid ).Id;
            var workPhoneTypeId = DefinedValueCache.Get( _workPhoneGuid ).Id;

            var phoneNumbers = person.PhoneNumbers.ToList();
            var personLocation = person.GetHomeLocation( RockContext );

            var personConnectionStatus = new ConnectionStatusBag
            {
                ConnectionStatusValueId = person.ConnectionStatusValueId ?? ( int? ) null,
                ConnectionStatusName = ( DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ) != null )
                                        ? DefinedValueCache.Get( person.ConnectionStatusValueId ?? 0 )?.Value ?? string.Empty
                                        : string.Empty,
                ConnectionStatusGuid = DefinedValueCache.Get( person.ConnectionStatusValueId ?? 0 )?.Guid ?? Guid.Empty
            };

            generatedPersonBag = new PersonBag
            {
                PersonIdKey = person.IdKey,
                PersonAliasId = personAlias.Id,
                PersonAliasGuid = personAlias.Guid != null ? personAlias.Guid : Guid.Empty,
                ConnectionStatus = personConnectionStatus,
                PhotoUrl = person.PhotoUrl ?? "",
                NickName = person.NickName ?? "",
                FirstName = person.FirstName ?? "",
                LastName = person.LastName ?? "",
                Location = GetLocationBag( personLocation ),
                HomePhoneNumber = GetPhoneNumberBag( phoneNumbers, homePhoneTypeId ),
                CellPhoneNumber = GetPhoneNumberBag( phoneNumbers, cellPhoneTypeId ),
                WorkPhoneNumber = GetPhoneNumberBag( phoneNumbers, workPhoneTypeId ),
                Email = person.Email ?? "",
                GovernmentId = "",
                RaceGuid = person.RaceValue?.Guid ?? Guid.Empty,
                EthnicityGuid = person.EthnicityValue?.Guid ?? Guid.Empty,
            };

            return true;
        }

        /// <summary>
        /// Constructs a <see cref="CampusBag"/> object for the specified campus identifier.
        /// </summary>
        /// <param name="campusId">The unique identifier of the campus to retrieve.</param>
        /// <returns>A <see cref="CampusBag"/> containing the campus details if found; otherwise, an empty <see
        /// cref="CampusBag"/>.</returns>
        private CampusBag BuildCampusBag( int campusId )
        {
            var campus = CampusService.Get( campusId );
            if ( campus != null )
            {
                return new CampusBag
                {
                    Id = campus.Id,
                    Guid = campus.Guid,
                    Name = campus.Name ?? "",
                    Description = campus.Description ?? "",
                };
            }

            return new CampusBag();
        }

        /// <summary>
        /// Constructs a <see cref="CampusBag"/> object for the specified campus identifier.
        /// </summary>
        /// <param name="campusGuid">The unique identifier of the campus to retrieve.</param>
        /// <returns>A <see cref="CampusBag"/> containing the campus details if found; otherwise, an empty <see
        /// cref="CampusBag"/>.</returns>
        private bool BuildCampusBag( Guid campusGuid, out CampusBag campusBag )
        {
            var campus = CampusService.Get( campusGuid );
            if ( campus != null )
            {
                campusBag = new CampusBag
                {
                    Id = campus.Id,
                    Guid = campus.Guid,
                    Name = campus.Name ?? "",
                    Description = campus.Description ?? "",
                };
                return true;
            }

            campusBag = new CampusBag();
            return false;
        }

        /// <summary>
        /// Creates a <see cref="PhoneNumberBag"/> object based on the specified phone number type ID.
        /// </summary>
        /// <remarks>If no phone number in the <paramref name="phoneNumbers"/> list matches the specified
        /// <paramref name="typeId"/>, the fallback value is used to populate the <see cref="PhoneNumberBag"/>. The
        /// country code will be empty in this case.</remarks>
        /// <param name="phoneNumbers">A list of <see cref="PhoneNumber"/> objects to search for the specified type ID.</param>
        /// <param name="typeId">The identifier for the phone number type to match.</param>
        /// <param name="fallback">A fallback phone number to use if no matching phone number is found in the <paramref name="phoneNumbers"/>
        /// list.</param>
        /// <returns>A <see cref="PhoneNumberBag"/> containing the phone number, formatted number, and country code for the
        /// matching phone number, or the fallback value
        private PhoneNumberBag GetPhoneNumberBag( List<PhoneNumber> phoneNumbers, int typeId, string fallback )
        {
            var phone = phoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == typeId );
            if ( phone != null )
            {
                return new PhoneNumberBag
                {
                    Number = phone.Number,
                    NumberFormatted = phone.NumberFormatted,
                    CountryCode = phone.CountryCode,
                    IsNumberFromPersonRecord = true,
                };
            }

            // Fallback logic: If there are multiple defined country codes, split fallback at first space.

            var activeCountryCodes = DefinedValueService
                .GetByDefinedTypeId( DefinedTypeCache.GetId( _countryCodeTypeGuid ).Value )
                .Where( dv => dv.IsActive )
                .Select( dv => dv.Value )
                .Distinct()
                .ToList();

            string countryCode = "";
            string number = fallback ?? "";
            string numberFormatted = fallback ?? "";

            if ( activeCountryCodes.Count > 1 && !string.IsNullOrWhiteSpace( fallback ) )
            {
                var firstSpaceIndex = fallback.IndexOf( ' ' );
                if ( firstSpaceIndex > 0 )
                {
                    countryCode = fallback.Substring( 0, firstSpaceIndex );
                    number = fallback.Substring( firstSpaceIndex + 1 );
                    numberFormatted = number;
                }
            }

            return new PhoneNumberBag
            {
                Number = number.Trim(),
                NumberFormatted = numberFormatted.Trim(),
                CountryCode = countryCode.Trim(),
                IsNumberFromPersonRecord = false,
            };
        }

        /// <summary>
        /// Creates a fallback <see cref="AddressControlBag"/> based on the location information  in the specified <see
        /// cref="BenevolenceRequest"/> entity.
        /// </summary>
        /// <param name="entity">The <see cref="BenevolenceRequest"/> containing the location data  to populate the <see
        /// cref="AddressControlBag"/>. If the location is <c>null</c>, an empty  <see cref="AddressControlBag"/> is
        /// returned.</param>
        /// <returns>An <see cref="AddressControlBag"/> populated with the location details from the  <paramref name="entity"/>.
        /// If the location is <c>null</c>, returns an empty  <see cref="AddressControlBag"/>.</returns>
        private AddressControlBag GetFallbackAddressControlBag( BenevolenceRequest entity )
        {
            return entity.Location != null
                ? new AddressControlBag
                {
                    Street1 = entity.Location.Street1,
                    Street2 = entity.Location.Street2,
                    City = entity.Location.City,
                    State = entity.Location.State,
                    PostalCode = entity.Location.PostalCode,
                    Country = entity.Location.Country
                }
                : new AddressControlBag();
        }

        /// <summary>
        /// Creates a <see cref="LocationBag"/> object based on the provided <paramref name="location"/> and optional
        /// fallback values.
        /// </summary>
        /// <param name="location">The primary <see cref="Location"/> object used to populate the <see cref="LocationBag"/>. If null, the
        /// fallback values are used.</param>
        /// <param name="fallbackId">An optional fallback identifier to use if <paramref name="location"/> does not have a valid ID.</param>
        /// <param name="fallbackGuid">An optional fallback GUID to use if <paramref name="location"/> does not have a valid GUID.</param>
        /// <param name="fallbackAddress">An optional fallback <see cref="AddressControlBag"/> to use if <paramref name="location"/> does not have
        /// address information.</param>
        /// <returns>A <see cref="LocationBag"/> object populated with data from <paramref name="location"/> if it is not null;
        /// otherwise, a <see cref="LocationBag"/> populated with the provided fallback values.</returns>
        private LocationBag GetLocationBag( Location location, int? fallbackId, Guid? fallbackGuid, AddressControlBag fallbackAddress )
        {
            if ( location == null )
                return new LocationBag { AddressFields = fallbackAddress ?? new AddressControlBag() };

            return new LocationBag
            {
                Id = location.Id > 0 ? location.Id : fallbackId,
                Guid = !location.Guid.IsEmpty() ? location.Guid : fallbackGuid ?? Guid.Empty,
                AddressFields = GetAddressControlBag( location, fallbackAddress )
            };
        }

        /// <summary>
        /// Creates an <see cref="AddressControlBag"/> instance based on the specified <paramref name="location"/>.
        /// </summary>
        /// <param name="location">The <see cref="Location"/> object containing address details. If <c>null</c>, the <paramref
        /// name="fallback"/> is used.</param>
        /// <param name="fallback">An optional <see cref="AddressControlBag"/> to provide default values if <paramref name="location"/> or its
        /// properties are <c>null</c>.</param>
        /// <returns>An <see cref="AddressControlBag"/> populated with address details from <paramref name="location"/>. If
        /// <paramref name="location"/> is <c>null</c>, the <paramref name="fallback"/> is returned, or a new, empty
        /// <see cref="AddressControlBag"/> if <paramref name="fallback"/> is also <c>null</c>.</returns>
        private AddressControlBag GetAddressControlBag( Location location, AddressControlBag fallback )
        {
            if ( location == null )
                return fallback ?? new AddressControlBag();

            return new AddressControlBag
            {
                Street1 = location.Street1 ?? fallback?.Street1 ?? "",
                Street2 = location.Street2 ?? fallback?.Street2 ?? "",
                City = location.City ?? fallback?.City ?? "",
                State = location.State ?? fallback?.State ?? "",
                PostalCode = location.PostalCode ?? fallback?.PostalCode ?? "",
                Country = location.Country ?? fallback?.Country ?? ""
            };
        }

        /// <summary>
        /// Creates a fallback <see cref="LocationBag"/> based on the specified <see cref="BenevolenceRequest"/> entity.
        /// </summary>
        /// <remarks>If the <paramref name="entity"/> contains a valid <c>LocationId</c> and
        /// <c>Location</c>, the returned <see cref="LocationBag"/> will include the corresponding GUID and address
        /// fields. If these values are not available, the method returns a <see cref="LocationBag"/> with empty address
        /// fields.</remarks>
        /// <param name="entity">The <see cref="BenevolenceRequest"/> containing location information. This parameter cannot be null.</param>
        /// <param name="locationService">The <see cref="LocationService"/> used to retrieve the GUID for the location. This parameter cannot be null.</param>
        /// <returns>A <see cref="LocationBag"/> populated with location details from the <paramref name="entity"/> if available;
        /// otherwise, an empty <see cref="LocationBag"/> with default address fields.</returns>
        private LocationBag GetFallbackLocationBag( BenevolenceRequest entity, LocationService locationService )
        {
            return entity.LocationId.HasValue && entity.Location != null
                ? new LocationBag
                {
                    Id = entity.LocationId.Value,
                    Guid = locationService.GetGuid( entity.LocationId.Value ) ?? Guid.Empty,
                    AddressFields = new AddressControlBag
                    {
                        Street1 = entity.Location?.Street1 ?? "",
                        Street2 = entity.Location?.Street2 ?? "",
                        City = entity.Location?.City ?? "",
                        State = entity.Location?.State ?? "",
                        PostalCode = entity.Location?.PostalCode ?? "",
                        Country = entity.Location?.Country ?? ""
                    }
                }
                : new LocationBag { AddressFields = new AddressControlBag() };
        }

        /// <summary>
        /// Creates a <see cref="PhoneNumberBag"/> instance based on the first phone number in the provided list  that
        /// matches the specified type identifier.
        /// </summary>
        /// <param name="phoneNumbers">A list of <see cref="PhoneNumber"/> objects to search through. Cannot be null.</param>
        /// <param name="typeId">The identifier of the phone number type to match.</param>
        /// <returns>A <see cref="PhoneNumberBag"/> containing the number and country code of the first matching phone number, 
        /// or an empty <see cref="PhoneNumberBag"/> if no match is found.</returns>
        private PhoneNumberBag GetPhoneNumberBag( List<PhoneNumber> phoneNumbers, int typeId )
        {
            var phone = phoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == typeId );
            return phone != null
                ? new PhoneNumberBag
                {
                    Number = phone.Number,
                    CountryCode = phone.CountryCode,
                    NumberFormatted = phone.NumberFormatted,
                    IsNumberFromPersonRecord = true,
                }
                : new PhoneNumberBag
                {
                    IsNumberFromPersonRecord = false,
                };
        }

        /// <summary>
        /// Creates a <see cref="LocationBag"/> object based on the specified <paramref name="location"/>.
        /// </summary>
        /// <param name="location">The <see cref="Location"/> object to convert. If <paramref name="location"/> is <see langword="null"/>, a
        /// default <see cref="LocationBag"/> with empty address fields is returned.</param>
        /// <returns>A <see cref="LocationBag"/> containing the data from the specified <paramref name="location"/>, or a default
        /// <see cref="LocationBag"/> if <paramref name="location"/> is <see langword="null"/>.</returns>
        private LocationBag GetLocationBag( Location location )
        {
            if ( location == null )
            {
                return new LocationBag
                {
                    AddressFields = new AddressControlBag()
                };
            }

            return new LocationBag
            {
                Id = location.Id,
                Guid = location.Guid != null ? location.Guid : Guid.Empty,
                AddressFields = new AddressControlBag
                {
                    Street1 = location.Street1 ?? "",
                    Street2 = location.Street2 ?? "",
                    City = location.City ?? "",
                    State = location.State ?? "",
                    PostalCode = location.PostalCode ?? "",
                    Country = location.Country ?? ""
                }
            };
        }

        /// <summary>
        /// Finds an existing person based on the provided information or creates a new person record if no match is
        /// found.
        /// </summary>
        /// <remarks>This method attempts to find a person using the provided details in <paramref
        /// name="personBag"/>. If no match is found, a new person record is created with the provided details. The
        /// method updates the <paramref name="createRecordNotes"/> to indicate the outcome of the operation.</remarks>
        /// <param name="personBag">An object containing the personal details used to find or create the person, such as name, email, and phone
        /// number.</param>
        /// <param name="campusId">The optional campus identifier to associate with the person if a new record is created. Can be <see
        /// langword="null"/>.</param>
        /// <param name="createRecordNotes">A <see cref="StringBuilder"/> to which notes about the operation are appended, indicating whether a new
        /// person was created or an existing one was found.</param>
        /// <returns>The <see cref="Person"/> object representing the existing person if found, or the newly created person if no
        /// match was found.</returns>
        private Person FindOrCreatePerson( PersonBag personBag, int? campusId, StringBuilder createRecordNotes, out bool hasPersonRecordAlready )
        {
            var firstName = personBag.FirstName.Trim();
            var lastName = personBag.LastName.Trim();
            var emailAddress = ( personBag.Email ?? "" ).Trim();

            var personQuery = new PersonService.PersonMatchQuery(
                firstName,
                lastName,
                emailAddress,
                personBag.CellPhoneNumber.Number
            );

            var personService = new PersonService( RockContext );
            var persons = personService.FindPersons( personQuery, true );
            var person = persons?.FirstOrDefault();

            if ( person == null )
            {
                person = new Person { FirstName = firstName, LastName = lastName, Email = emailAddress };

                if ( !person.ConnectionStatusValueId.HasValue )
                {
                    person.ConnectionStatusValueId = personBag.ConnectionStatus.ConnectionStatusValueId.Value;
                }

                if ( !person.RecordStatusValueId.HasValue )
                {
                    var activePersonStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE );
                    if ( activePersonStatus != null )
                    {
                        person.RecordStatusValueId = activePersonStatus.Id;
                    }
                }

                if ( personBag.RaceGuid != null && !personBag.RaceGuid.IsEmpty() )
                {
                    person.RaceValueId = DefinedValueCache.Get( personBag.RaceGuid ).Id;
                }

                if ( personBag.EthnicityGuid != null && !personBag.EthnicityGuid.IsEmpty() )
                {
                    person.EthnicityValueId = DefinedValueCache.Get( personBag.EthnicityGuid ).Id;
                }

                var group = PersonService.SaveNewPerson( person, RockContext, campusId );
                person.PrimaryFamily = group;

                hasPersonRecordAlready = false;
                createRecordNotes.Append( "Created new person record." );
            }
            else
            {
                hasPersonRecordAlready = true;
                createRecordNotes.Append( "Person already exists. Pulled record instead. To update contact information, please update the Person." );
            }

            return person;
        }

        /// <summary>
        /// Saves the phone numbers for the specified person based on the provided person bag.
        /// </summary>
        /// <remarks>This method updates the person's phone numbers in the database. If a phone number of
        /// a specific type (e.g., home, mobile, or work) does not already exist for the person, it will be added. If
        /// the phone number is empty or null, no changes will be made for that type.</remarks>
        /// <param name="person">The person whose phone numbers are being updated. This parameter cannot be null.</param>
        /// <param name="personBag">The bag containing the phone number details to be saved. This includes home, mobile, and work phone numbers.</param>
        private void SavePhoneNumbers( Person person, PersonBag personBag )
        {
            var phoneNumberService = new PhoneNumberService( RockContext );
            var phoneTypes = new[]
            {
                new {
                    TypeGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid(),
                    Number = PhoneNumber.CleanNumber(personBag.HomePhoneNumber.Number),
                    CountryCode = PhoneNumber.CleanNumber(personBag.HomePhoneNumber.CountryCode),
                },
                new {
                    TypeGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(),
                    Number = PhoneNumber.CleanNumber(personBag.CellPhoneNumber.Number),
                    CountryCode = PhoneNumber.CleanNumber(personBag.CellPhoneNumber.CountryCode),
                },
                new {
                    TypeGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid(),
                    Number = PhoneNumber.CleanNumber(personBag.WorkPhoneNumber.Number),
                    CountryCode = PhoneNumber.CleanNumber(personBag.WorkPhoneNumber.CountryCode),
                }
            };

            var phoneNumbersToSave = false;
            foreach ( var phoneTypeInfo in phoneTypes )
            {
                var typeValue = DefinedValueCache.Get( phoneTypeInfo.TypeGuid );
                if ( typeValue != null )
                {
                    var phoneNumber = phoneNumberService.Queryable()
                        .Where( n =>
                            n.PersonId == person.Id &&
                            n.NumberTypeValueId.HasValue &&
                            n.NumberTypeValueId.Value == typeValue.Id )
                        .FirstOrDefault();

                    if ( phoneNumber == null && phoneTypeInfo.Number.IsNotNullOrWhiteSpace() )
                    {
                        phoneNumber = new PhoneNumber();
                        phoneNumberService.Add( phoneNumber );

                        phoneNumber.PersonId = person.Id;
                        phoneNumber.NumberTypeValueId = typeValue.Id;
                        phoneNumber.Number = phoneTypeInfo.Number;
                        phoneNumber.CountryCode = phoneTypeInfo.CountryCode;

                        phoneNumbersToSave = true;
                    }
                }
            }

            if ( phoneNumbersToSave )
            {
                RockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Updates the primary family address of the specified person based on the provided address information.
        /// </summary>
        /// <remarks>This method updates the home address of the person's primary family if the provided
        /// address information is valid and complete. If the address already exists for the family, no changes are
        /// made. If the address is new, it is added as the family's home address.</remarks>
        /// <param name="person">The person whose primary family address will be updated. Must not be null.</param>
        /// <param name="personBag">The data bag containing the address information to be saved. Must not be null and must include valid address
        /// fields.</param>
        private void SaveFamilyAddress( Person person, PersonBag personBag )
        {
            var group = person.PrimaryFamily;
            if ( group != null
                && personBag.Location != null
                && personBag.Location.AddressFields != null
                && !string.IsNullOrWhiteSpace( personBag.Location.AddressFields.Street1 )
                && !string.IsNullOrWhiteSpace( personBag.Location.AddressFields.City )
                && !string.IsNullOrWhiteSpace( personBag.Location.AddressFields.State ) )
            {
                var address = personBag.Location.AddressFields;
                var homeLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
                if ( homeLocationType != null )
                {
                    var homeLocation = LocationService.Get(
                        address.Street1,
                        address.Street2,
                        address.City,
                        address.State,
                        address.Locality,
                        address.PostalCode,
                        address.Country,
                        new GetLocationArgs
                        {
                            ValidateLocation = false,
                            VerifyLocation = false,
                            CreateNewLocation = false,
                            Group = group,
                        }
                    );

                    var groupLocation = group.GroupLocations
                        .FirstOrDefault( l =>
                            l.GroupLocationTypeValueId.HasValue &&
                            l.GroupLocationTypeValueId.Value == homeLocationType.Id );

                    if ( homeLocation != null )
                    {
                        if ( groupLocation == null || groupLocation.LocationId != homeLocation.Id )
                        {
                            GroupService.AddNewGroupAddress( RockContext, group, homeLocationType.Guid.ToString(), homeLocation, true, string.Empty, true, true );
                            RockContext.SaveChanges();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validates a collection of phone numbers and identifies the first number that fails validation.
        /// </summary>
        /// <remarks>Validation is performed by cleaning and formatting each phone number according to its
        /// country code. The method stops at the first invalid number and returns its value in the out
        /// parameter.</remarks>
        /// <param name="phoneNumberBags">A list of phone number bags containing the numbers and associated country codes to validate.</param>
        /// <param name="numberThatFailedValidation">When this method returns, contains the phone number that failed validation, or an empty string if all
        /// numbers are valid.</param>
        /// <returns>true if all phone numbers in the collection pass validation; otherwise, false.</returns>
        private bool ValidatePhoneNumbers( List<PhoneNumberBag> phoneNumberBags, out string numberThatFailedValidation )
        {
            numberThatFailedValidation = string.Empty;

            foreach ( var bag in phoneNumberBags )
            {
                if ( bag == null || string.IsNullOrWhiteSpace( bag.Number ) )
                {
                    continue;
                }

                var cleanedPhoneNumber = PhoneNumber.CleanNumber( bag.Number );
                var formattedPhoneNumber = PhoneNumber.FormattedNumber( bag.CountryCode, cleanedPhoneNumber );
                var isValidNumber = formattedPhoneNumber != cleanedPhoneNumber && formattedPhoneNumber != "";

                if ( !isValidNumber )
                {
                    numberThatFailedValidation = formattedPhoneNumber;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the authorized manual workflows for the given benevolence request.
        /// </summary>
        /// <param name="benevolenceRequest">The benevolence request.</param>
        /// <returns>A list of authorized manual workflows.</returns>
        private List<BenevolenceWorkflow> GetAuthorizedManualWorkflows( BenevolenceRequest benevolenceRequest )
        {
            if ( benevolenceRequest?.BenevolenceType == null )
            {
                return new List<BenevolenceWorkflow>();
            }

            var benevolenceWorkflows = benevolenceRequest.BenevolenceType.BenevolenceWorkflows;
            var manualWorkflows = benevolenceWorkflows
                .Where( w => w.TriggerType == BenevolenceWorkflowTriggerType.Manual && w.WorkflowType != null )
                .OrderBy( w => w.WorkflowType.Name )
                .Distinct();

            var authorizedWorkflows = new List<BenevolenceWorkflow>();
            foreach ( var manualWorkflow in manualWorkflows )
            {
                if ( ( manualWorkflow.WorkflowType.IsActive ?? true ) && manualWorkflow.WorkflowType.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    authorizedWorkflows.Add( manualWorkflow );
                }
            }

            return authorizedWorkflows;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Adds a person to the system based on the provided requester data.
        /// </summary>
        /// <remarks>This method attempts to find an existing person matching the provided data or creates
        /// a new person if no match is found.  It also saves phone numbers and family address information for the
        /// person. The method ensures that the associated entity  for the benevolence request is valid before
        /// proceeding.</remarks>
        /// <param name="benevolenceRequestIdKey">The unique identifier key for the benevolence request. This is used to retrieve the associated entity for
        /// the action.</param>
        /// <param name="personBag">The data bag containing information about the person to be added. This parameter is required.</param>
        /// <param name="campusId">The optional identifier of the campus to associate with the person. If not provided, no campus association
        /// will be made.</param>
        /// <returns>A <see cref="BlockActionResult"/> containing the result of the operation. If successful, the result includes
        /// the created or updated person data and any notes about the record creation.</returns>
        [BlockAction]
        public BlockActionResult AddPersonFromRequesterData( string benevolenceRequestIdKey, PersonBag personBag, int? campusId )
        {
            if ( !TryGetEntityForEditAction( benevolenceRequestIdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            RockContext.Entry( entity ).State = EntityState.Detached;

            if ( personBag == null )
            {
                return ActionBadRequest( "PersonBag is required." );
            }

            var hasValidPhoneNumbers = ValidatePhoneNumbers( new List<PhoneNumberBag>
                {
                    personBag.HomePhoneNumber,
                    personBag.CellPhoneNumber,
                    personBag.WorkPhoneNumber,
                },
                out string invalidNumber
            );

            if ( !hasValidPhoneNumbers )
            {
                var errorPostfix = ( !string.IsNullOrEmpty( invalidNumber ) ? $": {invalidNumber}" : "." );
                return ActionBadRequest( $"Invalid phone number provided{errorPostfix}" );
            }

            var createRecordNotes = new StringBuilder();
            var person = FindOrCreatePerson( personBag, campusId, createRecordNotes, out bool hasRecordAlready );

            if ( !hasRecordAlready )
            {
                SavePhoneNumbers( person, personBag );
                SaveFamilyAddress( person, personBag );
            }

            BuildPersonBag( person.PrimaryAlias.Guid, out var returnPersonBag );
            return ActionOk( new { Person = returnPersonBag, CreateRecordNotes = createRecordNotes.ToString() } );
        }

        /// <summary>
        /// Adds a new benevolence request result to the system.
        /// </summary>
        /// <param name="benevolenceRequestIdKey">The key representing the benevolence request to which the result will be added. Must be a valid, non-null
        /// key.</param>
        /// <param name="benevolenceResultBag">The data bag containing the details of the benevolence result to be added. Cannot be null and must have a
        /// valid <see cref="BenevolenceResultBag.ResultTypeValueId"/>.</param>
        /// <returns>A <see cref="BlockActionResult"/> indicating the success or failure of the operation. Returns an error if
        /// the user is not authorized, if the <paramref name="benevolenceResultBag"/> is invalid, or if the <paramref
        /// name="benevolenceRequestIdKey"/> is invalid.</returns>
        [BlockAction]
        public BlockActionResult AddBenevolenceRequestResult( string benevolenceRequestIdKey, BenevolenceResultBag benevolenceResultBag )
        {
            // Is the user authorized to edit this benevolence request?
            if ( !TryGetEntityForEditAction( benevolenceRequestIdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Early out if benevolenceResultBag is null.
            if ( benevolenceResultBag == null )
            {
                return ActionBadRequest( "Invalid BenevolenceResultBag." );
            }

            // Early out if ResultTypeValueId is missing or zero.
            if ( benevolenceResultBag.ResultTypeValueId == 0 )
            {
                return ActionBadRequest( "Missing ResultTypeValueId." );
            }

            var benevolenceRequestId = Rock.Utility.IdHasher.Instance.GetId( benevolenceRequestIdKey );
            if ( !benevolenceRequestId.HasValue || benevolenceRequestId == 0 )
            {
                return ActionBadRequest( $"Invalid BenevolenceRequestId: {benevolenceRequestIdKey}" );
            }

            var resultService = new BenevolenceResultService( RockContext );

            var newResult = new BenevolenceResult
            {
                BenevolenceRequestId = benevolenceRequestId.Value,
                ResultTypeValueId = benevolenceResultBag.ResultTypeValueId,
                Amount = benevolenceResultBag.Amount,
                ResultSummary = benevolenceResultBag.ResultSummary ?? string.Empty
            };

            resultService.Add( newResult );
            RockContext.SaveChanges();

            var newResultBag = new BenevolenceResultBag
            {
                IdKey = newResult.IdKey,
                ResultTypeValueId = newResult.ResultTypeValueId,
                Amount = newResult.Amount,
                ResultSummary = newResult.ResultSummary,
            };

            return ActionOk( newResultBag );
        }

        /// <summary>
        /// Deletes a benevolence request result identified by the specified keys.
        /// </summary>
        /// <param name="benevolenceRequestIdKey">The key identifying the benevolence request associated with the result to be deleted. Cannot be null or
        /// empty.</param>
        /// <param name="benevolenceResultIdKey">The key identifying the specific benevolence result to be deleted. Cannot be null or empty.</param>
        /// <returns>A <see cref="BlockActionResult"/> indicating the outcome of the delete operation. Returns a bad request if
        /// the keys are invalid, not found if the result does not exist, or success if the deletion is successful.</returns>
        [BlockAction]
        public BlockActionResult DeleteBenevolenceRequestResult( string benevolenceRequestIdKey, string benevolenceResultIdKey )
        {
            // Is the user authorized to edit this benevolence request?
            if ( !TryGetEntityForEditAction( benevolenceRequestIdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Early out if resultIdKey is not valid.
            if ( string.IsNullOrWhiteSpace( benevolenceResultIdKey ) )
            {
                return ActionBadRequest( $"Invalid BenevolenceResultIdKey: {benevolenceResultIdKey}" );
            }

            var benevolenceRequestId = Rock.Utility.IdHasher.Instance.GetId( benevolenceRequestIdKey );
            var benevolenceResultId = Rock.Utility.IdHasher.Instance.GetId( benevolenceResultIdKey );

            if ( benevolenceRequestId == 0 || benevolenceResultId == 0 )
            {
                return ActionBadRequest( $"Invalid BenevolenceRequestId or BenevolenceResultId." );
            }

            var resultService = new BenevolenceResultService( RockContext );
            var result = resultService.Queryable()
                .Where( r => r.BenevolenceRequestId == benevolenceRequestId )
                .FirstOrDefault( r => r.Id == benevolenceResultId );

            if ( result == null )
            {
                return ActionNotFound( $"BenevolenceResult with IdKey {benevolenceResultIdKey} not found for request {benevolenceRequestIdKey}." );
            }

            resultService.Delete( result );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Generates a <see cref="PersonBag"/> object from the specified person alias GUID.
        /// </summary>
        /// <remarks>This method attempts to build a <see cref="PersonBag"/> using the provided GUID. If
        /// the GUID is invalid or the build process fails, an error result is returned.</remarks>
        /// <param name="personAliasGuid">The GUID representing the person alias from which to generate the <see cref="PersonBag"/>.</param>
        /// <returns>A <see cref="BlockActionResult"/> containing the generated <see cref="PersonBag"/> if successful; otherwise,
        /// an error message indicating the failure reason.</returns>
        [BlockAction]
        public BlockActionResult GeneratePersonBagFromPersonAliasGuid( Guid personAliasGuid )
        {
            string errorPrefix = "GeneratePersonBagFromPersonAliasGuid failed:";

            if ( personAliasGuid == null || personAliasGuid.IsEmpty() )
            {
                return ActionBadRequest( $"{errorPrefix} {personAliasGuid} not a valid Guid." );
            }

            var isSuccessfulBuild = BuildPersonBag( personAliasGuid, out PersonBag personBag );

            if ( !isSuccessfulBuild )
            {
                return ActionBadRequest( $"{errorPrefix} Failed to build PersonBag from Guid {personAliasGuid}" );
            }

            return ActionOk( personBag );
        }

        /// <summary>
        /// Generates a <see cref="CampusBag"/> object from the specified campus GUID.
        /// </summary>
        /// <remarks>This method attempts to build a <see cref="CampusBag"/> using the provided campus
        /// GUID. If the GUID is invalid or the build process fails, an error result is returned.</remarks>
        /// <param name="campusGuid">The GUID of the campus for which to generate the <see cref="CampusBag"/>. Must be a valid, non-empty GUID.</param>
        /// <returns>A <see cref="BlockActionResult"/> containing the generated <see cref="CampusBag"/> if successful; otherwise,
        /// an error message indicating the failure reason.</returns>
        [BlockAction]
        public BlockActionResult GenerateCampusBagFromCampusGuid( Guid campusGuid )
        {
            string errorPrefix = "GenerateCampusBagFromCampusGuid failed:";

            if ( campusGuid == null || campusGuid.IsEmpty() )
            {
                return ActionBadRequest( $"{errorPrefix} {campusGuid} not a valid Guid." );
            }

            var isSuccessfulBuild = BuildCampusBag( campusGuid, out CampusBag campusBag );

            if ( !isSuccessfulBuild )
            {
                return ActionBadRequest( $"{errorPrefix} Failed to build CampusBag from Guid {campusGuid}" );
            }

            return ActionOk( campusBag );
        }

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<BenevolenceRequestBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<BenevolenceRequestBag> box )
        {
            var entityService = new BenevolenceRequestService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateBenevolenceRequest( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.BenevolenceRequestId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<BenevolenceRequestBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new BenevolenceRequestService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
        }

        #endregion
    }
}
