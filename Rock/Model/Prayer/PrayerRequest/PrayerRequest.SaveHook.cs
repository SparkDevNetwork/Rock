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

using Microsoft.Extensions.DependencyInjection;

using Rock.AI;
using Rock.AI.Automations;
using Rock.Configuration;
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
                    using ( var aiAutomationRockContext = new RockContext() )
                    {
                        var aiConfig = PrayerRequestService.GetAutomationConfiguration( categoryId, aiAutomationRockContext );

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

                        var prayerRequestService = new PrayerRequestService( aiAutomationRockContext );
                        var prayerRequest = prayerRequestService.Get( Entity.Id );

                        // It's important that the text formatting changes are run first
                        // so that any subsequent completions use the updated text rather than the original text.
                        if ( hasTextChangingCompletions )
                        {
                            try
                            {
                                isEntityModified = await ProcessTextFormatting( prayerRequest, prayerRequestService, aiConfig );
                            }
                            catch ( Exception ex )
                            {
                                ExceptionLogService.LogException( ex );
                            }
                        }

                        // Analysis completions are items like auto-categorization and sentiment classification.
                        if ( hasAnalysisCompletions )
                        {
                            try
                            {
                                isEntityModified = await ProcessAnalysis( prayerRequest, prayerRequestService, aiConfig ) || isEntityModified;
                            }
                            catch ( Exception ex )
                            {
                                ExceptionLogService.LogException( ex );
                            }
                        }

                        // Moderation - looking for harmful or offensive content.
                        if ( aiConfig.EnableAIModeration )
                        {
                            try
                            {
                                isEntityModified = await ProcessModeration( prayerRequest, aiConfig ) || isEntityModified;
                            }
                            catch ( Exception ex )
                            {
                                ExceptionLogService.LogException( ex );
                            }
                        }

                        if ( isEntityModified )
                        {
                            // Disable save hooks so we don't endlessly update our text.
                            var disablePrePostSaveHooks = true;

                            aiAutomationRockContext.SaveChanges( disablePrePostSaveHooks );
                        }
                    }
                }
            }

            /// <summary>
            /// Performs the text formatting completion and updates the PrayerRequest entity if necessary.
            /// </summary>
            /// <param name="prayerRequest">The <see cref="PrayerRequest"/> to format.</param>
            /// <param name="prayerRequestService">The PrayerRequestService to use to call the formatter completion.</param>
            /// <param name="automationConfig">The configuration to use.</param>
            /// <returns><c>true</c> if the PrayerRequest was modified; otherwise <c>false</c>.</returns>
            private async Task<bool> ProcessTextFormatting( PrayerRequest prayerRequest, PrayerRequestService prayerRequestService, PrayerRequestAutomationConfiguration automationConfig )
            {
                var isEntityModified = false;

                // Get the AI Completion response from the AIProvider.
                var formatterResponse = await prayerRequestService.GetAutomationFormatterResultsAsync( prayerRequest, automationConfig );
                var hasModifiedText = !Entity.Text.Equals( formatterResponse, StringComparison.OrdinalIgnoreCase );

                // If the text was modified then capture the original text
                // (if not already captured) before updating the PrayerRequest.Text.
                if ( hasModifiedText )
                {
                    if ( prayerRequest.OriginalRequest.IsNullOrWhiteSpace() )
                    {
                        prayerRequest.OriginalRequest = prayerRequest.Text;
                    }

                    prayerRequest.Text = formatterResponse;
                    isEntityModified = true;
                }

                return isEntityModified;
            }

            /// <summary>
            /// Performs the text analysis completion and updates the PrayerRequest entity if necessary.
            /// </summary>
            /// <param name="prayerRequest">The <see cref="PrayerRequest"/> to analyze.</param>
            /// <param name="prayerRequestService">The PrayerRequestService to use to call the AIAutomationAnalyzer completion.</param>
            /// <param name="automationConfig">The AIAutomation configuration to use.</param>
            /// <returns><c>true</c> if the PrayerRequest was modified; otherwise <c>false</c>.</returns>
            private async Task<bool> ProcessAnalysis( PrayerRequest prayerRequest, PrayerRequestService prayerRequestService, PrayerRequestAutomationConfiguration automationConfig )
            {
                var wasModified = false;
                var analysisResponse = await prayerRequestService.GetAutomationAnalyzerResultsAsync( prayerRequest, automationConfig );

                // If the configuration was asked to classify sentiment
                // and there's a value in the response
                // and that response id is one of those we provided.
                // then update the Entity and the wasModified flag.
                if ( automationConfig.ClassifySentiment && analysisResponse.SentimentId.HasValue )
                {
                    var sentiments = DefinedTypeCache.Get( SystemGuid.DefinedType.SENTIMENT_EMOTIONS );

                    if ( sentiments.DefinedValues.Any( v => v.Id == analysisResponse.SentimentId ) )
                    {
                        prayerRequest.SentimentEmotionValueId = analysisResponse.SentimentId;
                        wasModified = true;
                    }
                }

                // If the configuration was asked to categorize
                // and there's a value in the response
                // and that response id is one of those we provided.
                // then update the Entity and the wasModified flag.
                if ( automationConfig.AutoCategorize && analysisResponse.CategoryId.HasValue )
                {
                    if ( automationConfig.ChildCategories.Any( c => c.Id == analysisResponse.CategoryId ) )
                    {
                        prayerRequest.CategoryId = analysisResponse.CategoryId;
                        wasModified = true;
                    }
                }

                // If the configuration was asked to check appropriateness
                // and the result is that the text is not appropriate for the public
                // then update the Entity properties IsPublic and FlagCount and the wasModified flag.
                var isInappropriate = analysisResponse.IsAppropriateForPublic.HasValue && analysisResponse.IsAppropriateForPublic.Value == false;
                if ( automationConfig.CheckPublicAppropriateness && isInappropriate )
                {
                    prayerRequest.IsPublic = false;

                    var flagCount = Entity.FlagCount.ToIntSafe() + 1;
                    prayerRequest.FlagCount = flagCount;

                    wasModified = true;
                }

                return wasModified;
            }

            /// <summary>
            /// Performs the moderation completion and updates the PrayerRequest entity if necessary.
            /// </summary>
            /// <param name="prayerRequest">The <see cref="PrayerRequest"/> to get moderations for.</param>
            /// <param name="automationConfig">The AIAutomation configuration to use.</param>
            /// <returns><c>true</c> if the PrayerRequest was modified; otherwise <c>false</c>.</returns>
            private async Task<bool> ProcessModeration( PrayerRequest prayerRequest, PrayerRequestAutomationConfiguration automationConfig )
            {
                var service = RockApp.Current.GetRequiredService<TextProcessingService>();
                var request = new ModerationRequest
                {
                    Text = Entity.OriginalRequest ?? Entity.Text,
                };

                var response = await service.GetModerationAsync( request );

                if ( !response.IsSuccessful )
                {
                    return false;
                }

                var wasModified = prayerRequest.ModerationFlags != response.ModerationFlags;

                // Set the bit mask of detected moderation flags.
                prayerRequest.ModerationFlags = response.ModerationFlags;

                // If there were any detected moderation flags and we have a moderation workflow
                // then launch the workflow and return true to indicate the entity was modified.
                var moderationWorkflow = automationConfig.ModerationAlertWorkflowType;
                var workflowTypeGuid = moderationWorkflow?.Guid ?? Guid.Empty;

                if ( prayerRequest.ModerationFlags > 0 && workflowTypeGuid != null && !workflowTypeGuid.IsEmpty() )
                {
                    var currentPersonAliasId = DbContext.GetCurrentPersonAliasId();
                    var workflowAttributes = new Dictionary<string, string>
                    {
                        { "IsHate", response.IsHate.ToString() },
                        { "IsSelfHarm", response.IsSelfHarm.ToString() },
                        { "IsSexual", response.IsSexual.ToString() },
                        { "IsSexualMinor", response.IsSexualMinor.ToString() },
                        { "IsThreat", response.IsThreat.ToString() },
                        { "IsViolent", response.IsViolent.ToString() },
                    };

                    prayerRequest.LaunchWorkflow( workflowTypeGuid, moderationWorkflow.Name, workflowAttributes, currentPersonAliasId );
                }

                return wasModified;
            }
        }
    }
}
