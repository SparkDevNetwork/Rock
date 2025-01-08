﻿// <copyright>
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
using System.Reflection;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Workflow.WorkflowTriggerDetail;
using Rock.ViewModels.Blocks.WorkFlow.WorkflowTriggerDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Workflow
{
    /// <summary>
    /// Displays the details of a particular workflow trigger.
    /// </summary>

    [DisplayName( "Workflow Trigger Detail" )]
    [Category( "WorkFlow" )]
    [Description( "Displays the details of a particular workflow trigger." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "3f0d9d0f-a739-4c92-94a7-70b2bbe03f46" )]
    [Rock.SystemGuid.BlockTypeGuid( "a8062fe5-5bcd-48ac-8c37-2124462656a7" )]
    public class WorkflowTriggerDetail : RockEntityDetailBlockType<WorkflowTrigger, WorkflowTriggerBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string WorkflowTriggerId = "WorkflowTriggerId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<WorkflowTriggerBag, WorkflowTriggerDetailOptionsBag>();
            SetBoxInitialEntityState( box );
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );
            return box;
        }

        /// <summary>
        /// Converts the <see cref="WorkflowTriggerType"/> enum to a List of <see cref="ListItemBag"/>
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private List<ListItemBag> GetWorkflowTriggerTypes()
        {
            var triggerTypes = new List<ListItemBag>();

            var type = typeof( WorkflowTriggerType );
            foreach ( var value in Enum.GetValues( type ) )
            {
                triggerTypes.Add( new ListItemBag() { Text = Enum.GetName( type, value ).SplitCase().Replace( " ", "-" ), Value = value.ToString() } );
            }

            return triggerTypes;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private WorkflowTriggerDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new WorkflowTriggerDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the WorkflowTrigger for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="workflowTrigger">The WorkflowTrigger to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the WorkflowTrigger is valid, <c>false</c> otherwise.</returns>
        private bool ValidateWorkflowTrigger( WorkflowTrigger workflowTrigger, out string errorMessage )
        {
            errorMessage = null;

            if ( !workflowTrigger.IsValid )
            {
                errorMessage = workflowTrigger.ValidationResults.Select( x => x.ErrorMessage ).JoinStrings( "," );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<WorkflowTriggerBag, WorkflowTriggerDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {WorkflowTrigger.FriendlyTypeName} was not found.";
                return;
            }

            var canEdit = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            box.IsEditable = canEdit || !entity.IsSystem;

            box.Entity = GetEntityBagForEdit( entity );

            if ( !canEdit )
            {
                box.Entity.ReadonlyNotificationMessage = EditModeMessage.ReadOnlyEditActionNotAllowed( WorkflowTrigger.FriendlyTypeName );
            }
            else if ( entity.IsSystem )
            {
                box.Entity.ReadonlyNotificationMessage = EditModeMessage.ReadOnlySystem( WorkflowTrigger.FriendlyTypeName );
            }

            PrepareDetailBox( box, entity );
        }

        // <inheritdoc/>
        protected override WorkflowTriggerBag GetEntityBagForView( WorkflowTrigger entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetEntityBag( entity );

            return bag;
        }

        // <inheritdoc/>
        protected override WorkflowTriggerBag GetEntityBagForEdit( WorkflowTrigger entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetEntityBag( entity );

            return bag;
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="WorkflowTriggerBag"/> that represents the entity.</returns>
        private WorkflowTriggerBag GetEntityBag( WorkflowTrigger entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var entityType = EntityTypeCache.Get( entity.EntityTypeId );
            var bag = new WorkflowTriggerBag
            {
                IdKey = entity.IdKey,
                EntityType = entity.EntityType.ToListItemBag(),
                EntityTypeQualifierColumn = entity.EntityTypeQualifierColumn,
                IsActive = entity.IsActive,
                IsSystem = entity.IsSystem,
                WorkflowName = entity.WorkflowName,
                WorkflowTriggerTypes = GetWorkflowTriggerTypes(),
                WorkflowTriggerType = entity.WorkflowTriggerType.ToString(),
                WorkflowType = entity.WorkflowType.ToListItemBag(),
                QualifierColumns = GetQualifierColumnItems( entityType )
            };

            SetQualifierValues( entity, bag );

            if ( entity.Id == 0 )
            {
                bag.WorkflowTriggerType = nameof( WorkflowTriggerType.PostSave );
                bag.IsActive = true;
            }

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( WorkflowTrigger entity, ValidPropertiesBox<WorkflowTriggerBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.EntityType ),
                () => entity.EntityTypeId = box.Bag.EntityType.GetEntityId<EntityType>( RockContext ).Value );

            box.IfValidProperty( nameof( box.Bag.EntityTypeQualifierColumn ),
                () => entity.EntityTypeQualifierColumn = box.Bag.EntityTypeQualifierColumn );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.WorkflowName ),
                () => entity.WorkflowName = box.Bag.WorkflowName );

            box.IfValidProperty( nameof( box.Bag.WorkflowTriggerType ),
                () => entity.WorkflowTriggerType = box.Bag.WorkflowTriggerType.ConvertToEnum<WorkflowTriggerType>() );

            box.IfValidProperty( nameof( box.Bag.WorkflowType ),
                () => entity.WorkflowTypeId = box.Bag.WorkflowType.GetEntityId<WorkflowType>( RockContext ).Value );

            return true;
        }

        /// <inheritdoc/>
        protected override WorkflowTrigger GetInitialEntity()
        {
            return GetInitialEntity<WorkflowTrigger, WorkflowTriggerService>( RockContext, PageParameterKey.WorkflowTriggerId );
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

        // <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out WorkflowTrigger entity, out BlockActionResult error )
        {
            var entityService = new WorkflowTriggerService( RockContext );
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
                entity = new WorkflowTrigger();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{WorkflowTrigger.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${WorkflowTrigger.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Shows the qualifier values in the correct fields using the given 
        /// workflow trigger if available.
        /// </summary>
        /// <param name="workflowTrigger">The workflow trigger.</param>
        private void SetQualifierValues( WorkflowTrigger workflowTrigger, WorkflowTriggerBag bag )
        {
            bool useOrShowPreviousValue = false;

            if ( workflowTrigger != null )
            {
                if ( workflowTrigger.WorkflowTriggerType == WorkflowTriggerType.PreSave ||
                    workflowTrigger.WorkflowTriggerType == WorkflowTriggerType.PostSave ||
                    workflowTrigger.WorkflowTriggerType == WorkflowTriggerType.ImmediatePostSave )
                {
                    useOrShowPreviousValue = true;
                }

                if ( useOrShowPreviousValue
                    && ( !string.IsNullOrEmpty( workflowTrigger.EntityTypeQualifierValue ) || !string.IsNullOrEmpty( workflowTrigger.EntityTypeQualifierValuePrevious ) )
                     && workflowTrigger.WorkflowTriggerValueChangeType == WorkflowTriggerValueChangeType.ChangeFromTo
                    )
                {
                    bag.EntityTypeQualifierValueAlt = workflowTrigger.EntityTypeQualifierValue;
                    bag.EntityTypeQualifierValuePrevious = workflowTrigger.EntityTypeQualifierValuePrevious;
                }
                else
                {
                    bag.EntityTypeQualifierValue = workflowTrigger.EntityTypeQualifierValue;
                }
            }

            if ( useOrShowPreviousValue )
            {
                bag.EntityTypeQualifierValueLabel = "Or value is";
                bag.ShowPreviousAndAltQualifierValueTextBoxes = true;
            }
            else
            {
                bag.EntityTypeQualifierValueLabel = "Value is";
                bag.ShowPreviousAndAltQualifierValueTextBoxes = false;
            }
        }

        private List<ListItemBag> GetQualifierColumnItems( EntityTypeCache entityType )
        {
            var columns = new List<ListItemBag>();

            if ( entityType != null )
            {
                Type type = entityType.GetEntityType();

                if ( type != null )
                {
                    var propertyNames = new List<string>();
                    foreach ( var property in type.GetProperties() )
                    {
                        if ( ( property.GetGetMethod() != null && !property.GetGetMethod().IsVirtual ) ||
                            property.GetCustomAttributes( typeof( IncludeAsEntityProperty ) ).Any() ||
                            property.Name == "Id" ||
                            property.Name == "Guid" ||
                            property.Name == "Order" ||
                            property.Name == "IsActive" )
                        {
                            propertyNames.Add( property.Name );
                        }
                    }

                    columns = propertyNames.OrderBy( n => n ).Select( n => new ListItemBag() { Text = n, Value = n } ).ToList();
                }
            }

            return columns;
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
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<WorkflowTriggerBag>
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
        public BlockActionResult Save( ValidPropertiesBox<WorkflowTriggerBag> box )
        {
            var entityService = new WorkflowTriggerService( RockContext );

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
            if ( !ValidateWorkflowTrigger( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            var triggerType = box.Bag.WorkflowTriggerType.ConvertToEnum<WorkflowTriggerType>();
            var usePreviousValue = triggerType == WorkflowTriggerType.PreSave || triggerType == WorkflowTriggerType.PostSave || triggerType == WorkflowTriggerType.ImmediatePostSave;
            // If the trigger type is PreSave and the tbQualifierValue does not exist, use the previous and alt qualifier value
            if ( usePreviousValue )
            {
                if ( !string.IsNullOrEmpty( box.Bag.EntityTypeQualifierValue ) )
                {
                    // in this case, use the same value as the previous and current qualifier value
                    entity.EntityTypeQualifierValue = box.Bag.EntityTypeQualifierValue;
                    entity.EntityTypeQualifierValuePrevious = box.Bag.EntityTypeQualifierValue;
                }
                else
                {
                    entity.EntityTypeQualifierValue = box.Bag.EntityTypeQualifierValueAlt;
                    entity.EntityTypeQualifierValuePrevious = box.Bag.EntityTypeQualifierValuePrevious;
                }
            }
            else
            {
                // use the regular qualifier and clear the previous value qualifier since it does not apply.
                entity.EntityTypeQualifierValue = box.Bag.EntityTypeQualifierValue;
                entity.EntityTypeQualifierValuePrevious = string.Empty;
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
            } );

            WorkflowTriggersCache.Remove();

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetParentPageUrl() );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            var bag = GetEntityBagForView( entity );

            return ActionOk( new ValidPropertiesBox<WorkflowTriggerBag>
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
            var entityService = new WorkflowTriggerService( RockContext );

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

        /// <summary>
        /// Gets the qualifier columns.
        /// </summary>
        /// <param name="entityTypeGuid">The entity type unique identifier.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetQualifierColumns( Guid entityTypeGuid )
        {
            var entityType = EntityTypeCache.Get( entityTypeGuid );
            return ActionOk( GetQualifierColumnItems( entityType ) );
        }

        #endregion
    }
}
