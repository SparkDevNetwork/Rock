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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Lava;
using Rock.Model;
using Rock.RealTime;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Utility
{
    /// <summary>
    /// Displays RealTime events from Rock with custom formatting options.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianDetailBlockType" />

    [DisplayName( "RealTime Visualizer" )]
    [Category( "Utility" )]
    [Description( "Displays RealTime events from Rock with custom formatting options." )]
    [IconCssClass( "fa fa-chart-bar" )]

    #region Block Attributes

    [KeyValueListField( "Channels",
        Description = "The list of topics and channels to subscribe to.",
        KeyPrompt = "Topic",
        ValuePrompt = "Channel",
        IsRequired = true,
        Order = 0,
        Key = AttributeKeys.Channels )]

    [BlockTemplateField( "Template",
        Description = "The lava template to use when processing the message for display. The 'Message' variable will contain the message name and 'Args' variable will be an array of the message arguments.",
        TemplateBlockValueGuid = "8ff590c7-84cf-4d17-83fc-61b61d05937a",
        DefaultValue = "",
        IsRequired = true,
        Order = 1,
        Key = AttributeKeys.Template )]

    [BlockTemplateField( "Display Script",
        Description = "The JavaScript that will be used to display the rendered message. The function 'showMessage' will be called.",
        TemplateBlockValueGuid = "961e542b-14e5-4d5c-8a03-90ecc175593e",
        DefaultValue = "",
        IsRequired = true,
        Order = 2,
        Key = AttributeKeys.DisplayScript )]

    [LavaCommandsField( "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this block.",
        IsRequired = false,
        Key = AttributeKeys.EnabledLavaCommands,
        Order = 3 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "77f4ea4a-ce87-4309-a7a0-2a1a75ab61cd" )]
    [Rock.SystemGuid.BlockTypeGuid( "ce185083-df13-48f9-8c97-83eda1ca65c2" )]
    public class RealTimeVisualizer : RockObsidianDetailBlockType
    {
        #region Keys

        private static class AttributeKeys
        {
            public const string Channels = "Channels";
            public const string Template = "Template";
            public const string DisplayScript = "DisplayScript";
            public const string EnabledLavaCommands = "EnabledLavaCommands";
        }

        #endregion

        #region Properties

        public override string BlockFileUrl => $"{base.BlockFileUrl}.obs";

        /// <summary>
        /// Gets the lava template to use when displaying messages.
        /// </summary>
        /// <value>
        /// The lava template to use when displaying messages.
        /// </value>
        protected string Template => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKeys.Template ) );

        /// <summary>
        /// Gets the JavaScript to use when displaying messages.
        /// </summary>
        /// <value>
        /// The JavaScript to use when displaying messages.
        /// </value>
        protected string DisplayScript => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKeys.DisplayScript ) );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new
            {
                Resolve = Template.IsNotNullOrWhiteSpace(),
                Topics = GetTopicsAndChannels().Select( t => t.Topic ).ToList(),
                JavaScript = DisplayScript ?? ""
            };
        }

        private List<(string Topic, string Channel)> GetTopicsAndChannels()
        {
            var channelValues = GetAttributeValue( AttributeKeys.Channels )?.Split( '|' ) ?? new string[0];
            var topicsAndChannels = new List<(string Topic, string Channel)>();

            foreach ( string channelValue in channelValues )
            {
                var topicAndChannel = channelValue.Split( new char[] { '^' } );

                // url decode array items just in case they were UrlEncoded (in the KeyValueList controls)
                topicAndChannel = topicAndChannel.Select( s => s.GetFullyUrlDecodedValue() ).ToArray();

                if ( topicAndChannel.Length != 2 )
                {
                    continue;
                }

                var topicIdentifier = topicAndChannel[0];
                var channelName = topicAndChannel[1];

                if ( topicIdentifier.IsNullOrWhiteSpace() || channelName.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                topicsAndChannels.Add( (topicIdentifier, channelName) );
            }

            return topicsAndChannels;
        }

        #endregion

        #region Block Actions

        [BlockAction]
        public async Task<BlockActionResult> Subscribe( string connectionId )
        {
            foreach ( var topicAndChannel in GetTopicsAndChannels() )
            {
                var topic = RealTimeHelper.Engine.GetTopicConfigurations()
                    .Where( c => c.TopicIdentifier == topicAndChannel.Topic )
                    .FirstOrDefault();

                if ( topic == null )
                {
                    return ActionBadRequest( $"Invalid topic '{topicAndChannel.Topic}'." );
                }

                var getTopicContextGeneric = typeof( RealTimeHelper )
                    .GetMethod( nameof( RealTimeHelper.GetTopicContext ), BindingFlags.Public | BindingFlags.Static );

                if ( getTopicContextGeneric == null )
                {
                    return ActionBadRequest( "Unable to resolve GetTopicContext method." );
                }

                var getTopicContext = getTopicContextGeneric.MakeGenericMethod( topic.ClientInterfaceType );

                var context = ( ITopic ) getTopicContext.Invoke( null, Array.Empty<object>() );

                await context.Channels.AddToChannelAsync( connectionId, topicAndChannel.Channel );
            }

            return ActionOk();
        }

        [BlockAction]
        public BlockActionResult Resolve( string topicIdentifier, string message, object[] arguments )
        {
            var enabledLavaCommands = GetAttributeValue( AttributeKeys.EnabledLavaCommands );
            var mergeFields = RequestContext.GetCommonMergeFields();

            mergeFields.AddOrReplace( "Topic", topicIdentifier );
            mergeFields.AddOrReplace( "Message", message );
            mergeFields.AddOrReplace( "Args", LavaHelper.JavaScriptObjectToLavaObject( arguments ) );

            var result = Template.ResolveMergeFields( mergeFields, null, enabledLavaCommands ).Trim();

            return ActionOk( result );
        }

        #endregion
    }
}