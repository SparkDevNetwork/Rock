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
using Rock.Transactions;
using Rock.Web.Cache;

using System.Collections.Generic;
using System.IO;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Tag which allows an Intent Interaction to be written.
    /// </summary>
    public class InteractionIntentWriteTag : LavaTagBase, ILavaSecured
    {
        #region Parameter Keys

        private class ParameterKey
        {
            public const string IntentValueId = "intentvalueid";
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

        /// <inheritdoc/>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            // First, ensure that this command is allowed in the context.
            if ( !this.IsAuthorized( context ) )
            {
                result.Write( string.Format( LavaBlockBase.NotAuthorizedMessage, this.SourceElementName ) );
                base.OnRender( context, result );
                return;
            }

            // Parse the Lava Command markup to retrieve parameters.
            var parms = new Dictionary<string, string>
            {
                { ParameterKey.Operation, "View" }
            };

            LavaHelper.ParseCommandMarkup( this.ElementAttributesMarkup, context, parms );

            // Set local variables from parsed parameters.
            var intentValueId = parms.GetValueOrNull( ParameterKey.IntentValueId ).AsIntegerOrNull();
            if ( !intentValueId.HasValue )
            {
                // Do nothing if an intent value ID wasn't specified.
                return;
            }

            var rockContext = LavaHelper.GetRockContextFromLavaContext( context );

            var channelTypeMediumValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_INTERACTION_INTENTS.AsGuid(), rockContext )?.Id;
            var definedTypeEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.DEFINED_TYPE.AsGuid(), rockContext )?.Id;
            var interactionIntentDefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.INTERACTION_INTENT.AsGuid(), rockContext )?.Id;

            if ( !channelTypeMediumValueId.HasValue
                || !definedTypeEntityTypeId.HasValue
                || !interactionIntentDefinedTypeId.HasValue )
            {
                // Missing required system values.
                return;
            }

            var intentValue = DefinedValueCache.Get( intentValueId.Value, rockContext );
            if ( intentValue == null
                || intentValue.DefinedTypeId != interactionIntentDefinedTypeId.Value
                || intentValue.Value.IsNullOrWhiteSpace() )
            {
                // The caller supplied an invalid Interaction Intent DefinedValue ID; nothing to do.
                return;
            }

            var operation = parms.GetValueOrNull( ParameterKey.Operation );
            var summary = parms.GetValueOrNull( ParameterKey.Summary );
            var source = parms.GetValueOrNull( ParameterKey.Source );
            var medium = parms.GetValueOrNull( ParameterKey.Medium );
            var campaign = parms.GetValueOrNull( ParameterKey.Campaign );
            var content = parms.GetValueOrNull( ParameterKey.Content );
            var term = parms.GetValueOrNull( ParameterKey.Term );

            var personAliasId = parms.GetValueOrNull( ParameterKey.PersonAliasId ).AsIntegerOrNull();
            if ( !personAliasId.HasValue )
            {
                var currentPerson = LavaHelper.GetCurrentPerson( context );
                personAliasId = currentPerson?.PrimaryAliasId;

                if ( !personAliasId.HasValue && currentPerson != null )
                {
                    personAliasId = LavaHelper.GetPrimaryPersonAliasId( currentPerson );
                }
            }

            // Write the interaction by way of a transaction.
            var info = new InteractionTransactionInfo
            {
                ChannelTypeMediumValueId = channelTypeMediumValueId,
                ChannelEntityId = interactionIntentDefinedTypeId,
                ChannelName = "Interaction Intents",
                ComponentEntityTypeId = definedTypeEntityTypeId,
                ComponentEntityId = intentValue.Id,
                ComponentName = intentValue.Value,
                InteractionOperation = operation,
                InteractionSummary = summary,
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
                return "InteractionIntentWrite";
            }
        }

        #endregion ILavaSecured
    }
}
