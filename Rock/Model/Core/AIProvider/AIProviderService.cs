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
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Rock.AI.Automations;
using Rock.AI.Classes.ChatCompletions;
using Rock.Data;
using Rock.Enums.AI;
using Rock.Lava;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    public partial class AIProviderService
    {
        /// <summary>
        /// Gets the active no tracking.
        /// </summary>
        /// <returns></returns>
        public IQueryable<AIProvider> GetActiveNoTracking()
        {
            return Queryable()
                .AsNoTracking()
                .Where( a => a.IsActive == true );
        }

        /// <summary>
        /// Gets all no tracking.
        /// </summary>
        /// <returns></returns>
        public IQueryable<AIProvider> GetAllNoTracking()
        {
            return Queryable().AsNoTracking();
        }

        /// <summary>
        /// Returns the active component
        /// </summary>
        /// <returns></returns>
        public AIProvider GetActiveProvider()
        {
            var activeProvider = this.Queryable().FirstOrDefault( p => p.IsActive );
            return activeProvider;
        }

        /// <summary>
        /// Get the AIAutomation configuration for the specified category.
        /// </summary>
        /// <remarks>
        /// Note that ChildCategories will only be populaed if the AIAutomation.AutoCategorize property is <c>true</c>.
        /// </remarks>
        /// <param name="entityCategoryId">The category identifier to get the AI automation for.</param>
        /// <returns>An AIAutomation configuration object.</returns>
        public AIAutomation GetCompletionConfiguration( int entityCategoryId )
        {
            var rockContext = this.Context as RockContext;

            // Get the AI Automation Attribute Category Id.
            var aiAutomationAttributeCategoryId = CategoryCache.GetId( SystemGuid.Category.AI_AUTOMATION.AsGuid() ).ToIntSafe();

            if ( aiAutomationAttributeCategoryId == 0 )
            {
                return null;
            }

            var categoryService = new CategoryService( rockContext );

            // Get all attributes that belong to the AI Automation Attribute Category.
            // Include the GUID for converting to a POCO instead of a list of AttributeValues.
            var attributeIdAndGuid = new AttributeService( rockContext ).GetByCategoryId( aiAutomationAttributeCategoryId )
                .Select( a => new
                {
                    a.Id,
                    a.Guid
                } )
                .ToDictionary( a => a.Id, a => a.Guid );

            // Get all the parent categories of the initial category.
            var entityCategories = categoryService.GetAllAncestors( entityCategoryId );
            var initialCategory = entityCategories.FirstOrDefault( c => c.Id == entityCategoryId );

            // Initialize an AttributeValueService for use in the recursive GetFirstCategoryWithAttributeValues method.
            var attributeValueService = new AttributeValueService( rockContext );
            var categoryWithAttributeValues = GetCategoryAIAutomationConfiguration(
                initialCategory,
                entityCategories,
                attributeIdAndGuid,
                attributeValueService );

            // No AI Automations configured or inherited for this Category.
            if ( categoryWithAttributeValues == null )
            {
                return null;
            }

            // Get the AI Provider and populate it from the Cache.
            var aiProviderAttribute = categoryWithAttributeValues.FirstOrDefault( av => av.Attribute.Guid == SystemGuid.Attribute.AI_AUTOMATION_AI_PROVIDER.AsGuid() );
            var aiProviderGuid = aiProviderAttribute.Value.AsGuid();
            var aiProvider = aiProviderGuid.IsEmpty() ? AIProviderCache.All().FirstOrDefault( ai => ai.IsActive ) : AIProviderCache.Get( aiProviderAttribute.Value );

            // Parse the remaining Attributes.
            Enum.TryParse<TextEnhancement>( categoryWithAttributeValues.FirstOrDefault( av => av.Attribute.Guid == SystemGuid.Attribute.AI_AUTOMATION_TEXT_ENHANCEMENT.AsGuid() )?.Value ?? "0", out var textEnhancement );
            Enum.TryParse<NameRemoval>( categoryWithAttributeValues.FirstOrDefault( av => av.Attribute.Guid == SystemGuid.Attribute.AI_AUTOMATION_REMOVE_NAMES.AsGuid() )?.Value ?? "0", out var removeNames );
            var classifySentiment = categoryWithAttributeValues.FirstOrDefault( av => av.Attribute.Guid == SystemGuid.Attribute.AI_AUTOMATION_CLASSIFY_SENTIMENT.AsGuid() )?.ValueAsBoolean ?? false;
            var autoCategorize = categoryWithAttributeValues.FirstOrDefault( av => av.Attribute.Guid == SystemGuid.Attribute.AI_AUTOMATION_AUTO_CATEGORIZE.AsGuid() )?.ValueAsBoolean ?? false;
            var enableAIModeration = categoryWithAttributeValues.FirstOrDefault( av => av.Attribute.Guid == SystemGuid.Attribute.AI_AUTOMATION_ENABLE_AI_MODERATION.AsGuid() )?.ValueAsBoolean ?? false;
            var checkForPublicAppropriateness = categoryWithAttributeValues.FirstOrDefault( av => av.Attribute.Guid == SystemGuid.Attribute.AI_AUTOMATION_CHECK_PUBLIC_APPROPRIATENESS.AsGuid() )?.ValueAsBoolean ?? false;

            // Only go get the categories if they're going to be used.
            var childCategories = autoCategorize ? categoryService.GetAllDescendents( entityCategoryId ).ToList() : new List<Category>();

            var workflowTypeGuid = categoryWithAttributeValues.FirstOrDefault( av => av.Attribute.Guid == SystemGuid.Attribute.AI_AUTOMATION_MODERATION_ALERT_WORKFLOW_TYPE.AsGuid() ).ConvertToGuidOrDefault();

            var moderationAlertWorkflowType =
                workflowTypeGuid.HasValue ?
                new WorkflowTypeService( rockContext ).Get( workflowTypeGuid.Value ) :
                new WorkflowType();

            return new AIAutomation
            {
                AIProvider = aiProvider.ToEntity(),
                TextEnhancement = textEnhancement,
                RemoveNames = removeNames,
                CheckPublicAppropriateness = checkForPublicAppropriateness,
                ClassifySentiment = classifySentiment,
                AutoCategorize = autoCategorize,
                EnableAIModeration = enableAIModeration,
                ChildCategories = childCategories,
                ModerationAlertWorkflowType = moderationAlertWorkflowType
            };
        }

        /// <summary>
        /// Gets the AI Automation Category AttributeValues for the specified category.
        /// If the Category doesn't have values the method is recursively called on parent categories
        /// until a parent whose "Child Categories Inherit Configuration" Attribute value is true or there are no more parents.
        /// </summary>
        /// <param name="category">The Category to check.</param>
        /// <param name="categories">The list of all ancestor categories.</param>
        /// <param name="attributeIdGuidDictionary">A Dictionary containing the Ids and Guids for the Attributes.</param>
        /// <param name="attributeValueService">The AttributeValueService to use for database calls.</param>
        /// <param name="depth">The number of iterations.</param>
        /// <returns>A List of AttributeValues that belong to the "AI Automations" Attribute Category or null if nothing was found.</returns>
        private List<AttributeValue> GetCategoryAIAutomationConfiguration(
            Category category,
            IEnumerable<Category> categories,
            Dictionary<int, Guid> attributeIdGuidDictionary,
            AttributeValueService attributeValueService,
            int depth = 0 )
        {
            // Get the attribute values for all AI Automation category Attributes.
            var attributeIds = attributeIdGuidDictionary.Keys.ToList();
            var aiAutomationAttributes = attributeValueService
                .GetByAttributeIdsAndEntityId( attributeIds, category.Id )
                .Where( av => av.Value != null && av.Value != "" )
                .ToList();

            // Get the AttributeId of the Attribute that controls whether child categories inherit
            var inheritConfigAttributeId = attributeIdGuidDictionary
            .FirstOrDefault( av => av.Value == SystemGuid.Attribute.AI_AUTOMATION_CHILD_CATEGORIES_INHERIT_CONFIGURATION.AsGuid() ).Key;

            // If there are AI Automation Attributes with values and...
            // (this is the initially requested category OR this category cascades configuration to children).
            if (
                aiAutomationAttributes.Any()
                && ( depth == 0 || aiAutomationAttributes.Any( av => av.AttributeId == inheritConfigAttributeId && av.ValueAsBoolean == true ) ) )
            {
                return aiAutomationAttributes;
            }

            // If nothing was found, but there's a parent category then recurse and increment the depth.
            var parentCategory = categories.FirstOrDefault( c => c.Id == category.ParentCategoryId.ToIntSafe() );
            if ( parentCategory != null )
            {
                return GetCategoryAIAutomationConfiguration( parentCategory, categories, attributeIdGuidDictionary, attributeValueService, ++depth );
            }

            // No AI Automations configured or inherited for this category.
            return null;
        }

        /// <summary>
        /// Sends the messages using the AIPRoviderComponent in the AIAutomation object and returns the first choice deserialized to the specified <typeparamref name="TResponse"/>.
        /// </summary>
        /// <typeparam name="TResponse">The type to deserialize the response to.</typeparam>
        /// <param name="aiAutomation">The AIAutomation object containing the configuration for the AI Automations.</param>
        /// <param name="messages">The list of ChatCompletionsRequestMessages to send.</param>
        /// <returns>The deserialized result or a default of the <typeparamref name="TResponse"/> if unable to deserialize.</returns>
        public async Task<TResponse> GetCompletionResponseFirstChoice<TResponse>( AIAutomation aiAutomation, List<ChatCompletionsRequestMessage> messages )
        {
            var chatRequest = new ChatCompletionsRequest
            {
                Model = aiAutomation.AIModel,
                Messages = messages
            };

            var aiModifiedText = await aiAutomation.AIProviderComponent.GetChatCompletions( aiAutomation.AIProvider, chatRequest );
            var responseText = aiModifiedText.Choices.FirstOrDefault()?.Text ?? string.Empty;

            try
            {
                return JsonConvert.DeserializeObject<TResponse>( responseText );
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Sends the messages using the AIPRoviderComponent in the AIAutomation object and returns the first choice.
        /// </summary>
        /// <param name="aiAutomation">The AIAutomation object containing the configuration for the AI Automations.</param>
        /// <param name="messages">The list of ChatCompletionsRequestMessages to send.</param>
        /// <returns>The raw Text of the first choice sent by the completion endpoint.</returns>
        public async Task<string> GetCompletionResponseFirstChoice( AIAutomation aiAutomation, List<ChatCompletionsRequestMessage> messages )
        {
            var chatRequest = new ChatCompletionsRequest
            {
                Model = aiAutomation.AIModel,
                Messages = messages
            };

            var aiModifiedText = await aiAutomation.AIProviderComponent.GetChatCompletions( aiAutomation.AIProvider, chatRequest );

            try
            {
                return aiModifiedText.Choices.FirstOrDefault()?.Text ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
