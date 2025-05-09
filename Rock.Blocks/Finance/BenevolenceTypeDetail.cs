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
    [SupportedSiteTypes( Model.SiteType.Web )]

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
    public class BenevolenceTypeDetail : RockEntityDetailBlockType<BenevolenceType, BenevolenceTypeBag>
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
            var box = new DetailBlockBox<BenevolenceTypeBag, BenevolenceTypeDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private BenevolenceTypeDetailOptionsBag GetBoxOptions()
        {
            var options = new BenevolenceTypeDetailOptionsBag
            {
                Statuses = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.BENEVOLENCE_REQUEST_STATUS.AsGuid() ).DefinedValues.ToListItemBagList(),
                TriggerTypes = ToListItemBag( typeof( BenevolenceWorkflowTriggerType ) )
            };

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
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the BenevolenceType is valid, <c>false</c> otherwise.</returns>
        private bool ValidateBenevolenceType( BenevolenceType benevolenceType, out string errorMessage )
        {
            errorMessage = null;

            if ( benevolenceType.Id == 0 )
            {
                // Check for existing
                var existingBenevolence = new BenevolenceTypeService( RockContext ).Queryable()
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
        private void SetBoxInitialEntityState( DetailBlockBox<BenevolenceTypeBag, BenevolenceTypeDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {BenevolenceType.FriendlyTypeName} was not found.";
                return;
            }

            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( box.IsEditable )
            {
                entity.LoadAttributes( RockContext );
                box.Entity = GetEntityBagForEdit( entity );
            }
            else
            {
                box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( BenevolenceType.FriendlyTypeName );
            }
        }

        /// <inheritdoc/>
        protected override BenevolenceTypeBag GetEntityBagForView( BenevolenceType entity )
        {
            return GetEntityBag( entity );
        }

        /// <inheritdoc/>
        protected override BenevolenceTypeBag GetEntityBagForEdit( BenevolenceType entity )
        {
            return GetEntityBag( entity );
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="BenevolenceTypeBag"/> that represents the entity.</returns>
        private BenevolenceTypeBag GetEntityBag( BenevolenceType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new BenevolenceTypeBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                IsActive = entity.Id == 0 || entity.IsActive,
                Name = entity.Name,
                RequestLavaTemplate = entity.RequestLavaTemplate,
                ShowFinancialResults = entity.Id == 0 || entity.ShowFinancialResults,
                MaximumNumberOfDocuments = entity.AdditionalSettingsJson?.FromJsonOrNull<BenevolenceType.AdditionalSettings>()?.MaximumNumberOfDocuments,
                CanAdminstrate = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ),
                Workflows = entity.BenevolenceWorkflows.Select( w => ToWorkflowBag( w ) ).OrderByDescending( w => w.IsInherited )
                .ThenBy( w => w.WorkflowTypeName )
                .ToList()
            };

            var attributeGuidList = GetAttributeValue( AttributeKey.BenevolenceTypeAttributes ).SplitDelimitedValues().AsGuidList();

            if ( attributeGuidList.Any() )
            {
                bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: false, attributeFilter: a => attributeGuidList.Any( ag => a.Guid == ag ) );
            }

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
                    Guid = workflow.Guid.ToString(),
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

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( BenevolenceType entity, ValidPropertiesBox<BenevolenceTypeBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.RequestLavaTemplate ),
                () => entity.RequestLavaTemplate = box.Bag.RequestLavaTemplate );

            box.IfValidProperty( nameof( box.Bag.ShowFinancialResults ),
                () => entity.ShowFinancialResults = box.Bag.ShowFinancialResults );

            box.IfValidProperty( nameof( box.Bag.Workflows ),
                () => SaveWorkflows( box.Bag, entity, RockContext ) );

            box.IfValidProperty( nameof( box.Bag.MaximumNumberOfDocuments ),
                () => SaveAdditionalSettings( box.Bag, entity ) );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: false );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override BenevolenceType GetInitialEntity()
        {
            return GetInitialEntity<BenevolenceType, BenevolenceTypeService>( RockContext, PageParameterKey.BenevolenceTypeId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out BenevolenceType entity, out BlockActionResult error )
        {
            var entityService = new BenevolenceTypeService( RockContext );
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

        /// <summary>
        /// Saves the additional settings.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="entity">The entity.</param>
        private static void SaveAdditionalSettings( BenevolenceTypeBag bag, BenevolenceType entity )
        {
            var additionalSettings = entity.AdditionalSettingsJson?.FromJsonOrNull<BenevolenceType.AdditionalSettings>() ?? new BenevolenceType.AdditionalSettings();
            additionalSettings.MaximumNumberOfDocuments = bag.MaximumNumberOfDocuments;
            entity.AdditionalSettingsJson = additionalSettings.ToJson();
        }

        /// <summary>
        /// Saves the workflows.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void SaveWorkflows( BenevolenceTypeBag bag, BenevolenceType entity, RockContext rockContext )
        {
            if ( bag.Workflows != null )
            {
                var benevolenceWorkflowService = new BenevolenceWorkflowService( rockContext );
                // remove any workflows that were removed in the UI
                var uiWorkflows = bag.Workflows.Select( l => l.Guid.AsGuid() );

                foreach ( var benevolenceWorkflow in entity.BenevolenceWorkflows.Where( l => !uiWorkflows.Contains( l.Guid ) ).ToList() )
                {
                    entity.BenevolenceWorkflows.Remove( benevolenceWorkflow );
                    benevolenceWorkflowService.Delete( benevolenceWorkflow );
                }

                // Add or Update workflows from the UI
                foreach ( var workflowBag in bag.Workflows )
                {
                    BenevolenceWorkflow benevolenceWorkflow = entity.BenevolenceWorkflows
                        .FirstOrDefault( b => !workflowBag.Guid.Equals( Guid.Empty ) && b.Guid.ToString() == workflowBag.Guid );

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
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<BenevolenceTypeBag> box )
        {
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
            if ( !ValidateBenevolenceType( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            return ActionOk( this.GetParentPageUrl() );
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
                var benevolenceWorkflowService = new BenevolenceWorkflowService( rockContext );

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
                    findWorkFlow = benevolenceWorkflowService.Queryable().FirstOrDefault( w => w.BenevolenceTypeId == id.Value
                        && w.TriggerType == trigger
                        && w.WorkflowTypeId == workflowTypeId );
                }
                else if (guid.HasValue)
                {
                    findWorkFlow = benevolenceWorkflowService.Queryable().FirstOrDefault( w => w.BenevolenceType.Guid == guid.Value
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
