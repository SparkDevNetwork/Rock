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

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.BenevolenceTypeDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays the details of a particular benevolence type.
    /// </summary>

    [DisplayName( "Benevolence Type Detail" )]
    [Category( "Finance" )]
    [Description("Block to display the benevolence type detail.")]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [AttributeField(
        "Benevolence Type Attributes",
        Key = AttributeKey.BenevolenceTypeAttributes,
        EntityTypeGuid = Rock.SystemGuid.EntityType.BENEVOLENCE_TYPE,
        Description = "The attributes that should be displayed / edited for benevolence types.",
        IsRequired = false,
        AllowMultiple = true,
        Order = 1 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "b39ba58d-83dd-46e0-ba47-787c4eb4eb69" )]
    [Rock.SystemGuid.BlockTypeGuid( "03397615-ef2b-4d33-bd62-a79186f56ace" )]
    public class BenevolenceTypeDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string BenevolenceTypeId = "BenevolenceTypeId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        private static class AttributeKey
        {
            public const string BenevolenceTypeAttributes = "BenevolenceTypeAttributes";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<BenevolenceTypeBag, BenevolenceTypeDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private BenevolenceTypeDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new BenevolenceTypeDetailOptionsBag();

            options.Statuses = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.BENEVOLENCE_REQUEST_STATUS.AsGuid() ).DefinedValues.ToListItemBagList();
            options.TriggerTypes = ToListItemBag( typeof( BenevolenceWorkflowTriggerType ) );

            return options;
        }

        /// <summary>
        /// Converts the enum to a List of ListItemBag items.
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <returns></returns>
        private List<ListItemBag> ToListItemBag( Type enumType )
        {
            var listItemBag = new List<ListItemBag>();
            foreach ( Enum enumValue in Enum.GetValues( enumType ) )
            {
                var text = enumValue.GetDescription() ?? enumValue.ToString().SplitCase();
                var value = enumValue.ToString().SplitCase();
                listItemBag.Add( new ListItemBag { Text = text, Value = value } );
            }

            return listItemBag.ToList();
        }

        /// <summary>
        /// Validates the BenevolenceType for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="benevolenceType">The BenevolenceType to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the BenevolenceType is valid, <c>false</c> otherwise.</returns>
        private bool ValidateBenevolenceType( BenevolenceType benevolenceType, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            if ( benevolenceType.Id == 0 )
            {
                // Check for existing
                var existingBenevolence = new BenevolenceTypeService( rockContext ).Queryable()
                    .Where( d => d.Name == benevolenceType.Name )
                    .FirstOrDefault();

                if ( existingBenevolence != null )
                {
                    errorMessage = $"A benevolence type already exists with the name '{existingBenevolence.Name}'. Please use a different benevolence type name.";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<BenevolenceTypeBag, BenevolenceTypeDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {BenevolenceType.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( BenevolenceType.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( BenevolenceType.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="BenevolenceTypeBag"/> that represents the entity.</returns>
        private BenevolenceTypeBag GetCommonEntityBag( BenevolenceType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new BenevolenceTypeBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                IsActive = entity.Id == 0 || entity.IsActive,
                Name = entity.Name,
                RequestLavaTemplate = entity.RequestLavaTemplate,
                ShowFinancialResults = entity.Id == 0 || entity.ShowFinancialResults,
                MaximumNumberOfDocuments = entity.AdditionalSettingsJson?.FromJsonOrNull<BenevolenceType.AdditionalSettings>()?.MaximumNumberOfDocuments,
                CanAdminstrate = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="BenevolenceTypeBag"/> that represents the entity.</returns>
        private BenevolenceTypeBag GetEntityBagForView( BenevolenceType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="BenevolenceTypeBag"/> that represents the entity.</returns>
        private BenevolenceTypeBag GetEntityBagForEdit( BenevolenceType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );
            bag.Workflows = entity.BenevolenceWorkflows.Select( w => ToWorkflowBag( w ) ).OrderByDescending( w => w.IsInherited )
            .ThenBy( w => w.WorkflowTypeName )
            .ToList();

            return bag;
        }

        /// <summary>
        /// Converts to workflowbag.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <returns></returns>
        private BenevolenceWorkflowBag ToWorkflowBag( BenevolenceWorkflow workflow )
        {
            if ( workflow == null )
            {
                return null;
            }
            else
            {
                return new BenevolenceWorkflowBag()
                {
                    BenevolenceTypeId = workflow.BenevolenceTypeId,
                    Guid = workflow.Guid,
                    WorkflowType = workflow.WorkflowType.ToListItemBag(),
                    Trigger = workflow.TriggerType.ConvertToString(),
                    WorkflowTypeName = workflow.BenevolenceTypeId > 0 ? workflow.WorkflowType.Name + " <span class='label label-default'>Inherited</span>" : workflow.WorkflowType.Name,
                    PrimaryQualifier = GetPrimaryQualifier( workflow.QualifierValue ),
                    SecondaryQualifier = GetSecondaryQualifier( workflow.QualifierValue ),
                    IsInherited = workflow.BenevolenceTypeId > 0,
                };
            }
        }

        /// <summary>
        /// Gets the secondary qualifier.
        /// </summary>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private string GetSecondaryQualifier( string qualifierValue )
        {
            var qualifierValues = qualifierValue.SplitDelimitedValues( "|" );
            var secondaryQualifier = string.Empty;

            if ( qualifierValues.Length > 2 && int.TryParse( qualifierValues[2], out int id ) )
            {
                secondaryQualifier = DefinedValueCache.GetGuid( id ).ToString();
            }

            return secondaryQualifier;
        }

        /// <summary>
        /// Gets the primary qualifier.
        /// </summary>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private string GetPrimaryQualifier( string qualifierValue )
        {
            var qualifierValues = qualifierValue.SplitDelimitedValues( "|" );
            var primaryQualifier = string.Empty;

            if ( qualifierValues.Length > 1 && int.TryParse( qualifierValues[1], out int id ) )
            {
                primaryQualifier = DefinedValueCache.GetGuid( id ).ToString();
            }

            return primaryQualifier;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( BenevolenceType entity, DetailBlockBox<BenevolenceTypeBag, BenevolenceTypeDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.RequestLavaTemplate ),
                () => entity.RequestLavaTemplate = box.Entity.RequestLavaTemplate );

            box.IfValidProperty( nameof( box.Entity.ShowFinancialResults ),
                () => entity.ShowFinancialResults = box.Entity.ShowFinancialResults );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="BenevolenceType"/> to be viewed or edited on the page.</returns>
        private BenevolenceType GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<BenevolenceType, BenevolenceTypeService>( rockContext, PageParameterKey.BenevolenceTypeId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = GetInitialEntity( rockContext );

                return GetSecurityGrantToken( entity );
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( BenevolenceType entity )
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out BenevolenceType entity, out BlockActionResult error )
        {
            var entityService = new BenevolenceTypeService( rockContext );
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
                entity = new BenevolenceType();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{BenevolenceType.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${BenevolenceType.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                var box = new DetailBlockBox<BenevolenceTypeBag, BenevolenceTypeDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<BenevolenceTypeBag, BenevolenceTypeDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var benevolenceWorkflowService = new BenevolenceWorkflowService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                var additionalSettings = entity.AdditionalSettingsJson?.FromJsonOrNull<BenevolenceType.AdditionalSettings>() ?? new BenevolenceType.AdditionalSettings();
                additionalSettings.MaximumNumberOfDocuments = box.Entity.MaximumNumberOfDocuments;
                entity.AdditionalSettingsJson = additionalSettings.ToJson();

                // remove any workflows that were removed in the UI
                var uiWorkflows = box.Entity.Workflows.Select( l => l.Guid );

                foreach ( var benevolenceWorkflow in entity.BenevolenceWorkflows.Where( l => !uiWorkflows.Contains( l.Guid ) ).ToList() )
                {
                    entity.BenevolenceWorkflows.Remove( benevolenceWorkflow );
                    benevolenceWorkflowService.Delete( benevolenceWorkflow );
                }

                // Add or Update workflows from the UI
                foreach ( var workflowBag in box.Entity.Workflows )
                {
                    BenevolenceWorkflow benevolenceWorkflow = entity.BenevolenceWorkflows
                        .FirstOrDefault( b => !workflowBag.Guid.Equals( Guid.Empty ) && b.Guid == workflowBag.Guid );

                    if ( benevolenceWorkflow == null )
                    {
                        benevolenceWorkflow = new BenevolenceWorkflow
                        {
                            BenevolenceTypeId = entity.Id,
                        };

                        entity.BenevolenceWorkflows.Add( benevolenceWorkflow );
                    }

                    benevolenceWorkflow.WorkflowTypeId = workflowBag.WorkflowType.GetEntityId<WorkflowType>( rockContext ).GetValueOrDefault();
                    benevolenceWorkflow.TriggerType = workflowBag.Trigger.ConvertToEnum<BenevolenceWorkflowTriggerType>();
                    benevolenceWorkflow.QualifierValue = $"|{DefinedValueCache.GetId( workflowBag.PrimaryQualifier.AsGuid() )}|{DefinedValueCache.GetId( workflowBag.SecondaryQualifier.AsGuid() )}|";
                }

                // Ensure everything is valid before saving.
                if ( !ValidateBenevolenceType( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                rockContext.WrapTransaction( () => rockContext.SaveChanges() );

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new BenevolenceTypeService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<BenevolenceTypeBag, BenevolenceTypeDetailOptionsBag> box )
        {
            return ActionBadRequest( "Attributes are not supported by this block." );
        }

        /// <summary>
        /// Gets the existing workflow.
        /// </summary>
        /// <param name="workflowBag">The workflow bag.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult IsExistingWorkflow( BenevolenceWorkflowBag workflowBag )
        {
            using ( var rockContext = new RockContext() )
            {
                BenevolenceWorkflow findWorkFlow = null;
                var entityId = RequestContext.GetPageParameter( PageParameterKey.BenevolenceTypeId );
                int? id = !PageCache.Layout.Site.DisablePredictableIds ? entityId.AsIntegerOrNull() : null;
                Guid? guid = null;
                var trigger = workflowBag.Trigger.ConvertToEnum<BenevolenceWorkflowTriggerType>();
                var workflowTypeId = workflowBag.WorkflowType.GetEntityId<WorkflowType>( rockContext );
                var workflowQualifier = $"|{DefinedValueCache.GetId( workflowBag.PrimaryQualifier.AsGuid() )}|{DefinedValueCache.GetId( workflowBag.SecondaryQualifier.AsGuid() )}|";

                if ( !id.HasValue )
                {
                    guid = entityId.AsGuidOrNull();

                    if ( !guid.HasValue )
                    {
                        id = Rock.Utility.IdHasher.Instance.GetId( entityId );
                    }
                }

                if ( id.HasValue )
                {
                    findWorkFlow = rockContext.BenevolenceWorkflows?.FirstOrDefault( w => w.BenevolenceTypeId == id.Value
                        && w.TriggerType == trigger
                        && w.WorkflowTypeId == workflowTypeId );
                }
                else if (guid.HasValue)
                {
                    findWorkFlow = rockContext.BenevolenceWorkflows?.FirstOrDefault( w => w.BenevolenceType.Guid == guid.Value
                        && w.TriggerType == trigger
                        && w.WorkflowTypeId == workflowTypeId );
                }

                var existingWorkflow = findWorkFlow != null && findWorkFlow.QualifierValue.Md5Hash() == workflowQualifier.Md5Hash();

                return ActionOk( new { exists = existingWorkflow } );
            }
        }

        #endregion
    }
}
