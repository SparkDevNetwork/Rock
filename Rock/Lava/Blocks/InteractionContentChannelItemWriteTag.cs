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
using System.Data.Entity;
using System.IO;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Transactions;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Tag which allows a Content Channel Item Interaction to be written.
    /// </summary>
    public class InteractionContentChannelItemWriteTag : LavaTagBase, ILavaSecured
    {
        #region Parameter Keys

        private class ParameterKey
        {
            public const string ContentChannelItemId = "contentchannelitemid";
            public const string Operation = "operation";
            public const string Summary = "summary";
            public const string PersonAliasId = "personaliasid";
            public const string Source = "source";
            public const string Medium = "medium";
            public const string Campaign = "campaign";
            public const string Content = "content";
            public const string Term = "term";
        }

        #endregion Parameter Keys

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            // First, ensure that this command is allowed in the context.
            if ( !this.IsAuthorized( context ) )
            {
                result.Write( string.Format( LavaBlockBase.NotAuthorizedMessage, this.SourceElementName ) );
                base.OnRender( context, result );
                return;
            }

            // Parse the Lava Command markup to retrieve paramters.
            var parms = new Dictionary<string, string>
            {
                { ParameterKey.Operation, "View" }
            };

            LavaHelper.ParseCommandMarkup( this.ElementAttributesMarkup, context, parms );

            // Set local variables from parsed parameters.
            int? contentChannelItemId = parms.GetValueOrNull( ParameterKey.ContentChannelItemId ).AsIntegerOrNull();
            if ( !contentChannelItemId.HasValue )
            {
                // Do nothing if a ContentChannelItem ID wasn't specified.
                return;
            }

            ContentChannelItem contentChannelItem = null;
            using ( var rockContext = new RockContext() )
            {
                contentChannelItem = new ContentChannelItemService( rockContext )
                    .Queryable( "ContentChannel" )
                    .AsNoTracking()
                    .FirstOrDefault( c => c.Id == contentChannelItemId.Value );
            }

            ContentChannel contentChannel = contentChannelItem.ContentChannel;
            if ( contentChannelItem == null || contentChannel == null )
            {
                // The caller supplied an invalid ContentChannelItem ID; nothing to do.
                return;
            }

            string operation = parms.GetValueOrNull( ParameterKey.Operation );
            string summary = parms.GetValueOrNull( ParameterKey.Summary );
            string source = parms.GetValueOrNull( ParameterKey.Source );
            string medium = parms.GetValueOrNull( ParameterKey.Medium );
            string campaign = parms.GetValueOrNull( ParameterKey.Campaign );
            string content = parms.GetValueOrNull( ParameterKey.Content );
            string term = parms.GetValueOrNull( ParameterKey.Term );

            int? personAliasId = parms.GetValueOrNull( ParameterKey.PersonAliasId ).AsIntegerOrNull();
            if ( !personAliasId.HasValue )
            {
                Person currentPerson = LavaHelper.GetCurrentPerson( context );
                personAliasId = LavaHelper.GetPrimaryPersonAliasId( currentPerson );
            }

            // Write the Interaction by way of a transaction.
            DefinedValueCache mediumType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CONTENTCHANNEL.AsGuid() );
            if ( mediumType == null )
            {
                return;
            }

            var info = new InteractionTransactionInfo
            {
                ChannelTypeMediumValueId = mediumType.Id,
                ChannelEntityId = contentChannel.Id,
                ChannelName = contentChannel.ToString(),
                ComponentEntityTypeId = contentChannel.TypeId,
                ComponentEntityId = contentChannelItem.Id,
                ComponentName = contentChannelItem.ToString(),
                InteractionOperation = operation,
                InteractionSummary = summary ?? contentChannelItem.Title,
                PersonAliasId = personAliasId,
                InteractionSource = source,
                InteractionMedium = medium,
                InteractionCampaign = campaign,
                InteractionContent = content,
                InteractionTerm = term
            };

            var interactionTransaction = new InteractionTransaction( info );
            interactionTransaction.Enqueue();
        }

        #region ILavaSecured

        /// <inheritdoc/>
        public string RequiredPermissionKey
        {
            get
            {
                return "InteractionContentChannelItemWrite";
            }
        }

        #endregion
    }
}
