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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent;
using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Core.AI.Agent;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.AISkillDetail;
using Rock.ViewModels.Blocks.Core.AISkillFunctionList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of ai skill functions.
    /// </summary>

    [DisplayName( "AI Skill Function List" )]
    [Category( "Core" )]
    [Description( "Displays a list of ai skill functions." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the ai skill function details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "fb397310-6bcb-49cd-9ccb-3506046cb14b" )]
    [Rock.SystemGuid.BlockTypeGuid( "1e257602-9c31-4f6c-a362-67912f06e807" )]
    [CustomizedGrid]
    public class AISkillFunctionList : RockEntityListBlockType<AISkillFunction>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string AddDetailPage = "AddDetailPage";
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string AISkillId = "AISkillId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<AISkillFunctionListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = box.IsAddEnabled;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();

            box.GridDefinition = builder.BuildDefinition();

            var qry = GetListQueryable( RockContext ).AsNoTracking();
            qry = GetOrderedListQueryable( qry, RockContext );
            var items = GetListItems( qry, RockContext );

            box.Options.GridData = builder.Build( items );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private AISkillFunctionListOptionsBag GetBoxOptions()
        {
            var options = new AISkillFunctionListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var skill = AISkillCache.Get( GetSkillId( null ), RockContext );

            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
                && skill != null
                && !skill.CodeEntityTypeId.HasValue;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "AISkillFunctionId", "((Key))" ),
                [NavigationUrlKey.AddDetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, new Dictionary<string, string>
                {
                    ["AISkillFunctionId"] = "((Key))",
                    [PageParameterKey.AISkillId] = RequestContext.GetPageParameter( PageParameterKey.AISkillId )
                } )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<AISkillFunction> GetListQueryable( RockContext rockContext )
        {
            var skillId = GetSkillId( null );

            return base.GetListQueryable( rockContext )
                .Where( sf => sf.AISkillId == skillId );
        }

        /// <inheritdoc/>
        protected override GridBuilder<AISkillFunction> GetGridBuilder()
        {
            var skill = AISkillCache.Get( GetSkillId( null ) );

            return new GridBuilder<AISkillFunction>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "description", a => a.Description )
                .AddField( "functionType", a => a.FunctionType )
                .AddField( "isCodeType", a => skill?.CodeEntityTypeId.HasValue == true );
        }

        /// <summary>
        /// Gets the skill identifier from the function or from the page parameters.
        /// </summary>
        /// <param name="function">The optional function to try to get the skill identifier from.</param>
        /// <returns>The identifier of the skill, may return <c>0</c> if one was not found.</returns>
        private int GetSkillId( AISkillFunction function )
        {
            if ( function != null && function.AISkillId != 0 )
            {
                return function.AISkillId;
            }

            var skillId = RequestContext.GetPageParameter( PageParameterKey.AISkillId );

            if ( skillId != null )
            {
                return IdHasher.Instance.GetId( skillId ) ?? 0;
            }

            return 0;
        }

        /// <summary>
        /// Gets the entity bag that is used when entering edit mode.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="AISkillFunctionBag"/> that represents the entity.</returns>
        private AISkillFunctionBag GetEntityBagForEdit( AISkillFunction entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var promptSettings = entity.GetAdditionalSettings<PromptInformationSettings>();

            return new AISkillFunctionBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                Name = entity.Name,
                UsageHint = entity.UsageHint,
                FunctionType = entity.FunctionType = entity.FunctionType,
                PreRenderLava = promptSettings.PreRenderLava,
                Temperature = promptSettings.Temperature,
                MaxTokens = promptSettings.MaxTokens,
                Prompt = promptSettings.Prompt,
                PromptParametersSchema = promptSettings.PromptParametersSchema
            };
        }

        /// <summary>
        /// Tries to get the entity for the edit action, either by loading
        /// an existing one or creating a new one.
        /// </summary>
        /// <param name="idKey">The identifier of the function to load.</param>
        /// <param name="entity">On return of <c>true</c> contains the function that was loaded or created.</param>
        /// <param name="error">On return of <c>false</c> contains the error resutl to respond with.</param>
        /// <returns><c>true</c> if an entity was loaded or created; otherwise <c>false</c>.</returns>
        private bool TryGetEntityForEditAction( string idKey, out AISkillFunction entity, out BlockActionResult error )
        {
            var entityService = new AISkillFunctionService( RockContext );
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
                entity = new AISkillFunction
                {
                    AISkillId = GetSkillId( null ),
                    FunctionType = FunctionType.ExecuteLava
                };

                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{AISkill.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${AISkill.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the AISkillFunction for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="aiSkillFunction">The AISkillFunction to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the AISkillFunction is valid, <c>false</c> otherwise.</returns>
        private bool ValidateAISkillFunction( AISkillFunction aiSkillFunction, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Updates the entity from the information contained in the box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box that contains the data</param>
        /// <returns><c>true</c> if the update was successful; otherwise <c>false</c>.</returns>
        private bool UpdateEntityFromBox( AISkillFunction entity, ValidPropertiesBox<AISkillFunctionBag> box )
        {
            var skill = AISkillCache.Get( entity.AISkillId );

            if ( box.ValidProperties == null )
            {
                return false;
            }

            // Do not allow modifying any C# based functions or skills.
            if ( entity.FunctionType == FunctionType.ExecuteCode || skill == null || skill.CodeEntityTypeId.HasValue )
            {
                return false;
            }

            // Do not allow changing type to ExecuteCode.
            if ( box.IsValidProperty( nameof( box.Bag.FunctionType ) ) && box.Bag.FunctionType != FunctionType.ExecuteCode )
            {
                entity.FunctionType = box.Bag.FunctionType;
            }

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.UsageHint ),
                () => entity.UsageHint = box.Bag.UsageHint );

            var promptSettings = entity.GetAdditionalSettings<PromptInformationSettings>();

            box.IfValidProperty( nameof( box.Bag.PreRenderLava ),
                () => promptSettings.PreRenderLava = box.Bag.PreRenderLava );

            box.IfValidProperty( nameof( box.Bag.Temperature ),
                () => promptSettings.Temperature = box.Bag.Temperature );

            box.IfValidProperty( nameof( box.Bag.MaxTokens ),
                () => promptSettings.MaxTokens = box.Bag.MaxTokens );

            box.IfValidProperty( nameof( box.Bag.Prompt ),
                () => promptSettings.Prompt = box.Bag.Prompt );

            box.IfValidProperty( nameof( box.Bag.PromptParametersSchema ),
                () => promptSettings.PromptParametersSchema = box.Bag.PromptParametersSchema );

            entity.SetAdditionalSettings( promptSettings );

            return true;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new AISkillFunctionService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );
            var skill = AISkillCache.Get( GetSkillId( entity ) );

            if ( entity == null )
            {
                return ActionBadRequest( $"{AISkillFunction.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {AISkillFunction.FriendlyTypeName}." );
            }

            if ( skill == null || skill.CodeEntityTypeId.HasValue )
            {
                return ActionBadRequest( "Cannot delete functions for code-based skills." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

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

            return ActionOk( new ValidPropertiesBox<AISkillFunctionBag>
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
        public BlockActionResult Save( ValidPropertiesBox<AISkillFunctionBag> box )
        {
            var entityService = new AISkillFunctionService( RockContext );

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
            if ( !ValidateAISkillFunction( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
            } );

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<AISkillFunctionBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        #endregion
    }
}
