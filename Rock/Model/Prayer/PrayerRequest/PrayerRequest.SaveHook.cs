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
using System.Linq;
using System.Threading.Tasks;

using Rock.AI.Automations;
using Rock.AI.Classes.Moderations;
using Rock.Data;
using Rock.Enums.AI;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class PrayerRequest
    {
        /// <summary>
        /// Save hook implementation for <see cref="PrayerRequest"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<PrayerRequest>
        {
            private bool _shouldRunAIAutomations = false;

            protected override void PreSave()
            {
                base.PreSave();

                if ( State == EntityContextState.Added )
                {
                    // Always looks for automations and run them (if any) for new records.
                    _shouldRunAIAutomations = true;
                }
                else if ( State == EntityContextState.Modified )
                {
                    var previousText = Entry.OriginalValues["Text"].ToStringSafe();
                    var newText = Entity.Text.ToStringSafe();

                    // Only run the automations for existing records if the text was changed.
                    if ( !previousText.Equals( newText, StringComparison.OrdinalIgnoreCase ) )
                    {
                        _shouldRunAIAutomations = true;
                    }
                }
            }

            /// <summary>
            /// Called after the save operation has been executed
            /// </summary>
            protected override void PostSave()
            {
                base.PostSave();

                if ( _shouldRunAIAutomations )
                {
                    Task.Run( async () => await RunAIAutomations() );
                }
            }

            /// <summary>
            /// Runs any configured AI Automations and updates the PrayerRequest entity.
            /// If changes are made to the PrayerRequest entity the Entity is saved again
            /// without Pre/Post SaveHooks being executed.
            /// </summary>
            private async Task RunAIAutomations()
            {
                // A flag to determine if any of the automations modified the entity
                // which would require us to save values.
                var isEntityModified = false;
                var categoryId = Entity.CategoryId.ToIntSafe();

                // The AI configuration is determined by the category
                // so if there's no category we can skip any additional checks.
                if ( categoryId > 0 )
                {
                    var aiProviderService = new AIProviderService( RockContext );
                    var aiConfig = aiProviderService.GetCompletionConfiguration( categoryId );

                    if ( aiConfig == null )
                    {
                        return;
                    }

                    // Determine if there are any AI automations that use the Formatter template ( text changes ).
                    var hasTextChangingCompletions =
                        aiConfig.RemoveNames != NameRemoval.NoChanges
                        || aiConfig.TextEnhancement != TextEnhancement.NoChanges;

                    // Determine if there are any AI automations that use the Analyzer template.
                    var hasAnalysisCompletions = aiConfig.ClassifySentiment ||
                        aiConfig.AutoCategorize ||
                        aiConfig.CheckPublicAppropriateness ||
                        aiConfig.EnableAIModeration;

                    // If there are no configured automations return without sending any requests.
                    if ( !hasTextChangingCompletions && !hasAnalysisCompletions )
                    {
                        return;
                    }

                    var prayerRequestService = new PrayerRequestService( RockContext );

                    // It's important that the text formatting changes are run first
                    // so that any subsequent completions use the updated text rather than the original text.
                    if ( hasTextChangingCompletions )
                    {
                        isEntityModified = await ProcessTextFormatting( prayerRequestService, aiConfig );
                    }

                    // Analysis completions are items like auto-categorization and sentiment classification.
                    if ( hasAnalysisCompletions )
                    {
                        isEntityModified = await ProcessAnalysis( prayerRequestService, aiConfig ) || isEntityModified;
                    }

                    // Moderation - looking for harmful or offensive content.
                    if ( aiConfig.EnableAIModeration )
                    {
                        isEntityModified = await ProcessModeration( aiConfig ) || isEntityModified;
                    }

                    if ( isEntityModified )
                    {
                        // Disable save hooks so we don't endlessly update our text.
                        var disablePrePostSaveHooks = true;
                        RockContext.SaveChanges( disablePrePostSaveHooks );
                    }
                }
            }

            /// <summary>
            /// Performs the text formatting completion and updates the PrayerRequest entity if necessary.
            /// </summary>
            /// <param name="prayerRequestService">The PrayerRequestService to use to call the AIAutomationFormatter completion.</param>
            /// <param name="aiAutomationConfig">The AIAutomation configuration to use.</param>
            /// <returns><c>true</c> if the PrayerRequest was modified; otherwise <c>false</c>.</returns>
            private async Task<bool> ProcessTextFormatting( PrayerRequestService prayerRequestService, AIAutomation aiAutomationConfig )
            {
                var isEntityModified = false;

                // Get the AI Completion response from the AIProvider.
                var formatterResponse = await prayerRequestService.GetAIAutomationFormatterResults( Entity, aiAutomationConfig );
                var hasModifiedText = !Entity.Text.Equals( formatterResponse.Content, StringComparison.OrdinalIgnoreCase );

                // If the text was modified then capture the original text
                // (if not already captured) before updating the PrayerRequest.Text.
                if ( hasModifiedText )
                {
                    if ( Entity.OriginalRequest.IsNullOrWhiteSpace() )
                    {
                        Entity.OriginalRequest = Entity.Text;
                    }

                    Entity.Text = formatterResponse.Content;
                    isEntityModified = true;
                }

                return isEntityModified;
            }

            /// <summary>
            /// Performs the text analysis completion and updates the PrayerRequest entity if necessary.
            /// </summary>
            /// <param name="prayerRequestService">The PrayerRequestService to use to call the AIAutomationAnalyzer completion.</param>
            /// <param name="aiAutomationConfig">The AIAutomation configuration to use.</param>
            /// <returns><c>true</c> if the PrayerRequest was modified; otherwise <c>false</c>.</returns>
            private async Task<bool> ProcessAnalysis( PrayerRequestService prayerRequestService, AIAutomation aiAutomationConfig )
            {
                var wasModified = false;
                var analysisResponse = await prayerRequestService.GetAIAutomationAnalyzerResults( Entity, aiAutomationConfig );

                // If the configuration was asked to classify sentiment
                // and there's a value in the response
                // and that response id is one of those we provided.
                // then update the Entity and the wasModified flag.
                if ( aiAutomationConfig.ClassifySentiment && analysisResponse.SentimentId.HasValue )
                {
                    var sentiments = DefinedTypeCache.Get( SystemGuid.DefinedType.SENTIMENT_EMOTIONS );

                    if ( sentiments.DefinedValues.Any( v => v.Id == analysisResponse.SentimentId ) )
                    {
                        Entity.SentimentEmotionValueId = analysisResponse.SentimentId;
                        wasModified = true;
                    }
                }

                // If the configuration was asked to categorize
                // and there's a value in the response
                // and that response id is one of those we provided.
                // then update the Entity and the wasModified flag.
                if ( aiAutomationConfig.AutoCategorize && analysisResponse.CategoryId.HasValue )
                {
                    if ( aiAutomationConfig.ChildCategories.Any( c => c.Id == analysisResponse.CategoryId ) )
                    {
                        Entity.CategoryId = analysisResponse.CategoryId;
                        wasModified = true;
                    }
                }

                // If the configuration was asked to check appropriateness
                // and the result is that the text is not appropriate for the public
                // then update the Entity properties IsPublic and FlagCount and the wasModified flag.
                var isInappropriate = analysisResponse.IsAppropriateForPublic.HasValue && analysisResponse.IsAppropriateForPublic.Value == false;
                if ( aiAutomationConfig.CheckPublicAppropriateness && isInappropriate )
                {
                    Entity.IsPublic = false;

                    var flagCount = Entity.FlagCount.ToIntSafe() + 1;
                    Entity.FlagCount = flagCount;

                    wasModified = true;
                }

                return wasModified;
            }

            /// <summary>
            /// Performs the moderation completion and updates the PrayerRequest entity if necessary.
            /// </summary>
            /// <param name="aiAutomationConfig">The AIAutomation configuration to use.</param>
            /// <returns><c>true</c> if the PrayerRequest was modified; otherwise <c>false</c>.</returns>
            private async Task<bool> ProcessModeration( AIAutomation aiAutomationConfig )
            {
                // Call the moderations endpoint for the AIProvider.
                var moderations = await aiAutomationConfig.AIProviderComponent.GetModerations( aiAutomationConfig.AIProvider, new ModerationsRequest
                {
                    Input = Entity.Text,
                    Model = "text-moderation-latest"
                } );

                // Get the bit mask of detected moderation flags.
                Entity.ModerationFlags = ( long ) moderations.ModerationsResponseCategories.ModerationFlags;

                // If there were any detected moderation flags and we have a moderation workflow
                // then launch the workflow and return true to indicate the entity was modified.
                var moderationWorkflow = aiAutomationConfig.ModerationAlertWorkflowType;
                var workflowTypeGuid = moderationWorkflow?.Guid ?? Guid.Empty;
                if ( Entity.ModerationFlags > 0 && workflowTypeGuid != null && !workflowTypeGuid.IsEmpty() )
                {
                    var currentPersonAliasId = DbContext.GetCurrentPersonAlias()?.Id;
                    var workflowAttributes = new Dictionary<string, string>
                    {
                        { "IsHate", moderations.ModerationsResponseCategories.IsHate.ToString() },
                        { "IsSelfHarm", moderations.ModerationsResponseCategories.IsSelfHarm.ToString() },
                        { "IsSexual", moderations.ModerationsResponseCategories.IsSexual.ToString() },
                        { "IsSexualMinor", moderations.ModerationsResponseCategories.IsSexualMinor.ToString() },
                        { "IsThreat", moderations.ModerationsResponseCategories.IsThreat.ToString() },
                        { "IsViolent", moderations.ModerationsResponseCategories.IsViolent.ToString() },
                    };

                    Entity.LaunchWorkflow( workflowTypeGuid, moderationWorkflow.Name, workflowAttributes, currentPersonAliasId );
                    return true;
                }

                return false;
            }
        }
    }
}
