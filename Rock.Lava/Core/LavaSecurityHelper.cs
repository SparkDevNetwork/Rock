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
using System.Linq;

namespace Rock.Lava
{
    /// <summary>
    /// A set of helper methods to implement Lava security.
    /// </summary>
    public static class LavaSecurityHelper
    {
        /// <summary>
        /// Determines whether the specified command is authorized within the context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="command">The command.</param>
        /// <returns>
        ///   <c>true</c> if the specified command is authorized; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAuthorized( ILavaRenderContext context, string command )
        {
            if ( command.IsNullOrWhiteSpace() )
            {
                return false;
            }

            var enabledCommands = context.GetEnabledCommands();

            if ( enabledCommands.Any() )
            {
                if ( enabledCommands.Contains( "All", StringComparer.OrdinalIgnoreCase )
                     || enabledCommands.Contains( command, StringComparer.OrdinalIgnoreCase ) )
                {
                    return true;
                }
            }

            return false;
        }
    }
}
