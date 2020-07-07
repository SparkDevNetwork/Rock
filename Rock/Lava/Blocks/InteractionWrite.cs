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
using System.IO;

using DotLiquid;

using Rock.Model;
using Rock.Transactions;
using Rock.Web.Cache;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Tag which allows an Interaction to be written.
    /// </summary>
    public class InteractionWrite : RockLavaBlockBase
    {
        #region Parameter Keys

        private class ParameterKey
        {
            // InteractionChannel parameters.
            public const string ChannelTypeMediumValueId = "channeltypemediumvalueid";
            public const string ChannelEntityId = "channelentityid";
            public const string ChannelName = "channelname";
            public const string ComponentEntityTypeId = "componententitytypeid";
            public const string InteractionEntityTypeId = "interactionentitytypeid";

            // InteractionComponent parameters.
            public const string ComponentEntityId = "componententityid";
            public const string ComponentName = "componentname";

            // Interaction parameters.
            public const string EntityId = "entityid";
            public const string Operation = "operation";
            public const string Summary = "summary";
            public const string RelatedEntityTypeId = "relatedentitytypeid";
            public const string RelatedEntityId = "relatedentityid";
            public const string ChannelCustom1 = "channelcustom1";
            public const string ChannelCustom2 = "channelcustom2";
            public const string ChannelCustomIndexed1 = "channelcustomindexed1";
            public const string PersonAliasId = "personaliasid";
        }

        #endregion Parameter Keys

        private string _markup;

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public override void OnStartup()
        {
            Template.RegisterTag<InteractionWrite>( "interactionwrite" );
        }

        /// <summary>
        /// Initializes the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="markup">The markup.</param>
        /// <param name="tokens">The tokens.</param>
        public override void Initialize( string tagName, string markup, List<string> tokens )
        {
            _markup = markup;

            base.Initialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void Render( Context context, TextWriter result )
        {
            // First, ensure that this command is allowed in the context.
            if ( !this.IsAuthorized( context ) )
            {
                result.Write( string.Format( RockLavaBlockBase.NotAuthorizedMessage, this.Name ) );
                base.Render( context, result );
                return;
            }

            // Parse the Lava Command markup to retrieve paramters.
            var parms = new Dictionary<string, string>();
            LavaHelper.ParseCommandMarkup( _markup, context, parms );

            // Set local variables from parsed parameters.
            // InteractionChannel variables.
            int? channelTypeMediumValueId = parms.GetValueOrNull( ParameterKey.ChannelTypeMediumValueId ).AsIntegerOrNull();
            if ( !channelTypeMediumValueId.HasValue )
            {
                // Do nothing if a ChannelTypeMediumValueId wasn't specified.
                return;
            }

            DefinedValueCache channelTypeMediumValue = DefinedValueCache.Get( channelTypeMediumValueId.Value );
            if ( channelTypeMediumValue == null )
            {
                // Do nothing if an invalid ChannelTypeMediumValueId was specified.
                return;
            }

            int? channelEntityId = parms.GetValueOrNull( ParameterKey.ChannelEntityId ).AsIntegerOrNull();
            string channelName = parms.GetValueOrNull( ParameterKey.ChannelName );

            int? componentEntityTypeId = parms.GetValueOrNull( ParameterKey.ComponentEntityTypeId ).AsIntegerOrNull();
            int? interactionEntityTypeId = parms.GetValueOrNull( ParameterKey.InteractionEntityTypeId ).AsIntegerOrNull();

            // InteractionComponent variables.
            int? componentEntityId = parms.GetValueOrNull( ParameterKey.ComponentEntityId ).AsIntegerOrNull();
            string componentName = parms.GetValueOrNull( ParameterKey.ComponentName );

            // Interaction variables.
            int? entityId = parms.GetValueOrNull( ParameterKey.EntityId ).AsIntegerOrNull();

            string operation = parms.GetValueOrNull( ParameterKey.Operation );
            string summary = parms.GetValueOrNull( ParameterKey.Summary );

            int? relatedEntityTypeId = parms.GetValueOrNull( ParameterKey.RelatedEntityTypeId ).AsIntegerOrNull();
            int? relatedEntityId = parms.GetValueOrNull( ParameterKey.RelatedEntityId ).AsIntegerOrNull();

            string channelCustom1 = parms.GetValueOrNull( ParameterKey.ChannelCustom1 );
            string channelCustom2 = parms.GetValueOrNull( ParameterKey.ChannelCustom2 );
            string channelCustomIndexed1 = parms.GetValueOrNull( ParameterKey.ChannelCustomIndexed1 );

            int? personAliasId = parms.GetValueOrNull( ParameterKey.PersonAliasId ).AsIntegerOrNull();
            if ( !personAliasId.HasValue )
            {
                Person currentPerson = LavaHelper.GetCurrentPerson( context );
                personAliasId = LavaHelper.GetPrimaryPersonAliasId( currentPerson );
            }

            string data = null;
            using ( TextWriter twData = new StringWriter() )
            {
                base.Render( context, twData );
                data = twData.ToString();
            }

            // Write the Interaction by way of a transaction.
            var info = new InteractionTransactionInfo
            {
                ChannelTypeMediumValueId = channelTypeMediumValueId,
                ChannelEntityId = channelEntityId,
                ChannelName = channelName,
                ComponentEntityTypeId = componentEntityTypeId,
                InteractionEntityTypeId = interactionEntityTypeId,

                ComponentEntityId = componentEntityId,
                ComponentName = componentName,

                InteractionEntityId = entityId,
                InteractionOperation = operation,
                InteractionSummary = summary,
                InteractionData = data,
                InteractionRelatedEntityTypeId = relatedEntityTypeId,
                InteractionRelatedEntityId = relatedEntityId,
                InteractionChannelCustom1 = channelCustom1,
                InteractionChannelCustom2 = channelCustom2,
                InteractionChannelCustomIndexed1 = channelCustomIndexed1,
                PersonAliasId = personAliasId
            };

            var interactionTransaction = new InteractionTransaction( info );
            interactionTransaction.Enqueue();
        }
    }
}
