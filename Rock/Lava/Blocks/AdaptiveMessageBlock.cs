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
        /// Parameter name for specifying the key.
        /// </summary>
        public static readonly string ParameterKey = "key";

        /// <summary>
        /// Parameter name for specifying the count.
        /// </summary>
        public static readonly string ParameterCount = "count";

        /// <summary>
        /// Parameter name for specifying the trackviews.
        /// </summary>
        public static readonly string ParameterTrackviews = "trackviews";

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

            _settings.ParseFromMarkup( _markup, context );

            try
            {
                var key = _settings.GetString( ParameterKey );
                if ( key.IsNullOrWhiteSpace() )
                {
                    throw new Exception( $"Invalid configuration setting. Key is unknown" );
                }

                var count = _settings.GetIntegerOrNull( ParameterCount );
                if ( !count.HasValue )
                {
                    count = 1;
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
                var adaptiveMessage = AdaptiveMessageCache.All().Where( a => a.Key == key ).FirstOrDefault();
                var adaptations = adaptiveMessage.Adaptations
                    .Where( a => !a.SegmentIds.Any() || a.SegmentIds.Any( b => personSegmentIdList.Contains( b ) ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name )
                    .Take( count.Value )
                    .ToList();
                
                AddLavaMergeFieldsToContext( context, adaptations );

                if ( isTrackViews && adaptations.Any() && adaptiveMessage != null )
                {
                    foreach ( var adaptation in adaptations )
                    {
                        var interactionChannelId = InteractionChannelCache.GetId( Rock.SystemGuid.InteractionChannel.ADAPTIVE_MESSAGES.AsGuid() );
                        var info = new InteractionTransactionInfo
                        {
                            InteractionChannelId = interactionChannelId.Value,
                            ComponentEntityId = adaptiveMessage.Id,
                            ComponentName = adaptiveMessage.Name,
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

        private void AddLavaMergeFieldsToContext( ILavaRenderContext context, List<AdaptiveMessageAdaptationCache> adaptations )
        {
            context["messageAdaptations"] = adaptations;
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
    }
}