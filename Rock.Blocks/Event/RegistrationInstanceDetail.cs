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
using Rock.Constants;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Security;
using Rock.Security.SecurityGrantRules;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Event.RegistrationInstanceDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays the details of a Registration Instance for viewing and editing.
    /// </summary>
    [DisplayName( "Registration Instance - Instance Detail" )]
    [Category( "Event" )]
    [Description( "Displays the details of a Registration Instance for viewing and editing." )]
    [IconCssClass( "ti ti-file" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [AccountField(
        "Default Account",
        Description = "The default account to use for new registration instances",
        Key = AttributeKey.DefaultAccount,
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.FinancialAccount.EVENT_REGISTRATION,
        Order = 0 )]

    [LinkedPage(
        "Payment Reminder Page",
        Key = AttributeKey.PaymentReminderPage,
        Description = "The page for manually sending payment reminders.",
        IsRequired = false,
        Order = 1 )]

    [LinkedPage(
        "Group Placement Page",
        Key = AttributeKey.GroupPlacementPage,
        DefaultValue = Rock.SystemGuid.Page.GROUP_PLACEMENT + "," + Rock.SystemGuid.PageRoute.GROUP_PLACEMENT,
        Description = "The page for managing group placements.",
        IsRequired = false,
        Order = 2 )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Primary )]
    [Rock.SystemGuid.EntityTypeGuid( "B0B1C8FA-AD68-40AC-A208-B9EF342593B7" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "9C276E2A-D799-4CF9-84B2-42B465336E28" )]
    [Rock.SystemGuid.BlockTypeGuid( "22B67EDB-6D13-4D29-B722-DF45367AA3CB" )]
    public class RegistrationInstanceDetail : RockEntityDetailBlockType<RegistrationInstance, RegistrationInstanceBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DefaultAccount = "DefaultAccount";
            public const string PaymentReminderPage = "PaymentReminderPage";
            public const string GroupPlacementPage = "GroupPlacementPage";
        }

        private static class PageParameterKey
        {
            public const string RegistrationInstanceId = "RegistrationInstanceId";
            public const string RegistrationTemplateId = "RegistrationTemplateId";
            public const string RegistrationTemplatePlacementId = "RegistrationTemplatePlacementId";
            public const string ReturnUrl = "ReturnUrl";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        /// <summary>
        /// Default values for the registration session timeout fields. The production values
        /// live on <see cref="RegistrationInstance.DefaultTimeoutLength"/> and
        /// <see cref="RegistrationInstance.DefaultTimeoutThreshold"/> but are marked internal,
        /// so we duplicate them here.
        /// </summary>
        private static class SessionTimeoutDefaults
        {
            public const int LengthMinutes = 15;
            public const int ThresholdPercentage = 33;
        }

        #endregion Keys

        #region Fields

        private RegistrationInstance _instance;
        private bool _isInstanceKeyMissing;

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<RegistrationInstanceBag, RegistrationInstanceDetailOptionsBag>();
            var entity = GetInitialEntity();

            SetBoxInitialEntityState( box, entity );

            box.Options = GetBoxOptions( entity );
            box.NavigationUrls = GetBoxNavigationUrls( entity );
            box.SecurityGrantToken = GetSecurityGrantToken();

            return box;
        }

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            return GetSecurityGrantToken();
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions. The
        /// AssetAndFileManagerSecurityGrantRule grants are required for the
        /// HtmlEditor's Image Browser and File Browser modals to load folder
        /// and file data from the asset manager API.
        /// </summary>
        /// <returns>A string that represents the security grant token.</returns>
        private string GetSecurityGrantToken()
        {
            var securityGrant = new SecurityGrant();

            securityGrant.AddRule( new AssetAndFileManagerSecurityGrantRule( Authorization.VIEW ) );
            securityGrant.AddRule( new AssetAndFileManagerSecurityGrantRule( Authorization.EDIT ) );
            securityGrant.AddRule( new AssetAndFileManagerSecurityGrantRule( Authorization.DELETE ) );

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Populates the box with the initial entity state and prepares it for view or edit.
        /// </summary>
        /// <param name="box">The box to populate.</param>
        /// <param name="entity">The resolved entity, or <c>null</c> when the supplied key did not match any record.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<RegistrationInstanceBag, RegistrationInstanceDetailOptionsBag> box, RegistrationInstance entity )
        {
            if ( entity == null )
            {
                if ( !_isInstanceKeyMissing )
                {
                    box.ErrorMessage = $"The specified {RegistrationInstance.FriendlyTypeName} could not be found.";
                }

                return;
            }

            var isViewable = entity.Id == 0 || entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = IsAuthorizedForEdit( entity );

            entity.LoadAttributes( RockContext );

            if ( entity.Id != 0 )
            {
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( RegistrationInstance.FriendlyTypeName );
                }
            }
            else
            {
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( RegistrationInstance.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <inheritdoc/>
        protected override RegistrationInstance GetInitialEntity()
        {
            return _instance ?? ( _instance = LoadInitialEntity() );
        }

        /// <summary>
        /// Resolves the initial entity from page parameters. Returns <c>null</c> when the
        /// RegistrationInstanceId parameter is missing, or was supplied but did not match an
        /// existing record — this lets the initial-state logic surface an error (or hide the
        /// panel) instead of silently dropping into a blank Add form.
        /// </summary>
        private RegistrationInstance LoadInitialEntity()
        {
            var entityService = new RegistrationInstanceService( RockContext );
            var key = PageParameter( PageParameterKey.RegistrationInstanceId );

            // A blank/missing RegistrationInstanceId parameter means the page wasn't scoped to
            // a specific instance — return null so the panel is hidden instead of seeding a
            // blank Add form (Add requires the explicit "0" sentinel below).
            if ( key.IsNullOrWhiteSpace() )
            {
                _isInstanceKeyMissing = true;
                return null;
            }

            if ( key != "0" )
            {
                var tracked = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );
                return tracked == null ? null : GetHydratedEntity( tracked.Id );
            }

            // Explicit "0" new-entity sentinel: seed a blank instance from the current block
            // defaults and any RegistrationTemplateId page parameter.
            var parentTemplateId = PageParameter( PageParameterKey.RegistrationTemplateId ).AsIntegerOrNull();

            var entity = new RegistrationInstance
            {
                Id = 0,
                Guid = Guid.Empty,
                IsActive = true,
                RegistrationTemplateId = parentTemplateId ?? 0
            };

            var accountGuid = GetAttributeValue( AttributeKey.DefaultAccount ).AsGuidOrNull();
            if ( accountGuid.HasValue )
            {
                var account = FinancialAccountCache.Get( accountGuid.Value );
                if ( account != null && account.IsActive )
                {
                    entity.AccountId = account.Id;

                    // Populate the Account navigation from the cache so GetCommonEntityBag's
                    // entity.Account.ToListItemBag() produces a valid picker value. Without
                    // this the default account configured on the block is silently dropped
                    // on the Add form because only AccountId was set.
                    entity.Account = new FinancialAccount
                    {
                        Id = account.Id,
                        Guid = account.Guid,
                        Name = account.Name,
                        IsActive = true
                    };
                }
            }

            if ( entity.RegistrationTemplateId > 0 )
            {
                entity.RegistrationTemplate = new RegistrationTemplateService( RockContext )
                    .Queryable()
                    .Include( t => t.FinancialGateway )
                    .Include( t => t.Fees )
                    .AsNoTracking()
                    .FirstOrDefault( t => t.Id == entity.RegistrationTemplateId );
            }

            return entity;
        }

        /// <inheritdoc/>
        protected override RegistrationInstanceBag GetEntityBagForView( RegistrationInstance entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );
            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );
            bag.GroupPlacements = GetGroupPlacements( entity );
            bag.HasActivePaymentPlans = HasActivePaymentPlans( entity );

            return bag;
        }

        /// <inheritdoc/>
        protected override RegistrationInstanceBag GetEntityBagForEdit( RegistrationInstance entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );
            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            PopulateGatewayPickers( bag, entity );

            return bag;
        }

        /// <summary>
        /// Builds the bag values that are common to both view and edit modes.
        /// </summary>
        private RegistrationInstanceBag GetCommonEntityBag( RegistrationInstance entity )
        {
            var template = entity.RegistrationTemplate;
            var isSetCostOnInstance = template?.SetCostOnInstance == true;
            var isGatewayConfigured = template?.FinancialGatewayId.HasValue == true;
            var isPaymentPlanAllowed = template?.IsPaymentPlanAllowed == true;
            var isRedirectionGateway = template?.FinancialGateway?.IsRedirectionGateway() == true;

            return new RegistrationInstanceBag
            {
                IdKey = entity.IdKey,
                Name = entity.Name,
                IsActive = entity.IsActive,
                Details = entity.Details,
                StartDateTime = entity.StartDateTime?.ToRockDateTimeOffset(),
                EndDateTime = entity.EndDateTime?.ToRockDateTimeOffset(),
                MaxAttendees = entity.MaxAttendees,
                RegistrationWorkflowType = entity.RegistrationWorkflowType.ToListItemBag(),
                RegistrantRecordSource = entity.RegistrantRecordSourceValue.ToListItemBag(),
                Cost = entity.Cost,
                MinimumInitialPayment = entity.MinimumInitialPayment,
                DefaultPayment = entity.DefaultPayment,
                Account = entity.Account.ToListItemBag(),
                ContactPersonAlias = entity.ContactPersonAlias.ToListItemBag(),
                ContactPhone = entity.ContactPhone,
                ContactEmail = entity.ContactEmail,
                SendReminderDateTime = entity.SendReminderDateTime?.ToRockDateTimeOffset(),
                PaymentDeadlineDate = entity.PaymentDeadlineDate?.ToRockDateTimeOffset(),
                ReminderSent = entity.ReminderSent,
                RegistrationInstructions = entity.RegistrationInstructions,
                AdditionalReminderDetails = entity.AdditionalReminderDetails,
                AdditionalConfirmationDetails = entity.AdditionalConfirmationDetails,
                TimeoutLengthMinutes = entity.TimeoutLengthMinutes ?? SessionTimeoutDefaults.LengthMinutes,
                TimeoutThreshold = entity.TimeoutThreshold ?? SessionTimeoutDefaults.ThresholdPercentage,
                ExternalGatewayMerchantId = entity.ExternalGatewayMerchantId?.ToString(),
                ExternalGatewayFundId = entity.ExternalGatewayFundId?.ToString(),
                RegistrationTemplateName = template?.Name,
                RegistrationTemplateIdKey = template?.IdKey,
                IsPaymentPlanAllowed = isPaymentPlanAllowed,
                IsSetCostOnInstance = isSetCostOnInstance,
                IsFinancialGatewayConfigured = isGatewayConfigured,
                IsRedirectionGateway = isRedirectionGateway,
                StatusText = ComputeStatusText( entity ),
                StatusLabelType = ComputeStatusLabelType( entity ),
                CanSendPaymentReminder = CanSendPaymentReminder( entity )
            };
        }

        /// <summary>
        /// Populates the merchant / fund pickers for a redirection gateway.
        /// </summary>
        private static void PopulateGatewayPickers( RegistrationInstanceBag bag, RegistrationInstance entity )
        {
            var gateway = entity.RegistrationTemplate?.FinancialGateway;
            if ( gateway == null || !gateway.IsRedirectionGateway() )
            {
                return;
            }

            var gatewayComponent = gateway.GetGatewayComponent() as IRedirectionGatewayComponent;
            if ( gatewayComponent == null )
            {
                return;
            }

            bag.GatewayMerchantFieldLabel = gatewayComponent.MerchantFieldLabel;
            bag.GatewayFundFieldLabel = gatewayComponent.FundFieldLabel;

            var merchants = gatewayComponent.GetMerchants()
                .ToDictionary( kvp => kvp.Key, kvp => kvp.Value );
            bag.GatewayMerchants = merchants
                .Select( kvp => new ListItemBag { Value = kvp.Key, Text = kvp.Value } )
                .ToList();

            if ( entity.ExternalGatewayMerchantId.HasValue
                 && merchants.ContainsKey( entity.ExternalGatewayMerchantId.Value.ToString() ) )
            {
                var funds = gatewayComponent.GetMerchantFunds( entity.ExternalGatewayMerchantId.Value.ToString() );
                bag.GatewayFunds = funds
                    .Select( kvp => new ListItemBag { Value = kvp.Key, Text = kvp.Value } )
                    .ToList();
            }
            else
            {
                bag.GatewayFunds = new List<ListItemBag>();
            }
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( RegistrationInstance entity, ValidPropertiesBox<RegistrationInstanceBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            // Details has no editor in this block but must be round-tripped so that
            // a copied instance carries its Details forward on the save following Copy.
            box.IfValidProperty( nameof( box.Bag.Details ),
                () => entity.Details = box.Bag.Details );

            box.IfValidProperty( nameof( box.Bag.StartDateTime ),
                () => entity.StartDateTime = box.Bag.StartDateTime?.DateTime );

            box.IfValidProperty( nameof( box.Bag.EndDateTime ),
                () => entity.EndDateTime = box.Bag.EndDateTime?.DateTime );

            box.IfValidProperty( nameof( box.Bag.MaxAttendees ),
                () =>
                {
                    entity.MaxAttendees = box.Bag.MaxAttendees;
                    // Session timeout auto-enables whenever MaxAttendees is set.
                    entity.TimeoutIsEnabled = box.Bag.MaxAttendees.HasValue;
                } );

            box.IfValidProperty( nameof( box.Bag.RegistrationWorkflowType ),
                () => entity.RegistrationWorkflowTypeId = box.Bag.RegistrationWorkflowType.GetEntityId<WorkflowType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.RegistrantRecordSource ),
                () => entity.RegistrantRecordSourceValueId = box.Bag.RegistrantRecordSource.GetEntityId<DefinedValue>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.Cost ),
                () => entity.Cost = box.Bag.Cost );

            box.IfValidProperty( nameof( box.Bag.MinimumInitialPayment ),
                () => entity.MinimumInitialPayment = box.Bag.MinimumInitialPayment );

            box.IfValidProperty( nameof( box.Bag.DefaultPayment ),
                () => entity.DefaultPayment = box.Bag.DefaultPayment );

            box.IfValidProperty( nameof( box.Bag.Account ),
                () => entity.AccountId = box.Bag.Account.GetEntityId<FinancialAccount>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.ContactPersonAlias ),
                () => entity.ContactPersonAliasId = box.Bag.ContactPersonAlias.GetEntityId<PersonAlias>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.ContactPhone ),
                () => entity.ContactPhone = box.Bag.ContactPhone );

            box.IfValidProperty( nameof( box.Bag.ContactEmail ),
                () => entity.ContactEmail = box.Bag.ContactEmail );

            box.IfValidProperty( nameof( box.Bag.SendReminderDateTime ),
                () => entity.SendReminderDateTime = box.Bag.SendReminderDateTime?.DateTime );

            box.IfValidProperty( nameof( box.Bag.PaymentDeadlineDate ),
                () => entity.PaymentDeadlineDate = box.Bag.PaymentDeadlineDate?.DateTime );

            box.IfValidProperty( nameof( box.Bag.ReminderSent ),
                () => entity.ReminderSent = box.Bag.ReminderSent );

            box.IfValidProperty( nameof( box.Bag.RegistrationInstructions ),
                () => entity.RegistrationInstructions = box.Bag.RegistrationInstructions );

            box.IfValidProperty( nameof( box.Bag.AdditionalReminderDetails ),
                () => entity.AdditionalReminderDetails = box.Bag.AdditionalReminderDetails );

            box.IfValidProperty( nameof( box.Bag.AdditionalConfirmationDetails ),
                () => entity.AdditionalConfirmationDetails = box.Bag.AdditionalConfirmationDetails );

            box.IfValidProperty( nameof( box.Bag.TimeoutLengthMinutes ),
                () => entity.TimeoutLengthMinutes = box.Bag.TimeoutLengthMinutes ?? SessionTimeoutDefaults.LengthMinutes );

            box.IfValidProperty( nameof( box.Bag.TimeoutThreshold ),
                () => entity.TimeoutThreshold = box.Bag.TimeoutThreshold ?? SessionTimeoutDefaults.ThresholdPercentage );

            if ( box.Bag.IsRedirectionGateway )
            {
                box.IfValidProperty( nameof( box.Bag.ExternalGatewayMerchantId ),
                    () => entity.ExternalGatewayMerchantId = box.Bag.ExternalGatewayMerchantId.AsIntegerOrNull() );

                box.IfValidProperty( nameof( box.Bag.ExternalGatewayFundId ),
                    () => entity.ExternalGatewayFundId = box.Bag.ExternalGatewayFundId.AsIntegerOrNull() );
            }
            else
            {
                entity.ExternalGatewayMerchantId = null;
                entity.ExternalGatewayFundId = null;
            }

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );
                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out RegistrationInstance entity, out BlockActionResult error )
        {
            var entityService = new RegistrationInstanceService( RockContext );
            error = null;

            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                entity = new RegistrationInstance();

                var parentTemplateId = PageParameter( PageParameterKey.RegistrationTemplateId ).AsIntegerOrNull();
                if ( parentTemplateId.HasValue )
                {
                    entity.RegistrationTemplateId = parentTemplateId.Value;
                }

                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{RegistrationInstance.FriendlyTypeName} not found." );
                return false;
            }

            if ( !IsAuthorizedForEdit( entity ) )
            {
                error = ActionForbidden( $"Not authorized to edit {RegistrationInstance.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true when the current person is authorized to edit, delete, or administrate
        /// this instance — block-level EDIT plus entity-level EDIT/ADMINISTRATE.
        /// </summary>
        private bool IsAuthorizedForEdit( RegistrationInstance entity )
        {
            if ( BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return true;
            }

            if ( entity == null )
            {
                return false;
            }

            return entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
                || entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var key = pageReference.GetPageParameter( PageParameterKey.RegistrationInstanceId );
            var breadCrumbs = new List<IBreadCrumb>();

            if ( key.IsNullOrWhiteSpace() )
            {
                return new BreadCrumbResult { BreadCrumbs = breadCrumbs };
            }

            if ( key == "0" )
            {
                breadCrumbs.Add( new BreadCrumbLink( "New Registration Instance", pageReference ) );
                return new BreadCrumbResult { BreadCrumbs = breadCrumbs };
            }

            var name = new RegistrationInstanceService( RockContext )
                .GetSelect( key, i => i.Name, !PageCache.Layout.Site.DisablePredictableIds );

            if ( name.IsNullOrWhiteSpace() )
            {
                return new BreadCrumbResult { BreadCrumbs = breadCrumbs };
            }

            var pageParameters = new Dictionary<string, string>
            {
                [PageParameterKey.RegistrationInstanceId] = key
            };

            breadCrumbs.Add( new BreadCrumbLink( name, new PageReference( pageReference.PageId, 0, pageParameters ) ) );

            return new BreadCrumbResult { BreadCrumbs = breadCrumbs };
        }

        /// <summary>
        /// Gets the navigation URLs used by the frontend. The ParentPage URL always includes
        /// the RegistrationTemplateId so cancel/delete/wizard redirects keep the user scoped
        /// to the correct template.
        /// </summary>
        /// <param name="entity">The resolved entity, used to derive the template id when it is
        /// not present on the page parameters (e.g. when viewing an existing instance).</param>
        private Dictionary<string, string> GetBoxNavigationUrls( RegistrationInstance entity )
        {
            var templateId = entity?.RegistrationTemplateId > 0
                ? entity.RegistrationTemplateId.ToString()
                : PageParameter( PageParameterKey.RegistrationTemplateId );

            var parentParams = new Dictionary<string, string>();
            if ( templateId.IsNotNullOrWhiteSpace() )
            {
                parentParams[PageParameterKey.RegistrationTemplateId] = templateId;
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( parentParams )
            };
        }

        /// <summary>
        /// Builds the options bag used by the view and edit panels. Takes the resolved entity
        /// so the reminder URL can use the entity's IdKey rather than echoing whatever raw
        /// identifier was on the page URL.
        /// </summary>
        /// <param name="entity">The resolved entity, or <c>null</c> when the supplied key did not match any record.</param>
        private RegistrationInstanceDetailOptionsBag GetBoxOptions( RegistrationInstance entity )
        {
            string reminderPageUrl = null;

            if ( GetAttributeValue( AttributeKey.PaymentReminderPage ).IsNotNullOrWhiteSpace() )
            {
                var reminderInstanceKey = entity != null && entity.Id > 0
                    ? entity.IdKey
                    : PageParameter( PageParameterKey.RegistrationInstanceId );

                reminderPageUrl = this.GetLinkedPageUrl( AttributeKey.PaymentReminderPage, new Dictionary<string, string>
                {
                    [PageParameterKey.RegistrationInstanceId] = reminderInstanceKey
                } );
            }

            return new RegistrationInstanceDetailOptionsBag
            {
                PaymentReminderPageUrl = reminderPageUrl
            };
        }

        /// <summary>
        /// Computes the "Open" or "Closed" label text for the given instance.
        /// </summary>
        private static string ComputeStatusText( RegistrationInstance entity )
        {
            return IsRegistrationOpen( entity ) ? "Open" : "Closed";
        }

        /// <summary>
        /// Computes the highlight label type for the status label.
        /// </summary>
        private static string ComputeStatusLabelType( RegistrationInstance entity )
        {
            return IsRegistrationOpen( entity ) ? "success" : "type";
        }

        /// <summary>
        /// Returns true when the registration window is currently open (or unbounded on either side).
        /// </summary>
        private static bool IsRegistrationOpen( RegistrationInstance entity )
        {
            var now = RockDateTime.Now;
            var isAfterStart = entity.StartDateTime == null || entity.StartDateTime.Value <= now;
            var isBeforeEnd = entity.EndDateTime == null || entity.EndDateTime.Value >= now;
            return isAfterStart && isBeforeEnd;
        }

        /// <summary>
        /// Returns true when the "Send Payment Reminders" shortcut should be shown.
        /// </summary>
        private bool CanSendPaymentReminder( RegistrationInstance entity )
        {
            if ( GetAttributeValue( AttributeKey.PaymentReminderPage ).IsNullOrWhiteSpace() )
            {
                return false;
            }

            var template = entity.RegistrationTemplate;
            if ( template == null )
            {
                return false;
            }

            if ( template.SetCostOnInstance == true && entity.Cost.HasValue && entity.Cost.Value > 0 )
            {
                return true;
            }

            if ( template.Cost > 0 )
            {
                return true;
            }

            return template.Fees != null && template.Fees.Count > 0;
        }

        /// <summary>
        /// Returns true when at least one registration in the instance has an active scheduled payment plan.
        /// </summary>
        private bool HasActivePaymentPlans( RegistrationInstance entity )
        {
            if ( entity.Id == 0 )
            {
                return false;
            }

            return new RegistrationService( RockContext ).Queryable()
                .AsNoTracking()
                .Any( r => r.RegistrationInstanceId == entity.Id
                    && r.PaymentPlanFinancialScheduledTransaction != null
                    && r.PaymentPlanFinancialScheduledTransaction.IsActive );
        }

        /// <summary>
        /// Builds the group placement link list displayed on the view panel.
        /// </summary>
        private List<RegistrationInstanceGroupPlacementBag> GetGroupPlacements( RegistrationInstance entity )
        {
            if ( entity.Id == 0 || GetAttributeValue( AttributeKey.GroupPlacementPage ).IsNullOrWhiteSpace() )
            {
                return new List<RegistrationInstanceGroupPlacementBag>();
            }

            var rawPlacements = new RegistrationTemplatePlacementService( RockContext ).Queryable()
                .Where( p => p.RegistrationTemplateId == entity.RegistrationTemplateId )
                .Select( p => new { p.Id, p.Name } )
                .ToList();

            var currentPageUrl = this.GetCurrentPageUrl();

            var placements = new List<RegistrationInstanceGroupPlacementBag>();
            foreach ( var placement in rawPlacements )
            {
                var url = this.GetLinkedPageUrl( AttributeKey.GroupPlacementPage, new Dictionary<string, string>
                {
                    [PageParameterKey.RegistrationInstanceId] = entity.Id.ToString(),
                    [PageParameterKey.RegistrationTemplatePlacementId] = placement.Id.ToString(),
                    [PageParameterKey.ReturnUrl] = currentPageUrl
                } );

                if ( url.IsNotNullOrWhiteSpace() )
                {
                    placements.Add( new RegistrationInstanceGroupPlacementBag
                    {
                        Name = placement.Name,
                        Url = url
                    } );
                }
            }

            return placements;
        }

        /// <summary>
        /// Gets the <see cref="RegistrationInstance"/> entity reloaded with navigation properties hydrated.
        /// Used after save / copy so the returned view bag can display template-dependent state.
        /// </summary>
        private RegistrationInstance GetHydratedEntity( int id )
        {
            return new RegistrationInstanceService( RockContext ).Queryable()
                .Include( a => a.RegistrationTemplate )
                .Include( a => a.RegistrationTemplate.FinancialGateway )
                .Include( a => a.RegistrationTemplate.Fees )
                .Include( a => a.Account )
                .Include( a => a.ContactPersonAlias.Person )
                .Include( a => a.RegistrationWorkflowType )
                .Include( a => a.RegistrantRecordSourceValue )
                .AsNoTracking()
                .FirstOrDefault( a => a.Id == id );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Returns the edit-mode bag for the requested entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // GetHydratedEntity returns a non-tracked version with navigation properties for building the bag.
            var hydrated = entity.Id > 0 ? GetHydratedEntity( entity.Id ) ?? entity : entity;
            hydrated.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( hydrated );

            return ActionOk( new ValidPropertiesBox<RegistrationInstanceBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the registration instance described by the box. Returns the refreshed view bag on edit,
        /// or a redirect URL on add so the page can re-enter with the new instance id.
        /// </summary>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<RegistrationInstanceBag> box )
        {
            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            /*
                 4/22/2026 - MSE

                 When copying a Registration Instance, the page URL does not include the RegistrationTemplateId.
                 Because of this, TryGetEntityForEditAction cannot set the template during save.

                 This logic uses the RegistrationTemplateIdKey from the bag to set the template if available.

                 Reason: Ensure copied Registration Instances keep the correct Registration Template.
            */
            if ( entity.Id == 0
                && entity.RegistrationTemplateId == 0
                && box.Bag.RegistrationTemplateIdKey.IsNotNullOrWhiteSpace() )
            {
                var resolvedTemplateId = new RegistrationTemplateService( RockContext )
                    .GetSelect( box.Bag.RegistrationTemplateIdKey, t => ( int? ) t.Id, !PageCache.Layout.Site.DisablePredictableIds );
                if ( resolvedTemplateId.HasValue )
                {
                    entity.RegistrationTemplateId = resolvedTemplateId.Value;
                }
            }

            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Server-side validation of required fields so a client that bypasses the frontend
            // rules can't persist an invalid record. Uses the bag's Is* flags because the
            // entity's RegistrationTemplate navigation may not be loaded at this point.
            if ( entity.Name.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Registration Instance Name is required." );
            }

            if ( box.Bag.IsFinancialGatewayConfigured && !entity.AccountId.HasValue )
            {
                return ActionBadRequest( "Account is required." );
            }

            if ( box.Bag.IsPaymentPlanAllowed && !entity.PaymentDeadlineDate.HasValue )
            {
                return ActionBadRequest( "Payment Deadline is required." );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            if ( isNew )
            {
                // Preserve the RegistrationTemplateId in the redirect URL so the page retains
                // its template scoping after a new-instance save.
                var urlParams = new Dictionary<string, string>
                {
                    [PageParameterKey.RegistrationInstanceId] = entity.IdKey
                };

                var templateId = entity.RegistrationTemplateId > 0
                    ? entity.RegistrationTemplateId.ToString()
                    : PageParameter( PageParameterKey.RegistrationTemplateId );

                if ( templateId.IsNotNullOrWhiteSpace() )
                {
                    urlParams[PageParameterKey.RegistrationTemplateId] = templateId;
                }

                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( urlParams ) );
            }

            var hydrated = GetHydratedEntity( entity.Id ) ?? entity;
            hydrated.LoadAttributes( RockContext );

            var bag = GetEntityBagForView( hydrated );

            return ActionOk( new ValidPropertiesBox<RegistrationInstanceBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Deletes the registration instance. If any registrations have active payment plans,
        /// they are cancelled first; any errors or warnings abort the delete.
        /// </summary>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new RegistrationInstanceService( RockContext );
            var financialScheduledTransactionService = new FinancialScheduledTransactionService( RockContext );
            var registrationService = new RegistrationService( RockContext );

            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionNotFound();
            }

            if ( !IsAuthorizedForEdit( entity ) )
            {
                return ActionForbidden( "You are not authorized to delete this registration instance." );
            }

            var errors = new List<string>();
            var warnings = new List<string>();

            foreach ( var registration in entity.Registrations.ToList() )
            {
                var success = registrationService.TryCancelPaymentPlan(
                    registration,
                    financialScheduledTransactionService,
                    out var error,
                    out var warning );

                var registrationInfo = $"Registration Id {registration.Id} ({registration.FirstName} {registration.LastName})";

                if ( !success )
                {
                    errors.Add( $"{registrationInfo}: {error ?? "Unknown error"}" );
                }

                if ( warning.IsNotNullOrWhiteSpace() )
                {
                    warnings.Add( $"{registrationInfo}: {warning}" );
                }
            }

            if ( errors.Any() )
            {
                return ActionBadRequest(
                    "The following registrations could not have their payment plans cancelled:<br/>"
                    + string.Join( "<br/>", errors ) );
            }

            if ( warnings.Any() )
            {
                return ActionBadRequest(
                    "Warnings occurred for the following registrations:<br/>"
                    + string.Join( "<br/>", warnings ) );
            }

            /*
                4/22/2026 - MSE

                Save the cancelled-plan changes and delete the instance + its registrations atomically.
                If any plan cancellation had errored or warned above, we bail *before* saving any of the
                plan changes — so there is no scenario where plans are cancelled in the database but
                the registrations themselves are not removed.

                Reason: Transactional consistency between plan cancellation and instance deletion.
            */

            var templateId = entity.RegistrationTemplateId;

            RockContext.SaveChanges();

            RockContext.WrapTransaction( () =>
            {
                registrationService.DeleteRange( entity.Registrations );
                entityService.Delete( entity );
                RockContext.SaveChanges();
            } );

            return ActionOk( this.GetParentPageUrl( new Dictionary<string, string>
            {
                [PageParameterKey.RegistrationTemplateId] = templateId.ToString()
            } ) );
        }

        /// <summary>
        /// Clones the specified registration instance (without saving) and returns a bag
        /// that the frontend uses to seed edit mode. The cloned instance gets a
        /// " - Copy" suffix, starts active, and has its reminder flags reset.
        /// </summary>
        [BlockAction]
        public BlockActionResult Copy( string key )
        {
            if ( key.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "A registration instance key is required." );
            }

            var entityService = new RegistrationInstanceService( RockContext );
            var original = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( original == null )
            {
                return ActionNotFound();
            }

            if ( !IsAuthorizedForEdit( original ) )
            {
                return ActionForbidden( "You are not authorized to copy this registration instance." );
            }

            var hydrated = GetHydratedEntity( original.Id ) ?? original;
            hydrated.LoadAttributes( RockContext );

            /*
                4/22/2026 - MSE

                Build the bag from the fully-hydrated source entity (so picker ListItemBags
                are populated from navigation properties), then reshape it to represent an
                unsaved copy — null out the IdKey, add " - Copy", reset reminder flags, etc.
                This avoids the fragility of CloneWithoutIdentity(), which preserves IDs on
                value properties but leaves all navigation references null — that caused
                pickers to render blank and their IDs to be silently cleared on save.

                Reason: Carry attributes + picker selections forward on Copy without the
                navigation-null trap of a deep entity clone.
            */

            var bag = GetEntityBagForEdit( hydrated );
            bag.IdKey = null;
            bag.Name = $"{hydrated.Name} - Copy";
            bag.IsActive = true;
            bag.ReminderSent = false;
            bag.SendReminderDateTime = null;

            // Don't carry forward the account if it has been deactivated since the original was created.
            if ( hydrated.Account != null && !hydrated.Account.IsActive )
            {
                bag.Account = null;
            }

            return ActionOk( new ValidPropertiesBox<RegistrationInstanceBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Returns the email and work phone for the selected contact person.
        /// Used by the edit panel to auto-fill blank contact fields when the user
        /// picks a person.
        /// </summary>
        /// <param name="personAliasGuid">The PersonAlias Guid emitted by the PersonPicker.</param>
        [BlockAction]
        public BlockActionResult GetContactInfo( Guid personAliasGuid )
        {
            var personId = new PersonAliasService( RockContext ).GetPersonId( personAliasGuid );
            if ( !personId.HasValue )
            {
                return ActionOk( new { Email = string.Empty, Phone = string.Empty } );
            }

            var workPhoneTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );

            var contactInfo = new PersonService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( p => p.Id == personId.Value )
                .Select( p => new
                {
                    Email = p.Email,
                    Phone = p.PhoneNumbers
                        .Where( n => n.NumberTypeValueId == workPhoneTypeId )
                        .Select( n => n.NumberFormatted )
                        .FirstOrDefault()
                } )
                .FirstOrDefault();

            return ActionOk( new
            {
                Email = contactInfo?.Email ?? string.Empty,
                Phone = contactInfo?.Phone ?? string.Empty
            } );
        }

        /// <summary>
        /// Returns the fund list for a selected gateway merchant. Used by the edit
        /// panel to cascade the fund dropdown when the merchant changes.
        /// </summary>
        /// <param name="templateKey">The registration template identifier key (from the bag).
        /// The template — not the instance — owns the gateway, so this works for both
        /// existing and new (unsaved) instances.</param>
        /// <param name="merchantId">The selected merchant identifier.</param>
        [BlockAction]
        public BlockActionResult GetGatewayFunds( string templateKey, string merchantId )
        {
            if ( templateKey.IsNullOrWhiteSpace() || merchantId.IsNullOrWhiteSpace() )
            {
                return ActionOk( new List<ListItemBag>() );
            }

            var gateway = new RegistrationTemplateService( RockContext )
                .GetSelect( templateKey, t => t.FinancialGateway, !PageCache.Layout.Site.DisablePredictableIds );

            if ( gateway == null || !gateway.IsRedirectionGateway() )
            {
                return ActionOk( new List<ListItemBag>() );
            }

            if ( !( gateway.GetGatewayComponent() is IRedirectionGatewayComponent gatewayComponent ) )
            {
                return ActionOk( new List<ListItemBag>() );
            }

            var funds = gatewayComponent.GetMerchantFunds( merchantId )
                .Select( kvp => new ListItemBag { Value = kvp.Key, Text = kvp.Value } )
                .ToList();

            return ActionOk( funds );
        }

        #endregion Block Actions
    }
}
