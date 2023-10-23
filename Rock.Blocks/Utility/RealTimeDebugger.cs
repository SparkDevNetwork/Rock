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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Model;
using Rock.RealTime;
using Rock.ViewModels.Utility;

namespace Rock.Blocks.Utility
{
    /// <summary>
    /// Provides a simple way to debug RealTime events.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "RealTime Debugger" )]
    [Category( "Utility" )]
    [Description( "Provides a simple way to debug RealTime events." )]
    [IconCssClass( "fa fa-bug" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "d7d88034-6c6b-4626-a268-420bb8694bfa" )]
    [Rock.SystemGuid.BlockTypeGuid( "e5fa4818-2e0c-4cc6-95f2-34dcc5b3d8c8" )]
    public class RealTimeDebugger : RockDetailBlockType
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var topics = RealTimeHelper.Engine.GetTopicConfigurations()
                .Select( c => new ListItemBag
                {
                    Value = c.TopicIdentifier,
                    Text = c.TopicType.FullName
                } )
                .ToList();

            return new
            {
                Topics = topics
            };
        }

        #endregion

        #region Block Actions

        [BlockAction]
        public async Task<BlockActionResult> JoinChannel( string connectionId, string topicIdentifier, string channelName )
        {
            var topic = RealTimeHelper.Engine.GetTopicConfigurations()
                .Where( c => c.TopicIdentifier == topicIdentifier )
                .FirstOrDefault();

            if ( topic == null )
            {
                return ActionBadRequest( "Invalid topic." );
            }

            var getTopicContextGeneric = typeof( RealTimeHelper )
                .GetMethod( nameof( RealTimeHelper.GetTopicContext ), BindingFlags.Public | BindingFlags.Static );

            if ( getTopicContextGeneric == null )
            {
                return ActionBadRequest( "Unable to resolve GetTopicContext method." );
            }

            var getTopicContext = getTopicContextGeneric.MakeGenericMethod( topic.ClientInterfaceType );

            var context = ( ITopic ) getTopicContext.Invoke( null, Array.Empty<object>() );

            await context.Channels.AddToChannelAsync( connectionId, channelName );

            return ActionOk();
        }

        #endregion
    }
}