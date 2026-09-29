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
using System.Reflection;

using Microsoft.Extensions.Logging;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Enums.Reporting;
using Rock.Field;
using Rock.Financial;
using Rock.Model;
using Rock.Reporting;
using Rock.Security;
using Rock.Security.SecurityGrantRules;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Event.RegistrationTemplateDetail;
using Rock.ViewModels.Reporting;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays the details of a registration template for viewing and editing.
    /// </summary>
    [DisplayName( "Registration Template Detail" )]
    [Category( "Event" )]
    [Description( "Displays the details of the given registration template." )]
    [IconCssClass( "ti ti-clipboard" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Group Placement Page",
        Key = AttributeKey.GroupPlacementPage,
        DefaultValue = Rock.SystemGuid.Page.GROUP_PLACEMENT + "," + Rock.SystemGuid.PageRoute.GROUP_PLACEMENT,
        Description = "The page used for performing group placements.",
        Order = 0 )]

    [CodeEditorField(
        "Default Confirmation Email",
        Key = AttributeKey.DefaultConfirmationEmail,
        Description = "The default Confirmation Email Template value to use for a new template",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 300,
        IsRequired = false,
        Order = 1,
        DefaultValue = RegistrationTemplateDefaults.ConfirmationEmail )]

    [CodeEditorField(
        "Default Reminder Email",
        Key = AttributeKey.DefaultReminderEmail,
        Description = "The default Reminder Email Template value to use for a new template",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 300,
        IsRequired = false,
        Order = 2,
        DefaultValue = RegistrationTemplateDefaults.ReminderEmail )]

    [CodeEditorField(
        "Default Success Text",
        Key = AttributeKey.DefaultSuccessText,
        Description = "The success text default to use for a new template",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 300,
        IsRequired = false,
        Order = 3,
        DefaultValue = RegistrationTemplateDefaults.SuccessText )]

    [CodeEditorField(
        "Default Payment Reminder Email",
        Key = AttributeKey.DefaultPaymentReminderEmail,
        Description = "The default Payment Reminder Email Template value to use for a new template",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 300,
        IsRequired = false,
        Order = 4,
        DefaultValue = RegistrationTemplateDefaults.PaymentReminderEmail )]

    [CodeEditorField(
        "Default Wait List Transition Email",
        Key = AttributeKey.DefaultWaitListTransitionEmail,
        Description = "The default Wait List Transition Email Template value to use for a new template",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 300,
        IsRequired = false,
        Order = 5,
        DefaultValue = RegistrationTemplateDefaults.WaitListTransitionEmail )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Primary )]
    [Rock.SystemGuid.EntityTypeGuid( "3C881EFC-A4D0-4046-AEC3-45B2E90B251B" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "3455B2A3-8756-4F31-8F89-A013053EAF6D" )]
    [Rock.SystemGuid.BlockTypeGuid( "91354899-304E-44C7-BD0D-55F42E6505D3" )]
    public class RegistrationTemplateDetail : RockEntityDetailBlockType<RegistrationTemplate, RegistrationTemplateBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string GroupPlacementPage = "GroupPlacementPage";
            public const string DefaultConfirmationEmail = "DefaultConfirmationEmail";
            public const string DefaultReminderEmail = "DefaultReminderEmail";
            public const string DefaultSuccessText = "DefaultSuccessText";
            public const string DefaultPaymentReminderEmail = "DefaultPaymentReminderEmail";
            public const string DefaultWaitListTransitionEmail = "DefaultWaitListTransitionEmail";
        }

        private static class PageParameterKey
        {
            public const string RegistrationTemplateId = "RegistrationTemplateId";
            public const string ParentCategoryId = "ParentCategoryId";
            public const string CategoryId = "CategoryId";
            public const string RegistrationTemplatePlacementId = "RegistrationTemplatePlacementId";
            public const string ReturnUrl = "ReturnUrl";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Defaults

        /// <summary>
        /// The terms used on the registration screens when the template does not define its own.
        /// </summary>
        private static class DefaultTerm
        {
            public const string Registration = "Registration";
            public const string Registrant = "Person";
            public const string Fee = "Additional Options";
            public const string DiscountCode = "Discount Code";
            public const string RegistrationAttributeTitle = "Registration Information";
        }

        /// <summary>
        /// The Lava values seeded onto a new template.
        /// </summary>
        private static class DefaultLava
        {
            public const string FromName = "{{ RegistrationInstance.ContactPersonAlias.Person.FullName }}";
            public const string FromEmail = "{{ RegistrationInstance.ContactEmail }}";
            public const string ConfirmationSubject = "{{ RegistrationInstance.Name }} Confirmation";
            public const string ReminderSubject = "{{ RegistrationInstance.Name }} Reminder";
            public const string PaymentReminderSubject = "{{ RegistrationInstance.Name }} Payment Reminder";
            public const string WaitListTransitionSubject = "{{ RegistrationInstance.Name }} Wait List Update";
            public const string SuccessTitle = "Congratulations {{ Registration.FirstName }}";
        }

        /// <summary>
        /// The name given to the form that is created when a template has no forms.
        /// </summary>
        private const string DefaultFormName = "Default Form";

        /// <summary>
        /// The attribute qualifier column that scopes registration and registrant
        /// attributes to a single template.
        /// </summary>
        private const string RegistrationTemplateQualifierColumn = "RegistrationTemplateId";

        #endregion Defaults

        #region Fields

        private bool _isTemplateKeyMissing;

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<RegistrationTemplateBag, RegistrationTemplateDetailOptionsBag>();
            var entity = GetInitialEntity();

            SetBoxInitialEntityState( box, entity );

            box.Options = entity != null
                ? GetBoxOptions( entity, includeEditOptions: entity.Id == 0 )
                : new RegistrationTemplateDetailOptionsBag();
            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        /// <summary>
        /// Builds the options used by the view and edit panels.
        /// </summary>
        /// <param name="entity">The template being displayed, used to keep inactive selections available.</param>
        /// <param name="includeEditOptions">When <c>false</c>, only the values needed by the view panel are loaded.</param>
        private RegistrationTemplateDetailOptionsBag GetBoxOptions( RegistrationTemplate entity, bool includeEditOptions )
        {
            var currencyInfo = new RockCurrencyCodeInfo();

            /*
                9/11/2026 - MSE

                Person attributes, signature templates, group types and grades are only
                used by the edit panel. Loading them on view ran Person.LoadAttributes
                and a signature-template query on every template open. Person attributes
                are loaded by GetPersonAttributes when the field modal opens.

                Reason: View does not need any of the edit panel lookups.
            */
            var options = new RegistrationTemplateDetailOptionsBag
            {
                FieldTypes = FieldTypeCache.All()
                    .OrderBy( f => f.Name )
                    .ToListItemBagList(),
                CurrencyInfo = new CurrencyInfoBag
                {
                    Symbol = currencyInfo.Symbol,
                    DecimalPlaces = currencyInfo.DecimalPlaces,
                    SymbolLocation = currencyInfo.SymbolLocation
                }
            };

            if ( !includeEditOptions )
            {
                return options;
            }

            var selectedSignatureDocumentTemplateId = entity.RequiredSignatureDocumentTemplateId;

            var signatureDocumentTemplates = new SignatureDocumentTemplateService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( t => t.IsActive || t.Id == selectedSignatureDocumentTemplateId )
                .OrderBy( t => t.Name )
                .ToList();

            var eligibility = entity.GetRegistrantEligibilitySettingsOrNull();

            options.GroupTypeGuids = GroupTypeCache.All()
                .Where( t => t.ShowInNavigation )
                .Select( t => t.Guid )
                .ToList();
            options.SignatureDocumentTemplates = signatureDocumentTemplates.ToListItemBagList();
            options.LegacySignatureDocumentTemplateGuids = signatureDocumentTemplates
                .Where( t => t.ProviderEntityTypeId.HasValue )
                .Select( t => t.Guid )
                .ToList();
            options.GradeOptions = GetGradeOptions( eligibility?.MinimumGradeOffset, eligibility?.MaximumGradeOffset );

            return options;
        }

        /// <summary>
        /// Adds the asset manager grants required by the HtmlEditor image and
        /// file browsers on top of the standard entity grants.
        /// </summary>
        /// <inheritdoc/>
        protected override SecurityGrant GetSecurityGrant( RegistrationTemplate entity )
        {
            var grant = base.GetSecurityGrant( entity );

            grant.AddRule( new AssetAndFileManagerSecurityGrantRule( Authorization.VIEW ) );
            grant.AddRule( new AssetAndFileManagerSecurityGrantRule( Authorization.EDIT ) );
            grant.AddRule( new AssetAndFileManagerSecurityGrantRule( Authorization.DELETE ) );

            return grant;
        }

        /// <summary>
        /// Populates the box with the initial entity state and prepares it for view or edit.
        /// </summary>
        /// <param name="box">The box to populate.</param>
        /// <param name="entity">The resolved entity, or <c>null</c> when the supplied key did not match any record.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<RegistrationTemplateBag, RegistrationTemplateDetailOptionsBag> box, RegistrationTemplate entity )
        {
            if ( entity == null )
            {
                // A missing page parameter means the page is not scoped to a
                // template yet, so the panel is hidden instead of showing an error.
                if ( !_isTemplateKeyMissing )
                {
                    box.ErrorMessage = $"The specified {RegistrationTemplate.FriendlyTypeName} could not be found.";
                }

                return;
            }

            var isViewable = entity.Id == 0 || entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = IsAuthorizedForEdit( entity );

            if ( entity.Id != 0 )
            {
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( RegistrationTemplate.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( RegistrationTemplate.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <inheritdoc/>
        protected override RegistrationTemplate GetInitialEntity()
        {
            var key = PageParameter( PageParameterKey.RegistrationTemplateId );

            if ( key.IsNullOrWhiteSpace() )
            {
                _isTemplateKeyMissing = true;
                return null;
            }

            // The explicit "0" sentinel requests a new template.
            if ( key == "0" )
            {
                return CreateNewTemplate();
            }

            var templateId = new RegistrationTemplateService( RockContext )
                .GetSelect( key, t => ( int? ) t.Id, !PageCache.Layout.Site.DisablePredictableIds );

            return templateId.HasValue ? GetHydratedEntity( templateId.Value ) : null;
        }

        /// <summary>
        /// Creates an unsaved template seeded with the block's default values and
        /// the parent category from the page parameters.
        /// </summary>
        private RegistrationTemplate CreateNewTemplate()
        {
            return new RegistrationTemplate
            {
                Id = 0,
                IsActive = true,
                CategoryId = GetParentCategory()?.Id,
                ConfirmationFromName = DefaultLava.FromName,
                ConfirmationFromEmail = DefaultLava.FromEmail,
                ConfirmationSubject = DefaultLava.ConfirmationSubject,
                ConfirmationEmailTemplate = GetAttributeValue( AttributeKey.DefaultConfirmationEmail ),
                ReminderFromName = DefaultLava.FromName,
                ReminderFromEmail = DefaultLava.FromEmail,
                ReminderSubject = DefaultLava.ReminderSubject,
                ReminderEmailTemplate = GetAttributeValue( AttributeKey.DefaultReminderEmail ),
                PaymentReminderFromName = DefaultLava.FromName,
                PaymentReminderFromEmail = DefaultLava.FromEmail,
                PaymentReminderSubject = DefaultLava.PaymentReminderSubject,
                PaymentReminderEmailTemplate = GetAttributeValue( AttributeKey.DefaultPaymentReminderEmail ),
                WaitListTransitionFromName = DefaultLava.FromName,
                WaitListTransitionFromEmail = DefaultLava.FromEmail,
                WaitListTransitionSubject = DefaultLava.WaitListTransitionSubject,
                WaitListTransitionEmailTemplate = GetAttributeValue( AttributeKey.DefaultWaitListTransitionEmail ),
                Notify = RegistrationNotify.None,
                SuccessTitle = DefaultLava.SuccessTitle,
                SuccessText = GetAttributeValue( AttributeKey.DefaultSuccessText ),
                AllowMultipleRegistrants = true,
                MaxRegistrants = 10,
                GroupMemberStatus = GroupMemberStatus.Active
            };
        }

        /// <summary>
        /// Gets the category identified by the ParentCategoryId page parameter, if any.
        /// </summary>
        private CategoryCache GetParentCategory()
        {
            var key = PageParameter( PageParameterKey.ParentCategoryId );

            if ( key.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return CategoryCache.Get( key, true );
        }

        /// <summary>
        /// Loads the template with every navigation property and child collection that
        /// the bags need so that no lazy loading happens while they are built.
        /// </summary>
        /// <param name="id">The identifier of the template.</param>
        private RegistrationTemplate GetHydratedEntity( int id )
        {
            return new RegistrationTemplateService( RockContext ).Queryable()
                .Include( t => t.Category )
                .Include( t => t.GroupType )
                .Include( t => t.FinancialGateway )
                .Include( t => t.RegistrationWorkflowType )
                .Include( t => t.RegistrantWorkflowType )
                .Include( t => t.RequiredSignatureDocumentTemplate )
                .Include( t => t.ConnectionStatusValue )
                .Include( t => t.RegistrantRecordSourceValue )
                .Include( t => t.Forms.Select( f => f.Fields ) )
                .Include( t => t.Fees.Select( f => f.FeeItems ) )
                .Include( t => t.Discounts )
                .Include( t => t.Placements )
                .AsNoTracking()
                .FirstOrDefault( t => t.Id == id );
        }

        /// <inheritdoc/>
        protected override RegistrationTemplateBag GetEntityBagForView( RegistrationTemplate entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.HasRegistrations = HasAnyRegistrations( entity.Id );
            bag.GroupPlacements = GetGroupPlacements( entity );

            return bag;
        }

        /// <inheritdoc/>
        protected override RegistrationTemplateBag GetEntityBagForEdit( RegistrationTemplate entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            EnsureDefaultForm( bag.Forms );
            bag.GatewayFeatures = GetGatewayFeatures( entity.FinancialGateway );

            return bag;
        }

        /// <summary>
        /// Builds the bag values that are common to both view and edit modes.
        /// </summary>
        private RegistrationTemplateBag GetCommonEntityBag( RegistrationTemplate entity )
        {
            var category = entity.CategoryId.HasValue ? CategoryCache.Get( entity.CategoryId.Value ) : null;
            var groupMemberRole = entity.GroupMemberRoleId.HasValue ? GroupTypeRoleCache.Get( entity.GroupMemberRoleId.Value ) : null;
            var eligibility = entity.GetRegistrantEligibilitySettingsOrNull() ?? new RegistrationTemplate.RegistrantEligibilitySettings();
            var eligibilityDataView = eligibility.EligibilityDataViewGuid.HasValue ? DataViewCache.Get( eligibility.EligibilityDataViewGuid.Value ) : null;

            return new RegistrationTemplateBag
            {
                IdKey = entity.IdKey,
                Name = entity.Name,
                Description = entity.Description,
                IsActive = entity.IsActive,
                Category = category.ToListItemBag(),
                CategoryName = category?.Name,

                GroupType = entity.GroupType.ToListItemBag(),
                GroupMemberRole = groupMemberRole.ToListItemBag(),
                GroupMemberStatus = entity.GroupMemberStatus,
                ConnectionStatus = entity.ConnectionStatusValue.ToListItemBag(),
                RecordSource = entity.RegistrantRecordSourceValue.ToListItemBag(),

                AllowMultipleRegistrants = entity.AllowMultipleRegistrants,
                MaxRegistrants = entity.MaxRegistrants,
                RegistrantsSameFamily = entity.RegistrantsSameFamily,
                ShowCurrentFamilyMembers = entity.ShowCurrentFamilyMembers,
                WaitListEnabled = entity.WaitListEnabled,
                RegistrarOption = entity.RegistrarOption,
                ShowSmsOptIn = entity.ShowSmsOptIn,
                AreDuplicateRegistrantsPrevented = entity.AreDuplicateRegistrantsPrevented,

                Notify = entity.Notify,
                AddPersonNote = entity.AddPersonNote,
                LoginRequired = entity.LoginRequired,

                IsSetCostOnInstance = entity.SetCostOnInstance ?? false,
                FinancialGateway = entity.FinancialGateway.ToListItemBag(),
                BatchNamePrefix = entity.BatchNamePrefix,
                Cost = entity.Cost,
                MinimumInitialPayment = entity.MinimumInitialPayment,
                DefaultPayment = entity.DefaultPayment,
                IsPaymentPlanAllowed = entity.IsPaymentPlanAllowed,
                PaymentPlanFrequencies = GetSelectedPaymentPlanFrequencies( entity ),
                IsFullPaymentOrPaymentPlanRequired = entity.IsFullPaymentOrPaymentPlanRequired,
                FullPaymentOrPaymentPlanRequiredMessage = entity.FullPaymentOrPaymentPlanRequiredMessage,

                RegistrationWorkflowType = entity.RegistrationWorkflowType.ToListItemBag(),
                RegistrantWorkflowType = entity.RegistrantWorkflowType.ToListItemBag(),
                AllowExternalRegistrationUpdates = entity.AllowExternalRegistrationUpdates,
                RequiredSignatureDocumentTemplate = entity.RequiredSignatureDocumentTemplate.ToListItemBag(),

                EligibilityMinimumAge = eligibility.MinimumAge,
                EligibilityMaximumAge = eligibility.MaximumAge,
                EligibilityAgeClassification = eligibility.AgeClassification,
                EligibilityMinimumGradeOffset = eligibility.MinimumGradeOffset,
                EligibilityMaximumGradeOffset = eligibility.MaximumGradeOffset,
                EligibilityGender = eligibility.Gender,
                EligibilityDataView = eligibilityDataView.ToListItemBag(),

                RegistrationTerm = entity.RegistrationTerm,
                RegistrantTerm = entity.RegistrantTerm,
                FeeTerm = entity.FeeTerm,
                DiscountCodeTerm = entity.DiscountCodeTerm,
                RegistrationAttributeTitleStart = entity.RegistrationAttributeTitleStart,
                RegistrationAttributeTitleEnd = entity.RegistrationAttributeTitleEnd,
                SuccessTitle = entity.SuccessTitle,
                SuccessText = entity.SuccessText,
                RegistrationInstructions = entity.RegistrationInstructions,

                ConfirmationFromName = entity.ConfirmationFromName,
                ConfirmationFromEmail = entity.ConfirmationFromEmail,
                ConfirmationSubject = entity.ConfirmationSubject,
                ConfirmationEmailTemplate = entity.ConfirmationEmailTemplate,
                ReminderFromName = entity.ReminderFromName,
                ReminderFromEmail = entity.ReminderFromEmail,
                ReminderSubject = entity.ReminderSubject,
                ReminderEmailTemplate = entity.ReminderEmailTemplate,
                PaymentReminderFromName = entity.PaymentReminderFromName,
                PaymentReminderFromEmail = entity.PaymentReminderFromEmail,
                PaymentReminderSubject = entity.PaymentReminderSubject,
                PaymentReminderEmailTemplate = entity.PaymentReminderEmailTemplate,
                PaymentReminderTimeSpan = entity.PaymentReminderTimeSpan,
                WaitListTransitionFromName = entity.WaitListTransitionFromName,
                WaitListTransitionFromEmail = entity.WaitListTransitionFromEmail,
                WaitListTransitionSubject = entity.WaitListTransitionSubject,
                WaitListTransitionEmailTemplate = entity.WaitListTransitionEmailTemplate,

                Forms = GetFormBags( entity ),
                RegistrationAttributes = GetRegistrationAttributes( entity.Id ),
                Fees = GetFeeBags( entity ),
                Discounts = GetDiscountBags( entity ),
                Placements = GetPlacementBags( entity ),
                GroupPlacements = new List<RegistrationTemplateGroupPlacementBag>()
            };
        }

        /// <summary>
        /// Gets the payment plan frequencies currently selected on the template.
        /// </summary>
        private static List<ListItemBag> GetSelectedPaymentPlanFrequencies( RegistrationTemplate entity )
        {
            return entity.PaymentPlanFrequencyValueIdsCollection
                .Select( id => DefinedValueCache.Get( id ) )
                .ToListItemBagList();
        }

        /// <summary>
        /// Builds the form bags, including the registrant attribute definitions and
        /// the public representation of each field's visibility rules.
        /// </summary>
        private List<RegistrationTemplateFormBag> GetFormBags( RegistrationTemplate entity )
        {
            var allFields = entity.Forms.SelectMany( f => f.Fields ).ToList();
            var fieldsByGuid = allFields
                .GroupBy( f => f.Guid )
                .ToDictionary( g => g.Key, g => g.First() );
            var registrantAttributes = GetRegistrantAttributesByAttributeId( allFields );

            return entity.Forms
                .OrderBy( f => f.Order )
                .Select( form => new RegistrationTemplateFormBag
                {
                    Guid = form.Guid,
                    Order = form.Order,
                    Name = form.Name,
                    Fields = form.Fields
                        .OrderBy( f => f.Order )
                        .Select( field => GetFormFieldBag( field, registrantAttributes, fieldsByGuid ) )
                        .ToList()
                } )
                .ToList();
        }

        /// <summary>
        /// Builds the bag for a single form field.
        /// </summary>
        /// <param name="field">The field to represent.</param>
        /// <param name="registrantAttributes">The registrant attribute definitions keyed by attribute identifier.</param>
        /// <param name="fieldsByGuid">Every field of the template keyed by unique identifier, used to resolve visibility rules.</param>
        private static RegistrationTemplateFormFieldBag GetFormFieldBag( RegistrationTemplateFormField field, Dictionary<int, PublicEditableAttributeBag> registrantAttributes, Dictionary<Guid, RegistrationTemplateFormField> fieldsByGuid )
        {
            var bag = new RegistrationTemplateFormFieldBag
            {
                Guid = field.Guid,
                Order = field.Order,
                FieldSource = field.FieldSource,
                PersonFieldType = field.PersonFieldType,
                IsInternal = field.IsInternal,
                IsSharedValue = field.IsSharedValue,
                ShowCurrentValue = field.ShowCurrentValue,
                IsRequired = field.IsRequired,
                IsGridField = field.IsGridField,
                ShowOnWaitlist = field.ShowOnWaitlist,
                IsLockedIfValuesExist = field.IsLockedIfValuesExist,
                PreText = field.PreText,
                PostText = field.PostText,
                VisibilityRules = GetPublicVisibilityRules( field.FieldVisibilityRules, fieldsByGuid )
            };

            if ( field.FieldSource == RegistrationFieldSource.PersonField )
            {
                bag.Name = field.PersonFieldType.ConvertToString();
            }
            else if ( field.FieldSource == RegistrationFieldSource.RegistrantAttribute )
            {
                if ( field.AttributeId.HasValue && registrantAttributes.TryGetValue( field.AttributeId.Value, out var registrantAttribute ) )
                {
                    bag.RegistrantAttribute = registrantAttribute;
                    bag.Name = registrantAttribute.Name;
                    bag.FieldTypeName = GetFieldTypeName( registrantAttribute );
                }
            }
            else
            {
                var attribute = field.AttributeId.HasValue ? AttributeCache.Get( field.AttributeId.Value ) : null;

                if ( attribute != null )
                {
                    bag.Attribute = attribute.ToListItemBag();
                    bag.Name = attribute.Name;
                    bag.FieldTypeName = attribute.FieldType?.Name;
                }
            }

            return bag;
        }

        /// <summary>
        /// Gets the display name of the field type used by an editable attribute.
        /// </summary>
        private static string GetFieldTypeName( PublicEditableAttributeBag attribute )
        {
            var fieldTypeGuid = attribute.RealFieldTypeGuid ?? attribute.FieldTypeGuid;

            return fieldTypeGuid.HasValue ? FieldTypeCache.Get( fieldTypeGuid.Value )?.Name : null;
        }

        /// <summary>
        /// Loads the registrant attribute definitions referenced by the registrant
        /// attribute fields of the template in a single query.
        /// </summary>
        /// <param name="fields">Every field of the template.</param>
        /// <returns>The editable attribute bags keyed by attribute identifier.</returns>
        private Dictionary<int, PublicEditableAttributeBag> GetRegistrantAttributesByAttributeId( List<RegistrationTemplateFormField> fields )
        {
            var attributeIds = fields
                .Where( f => f.FieldSource == RegistrationFieldSource.RegistrantAttribute && f.AttributeId.HasValue )
                .Select( f => f.AttributeId.Value )
                .Distinct()
                .ToList();

            if ( !attributeIds.Any() )
            {
                return new Dictionary<int, PublicEditableAttributeBag>();
            }

            return new AttributeService( RockContext ).Queryable()
                .Include( a => a.AttributeQualifiers )
                .Include( a => a.Categories )
                .AsNoTracking()
                .Where( a => attributeIds.Contains( a.Id ) )
                .ToList()
                .ToDictionary( a => a.Id, a => PublicAttributeHelper.GetPublicEditableAttribute( a ) );
        }

        /// <summary>
        /// Loads the attributes collected for each registration of the template.
        /// </summary>
        /// <param name="templateId">The identifier of the template.</param>
        private List<PublicEditableAttributeBag> GetRegistrationAttributes( int templateId )
        {
            if ( templateId == 0 )
            {
                return new List<PublicEditableAttributeBag>();
            }

            var registrationEntityTypeId = EntityTypeCache.Get<Registration>().Id;

            return new AttributeService( RockContext )
                .GetByEntityTypeQualifier( registrationEntityTypeId, RegistrationTemplateQualifierColumn, templateId.ToString(), true )
                .Include( a => a.AttributeQualifiers )
                .Include( a => a.Categories )
                .AsNoTracking()
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList()
                .ConvertAll( a => PublicAttributeHelper.GetPublicEditableAttribute( a ) );
        }

        /// <summary>
        /// Builds the fee bags and flags the fee items that registrants have already used.
        /// </summary>
        private List<RegistrationTemplateFeeBag> GetFeeBags( RegistrationTemplate entity )
        {
            var feeItemIdsInUse = GetFeeItemIdsInUse( entity.Fees.SelectMany( f => f.FeeItems ).Select( i => i.Id ).ToList() );

            return entity.Fees
                .OrderBy( f => f.Order )
                .Select( fee => new RegistrationTemplateFeeBag
                {
                    Guid = fee.Guid,
                    Order = fee.Order,
                    Name = fee.Name,
                    FeeType = fee.FeeType,
                    AllowMultiple = fee.AllowMultiple,
                    DiscountApplies = fee.DiscountApplies,
                    IsActive = fee.IsActive,
                    IsRequired = fee.IsRequired,
                    HideWhenNoneRemaining = fee.HideWhenNoneRemaining,
                    FeeItems = fee.FeeItems
                        .OrderBy( i => i.Order )
                        .Select( item => new RegistrationTemplateFeeItemBag
                        {
                            Guid = item.Guid,
                            Order = item.Order,
                            Name = item.Name,
                            Cost = item.Cost,
                            MaximumUsageCount = item.MaximumUsageCount,
                            IsInUse = feeItemIdsInUse.Contains( item.Id )
                        } )
                        .ToList()
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the identifiers of the fee items that at least one registrant has selected.
        /// </summary>
        /// <param name="feeItemIds">The fee item identifiers to check.</param>
        private HashSet<int> GetFeeItemIdsInUse( List<int> feeItemIds )
        {
            if ( !feeItemIds.Any() )
            {
                return new HashSet<int>();
            }

            return new RegistrationRegistrantFeeService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( rf => rf.RegistrationTemplateFeeItemId.HasValue && feeItemIds.Contains( rf.RegistrationTemplateFeeItemId.Value ) )
                .Select( rf => rf.RegistrationTemplateFeeItemId.Value )
                .Distinct()
                .ToList()
                .ToHashSet();
        }

        /// <summary>
        /// Builds the discount bags.
        /// </summary>
        private static List<RegistrationTemplateDiscountBag> GetDiscountBags( RegistrationTemplate entity )
        {
            return entity.Discounts
                .OrderBy( d => d.Order )
                .Select( discount => new RegistrationTemplateDiscountBag
                {
                    Guid = discount.Guid,
                    Order = discount.Order,
                    Code = discount.Code,
                    DiscountPercentage = discount.DiscountPercentage,
                    DiscountAmount = discount.DiscountAmount,
                    MaxUsage = discount.MaxUsage,
                    MaxRegistrants = discount.MaxRegistrants,
                    MinRegistrants = discount.MinRegistrants,
                    StartDate = discount.StartDate?.ToRockDateTimeOffset(),
                    EndDate = discount.EndDate?.ToRockDateTimeOffset(),
                    AutoApplyDiscount = discount.AutoApplyDiscount
                } )
                .ToList();
        }

        /// <summary>
        /// Builds the placement configuration bags along with their shared groups.
        /// </summary>
        private List<RegistrationTemplatePlacementBag> GetPlacementBags( RegistrationTemplate entity )
        {
            var sharedGroupsByPlacementId = GetSharedGroupsByPlacementId( entity.Placements.Select( p => p.Id ).ToList() );

            return entity.Placements
                .OrderBy( p => p.Order )
                .ThenBy( p => p.Name )
                .Select( placement =>
                {
                    var groupType = GroupTypeCache.Get( placement.GroupTypeId );

                    return new RegistrationTemplatePlacementBag
                    {
                        Guid = placement.Guid,
                        Order = placement.Order,
                        Name = placement.Name,
                        GroupType = groupType.ToListItemBag(),
                        IconCssClass = placement.IconCssClass,
                        AllowMultiplePlacements = placement.AllowMultiplePlacements,
                        SharedGroups = sharedGroupsByPlacementId.TryGetValue( placement.Id, out var sharedGroups )
                            ? sharedGroups
                            : new List<ListItemBag>()
                    };
                } )
                .ToList();
        }

        /// <summary>
        /// Loads the shared placement groups of every placement in a single query.
        /// The groups are stored as related entity records that point from the
        /// placement to the group.
        /// </summary>
        /// <param name="placementIds">The placement identifiers.</param>
        /// <returns>The shared groups keyed by placement identifier.</returns>
        private Dictionary<int, List<ListItemBag>> GetSharedGroupsByPlacementId( List<int> placementIds )
        {
            if ( !placementIds.Any() )
            {
                return new Dictionary<int, List<ListItemBag>>();
            }

            var placementEntityTypeId = EntityTypeCache.Get<RegistrationTemplatePlacement>().Id;
            var groupEntityTypeId = EntityTypeCache.Get<Rock.Model.Group>().Id;
            var purposeKey = RelatedEntityPurposeKey.RegistrationTemplateGroupPlacementTemplate;

            var relatedGroups = new RelatedEntityService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( re => re.SourceEntityTypeId == placementEntityTypeId
                    && re.TargetEntityTypeId == groupEntityTypeId
                    && re.PurposeKey == purposeKey
                    && placementIds.Contains( re.SourceEntityId ) )
                .Join( new GroupService( RockContext ).Queryable().AsNoTracking(),
                    re => re.TargetEntityId,
                    g => g.Id,
                    ( re, g ) => new
                    {
                        PlacementId = re.SourceEntityId,
                        g.Guid,
                        g.Name,
                        g.Order
                    } )
                .ToList();

            return relatedGroups
                .GroupBy( g => g.PlacementId )
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy( x => x.Order )
                        .ThenBy( x => x.Name )
                        .Select( x => new ListItemBag { Value = x.Guid.ToString(), Text = x.Name } )
                        .ToList() );
        }

        /// <summary>
        /// Builds the group placement link list displayed on the view panel.
        /// </summary>
        private List<RegistrationTemplateGroupPlacementBag> GetGroupPlacements( RegistrationTemplate entity )
        {
            var placements = new List<RegistrationTemplateGroupPlacementBag>();

            if ( entity.Id == 0 || GetAttributeValue( AttributeKey.GroupPlacementPage ).IsNullOrWhiteSpace() )
            {
                return placements;
            }

            var currentPageUrl = this.GetCurrentPageUrl();

            foreach ( var placement in entity.Placements.OrderBy( p => p.Order ).ThenBy( p => p.Name ) )
            {
                var url = this.GetLinkedPageUrl( AttributeKey.GroupPlacementPage, new Dictionary<string, string>
                {
                    [PageParameterKey.RegistrationTemplateId] = entity.IdKey,
                    [PageParameterKey.RegistrationTemplatePlacementId] = IdHasher.Instance.GetHash( placement.Id ),
                    [PageParameterKey.ReturnUrl] = currentPageUrl
                } );

                if ( url.IsNotNullOrWhiteSpace() )
                {
                    placements.Add( new RegistrationTemplateGroupPlacementBag
                    {
                        Name = placement.Name,
                        Url = url,
                        IconCssClass = placement.IconCssClass
                    } );
                }
            }

            return placements;
        }

        /// <summary>
        /// Makes sure the edit bag has a default form that collects the first and
        /// last name so that every template can identify its registrants.
        /// </summary>
        /// <param name="forms">The forms of the template.</param>
        private static void EnsureDefaultForm( List<RegistrationTemplateFormBag> forms )
        {
            /*
                9/10/2026 - MSE

                A template cannot function without a form that collects the registrant's
                first and last name, so the first form is seeded with those two person
                fields whenever they are missing. The first form is treated as the default
                form by the edit panel: it cannot be deleted and these two fields cannot be
                removed from it.

                Reason: Guarantee every template collects the fields the registration entry
                process depends on.
            */
            if ( !forms.Any() )
            {
                forms.Add( new RegistrationTemplateFormBag
                {
                    Guid = Guid.NewGuid(),
                    Order = 0,
                    Name = DefaultFormName,
                    Fields = new List<RegistrationTemplateFormFieldBag>()
                } );
            }

            var defaultForm = forms.OrderBy( f => f.Order ).First();
            defaultForm.Fields = defaultForm.Fields ?? new List<RegistrationTemplateFormFieldBag>();

            EnsurePersonField( defaultForm, RegistrationPersonFieldType.FirstName, "<div class='row'><div class='col-md-6'>", "    </div>" );
            EnsurePersonField( defaultForm, RegistrationPersonFieldType.LastName, "    <div class='col-md-6'>", "    </div></div>" );
        }

        /// <summary>
        /// Adds a required person field to the form when it does not already exist.
        /// </summary>
        private static void EnsurePersonField( RegistrationTemplateFormBag form, RegistrationPersonFieldType personFieldType, string preText, string postText )
        {
            var hasField = form.Fields.Any( f => f.FieldSource == RegistrationFieldSource.PersonField && f.PersonFieldType == personFieldType );

            if ( hasField )
            {
                return;
            }

            form.Fields.Add( new RegistrationTemplateFormFieldBag
            {
                Guid = Guid.NewGuid(),
                Order = form.Fields.Any() ? form.Fields.Max( f => f.Order ) + 1 : 0,
                FieldSource = RegistrationFieldSource.PersonField,
                PersonFieldType = personFieldType,
                Name = personFieldType.ConvertToString(),
                IsGridField = true,
                IsRequired = true,
                ShowOnWaitlist = true,
                PreText = preText,
                PostText = postText,
                VisibilityRules = GetPublicVisibilityRules( null, new Dictionary<Guid, RegistrationTemplateFormField>() )
            } );
        }

        /// <summary>
        /// Returns true when any instance of the template has a completed registration.
        /// </summary>
        private bool HasAnyRegistrations( int templateId )
        {
            if ( templateId == 0 )
            {
                return false;
            }

            return new RegistrationInstanceService( RockContext ).Queryable()
                .AsNoTracking()
                .Any( i => i.RegistrationTemplateId == templateId && i.Registrations.Any( r => !r.IsTemporary ) );
        }

        /// <summary>
        /// Converts the attributes the person is allowed to view into selectable items, ordered by name.
        /// </summary>
        private static List<RegistrationTemplateAttributeItemBag> GetAttributeItems( IEnumerable<AttributeCache> attributes, Person currentPerson )
        {
            return attributes
                .Where( a => a.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .OrderBy( a => a.Name )
                .Select( a => new RegistrationTemplateAttributeItemBag
                {
                    Guid = a.Guid,
                    Name = a.Name,
                    Key = a.Key,
                    FieldTypeName = a.FieldType?.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the school grades that can be selected for the eligibility grade range.
        /// Inactive grades are only included when they are currently selected.
        /// </summary>
        private static List<ListItemBag> GetGradeOptions( int? selectedMinimumOffset, int? selectedMaximumOffset )
        {
            var options = new List<ListItemBag>();
            var gradesDefinedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() );

            if ( gradesDefinedType == null )
            {
                return options;
            }

            foreach ( var grade in gradesDefinedType.DefinedValues.OrderBy( dv => dv.Order ) )
            {
                var abbreviation = grade.GetAttributeValue( "Abbreviation" );
                var gradeOffset = grade.Value.AsIntegerOrNull();

                if ( abbreviation.IsNullOrWhiteSpace() || !gradeOffset.HasValue )
                {
                    continue;
                }

                var isSelected = gradeOffset == selectedMinimumOffset || gradeOffset == selectedMaximumOffset;

                if ( grade.IsActive || isSelected )
                {
                    options.Add( new ListItemBag
                    {
                        Value = gradeOffset.Value.ToString(),
                        Text = abbreviation
                    } );
                }
            }

            return options;
        }

        /// <summary>
        /// Gets the navigation URLs used by the frontend. When the page was reached
        /// from a category tree the parent page URL returns to that category so the
        /// tree keeps its selection.
        /// </summary>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var parentCategory = GetParentCategory();

            var parentPageUrl = parentCategory != null
                ? this.GetCurrentPageUrl( new Dictionary<string, string> { [PageParameterKey.CategoryId] = parentCategory.IdKey }, skipExistingParameters: true )
                : this.GetParentPageUrl();

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = parentPageUrl
            };
        }

        /// <summary>
        /// Gets the capabilities of a financial gateway that affect the payment settings.
        /// </summary>
        /// <param name="gateway">The gateway, or <c>null</c> when none is selected.</param>
        private static RegistrationTemplateGatewayFeaturesBag GetGatewayFeatures( FinancialGateway gateway )
        {
            var features = new RegistrationTemplateGatewayFeaturesBag
            {
                PaymentPlanFrequencies = new List<ListItemBag>()
            };

            if ( gateway == null )
            {
                return features;
            }

            var component = gateway.GetGatewayComponent();

            features.IsRedirectionGateway = gateway.IsRedirectionGateway();
            features.IsPaymentPlanSupported = component is IScheduledNumberOfPaymentsGateway;

            if ( features.IsPaymentPlanSupported )
            {
                var oneTimeFrequencyGuid = Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME.AsGuid();

                features.PaymentPlanFrequencies = ( component.SupportedPaymentSchedules ?? new List<DefinedValueCache>() )
                    .Where( f => f.Guid != oneTimeFrequencyGuid )
                    .ToListItemBagList();
            }

            return features;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( RegistrationTemplate entity, ValidPropertiesBox<RegistrationTemplateBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.Category ),
                () => entity.CategoryId = box.Bag.Category.GetEntityId<Category>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.GroupType ),
                () => entity.GroupTypeId = box.Bag.GroupType.GetEntityId<GroupType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.GroupMemberRole ),
                () => entity.GroupMemberRoleId = box.Bag.GroupMemberRole.GetEntityId<GroupTypeRole>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.GroupMemberStatus ),
                () => entity.GroupMemberStatus = box.Bag.GroupMemberStatus );

            box.IfValidProperty( nameof( box.Bag.ConnectionStatus ),
                () => entity.ConnectionStatusValueId = box.Bag.ConnectionStatus.GetEntityId<DefinedValue>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.RecordSource ),
                () => entity.RegistrantRecordSourceValueId = box.Bag.RecordSource.GetEntityId<DefinedValue>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.AllowMultipleRegistrants ),
                () => entity.AllowMultipleRegistrants = box.Bag.AllowMultipleRegistrants );

            box.IfValidProperty( nameof( box.Bag.MaxRegistrants ),
                () => entity.MaxRegistrants = box.Bag.MaxRegistrants );

            // The maximum only applies when more than one registrant is allowed.
            if ( !entity.AllowMultipleRegistrants )
            {
                entity.MaxRegistrants = null;
            }

            box.IfValidProperty( nameof( box.Bag.RegistrantsSameFamily ),
                () => entity.RegistrantsSameFamily = box.Bag.RegistrantsSameFamily );

            box.IfValidProperty( nameof( box.Bag.ShowCurrentFamilyMembers ),
                () => entity.ShowCurrentFamilyMembers = box.Bag.ShowCurrentFamilyMembers );

            box.IfValidProperty( nameof( box.Bag.WaitListEnabled ),
                () => entity.WaitListEnabled = box.Bag.WaitListEnabled );

            box.IfValidProperty( nameof( box.Bag.RegistrarOption ),
                () => entity.RegistrarOption = box.Bag.RegistrarOption );

            box.IfValidProperty( nameof( box.Bag.ShowSmsOptIn ),
                () => entity.ShowSmsOptIn = box.Bag.ShowSmsOptIn );

            box.IfValidProperty( nameof( box.Bag.AreDuplicateRegistrantsPrevented ),
                () => entity.AreDuplicateRegistrantsPrevented = box.Bag.AreDuplicateRegistrantsPrevented );

            box.IfValidProperty( nameof( box.Bag.Notify ),
                () => entity.Notify = box.Bag.Notify );

            box.IfValidProperty( nameof( box.Bag.AddPersonNote ),
                () => entity.AddPersonNote = box.Bag.AddPersonNote );

            box.IfValidProperty( nameof( box.Bag.LoginRequired ),
                () => entity.LoginRequired = box.Bag.LoginRequired );

            box.IfValidProperty( nameof( box.Bag.IsSetCostOnInstance ),
                () => entity.SetCostOnInstance = box.Bag.IsSetCostOnInstance );

            box.IfValidProperty( nameof( box.Bag.FinancialGateway ),
                () => entity.FinancialGatewayId = box.Bag.FinancialGateway.GetEntityId<FinancialGateway>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.Cost ),
                () => entity.Cost = box.Bag.Cost ?? 0.0M );

            box.IfValidProperty( nameof( box.Bag.MinimumInitialPayment ),
                () => entity.MinimumInitialPayment = box.Bag.MinimumInitialPayment );

            box.IfValidProperty( nameof( box.Bag.DefaultPayment ),
                () => entity.DefaultPayment = box.Bag.DefaultPayment );

            UpdatePaymentSettingsFromBox( entity, box );

            box.IfValidProperty( nameof( box.Bag.RegistrationWorkflowType ),
                () => entity.RegistrationWorkflowTypeId = box.Bag.RegistrationWorkflowType.GetEntityId<WorkflowType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.RegistrantWorkflowType ),
                () => entity.RegistrantWorkflowTypeId = box.Bag.RegistrantWorkflowType.GetEntityId<WorkflowType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.AllowExternalRegistrationUpdates ),
                () => entity.AllowExternalRegistrationUpdates = box.Bag.AllowExternalRegistrationUpdates );

            box.IfValidProperty( nameof( box.Bag.RequiredSignatureDocumentTemplate ),
                () => entity.RequiredSignatureDocumentTemplateId = box.Bag.RequiredSignatureDocumentTemplate.GetEntityId<SignatureDocumentTemplate>( RockContext ) );

            UpdateSignatureDocumentSettings( entity );

            box.IfValidProperty( nameof( box.Bag.RegistrationTerm ),
                () => entity.RegistrationTerm = box.Bag.RegistrationTerm.IsNullOrWhiteSpace() ? DefaultTerm.Registration : box.Bag.RegistrationTerm );

            box.IfValidProperty( nameof( box.Bag.RegistrantTerm ),
                () => entity.RegistrantTerm = box.Bag.RegistrantTerm.IsNullOrWhiteSpace() ? DefaultTerm.Registrant : box.Bag.RegistrantTerm );

            box.IfValidProperty( nameof( box.Bag.FeeTerm ),
                () => entity.FeeTerm = box.Bag.FeeTerm.IsNullOrWhiteSpace() ? DefaultTerm.Fee : box.Bag.FeeTerm );

            box.IfValidProperty( nameof( box.Bag.DiscountCodeTerm ),
                () => entity.DiscountCodeTerm = box.Bag.DiscountCodeTerm.IsNullOrWhiteSpace() ? DefaultTerm.DiscountCode : box.Bag.DiscountCodeTerm );

            box.IfValidProperty( nameof( box.Bag.RegistrationAttributeTitleStart ),
                () => entity.RegistrationAttributeTitleStart = box.Bag.RegistrationAttributeTitleStart.IsNullOrWhiteSpace() ? DefaultTerm.RegistrationAttributeTitle : box.Bag.RegistrationAttributeTitleStart );

            box.IfValidProperty( nameof( box.Bag.RegistrationAttributeTitleEnd ),
                () => entity.RegistrationAttributeTitleEnd = box.Bag.RegistrationAttributeTitleEnd.IsNullOrWhiteSpace() ? DefaultTerm.RegistrationAttributeTitle : box.Bag.RegistrationAttributeTitleEnd );

            box.IfValidProperty( nameof( box.Bag.SuccessTitle ),
                () => entity.SuccessTitle = box.Bag.SuccessTitle );

            box.IfValidProperty( nameof( box.Bag.SuccessText ),
                () => entity.SuccessText = box.Bag.SuccessText );

            box.IfValidProperty( nameof( box.Bag.RegistrationInstructions ),
                () => entity.RegistrationInstructions = box.Bag.RegistrationInstructions );

            box.IfValidProperty( nameof( box.Bag.ConfirmationFromName ),
                () => entity.ConfirmationFromName = box.Bag.ConfirmationFromName );

            box.IfValidProperty( nameof( box.Bag.ConfirmationFromEmail ),
                () => entity.ConfirmationFromEmail = box.Bag.ConfirmationFromEmail );

            box.IfValidProperty( nameof( box.Bag.ConfirmationSubject ),
                () => entity.ConfirmationSubject = box.Bag.ConfirmationSubject );

            box.IfValidProperty( nameof( box.Bag.ConfirmationEmailTemplate ),
                () => entity.ConfirmationEmailTemplate = box.Bag.ConfirmationEmailTemplate );

            box.IfValidProperty( nameof( box.Bag.ReminderFromName ),
                () => entity.ReminderFromName = box.Bag.ReminderFromName );

            box.IfValidProperty( nameof( box.Bag.ReminderFromEmail ),
                () => entity.ReminderFromEmail = box.Bag.ReminderFromEmail );

            box.IfValidProperty( nameof( box.Bag.ReminderSubject ),
                () => entity.ReminderSubject = box.Bag.ReminderSubject );

            box.IfValidProperty( nameof( box.Bag.ReminderEmailTemplate ),
                () => entity.ReminderEmailTemplate = box.Bag.ReminderEmailTemplate );

            box.IfValidProperty( nameof( box.Bag.PaymentReminderFromName ),
                () => entity.PaymentReminderFromName = box.Bag.PaymentReminderFromName );

            box.IfValidProperty( nameof( box.Bag.PaymentReminderFromEmail ),
                () => entity.PaymentReminderFromEmail = box.Bag.PaymentReminderFromEmail );

            box.IfValidProperty( nameof( box.Bag.PaymentReminderSubject ),
                () => entity.PaymentReminderSubject = box.Bag.PaymentReminderSubject );

            box.IfValidProperty( nameof( box.Bag.PaymentReminderEmailTemplate ),
                () => entity.PaymentReminderEmailTemplate = box.Bag.PaymentReminderEmailTemplate );

            box.IfValidProperty( nameof( box.Bag.PaymentReminderTimeSpan ),
                () => entity.PaymentReminderTimeSpan = box.Bag.PaymentReminderTimeSpan );

            box.IfValidProperty( nameof( box.Bag.WaitListTransitionFromName ),
                () => entity.WaitListTransitionFromName = box.Bag.WaitListTransitionFromName );

            box.IfValidProperty( nameof( box.Bag.WaitListTransitionFromEmail ),
                () => entity.WaitListTransitionFromEmail = box.Bag.WaitListTransitionFromEmail );

            box.IfValidProperty( nameof( box.Bag.WaitListTransitionSubject ),
                () => entity.WaitListTransitionSubject = box.Bag.WaitListTransitionSubject );

            box.IfValidProperty( nameof( box.Bag.WaitListTransitionEmailTemplate ),
                () => entity.WaitListTransitionEmailTemplate = box.Bag.WaitListTransitionEmailTemplate );

            UpdateEligibilitySettingsFromBox( entity, box );

            return true;
        }

        /// <summary>
        /// Applies the payment settings that depend on the capabilities of the selected
        /// gateway. Batch prefixes are not used by redirection gateways and payment plans
        /// are only kept when the gateway supports scheduled payments.
        /// </summary>
        private void UpdatePaymentSettingsFromBox( RegistrationTemplate entity, ValidPropertiesBox<RegistrationTemplateBag> box )
        {
            var gateway = entity.FinancialGatewayId.HasValue
                ? new FinancialGatewayService( RockContext ).GetNoTracking( entity.FinancialGatewayId.Value )
                : null;
            var features = GetGatewayFeatures( gateway );

            box.IfValidProperty( nameof( box.Bag.BatchNamePrefix ),
                () => entity.BatchNamePrefix = box.Bag.BatchNamePrefix );

            if ( features.IsRedirectionGateway )
            {
                entity.BatchNamePrefix = null;
            }

            if ( features.IsPaymentPlanSupported )
            {
                box.IfValidProperty( nameof( box.Bag.IsPaymentPlanAllowed ),
                    () => entity.IsPaymentPlanAllowed = box.Bag.IsPaymentPlanAllowed );

                box.IfValidProperty( nameof( box.Bag.PaymentPlanFrequencies ),
                    () => entity.PaymentPlanFrequencyValueIdsCollection = GetSupportedPaymentPlanFrequencyIds( box.Bag.PaymentPlanFrequencies, features ) );
            }
            else
            {
                entity.IsPaymentPlanAllowed = false;
                entity.PaymentPlanFrequencyValueIdsCollection = new List<int>();
            }

            if ( entity.IsPaymentPlanAllowed )
            {
                box.IfValidProperty( nameof( box.Bag.IsFullPaymentOrPaymentPlanRequired ),
                    () => entity.IsFullPaymentOrPaymentPlanRequired = box.Bag.IsFullPaymentOrPaymentPlanRequired );
            }
            else
            {
                entity.IsFullPaymentOrPaymentPlanRequired = false;
            }

            if ( entity.IsFullPaymentOrPaymentPlanRequired )
            {
                box.IfValidProperty( nameof( box.Bag.FullPaymentOrPaymentPlanRequiredMessage ),
                    () => entity.FullPaymentOrPaymentPlanRequiredMessage = box.Bag.FullPaymentOrPaymentPlanRequiredMessage.IsNullOrWhiteSpace()
                        ? null
                        : box.Bag.FullPaymentOrPaymentPlanRequiredMessage );
            }
            else
            {
                entity.FullPaymentOrPaymentPlanRequiredMessage = null;
            }
        }

        /// <summary>
        /// Maps the selected payment plan frequencies to defined-value identifiers,
        /// keeping only frequencies the selected gateway actually supports.
        /// </summary>
        /// <param name="selectedFrequencies">The frequencies sent by the client.</param>
        /// <param name="features">The capabilities of the gateway that will be saved.</param>
        /// <returns>The supported frequency identifiers, in the order they were sent.</returns>
        private static List<int> GetSupportedPaymentPlanFrequencyIds( List<ListItemBag> selectedFrequencies, RegistrationTemplateGatewayFeaturesBag features )
        {
            var supportedGuids = ( features.PaymentPlanFrequencies ?? new List<ListItemBag>() )
                .Select( f => f?.Value.AsGuidOrNull() )
                .Where( guid => guid.HasValue )
                .Select( guid => guid.Value )
                .ToHashSet();

            return ( selectedFrequencies ?? new List<ListItemBag>() )
                .Select( f => f?.Value.AsGuidOrNull() )
                .Where( guid => guid.HasValue && supportedGuids.Contains( guid.Value ) )
                .Select( guid => DefinedValueCache.GetId( guid.Value ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Applies the rules tied to the required signature document. Signatures are
        /// always embedded, and registrations that use a non-legacy signature document
        /// cannot be updated externally because that could invalidate the signed document.
        /// </summary>
        private void UpdateSignatureDocumentSettings( RegistrationTemplate entity )
        {
            entity.SignatureDocumentAction = SignatureDocumentAction.Embed;

            if ( !entity.RequiredSignatureDocumentTemplateId.HasValue )
            {
                return;
            }

            var isLegacy = new SignatureDocumentTemplateService( RockContext )
                .GetSelect( entity.RequiredSignatureDocumentTemplateId.Value, t => t.ProviderEntityTypeId )
                .HasValue;

            if ( !isLegacy )
            {
                entity.AllowExternalRegistrationUpdates = false;
            }
        }

        /// <summary>
        /// Applies the registrant eligibility settings from the box.
        /// </summary>
        private static void UpdateEligibilitySettingsFromBox( RegistrationTemplate entity, ValidPropertiesBox<RegistrationTemplateBag> box )
        {
            var eligibility = entity.GetRegistrantEligibilitySettingsOrNull() ?? new RegistrationTemplate.RegistrantEligibilitySettings();

            box.IfValidProperty( nameof( box.Bag.EligibilityMinimumAge ),
                () => eligibility.MinimumAge = box.Bag.EligibilityMinimumAge );

            box.IfValidProperty( nameof( box.Bag.EligibilityMaximumAge ),
                () => eligibility.MaximumAge = box.Bag.EligibilityMaximumAge );

            box.IfValidProperty( nameof( box.Bag.EligibilityAgeClassification ),
                () => eligibility.AgeClassification = box.Bag.EligibilityAgeClassification );

            box.IfValidProperty( nameof( box.Bag.EligibilityMinimumGradeOffset ),
                () => eligibility.MinimumGradeOffset = box.Bag.EligibilityMinimumGradeOffset );

            box.IfValidProperty( nameof( box.Bag.EligibilityMaximumGradeOffset ),
                () => eligibility.MaximumGradeOffset = box.Bag.EligibilityMaximumGradeOffset );

            box.IfValidProperty( nameof( box.Bag.EligibilityGender ),
                () => eligibility.Gender = box.Bag.EligibilityGender );

            box.IfValidProperty( nameof( box.Bag.EligibilityDataView ),
                () => eligibility.EligibilityDataViewGuid = box.Bag.EligibilityDataView?.Value.AsGuidOrNull() );

            entity.SetRegistrantEligibilitySettings( eligibility );
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out RegistrationTemplate entity, out BlockActionResult error )
        {
            var entityService = new RegistrationTemplateService( RockContext );
            error = null;

            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    error = ActionBadRequest( $"{RegistrationTemplate.FriendlyTypeName} not found." );
                    return false;
                }

                if ( !IsAuthorizedForEdit( entity ) )
                {
                    error = ActionForbidden( $"Not authorized to edit {RegistrationTemplate.FriendlyTypeName}." );
                    return false;
                }

                return true;
            }

            /*
                9/10/2026 - MSE

                A new template has no security of its own, it inherits the rights of the
                category it is created in. That category is chosen on the edit form, so it
                is not known here. The save action validates that the person has edit rights
                to the selected category once the box has been applied to the entity.

                Reason: Allow people with category level edit rights to create templates.
            */
            entity = new RegistrationTemplate
            {
                CategoryId = GetParentCategory()?.Id
            };

            entityService.Add( entity );

            return true;
        }

        /// <summary>
        /// Returns true when the current person can edit the template through block
        /// level edit rights or entity level edit rights. A template that has not been
        /// saved inherits the rights of the category it will be created in.
        /// </summary>
        private bool IsAuthorizedForEdit( RegistrationTemplate entity )
        {
            var currentPerson = RequestContext.CurrentPerson;

            if ( BlockCache.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                return true;
            }

            if ( entity == null )
            {
                return false;
            }

            if ( entity.Id == 0 )
            {
                var category = entity.CategoryId.HasValue ? CategoryCache.Get( entity.CategoryId.Value ) : null;

                return category?.IsAuthorized( Authorization.EDIT, currentPerson ) == true;
            }

            return entity.IsAuthorized( Authorization.EDIT, currentPerson );
        }

        /// <summary>
        /// Returns true when the current person can delete or copy the template. In
        /// addition to edit rights, administrate rights on the template are accepted.
        /// </summary>
        private bool IsAuthorizedToDeleteOrCopy( RegistrationTemplate entity )
        {
            if ( IsAuthorizedForEdit( entity ) )
            {
                return true;
            }

            return entity != null && entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var key = pageReference.GetPageParameter( PageParameterKey.RegistrationTemplateId );
            var breadCrumbs = new List<IBreadCrumb>();

            if ( key.IsNullOrWhiteSpace() )
            {
                return new BreadCrumbResult { BreadCrumbs = breadCrumbs };
            }

            if ( key == "0" )
            {
                breadCrumbs.Add( new BreadCrumbLink( "New Registration Template", pageReference ) );
                return new BreadCrumbResult { BreadCrumbs = breadCrumbs };
            }

            var name = new RegistrationTemplateService( RockContext )
                .GetSelect( key, t => t.Name, !PageCache.Layout.Site.DisablePredictableIds );

            if ( name.IsNullOrWhiteSpace() )
            {
                return new BreadCrumbResult { BreadCrumbs = breadCrumbs };
            }

            var pageParameters = new Dictionary<string, string>
            {
                [PageParameterKey.RegistrationTemplateId] = key
            };

            breadCrumbs.Add( new BreadCrumbLink( name, new PageReference( pageReference.PageId, 0, pageParameters ) ) );

            return new BreadCrumbResult { BreadCrumbs = breadCrumbs };
        }

        #endregion Methods

        #region Visibility Rules

        /// <summary>
        /// Converts the stored visibility rules of a field into the public representation
        /// used by the field filter editor. Each rule points at the form field it compares
        /// against and its value is converted with the field type of that field.
        /// </summary>
        /// <param name="rules">The stored rules, or <c>null</c> when the field has none.</param>
        /// <param name="fieldsByGuid">Every field of the template keyed by unique identifier.</param>
        private static FieldFilterGroupBag GetPublicVisibilityRules( FieldVisibilityRules rules, Dictionary<Guid, RegistrationTemplateFormField> fieldsByGuid )
        {
            var group = new FieldFilterGroupBag
            {
                Guid = Guid.NewGuid(),
                ExpressionType = GetGroupExpressionType( rules?.FilterExpressionType ),
                Rules = new List<FieldFilterRuleBag>()
            };

            if ( rules?.RuleList == null )
            {
                return group;
            }

            foreach ( var rule in rules.RuleList.Where( r => r.ComparedToFormFieldGuid.HasValue ) )
            {
                var ruleBag = new FieldFilterRuleBag
                {
                    Guid = rule.Guid,
                    ComparisonType = rule.ComparisonType,
                    Value = rule.ComparedToValue,
                    SourceType = FieldFilterSourceType.Attribute,
                    AttributeGuid = rule.ComparedToFormFieldGuid
                };

                if ( fieldsByGuid.TryGetValue( rule.ComparedToFormFieldGuid.Value, out var comparedToField ) )
                {
                    var fieldType = GetComparableFieldType( comparedToField, out var privateConfigurationValues );

                    ApplyPublicFilterValue( ruleBag, fieldType, privateConfigurationValues, rule.ComparisonType, rule.ComparedToValue );
                }

                group.Rules.Add( ruleBag );
            }

            return group;
        }

        /// <summary>
        /// Converts the public rules edited on the client back into the stored
        /// representation. Values are converted with the field type of the field
        /// each rule compares against.
        /// </summary>
        /// <param name="group">The public rules, or <c>null</c> when the field has none.</param>
        /// <param name="fieldsByGuid">Every incoming field of the template keyed by unique identifier.</param>
        private static FieldVisibilityRules GetPrivateVisibilityRules( FieldFilterGroupBag group, Dictionary<Guid, RegistrationTemplateFormFieldBag> fieldsByGuid )
        {
            var rules = new FieldVisibilityRules();

            if ( group == null )
            {
                return rules;
            }

            rules.FilterExpressionType = GetGroupExpressionType( group.ExpressionType );

            foreach ( var ruleBag in ( group.Rules ?? new List<FieldFilterRuleBag>() ).Where( r => r != null && r.AttributeGuid.HasValue ) )
            {
                var rule = new FieldVisibilityRule
                {
                    Guid = ruleBag.Guid == Guid.Empty ? Guid.NewGuid() : ruleBag.Guid,
                    ComparisonType = ruleBag.ComparisonType,
                    ComparedToFormFieldGuid = ruleBag.AttributeGuid,
                    ComparedToValue = ruleBag.Value
                };

                if ( fieldsByGuid.TryGetValue( ruleBag.AttributeGuid.Value, out var comparedToField ) )
                {
                    var fieldType = GetComparableFieldType( comparedToField, out var privateConfigurationValues );

                    if ( fieldType?.Field != null )
                    {
                        var comparisonValue = new ComparisonValue
                        {
                            ComparisonType = ruleBag.ComparisonType,
                            Value = ruleBag.Value
                        };

                        var filterValues = fieldType.Field
                            .GetPrivateFilterValue( comparisonValue, privateConfigurationValues )
                            .FromJsonOrNull<List<string>>();

                        // Two values means the first is the comparison type and the
                        // second is the value. A single value has no comparison type.
                        if ( filterValues != null && filterValues.Count == 2 )
                        {
                            rule.ComparedToValue = filterValues[1];
                        }
                        else if ( filterValues != null && filterValues.Count == 1 )
                        {
                            rule.ComparedToValue = filterValues[0];
                        }
                    }
                }

                rules.RuleList.Add( rule );
            }

            return rules;
        }

        /// <summary>
        /// Gets an expression type that is valid for a rule group. The filter
        /// expression type only applies to individual rules.
        /// </summary>
        private static FilterExpressionType GetGroupExpressionType( FilterExpressionType? expressionType )
        {
            if ( !expressionType.HasValue || expressionType.Value == FilterExpressionType.Filter )
            {
                return FilterExpressionType.GroupAll;
            }

            return expressionType.Value;
        }

        /// <summary>
        /// Converts a stored filter value into its public representation and applies
        /// it to the rule bag.
        /// </summary>
        private static void ApplyPublicFilterValue( FieldFilterRuleBag ruleBag, FieldTypeCache fieldType, Dictionary<string, string> privateConfigurationValues, ComparisonType comparisonType, string comparedToValue )
        {
            if ( fieldType?.Field == null )
            {
                return;
            }

            var filterValues = new List<string>();
            var comparisonTypeValue = comparisonType.ConvertToString( false );

            if ( comparisonTypeValue != null )
            {
                filterValues.Add( comparisonTypeValue );
            }

            filterValues.Add( comparedToValue );

            var comparisonValue = fieldType.Field.GetPublicFilterValue( filterValues.ToJson(), privateConfigurationValues );

            ruleBag.ComparisonType = comparisonValue.ComparisonType ?? 0;
            ruleBag.Value = comparisonValue.Value;
        }

        /// <summary>
        /// Gets the field type and private configuration values used to convert the
        /// filter values of rules that compare against a stored field.
        /// </summary>
        /// <returns>The field type, or <c>null</c> when the field cannot be compared against.</returns>
        private static FieldTypeCache GetComparableFieldType( RegistrationTemplateFormField field, out Dictionary<string, string> privateConfigurationValues )
        {
            privateConfigurationValues = new Dictionary<string, string>();

            if ( field.FieldSource == RegistrationFieldSource.PersonField )
            {
                return FieldVisibilityRules.GetSupportedFieldTypeCache( field.PersonFieldType );
            }

            var attribute = field.AttributeId.HasValue ? AttributeCache.Get( field.AttributeId.Value ) : null;

            if ( attribute == null )
            {
                return null;
            }

            privateConfigurationValues = attribute.ConfigurationValues;

            return attribute.FieldType;
        }

        /// <summary>
        /// Gets the field type and private configuration values used to convert the
        /// filter values of rules that compare against an incoming field. Registrant
        /// attributes may not exist in the database yet, so their definition comes
        /// from the bag.
        /// </summary>
        /// <returns>The field type, or <c>null</c> when the field cannot be compared against.</returns>
        private static FieldTypeCache GetComparableFieldType( RegistrationTemplateFormFieldBag field, out Dictionary<string, string> privateConfigurationValues )
        {
            privateConfigurationValues = new Dictionary<string, string>();

            switch ( field.FieldSource )
            {
                case RegistrationFieldSource.PersonField:
                    return FieldVisibilityRules.GetSupportedFieldTypeCache( field.PersonFieldType );

                case RegistrationFieldSource.RegistrantAttribute:
                    {
                        var fieldTypeGuid = field.RegistrantAttribute?.RealFieldTypeGuid ?? field.RegistrantAttribute?.FieldTypeGuid;
                        var fieldType = fieldTypeGuid.HasValue ? FieldTypeCache.Get( fieldTypeGuid.Value ) : null;

                        if ( fieldType?.Field == null )
                        {
                            return null;
                        }

                        privateConfigurationValues = fieldType.Field.GetPrivateConfigurationValues( field.RegistrantAttribute.ConfigurationValues ?? new Dictionary<string, string>() );

                        return fieldType;
                    }

                default:
                    {
                        var attributeGuid = field.Attribute?.Value.AsGuidOrNull();
                        var attribute = attributeGuid.HasValue ? AttributeCache.Get( attributeGuid.Value ) : null;

                        if ( attribute == null )
                        {
                            return null;
                        }

                        privateConfigurationValues = attribute.ConfigurationValues;

                        return attribute.FieldType;
                    }
            }
        }

        /// <summary>
        /// Returns true when the attribute backing the field is active. Person
        /// fields are not attributes and should not call this.
        /// </summary>
        private static bool IsAttributeFieldActive( RegistrationTemplateFormFieldBag field )
        {
            if ( field.FieldSource == RegistrationFieldSource.RegistrantAttribute )
            {
                return field.RegistrantAttribute?.IsActive == true;
            }

            var attributeGuid = field.Attribute?.Value.AsGuidOrNull();

            return attributeGuid.HasValue && AttributeCache.Get( attributeGuid.Value )?.IsActive == true;
        }

        /// <summary>
        /// Returns true when the person is allowed to view the attribute that an
        /// incoming field resolves to. A field that does not resolve one is allowed,
        /// because the caller already skips fields with no usable field type.
        /// </summary>
        private static bool IsComparableFieldAuthorized( RegistrationTemplateFormFieldBag field, Person currentPerson )
        {
            if ( field.FieldSource == RegistrationFieldSource.PersonField
                 || field.FieldSource == RegistrationFieldSource.RegistrantAttribute )
            {
                return true;
            }

            var attributeGuid = field.Attribute?.Value.AsGuidOrNull();
            var attribute = attributeGuid.HasValue ? AttributeCache.Get( attributeGuid.Value ) : null;

            return attribute == null || attribute.IsAuthorized( Authorization.VIEW, currentPerson );
        }

        #endregion Visibility Rules

        #region Save Methods

        /// <summary>
        /// Validates the template and its child collections before anything is written.
        /// A child collection that is <c>null</c> was not edited and is skipped.
        /// </summary>
        /// <param name="entity">The template with the box already applied.</param>
        /// <param name="forms">The normalized incoming forms, or <c>null</c> when they were not edited.</param>
        /// <param name="fees">The normalized incoming fees, or <c>null</c> when they were not edited.</param>
        /// <param name="discounts">The normalized incoming discounts, or <c>null</c> when they were not edited.</param>
        /// <param name="placements">The normalized incoming placements, or <c>null</c> when they were not edited.</param>
        /// <param name="registrationAttributes">The incoming registration attributes, or <c>null</c> when they were not edited.</param>
        /// <param name="groupsByGuid">The shared placement groups keyed by unique identifier.</param>
        /// <returns>The validation errors, empty when the save can proceed.</returns>
        private List<string> ValidateSave( RegistrationTemplate entity, List<RegistrationTemplateFormBag> forms, List<RegistrationTemplateFeeBag> fees, List<RegistrationTemplateDiscountBag> discounts, List<RegistrationTemplatePlacementBag> placements, List<PublicEditableAttributeBag> registrationAttributes, Dictionary<Guid, Rock.Model.Group> groupsByGuid )
        {
            var errors = new List<string>();

            if ( entity.Name.IsNullOrWhiteSpace() )
            {
                errors.Add( "Name is required." );
            }

            var category = entity.CategoryId.HasValue ? CategoryCache.Get( entity.CategoryId.Value ) : null;

            if ( category == null )
            {
                errors.Add( "You must select a valid category." );
            }
            else if ( !category.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                errors.Add( "You are not authorized to create or edit templates for the selected category." );
            }

            var hasFees = fees != null ? fees.Any() : entity.Fees.Any();
            var hasCost = ( entity.SetCostOnInstance ?? false ) || entity.Cost > 0 || hasFees;

            if ( hasCost && !entity.FinancialGatewayId.HasValue )
            {
                errors.Add( "A Financial Gateway is required when the registration has a cost or additional fees or is configured to allow instances to set a cost." );
            }

            // A blank minimum means full payment is required, which makes payment plans unused.
            if ( entity.IsPaymentPlanAllowed && !( entity.SetCostOnInstance ?? false ) && !entity.MinimumInitialPayment.HasValue )
            {
                errors.Add( "Minimum Initial Payment is required when payment plans are enabled." );
            }

            /*
                9/18/2026 - MSE

                The edit panel keeps every amount at or above zero, but nothing on the
                server did, so a request that skips the UI could store a negative cost
                or fee. WebForms had the same gap; this closes it rather than matching
                it. Discount amounts and percentages are checked with their own rules.

                Reason: The client is not a trustworthy place to enforce a money range.
            */
            if ( entity.Cost < 0 )
            {
                errors.Add( "Cost cannot be negative." );
            }

            if ( entity.MinimumInitialPayment < 0 )
            {
                errors.Add( "Minimum Initial Payment cannot be negative." );
            }

            if ( entity.DefaultPayment < 0 )
            {
                errors.Add( "Default Payment Amount cannot be negative." );
            }

            if ( forms != null )
            {
                errors.AddRange( GetFormErrors( forms ) );
            }

            if ( registrationAttributes != null )
            {
                if ( registrationAttributes.Any( a => a.Key.IsNullOrWhiteSpace() || a.Name.IsNullOrWhiteSpace() ) )
                {
                    errors.Add( "Each registration attribute must have a name and a key." );
                }

                errors.AddRange( GetDuplicateKeyErrors( registrationAttributes.Select( a => a.Key ) ) );
            }

            if ( fees != null )
            {
                errors.AddRange( GetFeeErrors( entity, fees ) );
            }

            if ( discounts != null )
            {
                errors.AddRange( GetDiscountErrors( discounts ) );
            }

            if ( placements != null )
            {
                errors.AddRange( GetPlacementErrors( placements, groupsByGuid ) );
            }

            if ( !errors.Any() && !entity.IsValid )
            {
                errors.AddRange( entity.ValidationResults.Select( r => r.ErrorMessage.EncodeHtml() ) );
            }

            return errors;
        }

        /// <summary>
        /// Validates the forms and their registrant attributes.
        /// </summary>
        private static IEnumerable<string> GetFormErrors( List<RegistrationTemplateFormBag> forms )
        {
            var errors = new List<string>();

            if ( forms.Any( f => f.Name.IsNullOrWhiteSpace() ) )
            {
                errors.Add( "Each registrant form must have a name." );
            }

            // EnsureDefaultForm seeds these two fields when the edit bag is built, so
            // only a request that did not come from the edit panel can arrive without
            // them. Registration entry cannot identify a registrant without both.
            var personFieldTypes = forms
                .SelectMany( f => f.Fields )
                .Where( f => f.FieldSource == RegistrationFieldSource.PersonField )
                .Select( f => f.PersonFieldType )
                .ToHashSet();

            if ( !personFieldTypes.Contains( RegistrationPersonFieldType.FirstName )
                 || !personFieldTypes.Contains( RegistrationPersonFieldType.LastName ) )
            {
                errors.Add( "A registrant form must collect both the first name and the last name." );
            }

            var registrantAttributeFields = forms
                .SelectMany( f => f.Fields )
                .Where( f => f.FieldSource == RegistrationFieldSource.RegistrantAttribute && f.RegistrantAttribute != null )
                .ToList();

            if ( registrantAttributeFields.Any( f => f.RegistrantAttribute.Key.IsNullOrWhiteSpace() || f.RegistrantAttribute.Name.IsNullOrWhiteSpace() ) )
            {
                errors.Add( "Each registrant attribute must have a name and a key." );
            }

            errors.AddRange( GetDuplicateKeyErrors( registrantAttributeFields.Select( f => f.RegistrantAttribute.Key ) ) );

            return errors;
        }

        /// <summary>
        /// Validates the fees and their options.
        /// </summary>
        private IEnumerable<string> GetFeeErrors( RegistrationTemplate entity, List<RegistrationTemplateFeeBag> fees )
        {
            var errors = new List<string>();

            if ( fees.Any( f => f.Name.IsNullOrWhiteSpace() ) )
            {
                errors.Add( "Each fee must have a name." );
            }

            /*
                9/11/2026 - MSE

                WebForms only validated option names on rows that existed, so a
                Multiple fee with zero options saved. Registration then had
                nothing to pick.

                Reason: Block Multiple fees that have no options.
            */
            if ( fees.Any( f => f.FeeType == RegistrationFeeType.Multiple && !f.FeeItems.Any() ) )
            {
                errors.Add( "Each multiple-option fee must have at least one option." );
            }

            if ( fees.Where( f => f.FeeType == RegistrationFeeType.Multiple ).SelectMany( f => f.FeeItems ).Any( i => i.Name.IsNullOrWhiteSpace() ) )
            {
                errors.Add( "Option is required." );
            }

            if ( fees.SelectMany( f => f.FeeItems ).Any( i => i.Cost < 0 ) )
            {
                errors.Add( "A fee cost cannot be negative." );
            }

            errors.AddRange( GetRemovedFeeItemsInUseErrors( entity, fees ) );

            return errors;
        }

        /// <summary>
        /// Validates the discounts. Codes must be present and unique.
        /// </summary>
        private static IEnumerable<string> GetDiscountErrors( List<RegistrationTemplateDiscountBag> discounts )
        {
            var errors = new List<string>();

            if ( discounts.Any( d => d.Code.IsNullOrWhiteSpace() ) )
            {
                errors.Add( "Each discount must have a code." );
            }

            var duplicateCodes = discounts
                .Where( d => d.Code.IsNotNullOrWhiteSpace() )
                .GroupBy( d => d.Code.Trim(), StringComparer.OrdinalIgnoreCase )
                .Where( g => g.Count() > 1 )
                .Select( g => g.Key );

            errors.AddRange( duplicateCodes.Select( code => $"The discount code \"{code.EncodeHtml()}\" is already in use." ) );

            if ( discounts.Any( d => d.DiscountAmount < 0 ) )
            {
                errors.Add( "A discount amount cannot be negative." );
            }

            /*
                9/18/2026 - MSE

                Only the lower bound is enforced. WebForms put no maximum on the
                percentage box, so an existing template can legitimately hold more than
                100%, and rejecting those would stop people saving unrelated edits. The
                edit panel caps new entries at 100.

                Reason: Do not make old data unsaveable.
            */
            if ( discounts.Any( d => d.DiscountPercentage < 0 ) )
            {
                errors.Add( "A discount percentage cannot be negative." );
            }

            return errors;
        }

        /// <summary>
        /// Validates the placement configurations. Every placement needs a group type
        /// and its shared groups must belong to that group type.
        /// </summary>
        private static IEnumerable<string> GetPlacementErrors( List<RegistrationTemplatePlacementBag> placements, Dictionary<Guid, Rock.Model.Group> groupsByGuid )
        {
            var errors = new List<string>();

            foreach ( var placement in placements )
            {
                if ( placement.Name.IsNullOrWhiteSpace() )
                {
                    errors.Add( "Each placement configuration must have a name." );
                    continue;
                }

                var groupType = GetGroupTypeFromBag( placement.GroupType );

                if ( groupType == null )
                {
                    errors.Add( $"A group type is required for the placement configuration '{placement.Name.EncodeHtml()}'." );
                    continue;
                }

                var hasMismatchedGroup = GetGroupGuids( placement )
                    .Select( guid => groupsByGuid.TryGetValue( guid, out var group ) ? group : null )
                    .Any( group => group != null && group.GroupTypeId != groupType.Id );

                if ( hasMismatchedGroup )
                {
                    errors.Add( $"Group must have group type of {groupType.Name.EncodeHtml()}." );
                }
            }

            return errors;
        }

        /// <summary>
        /// Gets an error for every attribute key that is used more than once.
        /// </summary>
        private static IEnumerable<string> GetDuplicateKeyErrors( IEnumerable<string> keys )
        {
            return keys
                .Where( key => key.IsNotNullOrWhiteSpace() )
                .GroupBy( key => key.Trim(), StringComparer.OrdinalIgnoreCase )
                .Where( g => g.Count() > 1 )
                .Select( g => $"The Attribute Key '{g.Key.EncodeHtml()}' is already being used by this Registration Template" );
        }

        /// <summary>
        /// Gets an error for every fee item that was removed from a fee that still
        /// exists while a registrant has already selected that item. Removing a whole
        /// fee is allowed and also removes the registrant selections of that fee.
        /// </summary>
        private IEnumerable<string> GetRemovedFeeItemsInUseErrors( RegistrationTemplate entity, List<RegistrationTemplateFeeBag> fees )
        {
            if ( entity.Id == 0 )
            {
                return Enumerable.Empty<string>();
            }

            var incomingFeeGuids = fees.Select( f => f.Guid ).ToHashSet();
            var incomingFeeItemGuids = fees.SelectMany( f => f.FeeItems ).Select( i => i.Guid ).ToHashSet();

            var removedFeeItems = new RegistrationTemplateFeeItemService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( i => i.RegistrationTemplateFee.RegistrationTemplateId == entity.Id )
                .Select( i => new
                {
                    i.Id,
                    i.Guid,
                    i.Name,
                    FeeGuid = i.RegistrationTemplateFee.Guid
                } )
                .ToList()
                .Where( i => incomingFeeGuids.Contains( i.FeeGuid ) && !incomingFeeItemGuids.Contains( i.Guid ) )
                .ToList();

            if ( !removedFeeItems.Any() )
            {
                return Enumerable.Empty<string>();
            }

            var feeItemIdsInUse = GetFeeItemIdsInUse( removedFeeItems.Select( i => i.Id ).ToList() );

            return removedFeeItems
                .Where( i => feeItemIdsInUse.Contains( i.Id ) )
                .Select( i => $"The fee option '{i.Name.EncodeHtml()}' has already been used by a registrant and cannot be removed." );
        }

        /// <summary>
        /// Gets the group type selected in a picker bag.
        /// </summary>
        private static GroupTypeCache GetGroupTypeFromBag( ListItemBag bag )
        {
            var guid = bag?.Value.AsGuidOrNull();

            return guid.HasValue ? GroupTypeCache.Get( guid.Value ) : null;
        }

        /// <summary>
        /// Gets the unique identifiers of the shared groups of a placement.
        /// </summary>
        private static List<Guid> GetGroupGuids( RegistrationTemplatePlacementBag placement )
        {
            return ( placement.SharedGroups ?? new List<ListItemBag>() )
                .Select( g => g?.Value.AsGuidOrNull() )
                .Where( guid => guid.HasValue )
                .Select( guid => guid.Value )
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Loads every group referenced as a shared placement group in a single query.
        /// </summary>
        private Dictionary<Guid, Rock.Model.Group> GetSharedGroupsByGuid( List<RegistrationTemplatePlacementBag> placements )
        {
            var groupGuids = placements.SelectMany( GetGroupGuids ).Distinct().ToList();

            if ( !groupGuids.Any() )
            {
                return new Dictionary<Guid, Rock.Model.Group>();
            }

            return new GroupService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( g => groupGuids.Contains( g.Guid ) )
                .ToList()
                .ToDictionary( g => g.Guid );
        }

        /// <summary>
        /// Removes null entries, assigns identifiers to new items and renumbers the
        /// order of the forms and their fields to match the order sent by the client.
        /// Fields that cannot be collected (see <see cref="IsFieldPersistable"/>) are dropped.
        /// </summary>
        private List<RegistrationTemplateFormBag> NormalizeForms( List<RegistrationTemplateFormBag> forms, int? groupTypeId )
        {
            var validGroupMemberAttributeIds = GetGroupMemberAttributes( groupTypeId ).Select( a => a.Id ).ToHashSet();
            var normalizedForms = ( forms ?? new List<RegistrationTemplateFormBag>() ).Where( f => f != null ).ToList();

            for ( var formIndex = 0; formIndex < normalizedForms.Count; formIndex++ )
            {
                var form = normalizedForms[formIndex];

                form.Guid = form.Guid == Guid.Empty ? Guid.NewGuid() : form.Guid;
                form.Order = formIndex;
                form.Fields = ( form.Fields ?? new List<RegistrationTemplateFormFieldBag>() )
                    .Where( f => f != null && IsFieldPersistable( f, validGroupMemberAttributeIds ) )
                    .ToList();

                for ( var fieldIndex = 0; fieldIndex < form.Fields.Count; fieldIndex++ )
                {
                    var field = form.Fields[fieldIndex];

                    field.Guid = field.Guid == Guid.Empty ? Guid.NewGuid() : field.Guid;
                    field.Order = fieldIndex;
                }
            }

            return normalizedForms;
        }

        /// <summary>
        /// Returns true when the field still points at something Registration Entry
        /// can collect. WebForms only dropped group-member attributes that did not
        /// belong to the selected group type. Person attributes whose definition was
        /// deleted, and registrant attributes with no definition, were saved as empty
        /// fields; those are dropped here instead.
        /// </summary>
        private static bool IsFieldPersistable( RegistrationTemplateFormFieldBag field, HashSet<int> validGroupMemberAttributeIds )
        {
            switch ( field.FieldSource )
            {
                case RegistrationFieldSource.PersonField:
                    return true;

                case RegistrationFieldSource.PersonAttribute:
                    return GetAttributeIdFromBag( field.Attribute ).HasValue;

                case RegistrationFieldSource.GroupMemberAttribute:
                    {
                        var attributeId = GetAttributeIdFromBag( field.Attribute );

                        return attributeId.HasValue && validGroupMemberAttributeIds.Contains( attributeId.Value );
                    }

                case RegistrationFieldSource.RegistrantAttribute:
                    return field.RegistrantAttribute != null;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets the identifier of the attribute selected in a picker bag.
        /// </summary>
        private static int? GetAttributeIdFromBag( ListItemBag bag )
        {
            var guid = bag?.Value.AsGuidOrNull();

            return guid.HasValue ? AttributeCache.Get( guid.Value )?.Id : null;
        }

        /// <summary>
        /// Gets the group member attributes available for the specified group type.
        /// </summary>
        private List<AttributeCache> GetGroupMemberAttributes( int? groupTypeId )
        {
            var groupMember = new GroupMember
            {
                Group = new Rock.Model.Group
                {
                    GroupTypeId = groupTypeId ?? 0
                },
                GroupTypeId = groupTypeId ?? 0
            };

            groupMember.LoadAttributes( RockContext );

            return groupMember.Attributes.Values.ToList();
        }

        /// <summary>
        /// Removes null entries, assigns identifiers to new items and renumbers the order
        /// to match the order sent by the client.
        /// </summary>
        private static List<RegistrationTemplateFeeBag> NormalizeFees( List<RegistrationTemplateFeeBag> fees )
        {
            var normalizedFees = ( fees ?? new List<RegistrationTemplateFeeBag>() ).Where( f => f != null ).ToList();

            for ( var feeIndex = 0; feeIndex < normalizedFees.Count; feeIndex++ )
            {
                var fee = normalizedFees[feeIndex];

                fee.Guid = fee.Guid == Guid.Empty ? Guid.NewGuid() : fee.Guid;
                fee.Order = feeIndex;
                fee.FeeItems = ( fee.FeeItems ?? new List<RegistrationTemplateFeeItemBag>() ).Where( i => i != null ).ToList();

                // A single option fee has exactly one item that is named after the fee.
                if ( fee.FeeType == RegistrationFeeType.Single )
                {
                    var singleItem = fee.FeeItems.FirstOrDefault() ?? new RegistrationTemplateFeeItemBag();

                    singleItem.Name = fee.Name;
                    fee.FeeItems = new List<RegistrationTemplateFeeItemBag> { singleItem };
                }

                for ( var itemIndex = 0; itemIndex < fee.FeeItems.Count; itemIndex++ )
                {
                    var item = fee.FeeItems[itemIndex];

                    item.Guid = item.Guid == Guid.Empty ? Guid.NewGuid() : item.Guid;
                    item.Order = itemIndex;
                }
            }

            return normalizedFees;
        }

        /// <summary>
        /// Removes null entries, assigns identifiers to new items and renumbers the order
        /// to match the order sent by the client.
        /// </summary>
        private static List<RegistrationTemplateDiscountBag> NormalizeDiscounts( List<RegistrationTemplateDiscountBag> discounts )
        {
            var normalizedDiscounts = ( discounts ?? new List<RegistrationTemplateDiscountBag>() ).Where( d => d != null ).ToList();

            for ( var index = 0; index < normalizedDiscounts.Count; index++ )
            {
                var discount = normalizedDiscounts[index];

                discount.Guid = discount.Guid == Guid.Empty ? Guid.NewGuid() : discount.Guid;
                discount.Order = index;
            }

            return normalizedDiscounts;
        }

        /// <summary>
        /// Removes null entries, assigns identifiers to new items and renumbers the order
        /// to match the order sent by the client.
        /// </summary>
        private static List<RegistrationTemplatePlacementBag> NormalizePlacements( List<RegistrationTemplatePlacementBag> placements )
        {
            var normalizedPlacements = ( placements ?? new List<RegistrationTemplatePlacementBag>() ).Where( p => p != null ).ToList();

            for ( var index = 0; index < normalizedPlacements.Count; index++ )
            {
                var placement = normalizedPlacements[index];

                placement.Guid = placement.Guid == Guid.Empty ? Guid.NewGuid() : placement.Guid;
                placement.Order = index;
            }

            return normalizedPlacements;
        }

        /// <summary>
        /// Synchronizes related entities by comparing existing entities with incoming data, deleting removed items,
        /// and adding or updating entities as needed.
        /// </summary>
        /// <returns>The entities that remain after the sync, keyed by the incoming key.</returns>
        private Dictionary<TKey, TEntity> SyncRelatedEntities<TEntity, TBag, TKey>(
            Service<TEntity> service,
            IQueryable<TEntity> existingEntitiesQuery,
            IEnumerable<TBag> incomingBags,
            Func<TEntity, TKey> existingKeySelector,
            Func<TBag, TKey> incomingKeySelector,
            Func<TBag, TEntity> createNew,
            Action<TEntity, TBag> updateEntity )
            where TEntity : Entity<TEntity>, new()
        {
            // Load existing entities from database
            var existingEntities = existingEntitiesQuery.ToList();
            var existingByKey = existingEntities.ToDictionary( existingKeySelector );

            var incomingList = ( incomingBags ?? Enumerable.Empty<TBag>() ).ToList();
            var incomingKeys = incomingList.Select( incomingKeySelector ).ToHashSet();

            // Delete entities that are no longer in the incoming set
            foreach ( var entity in existingEntities.Where( e => !incomingKeys.Contains( existingKeySelector( e ) ) ).ToList() )
            {
                service.Delete( entity );
            }

            // Add or update entities based on incoming data
            var syncedEntities = new Dictionary<TKey, TEntity>();

            foreach ( var bag in incomingList )
            {
                var key = incomingKeySelector( bag );

                if ( !existingByKey.TryGetValue( key, out var entity ) )
                {
                    entity = createNew( bag );
                    service.Add( entity );
                }

                updateEntity( entity, bag );
                syncedEntities[key] = entity;
            }

            return syncedEntities;
        }

        /// <summary>
        /// Deletes the forms that were removed on the client, along with their fields,
        /// and the fields that were removed from the forms that remain. This runs before
        /// registrant attributes are deleted so that no field still references an
        /// attribute that is about to be removed.
        /// </summary>
        private void DeleteRemovedFormsAndFields( RegistrationTemplate entity, List<RegistrationTemplateFormBag> forms )
        {
            var formService = new RegistrationTemplateFormService( RockContext );
            var fieldService = new RegistrationTemplateFormFieldService( RockContext );

            var incomingFormGuids = forms.Select( f => f.Guid ).ToHashSet();
            var incomingFieldGuids = forms.SelectMany( f => f.Fields ).Select( f => f.Guid ).ToHashSet();

            var existingForms = formService.Queryable()
                .Include( f => f.Fields )
                .Where( f => f.RegistrationTemplateId == entity.Id )
                .ToList();

            foreach ( var form in existingForms )
            {
                if ( !incomingFormGuids.Contains( form.Guid ) )
                {
                    foreach ( var field in form.Fields.ToList() )
                    {
                        fieldService.Delete( field );
                    }

                    formService.Delete( form );
                    continue;
                }

                foreach ( var field in form.Fields.Where( f => !incomingFieldGuids.Contains( f.Guid ) ).ToList() )
                {
                    fieldService.Delete( field );
                }
            }
        }

        /// <summary>
        /// Saves the registrant attributes defined on the forms and deletes the ones
        /// that were removed. An attribute that another template still references is
        /// left in place.
        /// </summary>
        /// <returns>The identifier of the saved attribute for each registrant attribute field, keyed by field unique identifier.</returns>
        private Dictionary<Guid, int> SaveRegistrantAttributes( RegistrationTemplate entity, List<RegistrationTemplateFormBag> forms )
        {
            var attributeService = new AttributeService( RockContext );
            var registrantEntityTypeId = EntityTypeCache.Get<RegistrationRegistrant>().Id;
            var qualifierValue = entity.Id.ToString();

            var registrantAttributeFields = forms
                .SelectMany( f => f.Fields )
                .Where( f => f.FieldSource == RegistrationFieldSource.RegistrantAttribute && f.RegistrantAttribute != null )
                .ToList();

            var incomingAttributeGuids = registrantAttributeFields
                .Where( f => f.RegistrantAttribute.Guid.HasValue )
                .Select( f => f.RegistrantAttribute.Guid.Value )
                .ToHashSet();

            var removedAttributes = attributeService
                .GetByEntityTypeQualifier( registrantEntityTypeId, RegistrationTemplateQualifierColumn, qualifierValue, true )
                .ToList()
                .Where( a => !incomingAttributeGuids.Contains( a.Guid ) )
                .ToList();

            if ( removedAttributes.Any() )
            {
                var removedAttributeIds = removedAttributes.Select( a => a.Id ).ToList();
                var formFieldService = new RegistrationTemplateFormFieldService( RockContext );

                // Older data can have the same attribute referenced by a form field of
                // a different template, so those attributes must survive.
                var attributeIdsUsedByOtherTemplates = formFieldService.Queryable()
                    .AsNoTracking()
                    .Where( f => f.AttributeId.HasValue
                        && removedAttributeIds.Contains( f.AttributeId.Value )
                        && f.RegistrationTemplateForm.RegistrationTemplateId != entity.Id )
                    .Select( f => f.AttributeId.Value )
                    .Distinct()
                    .ToList()
                    .ToHashSet();

                var attributeIdsToDelete = removedAttributes
                    .Where( a => !attributeIdsUsedByOtherTemplates.Contains( a.Id ) )
                    .Select( a => a.Id )
                    .ToList();

                // A field whose source was changed away from a registrant attribute
                // still points at the attribute about to be deleted, so the reference
                // is cleared and saved first. SaveForms then sets the identifier the
                // field needs for the source it now has.
                var fieldsReferencingDeletedAttributes = formFieldService.Queryable()
                    .Where( f => f.AttributeId.HasValue
                        && attributeIdsToDelete.Contains( f.AttributeId.Value )
                        && f.RegistrationTemplateForm.RegistrationTemplateId == entity.Id )
                    .ToList();

                if ( fieldsReferencingDeletedAttributes.Any() )
                {
                    foreach ( var field in fieldsReferencingDeletedAttributes )
                    {
                        field.AttributeId = null;
                    }

                    RockContext.SaveChanges();
                }

                foreach ( var attribute in removedAttributes.Where( a => attributeIdsToDelete.Contains( a.Id ) ) )
                {
                    attributeService.Delete( attribute );
                }
            }

            RockContext.SaveChanges();

            var attributeIdsByFieldGuid = new Dictionary<Guid, int>();

            foreach ( var field in registrantAttributeFields )
            {
                var attribute = Rock.Attribute.Helper.SaveAttributeEdits( field.RegistrantAttribute, registrantEntityTypeId, RegistrationTemplateQualifierColumn, qualifierValue, RockContext );

                if ( attribute != null )
                {
                    attributeIdsByFieldGuid[field.Guid] = attribute.Id;
                }
            }

            return attributeIdsByFieldGuid;
        }

        /// <summary>
        /// Adds and updates the forms and their fields.
        /// </summary>
        /// <param name="entity">The saved template.</param>
        /// <param name="forms">The normalized incoming forms.</param>
        /// <param name="registrantAttributeIdsByFieldGuid">The saved registrant attribute identifiers keyed by field unique identifier.</param>
        private void SaveForms( RegistrationTemplate entity, List<RegistrationTemplateFormBag> forms, Dictionary<Guid, int> registrantAttributeIdsByFieldGuid )
        {
            var formService = new RegistrationTemplateFormService( RockContext );
            var fieldService = new RegistrationTemplateFormFieldService( RockContext );

            var allFieldsByGuid = forms
                .SelectMany( f => f.Fields )
                .GroupBy( f => f.Guid )
                .ToDictionary( g => g.Key, g => g.First() );

            var syncedForms = SyncRelatedEntities(
                formService,
                formService.Queryable().Where( f => f.RegistrationTemplateId == entity.Id ),
                forms,
                existingKeySelector: f => f.Guid,
                incomingKeySelector: b => b.Guid,
                createNew: b => new RegistrationTemplateForm { Guid = b.Guid },
                updateEntity: ( form, bag ) =>
                {
                    form.RegistrationTemplateId = entity.Id;
                    form.Name = bag.Name;
                    form.Order = bag.Order;
                } );

            // New forms need an identifier before their fields can point at them.
            RockContext.SaveChanges();

            foreach ( var formBag in forms )
            {
                var form = syncedForms[formBag.Guid];

                SyncRelatedEntities(
                    fieldService,
                    fieldService.Queryable().Where( f => f.RegistrationTemplateFormId == form.Id ),
                    formBag.Fields,
                    existingKeySelector: f => f.Guid,
                    incomingKeySelector: b => b.Guid,
                    createNew: b => new RegistrationTemplateFormField { Guid = b.Guid },
                    updateEntity: ( field, bag ) => UpdateFormField( field, bag, form, registrantAttributeIdsByFieldGuid, allFieldsByGuid ) );
            }
        }

        /// <summary>
        /// Copies the incoming field values onto the field entity, applying the rules
        /// that depend on the field source.
        /// </summary>
        private static void UpdateFormField( RegistrationTemplateFormField field, RegistrationTemplateFormFieldBag bag, RegistrationTemplateForm form, Dictionary<Guid, int> registrantAttributeIdsByFieldGuid, Dictionary<Guid, RegistrationTemplateFormFieldBag> allFieldsByGuid )
        {
            field.RegistrationTemplateFormId = form.Id;
            field.FieldSource = bag.FieldSource;
            field.PersonFieldType = bag.PersonFieldType;
            field.IsInternal = bag.IsInternal;
            field.IsSharedValue = bag.IsSharedValue;
            field.PreText = bag.PreText;
            field.PostText = bag.PostText;
            field.IsLockedIfValuesExist = bag.IsLockedIfValuesExist;
            field.Order = bag.Order;
            field.FieldVisibilityRules = GetPrivateVisibilityRules( bag.VisibilityRules, allFieldsByGuid );

            switch ( bag.FieldSource )
            {
                case RegistrationFieldSource.PersonField:
                    field.AttributeId = null;
                    field.ShowCurrentValue = bag.ShowCurrentValue;
                    field.IsGridField = bag.IsGridField;
                    field.IsRequired = bag.IsRequired;
                    field.ShowOnWaitlist = bag.ShowOnWaitlist;
                    break;

                case RegistrationFieldSource.PersonAttribute:
                    field.AttributeId = GetAttributeIdFromBag( bag.Attribute );
                    field.ShowCurrentValue = bag.ShowCurrentValue;
                    field.IsGridField = bag.IsGridField;
                    field.IsRequired = bag.IsRequired;
                    field.ShowOnWaitlist = bag.ShowOnWaitlist;
                    break;

                case RegistrationFieldSource.GroupMemberAttribute:
                    // Group member values do not exist until the registrant is placed in
                    // the group, so there is no current value to show and the field cannot
                    // be collected on the wait list.
                    field.AttributeId = GetAttributeIdFromBag( bag.Attribute );
                    field.ShowCurrentValue = false;
                    field.IsGridField = bag.IsGridField;
                    field.IsRequired = bag.IsRequired;
                    field.ShowOnWaitlist = false;
                    break;

                case RegistrationFieldSource.RegistrantAttribute:
                    // The attribute definition owns the required and grid settings.
                    field.AttributeId = registrantAttributeIdsByFieldGuid.TryGetValue( bag.Guid, out var attributeId ) ? attributeId : ( int? ) null;
                    field.ShowCurrentValue = false;
                    field.IsGridField = bag.RegistrantAttribute?.IsShowInGrid ?? bag.IsGridField;
                    field.IsRequired = bag.RegistrantAttribute?.IsRequired ?? bag.IsRequired;
                    field.ShowOnWaitlist = bag.ShowOnWaitlist;
                    break;
            }
        }

        /// <summary>
        /// Adds, updates and deletes the discounts to match the incoming list.
        /// </summary>
        private void SaveDiscounts( RegistrationTemplate entity, List<RegistrationTemplateDiscountBag> discounts )
        {
            var discountService = new RegistrationTemplateDiscountService( RockContext );

            SyncRelatedEntities(
                discountService,
                discountService.Queryable().Where( d => d.RegistrationTemplateId == entity.Id ),
                discounts,
                existingKeySelector: d => d.Guid,
                incomingKeySelector: b => b.Guid,
                createNew: b => new RegistrationTemplateDiscount { Guid = b.Guid },
                updateEntity: ( discount, bag ) =>
                {
                    discount.RegistrationTemplateId = entity.Id;
                    discount.Code = bag.Code;
                    discount.DiscountPercentage = bag.DiscountPercentage;
                    discount.DiscountAmount = bag.DiscountAmount;
                    discount.Order = bag.Order;
                    discount.MaxUsage = bag.MaxUsage;
                    discount.MaxRegistrants = bag.MaxRegistrants;
                    discount.MinRegistrants = bag.MinRegistrants;
                    discount.StartDate = bag.StartDate?.DateTime;
                    discount.EndDate = bag.EndDate?.DateTime;
                    discount.AutoApplyDiscount = bag.AutoApplyDiscount;
                } );
        }

        /// <summary>
        /// Adds, updates and deletes the fees and their items to match the incoming
        /// list. Registrant selections of a removed fee are removed with it.
        /// </summary>
        private void SaveFees( RegistrationTemplate entity, List<RegistrationTemplateFeeBag> fees )
        {
            var feeService = new RegistrationTemplateFeeService( RockContext );
            var feeItemService = new RegistrationTemplateFeeItemService( RockContext );
            var registrantFeeService = new RegistrationRegistrantFeeService( RockContext );

            var incomingFeeGuids = fees.Select( f => f.Guid ).ToHashSet();

            var removedFees = feeService.Queryable()
                .Include( f => f.FeeItems )
                .Where( f => f.RegistrationTemplateId == entity.Id )
                .ToList()
                .Where( f => !incomingFeeGuids.Contains( f.Guid ) )
                .ToList();

            if ( removedFees.Any() )
            {
                var removedFeeIds = removedFees.Select( f => f.Id ).ToList();

                var registrantFees = registrantFeeService.Queryable()
                    .Where( rf => removedFeeIds.Contains( rf.RegistrationTemplateFeeId ) )
                    .ToList();

                registrantFeeService.DeleteRange( registrantFees );

                foreach ( var fee in removedFees )
                {
                    feeItemService.DeleteRange( fee.FeeItems.ToList() );
                }
            }

            var syncedFees = SyncRelatedEntities(
                feeService,
                feeService.Queryable().Where( f => f.RegistrationTemplateId == entity.Id ),
                fees,
                existingKeySelector: f => f.Guid,
                incomingKeySelector: b => b.Guid,
                createNew: b => new RegistrationTemplateFee { Guid = b.Guid },
                updateEntity: ( fee, bag ) =>
                {
                    fee.RegistrationTemplateId = entity.Id;
                    fee.Name = bag.Name;
                    fee.FeeType = bag.FeeType;
                    fee.DiscountApplies = bag.DiscountApplies;
                    fee.AllowMultiple = bag.AllowMultiple;
                    fee.Order = bag.Order;
                    fee.IsActive = bag.IsActive;
                    fee.IsRequired = bag.IsRequired;
                    fee.HideWhenNoneRemaining = bag.HideWhenNoneRemaining;
                } );

            // New fees need an identifier before their items can point at them.
            RockContext.SaveChanges();

            foreach ( var feeBag in fees )
            {
                var fee = syncedFees[feeBag.Guid];

                SyncRelatedEntities(
                    feeItemService,
                    feeItemService.Queryable().Where( i => i.RegistrationTemplateFeeId == fee.Id ),
                    feeBag.FeeItems,
                    existingKeySelector: i => i.Guid,
                    incomingKeySelector: b => b.Guid,
                    createNew: b => new RegistrationTemplateFeeItem { Guid = b.Guid },
                    updateEntity: ( item, bag ) =>
                    {
                        item.RegistrationTemplateFeeId = fee.Id;
                        item.Name = bag.Name;
                        item.Cost = bag.Cost ?? 0.0M;
                        item.MaximumUsageCount = bag.MaximumUsageCount;
                        item.Order = bag.Order;
                    } );
            }
        }

        /// <summary>
        /// Adds, updates and deletes the placement configurations to match the incoming
        /// list and links the shared groups of each placement.
        /// </summary>
        /// <param name="entity">The saved template.</param>
        /// <param name="placements">The normalized incoming placements.</param>
        /// <param name="groupsByGuid">The shared groups keyed by unique identifier.</param>
        private void SavePlacements( RegistrationTemplate entity, List<RegistrationTemplatePlacementBag> placements, Dictionary<Guid, Rock.Model.Group> groupsByGuid )
        {
            var placementService = new RegistrationTemplatePlacementService( RockContext );
            var incomingPlacementGuids = placements.Select( p => p.Guid ).ToHashSet();

            // Shared groups are stored as related entity records, so clear them before
            // the placement itself goes away.
            var removedPlacements = placementService.Queryable()
                .Where( p => p.RegistrationTemplateId == entity.Id )
                .ToList()
                .Where( p => !incomingPlacementGuids.Contains( p.Guid ) )
                .ToList();

            foreach ( var placement in removedPlacements )
            {
                placementService.SetRegistrationTemplatePlacementPlacementGroups( placement, new List<Rock.Model.Group>() );
            }

            var syncedPlacements = SyncRelatedEntities(
                placementService,
                placementService.Queryable().Where( p => p.RegistrationTemplateId == entity.Id ),
                placements,
                existingKeySelector: p => p.Guid,
                incomingKeySelector: b => b.Guid,
                createNew: b => new RegistrationTemplatePlacement { Guid = b.Guid },
                updateEntity: ( placement, bag ) =>
                {
                    placement.RegistrationTemplateId = entity.Id;
                    placement.Name = bag.Name;
                    placement.GroupTypeId = GetGroupTypeFromBag( bag.GroupType )?.Id ?? placement.GroupTypeId;
                    placement.IconCssClass = bag.IconCssClass;
                    placement.Order = bag.Order;
                    placement.AllowMultiplePlacements = bag.AllowMultiplePlacements;
                } );

            // New placements need an identifier before groups can be related to them.
            RockContext.SaveChanges();

            foreach ( var placementBag in placements )
            {
                var placement = syncedPlacements[placementBag.Guid];

                var sharedGroups = GetGroupGuids( placementBag )
                    .Select( guid => groupsByGuid.TryGetValue( guid, out var group ) ? group : null )
                    .Where( group => group != null )
                    .ToList();

                placementService.SetRegistrationTemplatePlacementPlacementGroups( placement, sharedGroups );
            }
        }

        /// <summary>
        /// Saves the attributes for the specified entity type and qualifier, deleting
        /// the ones that were removed and ordering the rest as sent by the client.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier whose attributes are being edited.</param>
        /// <param name="qualifierColumn">The attribute qualifier column.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <param name="attributes">The attributes as edited in the UI.</param>
        private void SaveAttributes( int entityTypeId, string qualifierColumn, string qualifierValue, List<PublicEditableAttributeBag> attributes )
        {
            var attributeService = new AttributeService( RockContext );
            var existingAttributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true ).ToList();

            var remainingAttributeGuids = attributes
                .Where( a => a.Guid.HasValue )
                .Select( a => a.Guid.Value )
                .ToHashSet();

            foreach ( var attribute in existingAttributes.Where( a => !remainingAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attribute );
            }

            RockContext.SaveChanges();

            // The attributes are coming from the frontend already sorted in the correct order.
            var attributeOrder = 0;

            foreach ( var attributeBag in attributes )
            {
                var attribute = Rock.Attribute.Helper.SaveAttributeEdits( attributeBag, entityTypeId, qualifierColumn, qualifierValue, RockContext );

                if ( attribute != null )
                {
                    attribute.Order = attributeOrder++;
                }
            }

            RockContext.SaveChanges();
        }

        #endregion Save Methods

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

            var hydrated = entity.Id > 0 ? GetHydratedEntity( entity.Id ) ?? entity : entity;
            var bag = GetEntityBagForEdit( hydrated );

            return ActionOk( new ValidPropertiesBox<RegistrationTemplateBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the template described by the box, including its forms, fields,
        /// attributes, fees, discounts and placements. Returns the refreshed view bag
        /// on edit, or a redirect URL on add so the page can re-enter with the new id.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<RegistrationTemplateBag> box )
        {
            if ( box?.Bag == null )
            {
                return ActionBadRequest( "Invalid data." );
            }

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // A child collection is only synchronized when the box marks it as
            // valid. A collection that is not part of the box is left untouched.
            var forms = box.IsValidProperty( nameof( box.Bag.Forms ) )
                ? NormalizeForms( box.Bag.Forms, entity.GroupTypeId )
                : null;
            var fees = box.IsValidProperty( nameof( box.Bag.Fees ) )
                ? NormalizeFees( box.Bag.Fees )
                : null;
            var discounts = box.IsValidProperty( nameof( box.Bag.Discounts ) )
                ? NormalizeDiscounts( box.Bag.Discounts )
                : null;
            var placements = box.IsValidProperty( nameof( box.Bag.Placements ) )
                ? NormalizePlacements( box.Bag.Placements )
                : null;
            var registrationAttributes = box.IsValidProperty( nameof( box.Bag.RegistrationAttributes ) )
                ? ( box.Bag.RegistrationAttributes ?? new List<PublicEditableAttributeBag>() ).Where( a => a != null ).ToList()
                : null;
            var sharedGroupsByGuid = placements != null
                ? GetSharedGroupsByGuid( placements )
                : new Dictionary<Guid, Rock.Model.Group>();

            var errors = ValidateSave( entity, forms, fees, discounts, placements, registrationAttributes, sharedGroupsByGuid );

            if ( errors.Any() )
            {
                return ActionBadRequest( errors.Count == 1
                    ? errors[0]
                    : $"<ul class='list-unstyled'><li>{errors.AsDelimited( "</li><li>" )}</li></ul>" );
            }

            var isNew = entity.Id == 0;

            try
            {
                RockContext.WrapTransaction( () =>
                {
                    // The template needs an identifier before anything can reference it.
                    RockContext.SaveChanges();

                    if ( forms != null )
                    {
                        DeleteRemovedFormsAndFields( entity, forms );

                        var registrantAttributeIdsByFieldGuid = SaveRegistrantAttributes( entity, forms );

                        SaveForms( entity, forms, registrantAttributeIdsByFieldGuid );
                    }

                    if ( discounts != null )
                    {
                        SaveDiscounts( entity, discounts );
                    }

                    if ( fees != null )
                    {
                        SaveFees( entity, fees );
                    }

                    if ( placements != null )
                    {
                        SavePlacements( entity, placements, sharedGroupsByGuid );
                    }

                    if ( registrationAttributes != null )
                    {
                        SaveAttributes( EntityTypeCache.Get<Registration>().Id, RegistrationTemplateQualifierColumn, entity.Id.ToString(), registrationAttributes );
                    }

                    // A save that only changed child records still counts as a change to
                    // the template, so make sure its modified stamp reflects it.
                    entity.ModifiedDateTime = RockDateTime.Now;
                    entity.ModifiedByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;

                    RockContext.SaveChanges();
                } );
            }
            catch ( PropertyValidationException ex )
            {
                return ActionBadRequest( ex.Message.EncodeHtml() );
            }

            if ( isNew )
            {
                var urlParams = new Dictionary<string, string>
                {
                    [PageParameterKey.RegistrationTemplateId] = entity.IdKey
                };

                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( urlParams, skipExistingParameters: true ) );
            }

            var bag = GetEntityBagForView( GetHydratedEntity( entity.Id ) );

            return ActionOk( new ValidPropertiesBox<RegistrationTemplateBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Deletes the template along with every instance and registration that belongs to it.
        /// </summary>
        /// <param name="key">The identifier of the template to delete.</param>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new RegistrationTemplateService( RockContext );
            var entity = entityService
                .GetQueryableByKey( key, !PageCache.Layout.Site.DisablePredictableIds )
                .Include( t => t.Instances.Select( i => i.Registrations.Select( r => r.PaymentPlanFinancialScheduledTransaction ) ) )
                .FirstOrDefault();

            if ( entity == null )
            {
                return ActionNotFound();
            }

            if ( !IsAuthorizedToDeleteOrCopy( entity ) )
            {
                return ActionForbidden( "You are not authorized to delete this registration template." );
            }

            var registrationService = new RegistrationService( RockContext );
            var financialScheduledTransactionService = new FinancialScheduledTransactionService( RockContext );
            var registrations = entity.Instances.SelectMany( i => i.Registrations ).ToList();
            var cancelledPaymentPlanCount = 0;
            string cancelError = null;

            /*
                9/17/2026 - MSE

                Gateway cancellations cannot be undone, so this loop is built to be
                safe to re-run. Cancel only stamps InactivateDateTime; IsActive is
                cleared by the GetStatus that follows, so a failed GetStatus leaves the
                plan marked active and a retry would cancel it twice. IsActive is
                cleared here, and each cancellation is committed immediately, so a
                retry skips what already succeeded. A hard failure stops the loop
                because the delete is abandoned either way; a warning does not, because
                its cancellation already reached the gateway.

                Reason: The delete has to be resumable and must never cancel more plans
                than it needs to.
            */
            foreach ( var registration in registrations )
            {
                var paymentPlan = registration.PaymentPlanFinancialScheduledTransaction;

                if ( paymentPlan == null || !paymentPlan.IsActive )
                {
                    continue;
                }

                var isCancelled = registrationService.TryCancelPaymentPlan(
                    registration,
                    financialScheduledTransactionService,
                    out var error,
                    out var warning );

                if ( !isCancelled )
                {
                    var registrantName = $"{registration.FirstName} {registration.LastName}".EncodeHtml();

                    cancelError = $"The payment plan for registration Id {registration.Id} ({registrantName}) could not be cancelled: {( error ?? "Unknown error" ).EncodeHtml()}";
                    break;
                }

                if ( paymentPlan.IsActive && paymentPlan.InactivateDateTime.HasValue )
                {
                    paymentPlan.IsActive = false;
                }

                RockContext.SaveChanges();
                cancelledPaymentPlanCount++;

                if ( warning.IsNotNullOrWhiteSpace() )
                {
                    Logger.LogWarning(
                        "Cancelling the payment plan for Registration {RegistrationId} while deleting Registration Template {RegistrationTemplateId} reported: {CancelWarning}",
                        registration.Id,
                        entity.Id,
                        warning );
                }
            }

            if ( cancelError.IsNotNullOrWhiteSpace() )
            {
                var resumeNotice = cancelledPaymentPlanCount > 0
                    ? $" {cancelledPaymentPlanCount} {( cancelledPaymentPlanCount == 1 ? "payment plan was" : "payment plans were" )} already cancelled on the financial gateway and cannot be reinstated. Those plans will be skipped if you try again."
                    : string.Empty;

                return ActionBadRequest( $"The registration template was not deleted. {cancelError}{resumeNotice}" );
            }

            RockContext.WrapTransaction( () =>
            {
                registrationService.DeleteRange( registrations );
                new RegistrationInstanceService( RockContext ).DeleteRange( entity.Instances.ToList() );
                entityService.Delete( entity );
                RockContext.SaveChanges();
            } );

            return ActionOk( this.GetCurrentPageUrl( new Dictionary<string, string>(), skipExistingParameters: true ) );
        }

        /// <summary>
        /// Builds an unsaved copy of the template and returns a bag that the frontend
        /// uses to seed edit mode. Every child record receives a new identifier, and
        /// visibility rules are remapped to the copied fields.
        /// </summary>
        /// <param name="key">The identifier of the template to copy.</param>
        [BlockAction]
        public BlockActionResult Copy( string key )
        {
            if ( key.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "A registration template key is required." );
            }

            var templateId = new RegistrationTemplateService( RockContext )
                .GetSelect( key, t => ( int? ) t.Id, !PageCache.Layout.Site.DisablePredictableIds );
            var original = templateId.HasValue ? GetHydratedEntity( templateId.Value ) : null;

            if ( original == null )
            {
                return ActionNotFound();
            }

            if ( !IsAuthorizedToDeleteOrCopy( original ) )
            {
                return ActionForbidden( "You are not authorized to copy this registration template." );
            }

            /*
                9/10/2026 - MSE

                The copy is built from the edit bag of the source template rather than
                a cloned entity graph. Nothing is written until the person saves, which
                lets them rename or adjust the copy first and means a cancelled copy
                leaves no records behind.

                Reason: Copy behaves like a pre-filled Add form.
            */
            var bag = GetEntityBagForEdit( original );

            bag.IdKey = null;
            bag.Name = $"{original.Name} - Copy";
            bag.HasRegistrations = false;
            bag.GroupPlacements = new List<RegistrationTemplateGroupPlacementBag>();

            var fieldGuidMap = new Dictionary<Guid, Guid>();

            foreach ( var form in bag.Forms )
            {
                form.Guid = Guid.NewGuid();

                foreach ( var field in form.Fields )
                {
                    var newGuid = Guid.NewGuid();

                    fieldGuidMap[field.Guid] = newGuid;
                    field.Guid = newGuid;

                    if ( field.RegistrantAttribute != null )
                    {
                        field.RegistrantAttribute.Guid = null;
                        field.RegistrantAttribute.IsSystem = false;
                    }
                }
            }

            foreach ( var field in bag.Forms.SelectMany( f => f.Fields ).Where( f => f.VisibilityRules?.Rules != null ) )
            {
                foreach ( var rule in field.VisibilityRules.Rules )
                {
                    rule.Guid = Guid.NewGuid();
                    rule.AttributeGuid = rule.AttributeGuid.HasValue && fieldGuidMap.TryGetValue( rule.AttributeGuid.Value, out var newFieldGuid )
                        ? newFieldGuid
                        : ( Guid? ) null;
                }

                field.VisibilityRules.Rules.RemoveAll( r => !r.AttributeGuid.HasValue );
            }

            foreach ( var attribute in bag.RegistrationAttributes )
            {
                attribute.Guid = null;
                attribute.IsSystem = false;
            }

            foreach ( var fee in bag.Fees )
            {
                fee.Guid = Guid.NewGuid();

                foreach ( var item in fee.FeeItems )
                {
                    item.Guid = Guid.NewGuid();
                    item.IsInUse = false;
                }
            }

            foreach ( var discount in bag.Discounts )
            {
                discount.Guid = Guid.NewGuid();
            }

            foreach ( var placement in bag.Placements )
            {
                placement.Guid = Guid.NewGuid();
            }

            return ActionOk( new ValidPropertiesBox<RegistrationTemplateBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Gets the capabilities of a financial gateway so the edit panel can show
        /// the payment settings the gateway supports.
        /// </summary>
        /// <param name="financialGatewayGuid">The unique identifier of the gateway, or <c>null</c> when none is selected.</param>
        [BlockAction]
        public BlockActionResult GetFinancialGatewayFeatures( Guid? financialGatewayGuid )
        {
            if ( !IsAuthorizedForEdit( GetInitialEntity() ) )
            {
                return ActionForbidden( EditModeMessage.NotAuthorizedToEdit( RegistrationTemplate.FriendlyTypeName ) );
            }

            var gateway = financialGatewayGuid.HasValue
                ? new FinancialGatewayService( RockContext ).GetNoTracking( financialGatewayGuid.Value )
                : null;

            return ActionOk( GetGatewayFeatures( gateway ) );
        }

        /// <summary>
        /// Gets the person attributes that can be collected as registrant form fields.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetPersonAttributes()
        {
            if ( !IsAuthorizedForEdit( GetInitialEntity() ) )
            {
                return ActionForbidden( EditModeMessage.NotAuthorizedToEdit( RegistrationTemplate.FriendlyTypeName ) );
            }

            var person = new Person();
            person.LoadAttributes( RockContext );

            return ActionOk( GetAttributeItems( person.Attributes.Values, RequestContext.CurrentPerson ) );
        }

        /// <summary>
        /// Gets the group member attributes that can be collected for the specified group type.
        /// </summary>
        /// <param name="groupTypeGuid">The unique identifier of the group type, or <c>null</c> when none is selected.</param>
        [BlockAction]
        public BlockActionResult GetGroupMemberAttributes( Guid? groupTypeGuid )
        {
            if ( !IsAuthorizedForEdit( GetInitialEntity() ) )
            {
                return ActionForbidden( EditModeMessage.NotAuthorizedToEdit( RegistrationTemplate.FriendlyTypeName ) );
            }

            var groupTypeId = groupTypeGuid.HasValue ? GroupTypeCache.GetId( groupTypeGuid.Value ) : null;

            return ActionOk( GetAttributeItems( GetGroupMemberAttributes( groupTypeId ), RequestContext.CurrentPerson ) );
        }

        /// <summary>
        /// Gets the options used by the edit panel. Signature templates, group types
        /// and grades are not loaded on view, so the edit action fetches them here.
        /// </summary>
        /// <param name="key">The identifier of the template being edited.</param>
        [BlockAction]
        public BlockActionResult GetEditOptions( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            return ActionOk( GetBoxOptions( entity, includeEditOptions: true ) );
        }

        /// <summary>
        /// Gets the filter sources that a field's visibility rules can compare against.
        /// New rules match WebForms: Gender, plus public active attribute fields that
        /// have a filter control and raise change events. Internal and inactive fields
        /// are omitted because Registration Entry ignores them when evaluating rules.
        /// Fields named in <paramref name="includeFieldGuids"/> are also returned so
        /// existing rules remain editable.
        /// </summary>
        /// <param name="fields">The other fields of the same form.</param>
        /// <param name="includeFieldGuids">The unique identifiers of fields that existing rules already compare against.</param>
        [BlockAction]
        public BlockActionResult GetFieldFilterSources( List<RegistrationTemplateFormFieldBag> fields, List<Guid> includeFieldGuids )
        {
            if ( !IsAuthorizedForEdit( GetInitialEntity() ) )
            {
                return ActionForbidden( EditModeMessage.NotAuthorizedToEdit( RegistrationTemplate.FriendlyTypeName ) );
            }

            var sources = new List<FieldFilterSourceBag>();
            var includedGuids = ( includeFieldGuids ?? new List<Guid>() ).ToHashSet();

            foreach ( var field in ( fields ?? new List<RegistrationTemplateFormFieldBag>() ).Where( f => f != null ) )
            {
                if ( !IsComparableFieldAuthorized( field, RequestContext.CurrentPerson ) )
                {
                    continue;
                }

                var fieldType = GetComparableFieldType( field, out var privateConfigurationValues );

                if ( fieldType?.Field == null )
                {
                    continue;
                }

                var isPersonField = field.FieldSource == RegistrationFieldSource.PersonField;

                /*
                    9/11/2026 - MSE

                    WebForms FieldVisibilityRulesEditor.GetSupportedComparableFields()
                    only lists attribute fields that have both a filter control and a
                    change handler, plus Gender. That is the add-criteria list.

                    Registration Entry also skips internal fields and inactive attributes
                    when it resolves compared-to fields, so those are omitted for new
                    rules. Offering them would save a rule that never hides the field.

                    Existing rules can still point at a field that no longer qualifies.
                    Those fields are returned so opening and saving the filter modal
                    does not delete the rule.

                    Reason: Match what Registration Entry actually evaluates; do not
                    destroy existing rules.
                */
                if ( !includedGuids.Contains( field.Guid ) )
                {
                    if ( field.IsInternal )
                    {
                        continue;
                    }

                    if ( !isPersonField
                         && ( !IsAttributeFieldActive( field )
                              || !fieldType.Field.HasFilterControl()
                              || !fieldType.IsWebFormChangeNotificationSupported( privateConfigurationValues ) ) )
                    {
                        continue;
                    }
                }

                var universalFieldTypeGuid = fieldType.Field.GetType().GetCustomAttribute<UniversalFieldTypeGuidAttribute>()?.Guid;
                var name = isPersonField ? field.PersonFieldType.ConvertToString() : field.Name;

                sources.Add( new FieldFilterSourceBag
                {
                    Guid = field.Guid,
                    Type = FieldFilterSourceType.Attribute,
                    Attribute = new PublicAttributeBag
                    {
                        AttributeGuid = field.Guid,
                        FieldTypeGuid = universalFieldTypeGuid ?? fieldType.Guid,
                        Name = name,
                        Key = field.Guid.ToString( "N" ),
                        Order = field.Order,
                        ConfigurationValues = fieldType.Field.GetPublicConfigurationValues( privateConfigurationValues, ConfigurationValueUsage.Edit, null )
                    }
                } );
            }

            return ActionOk( sources );
        }

        #endregion Block Actions
    }
}
