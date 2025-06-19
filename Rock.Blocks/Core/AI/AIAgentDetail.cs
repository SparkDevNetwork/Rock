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

using Microsoft.Extensions.DependencyInjection;

using Rock.AI.Agent;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.AI.AIAgentDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular ai agent.
    /// </summary>

    [DisplayName( "AI Agent Detail" )]
    [Category( "Core > AI" )]
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

        #region Fields

        /// <summary>
        /// The container that will be used to access the agent skill components.
        /// </summary>
        private readonly AgentSkillContainer _agentSkillContainer;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AIAgentDetail"/> class.
        /// </summary>
        /// <param name="serviceProvider">The provider for our required services.</param>
        public AIAgentDetail( IServiceProvider serviceProvider )
        {
            _agentSkillContainer = serviceProvider.GetRequiredService<AgentSkillContainer>();
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            _agentSkillContainer.RegisterComponents();

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
                AvailableSkills = AISkillCache.All()
                    .Select( s => new ListItemBag
                    {
                        Value = s.Guid.ToString(),
                        Text = s.Name,
                        Category = s.Description
                    } )
                    .ToList()
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

            bag.Skills = entity.AIAgentSkills
                .Select( s => GetSkillBag( s, false ) )
                .ToList();

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

        /// <summary>
        /// Gets the bag that will represent an agent skill in the UI. This is used
        /// when the block is in view mode to allow the configuration of skills that
        /// should be attached to the agent.
        /// </summary>
        /// <param name="agentSkill">The record that links the agent to the skill.</param>
        /// <param name="isEditing"><c>true</c> if we need a bag for editing the linkage data.</param>
        /// <returns>A bag that represents the agent skill.</returns>
        private AgentSkillBag GetSkillBag( AIAgentSkill agentSkill, bool isEditing )
        {
            var bag = new AgentSkillBag
            {
                Guid = agentSkill.Guid,
                SkillGuid = agentSkill.AISkill.Guid,
                Name = agentSkill.AISkill.Name,
                Description = agentSkill.AISkill.Description
            };

            if ( isEditing )
            {
                var settings = agentSkill.GetAdditionalSettings<AgentSkillSettings>();

                bag.EnabledFunctions = agentSkill.AISkill
                    .AISkillFunctions
                    .Select( f => f.Guid )
                    .Where( g => !settings.DisabledFunctions.Contains( g ) )
                    .ToList();

                if ( agentSkill.AISkill.CodeEntityTypeId.HasValue )
                {
                    var entityType = EntityTypeCache.Get( agentSkill.AISkill.CodeEntityTypeId.Value, RockContext );
                    var skill = _agentSkillContainer.CreateInstance( entityType.Guid );

                    if ( skill != null )
                    {
                        bag.ConfigurationValues = skill.GetPublicConfiguration( settings.ConfigurationValues ?? new Dictionary<string, string>(), RockContext, RequestContext );
                    }
                }
            }

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

            RockContext.SaveChanges();

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

                var bag = GetEntityBagForView( entity );

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

        #region Agent Skill Block Actions

        /// <summary>
        /// Starts an edit operation of an existing agent skill. This will return
        /// the data required to open the UI for editing.
        /// </summary>
        /// <param name="agentSkillGuid">The unique identifier of the agent skill record.</param>
        /// <returns>A bag that contains the data required to edit the skill configuration.</returns>
        [BlockAction]
        public BlockActionResult EditSkill( Guid agentSkillGuid )
        {
            var agentSkillService = new AIAgentSkillService( RockContext );
            var agentSkill = agentSkillService.Get( agentSkillGuid );
            var response = new EditSkillResponseBag();

            if ( agentSkill.AISkill.CodeEntityType != null )
            {
                var component = _agentSkillContainer.CreateInstance( agentSkill.AISkill.CodeEntityType.Guid );
                var definition = component?.GetComponentDefinition( new Dictionary<string, string>(), RockContext, RequestContext );

                response.ComponentDefinition = definition;
            }

            response.Skill = GetSkillBag( agentSkill, true );
            response.AvailableFunctions = agentSkill.AISkill.AISkillFunctions
                .ToListItemBagList();

            return ActionOk( response );
        }

        /// <summary>
        /// Removes a skill from the agent. This will delete the <see cref="AIAgentSkill"/>
        /// record, not the <see cref="AISkill"/> itself.
        /// </summary>
        /// <param name="agentSkillGuid">The unique identifier of the agent skill to be removed.</param>
        /// <returns>A response that indicates if the skill was removed or not.</returns>
        [BlockAction]
        public BlockActionResult RemoveSkill( Guid agentSkillGuid )
        {
            var agentSkillService = new AIAgentSkillService( RockContext );
            var agentSkill = agentSkillService.Get( agentSkillGuid );

            if ( agentSkill == null )
            {
                return ActionNotFound( "That skill was not found." );
            }

            agentSkillService.Delete( agentSkill );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Saves the skill configuration for the agent. This will create a new
        /// <see cref="AIAgentSkill"/> record if required, otherwise an existing
        /// one will be updated.
        /// </summary>
        /// <param name="key">The key that identifies the agent the skill should be attached to.</param>
        /// <param name="bag">The configuration data for the skill.</param>
        /// <returns>A bag that represents the skill for viewing purposes.</returns>
        [BlockAction]
        public BlockActionResult SaveSkill( string key, AgentSkillBag bag )
        {
            var agentSkillService = new AIAgentSkillService( RockContext );
            var skillService = new AISkillService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var agent, out var actionError ) )
            {
                return actionError;
            }

            var agentSkill = agentSkillService.Get( bag.Guid );

            if ( agentSkill == null )
            {
                var skill = skillService.Get( bag.SkillGuid );

                agentSkill = new AIAgentSkill
                {
                    AIAgentId = agent.Id,
                    AISkillId = skill.Id,
                    AISkill = skill
                };

                agentSkillService.Add( agentSkill );
            }

            var settings = agentSkill.GetAdditionalSettings<AgentSkillSettings>();

            settings.DisabledFunctions = agentSkill.AISkill
                .AISkillFunctions
                .Select( f => f.Guid )
                .Where( g => !bag.EnabledFunctions.Contains( g ) )
                .ToList();

            if ( agentSkill.AISkill.CodeEntityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Get( agentSkill.AISkill.CodeEntityTypeId.Value, RockContext );
                var skill = _agentSkillContainer.CreateInstance( entityType.Guid );

                if ( skill != null )
                {
                    settings.ConfigurationValues = skill.GetPrivateConfiguration( bag.ConfigurationValues ?? new Dictionary<string, string>(), RockContext, RequestContext );
                }
            }

            agentSkill.SetAdditionalSettings( settings );

            RockContext.SaveChanges();

            return ActionOk( GetSkillBag( agentSkill, false ) );
        }

        /// <summary>
        /// Gets the <see cref="DynamicComponentDefinitionBag"/> that describes
        /// the UI component that will handle the configuration of the skill.
        /// </summary>
        /// <param name="skillGuid">The unique identifier of the skill.</param>
        /// <returns>A bag that contains the component definition.</returns>
        [BlockAction]
        public BlockActionResult GetSkillComponentDefinition( Guid skillGuid )
        {
            var skill = new AISkillService( RockContext ).Get( skillGuid );
            var response = new GetComponentDefinitionResponseBag();

            if ( skill.CodeEntityType != null )
            {
                var component = _agentSkillContainer.CreateInstance( skill.CodeEntityType.Guid );
                var definition = component?.GetComponentDefinition( new Dictionary<string, string>(), RockContext, RequestContext );

                response.ComponentDefinition = definition;
                response.ConfigurationValues = component?.GetPublicConfiguration( new Dictionary<string, string>(), RockContext, RequestContext );
            }

            response.AvailableFunctions = skill.AISkillFunctions
                .ToListItemBagList();

            return ActionOk( response );
        }

        /// <summary>
        /// Executes a request from the UI component to be processed by the
        /// server component. This is used to load additional information after
        /// the component has been initialized.
        /// </summary>
        /// <param name="skillGuid">The unique identifier of the skill.</param>
        /// <param name="request">The object that describes the parameters of the request.</param>
        /// <param name="securityGrantToken">The security grant token that was created when the component was initialized.</param>
        /// <returns>The response from the server component.</returns>
        [BlockAction]
        public BlockActionResult ExecuteSkillComponentRequest( Guid skillGuid, Dictionary<string, string> request, string securityGrantToken )
        {
            var skill = new AISkillService( RockContext ).Get( skillGuid );

            if ( skill.CodeEntityType == null )
            {
                return ActionOk( ( Dictionary<string, string> ) null );
            }

            var securityGrant = SecurityGrant.FromToken( securityGrantToken ) ?? new SecurityGrant();
            var component = _agentSkillContainer.CreateInstance( skill.CodeEntityType.Guid );

            var result = component?.ExecuteComponentRequest( request, securityGrant, RockContext, RequestContext );

            return ActionOk( result );
        }

        #endregion
    }
}
