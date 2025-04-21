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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Rock.Bus.Consumer;
using Rock.Bus.Message;
using Rock.Bus.Queue;
using Rock.Model;

namespace Rock.Cms
{
    /// <summary>
    /// Handles messages from a server in the farm (including this server)
    /// that a theme was updated. This needs to trigger a compile of the
    /// theme so that the files on disk match the database configuration.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class ThemeWasUpdatedConsumer : RockConsumer<EntityUpdateQueue, ThemeWasUpdatedMessage>
    {
        /// <inheritdoc/>
        public override void Consume( ThemeWasUpdatedMessage message )
        {
            // This could potentially take a little bit and we don't want to
            // hold up other bus messages. So do the processing on a background
            // task and log any errors.
            Task.Run( () =>
            {
                try
                {
                    ThemeService.BuildTheme( message.ThemeId );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                }
            } );
        }
    }
}
