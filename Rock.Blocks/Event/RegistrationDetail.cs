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
using Rock.Model.Event.Registration.Options;
using Rock.Security;
using Rock.Tasks;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Event.RegistrationDetail;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays the details of a given registration.
    /// </summary>

    [DisplayName( "Registration Detail" )]
    [Category( "Event" )]
    [Description( "Displays the details of a given registration." )]
    [IconCssClass( "ti ti-users" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [SecurityAction( SecurityActionKey.EditPaymentPlan, "The roles and/or users that can edit the payment plan for the selected persons." )]

    #region Block Attributes

    [LinkedPage( "Registrant Page",
        Description = "The page for viewing details about a registrant",
        Key = AttributeKey.RegistrantPage,
        IsRequired = true,
        Order = 0 )]

    [LinkedPage( "Transaction Page",
        Description = "The page for viewing transaction details",
        Key = AttributeKey.TransactionPage,
        IsRequired = true,
        Order = 1 )]

    [LinkedPage( "Group Detail Page",
        Description = "The page for viewing details about a group",
        Key = AttributeKey.GroupDetailPage,
        IsRequired = true,
        Order = 2 )]

    [LinkedPage( "Group Member Page",
        Description = "The page for viewing details about a group member",
        Key = AttributeKey.GroupMemberPage,
        IsRequired = true,
        Order = 3 )]

    [LinkedPage( "Transaction Detail Page",
        Description = "The page for viewing details about a payment",
        Key = AttributeKey.TransactionDetailPage,
        IsRequired = true,
        Order = 4 )]

    [LinkedPage( "Audit Page",
        Description = "Page used to display the history of changes to a registration.",
        Key = AttributeKey.AuditPage,
        IsRequired = true,
        Order = 5 )]

    [DefinedValueField( "Source",
        Description = "The Financial Source Type to use when creating transactions",
        Key = AttributeKey.Source,
        IsRequired = false,
        AllowMultiple = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE,
        DefaultValue = Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_ONSITE_COLLECTION,
        Order = 6 )]

    [TextField( "Batch Name Prefix",
        Description = "The batch prefix name to use when creating a new batch",
        Key = AttributeKey.BatchNamePrefix,
        IsRequired = false,
        DefaultValue = "Event Registration",
        Order = 7 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "2356BD81-F2E0-46FC-807C-56AEEB682A33" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "A1C967B2-EEDA-416F-A53C-7BE46D6DA4E1" )]
    [Rock.SystemGuid.BlockTypeGuid( "DB8EA84A-4087-4919-9CF0-9353E45ECEEC" )]
    public class RegistrationDetail : RockEntityDetailBlockType<Registration, RegistrationBag>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string RegistrantPage = "RegistrantPage";
            public const string TransactionPage = "TransactionPage";
            public const string GroupDetailPage = "GroupDetailPage";
            public const string GroupMemberPage = "GroupMemberPage";
            public const string TransactionDetailPage = "TransactionDetailPage";
            public const string AuditPage = "AuditPage";
            public const string Source = "Source";
            public const string BatchNamePrefix = "BatchNamePrefix";
        }

        private static class PageParameterKey
        {
            public const string RegistrationId = "RegistrationId";
            public const string RegistrationInstanceId = "RegistrationInstanceId";
            public const string RegistrationTemplateId = "RegistrationTemplateId";
            public const string RegistrantId = "RegistrantId";
            public const string GroupId = "GroupId";
            public const string GroupMemberId = "GroupMemberId";
            public const string TransactionId = "TransactionId";
            public const string ReturnUrl = "ReturnUrl";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
            public const string RegistrantPage = "RegistrantPage";
            public const string GroupMemberPage = "GroupMemberPage";
            public const string TransactionDetailPage = "TransactionDetailPage";
            public const string AuditPage = "AuditPage";
        }

        private static class SecurityActionKey
        {
            public const string EditPaymentPlan = "EditPaymentPlan";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The message returned when a state changing action is called with
        /// anything other than a POST request.
        /// </summary>
        private static readonly string InvalidRequestMethodMessage = "Invalid request method.";

        /// <summary>
        /// The message returned when a payment amount is not valid.
        /// </summary>
        private static readonly string InvalidPaymentAmountMessage = "Payment amount must be greater than zero and less than or equal to the balance due.";

        /// <summary>
        /// The message returned when the registration has no registrar to pay for.
        /// </summary>
        private static readonly string LinkToIndividualMessage = "In order to process a payment you will need to link this registration to an individual.";

        /// <summary>
        /// The message returned when the registration instance has no usable account.
        /// </summary>
        private static readonly string AccountConfigurationMessage = "There was a problem with the account configuration for this registration.";

        /// <summary>
        /// The maximum number of characters allowed in payment comments.
        /// </summary>
        private static readonly int MaximumPaymentCommentsLength = 1000;

        /// <summary>
        /// The maximum number of characters allowed in a history caption.
        /// </summary>
        private static readonly int MaximumHistoryCaptionLength = 200;

        /// <summary>
        /// The registration loaded from the page parameters, cached for the request.
        /// </summary>
        private Registration _initialEntity;

        /// <summary>
        /// Indicates whether <see cref="_initialEntity"/> has been resolved.
        /// </summary>
        private bool _isInitialEntityLoaded;

        /// <summary>
        /// The registration loaded with everything needed to build its bags,
        /// cached until the next change is saved.
        /// </summary>
        private Registration _hydratedRegistration;

        /// <summary>
        /// The calculated money amounts of the registration, cached until the
        /// next change is saved.
        /// </summary>
        private RegistrationAmounts _registrationAmounts;

        /// <summary>
        /// The calculated payment plan of the registration, cached until the
        /// next change is saved.
        /// </summary>
        private PaymentPlan _paymentPlan;

        /// <summary>
        /// Indicates whether <see cref="_paymentPlan"/> has been resolved.
        /// </summary>
        private bool _isPaymentPlanLoaded;

        /// <summary>
        /// The next payment date of the payment plan, cached until the next
        /// change is saved.
        /// </summary>
        private DateTime? _nextPaymentDate;

        /// <summary>
        /// Indicates whether <see cref="_nextPaymentDate"/> has been resolved.
        /// </summary>
        private bool _isNextPaymentDateLoaded;

        /// <summary>
        /// The edit authorization results, keyed by registration identifier.
        /// </summary>
        private readonly Dictionary<int, bool> _canEditByRegistrationId = new Dictionary<int, bool>();

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<RegistrationBag, RegistrationDetailOptionsBag>();
            var entity = GetInitialEntity();

            SetBoxInitialEntityState( box, entity );

            box.NavigationUrls = box.Entity != null
                ? GetBoxNavigationUrls( entity )
                : new Dictionary<string, string>();
            box.Options = new RegistrationDetailOptionsBag();

            return box;
        }

        /// <summary>
        /// Sets the initial entity state of the box. The registration is only
        /// placed in the box when the current person can view it.
        /// </summary>
        /// <param name="box">The box to populate.</param>
        /// <param name="entity">The registration, or <c>null</c> if it could not be found.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<RegistrationBag, RegistrationDetailOptionsBag> box, Registration entity )
        {
            if ( entity == null )
            {
                box.ErrorMessage = $"The {Registration.FriendlyTypeName} was not found.";
            }
            else if ( !CanView( entity ) )
            {
                box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Registration.FriendlyTypeName ).StripHtml();
            }
            else
            {
                box.IsEditable = CanEdit( entity );
                entity.LoadAttributes( RockContext );
                box.Entity = GetEntityBagForView( entity );
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the navigation URLs that do not change while the page is open.
        /// Templated URLs contain <c>((Key))</c> which the client replaces.
        /// </summary>
        /// <param name="registration">The registration being displayed.</param>
        /// <returns>The navigation URLs.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls( Registration registration )
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = GetRegistrationParentPageUrl( registration ),
                [NavigationUrlKey.RegistrantPage] = this.GetLinkedPageUrl( AttributeKey.RegistrantPage, new Dictionary<string, string>
                {
                    [PageParameterKey.RegistrantId] = "((Key))",
                    [PageParameterKey.RegistrationId] = registration.IdKey
                } ),
                [NavigationUrlKey.GroupMemberPage] = this.GetLinkedPageUrl( AttributeKey.GroupMemberPage, PageParameterKey.GroupMemberId, "((Key))" ),
                [NavigationUrlKey.TransactionDetailPage] = this.GetLinkedPageUrl( AttributeKey.TransactionDetailPage, PageParameterKey.TransactionId, "((Key))" ),
                [NavigationUrlKey.AuditPage] = this.GetLinkedPageUrl( AttributeKey.AuditPage, PageParameterKey.RegistrationId, registration.IdKey )
            };
        }

        /// <summary>
        /// Gets the URL of the parent page, scoped to the instance of the
        /// registration as it is now stored (not the page parameter).
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <returns>The URL of the parent page.</returns>
        private string GetRegistrationParentPageUrl( Registration registration )
        {
            var instanceIdKey = IdHasher.Instance.GetHash( registration.RegistrationInstanceId );

            return this.GetParentPageUrl( new Dictionary<string, string>
            {
                [PageParameterKey.RegistrationInstanceId] = instanceIdKey
            } );
        }

        /// <summary>
        /// Gets the return URL page parameter when it is safe to redirect to.
        /// </summary>
        /// <returns>The return URL, or <c>null</c> when it is missing or not safe.</returns>
        private string GetSafeReturnUrl()
        {
            /*
                6/5/26 - MSE

                This block's parent page is the Registrations tab, so navigating to
                the parent page loses the tab the individual came from (Registrants,
                Payments, etc.). Linking blocks pass ReturnUrl so we can send them back.
                Only URLs the site considers safe are honored, which prevents open
                redirects.

                Reason: Preserve the source tab when navigating away from this block.
            */
            var returnUrl = PageParameter( PageParameterKey.ReturnUrl );

            if ( returnUrl.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var site = PageCache?.Layout?.Site;

            if ( site == null || !site.IsSafeRedirectUrl( returnUrl, RequestContext.RequestUri ) )
            {
                return null;
            }

            return returnUrl;
        }

        /// <inheritdoc/>
        protected override Registration GetInitialEntity()
        {
            if ( !_isInitialEntityLoaded )
            {
                _initialEntity = LoadInitialEntity();
                _isInitialEntityLoaded = true;
            }

            return _initialEntity;
        }

        /// <summary>
        /// Loads the registration identified by the page parameters. A blank or
        /// empty key never creates a new registration; registrations are only
        /// created by the registration entry block.
        /// </summary>
        /// <returns>The registration, or <c>null</c> if it could not be found.</returns>
        private Registration LoadInitialEntity()
        {
            var key = PageParameter( PageParameterKey.RegistrationId );

            if ( IsBlankKey( key ) )
            {
                return null;
            }

            var query = new RegistrationService( RockContext )
                .GetQueryableByKey( key, !PageCache.Layout.Site.DisablePredictableIds );

            var registration = IncludeRegistrationGraph( query ).FirstOrDefault();

            if ( registration != null )
            {
                _hydratedRegistration = registration;
            }

            return registration;
        }

        /// <inheritdoc/>
        protected override SecurityGrant GetSecurityGrant( Registration entity )
        {
            /*
                10/6/2026 - MSE

                Attribute grants can include asset manager rules (HTML and asset
                attributes), and the token is also renewed from page parameters
                posted by the client. Only people who can edit the registration
                receive attribute rules.

                Reason: Do not hand out file manager access to view-only people.
            */
            if ( entity == null || !CanEdit( entity ) )
            {
                return new SecurityGrant();
            }

            return base.GetSecurityGrant( entity );
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out Registration entity, out BlockActionResult error )
        {
            entity = null;
            error = null;

            if ( IsBlankKey( idKey ) )
            {
                error = ActionNotFound( $"The {Registration.FriendlyTypeName} was not found." );
                return false;
            }

            entity = new RegistrationService( RockContext ).Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                error = ActionNotFound( $"The {Registration.FriendlyTypeName} was not found." );
                return false;
            }

            if ( !CanEdit( entity ) )
            {
                error = ActionForbidden( $"You are not authorized to edit this {Registration.FriendlyTypeName.ToLower()}." );
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        protected override RegistrationBag GetEntityBagForView( Registration entity )
        {
            if ( entity == null )
            {
                return null;
            }

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( RockContext );
            }

            var instance = entity.RegistrationInstance;
            var template = instance.RegistrationTemplate;
            var person = entity.PersonAlias?.Person;
            var amounts = GetRegistrationAmounts( entity );
            var canEdit = CanEdit( entity );

            var bag = GetCommonEntityBag( entity );
            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            bag.Group = entity.Group != null
                ? new ListItemBag { Value = entity.Group.Guid.ToString(), Text = entity.Group.Name }
                : null;
            bag.RegistrarName = person != null
                ? person.FullName
                : $"{entity.FirstName} {entity.LastName}".Trim();
            bag.RegistrarProfileUrl = person != null
                ? RequestContext.ResolveRockUrl( $"~/Person/{person.IdKey}" )
                : null;
            bag.GroupName = entity.Group?.Name;
            bag.GroupUrl = entity.Group != null
                ? this.GetLinkedPageUrl( AttributeKey.GroupDetailPage, PageParameterKey.GroupId, entity.Group.IdKey )
                : null;
            bag.DiscountPercentageText = entity.DiscountPercentage > 0.0m
                ? entity.DiscountPercentage.ToString( "#,##0.##%" )
                : null;
            bag.DiscountAmountText = entity.DiscountAmount > 0.0m
                ? entity.DiscountAmount.FormatAsCurrency()
                : null;
            bag.ReadOnlyMessage = canEdit
                ? null
                : EditModeMessage.ReadOnlyEditActionNotAllowed( Registration.FriendlyTypeName ).StripHtml();

            bag.IsPaymentPlanActive = entity.IsPaymentPlanActive;
            bag.IsCostLabelVisible = amounts.TotalCost > 0.0m;
            bag.CostText = amounts.DiscountedCost.FormatAsCurrency();
            bag.TotalPaidText = amounts.TotalPaid.FormatAsCurrency();
            bag.BalanceDueText = amounts.BalanceDue.FormatAsCurrency();
            bag.BalanceDueLabelType = GetBalanceDueLabelType( amounts.BalanceDue, bag.IsPaymentPlanActive );

            bag.IsDiscountedAmountColumnVisible = entity.DiscountPercentage > 0.0m;
            bag.FeeRows = GetFeeRows( entity, amounts );
            bag.Payments = GetPaymentRows( entity );
            bag.PaymentPlan = GetPaymentPlanSummary( entity, amounts );

            var canAcceptPayment = amounts.BalanceDue > 0.0m
                && instance.AccountId.HasValue
                && entity.PersonAliasId.HasValue
                && canEdit;

            bag.IsAddManualPaymentVisible = canAcceptPayment;
            bag.IsProcessPaymentVisible = canAcceptPayment && template.FinancialGateway != null;
            bag.IsProcessPaymentDisabled = bag.IsProcessPaymentVisible && !IsHostedPaymentSupported( template.FinancialGateway );
            bag.IsLinkToIndividualMessageVisible = !entity.PersonAliasId.HasValue && amounts.BalanceDue > 0.0m;

            bag.Registrants = GetRegistrantPanels( entity, out var registrantAttributes );
            bag.RegistrantAttributes = registrantAttributes;

            bag.RegistrationTemplateName = template.Name;
            bag.RegistrationTemplateUrl = GetRegistrationTemplateUrl( template );
            bag.RegistrationInstanceName = instance.Name;
            bag.RegistrationInstanceUrl = GetRegistrationInstanceUrl( instance );
            bag.RegistrationName = entity.ToString();

            return bag;
        }

        /// <inheritdoc/>
        protected override RegistrationBag GetEntityBagForEdit( Registration entity )
        {
            if ( entity == null )
            {
                return null;
            }

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( RockContext );
            }

            var bag = GetCommonEntityBag( entity );
            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            bag.DiscountCodeOptions = GetDiscountCodeOptions( entity );
            bag.GroupOptions = GetTargetGroupOptions( entity, out var currentGroup );
            bag.Group = currentGroup;

            return bag;
        }

        /// <summary>
        /// Gets the bag properties that are shared by the view and edit bags.
        /// </summary>
        /// <param name="entity">The registration.</param>
        /// <returns>The bag.</returns>
        private RegistrationBag GetCommonEntityBag( Registration entity )
        {
            var person = entity.PersonAlias?.Person;
            var campus = entity.CampusId.HasValue ? CampusCache.Get( entity.CampusId.Value ) : null;

            return new RegistrationBag
            {
                IdKey = entity.IdKey,
                PersonAlias = person != null
                    ? new ListItemBag { Value = entity.PersonAlias.Guid.ToString(), Text = person.FullName }
                    : null,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                ConfirmationEmail = entity.ConfirmationEmail,
                Campus = campus != null
                    ? new ListItemBag { Value = campus.Guid.ToString(), Text = campus.Name }
                    : null,
                DiscountCode = entity.DiscountCode,
                DiscountPercentage = entity.DiscountPercentage != 0.0m ? entity.DiscountPercentage * 100.0m : ( decimal? ) null,
                DiscountAmount = entity.DiscountAmount != 0.0m ? entity.DiscountAmount : ( decimal? ) null
            };
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( Registration entity, ValidPropertiesBox<RegistrationBag> box )
        {
            return TryUpdateEntityFromBox( entity, box, new History.HistoryChangeList(), out _, out _ );
        }

        /// <summary>
        /// Updates the registration from the edit box, validating every posted
        /// value and recording history for the changes. Only the properties on
        /// the edit form are read; anything else in the box is ignored.
        /// </summary>
        /// <param name="entity">The registration to update.</param>
        /// <param name="box">The box that contains the edited values.</param>
        /// <param name="changes">The history changes to add to.</param>
        /// <param name="isGroupChanged">Set to <c>true</c> when the target group changed.</param>
        /// <param name="errorMessage">Set to the validation error when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the registration was updated.</returns>
        private bool TryUpdateEntityFromBox( Registration entity, ValidPropertiesBox<RegistrationBag> box, History.HistoryChangeList changes, out bool isGroupChanged, out string errorMessage )
        {
            isGroupChanged = false;
            errorMessage = null;

            if ( box?.Bag == null || box.ValidProperties == null )
            {
                errorMessage = "Invalid data.";
                return false;
            }

            var bag = box.Bag;

            if ( box.IsValidProperty( nameof( bag.PersonAlias ) ) && !TryUpdateRegistrar( entity, bag.PersonAlias, changes, out errorMessage ) )
            {
                return false;
            }

            if ( box.IsValidProperty( nameof( bag.FirstName ) ) )
            {
                if ( bag.FirstName.IsNullOrWhiteSpace() )
                {
                    errorMessage = "Registered by First Name is required.";
                    return false;
                }

                var firstName = bag.FirstName.Trim();
                History.EvaluateChange( changes, "First Name", entity.FirstName, firstName );
                entity.FirstName = firstName;
            }

            if ( box.IsValidProperty( nameof( bag.LastName ) ) )
            {
                if ( bag.LastName.IsNullOrWhiteSpace() )
                {
                    errorMessage = "Registered by Last Name is required.";
                    return false;
                }

                var lastName = bag.LastName.Trim();
                History.EvaluateChange( changes, "Last Name", entity.LastName, lastName );
                entity.LastName = lastName;
            }

            if ( box.IsValidProperty( nameof( bag.ConfirmationEmail ) ) )
            {
                var email = bag.ConfirmationEmail?.Trim() ?? string.Empty;

                if ( email.IsNotNullOrWhiteSpace() && !email.IsValidEmail() )
                {
                    errorMessage = "Confirmation Email is not a valid email address.";
                    return false;
                }

                History.EvaluateChange( changes, "Confirmation Email", entity.ConfirmationEmail, email );
                entity.ConfirmationEmail = email;
            }

            if ( box.IsValidProperty( nameof( bag.Campus ) ) && !TryUpdateCampus( entity, bag.Campus, changes, out errorMessage ) )
            {
                return false;
            }

            if ( box.IsValidProperty( nameof( bag.Group ) ) && !TryUpdateTargetGroup( entity, bag.Group, changes, out isGroupChanged, out errorMessage ) )
            {
                return false;
            }

            if ( box.IsValidProperty( nameof( bag.DiscountCode ) ) && !TryUpdateDiscountCode( entity, bag.DiscountCode, changes, out errorMessage ) )
            {
                return false;
            }

            if ( box.IsValidProperty( nameof( bag.DiscountPercentage ) ) )
            {
                var percentage = bag.DiscountPercentage ?? 0.0m;

                if ( percentage < 0.0m || percentage > 100.0m )
                {
                    errorMessage = "Discount Percentage must be between 0 and 100.";
                    return false;
                }

                var discountPercentage = percentage / 100.0m;
                History.EvaluateChange( changes, "Discount Percentage", entity.DiscountPercentage, discountPercentage );
                entity.DiscountPercentage = discountPercentage;
            }

            if ( box.IsValidProperty( nameof( bag.DiscountAmount ) ) )
            {
                var discountAmount = bag.DiscountAmount ?? 0.0m;

                if ( discountAmount < 0.0m )
                {
                    errorMessage = "Discount Amount must not be negative.";
                    return false;
                }

                // Both values are compared as amounts so a missing amount and
                // a zero amount are the same and do not record a change.
                History.EvaluateChange( changes, "Discount Amount", entity.DiscountAmount, discountAmount );
                entity.DiscountAmount = discountAmount;
            }

            if ( box.IsValidProperty( nameof( bag.AttributeValues ) ) )
            {
                entity.LoadAttributes( RockContext );
                entity.SetPublicAttributeValues( bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
            }

            return true;
        }

        /// <summary>
        /// Updates the registrar from the posted person alias.
        /// </summary>
        /// <param name="entity">The registration.</param>
        /// <param name="personAliasBag">The posted person alias, or <c>null</c> to clear the registrar.</param>
        /// <param name="changes">The history changes to add to.</param>
        /// <param name="errorMessage">Set to the validation error when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the registrar was valid.</returns>
        private bool TryUpdateRegistrar( Registration entity, ListItemBag personAliasBag, History.HistoryChangeList changes, out string errorMessage )
        {
            errorMessage = null;

            var personAliasGuid = personAliasBag?.Value.AsGuidOrNull();
            PersonAlias personAlias = null;

            if ( personAliasGuid.HasValue )
            {
                personAlias = new PersonAliasService( RockContext )
                    .Queryable()
                    .Include( pa => pa.Person )
                    .FirstOrDefault( pa => pa.Guid == personAliasGuid.Value );

                if ( personAlias == null )
                {
                    errorMessage = "The selected registrar could not be found.";
                    return false;
                }
            }

            if ( entity.PersonAliasId != personAlias?.Id )
            {
                var previousName = entity.PersonAlias?.Person?.FullName ?? string.Empty;
                History.EvaluateChange( changes, "Registrar", previousName, personAlias?.Person?.FullName ?? string.Empty );

                entity.PersonAlias = personAlias;
                entity.PersonAliasId = personAlias?.Id;
            }

            return true;
        }

        /// <summary>
        /// Updates the campus from the posted campus. An inactive campus is only
        /// accepted when it is the campus the registration already has.
        /// </summary>
        /// <param name="entity">The registration.</param>
        /// <param name="campusBag">The posted campus, or <c>null</c> to clear the campus.</param>
        /// <param name="changes">The history changes to add to.</param>
        /// <param name="errorMessage">Set to the validation error when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the campus was valid.</returns>
        private bool TryUpdateCampus( Registration entity, ListItemBag campusBag, History.HistoryChangeList changes, out string errorMessage )
        {
            errorMessage = null;

            var campusGuid = campusBag?.Value.AsGuidOrNull();
            CampusCache campus = null;

            if ( campusGuid.HasValue )
            {
                campus = CampusCache.Get( campusGuid.Value );

                var isAllowed = campus != null && ( campus.IsActive == true || campus.Id == entity.CampusId );

                if ( !isAllowed )
                {
                    errorMessage = "The selected campus is not valid.";
                    return false;
                }
            }

            if ( entity.CampusId != campus?.Id )
            {
                var previousCampusName = entity.CampusId.HasValue ? CampusCache.Get( entity.CampusId.Value )?.Name : null;
                History.EvaluateChange( changes, "Campus", previousCampusName ?? string.Empty, campus?.Name ?? string.Empty );

                entity.CampusId = campus?.Id;
            }

            return true;
        }

        /// <summary>
        /// Updates the target group from the posted group. The group must be a
        /// group linked to the registration instance, or the group the
        /// registration already has (which may no longer be linked).
        /// </summary>
        /// <param name="entity">The registration.</param>
        /// <param name="groupBag">The posted group, or <c>null</c> to clear the target group.</param>
        /// <param name="changes">The history changes to add to.</param>
        /// <param name="isGroupChanged">Set to <c>true</c> when the target group changed.</param>
        /// <param name="errorMessage">Set to the validation error when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the group was valid.</returns>
        private bool TryUpdateTargetGroup( Registration entity, ListItemBag groupBag, History.HistoryChangeList changes, out bool isGroupChanged, out string errorMessage )
        {
            isGroupChanged = false;
            errorMessage = null;

            var groupGuid = groupBag?.Value.AsGuidOrNull();
            var currentGroup = GetGroupSummary( entity.GroupId );
            GroupSummary newGroup = null;

            if ( groupGuid.HasValue )
            {
                if ( currentGroup != null && currentGroup.Guid == groupGuid.Value )
                {
                    newGroup = currentGroup;
                }
                else
                {
                    newGroup = GetLinkedGroups( entity.RegistrationInstanceId )
                        .FirstOrDefault( g => g.Guid == groupGuid.Value );

                    if ( newGroup == null )
                    {
                        errorMessage = "The selected target group is not linked to this registration instance.";
                        return false;
                    }
                }
            }

            if ( entity.GroupId != newGroup?.Id )
            {
                History.EvaluateChange( changes, "Group", currentGroup?.Name ?? string.Empty, newGroup?.Name ?? string.Empty );

                entity.GroupId = newGroup?.Id;
                isGroupChanged = true;
            }

            return true;
        }

        /// <summary>
        /// Updates the discount code from the posted code. A changed code must
        /// be one of the discounts of the registration template.
        /// </summary>
        /// <param name="entity">The registration.</param>
        /// <param name="discountCode">The posted discount code.</param>
        /// <param name="changes">The history changes to add to.</param>
        /// <param name="errorMessage">Set to the validation error when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the discount code was valid.</returns>
        private bool TryUpdateDiscountCode( Registration entity, string discountCode, History.HistoryChangeList changes, out string errorMessage )
        {
            errorMessage = null;

            var code = discountCode?.Trim() ?? string.Empty;

            if ( code.IsNullOrWhiteSpace() && entity.DiscountCode.IsNullOrWhiteSpace() )
            {
                return true;
            }

            if ( code == entity.DiscountCode )
            {
                return true;
            }

            if ( code.IsNotNullOrWhiteSpace() )
            {
                var templateId = GetRegistrationTemplateId( entity );
                var templateCode = new RegistrationTemplateDiscountService( RockContext )
                    .Queryable()
                    .Where( d => d.RegistrationTemplateId == templateId && d.Code == code )
                    .Select( d => d.Code )
                    .FirstOrDefault();

                if ( templateCode == null )
                {
                    errorMessage = "The selected discount code is not valid.";
                    return false;
                }

                code = templateCode;
            }

            History.EvaluateChange( changes, "Discount Code", entity.DiscountCode, code );
            entity.DiscountCode = code;

            return true;
        }

        /// <summary>
        /// Gets the discount codes that can be selected for the registration,
        /// including the current code when it is no longer on the template.
        /// </summary>
        /// <param name="entity">The registration.</param>
        /// <returns>The discount code options.</returns>
        private List<RegistrationDiscountOptionBag> GetDiscountCodeOptions( Registration entity )
        {
            var templateId = GetRegistrationTemplateId( entity );
            var options = new List<RegistrationDiscountOptionBag>();
            var codes = new HashSet<string>();

            var discounts = new RegistrationTemplateDiscountService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( d => d.RegistrationTemplateId == templateId )
                .OrderBy( d => d.Code )
                .ToList();

            foreach ( var discount in discounts )
            {
                if ( !codes.Add( discount.Code ) )
                {
                    continue;
                }

                var discountString = discount.DiscountString;

                options.Add( new RegistrationDiscountOptionBag
                {
                    Code = discount.Code,
                    Text = discountString.IsNullOrWhiteSpace()
                        ? discount.Code
                        : $"{discount.Code} ({System.Net.WebUtility.HtmlDecode( discountString )})",
                    DiscountPercentage = discount.DiscountPercentage * 100.0m,
                    DiscountAmount = discount.DiscountAmount
                } );
            }

            if ( entity.DiscountCode.IsNotNullOrWhiteSpace() && codes.Add( entity.DiscountCode ) )
            {
                options.Add( new RegistrationDiscountOptionBag
                {
                    Code = entity.DiscountCode,
                    Text = entity.DiscountCode
                } );
            }

            return options;
        }

        /// <summary>
        /// Gets the target groups that can be selected for the registration:
        /// the groups linked to its instance plus the current group when it is
        /// not one of them, so saving never silently clears it.
        /// </summary>
        /// <param name="entity">The registration.</param>
        /// <param name="currentGroup">Set to the current target group, or <c>null</c> when there is none.</param>
        /// <returns>The target group options.</returns>
        private List<ListItemBag> GetTargetGroupOptions( Registration entity, out ListItemBag currentGroup )
        {
            var options = GetLinkedGroups( entity.RegistrationInstanceId )
                .Select( g => new ListItemBag { Value = g.Guid.ToString(), Text = g.Name } )
                .ToList();

            var group = GetGroupSummary( entity.GroupId );
            currentGroup = group != null
                ? new ListItemBag { Value = group.Guid.ToString(), Text = group.Name }
                : null;

            if ( currentGroup != null && !options.Any( o => o.Value == currentGroup.Value ) )
            {
                options.Insert( 0, currentGroup );
            }

            return options;
        }

        /// <summary>
        /// Gets the groups linked to a registration instance that are not archived.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <returns>The linked groups ordered by name.</returns>
        private List<GroupSummary> GetLinkedGroups( int registrationInstanceId )
        {
            return new EventItemOccurrenceGroupMapService( RockContext )
                .Queryable()
                .Where( l => l.RegistrationInstanceId == registrationInstanceId
                    && l.GroupId.HasValue
                    && !l.Group.IsArchived )
                .Select( l => new
                {
                    l.Group.Id,
                    l.Group.Guid,
                    l.Group.Name
                } )
                .Distinct()
                .OrderBy( g => g.Name )
                .ToList()
                .Select( g => new GroupSummary
                {
                    Id = g.Id,
                    Guid = g.Guid,
                    Name = g.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the identifier, unique identifier and name of a group, even when
        /// the group is archived.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <returns>The group summary, or <c>null</c> when there is no group.</returns>
        private GroupSummary GetGroupSummary( int? groupId )
        {
            if ( !groupId.HasValue )
            {
                return null;
            }

            return new GroupService( RockContext )
                .AsNoFilter()
                .Where( g => g.Id == groupId.Value )
                .Select( g => new
                {
                    g.Id,
                    g.Guid,
                    g.Name
                } )
                .ToList()
                .Select( g => new GroupSummary
                {
                    Id = g.Id,
                    Guid = g.Guid,
                    Name = g.Name
                } )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the registration template identifier of a registration without
        /// loading the instance when it is already loaded.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <returns>The registration template identifier.</returns>
        private int GetRegistrationTemplateId( Registration registration )
        {
            return registration.RegistrationInstance?.RegistrationTemplateId
                ?? new RegistrationInstanceService( RockContext ).GetSelect( registration.RegistrationInstanceId, i => i.RegistrationTemplateId );
        }

        /// <summary>
        /// Gets the URL of the registration instance page.
        /// </summary>
        /// <param name="instance">The registration instance.</param>
        /// <returns>The URL, or <c>null</c> when there is no parent page.</returns>
        private string GetRegistrationInstanceUrl( RegistrationInstance instance )
        {
            var instancePage = PageCache.ParentPage;

            if ( instancePage == null )
            {
                return null;
            }

            return new Rock.Web.PageReference( instancePage.Id, 0, new Dictionary<string, string>
            {
                [PageParameterKey.RegistrationInstanceId] = instance.IdKey
            } ).BuildUrl();
        }

        /// <summary>
        /// Gets the URL of the registration template page.
        /// </summary>
        /// <param name="template">The registration template.</param>
        /// <returns>The URL, or <c>null</c> when there is no grandparent page.</returns>
        private string GetRegistrationTemplateUrl( RegistrationTemplate template )
        {
            var templatePage = PageCache.ParentPage?.ParentPage;

            if ( templatePage == null )
            {
                return null;
            }

            return new Rock.Web.PageReference( templatePage.Id, 0, new Dictionary<string, string>
            {
                [PageParameterKey.RegistrationTemplateId] = template.IdKey
            } ).BuildUrl();
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the edit bag of the registration.
        /// </summary>
        /// <param name="key">The identifier key of the registration.</param>
        /// <returns>A box that contains the edit bag and the properties that can be edited.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var registration = GetHydratedRegistration( entity.Id );
            registration.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( registration );

            return ActionOk( new ValidPropertiesBox<RegistrationBag>
            {
                Bag = bag,
                ValidProperties = new List<string>
                {
                    nameof( bag.PersonAlias ),
                    nameof( bag.FirstName ),
                    nameof( bag.LastName ),
                    nameof( bag.ConfirmationEmail ),
                    nameof( bag.Campus ),
                    nameof( bag.Group ),
                    nameof( bag.DiscountCode ),
                    nameof( bag.DiscountPercentage ),
                    nameof( bag.DiscountAmount ),
                    nameof( bag.AttributeValues )
                }
            } );
        }

        /// <summary>
        /// Saves the edited registration. When the target group changes, every
        /// registrant not yet in a group is added to the new group.
        /// </summary>
        /// <param name="box">The box that contains the edited values.</param>
        /// <returns>The rebuilt view bag of the registration.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<RegistrationBag> box )
        {
            if ( !IsPostRequest() )
            {
                return ActionBadRequest( InvalidRequestMethodMessage );
            }

            if ( box?.Bag == null )
            {
                return ActionBadRequest( "Invalid data." );
            }

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var changes = new History.HistoryChangeList();

            if ( !TryUpdateEntityFromBox( entity, box, changes, out var isGroupChanged, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage ?? "Invalid data." );
            }

            if ( !entity.IsValid )
            {
                return ActionBadRequest( entity.ValidationResults.Select( r => r.ErrorMessage ).JoinStrings( ", " ) );
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();

                HistoryService.SaveChanges(
                    RockContext,
                    typeof( Registration ),
                    Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                    entity.Id,
                    changes );

                entity.SaveAttributeValues( RockContext );
            } );

            if ( isGroupChanged && entity.GroupId.HasValue )
            {
                var registration = GetHydratedRegistration( entity.Id );
                AddRegistrantsToGroup( registration, registration.Registrants.Where( r => !r.GroupMemberId.HasValue ).ToList() );
            }

            ClearRegistrationMemos();

            return ActionOk( new RegistrationActionResultBag
            {
                Registration = GetViewBag( entity.Id )
            } );
        }

        /// <summary>
        /// Deletes the registration. An active payment plan is deactivated first.
        /// </summary>
        /// <param name="key">The identifier key of the registration.</param>
        /// <returns>The URL to navigate to and any warning raised while deactivating the payment plan.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            if ( !IsPostRequest() )
            {
                return ActionBadRequest( InvalidRequestMethodMessage );
            }

            if ( IsBlankKey( key ) )
            {
                return ActionNotFound( $"The {Registration.FriendlyTypeName} was not found." );
            }

            var registrationService = new RegistrationService( RockContext );
            var registration = registrationService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( registration == null )
            {
                return ActionNotFound( $"The {Registration.FriendlyTypeName} was not found." );
            }

            if ( !CanEdit( registration ) )
            {
                return ActionForbidden( "You are not authorized to delete this registration." );
            }

            // Remove any expired sessions first so an abandoned, timed-out session cannot block the delete.
            RegistrationSessionService.RemoveExpiredSessionsForRegistration( registration.Id );

            if ( !registrationService.CanDelete( registration, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            if ( !registrationService.TryCancelPaymentPlan( registration, new FinancialScheduledTransactionService( RockContext ), out var cancelError, out var cancelWarning ) )
            {
                return ActionBadRequest( cancelError ?? "An unknown error occurred while deactivating a payment plan. The registration was not deleted." );
            }

            /*
                10/6/2026 - MSE

                A successful cancel (with or without a warning) means the gateway
                has stopped the payment plan (or the plan record no longer exists),
                so the delete continues instead of aborting. A warning usually means
                the plan was canceled on the gateway but not marked inactive in Rock,
                so the plan is marked inactive to match the gateway.

                The deactivation is saved immediately, before and outside of the
                delete's transaction, so a later failure can never roll back a cancel
                the gateway already confirmed. A retry then sees an inactive plan and
                skips the cancel. A failed cancel returns above before anything is
                saved, which keeps the in-memory InactivateDateTime set by the cancel
                attempt out of the database.

                Reason: Never lose or roll back a gateway-confirmed payment plan cancel.
            */
            if ( cancelWarning.IsNotNullOrWhiteSpace() && registration.PaymentPlanFinancialScheduledTransaction != null )
            {
                registration.PaymentPlanFinancialScheduledTransaction.IsActive = false;
            }

            RockContext.SaveChanges();

            var redirectUrl = GetSafeReturnUrl() ?? GetRegistrationParentPageUrl( registration );

            var changes = new History.HistoryChangeList();
            changes.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Registration" );

            RockContext.WrapTransaction( () =>
            {
                HistoryService.SaveChanges(
                    RockContext,
                    typeof( Registration ),
                    Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                    registration.Id,
                    changes );

                registrationService.Delete( registration );
                RockContext.SaveChanges();
            } );

            return ActionOk( new RegistrationDeleteResultBag
            {
                RedirectUrl = redirectUrl,
                WarningMessage = cancelWarning.IsNotNullOrWhiteSpace() ? cancelWarning : null
            } );
        }

        /// <summary>
        /// Gets the contact details of a person selected as the registrar so
        /// the edit form can fill in the name and confirmation email.
        /// </summary>
        /// <param name="key">The identifier key of the registration.</param>
        /// <param name="personAliasGuid">The unique identifier of the selected person alias.</param>
        /// <returns>The contact details of the person.</returns>
        [BlockAction]
        public BlockActionResult GetRegistrarDetails( string key, Guid personAliasGuid )
        {
            if ( !TryGetEntityForEditAction( key, out _, out var actionError ) )
            {
                return actionError;
            }

            var details = new PersonAliasService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( pa => pa.Guid == personAliasGuid )
                .Select( pa => new RegistrarDetailsBag
                {
                    Email = pa.Person.Email,
                    FirstName = pa.Person.NickName,
                    LastName = pa.Person.LastName
                } )
                .FirstOrDefault();

            if ( details == null )
            {
                return ActionNotFound( "The selected person could not be found." );
            }

            return ActionOk( details );
        }

        /// <summary>
        /// Sends the registration confirmation email again.
        /// </summary>
        /// <param name="key">The identifier key of the registration.</param>
        /// <returns>A message describing the result.</returns>
        [BlockAction]
        public BlockActionResult ResendConfirmation( string key )
        {
            if ( !IsPostRequest() )
            {
                return ActionBadRequest( InvalidRequestMethodMessage );
            }

            if ( !TryGetEntityForEditAction( key, out var registration, out var actionError ) )
            {
                return actionError;
            }

            if ( registration.ConfirmationEmail.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "This registration does not have a confirmation email." );
            }

            new ProcessSendRegistrationConfirmation.Message
            {
                RegistrationId = registration.Id,
                AppRoot = RequestContext.RootUrlPath.EnsureTrailingForwardslash(),
                ThemeRoot = RequestContext.RootUrlPath + RequestContext.ResolveRockUrl( "~~/" )
            }.Send();

            var changes = new History.HistoryChangeList();
            changes.AddChange( History.HistoryVerb.Sent, History.HistoryChangeType.Record, "Confirmation" ).SetRelatedData( "Resent", null, null );

            HistoryService.SaveChanges(
                RockContext,
                typeof( Registration ),
                Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                registration.Id,
                changes );

            return ActionOk( new RegistrationActionResultBag
            {
                SuccessMessage = "A new confirmation email has been sent."
            } );
        }

        /// <summary>
        /// Deletes a registrant of the registration.
        /// </summary>
        /// <param name="key">The identifier key of the registration.</param>
        /// <param name="registrantKey">The identifier key of the registrant.</param>
        /// <returns>The rebuilt view bag of the registration.</returns>
        [BlockAction]
        public BlockActionResult DeleteRegistrant( string key, string registrantKey )
        {
            if ( !IsPostRequest() )
            {
                return ActionBadRequest( InvalidRequestMethodMessage );
            }

            if ( !TryGetEntityForEditAction( key, out var registration, out var actionError ) )
            {
                return actionError;
            }

            if ( registrantKey.IsNullOrWhiteSpace() )
            {
                return ActionNotFound( "The registrant could not be found." );
            }

            var registrantService = new RegistrationRegistrantService( RockContext );
            var registrant = registrantService
                .GetQueryableByKey( registrantKey, false )
                .Include( r => r.PersonAlias.Person )
                .FirstOrDefault( r => r.RegistrationId == registration.Id );

            if ( registrant == null )
            {
                return ActionNotFound( "The registrant could not be found." );
            }

            if ( !registrantService.CanDelete( registrant, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            var changes = new History.HistoryChangeList();
            changes.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Registrant" )
                .SetOldValue( registrant.PersonAlias?.Person?.FullName ?? "Registrant" );

            RockContext.WrapTransaction( () =>
            {
                HistoryService.SaveChanges(
                    RockContext,
                    typeof( Registration ),
                    Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                    registration.Id,
                    changes );

                registrantService.Delete( registrant );
                RockContext.SaveChanges();
            } );

            ClearRegistrationMemos();

            return ActionOk( new RegistrationActionResultBag
            {
                Registration = GetViewBag( registration.Id )
            } );
        }

        /// <summary>
        /// Adds a registrant to the target group of the registration.
        /// </summary>
        /// <param name="key">The identifier key of the registration.</param>
        /// <param name="registrantKey">The identifier key of the registrant.</param>
        /// <returns>The rebuilt view bag of the registration.</returns>
        [BlockAction]
        public BlockActionResult AddRegistrantToGroup( string key, string registrantKey )
        {
            if ( !IsPostRequest() )
            {
                return ActionBadRequest( InvalidRequestMethodMessage );
            }

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var registration = GetHydratedRegistration( entity.Id );
            var registrant = FindRegistrant( registration, registrantKey );

            if ( registrant == null )
            {
                return ActionNotFound( "The registrant could not be found." );
            }

            /*
                10/6/2026 - MSE

                Adding a single registrant to the group does not write person notes.
                The notes helper skips every registrant already on the registration,
                so for an existing registration it never wrote anything and only cost
                extra queries.

                Reason: Remove a call that could never have an effect.
            */
            AddRegistrantsToGroup( registration, new List<RegistrationRegistrant> { registrant } );

            ClearRegistrationMemos();

            return ActionOk( new RegistrationActionResultBag
            {
                Registration = GetViewBag( registration.Id )
            } );
        }

        /// <summary>
        /// Gets the values needed to show the move registration dialog.
        /// </summary>
        /// <param name="key">The identifier key of the registration.</param>
        /// <param name="isShowAllInstances"><c>true</c> to include inactive and ended registration instances.</param>
        /// <returns>The move registration options.</returns>
        [BlockAction]
        public BlockActionResult GetMoveOptions( string key, bool isShowAllInstances )
        {
            if ( !TryGetEntityForEditAction( key, out var registration, out var actionError ) )
            {
                return actionError;
            }

            var currentInstance = registration.RegistrationInstance;
            var now = RockDateTime.Now;

            var query = new RegistrationInstanceService( RockContext )
                .Queryable()
                .Include( i => i.RegistrationTemplate )
                .Where( i => i.RegistrationTemplateId == currentInstance.RegistrationTemplateId
                    && i.Id != currentInstance.Id );

            if ( !isShowAllInstances )
            {
                query = query.Where( i => i.IsActive && ( i.EndDateTime >= now || i.EndDateTime == null ) );
            }

            var instances = query
                .OrderByDescending( i => i.StartDateTime )
                .ThenBy( i => i.Name )
                .ToList()
                .Where( i => IsMoveTargetAuthorized( i ) )
                .Select( i => new ListItemBag
                {
                    Value = i.IdKey,
                    Text = i.Name
                } )
                .ToList();

            return ActionOk( new MoveRegistrationOptionsBag
            {
                CurrentRegistrationInstanceName = currentInstance.Name,
                RegistrationInstances = instances
            } );
        }

        /// <summary>
        /// Gets the groups the registrants can be moved to for a registration instance.
        /// </summary>
        /// <param name="key">The identifier key of the registration.</param>
        /// <param name="registrationInstanceKey">The identifier key of the registration instance being moved to.</param>
        /// <returns>The groups linked to the registration instance.</returns>
        [BlockAction]
        public BlockActionResult GetMoveGroups( string key, string registrationInstanceKey )
        {
            if ( !TryGetEntityForEditAction( key, out var registration, out var actionError ) )
            {
                return actionError;
            }

            var targetInstance = registrationInstanceKey.IsNotNullOrWhiteSpace()
                ? new RegistrationInstanceService( RockContext ).Get( registrationInstanceKey, false )
                : null;

            if ( !IsValidMoveTarget( registration, targetInstance, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            var groups = GetLinkedGroups( targetInstance.Id )
                .Select( g => new ListItemBag
                {
                    Value = IdHasher.Instance.GetHash( g.Id ),
                    Text = g.Name
                } )
                .ToList();

            return ActionOk( groups );
        }

        /// <summary>
        /// Moves the registration to another registration instance of the same
        /// template, optionally moving the registrants into a group of that instance.
        /// </summary>
        /// <param name="key">The identifier key of the registration.</param>
        /// <param name="request">The values that describe the move.</param>
        /// <returns>The rebuilt view bag of the registration.</returns>
        [BlockAction]
        public BlockActionResult MoveRegistration( string key, MoveRegistrationRequestBag request )
        {
            if ( !IsPostRequest() )
            {
                return ActionBadRequest( InvalidRequestMethodMessage );
            }

            if ( request == null )
            {
                return ActionBadRequest( "Invalid data." );
            }

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var registration = GetHydratedRegistration( entity.Id );
            var currentInstance = registration.RegistrationInstance;
            var template = currentInstance.RegistrationTemplate;

            var targetInstance = request.RegistrationInstanceKey.IsNotNullOrWhiteSpace()
                ? new RegistrationInstanceService( RockContext ).Get( request.RegistrationInstanceKey, false )
                : null;

            if ( !IsValidMoveTarget( registration, targetInstance, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            Group targetGroup = null;
            int? groupRoleId = null;

            if ( request.GroupKey.IsNotNullOrWhiteSpace() )
            {
                targetGroup = new GroupService( RockContext ).Get( request.GroupKey, false );

                if ( targetGroup == null || !GetLinkedGroups( targetInstance.Id ).Any( g => g.Id == targetGroup.Id ) )
                {
                    return ActionBadRequest( "The selected group is not linked to the selected registration instance." );
                }

                groupRoleId = GetMoveGroupRoleId( targetGroup, template );

                if ( !groupRoleId.HasValue )
                {
                    return ActionBadRequest( "The group type has no roles." );
                }
            }

            var historyRecord = new History
            {
                EntityTypeId = EntityTypeCache.Get<Registration>().Id,
                EntityId = registration.Id,
                Verb = "MOVED",
                ValueName = "Registration Instance",
                ChangeType = "Moved",
                OldValue = currentInstance.Name,
                OldRawValue = currentInstance.Id.ToString(),
                NewValue = targetInstance.Name,
                NewRawValue = targetInstance.Id.ToString(),
                Caption = GetHistoryCaption( request.Comment ),
                CategoryId = CategoryCache.Get( Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION ).Id
            };

            RockContext.WrapTransaction( () =>
            {
                new HistoryService( RockContext ).Add( historyRecord );

                registration.RegistrationInstanceId = targetInstance.Id;
                registration.RegistrationInstance = targetInstance;

                if ( targetGroup != null )
                {
                    registration.GroupId = targetGroup.Id;
                    MoveRegistrantsToGroup( registration, targetGroup, groupRoleId.Value, template );
                }
                else
                {
                    RockContext.SaveChanges();
                }
            } );

            ClearRegistrationMemos();

            return ActionOk( new RegistrationActionResultBag
            {
                Registration = GetViewBag( registration.Id )
            } );
        }

        /// <summary>
        /// Records a manual payment for the registration.
        /// </summary>
        /// <param name="key">The identifier key of the registration.</param>
        /// <param name="request">The values that describe the payment.</param>
        /// <returns>The rebuilt view bag of the registration.</returns>
        [BlockAction]
        public BlockActionResult SaveManualPayment( string key, ManualPaymentRequestBag request )
        {
            if ( !IsPostRequest() )
            {
                return ActionBadRequest( InvalidRequestMethodMessage );
            }

            if ( request == null )
            {
                return ActionBadRequest( "Invalid data." );
            }

            /*
                10/6/2026 - MSE

                Manual payments are allowed for anyone who can edit the registration,
                including people who only have the Register permission on the
                template. The batch is chosen by the server, never by the client, so
                no additional finance permission is required. A new security action
                would deny everyone by default and lock event staff out.

                Reason: Keep manual payments available to the people who manage registrations.
            */
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var registration = GetHydratedRegistration( entity.Id );

            if ( !TryValidatePayment( registration, request.Amount, request.PayerPersonAliasGuid, request.Comments, out var payerPersonAliasId, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            if ( !TryGetManualPaymentDetail( request, out var paymentDetail, out errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            var transactionCode = request.TransactionCode?.Trim();

            if ( transactionCode?.Length > 50 )
            {
                return ActionBadRequest( "The transaction code must be 50 characters or fewer." );
            }

            var transaction = new FinancialTransaction
            {
                TransactionCode = transactionCode,
                FinancialPaymentDetail = paymentDetail
            };

            var options = GetPaymentTransactionOptions( registration, request.Amount, payerPersonAliasId, request.Comments );

            var changes = new History.HistoryChangeList();
            changes.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, $"Manual payment of {request.Amount.FormatAsCurrency()}." );

            try
            {
                // History is saved first: the payment helper queues the batch
                // history on a background task, so it must be the last step.
                RockContext.WrapTransaction( () =>
                {
                    SaveRegistrationHistory( registration.Id, changes );
                    new RegistrationService( RockContext ).SaveRegistrationPaymentTransaction( registration, transaction, options );
                } );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                return ActionBadRequest( "There was a problem saving the payment." );
            }

            ClearRegistrationMemos();

            return ActionOk( new RegistrationActionResultBag
            {
                Registration = GetViewBag( registration.Id )
            } );
        }

        /// <summary>
        /// Gets the values needed to show the form used to charge a new payment
        /// through the gateway of the registration template.
        /// </summary>
        /// <param name="key">The identifier key of the registration.</param>
        /// <returns>The process payment options.</returns>
        [BlockAction]
        public BlockActionResult GetProcessPaymentOptions( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var registration = GetHydratedRegistration( entity.Id );
            var gateway = registration.RegistrationInstance.RegistrationTemplate.FinancialGateway;

            if ( gateway == null )
            {
                return ActionBadRequest( "A Financial Gateway must be set on the Registration Template." );
            }

            if ( !IsHostedPaymentSupported( gateway ) )
            {
                return ActionBadRequest( "There was a problem creating the payment gateway information" );
            }

            var registrar = registration.PersonAlias?.Person;

            if ( registrar == null )
            {
                return ActionBadRequest( LinkToIndividualMessage );
            }

            var amounts = GetRegistrationAmounts( registration );

            if ( amounts.BalanceDue <= 0.0m )
            {
                return ActionBadRequest( "This registration does not have a balance due." );
            }

            var obsidianComponent = ( IObsidianHostedGatewayComponent ) gateway.GetGatewayComponent();

            return ActionOk( new ProcessPaymentOptionsBag
            {
                GatewayControl = new GatewayControlBag
                {
                    FileUrl = obsidianComponent.GetObsidianControlFileUrl( gateway ),
                    Settings = obsidianComponent.GetObsidianControlSettings( gateway, new HostedPaymentInfoControlOptions
                    {
                        EnableACH = false,
                        EnableCreditCard = true,
                        EnableBillingAddressCollection = false
                    } )
                },
                Amount = amounts.BalanceDue,
                Payer = new ListItemBag
                {
                    Value = registration.PersonAlias.Guid.ToString(),
                    Text = registrar.FullName
                },
                BillingAddress = GetHomeAddress( registrar )
            } );
        }

        /// <summary>
        /// Charges a new payment through the gateway of the registration template
        /// and records it.
        /// </summary>
        /// <param name="key">The identifier key of the registration.</param>
        /// <param name="request">The values that describe the payment.</param>
        /// <returns>The rebuilt view bag of the registration.</returns>
        [BlockAction]
        public BlockActionResult ProcessPayment( string key, ProcessPaymentRequestBag request )
        {
            if ( !IsPostRequest() )
            {
                return ActionBadRequest( InvalidRequestMethodMessage );
            }

            if ( request == null || request.TransactionGuid == Guid.Empty )
            {
                return ActionBadRequest( "Invalid data." );
            }

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var registration = GetHydratedRegistration( entity.Id );
            var template = registration.RegistrationInstance.RegistrationTemplate;
            var gateway = template.FinancialGateway;

            if ( gateway == null )
            {
                return ActionBadRequest( "A Financial Gateway must be set on the Registration Template." );
            }

            if ( !IsHostedPaymentSupported( gateway ) )
            {
                return ActionBadRequest( "There was a problem creating the payment gateway information" );
            }

            /*
                10/6/2026 - MSE

                The client creates the transaction Guid when the payment form opens and
                sends the same Guid with every retry of that form. If a transaction with
                this Guid was already recorded for this registration, the earlier attempt
                succeeded (for example the response was lost), so report success without
                charging again. A Guid that belongs to anything else is rejected.

                Reason: Never charge or record the same payment twice.
            */
            var registrationEntityTypeId = EntityTypeCache.Get<Registration>().Id;
            var registrationId = registration.Id;
            var existingTransaction = new FinancialTransactionService( RockContext )
                .Queryable()
                .Where( t => t.Guid == request.TransactionGuid )
                .Select( t => new
                {
                    IsForRegistration = t.TransactionDetails.Any( d => d.EntityTypeId == registrationEntityTypeId && d.EntityId == registrationId )
                } )
                .FirstOrDefault();

            if ( existingTransaction != null )
            {
                if ( !existingTransaction.IsForRegistration )
                {
                    return ActionBadRequest( "There was a problem processing the payment." );
                }

                return ActionOk( new RegistrationActionResultBag
                {
                    Registration = GetViewBag( registration.Id )
                } );
            }

            if ( !TryValidatePayment( registration, request.Amount, request.PayerPersonAliasGuid, request.Comments, out var payerPersonAliasId, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            if ( request.GatewayToken.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "There was a problem with the payment information." );
            }

            if ( request.BillingAddress == null || request.BillingAddress.Street1.IsNullOrWhiteSpace() || request.BillingAddress.City.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "A billing address is required." );
            }

            var component = gateway.GetGatewayComponent();
            var paymentInfo = GetHostedPaymentInfo( registration, request );

            /*
                10/6/2026 - MSE

                The gateway calls run before, and outside of, any database transaction
                so a slow gateway never holds database locks and a later database
                failure can never hide a successful charge.

                Reason: Keep gateway calls out of database transactions.
            */
            FinancialTransaction transaction;

            try
            {
                var customerToken = ( ( IObsidianHostedGatewayComponent ) component ).CreateCustomerAccount( gateway, paymentInfo, out var customerError );

                if ( customerError.IsNotNullOrWhiteSpace() || customerToken.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( customerError.IsNotNullOrWhiteSpace() ? customerError : "Unknown Error" );
                }

                paymentInfo.GatewayPersonIdentifier = customerToken;

                transaction = component.Charge( gateway, paymentInfo, out var chargeError );

                if ( transaction == null )
                {
                    return ActionBadRequest( chargeError.IsNotNullOrWhiteSpace() ? chargeError : "There was a problem with the payment" );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                return ActionBadRequest( "There was a problem processing the payment." );
            }

            transaction.Guid = request.TransactionGuid;

            var options = GetPaymentTransactionOptions( registration, request.Amount, payerPersonAliasId, request.Comments );
            options.PaymentInfo = paymentInfo;
            options.GatewayComponent = component;
            options.FinancialGatewayId = template.FinancialGatewayId;

            var changes = new History.HistoryChangeList();
            changes.AddChange( History.HistoryVerb.Process, History.HistoryChangeType.Record, $"Payment of {request.Amount.FormatAsCurrency()}." );

            try
            {
                // History is saved first: the payment helper queues the batch
                // history on a background task, so it must be the last step.
                RockContext.WrapTransaction( () =>
                {
                    SaveRegistrationHistory( registration.Id, changes );
                    new RegistrationService( RockContext ).SaveRegistrationPaymentTransaction( registration, transaction, options );
                } );
            }
            catch ( Exception ex )
            {
                /*
                    10/6/2026 - MSE

                    The card was charged but the payment could not be recorded. The
                    exception is logged with the gateway transaction code so finance can
                    reconcile it, and a Conflict status tells the client the payment must
                    not be retried (a retry would charge the card again).

                    Reason: Never invite a second charge after a failed save.
                */
                ExceptionLogService.LogException( new Exception( $"A registration payment was charged by the gateway (transaction code {transaction.TransactionCode}) for registration {registration.Id} but could not be recorded.", ex ) );

                return ActionConflict( $"The payment was charged (transaction code {transaction.TransactionCode}) but could not be recorded. Do not resubmit; contact your finance team." );
            }

            ClearRegistrationMemos();

            return ActionOk( new RegistrationActionResultBag
            {
                Registration = GetViewBag( registration.Id )
            } );
        }

        /// <summary>
        /// Calculates the payment plan configuration used by the change payment
        /// plan dialog, either from the existing plan or from the dialog values.
        /// </summary>
        /// <param name="key">The identifier key of the registration.</param>
        /// <param name="request">The dialog values.</param>
        /// <returns>The calculated payment plan configuration.</returns>
        [BlockAction]
        public BlockActionResult GetPaymentPlanConfiguration( string key, PaymentPlanConfigurationRequestBag request )
        {
            if ( !IsPostRequest() )
            {
                return ActionBadRequest( InvalidRequestMethodMessage );
            }

            if ( request == null )
            {
                return ActionBadRequest( "Invalid data." );
            }

            if ( !TryGetPaymentPlanRegistration( key, out var registration, out var actionError ) )
            {
                return actionError;
            }

            if ( !TryGetPaymentPlanConfiguration( registration, request, out var configuration, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            return ActionOk( GetPaymentPlanConfigurationBag( registration, configuration ) );
        }

        /// <summary>
        /// Updates the payment plan of the registration on the gateway and in Rock.
        /// </summary>
        /// <param name="key">The identifier key of the registration.</param>
        /// <param name="request">The dialog values.</param>
        /// <returns>The rebuilt view bag, or a warning when the values are not valid.</returns>
        [BlockAction]
        public BlockActionResult UpdatePaymentPlan( string key, PaymentPlanConfigurationRequestBag request )
        {
            if ( !IsPostRequest() )
            {
                return ActionBadRequest( InvalidRequestMethodMessage );
            }

            if ( request == null )
            {
                return ActionBadRequest( "Invalid data." );
            }

            if ( !TryGetPaymentPlanRegistration( key, out var registration, out var actionError ) )
            {
                return actionError;
            }

            var amounts = GetRegistrationAmounts( registration );
            var nextPaymentDate = GetNextPaymentDate( registration );

            if ( !nextPaymentDate.HasValue )
            {
                return ActionBadRequest( "The plan cannot be changed because it has no next payment." );
            }

            if ( nextPaymentDate.Value.Date == RockDateTime.Today )
            {
                return ActionBadRequest( "The plan cannot be changed because the next payment is today (and may be in process)." );
            }

            if ( amounts.BalanceDue <= 0.0m )
            {
                return ActionBadRequest( "The plan cannot be changed because the amount remaining is zero." );
            }

            request.IsInitial = false;

            if ( !TryGetPaymentPlanConfiguration( registration, request, out var configuration, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            if ( !configuration.IsValidForUpdatingExistingPaymentPlan( out var validationError ) )
            {
                return ActionOk( new RegistrationActionResultBag
                {
                    WarningMessage = validationError.ToStringOrDefault( "Unknown validation error" )
                } );
            }

            if ( !TryUpdatePaymentPlan( registration, configuration, out errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            ClearRegistrationMemos();

            return ActionOk( new RegistrationActionResultBag
            {
                Registration = GetViewBag( registration.Id )
            } );
        }

        /// <summary>
        /// Deactivates the payment plan of the registration.
        /// </summary>
        /// <param name="key">The identifier key of the registration.</param>
        /// <returns>The rebuilt view bag and any warning raised while deactivating the plan.</returns>
        [BlockAction]
        public BlockActionResult DeletePaymentPlan( string key )
        {
            if ( !IsPostRequest() )
            {
                return ActionBadRequest( InvalidRequestMethodMessage );
            }

            if ( !TryGetPaymentPlanRegistration( key, out var registration, out var actionError ) )
            {
                return actionError;
            }

            var paymentPlan = registration.PaymentPlanFinancialScheduledTransaction;

            if ( !GetNextPaymentDate( registration ).HasValue )
            {
                return ActionBadRequest( "The plan cannot be deleted because it has no next payment." );
            }

            if ( !new RegistrationService( RockContext ).TryCancelPaymentPlan( registration, new FinancialScheduledTransactionService( RockContext ), out var error, out var warning ) )
            {
                // Nothing is saved: the cancel attempt leaves unsaved changes on the plan.
                return ActionBadRequest( error ?? "An unknown error occurred while deactivating the payment plan." );
            }

            // The gateway confirmed the cancel, so make Rock match it (see the
            // engineering note in Delete).
            if ( warning.IsNotNullOrWhiteSpace() )
            {
                paymentPlan.IsActive = false;
            }

            var changes = new History.HistoryChangeList();
            changes.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Payment Plan" );

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                SaveRegistrationHistory( registration.Id, changes );
            } );

            ClearRegistrationMemos();

            return ActionOk( new RegistrationActionResultBag
            {
                Registration = GetViewBag( registration.Id ),
                WarningMessage = warning.IsNotNullOrWhiteSpace() ? warning : null
            } );
        }

        #endregion Block Actions

        #region Private Helpers

        /// <summary>
        /// Determines whether the current request is a POST. Every block action
        /// can also be called with a GET request, so state changing actions
        /// reject anything else to block cross-site links.
        /// </summary>
        /// <returns><c>true</c> if the request is a POST.</returns>
        private bool IsPostRequest()
        {
            return RequestContext.HttpMethod == "POST";
        }

        /// <summary>
        /// Determines whether a key can never identify an existing registration.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the key is blank, zero or an empty Guid.</returns>
        private static bool IsBlankKey( string key )
        {
            return key.IsNullOrWhiteSpace()
                || key.Trim() == "0"
                || key.AsGuidOrNull() == Guid.Empty;
        }

        /// <summary>
        /// Determines whether the current person can edit the registration.
        /// Block edit access or Register, Edit or Administrate access on the
        /// registration (which inherits from its instance and template) allows it.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <returns><c>true</c> if the current person can edit the registration.</returns>
        private bool CanEdit( Registration registration )
        {
            if ( registration == null )
            {
                return false;
            }

            if ( _canEditByRegistrationId.TryGetValue( registration.Id, out var canEdit ) )
            {
                return canEdit;
            }

            var currentPerson = RequestContext.CurrentPerson;

            canEdit = BlockCache.IsAuthorized( Authorization.EDIT, currentPerson )
                || registration.IsAuthorized( Authorization.REGISTER, currentPerson )
                || registration.IsAuthorized( Authorization.EDIT, currentPerson )
                || registration.IsAuthorized( Authorization.ADMINISTRATE, currentPerson );

            _canEditByRegistrationId[registration.Id] = canEdit;

            return canEdit;
        }

        /// <summary>
        /// Determines whether the current person can view the registration.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <returns><c>true</c> if the current person can view the registration.</returns>
        private bool CanView( Registration registration )
        {
            return CanEdit( registration )
                || registration.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Determines whether the current person can change or delete payment plans.
        /// </summary>
        /// <returns><c>true</c> if the current person has the Edit Payment Plan action.</returns>
        private bool CanEditPaymentPlan()
        {
            return BlockCache.IsAuthorized( SecurityActionKey.EditPaymentPlan, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Determines whether the current person can move registrations into a
        /// registration instance.
        /// </summary>
        /// <param name="instance">The registration instance.</param>
        /// <returns><c>true</c> if the current person can move registrations into the instance.</returns>
        private bool IsMoveTargetAuthorized( RegistrationInstance instance )
        {
            var currentPerson = RequestContext.CurrentPerson;

            return BlockCache.IsAuthorized( Authorization.EDIT, currentPerson )
                || instance.IsAuthorized( Authorization.REGISTER, currentPerson )
                || instance.IsAuthorized( Authorization.EDIT, currentPerson )
                || instance.IsAuthorized( Authorization.ADMINISTRATE, currentPerson );
        }

        /// <summary>
        /// Determines whether a registration instance is a valid target for
        /// moving the registration.
        /// </summary>
        /// <param name="registration">The registration being moved.</param>
        /// <param name="targetInstance">The registration instance being moved to.</param>
        /// <param name="errorMessage">Set to the reason when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the registration can be moved to the instance.</returns>
        private bool IsValidMoveTarget( Registration registration, RegistrationInstance targetInstance, out string errorMessage )
        {
            errorMessage = null;

            if ( targetInstance == null )
            {
                errorMessage = "The selected registration instance could not be found.";
                return false;
            }

            if ( targetInstance.Id == registration.RegistrationInstanceId
                || targetInstance.RegistrationTemplateId != GetRegistrationTemplateId( registration )
                || !IsMoveTargetAuthorized( targetInstance ) )
            {
                errorMessage = "The registration cannot be moved to the selected registration instance.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the role used for registrants moved into a group: the template
        /// role when the group is of the template group type, otherwise the
        /// default role of the group type, otherwise its first role.
        /// </summary>
        /// <param name="group">The group being moved to.</param>
        /// <param name="template">The registration template.</param>
        /// <returns>The group role identifier, or <c>null</c> when the group type has no roles.</returns>
        private static int? GetMoveGroupRoleId( Group group, RegistrationTemplate template )
        {
            if ( group.GroupTypeId == template.GroupTypeId && template.GroupMemberRoleId.HasValue )
            {
                return template.GroupMemberRoleId.Value;
            }

            var groupType = GroupTypeCache.Get( group.GroupTypeId );

            if ( groupType?.DefaultGroupRoleId.HasValue == true && groupType.DefaultGroupRoleId.Value > 0 )
            {
                return groupType.DefaultGroupRoleId.Value;
            }

            return groupType?.Roles
                .OrderBy( r => r.Order )
                .Select( r => ( int? ) r.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the caption written to the history of a move.
        /// </summary>
        /// <param name="comment">The comment entered in the move dialog.</param>
        /// <returns>The caption, or <c>null</c> when there is no comment.</returns>
        private static string GetHistoryCaption( string comment )
        {
            var caption = comment?.Trim();

            if ( caption.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return caption.Length > MaximumHistoryCaptionLength
                ? caption.Substring( 0, MaximumHistoryCaptionLength )
                : caption;
        }

        /// <summary>
        /// Finds a registrant of a loaded registration by identifier key or unique identifier.
        /// </summary>
        /// <param name="registration">The registration with its registrants loaded.</param>
        /// <param name="registrantKey">The identifier key or unique identifier of the registrant.</param>
        /// <returns>The registrant, or <c>null</c> when it is not part of the registration.</returns>
        private static RegistrationRegistrant FindRegistrant( Registration registration, string registrantKey )
        {
            if ( registrantKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var registrantGuid = registrantKey.AsGuidOrNull();

            if ( registrantGuid.HasValue )
            {
                return registration.Registrants.FirstOrDefault( r => r.Guid == registrantGuid.Value );
            }

            var registrantId = IdHasher.Instance.GetId( registrantKey );

            return registrantId.HasValue
                ? registration.Registrants.FirstOrDefault( r => r.Id == registrantId.Value )
                : null;
        }

        /// <summary>
        /// Adds registrants to the target group of the registration. Each
        /// person is added once; later registrants for the same person are linked
        /// to the same group member. Existing members have their status set to
        /// the status of the template.
        /// </summary>
        /// <param name="registration">The registration with its registrants and template loaded.</param>
        /// <param name="registrants">The registrants to add.</param>
        private void AddRegistrantsToGroup( Registration registration, List<RegistrationRegistrant> registrants )
        {
            var template = registration.RegistrationInstance.RegistrationTemplate;

            if ( !template.GroupTypeId.HasValue || !registration.GroupId.HasValue )
            {
                return;
            }

            var group = new GroupService( RockContext ).Get( registration.GroupId.Value );

            if ( group == null || group.GroupTypeId != template.GroupTypeId.Value )
            {
                return;
            }

            var groupRoleId = template.GroupMemberRoleId ?? GroupTypeCache.Get( group.GroupTypeId )?.DefaultGroupRoleId;

            if ( !groupRoleId.HasValue )
            {
                return;
            }

            var candidates = registrants
                .Where( r => r.PersonAlias != null && !r.GroupMemberId.HasValue )
                .OrderBy( r => r.Id )
                .ToList();

            if ( !candidates.Any() )
            {
                return;
            }

            var personIds = candidates.Select( r => r.PersonAlias.PersonId ).Distinct().ToList();
            var groupMemberService = new GroupMemberService( RockContext );

            var membersByPersonId = groupMemberService
                .Queryable()
                .Where( m => m.GroupId == group.Id
                    && m.GroupRoleId == groupRoleId.Value
                    && personIds.Contains( m.PersonId ) )
                .ToList()
                .GroupBy( m => m.PersonId )
                .ToDictionary( g => g.Key, g => g.First() );

            var registrantChanges = new List<(RegistrationRegistrant Registrant, History.HistoryChangeList Changes)>();

            foreach ( var registrant in candidates )
            {
                var personId = registrant.PersonAlias.PersonId;
                var changes = new History.HistoryChangeList();

                if ( !membersByPersonId.TryGetValue( personId, out var groupMember ) )
                {
                    groupMember = new GroupMember
                    {
                        GroupId = group.Id,
                        PersonId = personId,
                        GroupRoleId = groupRoleId.Value,
                        GroupMemberStatus = template.GroupMemberStatus
                    };

                    groupMemberService.Add( groupMember );
                    membersByPersonId[personId] = groupMember;

                    changes.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, $"Registrant to {group.Name} group" );
                }
                else
                {
                    changes.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Record, $"Registrant to existing person in {group.Name} group" );
                }

                groupMember.GroupMemberStatus = template.GroupMemberStatus;
                registrant.GroupMember = groupMember;

                registrantChanges.Add( (registrant, changes) );
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();

                foreach ( var registrantChange in registrantChanges )
                {
                    HistoryService.SaveChanges(
                        RockContext,
                        typeof( Registration ),
                        Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                        registration.Id,
                        registrantChange.Changes,
                        $"Registrant: {registrantChange.Registrant.PersonAlias.Person?.FullName}",
                        null,
                        null,
                        false );
                }

                RockContext.SaveChanges();
            } );
        }

        /// <summary>
        /// Moves the registrants of the registration into a group, copying the
        /// attribute values of their previous group membership. Must be called
        /// inside a database transaction; it saves the changes.
        /// </summary>
        /// <param name="registration">The registration with its registrants loaded.</param>
        /// <param name="targetGroup">The group being moved to.</param>
        /// <param name="groupRoleId">The role of the new group members.</param>
        /// <param name="template">The registration template.</param>
        private void MoveRegistrantsToGroup( Registration registration, Group targetGroup, int groupRoleId, RegistrationTemplate template )
        {
            /*
                10/6/2026 - MSE

                Moving registrants removes their previous group membership, even when
                that membership existed before the registration, and creates a new one
                in the selected group. People already in the selected group (in any
                role) are left alone. Previous memberships are loaded without the
                archive filter so archived members are found and removed instead of
                failing the move part way through.

                Reason: The dialog promises to remove registrants from their existing group.
            */
            var registrants = registration.Registrants
                .Where( r => r.PersonAlias != null )
                .OrderBy( r => r.Id )
                .ToList();

            var personIds = registrants.Select( r => r.PersonAlias.PersonId ).Distinct().ToList();
            var groupMemberService = new GroupMemberService( RockContext );

            var existingPersonIds = groupMemberService
                .Queryable()
                .Where( m => m.GroupId == targetGroup.Id && personIds.Contains( m.PersonId ) )
                .Select( m => m.PersonId )
                .Distinct()
                .ToList()
                .ToHashSet();

            var registrantsToMove = registrants
                .Where( r => !existingPersonIds.Contains( r.PersonAlias.PersonId ) )
                .ToList();

            var previousMemberIds = registrantsToMove
                .Where( r => r.GroupMemberId.HasValue )
                .Select( r => r.GroupMemberId.Value )
                .Distinct()
                .ToList();

            var previousMembers = previousMemberIds.Any()
                ? groupMemberService.AsNoFilter().Where( m => previousMemberIds.Contains( m.Id ) ).ToList()
                : new List<GroupMember>();

            previousMembers.LoadAttributes( RockContext );

            var previousMembersById = previousMembers.ToDictionary( m => m.Id );
            var newMembersByPersonId = new Dictionary<int, GroupMember>();
            var previousValuesByNewMember = new Dictionary<GroupMember, Dictionary<string, string>>();
            var deletedMemberIds = new HashSet<int>();

            var memberStatus = targetGroup.GroupTypeId == template.GroupTypeId
                ? template.GroupMemberStatus
                : GroupMemberStatus.Active;

            foreach ( var registrant in registrantsToMove )
            {
                var personId = registrant.PersonAlias.PersonId;
                Dictionary<string, string> previousValues = null;

                if ( registrant.GroupMemberId.HasValue && previousMembersById.TryGetValue( registrant.GroupMemberId.Value, out var previousMember ) )
                {
                    previousValues = previousMember.AttributeValues?.ToDictionary( v => v.Key, v => v.Value.Value );

                    registrant.GroupMember = null;
                    registrant.GroupMemberId = null;

                    if ( deletedMemberIds.Add( previousMember.Id ) )
                    {
                        groupMemberService.Delete( previousMember );
                    }
                }

                if ( !newMembersByPersonId.TryGetValue( personId, out var newMember ) )
                {
                    newMember = new GroupMember
                    {
                        GroupId = targetGroup.Id,
                        PersonId = personId,
                        GroupRoleId = groupRoleId,
                        GroupMemberStatus = memberStatus
                    };

                    groupMemberService.Add( newMember );
                    newMembersByPersonId[personId] = newMember;
                }

                if ( previousValues != null && !previousValuesByNewMember.ContainsKey( newMember ) )
                {
                    previousValuesByNewMember[newMember] = previousValues;
                }

                registrant.GroupMember = newMember;
            }

            RockContext.SaveChanges();

            var newMembers = newMembersByPersonId.Values.ToList();

            if ( !previousValuesByNewMember.Any() )
            {
                return;
            }

            newMembers.LoadAttributes( RockContext );

            foreach ( var newMember in newMembers )
            {
                if ( !previousValuesByNewMember.TryGetValue( newMember, out var previousValues ) || newMember.Attributes == null )
                {
                    continue;
                }

                foreach ( var attribute in newMember.Attributes )
                {
                    if ( previousValues.TryGetValue( attribute.Key, out var value ) )
                    {
                        newMember.SetAttributeValue( attribute.Key, value );
                    }
                }

                newMember.SaveAttributeValues( RockContext );
            }
        }

        /// <summary>
        /// Saves registration history changes on the block context.
        /// </summary>
        /// <param name="registrationId">The registration identifier.</param>
        /// <param name="changes">The changes to save.</param>
        private void SaveRegistrationHistory( int registrationId, History.HistoryChangeList changes )
        {
            HistoryService.SaveChanges(
                RockContext,
                typeof( Registration ),
                Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                registrationId,
                changes );
        }

        /// <summary>
        /// Validates the values shared by manual and gateway payments: the
        /// account, registrar, amount, payer and comments.
        /// </summary>
        /// <param name="registration">The registration with its registrants loaded.</param>
        /// <param name="amount">The requested payment amount.</param>
        /// <param name="payerPersonAliasGuid">The unique identifier of the payer person alias, or <c>null</c> for the registrar.</param>
        /// <param name="comments">The payment comments.</param>
        /// <param name="payerPersonAliasId">Set to the person alias identifier of the payer.</param>
        /// <param name="errorMessage">Set to the validation error when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the payment values are valid.</returns>
        private bool TryValidatePayment( Registration registration, decimal amount, Guid? payerPersonAliasGuid, string comments, out int? payerPersonAliasId, out string errorMessage )
        {
            payerPersonAliasId = null;
            errorMessage = null;

            var instance = registration.RegistrationInstance;

            if ( !instance.AccountId.HasValue || FinancialAccountCache.Get( instance.AccountId.Value ) == null )
            {
                errorMessage = AccountConfigurationMessage;
                return false;
            }

            if ( !registration.PersonAliasId.HasValue )
            {
                errorMessage = LinkToIndividualMessage;
                return false;
            }

            var balanceDue = GetRegistrationAmounts( registration ).BalanceDue;

            if ( amount <= 0.0m || amount > balanceDue || amount != amount.AsCurrency() )
            {
                errorMessage = InvalidPaymentAmountMessage;
                return false;
            }

            if ( comments?.Length > MaximumPaymentCommentsLength )
            {
                errorMessage = $"Comments must be {MaximumPaymentCommentsLength} characters or fewer.";
                return false;
            }

            if ( payerPersonAliasGuid.HasValue )
            {
                payerPersonAliasId = new PersonAliasService( RockContext )
                    .Queryable()
                    .Where( pa => pa.Guid == payerPersonAliasGuid.Value )
                    .Select( pa => ( int? ) pa.Id )
                    .FirstOrDefault();

                if ( !payerPersonAliasId.HasValue )
                {
                    errorMessage = "The selected payer could not be found.";
                    return false;
                }
            }
            else
            {
                payerPersonAliasId = registration.PersonAliasId;
            }

            return true;
        }

        /// <summary>
        /// Gets the payment detail of a manual payment from the selected currency
        /// and credit card types. The credit card type is only kept when the
        /// currency type is credit card.
        /// </summary>
        /// <param name="request">The manual payment request.</param>
        /// <param name="paymentDetail">Set to the payment detail.</param>
        /// <param name="errorMessage">Set to the validation error when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the selected types are valid.</returns>
        private static bool TryGetManualPaymentDetail( ManualPaymentRequestBag request, out FinancialPaymentDetail paymentDetail, out string errorMessage )
        {
            paymentDetail = new FinancialPaymentDetail();
            errorMessage = null;

            if ( !request.CurrencyTypeValueGuid.HasValue )
            {
                return true;
            }

            var currencyType = DefinedValueCache.Get( request.CurrencyTypeValueGuid.Value );
            var currencyDefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid() )?.Id;

            if ( currencyType == null || currencyType.DefinedTypeId != currencyDefinedTypeId )
            {
                errorMessage = "The selected currency type is not valid.";
                return false;
            }

            paymentDetail.CurrencyTypeValueId = currencyType.Id;

            var isCreditCard = currencyType.Guid == Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid();

            if ( !isCreditCard || !request.CreditCardTypeValueGuid.HasValue )
            {
                return true;
            }

            var creditCardType = DefinedValueCache.Get( request.CreditCardTypeValueGuid.Value );
            var creditCardDefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE.AsGuid() )?.Id;

            if ( creditCardType == null || creditCardType.DefinedTypeId != creditCardDefinedTypeId )
            {
                errorMessage = "The selected credit card type is not valid.";
                return false;
            }

            paymentDetail.CreditCardTypeValueId = creditCardType.Id;

            return true;
        }

        /// <summary>
        /// Gets the options used to save a payment for the registration. The
        /// account always comes from the registration instance.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="amount">The validated payment amount.</param>
        /// <param name="payerPersonAliasId">The person alias identifier of the payer.</param>
        /// <param name="comments">The payment comments.</param>
        /// <returns>The payment transaction options.</returns>
        private RegistrationPaymentTransactionOptions GetPaymentTransactionOptions( Registration registration, decimal amount, int? payerPersonAliasId, string comments )
        {
            var template = registration.RegistrationInstance.RegistrationTemplate;
            var paymentComment = GetPaymentComment( registration.RegistrationInstance );
            var sourceGuid = GetAttributeValue( AttributeKey.Source ).AsGuidOrNull();

            return new RegistrationPaymentTransactionOptions
            {
                Amount = amount,
                AccountId = registration.RegistrationInstance.AccountId,
                AuthorizedPersonAliasId = payerPersonAliasId ?? registration.PersonAliasId,
                SourceTypeValueId = sourceGuid.HasValue ? DefinedValueCache.Get( sourceGuid.Value )?.Id : null,
                Summary = comments.IsNotNullOrWhiteSpace() ? $"{paymentComment}: {comments.Trim()}" : paymentComment,
                BatchNamePrefix = template.BatchNamePrefix.IsNotNullOrWhiteSpace()
                    ? template.BatchNamePrefix
                    : GetAttributeValue( AttributeKey.BatchNamePrefix ),
                ModifiedByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId
            };
        }

        /// <summary>
        /// Gets the payment comment of a registration instance, which is the
        /// instance name followed by the general ledger code of its account.
        /// </summary>
        /// <param name="instance">The registration instance.</param>
        /// <returns>The payment comment.</returns>
        private static string GetPaymentComment( RegistrationInstance instance )
        {
            var glCode = instance.AccountId.HasValue ? FinancialAccountCache.Get( instance.AccountId.Value )?.GlCode : null;

            return glCode.IsNotNullOrWhiteSpace()
                ? $"{instance.Name} ({glCode})"
                : instance.Name;
        }

        /// <summary>
        /// Gets the payment information sent to the gateway for a new payment.
        /// The registrar details identify the card holder, matching the
        /// confirmation the registrar receives.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="request">The process payment request.</param>
        /// <returns>The payment information.</returns>
        private static ReferencePaymentInfo GetHostedPaymentInfo( Registration registration, ProcessPaymentRequestBag request )
        {
            var instance = registration.RegistrationInstance;

            return new ReferencePaymentInfo
            {
                ReferenceNumber = request.GatewayToken,
                Amount = request.Amount,
                AccountAllocations = new List<FinancialTransactionService.AccountAllocation>
                {
                    new FinancialTransactionService.AccountAllocation( instance.AccountId.Value, request.Amount )
                },
                Email = registration.ConfirmationEmail,
                FirstName = registration.FirstName,
                LastName = registration.LastName,
                Comment1 = GetPaymentComment( instance ),
                TransactionTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION.AsGuid() ).Id,
                Street1 = request.BillingAddress.Street1,
                Street2 = request.BillingAddress.Street2,
                City = request.BillingAddress.City,
                State = request.BillingAddress.State,
                PostalCode = request.BillingAddress.PostalCode,
                Country = request.BillingAddress.Country
            };
        }

        /// <summary>
        /// Determines whether new payments can be charged through a gateway
        /// from this block.
        /// </summary>
        /// <param name="gateway">The financial gateway.</param>
        /// <returns><c>true</c> if the gateway supports hosted payments with an Obsidian control.</returns>
        private static bool IsHostedPaymentSupported( FinancialGateway gateway )
        {
            if ( gateway == null || !gateway.IsActive )
            {
                return false;
            }

            var component = gateway.GetGatewayComponent();

            return component is IObsidianHostedGatewayComponent
                && component is IHostedGatewayComponent hostedComponent
                && hostedComponent.GetSupportedHostedGatewayModes( gateway ).Contains( HostedGatewayMode.Hosted );
        }

        /// <summary>
        /// Gets the home address of a person from their primary family, preferring
        /// a mapped location and then the most recent one.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns>The address, or <c>null</c> when the person has no home address.</returns>
        private AddressControlBag GetHomeAddress( Person person )
        {
            var homeLocationTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() )?.Id;

            if ( !person.PrimaryFamilyId.HasValue || !homeLocationTypeId.HasValue )
            {
                return null;
            }

            var familyId = person.PrimaryFamilyId.Value;

            return new GroupLocationService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gl => gl.GroupId == familyId && gl.GroupLocationTypeValueId == homeLocationTypeId.Value )
                .OrderByDescending( gl => gl.IsMappedLocation )
                .ThenByDescending( gl => gl.Id )
                .Select( gl => new AddressControlBag
                {
                    Street1 = gl.Location.Street1,
                    Street2 = gl.Location.Street2,
                    City = gl.Location.City,
                    State = gl.Location.State,
                    PostalCode = gl.Location.PostalCode,
                    Country = gl.Location.Country
                } )
                .FirstOrDefault();
        }

        /// <summary>
        /// Loads the registration for a payment plan action after checking edit
        /// access, the Edit Payment Plan action and that the plan is active.
        /// </summary>
        /// <param name="key">The identifier key of the registration.</param>
        /// <param name="registration">Set to the loaded registration.</param>
        /// <param name="error">Set to the action error when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the payment plan can be changed by the current person.</returns>
        private bool TryGetPaymentPlanRegistration( string key, out Registration registration, out BlockActionResult error )
        {
            registration = null;

            if ( !TryGetEntityForEditAction( key, out var entity, out error ) )
            {
                return false;
            }

            if ( !CanEditPaymentPlan() )
            {
                error = ActionForbidden( "You are not authorized to edit the payment plan." );
                return false;
            }

            registration = GetHydratedRegistration( entity.Id );

            if ( registration.PaymentPlanFinancialScheduledTransaction?.IsActive != true )
            {
                error = ActionBadRequest( "This registration does not have a payment plan to update" );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Calculates a payment plan configuration for the registration. The
        /// gateway of the existing plan is used, since the template gateway may
        /// have changed after the plan was created.
        /// </summary>
        /// <param name="registration">The registration with an active payment plan.</param>
        /// <param name="request">The requested values.</param>
        /// <param name="configuration">Set to the calculated configuration.</param>
        /// <param name="errorMessage">Set to the error when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the configuration could be calculated.</returns>
        private bool TryGetPaymentPlanConfiguration( Registration registration, PaymentPlanConfigurationRequestBag request, out PaymentPlanConfiguration configuration, out string errorMessage )
        {
            configuration = null;
            errorMessage = null;

            var scheduledTransaction = registration.PaymentPlanFinancialScheduledTransaction;
            var component = scheduledTransaction.FinancialGateway?.GetGatewayComponent();

            if ( component == null )
            {
                errorMessage = "Unable to retrieve payment gateway information";
                return false;
            }

            var instance = registration.RegistrationInstance;
            var frequencies = GetPaymentPlanFrequencies( instance.RegistrationTemplate, component );
            var amounts = GetRegistrationAmounts( registration );

            var options = new PaymentPlanConfigurationOptions
            {
                DesiredAllowedPaymentFrequencies = frequencies,
                AmountForPaymentPlan = amounts.BalanceDue,
                CurrencyPrecision = new RockCurrencyCodeInfo().DecimalPlaces,

                // Staff can choose any frequency and number of payments as long as there is at least one payment.
                IsNumberOfPaymentsLimited = false,
                MinNumberOfPayments = 1,

                // The payment deadline should have a value since it is required,
                // but default to next year just in case it is missing.
                EndDate = ( instance.PaymentDeadlineDate ?? RockDateTime.Now.AddYears( 1 ) ).Date
            };

            if ( request.IsInitial )
            {
                var paymentPlan = GetPaymentPlan( registration );

                // Default to the payments that remain, since the amount is the balance that remains.
                options.DesiredNumberOfPayments = Math.Max( 1, paymentPlan?.NumberOfPaymentsRemaining ?? scheduledTransaction.NumberOfPayments ?? 1 );
                options.DesiredStartDate = ( GetNextPaymentDate( registration ) ?? RockDateTime.Now.AddDays( 1 ) ).Date;
                options.DesiredPaymentFrequency = frequencies.FirstOrDefault( f => f.Id == scheduledTransaction.TransactionFrequencyValueId );
            }
            else
            {
                options.DesiredNumberOfPayments = request.NumberOfPayments ?? 0;
                options.DesiredStartDate = request.StartDate?.Date ?? RockDateTime.Today.AddDays( 1 );
                options.DesiredPaymentFrequency = request.FrequencyGuid.HasValue
                    ? frequencies.FirstOrDefault( f => f.Guid == request.FrequencyGuid.Value )
                    : null;
            }

            configuration = new PaymentPlanConfigurationService().Get( options );

            return true;
        }

        /// <summary>
        /// Gets the payment frequencies allowed for a payment plan: those chosen
        /// on the template, otherwise every recurring frequency the gateway supports.
        /// </summary>
        /// <param name="template">The registration template.</param>
        /// <param name="component">The gateway component of the payment plan.</param>
        /// <returns>The allowed payment frequencies.</returns>
        private static List<DefinedValueCache> GetPaymentPlanFrequencies( RegistrationTemplate template, GatewayComponent component )
        {
            var templateFrequencyIds = template.PaymentPlanFrequencyValueIdsCollection;

            if ( templateFrequencyIds?.Any() == true )
            {
                return templateFrequencyIds
                    .Select( id => DefinedValueCache.Get( id ) )
                    .Where( frequency => frequency != null )
                    .ToList();
            }

            var oneTimeGuid = Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid();

            return component.SupportedPaymentSchedules?
                .Where( f => f.Guid != oneTimeGuid )
                .ToList() ?? new List<DefinedValueCache>();
        }

        /// <summary>
        /// Gets the bag that describes a calculated payment plan configuration.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="configuration">The calculated configuration.</param>
        /// <returns>The payment plan configuration bag.</returns>
        private PaymentPlanConfigurationBag GetPaymentPlanConfigurationBag( Registration registration, PaymentPlanConfiguration configuration )
        {
            var balanceDue = GetRegistrationAmounts( registration ).BalanceDue;
            var frequency = configuration.PaymentFrequencyConfiguration?.PaymentFrequency;
            var remainingBalance = balanceDue - configuration.PlannedAmount;

            var summaryText = string.Empty;

            if ( configuration.NumberOfPayments > 0 && frequency != null )
            {
                var remainderSuffix = remainingBalance > 0.0m
                    ? $" (this will leave a remaining balance of {remainingBalance.FormatAsCurrency()})"
                    : string.Empty;

                summaryText = $"{configuration.AmountPerPayment.FormatAsCurrency()} × {configuration.NumberOfPayments} {frequency}{remainderSuffix}";
            }

            return new PaymentPlanConfigurationBag
            {
                Frequencies = configuration.AllowedPaymentFrequencyConfigurations?
                    .Select( c => new ListItemBag
                    {
                        Value = c.PaymentFrequency.Guid.ToString(),
                        Text = c.PaymentFrequency.ToString()
                    } )
                    .ToList() ?? new List<ListItemBag>(),
                FrequencyGuid = frequency?.Guid,
                StartDate = configuration.StartDate.ToString( "yyyy-MM-dd" ),
                NumberOfPayments = configuration.NumberOfPayments,
                MinNumberOfPayments = configuration.MinNumberOfPayments,
                IsSummaryVisible = configuration.AmountPerPayment > 0.0m,
                SummaryText = summaryText,
                BalanceDueMessage = $"The amount remaining for this registration is {balanceDue.FormatAsCurrency()}."
            };
        }

        /// <summary>
        /// Updates the payment plan on its gateway and saves the new plan, using
        /// the payment method already on the plan.
        /// </summary>
        /// <param name="registration">The registration with an active payment plan.</param>
        /// <param name="configuration">The validated configuration.</param>
        /// <param name="errorMessage">Set to the error when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the plan was updated.</returns>
        private bool TryUpdatePaymentPlan( Registration registration, PaymentPlanConfiguration configuration, out string errorMessage )
        {
            errorMessage = null;

            var scheduledTransaction = registration.PaymentPlanFinancialScheduledTransaction;
            var paymentDetail = scheduledTransaction.FinancialPaymentDetail;
            var component = scheduledTransaction.FinancialGateway?.GetGatewayComponent();

            if ( component == null )
            {
                errorMessage = "Unable to retrieve payment gateway information";
                return false;
            }

            if ( !( component is IScheduledNumberOfPaymentsGateway ) )
            {
                errorMessage = "Payment plans are not supported by this payment gateway";
                return false;
            }

            if ( paymentDetail == null )
            {
                errorMessage = "Unable to retrieve the payment information of the payment plan.";
                return false;
            }

            var referenceNumber = component.GetReferenceNumber( scheduledTransaction, out var referenceNumberError );

            var paymentInfo = new ReferencePaymentInfo
            {
                // Use the existing payment method.
                GatewayPersonIdentifier = paymentDetail.GatewayPersonIdentifier,
                FinancialPersonSavedAccountId = paymentDetail.FinancialPersonSavedAccountId,

                ReferenceNumber = referenceNumber,
                TransactionTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION.AsGuid() ).Id,
                InitialCurrencyTypeValue = paymentDetail.CurrencyTypeValueId.HasValue ? DefinedValueCache.Get( paymentDetail.CurrencyTypeValueId.Value ) : null,
                InitialCreditCardTypeValue = paymentDetail.CreditCardTypeValueId.HasValue ? DefinedValueCache.Get( paymentDetail.CreditCardTypeValueId.Value ) : null,
                MaskedAccountNumber = paymentDetail.AccountNumberMasked,
                PaymentExpirationDate = GetPaymentExpirationDate( paymentDetail ),
                FirstName = paymentDetail.NameOnCard,
                Street1 = paymentDetail.BillingLocation?.Street1,
                Street2 = paymentDetail.BillingLocation?.Street2,
                City = paymentDetail.BillingLocation?.City,
                State = paymentDetail.BillingLocation?.State,
                PostalCode = paymentDetail.BillingLocation?.PostalCode,
                Country = paymentDetail.BillingLocation?.Country
            };

            if ( referenceNumberError.IsNotNullOrWhiteSpace() )
            {
                errorMessage = referenceNumberError;
                return false;
            }

            configuration.CopyPaymentPlanDetailsTo( paymentInfo );
            configuration.CopyPaymentPlanDetailsTo( scheduledTransaction );

            var originalGatewayScheduleId = scheduledTransaction.GatewayScheduleId;

            try
            {
                /*
                    07/11/2025 - NA

                    Some gateways do not properly populate the FinancialPaymentDetail record they create
                    during the UpdateScheduledPayment() method. To compensate, we're preserving existing
                    values to restore them if needed.

                    Reason: Expedites resolution of issue #6367 by ensuring payment details remain complete.
                */
                var savedNameOnCard = paymentDetail.NameOnCard;
                var savedExpirationMonth = paymentDetail.ExpirationMonth;
                var savedExpirationYear = paymentDetail.ExpirationYear;

                // We are using the existing payment method; DO NOT clear out the FinancialPaymentDetail record.
                var isUpdated = component.UpdateScheduledPayment( scheduledTransaction, paymentInfo, out var updateError );

                if ( !isUpdated || updateError.IsNotNullOrWhiteSpace() )
                {
                    // Nothing is saved: the plan details copied above stay unsaved.
                    errorMessage = updateError.IsNotNullOrWhiteSpace() ? updateError : "Unknown error occurred while updating scheduled payment";
                    return false;
                }

                /*
                    07/11/2025 - NA

                    After the transaction is processed, the gateway should populate the FinancialPaymentDetail
                    since it has more data than Rock (as the gateway collects the payment information directly).
                    However, if paymentPlanPaymentInfo contains additional values, we’ll attempt to merge those
                    into FinancialPaymentDetail.

                    Reason: This mirrors similar logic used in the TransactionEntryV2 block.
                */
                paymentDetail.SetFromPaymentInfo( paymentInfo, component, RockContext );

                // See engineering note above for these three items:
                if ( string.IsNullOrEmpty( paymentDetail.NameOnCard ) && !string.IsNullOrWhiteSpace( savedNameOnCard ) )
                {
                    paymentDetail.NameOnCard = savedNameOnCard;
                }

                if ( paymentDetail.ExpirationMonth == null && savedExpirationMonth.HasValue )
                {
                    paymentDetail.ExpirationMonth = savedExpirationMonth;
                }

                if ( paymentDetail.ExpirationYear == null && savedExpirationYear.HasValue )
                {
                    paymentDetail.ExpirationYear = savedExpirationYear;
                }

                var scheduledTransactionDetail = scheduledTransaction.ScheduledTransactionDetails.FirstOrDefault();

                if ( scheduledTransactionDetail == null )
                {
                    // This shouldn't happen.
                    scheduledTransactionDetail = new FinancialScheduledTransactionDetail();
                    scheduledTransaction.ScheduledTransactionDetails.Add( scheduledTransactionDetail );
                }

                scheduledTransactionDetail.AccountId = registration.RegistrationInstance.AccountId ?? scheduledTransactionDetail.AccountId;
                scheduledTransactionDetail.EntityTypeId = EntityTypeCache.Get<Registration>().Id;
                scheduledTransactionDetail.EntityId = registration.Id;

                configuration.CopyPaymentPlanDetailsTo( scheduledTransactionDetail );

                var changes = new History.HistoryChangeList();
                changes.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Record, "Payment Plan" )
                    .SetNewValue( $"{configuration.AmountPerPayment.FormatAsCurrency()} × {configuration.NumberOfPayments} {configuration.PaymentFrequencyConfiguration?.PaymentFrequency} starting {configuration.StartDate.ToShortDateString()}" );

                RockContext.WrapTransaction( () =>
                {
                    RockContext.SaveChanges();
                    SaveRegistrationHistory( registration.Id, changes );
                } );

                return true;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );

                // if the GatewayScheduleId was updated, but there was an exception,
                // make sure we save the financialScheduledTransaction record with the updated GatewayScheduleId so we don't orphan it
                if ( scheduledTransaction.GatewayScheduleId.IsNotNullOrWhiteSpace() && originalGatewayScheduleId != scheduledTransaction.GatewayScheduleId )
                {
                    try
                    {
                        RockContext.SaveChanges();
                    }
                    catch ( Exception saveException )
                    {
                        ExceptionLogService.LogException( saveException );
                    }
                }

                errorMessage = "An error occurred while updating the payment plan.";
                return false;
            }
        }

        /// <summary>
        /// Gets the expiration date of a saved payment method as the first day
        /// of its expiration month.
        /// </summary>
        /// <param name="paymentDetail">The payment detail.</param>
        /// <returns>The expiration date, or <c>null</c> when it is not known.</returns>
        private static DateTime? GetPaymentExpirationDate( FinancialPaymentDetail paymentDetail )
        {
            var year = paymentDetail.ExpirationYear;
            var month = paymentDetail.ExpirationMonth;

            if ( !year.HasValue || !month.HasValue || year.Value < 1 || month.Value < 1 || month.Value > 12 )
            {
                return null;
            }

            return new DateTime( year.Value, month.Value, 1 );
        }

        /// <summary>
        /// Gets the registration with everything needed to build its bags, on
        /// the block context. The result is cached until the next change is saved.
        /// </summary>
        /// <param name="registrationId">The registration identifier.</param>
        /// <returns>The registration.</returns>
        private Registration GetHydratedRegistration( int registrationId )
        {
            if ( _hydratedRegistration != null && _hydratedRegistration.Id == registrationId )
            {
                return _hydratedRegistration;
            }

            var query = new RegistrationService( RockContext )
                .Queryable()
                .Where( r => r.Id == registrationId );

            _hydratedRegistration = IncludeRegistrationGraph( query ).FirstOrDefault();

            return _hydratedRegistration;
        }

        /// <summary>
        /// Adds the navigation properties used to build the bags of a registration.
        /// </summary>
        /// <param name="query">The registration query.</param>
        /// <returns>The query with the navigation properties included.</returns>
        private static IQueryable<Registration> IncludeRegistrationGraph( IQueryable<Registration> query )
        {
            return query
                .Include( r => r.RegistrationInstance.RegistrationTemplate.Forms.Select( f => f.Fields ) )
                .Include( r => r.RegistrationInstance.RegistrationTemplate.Fees )
                .Include( r => r.RegistrationInstance.RegistrationTemplate.FinancialGateway )
                .Include( r => r.RegistrationInstance.RegistrationTemplate.RequiredSignatureDocumentTemplate )
                .Include( r => r.PersonAlias.Person )
                .Include( r => r.Group )
                .Include( r => r.Registrants.Select( rg => rg.PersonAlias.Person ) )
                .Include( r => r.Registrants.Select( rg => rg.Fees.Select( f => f.RegistrationTemplateFee ) ) )
                .Include( r => r.Registrants.Select( rg => rg.Fees.Select( f => f.RegistrationTemplateFeeItem ) ) )
                .Include( r => r.PaymentPlanFinancialScheduledTransaction.TransactionFrequencyValue )
                .Include( r => r.PaymentPlanFinancialScheduledTransaction.FinancialGateway );
        }

        /// <summary>
        /// Gets the rebuilt view bag of a registration.
        /// </summary>
        /// <param name="registrationId">The registration identifier.</param>
        /// <returns>The view bag.</returns>
        private RegistrationBag GetViewBag( int registrationId )
        {
            return GetEntityBagForView( GetHydratedRegistration( registrationId ) );
        }

        /// <summary>
        /// Clears the cached registration data. Called after a change is saved
        /// so the rebuilt view bag shows the saved values.
        /// </summary>
        private void ClearRegistrationMemos()
        {
            _hydratedRegistration = null;
            _registrationAmounts = null;
            _paymentPlan = null;
            _isPaymentPlanLoaded = false;
            _nextPaymentDate = null;
            _isNextPaymentDateLoaded = false;
        }

        /// <summary>
        /// Gets the money amounts of a registration. The costs are calculated
        /// from the loaded registrants and fees, and the amount paid with a
        /// single query on the block context. The result is cached until the
        /// next change is saved.
        /// </summary>
        /// <param name="registration">The registration with its registrants and fees loaded.</param>
        /// <returns>The money amounts.</returns>
        private RegistrationAmounts GetRegistrationAmounts( Registration registration )
        {
            if ( _registrationAmounts != null && _registrationAmounts.RegistrationId == registration.Id )
            {
                return _registrationAmounts;
            }

            var totalCost = registration.Registrants.Sum( r => r.TotalCost );
            var discountedCost = registration.Registrants.Sum( r => r.DiscountedCost( registration.DiscountPercentage, registration.DiscountAmount ) );
            var totalPaid = new RegistrationService( RockContext ).GetTotalPayments( registration.Id );

            _registrationAmounts = new RegistrationAmounts
            {
                RegistrationId = registration.Id,
                TotalCost = totalCost,
                DiscountedCost = discountedCost,
                TotalPaid = totalPaid,
                BalanceDue = ( discountedCost - totalPaid ).AsCurrency()
            };

            return _registrationAmounts;
        }

        /// <summary>
        /// Gets the label type used to display the balance due.
        /// </summary>
        /// <param name="balanceDue">The balance due.</param>
        /// <param name="isPaymentPlanActive"><c>true</c> if the registration has an active payment plan.</param>
        /// <returns>The label type.</returns>
        private static string GetBalanceDueLabelType( decimal balanceDue, bool isPaymentPlanActive )
        {
            if ( balanceDue > 0.0m )
            {
                return isPaymentPlanActive ? "warning" : "danger";
            }

            return balanceDue < 0.0m ? "warning" : "success";
        }

        /// <summary>
        /// Gets the rows of the cost summary. Rows follow the same rules as the
        /// registration totals: wait listed registrants cost nothing and the
        /// discount amount never takes a registrant below zero, so the totals
        /// always match the account summary.
        /// </summary>
        /// <param name="registration">The registration with its registrants and fees loaded.</param>
        /// <param name="amounts">The money amounts of the registration.</param>
        /// <returns>The cost summary rows, or an empty list when there are no costs or fees.</returns>
        private static List<RegistrationFeeRowBag> GetFeeRows( Registration registration, RegistrationAmounts amounts )
        {
            var rows = new List<RegistrationFeeRowBag>();
            var discountPercentage = registration.DiscountPercentage;
            var discountAmount = registration.DiscountAmount;
            var totalAmountDiscount = 0.0m;

            foreach ( var registrant in registration.Registrants.OrderBy( r => r.Id ) )
            {
                var personName = registrant.PersonAlias?.Person?.FullName ?? string.Empty;
                var isDiscounted = registrant.DiscountApplies && !registrant.OnWaitList;
                var baseCost = registrant.OnWaitList ? 0.0m : registrant.Cost;
                var baseDiscountedCost = isDiscounted ? baseCost - ( baseCost * discountPercentage ) : baseCost;

                if ( registrant.Cost > 0.0m )
                {
                    rows.Add( new RegistrationFeeRowBag
                    {
                        RowType = "cost",
                        Description = registrant.OnWaitList ? $"{personName} (Waiting List)" : personName,
                        CostText = baseCost.FormatAsCurrency(),
                        DiscountedCostText = baseDiscountedCost.FormatAsCurrency()
                    } );
                }

                var feesDiscountedCost = 0.0m;

                foreach ( var fee in registrant.Fees.OrderBy( f => f.Id ) )
                {
                    var feeCost = registrant.OnWaitList ? 0.0m : fee.TotalCost;
                    var feeDiscountedCost = registrant.OnWaitList ? 0.0m : fee.DiscountedCost( discountPercentage );
                    var description = $"{GetFeeName( fee )} ({fee.Quantity:N0} @ {fee.Cost.FormatAsCurrency()})";

                    feesDiscountedCost += feeDiscountedCost;

                    rows.Add( new RegistrationFeeRowBag
                    {
                        RowType = "fee",
                        Description = registrant.OnWaitList ? $"{description} (Waiting List)" : description,
                        CostText = feeCost.FormatAsCurrency(),
                        DiscountedCostText = feeDiscountedCost.FormatAsCurrency()
                    } );
                }

                if ( isDiscounted && discountAmount > 0.0m )
                {
                    var costBeforeAmountDiscount = Math.Max( 0.0m, baseDiscountedCost + feesDiscountedCost );
                    totalAmountDiscount += Math.Min( discountAmount, costBeforeAmountDiscount );
                }
            }

            if ( !rows.Any() )
            {
                return rows;
            }

            if ( discountAmount > 0.0m )
            {
                rows.Add( new RegistrationFeeRowBag
                {
                    RowType = "discount",
                    Description = "Discount",
                    CostText = ( 0.0m - totalAmountDiscount ).FormatAsCurrency(),
                    DiscountedCostText = ( 0.0m - totalAmountDiscount ).FormatAsCurrency()
                } );
            }

            var isDiscountedColumnVisible = discountPercentage > 0.0m;

            rows.Add( new RegistrationFeeRowBag
            {
                RowType = "total",
                Description = "Total",
                CostText = ( isDiscountedColumnVisible ? amounts.TotalCost - totalAmountDiscount : amounts.DiscountedCost ).FormatAsCurrency(),
                DiscountedCostText = amounts.DiscountedCost.FormatAsCurrency()
            } );

            return rows;
        }

        /// <summary>
        /// Gets the display name of a registrant fee: the template fee name, with
        /// the selected option for multiple option fees.
        /// </summary>
        /// <param name="fee">The registrant fee with its template fee and item loaded.</param>
        /// <returns>The fee name.</returns>
        private static string GetFeeName( RegistrationRegistrantFee fee )
        {
            var feeLabel = fee.RegistrationTemplateFeeItem?.Name;
            var templateFee = fee.RegistrationTemplateFee;

            if ( templateFee == null )
            {
                return feeLabel.IsNotNullOrWhiteSpace() ? $"(Previous Cost)-{feeLabel}" : "(Previous Cost)";
            }

            if ( templateFee.FeeType == RegistrationFeeType.Multiple && feeLabel.IsNotNullOrWhiteSpace() )
            {
                return $"{templateFee.Name}-{feeLabel}";
            }

            return templateFee.Name;
        }

        /// <summary>
        /// Gets the payments of the registration with a single projection. The
        /// amount is the part of each transaction applied to this registration.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <returns>The payment rows, most recent first.</returns>
        private List<RegistrationPaymentRowBag> GetPaymentRows( Registration registration )
        {
            var registrationEntityTypeId = EntityTypeCache.Get<Registration>().Id;
            var registrationId = registration.Id;

            var transactionIdQuery = new FinancialTransactionDetailService( RockContext )
                .Queryable()
                .Where( d => d.EntityTypeId == registrationEntityTypeId && d.EntityId == registrationId )
                .Select( d => d.TransactionId );

            var transactions = new FinancialTransactionService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( t => transactionIdQuery.Contains( t.Id ) )
                .OrderByDescending( t => t.TransactionDateTime )
                .ThenByDescending( t => t.Id )
                .Select( t => new
                {
                    t.Id,
                    t.TransactionDateTime,
                    t.TransactionCode,
                    PayerFirstName = t.AuthorizedPersonAlias.Person.FirstName,
                    PayerLastName = t.AuthorizedPersonAlias.Person.LastName,
                    PayerSuffixValueId = t.AuthorizedPersonAlias.Person.SuffixValueId,
                    PayerRecordTypeValueId = t.AuthorizedPersonAlias.Person.RecordTypeValueId,
                    HasPayer = t.AuthorizedPersonAlias.Person != null,
                    CurrencyTypeValueId = t.FinancialPaymentDetail.CurrencyTypeValueId,
                    CreditCardTypeValueId = t.FinancialPaymentDetail.CreditCardTypeValueId,
                    AccountNumberMasked = t.FinancialPaymentDetail.AccountNumberMasked,
                    IsRefund = t.RefundDetails != null,
                    RegistrationAmount = t.TransactionDetails
                        .Where( d => d.EntityTypeId == registrationEntityTypeId && d.EntityId == registrationId )
                        .Sum( d => ( decimal? ) d.Amount ) ?? 0.0m
                } )
                .ToList();

            return transactions
                .Select( t => new RegistrationPaymentRowBag
                {
                    IdKey = IdHasher.Instance.GetHash( t.Id ),
                    TransactionDateText = t.TransactionDateTime?.ToShortDateString(),
                    TransactionTimeText = t.TransactionDateTime?.ToShortTimeString(),
                    PayerName = t.HasPayer ? GetFormalReversedName( t.PayerFirstName, t.PayerLastName, t.PayerSuffixValueId, t.PayerRecordTypeValueId ) : null,
                    CurrencyAndCreditCardType = GetCurrencyAndCreditCardType( t.CurrencyTypeValueId, t.CreditCardTypeValueId ),
                    AccountNumberMasked = t.AccountNumberMasked,
                    TransactionCode = t.TransactionCode,
                    IsRefund = t.IsRefund,
                    AmountText = t.RegistrationAmount.FormatAsCurrency()
                } )
                .ToList();
        }

        /// <summary>
        /// Formats a name as "Last Suffix, First", or just the last name for a business.
        /// </summary>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="suffixValueId">The suffix defined value identifier.</param>
        /// <param name="recordTypeValueId">The record type defined value identifier.</param>
        /// <returns>The formatted name.</returns>
        private static string GetFormalReversedName( string firstName, string lastName, int? suffixValueId, int? recordTypeValueId )
        {
            var businessRecordTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() )?.Id;

            if ( recordTypeValueId.HasValue && recordTypeValueId == businessRecordTypeId )
            {
                return lastName;
            }

            var suffix = suffixValueId.HasValue ? DefinedValueCache.Get( suffixValueId.Value )?.Value : null;

            return suffix.IsNotNullOrWhiteSpace()
                ? $"{lastName} {suffix}, {firstName}"
                : $"{lastName}, {firstName}";
        }

        /// <summary>
        /// Formats the currency type and credit card type of a payment.
        /// </summary>
        /// <param name="currencyTypeValueId">The currency type defined value identifier.</param>
        /// <param name="creditCardTypeValueId">The credit card type defined value identifier.</param>
        /// <returns>The formatted text.</returns>
        private static string GetCurrencyAndCreditCardType( int? currencyTypeValueId, int? creditCardTypeValueId )
        {
            var currencyType = currencyTypeValueId.HasValue ? DefinedValueCache.Get( currencyTypeValueId.Value )?.Value : null;
            var creditCardType = creditCardTypeValueId.HasValue ? DefinedValueCache.Get( creditCardTypeValueId.Value )?.Value : null;

            var text = currencyType ?? string.Empty;

            if ( creditCardType != null )
            {
                text += $" - {creditCardType}";
            }

            return text;
        }

        /// <summary>
        /// Gets the payment plan of the registration, calculated in the database.
        /// The result is cached until the next change is saved.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <returns>The payment plan, or <c>null</c> when there is none.</returns>
        private PaymentPlan GetPaymentPlan( Registration registration )
        {
            if ( _isPaymentPlanLoaded )
            {
                return _paymentPlan;
            }

            var scheduledTransactionId = registration.PaymentPlanFinancialScheduledTransactionId;

            _paymentPlan = scheduledTransactionId.HasValue
                ? new FinancialScheduledTransactionService( RockContext )
                    .Queryable()
                    .Where( f => f.Id == scheduledTransactionId.Value )
                    .SelectPaymentPlanPairs()
                    .Select( p => p.PaymentPlan )
                    .FirstOrDefault()
                : null;

            _isPaymentPlanLoaded = true;

            return _paymentPlan;
        }

        /// <summary>
        /// Gets the next payment date of the payment plan from the gateway of
        /// the plan. The gateway is asked at most once per request.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <returns>The next payment date, or <c>null</c> when there is no next payment.</returns>
        private DateTime? GetNextPaymentDate( Registration registration )
        {
            if ( _isNextPaymentDateLoaded )
            {
                return _nextPaymentDate;
            }

            _isNextPaymentDateLoaded = true;
            _nextPaymentDate = null;

            var scheduledTransaction = registration.PaymentPlanFinancialScheduledTransaction;
            var component = scheduledTransaction?.FinancialGateway?.GetGatewayComponent();

            if ( component == null )
            {
                return null;
            }

            var scheduledTransactionId = scheduledTransaction.Id;
            var lastTransactionDate = new FinancialTransactionService( RockContext )
                .Queryable()
                .Where( t => t.ScheduledTransactionId == scheduledTransactionId && t.TransactionDateTime.HasValue )
                .Max( t => ( DateTime? ) t.TransactionDateTime.Value );

            try
            {
                _nextPaymentDate = component.GetNextPaymentDate( scheduledTransaction, lastTransactionDate );
            }
            catch ( Exception ex )
            {
                // A failing gateway must not break the page; the plan then shows no next payment.
                ExceptionLogService.LogException( ex );
            }

            return _nextPaymentDate;
        }

        /// <summary>
        /// Gets the summary of the active payment plan of the registration.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="amounts">The money amounts of the registration.</param>
        /// <returns>The payment plan summary, or <c>null</c> when there is no active plan.</returns>
        private RegistrationPaymentPlanSummaryBag GetPaymentPlanSummary( Registration registration, RegistrationAmounts amounts )
        {
            if ( !registration.IsPaymentPlanActive )
            {
                return null;
            }

            var scheduledTransaction = registration.PaymentPlanFinancialScheduledTransaction;
            var paymentPlan = GetPaymentPlan( registration );
            var nextPaymentDate = GetNextPaymentDate( registration );

            var summary = new RegistrationPaymentPlanSummaryBag
            {
                PaymentAmountLabel = $"{scheduledTransaction.TransactionFrequencyValue} Payment Amount",
                PaymentAmountText = $"{( paymentPlan?.AmountPerPayment ?? 0.0m ).FormatAsCurrency()} × {scheduledTransaction.NumberOfPayments}",
                IsEditable = CanEditPaymentPlan() && CanEdit( registration ),
                IsAmountMismatch = IsPaymentPlanAmountMismatch( paymentPlan, amounts.BalanceDue )
            };

            if ( nextPaymentDate.HasValue )
            {
                summary.NextPaymentDateText = nextPaymentDate.Value.ToShortDateString();

                if ( nextPaymentDate.Value.Date == RockDateTime.Today )
                {
                    summary.IsChangeDisabled = true;
                    summary.ChangeDisabledReason = "The plan cannot be changed because the next payment is today (and may be in process).";
                }
                else if ( amounts.BalanceDue <= 0.0m )
                {
                    summary.IsChangeDisabled = true;
                    summary.ChangeDisabledReason = "The plan cannot be changed because the amount remaining is zero.";
                }
            }
            else
            {
                // The plan is complete, so it can be neither changed nor deleted.
                summary.IsChangeDisabled = true;
                summary.IsDeleteDisabled = true;
            }

            return summary;
        }

        /// <summary>
        /// Determines whether the remaining planned payments do not cover the
        /// balance due. Each payment is rounded down to the currency precision,
        /// so a difference smaller than one minor unit per remaining payment is
        /// expected and is not a mismatch.
        /// </summary>
        /// <param name="paymentPlan">The payment plan.</param>
        /// <param name="balanceDue">The balance due.</param>
        /// <returns><c>true</c> if the amounts do not match.</returns>
        private static bool IsPaymentPlanAmountMismatch( PaymentPlan paymentPlan, decimal balanceDue )
        {
            if ( paymentPlan == null || !paymentPlan.IsActive )
            {
                return false;
            }

            var difference = Math.Abs( balanceDue - paymentPlan.PlannedAmountRemaining );

            if ( difference == 0.0m )
            {
                return false;
            }

            var minorUnit = 1.0m;
            var decimalPlaces = new RockCurrencyCodeInfo().DecimalPlaces;

            for ( var i = 0; i < decimalPlaces; i++ )
            {
                minorUnit /= 10.0m;
            }

            var tolerance = Math.Max( 0, paymentPlan.NumberOfPaymentsRemaining ) * minorUnit;

            return difference >= tolerance;
        }

        /// <summary>
        /// Gets the registrant panels of the registration. Everything the panels
        /// need is loaded with a fixed number of queries, regardless of the
        /// number of registrants.
        /// </summary>
        /// <param name="registration">The registration with its registrants, fees and template loaded.</param>
        /// <param name="attributeDefinitions">Set to the attribute definitions used by the registrant field values.</param>
        /// <returns>The registrant panels.</returns>
        private List<RegistrantPanelBag> GetRegistrantPanels( Registration registration, out Dictionary<Guid, PublicAttributeBag> attributeDefinitions )
        {
            attributeDefinitions = new Dictionary<Guid, PublicAttributeBag>();

            var template = registration.RegistrationInstance.RegistrationTemplate;
            var registrants = registration.Registrants.OrderBy( r => r.Id ).ToList();
            var fields = template.Forms
                .OrderBy( f => f.Order )
                .SelectMany( f => f.Fields.OrderBy( field => field.Order ) )
                .ToList();

            var lookups = new RegistrantLookups( registration, registrants, fields );
            LoadGroupMemberInfo( lookups );
            LoadPersonFieldValues( lookups );
            LoadAttributeValues( lookups );
            LoadSignatureDocuments( lookups, template );

            var panels = new List<RegistrantPanelBag>();

            foreach ( var registrant in registrants )
            {
                panels.Add( GetRegistrantPanel( registration, registrant, fields, lookups, attributeDefinitions ) );
            }

            return panels;
        }

        /// <summary>
        /// Gets the panel of a single registrant.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="registrant">The registrant.</param>
        /// <param name="fields">The form fields of the template, in display order.</param>
        /// <param name="lookups">The data loaded for all registrants.</param>
        /// <param name="attributeDefinitions">The attribute definitions to add to.</param>
        /// <returns>The registrant panel.</returns>
        private RegistrantPanelBag GetRegistrantPanel( Registration registration, RegistrationRegistrant registrant, List<RegistrationTemplateFormField> fields, RegistrantLookups lookups, Dictionary<Guid, PublicAttributeBag> attributeDefinitions )
        {
            var template = registration.RegistrationInstance.RegistrationTemplate;
            var person = registrant.PersonAlias?.Person;
            var discountPercentage = registration.DiscountPercentage;
            var discountAmount = registration.DiscountAmount;
            var discountedTotalCost = registrant.DiscountedCost( discountPercentage, discountAmount );

            var panel = new RegistrantPanelBag
            {
                IdKey = registrant.IdKey,
                PersonName = person?.FullName ?? string.Empty,
                FirstName = person?.NickName ?? string.Empty,
                PersonProfileUrl = person != null ? RequestContext.ResolveRockUrl( $"~/Person/{person.IdKey}" ) : null,
                IsOnWaitList = registrant.OnWaitList,
                CostLabelText = discountedTotalCost != 0.0m ? discountedTotalCost.FormatAsCurrency() : null,
                Fields = GetRegistrantFieldValues( registrant, fields, lookups, attributeDefinitions ),
                Fees = GetRegistrantFeeRows( registrant, discountPercentage )
            };

            /*
                9/18/26 - MSE

                When the group a registrant was placed in has been archived, do not show
                the group on the registrant at all. The Group Member Detail block no longer
                displays members of archived groups, so linking to one is not useful.

                Reason: Registrant's group is hidden once the group has been archived. (Fixes #7047)
            */
            lookups.GroupMemberInfoById.TryGetValue( registrant.GroupMemberId ?? 0, out var groupMemberInfo );

            var isGroupRowAllowed = template.GroupTypeId.HasValue
                && registration.Group != null
                && registration.Group.GroupTypeId == template.GroupTypeId.Value;
            var isRegistrantGroupHidden = registrant.GroupMemberId.HasValue && groupMemberInfo?.IsGroupArchived == true;

            panel.IsGroupVisible = isGroupRowAllowed && !isRegistrantGroupHidden;

            if ( registrant.GroupMemberId.HasValue )
            {
                panel.GroupMemberIdKey = IdHasher.Instance.GetHash( registrant.GroupMemberId.Value );
                panel.GroupName = groupMemberInfo?.GroupName.IsNotNullOrWhiteSpace() == true ? groupMemberInfo.GroupName : "Group";
                panel.IsGroupMemberArchived = groupMemberInfo?.IsGroupMemberArchived == true;
            }

            if ( registrant.Cost > 0.0m )
            {
                if ( registrant.OnWaitList )
                {
                    panel.CostText = $"{registrant.Cost.FormatAsCurrency()} (Waiting List)";
                }
                else
                {
                    var discountedCost = registrant.Cost - ( registrant.DiscountApplies ? registrant.Cost * discountPercentage : 0.0m );
                    discountedCost -= registrant.DiscountApplies ? discountAmount : 0.0m;
                    discountedCost = Math.Max( 0.0m, discountedCost );

                    panel.CostText = registrant.Cost.FormatAsCurrency();
                    panel.DiscountedCostText = discountedCost != registrant.Cost ? discountedCost.FormatAsCurrency() : null;
                }
            }

            if ( template.RequiredSignatureDocumentTemplate != null )
            {
                panel.IsSignatureDocumentVisible = true;
                panel.SignatureDocumentTemplateName = template.RequiredSignatureDocumentTemplate.Name;

                if ( registrant.SignatureDocumentId.HasValue && lookups.SignatureDocumentsById.TryGetValue( registrant.SignatureDocumentId.Value, out var document ) )
                {
                    panel.IsSignatureDocumentSigned = true;
                    panel.SignatureDocumentUrl = FileUrlHelper.GetFileUrl( document.BinaryFileId );
                    panel.SignedOnText = document.SignedDateTime?.ToString( "dddd, MMMM d, yyyy" );
                    panel.SignedByName = document.SignedName;
                }
            }

            return panel;
        }

        /// <summary>
        /// Gets the fee rows of a registrant. Fees whose template fee was removed
        /// or that have no quantity are not shown.
        /// </summary>
        /// <param name="registrant">The registrant with its fees loaded.</param>
        /// <param name="discountPercentage">The discount percentage of the registration.</param>
        /// <returns>The fee rows.</returns>
        private static List<RegistrantFeeRowBag> GetRegistrantFeeRows( RegistrationRegistrant registrant, decimal discountPercentage )
        {
            var rows = new List<RegistrantFeeRowBag>();

            foreach ( var fee in registrant.Fees.OrderBy( f => f.Id ) )
            {
                if ( fee.RegistrationTemplateFee == null || fee.Quantity <= 0 )
                {
                    continue;
                }

                var text = fee.Quantity > 1
                    ? $"({fee.Quantity:N0} @ {fee.Cost.FormatAsCurrency()}) {fee.TotalCost.FormatAsCurrency()}"
                    : fee.TotalCost.FormatAsCurrency();

                var row = new RegistrantFeeRowBag
                {
                    Label = GetFeeName( fee )
                };

                if ( registrant.OnWaitList )
                {
                    row.Text = $"{text} (Waiting List)";
                }
                else
                {
                    var discountedCost = registrant.DiscountApplies ? fee.DiscountedCost( discountPercentage ) : fee.TotalCost;

                    row.Text = text;
                    row.DiscountedText = discountedCost != fee.TotalCost ? discountedCost.FormatAsCurrency() : null;
                }

                rows.Add( row );
            }

            return rows;
        }

        /// <summary>
        /// Gets the form field values of a registrant. First and last name are
        /// shown in the panel heading, and empty values and attributes the
        /// current person cannot view are skipped.
        /// </summary>
        /// <param name="registrant">The registrant.</param>
        /// <param name="fields">The form fields of the template, in display order.</param>
        /// <param name="lookups">The data loaded for all registrants.</param>
        /// <param name="attributeDefinitions">The attribute definitions to add to.</param>
        /// <returns>The field values.</returns>
        private List<RegistrantFieldValueBag> GetRegistrantFieldValues( RegistrationRegistrant registrant, List<RegistrationTemplateFormField> fields, RegistrantLookups lookups, Dictionary<Guid, PublicAttributeBag> attributeDefinitions )
        {
            var values = new List<RegistrantFieldValueBag>();
            var person = registrant.PersonAlias?.Person;

            foreach ( var field in fields )
            {
                if ( field.FieldSource == RegistrationFieldSource.PersonField )
                {
                    if ( person == null
                        || field.PersonFieldType == RegistrationPersonFieldType.FirstName
                        || field.PersonFieldType == RegistrationPersonFieldType.LastName )
                    {
                        continue;
                    }

                    var text = GetPersonFieldText( person, field.PersonFieldType, lookups );

                    if ( text.IsNotNullOrWhiteSpace() )
                    {
                        values.Add( new RegistrantFieldValueBag
                        {
                            Label = field.PersonFieldType.ConvertToString( true ),
                            TextValue = text
                        } );
                    }

                    continue;
                }

                if ( !field.AttributeId.HasValue || !lookups.ViewableAttributesById.TryGetValue( field.AttributeId.Value, out var attribute ) )
                {
                    continue;
                }

                var value = GetAttributeFieldValue( registrant, person, field.FieldSource, attribute, lookups );

                if ( value.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                var textValue = attribute.FieldType.Field?.GetTextValue( value, attribute.ConfigurationValues );

                if ( textValue.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                if ( !attributeDefinitions.ContainsKey( attribute.Guid ) )
                {
                    attributeDefinitions[attribute.Guid] = PublicAttributeHelper.GetPublicAttributeForView( attribute, null );
                }

                var publicValue = PublicAttributeHelper.GetPublicValueForView( attribute, value );

                // Registrants enter these values on a public form, so markup is
                // cleaned before it can be rendered as HTML.
                if ( attribute.FieldType.Field is Rock.Field.Types.HtmlFieldType )
                {
                    publicValue = publicValue.SanitizeHtml( false );
                }

                values.Add( new RegistrantFieldValueBag
                {
                    Label = attribute.Name,
                    AttributeGuid = attribute.Guid,
                    AttributeValue = publicValue
                } );
            }

            return values;
        }

        /// <summary>
        /// Gets the plain text of a person field of a registrant.
        /// </summary>
        /// <param name="person">The person of the registrant.</param>
        /// <param name="personFieldType">The person field type.</param>
        /// <param name="lookups">The data loaded for all registrants.</param>
        /// <returns>The text, or <c>null</c> when there is no value.</returns>
        private static string GetPersonFieldText( Person person, RegistrationPersonFieldType personFieldType, RegistrantLookups lookups )
        {
            switch ( personFieldType )
            {
                case RegistrationPersonFieldType.MiddleName:
                    return person.MiddleName;

                case RegistrationPersonFieldType.Campus:
                    return person.PrimaryCampusId.HasValue ? CampusCache.Get( person.PrimaryCampusId.Value )?.Name : null;

                case RegistrationPersonFieldType.Address:
                    return person.PrimaryFamilyId.HasValue ? lookups.HomeAddressByFamilyId.GetValueOrNull( person.PrimaryFamilyId.Value ) : null;

                case RegistrationPersonFieldType.Email:
                    return person.Email;

                case RegistrationPersonFieldType.Birthdate:
                    return person.BirthDate?.ToShortDateString();

                case RegistrationPersonFieldType.Grade:
                    return person.GraduationYear.HasValue ? Person.GradeFormattedFromGraduationYear( person.GraduationYear ) : null;

                case RegistrationPersonFieldType.Gender:
                    return person.Gender.ConvertToString();

                case RegistrationPersonFieldType.MaritalStatus:
                    return GetDefinedValueText( person.MaritalStatusValueId );

                case RegistrationPersonFieldType.AnniversaryDate:
                    return person.AnniversaryDate?.ToShortDateString();

                case RegistrationPersonFieldType.MobilePhone:
                case RegistrationPersonFieldType.HomePhone:
                case RegistrationPersonFieldType.WorkPhone:
                    return lookups.GetPhoneNumber( person.Id, personFieldType );

                case RegistrationPersonFieldType.ConnectionStatus:
                    return GetDefinedValueText( person.ConnectionStatusValueId );

                case RegistrationPersonFieldType.Race:
                    return GetDefinedValueText( person.RaceValueId );

                case RegistrationPersonFieldType.Ethnicity:
                    return GetDefinedValueText( person.EthnicityValueId );

                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the value of a defined value.
        /// </summary>
        /// <param name="definedValueId">The defined value identifier.</param>
        /// <returns>The value, or <c>null</c> when there is none.</returns>
        private static string GetDefinedValueText( int? definedValueId )
        {
            return definedValueId.HasValue ? DefinedValueCache.Get( definedValueId.Value )?.Value : null;
        }

        /// <summary>
        /// Gets the raw value of an attribute form field from the entity the
        /// field reads from.
        /// </summary>
        /// <param name="registrant">The registrant.</param>
        /// <param name="person">The person of the registrant.</param>
        /// <param name="fieldSource">The source of the field.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="lookups">The data loaded for all registrants.</param>
        /// <returns>The raw value, or <c>null</c> when there is none.</returns>
        private static string GetAttributeFieldValue( RegistrationRegistrant registrant, Person person, RegistrationFieldSource fieldSource, AttributeCache attribute, RegistrantLookups lookups )
        {
            switch ( fieldSource )
            {
                case RegistrationFieldSource.PersonAttribute:
                    return person?.GetAttributeValue( attribute.Key );

                case RegistrationFieldSource.GroupMemberAttribute:
                    return registrant.GroupMemberId.HasValue && lookups.GroupMembersById.TryGetValue( registrant.GroupMemberId.Value, out var groupMember )
                        ? groupMember.GetAttributeValue( attribute.Key )
                        : null;

                case RegistrationFieldSource.RegistrantAttribute:
                    return registrant.GetAttributeValue( attribute.Key );

                default:
                    return null;
            }
        }

        /// <summary>
        /// Loads the group name and archive state of each group member the
        /// registrants were placed as, including archived ones.
        /// </summary>
        /// <param name="lookups">The lookups to fill.</param>
        private void LoadGroupMemberInfo( RegistrantLookups lookups )
        {
            var groupMemberIds = lookups.Registrants
                .Where( r => r.GroupMemberId.HasValue )
                .Select( r => r.GroupMemberId.Value )
                .Distinct()
                .ToList();

            if ( !groupMemberIds.Any() )
            {
                return;
            }

            lookups.GroupMemberInfoById = new GroupMemberService( RockContext )
                .AsNoFilter()
                .Where( gm => groupMemberIds.Contains( gm.Id ) )
                .Select( gm => new
                {
                    gm.Id,
                    GroupName = gm.Group.Name,
                    gm.IsArchived,
                    IsGroupArchived = gm.Group.IsArchived
                } )
                .ToList()
                .ToDictionary( gm => gm.Id, gm => new GroupMemberInfo
                {
                    Id = gm.Id,
                    GroupName = gm.GroupName,
                    IsGroupMemberArchived = gm.IsArchived,
                    IsGroupArchived = gm.IsGroupArchived
                } );
        }

        /// <summary>
        /// Loads the home addresses and phone numbers needed by the person
        /// fields of the template, only when the template asks for them.
        /// </summary>
        /// <param name="lookups">The lookups to fill.</param>
        private void LoadPersonFieldValues( RegistrantLookups lookups )
        {
            var personFieldTypes = lookups.Fields
                .Where( f => f.FieldSource == RegistrationFieldSource.PersonField )
                .Select( f => f.PersonFieldType )
                .ToHashSet();

            if ( personFieldTypes.Contains( RegistrationPersonFieldType.Address ) )
            {
                LoadHomeAddresses( lookups );
            }

            var phoneTypeIds = new List<int>();

            foreach ( var phoneField in RegistrantLookups.PhoneTypeGuidByFieldType )
            {
                var phoneTypeId = DefinedValueCache.Get( phoneField.Value.AsGuid() )?.Id;

                if ( personFieldTypes.Contains( phoneField.Key ) && phoneTypeId.HasValue )
                {
                    phoneTypeIds.Add( phoneTypeId.Value );
                }
            }

            if ( phoneTypeIds.Any() && lookups.PersonIds.Any() )
            {
                var personIds = lookups.PersonIds;

                lookups.PhoneNumbers = new PhoneNumberService( RockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( n => personIds.Contains( n.PersonId )
                        && n.NumberTypeValueId.HasValue
                        && phoneTypeIds.Contains( n.NumberTypeValueId.Value ) )
                    .OrderBy( n => n.Id )
                    .Select( n => new
                    {
                        n.PersonId,
                        NumberTypeValueId = n.NumberTypeValueId.Value,
                        n.NumberFormatted
                    } )
                    .ToList()
                    .Select( n => new PhoneNumberInfo
                    {
                        PersonId = n.PersonId,
                        NumberTypeValueId = n.NumberTypeValueId,
                        NumberFormatted = n.NumberFormatted
                    } )
                    .ToList();
            }
        }

        /// <summary>
        /// Loads the home address of each primary family of the registrants,
        /// preferring a mapped location and then the most recent one.
        /// </summary>
        /// <param name="lookups">The lookups to fill.</param>
        private void LoadHomeAddresses( RegistrantLookups lookups )
        {
            var homeLocationTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() )?.Id;
            var familyIds = lookups.Persons
                .Where( p => p.PrimaryFamilyId.HasValue )
                .Select( p => p.PrimaryFamilyId.Value )
                .Distinct()
                .ToList();

            if ( !homeLocationTypeId.HasValue || !familyIds.Any() )
            {
                return;
            }

            var locations = new GroupLocationService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gl => familyIds.Contains( gl.GroupId ) && gl.GroupLocationTypeValueId == homeLocationTypeId.Value )
                .OrderByDescending( gl => gl.IsMappedLocation )
                .ThenByDescending( gl => gl.Id )
                .Select( gl => new
                {
                    gl.GroupId,
                    gl.Location.Name,
                    gl.Location.Street1,
                    gl.Location.Street2,
                    gl.Location.City,
                    gl.Location.State,
                    gl.Location.PostalCode,
                    gl.Location.Country
                } )
                .ToList();

            foreach ( var location in locations )
            {
                if ( lookups.HomeAddressByFamilyId.ContainsKey( location.GroupId ) )
                {
                    continue;
                }

                var address = new Location
                {
                    Name = location.Name,
                    Street1 = location.Street1,
                    Street2 = location.Street2,
                    City = location.City,
                    State = location.State,
                    PostalCode = location.PostalCode,
                    Country = location.Country
                };

                lookups.HomeAddressByFamilyId[location.GroupId] = address.ToString();
            }
        }

        /// <summary>
        /// Loads the attribute values needed by the attribute fields of the
        /// template, once per source, and the attributes the current person
        /// can view.
        /// </summary>
        /// <param name="lookups">The lookups to fill.</param>
        private void LoadAttributeValues( RegistrantLookups lookups )
        {
            var attributeFields = lookups.Fields
                .Where( f => f.FieldSource != RegistrationFieldSource.PersonField && f.AttributeId.HasValue )
                .ToList();

            foreach ( var attributeId in attributeFields.Select( f => f.AttributeId.Value ).Distinct() )
            {
                var attribute = AttributeCache.Get( attributeId );

                if ( attribute != null && attribute.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    lookups.ViewableAttributesById[attributeId] = attribute;
                }
            }

            HashSet<int> GetAttributeIds( RegistrationFieldSource fieldSource )
            {
                return attributeFields
                    .Where( f => f.FieldSource == fieldSource && lookups.ViewableAttributesById.ContainsKey( f.AttributeId.Value ) )
                    .Select( f => f.AttributeId.Value )
                    .ToHashSet();
            }

            var registrantAttributeIds = GetAttributeIds( RegistrationFieldSource.RegistrantAttribute );

            if ( registrantAttributeIds.Any() && lookups.Registrants.Any() )
            {
                lookups.Registrants.LoadFilteredAttributes( RockContext, a => registrantAttributeIds.Contains( a.Id ) );
            }

            var personAttributeIds = GetAttributeIds( RegistrationFieldSource.PersonAttribute );

            if ( personAttributeIds.Any() && lookups.Persons.Any() )
            {
                lookups.Persons.LoadFilteredAttributes( RockContext, a => personAttributeIds.Contains( a.Id ) );
            }

            var groupMemberAttributeIds = GetAttributeIds( RegistrationFieldSource.GroupMemberAttribute );
            var groupMemberIds = lookups.Registrants
                .Where( r => r.GroupMemberId.HasValue )
                .Select( r => r.GroupMemberId.Value )
                .Distinct()
                .ToList();

            if ( groupMemberAttributeIds.Any() && groupMemberIds.Any() )
            {
                var groupMembers = new GroupMemberService( RockContext )
                    .Queryable()
                    .Where( gm => groupMemberIds.Contains( gm.Id ) )
                    .ToList();

                groupMembers.LoadFilteredAttributes( RockContext, a => groupMemberAttributeIds.Contains( a.Id ) );

                lookups.GroupMembersById = groupMembers.ToDictionary( gm => gm.Id );
            }
        }

        /// <summary>
        /// Loads the signed documents of the registrants for the required
        /// signature document template.
        /// </summary>
        /// <param name="lookups">The lookups to fill.</param>
        /// <param name="template">The registration template.</param>
        private void LoadSignatureDocuments( RegistrantLookups lookups, RegistrationTemplate template )
        {
            if ( !template.RequiredSignatureDocumentTemplateId.HasValue )
            {
                return;
            }

            var documentTemplateId = template.RequiredSignatureDocumentTemplateId.Value;
            var documentIds = lookups.Registrants
                .Where( r => r.SignatureDocumentId.HasValue )
                .Select( r => r.SignatureDocumentId.Value )
                .Distinct()
                .ToList();

            if ( !documentIds.Any() )
            {
                return;
            }

            lookups.SignatureDocumentsById = new SignatureDocumentService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( d => documentIds.Contains( d.Id )
                    && d.SignatureDocumentTemplateId == documentTemplateId
                    && d.BinaryFileId.HasValue )
                .Select( d => new
                {
                    d.Id,
                    BinaryFileId = d.BinaryFileId.Value,
                    d.SignedDateTime,
                    d.SignedName
                } )
                .ToList()
                .ToDictionary( d => d.Id, d => new SignatureDocumentInfo
                {
                    Id = d.Id,
                    BinaryFileId = d.BinaryFileId,
                    SignedDateTime = d.SignedDateTime,
                    SignedName = d.SignedName
                } );
        }

        #endregion Private Helpers

        #region Support Classes

        /// <summary>
        /// The money amounts of a registration.
        /// </summary>
        private sealed class RegistrationAmounts
        {
            /// <summary>
            /// Gets or sets the registration these amounts belong to.
            /// </summary>
            public int RegistrationId { get; set; }

            /// <summary>
            /// Gets or sets the total cost before discounts, excluding the wait list.
            /// </summary>
            public decimal TotalCost { get; set; }

            /// <summary>
            /// Gets or sets the total cost after discounts.
            /// </summary>
            public decimal DiscountedCost { get; set; }

            /// <summary>
            /// Gets or sets the total amount paid.
            /// </summary>
            public decimal TotalPaid { get; set; }

            /// <summary>
            /// Gets or sets the balance due, rounded to the currency precision.
            /// </summary>
            public decimal BalanceDue { get; set; }
        }

        /// <summary>
        /// The identifier, unique identifier and name of a group.
        /// </summary>
        private sealed class GroupSummary
        {
            /// <summary>
            /// Gets or sets the group identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the group unique identifier.
            /// </summary>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the group name.
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// The group details of a group member a registrant was placed as.
        /// </summary>
        private sealed class GroupMemberInfo
        {
            /// <summary>
            /// Gets or sets the group member identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the group name.
            /// </summary>
            public string GroupName { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the group member is archived.
            /// </summary>
            public bool IsGroupMemberArchived { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the group is archived.
            /// </summary>
            public bool IsGroupArchived { get; set; }
        }

        /// <summary>
        /// A phone number of a registrant.
        /// </summary>
        private sealed class PhoneNumberInfo
        {
            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the phone type defined value identifier.
            /// </summary>
            public int NumberTypeValueId { get; set; }

            /// <summary>
            /// Gets or sets the formatted number.
            /// </summary>
            public string NumberFormatted { get; set; }
        }

        /// <summary>
        /// A signed document of a registrant.
        /// </summary>
        private sealed class SignatureDocumentInfo
        {
            /// <summary>
            /// Gets or sets the signature document identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the binary file identifier of the signed document.
            /// </summary>
            public int BinaryFileId { get; set; }

            /// <summary>
            /// Gets or sets the date and time the document was signed.
            /// </summary>
            public DateTime? SignedDateTime { get; set; }

            /// <summary>
            /// Gets or sets the name of the person who signed.
            /// </summary>
            public string SignedName { get; set; }
        }

        /// <summary>
        /// The data loaded once for all registrants of a registration.
        /// </summary>
        private sealed class RegistrantLookups
        {
            /// <summary>
            /// The phone type defined value of each phone person field.
            /// </summary>
            public static readonly Dictionary<RegistrationPersonFieldType, string> PhoneTypeGuidByFieldType = new Dictionary<RegistrationPersonFieldType, string>
            {
                [RegistrationPersonFieldType.MobilePhone] = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE,
                [RegistrationPersonFieldType.HomePhone] = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME,
                [RegistrationPersonFieldType.WorkPhone] = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK
            };

            /// <summary>
            /// Initializes a new instance of the <see cref="RegistrantLookups"/> class.
            /// </summary>
            /// <param name="registration">The registration.</param>
            /// <param name="registrants">The registrants in display order.</param>
            /// <param name="fields">The form fields of the template in display order.</param>
            public RegistrantLookups( Registration registration, List<RegistrationRegistrant> registrants, List<RegistrationTemplateFormField> fields )
            {
                Registration = registration;
                Registrants = registrants;
                Fields = fields;
                Persons = registrants
                    .Where( r => r.PersonAlias?.Person != null )
                    .Select( r => r.PersonAlias.Person )
                    .GroupBy( p => p.Id )
                    .Select( g => g.First() )
                    .ToList();
                PersonIds = Persons.Select( p => p.Id ).ToList();
            }

            /// <summary>
            /// Gets the registration.
            /// </summary>
            public Registration Registration { get; }

            /// <summary>
            /// Gets the registrants in display order.
            /// </summary>
            public List<RegistrationRegistrant> Registrants { get; }

            /// <summary>
            /// Gets the form fields of the template in display order.
            /// </summary>
            public List<RegistrationTemplateFormField> Fields { get; }

            /// <summary>
            /// Gets the distinct people of the registrants.
            /// </summary>
            public List<Person> Persons { get; }

            /// <summary>
            /// Gets the distinct person identifiers of the registrants.
            /// </summary>
            public List<int> PersonIds { get; }

            /// <summary>
            /// Gets or sets the group details keyed by group member identifier.
            /// </summary>
            public Dictionary<int, GroupMemberInfo> GroupMemberInfoById { get; set; } = new Dictionary<int, GroupMemberInfo>();

            /// <summary>
            /// Gets or sets the group members with attributes, keyed by identifier.
            /// </summary>
            public Dictionary<int, GroupMember> GroupMembersById { get; set; } = new Dictionary<int, GroupMember>();

            /// <summary>
            /// Gets the formatted home address keyed by family identifier.
            /// </summary>
            public Dictionary<int, string> HomeAddressByFamilyId { get; } = new Dictionary<int, string>();

            /// <summary>
            /// Gets or sets the phone numbers of the registrants, oldest first.
            /// </summary>
            public List<PhoneNumberInfo> PhoneNumbers { get; set; } = new List<PhoneNumberInfo>();

            /// <summary>
            /// Gets the attributes the current person can view, keyed by identifier.
            /// </summary>
            public Dictionary<int, AttributeCache> ViewableAttributesById { get; } = new Dictionary<int, AttributeCache>();

            /// <summary>
            /// Gets or sets the signed documents keyed by signature document identifier.
            /// </summary>
            public Dictionary<int, SignatureDocumentInfo> SignatureDocumentsById { get; set; } = new Dictionary<int, SignatureDocumentInfo>();

            /// <summary>
            /// Gets the formatted phone number of a person for a phone field.
            /// </summary>
            /// <param name="personId">The person identifier.</param>
            /// <param name="personFieldType">The phone person field type.</param>
            /// <returns>The formatted number, or <c>null</c> when there is none.</returns>
            public string GetPhoneNumber( int personId, RegistrationPersonFieldType personFieldType )
            {
                var phoneTypeId = DefinedValueCache.Get( PhoneTypeGuidByFieldType[personFieldType].AsGuid() )?.Id;

                return PhoneNumbers
                    .FirstOrDefault( n => n.PersonId == personId && n.NumberTypeValueId == phoneTypeId )
                    ?.NumberFormatted;
            }
        }

        #endregion Support Classes
    }
}
