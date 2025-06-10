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
using System.Data;
using System.Data.Entity;
using System.Dynamic;
using System.IO;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Transactions;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Adaptive Message Lava Command
    /// </summary>
    public class AdaptiveMessageBlock : LavaBlockBase, ILavaSecured
    {
        string _markup = string.Empty;
        LavaElementAttributes _settings = new LavaElementAttributes();

        #region Filter Parameter Names

        /// <summary>
        /// Parameter name for specifying the message key.
        /// </summary>
        public static readonly string ParameterMessageKey = "messagekey";

        /// <summary>
        /// Parameter name for specifying the adaptation per message.
        /// </summary>
        public static readonly string ParameterAdaptationPerMessage = "adaptationspermessage";

        /// <summary>
        /// Parameter name for specifying the trackviews.
        /// </summary>
        public static readonly string ParameterTrackviews = "trackviews";

        /// <summary>
        /// Parameter name for specifying the category identifier.
        /// </summary>
        public static readonly string ParameterCategoryId = "categoryid";

        /// <summary>
        /// Parameter name for specifying the max adaptation count.
        /// </summary>
        public static readonly string ParameterMaxAdaptations = "maxadaptations";

        #endregion

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        /// <exception cref="System.Exception">Could not find the variable to place results in.</exception>
        public override void OnInitialize( string tagName, string markup, List<string> tokens )
        {
            _markup = markup;

            base.OnInitialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            // first ensure that search commands are allowed in the context
            if ( !this.IsAuthorized( context ) )
            {
                result.Write( string.Format( LavaBlockBase.NotAuthorizedMessage, this.SourceElementName ) );
                base.OnRender( context, result );
                return;
            }

            // Parse the Lava Command markup to retrieve paramters.
            var parms = new Dictionary<string, string>();
            LavaHelper.ParseCommandMarkup( _markup, context, parms );

            try
            {
                CommandMode commandMode = CommandMode.Message;
                var messageKey = parms.GetValueOrNull( ParameterMessageKey );
                int? categoryId = null;
                if ( messageKey.IsNullOrWhiteSpace() )
                {
                    categoryId = parms.GetValueOrNull( ParameterCategoryId ).AsIntegerOrNull();
                    if ( !categoryId.HasValue )
                    {
                        throw new Exception( $"Invalid configuration setting. Key is unknown" );
                    }

                    commandMode = CommandMode.Category;
                }

                var adaptationPerMessage = parms.GetValueOrNull( ParameterAdaptationPerMessage ).AsIntegerOrNull();
                if ( !adaptationPerMessage.HasValue )
                {
                    adaptationPerMessage = 1;
                }

                int? maxAdaptations = null;
                if ( commandMode == CommandMode.Category )
                {
                    maxAdaptations = parms.GetValueOrNull( ParameterMaxAdaptations ).AsIntegerOrNull();
                    if ( !maxAdaptations.HasValue )
                    {
                        maxAdaptations = adaptationPerMessage;
                    }
                }

                var rockContext = new RockContext();
                var isTrackViews = parms.GetValueOrNull( ParameterTrackviews ).AsBooleanOrNull();
                var person = context.GetMergeField( "Person" ) as Model.Person;
                if ( person == null )
                {
                    person = LavaHelper.GetCurrentPerson( context );
                }

                if ( person == null )
                {
                    person = LavaHelper.GetCurrentVisitorInContext( context )?.Person;
                }

                var currentDate = RockDateTime.Now;

                List<AdaptiveMessageCache> adaptiveMessages = new List<AdaptiveMessageCache>();
                if ( commandMode == CommandMode.Message )
                {
                    adaptiveMessages = AdaptiveMessageCache.All().Where( a => a.Key == messageKey && IsCurrentlyActive( a.IsActive, a.StartDate, a.EndDate ) ).ToList();
                }
                else
                {
                    var parentCategory = CategoryCache.Get( categoryId.Value );
                    var allCategories = GetCategoryHierarchy( parentCategory );

                    adaptiveMessages = GetAdaptiveMessagesForCategories( rockContext, allCategories );
                }

                var interactionChannelId = InteractionChannelCache.GetId( Rock.SystemGuid.InteractionChannel.ADAPTIVE_MESSAGES.AsGuid() );
                Dictionary<int, int> interactionCounts = null;
                int? personAliasId = null;

                if ( person != null && person.PrimaryAliasId.HasValue )
                {
                    personAliasId = person.PrimaryAliasId.Value;

                    // Retrieves a list of adaptation Ids that have a View Saturation Count from our selected messages found in the Adaptive Message Cache.
                    var validAdaptationIds = adaptiveMessages
                        .SelectMany( m => m.Adaptations )
                        .Where( a => a.ViewSaturationCount.HasValue )
                        .Select( a => a.Id )
                        .ToList();

                    if ( validAdaptationIds.Any() )
                    {
                        // Retrieves interactions filtered by channel, entity existence, and person alias.
                        var interactionQry = new InteractionService( rockContext )
                            .Queryable()
                            .Where( i => i.InteractionComponent.InteractionChannelId == interactionChannelId &&
                                        i.EntityId.HasValue &&
                                        i.PersonAliasId == personAliasId );

                        // Retrieves adaptations where ViewSaturationCount is set.
                        var adaptationsQry = new AdaptiveMessageAdaptationService( rockContext )
                            .Queryable()
                            .Where( a => a.ViewSaturationCount.HasValue );

                        // Joins interactions with adaptations, filters by saturation rules, groups by adaptation, and counts interactions.
                        interactionCounts = interactionQry
                            .Join(
                                adaptationsQry,
                                i => i.EntityId,
                                a => a.Id,
                                ( i, a ) => new
                                {
                                    AdaptationId = a.Id,
                                    i.InteractionDateTime,
                                    SaturationDays = a.ViewSaturationInDays
                                }
                            )
                            .Where( joined => !joined.SaturationDays.HasValue ||
#if REVIEW_WEBFORMS
                                            joined.InteractionDateTime >= DbFunctions.AddDays( currentDate, -joined.SaturationDays.Value ) )
#else
                                            joined.InteractionDateTime >= currentDate.AddDays( -joined.SaturationDays.Value ) )
#endif
                            .GroupBy( joined => joined.AdaptationId )
                            .Select( g => new { AdaptationId = g.Key, InteractionCount = g.Count() } )
                            .ToDictionary( g => g.AdaptationId, g => g.InteractionCount );
                    }
                }

                // Check if any adaptation has SegmentIds before retrieving the person's segment list.
                bool requiresSegments = adaptiveMessages
                    .Any( m => m.Adaptations.Any( a => a.SegmentIds.Any() ) );

                List<int> personSegmentIdList = new List<int>();

                // Only retrieve segments if at least one adaptation has SegmentIds.
                if ( requiresSegments )
                {
#if REVIEW_WEBFORMS
                    personSegmentIdList = LavaPersonalizationHelper.GetPersonalizationSegmentIdListForPersonFromContextCookie(
                        context, System.Web.HttpContext.Current, person );
#else
                    throw new NotImplementedException();
#endif
                }

                var adaptationQry = adaptiveMessages
                    .Select( m => new
                    {
                        AdaptiveMessage = m,
                        Adaptations = m.Adaptations
                            .Where( a => IsCurrentlyActive( a.IsActive, a.StartDate, a.EndDate ) &&
                                        ( !a.SegmentIds.Any() || a.SegmentIds.Any( b => personSegmentIdList.Contains( b ) ) ) &&
                                        ( !a.ViewSaturationCount.HasValue ||
                                          !( interactionCounts?.ContainsKey( a.Id ) ?? false ) || // If interactionCounts is null or the adaptation is not found in interactionCounts, then it has a count of 0 and should be shown.
                                          interactionCounts[a.Id] < a.ViewSaturationCount.Value ) )
                            .OrderBy( a => a.Order )
                            .ThenBy( a => a.Name )
                            .Take( adaptationPerMessage.Value )
                    } )
                    .SelectMany( g => g.Adaptations );


                if ( commandMode == CommandMode.Category )
                {
                    adaptationQry = adaptationQry.Take( maxAdaptations.Value );
                }

                AddLavaMergeFieldsToContext( context, adaptationQry.ToList(), person );

                // Tracks interactions if explicitly enabled (set to true) or if tracking is undefined (set to null) and the given adaptation has a value for View Saturation Count.
                if ( isTrackViews == true || isTrackViews == null )
                {
                    foreach ( var adaptation in adaptationQry )
                    {
                        if ( isTrackViews == true || adaptation.ViewSaturationCount.HasValue )
                        {
                            var info = new InteractionTransactionInfo
                            {
                                InteractionChannelId = interactionChannelId.Value,
                                ComponentEntityId = adaptation.AdaptiveMessage.Id,
                                ComponentName = adaptation.AdaptiveMessage.Name,
                                InteractionOperation = "Viewed",
                                InteractionSummary = adaptation.Name,
                                InteractionEntityId = adaptation.Id,
                                PersonAliasId = personAliasId
                            };

                            var interactionTransaction = new InteractionTransaction( info );
                            interactionTransaction.Enqueue();
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                var message = "Adaptive Message not available. " + ex.Message;
                result.Write( message );
            }

            base.OnRender( context, result );
        }

        private static List<int> GetCategoryHierarchy( CategoryCache parentCategory )
        {
            var categoryIds = new List<int>();

            if ( parentCategory == null )
            {
                return categoryIds;
            }

            var queue = new Queue<CategoryCache>();
            var visitedCategories = new HashSet<int>();

            queue.Enqueue( parentCategory );
            visitedCategories.Add( parentCategory.Id );

            while ( queue.Count > 0 )
            {
                var currentCategory = queue.Dequeue();
                categoryIds.Add( currentCategory.Id );

                foreach ( var child in currentCategory.Categories )
                {
                    if ( !visitedCategories.Contains( child.Id ) )
                    {
                        queue.Enqueue( child );
                        visitedCategories.Add( child.Id );
                    }
                }
            }

            return categoryIds;
        }

        private List<AdaptiveMessageCache> GetAdaptiveMessagesForCategories( RockContext rockContext, List<int> categoryIds )
        {
            if ( categoryIds == null || !categoryIds.Any() )
            {
                return new List<AdaptiveMessageCache>();
            }

            var categoryMappings = new AdaptiveMessageCategoryService( rockContext )
                .Queryable()
                .Where( amc => categoryIds.Contains( amc.CategoryId ) )
                .OrderBy( amc => amc.Order )
                .Select( amc => new { amc.AdaptiveMessageId, amc.CategoryId, amc.Order } )
                .ToList();

            var adaptiveMessages = AdaptiveMessageCache.All()
                .Where( a => a.CategoryIds.Any( id => categoryIds.Contains( id ) ) && IsCurrentlyActive( a.IsActive, a.StartDate, a.EndDate ) )
                .ToList();

            // Perform an in-memory join to order messages correctly
            var orderedMessages = categoryMappings
                .Join(
                    adaptiveMessages,
                    mapping => mapping.AdaptiveMessageId,
                    message => message.Id,
                    ( mapping, message ) => new { Message = message, CategoryOrder = mapping.Order, mapping.CategoryId }
                )
                .OrderBy( joined => categoryIds.IndexOf( joined.CategoryId ) ) // Maintain category hierarchy order
                .ThenBy( joined => joined.CategoryOrder )
                .ThenBy( joined => joined.Message.Name )
                .Select( joined => joined.Message )
                .Distinct()
                .ToList();

            return orderedMessages;
        }

        private void AddLavaMergeFieldsToContext( ILavaRenderContext context, List<AdaptiveMessageAdaptationCache> adaptations, Person person )
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            mergeFields.Add( "Person", person );

            var resolvedAdaptations = adaptations.Select( a => new AdaptiveMessageAdaptation
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                IsActive = a.IsActive,
                Order = a.Order,
                ViewSaturationCount = a.ViewSaturationCount,
                ViewSaturationInDays = a.ViewSaturationInDays,
                AdaptiveMessageId = a.AdaptiveMessageId,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                Attributes = a.Attributes,
                AttributeValues = a.AttributeValues.ToDictionary(
                    av => av.Key,
                    av => new Rock.Web.Cache.AttributeValueCache
                    {
                        AttributeId = av.Value.AttributeId,
                        Value = av.Key == "CallToAction" ? av.Value.Value.ResolveMergeFields( mergeFields ) : av.Value.Value
                    }
                )
            } ).ToList();

            context["messageAdaptations"] = resolvedAdaptations;
            if ( resolvedAdaptations.Count == 1 )
            {
                context["messageAdaptation"] = resolvedAdaptations.First();
            }
            //context.SetMergeField( "messageAdaptations", adaptations, LavaContextRelativeScopeSpecifier.Root );
        }

        /// <summary>
        /// Helper method to determine if the entity is currently active.
        /// </summary>
        /// <param name="isActive">The isActive boolean property</param>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <returns>Boolean value that determines if the entity is currently active</returns>
        private bool IsCurrentlyActive( bool isActive, DateTime? startDate, DateTime? endDate )
        {
            return isActive &&
                   ( startDate == null || startDate <= RockDateTime.Now ) &&
                   ( endDate == null || endDate >= RockDateTime.Now );
        }

        #region ILavaSecured

        /// <inheritdoc/>
        public string RequiredPermissionKey
        {
            get
            {
                return "AdaptiveMessage";
            }
        }

        #endregion

        /// <summary>
        ///
        /// </summary>
        private class DataRowDrop : RockDynamic
        {
            private readonly DataRow _dataRow;

            public DataRowDrop( DataRow dataRow )
            {
                _dataRow = dataRow;
            }

            public override bool TryGetMember( GetMemberBinder binder, out object result )
            {
                if ( _dataRow.Table.Columns.Contains( binder.Name ) )
                {
                    result = _dataRow[binder.Name];
                    return true;
                }

                result = null;
                return false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public enum CommandMode
        {
            /// <summary>
            /// Message
            /// </summary>
            Message,

            /// <summary>
            /// Category
            /// </summary>
            Category
        }
    }
}