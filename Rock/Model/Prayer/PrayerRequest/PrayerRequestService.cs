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

using Microsoft.Extensions.DependencyInjection;

using Rock.AI;
using Rock.AI.Automations;
using Rock.AI.Classes.ChatCompletions;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.AI;
using Rock.Lava;
using Rock.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.PrayerRequest"/> entity objects.
    /// </summary>
    public partial class PrayerRequestService
    {
        /// <summary>
        /// Returns a collection of active <see cref="Rock.Model.PrayerRequest">PrayerRequests</see> that
        /// are in a specified <see cref="Rock.Model.Category"/> or any of its subcategories.
        /// </summary>
        /// <param name="categoryIds">A <see cref="System.Collections.Generic.List{Int32}"/> of
        /// the <see cref="Rock.Model.Category"/> IDs to retrieve PrayerRequests for.</param>
        /// <param name="onlyApproved">set false to include unapproved requests.</param>
        /// <param name="onlyUnexpired">set false to include expired requests.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.PrayerRequest"/> that
        /// are in the specified <see cref="Rock.Model.Category"/> or any of its subcategories.</returns>
        public IEnumerable<PrayerRequest> GetByCategoryIds( List<int> categoryIds, bool onlyApproved = true, bool onlyUnexpired = true )
        {
            PrayerRequest prayerRequest = new PrayerRequest();
            Type type = prayerRequest.GetType();

            var prayerRequestEntityTypeId = EntityTypeCache.GetId( type );

            // Get all PrayerRequest category Ids that are the **parent or child** of the given categoryIds.
            CategoryService categoryService = new CategoryService( ( RockContext ) Context );
            IEnumerable<int> expandedCategoryIds = categoryService.GetByEntityTypeId( prayerRequestEntityTypeId )
                .Where( c => categoryIds.Contains( c.Id ) || categoryIds.Contains( c.ParentCategoryId ?? -1 ) )
                .Select( a => a.Id );

            // Now find the active PrayerRequests that have any of those category Ids.
            var list = Queryable( "RequestedByPersonAlias.Person" ).Where( p => p.IsActive == true && expandedCategoryIds.Contains( p.CategoryId ?? -1 ) );

            if ( onlyApproved )
            {
                list = list.Where( p => p.IsApproved == true );
            }

            if ( onlyUnexpired )
            {
                list = list.Where( p => RockDateTime.Today <= p.ExpirationDate );
            }

            return list;
        }

        /// <summary>
        /// Returns a active, approved, unexpired <see cref="Rock.Model.PrayerRequest">PrayerRequests</see>
        /// order by urgency and then by total prayer count.
        /// </summary>
        /// <returns>A queryable collection of <see cref="Rock.Model.PrayerRequest">PrayerRequest</see>.</returns>
        public IOrderedQueryable<PrayerRequest> GetActiveApprovedUnexpired()
        {
            return Queryable()
                .Where( p => p.IsActive == true && p.IsApproved == true && RockDateTime.Today <= p.ExpirationDate )
                .OrderByDescending( p => p.IsUrgent ).ThenBy( p => p.PrayerCount );
        }

        /// <summary>
        /// Adds the prayer interaction to the queue to be processed later.
        /// </summary>
        /// <param name="prayerRequest">The prayer request.</param>
        /// <param name="currentPerson">The current person logged in and executing the action.</param>
        /// <param name="summary">The interaction summary text.</param>
        /// <param name="userAgent">The user agent of the request.</param>
        /// <param name="ipAddress">The IP address of the request.</param>
        /// <param name="browserSessionGuid">The browser session unique identifier.</param>
        public static void EnqueuePrayerInteraction( PrayerRequest prayerRequest, Person currentPerson, string summary, string userAgent, string ipAddress, Guid? browserSessionGuid )
        {
            var channelId = InteractionChannelCache.Get( Rock.SystemGuid.InteractionChannel.PRAYER_EVENTS ).Id;

            // Write the Interaction by way of a transaction.
            var info = new Rock.Transactions.InteractionTransactionInfo
            {
                InteractionChannelId = channelId,

                ComponentEntityId = prayerRequest.Id,
                ComponentName = prayerRequest.Name,

                InteractionSummary = summary,
                InteractionOperation = "Prayed",
                PersonAliasId = currentPerson?.PrimaryAliasId,

                UserAgent = userAgent,
                IPAddress = ipAddress,
                BrowserSessionId = browserSessionGuid
            };

            var interactionTransaction = new Rock.Transactions.InteractionTransaction( info );
            interactionTransaction.Enqueue();
        }

        /// <summary>
        /// Gets a collection of prayer requests by applying a standard filtering
        /// algorithm as specified in the options.
        /// </summary>
        /// <param name="options">The options that specifies the filters.</param>
        /// <returns>A collection of <see cref="PrayerRequest"/> objects.</returns>
        public IQueryable<PrayerRequest> GetPrayerRequests( PrayerRequestQueryOptions options )
        {
            var qryPrayerRequests = Queryable();

            // If not including inactive requests then filter them out.
            if ( !options.IncludeInactive )
            {
                qryPrayerRequests = qryPrayerRequests.Where( r => r.IsActive ?? true );
            }

            // If not including expired requests then filter them out.
            if ( !options.IncludeExpired )
            {
                qryPrayerRequests = qryPrayerRequests.Where( r => !r.ExpirationDate.HasValue
                    || r.ExpirationDate >= RockDateTime.Now );
            }

            // If not including unapproved requests then filter them out.
            if ( !options.IncludeUnapproved )
            {
                qryPrayerRequests = qryPrayerRequests.Where( r => r.IsApproved == true );
            }

            // If not including non-public requests then filter them out.
            if ( !options.IncludeNonPublic )
            {
                qryPrayerRequests = qryPrayerRequests.Where( r => r.IsPublic == true );
            }

            // Filter by category if we have been given any.
            if ( options.Categories != null && options.Categories.Any() )
            {
                var categoryService = new CategoryService( ( RockContext ) Context );
                var categories = new List<Guid>( options.Categories );

                // If filtered by category, only show prayer requests in that
                // category or any of its descendant categories.
                foreach ( var categoryGuid in options.Categories )
                {
                    categoryService.GetAllDescendents( categoryGuid )
                        .Select( a => a.Guid )
                        .ToList()
                        .ForEach( c => categories.Add( c ) );
                }

                categories = categories.Distinct().ToList();

                qryPrayerRequests = qryPrayerRequests
                    .Include( r => r.Category )
                    .Where( r => r.CategoryId.HasValue && categories.Contains( r.Category.Guid ) );
            }

            // Filter by campus if we have been given any.
            if ( options.Campuses != null && options.Campuses.Any() )
            {
                if ( options.IncludeEmptyCampus )
                {
                    qryPrayerRequests = qryPrayerRequests
                        .Include( r => r.Campus )
                        .Where( r => !r.CampusId.HasValue || ( r.CampusId.HasValue && options.Campuses.Contains( r.Campus.Guid ) ) );
                }
                else
                {
                    qryPrayerRequests = qryPrayerRequests
                        .Include( r => r.Campus )
                        .Where( r => r.CampusId.HasValue && options.Campuses.Contains( r.Campus.Guid ) );
                }
            }

            // Filter by group if it has been specified.
            if ( options.GroupGuids?.Any() ?? false )
            {
                qryPrayerRequests = qryPrayerRequests
                    .Where( a => options.GroupGuids.Contains( a.Group.Guid ) );
            }

            // If we are not filtering by group, then exclude any group requests
            // unless the block setting including them is enabled.
            if ( !( options.GroupGuids?.Any() ?? false ) && !options.IncludeGroupRequests )
            {
                qryPrayerRequests = qryPrayerRequests.Where( a => !a.GroupId.HasValue );
            }

            if ( options.MinutesToFilter != 0 && options.CurrentPersonId != null )
            {
                qryPrayerRequests = qryPrayerRequests.FilterByRecentlyPrayedFor( ( RockContext ) Context, options.CurrentPersonId.Value, options.MinutesToFilter );
            }

            return qryPrayerRequests;
        }

        /// <summary>
        /// Gets the last prayed details for a collection of prayer requests.
        /// </summary>
        /// <param name="prayerRequestIds">The prayer request identifiers.</param>
        /// <returns>A collection of <see cref="PrayerRequestLastPrayedDetail"/> that describe the most recent prayer interactions.</returns>
        public IEnumerable<PrayerRequestLastPrayedDetail> GetLastPrayedDetails( IEnumerable<int> prayerRequestIds )
        {
            var prayerRequestInteractionChannel = InteractionChannelCache.Get( Rock.SystemGuid.InteractionChannel.PRAYER_EVENTS ).Id;

            return new InteractionService( ( RockContext ) Context ).Queryable()
                .Where( i =>
                    i.InteractionComponent.EntityId.HasValue
                    && prayerRequestIds.Contains( i.InteractionComponent.EntityId.Value )
                    && i.InteractionComponent.InteractionChannelId == prayerRequestInteractionChannel
                    && i.Operation == "Prayed" )
                .GroupBy( i => i.InteractionComponentId )
                .Select( i => i.OrderByDescending( x => x.InteractionDateTime ).FirstOrDefault() )
                .Select( y => new PrayerRequestLastPrayedDetail
                {
                    RequestId = y.InteractionComponent.EntityId.Value,
                    PrayerDateTime = y.InteractionDateTime,
                    FirstName = y.PersonAlias.Person.NickName,
                    LastName = y.PersonAlias.Person.LastName
                } )
                .ToList();
        }

        /// <summary>
        /// Launches a workflow in response to a prayed for action.
        /// </summary>
        /// <param name="prayerRequest">The prayer request that was prayed for.</param>
        /// <param name="workflowTypeGuid">The workflow type unique identifier to be launched.</param>
        /// <param name="currentPerson">The person that prayed for the request.</param>
        public static void LaunchPrayedForWorkflow( PrayerRequest prayerRequest, Guid workflowTypeGuid, Person currentPerson )
        {
            var workflowType = WorkflowTypeCache.Get( workflowTypeGuid );

            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
            {
                try
                {
                    // Create parameters
                    var parameters = new Dictionary<string, string>();
                    parameters.Add( "EntityGuid", prayerRequest.Guid.ToString() );

                    if ( currentPerson != null )
                    {
                        parameters.Add( "PrayerOfferedByPerson", currentPerson.PrimaryAlias.Guid.ToString() );
                        parameters.Add( "PrayerOfferedByPersonAliasGuid", currentPerson.PrimaryAlias.Guid.ToString() );
                    }

                    prayerRequest.LaunchWorkflow( workflowTypeGuid, prayerRequest.Name, parameters, currentPerson?.PrimaryAliasId );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                }
            }
        }

        /// <summary>
        /// Launches a workflow in response to a prayer request flagged action.
        /// </summary>
        /// <param name="prayerRequest">The prayer request that was flagged.</param>
        /// <param name="workflowTypeGuid">The workflow type unique identifier to be launched.</param>
        /// <param name="currentPerson">The person that flagged the request.</param>
        public static void LaunchFlaggedWorkflow( PrayerRequest prayerRequest, Guid workflowTypeGuid, Person currentPerson )
        {
            var workflowType = WorkflowTypeCache.Get( workflowTypeGuid );

            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
            {
                try
                {
                    // Create parameters
                    var parameters = new Dictionary<string, string>();
                    parameters.Add( "EntityGuid", prayerRequest.Guid.ToString() );

                    if ( currentPerson != null )
                    {
                        parameters.Add( "FlaggedByPerson", currentPerson.PrimaryAlias.Guid.ToString() );
                        parameters.Add( "FlaggedByPersonAliasGuid", currentPerson.PrimaryAlias.Guid.ToString() );
                    }

                    prayerRequest.LaunchWorkflow( workflowTypeGuid, prayerRequest.Name, parameters, currentPerson?.PrimaryAliasId );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                }
            }
        }

        #region AI Automation

        private PrayerRequest.PrayerRequestAICompletions AICompletionTemplates =>
            SystemSettings.GetValue( SystemKey.SystemSetting.PRAYER_REQUEST_AI_COMPLETIONS )
                    .FromJsonOrNull<PrayerRequest.PrayerRequestAICompletions>();

        /// <summary>
        /// Get the PrayerRequestAutomationConfiguration for the specified category.
        /// </summary>
        /// <remarks>
        /// Note that ChildCategories will only be populaed if the AutoCategorize property is <c>true</c>.
        /// </remarks>
        /// <param name="entityCategoryId">The category identifier to get the AI automation for.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>An PrayerRequestAutomationConfiguration configuration object.</returns>
        internal static PrayerRequestAutomationConfiguration GetAutomationConfiguration( int entityCategoryId, RockContext rockContext )
        {
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

            var moderationAlertWorkflowType = workflowTypeGuid.HasValue
                ? new WorkflowTypeService( rockContext ).Get( workflowTypeGuid.Value )
                : null;

            return new PrayerRequestAutomationConfiguration
            {
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
        private static List<AttributeValue> GetCategoryAIAutomationConfiguration(
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
        /// Sends the Text Formatting Completion to the AI Provider's GetChatCompletions method and returns the first choice of the response.
        /// </summary>
        /// <param name="prayerRequest">The Prayer Request to run the text formatting completions for.</param>
        /// <param name="automationConfig">The AIAutomation configuration to use with the completion request.</param>
        /// <returns>A string containing the modified text.</returns>
        internal async Task<string> GetAutomationFormatterResultsAsync( PrayerRequest prayerRequest, PrayerRequestAutomationConfiguration automationConfig )
        {
            var template = AICompletionTemplates?.PrayerRequestFormatterTemplate;

            if ( template.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var service = RockApp.Current.GetRequiredService<TextProcessingService>();
            var request = new ChatCompletionRequest
            {
                Message = GetPrayerRequestFormatterText( prayerRequest, template, automationConfig ),
            };

            var response = await service.GetChatCompletionAsync( request );

            if ( !response.IsSuccessful )
            {
                return prayerRequest.Text;
            }

            return response.GetText();
        }

        /// <summary>
        /// Sends the Analyzer Completion to the AI Provider's GetChatCompletions method and returns the first choice of the response.
        /// </summary>
        /// <param name="prayerRequest">The Prayer Request to run the analysis completions for.</param>
        /// <param name="automationConfig">The configuration to use with the completion request.</param>
        /// <returns>A AnalyzePrayerRequestResponse containing the results of the analysis from the AIProvider's completion.</returns>
        internal async Task<AnalyzePrayerRequestResponse> GetAutomationAnalyzerResultsAsync( PrayerRequest prayerRequest, PrayerRequestAutomationConfiguration automationConfig )
        {
            var template = AICompletionTemplates?.PrayerRequestAnalyzerTemplate;

            if ( template.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var service = RockApp.Current.GetRequiredService<TextProcessingService>();
            var request = new ChatCompletionRequest
            {
                Message = GetPrayerRequestAnalyzerText( prayerRequest, template, automationConfig ),
            };

            var response = await service.GetChatCompletionAsync( request );

            if ( !response.IsSuccessful )
            {
                return null;
            }

            return response.GetText().FromJsonOrNull<AnalyzePrayerRequestResponse>();
        }

        /// <summary>
        /// Gets the ChatCompletionsRequestMessage list for a specified <paramref name="template"/>
        /// and the merge objects to be used by the Analyzer template.
        /// </summary>
        /// <param name="prayerRequest">The PrayerRequest the message should be generated for.</param>
        /// <param name="template">The template to use for resolving merge fields.</param>
        /// <param name="automationConfig">The AIAutomation object containing the configuration.</param>
        /// <returns>A List of ChatCompletionsRequestMessage objects.</returns>
        private static string GetPrayerRequestAnalyzerText( PrayerRequest prayerRequest, string template, PrayerRequestAutomationConfiguration automationConfig )
        {
            var sentiments = DefinedTypeCache.Get( SystemGuid.DefinedType.SENTIMENT_EMOTIONS ).ToEntity();

            // Get any data for use across all merge objects.
            var mergeObjects = new Dictionary<string, object>
            {
                { "PrayerRequest", prayerRequest },
                { "AutoCategorize", automationConfig.AutoCategorize },
                { "ClassifySentiment", automationConfig.ClassifySentiment },
                { "CheckAppropriateness", automationConfig.CheckPublicAppropriateness },
                { "SentimentEmotions", sentiments },
                { "Categories", automationConfig.ChildCategories }
            };

            return template.ResolveMergeFields( mergeObjects );
        }

        /// <summary>
        /// Gets the ChatCompletionsRequestMessage list for a specified <paramref name="template"/> 
        /// and the merge objects to be used by the Formatter template.
        /// </summary>
        /// <param name="prayerRequest">The PrayerRequest the message should be generated for.</param>
        /// <param name="template">The template to use for resolving merge fields.</param>
        /// <param name="automationConfig">The AIAutomation object containing the configuration.</param>
        /// <returns>A List of ChatCompletionsRequestMessage objects.</returns>
        private static string GetPrayerRequestFormatterText( PrayerRequest prayerRequest, string template, PrayerRequestAutomationConfiguration automationConfig )
        {
            var parentCategoryId = prayerRequest.Category?.ParentCategoryId.HasValue == true
                ? CategoryCache.Get( prayerRequest.Category.ParentCategoryId.Value )?.ParentCategoryId
                : null;

            // Get any data for use across all merge objects.
            var mergeObjects = new Dictionary<string, object>
            {
                { "PrayerRequest", prayerRequest },
                { "ParentCategoryId", parentCategoryId },
                { "EnableRemovalLastNames", automationConfig.RemoveNames == NameRemoval.LastNamesOnly },
                { "EnableRemovalFirstAndLastNames", automationConfig.RemoveNames == NameRemoval.FirstAndLastNames },
                { "EnableFixFormattingAndSpelling", automationConfig.TextEnhancement == TextEnhancement.MinorFormattingAndSpelling },
                { "EnableEnhancedReadability", automationConfig.TextEnhancement == TextEnhancement.EnhanceReadability }
            };

            return template.ResolveMergeFields( mergeObjects );
        }

        #region Obsolete Methods

        /// <summary>
        /// Sends the Text Formatting Completion to the AI Provider's GetChatCompletions method and returns the first choice of the response.
        /// </summary>
        /// <param name="prayerRequest">The Prayer Request to run the text formatting completions for.</param>
        /// <param name="aiAutomation">The AIAutomation configuration to use with the completion request.</param>
        /// <param name="template">
        ///     Optional template to use for the completion request.
        ///     If none is provided the SystemSetting "core_PrayerRequestAICompletions"
        ///     PrayerRequestFormatterTemplate will be used.
        /// </param>
        /// <returns>A PrayerRequestFormatterResponse containing the modified text from the AIProvider's completion.</returns>
        [Obsolete( "This method has been deprecated and moved to an internal feature." )]
        [RockObsolete( "21.0" )]
        public async Task<PrayerRequestFormatterResponse> GetAIAutomationFormatterResults( PrayerRequest prayerRequest, AIAutomation aiAutomation, string template = "" )
        {
            if ( template.IsNullOrWhiteSpace() )
            {
                template = AICompletionTemplates?.PrayerRequestFormatterTemplate;
            }

            if ( template.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var messages = PrayerRequestFormatterMessages( prayerRequest, template, aiAutomation );

            return new PrayerRequestFormatterResponse
            {
                Content = await new AIProviderService( ( RockContext ) Context )
                    .GetCompletionResponseFirstChoice( aiAutomation, messages )
            };
        }

        /// <summary>
        /// Sends the Analyzer Completion to the AI Provider's GetChatCompletions method and returns the first choice of the response.
        /// </summary>
        /// <param name="prayerRequest">The Prayer Request to run the analysis completions for.</param>
        /// <param name="aiAutomation">The AIAutomation configuration to use with the completion request.</param>
        /// <param name="template">
        ///     Optional template to use for the completion request.
        ///     If none is provided the SystemSetting "core_PrayerRequestAICompletions"
        ///     PrayerRequestAnalyzerTemplate will be used.
        /// </param>
        /// <returns>A PrayerRequestAnalyzerResponse containing the results of the analysis from the AIProvider's completion.</returns>
        [Obsolete( "This method has been deprecated and moved to an internal feature." )]
        [RockObsolete( "21.0" )]
        public async Task<PrayerRequestAnalyzerResponse> GetAIAutomationAnalyzerResults( PrayerRequest prayerRequest, AIAutomation aiAutomation, string template = "" )
        {
            if ( template.IsNullOrWhiteSpace() )
            {
                template = AICompletionTemplates?.PrayerRequestAnalyzerTemplate;
            }

            if ( template.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var messages = PrayerRequestAnalyzerMessages( prayerRequest, template, aiAutomation );

            return await new AIProviderService( ( RockContext ) Context )
                .GetCompletionResponseFirstChoice<PrayerRequestAnalyzerResponse>( aiAutomation, messages );
        }

        /// <summary>
        /// Gets the ChatCompletionsRequestMessage list for a specified <paramref name="template"/>
        /// and the merge objects to be used by the Analyzer template.
        /// </summary>
        /// <param name="prayerRequest">The PrayerRequest the message should be generated for.</param>
        /// <param name="template">The template to use for resolving merge fields.</param>
        /// <param name="aiAutomation">The AIAutomation object containing the configuration.</param>
        /// <returns>A List of ChatCompletionsRequestMessage objects.</returns>
        [Obsolete( "This method has been deprecated and moved to an internal feature." )]
        [RockObsolete( "21.0" )]
        public static List<ChatCompletionsRequestMessage> PrayerRequestAnalyzerMessages( PrayerRequest prayerRequest, string template, AIAutomation aiAutomation )
        {
            var sentiments = DefinedTypeCache.Get( SystemGuid.DefinedType.SENTIMENT_EMOTIONS ).ToEntity();

            // Get any data for use across all merge objects.
            var mergeObjects = new Dictionary<string, object>
                    {
                        { "PrayerRequest", prayerRequest },
                        { "AutoCategorize", aiAutomation.AutoCategorize },
                        { "ClassifySentiment", aiAutomation.ClassifySentiment },
                        { "CheckAppropriateness", aiAutomation.CheckPublicAppropriateness },
                        { "SentimentEmotions", sentiments },
                        { "Categories", aiAutomation.ChildCategories }
                    };

            return new List<ChatCompletionsRequestMessage>
            {
                new ChatCompletionsRequestMessage { Role = Enums.AI.ChatMessageRole.User, Content = template.ResolveMergeFields( mergeObjects ) }
            };
        }

        /// <summary>
        /// Gets the ChatCompletionsRequestMessage list for a specified <paramref name="template"/> 
        /// and the merge objects to be used by the Formatter template.
        /// </summary>
        /// <param name="prayerRequest">The PrayerRequest the message should be generated for.</param>
        /// <param name="template">The template to use for resolving merge fields.</param>
        /// <param name="aiAutomation">The AIAutomation object containing the configuration.</param>
        /// <returns>A List of ChatCompletionsRequestMessage objects.</returns>
        [Obsolete( "This method has been deprecated and moved to an internal feature." )]
        [RockObsolete( "21.0" )]
        public static List<ChatCompletionsRequestMessage> PrayerRequestFormatterMessages( PrayerRequest prayerRequest, string template, AIAutomation aiAutomation )
        {
            var parentCategoryId = prayerRequest.Category?.ParentCategoryId.HasValue == true ? CategoryCache.Get( prayerRequest.Category.ParentCategoryId.Value )?.ParentCategoryId : null;

            // Get any data for use across all merge objects.
            var mergeObjects = new Dictionary<string, object>
                    {
                        { "PrayerRequest", prayerRequest },
                        { "ParentCategoryId", parentCategoryId },
                        { "EnableRemovalLastNames", aiAutomation.RemoveNames == NameRemoval.LastNamesOnly },
                        { "EnableRemovalFirstAndLastNames", aiAutomation.RemoveNames == NameRemoval.FirstAndLastNames },
                        { "EnableFixFormattingAndSpelling", aiAutomation.TextEnhancement == TextEnhancement.MinorFormattingAndSpelling },
                        { "EnableEnhancedReadability", aiAutomation.TextEnhancement == TextEnhancement.EnhanceReadability }
                    };

            return new List<ChatCompletionsRequestMessage>
            {
                new ChatCompletionsRequestMessage { Role = Enums.AI.ChatMessageRole.User, Content = template.ResolveMergeFields( mergeObjects ) }
            };
        }

        #endregion

        #endregion
    }

    public static partial class PrayerRequestExtensionMethods
    {
        /// <summary>
        /// Orders the collection of <see cref="PrayerRequest"/> by a defined
        /// set of possible orders.
        /// </summary>
        /// <param name="prayerRequests">The prayer requests.</param>
        /// <param name="order">The order.</param>
        /// <returns>The collection in the requested order.</returns>
        public static IEnumerable<PrayerRequest> OrderBy( this IEnumerable<PrayerRequest> prayerRequests, PrayerRequestOrder order )
        {
            switch ( order )
            {
                case PrayerRequestOrder.Newest:
                    return prayerRequests.OrderByDescending( a => a.EnteredDateTime );

                case PrayerRequestOrder.Oldest:
                    return prayerRequests.OrderBy( a => a.EnteredDateTime );

                case PrayerRequestOrder.Random:
                    return prayerRequests.OrderBy( a => Guid.NewGuid() );

                case 0:
                default:
                    return prayerRequests.OrderBy( a => a.PrayerCount );
            }
        }

        /// <summary>
        /// Orders the collection of <see cref="PrayerRequest"/> by a defined
        /// set of possible orders.
        /// </summary>
        /// <param name="prayerRequests">The prayer requests.</param>
        /// <param name="order">The order.</param>
        /// <returns>The collection in the requested order.</returns>
        public static IEnumerable<PrayerRequest> ThenBy( this IOrderedEnumerable<PrayerRequest> prayerRequests, PrayerRequestOrder order )
        {
            switch ( order )
            {
                case PrayerRequestOrder.Newest:
                    return prayerRequests.ThenByDescending( a => a.EnteredDateTime );

                case PrayerRequestOrder.Oldest:
                    return prayerRequests.ThenBy( a => a.EnteredDateTime );

                case PrayerRequestOrder.Random:
                    return prayerRequests.ThenBy( a => Guid.NewGuid() );

                case 0:
                default:
                    return prayerRequests.ThenBy( a => a.PrayerCount );
            }
        }

        /// <summary>
        /// Orders the collection of <see cref="PrayerRequest"/> by a defined
        /// set of possible orders.
        /// </summary>
        /// <param name="prayerRequests">The prayer requests.</param>
        /// <param name="order">The order.</param>
        /// <returns>The collection in the requested order.</returns>
        internal static IOrderedQueryable<PrayerRequest> ThenBy( this IOrderedQueryable<PrayerRequest> prayerRequests, PrayerRequestOrder order )
        {
            switch ( order )
            {
                case PrayerRequestOrder.Newest:
                    return prayerRequests.ThenByDescending( a => a.EnteredDateTime );

                case PrayerRequestOrder.Oldest:
                    return prayerRequests.ThenBy( a => a.EnteredDateTime );

                case PrayerRequestOrder.Random:
                    return prayerRequests.ThenBy( a => Guid.NewGuid() );

                case 0:
                default:
                    return prayerRequests.ThenBy( a => a.PrayerCount );
            }
        }

        /// <summary>
        /// Filters out recently prayed for objects by x minutes, based on the Interaction data of the current person.
        /// </summary>
        /// <param name="prayerRequests">The current list of prayer requests.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="personId">The person identifier of the currently logged in person.</param>
        /// <param name="minutes">The number of minutes to use when determining which prayer requests to exclude.</param>
        /// <returns>An enumeration of prayer requests that have not been prayed for in the last <paramref name="minutes"/>.</returns>
        public static IQueryable<PrayerRequest> FilterByRecentlyPrayedFor( this IQueryable<PrayerRequest> prayerRequests, RockContext rockContext, int personId, int minutes )
        {
            if ( minutes == 0 )
            {
                return prayerRequests;
            }

            var interactionComponents = new InteractionComponentService( rockContext ).Queryable();
            var interactionChannels = new InteractionChannelService( rockContext ).Queryable();
            var interactions = new InteractionService( rockContext ).Queryable();
            var aliases = new PersonAliasService( rockContext ).Queryable();

            var prayerEventsGuid = SystemGuid.InteractionChannel.PRAYER_EVENTS.AsGuid();
            var entityIdsToFilter = interactionComponents
                .Join(
                // Join our Interaction Channels.
                interactionChannels,
                    ic => ic.InteractionChannelId, // InteractionComponent.InteractionChannelId
                    ich => ich.Id, // InteractionChannel.Id
                    ( ic, ich ) => new
                    {
                        ich.Guid, // InteractionChannel.Guid
                        ic.EntityId, // InteractionComponent.EntityId
                        ic.Id // InteractionComponent.Id
                    }
                )
                // Filter to prayer interaction channel by Guid.
                .Where( a => a.Guid == prayerEventsGuid )
                // Join our Interactions.
                .Join( interactions,
                    a => a.Id, // InteractionComponent.Id
                    i => i.InteractionComponentId, // Interaction.InteractionComponentId
                    ( a, i ) => new
                    {
                        a.EntityId, // InteractionComponent.EntityId
                        i.Operation, // Interaction.Operation
                        i.InteractionDateTime, // Interaction.InteractionDateTime
                        i.PersonAliasId // Interaction.PersonAliasId
                    }
                )
                // Join our PersonAliases.
                .Join( aliases,
                    a => a.PersonAliasId, // Interaction.PersonAliasId
                    al => al.Id, // PersonAlias.Id
                    ( a, al ) => new
                    {
                        a.EntityId, // InteractionComponent.EntityId
                        a.Operation, // Interaction.Operation
                        a.InteractionDateTime, // Interaction.InteractionDateTime
                        al.PersonId // PersonAlias.PersonId
                    }
                )
                // Where the request is this current person, the interaction date time is within our filtered minutes, and where the operation is Prayed.
                .Where( a => a.PersonId == personId
                    && a.InteractionDateTime >= System.Data.Entity.DbFunctions.AddMinutes( RockDateTime.Now, minutes * -1 )
                    && a.Operation == "Prayed" )
                .Select( a => a.EntityId );

            // Filter the prayer requests entity Ids we found from our query out of the original passed in list.
            return prayerRequests.Where( a => !entityIdsToFilter.Any( b => b == a.Id ) );
        }
    }
}
