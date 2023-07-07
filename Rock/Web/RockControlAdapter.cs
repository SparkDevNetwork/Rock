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

using System;
using System.Diagnostics;
using System.Web.UI.Adapters;
using Rock.Bus;
using Rock.Observability;
using Rock.Web.UI;

namespace Rock.Web
{
    /// <summary>
    /// Control adapter to help with the observability telemetry. This is registered in the observability http module. 
    /// </summary>
    public class RockControlAdapter : ControlAdapter
    {
        /// <summary>
        /// Overrides the blocks OnInit event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit( EventArgs e )
        {
            var rockBlock = ( Control as RockBlock );

            // Start the block observability activity
            ObservabilityHelper.StartActivity( $"BLOCK INIT {rockBlock.BlockCache.BlockType.Name} - {rockBlock.BlockCache.Name}" );
            Activity.Current?.AddTag( "rock-otel-type", "rock-block" );
            Activity.Current?.AddTag( "rock-blocktype-name", rockBlock.BlockCache.BlockType.Name );
            Activity.Current?.AddTag( "rock-blocktype-id", rockBlock.BlockCache.BlockType.Id );
            Activity.Current?.AddTag( "rock-node", RockMessageBus.NodeName );

            // Call the blocks OnInit
            //rockBlock.SendOnInit( e );

            base.OnInit( e );

            // Close out activity
            Activity.Current?.Dispose();
        }

        /// <summary>
        /// Overrides the blocks OnLoad event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad( EventArgs e )
        {
            var rockBlock = ( Control as RockBlock );

            // Start the block observability activity
            ObservabilityHelper.StartActivity( $"BLOCK LOAD {rockBlock.BlockCache.BlockType.Name} - {rockBlock.BlockCache.Name}" );
            Activity.Current?.AddTag( "rock-otel-type", "rock-block" );
            Activity.Current?.AddTag( "rock-blocktype-name", rockBlock.BlockCache.BlockType.Name );
            Activity.Current?.AddTag( "rock-blocktype-id", rockBlock.BlockCache.BlockType.Id );

            // Call the blocks OnInit
            //rockBlock.SendOnLoad( e );
            base.OnLoad( e );

            // Close out activity
            Activity.Current?.Dispose();
        }
    }
}
