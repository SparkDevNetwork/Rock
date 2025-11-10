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

using Rock;
using Rock.AI.Agent;
using Rock.Attribute;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.AI.AISkillToolList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;

namespace Rock.Blocks.AI
{
    /// <summary>
    /// Displays a list of AI skill tools.
    /// </summary>

    [DisplayName( "AI Skill Tool List" )]
    [Category( "AI" )]
    [Description( "Displays a list of AI skill tools." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( SiteType.Web )]

    [SystemGuid.EntityTypeGuid( "fb397310-6bcb-49cd-9ccb-3506046cb14b" )]
    [SystemGuid.BlockTypeGuid( "1e257602-9c31-4f6c-a362-67912f06e807" )]
    [CustomizedGrid]
    public class AISkillToolList : RockEntityListBlockType<AISkillTool>
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
            var box = new ListBlockBox<AISkillToolListOptionsBag>();
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
        private AISkillToolListOptionsBag GetBoxOptions()
        {
            var options = new AISkillToolListOptionsBag();

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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "AISkillToolId", "((Key))" ),
                [NavigationUrlKey.AddDetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, new Dictionary<string, string>
                {
                    ["AISkillToolId"] = "((Key))",
                    [PageParameterKey.AISkillId] = RequestContext.GetPageParameter( PageParameterKey.AISkillId )
                } )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<AISkillTool> GetListQueryable( RockContext rockContext )
        {
            var skillId = GetSkillId( null );

            return base.GetListQueryable( rockContext )
                .Where( sf => sf.AISkillId == skillId );
        }

        /// <inheritdoc/>
        protected override GridBuilder<AISkillTool> GetGridBuilder()
        {
            var skill = AISkillCache.Get( GetSkillId( null ) );

            return new GridBuilder<AISkillTool>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "description", a => a.Description )
                .AddField( "toolType", a => a.ToolType )
                .AddField( "isCodeType", a => skill?.CodeEntityTypeId.HasValue == true );
        }

