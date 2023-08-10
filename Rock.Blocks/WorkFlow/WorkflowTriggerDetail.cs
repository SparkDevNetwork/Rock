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
using System.Reflection;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Workflow.WorkflowTriggerDetail;
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

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "3f0d9d0f-a739-4c92-94a7-70b2bbe03f46" )]
    [Rock.SystemGuid.BlockTypeGuid( "a8062fe5-5bcd-48ac-8c37-2124462656a7" )]
    public class WorkflowTriggerDetail : RockDetailBlockType
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
            using ( var rockContext = new RockContext() )
            {
                var box = new WorkflowTriggerDetailBox();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();

                return box;
            }
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
        /// Validates the WorkflowTrigger for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="workflowTrigger">The WorkflowTrigger to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the WorkflowTrigger is valid, <c>false</c> otherwise.</returns>
        private bool ValidateWorkflowTrigger( WorkflowTrigger workflowTrigger, RockContext rockContext, out string errorMessage )
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
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( WorkflowTriggerDetailBox box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {WorkflowTrigger.FriendlyTypeName} was not found.";
                return;
            }

            var canEdit = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            box.IsEditable = canEdit || !entity.IsSystem;

            box.Entity = GetEntityBag( entity );
            box.SecurityGrantToken = GetSecurityGrantToken( entity );

            if ( !canEdit )
            {
                box.ReadonlyNotificationMessage = EditModeMessage.ReadOnlyEditActionNotAllowed( WorkflowTrigger.FriendlyTypeName );
            }
            else if ( entity.IsSystem )
            {
                box.ReadonlyNotificationMessage = EditModeMessage.ReadOnlySystem( WorkflowTrigger.FriendlyTypeName );
            }
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

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( WorkflowTrigger entity, WorkflowTriggerDetailBox box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.EntityType ),
                () => entity.EntityTypeId = box.Entity.EntityType.GetEntityId<EntityType>( rockContext ).Value );

            box.IfValidProperty( nameof( box.Entity.EntityTypeQualifierColumn ),
                () => entity.EntityTypeQualifierColumn = box.Entity.EntityTypeQualifierColumn );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.WorkflowName ),
                () => entity.WorkflowName = box.Entity.WorkflowName );

            box.IfValidProperty( nameof( box.Entity.WorkflowTriggerType ),
                () => entity.WorkflowTriggerType = box.Entity.WorkflowTriggerType.ConvertToEnum<WorkflowTriggerType>() );

            box.IfValidProperty( nameof( box.Entity.WorkflowType ),
                () => entity.WorkflowTypeId = box.Entity.WorkflowType.GetEntityId<WorkflowType>( rockContext ).Value );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="WorkflowTrigger"/> to be viewed or edited on the page.</returns>
        private WorkflowTrigger GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<WorkflowTrigger, WorkflowTriggerService>( rockContext, PageParameterKey.WorkflowTriggerId );
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
        private string GetSecurityGrantToken( WorkflowTrigger entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out WorkflowTrigger entity, out BlockActionResult error )
        {
            var entityService = new WorkflowTriggerService( rockContext );
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
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                var box = new DetailBlockBox<WorkflowTriggerBag, WorkflowTriggerDetailBox>
                {
                    Entity = GetEntityBag( entity )
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
        public BlockActionResult Save( WorkflowTriggerDetailBox box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new WorkflowTriggerService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateWorkflowTrigger( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                var triggerType = box.Entity.WorkflowTriggerType.ConvertToEnum<WorkflowTriggerType>();
                var usePreviousValue = triggerType == WorkflowTriggerType.PreSave || triggerType == WorkflowTriggerType.PostSave || triggerType == WorkflowTriggerType.ImmediatePostSave;
                // If the trigger type is PreSave and the tbQualifierValue does not exist, use the previous and alt qualifier value
                if ( usePreviousValue )
                {
                    if ( !string.IsNullOrEmpty( box.Entity.EntityTypeQualifierValue ) )
                    {
                        // in this case, use the same value as the previous and current qualifier value
                        entity.EntityTypeQualifierValue = box.Entity.EntityTypeQualifierValue;
                        entity.EntityTypeQualifierValuePrevious = box.Entity.EntityTypeQualifierValue;
                    }
                    else
                    {
                        entity.EntityTypeQualifierValue = box.Entity.EntityTypeQualifierValueAlt;
                        entity.EntityTypeQualifierValuePrevious = box.Entity.EntityTypeQualifierValuePrevious;
                    }
                }
                else
                {
                    // use the regular qualifier and clear the previous value qualifier since it does not apply.
                    entity.EntityTypeQualifierValue = box.Entity.EntityTypeQualifierValue;
                    entity.EntityTypeQualifierValuePrevious = string.Empty;
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                } );

                WorkflowTriggersCache.Remove();

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.WorkflowTriggerId] = entity.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
                entity = entityService.Get( entity.Id );

                return ActionOk( GetEntityBag( entity ) );
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
                var entityService = new WorkflowTriggerService( rockContext );

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
        public BlockActionResult RefreshAttributes( WorkflowTriggerDetailBox box )
        {
            return ActionBadRequest( "Attributes are not supported by this block." );
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
