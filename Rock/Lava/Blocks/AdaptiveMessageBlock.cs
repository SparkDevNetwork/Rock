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

using DocumentFormat.OpenXml.Bibliography;

using Lucene.Net.Search.Similarities;

using Nest;

using PuppeteerSharp.Media;

using Rock.Data;
using Rock.Model;
using Rock.Reporting.DataFilter.ContentChannelItem;
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
                var adaptiveMessageAdaptationService = new AdaptiveMessageAdaptationService( rockContext );
                var isTrackViews = _settings.GetBoolean( ParameterTrackviews );
                var person = context.GetMergeField( "Person" ) as Model.Person;
                if ( person == null )
                {
                    person = LavaHelper.GetCurrentPerson( context );
                }

                if ( person == null )
                {
                    person = LavaHelper.GetCurrentVisitorInContext( context )?.Person;
                }

                var personSegmentIdList = LavaPersonalizationHelper.GetPersonalizationSegmentIdListForPersonFromContextCookie( context, System.Web.HttpContext.Current, person );
                List<AdaptiveMessageCache> adaptiveMessages = new List<AdaptiveMessageCache>();
                if ( commandMode == CommandMode.Message )
                {
                    adaptiveMessages = AdaptiveMessageCache.All().Where( a => a.Key == messageKey ).ToList();
                }
                else
                {
                    var categoryService = new CategoryService( rockContext );
                    var categoryGuid = categoryService.GetGuid( categoryId.Value );

                    adaptiveMessages = AdaptiveMessageCache.All().Where( a => a.CategoryIds.Contains( categoryId.Value ) ).ToList();
                    GetAaptiveMessageForChildCategories( rockContext, adaptiveMessages, categoryGuid );
                }


                var adaptationQry = adaptiveMessages
                       .SelectMany( a => a.Adaptations.OrderBy( b => b.Order ).ThenBy( b => b.Name ).Take( adaptationPerMessage.Value ) )
                       .Where( a => !a.SegmentIds.Any() || a.SegmentIds.Any( b => personSegmentIdList.Contains( b ) ) );

                if ( commandMode == CommandMode.Category )
                {
                    adaptationQry = adaptationQry.Take( maxAdaptations.Value );
                }
                       
                AddLavaMergeFieldsToContext( context, adaptationQry.ToList() );

                if ( isTrackViews && adaptationQry.Any() )
                {
                    foreach ( var adaptation in adaptationQry )
                    {
                        var interactionChannelId = InteractionChannelCache.GetId( Rock.SystemGuid.InteractionChannel.ADAPTIVE_MESSAGES.AsGuid() );
                        var info = new InteractionTransactionInfo
                        {
                            InteractionChannelId = interactionChannelId.Value,
                            ComponentEntityId = adaptation.AdaptiveMessage.Id,
                            ComponentName = adaptation.AdaptiveMessage.Name,
                            InteractionOperation = "Viewed",
                            InteractionSummary = adaptation.Name,
                            InteractionEntityId = adaptation.Id,
                        };

                        var interactionTransaction = new InteractionTransaction( info );
                        interactionTransaction.Enqueue();
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

        private static void GetAaptiveMessageForChildCategories( RockContext rockContext, List<AdaptiveMessageCache> adaptiveMessages, Guid? categoryGuid )
        {
            var categories = new CategoryService( rockContext )
                .GetChildCategoryQuery( new Rock.Model.Core.Category.Options.ChildCategoryQueryOptions
                {
                    ParentGuid = categoryGuid
                } )
                .ToList()
                .OrderBy( c => c.Order )
                .ThenBy( c => c.Name )
                .ThenBy( c => c.Id )
                .ToList();

            // Message Should be in order of Category order.
            // When considering a tree of categories all of the 1's will be considered first, then all the 2's, then 3's
            foreach ( var category in categories )
            {
                var categoryAdaptiveMessages = AdaptiveMessageCache.All().Where( a => a.CategoryIds.Contains( category.Id ) );
                if ( categoryAdaptiveMessages.Any() )
                {
                    adaptiveMessages.AddRange( categoryAdaptiveMessages.ToList() );
                }
            }
        }

        private void AddLavaMergeFieldsToContext( ILavaRenderContext context, List<AdaptiveMessageAdaptationCache> adaptations )
        {
            context["messageAdaptations"] = adaptations;
            if ( adaptations.Count == 1 )
            {
                context["messageAdaptation"] = adaptations.First();
            }
            //context.SetMergeField( "messageAdaptations", adaptations, LavaContextRelativeScopeSpecifier.Root );
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