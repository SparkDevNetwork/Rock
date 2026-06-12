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
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Event.RegistrantDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays the details of a particular registrant and allows editing.
    /// </summary>

    [DisplayName( "Registrant Detail" )]
    [Category( "Event" )]
    [Description( "Displays the details of a particular registrant." )]
    [IconCssClass( "fa fa-user" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "6e675de9-b320-4f79-9a94-6389901039f7" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "8208ca56-7675-4741-b0d9-54a10b492011" )]
    [Rock.SystemGuid.BlockTypeGuid( "D72A1A61-43D1-4D5D-92EC-BAECA02EAC43" )]
    public class RegistrantDetail : RockEntityDetailBlockType<RegistrationRegistrant, RegistrantBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string RegistrantId = "RegistrantId";
            public const string RegistrationId = "RegistrationId";
            public const string ReturnUrl = "ReturnUrl";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
            public const string RegistrationInstancePage = "RegistrationInstancePage";
            public const string RegistrationTemplatePage = "RegistrationTemplatePage";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// Caches the registration (with its instance and template eagerly loaded) for the
        /// lifetime of the request so the callers of <see cref="GetRegistration"/>
        /// do not each re-query. Keyed on the registration Id it was loaded for so a call with
        /// a different registrant still resolves correctly.
        /// </summary>
        private Registration _registration;

        /// <summary>
        /// The <see cref="Registration.Id"/> that <see cref="_registration"/> was loaded for, or
        /// <c>0</c> if no load has been attempted yet.
        /// </summary>
        private int _registrationId;

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<RegistrantBag, RegistrantDetailOptionsBag>();
            var entity = GetInitialEntity();

            SetBoxInitialEntityState( box, entity );
            box.NavigationUrls = GetBoxNavigationUrls( entity );
            box.Options = GetBoxOptions( entity?.RegistrationId ?? 0 );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the edit panel,
        /// including fee template definitions and remaining-usage counts.
        /// Matches the legacy WebForms approach: counts all usage across the instance
        /// without excluding the current registrant.
        /// </summary>
        private RegistrantDetailOptionsBag GetBoxOptions( int registrationId )
        {
            if ( registrationId == 0 )
            {
                return new RegistrantDetailOptionsBag { Fees = new List<RegistrationTemplateFeeBag>() };
            }

            var registrationInfo = new RegistrationService( RockContext )
                .Queryable()
                .Where( r => r.Id == registrationId )
                .Select( r => new
                {
                    r.RegistrationInstanceId,
                    TemplateId = r.RegistrationInstance.RegistrationTemplateId
                } )
                .FirstOrDefault();

            if ( registrationInfo == null )
            {
                return new RegistrantDetailOptionsBag { Fees = new List<RegistrationTemplateFeeBag>() };
            }

            // Load fees with their items
            var templateFees = new RegistrationTemplateFeeService( RockContext )
                .Queryable()
                .Include( f => f.FeeItems )
                .Where( f => f.RegistrationTemplateId == registrationInfo.TemplateId && f.IsActive )
                .OrderBy( f => f.Order )
                .ToList();

            // Get the totals used for each item so we can know amountRemaining for the UI
            var feeItemUsage = new RegistrationRegistrantFeeService( RockContext )
                .Queryable()
                .Where( f => f.RegistrationRegistrant.Registration.RegistrationInstanceId == registrationInfo.RegistrationInstanceId
                    && f.RegistrationTemplateFeeItemId.HasValue )
                .GroupBy( f => f.RegistrationTemplateFeeItemId.Value )
                .Select( g => new { FeeItemId = g.Key, UsedCount = g.Sum( f => f.Quantity ) } )
                .ToDictionary( x => x.FeeItemId, x => x.UsedCount );

            var fees = templateFees
                .Select( f => new RegistrationTemplateFeeBag
                {
                    Id = f.Id,
                    Name = f.Name,
                    FeeType = f.FeeType,
                    AllowMultiple = f.AllowMultiple,
                    HideWhenNoneRemaining = f.HideWhenNoneRemaining,
                    Items = f.FeeItems
                        .OrderBy( i => i.Order )
                        .Select( i => new RegistrationTemplateFeeItemBag
                        {
                            Id = i.Id,
                            Name = i.Name,
                            Cost = i.Cost,
                            CountRemaining = i.MaximumUsageCount.HasValue
                                ? ( int? ) Math.Max( 0, i.MaximumUsageCount.Value - feeItemUsage.GetValueOrDefault( i.Id, 0 ) )
                                : null
                        } ).ToList()
                } ).ToList();

            return new RegistrantDetailOptionsBag
            {
                Fees = fees
            };
        }

        /// <summary>
        /// Validates the registrant entity for any issues not covered by model validation.
        /// </summary>
        /// <param name="entity">The entity to validate.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the entity is valid; otherwise <c>false</c>.</returns>
        private bool ValidateRegistrationRegistrant( RegistrationRegistrant entity, out string errorMessage )
        {
            errorMessage = null;

            if ( !entity.IsValid )
            {
                errorMessage = entity.ValidationResults
                    .Select( r => r.ErrorMessage )
                    .JoinStrings( ", " );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state on the box. This block is edit-only so the
        /// entity is always placed into edit mode if the person has permission.
        /// </summary>
        private void SetBoxInitialEntityState( DetailBlockBox<RegistrantBag, RegistrantDetailOptionsBag> box, RegistrationRegistrant entity )
        {
            if ( entity == null )
            {
                box.ErrorMessage = $"The {RegistrationRegistrant.FriendlyTypeName} was not found.";
                return;
            }

            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( box.IsEditable )
            {
                box.Entity = GetEntityBagForEdit( entity );
            }
            else
            {
                box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( RegistrationRegistrant.FriendlyTypeName );
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Builds the bag fields that are common to every call returning registrant data.
        /// </summary>
        private RegistrantBag GetCommonEntityBag( RegistrationRegistrant entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var registration = GetRegistration( entity );
            var instance = registration?.RegistrationInstance;
            var template = instance?.RegistrationTemplate;

            return new RegistrantBag
            {
                IdKey = entity.IdKey,
                PersonAlias = entity.PersonAlias != null
                    ? new ListItemBag
                    {
                        Value = entity.PersonAlias.IdKey,
                        Text = entity.PersonAlias.Person?.FullName
                    }
                    : null,
                Cost = entity.Cost,
                DiscountApplies = entity.DiscountApplies,
                IsOnWaitList = entity.OnWaitList,
                Fees = entity.Fees?.Select( f => new RegistrantFeeBag
                {
                    RegistrationTemplateFeeId = f.RegistrationTemplateFeeId,
                    RegistrationTemplateFeeItemId = f.RegistrationTemplateFeeItemId,
                    Quantity = f.Quantity,
                    Cost = f.Cost,
                    Option = f.Option
                } ).ToList(),
                HasGroupMember = entity.GroupMemberId.HasValue,
                RegistrationIdKey = registration?.IdKey,
                RegistrationTemplateName = template?.Name,
                RegistrationInstanceName = instance?.Name,
                RegistrationName = registration?.ToString(),
                IsWaitListEnabled = template?.WaitListEnabled ?? false,
                SignatureDocumentTemplateName = template?.RequiredSignatureDocumentTemplate?.Name,
                SignatureDocumentTemplateBinaryFileTypeGuid = template?.RequiredSignatureDocumentTemplate?.BinaryFileType?.Guid,
                HasExistingSignatureDocument = false
            };
        }

        /// <inheritdoc/>
        protected override RegistrantBag GetEntityBagForView( RegistrationRegistrant entity )
        {
            // This block is edit-only; view mode is not used.
            return GetEntityBagForEdit( entity );
        }

        /// <inheritdoc/>
        protected override RegistrantBag GetEntityBagForEdit( RegistrationRegistrant entity )
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

            // Only expose attributes that are registrant-attribute-sourced fields from the
            // registration template forms. Person and GroupMember attributes are edited on
            // their own detail blocks.
            var allowedAttributeKeys = GetRegistrantAttributeKeys( entity );
            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: false );

            if ( allowedAttributeKeys.Count > 0 )
            {
                bag.Attributes = bag.Attributes?
                    .Where( kvp => allowedAttributeKeys.Contains( kvp.Key ) )
                    .ToDictionary( kvp => kvp.Key, kvp => kvp.Value );

                bag.AttributeValues = bag.AttributeValues?
                    .Where( kvp => allowedAttributeKeys.Contains( kvp.Key ) )
                    .ToDictionary( kvp => kvp.Key, kvp => kvp.Value );
            }
            else
            {
                bag.Attributes = null;
                bag.AttributeValues = null;
            }

            // Carry the per-attribute field visibility rules so the client can conditionally
            // show or hide attribute fields as their dependent values change.
            bag.AttributeVisibilityRules = GetRegistrantAttributeVisibilityRules( entity );

            // Surface the registrant's signature document (or a valid existing one found for
            // the person) in the uploader and carry its id key so the save step can reuse it.
            ApplySignatureDocumentInfo( bag, entity );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( RegistrationRegistrant entity, ValidPropertiesBox<RegistrantBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            // For new registrants, resolve and set RegistrationId first since other
            // operations (e.g. attribute key lookup) depend on it.
            if ( entity.Id == 0 )
            {
                var registrationKey = PageParameter( PageParameterKey.RegistrationId );

                if ( registrationKey.IsNullOrWhiteSpace() )
                {
                    registrationKey = box.Bag.RegistrationIdKey;
                }

                var registrationId = registrationKey.IsNotNullOrWhiteSpace()
                    ? new RegistrationService( RockContext )
                        .Get( registrationKey, !PageCache.Layout.Site.DisablePredictableIds )
                        ?.Id
                    : null;

                if ( !registrationId.HasValue || registrationId.Value == 0 )
                {
                    return false;
                }

                entity.RegistrationId = registrationId.Value;
                entity.RegistrationTemplateId = new RegistrationService( RockContext )
                    .GetSelect( entity.RegistrationId, r => r.RegistrationTemplateId ) ?? 0;
            }

            box.IfValidProperty( nameof( box.Bag.PersonAlias ), () =>
            {
                if ( box.Bag.PersonAlias?.Value.IsNotNullOrWhiteSpace() == true )
                {
                    var personAlias = new PersonAliasService( RockContext )
                        .Get( box.Bag.PersonAlias.Value, !PageCache.Layout.Site.DisablePredictableIds );

                    entity.PersonAliasId = personAlias?.Id;
                }
            } );

            box.IfValidProperty( nameof( box.Bag.Cost ),
                () => entity.Cost = box.Bag.Cost );

            box.IfValidProperty( nameof( box.Bag.DiscountApplies ),
                () => entity.DiscountApplies = box.Bag.DiscountApplies );

            box.IfValidProperty( nameof( box.Bag.IsOnWaitList ),
                () => entity.OnWaitList = box.Bag.IsOnWaitList );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ), () =>
            {
                entity.LoadAttributes( RockContext );

                var allowedAttributeKeys = GetRegistrantAttributeKeys( entity );
                var filteredValues = box.Bag.AttributeValues?
                    .Where( kvp => allowedAttributeKeys.Contains( kvp.Key ) )
                    .ToDictionary( kvp => kvp.Key, kvp => kvp.Value );

                entity.SetPublicAttributeValues( filteredValues, RequestContext.CurrentPerson, enforceSecurity: false );
            } );

            return true;
        }

        /// <inheritdoc/>
        protected override RegistrationRegistrant GetInitialEntity()
        {
            var registrantParameter = PageParameter( PageParameterKey.RegistrantId );

            var isNewRegistrant = registrantParameter.IsNullOrWhiteSpace() || registrantParameter == "0";

            // New registrant - Get RegistrationId from Page Parameter to get Fee Template data
            if ( isNewRegistrant )
            {
                var registrationParameter = PageParameter( PageParameterKey.RegistrationId );

                if ( registrationParameter.IsNullOrWhiteSpace() )
                {
                    return null;
                }

                // Even on new Registrant, we want to cache the nav properties so when we get info in edit bag we don't fire off more queries.
                var registration = new RegistrationService( RockContext )
                    .GetQueryableByKey( registrationParameter, !PageCache.Layout.Site.DisablePredictableIds )
                    .Include( r => r.RegistrationInstance.RegistrationTemplate.Forms.Select( f => f.Fields ) )
                    .Include( r => r.RegistrationInstance.RegistrationTemplate.Fees )
                    .Include( r => r.RegistrationInstance.RegistrationTemplate.RequiredSignatureDocumentTemplate.BinaryFileType )
                    .FirstOrDefault();

                if ( registration == null )
                {
                    return null;
                }

                var template = registration.RegistrationInstance?.RegistrationTemplate;

                var cost = template?.SetCostOnInstance == true
                    ? registration.RegistrationInstance?.Cost ?? 0m
                    : template?.Cost ?? 0m;

                return new RegistrationRegistrant
                {
                    RegistrationId = registration.Id,
                    RegistrationTemplateId = template?.Id ?? 0,
                    Cost = cost,
                    Registration = registration
                };
            }

            return new RegistrationRegistrantService( RockContext )
                .GetQueryableByKey( registrantParameter, !PageCache.Layout.Site.DisablePredictableIds )
                .Include( r => r.Registration.RegistrationInstance.RegistrationTemplate.Forms.Select( f => f.Fields ) )
                .Include( r => r.Registration.RegistrationInstance.RegistrationTemplate.RequiredSignatureDocumentTemplate.BinaryFileType )
                .Include( r => r.PersonAlias.Person )
                .Include( r => r.Fees )
                .Include( r => r.SignatureDocument.BinaryFile )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the box navigation URLs for the wizard breadcrumb and the cancel/save redirect.
        /// Traverses the page hierarchy to build URLs for the registration, instance, and template pages.
        /// </summary>
        private Dictionary<string, string> GetBoxNavigationUrls( RegistrationRegistrant entity )
        {
            var registrationId = entity?.RegistrationId ?? 0;

            var parentParams = registrationId > 0
                ? new Dictionary<string, string> { [PageParameterKey.RegistrationId] = registrationId.ToString() }
                : null;

            var returnUrl = PageParameter( PageParameterKey.ReturnUrl );
            var urls = new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = returnUrl.IsNotNullOrWhiteSpace() ? returnUrl : this.GetParentPageUrl( parentParams )
            };

            var instancePage = PageCache.ParentPage?.ParentPage;
            var templatePage = PageCache.ParentPage?.ParentPage?.ParentPage;

            var instanceId = entity?.Registration?.RegistrationInstanceId ?? 0;
            var templateId = entity?.Registration?.RegistrationInstance?.RegistrationTemplateId ?? 0;

            if ( instancePage != null && instanceId > 0 )
            {
                urls[NavigationUrlKey.RegistrationInstancePage] = new Rock.Web.PageReference(
                    instancePage.Id, 0,
                    new Dictionary<string, string> { ["RegistrationInstanceId"] = instanceId.ToString() }
                ).BuildUrl();
            }

            if ( templatePage != null && templateId > 0 )
            {
                urls[NavigationUrlKey.RegistrationTemplatePage] = new Rock.Web.PageReference(
                    templatePage.Id, 0,
                    new Dictionary<string, string> { ["RegistrationTemplateId"] = templateId.ToString() }
                ).BuildUrl();
            }

            return urls;
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out RegistrationRegistrant entity, out BlockActionResult error )
        {
            var entityService = new RegistrationRegistrantService( RockContext );
            error = null;

            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                entity = new RegistrationRegistrant();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{RegistrationRegistrant.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit {RegistrationRegistrant.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the set of attribute keys the block may display and save, limited to
        /// <see cref="RegistrationFieldSource.RegistrantAttribute"/> fields defined on the
        /// registration template forms.
        /// </summary>
        private HashSet<string> GetRegistrantAttributeKeys( RegistrationRegistrant entity )
        {
            var template = GetRegistration( entity )?.RegistrationInstance?.RegistrationTemplate;

            if ( template?.Forms == null )
            {
                return new HashSet<string>();
            }

            return template.Forms
                .SelectMany( f => f.Fields )
                .Where( f => f.FieldSource == RegistrationFieldSource.RegistrantAttribute && f.AttributeId.HasValue )
                .Select( f => AttributeCache.Get( f.AttributeId.Value )?.Key )
                .Where( k => k != null )
                .ToHashSet();
        }

        /// <summary>
        /// Builds the registrant-attribute field visibility rules, translated from the form fields'
        /// internal Guid references into attribute keys so the client can evaluate them directly
        /// against the attribute values it already holds.
        /// </summary>
        /// <returns>
        /// A dictionary keyed by the governed attribute key, or <c>null</c> when no registrant
        /// attribute has any evaluable visibility rules.
        /// </returns>
        private Dictionary<string, RegistrantAttributeVisibilityBag> GetRegistrantAttributeVisibilityRules( RegistrationRegistrant entity )
        {
            var template = GetRegistration( entity )?.RegistrationInstance?.RegistrationTemplate;

            if ( template?.Forms == null )
            {
                return null;
            }

            var registrantAttributeFields = template.Forms
                .SelectMany( f => f.Fields )
                .Where( f => f.FieldSource == RegistrationFieldSource.RegistrantAttribute && f.AttributeId.HasValue )
                .ToList();

            // Map each registrant-attribute field's Guid to its attribute so rules, which reference
            // other fields by Guid, can be resolved into the attribute keys the client understands.
            var attributeByFieldGuid = new Dictionary<Guid, AttributeCache>();
            foreach ( var field in registrantAttributeFields )
            {
                var attribute = AttributeCache.Get( field.AttributeId.Value );
                if ( attribute != null )
                {
                    attributeByFieldGuid[field.Guid] = attribute;
                }
            }

            var visibilityRules = new Dictionary<string, RegistrantAttributeVisibilityBag>();

            foreach ( var field in registrantAttributeFields )
            {
                var ruleList = field.FieldVisibilityRules?.RuleList;
                if ( ruleList == null || ruleList.Count == 0 )
                {
                    continue;
                }

                var governedAttribute = attributeByFieldGuid.GetValueOrNull( field.Guid );
                if ( governedAttribute == null )
                {
                    continue;
                }

                var ruleBags = new List<RegistrantAttributeVisibilityRuleBag>();

                foreach ( var rule in ruleList )
                {
                    if ( !rule.ComparedToFormFieldGuid.HasValue )
                    {
                        continue;
                    }

                    var comparedToAttribute = attributeByFieldGuid.GetValueOrNull( rule.ComparedToFormFieldGuid.Value );
                    if ( comparedToAttribute == null )
                    {
                        // The rule compares to a field this block does not render (e.g. a person
                        // field). It cannot be evaluated client-side, so skip it.
                        continue;
                    }

                    var ruleBag = Rock.Field.FieldVisibilityRule.GetPublicRuleBag( comparedToAttribute, rule.ComparisonType, rule.ComparedToValue );
                    if ( ruleBag == null )
                    {
                        continue;
                    }

                    ruleBags.Add( new RegistrantAttributeVisibilityRuleBag
                    {
                        ComparedToAttributeKey = comparedToAttribute.Key,
                        ComparisonValue = new PublicComparisonValueBag
                        {
                            ComparisonType = ( int? ) ruleBag.ComparisonType,
                            Value = ruleBag.Value
                        }
                    } );
                }

                if ( ruleBags.Count == 0 )
                {
                    continue;
                }

                visibilityRules[governedAttribute.Key] = new RegistrantAttributeVisibilityBag
                {
                    FilterExpressionType = field.FieldVisibilityRules.FilterExpressionType,
                    Rules = ruleBags
                };
            }

            return visibilityRules.Count > 0 ? visibilityRules : null;
        }

        /// <summary>
        /// Populates the signature-document-related bag fields. The effective document is the one
        /// already attached to the registrant or, when none is attached, a still-valid document the
        /// person previously signed for the same template. This surfaces the document's file in the
        /// uploader and carries its id key so the save step can reuse the document (replacing its
        /// file, or cloning it when shared) instead of creating a duplicate.
        /// </summary>
        private void ApplySignatureDocumentInfo( RegistrantBag bag, RegistrationRegistrant entity )
        {
            var template = GetRegistration( entity )?.RegistrationInstance?.RegistrationTemplate;

            if ( template?.RequiredSignatureDocumentTemplate == null )
            {
                return;
            }

            var signatureDocument = entity.SignatureDocument;

            // When the registrant is not yet linked to a document, fall back to any still-valid
            // document the person previously signed for this template so it can be reused.
            if ( signatureDocument == null && entity.PersonAlias != null )
            {
                signatureDocument = new RegistrationRegistrantService( RockContext )
                    .GetValidSignatureDocument( entity.PersonAlias.PersonId, template.RequiredSignatureDocumentTemplate )
                    .FirstOrDefault();

                // Only the found (not-yet-attached) case shows the informational notice.
                bag.HasExistingSignatureDocument = signatureDocument != null;
            }

            if ( signatureDocument == null )
            {
                return;
            }

            bag.SignatureDocumentIdKey = signatureDocument.IdKey;
            bag.SignatureDocumentBinaryFileId = signatureDocument.BinaryFile?.Guid;
            bag.SignatureDocumentBinaryFileName = signatureDocument.BinaryFile?.FileName;
        }

        /// <summary>
        /// Returns the <see cref="Registration"/> for this registrant, loading it from the
        /// database with required navigation properties if the property is not already populated.
        /// </summary>
        private Registration GetRegistration( RegistrationRegistrant entity )
        {
            if ( entity == null )
            {
                return null;
            }

            // Return the cached registration when it matches the requested one. A non-zero
            // _registrationId means a load has already been attempted for that registration.
            if ( _registrationId != 0 && _registrationId == entity.RegistrationId )
            {
                return _registration;
            }

            if ( entity.RegistrationId == 0 )
            {
                return null;
            }

            _registrationId = entity.RegistrationId;

            if ( entity.Registration?.RegistrationInstance?.RegistrationTemplate != null )
            {
                _registration = entity.Registration;
                return _registration;
            }

            _registration = new RegistrationService( RockContext )
                .Queryable()
                .Include( r => r.RegistrationInstance.RegistrationTemplate.Forms.Select( f => f.Fields ) )
                .Include( r => r.RegistrationInstance.RegistrationTemplate.Fees )
                .Include( r => r.RegistrationInstance.RegistrationTemplate.RequiredSignatureDocumentTemplate.BinaryFileType )
                .FirstOrDefault( r => r.Id == entity.RegistrationId );

            return _registration;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Saves the registrant and redirects to the parent registration detail page.
        /// </summary>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<RegistrantBag> box )
        {
            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var isNew = entity.Id == 0;

            var personAliasService = new PersonAliasService( RockContext );
            var registrantService = new RegistrationRegistrantService( RockContext );
            var registrantFeeService = new RegistrationRegistrantFeeService( RockContext );

            var registrantChanges = new History.HistoryChangeList();

            if ( isNew )
            {
                registrantChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Registrant" );
            }

            // Capture pre-update state before UpdateEntityFromBox overwrites it.
            var originalPersonAliasId = entity.PersonAliasId;
            var originalOnWaitList = entity.OnWaitList;

            var originalPersonName = entity.PersonAliasId.HasValue
                ? personAliasService.GetPerson( entity.PersonAliasId.Value ).FullName
                : string.Empty;

            entity.LoadAttributes( RockContext );
            var originalAttributeValues = entity.Attributes?.Keys
                .ToDictionary( key => key, key => entity.GetAttributeValue( key ) )
                ?? new Dictionary<string, string>();

            History.EvaluateChange( registrantChanges, "Cost", entity.Cost, box.Bag.Cost );
            History.EvaluateChange( registrantChanges, "Discount Applies", entity.DiscountApplies, box.Bag.DiscountApplies );

            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            if ( !ValidateRegistrationRegistrant( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var previousRegistrantPersonIds = registrantService.Queryable()
                .Where( r => r.RegistrationId == entity.RegistrationId && r.PersonAlias != null )
                .Select( r => r.PersonAlias.PersonId )
                .ToList();

            var newPerson = entity.PersonAliasId.HasValue
                ? personAliasService.GetPerson( entity.PersonAliasId.Value )
                : null;

            var personChanged = !originalPersonAliasId.Equals( entity.PersonAliasId );

            if ( personChanged )
            {
                History.EvaluateChange( registrantChanges, "Person", originalPersonName, newPerson?.FullName ?? string.Empty );
            }

            var registration = GetRegistration( entity );
            var template = registration?.RegistrationInstance?.RegistrationTemplate;

            // ── Attribute History ─────────────────────────────────────────────
            // Build history before the transaction using the private stored values that
            // UpdateEntityFromBox already converted and placed on the entity. Comparing
            // against the raw public bag values would produce incorrect diff text and
            // an incorrect equality check for field types whose public and private
            // representations differ (e.g. Defined Value stores Id, sends Guid).
            var allowedAttributeKeys = GetRegistrantAttributeKeys( entity );
            foreach ( var key in allowedAttributeKeys )
            {
                var attribute = entity.Attributes?.GetValueOrNull( key );
                if ( attribute == null )
                {
                    continue;
                }

                string originalValue = originalAttributeValues.GetValueOrNull( key ) ?? string.Empty;
                string newValue = entity.GetAttributeValue( key ) ?? string.Empty;

                if ( originalValue.Trim() != newValue.Trim() )
                {
                    string formattedOriginal = originalValue.IsNotNullOrWhiteSpace()
                        ? attribute.FieldType.Field.FormatValue( null, originalValue, attribute.QualifierValues, false )
                        : string.Empty;

                    string formattedNew = newValue.IsNotNullOrWhiteSpace()
                        ? attribute.FieldType.Field.FormatValue( null, newValue, attribute.QualifierValues, false )
                        : string.Empty;

                    History.EvaluateChange( registrantChanges, attribute.Name, formattedOriginal, formattedNew );
                }
            }

            // ── Fees ──────────────────────────────────────────────────────────

            // Pre-load all fee names needed for history in a single query to
            // avoid one DB round-trip per fee inside the loops below.
            var existingFees = entity.Fees ?? Enumerable.Empty<RegistrationRegistrantFee>();

            var allFeeIds = ( box.Bag.Fees?.Select( f => f.RegistrationTemplateFeeId ) ?? Enumerable.Empty<int>() )
                .Union( existingFees.Select( f => f.RegistrationTemplateFeeId ) )
                .Distinct()
                .ToList();

            var templateFeeNames = new RegistrationTemplateFeeService( RockContext )
                .Queryable()
                .Where( f => allFeeIds.Contains( f.Id ) )
                .Select( f => new { f.Id, f.Name } )
                .ToDictionary( f => f.Id, f => f.Name );

            // Pre-load fee item option names so new fees get the authoritative name
            // from the database rather than trusting the client-supplied value.
            var allFeeItemIds = ( box.Bag.Fees?
                    .Where( f => f.RegistrationTemplateFeeItemId.HasValue )
                    .Select( f => f.RegistrationTemplateFeeItemId.Value ) ?? Enumerable.Empty<int>() )
                .Union( existingFees
                    .Where( f => f.RegistrationTemplateFeeItemId.HasValue )
                    .Select( f => f.RegistrationTemplateFeeItemId.Value ) )
                .Distinct()
                .ToList();

            var templateFeeItemOptions = new RegistrationTemplateFeeItemService( RockContext )
                .Queryable()
                .Where( i => allFeeItemIds.Contains( i.Id ) )
                .Select( i => new { i.Id, i.Name } )
                .ToDictionary( i => i.Id, i => i.Name );

            foreach ( var dbFee in existingFees.ToList() )
            {
                var uiFee = box.Bag.Fees?.FirstOrDefault( f =>
                    f.RegistrationTemplateFeeId == dbFee.RegistrationTemplateFeeId &&
                    f.RegistrationTemplateFeeItemId == dbFee.RegistrationTemplateFeeItemId &&
                    f.Quantity > 0 );

                if ( uiFee == null )
                {
                    var feeName = templateFeeNames.GetValueOrDefault( dbFee.RegistrationTemplateFeeId, "Fee" );
                    var oldValue = $"'{feeName}' Fee (Quantity:{dbFee.Quantity:N0}, Cost:{dbFee.Cost:C2}, Option:{dbFee.Option}";
                    registrantChanges.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Fee" ).SetOldValue( oldValue );
                    entity.Fees.Remove( dbFee );
                    registrantFeeService.Delete( dbFee );
                }
            }

            foreach ( var uiFee in box.Bag.Fees ?? new List<RegistrantFeeBag>() )
            {
                if ( uiFee.Quantity <= 0 )
                {
                    continue;
                }

                var dbFee = entity.Fees.FirstOrDefault( f =>
                    f.RegistrationTemplateFeeId == uiFee.RegistrationTemplateFeeId &&
                    f.RegistrationTemplateFeeItemId == uiFee.RegistrationTemplateFeeItemId );

                if ( dbFee == null )
                {
                    var option = uiFee.RegistrationTemplateFeeItemId.HasValue
                        ? templateFeeItemOptions.GetValueOrDefault( uiFee.RegistrationTemplateFeeItemId.Value, null )
                        : null;

                    dbFee = new RegistrationRegistrantFee
                    {
                        RegistrationTemplateFeeId = uiFee.RegistrationTemplateFeeId,
                        RegistrationTemplateFeeItemId = uiFee.RegistrationTemplateFeeItemId,
                        Option = option
                    };

                    entity.Fees.Add( dbFee );
                }

                var feeName = templateFeeNames.GetValueOrDefault( uiFee.RegistrationTemplateFeeId, "Fee" );

                if ( uiFee.Option.IsNotNullOrWhiteSpace() )
                {
                    feeName = $"{feeName} ({uiFee.Option})";
                }

                if ( dbFee.Id <= 0 )
                {
                    registrantChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Fee" ).SetNewValue( feeName );
                }

                History.EvaluateChange( registrantChanges, $"{feeName} Quantity", dbFee.Quantity, uiFee.Quantity );
                dbFee.Quantity = uiFee.Quantity;

                History.EvaluateChange( registrantChanges, $"{feeName} Cost", dbFee.Cost, uiFee.Cost );
                dbFee.Cost = uiFee.Cost;
            }

            // ── Fee Item Capacity Validation ──────────────────────────────────

            // Guard against two concurrent saves both claiming the last available
            // slot for a fee item that has a MaximumUsageCount.
            var feeCapacityError = ValidateFeeItemCapacity( entity, box.Bag.Fees, registration?.RegistrationInstanceId ?? 0 );
            if ( feeCapacityError != null )
            {
                return ActionBadRequest( feeCapacityError );
            }

            // ── Signature Document ────────────────────────────────────────────

            if ( template?.RequiredSignatureDocumentTemplate != null && entity.PersonAliasId.HasValue && newPerson != null )
            {
                // Resolve the document the form was showing (the registrant's own or a valid
                // existing one found for the person) so an uploaded replacement reuses it
                // instead of creating a duplicate. Mirrors the WebForms hfSignedDocumentId
                // round-trip; falls back to the registrant's own document id if none was sent.
                var existingDocumentId = entity.SignatureDocumentId;

                if ( box.Bag.SignatureDocumentIdKey.IsNotNullOrWhiteSpace() )
                {
                    existingDocumentId = new SignatureDocumentService( RockContext )
                        .Get( box.Bag.SignatureDocumentIdKey, !PageCache.Layout.Site.DisablePredictableIds )?.Id
                        ?? entity.SignatureDocumentId;
                }

                HandleSignatureDocument( entity, template, newPerson.Id, existingDocumentId, box.Bag.SignatureDocumentBinaryFileId );
            }

            // ── Registration Session ──────────────────────────────────────────

            Guid? registrationSessionGuid = null;

            if ( registration == null )
            {
                return ActionBadRequest( "Registration not found." );
            }

            var registrationInstance = new RegistrationInstanceService( RockContext ).Get( registration.RegistrationInstanceId );

            if ( registrationInstance?.TimeoutIsEnabled == true )
            {
                // Reserve a spot when adding a new registrant directly (not to the wait
                // list), or when an existing wait-list registrant is being promoted off it.
                bool isMovingOffWaitList = !isNew && originalOnWaitList && !entity.OnWaitList;
                bool needsCapacityCheck = ( isNew && !entity.OnWaitList ) || isMovingOffWaitList;

                if ( needsCapacityCheck )
                {
                    var registrationSession = CreateRegistrationSession( registration );
                    if ( registrationSession == null )
                    {
                        return ActionBadRequest( "Registration is full." );
                    }

                    registrationSessionGuid = registrationSession.Guid;
                }
            }

            // ── Save ──────────────────────────────────────────────────────────

            string registrantName = newPerson?.FullName ?? "Unknown";

            try
            {
                RockContext.WrapTransaction( () =>
                {
                    RockContext.SaveChanges();
                    entity.SaveAttributeValues( RockContext );
                } );

                if ( ( isNew || personChanged ) && template?.GroupTypeId.HasValue == true && newPerson != null )
                {
                    HandleGroupMember( entity, registration, template, newPerson.Id, registrantChanges );
                }

                if ( ( isNew || personChanged ) && template?.GroupTypeId.HasValue == true )
                {
                    if ( registration.FirstName.IsNotNullOrWhiteSpace() && registration.LastName.IsNotNullOrWhiteSpace() )
                    {
                        registration.SavePersonNotesAndHistory(
                            registration.FirstName,
                            registration.LastName,
                            RequestContext.CurrentPerson?.PrimaryAliasId,
                            previousRegistrantPersonIds );
                    }
                }

                HistoryService.SaveChanges(
                    RockContext,
                    typeof( Registration ),
                    Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                    entity.RegistrationId,
                    registrantChanges,
                    "Registrant: " + registrantName,
                    null,
                    null );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                return ActionBadRequest( ex.Message );
            }
            finally
            {
                if ( registrationSessionGuid.HasValue )
                {
                    RegistrationSessionService.CloseAndRemoveSession( registrationSessionGuid.Value );
                }
            }

            var registrationId = entity.RegistrationId;
            var parentParams = registrationId > 0
                ? new Dictionary<string, string> { [PageParameterKey.RegistrationId] = registrationId.ToString() }
                : null;

            var returnUrl = PageParameter( PageParameterKey.ReturnUrl );
            return ActionOk( returnUrl.IsNotNullOrWhiteSpace() ? returnUrl : this.GetParentPageUrl( parentParams ) );
        }

        #endregion Block Actions

        #region Private Helpers

        /// <summary>
        /// Checks whether any requested fee item would exceed its <see cref="RegistrationTemplateFeeItem.MaximumUsageCount"/>
        /// given the current usage across the registration instance. The current registrant's own
        /// existing fees are excluded so that updating a quantity is not counted twice.
        /// </summary>
        /// <returns>
        /// A user-facing error message if any item is over-limit; otherwise <c>null</c>.
        /// </returns>
        private string ValidateFeeItemCapacity( RegistrationRegistrant entity, List<RegistrantFeeBag> requestedFees, int registrationInstanceId )
        {
            if ( registrationInstanceId == 0 )
            {
                return null;
            }

            // Collect all fee items being requested that have a positive quantity.
            var requestedItems = requestedFees?
                .Where( f => f.Quantity > 0 && f.RegistrationTemplateFeeItemId.HasValue )
                .GroupBy( f => f.RegistrationTemplateFeeItemId.Value )
                .Select( g => new { FeeItemId = g.Key, Quantity = g.Sum( f => f.Quantity ) } )
                .ToList();

            if ( requestedItems == null || !requestedItems.Any() )
            {
                return null;
            }

            var requestedItemIds = requestedItems.Select( f => f.FeeItemId ).ToList();

            // Only fee items that have an enforced cap require validation.
            var limitedItems = new RegistrationTemplateFeeItemService( RockContext )
                .Queryable()
                .Where( i => requestedItemIds.Contains( i.Id ) && i.MaximumUsageCount.HasValue )
                .Select( i => new { i.Id, i.Name, i.MaximumUsageCount } )
                .ToList();

            if ( !limitedItems.Any() )
            {
                return null;
            }

            var limitedItemIds = limitedItems.Select( i => i.Id ).ToList();

            // dictionary of fee item id to number of items already taken
            var currentUsage = new RegistrationRegistrantFeeService( RockContext )
                .Queryable()
                .Where( f =>
                    f.RegistrationTemplateFeeItemId.HasValue &&
                    limitedItemIds.Contains( f.RegistrationTemplateFeeItemId.Value ) &&
                    f.RegistrationRegistrant.Registration.RegistrationInstanceId == registrationInstanceId && f.RegistrationRegistrantId != entity.Id )
                .GroupBy( f => f.RegistrationTemplateFeeItemId.Value )
                .Select( g => new { FeeItemId = g.Key, UsedCount = g.Sum( f => f.Quantity ) } )
                .ToDictionary( x => x.FeeItemId, x => x.UsedCount );

            foreach ( var item in limitedItems )
            {
                var requested = requestedItems.FirstOrDefault( f => f.FeeItemId == item.Id )?.Quantity ?? 0;
                var used = currentUsage.GetValueOrDefault( item.Id, 0 );

                if ( used + requested > item.MaximumUsageCount.Value )
                {
                    var remaining = Math.Max( 0, item.MaximumUsageCount.Value - used );
                    return $"The '{item.Name}' fee option only has {remaining:N0} spot(s) remaining and cannot be saved as requested.";
                }
            }

            return null;
        }

        /// <summary>
        /// Handles all signature document logic for the registrant: attaches an existing document,
        /// creates a new one from an uploaded file, or finds a valid existing document for the person
        /// when no file is provided.
        /// </summary>
        private void HandleSignatureDocument( RegistrationRegistrant entity, RegistrationTemplate template, int personId, int? existingDocumentId, Guid? newBinaryFileGuid )
        {
            var documentService = new SignatureDocumentService( RockContext );
            var binaryFileService = new BinaryFileService( RockContext );
            SignatureDocument document = null;

            // Resolve the uploaded file's Guid to its integer Id so the rest of the
            // method can work with the BinaryFileId used by SignatureDocument.
            int? newBinaryFileId = null;
            if ( newBinaryFileGuid.HasValue )
            {
                newBinaryFileId = binaryFileService.Queryable()
                    .Where( f => f.Guid == newBinaryFileGuid.Value )
                    .Select( f => ( int? ) f.Id )
                    .FirstOrDefault();
            }

            if ( existingDocumentId.HasValue )
            {
                document = documentService.Get( existingDocumentId.Value );
                if ( entity.SignatureDocument == null )
                {
                    entity.SignatureDocument = document;
                }
            }

            if ( document == null && newBinaryFileId.HasValue )
            {
                document = CreateSignatureDocument( documentService, entity, template, personId, newBinaryFileId );
            }
            else if ( document == null )
            {
                var existingId = new RegistrationRegistrantService( RockContext )
                    .GetValidSignatureDocument( personId, template.RequiredSignatureDocumentTemplate )
                    .Select( d => ( int? ) d.Id )
                    .FirstOrDefault();

                if ( existingId.HasValue )
                {
                    entity.SignatureDocumentId = existingId;
                }
            }

            if ( document != null && newBinaryFileId.HasValue && document.BinaryFileId != newBinaryFileId.Value )
            {
                var registrantsUsing = new RegistrationRegistrantService( RockContext )
                    .GetRegistrantsUsingSignatureDocument( document.Id );

                if ( registrantsUsing.Count() > 1 )
                {
                    document = CreateSignatureDocument( documentService, entity, template, personId, newBinaryFileId );
                }
                else
                {
                    int? origBinaryFileId = document.BinaryFileId;
                    if ( origBinaryFileId.HasValue )
                    {
                        var oldFile = binaryFileService.Get( origBinaryFileId.Value );
                        if ( oldFile != null && !oldFile.IsTemporary )
                        {
                            oldFile.IsTemporary = true;
                        }
                    }

                    document.BinaryFileId = newBinaryFileId;
                }
            }

            if ( document?.BinaryFileId.HasValue == true )
            {
                var binaryFile = binaryFileService.Get( document.BinaryFileId.Value );
                if ( binaryFile != null && binaryFile.IsTemporary )
                {
                    binaryFile.IsTemporary = false;
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="SignatureDocument"/> for the registrant using the uploaded binary file.
        /// </summary>
        private SignatureDocument CreateSignatureDocument( SignatureDocumentService documentService, RegistrationRegistrant entity, RegistrationTemplate template, int personId, int? binaryFileId )
        {
            var registration = GetRegistration( entity );
            var person = new PersonService( RockContext ).Get( personId );

            var document = new SignatureDocument
            {
                SignatureDocumentTemplateId = template.RequiredSignatureDocumentTemplate.Id,
                AppliesToPersonAliasId = entity.PersonAliasId.Value,
                AssignedToPersonAliasId = entity.PersonAliasId.Value,
                Name = $"{person?.FullName.ReplaceSpecialCharacters( " " )} ({registration?.RegistrationInstance?.Name ?? template.Name})",
                Status = SignatureDocumentStatus.Signed,
                LastStatusDate = RockDateTime.Now,
                SignedDateTime = RockDateTime.Now,
                BinaryFileId = binaryFileId
            };

            documentService.Add( document );
            entity.SignatureDocument = document;
            return document;
        }

        /// <summary>
        /// Adds or updates the group member record for a new or re-assigned registrant when
        /// the registration template is linked to a group type.
        /// </summary>
        private void HandleGroupMember( RegistrationRegistrant entity, Registration registration, RegistrationTemplate template, int personId, History.HistoryChangeList registrantChanges )
        {
               var reloadedRegistrant = new RegistrationRegistrantService( RockContext )
                    .Queryable()
                    .Include( r => r.Registration.Group.GroupType )
                    .Include( r => r.GroupMember )
                    .FirstOrDefault( r => r.Id == entity.Id );

                if ( reloadedRegistrant?.Registration?.Group?.GroupTypeId != template.GroupTypeId.Value )
                {
                    return;
                }

                int? groupRoleId = template.GroupMemberRoleId
                    ?? reloadedRegistrant.Registration.Group.GroupType.DefaultGroupRoleId;

                if ( !groupRoleId.HasValue )
                {
                    return;
                }

                var group = reloadedRegistrant.Registration.Group;
                var groupMemberService = new GroupMemberService( RockContext );

                var groupMember = groupMemberService.Queryable().FirstOrDefault( m =>
                    m.GroupId == group.Id &&
                    m.PersonId == personId &&
                    m.GroupRoleId == groupRoleId.Value );

                if ( groupMember == null )
                {
                    groupMember = new GroupMember
                    {
                        GroupId = group.Id,
                        PersonId = personId,
                        GroupRoleId = groupRoleId.Value,
                        GroupMemberStatus = template.GroupMemberStatus
                    };

                    groupMemberService.Add( groupMember );

                    RockContext.SaveChanges();

                    registrantChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, $"Registrant to {group.Name} group" );
                }
                else
                {
                    registrantChanges.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Record, $"Registrant to existing person in {group.Name} group" );
                }

                groupMember.GroupMemberStatus = template.GroupMemberStatus;

                if ( reloadedRegistrant.GroupMemberId.HasValue && reloadedRegistrant.GroupMemberId.Value != groupMember.Id )
                {
                    groupMemberService.Delete( reloadedRegistrant.GroupMember );
                    RockContext.SaveChanges();
                    registrantChanges.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, $"Registrant to previous person in {group.Name} group" );
                }

                reloadedRegistrant.GroupMemberId = groupMember.Id;
                RockContext.SaveChanges();
        }

        /// <summary>
        /// Creates a transient <see cref="RegistrationSession"/> to reserve a spot during save.
        /// Returns <c>null</c> if the registration is full.
        /// </summary>
        private RegistrationSession CreateRegistrationSession( Registration registration )
        {
            var session = RegistrationSessionService.CreateOrUpdateSession(
                Guid.Empty,
                () => new RegistrationSession
                {
                    Guid = Guid.NewGuid(),
                    RegistrationInstanceId = registration.RegistrationInstanceId,
                    RegistrationData = string.Empty,
                    SessionStartDateTime = RockDateTime.Now,
                    RegistrationCount = 1,
                    RegistrationId = registration.Id,
                    SessionStatus = SessionStatus.Transient
                },
                null,
                out string errorMessage );

            return errorMessage.IsNotNullOrWhiteSpace() ? null : session;
        }

        #endregion Private Helpers
    }
}
