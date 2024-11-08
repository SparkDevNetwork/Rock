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
using System.Web.UI.Adapters;

using Rock.Configuration;
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
            // Start the block observability activity. Note that some blocks are not loaded directly by RockPage and therefore don't have a block cache.
            if ( Control is RockBlock rockBlock && rockBlock.BlockCache != null )
            {
                using ( var activity = ObservabilityHelper.StartActivity( $"BLOCK INIT {rockBlock.BlockCache.BlockType.Name} - {rockBlock.BlockCache.Name}" ) )
                {
                    activity?.AddTag( "rock.otel_type", "rock-block" );
                    activity?.AddTag( "rock.blocktype.name", rockBlock.BlockCache.BlockType.Name );
                    activity?.AddTag( "rock.blocktype.id", rockBlock.BlockCache.BlockType.Id );
                    activity?.AddTag( "rock.node", RockApp.Current.HostingSettings.NodeName );

                    base.OnInit( e );
                }
            }
            else
            {
                base.OnInit( e );
            }
        }

        /// <summary>
        /// Overrides the blocks OnLoad event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad( EventArgs e )
        {
            // Start the block observability activity. Note that some blocks are not loaded directly by RockPage and therefore don't have a block cache.
            if ( Control is RockBlock rockBlock && rockBlock.BlockCache != null )
            {
                using ( var activity = ObservabilityHelper.StartActivity( $"BLOCK LOAD {rockBlock.BlockCache.BlockType.Name} - {rockBlock.BlockCache.Name}" ) )
                {
                    activity?.AddTag( "rock.otel_type", "rock-block" );
                    activity?.AddTag( "rock.blocktype.name", rockBlock.BlockCache.BlockType.Name );
                    activity?.AddTag( "rock.blocktype.id", rockBlock.BlockCache.BlockType.Id );

                    base.OnLoad( e );
                }
            }
            else
            {
                base.OnLoad( e );
            }
        }
    }
}