        /// <summary>
        /// Gets the skill identifier from the tool or from the page parameters.
        /// </summary>
        /// <param name="tool">The optional tool to try to get the skill identifier from.</param>
        /// <returns>The identifier of the skill, may return <c>0</c> if one was not found.</returns>
        private int GetSkillId( AISkillTool tool )
        {
            if ( tool != null && tool.AISkillId != 0 )
            {
                return tool.AISkillId;
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
        /// <returns>A <see cref="AISkillToolBag"/> that represents the entity.</returns>
        private AISkillToolBag GetEntityBagForEdit( AISkillTool entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var additionalSettings = entity.GetAdditionalSettings<ToolAdditionalSettings>();
            var promptSettings = entity.GetAdditionalSettings<PromptInformationSettings>();
            var instructions = entity.GetAdditionalSettings<ToolInstructionSettings>();

            return new AISkillToolBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                Name = entity.Name,
                Preamble = additionalSettings.Preamble,
                Instructions = ToInstructionBags( instructions ),
                ToolType = entity.ToolType = entity.ToolType,
                PreRenderLava = promptSettings.PreRenderLava,
                Temperature = promptSettings.Temperature,
                MaxTokens = promptSettings.MaxTokens,
                Prompt = promptSettings.Prompt,
                PromptParameters = promptSettings.PromptParameters?.Select( ToParameterBag ).ToList(),
            };
        }

        /// <summary>
        /// Tries to get the entity for the edit action, either by loading
        /// an existing one or creating a new one.
        /// </summary>
        /// <param name="idKey">The identifier of the tool to load.</param>
        /// <param name="entity">On return of <c>true</c> contains the tool that was loaded or created.</param>
        /// <param name="error">On return of <c>false</c> contains the error resutl to respond with.</param>
        /// <returns><c>true</c> if an entity was loaded or created; otherwise <c>false</c>.</returns>
        private bool TryGetEntityForEditAction( string idKey, out AISkillTool entity, out BlockActionResult error )
        {
            var entityService = new AISkillToolService( RockContext );
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
                entity = new AISkillTool
                {
                    AISkillId = GetSkillId( null ),
                    ToolType = ToolType.ExecuteLava
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
        /// Validates the AISkillTool for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="aiSkillTool">The AISkillTool to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the AISkillTool is valid, <c>false</c> otherwise.</returns>
        private bool ValidateAISkillTool( AISkillTool aiSkillTool, out string errorMessage )
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
        private bool UpdateEntityFromBox( AISkillTool entity, ValidPropertiesBox<AISkillToolBag> box )
        {
            var skill = AISkillCache.Get( entity.AISkillId );

            if ( box.ValidProperties == null )
            {
                return false;
            }

            // Do not allow modifying any C# based tools or skills.
            if ( entity.ToolType == ToolType.ExecuteCode || skill == null || skill.CodeEntityTypeId.HasValue )
            {
                return false;
            }

            // Do not allow changing type to ExecuteCode.
            if ( box.IsValidProperty( nameof( box.Bag.ToolType ) ) && box.Bag.ToolType != ToolType.ExecuteCode )
            {
                entity.ToolType = box.Bag.ToolType;
            }

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            var additionalSettings = entity.GetAdditionalSettings<ToolAdditionalSettings>();
            var instructions = entity.GetAdditionalSettings<ToolInstructionSettings>();
            var promptSettings = entity.GetAdditionalSettings<PromptInformationSettings>();

            box.IfValidProperty( nameof( box.Bag.Preamble ),
                () => additionalSettings.Preamble = box.Bag.Preamble );

            box.IfValidProperty( nameof( box.Bag.Instructions ),
                () => instructions = GetInstructionSettings( box.Bag.Instructions ) );

            box.IfValidProperty( nameof( box.Bag.PreRenderLava ),
                () => promptSettings.PreRenderLava = box.Bag.PreRenderLava );

            box.IfValidProperty( nameof( box.Bag.Temperature ),
                () => promptSettings.Temperature = box.Bag.Temperature );

            box.IfValidProperty( nameof( box.Bag.MaxTokens ),
                () => promptSettings.MaxTokens = box.Bag.MaxTokens );

            box.IfValidProperty( nameof( box.Bag.Prompt ),
                () => promptSettings.Prompt = box.Bag.Prompt );

            box.IfValidProperty( nameof( box.Bag.PromptParameters ),
                () => promptSettings.PromptParameters = box.Bag.PromptParameters?.Select( FromParameterBag ).ToList() );

            entity.SetAdditionalSettings( additionalSettings );
            entity.SetAdditionalSettings( instructions );
            entity.SetAdditionalSettings( promptSettings );

            return true;
        }

        /// <summary>
        /// Converts a <see cref="ParameterSchema"/> to a <see cref="ParameterSchemaBag"/>.
        /// </summary>
        /// <param name="parameter">The parameter to be converted.</param>
        /// <returns>An instance of <see cref="ParameterSchemaBag"/>.</returns>
        private static ParameterSchemaBag ToParameterBag( ParameterSchema parameter )
        {
            return new ParameterSchemaBag
            {
                AllowedValues = parameter.AllowedValues?.ToList(),
                DataType = parameter.DataType,
                DefaultValue = parameter.DefaultValue,
                Instructions = parameter.Instructions,
                IsRequired = parameter.IsRequired,
                IsCollection = parameter.IsCollection,
                Name = parameter.Name,
            };
        }

        /// <summary>
        /// Convert the tool instructions to bags that can be sent to the client.
        /// </summary>
        /// <param name="instructions">The current tool instructions.</param>
        /// <returns>A collection of <see cref="ListItemBag"/> objects.</returns>
        private static List<ListItemBag> ToInstructionBags( ToolInstructionSettings instructions )
        {
            var bags = new List<ListItemBag>();

            if ( instructions.Purposes != null )
            {
                bags.AddRange( instructions.Purposes.Select( i => new ListItemBag { Text = "Purpose", Value = i } ) );
            }

            if ( instructions.Usages != null )
            {
                bags.AddRange( instructions.Usages.Select( i => new ListItemBag { Text = "Usage", Value = i } ) );
            }

            if ( instructions.Guardrails != null )
            {
                bags.AddRange( instructions.Guardrails.Select( i => new ListItemBag { Text = "Guardrail", Value = i } ) );
            }

            if ( instructions.Prerequisites != null )
            {
                bags.AddRange( instructions.Prerequisites.Select( i => new ListItemBag { Text = "Prerequisite", Value = i } ) );
            }

            if ( instructions.Examples != null )
            {
                bags.AddRange( instructions.Examples.Select( i => new ListItemBag { Text = "Example", Value = i } ) );
            }

            if ( instructions.ReturnDescription?.IsNotNullOrWhiteSpace() == true )
            {
                bags.Add( new ListItemBag { Text = "Returns", Value = instructions.ReturnDescription } );
            }

            return bags;
        }

        /// <summary>
        /// Converts the instruction bags received from the client to the
        /// native instruction settings.
        /// </summary>
        /// <param name="bags">The bags from the client.</param>
        /// <returns>The native tool instructions.</returns>
        private static ToolInstructionSettings GetInstructionSettings( List<ListItemBag> bags )
        {
            if ( bags == null )
            {
                return new ToolInstructionSettings();
            }

            return new ToolInstructionSettings
            {
                Purposes = bags.Where( b => b.Text == "Purpose" ).Select( b => b.Value ).ToList(),
                Usages = bags.Where( b => b.Text == "Usage" ).Select( b => b.Value ).ToList(),
                Guardrails = bags.Where( b => b.Text == "Guardrail" ).Select( b => b.Value ).ToList(),
                Prerequisites = bags.Where( b => b.Text == "Prerequisite" ).Select( b => b.Value ).ToList(),
                Examples = bags.Where( b => b.Text == "Example" ).Select( b => b.Value ).ToList(),
                ReturnDescription = bags.Where( b => b.Text == "Returns" ).Select( b => b.Value ).FirstOrDefault()
            };
        }

        /// <summary>
        /// Converts a <see cref="ParameterSchemaBag"/> to a <see cref="ParameterSchema"/>.
        /// </summary>
        /// <param name="bag">The <see cref="ParameterSchemaBag"/> containing the parameter data to be converted.</param>
        /// <returns>A <see cref="ParameterSchema"/> object populated with the data from the specified <paramref name="bag"/>.</returns>
        private ParameterSchema FromParameterBag( ParameterSchemaBag bag )
        {
            return new ParameterSchema
            {
                AllowedValues = bag.AllowedValues?.ToList(),
                DataType = bag.DataType,
                DefaultValue = bag.DefaultValue,
                Instructions = bag.Instructions,
                IsRequired = bag.IsRequired,
                IsCollection = bag.IsCollection,
                Name = bag.Name,
            };
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
            var entityService = new AISkillToolService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );
            var skill = AISkillCache.Get( GetSkillId( entity ) );

            if ( entity == null )
            {
                return ActionBadRequest( $"{AISkillTool.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {AISkillTool.FriendlyTypeName}." );
            }

            if ( skill == null || skill.CodeEntityTypeId.HasValue )
            {
                return ActionBadRequest( "Cannot delete tools for code-based skills." );
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

            return ActionOk( new ValidPropertiesBox<AISkillToolBag>
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
        public BlockActionResult Save( ValidPropertiesBox<AISkillToolBag> box )
        {
            var entityService = new AISkillToolService( RockContext );

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
            if ( !ValidateAISkillTool( entity, out var validationMessage ) )
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

            return ActionOk( new ValidPropertiesBox<AISkillToolBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        #endregion
    }
}
