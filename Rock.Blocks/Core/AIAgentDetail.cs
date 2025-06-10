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

using Rock.AI.Agent;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.AIAgentDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular ai agent.
    /// </summary>

    [DisplayName( "AI Agent Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of a particular ai agent." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "45ae85c7-1370-4c59-8f98-e1b0e268e54d" )]
    [Rock.SystemGuid.BlockTypeGuid( "d898e9ce-fe9b-48f7-96bf-2d69de3c8e7c" )]
    public class AIAgentDetail : RockEntityDetailBlockType<AIAgent, AIAgentBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string AIAgentId = "AIAgentId";
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
            var box = new DetailBlockBox<AIAgentBag, AIAgentDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private AIAgentDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new AIAgentDetailOptionsBag
            {
                AvailableSkills = AISkillCache.All().ToListItemBagList()
            };

            return options;
        }

        /// <summary>
        /// Validates the AIAgent for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="aiAgent">The AIAgent to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the AIAgent is valid, <c>false</c> otherwise.</returns>
        private bool ValidateAIAgent( AIAgent aiAgent, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<AIAgentBag, AIAgentDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {AIAgent.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( AIAgent.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( AIAgent.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="AIAgentBag"/> that represents the entity.</returns>
        private AIAgentBag GetCommonEntityBag( AIAgent entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var settings = entity.GetAdditionalSettings<AgentSettings>();

            return new AIAgentBag
            {
                IdKey = entity.IdKey,
                Skills = entity.AIAgentSkills
                    .Select( s => s.AISkill )
                    .ToListItemBagList(),
                AvatarBinaryFile = entity.AvatarBinaryFile.ToListItemBag(),
                Description = entity.Description,
                Name = entity.Name,
                Persona = entity.Persona,
                Role = settings.Role
            };
        }

        /// <inheritdoc/>
        protected override AIAgentBag GetEntityBagForView( AIAgent entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            return bag;
        }

        //// <inheritdoc/>
        protected override AIAgentBag GetEntityBagForEdit( AIAgent entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( AIAgent entity, ValidPropertiesBox<AIAgentBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            var settings = entity.GetAdditionalSettings<AgentSettings>();

            box.IfValidProperty( nameof( box.Bag.AvatarBinaryFile ),
                () => entity.AvatarBinaryFileId = box.Bag.AvatarBinaryFile.GetEntityId<BinaryFile>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.Persona ),
                () => entity.Persona = box.Bag.Persona );

            box.IfValidProperty( nameof( box.Bag.Role ),
                () => settings.Role = box.Bag.Role );

            entity.SetAdditionalSettings( settings );

            return true;
        }

        /// <inheritdoc/>
        protected override AIAgent GetInitialEntity()
        {
            return GetInitialEntity<AIAgent, AIAgentService>( RockContext, PageParameterKey.AIAgentId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out AIAgent entity, out BlockActionResult error )
        {
            var entityService = new AIAgentService( RockContext );
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
                entity = new AIAgent();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{AIAgent.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${AIAgent.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates the linked skills for the agent based on the provided skill bags.
        /// </summary>
        /// <param name="agent">The agent to link the skills to.</param>
        /// <param name="skillBags">The bags that represent with skills should be linked.</param>
        private void UpdateLinkedSkills( AIAgent agent, List<ListItemBag> skillBags )
        {
            var agentSkillService = new AIAgentSkillService( RockContext );
            var skillService = new AISkillService( RockContext );
            var skillBagGuids = skillBags.Select( s => s.Value.AsGuid() ).ToList();
            var existingSkillGuids = agent.AIAgentSkills.Select( s => s.AISkill.Guid ).ToList();
            var newSkillGuids = skillBagGuids.Where( g => !existingSkillGuids.Contains( g ) );

            // Remove any existing skills that are not in the new list.
            agentSkillService.DeleteRange( agent.AIAgentSkills.Where( s => !skillBagGuids.Contains( s.AISkill.Guid ) ) );

            // Add any new skills that are not already linked.
            foreach ( var skillGuid in newSkillGuids )
            {
                agentSkillService.Add( new AIAgentSkill
                {
                    AIAgentId = agent.Id,
                    AISkillId = skillService.GetId( skillGuid ).Value
                } );
            }
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

            return ActionOk( new ValidPropertiesBox<AIAgentBag>
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
        public BlockActionResult Save( ValidPropertiesBox<AIAgentBag> box )
        {
            var entityService = new AIAgentService( RockContext );

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
            if ( !ValidateAIAgent( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();

                if ( box.IsValidProperty( nameof( box.Bag.Skills ) ) )
                {
                    UpdateLinkedSkills( entity, box.Bag.Skills );

                    RockContext.SaveChanges();
                }
            } );

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.AIAgentId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            using ( var rockContext2 = new RockContext() )
            {
                entity = new AIAgentService( rockContext2 ).Get( entity.Id );

                var bag = GetEntityBagForEdit( entity );

                return ActionOk( new ValidPropertiesBox<AIAgentBag>
                {
                    Bag = bag,
                    ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
                } );
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
            var entityService = new AIAgentService( RockContext );

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
